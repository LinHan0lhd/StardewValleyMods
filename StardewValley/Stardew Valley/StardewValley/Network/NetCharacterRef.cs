using System;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001DC RID: 476
	public class NetCharacterRef : INetObject<NetFields>
	{
		// Token: 0x17000373 RID: 883
		// (get) Token: 0x06002122 RID: 8482 RVA: 0x001727AA File Offset: 0x001709AA
		public NetFields NetFields { get; } = new NetFields("NetCharacterRef");

		// Token: 0x06002123 RID: 8483 RVA: 0x001727B4 File Offset: 0x001709B4
		public NetCharacterRef()
		{
			this.NetFields.SetOwner(this).AddField(this.npc.NetFields, "npc.NetFields").AddField(this.farmer.NetFields, "farmer.NetFields");
		}

		// Token: 0x06002124 RID: 8484 RVA: 0x00172824 File Offset: 0x00170A24
		public Character Get(GameLocation location)
		{
			NPC npcValue = this.npc.Get(location);
			if (npcValue != null)
			{
				return npcValue;
			}
			return this.farmer.Value;
		}

		// Token: 0x06002125 RID: 8485 RVA: 0x00172850 File Offset: 0x00170A50
		public void Set(GameLocation location, Character character)
		{
			NPC curNpc = character as NPC;
			if (curNpc != null)
			{
				this.npc.Set(location, curNpc);
				this.farmer.Value = null;
				return;
			}
			Farmer curFarmer = character as Farmer;
			if (curFarmer == null)
			{
				throw new ArgumentException();
			}
			this.npc.Clear();
			this.farmer.Value = curFarmer;
		}

		// Token: 0x06002126 RID: 8486 RVA: 0x001728AA File Offset: 0x00170AAA
		public void Clear()
		{
			this.npc.Clear();
			this.farmer.Value = null;
		}

		// Token: 0x040013E8 RID: 5096
		private readonly NetNPCRef npc = new NetNPCRef();

		// Token: 0x040013E9 RID: 5097
		private readonly NetFarmerRef farmer = new NetFarmerRef();
	}
}
