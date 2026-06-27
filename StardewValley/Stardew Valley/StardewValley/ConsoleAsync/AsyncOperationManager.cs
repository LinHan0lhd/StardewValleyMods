using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StardewValley.ConsoleAsync
{
	// Token: 0x0200036C RID: 876
	public class AsyncOperationManager
	{
		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x060035D4 RID: 13780 RVA: 0x002A793A File Offset: 0x002A5B3A
		public static AsyncOperationManager Use
		{
			get
			{
				return AsyncOperationManager._instance;
			}
		}

		// Token: 0x060035D5 RID: 13781 RVA: 0x002A7941 File Offset: 0x002A5B41
		public static void Init()
		{
			AsyncOperationManager._instance = new AsyncOperationManager();
		}

		// Token: 0x060035D6 RID: 13782 RVA: 0x002A794D File Offset: 0x002A5B4D
		private AsyncOperationManager()
		{
			this._pendingOps = new List<IAsyncOperation>();
			this._tempOps = new List<IAsyncOperation>();
			this._doneOps = new List<IAsyncOperation>();
		}

		// Token: 0x060035D7 RID: 13783 RVA: 0x002A7978 File Offset: 0x002A5B78
		public void AddPending(Task task, Action<GenericResult> doneAction)
		{
			AsyncOperationManager.<>c__DisplayClass8_0 CS$<>8__locals1 = new AsyncOperationManager.<>c__DisplayClass8_0();
			CS$<>8__locals1.doneAction = doneAction;
			CS$<>8__locals1.op = new GenericOp();
			CS$<>8__locals1.op.DoneCallback = new Action(CS$<>8__locals1.<AddPending>g__OnDone|0);
			CS$<>8__locals1.op.Task = task;
			if (task.Status > TaskStatus.Created)
			{
				CS$<>8__locals1.op.TaskStarted = true;
			}
			this.AddPending(CS$<>8__locals1.op);
		}

		// Token: 0x060035D8 RID: 13784 RVA: 0x002A79E4 File Offset: 0x002A5BE4
		public void AddPending(Action workAction, Action<GenericResult> doneAction)
		{
			AsyncOperationManager.<>c__DisplayClass9_0 CS$<>8__locals1 = new AsyncOperationManager.<>c__DisplayClass9_0();
			CS$<>8__locals1.doneAction = doneAction;
			CS$<>8__locals1.op = new GenericOp();
			CS$<>8__locals1.op.DoneCallback = new Action(CS$<>8__locals1.<AddPending>g__OnDone|0);
			Task task = new Task(workAction);
			CS$<>8__locals1.op.Task = task;
			this.AddPending(CS$<>8__locals1.op);
		}

		// Token: 0x060035D9 RID: 13785 RVA: 0x002A7A40 File Offset: 0x002A5C40
		public void AddPending(IAsyncOperation op)
		{
			List<IAsyncOperation> pendingOps = this._pendingOps;
			lock (pendingOps)
			{
				this._pendingOps.Add(op);
			}
		}

		// Token: 0x060035DA RID: 13786 RVA: 0x002A7A88 File Offset: 0x002A5C88
		public void Update()
		{
			List<IAsyncOperation> pendingOps = this._pendingOps;
			lock (pendingOps)
			{
				this._doneOps.Clear();
				this._tempOps.Clear();
				this._tempOps.AddRange(this._pendingOps);
				this._pendingOps.Clear();
				bool working = false;
				for (int i = 0; i < this._tempOps.Count; i++)
				{
					IAsyncOperation op = this._tempOps[i];
					if (working)
					{
						this._pendingOps.Add(op);
					}
					else
					{
						working = true;
						if (!op.Started)
						{
							op.Begin();
							this._pendingOps.Add(op);
						}
						else if (op.Done)
						{
							this._doneOps.Add(op);
						}
						else
						{
							this._pendingOps.Add(op);
						}
					}
				}
				this._tempOps.Clear();
			}
			for (int j = 0; j < this._doneOps.Count; j++)
			{
				this._doneOps[j].Conclude();
			}
			this._doneOps.Clear();
		}

		// Token: 0x04002342 RID: 9026
		private static AsyncOperationManager _instance;

		// Token: 0x04002343 RID: 9027
		private List<IAsyncOperation> _pendingOps;

		// Token: 0x04002344 RID: 9028
		private List<IAsyncOperation> _tempOps;

		// Token: 0x04002345 RID: 9029
		private List<IAsyncOperation> _doneOps;
	}
}
