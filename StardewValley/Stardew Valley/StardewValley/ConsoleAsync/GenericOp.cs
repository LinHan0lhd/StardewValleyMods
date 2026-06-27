using System;
using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync
{
	// Token: 0x0200036E RID: 878
	public sealed class GenericOp : AsyncTaskOperation
	{
		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x060035E0 RID: 13792 RVA: 0x002A7BEB File Offset: 0x002A5DEB
		public override bool Done
		{
			get
			{
				return this.Task.Status >= TaskStatus.RanToCompletion;
			}
		}

		// Token: 0x060035E1 RID: 13793 RVA: 0x002A7C00 File Offset: 0x002A5E00
		public override void Conclude()
		{
			Action e = this.DoneCallback;
			if (e != null)
			{
				e();
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x060035E2 RID: 13794 RVA: 0x002A7C20 File Offset: 0x002A5E20
		public bool Result
		{
			get
			{
				if (this.Task.Status < TaskStatus.RanToCompletion)
				{
					return false;
				}
				if (this.Task.IsFaulted)
				{
					Exception e = this.Task.Exception.GetBaseException();
					Console.WriteLine(e);
					Console.WriteLine("Task failed with exception: {0}.", e.Message);
					throw e;
				}
				return true;
			}
		}

		// Token: 0x04002348 RID: 9032
		public Action DoneCallback;
	}
}
