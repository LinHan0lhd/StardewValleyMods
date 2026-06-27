using System;
using System.Collections.Generic;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000DF RID: 223
	public class MultipleMutexRequest
	{
		// Token: 0x060010C9 RID: 4297 RVA: 0x000C827B File Offset: 0x000C647B
		public MultipleMutexRequest(List<NetMutex> mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
		{
			this._onSuccess = success_callback;
			this._onFailure = failure_callback;
			this._acquiredLocks = new List<NetMutex>();
			this._mutexList = new List<NetMutex>(mutexes);
			this._RequestMutexes();
		}

		// Token: 0x060010CA RID: 4298 RVA: 0x000C82AE File Offset: 0x000C64AE
		public MultipleMutexRequest(NetMutex[] mutexes, Action<MultipleMutexRequest> success_callback = null, Action<MultipleMutexRequest> failure_callback = null)
		{
			this._onSuccess = success_callback;
			this._onFailure = failure_callback;
			this._acquiredLocks = new List<NetMutex>();
			this._mutexList = new List<NetMutex>(mutexes);
			this._RequestMutexes();
		}

		// Token: 0x060010CB RID: 4299 RVA: 0x000C82E4 File Offset: 0x000C64E4
		protected void _RequestMutexes()
		{
			if (this._mutexList == null)
			{
				Action<MultipleMutexRequest> onFailure = this._onFailure;
				if (onFailure == null)
				{
					return;
				}
				onFailure(this);
				return;
			}
			else
			{
				if (this._mutexList.Count != 0)
				{
					int i = 0;
					while (i < this._mutexList.Count)
					{
						if (this._mutexList[i].IsLocked())
						{
							Action<MultipleMutexRequest> onFailure2 = this._onFailure;
							if (onFailure2 == null)
							{
								return;
							}
							onFailure2(this);
							return;
						}
						else
						{
							i++;
						}
					}
					for (int j = 0; j < this._mutexList.Count; j++)
					{
						NetMutex mutex = this._mutexList[j];
						mutex.RequestLock(delegate
						{
							this._OnLockAcquired(mutex);
						}, delegate
						{
							this._OnLockFailed(mutex);
						});
					}
					return;
				}
				Action<MultipleMutexRequest> onSuccess = this._onSuccess;
				if (onSuccess == null)
				{
					return;
				}
				onSuccess(this);
				return;
			}
		}

		// Token: 0x060010CC RID: 4300 RVA: 0x000C83BD File Offset: 0x000C65BD
		protected void _OnLockAcquired(NetMutex mutex)
		{
			this._reportedCount++;
			this._acquiredLocks.Add(mutex);
			if (this._reportedCount >= this._mutexList.Count)
			{
				this._Finalize();
			}
		}

		// Token: 0x060010CD RID: 4301 RVA: 0x000C83F2 File Offset: 0x000C65F2
		protected void _OnLockFailed(NetMutex mutex)
		{
			this._reportedCount++;
			if (this._reportedCount >= this._mutexList.Count)
			{
				this._Finalize();
			}
		}

		// Token: 0x060010CE RID: 4302 RVA: 0x000C841B File Offset: 0x000C661B
		protected void _Finalize()
		{
			if (this._acquiredLocks.Count < this._mutexList.Count)
			{
				this.ReleaseLocks();
				this._onFailure(this);
				return;
			}
			this._onSuccess(this);
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x000C8454 File Offset: 0x000C6654
		public void ReleaseLocks()
		{
			for (int i = 0; i < this._acquiredLocks.Count; i++)
			{
				this._acquiredLocks[i].ReleaseLock();
			}
			this._acquiredLocks.Clear();
		}

		// Token: 0x04000A12 RID: 2578
		protected int _reportedCount;

		// Token: 0x04000A13 RID: 2579
		protected List<NetMutex> _acquiredLocks;

		// Token: 0x04000A14 RID: 2580
		protected List<NetMutex> _mutexList;

		// Token: 0x04000A15 RID: 2581
		protected Action<MultipleMutexRequest> _onSuccess;

		// Token: 0x04000A16 RID: 2582
		protected Action<MultipleMutexRequest> _onFailure;
	}
}
