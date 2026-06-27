using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Rewards;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x020002AC RID: 684
	public class SpecialOrdersBoard : IClickableMenu
	{
		// Token: 0x06002CB0 RID: 11440 RVA: 0x00228914 File Offset: 0x00226B14
		public SpecialOrdersBoard(string board_type = "") : base(0, 0, 0, 0, true)
		{
			SpecialOrder.UpdateAvailableSpecialOrders(board_type, false);
			this.boardType = board_type;
			if (this.boardType == "Qi")
			{
				this.billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			else
			{
				this.billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			this.width = 1352;
			this.height = 792;
			Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
			this.xPositionOnScreen = (int)center.X;
			this.yPositionOnScreen = (int)center.Y;
			this.acceptLeftQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 4 - 128, this.yPositionOnScreen + this.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 0,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.acceptRightQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width * 3 / 4 - 128, this.yPositionOnScreen + this.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 1,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			this.leftOrder = Game1.player.team.GetAvailableSpecialOrder(0, this.GetOrderType());
			this.rightOrder = Game1.player.team.GetAvailableSpecialOrder(1, this.GetOrderType());
			this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 20, this.yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
			Game1.playSound("bigSelect", null);
			this.UpdateButtons();
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002CB1 RID: 11441 RVA: 0x00228D40 File Offset: 0x00226F40
		public virtual void UpdateButtons()
		{
			if (this.leftOrder == null)
			{
				this.acceptLeftQuestButton.visible = false;
			}
			if (this.rightOrder == null)
			{
				this.acceptRightQuestButton.visible = false;
			}
			if (Game1.player.team.acceptedSpecialOrderTypes.Contains(this.GetOrderType()))
			{
				this.acceptLeftQuestButton.visible = false;
				this.acceptRightQuestButton.visible = false;
			}
		}

		// Token: 0x06002CB2 RID: 11442 RVA: 0x00228DA9 File Offset: 0x00226FA9
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002CB3 RID: 11443 RVA: 0x00228DBE File Offset: 0x00226FBE
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Game1.activeClickableMenu = new SpecialOrdersBoard(this.boardType);
		}

		// Token: 0x06002CB4 RID: 11444 RVA: 0x00228DD8 File Offset: 0x00226FD8
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.playSound("bigDeSelect", null);
			base.exitThisMenu(true);
		}

		// Token: 0x06002CB5 RID: 11445 RVA: 0x00228E00 File Offset: 0x00227000
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.acceptLeftQuestButton.visible && this.acceptLeftQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact", null);
				if (this.leftOrder != null)
				{
					Game1.player.team.acceptedSpecialOrderTypes.Add(this.GetOrderType());
					SpecialOrder order = this.leftOrder;
					Game1.player.team.AddSpecialOrder(order.questKey.Value, new int?(order.generationSeed.Value), false);
					Game1.multiplayer.globalChatInfoMessage("AcceptedSpecialOrder", new string[]
					{
						Game1.player.Name,
						TokenStringBuilder.SpecialOrderName(order.questKey.Value)
					});
					this.UpdateButtons();
					return;
				}
			}
			else if (this.acceptRightQuestButton.visible && this.acceptRightQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact", null);
				if (this.rightOrder != null)
				{
					Game1.player.team.acceptedSpecialOrderTypes.Add(this.GetOrderType());
					SpecialOrder order2 = this.rightOrder;
					Game1.player.team.AddSpecialOrder(order2.questKey.Value, new int?(order2.generationSeed.Value), false);
					Game1.multiplayer.globalChatInfoMessage("AcceptedSpecialOrder", new string[]
					{
						Game1.player.Name,
						TokenStringBuilder.SpecialOrderName(order2.questKey.Value)
					});
					this.UpdateButtons();
				}
			}
		}

		// Token: 0x06002CB6 RID: 11446 RVA: 0x00228FA9 File Offset: 0x002271A9
		public string GetOrderType()
		{
			return this.boardType;
		}

		// Token: 0x06002CB7 RID: 11447 RVA: 0x00228FB4 File Offset: 0x002271B4
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (Game1.questOfTheDay != null && !Game1.questOfTheDay.accepted.Value)
			{
				float oldScale = this.acceptLeftQuestButton.scale;
				this.acceptLeftQuestButton.scale = (this.acceptLeftQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (this.acceptLeftQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
				oldScale = this.acceptRightQuestButton.scale;
				this.acceptRightQuestButton.scale = (this.acceptRightQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (this.acceptRightQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
			}
		}

		// Token: 0x06002CB8 RID: 11448 RVA: 0x0022909C File Offset: 0x0022729C
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			b.Draw(this.billboardTexture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(0, (this.boardType == "Qi") ? 198 : 0, 338, 198)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (this.leftOrder != null && this.leftOrder.IsIslandOrder())
			{
				b.Draw(this.billboardTexture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(338, 0, 169, 198)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (this.rightOrder != null && this.rightOrder.IsIslandOrder())
			{
				b.Draw(this.billboardTexture, new Vector2((float)(this.xPositionOnScreen + 676), (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(507, 0, 169, 198)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (!Game1.player.team.acceptedSpecialOrderTypes.Contains(this.GetOrderType()))
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:ChooseOne"), this.xPositionOnScreen + this.width / 2, Math.Max(10, this.yPositionOnScreen - 70), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\UI:ChooseOne") + "W", 999999), 1f, null, 0, 0.88f, false);
			}
			if (this.leftOrder != null)
			{
				SpecialOrder order = this.leftOrder;
				this.DrawQuestDetails(b, order, this.xPositionOnScreen + 64 + 32);
			}
			if (this.rightOrder != null)
			{
				SpecialOrder order2 = this.rightOrder;
				this.DrawQuestDetails(b, order2, this.xPositionOnScreen + 704 + 32);
			}
			if (this.acceptLeftQuestButton.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptLeftQuestButton.bounds.X, this.acceptLeftQuestButton.bounds.Y, this.acceptLeftQuestButton.bounds.Width, this.acceptLeftQuestButton.bounds.Height, (this.acceptLeftQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptLeftQuestButton.scale, true, -1f);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptLeftQuestButton.bounds.X + 12), (float)(this.acceptLeftQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			if (this.acceptRightQuestButton.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptRightQuestButton.bounds.X, this.acceptRightQuestButton.bounds.Y, this.acceptRightQuestButton.bounds.Width, this.acceptRightQuestButton.bounds.Height, (this.acceptRightQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptRightQuestButton.scale, true, -1f);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptRightQuestButton.bounds.X + 12), (float)(this.acceptRightQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (!Game1.options.SnappyMenus || this.acceptLeftQuestButton.visible || this.acceptRightQuestButton.visible)
			{
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x06002CB9 RID: 11449 RVA: 0x00229550 File Offset: 0x00227750
		public KeyValuePair<Texture2D, Rectangle>? GetPortraitForRequester(string requester_name)
		{
			if (requester_name == null)
			{
				return null;
			}
			for (int i = 0; i < this.emojiIndices.Length; i++)
			{
				if (this.emojiIndices[i] == requester_name)
				{
					return new KeyValuePair<Texture2D, Rectangle>?(new KeyValuePair<Texture2D, Rectangle>(ChatBox.emojiTexture, new Rectangle(i % 14 * 9, 99 + i / 14 * 9, 9, 9)));
				}
			}
			return null;
		}

		// Token: 0x06002CBA RID: 11450 RVA: 0x002295C0 File Offset: 0x002277C0
		public void DrawQuestDetails(SpriteBatch b, SpecialOrder order, int x)
		{
			bool dehighlight = false;
			bool found_match = false;
			foreach (SpecialOrder active_order in Game1.player.team.specialOrders)
			{
				if (active_order.questState.Value == SpecialOrderStatus.InProgress)
				{
					foreach (SpecialOrder available_order in Game1.player.team.availableSpecialOrders)
					{
						if (!(available_order.orderType.Value != this.GetOrderType()) && active_order.questKey.Value == available_order.questKey.Value)
						{
							if (order.questKey.Value != active_order.questKey.Value)
							{
								dehighlight = true;
							}
							found_match = true;
							break;
						}
					}
					if (found_match)
					{
						break;
					}
				}
			}
			if (!found_match && Game1.player.team.acceptedSpecialOrderTypes.Contains(this.GetOrderType()))
			{
				dehighlight = true;
			}
			SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
			Color font_color = Game1.textColor;
			float shadow_intensity = 0.5f;
			float graphic_alpha = 1f;
			if (dehighlight)
			{
				font_color = Game1.textColor * 0.25f;
				shadow_intensity = 0f;
				graphic_alpha = 0.25f;
			}
			if (this.boardType == "Qi")
			{
				font_color = Color.White;
				shadow_intensity = 0f;
				if (dehighlight)
				{
					font_color = Color.White * 0.25f;
					graphic_alpha = 0.25f;
				}
			}
			int header_y = this.yPositionOnScreen + 128;
			string order_name = order.GetName();
			KeyValuePair<Texture2D, Rectangle>? drawn_portrait = this.GetPortraitForRequester(order.requester.Value);
			if (drawn_portrait != null)
			{
				Utility.drawWithShadow(b, drawn_portrait.Value.Key, new Vector2((float)x, (float)header_y), drawn_portrait.Value.Value, Color.White * graphic_alpha, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, shadow_intensity * 0.6f);
			}
			Utility.drawTextWithShadow(b, order_name, font, new Vector2((float)(x + 256) - font.MeasureString(order_name).X / 2f, (float)header_y), font_color, 1f, -1f, -1, -1, shadow_intensity, 3);
			if (this.boardType == "" && Game1.player.team.completedSpecialOrders.Contains(order.questKey.Value))
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)x, (float)this.yPositionOnScreen + 576f + 32f + 8f), new Rectangle?(new Rectangle(404, 213, 11, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			string raw_description = order.GetDescription();
			string description = Game1.parseText(raw_description, font, 512);
			float height = font.MeasureString(description).Y;
			float scale = 1f;
			float max_height = 400f;
			while (height > max_height && scale > 0.25f)
			{
				scale -= 0.05f;
				description = Game1.parseText(raw_description, font, (int)(512f / scale));
				height = font.MeasureString(description).Y;
			}
			Utility.drawTextWithShadow(b, description, font, new Vector2((float)x, (float)(this.yPositionOnScreen + 192)), font_color, scale, -1f, -1, -1, shadow_intensity, 3);
			if (!dehighlight)
			{
				int days_left = order.GetDaysLeft();
				int due_date_y_position = this.yPositionOnScreen + 576;
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)x, (float)due_date_y_position), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, shadow_intensity * 0.6f);
				Utility.drawTextWithShadow(b, Game1.parseText((days_left > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", days_left) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", days_left), Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)(x + 48), (float)due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity, 3);
				if (this.boardType == "Qi")
				{
					int reward = -1;
					GemsReward gems = null;
					foreach (OrderReward orderReward in order.rewards)
					{
						GemsReward gemsReward = orderReward as GemsReward;
						if (gemsReward != null)
						{
							gems = gemsReward;
							break;
						}
					}
					if (gems != null)
					{
						reward = gems.amount.Value;
					}
					if (reward != -1)
					{
						Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(reward.ToString() ?? "").X - 12f - 60f, (float)(due_date_y_position - 8)), new Rectangle(288, 561, 15, 15), Color.White, 0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, shadow_intensity * 0.6f);
						Utility.drawTextWithShadow(b, Game1.parseText(reward.ToString() ?? "", Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(reward.ToString() ?? "").X - 4f, (float)due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity, 3);
						Utility.drawTextWithShadow(b, Game1.parseText(Utility.loadStringShort("StringsFromCSFiles", "QuestLog.cs.11376"), Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(Utility.loadStringShort("StringsFromCSFiles", "QuestLog.cs.11376")).X + 8f, (float)(due_date_y_position - 60)), font_color * 0.6f, 1f, -1f, -1, -1, shadow_intensity, 3);
						return;
					}
				}
				else
				{
					Object o = null;
					foreach (OrderReward orderReward2 in order.rewards)
					{
						ObjectReward objectReward = orderReward2 as ObjectReward;
						if (objectReward != null)
						{
							o = objectReward.objectInstance;
							break;
						}
					}
					if (o != null)
					{
						Utility.drawWithShadow(b, ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).GetTexture(), new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(o.Stack.ToString() ?? "").X - 12f - 60f, (float)(due_date_y_position - 8)), ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).GetSourceRect(0, null), Color.White, 0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, shadow_intensity * 0.6f);
						Utility.drawTextWithShadow(b, Game1.parseText(o.Stack.ToString() ?? "", Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(o.Stack.ToString() ?? "").X - 4f, (float)due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity, 3);
						Utility.drawTextWithShadow(b, Game1.parseText(ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).DisplayName, Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).DisplayName).X + 8f, (float)(due_date_y_position - 60)), font_color * 0.6f, 1f, -1f, -1, -1, shadow_intensity, 3);
					}
				}
			}
		}

		// Token: 0x04001E6B RID: 7787
		private Texture2D billboardTexture;

		// Token: 0x04001E6C RID: 7788
		public const int basewidth = 338;

		// Token: 0x04001E6D RID: 7789
		public const int baseheight = 198;

		// Token: 0x04001E6E RID: 7790
		public ClickableComponent acceptLeftQuestButton;

		// Token: 0x04001E6F RID: 7791
		public ClickableComponent acceptRightQuestButton;

		// Token: 0x04001E70 RID: 7792
		public string boardType = "";

		// Token: 0x04001E71 RID: 7793
		public SpecialOrder leftOrder;

		// Token: 0x04001E72 RID: 7794
		public SpecialOrder rightOrder;

		// Token: 0x04001E73 RID: 7795
		public string[] emojiIndices = new string[]
		{
			"Abigail",
			"Penny",
			"Maru",
			"Leah",
			"Haley",
			"Emily",
			"Alex",
			"Shane",
			"Sebastian",
			"Sam",
			"Harvey",
			"Elliott",
			"Sandy",
			"Evelyn",
			"Marnie",
			"Caroline",
			"Robin",
			"Pierre",
			"Pam",
			"Jodi",
			"Lewis",
			"Linus",
			"Marlon",
			"Willy",
			"Wizard",
			"Morris",
			"Jas",
			"Vincent",
			"Krobus",
			"Dwarf",
			"Gus",
			"Gunther",
			"George",
			"Demetrius",
			"Clint",
			"Baby",
			"Baby",
			"Bear"
		};
	}
}
