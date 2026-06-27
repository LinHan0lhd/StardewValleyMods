using System;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network.NetEvents
{
	// Token: 0x020001FC RID: 508
	public class NutDropRequest : NetEventArg
	{
		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x060022B8 RID: 8888 RVA: 0x001772BC File Offset: 0x001754BC
		// (set) Token: 0x060022B9 RID: 8889 RVA: 0x001772C4 File Offset: 0x001754C4
		public string Key { get; private set; }

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x060022BA RID: 8890 RVA: 0x001772CD File Offset: 0x001754CD
		// (set) Token: 0x060022BB RID: 8891 RVA: 0x001772D5 File Offset: 0x001754D5
		public string LocationName { get; private set; }

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x060022BC RID: 8892 RVA: 0x001772DE File Offset: 0x001754DE
		// (set) Token: 0x060022BD RID: 8893 RVA: 0x001772E6 File Offset: 0x001754E6
		public Point Tile { get; private set; }

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x060022BE RID: 8894 RVA: 0x001772EF File Offset: 0x001754EF
		// (set) Token: 0x060022BF RID: 8895 RVA: 0x001772F7 File Offset: 0x001754F7
		public int Limit { get; private set; } = 1;

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x060022C0 RID: 8896 RVA: 0x00177300 File Offset: 0x00175500
		// (set) Token: 0x060022C1 RID: 8897 RVA: 0x00177308 File Offset: 0x00175508
		public int RewardAmount { get; private set; } = 1;

		// Token: 0x060022C2 RID: 8898 RVA: 0x00177311 File Offset: 0x00175511
		public NutDropRequest()
		{
		}

		// Token: 0x060022C3 RID: 8899 RVA: 0x00177328 File Offset: 0x00175528
		public NutDropRequest(string key, string locationName, Point tile, int limit, int rewardAmount)
		{
			this.Key = key;
			this.LocationName = (locationName ?? "null");
			this.Tile = tile;
			this.Limit = limit;
			this.RewardAmount = rewardAmount;
		}

		// Token: 0x060022C4 RID: 8900 RVA: 0x00177378 File Offset: 0x00175578
		public void Read(BinaryReader reader)
		{
			this.Key = reader.ReadString();
			this.LocationName = reader.ReadString();
			this.Tile = new Point(reader.ReadInt32(), reader.ReadInt32());
			this.Limit = reader.ReadInt32();
			this.RewardAmount = reader.ReadInt32();
		}

		// Token: 0x060022C5 RID: 8901 RVA: 0x001773CC File Offset: 0x001755CC
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.Key);
			writer.Write(this.LocationName);
			writer.Write(this.Tile.X);
			writer.Write(this.Tile.Y);
			writer.Write(this.Limit);
			writer.Write(this.RewardAmount);
		}
	}
}
