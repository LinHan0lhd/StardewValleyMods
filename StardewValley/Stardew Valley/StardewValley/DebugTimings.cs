using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x02000095 RID: 149
	public class DebugTimings
	{
		// Token: 0x06000685 RID: 1669 RVA: 0x000250EC File Offset: 0x000232EC
		public bool Toggle()
		{
			Game1 game = Game1.game1;
			if (!(((game != null) ? new bool?(game.IsMainInstance) : null) ?? false))
			{
				return false;
			}
			this.Active = !this.Active;
			return this.Active;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x00025144 File Offset: 0x00023344
		public void StartDrawTimer()
		{
			if (this.Active)
			{
				Game1 game = Game1.game1;
				bool? flag = (game != null) ? new bool?(game.IsMainInstance) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					this.StopwatchDraw.Restart();
				}
			}
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x00025198 File Offset: 0x00023398
		public void StopDrawTimer()
		{
			if (this.Active)
			{
				Game1 game = Game1.game1;
				bool? flag = (game != null) ? new bool?(game.IsMainInstance) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					this.StopwatchDraw.Stop();
					this.LastTimingDraw = this.StopwatchDraw.Elapsed.TotalMilliseconds;
				}
			}
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00025204 File Offset: 0x00023404
		public void StartUpdateTimer()
		{
			if (this.Active)
			{
				Game1 game = Game1.game1;
				bool? flag = (game != null) ? new bool?(game.IsMainInstance) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					this.StopwatchUpdate.Restart();
				}
			}
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x00025258 File Offset: 0x00023458
		public void StopUpdateTimer()
		{
			if (this.Active)
			{
				Game1 game = Game1.game1;
				bool? flag = (game != null) ? new bool?(game.IsMainInstance) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					this.StopwatchUpdate.Stop();
					this.LastTimingUpdate = this.StopwatchUpdate.Elapsed.TotalMilliseconds;
				}
			}
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x000252C4 File Offset: 0x000234C4
		public void Draw()
		{
			if (this.Active)
			{
				Game1 game = Game1.game1;
				if ((((game != null) ? new bool?(game.IsMainInstance) : null) ?? false) && Game1.spriteBatch != null && Game1.dialogueFont != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
					if (this.DrawTextWidth <= 0f)
					{
						SpriteFont dialogueFont = Game1.dialogueFont;
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Draw time: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(0, "00.00");
						defaultInterpolatedStringHandler.AppendLiteral(" ms  ");
						this.DrawTextWidth = dialogueFont.MeasureString(defaultInterpolatedStringHandler.ToStringAndClear()).X;
					}
					Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, 64), Color.Black * 0.5f);
					SpriteBatch spriteBatch = Game1.spriteBatch;
					SpriteFont dialogueFont2 = Game1.dialogueFont;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Draw time: ");
					defaultInterpolatedStringHandler.AppendFormatted<double>(this.LastTimingDraw, "00.00");
					defaultInterpolatedStringHandler.AppendLiteral(" ms  ");
					spriteBatch.DrawString(dialogueFont2, defaultInterpolatedStringHandler.ToStringAndClear(), DebugTimings.DrawPos, Color.White);
					SpriteBatch spriteBatch2 = Game1.spriteBatch;
					SpriteFont dialogueFont3 = Game1.dialogueFont;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Update time: ");
					defaultInterpolatedStringHandler.AppendFormatted<double>(this.LastTimingUpdate, "00.00");
					defaultInterpolatedStringHandler.AppendLiteral(" ms");
					spriteBatch2.DrawString(dialogueFont3, defaultInterpolatedStringHandler.ToStringAndClear(), new Vector2(DebugTimings.DrawPos.X + this.DrawTextWidth, DebugTimings.DrawPos.Y), Color.White);
					return;
				}
			}
		}

		// Token: 0x04000339 RID: 825
		private static readonly Vector2 DrawPos = Vector2.One * 12f;

		// Token: 0x0400033A RID: 826
		private readonly Stopwatch StopwatchDraw = new Stopwatch();

		// Token: 0x0400033B RID: 827
		private readonly Stopwatch StopwatchUpdate = new Stopwatch();

		// Token: 0x0400033C RID: 828
		private double LastTimingDraw;

		// Token: 0x0400033D RID: 829
		private double LastTimingUpdate;

		// Token: 0x0400033E RID: 830
		private float DrawTextWidth = -1f;

		// Token: 0x0400033F RID: 831
		private bool Active;
	}
}
