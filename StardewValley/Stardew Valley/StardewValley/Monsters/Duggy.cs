using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Monsters
{
	// Token: 0x02000215 RID: 533
	public class Duggy : Monster
	{
		// Token: 0x0600235C RID: 9052 RVA: 0x0017F2FD File Offset: 0x0017D4FD
		public Duggy()
		{
			base.HideShadow = true;
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x0017F30C File Offset: 0x0017D50C
		public Duggy(Vector2 position) : base("Duggy", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.IsInvisible = true;
			base.DamageToFarmer = 0;
			this.Sprite.currentFrame = 0;
			base.HideShadow = true;
		}

		// Token: 0x0600235E RID: 9054 RVA: 0x0017F342 File Offset: 0x0017D542
		public Duggy(Vector2 position, bool magmaDuggy) : base("Magma Duggy", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.IsInvisible = true;
			base.DamageToFarmer = 0;
			this.Sprite.currentFrame = 0;
			base.HideShadow = true;
		}

		// Token: 0x0600235F RID: 9055 RVA: 0x0017F378 File Offset: 0x0017D578
		protected override void initNetFields()
		{
			base.initNetFields();
			this.position.Field.Interpolated(false, true);
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x0017F394 File Offset: 0x0017D594
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x0017F420 File Offset: 0x0017D620
		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("monsterdead", null, null, SoundContext.Default);
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.DarkRed, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 4, 64, 64);
		}

		// Token: 0x06002362 RID: 9058 RVA: 0x0017F4A0 File Offset: 0x0017D6A0
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x06002363 RID: 9059 RVA: 0x0017F4A4 File Offset: 0x0017D6A4
		public override void update(GameTime time, GameLocation location)
		{
			if (this.invincibleCountdown > 0)
			{
				this.glowingColor = Color.Cyan;
				this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (!location.farmers.Any())
			{
				return;
			}
			this.behaviorAtGameTick(time);
			Layer backLayer = location.map.RequireLayer("Back");
			if (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64))
			{
				location.characters.Remove(this);
			}
			base.updateGlow();
			if (this.stunTime.Value > 0)
			{
				this.stunTime.Value -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x06002364 RID: 9060 RVA: 0x0017F5AC File Offset: 0x0017D7AC
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				Rectangle bounds = this.GetBoundingBox();
				int standingY = base.StandingPixel.Y;
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(bounds.Height / 2 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(bounds.Height / 2 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
				}
			}
		}

		// Token: 0x06002365 RID: 9061 RVA: 0x0017F764 File Offset: 0x0017D964
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			this.isEmoting = false;
			this.Sprite.loop = false;
			if (this.stunTime.Value > 0)
			{
				return;
			}
			Rectangle r = this.GetBoundingBox();
			if (this.Sprite.currentFrame < 4)
			{
				r.Inflate(128, 128);
				if (!base.IsInvisible || r.Contains(base.Player.StandingPixel))
				{
					if (base.IsInvisible)
					{
						Tile tile = base.currentLocation.map.RequireLayer("Back").Tiles[base.Player.TilePoint.X, base.Player.TilePoint.Y];
						if (tile.Properties.ContainsKey("NPCBarrier") || (!tile.TileIndexProperties.ContainsKey("Diggable") && tile.TileIndex != 0))
						{
							return;
						}
						base.Position = new Vector2(base.Player.Position.X, base.Player.Position.Y + (float)base.Player.Sprite.SpriteHeight - (float)this.Sprite.SpriteHeight);
						base.currentLocation.localSound("Duggy", null, null, SoundContext.Default);
						base.Position = base.Player.Tile * 64f;
					}
					base.IsInvisible = false;
					this.Sprite.interval = 100f;
					this.Sprite.AnimateDown(time, 0, "");
				}
			}
			if (this.Sprite.currentFrame >= 4 && this.Sprite.currentFrame < 8)
			{
				r.Inflate(-128, -128);
				base.currentLocation.isCollidingPosition(r, Game1.viewport, false, 8, false, this);
				this.Sprite.AnimateRight(time, 0, "");
				this.Sprite.interval = 220f;
				base.DamageToFarmer = 8;
			}
			if (this.Sprite.currentFrame >= 8)
			{
				this.Sprite.AnimateUp(time, 0, "");
			}
			if (this.Sprite.currentFrame >= 10)
			{
				base.IsInvisible = true;
				this.Sprite.currentFrame = 0;
				Point tile2 = base.TilePoint;
				base.currentLocation.map.RequireLayer("Back").Tiles[tile2.X, tile2.Y].TileIndex = 0;
				base.currentLocation.removeObjectsAndSpawned(tile2.X, tile2.Y, 1, 1);
				base.DamageToFarmer = 0;
			}
		}
	}
}
