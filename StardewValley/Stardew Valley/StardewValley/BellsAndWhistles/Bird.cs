using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A1 RID: 929
	public class Bird
	{
		// Token: 0x060038B1 RID: 14513 RVA: 0x002CE978 File Offset: 0x002CCB78
		public Bird()
		{
			this.position = new Vector2(0f, 0f);
			this.startPosition = new Point(0, 0);
			this.endPosition = new Point(0, 0);
			this.birdType = Game1.random.Next(0, 4);
		}

		// Token: 0x060038B2 RID: 14514 RVA: 0x002CE9D4 File Offset: 0x002CCBD4
		public Bird(Point point, PerchingBirds context, int bird_type = 0, int flap_frames = 2)
		{
			this.startPosition.X = (this.endPosition.X = point.X);
			this.startPosition.Y = (this.endPosition.Y = point.Y);
			this.position.X = ((float)this.startPosition.X + 0.5f) * 64f;
			this.position.Y = ((float)this.startPosition.Y + 0.5f) * 64f;
			this.context = context;
			this.birdType = bird_type;
			this.framesUntilNextMove = Game1.random.Next(100, 300);
			this.peckDirection = Game1.random.Next(0, 2);
			this.flapFrames = flap_frames;
		}

		// Token: 0x060038B3 RID: 14515 RVA: 0x002CEAB4 File Offset: 0x002CCCB4
		public virtual void Draw(SpriteBatch b)
		{
			Vector2 offset_position = new Vector2(this.position.X, this.position.Y);
			offset_position.X += (float)Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.0025f)) * this.velocity * 2f;
			offset_position.Y += (float)Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.006f)) * this.velocity * 2f;
			offset_position.Y += (float)Math.Sin((double)this.pathPosition * 3.141592653589793) * -this.flyArcHeight;
			SpriteEffects effect = SpriteEffects.None;
			int frame;
			if (this.birdState == Bird.BirdState.Idle)
			{
				if (this.peckDirection == 1)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				if (!this.context.ShouldBirdsRoost())
				{
					if (this.peckFrames > 0)
					{
						frame = 1;
					}
					else
					{
						frame = 0;
					}
				}
				else if (this.peckFrames > 0)
				{
					frame = 9;
				}
				else
				{
					frame = 8;
				}
			}
			else
			{
				Vector2 offset = new Vector2((float)(this.endPosition.X - this.startPosition.X), (float)(this.endPosition.Y - this.startPosition.Y));
				offset.Normalize();
				if (Math.Abs(offset.X) > Math.Abs(offset.Y))
				{
					frame = 2;
					if (offset.X > 0f)
					{
						effect = SpriteEffects.FlipHorizontally;
					}
				}
				else if (offset.Y > 0f)
				{
					frame = 2 + this.flapFrames;
					if (offset.X > 0f)
					{
						effect = SpriteEffects.FlipHorizontally;
					}
				}
				else
				{
					frame = 2 + this.flapFrames * 2;
					if (offset.X < 0f)
					{
						effect = SpriteEffects.FlipHorizontally;
					}
				}
				if (this.pathPosition > 0.95f)
				{
					frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 50 % this.flapFrames;
				}
				else if (this.pathPosition <= 0.75f)
				{
					frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 100 % this.flapFrames;
				}
			}
			Rectangle source = new Rectangle(this.context.GetBirdWidth() * frame, this.context.GetBirdHeight() * this.birdType, this.context.GetBirdWidth(), this.context.GetBirdHeight());
			Rectangle draw_position = Game1.GlobalToLocal(Game1.viewport, new Rectangle((int)offset_position.X, (int)offset_position.Y, this.context.GetBirdWidth() * 4, this.context.GetBirdHeight() * 4));
			b.Draw(this.context.GetTexture(), draw_position, new Rectangle?(source), Color.White, 0f, this.context.GetBirdOrigin(), effect, this.position.Y / 10000f);
		}

		// Token: 0x060038B4 RID: 14516 RVA: 0x002CED8C File Offset: 0x002CCF8C
		public virtual void FlyToNewPoint()
		{
			Point point = this.context.GetFreeBirdPoint(this, 500);
			if (!(point != default(Point)))
			{
				this.framesUntilNextMove = Game1.random.Next(800, 1200);
				return;
			}
			this.context.ReserveBirdPoint(this, point);
			this.startPosition = this.endPosition;
			this.endPosition = point;
			this.pathPosition = 0f;
			this.velocity = 0f;
			if (this.context.ShouldBirdsRoost())
			{
				this.birdState = Bird.BirdState.Idle;
			}
			else
			{
				this.birdState = Bird.BirdState.Flying;
			}
			float tile_distance = Utility.distance((float)this.startPosition.X, (float)this.endPosition.X, (float)this.startPosition.Y, (float)this.endPosition.Y);
			if (tile_distance >= 7f)
			{
				this.flyArcHeight = 200f;
				return;
			}
			if (tile_distance >= 5f)
			{
				this.flyArcHeight = 150f;
				return;
			}
			this.flyArcHeight = 20f;
		}

		// Token: 0x060038B5 RID: 14517 RVA: 0x002CEE98 File Offset: 0x002CD098
		public virtual void Update(GameTime time)
		{
			if (this.peckFrames > 0)
			{
				this.peckFrames--;
			}
			else
			{
				this.nextPeck--;
				if (this.nextPeck <= 0)
				{
					if (this.context.ShouldBirdsRoost())
					{
						this.peckFrames = 50;
					}
					else
					{
						this.peckFrames = this.context.peckDuration;
					}
					this.nextPeck = Game1.random.Next(10, 30);
					if (Game1.random.NextDouble() <= 0.75)
					{
						this.nextPeck += Game1.random.Next(50, 100);
						if (!this.context.ShouldBirdsRoost())
						{
							this.peckDirection = Game1.random.Next(0, 2);
						}
					}
				}
			}
			Bird.BirdState birdState = this.birdState;
			if (birdState != Bird.BirdState.Idle)
			{
				if (birdState != Bird.BirdState.Flying)
				{
					return;
				}
			}
			else
			{
				if (this.context.ShouldBirdsRoost())
				{
					return;
				}
				using (FarmerCollection.Enumerator enumerator = Game1.currentLocation.farmers.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Farmer farmer = enumerator.Current;
						float num = Utility.distance(farmer.position.X, this.position.X, farmer.position.Y, this.position.Y);
						this.framesUntilNextMove--;
						if (num < 200f || this.framesUntilNextMove <= 0)
						{
							this.FlyToNewPoint();
						}
					}
					return;
				}
			}
			float distance = Utility.distance((float)(this.endPosition.X * 64) + 32f, this.position.X, (float)(this.endPosition.Y * 64) + 32f, this.position.Y);
			float max_velocity = this.context.birdSpeed;
			float slow_down_multiplier = 0.25f;
			if (distance > max_velocity / slow_down_multiplier)
			{
				this.velocity = Utility.MoveTowards(this.velocity, max_velocity, 0.5f);
			}
			else
			{
				this.velocity = Math.Max(Math.Min(distance * slow_down_multiplier, this.velocity), 1f);
			}
			float path_distance = Utility.distance((float)this.endPosition.X + 32f, (float)this.startPosition.X + 32f, (float)this.endPosition.Y + 32f, (float)this.startPosition.Y + 32f) * 64f;
			if (path_distance <= 0.0001f)
			{
				path_distance = 0.0001f;
			}
			float delta = this.velocity / path_distance;
			this.pathPosition += delta;
			this.position = new Vector2(Utility.Lerp((float)(this.startPosition.X * 64) + 32f, (float)(this.endPosition.X * 64) + 32f, this.pathPosition), Utility.Lerp((float)(this.startPosition.Y * 64) + 32f, (float)(this.endPosition.Y * 64) + 32f, this.pathPosition));
			if (this.pathPosition >= 1f)
			{
				this.position = new Vector2((float)(this.endPosition.X * 64) + 32f, (float)(this.endPosition.Y * 64) + 32f);
				this.birdState = Bird.BirdState.Idle;
				this.velocity = 0f;
				this.framesUntilNextMove = Game1.random.Next(350, 500);
				if (Game1.random.NextDouble() < 0.75)
				{
					this.framesUntilNextMove += Game1.random.Next(200, 300);
				}
			}
		}

		// Token: 0x0400252F RID: 9519
		public Vector2 position;

		// Token: 0x04002530 RID: 9520
		public Point startPosition;

		// Token: 0x04002531 RID: 9521
		public Point endPosition;

		// Token: 0x04002532 RID: 9522
		public float pathPosition;

		// Token: 0x04002533 RID: 9523
		public float velocity;

		// Token: 0x04002534 RID: 9524
		public int framesUntilNextMove;

		// Token: 0x04002535 RID: 9525
		public Bird.BirdState birdState;

		// Token: 0x04002536 RID: 9526
		public PerchingBirds context;

		// Token: 0x04002537 RID: 9527
		public int peckFrames;

		// Token: 0x04002538 RID: 9528
		public int nextPeck;

		// Token: 0x04002539 RID: 9529
		public int peckDirection;

		// Token: 0x0400253A RID: 9530
		public int birdType;

		// Token: 0x0400253B RID: 9531
		public int flapFrames = 2;

		// Token: 0x0400253C RID: 9532
		public float flyArcHeight;

		// Token: 0x020006BA RID: 1722
		public enum BirdState
		{
			// Token: 0x04003096 RID: 12438
			Idle,
			// Token: 0x04003097 RID: 12439
			Flying
		}
	}
}
