using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E6 RID: 486
	public class NetMutexQueue<T> : INetObject<NetFields>
	{
		// Token: 0x17000381 RID: 897
		// (get) Token: 0x06002180 RID: 8576 RVA: 0x0017394F File Offset: 0x00171B4F
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("NetMutexQueue");

		// Token: 0x06002181 RID: 8577 RVA: 0x00173958 File Offset: 0x00171B58
		public NetMutexQueue()
		{
			this.NetFields.SetOwner(this).AddField(this.requests, "requests").AddField(this.currentOwner, "currentOwner");
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x001739FC File Offset: 0x00171BFC
		public void Add(T job)
		{
			this.localJobs.Add(job);
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x00173A0A File Offset: 0x00171C0A
		public bool Contains(T job)
		{
			return this.localJobs.Contains(job);
		}

		// Token: 0x06002184 RID: 8580 RVA: 0x00173A18 File Offset: 0x00171C18
		public void Clear()
		{
			this.localJobs.Clear();
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x00173A28 File Offset: 0x00171C28
		public void Update(GameLocation location)
		{
			FarmerCollection farmers = location.farmers;
			if (farmers.Contains(Game1.player) && this.localJobs.Count > 0)
			{
				this.requests[Game1.player.UniqueMultiplayerID] = true;
			}
			else
			{
				this.requests.Remove(Game1.player.UniqueMultiplayerID);
			}
			if (Game1.IsMasterGame)
			{
				this.requests.RemoveWhere((KeyValuePair<long, bool> pair) => farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == pair.Key) == null);
				if (!this.requests.ContainsKey(this.currentOwner.Value))
				{
					this.currentOwner.Value = -1L;
				}
			}
			if (this.currentOwner.Value == Game1.player.UniqueMultiplayerID)
			{
				foreach (T job in this.localJobs)
				{
					this.Processor(job);
				}
				this.localJobs.Clear();
				this.requests.Remove(Game1.player.UniqueMultiplayerID);
				this.currentOwner.Value = -1L;
			}
			long ownerId;
			bool flag;
			if (Game1.IsMasterGame && this.currentOwner.Value == -1L && Utility.TryGetRandom<long, bool, NetBool, SerializableDictionary<long, bool>, NetLongDictionary<bool, NetBool>>(this.requests, out ownerId, out flag, null))
			{
				this.currentOwner.Value = ownerId;
			}
		}

		// Token: 0x04001405 RID: 5125
		private readonly NetLongDictionary<bool, NetBool> requests = new NetLongDictionary<bool, NetBool>
		{
			InterpolationWait = false
		};

		// Token: 0x04001406 RID: 5126
		private readonly NetLong currentOwner = new NetLong
		{
			InterpolationWait = false
		};

		// Token: 0x04001407 RID: 5127
		private readonly List<T> localJobs = new List<T>();

		// Token: 0x04001408 RID: 5128
		[XmlIgnore]
		public Action<T> Processor = delegate(T x)
		{
		};
	}
}
