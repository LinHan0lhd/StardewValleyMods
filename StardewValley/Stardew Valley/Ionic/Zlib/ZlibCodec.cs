using System;
using System.Runtime.InteropServices;

namespace Ionic.Zlib
{
	// Token: 0x02000020 RID: 32
	[Guid("ebc25cf6-9120-4283-b972-0e5520d0000D")]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public sealed class ZlibCodec
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000BA RID: 186 RVA: 0x00009EF0 File Offset: 0x000080F0
		public int Adler32
		{
			get
			{
				return (int)this._Adler32;
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00009EF8 File Offset: 0x000080F8
		public ZlibCodec()
		{
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00009F10 File Offset: 0x00008110
		public ZlibCodec(CompressionMode mode)
		{
			if (mode == CompressionMode.Compress)
			{
				if (this.InitializeDeflate() != 0)
				{
					throw new ZlibException("Cannot initialize for deflate.");
				}
			}
			else
			{
				if (mode != CompressionMode.Decompress)
				{
					throw new ZlibException("Invalid ZlibStreamFlavor.");
				}
				if (this.InitializeInflate() != 0)
				{
					throw new ZlibException("Cannot initialize for inflate.");
				}
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00009F6A File Offset: 0x0000816A
		public int InitializeInflate()
		{
			return this.InitializeInflate(this.WindowBits);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00009F78 File Offset: 0x00008178
		public int InitializeInflate(bool expectRfc1950Header)
		{
			return this.InitializeInflate(this.WindowBits, expectRfc1950Header);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00009F87 File Offset: 0x00008187
		public int InitializeInflate(int windowBits)
		{
			this.WindowBits = windowBits;
			return this.InitializeInflate(windowBits, true);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00009F98 File Offset: 0x00008198
		public int InitializeInflate(int windowBits, bool expectRfc1950Header)
		{
			this.WindowBits = windowBits;
			if (this.dstate != null)
			{
				throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
			}
			this.istate = new InflateManager(expectRfc1950Header);
			return this.istate.Initialize(this, windowBits);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00009FCD File Offset: 0x000081CD
		public int Inflate(FlushType flush)
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			return this.istate.Inflate(flush);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00009FEE File Offset: 0x000081EE
		public int EndInflate()
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			int result = this.istate.End();
			this.istate = null;
			return result;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000A015 File Offset: 0x00008215
		public int SyncInflate()
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			return this.istate.Sync();
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000A035 File Offset: 0x00008235
		public int InitializeDeflate()
		{
			return this._InternalInitializeDeflate(true);
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x0000A03E File Offset: 0x0000823E
		public int InitializeDeflate(CompressionLevel level)
		{
			this.CompressLevel = level;
			return this._InternalInitializeDeflate(true);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x0000A04E File Offset: 0x0000824E
		public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
		{
			this.CompressLevel = level;
			return this._InternalInitializeDeflate(wantRfc1950Header);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0000A05E File Offset: 0x0000825E
		public int InitializeDeflate(CompressionLevel level, int bits)
		{
			this.CompressLevel = level;
			this.WindowBits = bits;
			return this._InternalInitializeDeflate(true);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000A075 File Offset: 0x00008275
		public int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
		{
			this.CompressLevel = level;
			this.WindowBits = bits;
			return this._InternalInitializeDeflate(wantRfc1950Header);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0000A08C File Offset: 0x0000828C
		private int _InternalInitializeDeflate(bool wantRfc1950Header)
		{
			if (this.istate != null)
			{
				throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
			}
			this.dstate = new DeflateManager();
			this.dstate.WantRfc1950HeaderBytes = wantRfc1950Header;
			return this.dstate.Initialize(this, this.CompressLevel, this.WindowBits, this.Strategy);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0000A0E1 File Offset: 0x000082E1
		public int Deflate(FlushType flush)
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			return this.dstate.Deflate(flush);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0000A102 File Offset: 0x00008302
		public int EndDeflate()
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			this.dstate = null;
			return 0;
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000A11F File Offset: 0x0000831F
		public void ResetDeflate()
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			this.dstate.Reset();
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000A13F File Offset: 0x0000833F
		public int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			return this.dstate.SetParams(level, strategy);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000A161 File Offset: 0x00008361
		public int SetDictionary(byte[] dictionary)
		{
			if (this.istate != null)
			{
				return this.istate.SetDictionary(dictionary);
			}
			if (this.dstate != null)
			{
				return this.dstate.SetDictionary(dictionary);
			}
			throw new ZlibException("No Inflate or Deflate state!");
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000A198 File Offset: 0x00008398
		internal void flush_pending()
		{
			int len = this.dstate.pendingCount;
			if (len > this.AvailableBytesOut)
			{
				len = this.AvailableBytesOut;
			}
			if (len == 0)
			{
				return;
			}
			if (this.dstate.pending.Length <= this.dstate.nextPending || this.OutputBuffer.Length <= this.NextOut || this.dstate.pending.Length < this.dstate.nextPending + len || this.OutputBuffer.Length < this.NextOut + len)
			{
				throw new ZlibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", this.dstate.pending.Length, this.dstate.pendingCount));
			}
			Array.Copy(this.dstate.pending, this.dstate.nextPending, this.OutputBuffer, this.NextOut, len);
			this.NextOut += len;
			this.dstate.nextPending += len;
			this.TotalBytesOut += (long)len;
			this.AvailableBytesOut -= len;
			this.dstate.pendingCount -= len;
			if (this.dstate.pendingCount == 0)
			{
				this.dstate.nextPending = 0;
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000A2E4 File Offset: 0x000084E4
		internal int read_buf(byte[] buf, int start, int size)
		{
			int len = this.AvailableBytesIn;
			if (len > size)
			{
				len = size;
			}
			if (len == 0)
			{
				return 0;
			}
			this.AvailableBytesIn -= len;
			if (this.dstate.WantRfc1950HeaderBytes)
			{
				this._Adler32 = Adler.Adler32(this._Adler32, this.InputBuffer, this.NextIn, len);
			}
			Array.Copy(this.InputBuffer, this.NextIn, buf, start, len);
			this.NextIn += len;
			this.TotalBytesIn += (long)len;
			return len;
		}

		// Token: 0x04000119 RID: 281
		public byte[] InputBuffer;

		// Token: 0x0400011A RID: 282
		public int NextIn;

		// Token: 0x0400011B RID: 283
		public int AvailableBytesIn;

		// Token: 0x0400011C RID: 284
		public long TotalBytesIn;

		// Token: 0x0400011D RID: 285
		public byte[] OutputBuffer;

		// Token: 0x0400011E RID: 286
		public int NextOut;

		// Token: 0x0400011F RID: 287
		public int AvailableBytesOut;

		// Token: 0x04000120 RID: 288
		public long TotalBytesOut;

		// Token: 0x04000121 RID: 289
		public string Message;

		// Token: 0x04000122 RID: 290
		internal DeflateManager dstate;

		// Token: 0x04000123 RID: 291
		internal InflateManager istate;

		// Token: 0x04000124 RID: 292
		internal uint _Adler32;

		// Token: 0x04000125 RID: 293
		public CompressionLevel CompressLevel = CompressionLevel.Default;

		// Token: 0x04000126 RID: 294
		public int WindowBits = 15;

		// Token: 0x04000127 RID: 295
		public CompressionStrategy Strategy;
	}
}
