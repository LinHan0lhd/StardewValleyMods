using System;
using Microsoft.Xna.Framework;
using StardewValley.Audio;

namespace StardewValley.Monsters
{
	// Token: 0x02000226 RID: 550
	public class ShadowBrute : Monster
	{
		// Token: 0x06002479 RID: 9337 RVA: 0x0018E863 File Offset: 0x0018CA63
		public ShadowBrute()
		{
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x0018E86B File Offset: 0x0018CA6B
		public ShadowBrute(Vector2 position) : base("Shadow Brute", position)
		{
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x0018E891 File Offset: 0x0018CA91
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Brute");
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x0018E8BC File Offset: 0x0018CABC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			base.currentLocation.playSound("shadowHit", null, null, SoundContext.Default);
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x0018E8FC File Offset: 0x0018CAFC
		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0), base.currentLocation, 4, 64, 64);
			for (int i = 1; i < 3; i++)
			{
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, 1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, -1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 0f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 0f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
			}
			base.currentLocation.localSound("shadowDie", null, null, SoundContext.Default);
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x0018EB2C File Offset: 0x0018CD2C
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 2, this.Sprite.SourceRect.Y + 5, 16, 5), 10, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x0018EC14 File Offset: 0x0018CE14
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.isMoving())
			{
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
					break;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					return;
				default:
					return;
				}
			}
		}
	}
}
