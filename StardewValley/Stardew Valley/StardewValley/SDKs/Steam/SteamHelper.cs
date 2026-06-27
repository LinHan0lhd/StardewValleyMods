using System;
using Galaxy.Api;
using StardewValley.Menus;
using StardewValley.SDKs.GogGalaxy.Listeners;
using Steamworks;

namespace StardewValley.SDKs.Steam
{
	// Token: 0x02000164 RID: 356
	public class SteamHelper : SDKHelper
	{
		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06001B35 RID: 6965 RVA: 0x0013BF19 File Offset: 0x0013A119
		public SDKNetHelper Networking
		{
			get
			{
				return this.networking;
			}
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06001B36 RID: 6966 RVA: 0x0013BF21 File Offset: 0x0013A121
		// (set) Token: 0x06001B37 RID: 6967 RVA: 0x0013BF29 File Offset: 0x0013A129
		public bool ConnectionFinished { get; private set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06001B38 RID: 6968 RVA: 0x0013BF32 File Offset: 0x0013A132
		// (set) Token: 0x06001B39 RID: 6969 RVA: 0x0013BF3A File Offset: 0x0013A13A
		public int ConnectionProgress { get; private set; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06001B3A RID: 6970 RVA: 0x0013BF43 File Offset: 0x0013A143
		// (set) Token: 0x06001B3B RID: 6971 RVA: 0x0013BF4B File Offset: 0x0013A14B
		public bool GalaxyConnected { get; private set; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06001B3C RID: 6972 RVA: 0x0013BF54 File Offset: 0x0013A154
		public string Name { get; } = "Steam";

		// Token: 0x06001B3D RID: 6973 RVA: 0x0013BF5C File Offset: 0x0013A15C
		public void EarlyInitialize()
		{
		}

		// Token: 0x06001B3E RID: 6974 RVA: 0x0013BF5E File Offset: 0x0013A15E
		public virtual bool IsRunningOnSteamDeck()
		{
			return this._runningOnSteamDeck;
		}

		// Token: 0x06001B3F RID: 6975 RVA: 0x0013BF68 File Offset: 0x0013A168
		public void Initialize()
		{
			try
			{
				this.active = SteamAPI.Init();
				Game1.log.Verbose("Steam logged on: " + SteamUser.BLoggedOn().ToString());
				if (this.active)
				{
					this._runningOnSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
					Game1.log.Verbose("Initializing GalaxySDK");
					this.encryptedAppTicketResponse = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(this.onEncryptedAppTicketResponse));
					Game1.log.Verbose("Requesting Steam app ticket");
					SteamAPICall_t handle = SteamUser.RequestEncryptedAppTicket(LegacyShims.EmptyArray<byte>(), 0);
					this.encryptedAppTicketResponse.Set(handle, null);
					this.ConnectionProgress++;
					SteamNetworkingUtils.InitRelayNetworkAccess();
				}
			}
			catch (Exception e)
			{
				Game1.log.Error("Error connecting to Steam.", e);
				this.active = false;
				this.ConnectionFinished = true;
			}
			if (this.active)
			{
				try
				{
					GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a", "."));
					this.galaxyAuthListener = new GalaxyAuthListener(new Action(this.onGalaxyAuthSuccess), new Action<IAuthListener.FailureReason>(this.onGalaxyAuthFailure), new Action(this.onGalaxyAuthLost));
					this.galaxyStateChangeListener = new GalaxyOperationalStateChangeListener(new Action<uint>(this.onGalaxyStateChange));
				}
				catch (Exception e2)
				{
					Game1.log.Error("Error initializing the Galaxy API.", e2);
				}
				this.gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(new Callback<GameOverlayActivated_t>.DispatchDelegate(this.onGameOverlayActivated));
				this.gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(new Callback<GamepadTextInputDismissed_t>.DispatchDelegate(this.OnKeyboardDismissed));
			}
		}

		// Token: 0x06001B40 RID: 6976 RVA: 0x0013C104 File Offset: 0x0013A304
		public void CancelKeyboard()
		{
			this._keyboardTextBox = null;
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x0013C10D File Offset: 0x0013A30D
		public void ShowKeyboard(TextBox text_box)
		{
			this._keyboardTextBox = text_box;
			SteamUtils.ShowGamepadTextInput(text_box.PasswordBox ? EGamepadTextInputMode.k_EGamepadTextInputModePassword : EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, "", (uint)((text_box.textLimit < 0) ? 100 : text_box.textLimit), text_box.Text);
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x0013C148 File Offset: 0x0013A348
		public void OnKeyboardDismissed(GamepadTextInputDismissed_t callback)
		{
			if (this._keyboardTextBox == null)
			{
				return;
			}
			if (!callback.m_bSubmitted)
			{
				this._keyboardTextBox = null;
				return;
			}
			uint length = SteamUtils.GetEnteredGamepadTextLength();
			string entered_text;
			if (!SteamUtils.GetEnteredGamepadTextInput(out entered_text, length))
			{
				this._keyboardTextBox = null;
				return;
			}
			this._keyboardTextBox.RecieveTextInput(entered_text);
			this._keyboardTextBox = null;
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x0013C19C File Offset: 0x0013A39C
		private void onSetGalaxyProfileName(GalaxyID userID)
		{
			try
			{
				if (userID != GalaxyInstance.User().GetGalaxyID())
				{
					return;
				}
			}
			catch (Exception)
			{
				return;
			}
			Game1.log.Verbose("Successfully set GOG Galaxy profile name.");
			GalaxySpecificUserDataListener galaxySpecificUserDataListener = this.galaxySpecificUserDataListener;
			if (galaxySpecificUserDataListener != null)
			{
				galaxySpecificUserDataListener.Dispose();
			}
			this.galaxySpecificUserDataListener = null;
		}

		// Token: 0x06001B44 RID: 6980 RVA: 0x0013C1FC File Offset: 0x0013A3FC
		private void onGalaxyStateChange(uint operationalState)
		{
			if (this.networking != null)
			{
				return;
			}
			if ((operationalState & 1U) != 0U)
			{
				Game1.log.Verbose("Galaxy signed in");
				this.ConnectionProgress++;
			}
			if ((operationalState & 2U) != 0U)
			{
				Game1.log.Verbose("Galaxy logged on");
				this.networking = new SteamNetHelper();
				this.ConnectionProgress++;
				this.ConnectionFinished = true;
				this.GalaxyConnected = true;
				try
				{
					this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(new Action<GalaxyID>(this.onSetGalaxyProfileName));
					GalaxyInstance.User().SetUserData("StardewDisplayName", SteamFriends.GetPersonaName());
				}
				catch (Exception ex)
				{
					Game1.log.Error("Failed to set GOG Galaxy profile name.", ex);
				}
			}
		}

		// Token: 0x06001B45 RID: 6981 RVA: 0x0013C2C0 File Offset: 0x0013A4C0
		private void onGalaxyAuthSuccess()
		{
			Game1.log.Verbose("Galaxy auth success");
			this.ConnectionProgress++;
		}

		// Token: 0x06001B46 RID: 6982 RVA: 0x0013C2E0 File Offset: 0x0013A4E0
		private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
		{
			if (this.networking == null)
			{
				this.networking = new SteamNetHelper();
			}
			Game1.log.Error("Galaxy auth failure: " + reason.ToString(), null);
			this.ConnectionFinished = true;
			this.GalaxyConnected = false;
		}

		// Token: 0x06001B47 RID: 6983 RVA: 0x0013C330 File Offset: 0x0013A530
		private void onGalaxyAuthLost()
		{
			if (this.networking == null)
			{
				this.networking = new SteamNetHelper();
			}
			Game1.log.Error("Galaxy auth lost", null);
			this.ConnectionFinished = true;
			this.GalaxyConnected = false;
		}

		// Token: 0x06001B48 RID: 6984 RVA: 0x0013C364 File Offset: 0x0013A564
		private void onEncryptedAppTicketResponse(EncryptedAppTicketResponse_t response, bool ioFailure)
		{
			if (response.m_eResult == EResult.k_EResultOK)
			{
				byte[] ticket = new byte[1024];
				uint ticketSize;
				SteamUser.GetEncryptedAppTicket(ticket, 1024, out ticketSize);
				this.ConnectionProgress++;
				Game1.log.Verbose("Signing into GalaxySDK");
				try
				{
					GalaxyInstance.User().SignInSteam(ticket, ticketSize, SteamFriends.GetPersonaName());
					return;
				}
				catch (Exception e)
				{
					Game1.log.Error("Galaxy SignInSteam failed with an exception:", e);
					return;
				}
			}
			Game1.log.Error("Failed to retrieve encrypted app ticket: " + response.m_eResult.ToString() + ", " + ioFailure.ToString(), null);
			this.ConnectionFinished = true;
		}

		// Token: 0x06001B49 RID: 6985 RVA: 0x0013C420 File Offset: 0x0013A620
		private void onGameOverlayActivated(GameOverlayActivated_t pCallback)
		{
			if (this.active)
			{
				if (pCallback.m_bActive != 0)
				{
					Game1.paused = !Game1.IsMultiplayer;
					return;
				}
				Game1.paused = false;
			}
		}

		// Token: 0x06001B4A RID: 6986 RVA: 0x0013C446 File Offset: 0x0013A646
		public bool RetroactiveAchievementsAllowed()
		{
			return true;
		}

		// Token: 0x06001B4B RID: 6987 RVA: 0x0013C44C File Offset: 0x0013A64C
		public void GetAchievement(string achieve)
		{
			if (this.active && SteamAPI.IsSteamRunning())
			{
				if (achieve.Equals("0"))
				{
					achieve = "a0";
				}
				try
				{
					SteamUserStats.SetAchievement(achieve);
					SteamUserStats.StoreStats();
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x06001B4C RID: 6988 RVA: 0x0013C4A0 File Offset: 0x0013A6A0
		public void ResetAchievements()
		{
			if (this.active && SteamAPI.IsSteamRunning())
			{
				try
				{
					SteamUserStats.ResetAllStats(true);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x06001B4D RID: 6989 RVA: 0x0013C4D8 File Offset: 0x0013A6D8
		public void Update()
		{
			if (this.active)
			{
				SteamAPI.RunCallbacks();
				try
				{
					GalaxyInstance.ProcessData();
				}
				catch (Exception)
				{
				}
			}
			Game1.game1.IsMouseVisible = (Game1.paused || Game1.options.hardwareCursor);
		}

		// Token: 0x06001B4E RID: 6990 RVA: 0x0013C52C File Offset: 0x0013A72C
		public void Shutdown()
		{
			SteamAPI.Shutdown();
		}

		// Token: 0x06001B4F RID: 6991 RVA: 0x0013C533 File Offset: 0x0013A733
		public void DebugInfo()
		{
			if (SteamAPI.IsSteamRunning())
			{
				Game1.debugOutput = (SteamUser.BLoggedOn() ? "steam is running, user logged on" : "steam is running");
				return;
			}
			Game1.debugOutput = "steam is not running";
			SteamAPI.Init();
		}

		// Token: 0x06001B50 RID: 6992 RVA: 0x0013C565 File Offset: 0x0013A765
		public string FilterDirtyWords(string words)
		{
			return words;
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06001B51 RID: 6993 RVA: 0x0013C568 File Offset: 0x0013A768
		public bool HasOverlay
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06001B52 RID: 6994 RVA: 0x0013C56B File Offset: 0x0013A76B
		public bool IsJapaneseRegionRelease
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06001B53 RID: 6995 RVA: 0x0013C56E File Offset: 0x0013A76E
		public bool IsEnterButtonAssignmentFlipped
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04001091 RID: 4241
		private Callback<GameOverlayActivated_t> gameOverlayActivated;

		// Token: 0x04001092 RID: 4242
		private CallResult<EncryptedAppTicketResponse_t> encryptedAppTicketResponse;

		// Token: 0x04001093 RID: 4243
		private Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

		// Token: 0x04001094 RID: 4244
		private GalaxyAuthListener galaxyAuthListener;

		// Token: 0x04001095 RID: 4245
		private GalaxyOperationalStateChangeListener galaxyStateChangeListener;

		// Token: 0x04001096 RID: 4246
		private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

		// Token: 0x04001097 RID: 4247
		public bool active;

		// Token: 0x04001098 RID: 4248
		private SDKNetHelper networking;

		// Token: 0x0400109D RID: 4253
		private TextBox _keyboardTextBox;

		// Token: 0x0400109E RID: 4254
		protected bool _runningOnSteamDeck;
	}
}
