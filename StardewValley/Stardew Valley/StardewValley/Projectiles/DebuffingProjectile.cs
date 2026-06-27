using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles
{
	// Token: 0x02000199 RID: 409
	public class DebuffingProjectile : Projectile
	{
		// Token: 0x06001D26 RID: 7462 RVA: 0x0014E401 File Offset: 0x0014C601
		public DebuffingProjectile()
		{
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x0014E42C File Offset: 0x0014C62C
		public DebuffingProjectile(string debuff, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, GameLocation location = null, Character owner = null, bool hitsMonsters = false, bool playDefaultSoundOnFire = true) : this()
		{
			this.theOneWhoFiredMe.Set(location, owner);
			this.debuff.Value = debuff;
			this.currentTileSheetIndex.Value = spriteIndex;
			this.bouncesLeft.Value = bouncesTillDestruct;
			this.tailLength.Value = tailLength;
			this.rotationVelocity.Value = rotationVelocity;
			this.xVelocity.Value = xVelocity;
			this.yVelocity.Value = yVelocity;
			this.position.Value = startingPosition;
			this.damagesMonsters.Value = hitsMonsters;
			if (playDefaultSoundOnFire)
			{
				if (location == null)
				{
					Game1.playSound("debuffSpell", null);
					return;
				}
				location.playSound("debuffSpell", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06001D28 RID: 7464 RVA: 0x0014E4FC File Offset: 0x0014C6FC
		protected override void InitNetFields()
		{
			base.InitNetFields();
			base.NetFields.AddField(this.debuff, "debuff").AddField(this.wavyMotion, "wavyMotion").AddField(this.debuffIntensity, "debuffIntensity");
		}

		// Token: 0x06001D29 RID: 7465 RVA: 0x0014E53C File Offset: 0x0014C73C
		public override void updatePosition(GameTime time)
		{
			this.xVelocity.Value += this.acceleration.X;
			this.yVelocity.Value += this.acceleration.Y;
			this.position.X += this.xVelocity.Value;
			this.position.Y += this.yVelocity.Value;
			if (this.wavyMotion.Value)
			{
				this.position.X += (float)Math.Sin((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 128.0) * 8f;
				this.position.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 128.0) * 8f;
			}
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x0014E650 File Offset: 0x0014C850
		public override bool update(GameTime time, GameLocation location)
		{
			if (this.debuff.Value == "frozen")
			{
				this.periodicEffectTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.periodicEffectTimer > 50f)
				{
					this.periodicEffectTimer = 0f;
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Rectangle(32, 32, 16, 16), 9999f, 1, 1, this.position.Value, false, false, 1f, 0.01f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						motion = Utility.getRandom360degreeVector(1f) + new Vector2(this.xVelocity.Value, this.yVelocity.Value),
						drawAboveAlwaysFront = true
					});
				}
			}
			return base.update(time, location);
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x0014E748 File Offset: 0x0014C948
		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			if (!this.damagesMonsters.Value && Game1.random.Next(11) >= player.Immunity && !player.hasBuff("28") && !player.hasTrinketWithID("BasiliskPaw"))
			{
				NetInt piercesLeft = this.piercesLeft;
				int value = piercesLeft.Value;
				piercesLeft.Value = value - 1;
				if (Game1.player == player)
				{
					player.applyBuff(this.debuff.Value);
				}
				this.explosionAnimation(location);
				if (this.debuff.Value == "19")
				{
					location.playSound("frozen", null, null, SoundContext.Default);
					return;
				}
				location.playSound("debuffHit", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06001D2C RID: 7468 RVA: 0x0014E828 File Offset: 0x0014CA28
		public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
		{
			this.explosionAnimation(location);
			NetInt piercesLeft = this.piercesLeft;
			int value = piercesLeft.Value;
			piercesLeft.Value = value - 1;
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x0014E854 File Offset: 0x0014CA54
		public override void behaviorOnCollisionWithOther(GameLocation location)
		{
			this.explosionAnimation(location);
			NetInt piercesLeft = this.piercesLeft;
			int value = piercesLeft.Value;
			piercesLeft.Value = value - 1;
		}

		// Token: 0x06001D2E RID: 7470 RVA: 0x0014E880 File Offset: 0x0014CA80
		protected virtual void explosionAnimation(GameLocation location)
		{
			if (!(this.debuff.Value == "frozen"))
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(352, (float)Game1.random.Next(100, 150), 2, 1, this.position.Value, false, false)
				});
			}
		}

		// Token: 0x06001D2F RID: 7471 RVA: 0x0014E8E4 File Offset: 0x0014CAE4
		public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
		{
			if (this.damagesMonsters.Value && n is Monster && this.debuff.Value == "frozen")
			{
				Leaper leaper = n as Leaper;
				if (leaper != null && leaper.leaping.Value)
				{
					return;
				}
				if ((n as Monster).stunTime.Value < 51)
				{
					NetInt piercesLeft = this.piercesLeft;
					int value = piercesLeft.Value;
					piercesLeft.Value = value - 1;
				}
				if ((n as Monster).stunTime.Value < this.debuffIntensity.Value - 1000)
				{
					location.playSound("frozen", null, null, SoundContext.Default);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), new Vector2(0f, 0f), false, 0f, Color.White)
						{
							layerDepth = (float)(n.StandingPixel.Y + 2) / 10000f,
							animationLength = 1,
							interval = (float)this.debuffIntensity.Value,
							scale = 4f,
							id = (int)(n.position.X * 777f + n.position.Y * 77777f),
							positionFollowsAttachedCharacter = true,
							attachedCharacter = n
						}
					});
				}
				(n as Monster).stunTime.Value = this.debuffIntensity.Value;
			}
		}

		// Token: 0x040011D2 RID: 4562
		public readonly NetString debuff = new NetString();

		// Token: 0x040011D3 RID: 4563
		public NetBool wavyMotion = new NetBool(true);

		// Token: 0x040011D4 RID: 4564
		public NetInt debuffIntensity = new NetInt(-1);

		// Token: 0x040011D5 RID: 4565
		private float periodicEffectTimer;
	}
}
