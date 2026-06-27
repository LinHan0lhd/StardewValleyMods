using System;

namespace Ionic.Zlib
{
	// Token: 0x0200000F RID: 15
	internal sealed class InflateBlocks
	{
		// Token: 0x0600006D RID: 109 RVA: 0x00005480 File Offset: 0x00003680
		internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
		{
			this._codec = codec;
			this.hufts = new int[4320];
			this.window = new byte[w];
			this.end = w;
			this.checkfn = checkfn;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.Reset();
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00005500 File Offset: 0x00003700
		internal uint Reset()
		{
			uint result = this.check;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.bitk = 0;
			this.bitb = 0;
			this.readAt = (this.writeAt = 0);
			if (this.checkfn != null)
			{
				this._codec._Adler32 = (this.check = Adler.Adler32(0U, null, 0, 0));
			}
			return result;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00005560 File Offset: 0x00003760
		internal int Process(int r)
		{
			int p = this._codec.NextIn;
			int i = this._codec.AvailableBytesIn;
			int b = this.bitb;
			int j = this.bitk;
			int q = this.writeAt;
			int k = (q < this.readAt) ? (this.readAt - q - 1) : (this.end - q);
			int t;
			for (;;)
			{
				switch (this.mode)
				{
				case InflateBlocks.InflateBlockMode.TYPE:
					while (j < 3)
					{
						if (i == 0)
						{
							goto IL_96;
						}
						r = 0;
						i--;
						b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					t = (b & 7);
					this.last = (t & 1);
					switch ((uint)t >> 1)
					{
					case 0U:
						b >>= 3;
						j -= 3;
						t = (j & 7);
						b >>= t;
						j -= t;
						this.mode = InflateBlocks.InflateBlockMode.LENS;
						continue;
					case 1U:
					{
						int[] bl = new int[1];
						int[] bd = new int[1];
						int[][] tl = new int[1][];
						int[][] td = new int[1][];
						InfTree.inflate_trees_fixed(bl, bd, tl, td, this._codec);
						this.codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
						b >>= 3;
						j -= 3;
						this.mode = InflateBlocks.InflateBlockMode.CODES;
						continue;
					}
					case 2U:
						b >>= 3;
						j -= 3;
						this.mode = InflateBlocks.InflateBlockMode.TABLE;
						continue;
					case 3U:
						goto IL_1E7;
					default:
						continue;
					}
					break;
				case InflateBlocks.InflateBlockMode.LENS:
					while (j < 32)
					{
						if (i == 0)
						{
							goto IL_26B;
						}
						r = 0;
						i--;
						b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					if ((~b >> 16 & 65535) != (b & 65535))
					{
						goto Block_8;
					}
					this.left = (b & 65535);
					j = (b = 0);
					this.mode = ((this.left != 0) ? InflateBlocks.InflateBlockMode.STORED : ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE));
					continue;
				case InflateBlocks.InflateBlockMode.STORED:
					if (i == 0)
					{
						goto Block_11;
					}
					if (k == 0)
					{
						if (q == this.end && this.readAt != 0)
						{
							q = 0;
							k = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
						}
						if (k == 0)
						{
							this.writeAt = q;
							r = this.Flush(r);
							q = this.writeAt;
							k = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
							if (q == this.end && this.readAt != 0)
							{
								q = 0;
								k = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
							}
							if (k == 0)
							{
								goto Block_21;
							}
						}
					}
					r = 0;
					t = this.left;
					if (t > i)
					{
						t = i;
					}
					if (t > k)
					{
						t = k;
					}
					Array.Copy(this._codec.InputBuffer, p, this.window, q, t);
					p += t;
					i -= t;
					q += t;
					k -= t;
					if ((this.left -= t) == 0)
					{
						this.mode = ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE);
						continue;
					}
					continue;
				case InflateBlocks.InflateBlockMode.TABLE:
					while (j < 14)
					{
						if (i == 0)
						{
							goto IL_59C;
						}
						r = 0;
						i--;
						b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					t = (this.table = (b & 16383));
					if ((t & 31) > 29 || (t >> 5 & 31) > 29)
					{
						goto IL_645;
					}
					t = 258 + (t & 31) + (t >> 5 & 31);
					if (this.blens == null || this.blens.Length < t)
					{
						this.blens = new int[t];
					}
					else
					{
						Array.Clear(this.blens, 0, t);
					}
					b >>= 14;
					j -= 14;
					this.index = 0;
					this.mode = InflateBlocks.InflateBlockMode.BTREE;
					goto IL_7D1;
				case InflateBlocks.InflateBlockMode.BTREE:
					goto IL_7D1;
				case InflateBlocks.InflateBlockMode.DTREE:
					goto IL_8C4;
				case InflateBlocks.InflateBlockMode.CODES:
					goto IL_CC9;
				case InflateBlocks.InflateBlockMode.DRY:
					goto IL_DA2;
				case InflateBlocks.InflateBlockMode.DONE:
					goto IL_E49;
				case InflateBlocks.InflateBlockMode.BAD:
					goto IL_EA3;
				}
				break;
				IL_7D1:
				while (this.index < 4 + (this.table >> 10))
				{
					while (j < 3)
					{
						if (i == 0)
						{
							goto IL_71E;
						}
						r = 0;
						i--;
						b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					int[] array = this.blens;
					int[] array2 = InflateBlocks.border;
					int num = this.index;
					this.index = num + 1;
					array[array2[num]] = (b & 7);
					b >>= 3;
					j -= 3;
				}
				while (this.index < 19)
				{
					int[] array3 = this.blens;
					int[] array4 = InflateBlocks.border;
					int num = this.index;
					this.index = num + 1;
					array3[array4[num]] = 0;
				}
				this.bb[0] = 7;
				t = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this._codec);
				if (t != 0)
				{
					goto Block_34;
				}
				this.index = 0;
				this.mode = InflateBlocks.InflateBlockMode.DTREE;
				for (;;)
				{
					IL_8C4:
					t = this.table;
					if (this.index >= 258 + (t & 31) + (t >> 5 & 31))
					{
						break;
					}
					t = this.bb[0];
					while (j < t)
					{
						if (i == 0)
						{
							goto IL_8FE;
						}
						r = 0;
						i--;
						b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					t = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 1];
					int c = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 2];
					if (c < 16)
					{
						b >>= t;
						j -= t;
						int[] array5 = this.blens;
						int num = this.index;
						this.index = num + 1;
						array5[num] = c;
					}
					else
					{
						int l = (c == 18) ? 7 : (c - 14);
						int m = (c == 18) ? 11 : 3;
						while (j < t + l)
						{
							if (i == 0)
							{
								goto IL_A20;
							}
							r = 0;
							i--;
							b |= (int)(this._codec.InputBuffer[p++] & byte.MaxValue) << j;
							j += 8;
						}
						b >>= t;
						j -= t;
						m += (b & InternalInflateConstants.InflateMask[l]);
						b >>= l;
						j -= l;
						l = this.index;
						t = this.table;
						if (l + m > 258 + (t & 31) + (t >> 5 & 31) || (c == 16 && l < 1))
						{
							goto IL_B03;
						}
						c = ((c == 16) ? this.blens[l - 1] : 0);
						do
						{
							this.blens[l++] = c;
						}
						while (--m != 0);
						this.index = l;
					}
				}
				this.tb[0] = -1;
				int[] bl2 = new int[]
				{
					9
				};
				int[] bd2 = new int[]
				{
					6
				};
				int[] tl2 = new int[1];
				int[] td2 = new int[1];
				t = this.table;
				t = this.inftree.inflate_trees_dynamic(257 + (t & 31), 1 + (t >> 5 & 31), this.blens, bl2, bd2, tl2, td2, this.hufts, this._codec);
				if (t != 0)
				{
					goto Block_48;
				}
				this.codes.Init(bl2[0], bd2[0], this.hufts, tl2[0], this.hufts, td2[0]);
				this.mode = InflateBlocks.InflateBlockMode.CODES;
				IL_CC9:
				this.bitb = b;
				this.bitk = j;
				this._codec.AvailableBytesIn = i;
				this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
				this._codec.NextIn = p;
				this.writeAt = q;
				r = this.codes.Process(this, r);
				if (r != 1)
				{
					goto Block_50;
				}
				r = 0;
				p = this._codec.NextIn;
				i = this._codec.AvailableBytesIn;
				b = this.bitb;
				j = this.bitk;
				q = this.writeAt;
				k = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
				if (this.last != 0)
				{
					goto IL_D9B;
				}
				this.mode = InflateBlocks.InflateBlockMode.TYPE;
			}
			r = -2;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_96:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_1E7:
			b >>= 3;
			j -= 3;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid block type";
			r = -3;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_26B:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_8:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid stored block lengths";
			r = -3;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_11:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_21:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_59C:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_645:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "too many length or distance symbols";
			r = -3;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_71E:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_34:
			r = t;
			if (r == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_8FE:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_A20:
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_B03:
			this.blens = null;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid bit length repeat";
			r = -3;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_48:
			if (t == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			r = t;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			Block_50:
			return this.Flush(r);
			IL_D9B:
			this.mode = InflateBlocks.InflateBlockMode.DRY;
			IL_DA2:
			this.writeAt = q;
			r = this.Flush(r);
			q = this.writeAt;
			int num2 = (q < this.readAt) ? (this.readAt - q - 1) : (this.end - q);
			if (this.readAt != this.writeAt)
			{
				this.bitb = b;
				this.bitk = j;
				this._codec.AvailableBytesIn = i;
				this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
				this._codec.NextIn = p;
				this.writeAt = q;
				return this.Flush(r);
			}
			this.mode = InflateBlocks.InflateBlockMode.DONE;
			IL_E49:
			r = 1;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
			IL_EA3:
			r = -3;
			this.bitb = b;
			this.bitk = j;
			this._codec.AvailableBytesIn = i;
			this._codec.TotalBytesIn += (long)(p - this._codec.NextIn);
			this._codec.NextIn = p;
			this.writeAt = q;
			return this.Flush(r);
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000064C5 File Offset: 0x000046C5
		internal void Free()
		{
			this.Reset();
			this.window = null;
			this.hufts = null;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000064DC File Offset: 0x000046DC
		internal void SetDictionary(byte[] d, int start, int n)
		{
			Array.Copy(d, start, this.window, 0, n);
			this.writeAt = n;
			this.readAt = n;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00006508 File Offset: 0x00004708
		internal int SyncPoint()
		{
			return (this.mode == InflateBlocks.InflateBlockMode.LENS) ? 1 : 0;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00006514 File Offset: 0x00004714
		internal int Flush(int r)
		{
			for (int pass = 0; pass < 2; pass++)
			{
				int nBytes;
				if (pass == 0)
				{
					nBytes = ((this.readAt <= this.writeAt) ? this.writeAt : this.end) - this.readAt;
				}
				else
				{
					nBytes = this.writeAt - this.readAt;
				}
				if (nBytes == 0)
				{
					if (r == -5)
					{
						r = 0;
					}
					return r;
				}
				if (nBytes > this._codec.AvailableBytesOut)
				{
					nBytes = this._codec.AvailableBytesOut;
				}
				if (nBytes != 0 && r == -5)
				{
					r = 0;
				}
				this._codec.AvailableBytesOut -= nBytes;
				this._codec.TotalBytesOut += (long)nBytes;
				if (this.checkfn != null)
				{
					this._codec._Adler32 = (this.check = Adler.Adler32(this.check, this.window, this.readAt, nBytes));
				}
				Array.Copy(this.window, this.readAt, this._codec.OutputBuffer, this._codec.NextOut, nBytes);
				this._codec.NextOut += nBytes;
				this.readAt += nBytes;
				if (this.readAt == this.end && pass == 0)
				{
					this.readAt = 0;
					if (this.writeAt == this.end)
					{
						this.writeAt = 0;
					}
				}
				else
				{
					pass++;
				}
			}
			return r;
		}

		// Token: 0x04000070 RID: 112
		private const int MANY = 1440;

		// Token: 0x04000071 RID: 113
		internal static readonly int[] border = new int[]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};

		// Token: 0x04000072 RID: 114
		private InflateBlocks.InflateBlockMode mode;

		// Token: 0x04000073 RID: 115
		internal int left;

		// Token: 0x04000074 RID: 116
		internal int table;

		// Token: 0x04000075 RID: 117
		internal int index;

		// Token: 0x04000076 RID: 118
		internal int[] blens;

		// Token: 0x04000077 RID: 119
		internal int[] bb = new int[1];

		// Token: 0x04000078 RID: 120
		internal int[] tb = new int[1];

		// Token: 0x04000079 RID: 121
		internal InflateCodes codes = new InflateCodes();

		// Token: 0x0400007A RID: 122
		internal int last;

		// Token: 0x0400007B RID: 123
		internal ZlibCodec _codec;

		// Token: 0x0400007C RID: 124
		internal int bitk;

		// Token: 0x0400007D RID: 125
		internal int bitb;

		// Token: 0x0400007E RID: 126
		internal int[] hufts;

		// Token: 0x0400007F RID: 127
		internal byte[] window;

		// Token: 0x04000080 RID: 128
		internal int end;

		// Token: 0x04000081 RID: 129
		internal int readAt;

		// Token: 0x04000082 RID: 130
		internal int writeAt;

		// Token: 0x04000083 RID: 131
		internal object checkfn;

		// Token: 0x04000084 RID: 132
		internal uint check;

		// Token: 0x04000085 RID: 133
		internal InfTree inftree = new InfTree();

		// Token: 0x020003C1 RID: 961
		private enum InflateBlockMode
		{
			// Token: 0x04002666 RID: 9830
			TYPE,
			// Token: 0x04002667 RID: 9831
			LENS,
			// Token: 0x04002668 RID: 9832
			STORED,
			// Token: 0x04002669 RID: 9833
			TABLE,
			// Token: 0x0400266A RID: 9834
			BTREE,
			// Token: 0x0400266B RID: 9835
			DTREE,
			// Token: 0x0400266C RID: 9836
			CODES,
			// Token: 0x0400266D RID: 9837
			DRY,
			// Token: 0x0400266E RID: 9838
			DONE,
			// Token: 0x0400266F RID: 9839
			BAD
		}
	}
}
