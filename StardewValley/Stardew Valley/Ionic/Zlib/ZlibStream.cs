using System;
using System.IO;

namespace Ionic.Zlib
{
	// Token: 0x02000022 RID: 34
	public class ZlibStream : Stream
	{
		// Token: 0x060000D1 RID: 209 RVA: 0x0000A36E File Offset: 0x0000856E
		public ZlibStream(Stream stream, CompressionMode mode) : this(stream, mode, CompressionLevel.Default, false)
		{
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x0000A37A File Offset: 0x0000857A
		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level) : this(stream, mode, level, false)
		{
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0000A386 File Offset: 0x00008586
		public ZlibStream(Stream stream, CompressionMode mode, bool leaveOpen) : this(stream, mode, CompressionLevel.Default, leaveOpen)
		{
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0000A392 File Offset: 0x00008592
		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			this._baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.ZLIB, leaveOpen);
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x0000A3AF File Offset: 0x000085AF
		// (set) Token: 0x060000D6 RID: 214 RVA: 0x0000A3BC File Offset: 0x000085BC
		public virtual FlushType FlushMode
		{
			get
			{
				return this._baseStream._flushMode;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("ZlibStream");
				}
				this._baseStream._flushMode = value;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000D7 RID: 215 RVA: 0x0000A3DD File Offset: 0x000085DD
		// (set) Token: 0x060000D8 RID: 216 RVA: 0x0000A3EC File Offset: 0x000085EC
		public int BufferSize
		{
			get
			{
				return this._baseStream._bufferSize;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("ZlibStream");
				}
				if (this._baseStream._workingBuffer != null)
				{
					throw new ZlibException("The working buffer is already set.");
				}
				if (value < 1024)
				{
					throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
				}
				this._baseStream._bufferSize = value;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x0000A458 File Offset: 0x00008658
		public virtual long TotalIn
		{
			get
			{
				return this._baseStream._z.TotalBytesIn;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000DA RID: 218 RVA: 0x0000A46A File Offset: 0x0000866A
		public virtual long TotalOut
		{
			get
			{
				return this._baseStream._z.TotalBytesOut;
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000A47C File Offset: 0x0000867C
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this._disposed)
				{
					if (disposing && this._baseStream != null)
					{
						this._baseStream.Close();
					}
					this._disposed = true;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000DC RID: 220 RVA: 0x0000A4C8 File Offset: 0x000086C8
		public override bool CanRead
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("ZlibStream");
				}
				return this._baseStream._stream.CanRead;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000DD RID: 221 RVA: 0x0000A4ED File Offset: 0x000086ED
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000DE RID: 222 RVA: 0x0000A4F0 File Offset: 0x000086F0
		public override bool CanWrite
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("ZlibStream");
				}
				return this._baseStream._stream.CanWrite;
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000A515 File Offset: 0x00008715
		public override void Flush()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("ZlibStream");
			}
			this._baseStream.Flush();
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x0000A535 File Offset: 0x00008735
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x0000A53C File Offset: 0x0000873C
		// (set) Token: 0x060000E2 RID: 226 RVA: 0x0000A588 File Offset: 0x00008788
		public override long Position
		{
			get
			{
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
				{
					return this._baseStream._z.TotalBytesOut;
				}
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
				{
					return this._baseStream._z.TotalBytesIn;
				}
				return 0L;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000A58F File Offset: 0x0000878F
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("ZlibStream");
			}
			return this._baseStream.Read(buffer, offset, count);
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x0000A5B2 File Offset: 0x000087B2
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000A5B9 File Offset: 0x000087B9
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0000A5C0 File Offset: 0x000087C0
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("ZlibStream");
			}
			this._baseStream.Write(buffer, offset, count);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0000A5E4 File Offset: 0x000087E4
		public static byte[] CompressString(string s)
		{
			byte[] result;
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressString(s, compressor);
				result = ms.ToArray();
			}
			return result;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000A62C File Offset: 0x0000882C
		public static byte[] CompressBuffer(byte[] b)
		{
			byte[] result;
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressBuffer(b, compressor);
				result = ms.ToArray();
			}
			return result;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000A674 File Offset: 0x00008874
		public static string UncompressString(byte[] compressed)
		{
			string result;
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new ZlibStream(input, CompressionMode.Decompress);
				result = ZlibBaseStream.UncompressString(compressed, decompressor);
			}
			return result;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0000A6B8 File Offset: 0x000088B8
		public static byte[] UncompressBuffer(byte[] compressed)
		{
			byte[] result;
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new ZlibStream(input, CompressionMode.Decompress);
				result = ZlibBaseStream.UncompressBuffer(compressed, decompressor);
			}
			return result;
		}

		// Token: 0x04000132 RID: 306
		internal ZlibBaseStream _baseStream;

		// Token: 0x04000133 RID: 307
		private bool _disposed;
	}
}
