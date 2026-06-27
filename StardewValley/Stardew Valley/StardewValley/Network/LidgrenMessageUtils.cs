using System;
using System.IO;
using Lidgren.Network;

namespace StardewValley.Network
{
	// Token: 0x020001D5 RID: 469
	public static class LidgrenMessageUtils
	{
		// Token: 0x060020CF RID: 8399 RVA: 0x00171614 File Offset: 0x0016F814
		internal static void WriteMessage(OutgoingMessage srcMsg, NetOutgoingMessage destMsg)
		{
			byte[] dataRaw;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					srcMsg.Write(writer);
					dataRaw = stream.ToArray();
				}
			}
			using (MemoryStream srcStream = new MemoryStream(Program.netCompression.CompressAbove(dataRaw, 1024)))
			{
				using (NetBufferWriteStream destStream = new NetBufferWriteStream(destMsg))
				{
					srcStream.CopyTo(destStream);
				}
			}
		}

		// Token: 0x060020D0 RID: 8400 RVA: 0x001716C8 File Offset: 0x0016F8C8
		internal static void ReadStreamToMessage(NetBufferReadStream stream, IncomingMessage msg)
		{
			Stream messageStream = stream;
			byte[] decompressed;
			if (Program.netCompression.TryDecompressStream(stream, out decompressed))
			{
				messageStream = new MemoryStream(decompressed);
			}
			using (BinaryReader reader = new BinaryReader(messageStream))
			{
				msg.Read(reader);
			}
		}
	}
}
