using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	// Token: 0x02000242 RID: 578
	public class Slots : IMinigame
	{
		// Token: 0x06002688 RID: 9864 RVA: 0x001B455C File Offset: 0x001B275C
		public Slots(int toBet = -1, bool highStakes = false)
		{
			this.coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  ");
			this.currentBet = toBet;
			if (this.currentBet == -1)
			{
				this.currentBet = 10;
			}
			this.slots = new List<float>
			{
				0f,
				0f,
				0f
			};
			this.slotResults = new List<float>
			{
				0f,
				0f,
				0f
			};
			Game1.playSound("newArtifact", null);
			this.setSlotResults(this.slots);
			int buttonTopY = 44;
			this.spinButton10 = this.CreateSpinButton(32, buttonTopY, "Strings\\StringsFromCSFiles:Slots.cs.12117");
			this.spinButton100 = this.CreateSpinButton(37, buttonTopY + 64, "Strings\\StringsFromCSFiles:Slots.cs.12118");
			this.doneButton = this.CreateSpinButton(30, buttonTopY + 128, "Strings\\StringsFromCSFiles:NameSelect.cs.3864");
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				Game1.setMousePosition(this.spinButton10.bounds.Center);
				if (Game1.options.SnappyMenus)
				{
					this.currentlySnappedComponent = this.spinButton10;
				}
			}
		}

		// Token: 0x06002689 RID: 9865 RVA: 0x001B4698 File Offset: 0x001B2898
		private ClickableComponent CreateSpinButton(int baseWidth, int yOffset, string nameTranslationKey)
		{
			int extraButtonWidth = this.GetButtonSizeOffset();
			int width = (baseWidth + extraButtonWidth) * 4;
			Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, width, 52, -16, yOffset);
			return new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, 52), Game1.content.LoadString(nameTranslationKey));
		}

		// Token: 0x0600268A RID: 9866 RVA: 0x001B46EC File Offset: 0x001B28EC
		public void setSlotResults(List<float> toSet)
		{
			double d = Game1.random.NextDouble();
			double modifier = 1.0 + Game1.player.DailyLuck * 2.0 + (double)Game1.player.LuckLevel * 0.08;
			if (d < 0.001 * modifier)
			{
				this.set(toSet, 5);
				this.payoutModifier = 2500f;
				return;
			}
			if (d < 0.0016 * modifier)
			{
				this.set(toSet, 6);
				this.payoutModifier = 1000f;
				return;
			}
			if (d < 0.0025 * modifier)
			{
				this.set(toSet, 7);
				this.payoutModifier = 500f;
				return;
			}
			if (d < 0.005 * modifier)
			{
				this.set(toSet, 4);
				this.payoutModifier = 200f;
				return;
			}
			if (d < 0.007 * modifier)
			{
				this.set(toSet, 3);
				this.payoutModifier = 120f;
				return;
			}
			if (d < 0.01 * modifier)
			{
				this.set(toSet, 2);
				this.payoutModifier = 80f;
				return;
			}
			if (d < 0.02 * modifier)
			{
				this.set(toSet, 1);
				this.payoutModifier = 30f;
				return;
			}
			if (d < 0.12 * modifier)
			{
				int whereToPutNonStar = Game1.random.Next(3);
				for (int i = 0; i < 3; i++)
				{
					toSet[i] = (float)((i == whereToPutNonStar) ? Game1.random.Next(7) : 7);
				}
				this.payoutModifier = 3f;
				return;
			}
			if (d < 0.2 * modifier)
			{
				this.set(toSet, 0);
				this.payoutModifier = 5f;
				return;
			}
			if (d < 0.4 * modifier)
			{
				int whereToPutStar = Game1.random.Next(3);
				for (int j = 0; j < 3; j++)
				{
					toSet[j] = (float)((j == whereToPutStar) ? 7 : Game1.random.Next(7));
				}
				this.payoutModifier = 2f;
				return;
			}
			this.payoutModifier = 0f;
			int[] used = new int[8];
			for (int k = 0; k < 3; k++)
			{
				int next = Game1.random.Next(6);
				while (used[next] > 1)
				{
					next = Game1.random.Next(6);
				}
				toSet[k] = (float)next;
				used[next]++;
			}
		}

		// Token: 0x0600268B RID: 9867 RVA: 0x001B4949 File Offset: 0x001B2B49
		private void set(List<float> toSet, int number)
		{
			toSet[0] = (float)number;
			toSet[1] = (float)number;
			toSet[2] = (float)number;
		}

		// Token: 0x0600268C RID: 9868 RVA: 0x001B4968 File Offset: 0x001B2B68
		public bool tick(GameTime time)
		{
			if (this.spinning && this.endTimer <= 0)
			{
				for (int i = this.slotsFinished; i < this.slots.Count; i++)
				{
					float old = this.slots[i];
					List<float> list = this.slots;
					int index = i;
					list[index] += (float)time.ElapsedGameTime.Milliseconds * 0.008f * (1f - (float)i * 0.05f);
					list = this.slots;
					index = i;
					list[index] %= 8f;
					if (i == 2)
					{
						if (old % (0.25f + (float)this.slotsFinished * 0.5f) > this.slots[i] % (0.25f + (float)this.slotsFinished * 0.5f))
						{
							Game1.playSound("shiny4", null);
						}
						if (old > this.slots[i])
						{
							this.spinsCount++;
						}
					}
					if (this.spinsCount > 0 && i == this.slotsFinished && Math.Abs(this.slots[i] - this.slotResults[i]) <= (float)time.ElapsedGameTime.Milliseconds * 0.008f)
					{
						this.slots[i] = this.slotResults[i];
						this.slotsFinished++;
						this.spinsCount--;
						Game1.playSound("Cowboy_gunshot", null);
					}
				}
				if (this.slotsFinished >= 3)
				{
					this.endTimer = ((this.payoutModifier == 0f) ? 600 : 1000);
				}
			}
			if (this.endTimer > 0)
			{
				this.endTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.endTimer <= 0)
				{
					this.spinning = false;
					this.spinsCount = 0;
					this.slotsFinished = 0;
					if (this.payoutModifier > 0f)
					{
						this.showResult = true;
						Game1.playSound((this.payoutModifier >= 5f) ? ((this.payoutModifier >= 10f) ? "reward" : "money") : "newArtifact", null);
					}
					else
					{
						Game1.playSound("breathout", null);
					}
					Game1.player.clubCoins += (int)((float)this.currentBet * this.payoutModifier);
					if (this.payoutModifier == 2500f)
					{
						Game1.multiplayer.globalChatInfoMessage("Jackpot", new string[]
						{
							Game1.player.Name
						});
					}
				}
			}
			this.spinButton10.scale = ((!this.spinning && this.spinButton10.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			this.spinButton100.scale = ((!this.spinning && this.spinButton100.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			this.doneButton.scale = ((!this.spinning && this.doneButton.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			return false;
		}

		// Token: 0x0600268D RID: 9869 RVA: 0x001B4CF0 File Offset: 0x001B2EF0
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.spinning && Game1.player.clubCoins >= 10 && this.spinButton10.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				this.setSlotResults(this.slotResults);
				this.spinning = true;
				Game1.playSound("bigSelect", null);
				this.currentBet = 10;
				this.slotsFinished = 0;
				this.spinsCount = 0;
				this.showResult = false;
				Game1.player.clubCoins -= 10;
			}
			if (!this.spinning && Game1.player.clubCoins >= 100 && this.spinButton100.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				this.setSlotResults(this.slotResults);
				Game1.playSound("bigSelect", null);
				this.spinning = true;
				this.slotsFinished = 0;
				this.spinsCount = 0;
				this.showResult = false;
				this.currentBet = 100;
				Game1.player.clubCoins -= 100;
			}
			if (!this.spinning && this.doneButton.bounds.Contains(x, y))
			{
				Game1.playSound("bigDeSelect", null);
				Game1.currentMinigame = null;
			}
		}

		// Token: 0x0600268E RID: 9870 RVA: 0x001B4E53 File Offset: 0x001B3053
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x0600268F RID: 9871 RVA: 0x001B4E55 File Offset: 0x001B3055
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002690 RID: 9872 RVA: 0x001B4E57 File Offset: 0x001B3057
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x06002691 RID: 9873 RVA: 0x001B4E59 File Offset: 0x001B3059
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x06002692 RID: 9874 RVA: 0x001B4E5B File Offset: 0x001B305B
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x001B4E68 File Offset: 0x001B3068
		public void receiveKeyPress(Keys k)
		{
			if (!this.spinning && (k.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, k)))
			{
				this.unload();
				Game1.playSound("bigDeSelect", null);
				Game1.currentMinigame = null;
				return;
			}
			if (!this.spinning && this.currentlySnappedComponent != null)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					if (this.currentlySnappedComponent.Equals(this.spinButton10))
					{
						this.currentlySnappedComponent = this.spinButton100;
						Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
						return;
					}
					if (this.currentlySnappedComponent.Equals(this.spinButton100))
					{
						this.currentlySnappedComponent = this.doneButton;
						Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
						return;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					if (this.currentlySnappedComponent.Equals(this.doneButton))
					{
						this.currentlySnappedComponent = this.spinButton100;
						Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
						return;
					}
					if (this.currentlySnappedComponent.Equals(this.spinButton100))
					{
						this.currentlySnappedComponent = this.spinButton10;
						Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
					}
				}
			}
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x001B4FE5 File Offset: 0x001B31E5
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002695 RID: 9877 RVA: 0x001B4FE8 File Offset: 0x001B31E8
		public int getIconIndex(int index)
		{
			switch (index)
			{
			case 0:
				return 24;
			case 1:
				return 186;
			case 2:
				return 138;
			case 3:
				return 392;
			case 4:
				return 254;
			case 5:
				return 434;
			case 6:
				return 72;
			case 7:
				return 638;
			default:
				return 24;
			}
		}

		// Token: 0x06002696 RID: 9878 RVA: 0x001B504C File Offset: 0x001B324C
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Color(38, 0, 7));
			b.Draw(Game1.mouseCursors, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 228, 52, 0, -256), new Rectangle?(new Rectangle(441, 424, 66, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			int minSlotX = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112;
			for (int i = 0; i < 3; i++)
			{
				Vector2 topLeft = new Vector2((float)(minSlotX + i * 104), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128));
				b.Draw(Game1.mouseCursors, topLeft, new Rectangle?(new Rectangle(306, 320, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				float faceValue = (this.slots[i] + 1f) % 8f;
				int previous = this.getIconIndex(((int)faceValue + 8 - 1) % 8);
				int current = this.getIconIndex((previous + 1) % 8);
				b.Draw(Game1.objectSpriteSheet, topLeft - new Vector2(0f, -64f * (faceValue % 1f)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, previous, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, topLeft - new Vector2(0f, 64f - 64f * (faceValue % 1f)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, current, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 132 + i * 26 * 4), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 192)), new Rectangle?(new Rectangle(415, 385, 26, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			int slotMiddle = minSlotX + 136;
			this.spinButton10.bounds.X = slotMiddle - this.spinButton10.bounds.Width / 2;
			this.spinButton100.bounds.X = slotMiddle - this.spinButton100.bounds.Width / 2;
			this.doneButton.bounds.X = slotMiddle - this.doneButton.bounds.Width / 2;
			int extraButtonWidth = this.GetButtonSizeOffset();
			b.Draw(Game1.mouseCursors, new Vector2((float)this.spinButton10.bounds.X, (float)this.spinButton10.bounds.Y), new Rectangle?(new Rectangle(441, 385, 32 + extraButtonWidth, 13)), Color.White * ((!this.spinning && Game1.player.clubCoins >= 10) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.spinButton10.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2((float)this.spinButton100.bounds.X, (float)this.spinButton100.bounds.Y), new Rectangle?(new Rectangle(441, 398, 37 + extraButtonWidth, 13)), Color.White * ((!this.spinning && Game1.player.clubCoins >= 100) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.spinButton100.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2((float)this.doneButton.bounds.X, (float)this.doneButton.bounds.Y), new Rectangle?(new Rectangle(441, 411, 30 + extraButtonWidth, 13)), Color.White * ((!this.spinning) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.doneButton.scale, SpriteEffects.None, 0.99f);
			SpriteText.drawStringWithScrollBackground(b, this.coinBuffer + Game1.player.clubCoins.ToString(), Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376 + 4), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120 + 4)), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
			if (this.showResult)
			{
				SpriteText.drawString(b, "+" + (this.payoutModifier * (float)this.currentBet).ToString(), Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 372, this.spinButton10.bounds.Y - 64 + 8, 9999, -1, 9999, 1f, 1f, false, -1, "", new Color?(SpriteText.color_White), SpriteText.ScrollTextAlignment.Left);
			}
			Vector2 basePos = new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 + 200), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 352));
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), (int)basePos.X, (int)basePos.Y, 384, 704, Color.White, 4f, true, -1f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 8f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x2", (int)basePos.X + 192 + 16, (int)basePos.Y + 24, 9999, -1, 99999, 1f, 0.88f, false, -1, "", new Color?(SpriteText.color_White), SpriteText.ScrollTextAlignment.Left);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 76f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, 76f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x3", (int)basePos.X + 192 + 16, (int)basePos.Y + 68 + 24, 9999, -1, 99999, 1f, 0.88f, false, -1, "", new Color?(SpriteText.color_White), SpriteText.ScrollTextAlignment.Left);
			for (int j = 0; j < 8; j++)
			{
				int which = j;
				if (j != 5)
				{
					if (j == 7)
					{
						which = 5;
					}
				}
				else
				{
					which = 7;
				}
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, (float)(8 + (j + 2) * 68)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, (float)(8 + (j + 2) * 68)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(144f, (float)(8 + (j + 2) * 68)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				int payout = 0;
				switch (j)
				{
				case 0:
					payout = 5;
					break;
				case 1:
					payout = 30;
					break;
				case 2:
					payout = 80;
					break;
				case 3:
					payout = 120;
					break;
				case 4:
					payout = 200;
					break;
				case 5:
					payout = 500;
					break;
				case 6:
					payout = 1000;
					break;
				case 7:
					payout = 2500;
					break;
				}
				SpriteText.drawString(b, "x" + payout.ToString(), (int)basePos.X + 192 + 16, (int)basePos.Y + (j + 2) * 68 + 24, 9999, -1, 99999, 1f, 0.88f, false, -1, "", new Color?(SpriteText.color_White), SpriteText.ScrollTextAlignment.Left);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640, (int)basePos.Y, 1024, 704, Color.Red, 4f, false, -1f);
			for (int k = 1; k < 8; k++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640 - 4 * k, (int)basePos.Y - 4 * k, 1024 + 8 * k, 704 + 8 * k, Color.Red * (1f - (float)k * 0.15f), 4f, false, -1f);
			}
			for (int l = 0; l < 17; l++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(147, 472, 3, 3), (int)basePos.X - 640 + 8, (int)basePos.Y + l * 4 * 3 + 12, (int)(608f - (float)(l * 64) * 1.2f + (float)(l * l * 4) * 0.7f), 4, new Color(l * 25, (l > 8) ? (l * 10) : 0, 255 - l * 25), 4f, false, -1f);
			}
			if (Game1.IsMultiplayer)
			{
				Utility.drawTextWithColoredShadow(b, Game1.getTimeOfDayString(Game1.timeOfDay), Game1.dialogueFont, new Vector2(basePos.X + 416f - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).X, basePos.Y - 72f), Color.Purple, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
			}
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		// Token: 0x06002697 RID: 9879 RVA: 0x001B5D60 File Offset: 0x001B3F60
		public void changeScreenSize()
		{
		}

		// Token: 0x06002698 RID: 9880 RVA: 0x001B5D62 File Offset: 0x001B3F62
		public void unload()
		{
		}

		// Token: 0x06002699 RID: 9881 RVA: 0x001B5D64 File Offset: 0x001B3F64
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x0600269A RID: 9882 RVA: 0x001B5D66 File Offset: 0x001B3F66
		public string minigameId()
		{
			return "Slots";
		}

		// Token: 0x0600269B RID: 9883 RVA: 0x001B5D6D File Offset: 0x001B3F6D
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x0600269C RID: 9884 RVA: 0x001B5D70 File Offset: 0x001B3F70
		public bool forceQuit()
		{
			if (this.spinning)
			{
				Game1.player.clubCoins += this.currentBet;
			}
			this.unload();
			return true;
		}

		// Token: 0x0600269D RID: 9885 RVA: 0x001B5D98 File Offset: 0x001B3F98
		public int GetButtonSizeOffset()
		{
			switch (Game1.content.GetCurrentLanguage())
			{
			case LocalizedContentManager.LanguageCode.ru:
				return 9;
			case LocalizedContentManager.LanguageCode.pt:
				return 10;
			case LocalizedContentManager.LanguageCode.de:
				return 3;
			case LocalizedContentManager.LanguageCode.fr:
				return 6;
			case LocalizedContentManager.LanguageCode.it:
				return 2;
			case LocalizedContentManager.LanguageCode.hu:
				return 4;
			}
			return 0;
		}

		// Token: 0x040017EA RID: 6122
		public const float slotTurnRate = 0.008f;

		// Token: 0x040017EB RID: 6123
		public const int numberOfIcons = 8;

		// Token: 0x040017EC RID: 6124
		public const int defaultBet = 10;

		// Token: 0x040017ED RID: 6125
		private string coinBuffer;

		// Token: 0x040017EE RID: 6126
		private List<float> slots;

		// Token: 0x040017EF RID: 6127
		private List<float> slotResults;

		// Token: 0x040017F0 RID: 6128
		private ClickableComponent spinButton10;

		// Token: 0x040017F1 RID: 6129
		private ClickableComponent spinButton100;

		// Token: 0x040017F2 RID: 6130
		private ClickableComponent doneButton;

		// Token: 0x040017F3 RID: 6131
		public bool spinning;

		// Token: 0x040017F4 RID: 6132
		public bool showResult;

		// Token: 0x040017F5 RID: 6133
		public float payoutModifier;

		// Token: 0x040017F6 RID: 6134
		public int currentBet;

		// Token: 0x040017F7 RID: 6135
		public int spinsCount;

		// Token: 0x040017F8 RID: 6136
		public int slotsFinished;

		// Token: 0x040017F9 RID: 6137
		public int endTimer;

		// Token: 0x040017FA RID: 6138
		public ClickableComponent currentlySnappedComponent;
	}
}
