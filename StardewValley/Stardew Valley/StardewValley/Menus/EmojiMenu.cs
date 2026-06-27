using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200026A RID: 618
	public class EmojiMenu : IClickableMenu
	{
		// Token: 0x06002902 RID: 10498 RVA: 0x001E1A04 File Offset: 0x001DFC04
		public EmojiMenu(ChatBox chatBox, Texture2D emojiTexture, Texture2D chatBoxTexture)
		{
			this.chatBox = chatBox;
			this.chatBoxTexture = chatBoxTexture;
			this.emojiTexture = emojiTexture;
			this.width = 300;
			this.height = 248;
			for (int y = 0; y < 5; y++)
			{
				for (int x = 0; x < 6; x++)
				{
					this.emojiSelectionButtons.Add(new ClickableComponent(new Rectangle(16 + x * 10 * 4, 16 + y * 10 * 4, 36, 36), (x + y * 6).ToString() ?? ""));
				}
			}
			this.upArrow = new ClickableComponent(new Rectangle(256, 16, 32, 20), "");
			this.downArrow = new ClickableComponent(new Rectangle(256, 156, 32, 20), "");
			this.sendArrow = new ClickableComponent(new Rectangle(256, 188, 32, 32), "");
			EmojiMenu.totalEmojis = 197;
			EmojiMenu.totalVisibleEmojis = 196;
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x001E1B20 File Offset: 0x001DFD20
		public void leftClick(int x, int y, ChatBox cb)
		{
			if (this.isWithinBounds(x, y))
			{
				int relativeX = x - this.xPositionOnScreen;
				int relativeY = y - this.yPositionOnScreen;
				if (this.upArrow.containsPoint(relativeX, relativeY))
				{
					this.upArrowPressed(30);
				}
				else if (this.downArrow.containsPoint(relativeX, relativeY))
				{
					this.downArrowPressed(30);
				}
				else if (this.sendArrow.containsPoint(relativeX, relativeY) && cb.chatBox.currentWidth > 0f)
				{
					cb.textBoxEnter(cb.chatBox);
					this.sendArrow.scale = 0.5f;
					Game1.playSound("shwip", null);
				}
				foreach (ClickableComponent c in this.emojiSelectionButtons)
				{
					if (c.containsPoint(relativeX, relativeY))
					{
						int index = this.pageStartIndex + int.Parse(c.name);
						cb.chatBox.receiveEmoji(index);
						Game1.playSound("coin", null);
						break;
					}
				}
			}
		}

		// Token: 0x06002904 RID: 10500 RVA: 0x001E1C54 File Offset: 0x001DFE54
		private void upArrowPressed(int amountToScroll = 30)
		{
			if (this.pageStartIndex != 0)
			{
				Game1.playSound("Cowboy_Footstep", null);
			}
			this.pageStartIndex = Math.Max(0, this.pageStartIndex - amountToScroll);
			this.upArrow.scale = 0.75f;
		}

		// Token: 0x06002905 RID: 10501 RVA: 0x001E1CA4 File Offset: 0x001DFEA4
		private void downArrowPressed(int amountToScroll = 30)
		{
			if (this.pageStartIndex != EmojiMenu.totalVisibleEmojis - 30)
			{
				Game1.playSound("Cowboy_Footstep", null);
			}
			this.pageStartIndex = Math.Min(EmojiMenu.totalVisibleEmojis - 30, this.pageStartIndex + amountToScroll);
			this.downArrow.scale = 0.75f;
		}

		// Token: 0x06002906 RID: 10502 RVA: 0x001E1D00 File Offset: 0x001DFF00
		public override void receiveScrollWheelAction(int direction)
		{
			if (direction < 0)
			{
				this.downArrowPressed(6);
				return;
			}
			if (direction > 0)
			{
				this.upArrowPressed(6);
			}
		}

		// Token: 0x06002907 RID: 10503 RVA: 0x001E1D1C File Offset: 0x001DFF1C
		public override void draw(SpriteBatch b)
		{
			b.Draw(this.chatBoxTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height), new Rectangle?(new Rectangle(0, 56, 300, 244)), Color.White);
			for (int i = 0; i < this.emojiSelectionButtons.Count; i++)
			{
				b.Draw(this.emojiTexture, new Vector2((float)(this.emojiSelectionButtons[i].bounds.X + this.xPositionOnScreen), (float)(this.emojiSelectionButtons[i].bounds.Y + this.yPositionOnScreen)), new Rectangle?(new Rectangle((this.pageStartIndex + i) * 9 % this.emojiTexture.Width, (this.pageStartIndex + i) * 9 / this.emojiTexture.Width * 9, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			if (this.upArrow.scale < 1f)
			{
				this.upArrow.scale += 0.05f;
			}
			if (this.downArrow.scale < 1f)
			{
				this.downArrow.scale += 0.05f;
			}
			if (this.sendArrow.scale < 1f)
			{
				this.sendArrow.scale += 0.05f;
			}
			b.Draw(this.chatBoxTexture, new Vector2((float)(this.upArrow.bounds.X + this.xPositionOnScreen + 16), (float)(this.upArrow.bounds.Y + this.yPositionOnScreen + 10)), new Rectangle?(new Rectangle(156, 300, 32, 20)), Color.White * ((this.pageStartIndex == 0) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), this.upArrow.scale, SpriteEffects.None, 0.9f);
			b.Draw(this.chatBoxTexture, new Vector2((float)(this.downArrow.bounds.X + this.xPositionOnScreen + 16), (float)(this.downArrow.bounds.Y + this.yPositionOnScreen + 10)), new Rectangle?(new Rectangle(192, 300, 32, 20)), Color.White * ((this.pageStartIndex == EmojiMenu.totalVisibleEmojis - 30) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), this.downArrow.scale, SpriteEffects.None, 0.9f);
			b.Draw(this.chatBoxTexture, new Vector2((float)(this.sendArrow.bounds.X + this.xPositionOnScreen + 16), (float)(this.sendArrow.bounds.Y + this.yPositionOnScreen + 10)), new Rectangle?(new Rectangle(116, 304, 28, 28)), Color.White * ((this.chatBox.chatBox.currentWidth > 0f) ? 1f : 0.4f), 0f, new Vector2(14f, 16f), this.sendArrow.scale, SpriteEffects.None, 0.9f);
		}

		// Token: 0x04001AC2 RID: 6850
		public const int EMOJI_SIZE = 9;

		// Token: 0x04001AC3 RID: 6851
		private Texture2D chatBoxTexture;

		// Token: 0x04001AC4 RID: 6852
		private Texture2D emojiTexture;

		// Token: 0x04001AC5 RID: 6853
		private ChatBox chatBox;

		// Token: 0x04001AC6 RID: 6854
		private List<ClickableComponent> emojiSelectionButtons = new List<ClickableComponent>();

		// Token: 0x04001AC7 RID: 6855
		private int pageStartIndex;

		// Token: 0x04001AC8 RID: 6856
		private ClickableComponent upArrow;

		// Token: 0x04001AC9 RID: 6857
		private ClickableComponent downArrow;

		// Token: 0x04001ACA RID: 6858
		private ClickableComponent sendArrow;

		// Token: 0x04001ACB RID: 6859
		public static int totalEmojis;

		// Token: 0x04001ACC RID: 6860
		public static int totalVisibleEmojis;
	}
}
