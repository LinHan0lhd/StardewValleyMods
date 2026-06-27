using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200039A RID: 922
	public class MoneyDial
	{
		// Token: 0x0600386E RID: 14446 RVA: 0x002CA70C File Offset: 0x002C890C
		public MoneyDial(int numDigits, bool playSound = true)
		{
			this.numDigits = numDigits;
			this.playSounds = playSound;
			this.currentValue = 0;
			if (Game1.player != null)
			{
				this.currentValue = Game1.player.Money;
			}
			this.onPlaySound = new Action<int>(this.playDefaultSound);
		}

		// Token: 0x0600386F RID: 14447 RVA: 0x002CA778 File Offset: 0x002C8978
		public void playDefaultSound(int direction)
		{
			if (direction > 0)
			{
				Game1.playSound("moneyDial", null);
			}
		}

		// Token: 0x06003870 RID: 14448 RVA: 0x002CA7A0 File Offset: 0x002C89A0
		public void draw(SpriteBatch b, Vector2 position, int target)
		{
			if (this.previousTargetValue != target)
			{
				this.speed = (target - this.currentValue) / 100;
				this.previousTargetValue = target;
				this.soundTimer = Math.Max(6, 100 / (Math.Abs(this.speed) + 1));
			}
			if (this.moneyShineTimer > 0 && this.currentValue == target)
			{
				this.moneyShineTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (this.moneyMadeAccumulator > 0)
			{
				this.moneyMadeAccumulator -= (Math.Abs(this.speed / 2) + 1) * ((this.animations.Count <= 0) ? 100 : 1);
				if (this.moneyMadeAccumulator <= 0)
				{
					this.moneyShineTimer = this.numDigits * 60;
				}
			}
			if (this.ShouldShakeMainMoneyBox && this.moneyMadeAccumulator > 2000)
			{
				Game1.dayTimeMoneyBox.moneyShakeTimer = 100;
			}
			if (this.currentValue != target)
			{
				this.currentValue += this.speed + ((this.currentValue < target) ? 1 : -1);
				if (this.currentValue < target)
				{
					this.moneyMadeAccumulator += Math.Abs(this.speed);
				}
				this.soundTimer--;
				if (Math.Abs(target - this.currentValue) <= this.speed + 1 || (this.speed != 0 && Math.Sign(target - this.currentValue) != Math.Sign(this.speed)))
				{
					this.currentValue = target;
				}
				if (this.soundTimer <= 0)
				{
					if (this.playSounds)
					{
						Action<int> action = this.onPlaySound;
						if (action != null)
						{
							action(Math.Sign(target - this.currentValue));
						}
					}
					this.soundTimer = Math.Max(6, 100 / (Math.Abs(this.speed) + 1));
					if (Game1.random.NextDouble() < 0.4)
					{
						if (target > this.currentValue)
						{
							this.animations.Add(TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(Game1.random.Next(10, 12), position + new Vector2((float)Game1.random.Next(30, 190), (float)Game1.random.Next(-32, 48)), Color.Gold, 8, false, 100f, 0, -1, -1f, -1, 0));
						}
						else if (target < this.currentValue)
						{
							TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(356, 449, 1, 1), 999999f, 1, 44, position + new Vector2((float)Game1.random.Next(160), (float)Game1.random.Next(-32, 32)), false, false, 1f, 0.01f, Color.White, (float)(Game1.random.Next(1, 3) * 4), -0.001f, 0f, 0f, false);
							sprite.motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(-30, -5) / 10f);
							sprite.acceleration = new Vector2(0f, 0.25f);
							this.animations.Add(sprite);
						}
					}
				}
			}
			for (int i = this.animations.Count - 1; i >= 0; i--)
			{
				if (this.animations[i].update(Game1.currentGameTime))
				{
					this.animations.RemoveAt(i);
				}
				else
				{
					this.animations[i].draw(b, true, 0, 0, 1f);
				}
			}
			int xPosition = 0;
			int digitStrip = (int)Math.Pow(10.0, (double)(this.numDigits - 1));
			bool significant = false;
			for (int j = 0; j < this.numDigits; j++)
			{
				int currentDigit = this.currentValue / digitStrip % 10;
				if (currentDigit > 0 || j == this.numDigits - 1)
				{
					significant = true;
				}
				if (significant)
				{
					b.Draw(Game1.mouseCursors, position + new Vector2((float)xPosition, (Game1.activeClickableMenu is ShippingMenu && this.currentValue >= 1000000) ? ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.53096771240234 + (double)j) * (float)(this.currentValue / 1000000)) : 0f), new Rectangle?(new Rectangle(286, 502 - currentDigit * 8, 5, 8)), Color.Maroon, 0f, Vector2.Zero, 4f + ((this.moneyShineTimer / 60 == this.numDigits - j) ? 0.3f : 0f), SpriteEffects.None, 1f);
				}
				xPosition += 24;
				digitStrip /= 10;
			}
		}

		// Token: 0x040024DE RID: 9438
		public const int digitHeight = 8;

		// Token: 0x040024DF RID: 9439
		public int numDigits;

		// Token: 0x040024E0 RID: 9440
		public int currentValue;

		// Token: 0x040024E1 RID: 9441
		public int previousTargetValue;

		// Token: 0x040024E2 RID: 9442
		public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

		// Token: 0x040024E3 RID: 9443
		private int speed;

		// Token: 0x040024E4 RID: 9444
		private int soundTimer;

		// Token: 0x040024E5 RID: 9445
		private int moneyMadeAccumulator;

		// Token: 0x040024E6 RID: 9446
		private int moneyShineTimer;

		// Token: 0x040024E7 RID: 9447
		private bool playSounds = true;

		// Token: 0x040024E8 RID: 9448
		public Action<int> onPlaySound;

		// Token: 0x040024E9 RID: 9449
		public bool ShouldShakeMainMoneyBox = true;
	}
}
