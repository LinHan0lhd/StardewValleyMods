using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x02000294 RID: 660
	public class OptionsInputListener : OptionsElement
	{
		// Token: 0x06002B55 RID: 11093 RVA: 0x0020C3AC File Offset: 0x0020A5AC
		public OptionsInputListener(string label, int whichOption, int slotWidth, int x = -1, int y = -1) : base(label, x, y, slotWidth - x, 44, whichOption)
		{
			this.setbuttonBounds = new Rectangle(slotWidth - 112, y + 12, 84, 44);
			if (whichOption != -1)
			{
				Game1.options.setInputListenerToProperValue(this);
			}
		}

		// Token: 0x06002B56 RID: 11094 RVA: 0x0020C400 File Offset: 0x0020A600
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut && !this.listening && this.setbuttonBounds.Contains(x, y))
			{
				if (this.whichOption == -1)
				{
					Game1.options.setControlsToDefault();
					GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
					if (gameMenu == null)
					{
						return;
					}
					OptionsPage optionsPage = gameMenu.GetCurrentPage() as OptionsPage;
					if (optionsPage == null)
					{
						return;
					}
					using (List<OptionsElement>.Enumerator enumerator = optionsPage.options.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							OptionsElement optionsElement = enumerator.Current;
							OptionsInputListener listener = optionsElement as OptionsInputListener;
							if (listener != null)
							{
								Game1.options.setInputListenerToProperValue(listener);
							}
						}
						return;
					}
				}
				this.listening = true;
				Game1.playSound("breathin", null);
				GameMenu.forcePreventClose = true;
				this.listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11225");
			}
		}

		// Token: 0x06002B57 RID: 11095 RVA: 0x0020C4F4 File Offset: 0x0020A6F4
		public override void receiveKeyPress(Keys key)
		{
			if (!this.greyedOut && this.listening)
			{
				GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
				if (gameMenu != null)
				{
					OptionsPage optionsPage = gameMenu.GetCurrentPage() as OptionsPage;
					if (optionsPage != null)
					{
						optionsPage.lastRebindTick = Game1.ticks;
					}
				}
				if (key == Keys.Escape)
				{
					Game1.playSound("bigDeSelect", null);
					this.listening = false;
					GameMenu.forcePreventClose = false;
					return;
				}
				if (!Game1.options.isKeyInUse(key) || new InputButton(key).ToString().Equals(this.buttonNames[0]))
				{
					Game1.options.changeInputListenerValue(this.whichOption, key);
					this.buttonNames[0] = new InputButton(key).ToString();
					Game1.playSound("coin", null);
					this.listening = false;
					GameMenu.forcePreventClose = false;
					return;
				}
				this.listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11228");
			}
		}

		// Token: 0x06002B58 RID: 11096 RVA: 0x0020C604 File Offset: 0x0020A804
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			if (this.buttonNames.Count > 0 || this.whichOption == -1)
			{
				if (this.whichOption == -1)
				{
					Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(this.bounds.X + slotX), (float)(this.bounds.Y + slotY)), Game1.textColor, 1f, 0.15f, -1, -1, 1f, 3);
				}
				else
				{
					Utility.drawTextWithShadow(b, this.label + ": " + this.buttonNames.Last<string>() + ((this.buttonNames.Count > 1) ? (", " + this.buttonNames[0]) : ""), Game1.dialogueFont, new Vector2((float)(this.bounds.X + slotX), (float)(this.bounds.Y + slotY)), Game1.textColor, 1f, 0.15f, -1, -1, 1f, 3);
				}
			}
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.setbuttonBounds.X + slotX), (float)(this.setbuttonBounds.Y + slotY)), OptionsInputListener.setButtonSource, Color.White, 0f, Vector2.Zero, 4f, false, 0.15f, -1, -1, 0.35f);
			if (this.listening)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0.999f);
				b.DrawString(Game1.dialogueFont, this.listenerMessage, Utility.getTopLeftPositionForCenteringOnScreen(192, 64, 0, 0), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
			}
		}

		// Token: 0x04001CFF RID: 7423
		public List<string> buttonNames = new List<string>();

		// Token: 0x04001D00 RID: 7424
		private string listenerMessage;

		// Token: 0x04001D01 RID: 7425
		private bool listening;

		// Token: 0x04001D02 RID: 7426
		private Rectangle setbuttonBounds;

		// Token: 0x04001D03 RID: 7427
		public static Rectangle setButtonSource = new Rectangle(294, 428, 21, 11);
	}
}
