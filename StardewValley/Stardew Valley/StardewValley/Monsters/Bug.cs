using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.Monsters
{
	// Token: 0x02000213 RID: 531
	public class Bug : Monster
	{
		// Token: 0x06002340 RID: 9024 RVA: 0x0017DE77 File Offset: 0x0017C077
		public Bug()
		{
		}

		// Token: 0x06002341 RID: 9025 RVA: 0x0017DE8C File Offset: 0x0017C08C
		public Bug(Vector2 position, int facingDirection, string specialType) : this(position, 0)
		{
			this.faceDirection(facingDirection);
			if (specialType.Contains("Assassin"))
			{
				this.Sprite.LoadTexture("Characters\\Monsters\\Assassin Bug", true);
				base.DamageToFarmer = 50;
				base.Health = 500;
				int speed = base.speed;
				base.speed = speed + 1;
			}
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x0017DEEC File Offset: 0x0017C0EC
		public Bug(Vector2 position, int areaType) : base("Bug", position)
		{
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
			this.onCollision = new Monster.collisionBehavior(this.collide);
			this.yOffset = -32f;
			base.IsWalkingTowardPlayer = false;
			base.setMovingInFacingDirection();
			this.defaultAnimationInterval.Value = 40;
			this.collidesWithOtherCharacters.Value = false;
			if (areaType == 121)
			{
				this.isArmoredBug.Value = true;
				this.Sprite.LoadTexture("Characters\\Monsters\\Armored Bug", true);
				base.DamageToFarmer *= 2;
				base.Slipperiness = -1;
				base.Health = 150;
			}
			base.HideShadow = true;
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x0017DFB5 File Offset: 0x0017C1B5
		public Bug(Vector2 position, int facingDirection, MineShaft mine) : this(position, mine.getMineArea(-1))
		{
			this.faceDirection(facingDirection);
			base.HideShadow = true;
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x0017DFD3 File Offset: 0x0017C1D3
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.isArmoredBug, "isArmoredBug");
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x0017DFF2 File Offset: 0x0017C1F2
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			this.Sprite.faceDirection(this.FacingDirection);
			this.Sprite.animateOnce(time);
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x0017E012 File Offset: 0x0017C212
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteHeight = 16;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x0017E034 File Offset: 0x0017C234
		private void collide(GameLocation location)
		{
			Rectangle bb = this.nextPosition(this.FacingDirection);
			using (FarmerCollection.Enumerator enumerator = location.farmers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetBoundingBox().Intersects(bb))
					{
						return;
					}
				}
			}
			this.FacingDirection = (this.FacingDirection + 2) % 4;
			base.setMovingInFacingDirection();
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x0017E0B4 File Offset: 0x0017C2B4
		public override void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			this.FacingDirection = Math.Abs((this.FacingDirection + Game1.random.Next(-1, 2)) % 4);
			this.Halt();
			base.setMovingInFacingDirection();
			base.BuffForAdditionalDifficulty(additional_difficulty);
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x0017E0EC File Offset: 0x0017C2EC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (this.isArmoredBug.Value)
			{
				if (!isBomb)
				{
					MeleeWeapon weapon = who.CurrentTool as MeleeWeapon;
					if (weapon != null && weapon.hasEnchantmentOfType<BugKillerEnchantment>())
					{
						goto IL_62;
					}
				}
				base.currentLocation.playSound("crafting", null, null, SoundContext.Default);
				return 0;
			}
			IL_62:
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				if (this.isHardModeMonster.Value)
				{
					this.FacingDirection = Math.Abs((this.FacingDirection + Game1.random.Next(-1, 2)) % 4);
					this.Halt();
					base.setMovingInFacingDirection();
				}
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x0600234A RID: 9034 RVA: 0x0017E210 File Offset: 0x0017C410
		public override List<Item> getExtraDropItems()
		{
			if (this.isArmoredBug.Value)
			{
				List<Item> additional_drops = new List<Item>();
				if (Game1.random.NextDouble() <= 0.1)
				{
					additional_drops.Add(ItemRegistry.Create("(O)874", 1, 0, false));
				}
				return additional_drops;
			}
			return base.getExtraDropItems();
		}

		// Token: 0x0600234B RID: 9035 RVA: 0x0017E260 File Offset: 0x0017C460
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				Vector2 offset = default(Vector2);
				if (this.FacingDirection % 2 == 0)
				{
					offset.X = (float)(Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds / 1000f) * 6.283185307179586) * 10.0);
				}
				else
				{
					offset.Y = (float)(Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds / 1000f) * 6.283185307179586) * 10.0);
				}
				int standingY = base.StandingPixel.Y;
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f + offset.X, (float)(this.GetBoundingBox().Height * 5 / 2 - 48)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, Utility.PointToVector2(Game1.shadowTexture.Bounds.Center), (4f + (float)this.yJumpOffset / 40f) * this.scale.Value, SpriteEffects.None, Math.Max(0f, (float)standingY / 10000f) - 1E-06f);
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)this.yJumpOffset) + offset, new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(8f, 16f), 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			}
		}

		// Token: 0x0600234C RID: 9036 RVA: 0x0017E470 File Offset: 0x0017C670
		protected override void localDeathAnimation()
		{
			base.localDeathAnimation();
			base.currentLocation.localSound("slimedead", null, null, SoundContext.Default);
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -32f), Color.Violet, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 4, 64, 64);
		}

		// Token: 0x0600234D RID: 9037 RVA: 0x0017E50C File Offset: 0x0017C70C
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, this.Sprite.getHeight() * 4, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
		}

		// Token: 0x040014EB RID: 5355
		[XmlElement("isArmoredBug")]
		public readonly NetBool isArmoredBug = new NetBool(false);
	}
}
