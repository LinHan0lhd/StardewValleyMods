using System;

namespace Ionic.Zlib
{
	// Token: 0x02000012 RID: 18
	internal sealed class InflateManager
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00007807 File Offset: 0x00005A07
		// (set) Token: 0x0600007B RID: 123 RVA: 0x0000780F File Offset: 0x00005A0F
		internal bool HandleRfc1950HeaderBytes
		{
			get
			{
				return this._handleRfc1950HeaderBytes;
			}
			set
			{
				this._handleRfc1950HeaderBytes = value;
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00007818 File Offset: 0x00005A18
		public InflateManager()
		{
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00007827 File Offset: 0x00005A27
		public InflateManager(bool expectRfc1950HeaderBytes)
		{
			this._handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00007840 File Offset: 0x00005A40
		internal int Reset()
		{
			this._codec.TotalBytesIn = (this._codec.TotalBytesOut = 0L);
			this._codec.Message = null;
			this.mode = (this.HandleRfc1950HeaderBytes ? InflateManager.InflateManagerMode.METHOD : InflateManager.InflateManagerMode.BLOCKS);
			this.blocks.Reset();
			return 0;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00007893 File Offset: 0x00005A93
		internal int End()
		{
			if (this.blocks != null)
			{
				this.blocks.Free();
			}
			this.blocks = null;
			return 0;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000078B0 File Offset: 0x00005AB0
		internal int Initialize(ZlibCodec codec, int w)
		{
			this._codec = codec;
			this._codec.Message = null;
			this.blocks = null;
			if (w < 8 || w > 15)
			{
				this.End();
				throw new ZlibException("Bad window size.");
			}
			this.wbits = w;
			this.blocks = new InflateBlocks(codec, this.HandleRfc1950HeaderBytes ? this : null, 1 << w);
			this.Reset();
			return 0;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00007920 File Offset: 0x00005B20
		internal int Inflate(FlushType flush)
		{
			if (this._codec.InputBuffer == null)
			{
				throw new ZlibException("InputBuffer is null. ");
			}
			int f = 0;
			int r = -5;
			int nextIn;
			for (;;)
			{
				switch (this.mode)
				{
				case InflateManager.InflateManagerMode.METHOD:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					byte[] inputBuffer = this._codec.InputBuffer;
					ZlibCodec codec = this._codec;
					nextIn = codec.NextIn;
					codec.NextIn = nextIn + 1;
					if (((this.method = inputBuffer[nextIn]) & 15) != 8)
					{
						this.mode = InflateManager.InflateManagerMode.BAD;
						this._codec.Message = string.Format("unknown compression method (0x{0:X2})", this.method);
						this.marker = 5;
						continue;
					}
					if ((this.method >> 4) + 8 > this.wbits)
					{
						this.mode = InflateManager.InflateManagerMode.BAD;
						this._codec.Message = string.Format("invalid window size ({0})", (this.method >> 4) + 8);
						this.marker = 5;
						continue;
					}
					this.mode = InflateManager.InflateManagerMode.FLAG;
					continue;
				}
				case InflateManager.InflateManagerMode.FLAG:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					byte[] inputBuffer2 = this._codec.InputBuffer;
					ZlibCodec codec2 = this._codec;
					nextIn = codec2.NextIn;
					codec2.NextIn = nextIn + 1;
					int b = inputBuffer2[nextIn] & 255;
					if (((this.method << 8) + b) % 31 != 0)
					{
						this.mode = InflateManager.InflateManagerMode.BAD;
						this._codec.Message = "incorrect header check";
						this.marker = 5;
						continue;
					}
					this.mode = (((b & 32) == 0) ? InflateManager.InflateManagerMode.BLOCKS : InflateManager.InflateManagerMode.DICT4);
					continue;
				}
				case InflateManager.InflateManagerMode.DICT4:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					byte[] inputBuffer3 = this._codec.InputBuffer;
					ZlibCodec codec3 = this._codec;
					nextIn = codec3.NextIn;
					codec3.NextIn = nextIn + 1;
					this.expectedCheck = (uint)(inputBuffer3[nextIn] << 24 & (long)((ulong)-16777216));
					this.mode = InflateManager.InflateManagerMode.DICT3;
					continue;
				}
				case InflateManager.InflateManagerMode.DICT3:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					uint num = this.expectedCheck;
					byte[] inputBuffer4 = this._codec.InputBuffer;
					ZlibCodec codec4 = this._codec;
					nextIn = codec4.NextIn;
					codec4.NextIn = nextIn + 1;
					this.expectedCheck = num + (inputBuffer4[nextIn] << 16 & 16711680U);
					this.mode = InflateManager.InflateManagerMode.DICT2;
					continue;
				}
				case InflateManager.InflateManagerMode.DICT2:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					uint num2 = this.expectedCheck;
					byte[] inputBuffer5 = this._codec.InputBuffer;
					ZlibCodec codec5 = this._codec;
					nextIn = codec5.NextIn;
					codec5.NextIn = nextIn + 1;
					this.expectedCheck = num2 + (inputBuffer5[nextIn] << 8 & 65280U);
					this.mode = InflateManager.InflateManagerMode.DICT1;
					continue;
				}
				case InflateManager.InflateManagerMode.DICT1:
					goto IL_383;
				case InflateManager.InflateManagerMode.DICT0:
					goto IL_40D;
				case InflateManager.InflateManagerMode.BLOCKS:
					r = this.blocks.Process(r);
					if (r == -3)
					{
						this.mode = InflateManager.InflateManagerMode.BAD;
						this.marker = 0;
						continue;
					}
					if (r == 0)
					{
						r = f;
					}
					if (r != 1)
					{
						return r;
					}
					r = f;
					this.computedCheck = this.blocks.Reset();
					if (!this.HandleRfc1950HeaderBytes)
					{
						goto Block_16;
					}
					this.mode = InflateManager.InflateManagerMode.CHECK4;
					continue;
				case InflateManager.InflateManagerMode.CHECK4:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					byte[] inputBuffer6 = this._codec.InputBuffer;
					ZlibCodec codec6 = this._codec;
					nextIn = codec6.NextIn;
					codec6.NextIn = nextIn + 1;
					this.expectedCheck = (uint)(inputBuffer6[nextIn] << 24 & (long)((ulong)-16777216));
					this.mode = InflateManager.InflateManagerMode.CHECK3;
					continue;
				}
				case InflateManager.InflateManagerMode.CHECK3:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					uint num3 = this.expectedCheck;
					byte[] inputBuffer7 = this._codec.InputBuffer;
					ZlibCodec codec7 = this._codec;
					nextIn = codec7.NextIn;
					codec7.NextIn = nextIn + 1;
					this.expectedCheck = num3 + (inputBuffer7[nextIn] << 16 & 16711680U);
					this.mode = InflateManager.InflateManagerMode.CHECK2;
					continue;
				}
				case InflateManager.InflateManagerMode.CHECK2:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					uint num4 = this.expectedCheck;
					byte[] inputBuffer8 = this._codec.InputBuffer;
					ZlibCodec codec8 = this._codec;
					nextIn = codec8.NextIn;
					codec8.NextIn = nextIn + 1;
					this.expectedCheck = num4 + (inputBuffer8[nextIn] << 8 & 65280U);
					this.mode = InflateManager.InflateManagerMode.CHECK1;
					continue;
				}
				case InflateManager.InflateManagerMode.CHECK1:
				{
					if (this._codec.AvailableBytesIn == 0)
					{
						return r;
					}
					r = f;
					this._codec.AvailableBytesIn--;
					this._codec.TotalBytesIn += 1L;
					uint num5 = this.expectedCheck;
					byte[] inputBuffer9 = this._codec.InputBuffer;
					ZlibCodec codec9 = this._codec;
					nextIn = codec9.NextIn;
					codec9.NextIn = nextIn + 1;
					this.expectedCheck = num5 + (inputBuffer9[nextIn] & 255U);
					if (this.computedCheck != this.expectedCheck)
					{
						this.mode = InflateManager.InflateManagerMode.BAD;
						this._codec.Message = "incorrect data check";
						this.marker = 5;
						continue;
					}
					goto IL_6AE;
				}
				case InflateManager.InflateManagerMode.DONE:
					return 1;
				case InflateManager.InflateManagerMode.BAD:
					goto IL_6BA;
				}
				break;
			}
			throw new ZlibException("Stream error.");
			IL_383:
			if (this._codec.AvailableBytesIn == 0)
			{
				return r;
			}
			this._codec.AvailableBytesIn--;
			this._codec.TotalBytesIn += 1L;
			uint num6 = this.expectedCheck;
			byte[] inputBuffer10 = this._codec.InputBuffer;
			ZlibCodec codec10 = this._codec;
			nextIn = codec10.NextIn;
			codec10.NextIn = nextIn + 1;
			this.expectedCheck = num6 + (inputBuffer10[nextIn] & 255U);
			this._codec._Adler32 = this.expectedCheck;
			this.mode = InflateManager.InflateManagerMode.DICT0;
			return 2;
			IL_40D:
			this.mode = InflateManager.InflateManagerMode.BAD;
			this._codec.Message = "need dictionary";
			this.marker = 0;
			return -2;
			Block_16:
			this.mode = InflateManager.InflateManagerMode.DONE;
			return 1;
			IL_6AE:
			this.mode = InflateManager.InflateManagerMode.DONE;
			return 1;
			IL_6BA:
			throw new ZlibException(string.Format("Bad state ({0})", this._codec.Message));
		}

		// Token: 0x06000082 RID: 130 RVA: 0x0000800C File Offset: 0x0000620C
		internal int SetDictionary(byte[] dictionary)
		{
			int index = 0;
			int length = dictionary.Length;
			if (this.mode != InflateManager.InflateManagerMode.DICT0)
			{
				throw new ZlibException("Stream error.");
			}
			if (Adler.Adler32(1U, dictionary, 0, dictionary.Length) != this._codec._Adler32)
			{
				return -3;
			}
			this._codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
			if (length >= 1 << this.wbits)
			{
				length = (1 << this.wbits) - 1;
				index = dictionary.Length - length;
			}
			this.blocks.SetDictionary(dictionary, index, length);
			this.mode = InflateManager.InflateManagerMode.BLOCKS;
			return 0;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x0000809C File Offset: 0x0000629C
		internal int Sync()
		{
			if (this.mode != InflateManager.InflateManagerMode.BAD)
			{
				this.mode = InflateManager.InflateManagerMode.BAD;
				this.marker = 0;
			}
			int i;
			if ((i = this._codec.AvailableBytesIn) == 0)
			{
				return -5;
			}
			int p = this._codec.NextIn;
			int j = this.marker;
			while (i != 0 && j < 4)
			{
				if (this._codec.InputBuffer[p] == InflateManager.mark[j])
				{
					j++;
				}
				else if (this._codec.InputBuffer[p] != 0)
				{
					j = 0;
				}
				else
				{
					j = 4 - j;
				}
				p++;
				i--;
			}
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this._codec.AvailableBytesIn = i;
			this.marker = j;
			if (j != 4)
			{
				return -3;
			}
			long r = this._codec.TotalBytesIn;
			long w = this._codec.TotalBytesOut;
			this.Reset();
			this._codec.TotalBytesIn = r;
			this._codec.TotalBytesOut = w;
			this.mode = InflateManager.InflateManagerMode.BLOCKS;
			return 0;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000081B2 File Offset: 0x000063B2
		internal int SyncPoint(ZlibCodec z)
		{
			return this.blocks.SyncPoint();
		}

		// Token: 0x0400009F RID: 159
		private const int PRESET_DICT = 32;

		// Token: 0x040000A0 RID: 160
		private const int Z_DEFLATED = 8;

		// Token: 0x040000A1 RID: 161
		private InflateManager.InflateManagerMode mode;

		// Token: 0x040000A2 RID: 162
		internal ZlibCodec _codec;

		// Token: 0x040000A3 RID: 163
		internal int method;

		// Token: 0x040000A4 RID: 164
		internal uint computedCheck;

		// Token: 0x040000A5 RID: 165
		internal uint expectedCheck;

		// Token: 0x040000A6 RID: 166
		internal int marker;

		// Token: 0x040000A7 RID: 167
		private bool _handleRfc1950HeaderBytes = true;

		// Token: 0x040000A8 RID: 168
		internal int wbits;

		// Token: 0x040000A9 RID: 169
		internal InflateBlocks blocks;

		// Token: 0x040000AA RID: 170
		private static readonly byte[] mark = new byte[]
		{
			0,
			0,
			byte.MaxValue,
			byte.MaxValue
		};

		// Token: 0x020003C2 RID: 962
		private enum InflateManagerMode
		{
			// Token: 0x04002671 RID: 9841
			METHOD,
			// Token: 0x04002672 RID: 9842
			FLAG,
			// Token: 0x04002673 RID: 9843
			DICT4,
			// Token: 0x04002674 RID: 9844
			DICT3,
			// Token: 0x04002675 RID: 9845
			DICT2,
			// Token: 0x04002676 RID: 9846
			DICT1,
			// Token: 0x04002677 RID: 9847
			DICT0,
			// Token: 0x04002678 RID: 9848
			BLOCKS,
			// Token: 0x04002679 RID: 9849
			CHECK4,
			// Token: 0x0400267A RID: 9850
			CHECK3,
			// Token: 0x0400267B RID: 9851
			CHECK2,
			// Token: 0x0400267C RID: 9852
			CHECK1,
			// Token: 0x0400267D RID: 9853
			DONE,
			// Token: 0x0400267E RID: 9854
			BAD
		}
	}
}
