using System;
using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync
{
	// Token: 0x0200036D RID: 877
	public abstract class AsyncTaskOperation : IAsyncOperation
	{
		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x060035DB RID: 13787 RVA: 0x002A7BB4 File Offset: 0x002A5DB4
		bool IAsyncOperation.Started
		{
			get
			{
				return this.TaskStarted;
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x060035DC RID: 13788
		public abstract bool Done { get; }

		// Token: 0x060035DD RID: 13789 RVA: 0x002A7BBC File Offset: 0x002A5DBC
		void IAsyncOperation.Begin()
		{
			DebugTools.Assert(!this.TaskStarted, "AsyncTaskOperation.Begin called but TaskStarted already is true!");
			this.TaskStarted = true;
			this.Task.Start();
		}

		// Token: 0x060035DE RID: 13790
		public abstract void Conclude();

		// Token: 0x04002346 RID: 9030
		public Task Task;

		// Token: 0x04002347 RID: 9031
		public bool TaskStarted;
	}
}
