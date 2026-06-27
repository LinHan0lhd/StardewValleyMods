using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000397 RID: 919
	public class Firefly : Critter
	{
		// Token: 0x06003854 RID: 14420 RVA: 0x002C9305 File Offset: 0x002C7505
		public Firefly()
		{
		}

		// Token: 0x06003855 RID: 14421 RVA: 0x002C9310 File Offset: 0x002C7510
		public Firefly(Vector2 position)
		{
			this.baseFrame = -1;
			this.position = position * 64f;
			this.startingPosition = position * 64f;
			this.motion = new Vector2((float)Game1.random.Next(-10, 11) * 0.1f, (float)Game1.random.Next(-10, 11) * 0.1f);
			this.id = (int)(position.X * 10099f + position.Y * 77f + (float)Game1.random.Next(99999));
			this.light = new LightSource(this.GenerateLightSourceId(this.id), 4, position, (float)Game1.random.Next(4, 6) * 0.1f, Color.Purple * 0.8f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName);
			this.glowing = true;
			Game1.currentLightSources.Add(this.light);
		}

		// Token: 0x06003856 RID: 14422 RVA: 0x002C9414 File Offset: 0x002C7614
		public override bool update(GameTime time, GameLocation environment)
		{
			this.position += this.motion;
			this.motion.X = this.motion.X + (float)Game1.random.Next(-1, 2) * 0.1f;
			this.motion.Y = this.motion.Y + (float)Game1.random.Next(-1, 2) * 0.1f;
			if (this.motion.X < -1f)
			{
				this.motion.X = -1f;
			}
			if (this.motion.X > 1f)
			{
				this.motion.X = 1f;
			}
			if (this.motion.Y < -1f)
			{
				this.motion.Y = -1f;
			}
			if (this.motion.Y > 1f)
			{
				this.motion.Y = 1f;
			}
			if (this.glowing)
			{
				this.light.position.Value = this.position;
			}
			return this.position.X < -128f || this.position.Y < -128f || this.position.X > (float)environment.map.DisplayWidth || this.position.Y > (float)environment.map.DisplayHeight;
		}

		// Token: 0x06003857 RID: 14423 RVA: 0x002C957C File Offset: 0x002C777C
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(this.position), new Rectangle?(Game1.staminaRect.Bounds), this.glowing ? Color.White : Color.Brown, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}

		// Token: 0x040024D4 RID: 9428
		private bool glowing;

		// Token: 0x040024D5 RID: 9429
		private int id;

		// Token: 0x040024D6 RID: 9430
		private Vector2 motion;

		// Token: 0x040024D7 RID: 9431
		private LightSource light;
	}
}
