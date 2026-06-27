using System;
using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001DF RID: 479
	public class NetFarmerCollection : INetObject<NetFields>, ICollection<Farmer>, IEnumerable<Farmer>, IEnumerable
	{
		// Token: 0x17000376 RID: 886
		// (get) Token: 0x0600213C RID: 8508 RVA: 0x00172CE4 File Offset: 0x00170EE4
		public NetFields NetFields { get; } = new NetFields("NetFarmerCollection");

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x00172CEC File Offset: 0x00170EEC
		public int Count
		{
			get
			{
				return this.farmers.Count;
			}
		}

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x0600213E RID: 8510 RVA: 0x00172CF9 File Offset: 0x00170EF9
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x0600213F RID: 8511 RVA: 0x00172CFC File Offset: 0x00170EFC
		// (remove) Token: 0x06002140 RID: 8512 RVA: 0x00172D34 File Offset: 0x00170F34
		public event NetFarmerCollection.FarmerEvent FarmerAdded;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06002141 RID: 8513 RVA: 0x00172D6C File Offset: 0x00170F6C
		// (remove) Token: 0x06002142 RID: 8514 RVA: 0x00172DA4 File Offset: 0x00170FA4
		public event NetFarmerCollection.FarmerEvent FarmerRemoved;

		// Token: 0x06002143 RID: 8515 RVA: 0x00172DDC File Offset: 0x00170FDC
		public NetFarmerCollection()
		{
			this.NetFields.SetOwner(this).AddField(this.uids, "uids");
			this.uids.OnValueAdded += delegate(long uid, bool _)
			{
				Farmer f = this.getFarmer(uid);
				if (f != null && !this.farmers.Contains(f))
				{
					this.farmers.Add(f);
					NetFarmerCollection.FarmerEvent farmerAdded = this.FarmerAdded;
					if (farmerAdded == null)
					{
						return;
					}
					farmerAdded(f);
				}
			};
			this.uids.OnValueRemoved += delegate(long uid, bool _)
			{
				Farmer f = this.getFarmer(uid);
				if (f != null)
				{
					this.farmers.Remove(f);
					NetFarmerCollection.FarmerEvent farmerRemoved = this.FarmerRemoved;
					if (farmerRemoved == null)
					{
						return;
					}
					farmerRemoved(f);
				}
			};
		}

		// Token: 0x06002144 RID: 8516 RVA: 0x00172E60 File Offset: 0x00171060
		private static bool playerIsOnline(long uid)
		{
			return Game1.player.UniqueMultiplayerID == uid || (Game1.serverHost != null && Game1.serverHost.Value.UniqueMultiplayerID == uid) || (Game1.otherFarmers.ContainsKey(uid) && !Game1.multiplayer.isDisconnecting(uid));
		}

		// Token: 0x06002145 RID: 8517 RVA: 0x00172EB8 File Offset: 0x001710B8
		public bool RetainOnlinePlayers()
		{
			int origCount = this.uids.Length;
			if (origCount == 0)
			{
				return false;
			}
			this.uids.RemoveWhere((KeyValuePair<long, bool> pair) => !NetFarmerCollection.playerIsOnline(pair.Key));
			this.farmers.Clear();
			foreach (long uid in this.uids.Keys)
			{
				Farmer f = this.getFarmer(uid);
				if (f != null)
				{
					this.farmers.Add(f);
				}
			}
			return this.uids.Length < origCount;
		}

		// Token: 0x06002146 RID: 8518 RVA: 0x00172F7C File Offset: 0x0017117C
		private Farmer getFarmer(long uid)
		{
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.UniqueMultiplayerID == uid)
				{
					return farmer;
				}
			}
			return null;
		}

		// Token: 0x06002147 RID: 8519 RVA: 0x00172FD8 File Offset: 0x001711D8
		public void Add(Farmer item)
		{
			this.farmers.Add(item);
			this.uids.TryAdd(item.UniqueMultiplayerID, true);
		}

		// Token: 0x06002148 RID: 8520 RVA: 0x00172FF9 File Offset: 0x001711F9
		public void Clear()
		{
			this.farmers.Clear();
			this.uids.Clear();
		}

		// Token: 0x06002149 RID: 8521 RVA: 0x00173011 File Offset: 0x00171211
		public bool Contains(Farmer item)
		{
			return this.farmers.Contains(item);
		}

		// Token: 0x0600214A RID: 8522 RVA: 0x00173020 File Offset: 0x00171220
		public void CopyTo(Farmer[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (this.Count - arrayIndex > array.Length)
			{
				throw new ArgumentException();
			}
			foreach (Farmer value in this)
			{
				array[arrayIndex++] = value;
			}
		}

		// Token: 0x0600214B RID: 8523 RVA: 0x00173090 File Offset: 0x00171290
		public bool Remove(Farmer item)
		{
			this.uids.Remove(item.UniqueMultiplayerID);
			return this.farmers.Remove(item);
		}

		// Token: 0x0600214C RID: 8524 RVA: 0x001730B0 File Offset: 0x001712B0
		public IEnumerator<Farmer> GetEnumerator()
		{
			return this.farmers.GetEnumerator();
		}

		// Token: 0x0600214D RID: 8525 RVA: 0x001730C2 File Offset: 0x001712C2
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x040013EF RID: 5103
		private List<Farmer> farmers = new List<Farmer>();

		// Token: 0x040013F0 RID: 5104
		private NetLongDictionary<bool, NetBool> uids = new NetLongDictionary<bool, NetBool>();

		// Token: 0x0200057B RID: 1403
		// (Invoke) Token: 0x0600419C RID: 16796
		public delegate void FarmerEvent(Farmer f);
	}
}
