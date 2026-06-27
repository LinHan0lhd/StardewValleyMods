using System;
using System.Diagnostics;

namespace StardewValley.Util
{
	// Token: 0x02000122 RID: 290
	public class StackTraceHelper
	{
		// Token: 0x060017C9 RID: 6089 RVA: 0x001122D1 File Offset: 0x001104D1
		public static string FromException(Exception ex)
		{
			return ((ex != null) ? ex.StackTrace : null) ?? "";
		}

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x060017CA RID: 6090 RVA: 0x001122E8 File Offset: 0x001104E8
		public static string StackTrace
		{
			get
			{
				return Environment.StackTrace;
			}
		}

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x060017CB RID: 6091 RVA: 0x001122EF File Offset: 0x001104EF
		public int FrameCount
		{
			get
			{
				StackTrace stackTrace = this._StackTrace as StackTrace;
				if (stackTrace == null)
				{
					return 0;
				}
				return stackTrace.FrameCount;
			}
		}

		// Token: 0x060017CC RID: 6092 RVA: 0x00112307 File Offset: 0x00110507
		public StackTraceHelper()
		{
			this._StackTrace = new StackTrace();
		}

		// Token: 0x060017CD RID: 6093 RVA: 0x0011231A File Offset: 0x0011051A
		public StackFrame GetFrame(int index)
		{
			StackTrace stackTrace = this._StackTrace as StackTrace;
			if (stackTrace == null)
			{
				return null;
			}
			return stackTrace.GetFrame(index);
		}

		// Token: 0x060017CE RID: 6094 RVA: 0x00112333 File Offset: 0x00110533
		public StackFrame[] GetFrames()
		{
			StackTrace stackTrace = this._StackTrace as StackTrace;
			return ((stackTrace != null) ? stackTrace.GetFrames() : null) ?? LegacyShims.EmptyArray<StackFrame>();
		}

		// Token: 0x060017CF RID: 6095 RVA: 0x00112355 File Offset: 0x00110555
		public new string ToString()
		{
			StackTrace stackTrace = this._StackTrace as StackTrace;
			return ((stackTrace != null) ? stackTrace.ToString() : null) ?? "";
		}

		// Token: 0x04000E52 RID: 3666
		private object _StackTrace;
	}
}
