using System;
using Netcode;

namespace StardewValley.Minigames
{
	// Token: 0x0200023F RID: 575
	public class NetLeaderboardsEntry : INetObject<NetFields>
	{
		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06002660 RID: 9824 RVA: 0x001B37E8 File Offset: 0x001B19E8
		public NetFields NetFields { get; } = new NetFields("NetLeaderboardsEntry");

		// Token: 0x06002661 RID: 9825 RVA: 0x001B37F0 File Offset: 0x001B19F0
		public void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.name, "name").AddField(this.score, "score");
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x001B381F File Offset: 0x001B1A1F
		public NetLeaderboardsEntry()
		{
			this.InitNetFields();
		}

		// Token: 0x06002663 RID: 9827 RVA: 0x001B385C File Offset: 0x001B1A5C
		public NetLeaderboardsEntry(string new_name, int new_score)
		{
			this.InitNetFields();
			this.name.Value = new_name;
			this.score.Value = new_score;
		}

		// Token: 0x040017D8 RID: 6104
		public readonly NetString name = new NetString("");

		// Token: 0x040017D9 RID: 6105
		public readonly NetInt score = new NetInt(0);
	}
}
