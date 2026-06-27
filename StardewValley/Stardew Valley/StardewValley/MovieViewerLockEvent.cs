using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000D7 RID: 215
	public class MovieViewerLockEvent : NetEventArg
	{
		// Token: 0x06001081 RID: 4225 RVA: 0x000C6B30 File Offset: 0x000C4D30
		public MovieViewerLockEvent()
		{
			this.uids = new List<long>();
			this.movieStartTime = 0;
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x000C6B4C File Offset: 0x000C4D4C
		public MovieViewerLockEvent(List<Farmer> present_farmers, int movie_start_time)
		{
			this.movieStartTime = movie_start_time;
			this.uids = new List<long>();
			foreach (Farmer farmer in present_farmers)
			{
				this.uids.Add(farmer.UniqueMultiplayerID);
			}
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x000C6BBC File Offset: 0x000C4DBC
		public void Read(BinaryReader reader)
		{
			this.uids.Clear();
			this.movieStartTime = reader.ReadInt32();
			int capacity = reader.ReadInt32();
			for (int i = 0; i < capacity; i++)
			{
				this.uids.Add(reader.ReadInt64());
			}
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x000C6C04 File Offset: 0x000C4E04
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.movieStartTime);
			writer.Write(this.uids.Count);
			for (int i = 0; i < this.uids.Count; i++)
			{
				writer.Write(this.uids[i]);
			}
		}

		// Token: 0x04000A07 RID: 2567
		public List<long> uids;

		// Token: 0x04000A08 RID: 2568
		public int movieStartTime;
	}
}
