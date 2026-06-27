using System;

namespace Ionic.Zlib
{
	// Token: 0x02000011 RID: 17
	internal sealed class InflateCodes
	{
		// Token: 0x06000076 RID: 118 RVA: 0x000066A6 File Offset: 0x000048A6
		internal InflateCodes()
		{
		}

		// Token: 0x06000077 RID: 119 RVA: 0x000066AE File Offset: 0x000048AE
		internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
		{
			this.mode = 0;
			this.lbits = (byte)bl;
			this.dbits = (byte)bd;
			this.ltree = tl;
			this.ltree_index = tl_index;
			this.dtree = td;
			this.dtree_index = td_index;
			this.tree = null;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x000066F0 File Offset: 0x000048F0
		internal int Process(InflateBlocks blocks, int r)
		{
			ZlibCodec z = blocks._codec;
			int p = z.NextIn;
			int i = z.AvailableBytesIn;
			int b = blocks.bitb;
			int j = blocks.bitk;
			int q = blocks.writeAt;
			int k = (q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q);
			for (;;)
			{
				int l;
				switch (this.mode)
				{
				case 0:
					if (k >= 258 && i >= 10)
					{
						blocks.bitb = b;
						blocks.bitk = j;
						z.AvailableBytesIn = i;
						z.TotalBytesIn += (long)(p - z.NextIn);
						z.NextIn = p;
						blocks.writeAt = q;
						r = this.InflateFast((int)this.lbits, (int)this.dbits, this.ltree, this.ltree_index, this.dtree, this.dtree_index, blocks, z);
						p = z.NextIn;
						i = z.AvailableBytesIn;
						b = blocks.bitb;
						j = blocks.bitk;
						q = blocks.writeAt;
						k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
						if (r != 0)
						{
							this.mode = ((r == 1) ? 7 : 9);
							continue;
						}
					}
					this.need = (int)this.lbits;
					this.tree = this.ltree;
					this.tree_index = this.ltree_index;
					this.mode = 1;
					goto IL_1AA;
				case 1:
					goto IL_1AA;
				case 2:
					l = this.bitsToGet;
					while (j < l)
					{
						if (i == 0)
						{
							goto IL_36A;
						}
						r = 0;
						i--;
						b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					this.len += (b & InternalInflateConstants.InflateMask[l]);
					b >>= l;
					j -= l;
					this.need = (int)this.dbits;
					this.tree = this.dtree;
					this.tree_index = this.dtree_index;
					this.mode = 3;
					goto IL_434;
				case 3:
					goto IL_434;
				case 4:
					l = this.bitsToGet;
					while (j < l)
					{
						if (i == 0)
						{
							goto IL_5C3;
						}
						r = 0;
						i--;
						b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
						j += 8;
					}
					this.dist += (b & InternalInflateConstants.InflateMask[l]);
					b >>= l;
					j -= l;
					this.mode = 5;
					goto IL_669;
				case 5:
					goto IL_669;
				case 6:
					if (k == 0)
					{
						if (q == blocks.end && blocks.readAt != 0)
						{
							q = 0;
							k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
						}
						if (k == 0)
						{
							blocks.writeAt = q;
							r = blocks.Flush(r);
							q = blocks.writeAt;
							k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							if (q == blocks.end && blocks.readAt != 0)
							{
								q = 0;
								k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							}
							if (k == 0)
							{
								goto Block_44;
							}
						}
					}
					r = 0;
					blocks.window[q++] = (byte)this.lit;
					k--;
					this.mode = 0;
					continue;
				case 7:
					goto IL_913;
				case 8:
					goto IL_9C4;
				case 9:
					goto IL_A11;
				}
				break;
				IL_1AA:
				l = this.need;
				while (j < l)
				{
					if (i == 0)
					{
						goto IL_1BC;
					}
					r = 0;
					i--;
					b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
					j += 8;
				}
				int tindex = (this.tree_index + (b & InternalInflateConstants.InflateMask[l])) * 3;
				b >>= this.tree[tindex + 1];
				j -= this.tree[tindex + 1];
				int e = this.tree[tindex];
				if (e == 0)
				{
					this.lit = this.tree[tindex + 2];
					this.mode = 6;
					continue;
				}
				if ((e & 16) != 0)
				{
					this.bitsToGet = (e & 15);
					this.len = this.tree[tindex + 2];
					this.mode = 2;
					continue;
				}
				if ((e & 64) == 0)
				{
					this.need = e;
					this.tree_index = tindex / 3 + this.tree[tindex + 2];
					continue;
				}
				if ((e & 32) != 0)
				{
					this.mode = 7;
					continue;
				}
				goto IL_2F6;
				IL_434:
				l = this.need;
				while (j < l)
				{
					if (i == 0)
					{
						goto IL_446;
					}
					r = 0;
					i--;
					b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
					j += 8;
				}
				tindex = (this.tree_index + (b & InternalInflateConstants.InflateMask[l])) * 3;
				b >>= this.tree[tindex + 1];
				j -= this.tree[tindex + 1];
				e = this.tree[tindex];
				if ((e & 16) != 0)
				{
					this.bitsToGet = (e & 15);
					this.dist = this.tree[tindex + 2];
					this.mode = 4;
					continue;
				}
				if ((e & 64) == 0)
				{
					this.need = e;
					this.tree_index = tindex / 3 + this.tree[tindex + 2];
					continue;
				}
				goto IL_54F;
				IL_669:
				int f;
				for (f = q - this.dist; f < 0; f += blocks.end)
				{
				}
				while (this.len != 0)
				{
					if (k == 0)
					{
						if (q == blocks.end && blocks.readAt != 0)
						{
							q = 0;
							k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
						}
						if (k == 0)
						{
							blocks.writeAt = q;
							r = blocks.Flush(r);
							q = blocks.writeAt;
							k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							if (q == blocks.end && blocks.readAt != 0)
							{
								q = 0;
								k = ((q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q));
							}
							if (k == 0)
							{
								goto Block_32;
							}
						}
					}
					blocks.window[q++] = blocks.window[f++];
					k--;
					if (f == blocks.end)
					{
						f = 0;
					}
					this.len--;
				}
				this.mode = 0;
			}
			r = -2;
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_1BC:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_2F6:
			this.mode = 9;
			z.Message = "invalid literal/length code";
			r = -3;
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_36A:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_446:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_54F:
			this.mode = 9;
			z.Message = "invalid distance code";
			r = -3;
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_5C3:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			Block_32:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			Block_44:
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_913:
			if (j > 7)
			{
				j -= 8;
				i++;
				p--;
			}
			blocks.writeAt = q;
			r = blocks.Flush(r);
			q = blocks.writeAt;
			int num = (q < blocks.readAt) ? (blocks.readAt - q - 1) : (blocks.end - q);
			if (blocks.readAt != blocks.writeAt)
			{
				blocks.bitb = b;
				blocks.bitk = j;
				z.AvailableBytesIn = i;
				z.TotalBytesIn += (long)(p - z.NextIn);
				z.NextIn = p;
				blocks.writeAt = q;
				return blocks.Flush(r);
			}
			this.mode = 8;
			IL_9C4:
			r = 1;
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
			IL_A11:
			r = -3;
			blocks.bitb = b;
			blocks.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			blocks.writeAt = q;
			return blocks.Flush(r);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000071AC File Offset: 0x000053AC
		internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
		{
			int p = z.NextIn;
			int i = z.AvailableBytesIn;
			int b = s.bitb;
			int j = s.bitk;
			int q = s.writeAt;
			int k = (q < s.readAt) ? (s.readAt - q - 1) : (s.end - q);
			int ml = InternalInflateConstants.InflateMask[bl];
			int md = InternalInflateConstants.InflateMask[bd];
			int e;
			int c;
			for (;;)
			{
				if (j >= 20)
				{
					int t = b & ml;
					int tp_index_t_3 = (tl_index + t) * 3;
					if ((e = tl[tp_index_t_3]) == 0)
					{
						b >>= tl[tp_index_t_3 + 1];
						j -= tl[tp_index_t_3 + 1];
						s.window[q++] = (byte)tl[tp_index_t_3 + 2];
						k--;
					}
					else
					{
						for (;;)
						{
							b >>= tl[tp_index_t_3 + 1];
							j -= tl[tp_index_t_3 + 1];
							if ((e & 16) != 0)
							{
								break;
							}
							if ((e & 64) != 0)
							{
								goto IL_4B3;
							}
							t += tl[tp_index_t_3 + 2];
							t += (b & InternalInflateConstants.InflateMask[e]);
							tp_index_t_3 = (tl_index + t) * 3;
							if ((e = tl[tp_index_t_3]) == 0)
							{
								goto Block_20;
							}
						}
						e &= 15;
						c = tl[tp_index_t_3 + 2] + (b & InternalInflateConstants.InflateMask[e]);
						b >>= e;
						for (j -= e; j < 15; j += 8)
						{
							i--;
							b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
						}
						t = (b & md);
						tp_index_t_3 = (td_index + t) * 3;
						e = td[tp_index_t_3];
						for (;;)
						{
							b >>= td[tp_index_t_3 + 1];
							j -= td[tp_index_t_3 + 1];
							if ((e & 16) != 0)
							{
								break;
							}
							if ((e & 64) != 0)
							{
								goto IL_3C1;
							}
							t += td[tp_index_t_3 + 2];
							t += (b & InternalInflateConstants.InflateMask[e]);
							tp_index_t_3 = (td_index + t) * 3;
							e = td[tp_index_t_3];
						}
						e &= 15;
						while (j < e)
						{
							i--;
							b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
							j += 8;
						}
						int d = td[tp_index_t_3 + 2] + (b & InternalInflateConstants.InflateMask[e]);
						b >>= e;
						j -= e;
						k -= c;
						int r;
						if (q >= d)
						{
							r = q - d;
							if (q - r > 0 && 2 > q - r)
							{
								s.window[q++] = s.window[r++];
								s.window[q++] = s.window[r++];
								c -= 2;
							}
							else
							{
								Array.Copy(s.window, r, s.window, q, 2);
								q += 2;
								r += 2;
								c -= 2;
							}
						}
						else
						{
							r = q - d;
							do
							{
								r += s.end;
							}
							while (r < 0);
							e = s.end - r;
							if (c > e)
							{
								c -= e;
								if (q - r > 0 && e > q - r)
								{
									do
									{
										s.window[q++] = s.window[r++];
									}
									while (--e != 0);
								}
								else
								{
									Array.Copy(s.window, r, s.window, q, e);
									q += e;
									r += e;
								}
								r = 0;
							}
						}
						if (q - r > 0 && c > q - r)
						{
							do
							{
								s.window[q++] = s.window[r++];
							}
							while (--c != 0);
							goto IL_5C0;
						}
						Array.Copy(s.window, r, s.window, q, c);
						q += c;
						r += c;
						goto IL_5C0;
						Block_20:
						b >>= tl[tp_index_t_3 + 1];
						j -= tl[tp_index_t_3 + 1];
						s.window[q++] = (byte)tl[tp_index_t_3 + 2];
						k--;
					}
					IL_5C0:
					if (k < 258 || i < 10)
					{
						goto IL_5D2;
					}
				}
				else
				{
					i--;
					b |= (int)(z.InputBuffer[p++] & byte.MaxValue) << j;
					j += 8;
				}
			}
			IL_3C1:
			z.Message = "invalid distance code";
			c = z.AvailableBytesIn - i;
			c = ((j >> 3 < c) ? (j >> 3) : c);
			i += c;
			p -= c;
			j -= c << 3;
			s.bitb = b;
			s.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			s.writeAt = q;
			return -3;
			IL_4B3:
			if ((e & 32) != 0)
			{
				c = z.AvailableBytesIn - i;
				c = ((j >> 3 < c) ? (j >> 3) : c);
				i += c;
				p -= c;
				j -= c << 3;
				s.bitb = b;
				s.bitk = j;
				z.AvailableBytesIn = i;
				z.TotalBytesIn += (long)(p - z.NextIn);
				z.NextIn = p;
				s.writeAt = q;
				return 1;
			}
			z.Message = "invalid literal/length code";
			c = z.AvailableBytesIn - i;
			c = ((j >> 3 < c) ? (j >> 3) : c);
			i += c;
			p -= c;
			j -= c << 3;
			s.bitb = b;
			s.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			s.writeAt = q;
			return -3;
			IL_5D2:
			c = z.AvailableBytesIn - i;
			c = ((j >> 3 < c) ? (j >> 3) : c);
			i += c;
			p -= c;
			j -= c << 3;
			s.bitb = b;
			s.bitk = j;
			z.AvailableBytesIn = i;
			z.TotalBytesIn += (long)(p - z.NextIn);
			z.NextIn = p;
			s.writeAt = q;
			return 0;
		}

		// Token: 0x04000087 RID: 135
		private const int START = 0;

		// Token: 0x04000088 RID: 136
		private const int LEN = 1;

		// Token: 0x04000089 RID: 137
		private const int LENEXT = 2;

		// Token: 0x0400008A RID: 138
		private const int DIST = 3;

		// Token: 0x0400008B RID: 139
		private const int DISTEXT = 4;

		// Token: 0x0400008C RID: 140
		private const int COPY = 5;

		// Token: 0x0400008D RID: 141
		private const int LIT = 6;

		// Token: 0x0400008E RID: 142
		private const int WASH = 7;

		// Token: 0x0400008F RID: 143
		private const int END = 8;

		// Token: 0x04000090 RID: 144
		private const int BADCODE = 9;

		// Token: 0x04000091 RID: 145
		internal int mode;

		// Token: 0x04000092 RID: 146
		internal int len;

		// Token: 0x04000093 RID: 147
		internal int[] tree;

		// Token: 0x04000094 RID: 148
		internal int tree_index;

		// Token: 0x04000095 RID: 149
		internal int need;

		// Token: 0x04000096 RID: 150
		internal int lit;

		// Token: 0x04000097 RID: 151
		internal int bitsToGet;

		// Token: 0x04000098 RID: 152
		internal int dist;

		// Token: 0x04000099 RID: 153
		internal byte lbits;

		// Token: 0x0400009A RID: 154
		internal byte dbits;

		// Token: 0x0400009B RID: 155
		internal int[] ltree;

		// Token: 0x0400009C RID: 156
		internal int ltree_index;

		// Token: 0x0400009D RID: 157
		internal int[] dtree;

		// Token: 0x0400009E RID: 158
		internal int dtree_index;
	}
}
