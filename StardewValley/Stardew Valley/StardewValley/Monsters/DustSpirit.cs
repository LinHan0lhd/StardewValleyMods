using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace StardewValley.Monsters
{
	// Token: 0x02000216 RID: 534
	public class DustSpirit : Monster
	{
		// Token: 0x06002366 RID: 9062 RVA: 0x0017FA10 File Offset: 0x0017DC10
		public DustSpirit()
		{
		}

		// Token: 0x06002367 RID: 9063 RVA: 0x0017FA18 File Offset: 0x0017DC18
		public DustSpirit(Vector2 position) : base("Dust Spirit", position)
		{
			base.IsWalkingTowardPlayer = false;
			this.Sprite.interval = 45f;
			base.Scale = (float)Game1.random.Next(75, 101) / 100f;
			this.voice = (byte)Game1.random.Next(1, 24);
			base.HideShadow = true;
		}

		// Token: 0x06002368 RID: 9064 RVA: 0x0017FA80 File Offset: 0x0017DC80
		public DustSpirit(Vector2 position, bool chargingTowardFarmer) : base("Dust Spirit", position)
		{
			base.IsWalkingTowardPlayer = false;
			if (chargingTowardFarmer)
			{
				this.chargingFarmer = true;
				this.seenFarmer = true;
			}
			this.Sprite.interval = 45f;
			base.Scale = (float)Game1.random.Next(75, 101) / 100f;
			base.HideShadow = true;
		}

		// Token: 0x06002369 RID: 9065 RVA: 0x0017FAE4 File Offset: 0x0017DCE4
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				int standingY = base.StandingPixel.Y;
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(64 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(8f, 16f), new Vector2(this.scale.Value + (float)Math.Max(-0.1, (double)(this.yJumpOffset + 32) / 128.0), this.scale.Value - Math.Max(-0.1f, (float)this.yJumpOffset / 256f)) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(64 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
				}
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 80f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f + (float)this.yJumpOffset / 64f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
			}
		}

		// Token: 0x0600236A RID: 9066 RVA: 0x0017FD7B File Offset: 0x0017DF7B
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x0600236B RID: 9067 RVA: 0x0017FD80 File Offset: 0x0017DF80
		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("dustMeep", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0));
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 150,
				scale = 0.5f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 300,
				scale = 0.5f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 450,
				scale = 0.5f
			});
		}

		// Token: 0x0600236C RID: 9068 RVA: 0x0017FF50 File Offset: 0x0017E150
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 16, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, (base.Health <= 0) ? 4f : 2f);
		}

		// Token: 0x0600236D RID: 9069 RVA: 0x0017FFBE File Offset: 0x0017E1BE
		public void offScreenBehavior(Character c, GameLocation l)
		{
		}

		// Token: 0x0600236E RID: 9070 RVA: 0x0017FFC0 File Offset: 0x0017E1C0
		public virtual bool CaughtInWeb()
		{
			TerrainFeature terrainFeature;
			if (base.currentLocation != null && base.currentLocation.terrainFeatures.TryGetValue(base.Tile, out terrainFeature))
			{
				Grass grass = terrainFeature as Grass;
				if (grass != null)
				{
					return grass.grassType.Value == 6;
				}
			}
			return false;
		}

		// Token: 0x0600236F RID: 9071 RVA: 0x0018000C File Offset: 0x0017E20C
		protected override void updateAnimation(GameTime time)
		{
			if (this.yJumpOffset == 0)
			{
				if (this.isHardModeMonster.Value && this.CaughtInWeb())
				{
					this.Sprite.Animate(time, 5, 3, 200f);
					return;
				}
				this.jumpWithoutSound(8f);
				this.yJumpVelocity = (float)Game1.random.Next(50, 70) / 10f;
				if (Game1.random.NextDouble() < 0.1 && (this.meep == null || !this.meep.IsPlaying) && Utility.isOnScreen(base.Position, 64) && Game1.currentLocation == base.currentLocation)
				{
					Game1.playSound("dustMeep", (int)(this.voice * 100) + Game1.random.Next(-100, 100), out this.meep);
				}
			}
			this.Sprite.AnimateDown(time, 0, "");
			base.resetAnimationSpeed();
		}

		// Token: 0x06002370 RID: 9072 RVA: 0x001800FC File Offset: 0x0017E2FC
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.yJumpOffset == 0)
			{
				if (Game1.random.NextDouble() < 0.01)
				{
					Vector2 standingPixel = base.getStandingPosition();
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, standingPixel + new Vector2(-21f, 0f), false, false)
						{
							layerDepth = (standingPixel.Y - 10f) / 10000f
						}
					});
					foreach (Vector2 v in Utility.getAdjacentTileLocations(base.Tile))
					{
						Object obj;
						if (base.currentLocation.objects.TryGetValue(v, out obj) && (obj.IsBreakableStone() || obj.IsTwig()))
						{
							base.currentLocation.destroyObject(v, null);
						}
					}
					this.yJumpVelocity *= 2f;
				}
				if (!this.chargingFarmer)
				{
					this.xVelocity = (float)Game1.random.Next(-20, 21) / 5f;
				}
			}
			if (this.chargingFarmer)
			{
				base.Slipperiness = 10;
				Vector2 v2 = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), base.Player);
				this.xVelocity += -v2.X / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
				if (Math.Abs(this.xVelocity) > 5f)
				{
					this.xVelocity = (float)(Math.Sign(this.xVelocity) * 5);
				}
				this.yVelocity += -v2.Y / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
				if (Math.Abs(this.yVelocity) > 5f)
				{
					this.yVelocity = (float)(Math.Sign(this.yVelocity) * 5);
				}
				if (Game1.random.NextDouble() < 0.0001)
				{
					this.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, Game1.random.Next(4), null, 300);
					this.chargingFarmer = false;
				}
				if (this.isHardModeMonster.Value && this.CaughtInWeb())
				{
					this.xVelocity = 0f;
					this.yVelocity = 0f;
					if (this.shakeTimer <= 0 && Game1.random.NextDouble() < 0.05)
					{
						this.shakeTimer = 200;
						return;
					}
				}
			}
			else
			{
				if (!this.seenFarmer && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.getStandingPosition() / 64f, base.Player.getStandingPosition() / 64f, 8))
				{
					this.seenFarmer = true;
					return;
				}
				if (this.seenFarmer && this.controller == null && !this.runningAwayFromFarmer)
				{
					this.addedSpeed = 2f;
					this.controller = new PathFindController(this, base.currentLocation, new PathFindController.isAtEnd(Utility.isOffScreenEndFunction), -1, new PathFindController.endBehavior(this.offScreenBehavior), 350, Point.Zero, true);
					this.runningAwayFromFarmer = true;
					return;
				}
				if (this.controller == null && this.runningAwayFromFarmer)
				{
					this.chargingFarmer = true;
				}
			}
		}

		// Token: 0x040014F6 RID: 5366
		[XmlIgnore]
		public bool seenFarmer;

		// Token: 0x040014F7 RID: 5367
		[XmlIgnore]
		public bool runningAwayFromFarmer;

		// Token: 0x040014F8 RID: 5368
		[XmlIgnore]
		public bool chargingFarmer;

		// Token: 0x040014F9 RID: 5369
		public byte voice;

		// Token: 0x040014FA RID: 5370
		[XmlIgnore]
		public ICue meep;
	}
}
