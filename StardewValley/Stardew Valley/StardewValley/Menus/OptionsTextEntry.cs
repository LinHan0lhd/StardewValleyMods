using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000295 RID: 661
	public class OptionsTextEntry : OptionsElement
	{
		// Token: 0x06002B5A RID: 11098 RVA: 0x0020C810 File Offset: 0x0020AA10
		public OptionsTextEntry(string label, int whichOption, int x = -1, int y = -1) : base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
		{
			this.textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
			this.textBox.Width = this.bounds.Width;
		}

		// Token: 0x06002B5B RID: 11099 RVA: 0x0020C880 File Offset: 0x0020AA80
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			this.textBox.X = slotX + this.bounds.Left - 8;
			this.textBox.Y = slotY + this.bounds.Top;
			this.textBox.Draw(b, true);
			base.draw(b, slotX, slotY, context);
		}

		// Token: 0x06002B5C RID: 11100 RVA: 0x0020C8D7 File Offset: 0x0020AAD7
		public override void receiveLeftClick(int x, int y)
		{
			this.textBox.SelectMe();
			this.textBox.Update();
		}

		// Token: 0x04001D04 RID: 7428
		public const int pixelsHigh = 11;

		// Token: 0x04001D05 RID: 7429
		public TextBox textBox;
	}
}
