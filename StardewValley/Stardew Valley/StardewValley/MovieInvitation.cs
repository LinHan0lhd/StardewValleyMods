using System;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000D5 RID: 213
	public class MovieInvitation : INetObject<NetFields>
	{
		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06001073 RID: 4211 RVA: 0x000C686A File Offset: 0x000C4A6A
		public NetFields NetFields { get; } = new NetFields("MovieInvitation");

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06001074 RID: 4212 RVA: 0x000C6872 File Offset: 0x000C4A72
		// (set) Token: 0x06001075 RID: 4213 RVA: 0x000C687F File Offset: 0x000C4A7F
		public Farmer farmer
		{
			get
			{
				return this._farmer.Value;
			}
			set
			{
				this._farmer.Value = value;
			}
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06001076 RID: 4214 RVA: 0x000C688D File Offset: 0x000C4A8D
		// (set) Token: 0x06001077 RID: 4215 RVA: 0x000C68A1 File Offset: 0x000C4AA1
		public NPC invitedNPC
		{
			get
			{
				return Game1.getCharacterFromName(this._invitedNPCName.Value, true, false);
			}
			set
			{
				if (value == null)
				{
					this._invitedNPCName.Set(null);
					return;
				}
				this._invitedNPCName.Set(value.name.Value);
			}
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06001078 RID: 4216 RVA: 0x000C68C9 File Offset: 0x000C4AC9
		// (set) Token: 0x06001079 RID: 4217 RVA: 0x000C68D6 File Offset: 0x000C4AD6
		public bool fulfilled
		{
			get
			{
				return this._fulfilled.Value;
			}
			set
			{
				this._fulfilled.Set(value);
			}
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x000C68E4 File Offset: 0x000C4AE4
		public MovieInvitation()
		{
			this.NetFields.SetOwner(this).AddField(this._farmer.NetFields, "_farmer.NetFields").AddField(this._invitedNPCName, "_invitedNPCName").AddField(this._fulfilled, "_fulfilled");
		}

		// Token: 0x04000A01 RID: 2561
		private NetFarmerRef _farmer = new NetFarmerRef();

		// Token: 0x04000A02 RID: 2562
		protected NetString _invitedNPCName = new NetString();

		// Token: 0x04000A03 RID: 2563
		protected NetBool _fulfilled = new NetBool(false);
	}
}
