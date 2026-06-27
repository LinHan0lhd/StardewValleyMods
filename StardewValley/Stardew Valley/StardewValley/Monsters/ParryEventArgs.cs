using System;
using System.IO;
using Netcode;

namespace StardewValley.Monsters
{
	// Token: 0x02000220 RID: 544
	internal class ParryEventArgs : NetEventArg
	{
		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x060023F1 RID: 9201 RVA: 0x00188B4A File Offset: 0x00186D4A
		// (set) Token: 0x060023F2 RID: 9202 RVA: 0x00188B61 File Offset: 0x00186D61
		public Farmer who
		{
			get
			{
				return Game1.GetPlayer(this.farmerId, false) ?? Game1.MasterPlayer;
			}
			set
			{
				this.farmerId = value.UniqueMultiplayerID;
			}
		}

		// Token: 0x060023F3 RID: 9203 RVA: 0x00188B6F File Offset: 0x00186D6F
		public ParryEventArgs()
		{
		}

		// Token: 0x060023F4 RID: 9204 RVA: 0x00188B77 File Offset: 0x00186D77
		public ParryEventArgs(int damage, Farmer who)
		{
			this.damage = damage;
			this.who = who;
		}

		// Token: 0x060023F5 RID: 9205 RVA: 0x00188B8D File Offset: 0x00186D8D
		public void Read(BinaryReader reader)
		{
			this.damage = reader.ReadInt32();
			this.farmerId = reader.ReadInt64();
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x00188BA7 File Offset: 0x00186DA7
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.damage);
			writer.Write(this.farmerId);
		}

		// Token: 0x0400154E RID: 5454
		public int damage;

		// Token: 0x0400154F RID: 5455
		private long farmerId;
	}
}
