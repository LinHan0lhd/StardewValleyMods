using System;
using System.Collections.Generic;

namespace StardewValley
{
	// Token: 0x020000DD RID: 221
	[Obsolete("This is only kept for backwards compatibility. It should no longer be used, and no longer does anything besides wrap the provided list.")]
	public struct DisposableList<T>
	{
		// Token: 0x060010A8 RID: 4264 RVA: 0x000C8033 File Offset: 0x000C6233
		public DisposableList(List<T> list)
		{
			this._list = list;
		}

		// Token: 0x060010A9 RID: 4265 RVA: 0x000C803C File Offset: 0x000C623C
		public DisposableList<T>.Enumerator GetEnumerator()
		{
			return new DisposableList<T>.Enumerator(this);
		}

		// Token: 0x04000A0F RID: 2575
		private readonly List<T> _list;

		// Token: 0x020004AC RID: 1196
		public struct Enumerator : IDisposable
		{
			// Token: 0x06003EE9 RID: 16105 RVA: 0x002FB68D File Offset: 0x002F988D
			public Enumerator(DisposableList<T> parent)
			{
				this._parent = parent;
				this._index = 0;
			}

			// Token: 0x170004C5 RID: 1221
			// (get) Token: 0x06003EEA RID: 16106 RVA: 0x002FB69D File Offset: 0x002F989D
			public T Current
			{
				get
				{
					if (this._parent._list == null || this._index == 0)
					{
						throw new InvalidOperationException();
					}
					return this._parent._list[this._index - 1];
				}
			}

			// Token: 0x06003EEB RID: 16107 RVA: 0x002FB6D2 File Offset: 0x002F98D2
			public bool MoveNext()
			{
				this._index++;
				return this._parent._list != null && this._parent._list.Count >= this._index;
			}

			// Token: 0x06003EEC RID: 16108 RVA: 0x002FB70C File Offset: 0x002F990C
			public void Reset()
			{
				this._index = 0;
			}

			// Token: 0x06003EED RID: 16109 RVA: 0x002FB715 File Offset: 0x002F9915
			public void Dispose()
			{
			}

			// Token: 0x040028F3 RID: 10483
			private readonly DisposableList<T> _parent;

			// Token: 0x040028F4 RID: 10484
			private int _index;
		}
	}
}
