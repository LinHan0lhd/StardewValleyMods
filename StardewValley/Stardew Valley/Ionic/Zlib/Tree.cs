using System;

namespace Ionic.Zlib
{
	// Token: 0x02000014 RID: 20
	internal sealed class Tree
	{
		// Token: 0x0600008D RID: 141 RVA: 0x0000890A File Offset: 0x00006B0A
		internal static int DistanceCode(int dist)
		{
			if (dist >= 256)
			{
				return (int)Tree._dist_code[256 + SharedUtils.URShift(dist, 7)];
			}
			return (int)Tree._dist_code[dist];
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00008930 File Offset: 0x00006B30
		internal void gen_bitlen(DeflateManager s)
		{
			short[] tree = this.dyn_tree;
			short[] stree = this.staticTree.treeCodes;
			int[] extra = this.staticTree.extraBits;
			int base_Renamed = this.staticTree.extraBase;
			int max_length = this.staticTree.maxLength;
			int overflow = 0;
			for (int bits = 0; bits <= InternalConstants.MAX_BITS; bits++)
			{
				s.bl_count[bits] = 0;
			}
			tree[s.heap[s.heap_max] * 2 + 1] = 0;
			int h;
			for (h = s.heap_max + 1; h < Tree.HEAP_SIZE; h++)
			{
				int i = s.heap[h];
				int bits = (int)(tree[(int)(tree[i * 2 + 1] * 2 + 1)] + 1);
				if (bits > max_length)
				{
					bits = max_length;
					overflow++;
				}
				tree[i * 2 + 1] = (short)bits;
				if (i <= this.max_code)
				{
					short[] bl_count = s.bl_count;
					int num = bits;
					bl_count[num] += 1;
					int xbits = 0;
					if (i >= base_Renamed)
					{
						xbits = extra[i - base_Renamed];
					}
					short f = tree[i * 2];
					s.opt_len += (int)f * (bits + xbits);
					if (stree != null)
					{
						s.static_len += (int)f * ((int)stree[i * 2 + 1] + xbits);
					}
				}
			}
			if (overflow == 0)
			{
				return;
			}
			do
			{
				int bits = max_length - 1;
				while (s.bl_count[bits] == 0)
				{
					bits--;
				}
				short[] bl_count2 = s.bl_count;
				int num2 = bits;
				bl_count2[num2] -= 1;
				s.bl_count[bits + 1] = s.bl_count[bits + 1] + 2;
				short[] bl_count3 = s.bl_count;
				int num3 = max_length;
				bl_count3[num3] -= 1;
				overflow -= 2;
			}
			while (overflow > 0);
			for (int bits = max_length; bits != 0; bits--)
			{
				int i = (int)s.bl_count[bits];
				while (i != 0)
				{
					int j = s.heap[--h];
					if (j <= this.max_code)
					{
						if ((int)tree[j * 2 + 1] != bits)
						{
							s.opt_len = (int)((long)s.opt_len + ((long)bits - (long)tree[j * 2 + 1]) * (long)tree[j * 2]);
							tree[j * 2 + 1] = (short)bits;
						}
						i--;
					}
				}
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00008B50 File Offset: 0x00006D50
		internal void build_tree(DeflateManager s)
		{
			short[] tree = this.dyn_tree;
			short[] stree = this.staticTree.treeCodes;
			int elems = this.staticTree.elems;
			int max_code = -1;
			s.heap_len = 0;
			s.heap_max = Tree.HEAP_SIZE;
			int num;
			for (int i = 0; i < elems; i++)
			{
				if (tree[i * 2] != 0)
				{
					int[] heap = s.heap;
					num = s.heap_len + 1;
					s.heap_len = num;
					max_code = (heap[num] = i);
					s.depth[i] = 0;
				}
				else
				{
					tree[i * 2 + 1] = 0;
				}
			}
			int node;
			while (s.heap_len < 2)
			{
				int[] heap2 = s.heap;
				num = s.heap_len + 1;
				s.heap_len = num;
				node = (heap2[num] = ((max_code < 2) ? (++max_code) : 0));
				tree[node * 2] = 1;
				s.depth[node] = 0;
				s.opt_len--;
				if (stree != null)
				{
					s.static_len -= (int)stree[node * 2 + 1];
				}
			}
			this.max_code = max_code;
			for (int i = s.heap_len / 2; i >= 1; i--)
			{
				s.pqdownheap(tree, i);
			}
			node = elems;
			do
			{
				int i = s.heap[1];
				int[] heap3 = s.heap;
				int num2 = 1;
				int[] heap4 = s.heap;
				num = s.heap_len;
				s.heap_len = num - 1;
				heap3[num2] = heap4[num];
				s.pqdownheap(tree, 1);
				int j = s.heap[1];
				int[] heap5 = s.heap;
				num = s.heap_max - 1;
				s.heap_max = num;
				heap5[num] = i;
				int[] heap6 = s.heap;
				num = s.heap_max - 1;
				s.heap_max = num;
				heap6[num] = j;
				tree[node * 2] = tree[i * 2] + tree[j * 2];
				s.depth[node] = (sbyte)(Math.Max((byte)s.depth[i], (byte)s.depth[j]) + 1);
				tree[i * 2 + 1] = (tree[j * 2 + 1] = (short)node);
				s.heap[1] = node++;
				s.pqdownheap(tree, 1);
			}
			while (s.heap_len >= 2);
			int[] heap7 = s.heap;
			num = s.heap_max - 1;
			s.heap_max = num;
			heap7[num] = s.heap[1];
			this.gen_bitlen(s);
			Tree.gen_codes(tree, max_code, s.bl_count);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00008D90 File Offset: 0x00006F90
		internal static void gen_codes(short[] tree, int max_code, short[] bl_count)
		{
			short[] next_code = new short[InternalConstants.MAX_BITS + 1];
			short code = 0;
			for (int bits = 1; bits <= InternalConstants.MAX_BITS; bits++)
			{
				code = (next_code[bits] = (short)(code + bl_count[bits - 1] << 1));
			}
			for (int i = 0; i <= max_code; i++)
			{
				int len = (int)tree[i * 2 + 1];
				if (len != 0)
				{
					int num = i * 2;
					short[] array = next_code;
					int num2 = len;
					short num3 = array[num2];
					array[num2] = num3 + 1;
					tree[num] = (short)Tree.bi_reverse((int)num3, len);
				}
			}
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00008E08 File Offset: 0x00007008
		internal static int bi_reverse(int code, int len)
		{
			int res = 0;
			do
			{
				res |= (code & 1);
				code >>= 1;
				res <<= 1;
			}
			while (--len > 0);
			return res >> 1;
		}

		// Token: 0x040000C4 RID: 196
		private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

		// Token: 0x040000C5 RID: 197
		internal static readonly int[] ExtraLengthBits = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			1,
			1,
			2,
			2,
			2,
			2,
			3,
			3,
			3,
			3,
			4,
			4,
			4,
			4,
			5,
			5,
			5,
			5,
			0
		};

		// Token: 0x040000C6 RID: 198
		internal static readonly int[] ExtraDistanceBits = new int[]
		{
			0,
			0,
			0,
			0,
			1,
			1,
			2,
			2,
			3,
			3,
			4,
			4,
			5,
			5,
			6,
			6,
			7,
			7,
			8,
			8,
			9,
			9,
			10,
			10,
			11,
			11,
			12,
			12,
			13,
			13
		};

		// Token: 0x040000C7 RID: 199
		internal static readonly int[] extra_blbits = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			3,
			7
		};

		// Token: 0x040000C8 RID: 200
		internal static readonly sbyte[] bl_order = new sbyte[]
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

		// Token: 0x040000C9 RID: 201
		internal const int Buf_size = 16;

		// Token: 0x040000CA RID: 202
		private static readonly sbyte[] _dist_code = new sbyte[]
		{
			0,
			1,
			2,
			3,
			4,
			4,
			5,
			5,
			6,
			6,
			6,
			6,
			7,
			7,
			7,
			7,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			9,
			9,
			9,
			9,
			9,
			9,
			9,
			9,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			11,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			12,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			14,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			15,
			0,
			0,
			16,
			17,
			18,
			18,
			19,
			19,
			20,
			20,
			20,
			20,
			21,
			21,
			21,
			21,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			28,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29,
			29
		};

		// Token: 0x040000CB RID: 203
		internal static readonly sbyte[] LengthCode = new sbyte[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			8,
			9,
			9,
			10,
			10,
			11,
			11,
			12,
			12,
			12,
			12,
			13,
			13,
			13,
			13,
			14,
			14,
			14,
			14,
			15,
			15,
			15,
			15,
			16,
			16,
			16,
			16,
			16,
			16,
			16,
			16,
			17,
			17,
			17,
			17,
			17,
			17,
			17,
			17,
			18,
			18,
			18,
			18,
			18,
			18,
			18,
			18,
			19,
			19,
			19,
			19,
			19,
			19,
			19,
			19,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			20,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			22,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			23,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			24,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			25,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			26,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			27,
			28
		};

		// Token: 0x040000CC RID: 204
		internal static readonly int[] LengthBase = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			10,
			12,
			14,
			16,
			20,
			24,
			28,
			32,
			40,
			48,
			56,
			64,
			80,
			96,
			112,
			128,
			160,
			192,
			224,
			0
		};

		// Token: 0x040000CD RID: 205
		internal static readonly int[] DistanceBase = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			6,
			8,
			12,
			16,
			24,
			32,
			48,
			64,
			96,
			128,
			192,
			256,
			384,
			512,
			768,
			1024,
			1536,
			2048,
			3072,
			4096,
			6144,
			8192,
			12288,
			16384,
			24576
		};

		// Token: 0x040000CE RID: 206
		internal short[] dyn_tree;

		// Token: 0x040000CF RID: 207
		internal int max_code;

		// Token: 0x040000D0 RID: 208
		internal StaticTree staticTree;
	}
}
