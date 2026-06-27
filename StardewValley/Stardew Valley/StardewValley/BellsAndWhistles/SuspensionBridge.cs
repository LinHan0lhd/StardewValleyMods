using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AC RID: 940
	public class SuspensionBridge
	{
		// Token: 0x0600391A RID: 14618 RVA: 0x002D4AAA File Offset: 0x002D2CAA
		public SuspensionBridge()
		{
			this._texture = Game1.content.Load<Texture2D>("LooseSprites\\SuspensionBridge");
		}

		// Token: 0x0600391B RID: 14619 RVA: 0x002D4AE0 File Offset: 0x002D2CE0
		public SuspensionBridge(int tile_x, int tile_y) : this()
		{
			this.bridgeBounds = new Rectangle(tile_x * 64, tile_y * 64, 384, 64);
			this.bridgeEntrances.Add(new Rectangle((tile_x - 1) * 64, tile_y * 64, 64, 64));
			this.bridgeEntrances.Add(new Rectangle((tile_x + 6) * 64, tile_y * 64, 64, 64));
			this.bridgeSortRegions.Add(new Rectangle((tile_x - 1) * 64, (tile_y - 1) * 64, 128, 192));
			this.bridgeSortRegions.Add(new Rectangle((tile_x + 5) * 64, (tile_y - 1) * 64, 128, 192));
		}

		// Token: 0x0600391C RID: 14620 RVA: 0x002D4B98 File Offset: 0x002D2D98
		public virtual bool InEntranceArea(int x, int y)
		{
			foreach (Rectangle rect in this.bridgeEntrances)
			{
				if (rect.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600391D RID: 14621 RVA: 0x002D4BF8 File Offset: 0x002D2DF8
		public virtual bool InEntranceArea(Rectangle rectangle)
		{
			foreach (Rectangle rect in this.bridgeEntrances)
			{
				if (rect.Contains(rectangle))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600391E RID: 14622 RVA: 0x002D4C58 File Offset: 0x002D2E58
		public virtual bool CheckPlacementPrevention(Vector2 tileLocation)
		{
			using (List<Rectangle>.Enumerator enumerator = this.bridgeEntrances.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (Utility.doesRectangleIntersectTile(enumerator.Current, (int)tileLocation.X, (int)tileLocation.Y))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600391F RID: 14623 RVA: 0x002D4CC0 File Offset: 0x002D2EC0
		public virtual void OnFootstep(Vector2 position)
		{
			if (this.bridgeBounds.Contains((int)position.X, (int)position.Y) && position.X > (float)(this.bridgeBounds.X + 64) && position.X < (float)(this.bridgeBounds.Right - 64))
			{
				this.shakeTime = 0.4f;
			}
		}

		// Token: 0x06003920 RID: 14624 RVA: 0x002D4D24 File Offset: 0x002D2F24
		public virtual void Update(GameTime time)
		{
			if (this.shakeTime > 0f)
			{
				this.shakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.shakeTime < 0f)
				{
					this.shakeTime = 0f;
				}
			}
			if (Game1.player.bridge == null && this.InEntranceArea(Game1.player.GetBoundingBox()))
			{
				Game1.player.bridge = this;
			}
			if (Game1.player.bridge == this)
			{
				Rectangle playerBounds = Game1.player.GetBoundingBox();
				if (playerBounds.Top >= this.bridgeBounds.Top && playerBounds.Bottom <= this.bridgeBounds.Bottom && (playerBounds.Intersects(this.bridgeBounds) || this.InEntranceArea(playerBounds)))
				{
					Game1.player.SetOnBridge(true);
					return;
				}
				if (!this.InEntranceArea(playerBounds) && !playerBounds.Intersects(this.bridgeBounds))
				{
					Game1.player.SetOnBridge(false);
					Game1.player.bridge = null;
				}
			}
		}

		// Token: 0x06003921 RID: 14625 RVA: 0x002D4E30 File Offset: 0x002D3030
		public virtual void Draw(SpriteBatch b)
		{
			b.Draw(this._texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)this.bridgeBounds.X, (float)(this.bridgeBounds.Y - 128))), new Rectangle?(new Rectangle(0, 0, 96, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.bridgeBounds.Y / 10000f + 0.0256f);
			float[] shake_multipliers = new float[]
			{
				0f,
				0.5f,
				1f,
				1f,
				0.5f,
				0f
			};
			for (int i = 0; i < 6; i++)
			{
				float shake = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 10.0 + (double)(i * 5)) * 1f * 4f * shake_multipliers[i] * (this.shakeTime / 0.4f);
				b.Draw(this._texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.bridgeBounds.X + i * 64), (float)this.bridgeBounds.Y + shake)), new Rectangle?(new Rectangle(16 * i, 32, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.bridgeBounds.Y / 10000f + 0.0256f);
			}
		}

		// Token: 0x040025BE RID: 9662
		public Rectangle bridgeBounds;

		// Token: 0x040025BF RID: 9663
		public List<Rectangle> bridgeEntrances = new List<Rectangle>();

		// Token: 0x040025C0 RID: 9664
		public List<Rectangle> bridgeSortRegions = new List<Rectangle>();

		// Token: 0x040025C1 RID: 9665
		public const float BRIDGE_SORT_OFFSET = 0.0256f;

		// Token: 0x040025C2 RID: 9666
		protected Texture2D _texture;

		// Token: 0x040025C3 RID: 9667
		public float shakeTime;
	}
}
