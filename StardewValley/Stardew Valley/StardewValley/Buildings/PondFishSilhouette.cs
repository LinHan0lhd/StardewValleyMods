using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Buildings
{
	// Token: 0x02000383 RID: 899
	public class PondFishSilhouette
	{
		// Token: 0x060037A3 RID: 14243 RVA: 0x002C1DE0 File Offset: 0x002BFFE0
		public PondFishSilhouette(FishPond pond)
		{
			this._pond = pond;
			this._fishObject = this._pond.GetFishObject();
			if (this._fishObject.HasContextTag("fish_upright"))
			{
				this._upRight = true;
			}
			this.position = (this._pond.GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f;
			this._age = 0f;
			this._randomOffset = Utility.Lerp(0f, 500f, (float)Game1.random.NextDouble());
			this.ResetDartTime();
		}

		// Token: 0x060037A4 RID: 14244 RVA: 0x002C1E9A File Offset: 0x002C009A
		public void ResetDartTime()
		{
			this.nextDart = Utility.Lerp(20f, 40f, (float)Game1.random.NextDouble());
		}

		// Token: 0x060037A5 RID: 14245 RVA: 0x002C1EBC File Offset: 0x002C00BC
		public void Draw(SpriteBatch b)
		{
			float angle = 0.7853982f;
			if (this._upRight)
			{
				angle = 0f;
			}
			SpriteEffects effect = SpriteEffects.None;
			angle += (float)Math.Sin((double)(this._wiggleTimer + this._randomOffset)) * 2f * 3.1415927f / 180f;
			if (this._velocity.Y < 0f)
			{
				angle -= 0.17453294f;
			}
			if (this._velocity.Y > 0f)
			{
				angle += 0.17453294f;
			}
			if (this._flipped)
			{
				effect = SpriteEffects.FlipHorizontally;
				angle *= -1f;
			}
			float draw_scale = Utility.Lerp(0.75f, 0.65f, Utility.Clamp(this._sinkAmount, 0f, 1f));
			draw_scale *= Utility.Lerp(1f, 0.75f, (float)this._pond.currentOccupants.Value / 10f);
			Vector2 draw_position = this.position;
			draw_position.Y += (float)Math.Sin((double)(this._age * 2f + this._randomOffset)) * 5f;
			draw_position.Y += (float)((int)(this._sinkAmount * 4f));
			float transparency = Utility.Lerp(0.25f, 0.15f, Utility.Clamp(this._sinkAmount, 0f, 1f));
			Vector2 origin = new Vector2(8f, 8f);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._fishObject.QualifiedItemId);
			b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position), new Rectangle?(itemData.GetSourceRect(0, null)), Color.Black * transparency, angle, origin, 4f * draw_scale, effect, this.position.Y / 10000f + 1E-06f);
		}

		// Token: 0x060037A6 RID: 14246 RVA: 0x002C208D File Offset: 0x002C028D
		public bool IsMoving()
		{
			return this._velocity.LengthSquared() > 0f;
		}

		// Token: 0x060037A7 RID: 14247 RVA: 0x002C20A4 File Offset: 0x002C02A4
		public void Update(float time)
		{
			this.nextDart -= time;
			this._age += time;
			this._wiggleTimer += time;
			if (this.nextDart <= 0f || (this.nextDart <= 0.5f && Game1.random.NextDouble() < 0.10000000149011612))
			{
				this.ResetDartTime();
				int direction = Game1.random.Next(0, 2) * 2 - 1;
				if (direction < 0)
				{
					this._flipped = true;
				}
				else
				{
					this._flipped = false;
				}
				this._velocity = new Vector2((float)direction * Utility.Lerp(50f, 100f, (float)Game1.random.NextDouble()), Utility.Lerp(-50f, 50f, (float)Game1.random.NextDouble()));
			}
			bool moving = this._velocity.LengthSquared() > 0f;
			if (moving)
			{
				this._wiggleTimer += time * 30f;
				this._sinkAmount = Utility.MoveTowards(this._sinkAmount, 0f, 2f * time);
			}
			else
			{
				this._sinkAmount = Utility.MoveTowards(this._sinkAmount, 1f, 1f * time);
			}
			this.position += this._velocity * time;
			for (int i = 0; i < this._pond.GetFishSilhouettes().Count; i++)
			{
				PondFishSilhouette other_silhouette = this._pond.GetFishSilhouettes()[i];
				if (other_silhouette != this)
				{
					float push_amount = 30f;
					float push_other_amount = 30f;
					if (this.IsMoving())
					{
						push_amount = 0f;
					}
					if (other_silhouette.IsMoving())
					{
						push_other_amount = 0f;
					}
					if (Math.Abs(other_silhouette.position.X - this.position.X) < 32f)
					{
						if (other_silhouette.position.X > this.position.X)
						{
							PondFishSilhouette pondFishSilhouette = other_silhouette;
							pondFishSilhouette.position.X = pondFishSilhouette.position.X + push_other_amount * time;
							this.position.X = this.position.X + -push_amount * time;
						}
						else
						{
							PondFishSilhouette pondFishSilhouette2 = other_silhouette;
							pondFishSilhouette2.position.X = pondFishSilhouette2.position.X - push_other_amount * time;
							this.position.X = this.position.X + push_amount * time;
						}
					}
					if (Math.Abs(other_silhouette.position.Y - this.position.Y) < 32f)
					{
						if (other_silhouette.position.Y > this.position.Y)
						{
							PondFishSilhouette pondFishSilhouette3 = other_silhouette;
							pondFishSilhouette3.position.Y = pondFishSilhouette3.position.Y + push_other_amount * time;
							this.position.Y = this.position.Y + -1f * time;
						}
						else
						{
							PondFishSilhouette pondFishSilhouette4 = other_silhouette;
							pondFishSilhouette4.position.Y = pondFishSilhouette4.position.Y - push_other_amount * time;
							this.position.Y = this.position.Y + 1f * time;
						}
					}
				}
			}
			this._velocity.X = Utility.MoveTowards(this._velocity.X, 0f, 50f * time);
			this._velocity.Y = Utility.MoveTowards(this._velocity.Y, 0f, 20f * time);
			float border_width = 1.3f;
			if (this.position.X > ((float)(this._pond.tileX.Value + this._pond.tilesWide.Value) - border_width) * 64f)
			{
				this.position.X = ((float)(this._pond.tileX.Value + this._pond.tilesWide.Value) - border_width) * 64f;
				this._velocity.X = this._velocity.X * -1f;
				if (moving && (Game1.random.NextDouble() < 0.25 || Math.Abs(this._velocity.X) > 30f))
				{
					this._flipped = !this._flipped;
				}
			}
			if (this.position.X < ((float)this._pond.tileX.Value + border_width) * 64f)
			{
				this.position.X = ((float)this._pond.tileX.Value + border_width) * 64f;
				this._velocity.X = this._velocity.X * -1f;
				if (moving && (Game1.random.NextDouble() < 0.25 || Math.Abs(this._velocity.X) > 30f))
				{
					this._flipped = !this._flipped;
				}
			}
			if (this.position.Y > ((float)(this._pond.tileY.Value + this._pond.tilesHigh.Value) - border_width) * 64f)
			{
				this.position.Y = ((float)(this._pond.tileY.Value + this._pond.tilesHigh.Value) - border_width) * 64f;
				this._velocity.Y = this._velocity.Y * -1f;
			}
			if (this.position.Y < ((float)this._pond.tileY.Value + border_width) * 64f)
			{
				this.position.Y = ((float)this._pond.tileY.Value + border_width) * 64f;
				this._velocity.Y = this._velocity.Y * -1f;
			}
		}

		// Token: 0x04002426 RID: 9254
		public Vector2 position;

		// Token: 0x04002427 RID: 9255
		protected FishPond _pond;

		// Token: 0x04002428 RID: 9256
		protected Object _fishObject;

		// Token: 0x04002429 RID: 9257
		protected Vector2 _velocity = Vector2.Zero;

		// Token: 0x0400242A RID: 9258
		protected float nextDart;

		// Token: 0x0400242B RID: 9259
		protected bool _upRight;

		// Token: 0x0400242C RID: 9260
		protected float _age;

		// Token: 0x0400242D RID: 9261
		protected float _wiggleTimer;

		// Token: 0x0400242E RID: 9262
		protected float _sinkAmount = 1f;

		// Token: 0x0400242F RID: 9263
		protected float _randomOffset;

		// Token: 0x04002430 RID: 9264
		protected bool _flipped;
	}
}
