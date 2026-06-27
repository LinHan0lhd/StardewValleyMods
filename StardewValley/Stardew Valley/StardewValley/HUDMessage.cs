using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley
{
	// Token: 0x020000B4 RID: 180
	public class HUDMessage
	{
		// Token: 0x06000C96 RID: 3222 RVA: 0x0008DF0D File Offset: 0x0008C10D
		public HUDMessage(string message) : this(message, 3500f, false)
		{
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x0008DF1C File Offset: 0x0008C11C
		public HUDMessage(string message, int whatType) : this(message, 5250f, false)
		{
			this.achievement = true;
			this.whatType = whatType;
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x0008DF39 File Offset: 0x0008C139
		public HUDMessage(string message, float timeLeft, bool fadeIn = false)
		{
			this.message = message;
			this.timeLeft = timeLeft;
			if (fadeIn)
			{
				this.transparency = 0f;
			}
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x0008DF6F File Offset: 0x0008C16F
		public static HUDMessage ForItemGained(Item item, int count, string type = null)
		{
			return new HUDMessage(item.DisplayName)
			{
				number = count,
				type = (type ?? item.Name),
				messageSubject = item
			};
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x0008DF9B File Offset: 0x0008C19B
		public static HUDMessage ForCornerTextbox(string message)
		{
			message = Game1.parseText(message, Game1.dialogueFont, 384);
			return new HUDMessage(message)
			{
				noIcon = true,
				timeLeft = 5250f
			};
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x0008DFC7 File Offset: 0x0008C1C7
		public static HUDMessage ForAchievement(string achievementName)
		{
			return new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HUDMessage.cs.3824") + achievementName, 5250f, false)
			{
				achievement = true,
				whatType = 1
			};
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x0008DFF8 File Offset: 0x0008C1F8
		public virtual bool update(GameTime time)
		{
			this.timeLeft -= (float)time.ElapsedGameTime.Milliseconds;
			if (this.timeLeft < 0f)
			{
				this.transparency -= 0.02f;
				if (this.transparency < 0f)
				{
					return true;
				}
			}
			else if (this.transparency < 1f)
			{
				this.transparency = Math.Min(this.transparency + 0.02f, 1f);
			}
			return false;
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x0008E07C File Offset: 0x0008C27C
		public virtual void draw(SpriteBatch b, int i, ref int heightUsed)
		{
			Rectangle tsarea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
			if (this.noIcon)
			{
				int overrideX = tsarea.Left + 16;
				int height = (int)Game1.dialogueFont.MeasureString(this.message).Y + 64;
				int overrideY = ((Game1.uiViewport.Width < 1400) ? -64 : 0) + tsarea.Bottom - height - heightUsed - 64;
				heightUsed += height;
				IClickableMenu.drawHoverText(b, this.message, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, overrideX, overrideY, this.transparency, null, null, null, null, null, null, 1f, -1, -1);
				return;
			}
			int height2 = 112;
			Vector2 itemBoxPosition = new Vector2((float)(tsarea.Left + 16), (float)(tsarea.Bottom - height2 - heightUsed - 64));
			heightUsed += height2;
			if (Game1.isOutdoorMapSmallerThanViewport())
			{
				itemBoxPosition.X = (float)Math.Max(tsarea.Left + 16, -Game1.uiViewport.X + 16);
			}
			if (Game1.uiViewport.Width < 1400)
			{
				itemBoxPosition.Y -= 48f;
			}
			Texture2D mouseCursors = Game1.mouseCursors;
			Vector2 position = itemBoxPosition;
			Object obj = this.messageSubject as Object;
			b.Draw(mouseCursors, position, new Rectangle?((obj != null && obj.sellToStorePrice(-1L) > 500) ? new Rectangle(163, 399, 26, 24) : new Rectangle(293, 360, 26, 24)), Color.White * this.transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			float messageWidth = Game1.smallFont.MeasureString(this.message ?? "").X;
			b.Draw(Game1.mouseCursors, new Vector2(itemBoxPosition.X + 104f, itemBoxPosition.Y), new Rectangle?(new Rectangle(319, 360, 1, 24)), Color.White * this.transparency, 0f, Vector2.Zero, new Vector2(messageWidth, 4f), SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(itemBoxPosition.X + 104f + messageWidth, itemBoxPosition.Y), new Rectangle?(new Rectangle(323, 360, 6, 24)), Color.White * this.transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			itemBoxPosition.X += 16f;
			itemBoxPosition.Y += 16f;
			if (this.messageSubject == null)
			{
				switch (this.whatType)
				{
				case 1:
					b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(294, 392, 16, 16)), Color.White * this.transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(403, 496, 5, 14)), Color.White * this.transparency, 0f, new Vector2(3f, 7f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				case 3:
					b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(268, 470, 16, 16)), Color.White * this.transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				case 4:
					b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(0, 411, 16, 16)), Color.White * this.transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				case 5:
					b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(16, 411, 16, 16)), Color.White * this.transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				case 6:
					b.Draw(Game1.mouseCursors2, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle?(new Rectangle(96, 32, 16, 16)), Color.White * this.transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
					break;
				}
			}
			else
			{
				this.messageSubject.drawInMenu(b, itemBoxPosition, 1f + Math.Max(0f, (this.timeLeft - 3000f) / 900f), this.transparency, 1f, StackDrawType.Hide);
			}
			itemBoxPosition.X += 51f;
			itemBoxPosition.Y += 51f;
			if (this.number > 1)
			{
				Utility.drawTinyDigits(this.number, b, itemBoxPosition, 3f, 1f, Color.White * this.transparency);
			}
			itemBoxPosition.X += 32f;
			itemBoxPosition.Y -= 33f;
			Utility.drawTextWithShadow(b, this.message ?? "", Game1.smallFont, itemBoxPosition, Game1.textColor * this.transparency, 1f, 1f, -1, -1, this.transparency, 3);
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x0008E7D8 File Offset: 0x0008C9D8
		public static void numbersEasterEgg(int number)
		{
			if (number > 100000 && !Game1.player.mailReceived.Contains("numbersEgg1"))
			{
				Game1.player.mailReceived.Add("numbersEgg1");
				Game1.chatBox.addMessage("...", new Color(255, 255, 255));
			}
			if (number > 200000 && !Game1.player.mailReceived.Contains("numbersEgg2"))
			{
				Game1.player.mailReceived.Add("numbersEgg2");
				Game1.chatBox.addMessage("......", new Color(255, 255, 255));
			}
			if (number > 250000 && !Game1.player.mailReceived.Contains("numbersEgg3"))
			{
				Game1.player.mailReceived.Add("numbersEgg3");
				Game1.chatBox.addMessage((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en) ? "Shooting for a million?" : "...........???", new Color(255, 255, 255));
			}
			if (number > 500000 && !Game1.player.mailReceived.Contains("numbersEgg1.5"))
			{
				Game1.player.mailReceived.Add("numbersEgg1.5");
				Game1.chatBox.addMessage(".......................", new Color(255, 255, 255));
			}
			if (number > 1000000 && !Game1.player.mailReceived.Contains("numbersEgg7"))
			{
				Game1.player.mailReceived.Add("numbersEgg7");
				Game1.chatBox.addMessage((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en) ? "[196] Secret Iridium Stackmaster Trophy Achieved [196]" : "[196]", new Color(104, 214, 255));
				Game1.playSound("discoverMineral", null);
				if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
				{
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.chatBox.addMessage("Qi: *slow clap*... Congratulations, kid. Ya did it. Now, on to the next challenge!", new Color(100, 50, 255));
					}, 6000);
				}
			}
		}

		// Token: 0x0400089C RID: 2204
		public const float defaultTime = 3500f;

		// Token: 0x0400089D RID: 2205
		public const int achievement_type = 1;

		// Token: 0x0400089E RID: 2206
		public const int newQuest_type = 2;

		// Token: 0x0400089F RID: 2207
		public const int error_type = 3;

		// Token: 0x040008A0 RID: 2208
		public const int stamina_type = 4;

		// Token: 0x040008A1 RID: 2209
		public const int health_type = 5;

		// Token: 0x040008A2 RID: 2210
		public const int screenshot_type = 6;

		// Token: 0x040008A3 RID: 2211
		public string message;

		// Token: 0x040008A4 RID: 2212
		public string type;

		// Token: 0x040008A5 RID: 2213
		public float timeLeft;

		// Token: 0x040008A6 RID: 2214
		public float transparency = 1f;

		// Token: 0x040008A7 RID: 2215
		public int number = -1;

		// Token: 0x040008A8 RID: 2216
		public int whatType;

		// Token: 0x040008A9 RID: 2217
		public bool achievement;

		// Token: 0x040008AA RID: 2218
		public bool noIcon;

		// Token: 0x040008AB RID: 2219
		public Item messageSubject;
	}
}
