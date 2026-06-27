using System;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A5 RID: 933
	public class ScreenFade
	{
		// Token: 0x060038D7 RID: 14551 RVA: 0x002D0508 File Offset: 0x002CE708
		public ScreenFade(Func<bool> onFadeToBlack, Action onFadeIn)
		{
			this.onFadeToBlackComplete = onFadeToBlack;
			this.onFadedBackInComplete = onFadeIn;
		}

		// Token: 0x060038D8 RID: 14552 RVA: 0x002D0528 File Offset: 0x002CE728
		public bool UpdateFade(GameTime time)
		{
			if (this.fadeToBlack && (Game1.pauseTime == 0f || Game1.eventUp))
			{
				if (this.fadeToBlackAlpha > 1.1f && !Game1.messagePause)
				{
					this.fadeToBlackAlpha = 1f;
					if (this.onFadeToBlackComplete())
					{
						return true;
					}
					this.nonWarpFade = false;
					this.fadeIn = false;
					if (this.afterFade != null)
					{
						Game1.afterFadeFunction afterFadeFunction = this.afterFade;
						this.afterFade = null;
						afterFadeFunction();
					}
					this.globalFade = false;
				}
				if (this.fadeToBlackAlpha < -0.1f)
				{
					this.fadeToBlackAlpha = 0f;
					this.fadeToBlack = false;
					this.onFadedBackInComplete();
				}
				this.UpdateFadeAlpha(time);
			}
			return false;
		}

		// Token: 0x060038D9 RID: 14553 RVA: 0x002D05E8 File Offset: 0x002CE7E8
		public void UpdateFadeAlpha(GameTime time)
		{
			if (this.fadeIn)
			{
				this.fadeToBlackAlpha += ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
				this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 1.2f : this.fadeToBlackAlpha);
				return;
			}
			if (!Game1.messagePause && !Game1.dialogueUp)
			{
				this.fadeToBlackAlpha -= ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
				this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? -0.2f : this.fadeToBlackAlpha);
			}
		}

		// Token: 0x060038DA RID: 14554 RVA: 0x002D06AE File Offset: 0x002CE8AE
		public void FadeScreenToBlack(float startAlpha = 0f, bool stopMovement = true)
		{
			this.globalFade = false;
			this.fadeToBlack = true;
			this.fadeIn = true;
			this.fadeToBlackAlpha = startAlpha;
			if (stopMovement)
			{
				Game1.player.CanMove = false;
			}
		}

		// Token: 0x060038DB RID: 14555 RVA: 0x002D06DA File Offset: 0x002CE8DA
		public void FadeClear(float startAlpha = 1f)
		{
			this.globalFade = false;
			this.fadeIn = false;
			this.fadeToBlack = true;
			this.fadeToBlackAlpha = startAlpha;
		}

		// Token: 0x060038DC RID: 14556 RVA: 0x002D06F8 File Offset: 0x002CE8F8
		public void GlobalFadeToBlack(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			if (this.fadeToBlack && !this.fadeIn)
			{
				this.onFadedBackInComplete();
			}
			this.fadeToBlack = false;
			this.globalFade = true;
			this.fadeIn = false;
			this.afterFade = afterFade;
			this.globalFadeSpeed = fadeSpeed;
			this.fadeToBlackAlpha = 0f;
		}

		// Token: 0x060038DD RID: 14557 RVA: 0x002D0750 File Offset: 0x002CE950
		public void GlobalFadeToClear(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			if (this.fadeToBlack && this.fadeIn)
			{
				this.onFadeToBlackComplete();
			}
			this.fadeToBlack = false;
			this.globalFade = true;
			this.fadeIn = true;
			this.afterFade = afterFade;
			this.globalFadeSpeed = fadeSpeed;
			this.fadeToBlackAlpha = 1f;
		}

		// Token: 0x060038DE RID: 14558 RVA: 0x002D07A8 File Offset: 0x002CE9A8
		public void UpdateGlobalFade()
		{
			if (this.fadeIn)
			{
				if (this.fadeToBlackAlpha <= 0f)
				{
					this.globalFade = false;
					if (this.afterFade != null)
					{
						Game1.afterFadeFunction tmp = this.afterFade;
						this.afterFade();
						if (this.afterFade != null && this.afterFade.Equals(tmp))
						{
							this.afterFade = null;
						}
						if (Game1.nonWarpFade)
						{
							this.fadeToBlack = false;
						}
					}
				}
				this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 0f : Math.Max(0f, this.fadeToBlackAlpha - this.globalFadeSpeed));
				return;
			}
			if (this.fadeToBlackAlpha >= 1f)
			{
				this.globalFade = false;
				if (this.afterFade != null)
				{
					Game1.afterFadeFunction tmp2 = this.afterFade;
					this.afterFade();
					if (this.afterFade != null && this.afterFade.Equals(tmp2))
					{
						this.afterFade = null;
					}
					if (Game1.nonWarpFade)
					{
						this.fadeToBlack = false;
					}
				}
			}
			this.fadeToBlackAlpha = (Game1.IsDedicatedHost ? 1f : Math.Min(1f, this.fadeToBlackAlpha + this.globalFadeSpeed));
		}

		// Token: 0x04002554 RID: 9556
		public bool globalFade;

		// Token: 0x04002555 RID: 9557
		public bool fadeIn = true;

		// Token: 0x04002556 RID: 9558
		public bool fadeToBlack;

		// Token: 0x04002557 RID: 9559
		public bool nonWarpFade;

		// Token: 0x04002558 RID: 9560
		public float fadeToBlackAlpha;

		// Token: 0x04002559 RID: 9561
		public float globalFadeSpeed;

		// Token: 0x0400255A RID: 9562
		private const float fadeToFudge = 0.1f;

		// Token: 0x0400255B RID: 9563
		private Game1.afterFadeFunction afterFade;

		// Token: 0x0400255C RID: 9564
		private Func<bool> onFadeToBlackComplete;

		// Token: 0x0400255D RID: 9565
		private Action onFadedBackInComplete;
	}
}
