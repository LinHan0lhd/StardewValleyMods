using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace StardewValley.Monsters
{
	// Token: 0x02000222 RID: 546
	public class Mummy : Monster
	{
		// Token: 0x0600243C RID: 9276 RVA: 0x0018B3FF File Offset: 0x001895FF
		public Mummy()
		{
		}

		// Token: 0x0600243D RID: 9277 RVA: 0x0018B420 File Offset: 0x00189620
		public Mummy(Vector2 position) : base("Mummy", position)
		{
			this.Sprite.SpriteHeight = 32;
			this.Sprite.ignoreStopAnimation = true;
			this.Sprite.UpdateSourceRect();
			this._damageToFarmer = this.damageToFarmer.Value;
		}

		// Token: 0x0600243E RID: 9278 RVA: 0x0018B488 File Offset: 0x00189688
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.crumbleEvent, "crumbleEvent").AddField(this.reviveTimer, "reviveTimer");
			this.crumbleEvent.onEvent += this.performCrumble;
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x0600243F RID: 9279 RVA: 0x0018B4EA File Offset: 0x001896EA
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Mummy");
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			this.Sprite.ignoreStopAnimation = true;
		}

		// Token: 0x06002440 RID: 9280 RVA: 0x0018B520 File Offset: 0x00189720
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (this.reviveTimer.Value <= 0)
			{
				if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
				{
					actualDamage = -1;
				}
				else
				{
					base.Slipperiness = 2;
					base.Health -= actualDamage;
					base.setTrajectory(xTrajectory, yTrajectory);
					base.currentLocation.playSound("shadowHit", null, null, SoundContext.Default);
					base.currentLocation.playSound("skeletonStep", null, null, SoundContext.Default);
					base.IsWalkingTowardPlayer = true;
					if (base.Health <= 0)
					{
						if (!isBomb)
						{
							MeleeWeapon weapon = who.CurrentTool as MeleeWeapon;
							if (weapon != null && weapon.hasEnchantmentOfType<CrusaderEnchantment>())
							{
								Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.BlueViolet, 10, false, 100f, 0, -1, -1f, -1, 0)
								{
									holdLastFrame = true,
									alphaFade = 0.01f,
									interval = 70f
								}, base.currentLocation, 4, 64, 64);
								base.currentLocation.playSound("ghost", null, null, SoundContext.Default);
								return actualDamage;
							}
						}
						this.reviveTimer.Value = 10000;
						base.Health = base.MaxHealth;
						base.deathAnimation();
					}
				}
				return actualDamage;
			}
			if (isBomb)
			{
				base.Health = 0;
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.BlueViolet, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					holdLastFrame = true,
					alphaFade = 0.01f,
					interval = 70f
				}, base.currentLocation, 4, 64, 64);
				base.currentLocation.playSound("ghost", null, null, SoundContext.Default);
				return 999;
			}
			return -1;
		}

		// Token: 0x06002441 RID: 9281 RVA: 0x0018B735 File Offset: 0x00189935
		public override void defaultMovementBehavior(GameTime time)
		{
			if (this.reviveTimer.Value > 0)
			{
				return;
			}
			base.defaultMovementBehavior(time);
		}

		// Token: 0x06002442 RID: 9282 RVA: 0x0018B750 File Offset: 0x00189950
		public override List<Item> getExtraDropItems()
		{
			List<Item> items = new List<Item>();
			if (Game1.random.NextDouble() < 0.002)
			{
				items.Add(ItemRegistry.Create("(O)485", 1, 0, false));
			}
			return items;
		}

		// Token: 0x06002443 RID: 9283 RVA: 0x0018B78C File Offset: 0x0018998C
		protected override void sharedDeathAnimation()
		{
			this.Halt();
			this.crumble(false);
			this.collidesWithOtherCharacters.Value = false;
			base.IsWalkingTowardPlayer = false;
			this.moveTowardPlayerThreshold.Value = -1;
		}

		// Token: 0x06002444 RID: 9284 RVA: 0x0018B7BA File Offset: 0x001899BA
		protected override void localDeathAnimation()
		{
		}

		// Token: 0x06002445 RID: 9285 RVA: 0x0018B7BC File Offset: 0x001899BC
		public override void update(GameTime time, GameLocation location)
		{
			this.crumbleEvent.Poll();
			if (this.reviveTimer.Value > 0 && this.Sprite.CurrentAnimation == null && this.Sprite.currentFrame != 19)
			{
				this.Sprite.currentFrame = 19;
			}
			base.update(time, location);
		}

		// Token: 0x06002446 RID: 9286 RVA: 0x0018B813 File Offset: 0x00189A13
		private void crumble(bool reverse = false)
		{
			this.crumbleEvent.Fire(reverse);
		}

		// Token: 0x06002447 RID: 9287 RVA: 0x0018B824 File Offset: 0x00189A24
		private void performCrumble(bool reverse)
		{
			this.Sprite.setCurrentAnimation(this.getCrumbleAnimation(reverse));
			if (!reverse)
			{
				if (Game1.IsMasterGame)
				{
					this.damageToFarmer.Value = 0;
				}
				this.reviveTimer.Value = 10000;
				base.currentLocation.localSound("monsterdead", null, null, SoundContext.Default);
				return;
			}
			if (Game1.IsMasterGame)
			{
				this.damageToFarmer.Value = this._damageToFarmer;
			}
			this.reviveTimer.Value = 0;
			base.currentLocation.localSound("skeletonDie", null, null, SoundContext.Default);
		}

		// Token: 0x06002448 RID: 9288 RVA: 0x0018B8D4 File Offset: 0x00189AD4
		private List<FarmerSprite.AnimationFrame> getCrumbleAnimation(bool reverse = false)
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
			if (!reverse)
			{
				animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, false, false, null, false, 0));
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.behaviorAfterRevival), true, 0));
			}
			animation.Add(new FarmerSprite.AnimationFrame(17, 100, 0, false, false, null, false, 0));
			animation.Add(new FarmerSprite.AnimationFrame(18, 100, 0, false, false, null, false, 0));
			if (!reverse)
			{
				animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.behaviorAfterCrumble), false, 0));
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, false, false, null, false, 0));
			}
			if (reverse)
			{
				animation.Reverse();
			}
			return animation;
		}

		// Token: 0x06002449 RID: 9289 RVA: 0x0018B98F File Offset: 0x00189B8F
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.reviveTimer.Value <= 0 && this.withinPlayerThreshold())
			{
				base.IsWalkingTowardPlayer = true;
			}
			base.behaviorAtGameTick(time);
		}

		// Token: 0x0600244A RID: 9290 RVA: 0x0018B9B8 File Offset: 0x00189BB8
		protected override void updateAnimation(GameTime time)
		{
			if (this.Sprite.CurrentAnimation != null)
			{
				if (this.Sprite.animateOnce(time))
				{
					this.Sprite.CurrentAnimation = null;
				}
			}
			else if (this.reviveTimer.Value > 0)
			{
				this.reviveTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if (this.reviveTimer.Value < 2000)
				{
					base.shake(this.reviveTimer.Value);
				}
				if (this.reviveTimer.Value <= 0)
				{
					if (Game1.IsMasterGame)
					{
						this.crumble(true);
						base.IsWalkingTowardPlayer = true;
					}
					else
					{
						this.reviveTimer.Value = 1;
					}
				}
			}
			else if (!Game1.IsMasterGame)
			{
				if (this.isMoving())
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
				}
				else
				{
					this.Sprite.StopAnimation();
				}
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x0600244B RID: 9291 RVA: 0x0018BB13 File Offset: 0x00189D13
		private void behaviorAfterCrumble(Farmer who)
		{
			this.Halt();
			this.Sprite.currentFrame = 19;
			this.Sprite.CurrentAnimation = null;
		}

		// Token: 0x0600244C RID: 9292 RVA: 0x0018BB34 File Offset: 0x00189D34
		private void behaviorAfterRevival(Farmer who)
		{
			base.IsWalkingTowardPlayer = true;
			this.collidesWithOtherCharacters.Value = true;
			this.Sprite.currentFrame = 0;
			this.Sprite.oldFrame = 0;
			this.moveTowardPlayerThreshold.Value = 8;
			this.Sprite.CurrentAnimation = null;
		}

		// Token: 0x04001579 RID: 5497
		public NetInt reviveTimer = new NetInt(0);

		// Token: 0x0400157A RID: 5498
		public const int revivalTime = 10000;

		// Token: 0x0400157B RID: 5499
		protected int _damageToFarmer;

		// Token: 0x0400157C RID: 5500
		private readonly NetEvent1Field<bool, NetBool> crumbleEvent = new NetEvent1Field<bool, NetBool>();
	}
}
