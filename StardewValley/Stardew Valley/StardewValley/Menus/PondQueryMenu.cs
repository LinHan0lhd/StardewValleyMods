using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus
{
	// Token: 0x02000299 RID: 665
	public class PondQueryMenu : IClickableMenu
	{
		// Token: 0x06002B83 RID: 11139 RVA: 0x0020F30C File Offset: 0x0020D50C
		public PondQueryMenu(FishPond fish_pond) : base(Game1.uiViewport.Width / 2 - PondQueryMenu.width / 2, Game1.uiViewport.Height / 2 - PondQueryMenu.height / 2, PondQueryMenu.width, PondQueryMenu.height, false)
		{
			Game1.player.Halt();
			PondQueryMenu.width = 384;
			PondQueryMenu.height = 512;
			this._pond = fish_pond;
			this._fishItem = new Object(this._pond.fishType.Value, 1, false, -1, 0);
			this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + PondQueryMenu.width + 4, this.yPositionOnScreen + PondQueryMenu.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101,
				upNeighborID = -99998
			};
			this.emptyButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + PondQueryMenu.width + 4, this.yPositionOnScreen + PondQueryMenu.height - 256 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(32, 384, 16, 16), 4f, false)
			{
				myID = 103,
				downNeighborID = -99998
			};
			this.changeNettingButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + PondQueryMenu.width + 4, this.yPositionOnScreen + PondQueryMenu.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(48, 384, 16, 16), 4f, false)
			{
				myID = 106,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			this.UpdateState();
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.measureTotalHeight() / 2;
		}

		// Token: 0x06002B84 RID: 11140 RVA: 0x0020F523 File Offset: 0x0020D723
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B85 RID: 11141 RVA: 0x0020F53C File Offset: 0x0020D73C
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (Game1.options.menuButton.Contains(new InputButton(key)))
			{
				Game1.playSound("smallSelect", null);
				if (this.readyToClose())
				{
					Game1.exitActiveMenu();
					return;
				}
			}
			else if (Game1.options.SnappyMenus && !Game1.options.menuButton.Contains(new InputButton(key)))
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002B86 RID: 11142 RVA: 0x0020F5B4 File Offset: 0x0020D7B4
		public override void update(GameTime time)
		{
			base.update(time);
			this._age += (float)time.ElapsedGameTime.TotalSeconds;
		}

		// Token: 0x06002B87 RID: 11143 RVA: 0x0020F5E4 File Offset: 0x0020D7E4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (this.confirmingEmpty)
			{
				if (this.yesButton.containsPoint(x, y))
				{
					Game1.playSound("fishSlap", null);
					this._pond.ClearPond();
					base.exitThisMenu(true);
					return;
				}
				if (this.noButton.containsPoint(x, y))
				{
					this.confirmingEmpty = false;
					Game1.playSound("smallSelect", null);
					if (Game1.options.SnappyMenus)
					{
						this.currentlySnappedComponent = base.getComponentWithID(103);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
			}
			else
			{
				if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
				{
					Game1.exitActiveMenu();
					Game1.playSound("smallSelect", null);
				}
				if (this.changeNettingButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6", null);
					NetInt nettingStyle = this._pond.nettingStyle;
					int value = nettingStyle.Value;
					nettingStyle.Value = value + 1;
					this._pond.nettingStyle.Value %= 4;
					return;
				}
				if (this.emptyButton.containsPoint(x, y))
				{
					this._confirmationBoxRectangle = new Rectangle(0, 0, 400, 100);
					this._confirmationBoxRectangle.X = Game1.uiViewport.Width / 2 - this._confirmationBoxRectangle.Width / 2;
					this._confirmationText = Game1.content.LoadString("Strings\\UI:PondQuery_ConfirmEmpty");
					this._confirmationText = Game1.parseText(this._confirmationText, Game1.smallFont, this._confirmationBoxRectangle.Width);
					Vector2 text_size = Game1.smallFont.MeasureString(this._confirmationText);
					this._confirmationBoxRectangle.Height = (int)text_size.Y;
					this._confirmationBoxRectangle.Y = Game1.uiViewport.Height / 2 - this._confirmationBoxRectangle.Height / 2;
					this.confirmingEmpty = true;
					this.yesButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 - 64 - 4, this._confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
					{
						myID = 111,
						rightNeighborID = 105
					};
					this.noButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + 4, this._confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
					{
						myID = 105,
						leftNeighborID = 111
					};
					Game1.playSound("smallSelect", null);
					if (Game1.options.SnappyMenus)
					{
						this.populateClickableComponentList();
						this.currentlySnappedComponent = this.noButton;
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
			}
		}

		// Token: 0x06002B88 RID: 11144 RVA: 0x0020F8D7 File Offset: 0x0020DAD7
		public override bool readyToClose()
		{
			return base.readyToClose() && !Game1.globalFade;
		}

		// Token: 0x06002B89 RID: 11145 RVA: 0x0020F8EC File Offset: 0x0020DAEC
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (this.readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("smallSelect", null);
			}
		}

		// Token: 0x06002B8A RID: 11146 RVA: 0x0020F924 File Offset: 0x0020DB24
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			if (this.okButton != null)
			{
				if (this.okButton.containsPoint(x, y))
				{
					this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
				}
				else
				{
					this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
				}
			}
			if (this.emptyButton != null)
			{
				if (this.emptyButton.containsPoint(x, y))
				{
					this.emptyButton.scale = Math.Min(4.1f, this.emptyButton.scale + 0.05f);
					this.hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_EmptyPond", 10);
				}
				else
				{
					this.emptyButton.scale = Math.Max(4f, this.emptyButton.scale - 0.05f);
				}
			}
			if (this.changeNettingButton != null)
			{
				if (this.changeNettingButton.containsPoint(x, y))
				{
					this.changeNettingButton.scale = Math.Min(4.1f, this.changeNettingButton.scale + 0.05f);
					this.hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_ChangeNetting", 10);
				}
				else
				{
					this.changeNettingButton.scale = Math.Max(4f, this.emptyButton.scale - 0.05f);
				}
			}
			if (this.yesButton != null)
			{
				if (this.yesButton.containsPoint(x, y))
				{
					this.yesButton.scale = Math.Min(1.1f, this.yesButton.scale + 0.05f);
				}
				else
				{
					this.yesButton.scale = Math.Max(1f, this.yesButton.scale - 0.05f);
				}
			}
			if (this.noButton != null)
			{
				if (this.noButton.containsPoint(x, y))
				{
					this.noButton.scale = Math.Min(1.1f, this.noButton.scale + 0.05f);
					return;
				}
				this.noButton.scale = Math.Max(1f, this.noButton.scale - 0.05f);
			}
		}

		// Token: 0x06002B8B RID: 11147 RVA: 0x0020FB6C File Offset: 0x0020DD6C
		public static string GetFishTalkSuffix(Object fishItem)
		{
			HashSet<string> tags = fishItem.GetContextTags();
			if (tags.Contains("fish_talk_rude"))
			{
				return "_Rude";
			}
			if (tags.Contains("fish_talk_stiff"))
			{
				return "_Stiff";
			}
			if (tags.Contains("fish_talk_demanding"))
			{
				return "_Demanding";
			}
			foreach (string tag in tags)
			{
				if (tag.StartsWithIgnoreCase("fish_talk_"))
				{
					char[] array = tag.Substring("fish_talk".Length).ToCharArray();
					bool capitalizeNext = false;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == '_')
						{
							capitalizeNext = true;
						}
						else if (capitalizeNext)
						{
							array[i] = char.ToUpper(array[i]);
							capitalizeNext = false;
						}
					}
					return new string(array);
				}
			}
			if (tags.Contains("fish_carnivorous"))
			{
				return "_Carnivore";
			}
			return "";
		}

		// Token: 0x06002B8C RID: 11148 RVA: 0x0020FC74 File Offset: 0x0020DE74
		public static string getCompletedRequestString(FishPond pond, Object fishItem, Random r)
		{
			if (fishItem != null)
			{
				string talk_suffix = PondQueryMenu.GetFishTalkSuffix(fishItem);
				if (talk_suffix != "")
				{
					return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + talk_suffix + r.Next(3).ToString(), pond.neededItem.Value.DisplayName));
				}
			}
			return Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + r.Next(7).ToString(), pond.neededItem.Value.DisplayName);
		}

		// Token: 0x06002B8D RID: 11149 RVA: 0x0020FD08 File Offset: 0x0020DF08
		public void UpdateState()
		{
			Random r = Utility.CreateDaySaveRandom((double)this._pond.seedOffset.Value, 0.0, 0.0);
			if (this._pond.currentOccupants.Value <= 0)
			{
				this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusNoFish");
				return;
			}
			if (this._pond.neededItem.Value != null)
			{
				if (this._pond.hasCompletedRequest.Value)
				{
					this._statusText = PondQueryMenu.getCompletedRequestString(this._pond, this._fishItem, r);
					return;
				}
				if (this._pond.HasUnresolvedNeeds())
				{
					string item_count_string = this._pond.neededItemCount.Value.ToString() ?? "";
					if (this._pond.neededItemCount.Value <= 1)
					{
						item_count_string = Lexicon.getProperArticleForWord(this._pond.neededItem.Value.DisplayName);
						if (item_count_string == "")
						{
							item_count_string = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestOneCount");
						}
					}
					if (this._fishItem != null)
					{
						if (this._fishItem.HasContextTag("fish_talk_rude"))
						{
							this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Rude" + r.Next(3).ToString() + "_" + (Game1.player.IsMale ? "Male" : "Female"), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
							return;
						}
						string talk_suffix = PondQueryMenu.GetFishTalkSuffix(this._fishItem);
						if (talk_suffix != "")
						{
							this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + talk_suffix + r.Next(3).ToString(), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
							return;
						}
					}
					this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + r.Next(7).ToString(), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
					return;
				}
			}
			if (this._fishItem != null && (this._fishItem.QualifiedItemId == "(O)397" || this._fishItem.QualifiedItemId == "(O)393"))
			{
				this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk_Coral", this._fishItem.DisplayName);
				return;
			}
			this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk" + r.Next(7).ToString());
		}

		// Token: 0x06002B8E RID: 11150 RVA: 0x00210051 File Offset: 0x0020E251
		private int measureTotalHeight()
		{
			return 644 + this.measureExtraTextHeight(this.getDisplayedText());
		}

		// Token: 0x06002B8F RID: 11151 RVA: 0x00210065 File Offset: 0x0020E265
		private int measureExtraTextHeight(string displayed_text)
		{
			return Math.Max(0, (int)Game1.smallFont.MeasureString(displayed_text).Y - 90) + 4;
		}

		// Token: 0x06002B90 RID: 11152 RVA: 0x00210083 File Offset: 0x0020E283
		private string getDisplayedText()
		{
			return Game1.parseText(this._statusText, Game1.smallFont, PondQueryMenu.width - IClickableMenu.spaceToClearSideBorder * 2 - 64);
		}

		// Token: 0x06002B91 RID: 11153 RVA: 0x002100A8 File Offset: 0x0020E2A8
		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				bool has_unresolved_needs = this._pond.neededItem.Value != null && this._pond.HasUnresolvedNeeds() && !this._pond.hasCompletedRequest.Value;
				string pond_name_text = Game1.content.LoadString("Strings\\UI:PondQuery_Name", this._fishItem.DisplayName);
				Vector2 text_size = Game1.smallFont.MeasureString(pond_name_text);
				Game1.DrawBox((int)((float)(Game1.uiViewport.Width / 2) - (text_size.X + 64f) * 0.5f), this.yPositionOnScreen - 4 + 128, (int)(text_size.X + 64f), 64, null);
				Utility.drawTextWithShadow(b, pond_name_text, Game1.smallFont, new Vector2((float)(Game1.uiViewport.Width / 2) - text_size.X * 0.5f, (float)(this.yPositionOnScreen - 4) + 160f - text_size.Y * 0.5f), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				string displayed_text = this.getDisplayedText();
				int extraHeight = 0;
				if (has_unresolved_needs)
				{
					extraHeight += 116;
				}
				int extraTextHeight = this.measureExtraTextHeight(displayed_text);
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen + 128, PondQueryMenu.width, PondQueryMenu.height - 128 + extraHeight + extraTextHeight, false, true, null, false, true, -1, -1, -1);
				string population_text = Game1.content.LoadString("Strings\\UI:PondQuery_Population", this._pond.FishCount.ToString() ?? "", this._pond.maxOccupants);
				text_size = Game1.smallFont.MeasureString(population_text);
				Utility.drawTextWithShadow(b, population_text, Game1.smallFont, new Vector2(this._pond.goldenAnimalCracker.Value ? ((float)(this.xPositionOnScreen + IClickableMenu.borderWidth + 4)) : ((float)(this.xPositionOnScreen + PondQueryMenu.width / 2) - text_size.X * 0.5f), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				int slots_to_draw = this._pond.maxOccupants.Value;
				float slot_spacing = 13f;
				int x = 0;
				int y = 0;
				for (int i = 0; i < slots_to_draw; i++)
				{
					float y_offset = (float)Math.Sin((double)(this._age * 1f + (float)x * 0.75f + (float)y * 0.25f)) * 2f;
					if (i < this._pond.FishCount)
					{
						this._fishItem.drawInMenu(b, new Vector2((float)(this.xPositionOnScreen + PondQueryMenu.width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(this.yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 1f, 0f, StackDrawType.Hide, Color.White, false);
					}
					else
					{
						this._fishItem.drawInMenu(b, new Vector2((float)(this.xPositionOnScreen + PondQueryMenu.width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(this.yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 0.35f, 0f, StackDrawType.Hide, Color.Black, false);
					}
					x++;
					if (x == 5)
					{
						x = 0;
						y++;
					}
				}
				text_size = Game1.smallFont.MeasureString(displayed_text);
				Utility.drawTextWithShadow(b, displayed_text, Game1.smallFont, new Vector2((float)(this.xPositionOnScreen + PondQueryMenu.width / 2) - text_size.X * 0.5f, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight - (has_unresolved_needs ? 32 : 48)) - text_size.Y), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				if (has_unresolved_needs)
				{
					base.drawHorizontalPartition(b, (int)((float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight) - 48f), false, -1, -1, -1);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 60) + 8f * Game1.dialogueButtonScale / 10f, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 28)), new Rectangle(412, 495, 5, 4), Color.White, 1.5707964f, Vector2.Zero, -1f, false, -1f, -1, -1, 0.35f);
					string bring_text = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequest_Bring");
					text_size = Game1.smallFont.MeasureString(bring_text);
					int left_x = this.xPositionOnScreen + 88;
					float text_x = (float)left_x;
					float icon_x = text_x + text_size.X + 4f;
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
					{
						icon_x = (float)(left_x - 8);
						text_x = (float)(left_x + 76);
					}
					Utility.drawTextWithShadow(b, bring_text, Game1.smallFont, new Vector2(text_x, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 24)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this._pond.neededItem.Value.QualifiedItemId);
					Texture2D texture = dataOrErrorItem.GetTexture();
					Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
					b.Draw(texture, new Vector2(icon_x, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 4)), new Rectangle?(sourceRect), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(texture, new Vector2(icon_x + 4f, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight)), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if (this._pond.neededItemCount.Value > 1)
					{
						Utility.drawTinyDigits(this._pond.neededItemCount.Value, b, new Vector2(icon_x + 48f, (float)(this.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 48)), 3f, 1f, Color.White);
					}
				}
				if (this._pond.goldenAnimalCracker.Value && Game1.objectSpriteSheet_2 != null)
				{
					Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(this.xPositionOnScreen + PondQueryMenu.width) - 105.6f, (float)this.yPositionOnScreen + 224f), new Rectangle(16, 240, 16, 16), Color.White, 0f, Vector2.Zero, 4f, false, 0.89f, -1, -1, 0.35f);
				}
				this.okButton.draw(b);
				this.emptyButton.draw(b);
				this.changeNettingButton.draw(b);
				if (this.confirmingEmpty)
				{
					if (!Game1.options.showClearBackgrounds)
					{
						b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					}
					int padding = 16;
					this._confirmationBoxRectangle.Width = this._confirmationBoxRectangle.Width + padding;
					this._confirmationBoxRectangle.Height = this._confirmationBoxRectangle.Height + padding;
					this._confirmationBoxRectangle.X = this._confirmationBoxRectangle.X - padding / 2;
					this._confirmationBoxRectangle.Y = this._confirmationBoxRectangle.Y - padding / 2;
					Game1.DrawBox(this._confirmationBoxRectangle.X, this._confirmationBoxRectangle.Y, this._confirmationBoxRectangle.Width, this._confirmationBoxRectangle.Height, null);
					this._confirmationBoxRectangle.Width = this._confirmationBoxRectangle.Width - padding;
					this._confirmationBoxRectangle.Height = this._confirmationBoxRectangle.Height - padding;
					this._confirmationBoxRectangle.X = this._confirmationBoxRectangle.X + padding / 2;
					this._confirmationBoxRectangle.Y = this._confirmationBoxRectangle.Y + padding / 2;
					b.DrawString(Game1.smallFont, this._confirmationText, new Vector2((float)this._confirmationBoxRectangle.X, (float)this._confirmationBoxRectangle.Y), Game1.textColor);
					this.yesButton.draw(b);
					this.noButton.draw(b);
				}
				else
				{
					string text = this.hoverText;
					if (text != null && text.Length > 0)
					{
						IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
					}
				}
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001D1F RID: 7455
		public const int region_okButton = 101;

		// Token: 0x04001D20 RID: 7456
		public const int region_emptyButton = 103;

		// Token: 0x04001D21 RID: 7457
		public const int region_noButton = 105;

		// Token: 0x04001D22 RID: 7458
		public const int region_nettingButton = 106;

		// Token: 0x04001D23 RID: 7459
		public new static int width = 384;

		// Token: 0x04001D24 RID: 7460
		public new static int height = 512;

		// Token: 0x04001D25 RID: 7461
		public const int unresolved_needs_extra_height = 116;

		// Token: 0x04001D26 RID: 7462
		protected FishPond _pond;

		// Token: 0x04001D27 RID: 7463
		protected Object _fishItem;

		// Token: 0x04001D28 RID: 7464
		protected string _statusText = "";

		// Token: 0x04001D29 RID: 7465
		public ClickableTextureComponent okButton;

		// Token: 0x04001D2A RID: 7466
		public ClickableTextureComponent emptyButton;

		// Token: 0x04001D2B RID: 7467
		public ClickableTextureComponent yesButton;

		// Token: 0x04001D2C RID: 7468
		public ClickableTextureComponent noButton;

		// Token: 0x04001D2D RID: 7469
		public ClickableTextureComponent changeNettingButton;

		// Token: 0x04001D2E RID: 7470
		private bool confirmingEmpty;

		// Token: 0x04001D2F RID: 7471
		protected Rectangle _confirmationBoxRectangle;

		// Token: 0x04001D30 RID: 7472
		protected string _confirmationText;

		// Token: 0x04001D31 RID: 7473
		protected float _age;

		// Token: 0x04001D32 RID: 7474
		private string hoverText = "";
	}
}
