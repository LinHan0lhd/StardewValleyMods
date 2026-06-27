using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	// Token: 0x020002D8 RID: 728
	public class Wisp
	{
		// Token: 0x06002FE5 RID: 12261 RVA: 0x0025D55E File Offset: 0x0025B75E
		public Wisp(int index)
		{
			this.Reinitialize();
		}

		// Token: 0x06002FE6 RID: 12262 RVA: 0x0025D590 File Offset: 0x0025B790
		public virtual void Reinitialize()
		{
			this.baseColor = Color.White * Utility.RandomFloat(0.25f, 0.75f, null);
			this.rotationOffset = Utility.RandomFloat(0f, 360f, null);
			this.rotationSpeed = Utility.RandomFloat(0.5f, 2f, null);
			this.rotationRadius = Utility.RandomFloat(8f, 32f, null);
			this.lifeTime = Utility.RandomFloat(6f, 12f, null);
			this.age = 0f;
			this.position = new Vector2((float)Game1.random.Next(0, Game1.currentLocation.map.DisplayWidth), (float)Game1.random.Next(0, Game1.currentLocation.map.DisplayHeight));
			this.drawPosition = Vector2.Zero;
			for (int i = 0; i < this.oldPositions.Length; i++)
			{
				this.oldPositions[i] = Vector2.Zero;
			}
		}

		// Token: 0x06002FE7 RID: 12263 RVA: 0x0025D690 File Offset: 0x0025B890
		public virtual void Update(GameTime time)
		{
			this.age += (float)time.ElapsedGameTime.TotalSeconds;
			this.position.X = this.position.X - (Math.Max(0.4f, Math.Min(1f, (float)this.index * 0.01f)) - (float)((double)((float)this.index * 0.01f) * Math.Sin(6.283185307179586 * (double)time.TotalGameTime.Milliseconds / 8000.0)));
			this.position.Y = this.position.Y + Math.Max(0.5f, Math.Min(1.2f, (float)this.index * 0.02f));
			if (this.age >= this.lifeTime)
			{
				this.Reinitialize();
			}
			else if (this.position.Y > (float)Game1.currentLocation.map.DisplayHeight)
			{
				this.Reinitialize();
			}
			else if (this.position.X < 0f)
			{
				this.Reinitialize();
			}
			this.drawPosition = this.position + new Vector2((float)Math.Sin((double)(this.age * this.rotationSpeed + this.rotationOffset)), (float)Math.Sin((double)(this.age * this.rotationSpeed + this.rotationOffset))) * this.rotationRadius;
			this.tailUpdateTimer--;
			if (this.tailUpdateTimer <= 0)
			{
				this.tailUpdateTimer = 6;
				this.oldPositionIndex = (this.oldPositionIndex + 1) % this.oldPositions.Length;
				this.oldPositions[this.oldPositionIndex] = this.drawPosition;
			}
		}

		// Token: 0x06002FE8 RID: 12264 RVA: 0x0025D848 File Offset: 0x0025BA48
		public virtual void Draw(SpriteBatch b)
		{
			Color draw_color = this.baseColor;
			draw_color *= Utility.Lerp(0f, 1f, (float)Math.Sin((double)(this.age / this.lifeTime) * 3.141592653589793));
			float rotation = this.age * this.rotationSpeed * 2f + this.rotationOffset * (float)this.index;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.drawPosition), new Rectangle?(new Rectangle(346 + (int)(this.age / 0.25f + this.rotationOffset) % 4 * 5, 1971, 5, 5)), draw_color, rotation, new Vector2(2.5f, 2.5f), 4f, SpriteEffects.None, 1f);
			int tail_index = this.oldPositionIndex;
			for (int i = 0; i < this.oldPositions.Length; i++)
			{
				tail_index++;
				if (tail_index >= this.oldPositions.Length)
				{
					tail_index = 0;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.oldPositions[tail_index]), new Rectangle?(new Rectangle(356, 1971, 5, 5)), draw_color * ((float)i / (float)this.oldPositions.Length), rotation - (float)i, new Vector2(2.5f, 2.5f), 2f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x0400206C RID: 8300
		public Vector2 position;

		// Token: 0x0400206D RID: 8301
		public Vector2 drawPosition;

		// Token: 0x0400206E RID: 8302
		public Vector2[] oldPositions = new Vector2[16];

		// Token: 0x0400206F RID: 8303
		public int oldPositionIndex;

		// Token: 0x04002070 RID: 8304
		public int index;

		// Token: 0x04002071 RID: 8305
		public int tailUpdateTimer;

		// Token: 0x04002072 RID: 8306
		public float rotationSpeed;

		// Token: 0x04002073 RID: 8307
		public float rotationOffset;

		// Token: 0x04002074 RID: 8308
		public float rotationRadius = 16f;

		// Token: 0x04002075 RID: 8309
		public float age;

		// Token: 0x04002076 RID: 8310
		public float lifeTime = 1f;

		// Token: 0x04002077 RID: 8311
		public Color baseColor;
	}
}
