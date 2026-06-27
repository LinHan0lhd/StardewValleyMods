using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x0200021D RID: 541
	public class LavaLurk : Monster
	{
		// Token: 0x060023C2 RID: 9154 RVA: 0x00187014 File Offset: 0x00185214
		public LavaLurk()
		{
			this.Initialize();
		}

		// Token: 0x060023C3 RID: 9155 RVA: 0x00187090 File Offset: 0x00185290
		public LavaLurk(Vector2 position) : base("Lava Lurk", position)
		{
			this.Sprite.SpriteWidth = 16;
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
			this.Initialize();
			this.ignoreDamageLOS.Value = true;
			this.SetRandomMovement();
			this.stateTimer = Utility.RandomFloat(3f, 5f, null);
		}

		// Token: 0x060023C4 RID: 9156 RVA: 0x0018715F File Offset: 0x0018535F
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteWidth = 16;
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060023C5 RID: 9157 RVA: 0x00187190 File Offset: 0x00185390
		public virtual void Initialize()
		{
			base.HideShadow = true;
			this.submergedAnimation.AddRange(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(0, 750),
				new FarmerSprite.AnimationFrame(1, 1000)
			});
			this.lurkAnimation.AddRange(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(2, 250),
				new FarmerSprite.AnimationFrame(3, 250)
			});
			this.resubmergeAnimation.AddRange(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(3, 250),
				new FarmerSprite.AnimationFrame(2, 250),
				new FarmerSprite.AnimationFrame(1, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnDiveAnimationEnd), false)
			});
			List<FarmerSprite.AnimationFrame> list = this.emergeAnimation;
			FarmerSprite.AnimationFrame[] array;
			(array = new FarmerSprite.AnimationFrame[4])[0] = new FarmerSprite.AnimationFrame(2, 150);
			array[1] = new FarmerSprite.AnimationFrame(3, 150);
			array[2] = new FarmerSprite.AnimationFrame(4, 150);
			array[3] = new FarmerSprite.AnimationFrame(5, 150, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnEmergeAnimationEnd), true);
			list.AddRange(array);
			List<FarmerSprite.AnimationFrame> list2 = this.diveAnimation;
			FarmerSprite.AnimationFrame[] array2;
			(array2 = new FarmerSprite.AnimationFrame[4])[0] = new FarmerSprite.AnimationFrame(5, 150);
			array2[1] = new FarmerSprite.AnimationFrame(4, 150);
			array2[2] = new FarmerSprite.AnimationFrame(3, 150);
			array2[3] = new FarmerSprite.AnimationFrame(2, 150, false, false, new AnimatedSprite.endOfAnimationBehavior(this.OnDiveAnimationEnd), true);
			list2.AddRange(array2);
			List<FarmerSprite.AnimationFrame> list3 = this.idleAnimation;
			FarmerSprite.AnimationFrame[] array3;
			(array3 = new FarmerSprite.AnimationFrame[2])[0] = new FarmerSprite.AnimationFrame(5, 500);
			array3[1] = new FarmerSprite.AnimationFrame(6, 500);
			list3.AddRange(array3);
			this.fireAnimation.AddRange(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(7, 500)
			});
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x0018738F File Offset: 0x0018558F
		public virtual void OnEmergeAnimationEnd(Farmer who)
		{
			this.PlayAnimation(this.idleAnimation, true);
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x0018739F File Offset: 0x0018559F
		public virtual void OnDiveAnimationEnd(Farmer who)
		{
			this.PlayAnimation(this.submergedAnimation, true);
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x001873AF File Offset: 0x001855AF
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.currentState, "currentState");
		}

		// Token: 0x060023C9 RID: 9161 RVA: 0x001873D0 File Offset: 0x001855D0
		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie", null, null, SoundContext.Default);
			base.currentLocation.playSound("grunt", null, null, SoundContext.Default);
			Rectangle bounds = this.GetBoundingBox();
			for (int i = 0; i < 16; i++)
			{
				Game1.createRadialDebris(base.currentLocation, "Characters\\Monsters\\Pepper Rex", new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp((float)bounds.Left, (float)bounds.Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp((float)bounds.Bottom, (float)bounds.Top, (float)Game1.random.NextDouble()), 1, base.TilePoint.Y, Color.White, 4f);
			}
		}

		// Token: 0x060023CA RID: 9162 RVA: 0x001874B4 File Offset: 0x001856B4
		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			switch (this.currentState.Value)
			{
			case LavaLurk.State.Submerged:
				this.PlayAnimation(this.submergedAnimation, true);
				break;
			case LavaLurk.State.Lurking:
				if (this.PlayAnimation(this.lurkAnimation, false) && base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("waterSlosh", null);
				}
				break;
			case LavaLurk.State.Emerged:
				if (this.locallyPlayingAnimation != this.emergeAnimation && this.locallyPlayingAnimation != this.idleAnimation)
				{
					if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
					{
						Game1.playSound("waterSlosh", null);
					}
					this.PlayAnimation(this.emergeAnimation, false);
				}
				break;
			case LavaLurk.State.Firing:
				this.PlayAnimation(this.fireAnimation, true);
				break;
			case LavaLurk.State.Diving:
				if (this.locallyPlayingAnimation != this.diveAnimation && this.locallyPlayingAnimation != this.submergedAnimation && this.locallyPlayingAnimation != this.resubmergeAnimation)
				{
					if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
					{
						Game1.playSound("waterSlosh", null);
					}
					if (this.locallyPlayingAnimation == this.lurkAnimation)
					{
						this.PlayAnimation(this.resubmergeAnimation, false);
					}
					else
					{
						this.PlayAnimation(this.diveAnimation, false);
					}
				}
				break;
			}
			this.Sprite.animateOnce(time);
		}

		// Token: 0x060023CB RID: 9163 RVA: 0x00187660 File Offset: 0x00185860
		public virtual bool PlayAnimation(List<FarmerSprite.AnimationFrame> animation_to_play, bool loop)
		{
			if (this.locallyPlayingAnimation != animation_to_play)
			{
				this.locallyPlayingAnimation = animation_to_play;
				this.Sprite.setCurrentAnimation(animation_to_play);
				this.Sprite.loop = loop;
				if (!loop)
				{
					this.Sprite.oldFrame = animation_to_play.Last<FarmerSprite.AnimationFrame>().frame;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060023CC RID: 9164 RVA: 0x001876B4 File Offset: 0x001858B4
		public virtual bool TargetInRange()
		{
			return this.targettedFarmer != null && (Math.Abs(this.targettedFarmer.Position.X - base.Position.X) <= 640f && Math.Abs(this.targettedFarmer.Position.Y - base.Position.Y) <= 640f);
		}

		// Token: 0x060023CD RID: 9165 RVA: 0x0018771E File Offset: 0x0018591E
		public virtual void SetRandomMovement()
		{
			this.velocity = new Vector2((float)((Game1.random.Next(2) == 1) ? -1 : 1), (float)((Game1.random.Next(2) == 1) ? -1 : 1));
		}

		// Token: 0x060023CE RID: 9166 RVA: 0x00187751 File Offset: 0x00185951
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
		}

		// Token: 0x060023CF RID: 9167 RVA: 0x00187753 File Offset: 0x00185953
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (this.currentState.Value == LavaLurk.State.Submerged)
			{
				return -1;
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		// Token: 0x060023D0 RID: 9168 RVA: 0x00187774 File Offset: 0x00185974
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.targettedFarmer == null || this.targettedFarmer.currentLocation != base.currentLocation)
			{
				this.targettedFarmer = null;
				this.targettedFarmer = this.findPlayer();
			}
			if (this.stateTimer > 0f)
			{
				this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.stateTimer <= 0f)
				{
					this.stateTimer = 0f;
				}
			}
			switch (this.currentState.Value)
			{
			case LavaLurk.State.Submerged:
				this.swimSpeed = 2;
				if (this.stateTimer == 0f)
				{
					this.currentState.Value = LavaLurk.State.Lurking;
					this.stateTimer = 1f;
				}
				break;
			case LavaLurk.State.Lurking:
				this.swimSpeed = 1;
				if (this.stateTimer == 0f)
				{
					if (this.TargetInRange())
					{
						this.currentState.Value = LavaLurk.State.Emerged;
						this.stateTimer = 1f;
						this.swimSpeed = 0;
					}
					else
					{
						this.currentState.Value = LavaLurk.State.Diving;
						this.stateTimer = 1f;
					}
				}
				break;
			case LavaLurk.State.Emerged:
				if (this.stateTimer == 0f)
				{
					this.currentState.Value = LavaLurk.State.Firing;
					this.stateTimer = 1f;
					this.fireTimer = 0.25f;
				}
				break;
			case LavaLurk.State.Firing:
				if (this.stateTimer == 0f)
				{
					this.currentState.Value = LavaLurk.State.Diving;
					this.stateTimer = 1f;
				}
				if (this.fireTimer > 0f)
				{
					this.fireTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this.fireTimer <= 0f)
					{
						this.fireTimer = 0.25f;
						if (this.targettedFarmer != null)
						{
							Vector2 shot_origin = base.Position + new Vector2(0f, -32f);
							Vector2 shot_velocity = this.targettedFarmer.Position - shot_origin;
							shot_velocity.Normalize();
							shot_velocity *= 7f;
							base.currentLocation.playSound("fireball", null, null, SoundContext.Default);
							BasicProjectile projectile = new BasicProjectile(25, 10, 0, 3, 0.19634955f, shot_velocity.X, shot_velocity.Y, shot_origin, null, null, null, false, false, base.currentLocation, this, null, null);
							projectile.ignoreLocationCollision.Value = true;
							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.maxTravelDistance.Value = 640;
							base.currentLocation.projectiles.Add(projectile);
						}
					}
				}
				break;
			case LavaLurk.State.Diving:
				if (this.stateTimer == 0f)
				{
					this.currentState.Value = LavaLurk.State.Submerged;
					this.stateTimer = Utility.RandomFloat(3f, 5f, null);
					this.approachFarmer = !this.approachFarmer;
					if (this.approachFarmer)
					{
						this.targettedFarmer = this.findPlayer();
					}
					this.SetRandomMovement();
				}
				break;
			}
			if (this.targettedFarmer != null && this.approachFarmer)
			{
				Point curTile = base.TilePoint;
				Point playerTile = this.targettedFarmer.TilePoint;
				if (curTile.X > playerTile.X)
				{
					this.velocity.X = -1f;
				}
				else if (curTile.X < playerTile.X)
				{
					this.velocity.X = 1f;
				}
				if (curTile.Y > playerTile.Y)
				{
					this.velocity.Y = -1f;
				}
				else if (curTile.Y < playerTile.Y)
				{
					this.velocity.Y = 1f;
				}
			}
			if (this.velocity.X != 0f || this.velocity.Y != 0f)
			{
				Rectangle next_bounds = this.GetBoundingBox();
				Vector2 next_position = base.Position;
				next_bounds.Inflate(48, 48);
				next_bounds.X += (int)this.velocity.X * this.swimSpeed;
				next_position.X += (float)((int)this.velocity.X * this.swimSpeed);
				if (!this.CheckInWater(next_bounds))
				{
					this.velocity.X = this.velocity.X * -1f;
					next_bounds.X += (int)this.velocity.X * this.swimSpeed;
					next_position.X += (float)((int)this.velocity.X * this.swimSpeed);
				}
				next_bounds.Y += (int)this.velocity.Y * this.swimSpeed;
				next_position.Y += (float)((int)this.velocity.Y * this.swimSpeed);
				if (!this.CheckInWater(next_bounds))
				{
					this.velocity.Y = this.velocity.Y * -1f;
					next_bounds.Y += (int)this.velocity.Y * this.swimSpeed;
					next_position.Y += (float)((int)this.velocity.Y * this.swimSpeed);
				}
				if (base.Position != next_position)
				{
					base.Position = next_position;
				}
			}
		}

		// Token: 0x060023D1 RID: 9169 RVA: 0x00187CBA File Offset: 0x00185EBA
		public static bool IsLavaTile(GameLocation location, int x, int y)
		{
			return location.isWaterTile(x, y);
		}

		// Token: 0x060023D2 RID: 9170 RVA: 0x00187CC4 File Offset: 0x00185EC4
		public bool CheckInWater(Rectangle position)
		{
			for (int x = position.Left / 64; x <= position.Right / 64; x++)
			{
				for (int y = position.Top / 64; y <= position.Bottom / 64; y++)
				{
					if (!LavaLurk.IsLavaTile(base.currentLocation, x, y))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060023D3 RID: 9171 RVA: 0x00187D1F File Offset: 0x00185F1F
		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		// Token: 0x060023D4 RID: 9172 RVA: 0x00187D21 File Offset: 0x00185F21
		public override Debris ModifyMonsterLoot(Debris debris)
		{
			if (debris != null)
			{
				debris.chunksMoveTowardPlayer = true;
			}
			return debris;
		}

		// Token: 0x04001538 RID: 5432
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> submergedAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x04001539 RID: 5433
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> lurkAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153A RID: 5434
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> emergeAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153B RID: 5435
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> diveAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153C RID: 5436
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> resubmergeAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153D RID: 5437
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> idleAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153E RID: 5438
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> fireAnimation = new List<FarmerSprite.AnimationFrame>();

		// Token: 0x0400153F RID: 5439
		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> locallyPlayingAnimation;

		// Token: 0x04001540 RID: 5440
		[XmlIgnore]
		public bool approachFarmer;

		// Token: 0x04001541 RID: 5441
		[XmlIgnore]
		public Vector2 velocity = Vector2.Zero;

		// Token: 0x04001542 RID: 5442
		[XmlIgnore]
		public int swimSpeed;

		// Token: 0x04001543 RID: 5443
		[XmlIgnore]
		public Farmer targettedFarmer;

		// Token: 0x04001544 RID: 5444
		[XmlIgnore]
		public NetEnum<LavaLurk.State> currentState = new NetEnum<LavaLurk.State>();

		// Token: 0x04001545 RID: 5445
		[XmlIgnore]
		public float stateTimer;

		// Token: 0x04001546 RID: 5446
		[XmlIgnore]
		public float fireTimer;

		// Token: 0x0200058A RID: 1418
		public enum State
		{
			// Token: 0x04002BD8 RID: 11224
			Submerged,
			// Token: 0x04002BD9 RID: 11225
			Lurking,
			// Token: 0x04002BDA RID: 11226
			Emerged,
			// Token: 0x04002BDB RID: 11227
			Firing,
			// Token: 0x04002BDC RID: 11228
			Diving
		}
	}
}
