using System;
using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	// Token: 0x0200000E RID: 14
	public class GZipStream : Stream
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00004DB5 File Offset: 0x00002FB5
		// (set) Token: 0x0600004D RID: 77 RVA: 0x00004DBD File Offset: 0x00002FBD
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				this._Comment = value;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600004E RID: 78 RVA: 0x00004DD9 File Offset: 0x00002FD9
		// (set) Token: 0x0600004F RID: 79 RVA: 0x00004DE4 File Offset: 0x00002FE4
		public string FileName
		{
			get
			{
				return this._FileName;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				this._FileName = value;
				if (this._FileName == null)
				{
					return;
				}
				if (this._FileName.IndexOf("/") != -1)
				{
					this._FileName = this._FileName.Replace("/", "\\");
				}
				if (this._FileName.EndsWith("\\"))
				{
					throw new Exception("Illegal filename");
				}
				if (this._FileName.IndexOf("\\") != -1)
				{
					this._FileName = Path.GetFileName(this._FileName);
				}
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00004E83 File Offset: 0x00003083
		public int Crc32
		{
			get
			{
				return this._Crc32;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00004E8B File Offset: 0x0000308B
		public GZipStream(Stream stream, CompressionMode mode) : this(stream, mode, CompressionLevel.Default, false)
		{
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004E97 File Offset: 0x00003097
		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level) : this(stream, mode, level, false)
		{
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004EA3 File Offset: 0x000030A3
		public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen) : this(stream, mode, CompressionLevel.Default, leaveOpen)
		{
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004EAF File Offset: 0x000030AF
		public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			this._baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00004ECC File Offset: 0x000030CC
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00004ED9 File Offset: 0x000030D9
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
					throw new ObjectDisposedException("GZipStream");
				}
				this._baseStream._flushMode = value;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00004EFA File Offset: 0x000030FA
		// (set) Token: 0x06000058 RID: 88 RVA: 0x00004F08 File Offset: 0x00003108
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
					throw new ObjectDisposedException("GZipStream");
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

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00004F74 File Offset: 0x00003174
		public virtual long TotalIn
		{
			get
			{
				return this._baseStream._z.TotalBytesIn;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00004F86 File Offset: 0x00003186
		public virtual long TotalOut
		{
			get
			{
				return this._baseStream._z.TotalBytesOut;
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00004F98 File Offset: 0x00003198
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this._disposed)
				{
					if (disposing && this._baseStream != null)
					{
						this._baseStream.Close();
						this._Crc32 = this._baseStream.Crc32;
					}
					this._disposed = true;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00004FF8 File Offset: 0x000031F8
		public override bool CanRead
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				return this._baseStream._stream.CanRead;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600005D RID: 93 RVA: 0x0000501D File Offset: 0x0000321D
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600005E RID: 94 RVA: 0x00005020 File Offset: 0x00003220
		public override bool CanWrite
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				return this._baseStream._stream.CanWrite;
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00005045 File Offset: 0x00003245
		public override void Flush()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			this._baseStream.Flush();
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000060 RID: 96 RVA: 0x00005065 File Offset: 0x00003265
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000061 RID: 97 RVA: 0x0000506C File Offset: 0x0000326C
		// (set) Token: 0x06000062 RID: 98 RVA: 0x000050CD File Offset: 0x000032CD
		public override long Position
		{
			get
			{
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
				{
					return this._baseStream._z.TotalBytesOut + (long)this._headerByteCount;
				}
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
				{
					return this._baseStream._z.TotalBytesIn + (long)this._baseStream._gzipHeaderByteCount;
				}
				return 0L;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000050D4 File Offset: 0x000032D4
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			int result = this._baseStream.Read(buffer, offset, count);
			if (!this._firstReadDone)
			{
				this._firstReadDone = true;
				this.FileName = this._baseStream._GzipFileName;
				this.Comment = this._baseStream._GzipComment;
			}
			return result;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00005133 File Offset: 0x00003333
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000065 RID: 101 RVA: 0x0000513A File Offset: 0x0000333A
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00005144 File Offset: 0x00003344
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("GZipStream");
			}
			if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				if (!this._baseStream._wantCompress)
				{
					throw new InvalidOperationException();
				}
				this._headerByteCount = this.EmitHeader();
			}
			this._baseStream.Write(buffer, offset, count);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000051A4 File Offset: 0x000033A4
		private int EmitHeader()
		{
			byte[] commentBytes = (this.Comment == null) ? null : GZipStream.iso8859dash1.GetBytes(this.Comment);
			byte[] filenameBytes = (this.FileName == null) ? null : GZipStream.iso8859dash1.GetBytes(this.FileName);
			int cbLength = (this.Comment == null) ? 0 : (commentBytes.Length + 1);
			int fnLength = (this.FileName == null) ? 0 : (filenameBytes.Length + 1);
			byte[] header = new byte[10 + cbLength + fnLength];
			int i = 0;
			header[i++] = 31;
			header[i++] = 139;
			header[i++] = 8;
			byte flag = 0;
			if (this.Comment != null)
			{
				flag ^= 16;
			}
			if (this.FileName != null)
			{
				flag ^= 8;
			}
			header[i++] = flag;
			if (this.LastModified == null)
			{
				this.LastModified = new DateTime?(DateTime.Now);
			}
			Array.Copy(BitConverter.GetBytes((int)(this.LastModified.Value - GZipStream._unixEpoch).TotalSeconds), 0, header, i, 4);
			i += 4;
			header[i++] = 0;
			header[i++] = byte.MaxValue;
			if (fnLength != 0)
			{
				Array.Copy(filenameBytes, 0, header, i, fnLength - 1);
				i += fnLength - 1;
				header[i++] = 0;
			}
			if (cbLength != 0)
			{
				Array.Copy(commentBytes, 0, header, i, cbLength - 1);
				i += cbLength - 1;
				header[i++] = 0;
			}
			this._baseStream._stream.Write(header, 0, header.Length);
			return header.Length;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00005340 File Offset: 0x00003540
		public static byte[] CompressString(string s)
		{
			byte[] result;
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressString(s, compressor);
				result = ms.ToArray();
			}
			return result;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00005388 File Offset: 0x00003588
		public static byte[] CompressBuffer(byte[] b)
		{
			byte[] result;
			using (MemoryStream ms = new MemoryStream())
			{
				Stream compressor = new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZlibBaseStream.CompressBuffer(b, compressor);
				result = ms.ToArray();
			}
			return result;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x000053D0 File Offset: 0x000035D0
		public static string UncompressString(byte[] compressed)
		{
			string result;
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				result = ZlibBaseStream.UncompressString(compressed, decompressor);
			}
			return result;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00005414 File Offset: 0x00003614
		public static byte[] UncompressBuffer(byte[] compressed)
		{
			byte[] result;
			using (MemoryStream input = new MemoryStream(compressed))
			{
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				result = ZlibBaseStream.UncompressBuffer(compressed, decompressor);
			}
			return result;
		}

		// Token: 0x04000066 RID: 102
		public DateTime? LastModified;

		// Token: 0x04000067 RID: 103
		private int _headerByteCount;

		// Token: 0x04000068 RID: 104
		internal ZlibBaseStream _baseStream;

		// Token: 0x04000069 RID: 105
		private bool _disposed;

		// Token: 0x0400006A RID: 106
		private bool _firstReadDone;

		// Token: 0x0400006B RID: 107
		private string _FileName;

		// Token: 0x0400006C RID: 108
		private string _Comment;

		// Token: 0x0400006D RID: 109
		private int _Crc32;

		// Token: 0x0400006E RID: 110
		internal static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// Token: 0x0400006F RID: 111
		internal static readonly Encoding iso8859dash1 = Encoding.GetEncoding("iso-8859-1");
	}
}
