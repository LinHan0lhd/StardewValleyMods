using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.SaveSerialization;
using StardewValley.SDKs.Steam;

namespace StardewValley
{
	// Token: 0x020000F2 RID: 242
	public class Options
	{
		// Token: 0x1700022E RID: 558
		// (get) Token: 0x0600139F RID: 5023 RVA: 0x000F054F File Offset: 0x000EE74F
		// (set) Token: 0x060013A0 RID: 5024 RVA: 0x000F0561 File Offset: 0x000EE761
		public bool hardwareCursor
		{
			get
			{
				return !LocalMultiplayer.IsLocalMultiplayer(false) && this._hardwareCursor;
			}
			set
			{
				this._hardwareCursor = value;
			}
		}

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x060013A1 RID: 5025 RVA: 0x000F056A File Offset: 0x000EE76A
		public int lightingQuality
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x060013A2 RID: 5026 RVA: 0x000F056D File Offset: 0x000EE76D
		[XmlIgnore]
		public float zoomLevel
		{
			get
			{
				if (Game1.game1.takingMapScreenshot)
				{
					return this.baseZoomLevel;
				}
				return this.baseZoomLevel * Game1.game1.zoomModifier;
			}
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060013A3 RID: 5027 RVA: 0x000F0593 File Offset: 0x000EE793
		// (set) Token: 0x060013A4 RID: 5028 RVA: 0x000F05B6 File Offset: 0x000EE7B6
		[XmlIgnore]
		public float desiredBaseZoomLevel
		{
			get
			{
				if (LocalMultiplayer.IsLocalMultiplayer(false) || !Game1.game1.IsMainInstance)
				{
					return this.localCoopBaseZoomLevel;
				}
				return this.singlePlayerBaseZoomLevel;
			}
			set
			{
				if (LocalMultiplayer.IsLocalMultiplayer(false) || !Game1.game1.IsMainInstance)
				{
					this.localCoopBaseZoomLevel = value;
					return;
				}
				this.singlePlayerBaseZoomLevel = value;
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060013A5 RID: 5029 RVA: 0x000F05DB File Offset: 0x000EE7DB
		// (set) Token: 0x060013A6 RID: 5030 RVA: 0x000F060C File Offset: 0x000EE80C
		[XmlIgnore]
		public float desiredUIScale
		{
			get
			{
				if (Game1.gameMode != 3)
				{
					return 1f;
				}
				if (LocalMultiplayer.IsLocalMultiplayer(false) || !Game1.game1.IsMainInstance)
				{
					return this.localCoopDesiredUIScale;
				}
				return this.singlePlayerDesiredUIScale;
			}
			set
			{
				if (Game1.gameMode != 3)
				{
					return;
				}
				if (LocalMultiplayer.IsLocalMultiplayer(false) || !Game1.game1.IsMainInstance)
				{
					this.localCoopDesiredUIScale = value;
					return;
				}
				this.singlePlayerDesiredUIScale = value;
			}
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060013A7 RID: 5031 RVA: 0x000F063A File Offset: 0x000EE83A
		[XmlIgnore]
		public float uiScale
		{
			get
			{
				return this.baseUIScale * Game1.game1.zoomModifier;
			}
		}

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060013A8 RID: 5032 RVA: 0x000F0650 File Offset: 0x000EE850
		public bool allowStowing
		{
			get
			{
				Options.ItemStowingModes itemStowingModes = this.stowingMode;
				if (itemStowingModes == Options.ItemStowingModes.Off)
				{
					return false;
				}
				if (itemStowingModes != Options.ItemStowingModes.GamepadOnly)
				{
					return true;
				}
				if (this.gamepadControls)
				{
					SteamHelper steamHelper = Program.sdk as SteamHelper;
					return steamHelper == null || !steamHelper.IsRunningOnSteamDeck() || Game1.input.GetMouseState().LeftButton != ButtonState.Pressed;
				}
				return false;
			}
		}

		// Token: 0x060013A9 RID: 5033 RVA: 0x000F06AC File Offset: 0x000EE8AC
		public Options()
		{
			this.setToDefaults();
		}

		// Token: 0x060013AA RID: 5034 RVA: 0x000F0A34 File Offset: 0x000EEC34
		public string GetFilePathForDefaultOptions()
		{
			return Path.Combine(Program.GetAppDataFolder(null, true), "default_options");
		}

		// Token: 0x060013AB RID: 5035 RVA: 0x000F0A48 File Offset: 0x000EEC48
		public virtual void LoadDefaultOptions()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			Options default_options = null;
			string filePath = this.GetFilePathForDefaultOptions();
			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Open))
				{
					default_options = (this.defaultSettingsSerializer.DeserializeFast(stream) as Options);
				}
			}
			catch (Exception)
			{
			}
			if (default_options == null)
			{
				return;
			}
			Type type = typeof(Options);
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.GetCustomAttribute<DontLoadDefaultSetting>() == null && field.GetCustomAttribute<XmlIgnoreAttribute>() == null)
				{
					field.SetValue(this, field.GetValue(default_options));
				}
			}
			foreach (PropertyInfo property_info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property_info.GetCustomAttribute<DontLoadDefaultSetting>() == null && property_info.GetCustomAttribute<XmlIgnoreAttribute>() == null && property_info.GetSetMethod() != null && property_info.GetGetMethod() != null)
				{
					property_info.SetValue(this, property_info.GetValue(default_options, null), null);
				}
			}
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x000F0B70 File Offset: 0x000EED70
		public virtual void SaveDefaultOptions()
		{
			this.optionsDirty = false;
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			string filePath = this.GetFilePathForDefaultOptions();
			XmlWriterSettings settings = new XmlWriterSettings();
			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Create))
				{
					using (XmlWriter writer = XmlWriter.Create(stream, settings))
					{
						writer.WriteStartDocument();
						this.defaultSettingsSerializer.SerializeFast(writer, Game1.options);
						writer.WriteEndDocument();
						writer.Flush();
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x000F0C18 File Offset: 0x000EEE18
		public void platformClampValues()
		{
		}

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060013AE RID: 5038 RVA: 0x000F0C1C File Offset: 0x000EEE1C
		public bool SnappyMenus
		{
			get
			{
				return this.snappyMenus && this.gamepadControls && Game1.input.GetMouseState().LeftButton != ButtonState.Pressed && Game1.input.GetMouseState().RightButton != ButtonState.Pressed;
			}
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x000F0C68 File Offset: 0x000EEE68
		public Keys getFirstKeyboardKeyFromInputButtonList(InputButton[] inputButton)
		{
			for (int i = 0; i < inputButton.Length; i++)
			{
				if (inputButton[i].key != Keys.None)
				{
					return inputButton[i].key;
				}
			}
			return Keys.None;
		}

		// Token: 0x060013B0 RID: 5040 RVA: 0x000F0C9F File Offset: 0x000EEE9F
		public void reApplySetOptions()
		{
			this.platformClampValues();
			if (this.lightingQuality != this.appliedLightingQuality)
			{
				Program.gamePtr.refreshWindowSettings();
				this.appliedLightingQuality = this.lightingQuality;
			}
			Program.gamePtr.IsMouseVisible = this.hardwareCursor;
		}

		// Token: 0x060013B1 RID: 5041 RVA: 0x000F0CDC File Offset: 0x000EEEDC
		public void setToDefaults()
		{
			this.playFootstepSounds = true;
			this.showMenuBackground = false;
			this.showClearBackgrounds = false;
			this.showMerchantPortraits = true;
			this.showPortraits = true;
			this.autoRun = true;
			this.alwaysShowToolHitLocation = false;
			this.hideToolHitLocationWhenInMotion = true;
			this.dialogueTyping = true;
			this.rumble = true;
			this.fullscreen = false;
			this.pinToolbarToggle = false;
			this.baseZoomLevel = 1f;
			this.localCoopBaseZoomLevel = 1f;
			if (Game1.options == this)
			{
				Game1.forceSnapOnNextViewportUpdate = true;
			}
			this.zoomButtons = false;
			this.pauseWhenOutOfFocus = true;
			this.screenFlash = true;
			this.snowTransparency = 1f;
			this.invertScrollDirection = false;
			this.ambientOnlyToggle = false;
			this.showAdvancedCraftingInformation = false;
			this.stowingMode = Options.ItemStowingModes.Off;
			this.useLegacySlingshotFiring = false;
			this.gamepadMode = Options.GamepadModes.Auto;
			this.windowedBorderlessFullscreen = true;
			this.showPlacementTileForGamepad = true;
			this.hardwareCursor = false;
			this.musicVolumeLevel = 0.75f;
			this.ambientVolumeLevel = 0.75f;
			this.footstepVolumeLevel = 0.9f;
			this.soundVolumeLevel = 1f;
			this.dialogueFontScale = 1f;
			DisplayMode displayMode = Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.Last<DisplayMode>();
			this.preferredResolutionX = displayMode.Width;
			this.preferredResolutionY = displayMode.Height;
			this.vsyncEnabled = true;
			GameRunner.instance.OnWindowSizeChange(null, null);
			this.snappyMenus = true;
			this.ipConnectionsEnabled = true;
			this.enableServer = true;
			this.serverPrivacy = ServerPrivacy.FriendsOnly;
			this.enableFarmhandCreation = true;
			this.showMPEndOfNightReadyStatus = false;
			this.muteAnimalSounds = false;
			this.useChineseSmoothFont = false;
			this.useAlternateFont = false;
		}

		// Token: 0x060013B2 RID: 5042 RVA: 0x000F0E7C File Offset: 0x000EF07C
		public void setControlsToDefault()
		{
			this.actionButton = new InputButton[]
			{
				new InputButton(Keys.X),
				new InputButton(false)
			};
			this.cancelButton = new InputButton[]
			{
				new InputButton(Keys.V)
			};
			this.useToolButton = new InputButton[]
			{
				new InputButton(Keys.C),
				new InputButton(true)
			};
			this.moveUpButton = new InputButton[]
			{
				new InputButton(Keys.W)
			};
			this.moveRightButton = new InputButton[]
			{
				new InputButton(Keys.D)
			};
			this.moveDownButton = new InputButton[]
			{
				new InputButton(Keys.S)
			};
			this.moveLeftButton = new InputButton[]
			{
				new InputButton(Keys.A)
			};
			this.menuButton = new InputButton[]
			{
				new InputButton(Keys.E),
				new InputButton(Keys.Escape)
			};
			this.runButton = new InputButton[]
			{
				new InputButton(Keys.LeftShift)
			};
			this.tmpKeyToReplace = new InputButton[]
			{
				new InputButton(Keys.None)
			};
			this.chatButton = new InputButton[]
			{
				new InputButton(Keys.T),
				new InputButton(Keys.OemQuestion)
			};
			this.mapButton = new InputButton[]
			{
				new InputButton(Keys.M)
			};
			this.journalButton = new InputButton[]
			{
				new InputButton(Keys.F)
			};
			this.inventorySlot1 = new InputButton[]
			{
				new InputButton(Keys.D1)
			};
			this.inventorySlot2 = new InputButton[]
			{
				new InputButton(Keys.D2)
			};
			this.inventorySlot3 = new InputButton[]
			{
				new InputButton(Keys.D3)
			};
			this.inventorySlot4 = new InputButton[]
			{
				new InputButton(Keys.D4)
			};
			this.inventorySlot5 = new InputButton[]
			{
				new InputButton(Keys.D5)
			};
			this.inventorySlot6 = new InputButton[]
			{
				new InputButton(Keys.D6)
			};
			this.inventorySlot7 = new InputButton[]
			{
				new InputButton(Keys.D7)
			};
			this.inventorySlot8 = new InputButton[]
			{
				new InputButton(Keys.D8)
			};
			this.inventorySlot9 = new InputButton[]
			{
				new InputButton(Keys.D9)
			};
			this.inventorySlot10 = new InputButton[]
			{
				new InputButton(Keys.D0)
			};
			this.inventorySlot11 = new InputButton[]
			{
				new InputButton(Keys.OemMinus)
			};
			this.inventorySlot12 = new InputButton[]
			{
				new InputButton(Keys.OemPlus)
			};
			this.emoteButton = new InputButton[]
			{
				new InputButton(Keys.Y)
			};
			this.toolbarSwap = new InputButton[]
			{
				new InputButton(Keys.Tab)
			};
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x000F1188 File Offset: 0x000EF388
		public string getNameOfOptionFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4556");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4557");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4558");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4559");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560");
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561");
			case 6:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4562");
			default:
				return "";
			}
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x000F1230 File Offset: 0x000EF430
		public void changeCheckBoxOption(int which, bool value)
		{
			switch (which)
			{
			case 0:
				this.autoRun = value;
				Game1.player.setRunning(this.autoRun, false);
				break;
			case 3:
				this.dialogueTyping = value;
				break;
			case 7:
				this.showPortraits = value;
				break;
			case 8:
				this.showMerchantPortraits = value;
				break;
			case 9:
				this.showMenuBackground = value;
				break;
			case 10:
				this.playFootstepSounds = value;
				break;
			case 11:
				this.alwaysShowToolHitLocation = value;
				break;
			case 12:
				this.hideToolHitLocationWhenInMotion = value;
				break;
			case 14:
				this.pauseWhenOutOfFocus = value;
				break;
			case 15:
				this.pinToolbarToggle = value;
				break;
			case 16:
				this.rumble = value;
				break;
			case 17:
				this.ambientOnlyToggle = value;
				break;
			case 19:
				this.zoomButtons = value;
				break;
			case 22:
				this.invertScrollDirection = value;
				break;
			case 24:
				this.screenFlash = value;
				break;
			case 26:
				this.hardwareCursor = value;
				Program.gamePtr.IsMouseVisible = this.hardwareCursor;
				break;
			case 27:
				this.showPlacementTileForGamepad = value;
				break;
			case 29:
				this.snappyMenus = value;
				break;
			case 30:
				this.ipConnectionsEnabled = value;
				break;
			case 32:
			{
				this.enableFarmhandCreation = value;
				IGameServer server = Game1.server;
				if (server != null)
				{
					server.updateLobbyData();
				}
				break;
			}
			case 34:
				this.showAdvancedCraftingInformation = value;
				break;
			case 35:
				this.showMPEndOfNightReadyStatus = value;
				break;
			case 37:
				this.vsyncEnabled = value;
				GameRunner.instance.OnWindowSizeChange(null, null);
				break;
			case 43:
				this.muteAnimalSounds = value;
				break;
			case 44:
				this.useChineseSmoothFont = value;
				this.loadChineseFonts();
				break;
			case 46:
				this.useAlternateFont = value;
				break;
			}
			this.optionsDirty = true;
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x000F1464 File Offset: 0x000EF664
		public void changeSliderOption(int which, int value)
		{
			if (which <= 2)
			{
				if (which != 1)
				{
					if (which == 2)
					{
						this.soundVolumeLevel = (float)value / 100f;
						Game1.soundCategory.SetVolume(this.soundVolumeLevel);
					}
				}
				else
				{
					this.musicVolumeLevel = (float)value / 100f;
					Game1.musicCategory.SetVolume(this.musicVolumeLevel);
					Game1.musicPlayerVolume = this.musicVolumeLevel;
				}
			}
			else
			{
				switch (which)
				{
				case 18:
				{
					int zoomlvl = (int)(this.desiredBaseZoomLevel * 100f);
					int oldZoom = zoomlvl;
					int newValue = (int)((float)value * 100f);
					if (newValue >= zoomlvl + 10 || newValue >= 100)
					{
						zoomlvl += 10;
						zoomlvl = Math.Min(100, zoomlvl);
					}
					else if (newValue <= zoomlvl - 10 || newValue <= 50)
					{
						zoomlvl -= 10;
						zoomlvl = Math.Max(50, zoomlvl);
					}
					if (zoomlvl != oldZoom)
					{
						this.desiredBaseZoomLevel = (float)zoomlvl / 100f;
						Game1.forceSnapOnNextViewportUpdate = true;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4563") + this.zoomLevel.ToString());
					}
					break;
				}
				case 19:
				case 22:
					break;
				case 20:
					this.ambientVolumeLevel = (float)value / 100f;
					Game1.ambientCategory.SetVolume(this.ambientVolumeLevel);
					Game1.ambientPlayerVolume = this.ambientVolumeLevel;
					break;
				case 21:
					this.footstepVolumeLevel = (float)value / 100f;
					Game1.footstepCategory.SetVolume(this.footstepVolumeLevel);
					break;
				case 23:
					this.snowTransparency = (float)value / 100f;
					break;
				default:
					if (which != 39)
					{
						if (which == 45)
						{
							this.dialogueFontScale = (float)value / 100f + 1f;
						}
					}
					else
					{
						int zoomlvl2 = (int)(this.desiredUIScale * 100f);
						int newValue2 = (int)((float)value * 100f);
						if (newValue2 >= zoomlvl2 + 10 || newValue2 >= 100)
						{
							zoomlvl2 += 10;
							zoomlvl2 = Math.Min(100, zoomlvl2);
						}
						else if (newValue2 <= zoomlvl2 - 10 || newValue2 <= 50)
						{
							zoomlvl2 -= 10;
							zoomlvl2 = Math.Max(50, zoomlvl2);
						}
						this.desiredUIScale = (float)zoomlvl2 / 100f;
					}
					break;
				}
			}
			this.optionsDirty = true;
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x000F1680 File Offset: 0x000EF880
		public void loadChineseFonts()
		{
			if (this.useChineseSmoothFont)
			{
				Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SmallFont");
				Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SpriteFont1");
				SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
				return;
			}
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
			SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x000F16F4 File Offset: 0x000EF8F4
		public void setBackgroundMode(string setting)
		{
			if (setting == "Standard")
			{
				this.showMenuBackground = false;
				this.showClearBackgrounds = false;
				return;
			}
			if (setting == "Graphical")
			{
				this.showMenuBackground = true;
				return;
			}
			if (!(setting == "None"))
			{
				return;
			}
			this.showClearBackgrounds = true;
			this.showMenuBackground = false;
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x000F1750 File Offset: 0x000EF950
		public void setStowingMode(string setting)
		{
			if (setting == "off")
			{
				this.stowingMode = Options.ItemStowingModes.Off;
				return;
			}
			if (setting == "gamepad")
			{
				this.stowingMode = Options.ItemStowingModes.GamepadOnly;
				return;
			}
			if (!(setting == "both"))
			{
				return;
			}
			this.stowingMode = Options.ItemStowingModes.Both;
		}

		// Token: 0x060013B9 RID: 5049 RVA: 0x000F179C File Offset: 0x000EF99C
		public void setSlingshotMode(string setting)
		{
			if (setting == "legacy")
			{
				this.useLegacySlingshotFiring = true;
				return;
			}
			this.useLegacySlingshotFiring = false;
		}

		// Token: 0x060013BA RID: 5050 RVA: 0x000F17BC File Offset: 0x000EF9BC
		public void setBiteChime(string setting)
		{
			try
			{
				Game1.player.biteChime.Value = int.Parse(setting);
			}
			catch (Exception)
			{
				Game1.player.biteChime.Value = -1;
			}
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x000F1804 File Offset: 0x000EFA04
		public void setGamepadMode(string setting)
		{
			if (!(setting == "auto"))
			{
				if (!(setting == "force_on"))
				{
					if (setting == "force_off")
					{
						this.gamepadMode = Options.GamepadModes.ForceOff;
					}
				}
				else
				{
					this.gamepadMode = Options.GamepadModes.ForceOn;
				}
			}
			else
			{
				this.gamepadMode = Options.GamepadModes.Auto;
			}
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(false, false);
				startupPreferences.gamepadMode = this.gamepadMode;
				startupPreferences.savePreferences(false, false);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060013BC RID: 5052 RVA: 0x000F188C File Offset: 0x000EFA8C
		public void setMoveBuildingPermissions(string setting)
		{
			if (setting == "off")
			{
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
				return;
			}
			if (setting == "on")
			{
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
				return;
			}
			if (!(setting == "owned"))
			{
				return;
			}
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
		}

		// Token: 0x060013BD RID: 5053 RVA: 0x000F1904 File Offset: 0x000EFB04
		public void setServerMode(string setting)
		{
			if (setting == "offline")
			{
				this.enableServer = false;
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerOfflineMode);
				return;
			}
			if (!(setting == "friends"))
			{
				if (setting == "invite")
				{
					this.serverPrivacy = ServerPrivacy.InviteOnly;
				}
			}
			else
			{
				this.serverPrivacy = ServerPrivacy.FriendsOnly;
			}
			if (Game1.server == null && Game1.client == null)
			{
				this.enableServer = true;
				Game1.multiplayer.StartServer();
				return;
			}
			if (Game1.server != null)
			{
				this.enableServer = true;
				Game1.server.setPrivacy(this.serverPrivacy);
			}
		}

		// Token: 0x060013BE RID: 5054 RVA: 0x000F199C File Offset: 0x000EFB9C
		public void setWindowedOption(string setting)
		{
			if (setting == "Windowed")
			{
				this.setWindowedOption(1);
				return;
			}
			if (setting == "Fullscreen")
			{
				this.setWindowedOption(2);
				return;
			}
			if (!(setting == "Windowed Borderless"))
			{
				return;
			}
			this.setWindowedOption(0);
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x000F19E8 File Offset: 0x000EFBE8
		public void setWindowedOption(int setting)
		{
			this.windowedBorderlessFullscreen = this.isCurrentlyWindowedBorderless();
			this.fullscreen = (!this.windowedBorderlessFullscreen && Game1.graphics.IsFullScreen);
			int whichMode = -1;
			switch (setting)
			{
			case 0:
				if (!this.windowedBorderlessFullscreen)
				{
					this.windowedBorderlessFullscreen = true;
					Game1.toggleFullscreen();
					this.fullscreen = false;
				}
				whichMode = 0;
				break;
			case 1:
				if (Game1.graphics.IsFullScreen && !this.windowedBorderlessFullscreen)
				{
					this.fullscreen = false;
					Game1.toggleNonBorderlessWindowedFullscreen();
					this.windowedBorderlessFullscreen = false;
				}
				else if (this.windowedBorderlessFullscreen)
				{
					this.fullscreen = false;
					this.windowedBorderlessFullscreen = false;
					Game1.toggleFullscreen();
				}
				whichMode = 1;
				break;
			case 2:
				if (this.windowedBorderlessFullscreen)
				{
					this.fullscreen = true;
					this.windowedBorderlessFullscreen = false;
					Game1.toggleFullscreen();
				}
				else if (!Game1.graphics.IsFullScreen)
				{
					this.fullscreen = true;
					this.windowedBorderlessFullscreen = false;
					Game1.toggleNonBorderlessWindowedFullscreen();
					this.hardwareCursor = false;
					Program.gamePtr.IsMouseVisible = false;
				}
				whichMode = 2;
				break;
			}
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(false, false);
				startupPreferences.windowMode = whichMode;
				startupPreferences.fullscreenResolutionX = this.preferredResolutionX;
				startupPreferences.fullscreenResolutionY = this.preferredResolutionY;
				startupPreferences.displayIndex = GameRunner.instance.Window.GetDisplayIndex();
				startupPreferences.savePreferences(false, false);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060013C0 RID: 5056 RVA: 0x000F1B4C File Offset: 0x000EFD4C
		public void changeDropDownOption(int which, string value)
		{
			if (which <= 13)
			{
				if (which != 6)
				{
					if (which != 9)
					{
						if (which == 13)
						{
							this.setWindowedOption(value);
						}
					}
					else
					{
						this.setBackgroundMode(value);
					}
				}
				else
				{
					string[] array = ArgUtility.SplitBySpace(value);
					int width = Convert.ToInt32(array[0]);
					int height = Convert.ToInt32(array[2]);
					this.preferredResolutionX = width;
					this.preferredResolutionY = height;
					Game1.graphics.PreferredBackBufferWidth = width;
					Game1.graphics.PreferredBackBufferHeight = height;
					if (!this.isCurrentlyWindowed())
					{
						try
						{
							StartupPreferences startupPreferences = new StartupPreferences();
							startupPreferences.loadPreferences(false, false);
							startupPreferences.fullscreenResolutionX = this.preferredResolutionX;
							startupPreferences.fullscreenResolutionY = this.preferredResolutionY;
							startupPreferences.savePreferences(false, false);
						}
						catch (Exception)
						{
						}
					}
					Game1.graphics.ApplyChanges();
					GameRunner.instance.OnWindowSizeChange(null, null);
				}
			}
			else if (which <= 28)
			{
				if (which != 18)
				{
					if (which == 28)
					{
						this.setStowingMode(value);
					}
				}
				else
				{
					int newZoom = Convert.ToInt32(value.Replace("%", ""));
					this.desiredBaseZoomLevel = (float)newZoom / 100f;
					Game1.forceSnapOnNextViewportUpdate = true;
					if (Game1.debrisWeather != null)
					{
						Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
					}
					Game1.randomizeRainPositions();
				}
			}
			else if (which != 31)
			{
				switch (which)
				{
				case 38:
					this.setGamepadMode(value);
					break;
				case 39:
				{
					int newZoom2 = Convert.ToInt32(value.Replace("%", ""));
					this.desiredUIScale = (float)newZoom2 / 100f;
					break;
				}
				case 40:
					this.setMoveBuildingPermissions(value);
					break;
				case 41:
					this.setSlingshotMode(value);
					break;
				case 42:
					this.setBiteChime(value);
					Game1.player.PlayFishBiteChime();
					break;
				}
			}
			else
			{
				this.setServerMode(value);
			}
			this.optionsDirty = true;
		}

		// Token: 0x060013C1 RID: 5057 RVA: 0x000F1D1C File Offset: 0x000EFF1C
		public bool isKeyInUse(Keys key)
		{
			using (List<InputButton>.Enumerator enumerator = this.getAllUsedInputButtons().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.key == key)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060013C2 RID: 5058 RVA: 0x000F1D78 File Offset: 0x000EFF78
		public List<InputButton> getAllUsedInputButtons()
		{
			List<InputButton> list = new List<InputButton>();
			list.AddRange(this.useToolButton);
			list.AddRange(this.actionButton);
			list.AddRange(this.moveUpButton);
			list.AddRange(this.moveRightButton);
			list.AddRange(this.moveDownButton);
			list.AddRange(this.moveLeftButton);
			list.AddRange(this.runButton);
			list.AddRange(this.menuButton);
			list.AddRange(this.journalButton);
			list.AddRange(this.mapButton);
			list.AddRange(this.chatButton);
			list.AddRange(this.inventorySlot1);
			list.AddRange(this.inventorySlot2);
			list.AddRange(this.inventorySlot3);
			list.AddRange(this.inventorySlot4);
			list.AddRange(this.inventorySlot5);
			list.AddRange(this.inventorySlot6);
			list.AddRange(this.inventorySlot7);
			list.AddRange(this.inventorySlot8);
			list.AddRange(this.inventorySlot9);
			list.AddRange(this.inventorySlot10);
			list.AddRange(this.inventorySlot11);
			list.AddRange(this.inventorySlot12);
			list.AddRange(this.toolbarSwap);
			list.AddRange(this.emoteButton);
			return list;
		}

		// Token: 0x060013C3 RID: 5059 RVA: 0x000F1EB8 File Offset: 0x000F00B8
		public void setCheckBoxToProperValue(OptionsCheckbox checkbox)
		{
			switch (checkbox.whichOption)
			{
			case 0:
				checkbox.isChecked = this.autoRun;
				return;
			case 1:
			case 2:
			case 6:
			case 13:
			case 18:
			case 20:
			case 21:
			case 23:
			case 25:
			case 28:
			case 31:
			case 33:
			case 36:
			case 38:
			case 39:
			case 40:
			case 41:
			case 42:
			case 45:
				break;
			case 3:
				checkbox.isChecked = this.dialogueTyping;
				return;
			case 4:
				this.fullscreen = (Game1.graphics.IsFullScreen || this.windowedBorderlessFullscreen);
				checkbox.isChecked = this.fullscreen;
				return;
			case 5:
				checkbox.isChecked = this.windowedBorderlessFullscreen;
				checkbox.greyedOut = !this.fullscreen;
				return;
			case 7:
				checkbox.isChecked = this.showPortraits;
				return;
			case 8:
				checkbox.isChecked = this.showMerchantPortraits;
				return;
			case 9:
				checkbox.isChecked = this.showMenuBackground;
				return;
			case 10:
				checkbox.isChecked = this.playFootstepSounds;
				return;
			case 11:
				checkbox.isChecked = this.alwaysShowToolHitLocation;
				return;
			case 12:
				checkbox.isChecked = this.hideToolHitLocationWhenInMotion;
				return;
			case 14:
				checkbox.isChecked = this.pauseWhenOutOfFocus;
				return;
			case 15:
				checkbox.isChecked = this.pinToolbarToggle;
				return;
			case 16:
				checkbox.isChecked = this.rumble;
				checkbox.greyedOut = !this.gamepadControls;
				return;
			case 17:
				checkbox.isChecked = this.ambientOnlyToggle;
				return;
			case 19:
				checkbox.isChecked = this.zoomButtons;
				return;
			case 22:
				checkbox.isChecked = this.invertScrollDirection;
				return;
			case 24:
				checkbox.isChecked = this.screenFlash;
				return;
			case 26:
				checkbox.isChecked = this._hardwareCursor;
				checkbox.greyedOut = this.fullscreen;
				return;
			case 27:
				checkbox.isChecked = this.showPlacementTileForGamepad;
				checkbox.greyedOut = !this.gamepadControls;
				return;
			case 29:
				checkbox.isChecked = this.snappyMenus;
				return;
			case 30:
				checkbox.isChecked = this.ipConnectionsEnabled;
				return;
			case 32:
				checkbox.isChecked = this.enableFarmhandCreation;
				return;
			case 34:
				checkbox.isChecked = this.showAdvancedCraftingInformation;
				return;
			case 35:
				checkbox.isChecked = this.showMPEndOfNightReadyStatus;
				return;
			case 37:
				checkbox.isChecked = this.vsyncEnabled;
				return;
			case 43:
				checkbox.isChecked = this.muteAnimalSounds;
				return;
			case 44:
				checkbox.isChecked = this.useChineseSmoothFont;
				return;
			case 46:
				checkbox.isChecked = this.useAlternateFont;
				break;
			default:
				return;
			}
		}

		// Token: 0x060013C4 RID: 5060 RVA: 0x000F2150 File Offset: 0x000F0350
		public void setPlusMinusToProperValue(OptionsPlusMinus plusMinus)
		{
			int whichOption = plusMinus.whichOption;
			if (whichOption != 18)
			{
				if (whichOption == 39)
				{
					string currentZoom = Math.Round((double)(this.desiredUIScale * 100f)).ToString() + "%";
					for (int i = 0; i < plusMinus.options.Count; i++)
					{
						if (plusMinus.options[i].Equals(currentZoom))
						{
							plusMinus.selected = i;
							return;
						}
					}
					return;
				}
			}
			else
			{
				string currentZoom2 = Math.Round((double)(this.desiredBaseZoomLevel * 100f)).ToString() + "%";
				for (int j = 0; j < plusMinus.options.Count; j++)
				{
					if (plusMinus.options[j].Equals(currentZoom2))
					{
						plusMinus.selected = j;
						return;
					}
				}
			}
		}

		// Token: 0x060013C5 RID: 5061 RVA: 0x000F2228 File Offset: 0x000F0428
		public void setSliderToProperValue(OptionsSlider slider)
		{
			int whichOption = slider.whichOption;
			if (whichOption > 2)
			{
				switch (whichOption)
				{
				case 18:
					slider.value = (int)(this.desiredBaseZoomLevel * 100f);
					return;
				case 19:
				case 22:
					break;
				case 20:
					slider.value = (int)(this.ambientVolumeLevel * 100f);
					return;
				case 21:
					slider.value = (int)(this.footstepVolumeLevel * 100f);
					return;
				case 23:
					slider.value = (int)(this.snowTransparency * 100f);
					return;
				default:
					if (whichOption != 39)
					{
						if (whichOption == 45)
						{
							slider.value = (int)((this.dialogueFontScale - 1f) * 100f);
							return;
						}
					}
					else
					{
						slider.value = (int)(this.desiredUIScale * 100f);
					}
					break;
				}
				return;
			}
			if (whichOption == 1)
			{
				slider.value = (int)(this.musicVolumeLevel * 100f);
				return;
			}
			if (whichOption != 2)
			{
				return;
			}
			slider.value = (int)(this.soundVolumeLevel * 100f);
		}

		// Token: 0x060013C6 RID: 5062 RVA: 0x000F2320 File Offset: 0x000F0520
		public bool doesInputListContain(InputButton[] list, Keys key)
		{
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].key == key)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x000F2350 File Offset: 0x000F0550
		public void changeInputListenerValue(int whichListener, Keys key)
		{
			switch (whichListener)
			{
			case 7:
				this.actionButton[0] = new InputButton(key);
				break;
			case 10:
				this.useToolButton[0] = new InputButton(key);
				break;
			case 11:
				this.moveUpButton[0] = new InputButton(key);
				break;
			case 12:
				this.moveRightButton[0] = new InputButton(key);
				break;
			case 13:
				this.moveDownButton[0] = new InputButton(key);
				break;
			case 14:
				this.moveLeftButton[0] = new InputButton(key);
				break;
			case 15:
				this.menuButton[0] = new InputButton(key);
				break;
			case 16:
				this.runButton[0] = new InputButton(key);
				break;
			case 17:
				this.chatButton[0] = new InputButton(key);
				break;
			case 18:
				this.journalButton[0] = new InputButton(key);
				break;
			case 19:
				this.mapButton[0] = new InputButton(key);
				break;
			case 20:
				this.inventorySlot1[0] = new InputButton(key);
				break;
			case 21:
				this.inventorySlot2[0] = new InputButton(key);
				break;
			case 22:
				this.inventorySlot3[0] = new InputButton(key);
				break;
			case 23:
				this.inventorySlot4[0] = new InputButton(key);
				break;
			case 24:
				this.inventorySlot5[0] = new InputButton(key);
				break;
			case 25:
				this.inventorySlot6[0] = new InputButton(key);
				break;
			case 26:
				this.inventorySlot7[0] = new InputButton(key);
				break;
			case 27:
				this.inventorySlot8[0] = new InputButton(key);
				break;
			case 28:
				this.inventorySlot9[0] = new InputButton(key);
				break;
			case 29:
				this.inventorySlot10[0] = new InputButton(key);
				break;
			case 30:
				this.inventorySlot11[0] = new InputButton(key);
				break;
			case 31:
				this.inventorySlot12[0] = new InputButton(key);
				break;
			case 32:
				this.toolbarSwap[0] = new InputButton(key);
				break;
			case 33:
				this.emoteButton[0] = new InputButton(key);
				break;
			}
			this.optionsDirty = true;
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x000F2608 File Offset: 0x000F0808
		public void setInputListenerToProperValue(OptionsInputListener inputListener)
		{
			inputListener.buttonNames.Clear();
			switch (inputListener.whichOption)
			{
			case 7:
				foreach (InputButton b in this.actionButton)
				{
					inputListener.buttonNames.Add(b.ToString());
				}
				return;
			case 8:
			case 9:
				break;
			case 10:
				foreach (InputButton b2 in this.useToolButton)
				{
					inputListener.buttonNames.Add(b2.ToString());
				}
				return;
			case 11:
				foreach (InputButton b3 in this.moveUpButton)
				{
					inputListener.buttonNames.Add(b3.ToString());
				}
				return;
			case 12:
				foreach (InputButton b4 in this.moveRightButton)
				{
					inputListener.buttonNames.Add(b4.ToString());
				}
				return;
			case 13:
				foreach (InputButton b5 in this.moveDownButton)
				{
					inputListener.buttonNames.Add(b5.ToString());
				}
				return;
			case 14:
				foreach (InputButton b6 in this.moveLeftButton)
				{
					inputListener.buttonNames.Add(b6.ToString());
				}
				return;
			case 15:
				foreach (InputButton b7 in this.menuButton)
				{
					inputListener.buttonNames.Add(b7.ToString());
				}
				return;
			case 16:
				foreach (InputButton b8 in this.runButton)
				{
					inputListener.buttonNames.Add(b8.ToString());
				}
				return;
			case 17:
				foreach (InputButton b9 in this.chatButton)
				{
					inputListener.buttonNames.Add(b9.ToString());
				}
				return;
			case 18:
				foreach (InputButton b10 in this.journalButton)
				{
					inputListener.buttonNames.Add(b10.ToString());
				}
				return;
			case 19:
				foreach (InputButton b11 in this.mapButton)
				{
					inputListener.buttonNames.Add(b11.ToString());
				}
				return;
			case 20:
				foreach (InputButton b12 in this.inventorySlot1)
				{
					inputListener.buttonNames.Add(b12.ToString());
				}
				return;
			case 21:
				foreach (InputButton b13 in this.inventorySlot2)
				{
					inputListener.buttonNames.Add(b13.ToString());
				}
				return;
			case 22:
				foreach (InputButton b14 in this.inventorySlot3)
				{
					inputListener.buttonNames.Add(b14.ToString());
				}
				return;
			case 23:
				foreach (InputButton b15 in this.inventorySlot4)
				{
					inputListener.buttonNames.Add(b15.ToString());
				}
				return;
			case 24:
				foreach (InputButton b16 in this.inventorySlot5)
				{
					inputListener.buttonNames.Add(b16.ToString());
				}
				return;
			case 25:
				foreach (InputButton b17 in this.inventorySlot6)
				{
					inputListener.buttonNames.Add(b17.ToString());
				}
				return;
			case 26:
				foreach (InputButton b18 in this.inventorySlot7)
				{
					inputListener.buttonNames.Add(b18.ToString());
				}
				return;
			case 27:
				foreach (InputButton b19 in this.inventorySlot8)
				{
					inputListener.buttonNames.Add(b19.ToString());
				}
				return;
			case 28:
				foreach (InputButton b20 in this.inventorySlot9)
				{
					inputListener.buttonNames.Add(b20.ToString());
				}
				return;
			case 29:
				foreach (InputButton b21 in this.inventorySlot10)
				{
					inputListener.buttonNames.Add(b21.ToString());
				}
				return;
			case 30:
				foreach (InputButton b22 in this.inventorySlot11)
				{
					inputListener.buttonNames.Add(b22.ToString());
				}
				return;
			case 31:
				foreach (InputButton b23 in this.inventorySlot12)
				{
					inputListener.buttonNames.Add(b23.ToString());
				}
				return;
			case 32:
				foreach (InputButton b24 in this.toolbarSwap)
				{
					inputListener.buttonNames.Add(b24.ToString());
				}
				return;
			case 33:
				foreach (InputButton b25 in this.emoteButton)
				{
					inputListener.buttonNames.Add(b25.ToString());
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x000F2BFC File Offset: 0x000F0DFC
		public void setDropDownToProperValue(OptionsDropDown dropDown)
		{
			int whichOption = dropDown.whichOption;
			if (whichOption <= 13)
			{
				if (whichOption == 6)
				{
					try
					{
						StartupPreferences startupPreferences = new StartupPreferences();
						startupPreferences.loadPreferences(false, false);
						if (startupPreferences.fullscreenResolutionX != 0)
						{
							this.preferredResolutionX = startupPreferences.fullscreenResolutionX;
							this.preferredResolutionY = startupPreferences.fullscreenResolutionY;
						}
					}
					catch (Exception)
					{
					}
					int i = 0;
					foreach (DisplayMode v in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
					{
						if (v.Width >= 1280)
						{
							dropDown.dropDownOptions.Add(v.Width.ToString() + " x " + v.Height.ToString());
							dropDown.dropDownDisplayOptions.Add(v.Width.ToString() + " x " + v.Height.ToString());
							if (v.Width == this.preferredResolutionX && v.Height == this.preferredResolutionY)
							{
								dropDown.selectedOption = i;
							}
							i++;
						}
					}
					dropDown.greyedOut = (!this.fullscreen || this.windowedBorderlessFullscreen);
					return;
				}
				if (whichOption != 9)
				{
					if (whichOption != 13)
					{
						return;
					}
					this.windowedBorderlessFullscreen = this.isCurrentlyWindowedBorderless();
					this.fullscreen = (Game1.graphics.IsFullScreen && !this.windowedBorderlessFullscreen);
					dropDown.dropDownOptions.Add("Windowed");
					if (!this.windowedBorderlessFullscreen)
					{
						dropDown.dropDownOptions.Add("Fullscreen");
					}
					if (!this.fullscreen)
					{
						dropDown.dropDownOptions.Add("Windowed Borderless");
					}
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4564"));
					if (!this.windowedBorderlessFullscreen)
					{
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"));
					}
					if (!this.fullscreen)
					{
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"));
					}
					if (Game1.graphics.IsFullScreen || this.windowedBorderlessFullscreen)
					{
						dropDown.selectedOption = 1;
						return;
					}
					dropDown.selectedOption = 0;
					return;
				}
				else
				{
					dropDown.dropDownOptions.Add("Standard");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_0"));
					dropDown.dropDownOptions.Add("Graphical");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_1"));
					dropDown.dropDownOptions.Add("None");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_2"));
					if (this.showMenuBackground)
					{
						dropDown.selectedOption = 1;
						return;
					}
					if (!this.showClearBackgrounds)
					{
						dropDown.selectedOption = 0;
						return;
					}
					dropDown.selectedOption = 2;
					return;
				}
			}
			else if (whichOption != 28)
			{
				if (whichOption != 31)
				{
					switch (whichOption)
					{
					case 38:
						try
						{
							StartupPreferences startupPreferences2 = new StartupPreferences();
							startupPreferences2.loadPreferences(false, false);
							this.gamepadMode = startupPreferences2.gamepadMode;
						}
						catch (Exception)
						{
						}
						dropDown.dropDownOptions.Add("auto");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_Auto"));
						dropDown.dropDownOptions.Add("force_on");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOn"));
						dropDown.dropDownOptions.Add("force_off");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOff"));
						switch (this.gamepadMode)
						{
						case Options.GamepadModes.Auto:
							dropDown.selectedOption = 0;
							return;
						case Options.GamepadModes.ForceOn:
							dropDown.selectedOption = 1;
							return;
						case Options.GamepadModes.ForceOff:
							dropDown.selectedOption = 2;
							return;
						default:
							return;
						}
						break;
					case 39:
						break;
					case 40:
						dropDown.dropDownOptions.Add("on");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_On"));
						dropDown.dropDownOptions.Add("owned");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Owned"));
						dropDown.dropDownOptions.Add("off");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Off"));
						switch (Game1.player.team.farmhandsCanMoveBuildings.Value)
						{
						case FarmerTeam.RemoteBuildingPermissions.Off:
							dropDown.selectedOption = 2;
							break;
						case FarmerTeam.RemoteBuildingPermissions.OwnedBuildings:
							dropDown.selectedOption = 1;
							return;
						case FarmerTeam.RemoteBuildingPermissions.On:
							dropDown.selectedOption = 0;
							return;
						default:
							return;
						}
						break;
					case 41:
						dropDown.dropDownOptions.Add("hold");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Hold"));
						dropDown.dropDownOptions.Add("legacy");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Pull"));
						if (this.useLegacySlingshotFiring)
						{
							dropDown.selectedOption = 1;
							return;
						}
						dropDown.selectedOption = 0;
						return;
					case 42:
						dropDown.dropDownOptions.Add("-1");
						dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime_Default"));
						for (int j = 0; j <= 3; j++)
						{
							dropDown.dropDownOptions.Add(j.ToString());
							dropDown.dropDownDisplayOptions.Add((j + 1).ToString());
						}
						dropDown.selectedOption = Game1.player.biteChime.Value + 1;
						return;
					default:
						return;
					}
					return;
				}
				dropDown.dropDownOptions.Add("offline");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Offline"));
				if (Program.sdk.Networking != null)
				{
					dropDown.dropDownOptions.Add("friends");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_FriendsOnly"));
					dropDown.dropDownOptions.Add("invite");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_InviteOnly"));
				}
				else
				{
					dropDown.dropDownOptions.Add("online");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Online"));
				}
				if (Game1.server == null)
				{
					dropDown.selectedOption = 0;
				}
				else if (Program.sdk.Networking != null)
				{
					ServerPrivacy serverPrivacy = this.serverPrivacy;
					if (serverPrivacy != ServerPrivacy.InviteOnly)
					{
						if (serverPrivacy == ServerPrivacy.FriendsOnly)
						{
							dropDown.selectedOption = 1;
						}
					}
					else
					{
						dropDown.selectedOption = 2;
					}
				}
				else
				{
					dropDown.selectedOption = 1;
				}
				Game1.log.Verbose("setDropDownToProperValue( serverMode, " + dropDown.dropDownOptions[dropDown.selectedOption] + " ) called.");
				return;
			}
			else
			{
				dropDown.dropDownOptions.Add("off");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_Off"));
				dropDown.dropDownOptions.Add("gamepad");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_GamepadOnly"));
				dropDown.dropDownOptions.Add("both");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_On"));
				switch (this.stowingMode)
				{
				case Options.ItemStowingModes.Off:
					dropDown.selectedOption = 0;
					return;
				case Options.ItemStowingModes.GamepadOnly:
					dropDown.selectedOption = 1;
					return;
				case Options.ItemStowingModes.Both:
					dropDown.selectedOption = 2;
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x000F3398 File Offset: 0x000F1598
		public bool isCurrentlyWindowedBorderless()
		{
			return Game1.graphics.IsFullScreen && !Game1.graphics.HardwareModeSwitch;
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x000F33B5 File Offset: 0x000F15B5
		public bool isCurrentlyFullscreen()
		{
			return Game1.graphics.IsFullScreen && Game1.graphics.HardwareModeSwitch;
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x000F33CF File Offset: 0x000F15CF
		public bool isCurrentlyWindowed()
		{
			return !this.isCurrentlyWindowedBorderless() && !this.isCurrentlyFullscreen();
		}

		// Token: 0x04000BEF RID: 3055
		public const float minZoom = 0.75f;

		// Token: 0x04000BF0 RID: 3056
		public const float maxZoom = 2f;

		// Token: 0x04000BF1 RID: 3057
		public const float minUIZoom = 0.75f;

		// Token: 0x04000BF2 RID: 3058
		public const float maxUIZoom = 1.5f;

		// Token: 0x04000BF3 RID: 3059
		public const int toggleAutoRun = 0;

		// Token: 0x04000BF4 RID: 3060
		public const int musicVolume = 1;

		// Token: 0x04000BF5 RID: 3061
		public const int soundVolume = 2;

		// Token: 0x04000BF6 RID: 3062
		public const int toggleDialogueTypingSounds = 3;

		// Token: 0x04000BF7 RID: 3063
		public const int toggleFullscreen = 4;

		// Token: 0x04000BF8 RID: 3064
		public const int screenResolution = 6;

		// Token: 0x04000BF9 RID: 3065
		public const int showPortraitsToggle = 7;

		// Token: 0x04000BFA RID: 3066
		public const int showMerchantPortraitsToggle = 8;

		// Token: 0x04000BFB RID: 3067
		public const int menuBG = 9;

		// Token: 0x04000BFC RID: 3068
		public const int toggleFootsteps = 10;

		// Token: 0x04000BFD RID: 3069
		public const int alwaysShowToolHitLocationToggle = 11;

		// Token: 0x04000BFE RID: 3070
		public const int hideToolHitLocationWhenInMotionToggle = 12;

		// Token: 0x04000BFF RID: 3071
		public const int windowMode = 13;

		// Token: 0x04000C00 RID: 3072
		public const int pauseWhenUnfocused = 14;

		// Token: 0x04000C01 RID: 3073
		public const int pinToolbar = 15;

		// Token: 0x04000C02 RID: 3074
		public const int toggleRumble = 16;

		// Token: 0x04000C03 RID: 3075
		public const int ambientOnly = 17;

		// Token: 0x04000C04 RID: 3076
		public const int zoom = 18;

		// Token: 0x04000C05 RID: 3077
		public const int zoomButtonsToggle = 19;

		// Token: 0x04000C06 RID: 3078
		public const int ambientVolume = 20;

		// Token: 0x04000C07 RID: 3079
		public const int footstepVolume = 21;

		// Token: 0x04000C08 RID: 3080
		public const int invertScrollDirectionToggle = 22;

		// Token: 0x04000C09 RID: 3081
		public const int snowTransparencyToggle = 23;

		// Token: 0x04000C0A RID: 3082
		public const int screenFlashToggle = 24;

		// Token: 0x04000C0B RID: 3083
		public const int toggleHardwareCursor = 26;

		// Token: 0x04000C0C RID: 3084
		public const int toggleShowPlacementTileGamepad = 27;

		// Token: 0x04000C0D RID: 3085
		public const int stowingModeSelect = 28;

		// Token: 0x04000C0E RID: 3086
		public const int toggleSnappyMenus = 29;

		// Token: 0x04000C0F RID: 3087
		public const int toggleIPConnections = 30;

		// Token: 0x04000C10 RID: 3088
		public const int serverMode = 31;

		// Token: 0x04000C11 RID: 3089
		public const int toggleFarmhandCreation = 32;

		// Token: 0x04000C12 RID: 3090
		public const int toggleShowAdvancedCraftingInformation = 34;

		// Token: 0x04000C13 RID: 3091
		public const int toggleMPReadyStatus = 35;

		// Token: 0x04000C14 RID: 3092
		public const int mapScreenshot = 36;

		// Token: 0x04000C15 RID: 3093
		public const int toggleVsync = 37;

		// Token: 0x04000C16 RID: 3094
		public const int gamepadModeSelect = 38;

		// Token: 0x04000C17 RID: 3095
		public const int uiScaleSlider = 39;

		// Token: 0x04000C18 RID: 3096
		public const int moveBuildingPermissions = 40;

		// Token: 0x04000C19 RID: 3097
		public const int slingshotModeSelect = 41;

		// Token: 0x04000C1A RID: 3098
		public const int biteChime = 42;

		// Token: 0x04000C1B RID: 3099
		public const int toggleMuteAnimalSounds = 43;

		// Token: 0x04000C1C RID: 3100
		public const int toggleUseChineseSmoothFont = 44;

		// Token: 0x04000C1D RID: 3101
		public const int dialogueFontToggle = 45;

		// Token: 0x04000C1E RID: 3102
		public const int toggleUseAlternateFont = 46;

		// Token: 0x04000C1F RID: 3103
		public const int input_actionButton = 7;

		// Token: 0x04000C20 RID: 3104
		public const int input_cancelButton = 9;

		// Token: 0x04000C21 RID: 3105
		public const int input_useToolButton = 10;

		// Token: 0x04000C22 RID: 3106
		public const int input_moveUpButton = 11;

		// Token: 0x04000C23 RID: 3107
		public const int input_moveRightButton = 12;

		// Token: 0x04000C24 RID: 3108
		public const int input_moveDownButton = 13;

		// Token: 0x04000C25 RID: 3109
		public const int input_moveLeftButton = 14;

		// Token: 0x04000C26 RID: 3110
		public const int input_menuButton = 15;

		// Token: 0x04000C27 RID: 3111
		public const int input_runButton = 16;

		// Token: 0x04000C28 RID: 3112
		public const int input_chatButton = 17;

		// Token: 0x04000C29 RID: 3113
		public const int input_journalButton = 18;

		// Token: 0x04000C2A RID: 3114
		public const int input_mapButton = 19;

		// Token: 0x04000C2B RID: 3115
		public const int input_slot1 = 20;

		// Token: 0x04000C2C RID: 3116
		public const int input_slot2 = 21;

		// Token: 0x04000C2D RID: 3117
		public const int input_slot3 = 22;

		// Token: 0x04000C2E RID: 3118
		public const int input_slot4 = 23;

		// Token: 0x04000C2F RID: 3119
		public const int input_slot5 = 24;

		// Token: 0x04000C30 RID: 3120
		public const int input_slot6 = 25;

		// Token: 0x04000C31 RID: 3121
		public const int input_slot7 = 26;

		// Token: 0x04000C32 RID: 3122
		public const int input_slot8 = 27;

		// Token: 0x04000C33 RID: 3123
		public const int input_slot9 = 28;

		// Token: 0x04000C34 RID: 3124
		public const int input_slot10 = 29;

		// Token: 0x04000C35 RID: 3125
		public const int input_slot11 = 30;

		// Token: 0x04000C36 RID: 3126
		public const int input_slot12 = 31;

		// Token: 0x04000C37 RID: 3127
		public const int input_toolbarSwap = 32;

		// Token: 0x04000C38 RID: 3128
		public const int input_emoteButton = 33;

		// Token: 0x04000C39 RID: 3129
		public const float defaultZoomLevel = 1f;

		// Token: 0x04000C3A RID: 3130
		public const int defaultLightingQuality = 8;

		// Token: 0x04000C3B RID: 3131
		public const float defaultSplitScreenZoomLevel = 1f;

		// Token: 0x04000C3C RID: 3132
		public bool autoRun;

		// Token: 0x04000C3D RID: 3133
		public bool dialogueTyping;

		// Token: 0x04000C3E RID: 3134
		public bool showPortraits;

		// Token: 0x04000C3F RID: 3135
		public bool showMerchantPortraits;

		// Token: 0x04000C40 RID: 3136
		public bool showMenuBackground;

		// Token: 0x04000C41 RID: 3137
		public bool playFootstepSounds;

		// Token: 0x04000C42 RID: 3138
		public bool alwaysShowToolHitLocation;

		// Token: 0x04000C43 RID: 3139
		public bool hideToolHitLocationWhenInMotion;

		// Token: 0x04000C44 RID: 3140
		public bool pauseWhenOutOfFocus;

		// Token: 0x04000C45 RID: 3141
		public bool pinToolbarToggle;

		// Token: 0x04000C46 RID: 3142
		public bool mouseControls;

		// Token: 0x04000C47 RID: 3143
		public bool gamepadControls;

		// Token: 0x04000C48 RID: 3144
		public bool rumble;

		// Token: 0x04000C49 RID: 3145
		public bool ambientOnlyToggle;

		// Token: 0x04000C4A RID: 3146
		public bool zoomButtons;

		// Token: 0x04000C4B RID: 3147
		public bool invertScrollDirection;

		// Token: 0x04000C4C RID: 3148
		public bool screenFlash;

		// Token: 0x04000C4D RID: 3149
		public bool showPlacementTileForGamepad;

		// Token: 0x04000C4E RID: 3150
		public bool snappyMenus;

		// Token: 0x04000C4F RID: 3151
		public bool showAdvancedCraftingInformation;

		// Token: 0x04000C50 RID: 3152
		public bool showMPEndOfNightReadyStatus;

		// Token: 0x04000C51 RID: 3153
		public bool muteAnimalSounds;

		// Token: 0x04000C52 RID: 3154
		public bool vsyncEnabled;

		// Token: 0x04000C53 RID: 3155
		public bool fullscreen;

		// Token: 0x04000C54 RID: 3156
		public bool windowedBorderlessFullscreen;

		// Token: 0x04000C55 RID: 3157
		public bool showClearBackgrounds;

		// Token: 0x04000C56 RID: 3158
		public bool useChineseSmoothFont;

		// Token: 0x04000C57 RID: 3159
		public bool useAlternateFont;

		// Token: 0x04000C58 RID: 3160
		[DontLoadDefaultSetting]
		public bool ipConnectionsEnabled;

		// Token: 0x04000C59 RID: 3161
		[DontLoadDefaultSetting]
		public bool enableServer;

		// Token: 0x04000C5A RID: 3162
		[DontLoadDefaultSetting]
		public bool enableFarmhandCreation;

		// Token: 0x04000C5B RID: 3163
		protected bool _hardwareCursor;

		// Token: 0x04000C5C RID: 3164
		public Options.ItemStowingModes stowingMode;

		// Token: 0x04000C5D RID: 3165
		[DontLoadDefaultSetting]
		public Options.GamepadModes gamepadMode;

		// Token: 0x04000C5E RID: 3166
		public bool useLegacySlingshotFiring;

		// Token: 0x04000C5F RID: 3167
		public float musicVolumeLevel;

		// Token: 0x04000C60 RID: 3168
		public float soundVolumeLevel;

		// Token: 0x04000C61 RID: 3169
		public float footstepVolumeLevel;

		// Token: 0x04000C62 RID: 3170
		public float ambientVolumeLevel;

		// Token: 0x04000C63 RID: 3171
		public float snowTransparency;

		// Token: 0x04000C64 RID: 3172
		public float dialogueFontScale = 1f;

		// Token: 0x04000C65 RID: 3173
		[XmlIgnore]
		public float baseZoomLevel = 1f;

		// Token: 0x04000C66 RID: 3174
		[DontLoadDefaultSetting]
		[XmlElement("zoomLevel")]
		public float singlePlayerBaseZoomLevel = 1f;

		// Token: 0x04000C67 RID: 3175
		[DontLoadDefaultSetting]
		public float localCoopBaseZoomLevel = 1f;

		// Token: 0x04000C68 RID: 3176
		[DontLoadDefaultSetting]
		[XmlElement("uiScale")]
		public float singlePlayerDesiredUIScale = -1f;

		// Token: 0x04000C69 RID: 3177
		[DontLoadDefaultSetting]
		public float localCoopDesiredUIScale = 1.5f;

		// Token: 0x04000C6A RID: 3178
		[XmlIgnore]
		public float baseUIScale = 1f;

		// Token: 0x04000C6B RID: 3179
		public int preferredResolutionX;

		// Token: 0x04000C6C RID: 3180
		public int preferredResolutionY;

		// Token: 0x04000C6D RID: 3181
		[DontLoadDefaultSetting]
		public ServerPrivacy serverPrivacy = ServerPrivacy.FriendsOnly;

		// Token: 0x04000C6E RID: 3182
		public InputButton[] actionButton = new InputButton[]
		{
			new InputButton(Keys.X),
			new InputButton(false)
		};

		// Token: 0x04000C6F RID: 3183
		public InputButton[] cancelButton = new InputButton[]
		{
			new InputButton(Keys.V)
		};

		// Token: 0x04000C70 RID: 3184
		public InputButton[] useToolButton = new InputButton[]
		{
			new InputButton(Keys.C),
			new InputButton(true)
		};

		// Token: 0x04000C71 RID: 3185
		public InputButton[] moveUpButton = new InputButton[]
		{
			new InputButton(Keys.W)
		};

		// Token: 0x04000C72 RID: 3186
		public InputButton[] moveRightButton = new InputButton[]
		{
			new InputButton(Keys.D)
		};

		// Token: 0x04000C73 RID: 3187
		public InputButton[] moveDownButton = new InputButton[]
		{
			new InputButton(Keys.S)
		};

		// Token: 0x04000C74 RID: 3188
		public InputButton[] moveLeftButton = new InputButton[]
		{
			new InputButton(Keys.A)
		};

		// Token: 0x04000C75 RID: 3189
		public InputButton[] menuButton = new InputButton[]
		{
			new InputButton(Keys.E),
			new InputButton(Keys.Escape)
		};

		// Token: 0x04000C76 RID: 3190
		public InputButton[] runButton = new InputButton[]
		{
			new InputButton(Keys.LeftShift)
		};

		// Token: 0x04000C77 RID: 3191
		public InputButton[] tmpKeyToReplace = new InputButton[]
		{
			new InputButton(Keys.None)
		};

		// Token: 0x04000C78 RID: 3192
		public InputButton[] chatButton = new InputButton[]
		{
			new InputButton(Keys.T),
			new InputButton(Keys.OemQuestion)
		};

		// Token: 0x04000C79 RID: 3193
		public InputButton[] mapButton = new InputButton[]
		{
			new InputButton(Keys.M)
		};

		// Token: 0x04000C7A RID: 3194
		public InputButton[] journalButton = new InputButton[]
		{
			new InputButton(Keys.F)
		};

		// Token: 0x04000C7B RID: 3195
		public InputButton[] inventorySlot1 = new InputButton[]
		{
			new InputButton(Keys.D1)
		};

		// Token: 0x04000C7C RID: 3196
		public InputButton[] inventorySlot2 = new InputButton[]
		{
			new InputButton(Keys.D2)
		};

		// Token: 0x04000C7D RID: 3197
		public InputButton[] inventorySlot3 = new InputButton[]
		{
			new InputButton(Keys.D3)
		};

		// Token: 0x04000C7E RID: 3198
		public InputButton[] inventorySlot4 = new InputButton[]
		{
			new InputButton(Keys.D4)
		};

		// Token: 0x04000C7F RID: 3199
		public InputButton[] inventorySlot5 = new InputButton[]
		{
			new InputButton(Keys.D5)
		};

		// Token: 0x04000C80 RID: 3200
		public InputButton[] inventorySlot6 = new InputButton[]
		{
			new InputButton(Keys.D6)
		};

		// Token: 0x04000C81 RID: 3201
		public InputButton[] inventorySlot7 = new InputButton[]
		{
			new InputButton(Keys.D7)
		};

		// Token: 0x04000C82 RID: 3202
		public InputButton[] inventorySlot8 = new InputButton[]
		{
			new InputButton(Keys.D8)
		};

		// Token: 0x04000C83 RID: 3203
		public InputButton[] inventorySlot9 = new InputButton[]
		{
			new InputButton(Keys.D9)
		};

		// Token: 0x04000C84 RID: 3204
		public InputButton[] inventorySlot10 = new InputButton[]
		{
			new InputButton(Keys.D0)
		};

		// Token: 0x04000C85 RID: 3205
		public InputButton[] inventorySlot11 = new InputButton[]
		{
			new InputButton(Keys.OemMinus)
		};

		// Token: 0x04000C86 RID: 3206
		public InputButton[] inventorySlot12 = new InputButton[]
		{
			new InputButton(Keys.OemPlus)
		};

		// Token: 0x04000C87 RID: 3207
		public InputButton[] toolbarSwap = new InputButton[]
		{
			new InputButton(Keys.Tab)
		};

		// Token: 0x04000C88 RID: 3208
		public InputButton[] emoteButton = new InputButton[]
		{
			new InputButton(Keys.Y)
		};

		// Token: 0x04000C89 RID: 3209
		[XmlIgnore]
		public bool optionsDirty;

		// Token: 0x04000C8A RID: 3210
		[XmlIgnore]
		private XmlSerializer defaultSettingsSerializer = SaveSerializer.GetSerializer(typeof(Options));

		// Token: 0x04000C8B RID: 3211
		private int appliedLightingQuality = -1;

		// Token: 0x020004D0 RID: 1232
		public enum ItemStowingModes
		{
			// Token: 0x04002989 RID: 10633
			Off,
			// Token: 0x0400298A RID: 10634
			GamepadOnly,
			// Token: 0x0400298B RID: 10635
			Both
		}

		// Token: 0x020004D1 RID: 1233
		public enum GamepadModes
		{
			// Token: 0x0400298D RID: 10637
			Auto,
			// Token: 0x0400298E RID: 10638
			ForceOn,
			// Token: 0x0400298F RID: 10639
			ForceOff
		}
	}
}
