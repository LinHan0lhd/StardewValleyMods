using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x02000284 RID: 644
	public class LevelUpMenu : IClickableMenu
	{
		// Token: 0x06002A96 RID: 10902 RVA: 0x002009A0 File Offset: 0x001FEBA0
		public LevelUpMenu() : base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512, false)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			this.width = 768;
			this.height = 512;
			this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101
			};
			this.RepositionOkButton();
		}

		// Token: 0x06002A97 RID: 10903 RVA: 0x00200ACC File Offset: 0x001FECCC
		public LevelUpMenu(int skill, int level) : base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512, false)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			this.timerBeforeStart = 250;
			this.isActive = true;
			this.width = 960;
			this.height = 512;
			this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101
			};
			this.informationUp = true;
			this.isProfessionChooser = ((level == 5 || level == 10) && skill != 5);
			this.currentLevel = level;
			this.currentSkill = skill;
			this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", level, Farmer.getSkillDisplayNameFromIndex(skill));
			this.extraInfoForLevel = this.getExtraInfoForLevel(skill, level);
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.stats.checkForSkillAchievements(true);
			Game1.player.AddMissedMailAndRecipes();
			switch (skill)
			{
			case 0:
				this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
				break;
			case 1:
				this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
				break;
			case 2:
				this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
				break;
			case 3:
				this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
				break;
			case 4:
				this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
				break;
			case 5:
				this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
				break;
			}
			int newHeight = 0;
			foreach (KeyValuePair<string, string> v in CraftingRecipe.craftingRecipes)
			{
				int reqSkillNumber;
				int minLevel;
				if (CraftingRecipe.TryParseLevelRequirement(v.Key, v.Value, false, out reqSkillNumber, out minLevel, true) && reqSkillNumber == skill && minLevel == level)
				{
					CraftingRecipe recipe = new CraftingRecipe(v.Key, false);
					this.newCraftingRecipes.Add(recipe);
					newHeight += (recipe.bigCraftable ? 128 : 64);
				}
			}
			foreach (KeyValuePair<string, string> v2 in CraftingRecipe.cookingRecipes)
			{
				int reqSkillNumber2;
				int minLevel2;
				if (CraftingRecipe.TryParseLevelRequirement(v2.Key, v2.Value, true, out reqSkillNumber2, out minLevel2, true) && reqSkillNumber2 == skill && minLevel2 == level)
				{
					CraftingRecipe recipe2 = new CraftingRecipe(v2.Key, true);
					this.newCraftingRecipes.Add(recipe2);
					newHeight += (recipe2.bigCraftable ? 128 : 64);
				}
			}
			this.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
			Game1.player.freezePause = 100;
			this.gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
			if (this.isProfessionChooser)
			{
				this.leftProfession = new ClickableComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + 128, this.width / 2, this.height), "")
				{
					myID = 102,
					rightNeighborID = 103
				};
				this.rightProfession = new ClickableComponent(new Rectangle(this.width / 2 + this.xPositionOnScreen, this.yPositionOnScreen + 128, this.width / 2, this.height), "")
				{
					myID = 103,
					leftNeighborID = 102
				};
			}
			this.populateClickableComponentList();
		}

		// Token: 0x06002A98 RID: 10904 RVA: 0x00200F24 File Offset: 0x001FF124
		public bool CanReceiveInput()
		{
			return this.informationUp && this.timerBeforeStart <= 0;
		}

		// Token: 0x06002A99 RID: 10905 RVA: 0x00200F3C File Offset: 0x001FF13C
		public override void snapToDefaultClickableComponent()
		{
			if (this.isProfessionChooser)
			{
				this.currentlySnappedComponent = base.getComponentWithID(103);
				Game1.setMousePosition(this.xPositionOnScreen + this.width + 64, this.yPositionOnScreen + this.height + 64);
				return;
			}
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A9A RID: 10906 RVA: 0x00200F99 File Offset: 0x001FF199
		public override void applyMovementKey(int direction)
		{
			if (!this.CanReceiveInput())
			{
				return;
			}
			if (direction == 3 || direction == 1)
			{
				this.hasMovedSelection = true;
			}
			base.applyMovementKey(direction);
		}

		// Token: 0x06002A9B RID: 10907 RVA: 0x00200FBA File Offset: 0x001FF1BA
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
			this.RepositionOkButton();
		}

		// Token: 0x06002A9C RID: 10908 RVA: 0x00200FF8 File Offset: 0x001FF1F8
		public virtual void RepositionOkButton()
		{
			this.okButton.bounds = new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64);
			if (this.okButton.bounds.Right > Game1.uiViewport.Width)
			{
				this.okButton.bounds.X = Game1.uiViewport.Width - 64;
			}
			if (this.okButton.bounds.Bottom > Game1.uiViewport.Height)
			{
				this.okButton.bounds.Y = Game1.uiViewport.Height - 64;
			}
		}

		// Token: 0x06002A9D RID: 10909 RVA: 0x002010B0 File Offset: 0x001FF2B0
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002A9E RID: 10910 RVA: 0x002010B4 File Offset: 0x001FF2B4
		public List<string> getExtraInfoForLevel(int whichSkill, int whichLevel)
		{
			List<string> extraInfo = new List<string>();
			switch (whichSkill)
			{
			case 0:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming1"));
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming2"));
				break;
			case 1:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Fishing"));
				break;
			case 2:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging1"));
				if (whichLevel != 1)
				{
					if (whichLevel == 4 || whichLevel == 8)
					{
						extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging3"));
					}
				}
				else
				{
					extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging2"));
				}
				break;
			case 3:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Mining"));
				break;
			case 4:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Combat"));
				break;
			case 5:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Luck"));
				break;
			}
			return extraInfo;
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x002011C8 File Offset: 0x001FF3C8
		private static void addProfessionDescriptions(List<string> descriptions, string professionName)
		{
			descriptions.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + professionName));
			descriptions.AddRange(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + professionName).Split('\n', StringSplitOptions.None));
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x00201208 File Offset: 0x001FF408
		private static string getProfessionName(int whichProfession)
		{
			switch (whichProfession)
			{
			case 0:
				return "Rancher";
			case 1:
				return "Tiller";
			case 2:
				return "Coopmaster";
			case 3:
				return "Shepherd";
			case 4:
				return "Artisan";
			case 5:
				return "Agriculturist";
			case 6:
				return "Fisher";
			case 7:
				return "Trapper";
			case 8:
				return "Angler";
			case 9:
				return "Pirate";
			case 10:
				return "Mariner";
			case 11:
				return "Luremaster";
			case 12:
				return "Forester";
			case 13:
				return "Gatherer";
			case 14:
				return "Lumberjack";
			case 15:
				return "Tapper";
			case 16:
				return "Botanist";
			case 17:
				return "Tracker";
			case 18:
				return "Miner";
			case 19:
				return "Geologist";
			case 20:
				return "Blacksmith";
			case 21:
				return "Prospector";
			case 22:
				return "Excavator";
			case 23:
				return "Gemologist";
			case 24:
				return "Fighter";
			case 25:
				return "Scout";
			case 26:
				return "Brute";
			case 27:
				return "Defender";
			case 28:
				return "Acrobat";
			default:
				return "Desperado";
			}
		}

		// Token: 0x06002AA1 RID: 10913 RVA: 0x00201347 File Offset: 0x001FF547
		public static List<string> getProfessionDescription(int whichProfession)
		{
			List<string> list = new List<string>();
			LevelUpMenu.addProfessionDescriptions(list, LevelUpMenu.getProfessionName(whichProfession));
			return list;
		}

		// Token: 0x06002AA2 RID: 10914 RVA: 0x0020135A File Offset: 0x001FF55A
		public static string getProfessionTitleFromNumber(int whichProfession)
		{
			return Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + LevelUpMenu.getProfessionName(whichProfession));
		}

		// Token: 0x06002AA3 RID: 10915 RVA: 0x00201376 File Offset: 0x001FF576
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x06002AA4 RID: 10916 RVA: 0x00201378 File Offset: 0x001FF578
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if ((button == Buttons.Start || button == Buttons.B) && !this.isProfessionChooser && this.isActive)
			{
				this.okButtonClicked();
			}
		}

		// Token: 0x06002AA5 RID: 10917 RVA: 0x002013A4 File Offset: 0x001FF5A4
		public static void AddMissedProfessionChoices(Farmer farmer)
		{
			foreach (int skill in new int[]
			{
				0,
				1,
				2,
				3,
				4
			})
			{
				if (farmer.GetUnmodifiedSkillLevel(skill) >= 5 && !farmer.newLevels.Contains(new Point(skill, 5)) && farmer.getProfessionForSkill(skill, 5) == -1)
				{
					farmer.newLevels.Add(new Point(skill, 5));
				}
				if (farmer.GetUnmodifiedSkillLevel(skill) >= 10 && !farmer.newLevels.Contains(new Point(skill, 10)) && farmer.getProfessionForSkill(skill, 10) == -1)
				{
					farmer.newLevels.Add(new Point(skill, 10));
				}
			}
		}

		// Token: 0x06002AA6 RID: 10918 RVA: 0x00201458 File Offset: 0x001FF658
		public static void removeImmediateProfessionPerk(int whichProfession)
		{
			if (whichProfession != 24)
			{
				if (whichProfession == 27)
				{
					Game1.player.maxHealth -= 25;
				}
			}
			else
			{
				Game1.player.maxHealth -= 15;
			}
			if (Game1.player.health > Game1.player.maxHealth)
			{
				Game1.player.health = Game1.player.maxHealth;
			}
		}

		// Token: 0x06002AA7 RID: 10919 RVA: 0x002014C4 File Offset: 0x001FF6C4
		public void getImmediateProfessionPerk(int whichProfession)
		{
			if (whichProfession != 24)
			{
				if (whichProfession == 27)
				{
					Game1.player.maxHealth += 25;
				}
			}
			else
			{
				Game1.player.maxHealth += 15;
			}
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.stamina = (float)Game1.player.MaxStamina;
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x00201530 File Offset: 0x001FF730
		public static void RevalidateHealth(Farmer farmer)
		{
			int expected_max_health = 100;
			if (farmer.mailReceived.Contains("qiCave"))
			{
				expected_max_health += 25;
			}
			for (int i = 1; i <= farmer.GetUnmodifiedSkillLevel(4); i++)
			{
				if (!farmer.newLevels.Contains(new Point(4, i)) && i != 5 && i != 10)
				{
					expected_max_health += 5;
				}
			}
			if (farmer.professions.Contains(24))
			{
				expected_max_health += 15;
			}
			if (farmer.professions.Contains(27))
			{
				expected_max_health += 25;
			}
			if (farmer.maxHealth < expected_max_health)
			{
				Game1.log.Verbose(string.Concat(new string[]
				{
					"Fixing max health of: ",
					farmer.Name,
					" was ",
					farmer.maxHealth.ToString(),
					" (expected: ",
					expected_max_health.ToString(),
					")"
				}));
				int difference = expected_max_health - farmer.maxHealth;
				farmer.maxHealth = expected_max_health;
				farmer.health += difference;
			}
		}

		// Token: 0x06002AA9 RID: 10921 RVA: 0x00201630 File Offset: 0x001FF830
		public override void update(GameTime time)
		{
			if (!this.isActive)
			{
				base.exitThisMenu(true);
				return;
			}
			if (this.isProfessionChooser && !this.hasUpdatedProfessions)
			{
				if (this.currentLevel == 5)
				{
					this.professionsToChoose.Add(this.currentSkill * 6);
					this.professionsToChoose.Add(this.currentSkill * 6 + 1);
				}
				else if (Game1.player.professions.Contains(this.currentSkill * 6))
				{
					this.professionsToChoose.Add(this.currentSkill * 6 + 2);
					this.professionsToChoose.Add(this.currentSkill * 6 + 3);
				}
				else
				{
					this.professionsToChoose.Add(this.currentSkill * 6 + 4);
					this.professionsToChoose.Add(this.currentSkill * 6 + 5);
				}
				this.leftProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
				this.rightProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
				this.hasUpdatedProfessions = true;
			}
			this.littleStars.RemoveWhere((TemporaryAnimatedSprite star) => star.update(time));
			if (Game1.random.NextDouble() < 0.03)
			{
				Vector2 position = new Vector2(0f, (float)(Game1.random.Next(this.yPositionOnScreen - 128, this.yPositionOnScreen - 4) / 20 * 4 * 5 + 32));
				if (Game1.random.NextBool())
				{
					position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 228, this.xPositionOnScreen + this.width / 2 - 132);
				}
				else
				{
					position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 + 116, this.xPositionOnScreen + this.width - 160);
				}
				if (position.Y < (float)(this.yPositionOnScreen - 64 - 8))
				{
					position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 116, this.xPositionOnScreen + this.width / 2 + 116);
				}
				position.X = position.X / 20f * 4f * 5f;
				this.littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					local = true
				});
			}
			if (this.timerBeforeStart > 0)
			{
				this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
				if (this.timerBeforeStart <= 0 && Game1.options.SnappyMenus)
				{
					this.populateClickableComponentList();
					this.snapToDefaultClickableComponent();
				}
				return;
			}
			if (this.isActive && this.isProfessionChooser)
			{
				this.leftProfessionColor = Game1.textColor;
				this.rightProfessionColor = Game1.textColor;
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.freezePause = 100;
				if (Game1.getMouseY() > this.yPositionOnScreen + 192 && Game1.getMouseY() < this.yPositionOnScreen + this.height)
				{
					if (Game1.getMouseX() > this.xPositionOnScreen && Game1.getMouseX() < this.xPositionOnScreen + this.width / 2)
					{
						this.leftProfessionColor = Color.Green;
						if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
						{
							Game1.player.professions.Add(this.professionsToChoose[0]);
							this.getImmediateProfessionPerk(this.professionsToChoose[0]);
							this.isActive = false;
							this.informationUp = false;
							this.isProfessionChooser = false;
							this.RemoveLevelFromLevelList();
						}
					}
					else if (Game1.getMouseX() > this.xPositionOnScreen + this.width / 2 && Game1.getMouseX() < this.xPositionOnScreen + this.width)
					{
						this.rightProfessionColor = Color.Green;
						if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
						{
							Game1.player.professions.Add(this.professionsToChoose[1]);
							this.getImmediateProfessionPerk(this.professionsToChoose[1]);
							this.isActive = false;
							this.informationUp = false;
							this.isProfessionChooser = false;
							this.RemoveLevelFromLevelList();
						}
					}
				}
				this.height = 512;
			}
			this.oldMouseState = Game1.input.GetMouseState();
			if (this.isActive && !this.informationUp && this.starIcon != null)
			{
				if (this.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					this.starIcon.sourceRect.X = 294;
				}
				else
				{
					this.starIcon.sourceRect.X = 310;
				}
			}
			if (this.isActive && this.starIcon != null && !this.informationUp && (this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.starIcon.containsPoint(this.oldMouseState.X, this.oldMouseState.Y))
			{
				this.newCraftingRecipes.Clear();
				this.extraInfoForLevel.Clear();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.playSound("bigSelect", null);
				this.informationUp = true;
				this.isProfessionChooser = false;
				Point newLevel = Game1.player.newLevels[0];
				this.currentLevel = newLevel.Y;
				this.currentSkill = newLevel.X;
				this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", this.currentLevel, Farmer.getSkillDisplayNameFromIndex(this.currentSkill));
				this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentSkill, this.currentLevel);
				switch (this.currentSkill)
				{
				case 0:
					this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
					break;
				case 1:
					this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
					break;
				case 2:
					this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
					break;
				case 3:
					this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
					break;
				case 4:
					this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
					break;
				case 5:
					this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
					break;
				}
				if ((this.currentLevel == 5 || this.currentLevel == 10) && this.currentSkill != 5)
				{
					this.professionsToChoose.Clear();
					this.isProfessionChooser = true;
					if (this.currentLevel == 5)
					{
						this.professionsToChoose.Add(this.currentSkill * 6);
						this.professionsToChoose.Add(this.currentSkill * 6 + 1);
					}
					else if (Game1.player.professions.Contains(this.currentSkill * 6))
					{
						this.professionsToChoose.Add(this.currentSkill * 6 + 2);
						this.professionsToChoose.Add(this.currentSkill * 6 + 3);
					}
					else
					{
						this.professionsToChoose.Add(this.currentSkill * 6 + 4);
						this.professionsToChoose.Add(this.currentSkill * 6 + 5);
					}
					this.leftProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
					this.rightProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
				}
				int newHeight = 0;
				foreach (KeyValuePair<string, string> v in CraftingRecipe.craftingRecipes)
				{
					string conditions = ArgUtility.Get(v.Value.Split('/', StringSplitOptions.None), 4, "", true);
					if (conditions.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && conditions.Contains(this.currentLevel.ToString() ?? ""))
					{
						CraftingRecipe recipe = new CraftingRecipe(v.Key, false);
						this.newCraftingRecipes.Add(recipe);
						Game1.player.craftingRecipes.TryAdd(v.Key, 0);
						newHeight += (recipe.bigCraftable ? 128 : 64);
					}
				}
				foreach (KeyValuePair<string, string> v2 in CraftingRecipe.cookingRecipes)
				{
					string conditions2 = ArgUtility.Get(v2.Value.Split('/', StringSplitOptions.None), 3, "", true);
					if (conditions2.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && conditions2.Contains(this.currentLevel.ToString() ?? ""))
					{
						CraftingRecipe recipe2 = new CraftingRecipe(v2.Key, true);
						this.newCraftingRecipes.Add(recipe2);
						if (!Game1.player.cookingRecipes.ContainsKey(v2.Key))
						{
							Game1.player.cookingRecipes.Add(v2.Key, 0);
						}
						newHeight += (recipe2.bigCraftable ? 128 : 64);
					}
				}
				this.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
				Game1.player.freezePause = 100;
			}
			if (this.isActive && this.informationUp)
			{
				Game1.player.completelyStopAnimatingOrDoingAction();
				if (this.okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.isProfessionChooser)
				{
					this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
					if ((this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
					{
						this.okButtonClicked();
					}
				}
				else
				{
					this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
				}
				Game1.player.freezePause = 100;
			}
		}

		// Token: 0x06002AAA RID: 10922 RVA: 0x00202198 File Offset: 0x00200398
		protected override void cleanupBeforeExit()
		{
			if (this.isActive)
			{
				this.okButtonClicked();
			}
		}

		// Token: 0x06002AAB RID: 10923 RVA: 0x002021A8 File Offset: 0x002003A8
		public void okButtonClicked()
		{
			this.getLevelPerk(this.currentSkill, this.currentLevel);
			this.RemoveLevelFromLevelList();
			this.isActive = false;
			this.informationUp = false;
		}

		// Token: 0x06002AAC RID: 10924 RVA: 0x002021D0 File Offset: 0x002003D0
		public virtual void RemoveLevelFromLevelList()
		{
			Game1.player.newLevels.RemoveWhere((Point level) => level.X == this.currentSkill && level.Y == this.currentLevel);
		}

		// Token: 0x06002AAD RID: 10925 RVA: 0x002021EE File Offset: 0x002003EE
		public override void receiveKeyPress(Keys key)
		{
			if ((!Game1.options.doesInputListContain(Game1.options.cancelButton, key) && !Game1.options.doesInputListContain(Game1.options.menuButton, key)) || !this.isProfessionChooser)
			{
				base.receiveKeyPress(key);
				return;
			}
		}

		// Token: 0x06002AAE RID: 10926 RVA: 0x00202230 File Offset: 0x00200430
		public void getLevelPerk(int skill, int level)
		{
			if (skill != 1)
			{
				if (skill == 4)
				{
					Game1.player.maxHealth += 5;
				}
			}
			else if (level != 2)
			{
				if (level == 6)
				{
					if (!Game1.player.hasOrWillReceiveMail("fishing6"))
					{
						Game1.addMailForTomorrow("fishing6", false, false);
					}
				}
			}
			else if (!Game1.player.hasOrWillReceiveMail("fishing2"))
			{
				Game1.addMailForTomorrow("fishing2", false, false);
			}
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.Stamina = (float)Game1.player.maxStamina.Value;
		}

		// Token: 0x06002AAF RID: 10927 RVA: 0x002022D0 File Offset: 0x002004D0
		public override void draw(SpriteBatch b)
		{
			if (this.timerBeforeStart > 0)
			{
				return;
			}
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.littleStars)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 116), (float)(this.yPositionOnScreen - 32 + 12)), new Rectangle?(new Rectangle(363, 87, 58, 22)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (!this.informationUp && this.isActive && this.starIcon != null)
			{
				this.starIcon.draw(b);
				return;
			}
			if (this.informationUp)
			{
				if (this.isProfessionChooser)
				{
					if (this.professionsToChoose.Count == 0)
					{
						return;
					}
					Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
					base.drawHorizontalPartition(b, this.yPositionOnScreen + 192, false, -1, -1, -1);
					base.drawVerticalIntersectingPartition(b, this.xPositionOnScreen + this.width / 2 - 32, this.yPositionOnScreen + 192, -1, -1, -1);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, -1, -1, 0.35f);
					b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), Game1.textColor);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2((float)(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, -1, -1, 0.35f);
					string chooseProfession = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
					b.DrawString(Game1.smallFont, chooseProfession, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString(chooseProfession).X / 2f, (float)(this.yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder)), Game1.textColor);
					b.DrawString(Game1.dialogueFont, this.leftProfessionDescription[0], new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160)), this.leftProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2 - 112), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16)), new Rectangle?(new Rectangle(this.professionsToChoose[0] % 6 * 16, 624 + this.professionsToChoose[0] / 6 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					for (int i = 1; i < this.leftProfessionDescription.Count; i++)
					{
						b.DrawString(Game1.smallFont, Game1.parseText(this.leftProfessionDescription[i], Game1.smallFont, this.width / 2 - 64), new Vector2((float)(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (i + 1))), this.leftProfessionColor);
					}
					b.DrawString(Game1.dialogueFont, this.rightProfessionDescription[0], new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160)), this.rightProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width - 128), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16)), new Rectangle?(new Rectangle(this.professionsToChoose[1] % 6 * 16, 624 + this.professionsToChoose[1] / 6 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					for (int j = 1; j < this.rightProfessionDescription.Count; j++)
					{
						b.DrawString(Game1.smallFont, Game1.parseText(this.rightProfessionDescription[j], Game1.smallFont, this.width / 2 - 48), new Vector2((float)(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (j + 1))), this.rightProfessionColor);
					}
				}
				else
				{
					Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, -1, -1, 0.35f);
					b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), Game1.textColor);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2((float)(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, -1, -1, 0.35f);
					int y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 80;
					foreach (string s in this.extraInfoForLevel)
					{
						b.DrawString(Game1.smallFont, s, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString(s).X / 2f, (float)y), Game1.textColor);
						y += 48;
					}
					foreach (CraftingRecipe s2 in this.newCraftingRecipes)
					{
						string cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_" + (s2.isCookingRecipe ? "cooking" : "crafting"));
						string message = Game1.content.LoadString("Strings\\UI:LevelUp_NewRecipe", cookingOrCrafting, s2.DisplayName);
						b.DrawString(Game1.smallFont, message, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString(message).X / 2f - 64f, (float)(y + (s2.bigCraftable ? 38 : 12))), Game1.textColor);
						s2.drawMenuView(b, (int)((float)(this.xPositionOnScreen + this.width / 2) + Game1.smallFont.MeasureString(message).X / 2f - 48f), y - 16, 0.88f, true);
						y += (s2.bigCraftable ? 128 : 64) + 8;
					}
					this.okButton.draw(b);
				}
				if (!Game1.options.SnappyMenus || !this.isProfessionChooser || this.hasMovedSelection)
				{
					Game1.mouseCursorTransparency = 1f;
					base.drawMouse(b, false, -1);
				}
			}
		}

		// Token: 0x04001C4B RID: 7243
		public const int region_okButton = 101;

		// Token: 0x04001C4C RID: 7244
		public const int region_leftProfession = 102;

		// Token: 0x04001C4D RID: 7245
		public const int region_rightProfession = 103;

		// Token: 0x04001C4E RID: 7246
		public const int basewidth = 768;

		// Token: 0x04001C4F RID: 7247
		public const int baseheight = 512;

		// Token: 0x04001C50 RID: 7248
		public bool informationUp;

		// Token: 0x04001C51 RID: 7249
		public bool isActive;

		// Token: 0x04001C52 RID: 7250
		public bool isProfessionChooser;

		// Token: 0x04001C53 RID: 7251
		public bool hasUpdatedProfessions;

		// Token: 0x04001C54 RID: 7252
		private int currentLevel;

		// Token: 0x04001C55 RID: 7253
		private int currentSkill;

		// Token: 0x04001C56 RID: 7254
		private int timerBeforeStart;

		// Token: 0x04001C57 RID: 7255
		private Color leftProfessionColor = Game1.textColor;

		// Token: 0x04001C58 RID: 7256
		private Color rightProfessionColor = Game1.textColor;

		// Token: 0x04001C59 RID: 7257
		private MouseState oldMouseState;

		// Token: 0x04001C5A RID: 7258
		public ClickableTextureComponent starIcon;

		// Token: 0x04001C5B RID: 7259
		public ClickableTextureComponent okButton;

		// Token: 0x04001C5C RID: 7260
		public ClickableComponent leftProfession;

		// Token: 0x04001C5D RID: 7261
		public ClickableComponent rightProfession;

		// Token: 0x04001C5E RID: 7262
		private List<CraftingRecipe> newCraftingRecipes = new List<CraftingRecipe>();

		// Token: 0x04001C5F RID: 7263
		private List<string> extraInfoForLevel = new List<string>();

		// Token: 0x04001C60 RID: 7264
		private List<string> leftProfessionDescription = new List<string>();

		// Token: 0x04001C61 RID: 7265
		private List<string> rightProfessionDescription = new List<string>();

		// Token: 0x04001C62 RID: 7266
		private Rectangle sourceRectForLevelIcon;

		// Token: 0x04001C63 RID: 7267
		private string title;

		// Token: 0x04001C64 RID: 7268
		private List<int> professionsToChoose = new List<int>();

		// Token: 0x04001C65 RID: 7269
		private TemporaryAnimatedSpriteList littleStars = new TemporaryAnimatedSpriteList();

		// Token: 0x04001C66 RID: 7270
		public bool hasMovedSelection;
	}
}
