using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000254 RID: 596
	public class ButtonTutorialMenu : IClickableMenu
	{
		// Token: 0x06002790 RID: 10128 RVA: 0x001C3AF4 File Offset: 0x001C1CF4
		public ButtonTutorialMenu(int which) : base(-168, Game1.uiViewport.Height / 2 - 218, 168, 436, false)
		{
			this.which = which;
			ButtonTutorialMenu.current++;
			this.myID = ButtonTutorialMenu.current;
		}

		// Token: 0x06002791 RID: 10129 RVA: 0x001C3B54 File Offset: 0x001C1D54
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.myID != ButtonTutorialMenu.current)
			{
				this.destroy = true;
			}
			if (this.xPositionOnScreen < 0 && this.timerToclose > 0)
			{
				this.xPositionOnScreen += (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
				if (this.xPositionOnScreen >= 0)
				{
					this.xPositionOnScreen = 0;
					return;
				}
			}
			else
			{
				this.timerToclose -= time.ElapsedGameTime.Milliseconds;
				if (this.timerToclose <= 0)
				{
					if (this.xPositionOnScreen >= -232)
					{
						this.xPositionOnScreen -= (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
						return;
					}
					this.destroy = true;
				}
			}
		}

		// Token: 0x06002792 RID: 10130 RVA: 0x001C3C20 File Offset: 0x001C1E20
		public override void draw(SpriteBatch b)
		{
			if (this.destroy)
			{
				return;
			}
			if (!Game1.options.gamepadControls)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(275 + this.which * 42, 0, 42, 109)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
				return;
			}
			b.Draw(Game1.controllerMaps, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(Utility.controllerMapSourceRect(new Rectangle(512 + this.which * 42 * 2, 0, 84, 218))), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.82f);
		}

		// Token: 0x040018DB RID: 6363
		public const int move_run_check = 0;

		// Token: 0x040018DC RID: 6364
		public const int useTool_menu = 1;

		// Token: 0x040018DD RID: 6365
		public const float movementSpeed = 0.2f;

		// Token: 0x040018DE RID: 6366
		public new const int width = 42;

		// Token: 0x040018DF RID: 6367
		public new const int height = 109;

		// Token: 0x040018E0 RID: 6368
		private int timerToclose = 15000;

		// Token: 0x040018E1 RID: 6369
		private int which;

		// Token: 0x040018E2 RID: 6370
		private static int current;

		// Token: 0x040018E3 RID: 6371
		private int myID;
	}
}
