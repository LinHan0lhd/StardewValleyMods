using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Network;
using StardewValley.SDKs;

namespace StardewValley.Menus
{
	// Token: 0x02000263 RID: 611
	public class CoopMenu : LoadGameMenu
	{
		// Token: 0x06002874 RID: 10356 RVA: 0x001D8128 File Offset: 0x001D6328
		public CoopMenu(bool tooManyFarms, bool splitScreen = false, CoopMenu.Tab initialTab = CoopMenu.Tab.JOIN_TAB, string filter = null) : base(null)
		{
			this.tooManyFarms = tooManyFarms;
			this.currentTab = initialTab;
			this.Filter = filter;
			this._splitScreen = splitScreen;
		}

		// Token: 0x06002875 RID: 10357 RVA: 0x001D817A File Offset: 0x001D637A
		public override bool readyToClose()
		{
			return !this.isSetUp || base.readyToClose();
		}

		// Token: 0x06002876 RID: 10358 RVA: 0x001D818C File Offset: 0x001D638C
		protected override bool hasDeleteButtons()
		{
			return false;
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06002877 RID: 10359 RVA: 0x001D8190 File Offset: 0x001D6390
		// (set) Token: 0x06002878 RID: 10360 RVA: 0x001D81CC File Offset: 0x001D63CC
		public override List<LoadGameMenu.MenuSlot> MenuSlots
		{
			get
			{
				if (this._splitScreen)
				{
					return this.hostSlots;
				}
				CoopMenu.Tab tab = this.currentTab;
				if (tab == CoopMenu.Tab.JOIN_TAB)
				{
					return this.menuSlots;
				}
				if (tab != CoopMenu.Tab.HOST_TAB)
				{
					return null;
				}
				return this.hostSlots;
			}
			set
			{
				if (this._splitScreen)
				{
					this.hostSlots = value;
					return;
				}
				CoopMenu.Tab tab = this.currentTab;
				if (tab == CoopMenu.Tab.JOIN_TAB)
				{
					this.menuSlots = value;
					return;
				}
				if (tab != CoopMenu.Tab.HOST_TAB)
				{
					return;
				}
				this.hostSlots = value;
			}
		}

		// Token: 0x06002879 RID: 10361 RVA: 0x001D8207 File Offset: 0x001D6407
		protected override void startListPopulation(string filter)
		{
		}

		// Token: 0x0600287A RID: 10362 RVA: 0x001D820C File Offset: 0x001D640C
		protected virtual void connectionFinished()
		{
			string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			Vector2 pos = new Vector2((float)(this.backButton.bounds.Right - width), (float)(this.backButton.bounds.Y - 128));
			this.refreshButton = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, 96), "", label)
			{
				myID = 810,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 81114
			};
			this._refreshDelay = 8f;
			this.smallScreenFormat = (Game1.graphics.GraphicsDevice.Viewport.Height < 1080);
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			pos = (this.smallScreenFormat ? new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen) : new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen - 96)));
			this.joinTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, this.smallScreenFormat ? 72 : 64), "", label)
			{
				myID = 811,
				downNeighborID = -99998,
				rightNeighborID = 812,
				region = 1000
			};
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			pos = (this.smallScreenFormat ? new Vector2((float)(this.joinTab.bounds.Right + (this.smallScreenFormat ? 0 : 4)), (float)this.yPositionOnScreen) : new Vector2((float)(this.joinTab.bounds.Right + 4), (float)(this.yPositionOnScreen - 64)));
			this.hostTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, this.smallScreenFormat ? 72 : 64), "", label)
			{
				myID = 812,
				downNeighborID = -99998,
				leftNeighborID = 811,
				rightNeighborID = 800,
				region = 1000
			};
			this.backButton.upNeighborID = 810;
			if (this.tooManyFarms)
			{
				this.hostSlots.Add(new CoopMenu.TooManyFarmsSlot(this));
			}
			else
			{
				this.hostSlots.Add(new CoopMenu.HostNewFarmSlot(this, !this._splitScreen));
			}
			if (this._splitScreen)
			{
				this.refreshButton.visible = false;
				this.joinTab.visible = false;
				this.hostTab.visible = false;
				this.backButton.upNeighborID = 0;
			}
			else
			{
				this.menuSlots.Add(new CoopMenu.LanSlot(this));
				if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
				{
					this.menuSlots.Add(new CoopMenu.InviteCodeSlot(this));
				}
				this.SetTab(this.currentTab, false);
			}
			this.isSetUp = true;
			Game1.mouseCursor = 0;
			base.startListPopulation(this.Filter);
			this.populateClickableComponentList();
		}

		// Token: 0x0600287B RID: 10363 RVA: 0x001D858C File Offset: 0x001D678C
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (this.isSetUp && !base.IsDoingTask())
			{
				if (button != Buttons.RightTrigger)
				{
					if (button == Buttons.LeftTrigger)
					{
						ClickableComponent clickableComponent = this.joinTab;
						if (clickableComponent != null && clickableComponent.visible)
						{
							this.SetTab(CoopMenu.Tab.JOIN_TAB, true);
							this.setCurrentlySnappedComponentTo(this.joinTab.myID);
							this.snapCursorToCurrentSnappedComponent();
							return;
						}
					}
				}
				else
				{
					ClickableComponent clickableComponent2 = this.hostTab;
					if (clickableComponent2 != null && clickableComponent2.visible)
					{
						this.SetTab(CoopMenu.Tab.HOST_TAB, true);
						this.setCurrentlySnappedComponentTo(this.hostTab.myID);
						this.snapCursorToCurrentSnappedComponent();
					}
				}
			}
		}

		// Token: 0x0600287C RID: 10364 RVA: 0x001D8628 File Offset: 0x001D6828
		public override void UpdateButtons()
		{
			base.UpdateButtons();
			if (this._splitScreen)
			{
				return;
			}
			foreach (ClickableComponent c in this.slotButtons)
			{
				if (c.myID == 0)
				{
					if (this.currentItemIndex == 0)
					{
						c.upNeighborID = 811;
					}
					else
					{
						c.upNeighborID = -7777;
					}
				}
			}
		}

		// Token: 0x0600287D RID: 10365 RVA: 0x001D86AC File Offset: 0x001D68AC
		public override void update(GameTime time)
		{
			float elapsed = (float)time.ElapsedGameTime.TotalSeconds;
			this.updateCounter++;
			if (!this.isSetUp)
			{
				if (this._splitScreen)
				{
					if (this.updateCounter > 1)
					{
						this.connectionFinished();
						return;
					}
				}
				else
				{
					if (Program.sdk.ConnectionFinished)
					{
						this.connectionFinished();
						return;
					}
					Game1.mouseCursor = 1;
				}
				return;
			}
			if (this.refreshButton != null && this.refreshButton.visible && this._refreshDelay > 0f)
			{
				this._refreshDelay -= elapsed;
			}
			base.update(time);
		}

		// Token: 0x0600287E RID: 10366 RVA: 0x001D8748 File Offset: 0x001D6948
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			if (this.joinTab == null || this.hostTab == null || this.backButton == null || this.refreshButton == null)
			{
				return;
			}
			this.smallScreenFormat = (Game1.graphics.GraphicsDevice.Viewport.Height < 1080);
			string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			Vector2 pos = this.smallScreenFormat ? new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen) : new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen - 96));
			this.joinTab.bounds.X = (int)pos.X;
			this.joinTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			pos = (this.smallScreenFormat ? new Vector2((float)(this.joinTab.bounds.Right + (this.smallScreenFormat ? 0 : 4)), (float)this.yPositionOnScreen) : new Vector2((float)(this.joinTab.bounds.Right + 4), (float)(this.yPositionOnScreen - 64)));
			this.hostTab.bounds.X = (int)pos.X;
			this.hostTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			pos = new Vector2((float)(this.backButton.bounds.Right - width), (float)(this.backButton.bounds.Y - 128));
			this.refreshButton.bounds.X = (int)pos.X;
			this.refreshButton.bounds.Y = (int)pos.Y;
		}

		// Token: 0x0600287F RID: 10367 RVA: 0x001D8934 File Offset: 0x001D6B34
		protected override void saveFileScanComplete()
		{
			if (this._splitScreen)
			{
				return;
			}
			if (Program.sdk.Networking != null)
			{
				this.lobbyUpdateListener = new CoopMenu.LobbyUpdateCallback(new Action<object>(this.onLobbyUpdate));
				Program.sdk.Networking.AddLobbyUpdateListener(this.lobbyUpdateListener);
				Program.sdk.Networking.RequestFriendLobbyData();
			}
		}

		// Token: 0x06002880 RID: 10368 RVA: 0x001D8994 File Offset: 0x001D6B94
		protected virtual CoopMenu.FriendFarmData readLobbyFarmData(object lobby)
		{
			CoopMenu.FriendFarmData farm = new CoopMenu.FriendFarmData
			{
				Lobby = lobby,
				Date = new WorldDate()
			};
			farm.OwnerName = Program.sdk.Networking.GetLobbyOwnerName(lobby);
			farm.FarmName = Program.sdk.Networking.GetLobbyData(lobby, "farmName");
			string farmType = Program.sdk.Networking.GetLobbyData(lobby, "farmType");
			string mod_farm_type = Program.sdk.Networking.GetLobbyData(lobby, "modFarmType");
			string lobbyData = Program.sdk.Networking.GetLobbyData(lobby, "date");
			int farmType_i = Convert.ToInt32(farmType);
			int farmDate_day = Convert.ToInt32(lobbyData);
			farm.FarmType = farmType_i;
			farm.ModFarmType = null;
			if (!string.IsNullOrEmpty(mod_farm_type))
			{
				List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
				if (farm_types != null)
				{
					foreach (ModFarmType farm_type in farm_types)
					{
						if (farm_type.Id == mod_farm_type)
						{
							farm.ModFarmType = farm_type;
							break;
						}
					}
				}
			}
			farm.Date.TotalDays = farmDate_day;
			farm.ProtocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
			farm.FarmName = Program.sdk.FilterDirtyWords(farm.FarmName);
			farm.OwnerName = Program.sdk.FilterDirtyWords(farm.OwnerName);
			return farm;
		}

		// Token: 0x06002881 RID: 10369 RVA: 0x001D8B0C File Offset: 0x001D6D0C
		protected virtual bool checkFriendFarmCompatibility(CoopMenu.FriendFarmData farm)
		{
			return farm.FarmType >= 0 && farm.FarmType <= 7 && !(farm.ProtocolVersion != Multiplayer.protocolVersion);
		}

		// Token: 0x06002882 RID: 10370 RVA: 0x001D8B38 File Offset: 0x001D6D38
		protected virtual void onLobbyUpdate(object lobby)
		{
			try
			{
				string protocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
				if (!(protocolVersion != Multiplayer.protocolVersion))
				{
					Game1.log.Verbose(string.Concat(new string[]
					{
						"Receiving friend lobby data...\nOwner: ",
						Program.sdk.Networking.GetLobbyOwnerName(lobby),
						"\nfarmName = ",
						Program.sdk.Networking.GetLobbyData(lobby, "farmName"),
						"\nfarmType = ",
						Program.sdk.Networking.GetLobbyData(lobby, "farmType"),
						"\ndate = ",
						Program.sdk.Networking.GetLobbyData(lobby, "date"),
						"\nprotocolVersion = ",
						protocolVersion,
						"\nfarmhands = ",
						Program.sdk.Networking.GetLobbyData(lobby, "farmhands"),
						"\nnewFarmhands = ",
						Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands")
					}));
					CoopMenu.FriendFarmData farm = this.readLobbyFarmData(lobby);
					if (this.checkFriendFarmCompatibility(farm))
					{
						if (farm.FarmType != 7 || farm.ModFarmType != null)
						{
							string selfID = Program.sdk.Networking.GetUserID();
							string farmhands = Program.sdk.Networking.GetLobbyData(lobby, "farmhands");
							bool newFarmhands = Convert.ToBoolean(Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
							if (!(farmhands == "") || newFarmhands)
							{
								string[] farmUsers = farmhands.Split(',', StringSplitOptions.None);
								if (farmUsers.Contains(selfID) || newFarmhands)
								{
									farm.PreviouslyJoined = farmUsers.Contains(selfID);
									if (this.menuSlots != null)
									{
										foreach (LoadGameMenu.MenuSlot menuSlot in this.menuSlots)
										{
											CoopMenu.FriendFarmSlot farmSlot = menuSlot as CoopMenu.FriendFarmSlot;
											if (farmSlot != null && farmSlot.MatchAddress(lobby))
											{
												farmSlot.Update(farm);
												return;
											}
										}
										this.menuSlots.Add(new CoopMenu.FriendFarmSlot(this, farm));
										this.UpdateButtons();
										this.populateClickableComponentList();
									}
								}
							}
						}
					}
				}
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}
		}

		// Token: 0x06002883 RID: 10371 RVA: 0x001D8DCC File Offset: 0x001D6FCC
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (a.region != 1000 || (direction != 2 && direction != 0) || b.region != 1000) && (a.myID != 810 || direction != 0 || b.region == 900) && (a.myID != 810 || direction != 1 || b.myID != 81114) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x06002884 RID: 10372 RVA: 0x001D8E44 File Offset: 0x001D7044
		protected override void addSaveFiles(List<Farmer> files)
		{
			this.hostSlots.AddRange(from file in files
			where file.slotCanHost
			select new CoopMenu.HostFileSlot(this, !this._splitScreen, file));
			this.UpdateButtons();
		}

		// Token: 0x06002885 RID: 10373 RVA: 0x001D8E98 File Offset: 0x001D7098
		protected virtual void setMenu(IClickableMenu menu)
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				TitleMenu.subMenu = menu;
				return;
			}
			Game1.activeClickableMenu = menu;
		}

		// Token: 0x06002886 RID: 10374 RVA: 0x001D8EB4 File Offset: 0x001D70B4
		private void enterIPPressed()
		{
			string last_entered_ip = "";
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(false, false);
				last_entered_ip = startupPreferences.lastEnteredIP;
			}
			catch (Exception)
			{
			}
			string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterIP");
			this.setMenu(new TitleTextInputMenu(title, delegate(string address)
			{
				try
				{
					StartupPreferences startupPreferences2 = new StartupPreferences();
					startupPreferences2.loadPreferences(false, false);
					startupPreferences2.lastEnteredIP = address;
					startupPreferences2.savePreferences(false, false);
				}
				catch (Exception)
				{
				}
				if (address == "")
				{
					address = "localhost";
				}
				this.setMenu(new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address))));
			}, last_entered_ip, "join_menu", false));
		}

		// Token: 0x06002887 RID: 10375 RVA: 0x001D8F20 File Offset: 0x001D7120
		private void enterInviteCodePressed()
		{
			if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
			{
				string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode");
				this.setMenu(new TitleTextInputMenu(title, delegate(string code)
				{
					CoopMenu.lastEnteredInviteCode = code;
					object lobby = Program.sdk.Networking.GetLobbyFromInviteCode(code);
					if (lobby != null)
					{
						Client client = Program.sdk.Networking.CreateClient(lobby);
						this.setMenu(new FarmhandMenu(client));
					}
				}, CoopMenu.lastEnteredInviteCode, "join_menu", false));
			}
		}

		// Token: 0x06002888 RID: 10376 RVA: 0x001D8F80 File Offset: 0x001D7180
		private bool tabClick(int x, int y)
		{
			if (this.joinTab.visible && this.joinTab.containsPoint(x, y))
			{
				this.SetTab(CoopMenu.Tab.JOIN_TAB, true);
				return true;
			}
			if (this.hostTab.visible && this.hostTab.containsPoint(x, y))
			{
				this.SetTab(CoopMenu.Tab.HOST_TAB, true);
				return true;
			}
			return false;
		}

		// Token: 0x06002889 RID: 10377 RVA: 0x001D8FDC File Offset: 0x001D71DC
		public virtual void SetTab(CoopMenu.Tab newTab, bool playSound = true)
		{
			if (this.currentTab != newTab)
			{
				this.currentTab = newTab;
				if (!this.smallScreenFormat && this.isSetUp)
				{
					if (this.currentTab == CoopMenu.Tab.HOST_TAB)
					{
						this.hostTab.bounds.Y = this.yPositionOnScreen - 96;
						this.joinTab.bounds.Y = this.yPositionOnScreen - 64;
					}
					else
					{
						this.hostTab.bounds.Y = this.yPositionOnScreen - 64;
						this.joinTab.bounds.Y = this.yPositionOnScreen - 96;
					}
				}
				if (playSound)
				{
					Game1.playSound("smallSelect", null);
				}
				if (this.isSetUp)
				{
					this.UpdateButtons();
				}
				this.currentItemIndex = 0;
			}
		}

		// Token: 0x0600288A RID: 10378 RVA: 0x001D90A8 File Offset: 0x001D72A8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.isSetUp)
			{
				return;
			}
			if (this.refreshButton.visible && this.refreshButton.containsPoint(x, y))
			{
				if (this._refreshDelay < 0f)
				{
					Game1.playSound("bigDeSelect", null);
					this.setMenu(new CoopMenu(this.tooManyFarms, this._splitScreen, CoopMenu.Tab.JOIN_TAB, null));
				}
				return;
			}
			if (this.smallScreenFormat && this.tabClick(x, y))
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (!this.smallScreenFormat && !this.loading)
			{
				this.tabClick(x, y);
			}
		}

		// Token: 0x0600288B RID: 10379 RVA: 0x001D914C File Offset: 0x001D734C
		public override void performHoverAction(int x, int y)
		{
			if (!this.isSetUp)
			{
				return;
			}
			if (this.refreshButton.visible && this.refreshButton.containsPoint(x, y))
			{
				this.refreshButton.scale = 1f;
			}
			else
			{
				this.refreshButton.scale = 0f;
			}
			if (this.smallScreenFormat && (this.hostTab.containsPoint(x, y) || this.joinTab.containsPoint(x, y)))
			{
				base.performHoverAction(-100, -100);
				return;
			}
			base.performHoverAction(x, y);
		}

		// Token: 0x0600288C RID: 10380 RVA: 0x001D91D9 File Offset: 0x001D73D9
		protected override string getStatusText()
		{
			return null;
		}

		// Token: 0x0600288D RID: 10381 RVA: 0x001D91DC File Offset: 0x001D73DC
		private void drawTabs(SpriteBatch b)
		{
			if (this._splitScreen)
			{
				return;
			}
			if (!this.isSetUp)
			{
				return;
			}
			Color selectColor = this.smallScreenFormat ? Color.Orange : new Color(255, 255, 150);
			Color hoverColor = Color.Yellow;
			Color selectShadow = this.smallScreenFormat ? Color.DarkOrange : Game1.textShadowDarkerColor;
			Color hoverShadow = Color.DarkGoldenrod;
			if (this.joinTab.visible)
			{
				bool colorSelect = this.currentTab == CoopMenu.Tab.JOIN_TAB;
				bool colorHover = this.currentTab != CoopMenu.Tab.JOIN_TAB && this.joinTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.joinTab.bounds.X, this.joinTab.bounds.Y, this.joinTab.bounds.Width, this.joinTab.bounds.Height + (this.smallScreenFormat ? 0 : 64), colorSelect ? selectColor : (colorHover ? hoverColor : Color.White), 1f, false, -1f);
				Utility.drawTextWithColoredShadow(b, this.joinTab.label, Game1.dialogueFont, new Vector2((float)this.joinTab.bounds.Center.X, (float)(this.joinTab.bounds.Y + 40)) - Game1.dialogueFont.MeasureString(this.joinTab.label) / 2f, Game1.textColor, colorHover ? hoverShadow : (colorSelect ? selectShadow : Game1.textShadowDarkerColor), 1.01f, -1f, -1, -1, 3);
			}
			if (this.hostTab.visible)
			{
				bool colorSelect2 = this.currentTab == CoopMenu.Tab.HOST_TAB;
				bool colorHover2 = this.currentTab != CoopMenu.Tab.HOST_TAB && this.hostTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.hostTab.bounds.X, this.hostTab.bounds.Y, this.hostTab.bounds.Width, this.hostTab.bounds.Height + (this.smallScreenFormat ? 0 : 64), colorSelect2 ? selectColor : (colorHover2 ? hoverColor : Color.White), 1f, false, -1f);
				Utility.drawTextWithColoredShadow(b, this.hostTab.label, Game1.dialogueFont, new Vector2((float)this.hostTab.bounds.Center.X, (float)(this.hostTab.bounds.Y + 40)) - Game1.dialogueFont.MeasureString(this.hostTab.label) / 2f, Game1.textColor, colorHover2 ? hoverShadow : (colorSelect2 ? selectShadow : Game1.textShadowDarkerColor), 1.01f, -1f, -1, -1, 3);
			}
		}

		// Token: 0x0600288E RID: 10382 RVA: 0x001D94E3 File Offset: 0x001D76E3
		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			if (this.currentlySnappedComponent == null)
			{
				if (!this._splitScreen)
				{
					this.currentlySnappedComponent = base.getComponentWithID(811);
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x0600288F RID: 10383 RVA: 0x001D9512 File Offset: 0x001D7712
		protected override void drawBefore(SpriteBatch b)
		{
			base.drawBefore(b);
			if (!this.isSetUp)
			{
				return;
			}
			if (!this.smallScreenFormat)
			{
				this.drawTabs(b);
			}
		}

		// Token: 0x06002890 RID: 10384 RVA: 0x001D9534 File Offset: 0x001D7734
		protected override void drawExtra(SpriteBatch b)
		{
			base.drawExtra(b);
			if (!this.isSetUp)
			{
				return;
			}
			if (this.refreshButton.visible)
			{
				Color color = (this.refreshButton.scale > 0f) ? Color.Wheat : Color.White;
				if (this._refreshDelay > 0f)
				{
					color = Color.Gray;
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), this.refreshButton.bounds.X, this.refreshButton.bounds.Y, this.refreshButton.bounds.Width, this.refreshButton.bounds.Height, color, 4f, true, -1f);
				Utility.drawTextWithShadow(b, this.refreshButton.label, Game1.dialogueFont, new Vector2((float)this.refreshButton.bounds.Center.X, (float)(this.refreshButton.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.refreshButton.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f, 3);
			}
			if (this.smallScreenFormat)
			{
				this.drawTabs(b);
			}
		}

		// Token: 0x06002891 RID: 10385 RVA: 0x001D9690 File Offset: 0x001D7890
		protected override void drawStatusText(SpriteBatch b)
		{
			if (this._splitScreen)
			{
				return;
			}
			if (this.getStatusText() != null)
			{
				base.drawStatusText(b);
				return;
			}
			if (!this.isSetUp)
			{
				int maxEllipsis = 1 + Program.sdk.ConnectionProgress;
				int ellipsisCount = this.updateCounter / 5 % maxEllipsis;
				string basicText = Game1.content.LoadString("Strings\\UI:CoopMenu_ConnectingOnlineServices");
				this._stringBuilder.Clear();
				this._stringBuilder.Append(basicText);
				for (int i = 0; i < ellipsisCount; i++)
				{
					this._stringBuilder.Append(".");
				}
				string currentText = this._stringBuilder.ToString();
				for (int j = ellipsisCount; j < maxEllipsis; j++)
				{
					this._stringBuilder.Append(".");
				}
				int maxWidth = SpriteText.getWidthOfString(this._stringBuilder.ToString(), 999999);
				SpriteText.drawString(b, currentText, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - maxWidth / 2, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}
		}

		// Token: 0x06002892 RID: 10386 RVA: 0x001D97E8 File Offset: 0x001D79E8
		protected override void Dispose(bool disposing)
		{
			if (!this._splitScreen)
			{
				if (this.lobbyUpdateListener != null && Program.sdk.Networking != null)
				{
					Program.sdk.Networking.RemoveLobbyUpdateListener(this.lobbyUpdateListener);
				}
				this.lobbyUpdateListener = null;
			}
			base.Dispose(disposing);
		}

		// Token: 0x04001A35 RID: 6709
		public const int region_refresh = 810;

		// Token: 0x04001A36 RID: 6710
		public const int region_joinTab = 811;

		// Token: 0x04001A37 RID: 6711
		public const int region_hostTab = 812;

		// Token: 0x04001A38 RID: 6712
		public const int region_tabs = 1000;

		// Token: 0x04001A39 RID: 6713
		protected List<LoadGameMenu.MenuSlot> hostSlots = new List<LoadGameMenu.MenuSlot>();

		// Token: 0x04001A3A RID: 6714
		public ClickableComponent refreshButton;

		// Token: 0x04001A3B RID: 6715
		public ClickableComponent joinTab;

		// Token: 0x04001A3C RID: 6716
		public ClickableComponent hostTab;

		// Token: 0x04001A3D RID: 6717
		private LobbyUpdateListener lobbyUpdateListener;

		// Token: 0x04001A3E RID: 6718
		public CoopMenu.Tab currentTab;

		// Token: 0x04001A3F RID: 6719
		private bool smallScreenFormat;

		// Token: 0x04001A40 RID: 6720
		private bool isSetUp;

		// Token: 0x04001A41 RID: 6721
		private int updateCounter;

		// Token: 0x04001A42 RID: 6722
		private string Filter;

		// Token: 0x04001A43 RID: 6723
		private float _refreshDelay = -1f;

		// Token: 0x04001A44 RID: 6724
		public bool tooManyFarms;

		// Token: 0x04001A45 RID: 6725
		private readonly bool _splitScreen;

		// Token: 0x04001A46 RID: 6726
		public static string lastEnteredInviteCode;

		// Token: 0x04001A47 RID: 6727
		private StringBuilder _stringBuilder = new StringBuilder();

		// Token: 0x020005F9 RID: 1529
		public enum Tab
		{
			// Token: 0x04002E3B RID: 11835
			JOIN_TAB,
			// Token: 0x04002E3C RID: 11836
			HOST_TAB
		}

		// Token: 0x020005FA RID: 1530
		protected abstract class CoopMenuSlot : LoadGameMenu.MenuSlot
		{
			// Token: 0x060043CF RID: 17359 RVA: 0x0031AF05 File Offset: 0x00319105
			public CoopMenuSlot(CoopMenu menu) : base(menu)
			{
				this.menu = menu;
			}

			// Token: 0x04002E3D RID: 11837
			protected new CoopMenu menu;
		}

		// Token: 0x020005FB RID: 1531
		protected abstract class LabeledSlot : CoopMenu.CoopMenuSlot
		{
			// Token: 0x060043D0 RID: 17360 RVA: 0x0031AF15 File Offset: 0x00319115
			public LabeledSlot(CoopMenu menu, string message) : base(menu)
			{
				this.message = message;
			}

			// Token: 0x060043D1 RID: 17361
			public abstract override void Activate();

			// Token: 0x060043D2 RID: 17362 RVA: 0x0031AF28 File Offset: 0x00319128
			public override void Draw(SpriteBatch b, int i)
			{
				int strWidth = SpriteText.getWidthOfString(this.message, 999999);
				int strHeight = SpriteText.getHeightOfString(this.message, 999999);
				Rectangle bounds = this.menu.slotButtons[i].bounds;
				int x = bounds.X + (bounds.Width - strWidth) / 2;
				int y = bounds.Y + (bounds.Height - strHeight) / 2;
				SpriteText.drawString(b, this.message, x, y, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}

			// Token: 0x04002E3E RID: 11838
			private string message;
		}

		// Token: 0x020005FC RID: 1532
		protected class LanSlot : CoopMenu.LabeledSlot
		{
			// Token: 0x060043D3 RID: 17363 RVA: 0x0031AFC9 File Offset: 0x003191C9
			public LanSlot(CoopMenu menu) : base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_JoinLANGame"))
			{
			}

			// Token: 0x060043D4 RID: 17364 RVA: 0x0031AFE1 File Offset: 0x003191E1
			public override void Activate()
			{
				this.menu.enterIPPressed();
			}
		}

		// Token: 0x020005FD RID: 1533
		protected class InviteCodeSlot : CoopMenu.LabeledSlot
		{
			// Token: 0x060043D5 RID: 17365 RVA: 0x0031AFEE File Offset: 0x003191EE
			public InviteCodeSlot(CoopMenu menu) : base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode"))
			{
			}

			// Token: 0x060043D6 RID: 17366 RVA: 0x0031B006 File Offset: 0x00319206
			public override void Activate()
			{
				this.menu.enterInviteCodePressed();
			}
		}

		// Token: 0x020005FE RID: 1534
		protected class HostNewFarmSlot : CoopMenu.LabeledSlot
		{
			// Token: 0x060043D7 RID: 17367 RVA: 0x0031B013 File Offset: 0x00319213
			public HostNewFarmSlot(CoopMenu menu, bool multiplayer) : base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_HostNewFarm"))
			{
				this.ActivateDelay = 2150;
				this._multiplayer = multiplayer;
			}

			// Token: 0x060043D8 RID: 17368 RVA: 0x0031B03D File Offset: 0x0031923D
			public override void Activate()
			{
				Game1.resetPlayer();
				TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.HostNewFarm, this._multiplayer);
				Game1.changeMusicTrack("CloudCountry", false, MusicContext.Default);
			}

			// Token: 0x04002E3F RID: 11839
			private bool _multiplayer;
		}

		// Token: 0x020005FF RID: 1535
		protected class TooManyFarmsSlot : CoopMenu.LabeledSlot
		{
			// Token: 0x060043D9 RID: 17369 RVA: 0x0031B061 File Offset: 0x00319261
			public TooManyFarmsSlot(CoopMenu menu) : base(menu, Game1.content.LoadString("Strings\\UI:TooManyFarmsMenu_TooManyFarms"))
			{
			}

			// Token: 0x060043DA RID: 17370 RVA: 0x0031B079 File Offset: 0x00319279
			public override void Activate()
			{
			}
		}

		// Token: 0x02000600 RID: 1536
		protected class HostFileSlot : LoadGameMenu.SaveFileSlot
		{
			// Token: 0x060043DB RID: 17371 RVA: 0x0031B07C File Offset: 0x0031927C
			public HostFileSlot(CoopMenu menu, bool multiplayer, Farmer farmer) : base(menu, farmer, null)
			{
				this.menu = menu;
				this._multiplayer = multiplayer;
			}

			// Token: 0x060043DC RID: 17372 RVA: 0x0031B0A8 File Offset: 0x003192A8
			public override void Activate()
			{
				Game1.multiplayerMode = (this._multiplayer ? 2 : 0);
				base.Activate();
			}

			// Token: 0x060043DD RID: 17373 RVA: 0x0031B0C1 File Offset: 0x003192C1
			protected override void drawSlotSaveNumber(SpriteBatch b, int i)
			{
			}

			// Token: 0x060043DE RID: 17374 RVA: 0x0031B0C3 File Offset: 0x003192C3
			protected override string slotName()
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_HostFile", this.Farmer.Name, this.Farmer.farmName.Value);
			}

			// Token: 0x060043DF RID: 17375 RVA: 0x0031B0EF File Offset: 0x003192EF
			protected override string slotSubName()
			{
				return this.Farmer.Name;
			}

			// Token: 0x060043E0 RID: 17376 RVA: 0x0031B0FC File Offset: 0x003192FC
			protected override Vector2 portraitOffset()
			{
				return base.portraitOffset() - new Vector2(32f, 0f);
			}

			// Token: 0x04002E40 RID: 11840
			protected new CoopMenu menu;

			// Token: 0x04002E41 RID: 11841
			private bool _multiplayer;
		}

		// Token: 0x02000601 RID: 1537
		protected class FriendFarmData
		{
			// Token: 0x04002E42 RID: 11842
			public object Lobby;

			// Token: 0x04002E43 RID: 11843
			public string OwnerName;

			// Token: 0x04002E44 RID: 11844
			public string FarmName;

			// Token: 0x04002E45 RID: 11845
			public int FarmType;

			// Token: 0x04002E46 RID: 11846
			public ModFarmType ModFarmType;

			// Token: 0x04002E47 RID: 11847
			public WorldDate Date;

			// Token: 0x04002E48 RID: 11848
			public bool PreviouslyJoined;

			// Token: 0x04002E49 RID: 11849
			public string ProtocolVersion;
		}

		// Token: 0x02000602 RID: 1538
		protected class FriendFarmSlot : CoopMenu.CoopMenuSlot
		{
			// Token: 0x060043E2 RID: 17378 RVA: 0x0031B120 File Offset: 0x00319320
			public FriendFarmSlot(CoopMenu menu, CoopMenu.FriendFarmData farm) : base(menu)
			{
				this.Farm = farm;
			}

			// Token: 0x060043E3 RID: 17379 RVA: 0x0031B130 File Offset: 0x00319330
			public bool MatchAddress(object Lobby)
			{
				return object.Equals(this.Farm.Lobby, Lobby);
			}

			// Token: 0x060043E4 RID: 17380 RVA: 0x0031B143 File Offset: 0x00319343
			public void Update(CoopMenu.FriendFarmData newData)
			{
				this.Farm = newData;
			}

			// Token: 0x060043E5 RID: 17381 RVA: 0x0031B14C File Offset: 0x0031934C
			public override void Activate()
			{
				this.menu.setMenu(new FarmhandMenu(Program.sdk.Networking.CreateClient(this.Farm.Lobby)));
			}

			// Token: 0x060043E6 RID: 17382 RVA: 0x0031B178 File Offset: 0x00319378
			protected virtual string slotName()
			{
				string messageKey = this.Farm.PreviouslyJoined ? "Strings\\UI:CoopMenu_RevisitFriendFarm" : "Strings\\UI:CoopMenu_JoinFriendFarm";
				return Game1.content.LoadString(messageKey, this.Farm.FarmName);
			}

			// Token: 0x060043E7 RID: 17383 RVA: 0x0031B1B8 File Offset: 0x003193B8
			protected virtual void drawSlotName(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, this.slotName(), this.menu.slotButtons[i].bounds.X + 128 + 36, this.menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}

			// Token: 0x060043E8 RID: 17384 RVA: 0x0031B23C File Offset: 0x0031943C
			protected virtual void drawSlotDate(SpriteBatch b, int i)
			{
				Utility.drawTextWithShadow(b, this.Farm.Date.Localize(), Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + 128 + 32), (float)(this.menu.slotButtons[i].bounds.Y + 64 + 40)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x060043E9 RID: 17385 RVA: 0x0031B2C8 File Offset: 0x003194C8
			protected virtual void drawSlotFarm(SpriteBatch b, int i)
			{
				int drawn_farm_type = this.Farm.FarmType;
				if (drawn_farm_type == 7)
				{
					drawn_farm_type = 0;
				}
				Rectangle sourceRect = new Rectangle(22 * (drawn_farm_type % 5), 324 + 21 * (drawn_farm_type / 5), 22, 20);
				Texture2D texture = Game1.mouseCursors;
				Rectangle space = new Rectangle(this.menu.slotButtons[i].bounds.X, this.menu.slotButtons[i].bounds.Y, 160, this.menu.slotButtons[i].bounds.Height);
				Rectangle destRect = new Rectangle(space.X + (space.Width - sourceRect.Width * 4) / 2, space.Y + (space.Height - sourceRect.Height * 4) / 2, sourceRect.Width * 4, sourceRect.Height * 4);
				ModFarmType modFarmType = this.Farm.ModFarmType;
				if (((modFarmType != null) ? modFarmType.IconTexture : null) != null)
				{
					texture = Game1.content.Load<Texture2D>(this.Farm.ModFarmType.IconTexture);
					b.Draw(texture, destRect, null, Color.White);
					return;
				}
				b.Draw(texture, destRect, new Rectangle?(sourceRect), Color.White);
			}

			// Token: 0x060043EA RID: 17386 RVA: 0x0031B410 File Offset: 0x00319610
			protected virtual void drawSlotOwnerName(SpriteBatch b, int i)
			{
				float scale = 1f;
				float x_pos_offset = 128f;
				float y_pos_offset = 44f;
				Utility.drawTextWithShadow(b, this.Farm.OwnerName, Game1.dialogueFont, new Vector2((float)(this.menu.slotButtons[i].bounds.X + this.menu.width) - x_pos_offset - Game1.dialogueFont.MeasureString(this.Farm.OwnerName).X * scale, (float)this.menu.slotButtons[i].bounds.Y + y_pos_offset), Game1.textColor, scale, -1f, -1, -1, 1f, 3);
			}

			// Token: 0x060043EB RID: 17387 RVA: 0x0031B4C2 File Offset: 0x003196C2
			public override void Draw(SpriteBatch b, int i)
			{
				this.drawSlotName(b, i);
				this.drawSlotDate(b, i);
				this.drawSlotFarm(b, i);
				this.drawSlotOwnerName(b, i);
			}

			// Token: 0x04002E4A RID: 11850
			public CoopMenu.FriendFarmData Farm;
		}

		// Token: 0x02000603 RID: 1539
		public class LobbyUpdateCallback : LobbyUpdateListener
		{
			// Token: 0x060043EC RID: 17388 RVA: 0x0031B4E4 File Offset: 0x003196E4
			public LobbyUpdateCallback(Action<object> callback)
			{
				this.callback = callback;
			}

			// Token: 0x060043ED RID: 17389 RVA: 0x0031B4F3 File Offset: 0x003196F3
			public void OnLobbyUpdate(object lobby)
			{
				Action<object> action = this.callback;
				if (action == null)
				{
					return;
				}
				action(lobby);
			}

			// Token: 0x04002E4B RID: 11851
			private Action<object> callback;
		}
	}
}
