using System;

namespace Ionic.Zlib
{
	// Token: 0x0200000D RID: 13
	internal sealed class DeflateManager
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00002970 File Offset: 0x00000B70
		internal DeflateManager()
		{
			this.dyn_ltree = new short[DeflateManager.HEAP_SIZE * 2];
			this.dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2];
			this.bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2];
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002A24 File Offset: 0x00000C24
		private void _InitializeLazyMatch()
		{
			this.window_size = 2 * this.w_size;
			Array.Clear(this.head, 0, this.hash_size);
			this.config = DeflateManager.Config.Lookup(this.compressionLevel);
			this.SetDeflater();
			this.strstart = 0;
			this.block_start = 0;
			this.lookahead = 0;
			this.match_length = (this.prev_length = DeflateManager.MIN_MATCH - 1);
			this.match_available = 0;
			this.ins_h = 0;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002AA4 File Offset: 0x00000CA4
		private void _InitializeTreeData()
		{
			this.treeLiterals.dyn_tree = this.dyn_ltree;
			this.treeLiterals.staticTree = StaticTree.Literals;
			this.treeDistances.dyn_tree = this.dyn_dtree;
			this.treeDistances.staticTree = StaticTree.Distances;
			this.treeBitLengths.dyn_tree = this.bl_tree;
			this.treeBitLengths.staticTree = StaticTree.BitLengths;
			this.bi_buf = 0;
			this.bi_valid = 0;
			this.last_eob_len = 8;
			this._InitializeBlocks();
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002B30 File Offset: 0x00000D30
		internal void _InitializeBlocks()
		{
			for (int i = 0; i < InternalConstants.L_CODES; i++)
			{
				this.dyn_ltree[i * 2] = 0;
			}
			for (int j = 0; j < InternalConstants.D_CODES; j++)
			{
				this.dyn_dtree[j * 2] = 0;
			}
			for (int k = 0; k < InternalConstants.BL_CODES; k++)
			{
				this.bl_tree[k * 2] = 0;
			}
			this.dyn_ltree[DeflateManager.END_BLOCK * 2] = 1;
			this.opt_len = (this.static_len = 0);
			this.last_lit = (this.matches = 0);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002BC0 File Offset: 0x00000DC0
		internal void pqdownheap(short[] tree, int k)
		{
			int v = this.heap[k];
			for (int i = k << 1; i <= this.heap_len; i <<= 1)
			{
				if (i < this.heap_len && DeflateManager._IsSmaller(tree, this.heap[i + 1], this.heap[i], this.depth))
				{
					i++;
				}
				if (DeflateManager._IsSmaller(tree, v, this.heap[i], this.depth))
				{
					break;
				}
				this.heap[k] = this.heap[i];
				k = i;
			}
			this.heap[k] = v;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002C4C File Offset: 0x00000E4C
		internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
		{
			short tn2 = tree[n * 2];
			short tm2 = tree[m * 2];
			return tn2 < tm2 || (tn2 == tm2 && depth[n] <= depth[m]);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002C7C File Offset: 0x00000E7C
		internal void scan_tree(short[] tree, int max_code)
		{
			int prevlen = -1;
			int nextlen = (int)tree[1];
			int count = 0;
			int max_count = 7;
			int min_count = 4;
			if (nextlen == 0)
			{
				max_count = 138;
				min_count = 3;
			}
			tree[(max_code + 1) * 2 + 1] = short.MaxValue;
			for (int i = 0; i <= max_code; i++)
			{
				int curlen = nextlen;
				nextlen = (int)tree[(i + 1) * 2 + 1];
				if (++count >= max_count || curlen != nextlen)
				{
					if (count < min_count)
					{
						this.bl_tree[curlen * 2] = (short)((int)this.bl_tree[curlen * 2] + count);
					}
					else if (curlen != 0)
					{
						if (curlen != prevlen)
						{
							short[] array = this.bl_tree;
							int num = curlen * 2;
							array[num] += 1;
						}
						short[] array2 = this.bl_tree;
						int num2 = InternalConstants.REP_3_6 * 2;
						array2[num2] += 1;
					}
					else if (count <= 10)
					{
						short[] array3 = this.bl_tree;
						int num3 = InternalConstants.REPZ_3_10 * 2;
						array3[num3] += 1;
					}
					else
					{
						short[] array4 = this.bl_tree;
						int num4 = InternalConstants.REPZ_11_138 * 2;
						array4[num4] += 1;
					}
					count = 0;
					prevlen = curlen;
					if (nextlen == 0)
					{
						max_count = 138;
						min_count = 3;
					}
					else if (curlen == nextlen)
					{
						max_count = 6;
						min_count = 3;
					}
					else
					{
						max_count = 7;
						min_count = 4;
					}
				}
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002D98 File Offset: 0x00000F98
		internal int build_bl_tree()
		{
			this.scan_tree(this.dyn_ltree, this.treeLiterals.max_code);
			this.scan_tree(this.dyn_dtree, this.treeDistances.max_code);
			this.treeBitLengths.build_tree(this);
			int max_blindex = InternalConstants.BL_CODES - 1;
			while (max_blindex >= 3 && this.bl_tree[(int)(Tree.bl_order[max_blindex] * 2 + 1)] == 0)
			{
				max_blindex--;
			}
			this.opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;
			return max_blindex;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002E20 File Offset: 0x00001020
		internal void send_all_trees(int lcodes, int dcodes, int blcodes)
		{
			this.send_bits(lcodes - 257, 5);
			this.send_bits(dcodes - 1, 5);
			this.send_bits(blcodes - 4, 4);
			for (int rank = 0; rank < blcodes; rank++)
			{
				this.send_bits((int)this.bl_tree[(int)(Tree.bl_order[rank] * 2 + 1)], 3);
			}
			this.send_tree(this.dyn_ltree, lcodes - 1);
			this.send_tree(this.dyn_dtree, dcodes - 1);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002E94 File Offset: 0x00001094
		internal void send_tree(short[] tree, int max_code)
		{
			int prevlen = -1;
			int nextlen = (int)tree[1];
			int count = 0;
			int max_count = 7;
			int min_count = 4;
			if (nextlen == 0)
			{
				max_count = 138;
				min_count = 3;
			}
			for (int i = 0; i <= max_code; i++)
			{
				int curlen = nextlen;
				nextlen = (int)tree[(i + 1) * 2 + 1];
				if (++count >= max_count || curlen != nextlen)
				{
					if (count < min_count)
					{
						do
						{
							this.send_code(curlen, this.bl_tree);
						}
						while (--count != 0);
					}
					else if (curlen != 0)
					{
						if (curlen != prevlen)
						{
							this.send_code(curlen, this.bl_tree);
							count--;
						}
						this.send_code(InternalConstants.REP_3_6, this.bl_tree);
						this.send_bits(count - 3, 2);
					}
					else if (count <= 10)
					{
						this.send_code(InternalConstants.REPZ_3_10, this.bl_tree);
						this.send_bits(count - 3, 3);
					}
					else
					{
						this.send_code(InternalConstants.REPZ_11_138, this.bl_tree);
						this.send_bits(count - 11, 7);
					}
					count = 0;
					prevlen = curlen;
					if (nextlen == 0)
					{
						max_count = 138;
						min_count = 3;
					}
					else if (curlen == nextlen)
					{
						max_count = 6;
						min_count = 3;
					}
					else
					{
						max_count = 7;
						min_count = 4;
					}
				}
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002FAA File Offset: 0x000011AA
		private void put_bytes(byte[] p, int start, int len)
		{
			Array.Copy(p, start, this.pending, this.pendingCount, len);
			this.pendingCount += len;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002FD0 File Offset: 0x000011D0
		internal void send_code(int c, short[] tree)
		{
			int c2 = c * 2;
			this.send_bits((int)tree[c2] & 65535, (int)tree[c2 + 1] & 65535);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002FFC File Offset: 0x000011FC
		internal void send_bits(int value, int length)
		{
			if (this.bi_valid > DeflateManager.Buf_size - length)
			{
				this.bi_buf |= (short)(value << this.bi_valid & 65535);
				byte[] array = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array[num] = (byte)this.bi_buf;
				byte[] array2 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array2[num] = (byte)(this.bi_buf >> 8);
				this.bi_buf = (short)((uint)value >> DeflateManager.Buf_size - this.bi_valid);
				this.bi_valid += length - DeflateManager.Buf_size;
				return;
			}
			this.bi_buf |= (short)(value << this.bi_valid & 65535);
			this.bi_valid += length;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000030D8 File Offset: 0x000012D8
		internal void _tr_align()
		{
			this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
			this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
			this.bi_flush();
			if (1 + this.last_eob_len + 10 - this.bi_valid < 9)
			{
				this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
				this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
				this.bi_flush();
			}
			this.last_eob_len = 7;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x0000314C File Offset: 0x0000134C
		internal bool _tr_tally(int dist, int lc)
		{
			this.pending[this._distanceOffset + this.last_lit * 2] = (byte)((uint)dist >> 8);
			this.pending[this._distanceOffset + this.last_lit * 2 + 1] = (byte)dist;
			this.pending[this._lengthOffset + this.last_lit] = (byte)lc;
			this.last_lit++;
			if (dist == 0)
			{
				short[] array = this.dyn_ltree;
				int num = lc * 2;
				array[num] += 1;
			}
			else
			{
				this.matches++;
				dist--;
				short[] array2 = this.dyn_ltree;
				int num2 = ((int)Tree.LengthCode[lc] + InternalConstants.LITERALS + 1) * 2;
				array2[num2] += 1;
				short[] array3 = this.dyn_dtree;
				int num3 = Tree.DistanceCode(dist) * 2;
				array3[num3] += 1;
			}
			if ((this.last_lit & 8191) == 0 && this.compressionLevel > CompressionLevel.Level2)
			{
				int out_length = this.last_lit << 3;
				int in_length = this.strstart - this.block_start;
				for (int dcode = 0; dcode < InternalConstants.D_CODES; dcode++)
				{
					out_length = (int)((long)out_length + (long)this.dyn_dtree[dcode * 2] * (5L + (long)Tree.ExtraDistanceBits[dcode]));
				}
				out_length >>= 3;
				if (this.matches < this.last_lit / 2 && out_length < in_length / 2)
				{
					return true;
				}
			}
			return this.last_lit == this.lit_bufsize - 1 || this.last_lit == this.lit_bufsize;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000032B0 File Offset: 0x000014B0
		internal void send_compressed_block(short[] ltree, short[] dtree)
		{
			int lx = 0;
			if (this.last_lit != 0)
			{
				do
				{
					int ix = this._distanceOffset + lx * 2;
					int distance = ((int)this.pending[ix] << 8 & 65280) | (int)(this.pending[ix + 1] & byte.MaxValue);
					int lc = (int)(this.pending[this._lengthOffset + lx] & byte.MaxValue);
					lx++;
					if (distance == 0)
					{
						this.send_code(lc, ltree);
					}
					else
					{
						int code = (int)Tree.LengthCode[lc];
						this.send_code(code + InternalConstants.LITERALS + 1, ltree);
						int extra = Tree.ExtraLengthBits[code];
						if (extra != 0)
						{
							lc -= Tree.LengthBase[code];
							this.send_bits(lc, extra);
						}
						distance--;
						code = Tree.DistanceCode(distance);
						this.send_code(code, dtree);
						extra = Tree.ExtraDistanceBits[code];
						if (extra != 0)
						{
							distance -= Tree.DistanceBase[code];
							this.send_bits(distance, extra);
						}
					}
				}
				while (lx < this.last_lit);
			}
			this.send_code(DeflateManager.END_BLOCK, ltree);
			this.last_eob_len = (int)ltree[DeflateManager.END_BLOCK * 2 + 1];
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000033B8 File Offset: 0x000015B8
		internal void set_data_type()
		{
			int i = 0;
			int ascii_freq = 0;
			int bin_freq = 0;
			while (i < 7)
			{
				bin_freq += (int)this.dyn_ltree[i * 2];
				i++;
			}
			while (i < 128)
			{
				ascii_freq += (int)this.dyn_ltree[i * 2];
				i++;
			}
			while (i < InternalConstants.LITERALS)
			{
				bin_freq += (int)this.dyn_ltree[i * 2];
				i++;
			}
			this.data_type = (sbyte)((bin_freq > ascii_freq >> 2) ? DeflateManager.Z_BINARY : DeflateManager.Z_ASCII);
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003434 File Offset: 0x00001634
		internal void bi_flush()
		{
			if (this.bi_valid == 16)
			{
				byte[] array = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array[num] = (byte)this.bi_buf;
				byte[] array2 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array2[num] = (byte)(this.bi_buf >> 8);
				this.bi_buf = 0;
				this.bi_valid = 0;
				return;
			}
			if (this.bi_valid >= 8)
			{
				byte[] array3 = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array3[num] = (byte)this.bi_buf;
				this.bi_buf = (short)(this.bi_buf >> 8);
				this.bi_valid -= 8;
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000034E0 File Offset: 0x000016E0
		internal void bi_windup()
		{
			if (this.bi_valid > 8)
			{
				byte[] array = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array[num] = (byte)this.bi_buf;
				byte[] array2 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array2[num] = (byte)(this.bi_buf >> 8);
			}
			else if (this.bi_valid > 0)
			{
				byte[] array3 = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array3[num] = (byte)this.bi_buf;
			}
			this.bi_buf = 0;
			this.bi_valid = 0;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003570 File Offset: 0x00001770
		internal void copy_block(int buf, int len, bool header)
		{
			this.bi_windup();
			this.last_eob_len = 8;
			if (header)
			{
				byte[] array = this.pending;
				int num = this.pendingCount;
				this.pendingCount = num + 1;
				array[num] = (byte)len;
				byte[] array2 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array2[num] = (byte)(len >> 8);
				byte[] array3 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array3[num] = (byte)(~(byte)len);
				byte[] array4 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array4[num] = (byte)(~len >> 8);
			}
			this.put_bytes(this.window, buf, len);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00003609 File Offset: 0x00001809
		internal void flush_block_only(bool eof)
		{
			this._tr_flush_block((this.block_start >= 0) ? this.block_start : -1, this.strstart - this.block_start, eof);
			this.block_start = this.strstart;
			this._codec.flush_pending();
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00003648 File Offset: 0x00001848
		internal BlockState DeflateNone(FlushType flush)
		{
			int max_block_size = 65535;
			if (max_block_size > this.pending.Length - 5)
			{
				max_block_size = this.pending.Length - 5;
			}
			for (;;)
			{
				if (this.lookahead <= 1)
				{
					this._fillWindow();
					if (this.lookahead == 0 && flush == FlushType.None)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto IL_DB;
					}
				}
				this.strstart += this.lookahead;
				this.lookahead = 0;
				int max_start = this.block_start + max_block_size;
				if (this.strstart == 0 || this.strstart >= max_start)
				{
					this.lookahead = this.strstart - max_start;
					this.strstart = max_start;
					this.flush_block_only(false);
					if (this._codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
				if (this.strstart - this.block_start >= this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					this.flush_block_only(false);
					if (this._codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
			}
			return BlockState.NeedMore;
			IL_DB:
			this.flush_block_only(flush == FlushType.Finish);
			if (this._codec.AvailableBytesOut == 0)
			{
				if (flush != FlushType.Finish)
				{
					return BlockState.NeedMore;
				}
				return BlockState.FinishStarted;
			}
			else
			{
				if (flush != FlushType.Finish)
				{
					return BlockState.BlockDone;
				}
				return BlockState.FinishDone;
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003756 File Offset: 0x00001956
		internal void _tr_stored_block(int buf, int stored_len, bool eof)
		{
			this.send_bits((DeflateManager.STORED_BLOCK << 1) + ((eof > false) ? 1 : 0), 3);
			this.copy_block(buf, stored_len, true);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003774 File Offset: 0x00001974
		internal void _tr_flush_block(int buf, int stored_len, bool eof)
		{
			int max_blindex = 0;
			int opt_lenb;
			int static_lenb;
			if (this.compressionLevel > CompressionLevel.None)
			{
				if ((int)this.data_type == DeflateManager.Z_UNKNOWN)
				{
					this.set_data_type();
				}
				this.treeLiterals.build_tree(this);
				this.treeDistances.build_tree(this);
				max_blindex = this.build_bl_tree();
				opt_lenb = this.opt_len + 3 + 7 >> 3;
				static_lenb = this.static_len + 3 + 7 >> 3;
				if (static_lenb <= opt_lenb)
				{
					opt_lenb = static_lenb;
				}
			}
			else
			{
				static_lenb = (opt_lenb = stored_len + 5);
			}
			if (stored_len + 4 <= opt_lenb && buf != -1)
			{
				this._tr_stored_block(buf, stored_len, eof);
			}
			else if (static_lenb == opt_lenb)
			{
				this.send_bits((DeflateManager.STATIC_TREES << 1) + ((eof > false) ? 1 : 0), 3);
				this.send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
			}
			else
			{
				this.send_bits((DeflateManager.DYN_TREES << 1) + ((eof > false) ? 1 : 0), 3);
				this.send_all_trees(this.treeLiterals.max_code + 1, this.treeDistances.max_code + 1, max_blindex + 1);
				this.send_compressed_block(this.dyn_ltree, this.dyn_dtree);
			}
			this._InitializeBlocks();
			if (eof)
			{
				this.bi_windup();
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000387C File Offset: 0x00001A7C
		private void _fillWindow()
		{
			for (;;)
			{
				int more = this.window_size - this.lookahead - this.strstart;
				int i;
				if (more == 0 && this.strstart == 0 && this.lookahead == 0)
				{
					more = this.w_size;
				}
				else if (more == -1)
				{
					more--;
				}
				else if (this.strstart >= this.w_size + this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					Array.Copy(this.window, this.w_size, this.window, 0, this.w_size);
					this.match_start -= this.w_size;
					this.strstart -= this.w_size;
					this.block_start -= this.w_size;
					i = this.hash_size;
					int p = i;
					do
					{
						int j = (int)this.head[--p] & 65535;
						this.head[p] = (short)((j >= this.w_size) ? (j - this.w_size) : 0);
					}
					while (--i != 0);
					i = this.w_size;
					p = i;
					do
					{
						int j = (int)this.prev[--p] & 65535;
						this.prev[p] = (short)((j >= this.w_size) ? (j - this.w_size) : 0);
					}
					while (--i != 0);
					more += this.w_size;
				}
				if (this._codec.AvailableBytesIn == 0)
				{
					break;
				}
				i = this._codec.read_buf(this.window, this.strstart + this.lookahead, more);
				this.lookahead += i;
				if (this.lookahead >= DeflateManager.MIN_MATCH)
				{
					this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask);
				}
				if (this.lookahead >= DeflateManager.MIN_LOOKAHEAD || this._codec.AvailableBytesIn == 0)
				{
					return;
				}
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003A7C File Offset: 0x00001C7C
		internal BlockState DeflateFast(FlushType flush)
		{
			int hash_head = 0;
			for (;;)
			{
				if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
				{
					this._fillWindow();
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto IL_2E8;
					}
				}
				if (this.lookahead >= DeflateManager.MIN_MATCH)
				{
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask);
					hash_head = ((int)this.head[this.ins_h] & 65535);
					this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
					this.head[this.ins_h] = (short)this.strstart;
				}
				if ((long)hash_head != 0L && (this.strstart - hash_head & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD && this.compressionStrategy != CompressionStrategy.HuffmanOnly)
				{
					this.match_length = this.longest_match(hash_head);
				}
				bool bflush;
				if (this.match_length >= DeflateManager.MIN_MATCH)
				{
					bflush = this._tr_tally(this.strstart - this.match_start, this.match_length - DeflateManager.MIN_MATCH);
					this.lookahead -= this.match_length;
					if (this.match_length <= this.config.MaxLazy && this.lookahead >= DeflateManager.MIN_MATCH)
					{
						this.match_length--;
						int num;
						do
						{
							this.strstart++;
							this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask);
							hash_head = ((int)this.head[this.ins_h] & 65535);
							this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
							this.head[this.ins_h] = (short)this.strstart;
							num = this.match_length - 1;
							this.match_length = num;
						}
						while (num != 0);
						this.strstart++;
					}
					else
					{
						this.strstart += this.match_length;
						this.match_length = 0;
						this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
						this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask);
					}
				}
				else
				{
					bflush = this._tr_tally(0, (int)(this.window[this.strstart] & byte.MaxValue));
					this.lookahead--;
					this.strstart++;
				}
				if (bflush)
				{
					this.flush_block_only(false);
					if (this._codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
			}
			return BlockState.NeedMore;
			IL_2E8:
			this.flush_block_only(flush == FlushType.Finish);
			if (this._codec.AvailableBytesOut == 0)
			{
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			}
			else
			{
				if (flush != FlushType.Finish)
				{
					return BlockState.BlockDone;
				}
				return BlockState.FinishDone;
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003D98 File Offset: 0x00001F98
		internal BlockState DeflateSlow(FlushType flush)
		{
			int hash_head = 0;
			for (;;)
			{
				if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
				{
					this._fillWindow();
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto IL_363;
					}
				}
				if (this.lookahead >= DeflateManager.MIN_MATCH)
				{
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask);
					hash_head = ((int)this.head[this.ins_h] & 65535);
					this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
					this.head[this.ins_h] = (short)this.strstart;
				}
				this.prev_length = this.match_length;
				this.prev_match = this.match_start;
				this.match_length = DeflateManager.MIN_MATCH - 1;
				if (hash_head != 0 && this.prev_length < this.config.MaxLazy && (this.strstart - hash_head & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					if (this.compressionStrategy != CompressionStrategy.HuffmanOnly)
					{
						this.match_length = this.longest_match(hash_head);
					}
					if (this.match_length <= 5 && (this.compressionStrategy == CompressionStrategy.Filtered || (this.match_length == DeflateManager.MIN_MATCH && this.strstart - this.match_start > 4096)))
					{
						this.match_length = DeflateManager.MIN_MATCH - 1;
					}
				}
				if (this.prev_length >= DeflateManager.MIN_MATCH && this.match_length <= this.prev_length)
				{
					int max_insert = this.strstart + this.lookahead - DeflateManager.MIN_MATCH;
					bool bflush = this._tr_tally(this.strstart - 1 - this.prev_match, this.prev_length - DeflateManager.MIN_MATCH);
					this.lookahead -= this.prev_length - 1;
					this.prev_length -= 2;
					int num;
					do
					{
						num = this.strstart + 1;
						this.strstart = num;
						if (num <= max_insert)
						{
							this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask);
							hash_head = ((int)this.head[this.ins_h] & 65535);
							this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
							this.head[this.ins_h] = (short)this.strstart;
						}
						num = this.prev_length - 1;
						this.prev_length = num;
					}
					while (num != 0);
					this.match_available = 0;
					this.match_length = DeflateManager.MIN_MATCH - 1;
					this.strstart++;
					if (bflush)
					{
						this.flush_block_only(false);
						if (this._codec.AvailableBytesOut == 0)
						{
							return BlockState.NeedMore;
						}
					}
				}
				else if (this.match_available != 0)
				{
					bool bflush = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
					if (bflush)
					{
						this.flush_block_only(false);
					}
					this.strstart++;
					this.lookahead--;
					if (this._codec.AvailableBytesOut == 0)
					{
						return BlockState.NeedMore;
					}
				}
				else
				{
					this.match_available = 1;
					this.strstart++;
					this.lookahead--;
				}
			}
			return BlockState.NeedMore;
			IL_363:
			if (this.match_available != 0)
			{
				bool bflush = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
				this.match_available = 0;
			}
			this.flush_block_only(flush == FlushType.Finish);
			if (this._codec.AvailableBytesOut == 0)
			{
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			}
			else
			{
				if (flush != FlushType.Finish)
				{
					return BlockState.BlockDone;
				}
				return BlockState.FinishDone;
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000415C File Offset: 0x0000235C
		internal int longest_match(int cur_match)
		{
			int chain_length = this.config.MaxChainLength;
			int scan = this.strstart;
			int best_len = this.prev_length;
			int limit = (this.strstart > this.w_size - DeflateManager.MIN_LOOKAHEAD) ? (this.strstart - (this.w_size - DeflateManager.MIN_LOOKAHEAD)) : 0;
			int niceLength = this.config.NiceLength;
			int wmask = this.w_mask;
			int strend = this.strstart + DeflateManager.MAX_MATCH;
			byte scan_end = this.window[scan + best_len - 1];
			byte scan_end2 = this.window[scan + best_len];
			if (this.prev_length >= this.config.GoodLength)
			{
				chain_length >>= 2;
			}
			if (niceLength > this.lookahead)
			{
				niceLength = this.lookahead;
			}
			do
			{
				int match = cur_match;
				if (this.window[match + best_len] == scan_end2 && this.window[match + best_len - 1] == scan_end && this.window[match] == this.window[scan] && this.window[++match] == this.window[scan + 1])
				{
					scan += 2;
					match++;
					while (this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && scan < strend)
					{
					}
					int len = DeflateManager.MAX_MATCH - (strend - scan);
					scan = strend - DeflateManager.MAX_MATCH;
					if (len > best_len)
					{
						this.match_start = cur_match;
						best_len = len;
						if (len >= niceLength)
						{
							break;
						}
						scan_end = this.window[scan + best_len - 1];
						scan_end2 = this.window[scan + best_len];
					}
				}
			}
			while ((cur_match = ((int)this.prev[cur_match & wmask] & 65535)) > limit && --chain_length != 0);
			if (best_len <= this.lookahead)
			{
				return best_len;
			}
			return this.lookahead;
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600003F RID: 63 RVA: 0x000043D2 File Offset: 0x000025D2
		// (set) Token: 0x06000040 RID: 64 RVA: 0x000043DA File Offset: 0x000025DA
		internal bool WantRfc1950HeaderBytes
		{
			get
			{
				return this._WantRfc1950HeaderBytes;
			}
			set
			{
				this._WantRfc1950HeaderBytes = value;
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000043E3 File Offset: 0x000025E3
		internal int Initialize(ZlibCodec codec, CompressionLevel level)
		{
			return this.Initialize(codec, level, 15);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000043EF File Offset: 0x000025EF
		internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits)
		{
			return this.Initialize(codec, level, bits, DeflateManager.MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00004400 File Offset: 0x00002600
		internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
		{
			return this.Initialize(codec, level, bits, DeflateManager.MEM_LEVEL_DEFAULT, compressionStrategy);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004414 File Offset: 0x00002614
		internal int Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
		{
			this._codec = codec;
			this._codec.Message = null;
			if (windowBits < 9 || windowBits > 15)
			{
				throw new ZlibException("windowBits must be in the range 9..15.");
			}
			if (memLevel < 1 || memLevel > DeflateManager.MEM_LEVEL_MAX)
			{
				throw new ZlibException(string.Format("memLevel must be in the range 1.. {0}", DeflateManager.MEM_LEVEL_MAX));
			}
			this._codec.dstate = this;
			this.w_bits = windowBits;
			this.w_size = 1 << this.w_bits;
			this.w_mask = this.w_size - 1;
			this.hash_bits = memLevel + 7;
			this.hash_size = 1 << this.hash_bits;
			this.hash_mask = this.hash_size - 1;
			this.hash_shift = (this.hash_bits + DeflateManager.MIN_MATCH - 1) / DeflateManager.MIN_MATCH;
			this.window = new byte[this.w_size * 2];
			this.prev = new short[this.w_size];
			this.head = new short[this.hash_size];
			this.lit_bufsize = 1 << memLevel + 6;
			this.pending = new byte[this.lit_bufsize * 4];
			this._distanceOffset = this.lit_bufsize;
			this._lengthOffset = 3 * this.lit_bufsize;
			this.compressionLevel = level;
			this.compressionStrategy = strategy;
			this.Reset();
			return 0;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004570 File Offset: 0x00002770
		internal void Reset()
		{
			this._codec.TotalBytesIn = (this._codec.TotalBytesOut = 0L);
			this._codec.Message = null;
			this.pendingCount = 0;
			this.nextPending = 0;
			this.Rfc1950BytesEmitted = false;
			this.status = (this.WantRfc1950HeaderBytes ? DeflateManager.INIT_STATE : DeflateManager.BUSY_STATE);
			this._codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
			this.last_flush = 0;
			this._InitializeTreeData();
			this._InitializeLazyMatch();
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000045FC File Offset: 0x000027FC
		internal int End()
		{
			if (this.status != DeflateManager.INIT_STATE && this.status != DeflateManager.BUSY_STATE && this.status != DeflateManager.FINISH_STATE)
			{
				return -2;
			}
			this.pending = null;
			this.head = null;
			this.prev = null;
			this.window = null;
			if (this.status != DeflateManager.BUSY_STATE)
			{
				return 0;
			}
			return -3;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00004660 File Offset: 0x00002860
		private void SetDeflater()
		{
			switch (this.config.Flavor)
			{
			case DeflateFlavor.Store:
				this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateNone);
				return;
			case DeflateFlavor.Fast:
				this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateFast);
				return;
			case DeflateFlavor.Slow:
				this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateSlow);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000046C4 File Offset: 0x000028C4
		internal int SetParams(CompressionLevel level, CompressionStrategy strategy)
		{
			int result = 0;
			if (this.compressionLevel != level)
			{
				DeflateManager.Config newConfig = DeflateManager.Config.Lookup(level);
				if (newConfig.Flavor != this.config.Flavor && this._codec.TotalBytesIn != 0L)
				{
					result = this._codec.Deflate(FlushType.Partial);
				}
				this.compressionLevel = level;
				this.config = newConfig;
				this.SetDeflater();
			}
			this.compressionStrategy = strategy;
			return result;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x0000472C File Offset: 0x0000292C
		internal int SetDictionary(byte[] dictionary)
		{
			int length = dictionary.Length;
			int index = 0;
			if (dictionary == null || this.status != DeflateManager.INIT_STATE)
			{
				throw new ZlibException("Stream error.");
			}
			this._codec._Adler32 = Adler.Adler32(this._codec._Adler32, dictionary, 0, dictionary.Length);
			if (length < DeflateManager.MIN_MATCH)
			{
				return 0;
			}
			if (length > this.w_size - DeflateManager.MIN_LOOKAHEAD)
			{
				length = this.w_size - DeflateManager.MIN_LOOKAHEAD;
				index = dictionary.Length - length;
			}
			Array.Copy(dictionary, index, this.window, 0, length);
			this.strstart = length;
			this.block_start = length;
			this.ins_h = (int)(this.window[0] & byte.MaxValue);
			this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[1] & byte.MaxValue)) & this.hash_mask);
			for (int i = 0; i <= length - DeflateManager.MIN_MATCH; i++)
			{
				this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[i + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask);
				this.prev[i & this.w_mask] = this.head[this.ins_h];
				this.head[this.ins_h] = (short)i;
			}
			return 0;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004878 File Offset: 0x00002A78
		internal int Deflate(FlushType flush)
		{
			if (this._codec.OutputBuffer == null || (this._codec.InputBuffer == null && this._codec.AvailableBytesIn != 0) || (this.status == DeflateManager.FINISH_STATE && flush != FlushType.Finish))
			{
				this._codec.Message = DeflateManager._ErrorMessage[4];
				throw new ZlibException(string.Format("Something is fishy. [{0}]", this._codec.Message));
			}
			if (this._codec.AvailableBytesOut == 0)
			{
				this._codec.Message = DeflateManager._ErrorMessage[7];
				throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
			}
			int old_flush = this.last_flush;
			this.last_flush = (int)flush;
			int num;
			if (this.status == DeflateManager.INIT_STATE)
			{
				int header = DeflateManager.Z_DEFLATED + (this.w_bits - 8 << 4) << 8;
				int level_flags = (this.compressionLevel - CompressionLevel.BestSpeed & 255) >> 1;
				if (level_flags > 3)
				{
					level_flags = 3;
				}
				header |= level_flags << 6;
				if (this.strstart != 0)
				{
					header |= DeflateManager.PRESET_DICT;
				}
				header += 31 - header % 31;
				this.status = DeflateManager.BUSY_STATE;
				byte[] array = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array[num] = (byte)(header >> 8);
				byte[] array2 = this.pending;
				num = this.pendingCount;
				this.pendingCount = num + 1;
				array2[num] = (byte)header;
				if (this.strstart != 0)
				{
					byte[] array3 = this.pending;
					num = this.pendingCount;
					this.pendingCount = num + 1;
					array3[num] = (byte)((this._codec._Adler32 & 4278190080U) >> 24);
					byte[] array4 = this.pending;
					num = this.pendingCount;
					this.pendingCount = num + 1;
					array4[num] = (byte)((this._codec._Adler32 & 16711680U) >> 16);
					byte[] array5 = this.pending;
					num = this.pendingCount;
					this.pendingCount = num + 1;
					array5[num] = (byte)((this._codec._Adler32 & 65280U) >> 8);
					byte[] array6 = this.pending;
					num = this.pendingCount;
					this.pendingCount = num + 1;
					array6[num] = (byte)(this._codec._Adler32 & 255U);
				}
				this._codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
			}
			if (this.pendingCount != 0)
			{
				this._codec.flush_pending();
				if (this._codec.AvailableBytesOut == 0)
				{
					this.last_flush = -1;
					return 0;
				}
			}
			else if (this._codec.AvailableBytesIn == 0 && flush <= (FlushType)old_flush && flush != FlushType.Finish)
			{
				return 0;
			}
			if (this.status == DeflateManager.FINISH_STATE && this._codec.AvailableBytesIn != 0)
			{
				this._codec.Message = DeflateManager._ErrorMessage[7];
				throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
			}
			if (this._codec.AvailableBytesIn != 0 || this.lookahead != 0 || (flush != FlushType.None && this.status != DeflateManager.FINISH_STATE))
			{
				BlockState bstate = this.DeflateFunction(flush);
				if (bstate == BlockState.FinishStarted || bstate == BlockState.FinishDone)
				{
					this.status = DeflateManager.FINISH_STATE;
				}
				if (bstate == BlockState.NeedMore || bstate == BlockState.FinishStarted)
				{
					if (this._codec.AvailableBytesOut == 0)
					{
						this.last_flush = -1;
					}
					return 0;
				}
				if (bstate == BlockState.BlockDone)
				{
					if (flush == FlushType.Partial)
					{
						this._tr_align();
					}
					else
					{
						this._tr_stored_block(0, 0, false);
						if (flush == FlushType.Full)
						{
							for (int i = 0; i < this.hash_size; i++)
							{
								this.head[i] = 0;
							}
						}
					}
					this._codec.flush_pending();
					if (this._codec.AvailableBytesOut == 0)
					{
						this.last_flush = -1;
						return 0;
					}
				}
			}
			if (flush != FlushType.Finish)
			{
				return 0;
			}
			if (!this.WantRfc1950HeaderBytes || this.Rfc1950BytesEmitted)
			{
				return 1;
			}
			byte[] array7 = this.pending;
			num = this.pendingCount;
			this.pendingCount = num + 1;
			array7[num] = (byte)((this._codec._Adler32 & 4278190080U) >> 24);
			byte[] array8 = this.pending;
			num = this.pendingCount;
			this.pendingCount = num + 1;
			array8[num] = (byte)((this._codec._Adler32 & 16711680U) >> 16);
			byte[] array9 = this.pending;
			num = this.pendingCount;
			this.pendingCount = num + 1;
			array9[num] = (byte)((this._codec._Adler32 & 65280U) >> 8);
			byte[] array10 = this.pending;
			num = this.pendingCount;
			this.pendingCount = num + 1;
			array10[num] = (byte)(this._codec._Adler32 & 255U);
			this._codec.flush_pending();
			this.Rfc1950BytesEmitted = true;
			return (this.pendingCount == 0) ? 1 : 0;
		}

		// Token: 0x0400001C RID: 28
		private static readonly int MEM_LEVEL_MAX = 9;

		// Token: 0x0400001D RID: 29
		private static readonly int MEM_LEVEL_DEFAULT = 8;

		// Token: 0x0400001E RID: 30
		private DeflateManager.CompressFunc DeflateFunction;

		// Token: 0x0400001F RID: 31
		private static readonly string[] _ErrorMessage = new string[]
		{
			"need dictionary",
			"stream end",
			"",
			"file error",
			"stream error",
			"data error",
			"insufficient memory",
			"buffer error",
			"incompatible version",
			""
		};

		// Token: 0x04000020 RID: 32
		private static readonly int PRESET_DICT = 32;

		// Token: 0x04000021 RID: 33
		private static readonly int INIT_STATE = 42;

		// Token: 0x04000022 RID: 34
		private static readonly int BUSY_STATE = 113;

		// Token: 0x04000023 RID: 35
		private static readonly int FINISH_STATE = 666;

		// Token: 0x04000024 RID: 36
		private static readonly int Z_DEFLATED = 8;

		// Token: 0x04000025 RID: 37
		private static readonly int STORED_BLOCK = 0;

		// Token: 0x04000026 RID: 38
		private static readonly int STATIC_TREES = 1;

		// Token: 0x04000027 RID: 39
		private static readonly int DYN_TREES = 2;

		// Token: 0x04000028 RID: 40
		private static readonly int Z_BINARY = 0;

		// Token: 0x04000029 RID: 41
		private static readonly int Z_ASCII = 1;

		// Token: 0x0400002A RID: 42
		private static readonly int Z_UNKNOWN = 2;

		// Token: 0x0400002B RID: 43
		private static readonly int Buf_size = 16;

		// Token: 0x0400002C RID: 44
		private static readonly int MIN_MATCH = 3;

		// Token: 0x0400002D RID: 45
		private static readonly int MAX_MATCH = 258;

		// Token: 0x0400002E RID: 46
		private static readonly int MIN_LOOKAHEAD = DeflateManager.MAX_MATCH + DeflateManager.MIN_MATCH + 1;

		// Token: 0x0400002F RID: 47
		private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

		// Token: 0x04000030 RID: 48
		private static readonly int END_BLOCK = 256;

		// Token: 0x04000031 RID: 49
		internal ZlibCodec _codec;

		// Token: 0x04000032 RID: 50
		internal int status;

		// Token: 0x04000033 RID: 51
		internal byte[] pending;

		// Token: 0x04000034 RID: 52
		internal int nextPending;

		// Token: 0x04000035 RID: 53
		internal int pendingCount;

		// Token: 0x04000036 RID: 54
		internal sbyte data_type;

		// Token: 0x04000037 RID: 55
		internal int last_flush;

		// Token: 0x04000038 RID: 56
		internal int w_size;

		// Token: 0x04000039 RID: 57
		internal int w_bits;

		// Token: 0x0400003A RID: 58
		internal int w_mask;

		// Token: 0x0400003B RID: 59
		internal byte[] window;

		// Token: 0x0400003C RID: 60
		internal int window_size;

		// Token: 0x0400003D RID: 61
		internal short[] prev;

		// Token: 0x0400003E RID: 62
		internal short[] head;

		// Token: 0x0400003F RID: 63
		internal int ins_h;

		// Token: 0x04000040 RID: 64
		internal int hash_size;

		// Token: 0x04000041 RID: 65
		internal int hash_bits;

		// Token: 0x04000042 RID: 66
		internal int hash_mask;

		// Token: 0x04000043 RID: 67
		internal int hash_shift;

		// Token: 0x04000044 RID: 68
		internal int block_start;

		// Token: 0x04000045 RID: 69
		private DeflateManager.Config config;

		// Token: 0x04000046 RID: 70
		internal int match_length;

		// Token: 0x04000047 RID: 71
		internal int prev_match;

		// Token: 0x04000048 RID: 72
		internal int match_available;

		// Token: 0x04000049 RID: 73
		internal int strstart;

		// Token: 0x0400004A RID: 74
		internal int match_start;

		// Token: 0x0400004B RID: 75
		internal int lookahead;

		// Token: 0x0400004C RID: 76
		internal int prev_length;

		// Token: 0x0400004D RID: 77
		internal CompressionLevel compressionLevel;

		// Token: 0x0400004E RID: 78
		internal CompressionStrategy compressionStrategy;

		// Token: 0x0400004F RID: 79
		internal short[] dyn_ltree;

		// Token: 0x04000050 RID: 80
		internal short[] dyn_dtree;

		// Token: 0x04000051 RID: 81
		internal short[] bl_tree;

		// Token: 0x04000052 RID: 82
		internal Tree treeLiterals = new Tree();

		// Token: 0x04000053 RID: 83
		internal Tree treeDistances = new Tree();

		// Token: 0x04000054 RID: 84
		internal Tree treeBitLengths = new Tree();

		// Token: 0x04000055 RID: 85
		internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

		// Token: 0x04000056 RID: 86
		internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

		// Token: 0x04000057 RID: 87
		internal int heap_len;

		// Token: 0x04000058 RID: 88
		internal int heap_max;

		// Token: 0x04000059 RID: 89
		internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

		// Token: 0x0400005A RID: 90
		internal int _lengthOffset;

		// Token: 0x0400005B RID: 91
		internal int lit_bufsize;

		// Token: 0x0400005C RID: 92
		internal int last_lit;

		// Token: 0x0400005D RID: 93
		internal int _distanceOffset;

		// Token: 0x0400005E RID: 94
		internal int opt_len;

		// Token: 0x0400005F RID: 95
		internal int static_len;

		// Token: 0x04000060 RID: 96
		internal int matches;

		// Token: 0x04000061 RID: 97
		internal int last_eob_len;

		// Token: 0x04000062 RID: 98
		internal short bi_buf;

		// Token: 0x04000063 RID: 99
		internal int bi_valid;

		// Token: 0x04000064 RID: 100
		private bool Rfc1950BytesEmitted;

		// Token: 0x04000065 RID: 101
		private bool _WantRfc1950HeaderBytes = true;

		// Token: 0x020003BF RID: 959
		// (Invoke) Token: 0x0600396F RID: 14703
		internal delegate BlockState CompressFunc(FlushType flush);

		// Token: 0x020003C0 RID: 960
		internal class Config
		{
			// Token: 0x06003972 RID: 14706 RVA: 0x002D725D File Offset: 0x002D545D
			private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
			{
				this.GoodLength = goodLength;
				this.MaxLazy = maxLazy;
				this.NiceLength = niceLength;
				this.MaxChainLength = maxChainLength;
				this.Flavor = flavor;
			}

			// Token: 0x06003973 RID: 14707 RVA: 0x002D728A File Offset: 0x002D548A
			public static DeflateManager.Config Lookup(CompressionLevel level)
			{
				return DeflateManager.Config.Table[(int)level];
			}

			// Token: 0x0400265F RID: 9823
			internal int GoodLength;

			// Token: 0x04002660 RID: 9824
			internal int MaxLazy;

			// Token: 0x04002661 RID: 9825
			internal int NiceLength;

			// Token: 0x04002662 RID: 9826
			internal int MaxChainLength;

			// Token: 0x04002663 RID: 9827
			internal DeflateFlavor Flavor;

			// Token: 0x04002664 RID: 9828
			private static readonly DeflateManager.Config[] Table = new DeflateManager.Config[]
			{
				new DeflateManager.Config(0, 0, 0, 0, DeflateFlavor.Store),
				new DeflateManager.Config(4, 4, 8, 4, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 5, 16, 8, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 6, 32, 32, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 4, 16, 16, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 16, 32, 32, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 16, 128, 128, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 32, 128, 256, DeflateFlavor.Slow),
				new DeflateManager.Config(32, 128, 258, 1024, DeflateFlavor.Slow),
				new DeflateManager.Config(32, 258, 258, 4096, DeflateFlavor.Slow)
			};
		}
	}
}
