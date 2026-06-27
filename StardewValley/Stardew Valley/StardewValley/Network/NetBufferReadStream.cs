using System;
using System.IO;
using Lidgren.Network;

namespace StardewValley.Network
{
	// Token: 0x020001D9 RID: 473
	public class NetBufferReadStream : Stream
	{
		// Token: 0x06002104 RID: 8452 RVA: 0x001725C6 File Offset: 0x001707C6
		public NetBufferReadStream(NetBuffer buffer)
		{
			this.Buffer = buffer;
			this.offset = buffer.Position;
		}

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06002105 RID: 8453 RVA: 0x001725E1 File Offset: 0x001707E1
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06002106 RID: 8454 RVA: 0x001725E4 File Offset: 0x001707E4
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06002107 RID: 8455 RVA: 0x001725E7 File Offset: 0x001707E7
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06002108 RID: 8456 RVA: 0x001725EA File Offset: 0x001707EA
		public override long Length
		{
			get
			{
				return ((long)this.Buffer.LengthBits - this.offset) / 8L;
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06002109 RID: 8457 RVA: 0x00172602 File Offset: 0x00170802
		// (set) Token: 0x0600210A RID: 8458 RVA: 0x00172619 File Offset: 0x00170819
		public override long Position
		{
			get
			{
				return (this.Buffer.Position - this.offset) / 8L;
			}
			set
			{
				this.Buffer.Position = this.offset + value * 8L;
			}
		}

		// Token: 0x0600210B RID: 8459 RVA: 0x00172631 File Offset: 0x00170831
		public override void Flush()
		{
		}

		// Token: 0x0600210C RID: 8460 RVA: 0x00172633 File Offset: 0x00170833
		public override int Read(byte[] buffer, int offset, int count)
		{
			this.Buffer.ReadBytes(buffer, offset, count);
			return count;
		}

		// Token: 0x0600210D RID: 8461 RVA: 0x00172644 File Offset: 0x00170844
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
				this.Position = this.Length + offset;
				break;
			}
			return this.Position;
		}

		// Token: 0x0600210E RID: 8462 RVA: 0x00172692 File Offset: 0x00170892
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600210F RID: 8463 RVA: 0x00172699 File Offset: 0x00170899
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		// Token: 0x040013E4 RID: 5092
		private long offset;

		// Token: 0x040013E5 RID: 5093
		public NetBuffer Buffer;
	}
}
