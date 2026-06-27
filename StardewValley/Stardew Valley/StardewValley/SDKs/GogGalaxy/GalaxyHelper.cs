using System;
using Galaxy.Api;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy
{
	// Token: 0x0200016C RID: 364
	public class GalaxyHelper : SDKHelper
	{
		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06001BB3 RID: 7091 RVA: 0x0013E9CC File Offset: 0x0013CBCC
		public string Name { get; } = "Galaxy";

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06001BB4 RID: 7092 RVA: 0x0013E9D4 File Offset: 0x0013CBD4
		// (set) Token: 0x06001BB5 RID: 7093 RVA: 0x0013E9DC File Offset: 0x0013CBDC
		public bool ConnectionFinished { get; private set; }

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06001BB6 RID: 7094 RVA: 0x0013E9E5 File Offset: 0x0013CBE5
		// (set) Token: 0x06001BB7 RID: 7095 RVA: 0x0013E9ED File Offset: 0x0013CBED
		public int ConnectionProgress { get; private set; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06001BB8 RID: 7096 RVA: 0x0013E9F6 File Offset: 0x0013CBF6
		public SDKNetHelper Networking
		{
			get
			{
				return this.networking;
			}
		}

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06001BB9 RID: 7097 RVA: 0x0013E9FE File Offset: 0x0013CBFE
		public bool HasOverlay
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001BBA RID: 7098 RVA: 0x0013EA01 File Offset: 0x0013CC01
		public void EarlyInitialize()
		{
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x0013EA04 File Offset: 0x0013CC04
		public void Initialize()
		{
			try
			{
				GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a"));
				this.authListener = new GalaxyAuthListener(new Action(this.onGalaxyAuthSuccess), new Action<IAuthListener.FailureReason>(this.onGalaxyAuthFailure), new Action(this.onGalaxyAuthLost));
				this.stateChangeListener = new GalaxyOperationalStateChangeListener(new Action<uint>(this.onGalaxyStateChange));
				GalaxyInstance.User().SignInGalaxy(true);
				this.active = true;
				this.ConnectionProgress++;
			}
			catch (Exception e)
			{
				Game1.log.Error("Error initializing GalaxyHelper.", e);
				this.ConnectionFinished = true;
			}
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x0013EAB8 File Offset: 0x0013CCB8
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
				this.networking = new GalaxyNetHelper();
				this.ConnectionProgress++;
				this.ConnectionFinished = true;
			}
		}

		// Token: 0x06001BBD RID: 7101 RVA: 0x0013EB24 File Offset: 0x0013CD24
		private void onGalaxyAuthSuccess()
		{
			Game1.log.Verbose("Galaxy auth success");
			this.ConnectionProgress++;
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x0013EB43 File Offset: 0x0013CD43
		private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
		{
			Game1.log.Error("Galaxy auth failure: " + reason.ToString(), null);
			this.ConnectionFinished = true;
		}

		// Token: 0x06001BBF RID: 7103 RVA: 0x0013EB6E File Offset: 0x0013CD6E
		private void onGalaxyAuthLost()
		{
			Game1.log.Error("Galaxy auth lost", null);
			this.ConnectionFinished = true;
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x0013EB87 File Offset: 0x0013CD87
		public bool RetroactiveAchievementsAllowed()
		{
			return true;
		}

		// Token: 0x06001BC1 RID: 7105 RVA: 0x0013EB8A File Offset: 0x0013CD8A
		public void GetAchievement(string achieve)
		{
		}

		// Token: 0x06001BC2 RID: 7106 RVA: 0x0013EB8C File Offset: 0x0013CD8C
		public void ResetAchievements()
		{
			if (this.active)
			{
				GalaxyInstance.Stats().ResetStatsAndAchievements();
			}
		}

		// Token: 0x06001BC3 RID: 7107 RVA: 0x0013EBA0 File Offset: 0x0013CDA0
		public void Update()
		{
			if (this.active)
			{
				GalaxyInstance.ProcessData();
			}
		}

		// Token: 0x06001BC4 RID: 7108 RVA: 0x0013EBAF File Offset: 0x0013CDAF
		public void Shutdown()
		{
		}

		// Token: 0x06001BC5 RID: 7109 RVA: 0x0013EBB1 File Offset: 0x0013CDB1
		public void DebugInfo()
		{
		}

		// Token: 0x06001BC6 RID: 7110 RVA: 0x0013EBB3 File Offset: 0x0013CDB3
		public string FilterDirtyWords(string words)
		{
			return words;
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06001BC7 RID: 7111 RVA: 0x0013EBB6 File Offset: 0x0013CDB6
		public bool IsJapaneseRegionRelease
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06001BC8 RID: 7112 RVA: 0x0013EBB9 File Offset: 0x0013CDB9
		public bool IsEnterButtonAssignmentFlipped
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040010CB RID: 4299
		public const string ClientID = "48767653913349277";

		// Token: 0x040010CC RID: 4300
		public const string ClientSecret = "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a";

		// Token: 0x040010CD RID: 4301
		public const string DisplayNameDataKey = "StardewDisplayName";

		// Token: 0x040010CE RID: 4302
		public bool active;

		// Token: 0x040010CF RID: 4303
		private GalaxyAuthListener authListener;

		// Token: 0x040010D0 RID: 4304
		private GalaxyOperationalStateChangeListener stateChangeListener;

		// Token: 0x040010D4 RID: 4308
		private GalaxyNetHelper networking;
	}
}
