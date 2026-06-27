using System;
using System.IO;

namespace Ionic.Crc
{
	// Token: 0x02000024 RID: 36
	public class CrcCalculatorStream : Stream, IDisposable
	{
		// Token: 0x060000FE RID: 254 RVA: 0x0000ABAA File Offset: 0x00008DAA
		public CrcCalculatorStream(Stream stream) : this(true, CrcCalculatorStream.UnsetLengthLimit, stream, null)
		{
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000ABBA File Offset: 0x00008DBA
		public CrcCalculatorStream(Stream stream, bool leaveOpen) : this(leaveOpen, CrcCalculatorStream.UnsetLengthLimit, stream, null)
		{
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000ABCA File Offset: 0x00008DCA
		public CrcCalculatorStream(Stream stream, long length) : this(true, length, stream, null)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000ABE6 File Offset: 0x00008DE6
		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen) : this(leaveOpen, length, stream, null)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0000AC02 File Offset: 0x00008E02
		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen, CRC32 crc32) : this(leaveOpen, length, stream, crc32)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000AC1F File Offset: 0x00008E1F
		private CrcCalculatorStream(bool leaveOpen, long length, Stream stream, CRC32 crc32)
		{
			this._innerStream = stream;
			this._Crc32 = (crc32 ?? new CRC32());
			this._lengthLimit = length;
			this._leaveOpen = leaveOpen;
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000104 RID: 260 RVA: 0x0000AC56 File Offset: 0x00008E56
		public long TotalBytesSlurped
		{
			get
			{
				return this._Crc32.TotalBytesRead;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000105 RID: 261 RVA: 0x0000AC63 File Offset: 0x00008E63
		public int Crc
		{
			get
			{
				return this._Crc32.Crc32Result;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000106 RID: 262 RVA: 0x0000AC70 File Offset: 0x00008E70
		// (set) Token: 0x06000107 RID: 263 RVA: 0x0000AC78 File Offset: 0x00008E78
		public bool LeaveOpen
		{
			get
			{
				return this._leaveOpen;
			}
			set
			{
				this._leaveOpen = value;
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000AC84 File Offset: 0x00008E84
		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesToRead = count;
			if (this._lengthLimit != CrcCalculatorStream.UnsetLengthLimit)
			{
				if (this._Crc32.TotalBytesRead >= this._lengthLimit)
				{
					return 0;
				}
				long bytesRemaining = this._lengthLimit - this._Crc32.TotalBytesRead;
				if (bytesRemaining < (long)count)
				{
					bytesToRead = (int)bytesRemaining;
				}
			}
			int i = this._innerStream.Read(buffer, offset, bytesToRead);
			if (i > 0)
			{
				this._Crc32.SlurpBlock(buffer, offset, i);
			}
			return i;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000ACF2 File Offset: 0x00008EF2
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				this._Crc32.SlurpBlock(buffer, offset, count);
			}
			this._innerStream.Write(buffer, offset, count);
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600010A RID: 266 RVA: 0x0000AD14 File Offset: 0x00008F14
		public override bool CanRead
		{
			get
			{
				return this._innerStream.CanRead;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600010B RID: 267 RVA: 0x0000AD21 File Offset: 0x00008F21
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600010C RID: 268 RVA: 0x0000AD24 File Offset: 0x00008F24
		public override bool CanWrite
		{
			get
			{
				return this._innerStream.CanWrite;
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000AD31 File Offset: 0x00008F31
		public override void Flush()
		{
			this._innerStream.Flush();
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600010E RID: 270 RVA: 0x0000AD3E File Offset: 0x00008F3E
		public override long Length
		{
			get
			{
				if (this._lengthLimit == CrcCalculatorStream.UnsetLengthLimit)
				{
					return this._innerStream.Length;
				}
				return this._lengthLimit;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x0600010F RID: 271 RVA: 0x0000AD5F File Offset: 0x00008F5F
		// (set) Token: 0x06000110 RID: 272 RVA: 0x0000AD6C File Offset: 0x00008F6C
		public override long Position
		{
			get
			{
				return this._Crc32.TotalBytesRead;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0000AD73 File Offset: 0x00008F73
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000AD7A File Offset: 0x00008F7A
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000AD81 File Offset: 0x00008F81
		void IDisposable.Dispose()
		{
			this.Close();
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000AD89 File Offset: 0x00008F89
		public override void Close()
		{
			base.Close();
			if (!this._leaveOpen)
			{
				this._innerStream.Close();
			}
		}

		// Token: 0x0400013A RID: 314
		private static readonly long UnsetLengthLimit = -99L;

		// Token: 0x0400013B RID: 315
		internal Stream _innerStream;

		// Token: 0x0400013C RID: 316
		private CRC32 _Crc32;

		// Token: 0x0400013D RID: 317
		private long _lengthLimit = -99L;

		// Token: 0x0400013E RID: 318
		private bool _leaveOpen;
	}
}
