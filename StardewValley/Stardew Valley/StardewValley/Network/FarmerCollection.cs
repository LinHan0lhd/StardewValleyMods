using System;
using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001CB RID: 459
	public class FarmerCollection : IEnumerable<Farmer>, IEnumerable
	{
		// Token: 0x0600204E RID: 8270 RVA: 0x0016F168 File Offset: 0x0016D368
		public FarmerCollection(GameLocation locationFilter = null)
		{
			this._locationFilter = locationFilter;
		}

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x0600204F RID: 8271 RVA: 0x0016F178 File Offset: 0x0016D378
		public int Count
		{
			get
			{
				int count = 0;
				foreach (Farmer farmer in this)
				{
					count++;
				}
				return count;
			}
		}

		// Token: 0x06002050 RID: 8272 RVA: 0x0016F1C8 File Offset: 0x0016D3C8
		public bool Any()
		{
			using (FarmerCollection.Enumerator enumerator = this.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Farmer farmer = enumerator.Current;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002051 RID: 8273 RVA: 0x0016F218 File Offset: 0x0016D418
		public bool Contains(Farmer farmer)
		{
			using (FarmerCollection.Enumerator enumerator = this.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == farmer)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002052 RID: 8274 RVA: 0x0016F268 File Offset: 0x0016D468
		public FarmerCollection.Enumerator GetEnumerator()
		{
			return new FarmerCollection.Enumerator(this._locationFilter);
		}

		// Token: 0x06002053 RID: 8275 RVA: 0x0016F275 File Offset: 0x0016D475
		IEnumerator<Farmer> IEnumerable<Farmer>.GetEnumerator()
		{
			return new FarmerCollection.Enumerator(this._locationFilter);
		}

		// Token: 0x06002054 RID: 8276 RVA: 0x0016F287 File Offset: 0x0016D487
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new FarmerCollection.Enumerator(this._locationFilter);
		}

		// Token: 0x040013BB RID: 5051
		private GameLocation _locationFilter;

		// Token: 0x0200056D RID: 1389
		public struct Enumerator : IEnumerator<Farmer>, IEnumerator, IDisposable
		{
			// Token: 0x06004173 RID: 16755 RVA: 0x00308136 File Offset: 0x00306336
			public Enumerator(GameLocation locationFilter)
			{
				this._locationFilter = locationFilter;
				this._player = Game1.player;
				this._enumerator = Game1.otherFarmers.Roots.GetEnumerator();
				this._current = null;
				this._done = 2;
			}

			// Token: 0x06004174 RID: 16756 RVA: 0x00308170 File Offset: 0x00306370
			public bool MoveNext()
			{
				if (this._done == 2)
				{
					this._done = 1;
					if (this._locationFilter == null || (this._player.currentLocation != null && this._locationFilter.Equals(this._player.currentLocation)))
					{
						this._current = this._player;
						return true;
					}
				}
				while (this._enumerator.MoveNext())
				{
					KeyValuePair<long, NetRoot<Farmer>> keyValuePair = this._enumerator.Current;
					Farmer player = keyValuePair.Value.Value;
					if (player != this._player && (this._locationFilter == null || (player.currentLocation != null && this._locationFilter.Equals(player.currentLocation))))
					{
						this._current = player;
						return true;
					}
				}
				this._done = 0;
				this._current = null;
				return false;
			}

			// Token: 0x170004E2 RID: 1250
			// (get) Token: 0x06004175 RID: 16757 RVA: 0x00308235 File Offset: 0x00306435
			public Farmer Current
			{
				get
				{
					return this._current;
				}
			}

			// Token: 0x06004176 RID: 16758 RVA: 0x0030823D File Offset: 0x0030643D
			public void Dispose()
			{
			}

			// Token: 0x170004E3 RID: 1251
			// (get) Token: 0x06004177 RID: 16759 RVA: 0x0030823F File Offset: 0x0030643F
			object IEnumerator.Current
			{
				get
				{
					if (this._done == 0)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			// Token: 0x06004178 RID: 16760 RVA: 0x00308255 File Offset: 0x00306455
			void IEnumerator.Reset()
			{
				this._player = Game1.player;
				this._enumerator = Game1.otherFarmers.Roots.GetEnumerator();
				this._current = null;
				this._done = 2;
			}

			// Token: 0x04002B7E RID: 11134
			private GameLocation _locationFilter;

			// Token: 0x04002B7F RID: 11135
			private Dictionary<long, NetRoot<Farmer>>.Enumerator _enumerator;

			// Token: 0x04002B80 RID: 11136
			private Farmer _player;

			// Token: 0x04002B81 RID: 11137
			private Farmer _current;

			// Token: 0x04002B82 RID: 11138
			private int _done;
		}
	}
}
