using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x02000273 RID: 627
	public class GameMenu : IClickableMenu
	{
		// Token: 0x0600296B RID: 10603 RVA: 0x001E8958 File Offset: 0x001E6B58
		public GameMenu(bool playOpeningSound = true) : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true)
		{
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "inventory", Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"))
			{
				myID = 12340,
				downNeighborID = 0,
				rightNeighborID = 12341,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new InventoryPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 128, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "skills", Game1.content.LoadString("Strings\\UI:GameMenu_Skills"))
			{
				myID = 12341,
				downNeighborID = 1,
				rightNeighborID = 12342,
				leftNeighborID = 12340,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new SkillsPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? 64 : 0), this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 192, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "social", Game1.content.LoadString("Strings\\UI:GameMenu_Social"))
			{
				myID = 12342,
				downNeighborID = 2,
				rightNeighborID = 12343,
				leftNeighborID = 12341,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new SocialPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width + 36, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 256, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "map", Game1.content.LoadString("Strings\\UI:GameMenu_Map"))
			{
				myID = 12343,
				downNeighborID = 3,
				rightNeighborID = 12344,
				leftNeighborID = 12342,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new MapPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 320, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "crafting", Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"))
			{
				myID = 12344,
				downNeighborID = 4,
				rightNeighborID = 12345,
				leftNeighborID = 12343,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new CraftingPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, false, null));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 384, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "animals", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Animals"))
			{
				myID = 12345,
				downNeighborID = 5,
				rightNeighborID = 12346,
				leftNeighborID = 12344,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new AnimalPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width - 64 - 16, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 448, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "powers", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Powers"))
			{
				myID = 12346,
				downNeighborID = 6,
				rightNeighborID = 12347,
				leftNeighborID = 12345,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new PowersTab(this.xPositionOnScreen, this.yPositionOnScreen, this.width - 64 - 16, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 512, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "collections", Game1.content.LoadString("Strings\\UI:GameMenu_Collections"))
			{
				myID = 12347,
				downNeighborID = 7,
				rightNeighborID = 12348,
				leftNeighborID = 12346,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new CollectionsPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width - 64 - 16, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 576, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "options", Game1.content.LoadString("Strings\\UI:GameMenu_Options"))
			{
				myID = 12348,
				downNeighborID = 8,
				rightNeighborID = 12349,
				leftNeighborID = 12347,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			int extraWidth = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 96 : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? 192 : 0);
			this.pages.Add(new OptionsPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width + extraWidth, this.height));
			this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 640, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "exit", Game1.content.LoadString("Strings\\UI:GameMenu_Exit"))
			{
				myID = 12349,
				downNeighborID = 9,
				leftNeighborID = 12348,
				tryDefaultIfNoDownNeighborExists = true,
				fullyImmutable = true
			});
			this.pages.Add(new ExitPage(this.xPositionOnScreen, this.yPositionOnScreen, this.width - 64 - 16, this.height));
			if (Game1.activeClickableMenu == null && playOpeningSound)
			{
				Game1.playSound("bigSelect", null);
			}
			GameMenu.forcePreventClose = false;
			Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).refreshBundlesIngredientsInfo();
			this.pages[this.currentTab].populateClickableComponentList();
			this.AddTabsToClickableComponents(this.pages[this.currentTab]);
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x0600296C RID: 10604 RVA: 0x001E912B File Offset: 0x001E732B
		public void AddTabsToClickableComponents(IClickableMenu menu)
		{
			menu.allClickableComponents.AddRange(this.tabs);
		}

		// Token: 0x0600296D RID: 10605 RVA: 0x001E913E File Offset: 0x001E733E
		public GameMenu(int startingTab, int extra = -1, bool playOpeningSound = true) : this(playOpeningSound)
		{
			this.changeTab(startingTab, false);
			if (startingTab == GameMenu.optionsTab && extra != -1)
			{
				(this.pages[GameMenu.optionsTab] as OptionsPage).currentItemIndex = extra;
			}
		}

		// Token: 0x0600296E RID: 10606 RVA: 0x001E9176 File Offset: 0x001E7376
		public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (this.GetCurrentPage() != null)
			{
				this.GetCurrentPage().automaticSnapBehavior(direction, oldRegion, oldID);
				return;
			}
			base.automaticSnapBehavior(direction, oldRegion, oldID);
		}

		// Token: 0x0600296F RID: 10607 RVA: 0x001E9198 File Offset: 0x001E7398
		public override void snapToDefaultClickableComponent()
		{
			if (this.currentTab < this.pages.Count)
			{
				this.pages[this.currentTab].snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002970 RID: 10608 RVA: 0x001E91C4 File Offset: 0x001E73C4
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button != Buttons.RightTrigger)
			{
				if (button != Buttons.LeftTrigger)
				{
					this.pages[this.currentTab].receiveGamePadButton(button);
				}
				else
				{
					if (this.currentTab == GameMenu.mapTab)
					{
						Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab - 1, -1, true);
						Game1.playSound("smallSelect", null);
						return;
					}
					if (this.currentTab > 0 && this.pages[this.currentTab].readyToClose())
					{
						this.changeTab(this.currentTab - 1, true);
						return;
					}
				}
			}
			else
			{
				if (this.currentTab == GameMenu.mapTab)
				{
					Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab + 1, -1, true);
					Game1.playSound("smallSelect", null);
					return;
				}
				if (this.currentTab < GameMenu.numberOfTabs && this.pages[this.currentTab].readyToClose())
				{
					this.changeTab(this.currentTab + 1, true);
					return;
				}
			}
		}

		// Token: 0x06002971 RID: 10609 RVA: 0x001E92DA File Offset: 0x001E74DA
		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			if (this.pages.Count > this.currentTab)
			{
				this.pages[this.currentTab].setUpForGamePadMode();
			}
		}

		// Token: 0x06002972 RID: 10610 RVA: 0x001E930B File Offset: 0x001E750B
		public override ClickableComponent getCurrentlySnappedComponent()
		{
			return this.pages[this.currentTab].getCurrentlySnappedComponent();
		}

		// Token: 0x06002973 RID: 10611 RVA: 0x001E9323 File Offset: 0x001E7523
		public override void setCurrentlySnappedComponentTo(int id)
		{
			this.pages[this.currentTab].setCurrentlySnappedComponentTo(id);
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x001E933C File Offset: 0x001E753C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			CollectionsPage collectionsPage = this.pages[this.currentTab] as CollectionsPage;
			if (((collectionsPage != null) ? collectionsPage.letterviewerSubMenu : null) == null)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (!this.invisible && !GameMenu.forcePreventClose)
			{
				for (int i = 0; i < this.tabs.Count; i++)
				{
					if (this.tabs[i].containsPoint(x, y) && this.currentTab != i && this.pages[this.currentTab].readyToClose())
					{
						this.changeTab(this.getTabNumberFromName(this.tabs[i].name), true);
						return;
					}
				}
			}
			this.pages[this.currentTab].receiveLeftClick(x, y, true);
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x001E940C File Offset: 0x001E760C
		public static string getLabelOfTabFromIndex(int index)
		{
			string translationKey;
			if (!GameMenu.TabTranslationKeys.TryGetValue(index, out translationKey))
			{
				return "";
			}
			return Game1.content.LoadString(translationKey);
		}

		// Token: 0x06002976 RID: 10614 RVA: 0x001E9439 File Offset: 0x001E7639
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.pages[this.currentTab].receiveRightClick(x, y, true);
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x001E9454 File Offset: 0x001E7654
		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			this.pages[this.currentTab].receiveScrollWheelAction(direction);
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x001E9474 File Offset: 0x001E7674
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.hoverText = "";
			this.pages[this.currentTab].performHoverAction(x, y);
			foreach (ClickableComponent c in this.tabs)
			{
				if (c.containsPoint(x, y))
				{
					this.hoverText = c.label;
					break;
				}
			}
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x001E9504 File Offset: 0x001E7704
		public int getTabNumberFromName(string name)
		{
			int whichTab = -1;
			if (name != null)
			{
				switch (name.Length)
				{
				case 3:
					if (name == "map")
					{
						whichTab = GameMenu.mapTab;
					}
					break;
				case 4:
					if (name == "exit")
					{
						whichTab = GameMenu.exitTab;
					}
					break;
				case 6:
				{
					char c = name[2];
					if (c != 'c')
					{
						if (c != 'i')
						{
							if (c == 'w')
							{
								if (name == "powers")
								{
									whichTab = GameMenu.powersTab;
								}
							}
						}
						else if (name == "skills")
						{
							whichTab = GameMenu.skillsTab;
						}
					}
					else if (name == "social")
					{
						whichTab = GameMenu.socialTab;
					}
					break;
				}
				case 7:
				{
					char c = name[0];
					if (c != 'a')
					{
						if (c == 'o')
						{
							if (name == "options")
							{
								whichTab = GameMenu.optionsTab;
							}
						}
					}
					else if (name == "animals")
					{
						whichTab = GameMenu.animalsTab;
					}
					break;
				}
				case 8:
					if (name == "crafting")
					{
						whichTab = GameMenu.craftingTab;
					}
					break;
				case 9:
					if (name == "inventory")
					{
						whichTab = GameMenu.inventoryTab;
					}
					break;
				case 11:
					if (name == "collections")
					{
						whichTab = GameMenu.collectionsTab;
					}
					break;
				}
			}
			return whichTab;
		}

		// Token: 0x0600297A RID: 10618 RVA: 0x001E968D File Offset: 0x001E788D
		public override void update(GameTime time)
		{
			base.update(time);
			this.pages[this.currentTab].update(time);
		}

		// Token: 0x0600297B RID: 10619 RVA: 0x001E96AD File Offset: 0x001E78AD
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.pages[this.currentTab].releaseLeftClick(x, y);
		}

		// Token: 0x0600297C RID: 10620 RVA: 0x001E96CF File Offset: 0x001E78CF
		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			this.pages[this.currentTab].leftClickHeld(x, y);
		}

		// Token: 0x0600297D RID: 10621 RVA: 0x001E96F1 File Offset: 0x001E78F1
		public override bool readyToClose()
		{
			return !GameMenu.forcePreventClose && this.pages[this.currentTab].readyToClose();
		}

		// Token: 0x0600297E RID: 10622 RVA: 0x001E9714 File Offset: 0x001E7914
		public void changeTab(int whichTab, bool playSound = true)
		{
			this.currentTab = this.getTabNumberFromName(this.tabs[whichTab].name);
			if (this.currentTab == GameMenu.mapTab)
			{
				this.invisible = true;
				this.width += 128;
				base.initializeUpperRightCloseButton();
			}
			else
			{
				this.lastOpenedNonMapTab = this.currentTab;
				this.width = 800 + IClickableMenu.borderWidth * 2;
				base.initializeUpperRightCloseButton();
				this.invisible = false;
			}
			if (playSound)
			{
				Game1.playSound("smallSelect", null);
			}
			this.pages[this.currentTab].populateClickableComponentList();
			this.AddTabsToClickableComponents(this.pages[this.currentTab]);
			this.setTabNeighborsForCurrentPage();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x0600297F RID: 10623 RVA: 0x001E97F4 File Offset: 0x001E79F4
		public IClickableMenu GetCurrentPage()
		{
			if (this.currentTab >= this.pages.Count || this.currentTab < 0)
			{
				return null;
			}
			return this.pages[this.currentTab];
		}

		// Token: 0x06002980 RID: 10624 RVA: 0x001E9828 File Offset: 0x001E7A28
		public void setTabNeighborsForCurrentPage()
		{
			if (this.currentTab == GameMenu.inventoryTab)
			{
				for (int i = 0; i < this.tabs.Count; i++)
				{
					this.tabs[i].downNeighborID = i;
				}
				return;
			}
			if (this.currentTab == GameMenu.exitTab)
			{
				for (int j = 0; j < this.tabs.Count; j++)
				{
					this.tabs[j].downNeighborID = 535;
				}
				return;
			}
			for (int k = 0; k < this.tabs.Count; k++)
			{
				this.tabs[k].downNeighborID = -99999;
			}
		}

		// Token: 0x06002981 RID: 10625 RVA: 0x001E98D4 File Offset: 0x001E7AD4
		public override void draw(SpriteBatch b)
		{
			if (!this.invisible)
			{
				if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
				}
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.pages[this.currentTab].width, this.pages[this.currentTab].height, false, true, null, false, true, -1, -1, -1);
				b.End();
				b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				foreach (ClickableComponent c in this.tabs)
				{
					int sheetIndex = -1;
					string name = c.name;
					if (name != null)
					{
						switch (name.Length)
						{
						case 3:
							if (name == "map")
							{
								sheetIndex = 3;
							}
							break;
						case 4:
						{
							char c2 = name[0];
							if (c2 != 'c')
							{
								if (c2 == 'e')
								{
									if (name == "exit")
									{
										sheetIndex = 7;
									}
								}
							}
							else if (name == "coop")
							{
								sheetIndex = 1;
							}
							break;
						}
						case 6:
						{
							char c2 = name[2];
							if (c2 != 'c')
							{
								if (c2 != 'i')
								{
									if (c2 == 'w')
									{
										if (name == "powers")
										{
											b.Draw(Game1.mouseCursors_1_6, new Vector2((float)c.bounds.X, (float)(c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0))), new Rectangle?(new Rectangle(216, 494, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
										}
									}
								}
								else if (name == "skills")
								{
									sheetIndex = 1;
								}
							}
							else if (name == "social")
							{
								sheetIndex = 2;
							}
							break;
						}
						case 7:
						{
							char c2 = name[0];
							if (c2 != 'a')
							{
								if (c2 == 'o')
								{
									if (name == "options")
									{
										sheetIndex = 6;
									}
								}
							}
							else if (name == "animals")
							{
								b.Draw(Game1.mouseCursors_1_6, new Vector2((float)c.bounds.X, (float)(c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0))), new Rectangle?(new Rectangle(257, 246, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
							}
							break;
						}
						case 8:
							if (name == "crafting")
							{
								sheetIndex = 4;
							}
							break;
						case 9:
						{
							char c2 = name[0];
							if (c2 != 'c')
							{
								if (c2 == 'i')
								{
									if (name == "inventory")
									{
										sheetIndex = 0;
									}
								}
							}
							else if (name == "catalogue")
							{
								sheetIndex = 7;
							}
							break;
						}
						case 11:
							if (name == "collections")
							{
								sheetIndex = 5;
							}
							break;
						}
					}
					if (sheetIndex != -1)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)c.bounds.X, (float)(c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0))), new Rectangle?(new Rectangle(sheetIndex * 16, 368, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					}
					if (c.name.Equals("skills"))
					{
						Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(c.bounds.X + 8), (float)(c.bounds.Y + 12 + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0))), 0.00011f, 3f, 2, Game1.player, 1f);
					}
				}
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				this.pages[this.currentTab].draw(b);
				if (!this.hoverText.Equals(""))
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
			else
			{
				this.pages[this.currentTab].draw(b);
			}
			if (!GameMenu.forcePreventClose && this.pages[this.currentTab].shouldDrawCloseButton())
			{
				base.draw(b);
			}
			if (Game1.options.SnappyMenus)
			{
				CollectionsPage collectionsPage = this.pages[this.currentTab] as CollectionsPage;
				if (((collectionsPage != null) ? collectionsPage.letterviewerSubMenu : null) != null)
				{
					return;
				}
			}
			if (!Game1.options.hardwareCursor)
			{
				base.drawMouse(b, true, -1);
			}
		}

		// Token: 0x06002982 RID: 10626 RVA: 0x001E9F00 File Offset: 0x001E8100
		public override bool areGamePadControlsImplemented()
		{
			return false;
		}

		// Token: 0x06002983 RID: 10627 RVA: 0x001E9F04 File Offset: 0x001E8104
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.menuButton.Contains(new InputButton(key)) && this.readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect", null);
			}
			this.pages[this.currentTab].receiveKeyPress(key);
		}

		// Token: 0x06002984 RID: 10628 RVA: 0x001E9F60 File Offset: 0x001E8160
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			this.pages[this.currentTab].emergencyShutDown();
		}

		// Token: 0x06002985 RID: 10629 RVA: 0x001E9F7E File Offset: 0x001E817E
		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			if (Game1.options.optionsDirty)
			{
				Game1.options.SaveDefaultOptions();
			}
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x001E9F9C File Offset: 0x001E819C
		// Note: this type is marked as 'beforefieldinit'.
		static GameMenu()
		{
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			int key = GameMenu.inventoryTab;
			dictionary[key] = "Strings\\UI:GameMenu_Inventory";
			int key2 = GameMenu.skillsTab;
			dictionary[key2] = "Strings\\UI:GameMenu_Skills";
			int key3 = GameMenu.socialTab;
			dictionary[key3] = "Strings\\UI:GameMenu_Social";
			int key4 = GameMenu.mapTab;
			dictionary[key4] = "Strings\\UI:GameMenu_Map";
			int key5 = GameMenu.craftingTab;
			dictionary[key5] = "Strings\\UI:GameMenu_Crafting";
			int key6 = GameMenu.powersTab;
			dictionary[key6] = "Strings\\1_6_Strings:GameMenu_Powers";
			int key7 = GameMenu.exitTab;
			dictionary[key7] = "Strings\\UI:GameMenu_Exit";
			int key8 = GameMenu.collectionsTab;
			dictionary[key8] = "Strings\\UI:GameMenu_Collections";
			int key9 = GameMenu.optionsTab;
			dictionary[key9] = "Strings\\UI:GameMenu_Options";
			int key10 = GameMenu.exitTab;
			dictionary[key10] = "Strings\\UI:GameMenu_Exit";
			GameMenu.TabTranslationKeys = dictionary;
		}

		// Token: 0x04001B0E RID: 6926
		public static readonly int inventoryTab = 0;

		// Token: 0x04001B0F RID: 6927
		public static readonly int skillsTab = 1;

		// Token: 0x04001B10 RID: 6928
		public static readonly int socialTab = 2;

		// Token: 0x04001B11 RID: 6929
		public static readonly int mapTab = 3;

		// Token: 0x04001B12 RID: 6930
		public static readonly int craftingTab = 4;

		// Token: 0x04001B13 RID: 6931
		public static readonly int animalsTab = 5;

		// Token: 0x04001B14 RID: 6932
		public static readonly int powersTab = 6;

		// Token: 0x04001B15 RID: 6933
		public static readonly int collectionsTab = 7;

		// Token: 0x04001B16 RID: 6934
		public static readonly int optionsTab = 8;

		// Token: 0x04001B17 RID: 6935
		public static readonly int exitTab = 9;

		// Token: 0x04001B18 RID: 6936
		public const int region_inventoryTab = 12340;

		// Token: 0x04001B19 RID: 6937
		public const int region_skillsTab = 12341;

		// Token: 0x04001B1A RID: 6938
		public const int region_socialTab = 12342;

		// Token: 0x04001B1B RID: 6939
		public const int region_mapTab = 12343;

		// Token: 0x04001B1C RID: 6940
		public const int region_craftingTab = 12344;

		// Token: 0x04001B1D RID: 6941
		public const int region_animalsTab = 12345;

		// Token: 0x04001B1E RID: 6942
		public const int region_powersTab = 12346;

		// Token: 0x04001B1F RID: 6943
		public const int region_collectionsTab = 12347;

		// Token: 0x04001B20 RID: 6944
		public const int region_optionsTab = 12348;

		// Token: 0x04001B21 RID: 6945
		public const int region_exitTab = 12349;

		// Token: 0x04001B22 RID: 6946
		public static readonly int numberOfTabs = 9;

		// Token: 0x04001B23 RID: 6947
		public int currentTab;

		// Token: 0x04001B24 RID: 6948
		public int lastOpenedNonMapTab = GameMenu.inventoryTab;

		// Token: 0x04001B25 RID: 6949
		public string hoverText = "";

		// Token: 0x04001B26 RID: 6950
		public string descriptionText = "";

		// Token: 0x04001B27 RID: 6951
		public List<ClickableComponent> tabs = new List<ClickableComponent>();

		// Token: 0x04001B28 RID: 6952
		public List<IClickableMenu> pages = new List<IClickableMenu>();

		// Token: 0x04001B29 RID: 6953
		public bool invisible;

		// Token: 0x04001B2A RID: 6954
		public static bool forcePreventClose;

		// Token: 0x04001B2B RID: 6955
		public static bool bundleItemHovered;

		// Token: 0x04001B2C RID: 6956
		private static readonly Dictionary<int, string> TabTranslationKeys;
	}
}
