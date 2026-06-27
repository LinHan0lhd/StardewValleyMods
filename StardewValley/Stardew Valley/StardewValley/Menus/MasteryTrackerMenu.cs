using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.GameData;

namespace StardewValley.Menus
{
	// Token: 0x02000288 RID: 648
	public class MasteryTrackerMenu : IClickableMenu
	{
		// Token: 0x06002AEE RID: 10990 RVA: 0x00206098 File Offset: 0x00204298
		public MasteryTrackerMenu(int whichSkill = -1) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(800, 320, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(800, 320, 0, 0).Y, 800, 320, true)
		{
			this.which = whichSkill;
			this.closeSound = "stone_button";
			Texture2D objects2Tex = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
			switch (whichSkill)
			{
			case 0:
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.content.Load<Texture2D>("TileSheets\\weapons"), new Rectangle(32, 128, 16, 16), 4f, true)
				{
					name = Game1.content.LoadString("Strings\\Weapons:IridiumScythe_Name"),
					label = Game1.content.LoadString("Strings\\Weapons:IridiumScythe_Description"),
					hoverText = "(W)66"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(32, 1152, 16, 32), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfBlessings").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfBlessings").Description,
					myAlternateID = 1,
					hoverText = "Statue Of Blessings"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, true)
				{
					name = "",
					label = Game1.content.LoadString("Strings\\1_6_Strings:Farming_Mastery"),
					myAlternateID = 0
				});
				Game1.playSound("weed_cut", null);
				break;
			case 1:
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.toolSpriteSheet, new Rectangle(272, 0, 16, 16), 4f, true)
				{
					name = Game1.content.LoadString("Strings\\Tools:FishingRod_AdvancedIridium_Name"),
					label = Game1.content.LoadString("Strings\\Tools:FishingRod_AdvancedIridium_Description"),
					hoverText = "(T)AdvancedIridiumRod"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(0, 144, 16, 16), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(O)ChallengeBait").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(O)ChallengeBait").Description,
					myAlternateID = 1,
					hoverText = "Challenge Bait"
				});
				Game1.playSound("waterSlosh", null);
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, true)
				{
					name = "",
					label = Game1.content.LoadString("Strings\\1_6_Strings:Fishing_Mastery"),
					myAlternateID = 0
				});
				break;
			case 2:
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(80, 112, 16, 16), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(O)MysticTreeSeed").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(O)MysticTreeSeed").Description,
					myAlternateID = 1,
					hoverText = "Mystic Tree Seed"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(112, 128, 16, 16), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(O)TreasureTotem").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(O)TreasureTotem").Description,
					myAlternateID = 1,
					hoverText = "Treasure Totem"
				});
				Game1.playSound("axchop", null);
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, true)
				{
					name = "",
					label = Game1.content.LoadString("Strings\\1_6_Strings:Foraging_Mastery"),
					myAlternateID = 0
				});
				break;
			case 3:
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(64, 1152, 16, 32), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfTheDwarfKing").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("StatueOfTheDwarfKing").Description,
					myAlternateID = 1,
					hoverText = "Statue Of The Dwarf King"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(0, 1152, 16, 32), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(BC)HeavyFurnace").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(BC)HeavyFurnace").Description,
					myAlternateID = 1,
					hoverText = "Heavy Furnace"
				});
				Game1.playSound("stoneCrack", null);
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, true)
				{
					name = "",
					label = Game1.content.LoadString("Strings\\1_6_Strings:Mining_Mastery"),
					myAlternateID = 0
				});
				break;
			case 4:
				Game1.playSound("cavedrip", null);
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(80, 1152, 16, 32), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(BC)Anvil").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(BC)Anvil").Description,
					myAlternateID = 1,
					hoverText = "Anvil"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(96, 1152, 16, 32), 4f, true)
				{
					name = ItemRegistry.GetDataOrErrorItem("(BC)MiniForge").DisplayName,
					label = ItemRegistry.GetDataOrErrorItem("(BC)MiniForge").Description,
					myAlternateID = 1,
					hoverText = "Mini-Forge"
				});
				this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, true)
				{
					name = "",
					label = Game1.content.LoadString("Strings\\1_6_Strings:Trinkets_Description"),
					myAlternateID = 0
				});
				break;
			}
			float yHeight = 80f;
			for (int i = 0; i < this.rewards.Count; i++)
			{
				this.rewards[i].bounds = new Rectangle(this.xPositionOnScreen + 40, this.yPositionOnScreen + 64 + (int)yHeight, 64, 64);
				this.rewards[i].label = Game1.parseText(this.rewards[i].label, Game1.smallFont, this.width - 200);
				yHeight += Game1.smallFont.MeasureString(this.rewards[i].label).Y;
				if (i < this.rewards.Count - 1)
				{
					yHeight += (float)((this.rewards[i].sourceRect.Height > 16) ? 132 : 80);
				}
			}
			this.height += (int)yHeight;
			this.height -= 48;
			if (whichSkill != -1)
			{
				this.height -= 64;
			}
			int yPositionOnScreen = this.yPositionOnScreen;
			this.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, this.height, 0, 0).Y;
			int offset = yPositionOnScreen - this.yPositionOnScreen;
			foreach (ClickableTextureComponent clickableTextureComponent in this.rewards)
			{
				clickableTextureComponent.bounds.Y = clickableTextureComponent.bounds.Y - offset;
			}
			ClickableTextureComponent upperRightCloseButton = this.upperRightCloseButton;
			upperRightCloseButton.bounds.Y = upperRightCloseButton.bounds.Y - offset;
			int levelsNotSpent = MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent");
			this.canClaim = (levelsNotSpent > 0);
			if (Game1.player.stats.Get(StatKeys.Mastery(whichSkill)) <= 0U)
			{
				this.mainButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 84, this.yPositionOnScreen + this.height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f, false)
				{
					visible = (whichSkill != -1),
					myID = 0
				};
			}
			if (whichSkill == -1)
			{
				Game1.playSound("boulderCrack", null);
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				if (this.mainButton == null)
				{
					this.currentlySnappedComponent = base.getComponentWithID(this.upperRightCloseButton.myID);
				}
				else
				{
					this.currentlySnappedComponent = base.getComponentWithID(0);
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002AEF RID: 10991 RVA: 0x00206A1C File Offset: 0x00204C1C
		public override void performHoverAction(int x, int y)
		{
			if (this.destroyTimer > 0f)
			{
				return;
			}
			if (this.mainButton != null && this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && this.canClaim)
			{
				if (this.mainButton.sourceRect.X == 0)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.mainButton.sourceRect.X = 42;
			}
			else if (this.mainButton != null)
			{
				this.mainButton.sourceRect.X = 0;
			}
			base.performHoverAction(x, y);
		}

		// Token: 0x06002AF0 RID: 10992 RVA: 0x00206AC0 File Offset: 0x00204CC0
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.destroyTimer > 0f)
			{
				return;
			}
			if (this.mainButton != null && this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && this.canClaim)
			{
				Game1.playSound("cowboy_monsterhit", null);
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200, null, null, -1, false);
				this.pressedButtonTimer = 200f;
				this.claimReward();
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002AF1 RID: 10993 RVA: 0x00206B50 File Offset: 0x00204D50
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, playSound);
			base.exitThisMenu(true);
		}

		// Token: 0x06002AF2 RID: 10994 RVA: 0x00206B64 File Offset: 0x00204D64
		private void claimReward()
		{
			List<Item> toDrop = new List<Item>();
			foreach (ClickableTextureComponent c in this.rewards)
			{
				if (c.myAlternateID == 1)
				{
					Game1.player.craftingRecipes.TryAdd(c.hoverText, 0);
				}
				else
				{
					string hoverText = c.hoverText;
					if (hoverText != null && hoverText.Length > 0)
					{
						Item i = ItemRegistry.Create(c.hoverText, 1, 0, false);
						if (!Game1.player.addItemToInventoryBool(i, false))
						{
							toDrop.Add(i);
						}
					}
				}
			}
			foreach (Item item in toDrop)
			{
				Game1.createItemDebris(item, Game1.player.getStandingPosition(), 2, null, -1, false);
			}
			Game1.player.stats.Increment(StatKeys.Mastery(this.which), 1);
			if (this.which == 4)
			{
				Game1.player.stats.Set("trinketSlots", 1);
			}
			Game1.stats.Increment("masteryLevelsSpent", 1U);
			Game1.currentLocation.removeTemporarySpritesWithID(8765 + this.which);
			MasteryTrackerMenu.addSkillFlairPlaque(this.which);
			Game1.stats.Get("MasteryExp");
			if (MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent") <= 0)
			{
				Game1.currentLocation.removeTemporarySpritesWithID(8765);
				Game1.currentLocation.removeTemporarySpritesWithID(8766);
				Game1.currentLocation.removeTemporarySpritesWithID(8767);
				Game1.currentLocation.removeTemporarySpritesWithID(8768);
				Game1.currentLocation.removeTemporarySpritesWithID(8769);
			}
			if (MasteryTrackerMenu.hasCompletedAllMasteryPlaques())
			{
				DelayedAction.functionAfterDelay(delegate
				{
					MasteryTrackerMenu.addSpiritCandles(false);
				}, 500);
				Game1.player.freezePause = 2000;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.changeMusicTrack("grandpas_theme", false, MusicContext.Default);
				}, 2000);
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCompleteToast"));
					Game1.playSound("newArtifact", null);
				}, 4000);
			}
		}

		// Token: 0x06002AF3 RID: 10995 RVA: 0x00206DD4 File Offset: 0x00204FD4
		public static void addSpiritCandles(bool instant = false)
		{
			MasteryTrackerMenu.addCandle(58, 67, instant ? 0 : 500);
			MasteryTrackerMenu.addCandle(88, 51, instant ? 0 : 700);
			MasteryTrackerMenu.addCandle(120, 51, instant ? 0 : 900);
			MasteryTrackerMenu.addCandle(152, 51, instant ? 0 : 1100);
			MasteryTrackerMenu.addCandle(183, 67, instant ? 0 : 1300);
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(483, 0, 29, 27), new Vector2(61f, 82f) * 4f, false, 0f, Color.White)
			{
				interval = 99999f,
				totalNumberOfLoops = 99999,
				animationLength = 1,
				lightId = "MasteryTrackerMenu_GrandpaHat",
				id = 6666,
				lightRadius = 1f,
				scale = 4f,
				layerDepth = 0.0449f,
				delayBeforeAnimationStart = (instant ? 0 : 250)
			});
			Game1.currentLocation.removeTile(10, 9, "Buildings");
			if (!instant)
			{
				Utility.addSprinklesToLocation(Game1.currentLocation, 10, 9, 1, 1, 300, 100, Color.White, null, false);
				Utility.addSprinklesToLocation(Game1.currentLocation, 4, 6, 1, 2, 300, 50, Color.White, null, false);
			}
		}

		// Token: 0x06002AF4 RID: 10996 RVA: 0x00206F4C File Offset: 0x0020514C
		private static void addCandle(int x, int y, int delay)
		{
			TemporaryAnimatedSpriteList temporarySprites = Game1.currentLocation.temporarySprites;
			TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(536, 1945, 8, 8), new Vector2((float)x, (float)y) * 4f + new Vector2(-3f, -6f) * 4f, false, 0f, Color.White);
			temporaryAnimatedSprite.interval = 50f + (float)Game1.random.Next(15);
			temporaryAnimatedSprite.totalNumberOfLoops = 99999;
			temporaryAnimatedSprite.animationLength = 7;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
			defaultInterpolatedStringHandler.AppendFormatted("MasteryTrackerMenu");
			defaultInterpolatedStringHandler.AppendLiteral("_SpiritCandle_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(x);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(y);
			temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
			temporaryAnimatedSprite.id = 6666;
			temporaryAnimatedSprite.lightRadius = 1f;
			temporaryAnimatedSprite.scale = 3f;
			temporaryAnimatedSprite.layerDepth = 0.038500004f;
			temporaryAnimatedSprite.delayBeforeAnimationStart = delay;
			temporaryAnimatedSprite.startSound = ((delay > 0) ? "fireball" : null);
			temporaryAnimatedSprite.drawAboveAlwaysFront = true;
			temporarySprites.Add(temporaryAnimatedSprite);
		}

		// Token: 0x06002AF5 RID: 10997 RVA: 0x00207084 File Offset: 0x00205284
		public static void addSkillFlairPlaque(int which)
		{
			switch (which)
			{
			case 0:
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(21, 59, 15, 21), new Vector2(113f, 61f) * 4f, false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 9999f,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				return;
			case 1:
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(37, 59, 16, 21), new Vector2(143f, 63f) * 4f, false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 9999f,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				return;
			case 2:
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(10, 59, 10, 21), new Vector2(82f, 61f) * 4f, false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 9999f,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				return;
			case 3:
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(54, 59, 16, 21), new Vector2(175f, 75f) * 4f, false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 9999f,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				return;
			case 4:
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(0, 59, 9, 21), new Vector2(53f, 75f) * 4f, false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 9999f,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				return;
			default:
				return;
			}
		}

		// Token: 0x06002AF6 RID: 10998 RVA: 0x002072EC File Offset: 0x002054EC
		public static bool hasCompletedAllMasteryPlaques()
		{
			return Game1.player.stats.Get(StatKeys.Mastery(0)) > 0U && Game1.player.stats.Get(StatKeys.Mastery(1)) > 0U && Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U && Game1.player.stats.Get(StatKeys.Mastery(3)) > 0U && Game1.player.stats.Get(StatKeys.Mastery(4)) > 0U;
		}

		// Token: 0x06002AF7 RID: 10999 RVA: 0x00207374 File Offset: 0x00205574
		public override void update(GameTime time)
		{
			if (this.destroyTimer > 0f)
			{
				this.destroyTimer -= (float)((int)time.ElapsedGameTime.TotalMilliseconds);
				if (this.destroyTimer <= 0f)
				{
					Game1.activeClickableMenu = null;
					Game1.playSound("discoverMineral", null);
				}
			}
			if (this.pressedButtonTimer > 0f)
			{
				this.pressedButtonTimer -= (float)((int)time.ElapsedGameTime.TotalMilliseconds);
				this.mainButton.sourceRect.X = 84;
				if (this.pressedButtonTimer <= 0f)
				{
					this.destroyTimer = 100f;
				}
			}
			base.update(time);
		}

		// Token: 0x06002AF8 RID: 11000 RVA: 0x00207430 File Offset: 0x00205630
		public static int getMasteryExpNeededForLevel(int level)
		{
			switch (level)
			{
			case 0:
				return 0;
			case 1:
				return 10000;
			case 2:
				return 25000;
			case 3:
				return 45000;
			case 4:
				return 70000;
			case 5:
				return 100000;
			default:
				return int.MaxValue;
			}
		}

		// Token: 0x06002AF9 RID: 11001 RVA: 0x00207484 File Offset: 0x00205684
		public static int getCurrentMasteryLevel()
		{
			int masteryExp = (int)Game1.stats.Get("MasteryExp");
			int level = 0;
			for (int i = 1; i <= 5; i++)
			{
				if (masteryExp >= MasteryTrackerMenu.getMasteryExpNeededForLevel(i))
				{
					level++;
				}
			}
			return level;
		}

		// Token: 0x06002AFA RID: 11002 RVA: 0x002074C0 File Offset: 0x002056C0
		public static void drawBar(SpriteBatch b, Vector2 topLeftSpot, float widthScale = 1f)
		{
			int masteryExp = (int)Game1.stats.Get("MasteryExp");
			int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
			float currentProgressXP = (float)(masteryExp - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved));
			float expNeededToReachNextLevel = (float)(MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved + 1) - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved));
			int barWidth = (int)(576f * currentProgressXP / expNeededToReachNextLevel * widthScale);
			if (levelsAchieved >= 5)
			{
				barWidth = (int)(576f * widthScale);
			}
			if (levelsAchieved >= 5 || barWidth > 0)
			{
				Color light = new Color(60, 180, 80);
				Color med = new Color(0, 113, 62);
				Color medDark = new Color(0, 80, 50);
				Color dark = new Color(0, 60, 30);
				if (levelsAchieved >= 5 && widthScale == 1f)
				{
					light = new Color(220, 220, 220);
					med = new Color(140, 140, 140);
					medDark = new Color(80, 80, 80);
					dark = med;
				}
				if (widthScale != 1f)
				{
					dark = medDark;
				}
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 144, barWidth, 32), med);
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 148, 4, 28), medDark);
				if (barWidth > 8)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 172, barWidth - 8, 4), medDark);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 116, (int)topLeftSpot.Y + 144, barWidth - 4, 4), light);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 104 + barWidth, (int)topLeftSpot.Y + 144, 4, 28), light);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 108 + barWidth, (int)topLeftSpot.Y + 144, 4, 32), dark);
				}
			}
			if (levelsAchieved < 5)
			{
				string s = (masteryExp - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved)).ToString() + "/" + (MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved + 1) - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved)).ToString();
				b.DrawString(Game1.smallFont, s, new Vector2((float)((int)topLeftSpot.X + 112) + 288f * widthScale - Game1.smallFont.MeasureString(s).X / 2f, (float)((int)topLeftSpot.Y) + 146f), Color.White * 0.75f);
			}
		}

		// Token: 0x06002AFB RID: 11003 RVA: 0x00207760 File Offset: 0x00205960
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(1, 85, 21, 21), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White, 4f, true, -1f);
			b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(6f, 7f) * 4f, new Rectangle?(new Rectangle(0, 144, 23, 23)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(24f, (float)(this.height - 24)), new Rectangle?(new Rectangle(0, 144, 23, 23)), Color.White, -1.5707964f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2((float)(this.width - 24), 28f), new Rectangle?(new Rectangle(0, 144, 23, 23)), Color.White, -4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2((float)(this.width - 24), (float)(this.height - 24)), new Rectangle?(new Rectangle(0, 144, 23, 23)), Color.White, 3.1415927f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Game1.stats.Get("MasteryExp");
			int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
			int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");
			if (this.which == -1)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\1_6_Strings:FinalPath"), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, false, new Color?(Color.Black), 99999);
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(0, 107, 15, 15), this.xPositionOnScreen + 100, this.yPositionOnScreen + 128, 600, 64, Color.White, 4f, true, -1f);
				MasteryTrackerMenu.drawBar(b, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), 1f);
				for (int i = 0; i < 5; i++)
				{
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - 110f + (float)(i * 11 * 4), (float)(this.yPositionOnScreen + 220)), new Rectangle?(new Rectangle((i >= levelsAchieved - levelsNotSpent && i < levelsAchieved) ? (43 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600 / 100 * 10) : ((levelsAchieved > i) ? 33 : 23), 89, 10, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
			}
			else
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\1_6_Strings:" + this.which.ToString() + "_Mastery"), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, false, new Color?(Color.Black), 99999);
				float yMeasure = Game1.smallFont.MeasureString("I").Y;
				foreach (ClickableTextureComponent c in this.rewards)
				{
					if (Game1.smallFont.MeasureString(c.label).Y < yMeasure * 2f)
					{
						Utility.drawWithShadow(b, c.texture, c.getVector2() + new Vector2(0f, -16f), c.sourceRect, Color.White, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, 0.35f);
					}
					else
					{
						Utility.drawWithShadow(b, c.texture, c.getVector2(), c.sourceRect, Color.White, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, 0.35f);
					}
					if (c.name != "")
					{
						Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont, c.getVector2() + new Vector2(104f, 0f), Color.Black, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
					}
					Utility.drawTextWithColoredShadow(b, c.label, Game1.smallFont, c.getVector2() + new Vector2(104f, (float)((c.name == "") ? 0 : 48)), Color.Black, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
					if (c.myAlternateID == 1)
					{
						b.Draw(Game1.objectSpriteSheet, c.getVector2() + new Vector2(32f, (float)(32 + ((c.sourceRect.Height > 16) ? 64 : 0))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16)), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
					}
				}
				if (this.mainButton != null)
				{
					ClickableTextureComponent clickableTextureComponent = this.mainButton;
					if (clickableTextureComponent != null)
					{
						clickableTextureComponent.draw(b, (levelsNotSpent > 0) ? Color.White : (Color.White * 0.5f), 0.88f, 0, 0, 0);
					}
					string s = Game1.content.LoadString("Strings\\1_6_Strings:Claim");
					Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont, this.mainButton.getVector2() + new Vector2((float)(this.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, 6f + (float)((this.mainButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * ((levelsNotSpent > 0) ? 1f : 0.5f), Color.Black * 0.2f, 1f, 0.9f, -1, -1, 3);
				}
			}
			base.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001C92 RID: 7314
		public const int MASTERY_EXP_PER_LEVEL = 10000;

		// Token: 0x04001C93 RID: 7315
		public const int WIDTH = 200;

		// Token: 0x04001C94 RID: 7316
		public const int HEIGHT = 80;

		// Token: 0x04001C95 RID: 7317
		public ClickableTextureComponent mainButton;

		// Token: 0x04001C96 RID: 7318
		private float pressedButtonTimer;

		// Token: 0x04001C97 RID: 7319
		private float destroyTimer;

		// Token: 0x04001C98 RID: 7320
		private List<ClickableTextureComponent> rewards = new List<ClickableTextureComponent>();

		// Token: 0x04001C99 RID: 7321
		private int which;

		// Token: 0x04001C9A RID: 7322
		private bool canClaim;
	}
}
