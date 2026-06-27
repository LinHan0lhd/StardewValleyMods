using System;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E7 RID: 487
	public class NetNPCRef : INetObject<NetFields>
	{
		// Token: 0x17000382 RID: 898
		// (get) Token: 0x06002186 RID: 8582 RVA: 0x00173BA0 File Offset: 0x00171DA0
		public NetFields NetFields { get; } = new NetFields("NetNPCRef");

		// Token: 0x06002187 RID: 8583 RVA: 0x00173BA8 File Offset: 0x00171DA8
		public NetNPCRef()
		{
			this.NetFields.SetOwner(this).AddField(this.guid, "guid");
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x00173BE8 File Offset: 0x00171DE8
		public NPC Get(GameLocation location)
		{
			NPC npc;
			if (!(this.guid.Value != Guid.Empty) || location == null || !location.characters.TryGetValue(this.guid.Value, out npc))
			{
				return null;
			}
			return npc;
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00173C2C File Offset: 0x00171E2C
		public void Set(GameLocation location, NPC npc)
		{
			if (npc == null)
			{
				this.guid.Value = Guid.Empty;
				return;
			}
			Guid newGuid = location.characters.GuidOf(npc);
			if (newGuid == Guid.Empty)
			{
				throw new ArgumentException();
			}
			this.guid.Value = newGuid;
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x00173C79 File Offset: 0x00171E79
		public void Clear()
		{
			this.guid.Value = Guid.Empty;
		}

		// Token: 0x0400140A RID: 5130
		private readonly NetGuid guid = new NetGuid();
	}
}
