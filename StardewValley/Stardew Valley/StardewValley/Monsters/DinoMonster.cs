using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace StardewValley.Monsters
{
	// Token: 0x02000214 RID: 532
	[XmlInclude(typeof(DinoMonster.BreathProjectile))]
	public class DinoMonster : Monster
	{
		// Token: 0x0600234E RID: 9038 RVA: 0x0017E575 File Offset: 0x0017C775
		public DinoMonster()
		{
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x0017E5A4 File Offset: 0x0017C7A4
		public DinoMonster(Vector2 position) : base("Pepper Rex", position)
		{
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			this.timeUntilNextAttack = 2000;
			this.nextChangeDirectionTime = Game1.random.Next(1000, 3000);
			this.nextWanderTime = Game1.random.Next(1000, 2000);
			for (int i = 0; i < this.projectiles.Count; i++)
			{
				this.projectiles[i] = new DinoMonster.BreathProjectile();
			}
		}

		// Token: 0x06002350 RID: 9040 RVA: 0x0017E66C File Offset: 0x0017C86C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.attackState, "attackState").AddField(this.firing, "firing").AddField(this.projectiles, "projectiles");
		}

		// Token: 0x06002351 RID: 9041 RVA: 0x0017E6AB File Offset: 0x0017C8AB
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x06002352 RID: 9042 RVA: 0x0017E6DC File Offset: 0x0017C8DC
		public override void draw(SpriteBatch b)
		{
			if (base.Health > 0 && !base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				int standingY = base.StandingPixel.Y;
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, this.scale.Value), this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
				}
			}
			foreach (DinoMonster.BreathProjectile projectile in this.projectiles)
			{
				if (Utility.isOnScreen(projectile.position.Value, 64))
				{
					projectile.Draw(b);
				}
			}
		}

		// Token: 0x06002353 RID: 9043 RVA: 0x0017E8E0 File Offset: 0x0017CAE0
		public override Rectangle GetBoundingBox()
		{
			if (base.Health <= 0)
			{
				return new Rectangle(-100, -100, 0, 0);
			}
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 64);
		}

		// Token: 0x06002354 RID: 9044 RVA: 0x0017E934 File Offset: 0x0017CB34
		public override List<Item> getExtraDropItems()
		{
			List<Item> extra_items = new List<Item>();
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				extra_items.Add(ItemRegistry.Create("(O)107", 1, 0, false));
			}
			else
			{
				Item[] non_egg_items = new Item[]
				{
					ItemRegistry.Create("(O)580", 1, 0, false),
					ItemRegistry.Create("(O)583", 1, 0, false),
					ItemRegistry.Create("(O)584", 1, 0, false)
				};
				extra_items.Add(Game1.random.ChooseFrom(non_egg_items));
			}
			return extra_items;
		}

		// Token: 0x06002355 RID: 9045 RVA: 0x0017E9BC File Offset: 0x0017CBBC
		public override bool ShouldMonsterBeRemoved()
		{
			using (IEnumerator<DinoMonster.BreathProjectile> enumerator = this.projectiles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.active.Value)
					{
						return false;
					}
				}
			}
			return base.ShouldMonsterBeRemoved();
		}

		// Token: 0x06002356 RID: 9046 RVA: 0x0017EA1C File Offset: 0x0017CC1C
		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie", null, null, SoundContext.Default);
			base.currentLocation.playSound("grunt", null, null, SoundContext.Default);
			Rectangle bounds = this.GetBoundingBox();
			for (int i = 0; i < 16; i++)
			{
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp((float)bounds.Left, (float)bounds.Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp((float)bounds.Bottom, (float)bounds.Top, (float)Game1.random.NextDouble()), 1, base.TilePoint.Y, Color.White, 4f);
			}
		}

		// Token: 0x06002357 RID: 9047 RVA: 0x0017EB10 File Offset: 0x0017CD10
		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 8, 96, 64);
		}

		// Token: 0x06002358 RID: 9048 RVA: 0x0017EB70 File Offset: 0x0017CD70
		public override void update(GameTime time, GameLocation location)
		{
			if (base.Health > 0)
			{
				base.update(time, location);
			}
			foreach (DinoMonster.BreathProjectile breathProjectile in this.projectiles)
			{
				breathProjectile.Update(time, location, this);
			}
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x0017EBD0 File Offset: 0x0017CDD0
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.attackState.Value == 1)
			{
				base.IsWalkingTowardPlayer = false;
				this.Halt();
			}
			else if (this.withinPlayerThreshold())
			{
				base.IsWalkingTowardPlayer = true;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
				this.nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
				this.nextWanderTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextChangeDirectionTime < 0)
				{
					this.nextChangeDirectionTime = Game1.random.Next(500, 1000);
					this.facingDirection.Value = (this.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
				}
				if (this.nextWanderTime < 0)
				{
					if (this.wanderState)
					{
						this.nextWanderTime = Game1.random.Next(1000, 2000);
					}
					else
					{
						this.nextWanderTime = Game1.random.Next(1000, 3000);
					}
					this.wanderState = !this.wanderState;
				}
				if (this.wanderState)
				{
					this.moveLeft = (this.moveUp = (this.moveRight = (this.moveDown = false)));
					base.tryToMoveInDirection(this.facingDirection.Value, false, base.DamageToFarmer, this.isGlider.Value);
				}
			}
			this.timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
			if (this.attackState.Value == 0 && this.withinPlayerThreshold(2))
			{
				this.firing.Set(false);
				if (this.timeUntilNextAttack < 0)
				{
					this.timeUntilNextAttack = 0;
					this.attackState.Set(1);
					this.nextFireTime = 500;
					this.totalFireTime = 3000;
					base.currentLocation.playSound("croak", null, null, SoundContext.Default);
					return;
				}
			}
			else if (this.totalFireTime > 0)
			{
				if (!this.firing.Value)
				{
					Farmer player = base.Player;
					if (player != null)
					{
						base.faceGeneralDirection(player.Position, 0, false);
					}
				}
				this.totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextFireTime > 0)
				{
					this.nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (this.nextFireTime <= 0)
					{
						if (!this.firing.Value)
						{
							this.firing.Set(true);
							base.currentLocation.playSound("furnace", null, null, SoundContext.Default);
						}
						float fire_angle = 0f;
						Point standingPixel = base.StandingPixel;
						Vector2 shot_origin = new Vector2((float)standingPixel.X - 32f, (float)standingPixel.Y - 32f);
						switch (this.facingDirection.Value)
						{
						case 0:
							this.yVelocity = -1f;
							shot_origin.Y -= 64f;
							fire_angle = 90f;
							break;
						case 1:
							this.xVelocity = -1f;
							shot_origin.X += 64f;
							fire_angle = 0f;
							break;
						case 2:
							this.yVelocity = 1f;
							fire_angle = 270f;
							break;
						case 3:
							this.xVelocity = 1f;
							shot_origin.X -= 64f;
							fire_angle = 180f;
							break;
						}
						fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * 3.141592653589793 / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.141592653589793 / 180.0), -(float)Math.Sin((double)fire_angle * 3.141592653589793 / 180.0));
						shot_velocity *= 10f;
						DinoMonster.BreathProjectile projectile = this.projectiles[this.lastProjectileSlot];
						projectile.active.Value = true;
						projectile.position.Value = (projectile.startPosition.Value = shot_origin);
						projectile.velocity.Value = shot_velocity;
						this.lastProjectileSlot = (this.lastProjectileSlot + 1) % this.projectiles.Count;
						this.nextFireTime = 70;
					}
				}
				if (this.totalFireTime <= 0)
				{
					this.totalFireTime = 0;
					this.nextFireTime = 0;
					this.attackState.Set(0);
					this.timeUntilNextAttack = Game1.random.Next(1000, 2000);
				}
			}
		}

		// Token: 0x0600235A RID: 9050 RVA: 0x0017F09C File Offset: 0x0017D29C
		protected override void updateAnimation(GameTime time)
		{
			int direction_offset = 0;
			switch (this.FacingDirection)
			{
			case 0:
				direction_offset = 8;
				break;
			case 1:
				direction_offset = 4;
				break;
			case 2:
				direction_offset = 0;
				break;
			case 3:
				direction_offset = 12;
				break;
			}
			if (this.attackState.Value == 1)
			{
				if (this.firing.Value)
				{
					this.Sprite.CurrentFrame = 16 + direction_offset;
					return;
				}
				this.Sprite.CurrentFrame = 17 + direction_offset;
				return;
			}
			else
			{
				if (!this.isMoving() && !this.wanderState)
				{
					switch (this.FacingDirection)
					{
					case 0:
						this.Sprite.AnimateUp(time, 0, "");
						break;
					case 1:
						this.Sprite.AnimateRight(time, 0, "");
						break;
					case 2:
						this.Sprite.AnimateDown(time, 0, "");
						break;
					case 3:
						this.Sprite.AnimateLeft(time, 0, "");
						break;
					}
					this.Sprite.StopAnimation();
					return;
				}
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.AnimateUp(time, 0, "");
					return;
				case 1:
					this.Sprite.AnimateRight(time, 0, "");
					return;
				case 2:
					this.Sprite.AnimateDown(time, 0, "");
					return;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x0600235B RID: 9051 RVA: 0x0017F208 File Offset: 0x0017D408
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			int direction_offset = 0;
			switch (this.FacingDirection)
			{
			case 0:
				direction_offset = 8;
				break;
			case 1:
				direction_offset = 4;
				break;
			case 2:
				direction_offset = 0;
				break;
			case 3:
				direction_offset = 12;
				break;
			}
			if (this.attackState.Value == 1)
			{
				if (this.firing.Value)
				{
					this.Sprite.CurrentFrame = 16 + direction_offset;
					return;
				}
				this.Sprite.CurrentFrame = 17 + direction_offset;
				return;
			}
			else
			{
				if (!this.isMoving())
				{
					this.Sprite.StopAnimation();
					return;
				}
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.AnimateUp(time, 0, "");
					return;
				case 1:
					this.Sprite.AnimateRight(time, 0, "");
					return;
				case 2:
					this.Sprite.AnimateDown(time, 0, "");
					return;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x040014EC RID: 5356
		public int timeUntilNextAttack;

		// Token: 0x040014ED RID: 5357
		public readonly NetBool firing = new NetBool(false);

		// Token: 0x040014EE RID: 5358
		public NetInt attackState = new NetInt();

		// Token: 0x040014EF RID: 5359
		public int nextFireTime;

		// Token: 0x040014F0 RID: 5360
		public int totalFireTime;

		// Token: 0x040014F1 RID: 5361
		public int nextChangeDirectionTime;

		// Token: 0x040014F2 RID: 5362
		public int nextWanderTime;

		// Token: 0x040014F3 RID: 5363
		public bool wanderState;

		// Token: 0x040014F4 RID: 5364
		public readonly NetObjectArray<DinoMonster.BreathProjectile> projectiles = new NetObjectArray<DinoMonster.BreathProjectile>(15);

		// Token: 0x040014F5 RID: 5365
		public int lastProjectileSlot;

		// Token: 0x02000586 RID: 1414
		public enum AttackState
		{
			// Token: 0x04002BC8 RID: 11208
			None,
			// Token: 0x04002BC9 RID: 11209
			Fireball,
			// Token: 0x04002BCA RID: 11210
			Charge
		}

		// Token: 0x02000587 RID: 1415
		public class BreathProjectile : INetObject<NetFields>
		{
			// Token: 0x170004E6 RID: 1254
			// (get) Token: 0x060041B9 RID: 16825 RVA: 0x00308AC0 File Offset: 0x00306CC0
			public NetFields NetFields { get; } = new NetFields("BreathProjectile");

			// Token: 0x060041BA RID: 16826 RVA: 0x00308AC8 File Offset: 0x00306CC8
			public BreathProjectile()
			{
				this.NetFields.SetOwner(this).AddField(this.active, "active").AddField(this.position, "position").AddField(this.startPosition, "startPosition").AddField(this.velocity, "velocity");
				this.active.InterpolationEnabled = (this.active.InterpolationWait = false);
				this.position.InterpolationEnabled = (this.position.InterpolationWait = false);
				this.startPosition.InterpolationEnabled = (this.startPosition.InterpolationWait = false);
				this.velocity.InterpolationEnabled = (this.velocity.InterpolationWait = false);
			}

			// Token: 0x060041BB RID: 16827 RVA: 0x00308BCC File Offset: 0x00306DCC
			public Rectangle GetBoundingBox()
			{
				Vector2 pos = this.position.Value;
				int damageSize = 29;
				float currentScale = 1f;
				damageSize = (int)((float)damageSize * currentScale);
				return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
			}

			// Token: 0x060041BC RID: 16828 RVA: 0x00308C17 File Offset: 0x00306E17
			public Rectangle GetSourceRect()
			{
				return Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 10, 16, 16);
			}

			// Token: 0x060041BD RID: 16829 RVA: 0x00308C2C File Offset: 0x00306E2C
			public void ExplosionAnimation(GameLocation location)
			{
				Rectangle sourceRect = this.GetSourceRect();
				sourceRect.X += 4;
				sourceRect.Y += 4;
				sourceRect.Width = 8;
				sourceRect.Height = 8;
				Game1.createRadialDebris_MoreNatural(location, "TileSheets\\Projectiles", sourceRect, 1, (int)this.position.X + 32, (int)this.position.Y + 32, 6, (int)(this.position.Y / 64f) + 1);
			}

			// Token: 0x060041BE RID: 16830 RVA: 0x00308CA8 File Offset: 0x00306EA8
			public void Update(GameTime time, GameLocation location, DinoMonster parent)
			{
				if (!this.active.Value)
				{
					return;
				}
				this.position.Value += this.velocity.Value;
				if (!Game1.IsMasterGame)
				{
					this.position.MarkClean();
					this.position.ResetNewestReceivedChangeVersion();
				}
				float dist = Vector2.Distance(this.position.Value, this.startPosition.Value);
				if (dist > 128f)
				{
					this.alpha = (256f - dist) / 128f;
				}
				else
				{
					this.alpha = 1f;
				}
				if (dist > 256f)
				{
					this.active.Value = false;
					return;
				}
				Rectangle boundingBox = this.GetBoundingBox();
				if (Game1.player.currentLocation == location && Game1.player.CanBeDamaged() && boundingBox.Intersects(Game1.player.GetBoundingBox()))
				{
					Game1.player.takeDamage(25, false, null);
					this.ExplosionAnimation(location);
					this.active.Value = false;
					return;
				}
				foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
				{
					TerrainFeature feature;
					if (location.terrainFeatures.TryGetValue(tile, out feature) && !feature.isPassable(null))
					{
						this.ExplosionAnimation(location);
						this.active.Value = false;
						return;
					}
				}
				if (!location.isTileOnMap(this.position.Value / 64f) || location.isCollidingPosition(boundingBox, Game1.viewport, false, 0, true, parent, false, true, false, false))
				{
					this.ExplosionAnimation(location);
					this.active.Value = false;
					return;
				}
			}

			// Token: 0x060041BF RID: 16831 RVA: 0x00308E64 File Offset: 0x00307064
			public void Draw(SpriteBatch b)
			{
				if (!this.active.Value)
				{
					return;
				}
				float currentScale = 4f;
				Texture2D texture = Projectile.projectileSheet;
				Rectangle sourceRect = this.GetSourceRect();
				Vector2 pixelPosition = this.position.Value;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), new Rectangle?(sourceRect), Color.White * this.alpha, this.rotation, new Vector2(8f, 8f), currentScale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
			}

			// Token: 0x04002BCC RID: 11212
			public readonly NetBool active = new NetBool();

			// Token: 0x04002BCD RID: 11213
			public readonly NetVector2 position = new NetVector2();

			// Token: 0x04002BCE RID: 11214
			public readonly NetVector2 startPosition = new NetVector2();

			// Token: 0x04002BCF RID: 11215
			public readonly NetVector2 velocity = new NetVector2();

			// Token: 0x04002BD0 RID: 11216
			public float rotation;

			// Token: 0x04002BD1 RID: 11217
			public float alpha;
		}
	}
}
