using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Crc;

namespace Ionic.Zlib
{
	// Token: 0x0200001F RID: 31
	internal class ZlibBaseStream : Stream
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x0000925A File Offset: 0x0000745A
		internal int Crc32
		{
			get
			{
				if (this.crc == null)
				{
					return 0;
				}
				return this.crc.Crc32Result;
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00009274 File Offset: 0x00007474
		public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen)
		{
			this._flushMode = FlushType.None;
			this._stream = stream;
			this._leaveOpen = leaveOpen;
			this._compressionMode = compressionMode;
			this._flavor = flavor;
			this._level = level;
			if (flavor == ZlibStreamFlavor.GZIP)
			{
				this.crc = new CRC32();
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x000092E5 File Offset: 0x000074E5
		protected internal bool _wantCompress
		{
			get
			{
				return this._compressionMode == CompressionMode.Compress;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x000092F0 File Offset: 0x000074F0
		private ZlibCodec z
		{
			get
			{
				if (this._z == null)
				{
					bool wantRfc1950Header = this._flavor == ZlibStreamFlavor.ZLIB;
					this._z = new ZlibCodec();
					if (this._compressionMode == CompressionMode.Decompress)
					{
						this._z.InitializeInflate(wantRfc1950Header);
					}
					else
					{
						this._z.Strategy = this.Strategy;
						this._z.InitializeDeflate(this._level, wantRfc1950Header);
					}
				}
				return this._z;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x00009360 File Offset: 0x00007560
		private byte[] workingBuffer
		{
			get
			{
				if (this._workingBuffer == null)
				{
					this._workingBuffer = new byte[this._bufferSize];
				}
				return this._workingBuffer;
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00009384 File Offset: 0x00007584
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.crc != null)
			{
				this.crc.SlurpBlock(buffer, offset, count);
			}
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				this._streamMode = ZlibBaseStream.StreamMode.Writer;
			}
			else if (this._streamMode != ZlibBaseStream.StreamMode.Writer)
			{
				throw new ZlibException("Cannot Write after Reading.");
			}
			if (count == 0)
			{
				return;
			}
			this.z.InputBuffer = buffer;
			this._z.NextIn = offset;
			this._z.AvailableBytesIn = count;
			for (;;)
			{
				this._z.OutputBuffer = this.workingBuffer;
				this._z.NextOut = 0;
				this._z.AvailableBytesOut = this._workingBuffer.Length;
				int rc = this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode);
				if (rc != 0 && rc != 1)
				{
					break;
				}
				this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
				bool done = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
				if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
				{
					done = (this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0);
				}
				if (done)
				{
					return;
				}
			}
			throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000950C File Offset: 0x0000770C
		private void finish()
		{
			if (this._z == null)
			{
				return;
			}
			if (this._streamMode == ZlibBaseStream.StreamMode.Writer)
			{
				int rc;
				for (;;)
				{
					this._z.OutputBuffer = this.workingBuffer;
					this._z.NextOut = 0;
					this._z.AvailableBytesOut = this._workingBuffer.Length;
					rc = (this._wantCompress ? this._z.Deflate(FlushType.Finish) : this._z.Inflate(FlushType.Finish));
					if (rc != 1 && rc != 0)
					{
						break;
					}
					if (this._workingBuffer.Length - this._z.AvailableBytesOut > 0)
					{
						this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
					}
					bool done = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
					if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
					{
						done = (this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0);
					}
					if (done)
					{
						goto Block_12;
					}
				}
				string verb = (this._wantCompress ? "de" : "in") + "flating";
				if (this._z.Message == null)
				{
					throw new ZlibException(string.Format("{0}: (rc = {1})", verb, rc));
				}
				throw new ZlibException(verb + ": " + this._z.Message);
				Block_12:
				this.Flush();
				if (this._flavor == ZlibStreamFlavor.GZIP)
				{
					if (this._wantCompress)
					{
						int c = this.crc.Crc32Result;
						this._stream.Write(BitConverter.GetBytes(c), 0, 4);
						int c2 = (int)(this.crc.TotalBytesRead & (long)((ulong)-1));
						this._stream.Write(BitConverter.GetBytes(c2), 0, 4);
						return;
					}
					throw new ZlibException("Writing with decompression is not supported.");
				}
			}
			else if (this._streamMode == ZlibBaseStream.StreamMode.Reader && this._flavor == ZlibStreamFlavor.GZIP)
			{
				if (this._wantCompress)
				{
					throw new ZlibException("Reading with compression is not supported.");
				}
				if (this._z.TotalBytesOut == 0L)
				{
					return;
				}
				byte[] trailer = new byte[8];
				if (this._z.AvailableBytesIn < 8)
				{
					Array.Copy(this._z.InputBuffer, this._z.NextIn, trailer, 0, this._z.AvailableBytesIn);
					int bytesNeeded = 8 - this._z.AvailableBytesIn;
					int bytesRead = this._stream.Read(trailer, this._z.AvailableBytesIn, bytesNeeded);
					if (bytesNeeded != bytesRead)
					{
						throw new ZlibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", this._z.AvailableBytesIn + bytesRead));
					}
				}
				else
				{
					Array.Copy(this._z.InputBuffer, this._z.NextIn, trailer, 0, trailer.Length);
				}
				int crc32_expected = BitConverter.ToInt32(trailer, 0);
				int crc32_actual = this.crc.Crc32Result;
				int isize_expected = BitConverter.ToInt32(trailer, 4);
				int isize_actual = (int)(this._z.TotalBytesOut & (long)((ulong)-1));
				if (crc32_actual != crc32_expected)
				{
					throw new ZlibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32_actual, crc32_expected));
				}
				if (isize_actual != isize_expected)
				{
					throw new ZlibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", isize_actual, isize_expected));
				}
			}
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000985C File Offset: 0x00007A5C
		private void end()
		{
			if (this.z == null)
			{
				return;
			}
			if (this._wantCompress)
			{
				this._z.EndDeflate();
			}
			else
			{
				this._z.EndInflate();
			}
			this._z = null;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00009890 File Offset: 0x00007A90
		public override void Close()
		{
			if (this._stream == null)
			{
				return;
			}
			try
			{
				this.finish();
			}
			finally
			{
				this.end();
				if (!this._leaveOpen)
				{
					this._stream.Close();
				}
				this._stream = null;
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000098E0 File Offset: 0x00007AE0
		public override void Flush()
		{
			this._stream.Flush();
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000098ED File Offset: 0x00007AED
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060000AC RID: 172 RVA: 0x000098F4 File Offset: 0x00007AF4
		public override void SetLength(long value)
		{
			this._stream.SetLength(value);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00009904 File Offset: 0x00007B04
		private string ReadZeroTerminatedString()
		{
			List<byte> list = new List<byte>();
			bool done = false;
			while (this._stream.Read(this._buf1, 0, 1) == 1)
			{
				if (this._buf1[0] == 0)
				{
					done = true;
				}
				else
				{
					list.Add(this._buf1[0]);
				}
				if (done)
				{
					byte[] a = list.ToArray();
					return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
				}
			}
			throw new ZlibException("Unexpected EOF reading GZIP header.");
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00009970 File Offset: 0x00007B70
		private int _ReadAndValidateGzipHeader()
		{
			int totalBytesRead = 0;
			byte[] header = new byte[10];
			int i = this._stream.Read(header, 0, header.Length);
			if (i == 0)
			{
				return 0;
			}
			if (i != 10)
			{
				throw new ZlibException("Not a valid GZIP stream.");
			}
			if (header[0] != 31 || header[1] != 139 || header[2] != 8)
			{
				throw new ZlibException("Bad GZIP header.");
			}
			int timet = BitConverter.ToInt32(header, 4);
			this._GzipMtime = GZipStream._unixEpoch.AddSeconds((double)timet);
			totalBytesRead += i;
			if ((header[3] & 4) == 4)
			{
				i = this._stream.Read(header, 0, 2);
				totalBytesRead += i;
				short extraLength = (short)((int)header[0] + (int)header[1] * 256);
				byte[] extra = new byte[(int)extraLength];
				i = this._stream.Read(extra, 0, extra.Length);
				if (i != (int)extraLength)
				{
					throw new ZlibException("Unexpected end-of-file reading GZIP header.");
				}
				totalBytesRead += i;
			}
			if ((header[3] & 8) == 8)
			{
				this._GzipFileName = this.ReadZeroTerminatedString();
			}
			if ((header[3] & 16) == 16)
			{
				this._GzipComment = this.ReadZeroTerminatedString();
			}
			if ((header[3] & 2) == 2)
			{
				this.Read(this._buf1, 0, 1);
			}
			return totalBytesRead;
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00009A8C File Offset: 0x00007C8C
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				if (!this._stream.CanRead)
				{
					throw new ZlibException("The stream is not readable.");
				}
				this._streamMode = ZlibBaseStream.StreamMode.Reader;
				this.z.AvailableBytesIn = 0;
				if (this._flavor == ZlibStreamFlavor.GZIP)
				{
					this._gzipHeaderByteCount = this._ReadAndValidateGzipHeader();
					if (this._gzipHeaderByteCount == 0)
					{
						return 0;
					}
				}
			}
			if (this._streamMode != ZlibBaseStream.StreamMode.Reader)
			{
				throw new ZlibException("Cannot Read after Writing.");
			}
			if (count == 0)
			{
				return 0;
			}
			if (this.nomoreinput && this._wantCompress)
			{
				return 0;
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (offset < buffer.GetLowerBound(0))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.GetLength(0))
			{
				throw new ArgumentOutOfRangeException("count");
			}
			this._z.OutputBuffer = buffer;
			this._z.NextOut = offset;
			this._z.AvailableBytesOut = count;
			this._z.InputBuffer = this.workingBuffer;
			int rc;
			for (;;)
			{
				if (this._z.AvailableBytesIn == 0 && !this.nomoreinput)
				{
					this._z.NextIn = 0;
					this._z.AvailableBytesIn = this._stream.Read(this._workingBuffer, 0, this._workingBuffer.Length);
					if (this._z.AvailableBytesIn == 0)
					{
						this.nomoreinput = true;
					}
				}
				rc = (this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode));
				if (this.nomoreinput && rc == -5)
				{
					break;
				}
				if (rc != 0 && rc != 1)
				{
					goto Block_20;
				}
				if (((this.nomoreinput || rc == 1) && this._z.AvailableBytesOut == count) || this._z.AvailableBytesOut <= 0 || this.nomoreinput || rc != 0)
				{
					goto IL_20A;
				}
			}
			return 0;
			Block_20:
			throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", this._wantCompress ? "de" : "in", rc, this._z.Message));
			IL_20A:
			if (this._z.AvailableBytesOut > 0)
			{
				if (rc == 0)
				{
					int availableBytesIn = this._z.AvailableBytesIn;
				}
				if (this.nomoreinput && this._wantCompress)
				{
					rc = this._z.Deflate(FlushType.Finish);
					if (rc != 0 && rc != 1)
					{
						throw new ZlibException(string.Format("Deflating:  rc={0}  msg={1}", rc, this._z.Message));
					}
				}
			}
			rc = count - this._z.AvailableBytesOut;
			if (this.crc != null)
			{
				this.crc.SlurpBlock(buffer, offset, rc);
			}
			return rc;
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00009D2A File Offset: 0x00007F2A
		public override bool CanRead
		{
			get
			{
				return this._stream.CanRead;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00009D37 File Offset: 0x00007F37
		public override bool CanSeek
		{
			get
			{
				return this._stream.CanSeek;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x00009D44 File Offset: 0x00007F44
		public override bool CanWrite
		{
			get
			{
				return this._stream.CanWrite;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00009D51 File Offset: 0x00007F51
		public override long Length
		{
			get
			{
				return this._stream.Length;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x00009D5E File Offset: 0x00007F5E
		// (set) Token: 0x060000B5 RID: 181 RVA: 0x00009D65 File Offset: 0x00007F65
		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00009D6C File Offset: 0x00007F6C
		public static void CompressString(string s, Stream compressor)
		{
			byte[] uncompressed = Encoding.UTF8.GetBytes(s);
			try
			{
				compressor.Write(uncompressed, 0, uncompressed.Length);
			}
			finally
			{
				if (compressor != null)
				{
					((IDisposable)compressor).Dispose();
				}
			}
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00009DB0 File Offset: 0x00007FB0
		public static void CompressBuffer(byte[] b, Stream compressor)
		{
			try
			{
				compressor.Write(b, 0, b.Length);
			}
			finally
			{
				if (compressor != null)
				{
					((IDisposable)compressor).Dispose();
				}
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00009DE8 File Offset: 0x00007FE8
		public static string UncompressString(byte[] compressed, Stream decompressor)
		{
			byte[] working = new byte[1024];
			Encoding encoding = Encoding.UTF8;
			string result;
			using (MemoryStream output = new MemoryStream())
			{
				try
				{
					int i;
					while ((i = decompressor.Read(working, 0, working.Length)) != 0)
					{
						output.Write(working, 0, i);
					}
				}
				finally
				{
					if (decompressor != null)
					{
						((IDisposable)decompressor).Dispose();
					}
				}
				output.Seek(0L, SeekOrigin.Begin);
				result = new StreamReader(output, encoding).ReadToEnd();
			}
			return result;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00009E78 File Offset: 0x00008078
		public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
		{
			byte[] working = new byte[1024];
			byte[] result;
			using (MemoryStream output = new MemoryStream())
			{
				try
				{
					int i;
					while ((i = decompressor.Read(working, 0, working.Length)) != 0)
					{
						output.Write(working, 0, i);
					}
				}
				finally
				{
					if (decompressor != null)
					{
						((IDisposable)decompressor).Dispose();
					}
				}
				result = output.ToArray();
			}
			return result;
		}

		// Token: 0x04000107 RID: 263
		protected internal ZlibCodec _z;

		// Token: 0x04000108 RID: 264
		protected internal ZlibBaseStream.StreamMode _streamMode = ZlibBaseStream.StreamMode.Undefined;

		// Token: 0x04000109 RID: 265
		protected internal FlushType _flushMode;

		// Token: 0x0400010A RID: 266
		protected internal ZlibStreamFlavor _flavor;

		// Token: 0x0400010B RID: 267
		protected internal CompressionMode _compressionMode;

		// Token: 0x0400010C RID: 268
		protected internal CompressionLevel _level;

		// Token: 0x0400010D RID: 269
		protected internal bool _leaveOpen;

		// Token: 0x0400010E RID: 270
		protected internal byte[] _workingBuffer;

		// Token: 0x0400010F RID: 271
		protected internal int _bufferSize = 16384;

		// Token: 0x04000110 RID: 272
		protected internal byte[] _buf1 = new byte[1];

		// Token: 0x04000111 RID: 273
		protected internal Stream _stream;

		// Token: 0x04000112 RID: 274
		protected internal CompressionStrategy Strategy;

		// Token: 0x04000113 RID: 275
		private CRC32 crc;

		// Token: 0x04000114 RID: 276
		protected internal string _GzipFileName;

		// Token: 0x04000115 RID: 277
		protected internal string _GzipComment;

		// Token: 0x04000116 RID: 278
		protected internal DateTime _GzipMtime;

		// Token: 0x04000117 RID: 279
		protected internal int _gzipHeaderByteCount;

		// Token: 0x04000118 RID: 280
		private bool nomoreinput;

		// Token: 0x020003C3 RID: 963
		internal enum StreamMode
		{
			// Token: 0x04002680 RID: 9856
			Writer,
			// Token: 0x04002681 RID: 9857
			Reader,
			// Token: 0x04002682 RID: 9858
			Undefined
		}
	}
}
