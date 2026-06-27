using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200025D RID: 605
	public class ClickableAnimatedComponent : ClickableComponent
	{
		// Token: 0x0600282B RID: 10283 RVA: 0x001D3F04 File Offset: 0x001D2104
		public ClickableAnimatedComponent(Rectangle bounds, string name, string hoverText, TemporaryAnimatedSprite sprite, bool drawLabel) : base(bounds, name)
		{
			this.sprite = sprite;
			this.sprite.position = new Vector2((float)bounds.X, (float)bounds.Y);
			this.baseScale = sprite.scale;
			this.hoverText = hoverText;
			this.drawLabel = drawLabel;
		}

		// Token: 0x0600282C RID: 10284 RVA: 0x001D3F66 File Offset: 0x001D2166
		public ClickableAnimatedComponent(Rectangle bounds, string name, string hoverText, TemporaryAnimatedSprite sprite) : this(bounds, name, hoverText, sprite, true)
		{
		}

		// Token: 0x0600282D RID: 10285 RVA: 0x001D3F74 File Offset: 0x001D2174
		public void update(GameTime time)
		{
			this.sprite.update(time);
		}

		// Token: 0x0600282E RID: 10286 RVA: 0x001D3F84 File Offset: 0x001D2184
		public string tryHover(int x, int y)
		{
			if (this.bounds.Contains(x, y))
			{
				this.sprite.scale = Math.Min(this.sprite.scale + 0.02f, this.baseScale + 0.1f);
				return this.hoverText;
			}
			this.sprite.scale = Math.Max(this.sprite.scale - 0.02f, this.baseScale);
			return null;
		}

		// Token: 0x0600282F RID: 10287 RVA: 0x001D3FFC File Offset: 0x001D21FC
		public void draw(SpriteBatch b)
		{
			this.sprite.draw(b, true, 0, 0, 1f);
		}

		// Token: 0x040019D9 RID: 6617
		public TemporaryAnimatedSprite sprite;

		// Token: 0x040019DA RID: 6618
		public Rectangle sourceRect;

		// Token: 0x040019DB RID: 6619
		public float baseScale;

		// Token: 0x040019DC RID: 6620
		public string hoverText = "";

		// Token: 0x040019DD RID: 6621
		private bool drawLabel;
	}
}
