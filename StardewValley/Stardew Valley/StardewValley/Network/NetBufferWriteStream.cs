using System;
using System.IO;
using Lidgren.Network;

namespace StardewValley.Network
{
	// Token: 0x020001DA RID: 474
	public class NetBufferWriteStream : Stream
	{
		// Token: 0x06002110 RID: 8464 RVA: 0x001726A0 File Offset: 0x001708A0
		public NetBufferWriteStream(NetBuffer buffer)
		{
			this.Buffer = buffer;
			this.offset = buffer.LengthBits;
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06002111 RID: 8465 RVA: 0x001726BB File Offset: 0x001708BB
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06002112 RID: 8466 RVA: 0x001726BE File Offset: 0x001708BE
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06002113 RID: 8467 RVA: 0x001726C1 File Offset: 0x001708C1
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06002114 RID: 8468 RVA: 0x001726C4 File Offset: 0x001708C4
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06002115 RID: 8469 RVA: 0x001726CB File Offset: 0x001708CB
		// (set) Token: 0x06002116 RID: 8470 RVA: 0x001726E2 File Offset: 0x001708E2
		public override long Position
		{
			get
			{
				return (long)((this.Buffer.LengthBits - this.offset) / 8);
			}
			set
			{
				this.Buffer.LengthBits = (int)((long)this.offset + value * 8L);
			}
		}

		// Token: 0x06002117 RID: 8471 RVA: 0x001726FC File Offset: 0x001708FC
		public override void Flush()
		{
		}

		// Token: 0x06002118 RID: 8472 RVA: 0x001726FE File Offset: 0x001708FE
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x00172705 File Offset: 0x00170905
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				this.Position = offset;
				break;
			case SeekOrigin.Current:
				this.Position += offset;
				break;
			case SeekOrigin.End:
				throw new NotSupportedException();
			}
			return this.Position;
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x00172740 File Offset: 0x00170940
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600211B RID: 8475 RVA: 0x00172747 File Offset: 0x00170947
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.Buffer.Write(buffer, offset, count);
		}

		// Token: 0x040013E6 RID: 5094
		private int offset;

		// Token: 0x040013E7 RID: 5095
		public NetBuffer Buffer;
	}
}
