using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Monsters
{
	// Token: 0x02000211 RID: 529
	public class BigSlime : Monster
	{
		// Token: 0x06002328 RID: 9000 RVA: 0x0017C404 File Offset: 0x0017A604
		public BigSlime()
		{
		}

		// Token: 0x06002329 RID: 9001 RVA: 0x0017C422 File Offset: 0x0017A622
		public BigSlime(Vector2 position, MineShaft mine) : this(position, mine.getMineArea(-1))
		{
			this.Sprite.ignoreStopAnimation = true;
			this.ignoreMovementAnimations = true;
			base.HideShadow = true;
		}

		// Token: 0x0600232A RID: 9002 RVA: 0x0017C44C File Offset: 0x0017A64C
		public BigSlime(Vector2 position, int mineArea) : base("Big Slime", position)
		{
			this.ignoreMovementAnimations = true;
			this.Sprite.ignoreStopAnimation = true;
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			this.Sprite.framesPerAnimation = 8;
			this.c.Value = Color.White;
			if (mineArea <= 10)
			{
				if (mineArea == 0 || mineArea == 10)
				{
					this.c.Value = Color.Lime;
				}
			}
			else if (mineArea != 40)
			{
				if (mineArea != 80)
				{
					if (mineArea == 121)
					{
						this.c.Value = Color.BlueViolet;
						base.Health *= 4;
						base.DamageToFarmer *= 3;
						base.ExperienceGained *= 3;
					}
				}
				else
				{
					this.c.Value = Color.Red;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
					base.ExperienceGained *= 3;
				}
			}
			else
			{
				this.c.Value = Color.Turquoise;
				base.Health *= 2;
				base.ExperienceGained *= 2;
			}
			int r = (int)this.c.R;
			int g = (int)this.c.G;
			int b = (int)this.c.B;
			r += Game1.random.Next(-20, 21);
			g += Game1.random.Next(-20, 21);
			b += Game1.random.Next(-20, 21);
			this.c.R = (byte)Math.Max(Math.Min(255, r), 0);
			this.c.G = (byte)Math.Max(Math.Min(255, g), 0);
			this.c.B = (byte)Math.Max(Math.Min(255, b), 0);
			this.c.Value *= (float)Game1.random.Next(7, 11) / 10f;
			this.Sprite.interval = 300f;
			base.HideShadow = true;
			if (Game1.random.NextDouble() < 0.01 && mineArea >= 40)
			{
				this.heldItem.Value = ItemRegistry.Create("(O)221", 1, 0, false);
			}
			if (Game1.mine != null && Game1.mine.GetAdditionalDifficulty() > 0)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					this.heldItem.Value = ItemRegistry.Create("(O)858", 1, 0, false);
				}
				else if (Game1.random.NextDouble() < 0.005)
				{
					this.heldItem.Value = ItemRegistry.Create("(O)896", 1, 0, false);
				}
			}
			if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD", null))
			{
				this.heldItem.Value = ItemRegistry.Create("(O)930", 1, 0, false);
			}
		}

		// Token: 0x0600232B RID: 9003 RVA: 0x0017C77D File Offset: 0x0017A97D
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.c, "c").AddField(this.heldItem, "heldItem");
		}

		// Token: 0x0600232C RID: 9004 RVA: 0x0017C7AC File Offset: 0x0017A9AC
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteWidth = 32;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.interval = 300f;
			this.Sprite.ignoreStopAnimation = true;
			this.ignoreMovementAnimations = true;
			base.HideShadow = true;
			this.Sprite.UpdateSourceRect();
			this.Sprite.framesPerAnimation = 8;
		}

		// Token: 0x0600232D RID: 9005 RVA: 0x0017C81C File Offset: 0x0017AA1C
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Slipperiness = 3;
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				base.IsWalkingTowardPlayer = true;
				if (base.Health <= 0)
				{
					base.deathAnimation();
					Stats stats = Game1.stats;
					uint slimesKilled = stats.SlimesKilled;
					stats.SlimesKilled = slimesKilled + 1U;
					if (Game1.gameMode == 3 && Game1.random.NextDouble() < 0.75)
					{
						int toCreate = Game1.random.Next(2, 5);
						for (int i = 0; i < toCreate; i++)
						{
							base.currentLocation.characters.Add(new GreenSlime(base.Position, Game1.CurrentMineLevel));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-2, 3), yTrajectory / 8 + Game1.random.Next(-2, 3));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].moveTowardPlayer(4);
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].currentLocation = base.currentLocation;
						}
					}
				}
			}
			return actualDamage;
		}

		// Token: 0x0600232E RID: 9006 RVA: 0x0017CA48 File Offset: 0x0017AC48
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, this.c.Value, 10, false, 70f, 0, -1, -1f, -1, 0));
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(-32f, 0f), this.c.Value, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 100
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(32f, 0f), this.c.Value, 10, false, 70f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 200
			});
			base.currentLocation.localSound("slimedead", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -32f), this.c.Value, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 300
			});
		}

		// Token: 0x0600232F RID: 9007 RVA: 0x0017CBB8 File Offset: 0x0017ADB8
		protected override void updateAnimation(GameTime time)
		{
			int currentIndex = this.Sprite.currentFrame;
			this.Sprite.AnimateDown(time, 0, "");
			if (this.isMoving())
			{
				this.Sprite.interval = 100f;
				this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.007853982f;
			}
			else
			{
				this.Sprite.interval = 200f;
				this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.003926991f;
			}
			if (Utility.isOnScreen(base.Position, 128) && this.Sprite.currentFrame == 0 && currentIndex == 7)
			{
				base.currentLocation.localSound("slimeHit", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06002330 RID: 9008 RVA: 0x0017CC97 File Offset: 0x0017AE97
		public override List<Item> getExtraDropItems()
		{
			if (this.heldItem.Value != null)
			{
				return new List<Item>
				{
					this.heldItem.Value
				};
			}
			return base.getExtraDropItems();
		}

		// Token: 0x06002331 RID: 9009 RVA: 0x0017CCC4 File Offset: 0x0017AEC4
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				int standingY = base.StandingPixel.Y;
				Item value = this.heldItem.Value;
				if (value != null)
				{
					value.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin((double)(this.heldObjectBobTimer + 1f)) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f, StackDrawType.Hide, Color.White, false);
				}
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.c.Value, this.rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, this.scale.Value), this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
				}
			}
		}

		// Token: 0x06002332 RID: 9010 RVA: 0x0017CED8 File Offset: 0x0017B0D8
		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 64);
		}

		// Token: 0x06002333 RID: 9011 RVA: 0x0017CF14 File Offset: 0x0017B114
		public override void shedChunks(int number, float scale)
		{
		}

		// Token: 0x040014E0 RID: 5344
		[XmlElement("c")]
		public readonly NetColor c = new NetColor();

		// Token: 0x040014E1 RID: 5345
		[XmlElement("heldObject")]
		public readonly NetRef<Item> heldItem = new NetRef<Item>();

		// Token: 0x040014E2 RID: 5346
		private float heldObjectBobTimer;
	}
}
