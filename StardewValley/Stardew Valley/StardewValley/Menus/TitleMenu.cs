using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Minigames;

namespace StardewValley.Menus
{
	// Token: 0x020002B4 RID: 692
	public class TitleMenu : IClickableMenu, IDisposable
	{
		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06002D2B RID: 11563 RVA: 0x00230658 File Offset: 0x0022E858
		// (remove) Token: 0x06002D2C RID: 11564 RVA: 0x0023068C File Offset: 0x0022E88C
		public static event Action OnCreatedNewCharacter;

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06002D2D RID: 11565 RVA: 0x002306BF File Offset: 0x0022E8BF
		// (set) Token: 0x06002D2E RID: 11566 RVA: 0x002306C8 File Offset: 0x0022E8C8
		public static IClickableMenu subMenu
		{
			get
			{
				return TitleMenu._subMenu;
			}
			set
			{
				if (TitleMenu._subMenu != null)
				{
					TitleMenu._subMenu.exitFunction = null;
					IDisposable disposable = TitleMenu._subMenu as IDisposable;
					if (disposable != null && !TitleMenu.subMenu.HasDependencies())
					{
						disposable.Dispose();
					}
				}
				TitleMenu._subMenu = value;
				if (TitleMenu._subMenu != null)
				{
					TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
					if (titleMenu != null)
					{
						IClickableMenu subMenu = TitleMenu._subMenu;
						subMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(subMenu.exitFunction, new IClickableMenu.onExit(titleMenu.CloseSubMenu));
					}
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						TitleMenu._subMenu.snapToDefaultClickableComponent();
					}
				}
			}
		}

		// Token: 0x06002D2F RID: 11567 RVA: 0x0023076A File Offset: 0x0022E96A
		public static void ReturnToMainTitleScreen()
		{
			TitleMenu.subMenu = null;
			Game1.game1.ResetGameStateOnTitleScreen();
		}

		// Token: 0x06002D30 RID: 11568 RVA: 0x0023077C File Offset: 0x0022E97C
		public void ForceSubmenu(IClickableMenu menu)
		{
			this.skipToTitleButtons();
			TitleMenu.subMenu = menu;
			this.moveFeatures(1920, 0);
			this.globalXOffset = 1920;
			this.buttonsToShow = 4;
			this.showButtonsTimer = 0;
			this.viewportDY = 0f;
			this.logoSwipeTimer = 0f;
			this.titleInPosition = true;
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06002D31 RID: 11569 RVA: 0x002307D7 File Offset: 0x0022E9D7
		public bool HasActiveUser
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002D32 RID: 11570 RVA: 0x002307DC File Offset: 0x0022E9DC
		public TitleMenu() : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, false)
		{
			LocalizedContentManager.OnLanguageChange += this.OnLanguageChange;
			this.cloudsTexture = this.menuContent.Load<Texture2D>("Minigames\\Clouds");
			this.titleButtonsTexture = this.menuContent.Load<Texture2D>("Minigames\\TitleButtons");
			if (Program.sdk.IsJapaneseRegionRelease)
			{
				this.amuzioTexture = this.menuContent.Load<Texture2D>("Minigames\\Amuzio");
			}
			this.viewportY = 0f;
			this.fadeFromWhiteTimer = 4000;
			this.logoFadeTimer = 5000;
			if (Program.sdk.IsJapaneseRegionRelease)
			{
				this.amuzioTimer = 4000;
			}
			this.bigClouds.Add((float)(this.width * 3 / 4));
			this.shades = Game1.random.NextBool();
			this.smallClouds.Add((float)(this.width - 1));
			this.smallClouds.Add((float)(this.width - 1 + 230 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width * 2 / 3));
			this.smallClouds.Add((float)(this.width / 8));
			this.smallClouds.Add((float)(this.width - 1 + 430 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width * 3 / 4));
			this.smallClouds.Add(1f);
			this.smallClouds.Add((float)(this.width / 2 + 150 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width - 1 + 630 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width - 1 + 130 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width / 3 + 190 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(1 + 100 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width / 2 + 830 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width * 2 / 3 + 120 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width * 3 / 4 + 170 * TitleMenu.pixelZoom));
			this.smallClouds.Add((float)(this.width / 4 + 220 * TitleMenu.pixelZoom));
			int num;
			for (int i = 0; i < this.smallClouds.Count; i++)
			{
				List<float> list = this.smallClouds;
				num = i;
				list[num] += (float)Game1.random.Next(400);
			}
			this.birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2((float)(this.width - 70 * TitleMenu.pixelZoom), (float)(this.height - 130 * TitleMenu.pixelZoom)), false, 0f, Color.White)
			{
				scale = (float)TitleMenu.pixelZoom,
				pingPong = true,
				animationLength = 4,
				interval = 100f,
				totalNumberOfLoops = 9999,
				local = true,
				motion = new Vector2(-1f, 0f),
				layerDepth = 0.25f
			});
			this.birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2((float)(this.width - 40 * TitleMenu.pixelZoom), (float)(this.height - 120 * TitleMenu.pixelZoom)), false, 0f, Color.White)
			{
				scale = (float)TitleMenu.pixelZoom,
				pingPong = true,
				animationLength = 4,
				interval = 100f,
				totalNumberOfLoops = 9999,
				local = true,
				delayBeforeAnimationStart = 100,
				motion = new Vector2(-1f, 0f),
				layerDepth = 0.25f
			});
			this.setUpIcons();
			this.muteMusicButton = new ClickableTextureComponent(new Rectangle(16, 16, 36, 36), Game1.mouseCursors, new Rectangle(128, 384, 9, 9), 4f, false)
			{
				myID = 81111,
				downNeighborID = 81115,
				rightNeighborID = 81112
			};
			this.windowedButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f, false)
			{
				myID = 81112,
				leftNeighborID = 81111,
				downNeighborID = 81113
			};
			this.startupPreferences = new StartupPreferences();
			this.startupPreferences.loadPreferences(false, false);
			this.applyPreferences();
			num = this.startupPreferences.timesPlayed;
			if (num <= 30)
			{
				switch (num)
				{
				case 2:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11717");
					break;
				case 3:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11718");
					break;
				case 4:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11719");
					break;
				case 5:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11720");
					break;
				case 6:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11721");
					break;
				case 7:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11722");
					break;
				case 8:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11723");
					break;
				case 9:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11724");
					break;
				case 10:
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11725");
					break;
				case 11:
				case 12:
				case 13:
				case 14:
				case 16:
				case 17:
				case 18:
				case 19:
					break;
				case 15:
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
					{
						string noun = Dialogue.getRandomNoun();
						string noun2 = Dialogue.getRandomNoun();
						this.startupMessage = string.Concat(new string[]
						{
							this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11726"),
							Environment.NewLine,
							"The ",
							Dialogue.getRandomAdjective(),
							" ",
							noun,
							" ",
							Dialogue.getRandomVerb(),
							" ",
							Dialogue.getRandomPositional(),
							" the ",
							noun.Equals(noun2) ? ("other " + noun2) : noun2
						});
					}
					else
					{
						int randSentence = Game1.random.Next(1, 15);
						this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11726") + this.menuContent.LoadString("Strings\\StringsFromCSFiles:RandomSentence." + randSentence.ToString());
					}
					break;
				case 20:
					this.startupMessage = "<";
					break;
				default:
					if (num == 30)
					{
						this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11731");
					}
					break;
				}
			}
			else if (num != 100)
			{
				if (num != 1000)
				{
					if (num == 10000)
					{
						this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11734");
					}
				}
				else
				{
					this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11733");
				}
			}
			else
			{
				this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11732");
			}
			this.startupPreferences.savePreferences(false, false);
			Game1.setRichPresence("menus", null);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			if (TitleMenu.SkipSplashScreens)
			{
				this.skipToTitleButtons();
				return;
			}
			TitleMenu.SkipSplashScreens = true;
		}

		// Token: 0x06002D33 RID: 11571 RVA: 0x002310E1 File Offset: 0x0022F2E1
		private bool alternativeTitleGraphic()
		{
			return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
		}

		// Token: 0x06002D34 RID: 11572 RVA: 0x002310EC File Offset: 0x0022F2EC
		public void applyPreferences()
		{
			if (this.startupPreferences.playerLimit > 0)
			{
				Game1.multiplayer.playerLimit = this.startupPreferences.playerLimit;
			}
			if (this.startupPreferences.startMuted)
			{
				if (Utility.toggleMuteMusic())
				{
					this.muteMusicButton.sourceRect.X = 137;
				}
				else
				{
					this.muteMusicButton.sourceRect.X = 128;
				}
			}
			if (this.startupPreferences.skipWindowPreparation && TitleMenu.windowNumber == 3)
			{
				TitleMenu.windowNumber = -1;
			}
			if (this.startupPreferences.windowMode == 2 && this.startupPreferences.fullscreenResolutionX != 0 && this.startupPreferences.fullscreenResolutionY != 0)
			{
				Game1.options.preferredResolutionX = this.startupPreferences.fullscreenResolutionX;
				Game1.options.preferredResolutionY = this.startupPreferences.fullscreenResolutionY;
			}
			Game1.options.gamepadMode = this.startupPreferences.gamepadMode;
			Game1.game1.CheckGamepadMode();
			if (Game1.options.gamepadControls && Game1.options.snappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002D35 RID: 11573 RVA: 0x0023120C File Offset: 0x0022F40C
		private void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			this.titleButtonsTexture = this.menuContent.Load<Texture2D>("Minigames\\TitleButtons");
			this.setUpIcons();
			this.tempSprites.Clear();
			this.startupPreferences.OnLanguageChange(code);
		}

		// Token: 0x06002D36 RID: 11574 RVA: 0x00231244 File Offset: 0x0022F444
		public void skipToTitleButtons()
		{
			this.logoFadeTimer = 0;
			this.logoSwipeTimer = 0f;
			this.titleInPosition = false;
			this.pauseBeforeViewportRiseTimer = 0;
			this.fadeFromWhiteTimer = 0;
			this.viewportY = -999f;
			this.viewportDY = -0.01f;
			this.birds.Clear();
			this.logoSwipeTimer = 1f;
			this.amuzioTimer = 0;
			Game1.changeMusicTrack("MainTheme", false, MusicContext.Default);
			if (Game1.options.SnappyMenus && Game1.options.gamepadControls)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002D37 RID: 11575 RVA: 0x002312D8 File Offset: 0x0022F4D8
		public void setUpIcons()
		{
			this.buttons.Clear();
			int buttonWidth = 74;
			int mainButtonSetWidth = buttonWidth * 4 * TitleMenu.pixelZoom;
			mainButtonSetWidth += 24 * TitleMenu.pixelZoom;
			int curx = this.width / 2 - mainButtonSetWidth / 2;
			this.buttons.Add(new ClickableTextureComponent("New", new Rectangle(curx, this.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, buttonWidth * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(0, 187, 74, 58), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81115,
				rightNeighborID = 81116,
				upNeighborID = 81111
			});
			curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
			this.buttons.Add(new ClickableTextureComponent("Load", new Rectangle(curx, this.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(74, 187, 74, 58), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81116,
				leftNeighborID = 81115,
				rightNeighborID = -7777,
				upNeighborID = 81111
			});
			curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
			this.buttons.Add(new ClickableTextureComponent("Co-op", new Rectangle(curx, this.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(148, 187, 74, 58), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81119,
				leftNeighborID = 81116,
				rightNeighborID = 81117
			});
			curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
			this.buttons.Add(new ClickableTextureComponent("Exit", new Rectangle(curx, this.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(222, 187, 74, 58), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81117,
				leftNeighborID = 81119,
				rightNeighborID = 81118,
				upNeighborID = 81111
			});
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			this.eRect = new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
			this.screwRect = new Rectangle(this.width / 2 + 150 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
			this.cornerRect = new Rectangle(this.width / 2 - 200 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 165 * zoom, 20 * zoom, 20 * zoom);
			this.r_hole_rect = new Rectangle(this.width / 2 - 21 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 39 * zoom, 10 * zoom, 11 * zoom);
			this.r_hole_rect2 = new Rectangle(this.width / 2 - 35 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 24 * zoom, 7 * zoom, 7 * zoom);
			this.populateLeafRects();
			this.backButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(this.width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, this.height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(296, 252, 66, 27), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81114
			};
			this.aboutButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(this.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, this.height - 25 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 22 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(8, 458, 22, 25), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81113,
				upNeighborID = 81118,
				leftNeighborID = -7777
			};
			this.languageButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(this.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, this.height - 25 * TitleMenu.pixelZoom * 2 - 16 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(52, 458, 27, 25), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81118,
				downNeighborID = 81113,
				leftNeighborID = -7777,
				upNeighborID = 81112
			};
			this.skipButton = new ClickableComponent(new Rectangle(this.width / 2 - 87 * TitleMenu.pixelZoom, this.height / 2 - 34 * TitleMenu.pixelZoom, 83 * TitleMenu.pixelZoom, 67 * TitleMenu.pixelZoom), this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11741"));
			if (this.globalXOffset > this.width)
			{
				this.globalXOffset = this.width;
			}
			foreach (ClickableTextureComponent clickableTextureComponent in this.buttons)
			{
				clickableTextureComponent.bounds.X = clickableTextureComponent.bounds.X + this.globalXOffset;
			}
			if (Game1.options.gamepadControls && Game1.options.snappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002D38 RID: 11576 RVA: 0x00231988 File Offset: 0x0022FB88
		public override void snapToDefaultClickableComponent()
		{
			if (TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.snapToDefaultClickableComponent();
				return;
			}
			StartupPreferences startupPreferences = this.startupPreferences;
			this.currentlySnappedComponent = base.getComponentWithID((startupPreferences != null && startupPreferences.timesPlayed > 0) ? 81116 : 81115);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002D39 RID: 11577 RVA: 0x002319DC File Offset: 0x0022FBDC
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID != 81116 || direction != 1)
			{
				if ((oldID == 81118 || oldID == 81113) && direction == 3)
				{
					if (base.getComponentWithID(81117) != null)
					{
						this.setCurrentlySnappedComponentTo(81117);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
					this.setCurrentlySnappedComponentTo(81116);
					this.snapCursorToCurrentSnappedComponent();
				}
				return;
			}
			if (base.getComponentWithID(81119) != null)
			{
				this.setCurrentlySnappedComponentTo(81119);
				this.snapCursorToCurrentSnappedComponent();
				return;
			}
			if (base.getComponentWithID(81117) != null)
			{
				this.setCurrentlySnappedComponentTo(81117);
				this.snapCursorToCurrentSnappedComponent();
				return;
			}
			this.setCurrentlySnappedComponentTo(81118);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002D3A RID: 11578 RVA: 0x00231A8C File Offset: 0x0022FC8C
		public void populateLeafRects()
		{
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			this.leafRects = new List<Rectangle>
			{
				new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom - 196 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom + 109 * zoom, 17 * zoom, 30 * zoom),
				new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom + 91 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 26 * zoom, 17 * zoom, 31 * zoom),
				new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom + 79 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom + 83 * zoom, 25 * zoom, 17 * zoom),
				new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom - 213 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 24 * zoom, 14 * zoom, 23 * zoom),
				new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom - 234 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 11 * zoom, 18 * zoom, 12 * zoom)
			};
		}

		// Token: 0x06002D3B RID: 11579 RVA: 0x00231C53 File Offset: 0x0022FE53
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!this.ShouldAllowInteraction())
			{
				return;
			}
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu == null)
			{
				return;
			}
			subMenu.receiveRightClick(x, y, true);
		}

		// Token: 0x06002D3C RID: 11580 RVA: 0x00231C79 File Offset: 0x0022FE79
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x06002D3D RID: 11581 RVA: 0x00231C7C File Offset: 0x0022FE7C
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return !this.titleInPosition;
		}

		// Token: 0x06002D3E RID: 11582 RVA: 0x00231C87 File Offset: 0x0022FE87
		public override void leftClickHeld(int x, int y)
		{
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.leftClickHeld(x, y);
			}
		}

		// Token: 0x06002D3F RID: 11583 RVA: 0x00231CAD File Offset: 0x0022FEAD
		public override void releaseLeftClick(int x, int y)
		{
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu == null)
			{
				return;
			}
			subMenu.releaseLeftClick(x, y);
		}

		// Token: 0x06002D40 RID: 11584 RVA: 0x00231CDC File Offset: 0x0022FEDC
		public override void receiveKeyPress(Keys key)
		{
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			if (key != Keys.Escape && key != Keys.B)
			{
				if (key == Keys.N && !Program.releaseBuild && Game1.oldKBState.IsKeyDown(Keys.RightShift) && Game1.oldKBState.IsKeyDown(Keys.LeftControl))
				{
					Season season = Season.Spring;
					if (Game1.oldKBState.IsKeyDown(Keys.D1))
					{
						Game1.whichFarm = 1;
					}
					else if (Game1.oldKBState.IsKeyDown(Keys.D2))
					{
						Game1.whichFarm = 2;
					}
					else if (Game1.oldKBState.IsKeyDown(Keys.D3))
					{
						Game1.whichFarm = 3;
					}
					else if (Game1.oldKBState.IsKeyDown(Keys.D4))
					{
						Game1.whichFarm = 4;
					}
					else if (Game1.oldKBState.IsKeyDown(Keys.D5))
					{
						Game1.whichFarm = 5;
					}
					else if (Game1.oldKBState.IsKeyDown(Keys.D6))
					{
						Game1.whichFarm = 6;
					}
					if (Game1.oldKBState.IsKeyDown(Keys.C))
					{
						Game1.whichFarm = Game1.random.Next(6);
						Game1.season = (Season)Game1.random.Next(4);
					}
					Game1.game1.loadForNewGame(false);
					Game1.saveOnNewDay = false;
					Game1.player.eventsSeen.Add("60367");
					Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
					Game1.player.Position = new Vector2(9f, 9f) * 64f;
					Game1.player.isInBed.Value = true;
					Game1.player.farmName.Value = "Test";
					if (Game1.oldKBState.IsKeyDown(Keys.C))
					{
						Game1.season = season;
						Game1.setGraphicsForSeason(true);
					}
					Game1.player.mailReceived.Add("button_tut_1");
					Game1.player.mailReceived.Add("button_tut_2");
					Game1.NewDay(0f);
					Game1.exitActiveMenu();
					Game1.setGameMode(3);
					return;
				}
			}
			else if (this.logoFadeTimer > 0)
			{
				this.bCount++;
				if (key == Keys.Escape)
				{
					this.bCount += 3;
				}
				if (this.bCount >= 3)
				{
					Game1.playSound("bigDeSelect", null);
					this.logoFadeTimer = 0;
					this.fadeFromWhiteTimer = 0;
					Game1.delayedActions.Clear();
					Game1.morningSongPlayAction = null;
					this.pauseBeforeViewportRiseTimer = 0;
					this.fadeFromWhiteTimer = 0;
					this.viewportY = -999f;
					this.viewportDY = -0.01f;
					this.birds.Clear();
					this.logoSwipeTimer = 1f;
					this.amuzioTimer = 0;
					Game1.changeMusicTrack("MainTheme", false, MusicContext.Default);
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				return;
			}
			if (!this.ShouldAllowInteraction())
			{
				return;
			}
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu != null)
			{
				subMenu.receiveKeyPress(key);
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && TitleMenu.subMenu == null)
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002D41 RID: 11585 RVA: 0x00231FD8 File Offset: 0x002301D8
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu != null)
			{
				subMenu.receiveGamePadButton(button);
			}
			if (button == Buttons.B && this.titleInPosition && this.logoFadeTimer <= 0 && this.fadeFromWhiteTimer <= 0)
			{
				IClickableMenu subMenu2 = TitleMenu.subMenu;
				LoadGameMenu loadGameMenu = subMenu2 as LoadGameMenu;
				if (loadGameMenu == null)
				{
					CharacterCustomization customizationMenu = subMenu2 as CharacterCustomization;
					if (customizationMenu == null)
					{
						this.backButtonPressed();
					}
					else if (!customizationMenu.showingCoopHelp)
					{
						this.backButtonPressed();
						return;
					}
				}
				else if (!loadGameMenu.deleteConfirmationScreen)
				{
					this.backButtonPressed();
					return;
				}
			}
		}

		// Token: 0x06002D42 RID: 11586 RVA: 0x0023205F File Offset: 0x0023025F
		public override void gamePadButtonHeld(Buttons b)
		{
			if (!Game1.lastCursorMotionWasMouse)
			{
				this._movedCursor = true;
			}
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu == null)
			{
				return;
			}
			subMenu.gamePadButtonHeld(b);
		}

		// Token: 0x06002D43 RID: 11587 RVA: 0x00232080 File Offset: 0x00230280
		public void backButtonPressed()
		{
			if (TitleMenu.subMenu != null && TitleMenu.subMenu.readyToClose())
			{
				Game1.playSound("bigDeSelect", null);
				this.buttonsDX = -1;
				if (TitleMenu.subMenu is AboutMenu)
				{
					TitleMenu.ReturnToMainTitleScreen();
					this.buttonsDX = 0;
					if (Game1.options.SnappyMenus)
					{
						this.setCurrentlySnappedComponentTo(81113);
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
				TitleTextInputMenu titleTextInputMenu = TitleMenu.subMenu as TitleTextInputMenu;
				if ((titleTextInputMenu != null && titleTextInputMenu.context == "join_menu") || TitleMenu.subMenu is FarmhandMenu)
				{
					this.buttonsDX = 0;
					(TitleMenu.subMenu = new CoopMenu(false, false, CoopMenu.Tab.JOIN_TAB, null)).SetTab(CoopMenu.Tab.JOIN_TAB, false);
					if (Game1.options.SnappyMenus)
					{
						TitleMenu.subMenu.snapToDefaultClickableComponent();
					}
					return;
				}
				CharacterCustomization customizationMenu = TitleMenu.subMenu as CharacterCustomization;
				if (customizationMenu != null && customizationMenu.source == CharacterCustomization.Source.HostNewFarm)
				{
					this.buttonsDX = 0;
					(TitleMenu.subMenu = new CoopMenu(false, false, CoopMenu.Tab.JOIN_TAB, null)).SetTab(CoopMenu.Tab.HOST_TAB, false);
					Game1.changeMusicTrack("title_night", false, MusicContext.Default);
					if (Game1.options.SnappyMenus)
					{
						TitleMenu.subMenu.snapToDefaultClickableComponent();
					}
					return;
				}
				this.isTransitioningButtons = true;
				if (TitleMenu.subMenu is LoadGameMenu)
				{
					this.transitioningFromLoadScreen = true;
				}
				TitleMenu.ReturnToMainTitleScreen();
				Game1.changeMusicTrack("spring_day_ambient", false, MusicContext.Default);
			}
		}

		// Token: 0x06002D44 RID: 11588 RVA: 0x002321DC File Offset: 0x002303DC
		private void UpdateHasRoomAnotherFarm()
		{
			lock (this)
			{
				this.hasRoomAnotherFarm = null;
			}
			Game1.GetHasRoomAnotherFarmAsync(delegate(bool yes)
			{
				lock (this)
				{
					this.hasRoomAnotherFarm = new bool?(yes);
				}
			});
		}

		// Token: 0x06002D45 RID: 11589 RVA: 0x00232230 File Offset: 0x00230430
		protected void CloseSubMenu()
		{
			if (TitleMenu.subMenu.readyToClose())
			{
				this.buttonsDX = -1;
				if (TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu)
				{
					TitleMenu.subMenu = null;
					this.buttonsDX = 0;
					return;
				}
				this.isTransitioningButtons = true;
				if (TitleMenu.subMenu is LoadGameMenu)
				{
					this.transitioningFromLoadScreen = true;
				}
				TitleMenu.subMenu = null;
				Game1.changeMusicTrack("spring_day_ambient", false, MusicContext.Default);
			}
		}

		// Token: 0x06002D46 RID: 11590 RVA: 0x002322A4 File Offset: 0x002304A4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.HasActiveUser && this.muteMusicButton.containsPoint(x, y))
			{
				this.startupPreferences.startMuted = Utility.toggleMuteMusic();
				if (this.muteMusicButton.sourceRect.X == 128)
				{
					this.muteMusicButton.sourceRect.X = 137;
				}
				else
				{
					this.muteMusicButton.sourceRect.X = 128;
				}
				Game1.playSound("drumkit6", null);
				this.startupPreferences.savePreferences(false, false);
				return;
			}
			if (this.HasActiveUser && this.windowedButton.containsPoint(x, y))
			{
				if (!Game1.options.isCurrentlyWindowed())
				{
					Game1.options.setWindowedOption("Windowed");
					this.windowedButton.sourceRect.X = 146;
					this.startupPreferences.windowMode = 1;
				}
				else
				{
					Game1.options.setWindowedOption("Windowed Borderless");
					this.windowedButton.sourceRect.X = 155;
					this.startupPreferences.windowMode = 0;
				}
				this.startupPreferences.savePreferences(false, false);
				Game1.playSound("drumkit6", null);
				return;
			}
			if (this.logoFadeTimer > 0 && this.skipButton != null && this.skipButton.containsPoint(x, y))
			{
				if (this.logoSurprisedTimer <= 0)
				{
					int pitch = 1200;
					this.logoSurprisedTimer = 1500;
					string soundtoPlay = "fishSlap";
					Game1.changeMusicTrack("none", false, MusicContext.Default);
					int num = Game1.random.Next(2);
					if (num != 0)
					{
						if (num == 1)
						{
							soundtoPlay = "fishSlap";
						}
					}
					else
					{
						soundtoPlay = "Duck";
						pitch = 0;
					}
					if (Game1.random.NextDouble() < 0.02)
					{
						this.specialSurprised = true;
						Game1.playSound("moss_cut", null);
						this.fadeFromWhiteTimer = 3000;
					}
					else
					{
						Game1.playSound(soundtoPlay, new int?(pitch));
					}
				}
				else if (this.logoSurprisedTimer > 1)
				{
					this.logoSurprisedTimer = Math.Max(1, this.logoSurprisedTimer - 500);
				}
			}
			if (this.amuzioTimer > 500)
			{
				this.amuzioTimer = 500;
			}
			if (this.logoFadeTimer > 0 || this.fadeFromWhiteTimer > 0)
			{
				return;
			}
			if (this.transitioningCharacterCreationMenu)
			{
				return;
			}
			if (TitleMenu.subMenu != null)
			{
				bool should_ignore_back_button_press = false;
				if (Game1.options.SnappyMenus && TitleMenu.subMenu.currentlySnappedComponent != null && TitleMenu.subMenu.currentlySnappedComponent.myID != 81114)
				{
					should_ignore_back_button_press = true;
				}
				bool handled_submenu_close = false;
				if (TitleMenu.subMenu.readyToClose() && this.backButton != null && this.backButton.containsPoint(x, y) && !should_ignore_back_button_press)
				{
					this.backButtonPressed();
					handled_submenu_close = true;
				}
				else if (!this.isTransitioningButtons)
				{
					TitleMenu.subMenu.receiveLeftClick(x, y, true);
				}
				if (!handled_submenu_close && TitleMenu.subMenu != null && TitleMenu.subMenu.readyToClose() && (TitleMenu.subMenu is TooManyFarmsMenu || (this.backButton != null && this.backButton.containsPoint(x, y))) && !should_ignore_back_button_press)
				{
					Game1.playSound("bigDeSelect", null);
					this.buttonsDX = -1;
					if (TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu)
					{
						TitleMenu.ReturnToMainTitleScreen();
						this.buttonsDX = 0;
						return;
					}
					this.isTransitioningButtons = true;
					if (TitleMenu.subMenu is LoadGameMenu)
					{
						this.transitioningFromLoadScreen = true;
					}
					TitleMenu.ReturnToMainTitleScreen();
					Game1.changeMusicTrack("spring_day_ambient", false, MusicContext.Default);
					return;
				}
			}
			else
			{
				if (this.logoFadeTimer <= 0 && !this.titleInPosition && this.logoSwipeTimer == 0f)
				{
					this.pauseBeforeViewportRiseTimer = 0;
					this.fadeFromWhiteTimer = 0;
					this.viewportY = -999f;
					this.viewportDY = -0.01f;
					this.birds.Clear();
					this.logoSwipeTimer = 1f;
					return;
				}
				if (!this.alternativeTitleGraphic())
				{
					if (this.clicksOnLeaf >= 10 && Game1.random.NextDouble() < 0.001)
					{
						Game1.playSound("junimoMeep1", null);
					}
					if (this.titleInPosition && this.eRect.Contains(x, y) && this.clicksOnE < 10)
					{
						this.clicksOnE++;
						Game1.playSound("woodyStep", null);
						if (this.clicksOnE == 10)
						{
							int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
							Game1.playSound("openChest", null);
							this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(0, 491, 42, 68), new Vector2((float)(this.width / 2 - 200 * zoom + 251 * zoom), (float)(-300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom)), false, 0f, Color.White)
							{
								scale = (float)zoom,
								animationLength = 9,
								interval = 200f,
								local = true,
								holdLastFrame = true
							});
						}
					}
					else if (this.titleInPosition)
					{
						bool clicked = false;
						foreach (Rectangle r in this.leafRects)
						{
							if (r.Contains(x, y))
							{
								clicked = true;
								break;
							}
						}
						if (this.screwRect.Contains(x, y) && this.clicksOnScrew < 10)
						{
							Game1.playSound("cowboy_monsterhit", null);
							this.clicksOnScrew++;
							if (this.clicksOnScrew == 10)
							{
								this.showButterflies();
							}
						}
						if (Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
						{
							if (this.cornerPhaseHolding && (this.r_hole_rect.Contains(x, y) || this.r_hole_rect2.Contains(x, y)) && this.cornerClicks < 999)
							{
								Game1.playSound("coin", null);
								this.cornerClickEndTimer = 1000f;
								this.cornerClickSoundEffectTimer = 400f;
								this.cornerClicks = 9999;
								this.showCornerClickEasterEgg = true;
							}
							else if (this.cornerRect.Contains(x, y) && !this.cornerPhaseHolding)
							{
								int zoom2 = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
								this.cornerClicks++;
								if (this.cornerClicks > 5)
								{
									if (!this.cornerPhaseHolding)
									{
										Game1.playSound("coin", null);
										this.cornerClicks = 0;
										this.cornerPhaseHolding = true;
									}
								}
								else
								{
									Game1.playSound("hammer", null);
									for (int i = 0; i < 3; i++)
									{
										this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(280 + Game1.random.Choose(8, 0), 1954, 8, 8), 1000f, 1, 99, new Vector2((float)(this.width / 2 - 190 * zoom2), (float)(-300 * zoom2 - (int)(this.viewportY / 3f) * zoom2 + 175 * zoom2)), false, false, 1f, 0f, Color.White, (float)TitleMenu.pixelZoom, 0f, 0f, (float)Game1.random.Next(-10, 11) / 100f, false)
										{
											motion = new Vector2((float)Game1.random.Next(-4, 5), -8f + (float)Game1.random.Next(-10, 1) / 100f),
											acceleration = new Vector2(0f, 0.3f),
											local = true,
											delayBeforeAnimationStart = i * 15
										});
									}
								}
							}
						}
						if (clicked)
						{
							this.clicksOnLeaf++;
							if (this.clicksOnLeaf == 10)
							{
								int zoom3 = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
								Game1.playSound("discoverMineral", null);
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(264, 464, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 80 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 10 * zoom3 + 2)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 8,
									interval = 80f,
									totalNumberOfLoops = 999999,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = 200
								});
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 80 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 10 * zoom3)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 8,
									interval = 50f,
									local = true,
									holdLastFrame = false
								});
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(200, 464, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 178 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 141 * zoom3 + 2)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 4,
									interval = 150f,
									totalNumberOfLoops = 999999,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = 400
								});
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 178 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 141 * zoom3)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 8,
									interval = 50f,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = 200
								});
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 464, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 294 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 89 * zoom3 + 2)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 4,
									interval = 150f,
									totalNumberOfLoops = 999999,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = 600
								});
								this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2((float)(this.width / 2 - 200 * zoom3 + 294 * zoom3), (float)(-300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 89 * zoom3)), false, 0f, Color.White)
								{
									scale = (float)zoom3,
									animationLength = 8,
									interval = 50f,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = 400
								});
							}
							else
							{
								Game1.playSound("leafrustle", null);
								int zoom4 = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
								for (int j = 0; j < 2; j++)
								{
									this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199 + Game1.random.Next(-1, 2) * 16, 16, 16), new Vector2((float)(x + Game1.random.Next(-8, 9)), (float)(y + Game1.random.Next(-8, 9))), Game1.random.NextBool(), 0f, Color.White)
									{
										scale = (float)zoom4,
										animationLength = 11,
										interval = (float)(50 + Game1.random.Next(50)),
										totalNumberOfLoops = 999,
										motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
										xPeriodic = Game1.random.NextBool(),
										xPeriodicLoopTime = (float)Game1.random.Next(6000, 16000),
										xPeriodicRange = (float)Game1.random.Next(64, 192),
										alphaFade = 0.001f,
										local = true,
										holdLastFrame = false,
										delayBeforeAnimationStart = j * 20
									});
								}
							}
						}
					}
				}
				if (!this.ShouldAllowInteraction())
				{
					return;
				}
				if (!this.HasActiveUser)
				{
					return;
				}
				if ((TitleMenu.subMenu == null || TitleMenu.subMenu.readyToClose()) && !this.isTransitioningButtons)
				{
					for (int k = 0; k < this.buttons.Count; k++)
					{
						ClickableTextureComponent c = this.buttons[k];
						if (c.containsPoint(x, y))
						{
							this.performButtonAction(c.name);
						}
					}
					if (this.aboutButton.containsPoint(x, y))
					{
						TitleMenu.subMenu = new AboutMenu();
						Game1.playSound("newArtifact", null);
					}
					if (this.languageButton.visible && this.languageButton.containsPoint(x, y))
					{
						TitleMenu.subMenu = new LanguageSelectionMenu();
						Game1.playSound("newArtifact", null);
					}
				}
			}
		}

		// Token: 0x06002D47 RID: 11591 RVA: 0x0023318C File Offset: 0x0023138C
		public void performButtonAction(string which)
		{
			this.whichSubMenu = which;
			if (which == "New")
			{
				this.buttonsDX = 1;
				this.isTransitioningButtons = true;
				Game1.playSound("select", null);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
				{
					temporaryAnimatedSprite.pingPong = false;
				}
				this.UpdateHasRoomAnotherFarm();
				return;
			}
			if (which == "Co-op")
			{
				this.buttonsDX = 1;
				this.isTransitioningButtons = true;
				Game1.playSound("select", null);
				this.UpdateHasRoomAnotherFarm();
				return;
			}
			if (which == "Load" || which == "Invite")
			{
				this.buttonsDX = 1;
				this.isTransitioningButtons = true;
				Game1.playSound("select", null);
				return;
			}
			if (!(which == "Exit"))
			{
				return;
			}
			Game1.playSound("bigDeSelect", null);
			Game1.changeMusicTrack("none", false, MusicContext.Default);
			this.quitTimer = 500;
		}

		// Token: 0x06002D48 RID: 11592 RVA: 0x002332CC File Offset: 0x002314CC
		private void addRightLeafGust()
		{
			if (this.isTransitioningButtons || this.tempSprites.Count > 0 || this.alternativeTitleGraphic())
			{
				return;
			}
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 187, 27, 21), new Vector2((float)(this.width / 2 - 200 * zoom + 327 * zoom), (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(107 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				pingPong = true,
				animationLength = 3,
				interval = 100f,
				totalNumberOfLoops = 3,
				local = true
			});
		}

		// Token: 0x06002D49 RID: 11593 RVA: 0x002333A7 File Offset: 0x002315A7
		public bool ShouldShrinkLogo()
		{
			return this.height <= 850;
		}

		// Token: 0x06002D4A RID: 11594 RVA: 0x002333BC File Offset: 0x002315BC
		private void addLeftLeafGust()
		{
			if (this.isTransitioningButtons || this.tempSprites.Count > 0 || this.alternativeTitleGraphic())
			{
				return;
			}
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 208, 22, 18), new Vector2((float)(this.width / 2 - 200 * zoom + 16 * zoom), (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(16 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				pingPong = true,
				animationLength = 3,
				interval = 100f,
				totalNumberOfLoops = 3,
				local = true
			});
		}

		// Token: 0x06002D4B RID: 11595 RVA: 0x00233494 File Offset: 0x00231694
		public void createdNewCharacter(bool skipIntro)
		{
			Action onCreatedNewCharacter = TitleMenu.OnCreatedNewCharacter;
			if (onCreatedNewCharacter != null)
			{
				onCreatedNewCharacter();
			}
			Game1.playSound("smallSelect", null);
			TitleMenu.subMenu = null;
			this.transitioningCharacterCreationMenu = true;
			if (skipIntro)
			{
				Game1.game1.loadForNewGame(false);
				Game1.saveOnNewDay = true;
				Game1.player.eventsSeen.Add("60367");
				Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
				Game1.player.Position = new Vector2(9f, 9f) * 64f;
				Game1.player.isInBed.Value = true;
				Game1.NewDay(0f);
				Game1.exitActiveMenu();
				Game1.setGameMode(3);
			}
		}

		// Token: 0x06002D4C RID: 11596 RVA: 0x0023355C File Offset: 0x0023175C
		public override void update(GameTime time)
		{
			if (Game1.game1.IsMainInstance)
			{
				if (TitleMenu.ticksUntilLanguageLoad > 0)
				{
					TitleMenu.ticksUntilLanguageLoad--;
				}
				else if (TitleMenu.ticksUntilLanguageLoad == 0)
				{
					TitleMenu.ticksUntilLanguageLoad--;
					this.startupPreferences.loadPreferences(false, true);
				}
			}
			if (TitleMenu.windowNumber > 0)
			{
				if (this.startupPreferences.displayIndex >= 0 && !GameRunner.instance.Window.CenterOnDisplay(this.startupPreferences.displayIndex))
				{
					Game1.log.Error("Error: Couldn't find display with index " + this.startupPreferences.displayIndex.ToString() + ". Reverting to windowed mode on display 0.", null);
					this.startupPreferences.windowMode = 1;
				}
				Game1.options.setWindowedOption(this.startupPreferences.windowMode);
				TitleMenu.windowNumber = 0;
			}
			if (!Game1.options.isCurrentlyWindowed())
			{
				Vector2 corner_position = new Vector2((float)(Game1.viewport.Width - 36 - 16), 16f);
				corner_position.X = (float)(Math.Min(GameRunner.instance.Window.GetDisplayBounds(GameRunner.instance.Window.GetDisplayIndex()).Right - GameRunner.instance.Window.ClientBounds.Left, Game1.viewport.Width) - 36 - 16);
				this.windowedButton.setPosition(corner_position);
			}
			base.update(time);
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu != null)
			{
				subMenu.update(time);
			}
			if (this.transitioningCharacterCreationMenu)
			{
				this.globalCloudAlpha -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
				if (this.globalCloudAlpha <= 0f)
				{
					this.transitioningCharacterCreationMenu = false;
					this.globalCloudAlpha = 0f;
					TitleMenu.subMenu = null;
					Game1.currentMinigame = new GrandpaStory();
					Game1.exitActiveMenu();
					Game1.setGameMode(3);
				}
			}
			if (this.quitTimer > 0)
			{
				this.quitTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.quitTimer <= 0)
				{
					Game1.quit = true;
					Game1.exitActiveMenu();
				}
			}
			if (this.amuzioTimer > 0)
			{
				this.amuzioTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else if (this.logoFadeTimer > 0)
			{
				if (this.logoSurprisedTimer > 0)
				{
					this.logoSurprisedTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.logoSurprisedTimer <= 0)
					{
						this.logoFadeTimer = 1;
					}
				}
				else
				{
					int old = this.logoFadeTimer;
					this.logoFadeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.logoFadeTimer < 4000 & old >= 4000)
					{
						Game1.playSound("mouseClick", null);
					}
					if (this.logoFadeTimer < 2500 & old >= 2500)
					{
						Game1.playSound("mouseClick", null);
					}
					if (this.logoFadeTimer < 2000 & old >= 2000)
					{
						Game1.playSound("mouseClick", null);
					}
					if (this.logoFadeTimer <= 0)
					{
						Game1.changeMusicTrack("MainTheme", false, MusicContext.Default);
					}
				}
			}
			else if (this.fadeFromWhiteTimer > 0)
			{
				this.fadeFromWhiteTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.fadeFromWhiteTimer <= 0)
				{
					this.pauseBeforeViewportRiseTimer = 3500;
				}
			}
			else if (this.pauseBeforeViewportRiseTimer > 0)
			{
				this.pauseBeforeViewportRiseTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.pauseBeforeViewportRiseTimer <= 0)
				{
					this.viewportDY = -0.05f;
				}
			}
			this.viewportY += this.viewportDY;
			if (this.viewportDY < 0f)
			{
				this.viewportDY -= 0.006f;
			}
			if (this.viewportY <= -1000f)
			{
				if (this.viewportDY != 0f)
				{
					this.logoSwipeTimer = 1000f;
					this.showButtonsTimer = 200;
				}
				this.viewportDY = 0f;
			}
			if (this.logoSwipeTimer > 0f)
			{
				this.logoSwipeTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.logoSwipeTimer <= 0f)
				{
					this.addLeftLeafGust();
					this.addRightLeafGust();
					this.titleInPosition = true;
					int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
					this.eRect = new Rectangle(this.width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
					this.screwRect = new Rectangle(this.width / 2 + 150 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
					this.cornerRect = new Rectangle(this.width / 2 - 200 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 165 * zoom, 20 * zoom, 20 * zoom);
					this.r_hole_rect = new Rectangle(this.width / 2 - 21 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 39 * zoom, 10 * zoom, 11 * zoom);
					this.r_hole_rect2 = new Rectangle(this.width / 2 - 35 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 24 * zoom, 7 * zoom, 7 * zoom);
					this.populateLeafRects();
				}
			}
			if (this.showButtonsTimer > 0 && this.HasActiveUser && TitleMenu.subMenu == null)
			{
				this.showButtonsTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.showButtonsTimer <= 0)
				{
					if (this.buttonsToShow < 4)
					{
						this.buttonsToShow++;
						Game1.playSound("Cowboy_gunshot", null);
						this.showButtonsTimer = 200;
					}
					else if (Game1.options.gamepadControls && Game1.options.snappyMenus)
					{
						this.populateClickableComponentList();
						this.snapToDefaultClickableComponent();
					}
				}
			}
			if (this.titleInPosition && !this.isTransitioningButtons && this.globalXOffset == 0 && Game1.random.NextDouble() < 0.005)
			{
				if (Game1.random.NextBool())
				{
					this.addLeftLeafGust();
				}
				else
				{
					this.addRightLeafGust();
				}
			}
			if (this.titleInPosition)
			{
				if (this.isTransitioningButtons)
				{
					int dx = this.buttonsDX * (int)time.ElapsedGameTime.TotalMilliseconds;
					int offsetx = this.globalXOffset + dx;
					int over = offsetx - this.width;
					if (over > 0)
					{
						offsetx -= over;
						dx -= over;
					}
					this.globalXOffset = offsetx;
					this.moveFeatures(dx, 0);
					if (this.buttonsDX > 0 && this.globalXOffset >= this.width)
					{
						if (TitleMenu.subMenu != null)
						{
							if (TitleMenu.subMenu.readyToClose())
							{
								this.isTransitioningButtons = false;
								this.buttonsDX = 0;
							}
						}
						else
						{
							string a = this.whichSubMenu;
							if (!(a == "Load"))
							{
								if (!(a == "Co-op"))
								{
									if (!(a == "Invite"))
									{
										if (a == "New")
										{
											if (this.hasRoomAnotherFarm != null)
											{
												if (!this.hasRoomAnotherFarm.Value)
												{
													TitleMenu.subMenu = new TooManyFarmsMenu();
													Game1.playSound("newArtifact", null);
													this.buttonsDX = 0;
													this.isTransitioningButtons = false;
												}
												else
												{
													Game1.resetPlayer();
													TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame, false);
													if (this.startupPreferences.timesPlayed > 1 && !this.startupPreferences.sawAdvancedCharacterCreationIndicator)
													{
														CharacterCustomization custom = TitleMenu.subMenu as CharacterCustomization;
														if (custom != null)
														{
															custom.showAdvancedCharacterCreationHighlight();
														}
														this.startupPreferences.sawAdvancedCharacterCreationIndicator = true;
														this.startupPreferences.savePreferences(false, false);
													}
													Game1.playSound("select", null);
													Game1.changeMusicTrack("CloudCountry", false, MusicContext.Default);
													Game1.player.favoriteThing.Value = "";
													this.buttonsDX = 0;
													this.isTransitioningButtons = false;
												}
											}
										}
									}
									else
									{
										TitleMenu.subMenu = new FarmhandMenu();
										Game1.changeMusicTrack("title_night", false, MusicContext.Default);
										this.buttonsDX = 0;
										this.isTransitioningButtons = false;
									}
								}
								else if (this.hasRoomAnotherFarm != null)
								{
									bool flag = true;
									this.buttonsDX = 0;
									this.isTransitioningButtons = false;
									if (flag)
									{
										TitleMenu.subMenu = new CoopMenu(!this.hasRoomAnotherFarm.Value, false, CoopMenu.Tab.JOIN_TAB, null);
										Game1.changeMusicTrack("title_night", false, MusicContext.Default);
									}
									else
									{
										Game1.playSound("bigDeSelect", null);
										if (Game1.options.SnappyMenus)
										{
											this.setCurrentlySnappedComponentTo(81119);
											this.snapCursorToCurrentSnappedComponent();
										}
									}
								}
							}
							else
							{
								TitleMenu.subMenu = new LoadGameMenu(null);
								Game1.changeMusicTrack("title_night", false, MusicContext.Default);
								this.buttonsDX = 0;
								this.isTransitioningButtons = false;
							}
						}
						if (!this.isTransitioningButtons)
						{
							this.whichSubMenu = "";
						}
					}
					else if (this.buttonsDX < 0 && this.globalXOffset <= 0)
					{
						this.globalXOffset = 0;
						this.isTransitioningButtons = false;
						this.buttonsDX = 0;
						this.setUpIcons();
						this.whichSubMenu = "";
						this.transitioningFromLoadScreen = false;
					}
				}
				if (this.cornerClickEndTimer > 0f)
				{
					this.cornerClickEndTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					if (this.cornerClickEndTimer <= 0f)
					{
						this.cornerClickParrotTimer = 400f;
					}
				}
				if (this.cornerClickSoundEffectTimer > 0f)
				{
					this.cornerClickSoundEffectTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					if (this.cornerClickSoundEffectTimer <= 0f)
					{
						Game1.playSound("goldenWalnut", null);
					}
				}
				if (this.cornerClickParrotTimer > 0f)
				{
					this.cornerClickParrotTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					if (this.cornerClickParrotTimer <= 0f)
					{
						int zoom2 = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 0, 24, 24), 100f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), false, false, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(-6f, -1f),
							acceleration = new Vector2(0.02f, 0.02f)
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 48, 24, 24), 95f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), false, false, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(-6f, -1f),
							acceleration = new Vector2(0.02f, 0.02f),
							delayBeforeAnimationStart = 300,
							startSound = "leafrustle"
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 24, 24, 24), 100f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), false, false, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(-6f, -1f),
							acceleration = new Vector2(0.02f, 0.02f),
							delayBeforeAnimationStart = 600,
							startSound = "parrot_squawk"
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 72, 24, 24), 95f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), false, false, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(-6f, -1f),
							acceleration = new Vector2(0.02f, 0.02f),
							delayBeforeAnimationStart = 1300,
							startSound = "leafrustle"
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 0, 24, 24), 100f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 + 200 * zoom2 - 24 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), false, true, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(6f, -1f),
							acceleration = new Vector2(-0.02f, -0.02f),
							delayBeforeAnimationStart = 600
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 48, 24, 24), 95f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 + 200 * zoom2 - 24 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), false, true, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(6f, -1f),
							acceleration = new Vector2(-0.02f, -0.02f),
							delayBeforeAnimationStart = 900,
							startSound = "leafrustle"
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 24, 24, 24), 100f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 + 200 * zoom2 - 24 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), false, true, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(6f, -1f),
							acceleration = new Vector2(-0.02f, -0.02f),
							delayBeforeAnimationStart = 1200
						});
						this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 72, 24, 24), 95f, 3, 999, new Vector2((float)(this.globalXOffset + this.width / 2 + 200 * zoom2 - 24 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), false, true, 0.2f, 0f, Color.White, (float)zoom2, 0.01f, 0f, 0f, true)
						{
							pingPong = true,
							motion = new Vector2(6f, -1f),
							acceleration = new Vector2(-0.02f, -0.02f),
							delayBeforeAnimationStart = 1500
						});
						for (int i = 0; i < 14; i++)
						{
							this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199, 16, 16), new Vector2((float)(this.globalXOffset + this.width / 2 - 220 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(60 * zoom2) + (float)(Game1.random.Next(100) * zoom2)), Game1.random.NextBool(), 0f, new Color(180, 180, 240))
							{
								scale = (float)zoom2,
								animationLength = 11,
								interval = (float)(50 + Game1.random.Next(50)),
								totalNumberOfLoops = 999,
								motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
								xPeriodic = Game1.random.NextBool(),
								xPeriodicLoopTime = (float)Game1.random.Next(6000, 16000),
								xPeriodicRange = (float)Game1.random.Next(64, 192),
								alphaFade = 0.001f,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 100 + i * 20
							});
						}
						for (int j = 0; j < 14; j++)
						{
							this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199, 16, 16), new Vector2((float)(this.globalXOffset + this.width / 2 + 220 * zoom2), (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(60 * zoom2) + (float)(Game1.random.Next(100) * zoom2)), Game1.random.NextBool(), 0f, new Color(180, 180, 240))
							{
								scale = (float)zoom2,
								animationLength = 11,
								interval = (float)(50 + Game1.random.Next(50)),
								totalNumberOfLoops = 999,
								motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
								xPeriodic = Game1.random.NextBool(),
								xPeriodicLoopTime = (float)Game1.random.Next(6000, 16000),
								xPeriodicRange = (float)Game1.random.Next(64, 192),
								alphaFade = 0.001f,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 900 + j * 20
							});
						}
					}
				}
			}
			for (int k = this.bigClouds.Count - 1; k >= 0; k--)
			{
				List<float> list = this.bigClouds;
				int index = k;
				list[index] -= 0.1f;
				list = this.bigClouds;
				index = k;
				list[index] += (float)(this.buttonsDX * time.ElapsedGameTime.Milliseconds / 2);
				if (this.bigClouds[k] < (float)(-512 * TitleMenu.pixelZoom))
				{
					this.bigClouds[k] = (float)this.width;
				}
			}
			for (int l = this.smallClouds.Count - 1; l >= 0; l--)
			{
				List<float> list = this.smallClouds;
				int index = l;
				list[index] -= 0.3f;
				list = this.smallClouds;
				index = l;
				list[index] += (float)(this.buttonsDX * time.ElapsedGameTime.Milliseconds / 2);
				if (this.smallClouds[l] < (float)(-149 * TitleMenu.pixelZoom))
				{
					this.smallClouds[l] = (float)this.width;
				}
			}
			this.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			this.behindSignTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			this.birds.RemoveWhere(delegate(TemporaryAnimatedSprite bird)
			{
				bird.position.Y = bird.position.Y - this.viewportDY * 2f;
				return bird.update(time);
			});
		}

		// Token: 0x06002D4D RID: 11597 RVA: 0x00234BF0 File Offset: 0x00232DF0
		private void moveFeatures(int dx, int dy)
		{
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
			{
				temporaryAnimatedSprite.position.X = temporaryAnimatedSprite.position.X + (float)dx;
				temporaryAnimatedSprite.position.Y = temporaryAnimatedSprite.position.Y + (float)dy;
			}
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in this.behindSignTempSprites)
			{
				temporaryAnimatedSprite2.position.X = temporaryAnimatedSprite2.position.X + (float)dx;
				temporaryAnimatedSprite2.position.Y = temporaryAnimatedSprite2.position.Y + (float)dy;
			}
			foreach (ClickableTextureComponent clickableTextureComponent in this.buttons)
			{
				clickableTextureComponent.bounds.X = clickableTextureComponent.bounds.X + dx;
				clickableTextureComponent.bounds.Y = clickableTextureComponent.bounds.Y + dy;
			}
		}

		// Token: 0x06002D4E RID: 11598 RVA: 0x00234D04 File Offset: 0x00232F04
		public override void receiveScrollWheelAction(int direction)
		{
			if (!this.ShouldAllowInteraction())
			{
				return;
			}
			base.receiveScrollWheelAction(direction);
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu == null)
			{
				return;
			}
			subMenu.receiveScrollWheelAction(direction);
		}

		// Token: 0x06002D4F RID: 11599 RVA: 0x00234D28 File Offset: 0x00232F28
		public override void performHoverAction(int x, int y)
		{
			if (!this.ShouldAllowInteraction())
			{
				x = int.MinValue;
				y = int.MinValue;
			}
			base.performHoverAction(x, y);
			this.muteMusicButton.tryHover(x, y, 0.1f);
			if (TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.performHoverAction(x, y);
				if (this.backButton != null && TitleMenu.subMenu.readyToClose())
				{
					if (this.backButton.containsPoint(x, y))
					{
						if (this.backButton.sourceRect.Y == 252)
						{
							Game1.playSound("Cowboy_Footstep", null);
						}
						this.backButton.sourceRect.Y = 279;
					}
					else
					{
						this.backButton.sourceRect.Y = 252;
					}
					this.backButton.tryHover(x, y, 0.25f);
					return;
				}
			}
			else if (this.titleInPosition && this.HasActiveUser)
			{
				foreach (ClickableTextureComponent c in this.buttons)
				{
					if (c.containsPoint(x, y))
					{
						if (c.sourceRect.Y == 187)
						{
							Game1.playSound("Cowboy_Footstep", null);
						}
						c.sourceRect.Y = 245;
					}
					else
					{
						c.sourceRect.Y = 187;
					}
					c.tryHover(x, y, 0.25f);
				}
				this.aboutButton.tryHover(x, y, 0.25f);
				if (this.aboutButton.containsPoint(x, y))
				{
					if (this.aboutButton.sourceRect.X == 8)
					{
						Game1.playSound("Cowboy_Footstep", null);
					}
					this.aboutButton.sourceRect.X = 30;
				}
				else
				{
					this.aboutButton.sourceRect.X = 8;
				}
				if (this.languageButton.visible)
				{
					this.languageButton.tryHover(x, y, 0.25f);
					if (this.languageButton.containsPoint(x, y))
					{
						if (this.languageButton.sourceRect.X == 52)
						{
							Game1.playSound("Cowboy_Footstep", null);
						}
						this.languageButton.sourceRect.X = 79;
						return;
					}
					this.languageButton.sourceRect.X = 52;
				}
			}
		}

		// Token: 0x06002D50 RID: 11600 RVA: 0x00234FAC File Offset: 0x002331AC
		public override void draw(SpriteBatch b)
		{
			bool shouldDrawMenu = TitleMenu.subMenu == null || TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu;
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), new Color(64, 136, 248));
			b.Draw(Game1.mouseCursors, new Rectangle(0, (int)((float)(-300 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), this.width, 300 * TitleMenu.pixelZoom + this.height - 120 * TitleMenu.pixelZoom), new Rectangle?(new Rectangle(703, 1912, 1, 264)), Color.White);
			if (!this.whichSubMenu.Equals("Load"))
			{
				for (int x = -10; x < this.width; x += 638)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(x * TitleMenu.pixelZoom), (float)(-360 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle?(new Rectangle(0, 1453, 638, 195)), Color.White * (1f - (float)this.globalXOffset / 1200f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
				}
			}
			foreach (float f in this.bigClouds)
			{
				b.Draw(this.cloudsTexture, new Vector2(f, (float)(this.height - 250 * TitleMenu.pixelZoom) - this.viewportY * 0.5f), new Rectangle?(new Rectangle(0, 0, 512, 337)), Color.White * this.globalCloudAlpha, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.01f);
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(-30 * TitleMenu.pixelZoom), (float)(this.height - 158 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle?(new Rectangle(0, 886, 639, 148)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(-30 * TitleMenu.pixelZoom + 639 * TitleMenu.pixelZoom), (float)(this.height - 158 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle?(new Rectangle(0, 886, 640, 148)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.08f);
			for (int i = 0; i < this.smallClouds.Count; i++)
			{
				b.Draw(this.cloudsTexture, new Vector2(this.smallClouds[i], (float)(this.height - 300 * TitleMenu.pixelZoom - i * 12 * TitleMenu.pixelZoom) - this.viewportY * 0.5f), new Rectangle?((i % 3 == 0) ? new Rectangle(152, 447, 123, 55) : ((i % 3 == 1) ? new Rectangle(0, 471, 149, 66) : new Rectangle(410, 467, 63, 37))), Color.White * this.globalCloudAlpha, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.01f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.height - 148 * TitleMenu.pixelZoom) - this.viewportY * 1f), new Rectangle?(new Rectangle(0, 737, 639, 148)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.1f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(639 * TitleMenu.pixelZoom), (float)(this.height - 148 * TitleMenu.pixelZoom) - this.viewportY * 1f), new Rectangle?(new Rectangle(0, 737, 640, 148)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.1f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.birds)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			b.Draw(this.cloudsTexture, new Vector2(0f, (float)(this.height - 142 * TitleMenu.pixelZoom) - this.viewportY * 2f), new Rectangle?(new Rectangle(0, 554, 165, 142)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
			b.Draw(this.cloudsTexture, new Vector2((float)(this.width - 122 * TitleMenu.pixelZoom), (float)(this.height - 153 * TitleMenu.pixelZoom) - this.viewportY * 2f), new Rectangle?(new Rectangle(390, 543, 122, 153)), Color.White, 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			if (!this.whichSubMenu.Equals("Load") && !this.whichSubMenu.Equals("Co-op") && !(TitleMenu.subMenu is LoadGameMenu))
			{
				CharacterCustomization characterCustomization = TitleMenu.subMenu as CharacterCustomization;
				if ((characterCustomization == null || characterCustomization.source != CharacterCustomization.Source.HostNewFarm) && !this.transitioningFromLoadScreen)
				{
					goto IL_73A;
				}
			}
			Texture2D texture = Game1.mouseCursors;
			Rectangle dstRect = new Rectangle(0, 0, this.width, this.height);
			Rectangle srcRect = new Rectangle(702, 1912, 1, 264);
			b.Draw(texture, dstRect, new Rectangle?(srcRect), Color.White * ((float)this.globalXOffset / 1200f));
			SpriteEffects effect = SpriteEffects.None;
			for (int y = 0; y < this.height; y += 195)
			{
				for (int x2 = 0; x2 < this.width; x2 += 638)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x2, (float)y) * 4f, new Rectangle?(new Rectangle(0, 1453, 638, 195)), Color.White * ((float)this.globalXOffset / 1200f), 0f, Vector2.Zero, 4f, effect, 0.8f);
				}
				effect = ((effect == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			}
			IL_73A:
			if (shouldDrawMenu)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in this.behindSignTempSprites)
				{
					temporaryAnimatedSprite2.draw(b, false, 0, 0, 1f);
				}
				if (this.showCornerClickEasterEgg && Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
				{
					float movementPercent = 1f - Math.Min(1f, 1f - this.cornerClickEndTimer / 700f);
					float yOffset = (float)(40 * zoom) * movementPercent;
					Vector2 baseVect = new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom), (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(80 * zoom), (float)(-10 * zoom) + yOffset), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(120 * zoom), (float)(-15 * zoom) + yOffset), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors, baseVect + new Vector2((float)(160 * zoom), (float)(-25 * zoom) + yOffset), new Rectangle?(new Rectangle(646, 895, 55, 48)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(220 * zoom), (float)(-15 * zoom) + yOffset), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(260 * zoom), (float)(-5 * zoom) + yOffset), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					float xOffset = (float)(40 * zoom) * movementPercent;
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-10 * zoom) + xOffset, (float)(70 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, -1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-5 * zoom) + xOffset, (float)(100 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, -1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-12 * zoom) + xOffset, (float)(130 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, -1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-10 * zoom) + xOffset, (float)(160 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, -1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					xOffset = (float)(-40 * zoom) * movementPercent;
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(410 * zoom) + xOffset, (float)(40 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(415 * zoom) + xOffset, (float)(70 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(405 * zoom) + xOffset, (float)(100 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
					b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(410 * zoom) + xOffset, (float)(130 * zoom)), new Rectangle?(new Rectangle(224, 148, 32, 21)), Color.White, 1.5707964f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.01f);
				}
				b.Draw(this.titleButtonsTexture, new Vector2((float)(this.globalXOffset + this.width / 2 - 200 * zoom), (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom), new Rectangle?(new Rectangle(0, 0, 400, 187)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.2f);
				if (this.logoSwipeTimer > 0f)
				{
					b.Draw(this.titleButtonsTexture, new Vector2((float)(this.globalXOffset + this.width / 2), (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(93 * zoom)), new Rectangle?(new Rectangle(0, 0, 400, 187)), Color.White, 0f, new Vector2(200f, 93f), (float)zoom + (0.5f - Math.Abs(this.logoSwipeTimer / 1000f - 0.5f)) * 0.1f, SpriteEffects.None, 0.2f);
				}
				if (this.cornerPhaseHolding && this.cornerClicks > 999 && Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
				{
					b.Draw(Game1.mouseCursors2, new Vector2((float)(this.globalXOffset + this.r_hole_rect.X + zoom), (float)(this.r_hole_rect.Y - 2)), new Rectangle?(new Rectangle(131, 196, 9, 10)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.24f);
				}
			}
			if (shouldDrawMenu)
			{
				bool greyButtons = TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu;
				for (int j = 0; j < this.buttonsToShow; j++)
				{
					if (this.buttons.Count > j)
					{
						this.buttons[j].draw(b, (TitleMenu.subMenu == null || !greyButtons) ? Color.White : (Color.LightGray * 0.8f), 1f, 0, 0, 0);
					}
				}
				if (TitleMenu.subMenu == null)
				{
					foreach (TemporaryAnimatedSprite temporaryAnimatedSprite3 in this.tempSprites)
					{
						temporaryAnimatedSprite3.draw(b, false, 0, 0, 1f);
					}
				}
			}
			if (TitleMenu.subMenu != null && !this.isTransitioningButtons)
			{
				if (this.backButton != null && TitleMenu.subMenu.readyToClose())
				{
					this.backButton.draw(b);
				}
				TitleMenu.subMenu.draw(b);
				if (this.backButton != null && !(TitleMenu.subMenu is CharacterCustomization) && TitleMenu.subMenu.readyToClose())
				{
					this.backButton.draw(b);
				}
			}
			else if (TitleMenu.subMenu == null && this.isTransitioningButtons && (this.whichSubMenu.Equals("Load") || this.whichSubMenu.Equals("New")))
			{
				int x3 = 84;
				int y2 = Game1.uiViewport.Height - 64;
				int w = 0;
				int h = 64;
				Utility.makeSafe(ref x3, ref y2, w, h);
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3689"), x3, y2, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
			}
			else if (TitleMenu.subMenu == null && !this.isTransitioningButtons && this.titleInPosition && !this.transitioningCharacterCreationMenu && this.HasActiveUser && shouldDrawMenu)
			{
				this.aboutButton.draw(b);
				this.languageButton.draw(b);
			}
			if (this.amuzioTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), Color.White);
				Vector2 pos = new Vector2((float)(this.width / 2 - this.amuzioTexture.Width / 2 * 4), (float)(this.height / 2 - this.amuzioTexture.Height / 2 * 4));
				pos.X = MathHelper.Lerp(pos.X, (float)(-(float)this.amuzioTexture.Width * 4), (float)Math.Max(0, this.amuzioTimer - 3750) / 250f);
				b.Draw(this.amuzioTexture, pos, null, Color.White * Math.Min(1f, (float)this.amuzioTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
			}
			else if (this.logoFadeTimer > 0 || this.fadeFromWhiteTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), Color.White * ((float)this.fadeFromWhiteTimer / 2000f));
				if (!this.specialSurprised)
				{
					b.Draw(this.titleButtonsTexture, new Vector2((float)(this.width / 2), (float)(this.height / 2 - 30 * TitleMenu.pixelZoom)), new Rectangle?(new Rectangle(171 + ((this.logoFadeTimer / 100 % 2 == 0 && this.logoSurprisedTimer <= 0) ? 111 : 0), 311, 111, 60)), Color.White * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
				}
				if (this.logoSurprisedTimer <= 0)
				{
					b.Draw(this.titleButtonsTexture, new Vector2((float)(this.width / 2 - 87 * TitleMenu.pixelZoom), (float)(this.height / 2 - 34 * TitleMenu.pixelZoom)), new Rectangle?(new Rectangle((this.logoFadeTimer / 100 % 2 == 0) ? 85 : 0, 306 + (this.shades ? 69 : 0), 85, 69)), Color.White * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
				}
				if (this.specialSurprised)
				{
					if (this.logoFadeTimer > 0)
					{
						b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), new Color(221, 255, 198));
					}
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), new Color(221, 255, 198) * ((float)this.fadeFromWhiteTimer / 2000f));
					int time = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
					for (int x4 = 64; x4 < this.width + 1000; x4 += 192)
					{
						for (int y3 = -1000; y3 < this.height; y3 += 192)
						{
							b.Draw(Game1.mouseCursors, new Vector2((float)x4, (float)y3) + new Vector2((float)(-(float)time) / 20f, (float)time / 20f), new Rectangle?(new Rectangle(355 + (time + x4 * 77 + y3 * 77) / 12 % 110 / 11 * 16, 1200, 16, 16)), Color.White * 0.66f * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.18f);
						}
					}
					b.Draw(this.titleButtonsTexture, new Vector2((float)(this.width / 2), (float)(this.height / 2 - 30 * TitleMenu.pixelZoom)), new Rectangle?(new Rectangle(171 + ((time / 200 % 2 == 0) ? 111 : 0), 563, 111, 60)), Color.White * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
					this.specialSurprisedTimeStamp += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
					Utility.drawWithShadow(b, this.titleButtonsTexture, new Vector2((float)(this.width / 2 - 87 * TitleMenu.pixelZoom), (float)(this.height / 2 - 34 * TitleMenu.pixelZoom)), new Rectangle((time / 200 % 2 == 0) ? 85 : 0, 559, 85, 69), Color.White * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, false, 0.2f, -4, -4, 0f);
				}
				else if (this.logoSurprisedTimer > 0)
				{
					b.Draw(this.titleButtonsTexture, new Vector2((float)(this.width / 2 - 87 * TitleMenu.pixelZoom), (float)(this.height / 2 - 34 * TitleMenu.pixelZoom)), new Rectangle?(new Rectangle((this.logoSurprisedTimer > 800 || this.logoSurprisedTimer < 400) ? 176 : 260, 375, 85, 69)), Color.White * ((this.logoSurprisedTimer < 200) ? ((float)this.logoSurprisedTimer / 200f) : 1f), 0f, Vector2.Zero, (float)TitleMenu.pixelZoom, SpriteEffects.None, 0.22f);
				}
				if (this.startupMessage.Length > 0 && this.logoFadeTimer > 0)
				{
					b.DrawString(Game1.smallFont, Game1.parseText(this.startupMessage, Game1.smallFont, 640), new Vector2(8f, (float)Game1.uiViewport.Height - Game1.smallFont.MeasureString(Game1.parseText(this.startupMessage, Game1.smallFont, 640)).Y - 4f), this.startupMessageColor * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)));
				}
			}
			if (this.quitTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, this.width, this.height), Color.Black * (1f - (float)this.quitTimer / 500f));
			}
			if (this.HasActiveUser)
			{
				this.muteMusicButton.draw(b);
				this.windowedButton.draw(b);
			}
			if (this.ShouldDrawCursor())
			{
				int whichCursor = -1;
				if (TitleMenu.subMenu is LoadGameMenu)
				{
					whichCursor = ((TitleMenu.subMenu as LoadGameMenu).IsDoingTask() ? 1 : -1);
				}
				base.drawMouse(b, false, whichCursor);
				if (this.cornerPhaseHolding && this.cornerClicks < 100)
				{
					b.Draw(Game1.mouseCursors2, new Vector2((float)(Game1.getMouseX() + 32 + 4), (float)(Game1.getMouseY() + 32 + 4)), new Rectangle?(new Rectangle(131, 196, 9, 10)), Color.White, 0f, Vector2.Zero, (float)zoom, SpriteEffects.None, 0.9999f);
				}
			}
		}

		// Token: 0x06002D51 RID: 11601 RVA: 0x0023685C File Offset: 0x00234A5C
		protected bool ShouldAllowInteraction()
		{
			if (this.quitTimer > 0)
			{
				return false;
			}
			if (this.isTransitioningButtons)
			{
				return false;
			}
			if (this.showButtonsTimer > 0 && this.HasActiveUser && TitleMenu.subMenu == null)
			{
				return false;
			}
			if (TitleMenu.subMenu != null)
			{
				LoadGameMenu loadGameMenu = TitleMenu.subMenu as LoadGameMenu;
				if (loadGameMenu != null && loadGameMenu.IsDoingTask())
				{
					return false;
				}
			}
			else if (!this.titleInPosition)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06002D52 RID: 11602 RVA: 0x002368C4 File Offset: 0x00234AC4
		protected bool ShouldDrawCursor()
		{
			if (!Game1.options.gamepadControls || !Game1.options.snappyMenus)
			{
				return true;
			}
			if (this.pauseBeforeViewportRiseTimer > 0)
			{
				return false;
			}
			if (this.logoSwipeTimer > 0f)
			{
				return false;
			}
			if (this.logoFadeTimer > 0)
			{
				return this._movedCursor;
			}
			return this.fadeFromWhiteTimer <= 0 && this.titleInPosition && this.viewportDY == 0f && !(TitleMenu._subMenu is TooManyFarmsMenu) && this.ShouldAllowInteraction();
		}

		// Token: 0x06002D53 RID: 11603 RVA: 0x00236958 File Offset: 0x00234B58
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			if (this.globalXOffset >= this.width)
			{
				this.globalXOffset = Game1.uiViewport.Width;
			}
			this.width = Game1.uiViewport.Width;
			this.height = Game1.uiViewport.Height;
			this.setUpIcons();
			IClickableMenu subMenu = TitleMenu.subMenu;
			if (subMenu != null)
			{
				subMenu.gameWindowSizeChanged(oldBounds, newBounds);
			}
			this.backButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(this.width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, this.height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(296, 252, 66, 27), (float)TitleMenu.pixelZoom, false)
			{
				myID = 81114
			};
			this.tempSprites.Clear();
			if (this.birds.Count > 0 && !this.titleInPosition)
			{
				for (int i = 0; i < this.birds.Count; i++)
				{
					this.birds[i].position = ((i % 2 == 0) ? new Vector2((float)(this.width - 70 * TitleMenu.pixelZoom), (float)(this.height - 120 * TitleMenu.pixelZoom)) : new Vector2((float)(this.width - 40 * TitleMenu.pixelZoom), (float)(this.height - 110 * TitleMenu.pixelZoom)));
				}
			}
			this.windowedButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f, false)
			{
				myID = 81112,
				leftNeighborID = 81111,
				downNeighborID = 81113
			};
			if (Game1.options.SnappyMenus)
			{
				int id = (this.currentlySnappedComponent != null) ? this.currentlySnappedComponent.myID : 81115;
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(id);
				if (TitleMenu._subMenu != null)
				{
					TitleMenu._subMenu.snapCursorToCurrentSnappedComponent();
					return;
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002D54 RID: 11604 RVA: 0x00236BBC File Offset: 0x00234DBC
		private void showButterflies()
		{
			Game1.playSound("yoba", null);
			int zoom = this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom;
			this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(128, 96, 16, 16), new Vector2((float)(this.width / 2 - 240 * zoom), (float)(-300 * zoom - (int)(this.viewportY / 3f) * zoom + 86 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				interval = 75f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 3200f,
				yPeriodicRange = 16f,
				xPeriodic = true,
				xPeriodicLoopTime = 5000f,
				xPeriodicRange = 21f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			TemporaryAnimatedSpriteList i = Utility.sparkleWithinArea(new Rectangle(this.width / 2 - 240 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 86 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f, 100, 0, "");
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in i)
			{
				temporaryAnimatedSprite.local = true;
				temporaryAnimatedSprite.scale = (float)zoom / 4f;
			}
			this.tempSprites.AddRange(i);
			this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(192, 96, 16, 16), new Vector2((float)(this.width / 2 + 220 * zoom), (float)(-300 * zoom - (int)(this.viewportY / 3f) * zoom + 15 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				delayBeforeAnimationStart = 10,
				interval = 70f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 2800f,
				yPeriodicRange = 12f,
				xPeriodic = true,
				xPeriodicLoopTime = 4000f,
				xPeriodicRange = 16f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			i = Utility.sparkleWithinArea(new Rectangle(this.width / 2 + 220 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 15 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f, 100, 0, "");
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in i)
			{
				temporaryAnimatedSprite2.local = true;
				temporaryAnimatedSprite2.scale = (float)zoom / 4f;
			}
			this.tempSprites.AddRange(i);
			this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(256, 96, 16, 16), new Vector2((float)(this.width / 2 - 250 * zoom), (float)(-300 * zoom - (int)(this.viewportY / 3f) * zoom + 35 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				delayBeforeAnimationStart = 20,
				interval = 65f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 3500f,
				yPeriodicRange = 16f,
				xPeriodic = true,
				xPeriodicLoopTime = 3000f,
				xPeriodicRange = 10f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			i = Utility.sparkleWithinArea(new Rectangle(this.width / 2 - 250 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 35 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f, 100, 0, "");
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite3 in i)
			{
				temporaryAnimatedSprite3.local = true;
				temporaryAnimatedSprite3.scale = (float)zoom / 4f;
			}
			this.tempSprites.AddRange(i);
			this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(256, 112, 16, 16), new Vector2((float)(this.width / 2 + 250 * zoom), (float)(-300 * zoom - (int)(this.viewportY / 3f) * zoom + 60 * zoom)), false, 0f, Color.White)
			{
				scale = (float)zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				yPeriodic = true,
				yPeriodicLoopTime = 3000f,
				yPeriodicRange = 16f,
				pingPong = true,
				delayBeforeAnimationStart = 30,
				interval = 85f,
				local = true,
				xPeriodic = true,
				xPeriodicLoopTime = 5000f,
				xPeriodicRange = 16f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			i = Utility.sparkleWithinArea(new Rectangle(this.width / 2 + 250 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 60 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f, 100, 0, "");
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite4 in i)
			{
				temporaryAnimatedSprite4.local = true;
				temporaryAnimatedSprite4.scale = (float)zoom / 4f;
			}
			this.tempSprites.AddRange(i);
		}

		// Token: 0x06002D55 RID: 11605 RVA: 0x0023724C File Offset: 0x0023544C
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = this.tempSprites;
					if (temporaryAnimatedSpriteList != null)
					{
						temporaryAnimatedSpriteList.Clear();
					}
					if (this.menuContent != null)
					{
						this.menuContent.Dispose();
						this.menuContent = null;
					}
					LocalizedContentManager.OnLanguageChange -= this.OnLanguageChange;
					TitleMenu.subMenu = null;
				}
				this.disposedValue = true;
			}
		}

		// Token: 0x06002D56 RID: 11606 RVA: 0x002372B0 File Offset: 0x002354B0
		~TitleMenu()
		{
			this.Dispose(false);
		}

		// Token: 0x06002D57 RID: 11607 RVA: 0x002372E0 File Offset: 0x002354E0
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x04001ECB RID: 7883
		public static bool SkipSplashScreens = false;

		// Token: 0x04001ECC RID: 7884
		public const int region_muteMusic = 81111;

		// Token: 0x04001ECD RID: 7885
		public const int region_windowedButton = 81112;

		// Token: 0x04001ECE RID: 7886
		public const int region_aboutButton = 81113;

		// Token: 0x04001ECF RID: 7887
		public const int region_backButton = 81114;

		// Token: 0x04001ED0 RID: 7888
		public const int region_newButton = 81115;

		// Token: 0x04001ED1 RID: 7889
		public const int region_loadButton = 81116;

		// Token: 0x04001ED2 RID: 7890
		public const int region_coopButton = 81119;

		// Token: 0x04001ED3 RID: 7891
		public const int region_exitButton = 81117;

		// Token: 0x04001ED4 RID: 7892
		public const int region_languagesButton = 81118;

		// Token: 0x04001ED5 RID: 7893
		public const int fadeFromWhiteDuration = 2000;

		// Token: 0x04001ED6 RID: 7894
		public const int viewportFinalPosition = -1000;

		// Token: 0x04001ED7 RID: 7895
		public const int logoSwipeDuration = 1000;

		// Token: 0x04001ED8 RID: 7896
		public const int numberOfButtons = 4;

		// Token: 0x04001ED9 RID: 7897
		public const int spaceBetweenButtons = 8;

		// Token: 0x04001EDA RID: 7898
		public const float bigCloudDX = 0.1f;

		// Token: 0x04001EDB RID: 7899
		public const float mediumCloudDX = 0.2f;

		// Token: 0x04001EDC RID: 7900
		public const float smallCloudDX = 0.3f;

		// Token: 0x04001EDD RID: 7901
		public const float bgmountainsParallaxSpeed = 0.66f;

		// Token: 0x04001EDE RID: 7902
		public const float mountainsParallaxSpeed = 1f;

		// Token: 0x04001EDF RID: 7903
		public const float foregroundJungleParallaxSpeed = 2f;

		// Token: 0x04001EE0 RID: 7904
		public const float cloudsParallaxSpeed = 0.5f;

		// Token: 0x04001EE1 RID: 7905
		public static int pixelZoom = 3;

		// Token: 0x04001EE2 RID: 7906
		public const string titleButtonsTextureName = "Minigames\\TitleButtons";

		// Token: 0x04001EE4 RID: 7908
		public LocalizedContentManager menuContent = Game1.content.CreateTemporary();

		// Token: 0x04001EE5 RID: 7909
		public Texture2D cloudsTexture;

		// Token: 0x04001EE6 RID: 7910
		public Texture2D titleButtonsTexture;

		// Token: 0x04001EE7 RID: 7911
		public bool specialSurprised;

		// Token: 0x04001EE8 RID: 7912
		public float specialSurprisedTimeStamp;

		// Token: 0x04001EE9 RID: 7913
		private Texture2D amuzioTexture;

		// Token: 0x04001EEA RID: 7914
		private List<float> bigClouds = new List<float>();

		// Token: 0x04001EEB RID: 7915
		private List<float> smallClouds = new List<float>();

		// Token: 0x04001EEC RID: 7916
		private TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04001EED RID: 7917
		private TemporaryAnimatedSpriteList behindSignTempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04001EEE RID: 7918
		public List<ClickableTextureComponent> buttons = new List<ClickableTextureComponent>();

		// Token: 0x04001EEF RID: 7919
		public ClickableTextureComponent backButton;

		// Token: 0x04001EF0 RID: 7920
		public ClickableTextureComponent muteMusicButton;

		// Token: 0x04001EF1 RID: 7921
		public ClickableTextureComponent aboutButton;

		// Token: 0x04001EF2 RID: 7922
		public ClickableTextureComponent languageButton;

		// Token: 0x04001EF3 RID: 7923
		public ClickableTextureComponent windowedButton;

		// Token: 0x04001EF4 RID: 7924
		public ClickableComponent skipButton;

		// Token: 0x04001EF5 RID: 7925
		protected bool _movedCursor;

		// Token: 0x04001EF6 RID: 7926
		public TemporaryAnimatedSpriteList birds = new TemporaryAnimatedSpriteList();

		// Token: 0x04001EF7 RID: 7927
		private Rectangle eRect;

		// Token: 0x04001EF8 RID: 7928
		private Rectangle screwRect;

		// Token: 0x04001EF9 RID: 7929
		private Rectangle cornerRect;

		// Token: 0x04001EFA RID: 7930
		private Rectangle r_hole_rect;

		// Token: 0x04001EFB RID: 7931
		private Rectangle r_hole_rect2;

		// Token: 0x04001EFC RID: 7932
		private List<Rectangle> leafRects;

		// Token: 0x04001EFD RID: 7933
		[InstancedStatic]
		internal static IClickableMenu _subMenu;

		// Token: 0x04001EFE RID: 7934
		public readonly StartupPreferences startupPreferences;

		// Token: 0x04001EFF RID: 7935
		public int globalXOffset;

		// Token: 0x04001F00 RID: 7936
		public float viewportY;

		// Token: 0x04001F01 RID: 7937
		public float viewportDY;

		// Token: 0x04001F02 RID: 7938
		public float logoSwipeTimer;

		// Token: 0x04001F03 RID: 7939
		public float globalCloudAlpha = 1f;

		// Token: 0x04001F04 RID: 7940
		public float cornerClickEndTimer;

		// Token: 0x04001F05 RID: 7941
		public float cornerClickParrotTimer;

		// Token: 0x04001F06 RID: 7942
		public float cornerClickSoundEffectTimer;

		// Token: 0x04001F07 RID: 7943
		private bool? hasRoomAnotherFarm = new bool?(false);

		// Token: 0x04001F08 RID: 7944
		public int fadeFromWhiteTimer;

		// Token: 0x04001F09 RID: 7945
		public int pauseBeforeViewportRiseTimer;

		// Token: 0x04001F0A RID: 7946
		public int buttonsToShow;

		// Token: 0x04001F0B RID: 7947
		public int showButtonsTimer;

		// Token: 0x04001F0C RID: 7948
		public int logoFadeTimer;

		// Token: 0x04001F0D RID: 7949
		public int logoSurprisedTimer;

		// Token: 0x04001F0E RID: 7950
		public int clicksOnE;

		// Token: 0x04001F0F RID: 7951
		public int clicksOnLeaf;

		// Token: 0x04001F10 RID: 7952
		public int clicksOnScrew;

		// Token: 0x04001F11 RID: 7953
		public int cornerClicks;

		// Token: 0x04001F12 RID: 7954
		public int buttonsDX;

		// Token: 0x04001F13 RID: 7955
		public bool titleInPosition;

		// Token: 0x04001F14 RID: 7956
		public bool isTransitioningButtons;

		// Token: 0x04001F15 RID: 7957
		public bool shades;

		// Token: 0x04001F16 RID: 7958
		public bool cornerPhaseHolding;

		// Token: 0x04001F17 RID: 7959
		public bool showCornerClickEasterEgg;

		// Token: 0x04001F18 RID: 7960
		public bool transitioningCharacterCreationMenu;

		// Token: 0x04001F19 RID: 7961
		private int amuzioTimer;

		// Token: 0x04001F1A RID: 7962
		internal static int windowNumber = 3;

		// Token: 0x04001F1B RID: 7963
		public string startupMessage = "";

		// Token: 0x04001F1C RID: 7964
		public Color startupMessageColor = Color.DeepSkyBlue;

		// Token: 0x04001F1D RID: 7965
		public string debugSaveFileToTry;

		// Token: 0x04001F1E RID: 7966
		private int bCount;

		// Token: 0x04001F1F RID: 7967
		private string whichSubMenu = "";

		// Token: 0x04001F20 RID: 7968
		private int quitTimer;

		// Token: 0x04001F21 RID: 7969
		private bool transitioningFromLoadScreen;

		// Token: 0x04001F22 RID: 7970
		[NonInstancedStatic]
		public static int ticksUntilLanguageLoad = 1;

		// Token: 0x04001F23 RID: 7971
		private bool disposedValue;
	}
}
