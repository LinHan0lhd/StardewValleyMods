using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200025A RID: 602
	public class ChatTextBox : TextBox
	{
		// Token: 0x06002811 RID: 10257 RVA: 0x001D2084 File Offset: 0x001D0284
		public ChatTextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor) : base(textBoxTexture, caretTexture, font, textColor)
		{
		}

		// Token: 0x06002812 RID: 10258 RVA: 0x001D209C File Offset: 0x001D029C
		public void reset()
		{
			this.currentWidth = 0f;
			this.finalText.Clear();
		}

		// Token: 0x06002813 RID: 10259 RVA: 0x001D20B4 File Offset: 0x001D02B4
		public void setText(string text)
		{
			this.reset();
			this.RecieveTextInput(text);
		}

		// Token: 0x06002814 RID: 10260 RVA: 0x001D20C4 File Offset: 0x001D02C4
		public override void RecieveTextInput(string text)
		{
			if (this.finalText.Count == 0)
			{
				this.finalText.Add(new ChatSnippet("", LocalizedContentManager.CurrentLanguageCode));
			}
			if (this.currentWidth + ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(text).X >= (float)(base.Width - 16))
			{
				return;
			}
			ChatSnippet lastSnippet = this.finalText.Last<ChatSnippet>();
			if (lastSnippet.message != null)
			{
				ChatSnippet chatSnippet = lastSnippet;
				chatSnippet.message += text;
			}
			else
			{
				this.finalText.Add(new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
			}
			this.finalText.Last<ChatSnippet>().message = Utility.FilterDirtyWordsIfStrictPlatform(this.finalText.Last<ChatSnippet>().message);
			this.updateWidth();
		}

		// Token: 0x06002815 RID: 10261 RVA: 0x001D218A File Offset: 0x001D038A
		public override void RecieveTextInput(char inputChar)
		{
			this.RecieveTextInput(inputChar.ToString() ?? "");
		}

		// Token: 0x06002816 RID: 10262 RVA: 0x001D21A2 File Offset: 0x001D03A2
		public override void RecieveCommandInput(char command)
		{
			if (base.Selected && command == '\b')
			{
				this.backspace();
				return;
			}
			base.RecieveCommandInput(command);
		}

		// Token: 0x06002817 RID: 10263 RVA: 0x001D21C0 File Offset: 0x001D03C0
		public void backspace()
		{
			if (this.finalText.Count > 0)
			{
				ChatSnippet lastSnippet = this.finalText.Last<ChatSnippet>();
				if (lastSnippet.message != null)
				{
					if (lastSnippet.message.Length > 1)
					{
						lastSnippet.message = lastSnippet.message.Remove(lastSnippet.message.Length - 1);
					}
					else
					{
						this.finalText.RemoveAt(this.finalText.Count - 1);
					}
				}
				else if (lastSnippet.emojiIndex != -1)
				{
					this.finalText.RemoveAt(this.finalText.Count - 1);
				}
			}
			this.updateWidth();
		}

		// Token: 0x06002818 RID: 10264 RVA: 0x001D225E File Offset: 0x001D045E
		public void receiveEmoji(int emoji)
		{
			if (this.currentWidth + 40f > (float)(base.Width - 16))
			{
				return;
			}
			this.finalText.Add(new ChatSnippet(emoji));
			this.updateWidth();
		}

		// Token: 0x06002819 RID: 10265 RVA: 0x001D2290 File Offset: 0x001D0490
		public void updateWidth()
		{
			this.currentWidth = 0f;
			foreach (ChatSnippet cs in this.finalText)
			{
				if (cs.message != null)
				{
					cs.myLength = ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(cs.message).X;
				}
				this.currentWidth += cs.myLength;
			}
		}

		// Token: 0x0600281A RID: 10266 RVA: 0x001D2324 File Offset: 0x001D0524
		public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
		{
			bool flag = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
			if (this._textBoxTexture != null)
			{
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(base.X, base.Y, 16, base.Height), new Rectangle?(new Rectangle(0, 0, 16, base.Height)), Color.White);
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(base.X + 16, base.Y, base.Width - 32, base.Height), new Rectangle?(new Rectangle(16, 0, 4, base.Height)), Color.White);
				spriteBatch.Draw(this._textBoxTexture, new Rectangle(base.X + base.Width - 16, base.Y, 16, base.Height), new Rectangle?(new Rectangle(this._textBoxTexture.Bounds.Width - 16, 0, 16, base.Height)), Color.White);
			}
			else
			{
				Game1.drawDialogueBox(base.X - 32, base.Y - 112 + 10, base.Width + 80, base.Height, false, true, null, false, true, -1, -1, -1);
			}
			if (flag && base.Selected)
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(base.X + 16 + (int)this.currentWidth - 2, base.Y + 8, 4, 32), this._textColor);
			}
			float xPositionSoFar = 0f;
			for (int i = 0; i < this.finalText.Count; i++)
			{
				if (this.finalText[i].emojiIndex != -1)
				{
					spriteBatch.Draw(ChatBox.emojiTexture, new Vector2((float)base.X + xPositionSoFar + 12f, (float)(base.Y + 12)), new Rectangle?(new Rectangle(this.finalText[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, this.finalText[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				else if (this.finalText[i].message != null)
				{
					spriteBatch.DrawString(ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode), this.finalText[i].message, new Vector2((float)base.X + xPositionSoFar + 12f, (float)(base.Y + 12)), ChatMessage.getColorFromName(Game1.player.defaultChatColor), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
				xPositionSoFar += this.finalText[i].myLength;
			}
		}

		// Token: 0x040019B6 RID: 6582
		public IClickableMenu parentMenu;

		// Token: 0x040019B7 RID: 6583
		public List<ChatSnippet> finalText = new List<ChatSnippet>();

		// Token: 0x040019B8 RID: 6584
		public float currentWidth;
	}
}
