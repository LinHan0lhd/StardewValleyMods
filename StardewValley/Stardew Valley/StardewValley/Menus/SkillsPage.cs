using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x020002A8 RID: 680
	public class SkillsPage : IClickableMenu
	{
		// Token: 0x06002C79 RID: 11385 RVA: 0x00222FAC File Offset: 0x002211AC
		public SkillsPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			int xPositionOnScreen = this.xPositionOnScreen;
			int spaceToClearSideBorder = IClickableMenu.spaceToClearSideBorder;
			int yPositionOnScreen = this.yPositionOnScreen;
			int spaceToClearTopBorder = IClickableMenu.spaceToClearTopBorder;
			float num = (float)height / 2f;
			this.playerPanel = new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 128, 192);
			ClickableComponent.SetUpNeighbors<ClickableTextureComponent>(this.specialItems, 4);
			ClickableComponent.ChainNeighborsLeftRight<ClickableTextureComponent>(this.specialItems);
			if (!Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.hasOrWillReceiveMail("JojaMember") && (Game1.MasterPlayer.hasOrWillReceiveMail("canReadJunimoText") || Game1.player.hasOrWillReceiveMail("canReadJunimoText")))
			{
				int cc_y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)height / 2f) + 21;
				int cc_x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2;
				cc_x += 80;
				cc_y += 16;
				CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletin"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x, cc_y, 44, 44), 5.ToString() ?? "", cc.shouldNoteAppearInArea(5) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_BulletinBoard") : "???")
					{
						myID = 30211,
						downNeighborID = -99998,
						rightNeighborID = -99998,
						leftNeighborID = -99998,
						upNeighborID = 4
					});
				}
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccBoilerRoom"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x + 60, cc_y + 28, 44, 44), 3.ToString() ?? "", cc.shouldNoteAppearInArea(3) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_BoilerRoom") : "???")
					{
						myID = 30212,
						upNeighborID = 30211,
						leftNeighborID = 30211,
						downNeighborID = 30213,
						rightNeighborID = 4
					});
				}
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccVault"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x + 60, cc_y + 88, 44, 44), 4.ToString() ?? "", cc.shouldNoteAppearInArea(4) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_Vault") : "???")
					{
						myID = 30213,
						upNeighborID = 30212,
						downNeighborID = 30216,
						leftNeighborID = 30215
					});
				}
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccCraftsRoom"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x - 60, cc_y + 28, 44, 44), 1.ToString() ?? "", cc.shouldNoteAppearInArea(1) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_CraftsRoom") : "???")
					{
						myID = 30214,
						upNeighborID = 30211,
						downNeighborID = 30215,
						rightNeighborID = 30212
					});
				}
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccFishTank"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x - 60, cc_y + 88, 44, 44), 2.ToString() ?? "", cc.shouldNoteAppearInArea(2) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_FishTank") : "???")
					{
						myID = 30215,
						upNeighborID = 30214,
						downNeighborID = 30216,
						rightNeighborID = 30213
					});
				}
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccPantry"))
				{
					this.ccTrackerButtons.Add(new ClickableComponent(new Rectangle(cc_x, cc_y + 120, 44, 44), 0.ToString() ?? "", cc.shouldNoteAppearInArea(0) ? Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_Pantry") : "???")
					{
						myID = 30216,
						upNeighborID = 30211,
						rightNeighborID = 30213,
						leftNeighborID = 30215
					});
				}
			}
			int addedX = 0;
			int drawX = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? (this.xPositionOnScreen + width - 448 - 48 + 4) : (this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 4);
			int drawY = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 12;
			for (int i = 4; i < 10; i += 5)
			{
				for (int j = 0; j < 5; j++)
				{
					string professionBlurb = "";
					string professionTitle = "";
					bool drawRed = false;
					int professionNumber = -1;
					switch (j)
					{
					case 0:
						drawRed = (Game1.player.FarmingLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(0, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					case 1:
						drawRed = (Game1.player.MiningLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(3, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					case 2:
						drawRed = (Game1.player.ForagingLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(2, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					case 3:
						drawRed = (Game1.player.FishingLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(1, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					case 4:
						drawRed = (Game1.player.CombatLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(4, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					case 5:
						drawRed = (Game1.player.LuckLevel > i);
						professionNumber = Game1.player.getProfessionForSkill(5, i + 1);
						this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(professionNumber));
						break;
					}
					if (drawRed && (i + 1) % 5 == 0)
					{
						this.skillBars.Add(new ClickableTextureComponent(professionNumber.ToString() ?? "", new Rectangle(addedX + drawX - 4 + i * 36, drawY + j * 68, 56, 36), null, professionBlurb, Game1.mouseCursors, new Rectangle(159, 338, 14, 9), 4f, true)
						{
							myID = ((i + 1 == 5) ? (100 + j) : (200 + j)),
							leftNeighborID = ((i + 1 == 5) ? j : (100 + j)),
							rightNeighborID = ((i + 1 == 5) ? (200 + j) : -1),
							downNeighborID = -99998
						});
					}
				}
				addedX += 24;
			}
			for (int k = 0; k < this.skillBars.Count; k++)
			{
				if (k < this.skillBars.Count - 1 && Math.Abs(this.skillBars[k + 1].myID - this.skillBars[k].myID) < 50)
				{
					this.skillBars[k].downNeighborID = this.skillBars[k + 1].myID;
					this.skillBars[k + 1].upNeighborID = this.skillBars[k].myID;
				}
			}
			if (this.skillBars.Count > 1 && this.skillBars.Last<ClickableTextureComponent>().myID >= 200 && this.skillBars[this.skillBars.Count - 2].myID >= 200)
			{
				this.skillBars.Last<ClickableTextureComponent>().upNeighborID = this.skillBars[this.skillBars.Count - 2].myID;
			}
			for (int l = 0; l < 5; l++)
			{
				int index;
				if (l != 1)
				{
					if (l != 3)
					{
						index = l;
					}
					else
					{
						index = 1;
					}
				}
				else
				{
					index = 3;
				}
				string text = "";
				switch (index)
				{
				case 0:
					if (Game1.player.FarmingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11592", Game1.player.FarmingLevel) + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11594", Game1.player.FarmingLevel);
					}
					break;
				case 1:
					if (Game1.player.FishingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11598", Game1.player.FishingLevel);
					}
					break;
				case 2:
					if (Game1.player.ForagingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11596", Game1.player.ForagingLevel);
					}
					break;
				case 3:
					if (Game1.player.MiningLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11600", Game1.player.MiningLevel);
					}
					break;
				case 4:
					if (Game1.player.CombatLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11602", Game1.player.CombatLevel * 5);
					}
					break;
				}
				this.skillAreas.Add(new ClickableTextureComponent(index.ToString() ?? "", new Rectangle(drawX - 128 - 48, drawY + l * 68, 148, 36), index.ToString() ?? "", text, null, Rectangle.Empty, 1f, false)
				{
					myID = l,
					downNeighborID = ((l < 4) ? (l + 1) : -99998),
					upNeighborID = ((l > 0) ? (l - 1) : 12341),
					rightNeighborID = 100 + l
				});
			}
		}

		// Token: 0x06002C7A RID: 11386 RVA: 0x00223A94 File Offset: 0x00221C94
		private void parseProfessionDescription(ref string professionBlurb, ref string professionTitle, List<string> professionDescription)
		{
			if (professionDescription.Count > 0)
			{
				professionTitle = professionDescription[0];
				for (int i = 1; i < professionDescription.Count; i++)
				{
					professionBlurb += professionDescription[i];
					if (i < professionDescription.Count - 1)
					{
						professionBlurb += Environment.NewLine;
					}
				}
			}
		}

		// Token: 0x06002C7B RID: 11387 RVA: 0x00223AED File Offset: 0x00221CED
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = ((this.skillAreas.Count > 0) ? base.getComponentWithID(0) : null);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C7C RID: 11388 RVA: 0x00223B14 File Offset: 0x00221D14
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (x > this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 && x < this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 + 200 && y > this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)this.height / 2f) + 21 && y < this.yPositionOnScreen + this.height && Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.hasOrWillReceiveMail("JojaMember") && !Game1.player.mailReceived.Contains("activatedJungleJunimo"))
			{
				this.timesClickedJunimo++;
				if (this.timesClickedJunimo > 6)
				{
					Game1.playSound("discoverMineral", null);
					Game1.playSound("leafrustle", null);
					Game1.player.mailReceived.Add("activatedJungleJunimo");
				}
				else
				{
					Game1.playSound("hammer", null);
				}
			}
			foreach (ClickableComponent c in this.ccTrackerButtons)
			{
				if (c != null && c.containsPoint(x, y) && !c.label.Equals("???"))
				{
					Game1.activeClickableMenu = new JunimoNoteMenu(true, Convert.ToInt32(c.name), true)
					{
						gameMenuTabToReturnTo = GameMenu.skillsTab
					};
					break;
				}
			}
		}

		// Token: 0x06002C7D RID: 11389 RVA: 0x00223CAC File Offset: 0x00221EAC
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.hoverTitle = "";
			this.professionImage = -1;
			foreach (ClickableComponent c in this.ccTrackerButtons)
			{
				if (c != null && c.containsPoint(x, y))
				{
					this.hoverText = c.label;
					break;
				}
			}
			foreach (ClickableTextureComponent c2 in this.skillBars)
			{
				c2.scale = 4f;
				if (c2.containsPoint(x, y) && c2.hoverText.Length > 0 && !c2.name.Equals("-1"))
				{
					this.hoverText = c2.hoverText;
					this.hoverTitle = LevelUpMenu.getProfessionTitleFromNumber(Convert.ToInt32(c2.name));
					this.professionImage = Convert.ToInt32(c2.name);
					c2.scale = 0f;
				}
			}
			foreach (ClickableTextureComponent c3 in this.skillAreas)
			{
				if (c3.containsPoint(x, y) && c3.hoverText.Length > 0)
				{
					this.hoverText = c3.hoverText;
					this.hoverTitle = Farmer.getSkillDisplayNameFromIndex(Convert.ToInt32(c3.name));
					break;
				}
			}
			if (this.playerPanel.Contains(x, y))
			{
				this.playerPanelTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (this.playerPanelTimer <= 0)
				{
					this.playerPanelIndex = (this.playerPanelIndex + 1) % 4;
					this.playerPanelTimer = 150;
					return;
				}
			}
			else
			{
				this.playerPanelIndex = 0;
			}
		}

		// Token: 0x06002C7E RID: 11390 RVA: 0x00223EB8 File Offset: 0x002220B8
		public override void draw(SpriteBatch b)
		{
			int x = this.xPositionOnScreen + 64 - 8;
			int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2((float)x, (float)(y - 16 - 4)), Color.White);
			FarmerRenderer.isDrawingForUI = true;
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(Game1.player.bathingClothes.Value ? 108 : this.playerPanelFrames[this.playerPanelIndex], 0, false, false, null, false), Game1.player.bathingClothes.Value ? 108 : this.playerPanelFrames[this.playerPanelIndex], new Rectangle(this.playerPanelFrames[this.playerPanelIndex] * 16, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(x + 32), (float)(y + 16 - 4)), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
			if (Game1.timeOfDay >= 1900)
			{
				Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(this.playerPanelFrames[this.playerPanelIndex], 0, false, false, null, false), this.playerPanelFrames[this.playerPanelIndex], new Rectangle(this.playerPanelFrames[this.playerPanelIndex] * 16, 0, 16, 32), new Vector2((float)(x + 32), (float)(y + 16 - 4)), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
			}
			FarmerRenderer.isDrawingForUI = false;
			b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)this.height / 2f) + 21, this.width - IClickableMenu.spaceToClearSideBorder * 4 - 8, 4), new Color(214, 143, 84));
			b.DrawString(Game1.smallFont, Game1.player.Name, new Vector2((float)(x + 64) - Game1.smallFont.MeasureString(Game1.player.Name).X / 2f, (float)(y + 192 - 17)), Game1.textColor);
			b.DrawString(Game1.smallFont, Game1.player.getTitle(), new Vector2((float)(x + 64) - Game1.smallFont.MeasureString(Game1.player.getTitle()).X / 2f, (float)(y + 256 - 32 - 19)), Game1.textColor);
			x = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? (this.xPositionOnScreen + this.width - 448 - 48) : (this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8));
			y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 8;
			int addedX = 0;
			int verticalSpacing = 68;
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					bool drawRed = false;
					bool addedSkill = false;
					string skill = "";
					int skillLevel = 0;
					Rectangle iconSource = Rectangle.Empty;
					switch (j)
					{
					case 0:
						drawRed = (Game1.player.FarmingLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604");
						}
						skillLevel = Game1.player.FarmingLevel;
						addedSkill = (Game1.player.buffs.FarmingLevel > 0);
						iconSource = new Rectangle(10, 428, 10, 10);
						break;
					case 1:
						drawRed = (Game1.player.MiningLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605");
						}
						skillLevel = Game1.player.MiningLevel;
						addedSkill = (Game1.player.buffs.MiningLevel > 0);
						iconSource = new Rectangle(30, 428, 10, 10);
						break;
					case 2:
						drawRed = (Game1.player.ForagingLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606");
						}
						skillLevel = Game1.player.ForagingLevel;
						addedSkill = (Game1.player.buffs.ForagingLevel > 0);
						iconSource = new Rectangle(60, 428, 10, 10);
						break;
					case 3:
						drawRed = (Game1.player.FishingLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607");
						}
						skillLevel = Game1.player.FishingLevel;
						addedSkill = (Game1.player.buffs.FishingLevel > 0);
						iconSource = new Rectangle(20, 428, 10, 10);
						break;
					case 4:
						drawRed = (Game1.player.CombatLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608");
						}
						skillLevel = Game1.player.CombatLevel;
						addedSkill = (Game1.player.buffs.CombatLevel > 0);
						iconSource = new Rectangle(120, 428, 10, 10);
						break;
					case 5:
						drawRed = (Game1.player.LuckLevel > i);
						if (i == 0)
						{
							skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11609");
						}
						skillLevel = Game1.player.LuckLevel;
						addedSkill = (Game1.player.buffs.LuckLevel > 0);
						iconSource = new Rectangle(50, 428, 10, 10);
						break;
					}
					if (!skill.Equals(""))
					{
						b.DrawString(Game1.smallFont, skill, new Vector2((float)x - Game1.smallFont.MeasureString(skill).X + 4f - 64f, (float)(y + 4 + j * verticalSpacing)), Game1.textColor);
						b.Draw(Game1.mouseCursors, new Vector2((float)(x - 56), (float)(y + j * verticalSpacing)), new Rectangle?(iconSource), Color.Black * 0.3f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
						b.Draw(Game1.mouseCursors, new Vector2((float)(x - 52), (float)(y - 4 + j * verticalSpacing)), new Rectangle?(iconSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					if (!drawRed && (i + 1) % 5 == 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(addedX + x - 4 + i * 36), (float)(y + j * verticalSpacing)), new Rectangle?(new Rectangle(145, 338, 14, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
						b.Draw(Game1.mouseCursors, new Vector2((float)(addedX + x + i * 36), (float)(y - 4 + j * verticalSpacing)), new Rectangle?(new Rectangle(145 + (drawRed ? 14 : 0), 338, 14, 9)), Color.White * (drawRed ? 1f : 0.65f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					else if ((i + 1) % 5 != 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(addedX + x - 4 + i * 36), (float)(y + j * verticalSpacing)), new Rectangle?(new Rectangle(129, 338, 8, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
						b.Draw(Game1.mouseCursors, new Vector2((float)(addedX + x + i * 36), (float)(y - 4 + j * verticalSpacing)), new Rectangle?(new Rectangle(129 + (drawRed ? 8 : 0), 338, 8, 9)), Color.White * (drawRed ? 1f : 0.65f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					if (i == 9)
					{
						NumberSprite.draw(skillLevel, b, new Vector2((float)(addedX + x + (i + 2) * 36 + 12 + ((skillLevel >= 10) ? 12 : 0)), (float)(y + 16 + j * verticalSpacing)), Color.Black * 0.35f, 1f, 0.85f, 1f, 0, 0);
						NumberSprite.draw(skillLevel, b, new Vector2((float)(addedX + x + (i + 2) * 36 + 16 + ((skillLevel >= 10) ? 12 : 0)), (float)(y + 12 + j * verticalSpacing)), (addedSkill ? Color.LightGreen : Color.SandyBrown) * ((skillLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0, 0);
					}
				}
				if ((i + 1) % 5 == 0)
				{
					addedX += 24;
				}
			}
			foreach (ClickableTextureComponent clickableTextureComponent in this.skillBars)
			{
				clickableTextureComponent.draw(b);
			}
			foreach (ClickableTextureComponent c in this.skillBars)
			{
				if (c.scale == 0f)
				{
					IClickableMenu.drawTextureBox(b, c.bounds.X - 16 - 8, c.bounds.Y - 16 - 16, 96, 96, Color.White);
					b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X - 8), (float)(c.bounds.Y - 32 + 16)), new Rectangle?(new Rectangle(this.professionImage % 6 * 16, 624 + this.professionImage / 6 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 - 8;
			y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 320 - 36;
			if (Game1.netWorldState.Value.GoldenWalnuts > 0)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + ((Game1.player.QiGems <= 0) ? 24 : 0)), (float)y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
				x += ((Game1.player.QiGems <= 0) ? 60 : 36);
				b.DrawString(Game1.smallFont, Game1.netWorldState.Value.GoldenWalnuts.ToString() ?? "", new Vector2((float)x, (float)y), Game1.textColor);
				x += 56;
			}
			if (Game1.player.QiGems > 0)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + ((Game1.netWorldState.Value.GoldenWalnuts <= 0) ? 24 : 0)), (float)y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
				x += ((Game1.netWorldState.Value.GoldenWalnuts <= 0) ? 60 : 36);
				b.DrawString(Game1.smallFont, Game1.player.QiGems.ToString() ?? "", new Vector2((float)x, (float)y), Game1.textColor);
				x += 64;
			}
			y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)this.height / 2f) + 21;
			x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2;
			bool isJoja = Game1.MasterPlayer.mailReceived.Contains("JojaMember");
			x += 80;
			y += 16;
			if (isJoja || Game1.MasterPlayer.hasOrWillReceiveMail("canReadJunimoText") || Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
			{
				if (!isJoja)
				{
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletin") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				}
				else
				{
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 80), (float)(y - 16)), new Rectangle?(new Rectangle(363, 250, 51, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				}
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 60), (float)(y + 28)), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccBoilerRoom") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 60), (float)(y + 88)), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccVault") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 60), (float)(y + 28)), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccCraftsRoom") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 60), (float)(y + 88)), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccFishTank") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)x, (float)(y + 120)), new Rectangle?(new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccPantry") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				if (!Utility.hasFinishedJojaRoute() && Game1.MasterPlayer.hasCompletedCommunityCenter())
				{
					b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 4) + 30f, (float)(y + 52) + 30f), new Rectangle?(new Rectangle(386, 299, 13, 15)), Color.White, 0f, new Vector2(7.5f), 4f + (float)this.timesClickedJunimo * 0.2f, SpriteEffects.None, 0.7f);
					if (Game1.player.mailReceived.Contains("activatedJungleJunimo"))
					{
						b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 80), (float)(y - 16)), new Rectangle?(new Rectangle(311, 251, 51, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
					}
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 80), (float)(y - 16)), new Rectangle?(new Rectangle(414, 250, 52, 47)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			}
			x += 124;
			b.Draw(Game1.staminaRect, new Rectangle(x, y - 16, 4, (int)((float)this.height / 3f) - 32 - 4), new Color(214, 143, 84));
			int xHouseOffset = 0;
			if (Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.houseUpgradeLevel.Value + 1)).X > 120f)
			{
				xHouseOffset -= 20;
			}
			y += 108;
			x += 28;
			b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset + 20), (float)(y - 4)), new Rectangle?(new Rectangle(653, 880, 10, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.houseUpgradeLevel.Value + 1), Game1.smallFont, new Vector2((float)(x + xHouseOffset + 72), (float)y), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			if (Game1.player.houseUpgradeLevel.Value >= 3)
			{
				int interval = 709;
				b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)(-(float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) * 0.01f), new Rectangle?(new Rectangle(372, 1956, 10, 10)), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 2000f), (float)(-(float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) * 0.001f, new Vector2(3f, 3f), 0.5f + (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)(-(float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) * 0.01f), new Rectangle?(new Rectangle(372, 1956, 10, 10)), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 2000f), (float)(-(float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) * 0.001f, new Vector2(5f, 5f), 0.5f + (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)(-(float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) * 0.01f), new Rectangle?(new Rectangle(372, 1956, 10, 10)), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 2000f), (float)(-(float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) * 0.001f, new Vector2(4f, 4f), 0.5f + (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
			}
			x += 180;
			y -= 8;
			bool drawSkull = false;
			int lowestLevel = MineShaft.lowestLevelReached;
			if (lowestLevel > 120)
			{
				lowestLevel -= 120;
				drawSkull = true;
			}
			b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 8), (float)y), new Rectangle?(new Rectangle((lowestLevel == 0) ? 434 : 385, 315, 13, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			if (lowestLevel != 0)
			{
				Utility.drawTextWithShadow(b, lowestLevel.ToString() ?? "", Game1.smallFont, new Vector2((float)(x + 72 + (drawSkull ? 8 : 0)), (float)(y + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			if (drawSkull)
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 40), (float)(y + 24)), new Rectangle?(new Rectangle(412, 319, 8, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			}
			x += 120;
			int numStardrops = Utility.numStardropsFound(null);
			if (numStardrops > 0)
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 32), (float)(y - 4)), new Rectangle?(new Rectangle(399, 314, 12, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				Utility.drawTextWithShadow(b, "x " + numStardrops.ToString(), Game1.smallFont, new Vector2((float)(x + 88), (float)(y + 8)), (numStardrops >= 7) ? new Color(160, 30, 235) : Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			else
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 32), (float)(y - 4)), new Rectangle?(new Rectangle(421, 314, 12, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			}
			if (Game1.stats.Get("MasteryExp") > 0U)
			{
				int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
				string masteryText = Game1.content.LoadString("Strings\\1_6_Strings:Mastery");
				masteryText = masteryText.TrimEnd(':');
				float masteryStringWidth = Game1.smallFont.MeasureString(masteryText).X;
				int xOffset = (int)masteryStringWidth - 64;
				int yOffset = 84;
				b.DrawString(Game1.smallFont, masteryText, new Vector2((float)(this.xPositionOnScreen + 256), (float)(yOffset + this.yPositionOnScreen + 408)), Game1.textColor);
				Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2((float)(xOffset + this.xPositionOnScreen + 332), (float)(yOffset + this.yPositionOnScreen + 400)), new Rectangle(457, 298, 11, 11), Color.White, 0f, Vector2.Zero, -1f, false, -1f, -1, -1, 0.35f);
				float width = 0.64f;
				width -= (masteryStringWidth - 100f) / 800f;
				if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
				{
					width += 0.1f;
				}
				b.Draw(Game1.staminaRect, new Rectangle(xOffset + this.xPositionOnScreen + 380 - 1, yOffset + this.yPositionOnScreen + 408, (int)(584f * width) + 4, 40), Color.Black * 0.35f);
				b.Draw(Game1.staminaRect, new Rectangle(xOffset + this.xPositionOnScreen + 384, yOffset + this.yPositionOnScreen + 404, (int)((float)(((masteryLevel >= 5) ? 144 : 146) * 4) * width) + 4, 40), new Color(60, 60, 25));
				b.Draw(Game1.staminaRect, new Rectangle(xOffset + this.xPositionOnScreen + 388, yOffset + this.yPositionOnScreen + 408, (int)(576f * width), 32), new Color(173, 129, 79));
				MasteryTrackerMenu.drawBar(b, new Vector2((float)(xOffset + this.xPositionOnScreen + 276), (float)(yOffset + this.yPositionOnScreen + 264)), width);
				NumberSprite.draw(masteryLevel, b, new Vector2((float)(xOffset + this.xPositionOnScreen + 408 + (int)(584f * width)), (float)(yOffset + this.yPositionOnScreen + 428)), Color.Black * 0.35f, 1f, 0.85f, 1f, 0, 0);
				NumberSprite.draw(masteryLevel, b, new Vector2((float)(xOffset + this.xPositionOnScreen + 412 + (int)(584f * width)), (float)(yOffset + this.yPositionOnScreen + 424)), Color.SandyBrown * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0, 0);
			}
			else
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 304), (float)(y - 88)), new Rectangle?(new Rectangle(366, 236, 142, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			}
			Rectangle doodleSource = new Rectangle(394, 120 + Game1.seasonIndex * 23, 33, 23);
			if (Game1.isGreenRain)
			{
				doodleSource = new Rectangle(427, 143, 33, 23);
			}
			else if (Game1.player.activeDialogueEvents.ContainsKey("married"))
			{
				doodleSource = new Rectangle(427, 97, 33, 23);
			}
			else if (Game1.IsSpring && Game1.dayOfMonth == 13)
			{
				doodleSource.X += 33;
			}
			else if (Game1.IsSummer && Game1.dayOfMonth == 11)
			{
				doodleSource.X += 66;
			}
			else if (Game1.IsFall && Game1.dayOfMonth == 27)
			{
				doodleSource.X += 33;
			}
			else if (Game1.IsWinter && Game1.dayOfMonth == 25)
			{
				doodleSource.X += 33;
			}
			b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x + 144), (float)(y - 20)), new Rectangle?(doodleSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			if (Game1.IsWinter && Game1.player.mailReceived.Contains("sawSecretSanta" + Game1.year.ToString()) && ((Game1.dayOfMonth >= 18 && Game1.dayOfMonth < 25) || (Game1.dayOfMonth == 25 && Game1.timeOfDay < 1500)))
			{
				NPC k = Utility.GetRandomWinterStarParticipant(null);
				Texture2D character_texture;
				try
				{
					character_texture = Game1.content.Load<Texture2D>("Characters\\" + k.Name + "_Winter");
				}
				catch
				{
					character_texture = k.Sprite.Texture;
				}
				Rectangle src = k.getMugShotSourceRect();
				src.Height -= 5;
				b.Draw(character_texture, new Vector2((float)(x + 180), (float)y), new Rectangle?(src), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(x + 244), (float)(y + 40)), new Rectangle?(new Rectangle(147, 412, 10, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			}
			if (this.hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (this.hoverTitle.Length > 0) ? this.hoverTitle : null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001E39 RID: 7737
		public const int region_special1 = 10201;

		// Token: 0x04001E3A RID: 7738
		public const int region_special2 = 10202;

		// Token: 0x04001E3B RID: 7739
		public const int region_special3 = 10203;

		// Token: 0x04001E3C RID: 7740
		public const int region_special4 = 10204;

		// Token: 0x04001E3D RID: 7741
		public const int region_special5 = 10205;

		// Token: 0x04001E3E RID: 7742
		public const int region_special6 = 10206;

		// Token: 0x04001E3F RID: 7743
		public const int region_special7 = 10207;

		// Token: 0x04001E40 RID: 7744
		public const int region_special8 = 10208;

		// Token: 0x04001E41 RID: 7745
		public const int region_special9 = 10209;

		// Token: 0x04001E42 RID: 7746
		public const int region_special_skullkey = 10210;

		// Token: 0x04001E43 RID: 7747
		public const int region_special_townkey = 10211;

		// Token: 0x04001E44 RID: 7748
		public const int region_ccTracker = 30211;

		// Token: 0x04001E45 RID: 7749
		public const int region_skillArea1 = 0;

		// Token: 0x04001E46 RID: 7750
		public const int region_skillArea2 = 1;

		// Token: 0x04001E47 RID: 7751
		public const int region_skillArea3 = 2;

		// Token: 0x04001E48 RID: 7752
		public const int region_skillArea4 = 3;

		// Token: 0x04001E49 RID: 7753
		public const int region_skillArea5 = 4;

		// Token: 0x04001E4A RID: 7754
		public List<ClickableTextureComponent> skillBars = new List<ClickableTextureComponent>();

		// Token: 0x04001E4B RID: 7755
		public List<ClickableTextureComponent> skillAreas = new List<ClickableTextureComponent>();

		// Token: 0x04001E4C RID: 7756
		public List<ClickableTextureComponent> specialItems = new List<ClickableTextureComponent>();

		// Token: 0x04001E4D RID: 7757
		public List<ClickableComponent> ccTrackerButtons = new List<ClickableComponent>();

		// Token: 0x04001E4E RID: 7758
		private string hoverText = "";

		// Token: 0x04001E4F RID: 7759
		private string hoverTitle = "";

		// Token: 0x04001E50 RID: 7760
		private int professionImage = -1;

		// Token: 0x04001E51 RID: 7761
		private int playerPanelIndex;

		// Token: 0x04001E52 RID: 7762
		private int playerPanelTimer;

		// Token: 0x04001E53 RID: 7763
		private Rectangle playerPanel;

		// Token: 0x04001E54 RID: 7764
		private int[] playerPanelFrames = new int[]
		{
			0,
			1,
			0,
			2
		};

		// Token: 0x04001E55 RID: 7765
		private int timesClickedJunimo;
	}
}
