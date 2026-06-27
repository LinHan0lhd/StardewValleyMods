using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Menus
{
	// Token: 0x020002A6 RID: 678
	public class ShippingMenu : IClickableMenu
	{
		// Token: 0x06002C2C RID: 11308 RVA: 0x0021B17C File Offset: 0x0021937C
		public ShippingMenu(IList<Item> items) : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, false)
		{
			this._activated = false;
			this.parseItems(items);
			if (!Game1.wasRainingYesterday)
			{
				Game1.changeMusicTrack(Game1.IsSummer ? "nightTime" : "none", false, MusicContext.Default);
			}
			this.wasGreenRain = Utility.isGreenRainDay(Game1.dayOfMonth - 1, Game1.season);
			this.categoryLabelsWidth = 512;
			this.plusButtonWidth = 40;
			this.itemSlotWidth = 96;
			this.itemAndPlusButtonWidth = this.plusButtonWidth + this.itemSlotWidth + 8;
			this.totalWidth = this.categoryLabelsWidth + this.itemAndPlusButtonWidth;
			this.centerX = Game1.uiViewport.Width / 2;
			this.centerY = Game1.uiViewport.Height / 2;
			this._hasFinished = false;
			int xOffset = (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0;
			int lastVisible = -1;
			for (int i = 0; i < 6; i++)
			{
				this.categories.Add(new ClickableTextureComponent("", new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.plusButtonWidth, this.centerY - 300 + i * 27 * 4, this.plusButtonWidth, 44), "", this.getCategoryName(i), Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f, false)
				{
					visible = (i < 5 && this.categoryItems[i].Count > 0),
					myID = i,
					downNeighborID = ((i < 4) ? (i + 1) : 101),
					upNeighborID = ((i > 0) ? lastVisible : -1),
					upNeighborImmutable = true
				});
				lastVisible = ((i < 5 && this.categoryItems[i].Count > 0) ? i : lastVisible);
			}
			this.dayPlaqueY = this.categories[0].bounds.Y - 128;
			Rectangle okRect = new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.itemAndPlusButtonWidth + 32, this.centerY + 300 - 64, 64, 64);
			this.okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), okRect, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f, false)
			{
				myID = 101,
				upNeighborID = lastVisible
			};
			this.backButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 103,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 103
			};
			if (Game1.dayOfMonth == 25 && Game1.season == Season.Winter)
			{
				Vector2 startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(0, 200));
				Rectangle sourceRect = new Rectangle(640, 800, 32, 16);
				int loops = 1000;
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 80f, 2, loops, startingPosition, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
				t.motion = new Vector2(-4f, 0f);
				t.delayBeforeAnimationStart = 3000;
				this.animations.Add(t);
			}
			Game1.stats.checkForShippingAchievements();
			this.RepositionItems();
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002C2D RID: 11309 RVA: 0x0021B644 File Offset: 0x00219844
		public void RepositionItems()
		{
			this.centerX = Game1.uiViewport.Width / 2;
			this.centerY = Game1.uiViewport.Height / 2;
			int boxwidth = Game1.uiViewport.Width;
			int boxheight = Game1.uiViewport.Height;
			boxwidth = Math.Min(this.width, 1280);
			boxheight = Math.Min(this.height, 920);
			int xOffset = (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0;
			for (int i = 0; i < 6; i++)
			{
				this.categories[i].bounds = new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.plusButtonWidth, this.centerY - 300 + i * 27 * 4, this.plusButtonWidth, 44);
			}
			this.dayPlaqueY = this.categories[0].bounds.Y - 128;
			if (this.dayPlaqueY < 0)
			{
				this.dayPlaqueY = -64;
			}
			this.backButton.bounds.X = this.centerX - boxwidth / 2 - 64;
			this.backButton.bounds.Y = this.centerY + boxheight / 2 - 48;
			if (this.backButton.bounds.X < 0)
			{
				this.backButton.bounds.X = this.xPositionOnScreen + 32;
			}
			if (this.backButton.bounds.Y > Game1.uiViewport.Height - 32)
			{
				this.backButton.bounds.Y = Game1.uiViewport.Height - 80;
			}
			this.forwardButton.bounds.X = this.centerX + boxwidth / 2 + 8;
			this.forwardButton.bounds.Y = this.centerY + boxheight / 2 - 48;
			if (this.forwardButton.bounds.X > Game1.uiViewport.Width - 32)
			{
				this.forwardButton.bounds.X = this.xPositionOnScreen + this.width - 32 - 48;
			}
			if (this.forwardButton.bounds.Y > Game1.uiViewport.Height - 32)
			{
				this.forwardButton.bounds.Y = Game1.uiViewport.Height - 80;
			}
			Rectangle okRect = new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.itemAndPlusButtonWidth + 32, this.centerY + 300 - 64, 64, 64);
			this.okButton.bounds = okRect;
			int spaceHeight = Math.Min(this.height, 920);
			float item_space = (float)(this.yPositionOnScreen + spaceHeight - 64 - (this.yPositionOnScreen + 32));
			this.itemsPerCategoryPage = (int)(item_space / 68f);
			if (this.currentPage >= 0)
			{
				this.currentTab = Utility.Clamp(this.currentTab, 0, (this.categoryItems[this.currentPage].Count - 1) / this.itemsPerCategoryPage);
			}
		}

		// Token: 0x06002C2E RID: 11310 RVA: 0x0021B95C File Offset: 0x00219B5C
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID == 103 && direction == 1 && this.showForwardButton())
			{
				this.currentlySnappedComponent = base.getComponentWithID(102);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002C2F RID: 11311 RVA: 0x0021B983 File Offset: 0x00219B83
		public override void snapToDefaultClickableComponent()
		{
			if (this.currentPage != -1)
			{
				this.currentlySnappedComponent = base.getComponentWithID(103);
			}
			else
			{
				this.currentlySnappedComponent = base.getComponentWithID(101);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C30 RID: 11312 RVA: 0x0021B9B4 File Offset: 0x00219BB4
		public void parseItems(IList<Item> items)
		{
			Utility.consolidateStacks(items);
			for (int i = 0; i < 6; i++)
			{
				this.categoryItems.Add(new List<Item>());
				this.categoryTotals.Add(0);
				this.categoryDials.Add(new MoneyDial(7, i == 5));
			}
			foreach (Item item in items)
			{
				if (item != null)
				{
					int category = this.getCategoryIndexForObject(item);
					this.categoryItems[category].Add(item);
					int sellToStorePrice = item.sellToStorePrice(-1L);
					int price = sellToStorePrice * item.Stack;
					List<int> list = this.categoryTotals;
					int index = category;
					list[index] += price;
					this.itemValues[item] = price;
					this.singleItemValues[item] = sellToStorePrice;
					Game1.stats.ItemsShipped += (uint)item.Stack;
					if (item.Category == -75 || item.Category == -79)
					{
						Game1.stats.CropsShipped += (uint)item.Stack;
					}
					Object o = item as Object;
					if (o != null && o.countsForShippedCollection())
					{
						Game1.player.shippedBasic(o.ItemId, o.stack.Value);
					}
				}
			}
			for (int j = 0; j < 5; j++)
			{
				List<int> list = this.categoryTotals;
				list[5] = list[5] + this.categoryTotals[j];
				this.categoryItems[5].AddRange(this.categoryItems[j]);
				this.categoryDials[j].currentValue = this.categoryTotals[j];
				this.categoryDials[j].previousTargetValue = this.categoryDials[j].currentValue;
			}
			this.categoryDials[5].currentValue = this.categoryTotals[5];
			Game1.setRichPresence("earnings", this.categoryTotals[5]);
		}

		// Token: 0x06002C31 RID: 11313 RVA: 0x0021BBF8 File Offset: 0x00219DF8
		public int getCategoryIndexForObject(Item item)
		{
			string qualifiedItemId = item.QualifiedItemId;
			int num;
			if (qualifiedItemId != null)
			{
				num = qualifiedItemId.Length;
				if (num == 6)
				{
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(O)410"))
						{
							goto IL_B7;
						}
						break;
					case '1':
					case '3':
					case '5':
					case '7':
						goto IL_B7;
					case '2':
						if (!(qualifiedItemId == "(O)402"))
						{
							goto IL_B7;
						}
						break;
					case '4':
						if (!(qualifiedItemId == "(O)414"))
						{
							goto IL_B7;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(O)396") && !(qualifiedItemId == "(O)406") && !(qualifiedItemId == "(O)296"))
						{
							goto IL_B7;
						}
						break;
					case '8':
						if (!(qualifiedItemId == "(O)418"))
						{
							goto IL_B7;
						}
						break;
					default:
						goto IL_B7;
					}
					return 1;
				}
			}
			IL_B7:
			Object o = item as Object;
			if (o != null && (o.preserve.Value.GetValueOrDefault() == Object.PreserveType.SmokedFish || o.preserve.Value.GetValueOrDefault() == Object.PreserveType.AgedRoe || o.preserve.Value.GetValueOrDefault() == Object.PreserveType.Roe))
			{
				return 2;
			}
			num = item.Category;
			if (num <= -20)
			{
				switch (num)
				{
				case -81:
					break;
				case -80:
				case -79:
				case -75:
					return 0;
				case -78:
				case -77:
				case -76:
					return 4;
				default:
					switch (num)
					{
					case -27:
					case -23:
						break;
					case -26:
						return 0;
					case -25:
					case -24:
					case -22:
						return 4;
					case -21:
					case -20:
						return 2;
					default:
						return 4;
					}
					break;
				}
				return 1;
			}
			switch (num)
			{
			case -15:
			case -12:
				break;
			case -14:
				return 0;
			case -13:
				return 4;
			default:
				switch (num)
				{
				case -6:
				case -5:
					return 0;
				case -4:
					return 2;
				case -3:
					return 4;
				case -2:
					break;
				default:
					return 4;
				}
				break;
			}
			return 3;
		}

		// Token: 0x06002C32 RID: 11314 RVA: 0x0021BDAC File Offset: 0x00219FAC
		public string getCategoryName(int index)
		{
			switch (index)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11389");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11390");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11391");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11392");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11393");
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11394");
			default:
				return "";
			}
		}

		// Token: 0x06002C33 RID: 11315 RVA: 0x0021BE40 File Offset: 0x0021A040
		public override void update(GameTime time)
		{
			base.update(time);
			if (!this._activated)
			{
				this._activated = true;
				Game1.player.team.endOfNightStatus.UpdateState("shipment");
			}
			if (!this._hasFinished)
			{
				if (this.saveGameMenu != null)
				{
					this.saveGameMenu.update(time);
					if (this.saveGameMenu.quit)
					{
						this.saveGameMenu = null;
						this.savedYet = true;
					}
				}
				this.weatherX += (float)time.ElapsedGameTime.Milliseconds * 0.03f;
				this.animations.RemoveWhere((TemporaryAnimatedSprite animation) => animation.update(time));
				if (this.outro)
				{
					if (this.outroFadeTimer > 0)
					{
						this.outroFadeTimer -= time.ElapsedGameTime.Milliseconds;
					}
					else if (this.outroFadeTimer <= 0 && this.dayPlaqueY < this.centerY - 64)
					{
						if (this.animations.Count > 0)
						{
							this.animations.Clear();
						}
						this.dayPlaqueY += (int)Math.Ceiling((double)((float)time.ElapsedGameTime.Milliseconds * 0.35f));
						if (this.dayPlaqueY >= this.centerY - 64)
						{
							this.outroPauseBeforeDateChange = 700;
						}
					}
					else if (this.outroPauseBeforeDateChange > 0)
					{
						this.outroPauseBeforeDateChange -= time.ElapsedGameTime.Milliseconds;
						if (this.outroPauseBeforeDateChange <= 0)
						{
							this.newDayPlaque = true;
							Game1.playSound("newRecipe", null);
							if (Game1.season != Season.Winter && Game1.game1.IsMainInstance)
							{
								DelayedAction.playSoundAfterDelay(Game1.IsRainingHere(null) ? "rainsound" : "rooster", 1500, null, null, -1, false);
							}
							this.finalOutroTimer = 2000;
							this.animations.Clear();
							if (!this.savedYet)
							{
								if (this.saveGameMenu == null)
								{
									this.saveGameMenu = new SaveGameMenu();
								}
								return;
							}
						}
					}
					else if (this.finalOutroTimer > 0 && this.savedYet)
					{
						this.finalOutroTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.finalOutroTimer <= 0)
						{
							this._hasFinished = true;
						}
					}
				}
				if (this.introTimer >= 0)
				{
					int num = this.introTimer;
					this.introTimer -= time.ElapsedGameTime.Milliseconds * ((Game1.oldMouseState.LeftButton == ButtonState.Pressed) ? 3 : 1);
					if (num % 500 < this.introTimer % 500 && this.introTimer <= 3000)
					{
						int categoryThatPoppedUp = 4 - this.introTimer / 500;
						if (categoryThatPoppedUp < 6 && categoryThatPoppedUp > -1)
						{
							if (this.categoryItems[categoryThatPoppedUp].Count > 0)
							{
								Game1.playSound(this.getCategorySound(categoryThatPoppedUp), null);
								this.categoryDials[categoryThatPoppedUp].currentValue = 0;
								this.categoryDials[categoryThatPoppedUp].previousTargetValue = 0;
							}
							else
							{
								Game1.playSound("stoneStep", null);
							}
						}
					}
					if (this.introTimer < 0)
					{
						if (Game1.options.SnappyMenus)
						{
							this.snapToDefaultClickableComponent();
						}
						Game1.playSound("money", null);
						this.categoryDials[5].currentValue = 0;
						this.categoryDials[5].previousTargetValue = 0;
					}
				}
				else if (Game1.dayOfMonth != 28 && !this.outro)
				{
					if (!Game1.wasRainingYesterday)
					{
						Vector2 startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(200));
						Rectangle sourceRect = new Rectangle(640, 752, 16, 16);
						int rows = Game1.random.Next(1, 4);
						if (Game1.random.NextDouble() < 0.001)
						{
							bool flip = Game1.random.NextBool();
							if (Game1.random.NextBool())
							{
								this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(640, 826, 16, 8), 40f, 4, 0, new Vector2((float)Game1.random.Next(this.centerX * 2), (float)Game1.random.Next(this.centerY)), false, flip)
								{
									rotation = 3.1415927f,
									scale = 4f,
									motion = new Vector2((float)(flip ? -8 : 8), 8f),
									local = true
								});
							}
							else
							{
								this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(258, 1680, 16, 16), 40f, 4, 0, new Vector2((float)Game1.random.Next(this.centerX * 2), (float)Game1.random.Next(this.centerY)), false, flip)
								{
									scale = 4f,
									motion = new Vector2((float)(flip ? -8 : 8), 8f),
									local = true
								});
							}
						}
						else if (Game1.random.NextDouble() < 0.0002)
						{
							startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(4, 256));
							TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("", new Rectangle(0, 0, 1, 1), 9999f, 1, 10000, startingPosition, false, false, 0.01f, 0f, Color.White * (0.25f + (float)Game1.random.NextDouble()), 4f, 0f, 0f, 0f, true);
							bird.motion = new Vector2(-0.25f, 0f);
							this.animations.Add(bird);
						}
						else if (Game1.random.NextDouble() < 5E-05)
						{
							startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)(Game1.uiViewport.Height - 192));
							for (int i = 0; i < rows; i++)
							{
								TemporaryAnimatedSprite bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, (float)Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((float)((i + 1) * Game1.random.Next(15, 18)), (float)((i + 1) * -20)), false, false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, true);
								bird2.motion = new Vector2(-1f, 0f);
								this.animations.Add(bird2);
								bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, (float)Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((float)((i + 1) * Game1.random.Next(15, 18)), (float)((i + 1) * 20)), false, false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, true);
								bird2.motion = new Vector2(-1f, 0f);
								this.animations.Add(bird2);
							}
						}
						else if (Game1.random.NextDouble() < 1E-05)
						{
							sourceRect = new Rectangle(640, 784, 16, 16);
							TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 75f, 4, 1000, startingPosition, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
							t.motion = new Vector2(-3f, 0f);
							t.yPeriodic = true;
							t.yPeriodicLoopTime = 1000f;
							t.yPeriodicRange = 8f;
							t.shakeIntensity = 0.5f;
							this.animations.Add(t);
						}
					}
					this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.smokeTimer <= 0)
					{
						this.smokeTimer = 50;
						this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(684, 1075, 1, 1), 1000f, 1, 1000, new Vector2(188f, (float)(Game1.uiViewport.Height - 128 + 20)), false, false)
						{
							color = (Game1.wasRainingYesterday ? Color.SlateGray : Color.White),
							scale = 4f,
							scaleChange = 0f,
							alphaFade = 0.0025f,
							motion = new Vector2(0f, (float)(-(float)Game1.random.Next(25, 75)) / 100f / 4f),
							acceleration = new Vector2(-0.001f, 0f)
						});
					}
				}
				if (this.moonShake > 0)
				{
					this.moonShake -= time.ElapsedGameTime.Milliseconds;
				}
				return;
			}
			if (Game1.PollForEndOfNewDaySync())
			{
				base.exitThisMenu(false);
				return;
			}
		}

		// Token: 0x06002C34 RID: 11316 RVA: 0x0021C800 File Offset: 0x0021AA00
		public string getCategorySound(int which)
		{
			switch (which)
			{
			case 0:
			{
				Object @object = this.categoryItems[0][0] as Object;
				if (!(((@object != null) ? new bool?(@object.isAnimalProduct()) : null) ?? false))
				{
					return "harvest";
				}
				return "cluck";
			}
			case 1:
				return "leafrustle";
			case 2:
				return "button1";
			case 3:
				return "hammer";
			case 4:
				return "coin";
			case 5:
				return "money";
			default:
				return "stoneStep";
			}
		}

		// Token: 0x06002C35 RID: 11317 RVA: 0x0021C89F File Offset: 0x0021AA9F
		public override void applyMovementKey(int direction)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			base.applyMovementKey(direction);
		}

		// Token: 0x06002C36 RID: 11318 RVA: 0x0021C8B4 File Offset: 0x0021AAB4
		public override void performHoverAction(int x, int y)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			base.performHoverAction(x, y);
			if (this.currentPage == -1)
			{
				this.okButton.tryHover(x, y, 0.1f);
				using (List<ClickableTextureComponent>.Enumerator enumerator = this.categories.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ClickableTextureComponent c = enumerator.Current;
						if (c.containsPoint(x, y))
						{
							c.sourceRect.X = 402;
						}
						else
						{
							c.sourceRect.X = 392;
						}
					}
					return;
				}
			}
			this.backButton.tryHover(x, y, 0.5f);
			this.forwardButton.tryHover(x, y, 0.5f);
		}

		// Token: 0x06002C37 RID: 11319 RVA: 0x0021C97C File Offset: 0x0021AB7C
		public bool CanReceiveInput()
		{
			return this.introTimer <= 0 && this.saveGameMenu == null && !this.outro;
		}

		// Token: 0x06002C38 RID: 11320 RVA: 0x0021C9A0 File Offset: 0x0021ABA0
		public override void receiveKeyPress(Keys key)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			if (this.introTimer > 0 || Game1.options.gamepadControls || (!key.Equals(Keys.Escape) && !Game1.options.doesInputListContain(Game1.options.menuButton, key)))
			{
				if (this.introTimer <= 0 && (!Game1.options.gamepadControls || !Game1.options.doesInputListContain(Game1.options.menuButton, key)))
				{
					base.receiveKeyPress(key);
				}
				return;
			}
			if (this.currentPage == -1)
			{
				this.receiveLeftClick(this.okButton.bounds.Center.X, this.okButton.bounds.Center.Y, true);
				return;
			}
			this.receiveLeftClick(this.backButton.bounds.Center.X, this.backButton.bounds.Center.Y, true);
		}

		// Token: 0x06002C39 RID: 11321 RVA: 0x0021CAA0 File Offset: 0x0021ACA0
		public override void receiveGamePadButton(Buttons button)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			base.receiveGamePadButton(button);
			if (button == Buttons.Start || button == Buttons.B)
			{
				if (button == Buttons.B && this.currentPage != -1)
				{
					if (this.currentTab == 0)
					{
						if (Game1.options.SnappyMenus)
						{
							this.currentlySnappedComponent = base.getComponentWithID(this.currentPage);
							this.snapCursorToCurrentSnappedComponent();
						}
						this.currentPage = -1;
					}
					else
					{
						this.currentTab--;
					}
					Game1.playSound("shwip", null);
					return;
				}
				if (this.currentPage == -1 && !this.outro)
				{
					if (this.introTimer <= 0)
					{
						this.okClicked();
						return;
					}
					this.introTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds * 2;
				}
			}
		}

		// Token: 0x06002C3A RID: 11322 RVA: 0x0021CB78 File Offset: 0x0021AD78
		private void okClicked()
		{
			this.outro = true;
			this.outroFadeTimer = 800;
			Game1.playSound("bigDeSelect", null);
			Game1.changeMusicTrack("none", false, MusicContext.Default);
		}

		// Token: 0x06002C3B RID: 11323 RVA: 0x0021CBB8 File Offset: 0x0021ADB8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			if (this.outro && !this.savedYet)
			{
				return;
			}
			if (this.savedYet)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (this.currentPage == -1 && this.introTimer <= 0 && this.okButton.containsPoint(x, y))
			{
				this.okClicked();
			}
			if (this.currentPage == -1)
			{
				int i = 0;
				while (i < this.categories.Count)
				{
					if (this.categories[i].visible && this.categories[i].containsPoint(x, y))
					{
						this.currentPage = i;
						Game1.playSound("shwip", null);
						if (Game1.options.SnappyMenus)
						{
							this.currentlySnappedComponent = base.getComponentWithID(103);
							this.snapCursorToCurrentSnappedComponent();
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
				if (Game1.dayOfMonth == 28 && this.timesPokedMoon <= 10 && new Rectangle(Game1.uiViewport.Width - 176, 4, 172, 172).Contains(x, y))
				{
					this.moonShake = 100;
					this.timesPokedMoon++;
					if (this.timesPokedMoon > 10)
					{
						Game1.playSound("shadowDie", null);
						return;
					}
					Game1.playSound("thudStep", null);
					return;
				}
			}
			else
			{
				if (this.backButton.containsPoint(x, y))
				{
					if (this.currentTab == 0)
					{
						if (Game1.options.SnappyMenus)
						{
							this.currentlySnappedComponent = base.getComponentWithID(this.currentPage);
							this.snapCursorToCurrentSnappedComponent();
						}
						this.currentPage = -1;
					}
					else
					{
						this.currentTab--;
					}
					Game1.playSound("shwip", null);
					return;
				}
				if (this.showForwardButton() && this.forwardButton.containsPoint(x, y))
				{
					this.currentTab++;
					Game1.playSound("shwip", null);
				}
			}
		}

		// Token: 0x06002C3C RID: 11324 RVA: 0x0021CDD1 File Offset: 0x0021AFD1
		public bool showForwardButton()
		{
			return this.categoryItems[this.currentPage].Count > this.itemsPerCategoryPage * (this.currentTab + 1);
		}

		// Token: 0x06002C3D RID: 11325 RVA: 0x0021CDFA File Offset: 0x0021AFFA
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.initialize(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, false);
			this.RepositionItems();
		}

		// Token: 0x06002C3E RID: 11326 RVA: 0x0021CE20 File Offset: 0x0021B020
		public override void draw(SpriteBatch b)
		{
			bool isWinter = Game1.season == Season.Winter;
			if (Game1.wasRainingYesterday)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(this.wasGreenRain ? 640 : 639, 858, 1, 184)), (isWinter ? Color.LightSlateGray : (this.wasGreenRain ? Color.LightGreen : Color.SlateGray)) * (1f - (float)this.introTimer / 3500f));
				if (this.wasGreenRain)
				{
					b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(this.wasGreenRain ? 640 : 639, 858, 1, 184)), Color.DimGray * 0.8f * (1f - (float)this.introTimer / 3500f));
				}
				for (int x = -244; x < Game1.uiViewport.Width + 244; x += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x + this.weatherX / 2f % 244f, 32f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.DarkSlateGray * 1f * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				for (int x2 = 0; x2 < this.width; x2 += 639)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(x2 * 4), (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 48)), (isWinter ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
					b.Draw(Game1.mouseCursors, new Vector2((float)(x2 * 4), (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 32)), (isWinter ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				b.Draw(Game1.mouseCursors, new Vector2(160f, (float)(Game1.uiViewport.Height - 128 + 16 + 8)), new Rectangle?(new Rectangle(653, 880, 10, 10)), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				for (int x3 = -244; x3 < Game1.uiViewport.Width + 244; x3 += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x3 + this.weatherX % 244f, -32f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.SlateGray * 0.85f * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.animations)
				{
					temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				}
				for (int x4 = -244; x4 < Game1.uiViewport.Width + 244; x4 += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x4 + this.weatherX * 1.5f % 244f, -128f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.LightSlateGray * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.White * (1f - (float)this.introTimer / 3500f));
				for (int x5 = 0; x5 < this.width; x5 += 639)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(x5 * 4), 0f), new Rectangle?(new Rectangle(0, 1453, 639, 195)), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				if (Game1.dayOfMonth == 28)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.uiViewport.Width - 176), 4f) + ((this.moonShake > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(642, 835, 43, 43)), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if (this.timesPokedMoon > 10)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.uiViewport.Width - 136), 48f) + ((this.moonShake > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(685, 844 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4000.0 < 200.0 || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 > 7600.0 && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 < 7800.0)) ? 21 : 0), 19, 21)), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 48)), (isWinter ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 48)), (isWinter ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 32)), (isWinter ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, isWinter ? 1034 : 737, 639, 32)), (isWinter ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(160f, (float)(Game1.uiViewport.Height - 128 + 16 + 8)), new Rectangle?(new Rectangle(653, 880, 10, 10)), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (!this.outro && !Game1.wasRainingYesterday)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in this.animations)
				{
					temporaryAnimatedSprite2.draw(b, true, 0, 0, 1f);
				}
			}
			if (this.wasGreenRain)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Green * 0.1f);
			}
			if (this.currentPage == -1)
			{
				int scroll_draw_y = this.categories[0].bounds.Y - 128;
				if (scroll_draw_y >= 0)
				{
					SpriteText.drawStringWithScrollCenteredAt(b, Utility.getYesterdaysDate(), Game1.uiViewport.Width / 2, scroll_draw_y, "", 1f, null, 0, 0.88f, false);
				}
				int extraWidth = (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0;
				int yOffset = -20;
				int i = 0;
				foreach (ClickableTextureComponent c in this.categories)
				{
					if (this.introTimer < 2500 - i * 500)
					{
						Vector2 start = c.getVector2() + new Vector2((float)(12 - extraWidth), -8f);
						if (c.visible)
						{
							c.draw(b);
							b.Draw(Game1.mouseCursors, start + new Vector2((float)(-104 + extraWidth), (float)(yOffset + 4)), new Rectangle?(new Rectangle(293, 360, 24, 24)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
							this.categoryItems[i][0].drawInMenu(b, start + new Vector2((float)(-88 + extraWidth), (float)(yOffset + 16)), 1f, 1f, 0.9f, StackDrawType.Hide);
						}
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), (int)(start.X + (float)(-(float)this.itemSlotWidth) - (float)this.categoryLabelsWidth - 12f), (int)(start.Y + (float)yOffset), this.categoryLabelsWidth + extraWidth, 104, Color.White, 4f, false, -1f);
						SpriteText.drawString(b, c.hoverText, (int)start.X - this.itemSlotWidth - this.categoryLabelsWidth + 8, (int)start.Y + 4, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						for (int j = 0; j < 6; j++)
						{
							b.Draw(Game1.mouseCursors, start + new Vector2((float)(-(float)this.itemSlotWidth + extraWidth - 192 - 24 + j * 6 * 4), 12f), new Rectangle?(new Rectangle(355, 476, 7, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
						}
						this.categoryDials[i].draw(b, start + new Vector2((float)(-(float)this.itemSlotWidth + extraWidth - 192 - 48 + 4), 20f), this.categoryTotals[i]);
						b.Draw(Game1.mouseCursors, start + new Vector2((float)(-(float)this.itemSlotWidth + extraWidth - 64 - 4), 12f), new Rectangle?(new Rectangle(408, 476, 9, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
					i++;
				}
				if (this.introTimer <= 0)
				{
					this.okButton.draw(b);
				}
			}
			else
			{
				int boxwidth = Game1.uiViewport.Width;
				int boxheight = Game1.uiViewport.Height;
				boxwidth = Math.Min(this.width, 1280);
				boxheight = Math.Min(this.height, 920);
				int xPos = Game1.uiViewport.Width / 2 - boxwidth / 2;
				int yPos = Game1.uiViewport.Height / 2 - boxheight / 2;
				IClickableMenu.drawTextureBox(b, xPos, yPos, boxwidth, boxheight, Color.White);
				Vector2 position = new Vector2((float)(xPos + 32), (float)(yPos + 32));
				for (int k = this.currentTab * this.itemsPerCategoryPage; k < this.currentTab * this.itemsPerCategoryPage + this.itemsPerCategoryPage; k++)
				{
					if (this.categoryItems[this.currentPage].Count > k)
					{
						Item item = this.categoryItems[this.currentPage][k];
						item.drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw);
						string subtotalStr = item.DisplayName + " x" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.singleItemValues[item]);
						string totalStr = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(this.itemValues[item]));
						string dotsAndName = subtotalStr;
						int totalPosX = (int)position.X + boxwidth - 64 - SpriteText.getWidthOfString(totalStr, 999999);
						while (SpriteText.getWidthOfString(dotsAndName + totalStr, 999999) < boxwidth - 192)
						{
							dotsAndName += " .";
						}
						if (SpriteText.getWidthOfString(dotsAndName + totalStr, 999999) >= boxwidth)
						{
							dotsAndName = dotsAndName.Remove(dotsAndName.Length - 1);
						}
						SpriteText.drawString(b, dotsAndName, (int)position.X + 64 + 12, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						SpriteText.drawString(b, totalStr, totalPosX, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						position.Y += 68f;
					}
				}
				this.backButton.draw(b);
				if (this.showForwardButton())
				{
					this.forwardButton.draw(b);
				}
			}
			if (this.outro)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.Black * (1f - (float)this.outroFadeTimer / 800f));
				SpriteText.drawStringWithScrollCenteredAt(b, this.newDayPlaque ? Utility.getDateString(0) : Utility.getYesterdaysDate(), Game1.uiViewport.Width / 2, this.dayPlaqueY, "", 1f, null, 0, 0.88f, false);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite3 in this.animations)
				{
					temporaryAnimatedSprite3.draw(b, true, 0, 0, 1f);
				}
				if (this.finalOutroTimer > 0 || this._hasFinished)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Black * (1f - (float)this.finalOutroTimer / 2000f));
				}
			}
			SaveGameMenu saveGameMenu = this.saveGameMenu;
			if (saveGameMenu != null)
			{
				saveGameMenu.draw(b);
			}
			if (!Game1.options.SnappyMenus || (this.introTimer <= 0 && !this.outro))
			{
				Game1.mouseCursorTransparency = 1f;
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x04001DD8 RID: 7640
		public const int region_okbutton = 101;

		// Token: 0x04001DD9 RID: 7641
		public const int region_forwardButton = 102;

		// Token: 0x04001DDA RID: 7642
		public const int region_backButton = 103;

		// Token: 0x04001DDB RID: 7643
		public const int farming_category = 0;

		// Token: 0x04001DDC RID: 7644
		public const int foraging_category = 1;

		// Token: 0x04001DDD RID: 7645
		public const int fishing_category = 2;

		// Token: 0x04001DDE RID: 7646
		public const int mining_category = 3;

		// Token: 0x04001DDF RID: 7647
		public const int other_category = 4;

		// Token: 0x04001DE0 RID: 7648
		public const int total_category = 5;

		// Token: 0x04001DE1 RID: 7649
		public const int timePerIntroCategory = 500;

		// Token: 0x04001DE2 RID: 7650
		public const int outroFadeTime = 800;

		// Token: 0x04001DE3 RID: 7651
		public const int smokeRate = 100;

		// Token: 0x04001DE4 RID: 7652
		public const int categorylabelHeight = 25;

		// Token: 0x04001DE5 RID: 7653
		public int itemsPerCategoryPage = 9;

		// Token: 0x04001DE6 RID: 7654
		public int currentPage = -1;

		// Token: 0x04001DE7 RID: 7655
		public int currentTab;

		// Token: 0x04001DE8 RID: 7656
		public List<ClickableTextureComponent> categories = new List<ClickableTextureComponent>();

		// Token: 0x04001DE9 RID: 7657
		public ClickableTextureComponent okButton;

		// Token: 0x04001DEA RID: 7658
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001DEB RID: 7659
		public ClickableTextureComponent backButton;

		// Token: 0x04001DEC RID: 7660
		private List<int> categoryTotals = new List<int>();

		// Token: 0x04001DED RID: 7661
		private List<MoneyDial> categoryDials = new List<MoneyDial>();

		// Token: 0x04001DEE RID: 7662
		private Dictionary<Item, int> itemValues = new Dictionary<Item, int>();

		// Token: 0x04001DEF RID: 7663
		private Dictionary<Item, int> singleItemValues = new Dictionary<Item, int>();

		// Token: 0x04001DF0 RID: 7664
		private List<List<Item>> categoryItems = new List<List<Item>>();

		// Token: 0x04001DF1 RID: 7665
		private int categoryLabelsWidth;

		// Token: 0x04001DF2 RID: 7666
		private int plusButtonWidth;

		// Token: 0x04001DF3 RID: 7667
		private int itemSlotWidth;

		// Token: 0x04001DF4 RID: 7668
		private int itemAndPlusButtonWidth;

		// Token: 0x04001DF5 RID: 7669
		private int totalWidth;

		// Token: 0x04001DF6 RID: 7670
		private int centerX;

		// Token: 0x04001DF7 RID: 7671
		private int centerY;

		// Token: 0x04001DF8 RID: 7672
		private int introTimer = 3500;

		// Token: 0x04001DF9 RID: 7673
		private int outroFadeTimer;

		// Token: 0x04001DFA RID: 7674
		private int outroPauseBeforeDateChange;

		// Token: 0x04001DFB RID: 7675
		private int finalOutroTimer;

		// Token: 0x04001DFC RID: 7676
		private int smokeTimer;

		// Token: 0x04001DFD RID: 7677
		private int dayPlaqueY;

		// Token: 0x04001DFE RID: 7678
		private int moonShake = -1;

		// Token: 0x04001DFF RID: 7679
		private int timesPokedMoon;

		// Token: 0x04001E00 RID: 7680
		private float weatherX;

		// Token: 0x04001E01 RID: 7681
		private bool outro;

		// Token: 0x04001E02 RID: 7682
		private bool newDayPlaque;

		// Token: 0x04001E03 RID: 7683
		private bool savedYet;

		// Token: 0x04001E04 RID: 7684
		public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

		// Token: 0x04001E05 RID: 7685
		private SaveGameMenu saveGameMenu;

		// Token: 0x04001E06 RID: 7686
		protected bool _hasFinished;

		// Token: 0x04001E07 RID: 7687
		public bool _activated;

		// Token: 0x04001E08 RID: 7688
		private bool wasGreenRain;
	}
}
