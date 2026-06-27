using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Companions
{
	// Token: 0x02000373 RID: 883
	public class HoppingCompanion : Companion
	{
		// Token: 0x06003600 RID: 13824 RVA: 0x002A8B8C File Offset: 0x002A6D8C
		public HoppingCompanion()
		{
		}

		// Token: 0x06003601 RID: 13825 RVA: 0x002A8B94 File Offset: 0x002A6D94
		public HoppingCompanion(int which = 0)
		{
			this.whichVariant.Value = which;
		}

		// Token: 0x06003602 RID: 13826 RVA: 0x002A8BA8 File Offset: 0x002A6DA8
		public override void Draw(SpriteBatch b)
		{
			Farmer owner = base.Owner;
			if (((owner != null) ? owner.currentLocation : null) == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
			{
				return;
			}
			Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			this._draw(b, texture, new Rectangle(0, 16, 16, 16));
		}

		// Token: 0x06003603 RID: 13827 RVA: 0x002A8C14 File Offset: 0x002A6E14
		protected void _draw(SpriteBatch b, Texture2D texture, Rectangle startingSourceRect)
		{
			SpriteEffects effect = SpriteEffects.None;
			if (this.direction.Value == 3)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			if (this.height > 0f)
			{
				if (this.gravity > 0f)
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 16, 0)), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, this._position.Y / 10000f);
				}
				else if (this.gravity > -0.15f)
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 32, 0)), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, this._position.Y / 10000f);
				}
				else
				{
					b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(Utility.translateRect(startingSourceRect, 48, 0)), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, this._position.Y / 10000f);
				}
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f)), new Rectangle?(startingSourceRect), Color.White, 0f, new Vector2(8f, 16f), 4f, effect, this._position.Y / 10000f);
			}
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(this.height, 1f)), SpriteEffects.None, 0f);
		}
	}
}
