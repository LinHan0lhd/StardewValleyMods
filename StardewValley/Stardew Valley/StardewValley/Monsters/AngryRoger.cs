using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters
{
	// Token: 0x0200020F RID: 527
	public class AngryRoger : Monster
	{
		// Token: 0x06002311 RID: 8977 RVA: 0x00178F83 File Offset: 0x00177183
		public AngryRoger()
		{
			this.lightSourceId = this.GenerateLightSourceId(this.identifier);
		}

		// Token: 0x06002312 RID: 8978 RVA: 0x00178FB8 File Offset: 0x001771B8
		public AngryRoger(Vector2 position) : base("Ghost", position)
		{
			base.Slipperiness = 8;
			this.isGlider.Value = true;
			base.HideShadow = true;
			this.lightSourceId = this.GenerateLightSourceId(this.identifier);
		}

		// Token: 0x06002313 RID: 8979 RVA: 0x00179017 File Offset: 0x00177217
		public AngryRoger(Vector2 position, string name) : base(name, position)
		{
			base.Slipperiness = 8;
			this.isGlider.Value = true;
			base.HideShadow = true;
		}

		// Token: 0x06002314 RID: 8980 RVA: 0x00179055 File Offset: 0x00177255
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + this.name.Value);
		}

		// Token: 0x06002315 RID: 8981 RVA: 0x00179078 File Offset: 0x00177278
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(21 + this.yOffset)), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
		}

		// Token: 0x06002316 RID: 8982 RVA: 0x001791D0 File Offset: 0x001773D0
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			base.Slipperiness = 8;
			Utility.addSprinklesToLocation(base.currentLocation, base.TilePoint.X, base.TilePoint.Y, 2, 2, 101, 50, Color.LightBlue, null, false);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
				base.setTrajectory(xTrajectory, yTrajectory);
			}
			this.addedSpeed = -1f;
			Utility.removeLightSource(this.lightSourceId);
			return actualDamage;
		}

		// Token: 0x06002317 RID: 8983 RVA: 0x00179290 File Offset: 0x00177490
		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("ghost", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, false, false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, 0.049087387f, false));
		}

		// Token: 0x06002318 RID: 8984 RVA: 0x00179324 File Offset: 0x00177524
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x06002319 RID: 8985 RVA: 0x00179328 File Offset: 0x00177528
		protected override void updateAnimation(GameTime time)
		{
			this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * 6.283185307179586) * 20.0) - this.yOffsetExtra;
			if (base.currentLocation == Game1.currentLocation)
			{
				LightSource light;
				if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out light))
				{
					light.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)this.yOffset);
				}
				else
				{
					Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
				}
			}
			Point monsterPixel = base.StandingPixel;
			Point standingPixel = base.Player.StandingPixel;
			float xSlope = (float)(-(float)(standingPixel.X - monsterPixel.X));
			float ySlope = (float)(standingPixel.Y - monsterPixel.Y);
			float t = 400f;
			xSlope /= t;
			ySlope /= t;
			if (this.wasHitCounter <= 0)
			{
				this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.5707964f;
				if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextBool())
				{
					this.turningRight = true;
				}
				else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
				{
					this.turningRight = false;
				}
				if (this.turningRight)
				{
					this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
				}
				else
				{
					this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
				}
				this.rotation %= 6.2831855f;
				this.wasHitCounter = 0;
			}
			float maxAccel = Math.Min(4f, Math.Max(1f, 5f - t / 64f / 2f));
			xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
			ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
			this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * 5f))
			{
				this.xVelocity -= -xSlope * maxAccel / 6f;
			}
			if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * 5f))
			{
				this.yVelocity -= -ySlope * maxAccel / 6f;
			}
			base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false);
			base.resetAnimationSpeed();
		}

		// Token: 0x0600231A RID: 8986 RVA: 0x00179698 File Offset: 0x00177898
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			Microsoft.Xna.Framework.Rectangle monsterBounds = this.GetBoundingBox();
			Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
			if (monsterBounds.Intersects(playerBounds) && base.Player.temporarilyInvincible)
			{
				Layer backLayer = base.currentLocation.map.RequireLayer("Back");
				Point playerCenter = playerBounds.Center;
				int attempts = 0;
				Vector2 attemptedPosition = new Vector2((float)(playerCenter.X / 64 + Game1.random.Next(-12, 12)), (float)(playerCenter.Y / 64 + Game1.random.Next(-12, 12)));
				while (attempts < 3 && (attemptedPosition.X >= (float)backLayer.LayerWidth || attemptedPosition.Y >= (float)backLayer.LayerHeight || attemptedPosition.X < 0f || attemptedPosition.Y < 0f || backLayer.Tiles[(int)attemptedPosition.X, (int)attemptedPosition.Y] == null || !base.currentLocation.isTilePassable(new Location((int)attemptedPosition.X, (int)attemptedPosition.Y), Game1.viewport) || attemptedPosition.Equals(new Vector2((float)(playerCenter.X / 64), (float)(playerCenter.Y / 64)))))
				{
					attemptedPosition = new Vector2((float)(playerCenter.X / 64 + Game1.random.Next(-12, 12)), (float)(playerCenter.Y / 64 + Game1.random.Next(-12, 12)));
					attempts++;
				}
				if (attempts < 3)
				{
					base.Position = new Vector2(attemptedPosition.X * 64f, attemptedPosition.Y * 64f - 32f);
					this.Halt();
				}
			}
		}

		// Token: 0x040014C4 RID: 5316
		public const float rotationIncrement = 0.049087387f;

		// Token: 0x040014C5 RID: 5317
		[XmlIgnore]
		public int wasHitCounter;

		// Token: 0x040014C6 RID: 5318
		[XmlIgnore]
		public float targetRotation;

		// Token: 0x040014C7 RID: 5319
		[XmlIgnore]
		public bool turningRight;

		// Token: 0x040014C8 RID: 5320
		[XmlIgnore]
		public int identifier = Game1.random.Next(-99999, 99999);

		// Token: 0x040014C9 RID: 5321
		[XmlIgnore]
		public new int yOffset;

		// Token: 0x040014CA RID: 5322
		[XmlIgnore]
		public int yOffsetExtra;

		// Token: 0x040014CB RID: 5323
		public string lightSourceId;
	}
}
