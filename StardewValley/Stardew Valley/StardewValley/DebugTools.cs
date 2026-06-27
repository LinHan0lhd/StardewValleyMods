using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x02000096 RID: 150
	public static class DebugTools
	{
		// Token: 0x0600068D RID: 1677 RVA: 0x000254A4 File Offset: 0x000236A4
		public static string FormatDivider(string label = null)
		{
			if (string.IsNullOrEmpty(label))
			{
				return "#----------------------------------------------------------------------------#";
			}
			label = " " + label + " ";
			int src = "#----------------------------------------------------------------------------#".Length / 2 - label.Length / 2;
			int dst = src + label.Length;
			return "#----------------------------------------------------------------------------#".Substring(0, src) + label + "#----------------------------------------------------------------------------#".Substring(dst);
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0002550D File Offset: 0x0002370D
		[Conditional("VALIDATE_MAIN_THREAD_ENABLED")]
		public static void ValidateIsMainThread(bool req)
		{
			if (Thread.CurrentThread.ManagedThreadId == DebugTools._mainThreadId != req)
			{
				Game1.log.Warn(DebugTools.FormatDivider("ERROR: CODE EXECUTED ON UNSAFE THREAD!"));
				Debugger.Break();
				Environment.Exit(1);
			}
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x00025542 File Offset: 0x00023742
		public static bool IsMainThread()
		{
			return Thread.CurrentThread.ManagedThreadId == DebugTools._mainThreadId;
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x00025555 File Offset: 0x00023755
		public static void Assert(bool expression, string failureMessage)
		{
			if (!expression)
			{
				Game1.log.Error(failureMessage, null);
			}
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00025566 File Offset: 0x00023766
		public static void GameConstructed(Game game)
		{
			DebugTools._mainThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00025577 File Offset: 0x00023777
		public static void GameLoadContent(Game game)
		{
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00025579 File Offset: 0x00023779
		public static void BeforeGameInitialize(Game game)
		{
			DebugTools.ApplyNoFpsCap(DebugTools._noFpsCap);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00025585 File Offset: 0x00023785
		public static void BeforeGameUpdate(Game1 game, ref GameTime gameTime)
		{
			if (Program.releaseBuild)
			{
				return;
			}
			DebugTools.CheckInput(game);
			if (DebugTools._noFpsCap)
			{
				gameTime = new GameTime(gameTime.TotalGameTime, game.TargetElapsedTime, gameTime.IsRunningSlowly);
			}
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x000255B7 File Offset: 0x000237B7
		public static void BeforeGameDraw(Game1 game, ref GameTime time)
		{
			if (DebugTools._noFpsCap)
			{
				time = new GameTime(time.TotalGameTime, game.TargetElapsedTime, time.IsRunningSlowly);
			}
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x000255DC File Offset: 0x000237DC
		private static void CheckInput(Game1 game)
		{
			GamePadState state = Game1.input.GetGamePadState();
			if (Game1.IsPressEvent(ref state, Buttons.LeftStick))
			{
				if (DebugTools._metrics != null)
				{
					DebugTools._metrics.Visible = !DebugTools._metrics.Visible;
				}
				Game1.log.Verbose("Toggling Metrics (" + ((DebugTools._metrics == null) ? "[null]" : DebugTools._metrics.Visible.ToString()) + ")");
			}
			if (Game1.IsPressEvent(ref state, Buttons.RightStick) && state.IsButtonDown(Buttons.LeftStick))
			{
				DebugTools._noFpsCap = !DebugTools._noFpsCap;
				DebugTools.ApplyNoFpsCap(DebugTools._noFpsCap);
			}
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00025686 File Offset: 0x00023886
		private static void ApplyNoFpsCap(bool nocap)
		{
		}

		// Token: 0x04000340 RID: 832
		private static int _mainThreadId;

		// Token: 0x04000341 RID: 833
		private const string CommentFormat = "#----------------------------------------------------------------------------#";

		// Token: 0x04000342 RID: 834
		public static DebugMetricsComponent _metrics;

		// Token: 0x04000343 RID: 835
		private static bool _noFpsCap;
	}
}
