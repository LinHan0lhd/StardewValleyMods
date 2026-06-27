using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000390 RID: 912
	public class Butterfly : Critter
	{
		// Token: 0x0600382A RID: 14378 RVA: 0x002C691C File Offset: 0x002C4B1C
		public Butterfly(GameLocation location, Vector2 position, bool islandButterfly = false, bool forceSummerButterfly = false, int baseFrameOverride = -1, bool prismatic = false)
		{
			this.position = position * 64f;
			this.startingPosition = this.position;
			this.isPrismatic = prismatic;
			if (location.IsWinterHere())
			{
				this.baseFrame = 397;
				this.isLit = true;
			}
			else if (location.IsSpringHere() && !forceSummerButterfly)
			{
				this.baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 3 + 160) : (Game1.random.Next(3) * 3 + 180));
			}
			else
			{
				this.baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 4 + 128) : (Game1.random.Next(3) * 4 + 148));
				this.summerButterfly = true;
				if (Game1.random.NextDouble() < 0.05)
				{
					this.baseFrame = Game1.random.Next(2) * 4 + 169;
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					this.baseFrame = Game1.random.Next(2) * 4 + 480;
				}
			}
			if (islandButterfly)
			{
				this.baseFrame = Game1.random.Next(4) * 4 + 364;
				this.summerButterfly = true;
			}
			if (baseFrameOverride != -1)
			{
				this.baseFrame = baseFrameOverride;
				this.summerButterfly = false;
				this.isLit = false;
			}
			this.motion = new Vector2((float)(Game1.random.NextDouble() + 0.25) * 3f * (float)Game1.random.Choose(-1, 1) / 2f, (float)(Game1.random.NextDouble() + 0.5) * 3f * (float)Game1.random.Choose(-1, 1) / 2f);
			this.flapSpeed = Game1.random.Next(45, 80);
			this.sprite = new AnimatedSprite(Critter.critterTexture, this.baseFrame, 16, 16);
			this.sprite.loop = false;
			this.startingPosition = position;
			if (this.isLit)
			{
				this.lightId = this.GenerateLightSourceId(Game1.random.Next());
				Game1.currentLightSources.Add(new LightSource(this.lightId, 10, position + new Vector2(-30.72f, -93.44f), 0.66f, Color.Black * 0.75f, LightSource.LightContext.None, 0L, location.NameOrUniqueName));
			}
		}

		// Token: 0x0600382B RID: 14379 RVA: 0x002C6BC8 File Offset: 0x002C4DC8
		public void doneWithFlap(Farmer who)
		{
			this.flapTimer = 200 + Game1.random.Next(-5, 6);
		}

		// Token: 0x0600382C RID: 14380 RVA: 0x002C6BE3 File Offset: 0x002C4DE3
		public Butterfly setStayInbounds(bool stayInbounds)
		{
			this.stayInbounds = stayInbounds;
			return this;
		}

		// Token: 0x0600382D RID: 14381 RVA: 0x002C6BF0 File Offset: 0x002C4DF0
		public override bool update(GameTime time, GameLocation environment)
		{
			this.flapTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.flapTimer <= 0 && this.sprite.CurrentAnimation == null)
			{
				this.motionMultiplier = 1f;
				this.motion.X = this.motion.X + (float)Game1.random.Next(-80, 81) / 100f;
				this.motion.Y = (float)(Game1.random.NextDouble() + 0.25) * -3f / 2f;
				if (Math.Abs(this.motion.X) > 1.5f)
				{
					this.motion.X = 3f * (float)Math.Sign(this.motion.X) / 2f;
				}
				if (Math.Abs(this.motion.Y) > 3f)
				{
					this.motion.Y = 3f * (float)Math.Sign(this.motion.Y);
				}
				if (this.stayInbounds)
				{
					if (this.position.X < 128f)
					{
						this.motion.X = 0.8f;
					}
					if (this.position.Y < 192f)
					{
						this.motion.Y = this.motion.Y / 2f;
						this.flapTimer = 1000;
					}
					if (this.position.X > (float)(environment.map.DisplayWidth - 128))
					{
						this.motion.X = -0.8f;
					}
					if (this.position.Y > (float)(environment.map.DisplayHeight - 128))
					{
						this.motion.Y = -1f;
						this.flapTimer = 100;
					}
				}
				if (this.summerButterfly)
				{
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame + 1, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 2, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 3, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 2, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 1, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame, this.flapSpeed, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneWithFlap), false)
					});
				}
				else
				{
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame + 1, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 2, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame + 1, this.flapSpeed),
						new FarmerSprite.AnimationFrame(this.baseFrame, this.flapSpeed, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneWithFlap), false)
					});
				}
				if (this.isPrismatic && this.prismaticCaptureTimer < 0f)
				{
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), (float)Game1.random.Next(100, 200), 6, 1, this.position + new Vector2((float)(-48 + Game1.random.Next(-32, 32)), (float)(-96 + Game1.random.Next(-32, 32))), false, false, Math.Max(0f, (this.position.Y + 64f - 24f) / 10000f) + this.position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f, false)
					{
						drawAboveAlwaysFront = true
					}, environment, 4, 64, 64);
				}
			}
			if (this.prismaticCaptureTimer > 0f)
			{
				this.motion = Game1.player.position.Value + new Vector2(64f, -32f) - this.position;
				this.motion *= 0.1f;
				this.prismaticCaptureTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				this.position += this.motion;
				this.position += new Vector2((float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (this.prismaticCaptureTimer / 150f), (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (this.prismaticCaptureTimer / 150f));
				this.prismaticSprinkleTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.prismaticSprinkleTimer <= 0f)
				{
					environment.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), (float)Game1.random.Next(100, 200), 6, 1, this.position + new Vector2(-48f, -96f), false, false, Math.Max(0f, (this.position.Y + 64f - 24f) / 10000f) + this.position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f, false)
					{
						drawAboveAlwaysFront = true
					});
					this.prismaticSprinkleTimer = 80f;
				}
				if (this.prismaticCaptureTimer <= 0f)
				{
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), (float)Game1.random.Next(100, 200), 6, 1, this.position + new Vector2(-48f, -96f), false, false, Math.Max(0f, (this.position.Y + 64f - 24f) / 10000f) + this.position.X / 64f * 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						drawAboveAlwaysFront = true
					}, environment, 16, 64, 64);
					Game1.playSound("yoba", null);
					Game1.player.buffs.Remove("statue_of_blessings_6");
					if (Utility.CreateDaySaveRandom((double)(Game1.player.UniqueMultiplayerID % 10000L), 0.0, 0.0).NextDouble() < 0.05000000074505806 + Game1.player.DailyLuck)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)74", 1, 0, false), this.position + new Vector2(-48f, -96f), 2, environment, (int)Game1.player.position.Y, false);
					}
					Game1.player.Money += Math.Max(100, Math.Min(50000, (int)(Game1.player.totalMoneyEarned * 0.005f)));
					return true;
				}
			}
			else
			{
				this.position += this.motion * this.motionMultiplier;
				this.motion.Y = this.motion.Y + 0.005f * (float)time.ElapsedGameTime.Milliseconds;
				this.motionMultiplier -= 0.0005f * (float)time.ElapsedGameTime.Milliseconds;
				if (this.motionMultiplier <= 0f)
				{
					this.motionMultiplier = 0f;
				}
			}
			if (this.isPrismatic && this.prismaticCaptureTimer < 0f && Utility.distance(this.position.X, Game1.player.position.X, this.position.Y, Game1.player.position.Y) < 128f)
			{
				this.prismaticCaptureTimer = 2000f;
			}
			if (this.isLit)
			{
				Utility.repositionLightSource(this.lightId, this.position + new Vector2(-30.72f, -93.44f));
			}
			return base.update(time, environment);
		}

		// Token: 0x0600382E RID: 14382 RVA: 0x002C74DE File Offset: 0x002C56DE
		public override void draw(SpriteBatch b)
		{
		}

		// Token: 0x0600382F RID: 14383 RVA: 0x002C74E0 File Offset: 0x002C56E0
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(-64f, -128f + this.yJumpOffset + this.yOffset)), this.position.Y / 10000f, 0, 0, this.isPrismatic ? Utility.GetPrismaticColor(0, 10f) : Color.White, this.flip, 4f, 0f, false);
		}

		// Token: 0x0400248B RID: 9355
		public const float maxSpeed = 3f;

		// Token: 0x0400248C RID: 9356
		private int flapTimer;

		// Token: 0x0400248D RID: 9357
		private int flapSpeed = 50;

		// Token: 0x0400248E RID: 9358
		private Vector2 motion;

		// Token: 0x0400248F RID: 9359
		private float motionMultiplier = 1f;

		// Token: 0x04002490 RID: 9360
		private float prismaticCaptureTimer = -1f;

		// Token: 0x04002491 RID: 9361
		private float prismaticSprinkleTimer;

		// Token: 0x04002492 RID: 9362
		private bool summerButterfly;

		// Token: 0x04002493 RID: 9363
		public bool stayInbounds;

		// Token: 0x04002494 RID: 9364
		public bool isPrismatic;

		// Token: 0x04002495 RID: 9365
		public bool isLit;

		// Token: 0x04002496 RID: 9366
		private string lightId;
	}
}
