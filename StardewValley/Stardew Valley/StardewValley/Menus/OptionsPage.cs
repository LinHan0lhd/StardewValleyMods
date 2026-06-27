using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x02000297 RID: 663
	public class OptionsPage : IClickableMenu
	{
		// Token: 0x06002B65 RID: 11109 RVA: 0x0020D0FC File Offset: 0x0020B2FC
		public OptionsPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
			this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, height - 128 - this.upArrow.bounds.Height - 8);
			for (int i = 0; i < 7; i++)
			{
				this.optionSlots.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 80 + 4 + i * ((height - 128) / 7) + 16, width - 32, (height - 128) / 7 + 4), i.ToString() ?? "")
				{
					myID = i,
					downNeighborID = ((i < 6) ? (i + 1) : -7777),
					upNeighborID = ((i > 0) ? (i - 1) : -7777),
					fullyImmutable = true
				});
			}
			this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11233")));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11234"), 0, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11235"), 7, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11236"), 8, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11237"), 11, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11238"), 12, -1, -1));
			if (Game1.game1.IsMainInstance)
			{
				this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_GamepadMode"), 38, -1, -1));
			}
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_StowingMode"), 28, -1, -1));
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode"), 41, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11239"), 27, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11240"), 14, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_GamepadStyleMenus"), 29, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_ShowAdvancedCraftingInformation"), 34, -1, -1));
			bool showLocalCoopOptions = Game1.game1.IsMainInstance && Game1.game1.IsLocalCoopJoinable();
			if (Game1.multiplayerMode == 2 || showLocalCoopOptions)
			{
				this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_MultiplayerSection")));
			}
			if (Game1.multiplayerMode == 2 && Game1.server != null && !Game1.server.IsLocalMultiplayerInitiatedServer())
			{
				this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode"), 31, -1, -1));
				this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_IPConnections"), 30, -1, -1));
				this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_FarmhandCreation"), 32, -1, -1));
			}
			if (Game1.multiplayerMode == 2 && Game1.server != null)
			{
				this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions"), 40, -1, -1));
			}
			if (Game1.multiplayerMode == 2 && Game1.server != null && !Game1.server.IsLocalMultiplayerInitiatedServer() && Program.sdk.Networking != null)
			{
				this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:GameMenu_ServerInvite"), new Action(this.offerInvite)));
				if (Program.sdk.Networking.SupportsInviteCodes())
				{
					this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowInviteCode"), new Action(this.showInviteCode)));
				}
			}
			if (showLocalCoopOptions)
			{
				this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:StartLocalMulti"), delegate()
				{
					base.exitThisMenu(false);
					Game1.game1.ShowLocalCoopJoinMenu();
				}));
			}
			if (Game1.IsMultiplayer)
			{
				this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowReadyStatus"), 35, -1, -1));
			}
			this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11241")));
			if (Game1.game1.IsMainInstance)
			{
				this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11242"), 1, -1, -1));
				this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11243"), 2, -1, -1));
				this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11244"), 20, -1, -1));
				this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11245"), 21, -1, -1));
			}
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime"), 42, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11246"), 3, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options_ToggleAnimalSounds"), 43, -1, -1));
			this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11247")));
			if (!Game1.conventionMode && Game1.game1.IsMainInstance)
			{
				this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11248"), 13, -1, -1));
				this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11251"), 6, -1, -1));
			}
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11252"), 9, -1, -1));
			if (Game1.game1.IsMainInstance)
			{
				this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_Vsync"), 37, -1, -1));
			}
			List<string> zoom_options = new List<string>();
			for (int zoom = 75; zoom <= 150; zoom += 5)
			{
				zoom_options.Add(zoom.ToString() + "%");
			}
			this.options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage_UIScale"), 39, zoom_options, zoom_options, -1, -1));
			zoom_options = new List<string>();
			for (int zoom2 = 75; zoom2 <= 200; zoom2 += 5)
			{
				zoom_options.Add(zoom2.ToString() + "%");
			}
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11253"), 15, -1, -1));
			this.options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 18, zoom_options, zoom_options, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11266"), 19, -1, -1));
			this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11271"), 23, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11272"), 24, -1, -1));
			if (!LocalMultiplayer.IsLocalMultiplayer(false))
			{
				this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11273"), 26, -1, -1));
			}
			if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
			{
				this.options.Add(new OptionsCheckbox("使用平滑字体", 44, -1, -1));
				this.options.Add(new OptionsSlider("对话字体大小", 45, -1, -1));
			}
			else if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
			{
				this.options.Add(new OptionsCheckbox("Использовать альтернативный шрифт", 46, -1, -1));
			}
			this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11274")));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11275"), 16, -1, -1));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11276"), 22, -1, -1));
			if (Game1.game1.IsMainInstance)
			{
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11277"), -1, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11278"), 7, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11279"), 10, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11280"), 15, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11281"), 18, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11282"), 19, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11283"), 11, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11284"), 14, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11285"), 13, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11286"), 12, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11287"), 17, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\UI:Input_EmoteButton"), 33, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11288"), 16, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.toolbarSwap"), 32, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11289"), 20, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11290"), 21, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11291"), 22, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11292"), 23, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11293"), 24, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11294"), 25, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11295"), 26, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11296"), 27, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11297"), 28, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11298"), 29, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11299"), 30, this.optionSlots[0].bounds.Width, -1, -1));
				this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11300"), 31, this.optionSlots[0].bounds.Width, -1, -1));
			}
			if (Game1.game1.CanTakeScreenshots())
			{
				OptionsPage.<>c__DisplayClass13_0 CS$<>8__locals1 = new OptionsPage.<>c__DisplayClass13_0();
				CS$<>8__locals1.<>4__this = this;
				this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader")));
				CS$<>8__locals1.index = this.options.Count;
				if (!Game1.game1.CanZoomScreenshots())
				{
					OptionsButton btn = new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader").Replace(":", ""), new Action(CS$<>8__locals1.<.ctor>g__TakeScreenshot|2));
					if (Game1.game1.ScreenshotBusy)
					{
						btn.greyedOut = true;
					}
					this.options.Add(btn);
				}
				else
				{
					List<OptionsElement> list = this.options;
					string label = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254");
					int whichOptions = 36;
					List<string> list2 = new List<string>();
					list2.Add("25%");
					list2.Add("50%");
					list2.Add("75%");
					list2.Add("100%");
					List<string> list3 = new List<string>();
					list3.Add("25%");
					list3.Add("50%");
					list3.Add("75%");
					list3.Add("100%");
					list.Add(new OptionsPlusMinusButton(label, whichOptions, list2, list3, Game1.mouseCursors2, new Rectangle(72, 31, 18, 16), delegate(string selection)
					{
						Game1.flashAlpha = 1f;
						selection = selection.Substring(0, selection.Length - 1);
						int zoom3;
						if (!int.TryParse(selection, out zoom3))
						{
							zoom3 = 25;
						}
						string screenshot = Game1.game1.takeMapScreenshot(new float?((float)zoom3 / 100f), null, null);
						if (screenshot != null)
						{
							Game1.addHUDMessage(new HUDMessage(screenshot, 6));
						}
						Game1.playSound("cameraNoise", null);
					}, -1, -1));
				}
				if (Game1.game1.CanBrowseScreenshots())
				{
					this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_OpenFolder"), new Action(Game1.game1.BrowseScreenshots)));
				}
			}
		}

		// Token: 0x06002B66 RID: 11110 RVA: 0x0020E285 File Offset: 0x0020C485
		public override bool readyToClose()
		{
			return this.lastRebindTick != Game1.ticks && base.readyToClose();
		}

		// Token: 0x06002B67 RID: 11111 RVA: 0x0020E29C File Offset: 0x0020C49C
		private void waitForServerConnection(Action onConnection)
		{
			OptionsPage.<>c__DisplayClass15_0 CS$<>8__locals1 = new OptionsPage.<>c__DisplayClass15_0();
			CS$<>8__locals1.onConnection = onConnection;
			if (Game1.server == null)
			{
				return;
			}
			if (Game1.server.connected())
			{
				CS$<>8__locals1.onConnection();
				return;
			}
			CS$<>8__locals1.thisMenu = Game1.activeClickableMenu;
			Game1.activeClickableMenu = new ServerConnectionDialog(new ConfirmationDialog.behavior(CS$<>8__locals1.<waitForServerConnection>g__OnConfirm|1), new ConfirmationDialog.behavior(CS$<>8__locals1.<waitForServerConnection>g__OnClose|0));
		}

		// Token: 0x06002B68 RID: 11112 RVA: 0x0020E303 File Offset: 0x0020C503
		private void offerInvite()
		{
			this.waitForServerConnection(new Action(Game1.server.offerInvite));
		}

		// Token: 0x06002B69 RID: 11113 RVA: 0x0020E31C File Offset: 0x0020C51C
		private void showInviteCode()
		{
			IClickableMenu thisMenu = Game1.activeClickableMenu;
			this.waitForServerConnection(delegate
			{
				Game1.activeClickableMenu = new InviteCodeDialog(Game1.server.getInviteCode(), new ConfirmationDialog.behavior(base.<showInviteCode>g__OnClose|1));
			});
		}

		// Token: 0x06002B6A RID: 11114 RVA: 0x0020E34C File Offset: 0x0020C54C
		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			this.currentlySnappedComponent = base.getComponentWithID(1);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B6B RID: 11115 RVA: 0x0020E367 File Offset: 0x0020C567
		public override void applyMovementKey(int direction)
		{
			if (!this.IsDropdownActive())
			{
				base.applyMovementKey(direction);
			}
		}

		// Token: 0x06002B6C RID: 11116 RVA: 0x0020E378 File Offset: 0x0020C578
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			if (oldID == 6 && direction == 2 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4", null);
				return;
			}
			if (oldID == 0 && direction == 0)
			{
				if (this.currentItemIndex > 0)
				{
					this.upArrowPressed();
					Game1.playSound("shiny4", null);
					return;
				}
				this.currentlySnappedComponent = base.getComponentWithID(12348);
				if (this.currentlySnappedComponent != null)
				{
					this.currentlySnappedComponent.downNeighborID = 0;
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002B6D RID: 11117 RVA: 0x0020E424 File Offset: 0x0020C624
		private void setScrollBarToCurrentIndex()
		{
			if (this.options.Count > 0)
			{
				this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.options.Count - 7 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
				if (this.scrollBar.bounds.Y > this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4)
				{
					this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
				}
			}
		}

		// Token: 0x06002B6E RID: 11118 RVA: 0x0020E4F8 File Offset: 0x0020C6F8
		public override void snapCursorToCurrentSnappedComponent()
		{
			if (this.currentlySnappedComponent == null || this.currentlySnappedComponent.myID >= this.options.Count)
			{
				if (this.currentlySnappedComponent != null)
				{
					base.snapCursorToCurrentSnappedComponent();
				}
				return;
			}
			OptionsElement optionsElement = this.options[this.currentlySnappedComponent.myID + this.currentItemIndex];
			OptionsDropDown dropdown = optionsElement as OptionsDropDown;
			if (dropdown != null)
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, this.currentlySnappedComponent.bounds.Center.Y - 4);
				return;
			}
			if (optionsElement is OptionsPlusMinusButton)
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 64, this.currentlySnappedComponent.bounds.Center.Y + 4);
				return;
			}
			if (!(optionsElement is OptionsInputListener))
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 48, this.currentlySnappedComponent.bounds.Center.Y - 12);
				return;
			}
			Game1.setMousePosition(this.currentlySnappedComponent.bounds.Right - 48, this.currentlySnappedComponent.bounds.Center.Y - 12);
		}

		// Token: 0x06002B6F RID: 11119 RVA: 0x0020E644 File Offset: 0x0020C844
		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				int y2 = this.scrollBar.bounds.Y;
				this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + 20));
				float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
				this.currentItemIndex = Math.Min(this.options.Count - 7, Math.Max(0, (int)((float)this.options.Count * percentage)));
				this.setScrollBarToCurrentIndex();
				if (y2 != this.scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4", null);
					return;
				}
			}
			else if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
		}

		// Token: 0x06002B70 RID: 11120 RVA: 0x0020E7C8 File Offset: 0x0020C9C8
		public override void setCurrentlySnappedComponentTo(int id)
		{
			this.currentlySnappedComponent = base.getComponentWithID(id);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B71 RID: 11121 RVA: 0x0020E7E0 File Offset: 0x0020C9E0
		public override void receiveKeyPress(Keys key)
		{
			if ((this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
			{
				if (this.currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + this.currentlySnappedComponent.myID && this.currentItemIndex + this.currentlySnappedComponent.myID >= 0)
				{
					this.options[this.currentItemIndex + this.currentlySnappedComponent.myID].receiveKeyPress(key);
				}
				else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
				{
					this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
				}
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002B72 RID: 11122 RVA: 0x0020E8F8 File Offset: 0x0020CAF8
		public override void receiveScrollWheelAction(int direction)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (this.IsDropdownActive())
			{
				return;
			}
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4", null);
			}
			else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4", null);
			}
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002B73 RID: 11123 RVA: 0x0020E994 File Offset: 0x0020CB94
		public override void releaseLeftClick(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
			this.optionsSlotHeld = -1;
			this.scrolling = false;
		}

		// Token: 0x06002B74 RID: 11124 RVA: 0x0020EA3C File Offset: 0x0020CC3C
		public bool IsDropdownActive()
		{
			return this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count && this.options[this.currentItemIndex + this.optionsSlotHeld] is OptionsDropDown;
		}

		// Token: 0x06002B75 RID: 11125 RVA: 0x0020EA8E File Offset: 0x0020CC8E
		private void downArrowPressed()
		{
			if (this.IsDropdownActive())
			{
				return;
			}
			this.UnsubscribeFromSelectedTextbox();
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002B76 RID: 11126 RVA: 0x0020EACC File Offset: 0x0020CCCC
		public virtual void UnsubscribeFromSelectedTextbox()
		{
			if (Game1.keyboardDispatcher.Subscriber != null)
			{
				foreach (OptionsElement optionsElement in this.options)
				{
					OptionsTextEntry entry = optionsElement as OptionsTextEntry;
					if (entry != null && Game1.keyboardDispatcher.Subscriber == entry.textBox)
					{
						Game1.keyboardDispatcher.Subscriber = null;
						break;
					}
				}
			}
		}

		// Token: 0x06002B77 RID: 11127 RVA: 0x0020EB4C File Offset: 0x0020CD4C
		public void postWindowSizeChange(IClickableMenu oldPage)
		{
			OptionsPage oldCollectionsPage = oldPage as OptionsPage;
			if (oldCollectionsPage != null)
			{
				ClickableComponent currentlySnappedComponent = oldCollectionsPage.getCurrentlySnappedComponent();
				int lastSelectedIndex = (currentlySnappedComponent != null) ? currentlySnappedComponent.myID : -1;
				int lastCurrentItemIndex = oldCollectionsPage.currentItemIndex;
				if (Game1.options.SnappyMenus)
				{
					Game1.activeClickableMenu.setCurrentlySnappedComponentTo(lastSelectedIndex);
				}
				this.currentItemIndex = lastCurrentItemIndex;
				this.setScrollBarToCurrentIndex();
			}
		}

		// Token: 0x06002B78 RID: 11128 RVA: 0x0020EBA1 File Offset: 0x0020CDA1
		private void upArrowPressed()
		{
			if (this.IsDropdownActive())
			{
				return;
			}
			this.UnsubscribeFromSelectedTextbox();
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002B79 RID: 11129 RVA: 0x0020EBDC File Offset: 0x0020CDDC
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
			this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - 7, this.currentItemIndex));
			this.UnsubscribeFromSelectedTextbox();
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.optionSlots[i].bounds.Contains(x, y) && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
				{
					this.options[this.currentItemIndex + i].receiveLeftClick(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y);
					this.optionsSlotHeld = i;
					return;
				}
			}
		}

		// Token: 0x06002B7A RID: 11130 RVA: 0x0020EE08 File Offset: 0x0020D008
		public override void performHoverAction(int x, int y)
		{
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
				{
					Game1.SetFreeCursorDrag();
					break;
				}
			}
			if (this.scrollBarRunner.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
			}
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			this.hoverText = "";
			this.upArrow.tryHover(x, y, 0.1f);
			this.downArrow.tryHover(x, y, 0.1f);
			this.scrollBar.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002B7B RID: 11131 RVA: 0x0020EF00 File Offset: 0x0020D100
		public override void draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			for (int i = 0; i < this.optionSlots.Count; i++)
			{
				if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count)
				{
					this.options[this.currentItemIndex + i].draw(b, this.optionSlots[i].bounds.X, this.optionSlots[i].bounds.Y, this);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (!GameMenu.forcePreventClose)
			{
				this.upArrow.draw(b);
				this.downArrow.draw(b);
				if (this.options.Count > 7)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, false, -1f);
					this.scrollBar.draw(b);
				}
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001D11 RID: 7441
		public const int itemsPerPage = 7;

		// Token: 0x04001D12 RID: 7442
		private string hoverText = "";

		// Token: 0x04001D13 RID: 7443
		public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

		// Token: 0x04001D14 RID: 7444
		public int currentItemIndex;

		// Token: 0x04001D15 RID: 7445
		private ClickableTextureComponent upArrow;

		// Token: 0x04001D16 RID: 7446
		private ClickableTextureComponent downArrow;

		// Token: 0x04001D17 RID: 7447
		private ClickableTextureComponent scrollBar;

		// Token: 0x04001D18 RID: 7448
		private bool scrolling;

		// Token: 0x04001D19 RID: 7449
		public List<OptionsElement> options = new List<OptionsElement>();

		// Token: 0x04001D1A RID: 7450
		private Rectangle scrollBarRunner;

		// Token: 0x04001D1B RID: 7451
		internal static int _lastSelectedIndex;

		// Token: 0x04001D1C RID: 7452
		internal static int _lastCurrentItemIndex;

		// Token: 0x04001D1D RID: 7453
		public int lastRebindTick = -1;

		// Token: 0x04001D1E RID: 7454
		private int optionsSlotHeld = -1;
	}
}
