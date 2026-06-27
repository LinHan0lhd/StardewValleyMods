using System;
using System.IO;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001D3 RID: 467
	public class IncomingMessage : IDisposable
	{
		// Token: 0x17000359 RID: 857
		// (get) Token: 0x060020B7 RID: 8375 RVA: 0x00170F1E File Offset: 0x0016F11E
		public byte MessageType
		{
			get
			{
				return this.messageType;
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x060020B8 RID: 8376 RVA: 0x00170F26 File Offset: 0x0016F126
		public long FarmerID
		{
			get
			{
				return this.farmerID;
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x060020B9 RID: 8377 RVA: 0x00170F2E File Offset: 0x0016F12E
		public Farmer SourceFarmer
		{
			get
			{
				return Game1.GetPlayer(this.farmerID, false) ?? Game1.MasterPlayer;
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x060020BA RID: 8378 RVA: 0x00170F45 File Offset: 0x0016F145
		public byte[] Data
		{
			get
			{
				return this.data;
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x060020BB RID: 8379 RVA: 0x00170F4D File Offset: 0x0016F14D
		public BinaryReader Reader
		{
			get
			{
				return this.reader;
			}
		}

		// Token: 0x060020BC RID: 8380 RVA: 0x00170F58 File Offset: 0x0016F158
		public void Read(BinaryReader reader)
		{
			this.Dispose();
			this.messageType = reader.ReadByte();
			this.farmerID = reader.ReadInt64();
			this.data = reader.ReadSkippableBytes();
			this.stream = new MemoryStream(this.data);
			this.reader = new BinaryReader(this.stream);
		}

		// Token: 0x060020BD RID: 8381 RVA: 0x00170FB1 File Offset: 0x0016F1B1
		public void Dispose()
		{
			BinaryReader binaryReader = this.reader;
			if (binaryReader != null)
			{
				binaryReader.Dispose();
			}
			MemoryStream memoryStream = this.stream;
			if (memoryStream != null)
			{
				memoryStream.Dispose();
			}
			this.stream = null;
			this.reader = null;
		}

		// Token: 0x040013C6 RID: 5062
		private byte messageType;

		// Token: 0x040013C7 RID: 5063
		private long farmerID;

		// Token: 0x040013C8 RID: 5064
		private byte[] data;

		// Token: 0x040013C9 RID: 5065
		private MemoryStream stream;

		// Token: 0x040013CA RID: 5066
		private BinaryReader reader;
	}
}
