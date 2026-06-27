using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Crc
{
	// Token: 0x02000023 RID: 35
	[Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CRC32
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000EB RID: 235 RVA: 0x0000A6FC File Offset: 0x000088FC
		public long TotalBytesRead
		{
			get
			{
				return this._TotalBytesRead;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000EC RID: 236 RVA: 0x0000A704 File Offset: 0x00008904
		public int Crc32Result
		{
			get
			{
				return (int)(~(int)this._register);
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x0000A70D File Offset: 0x0000890D
		public int GetCrc32(Stream input)
		{
			return this.GetCrc32AndCopy(input, null);
		}

		// Token: 0x060000EE RID: 238 RVA: 0x0000A718 File Offset: 0x00008918
		public int GetCrc32AndCopy(Stream input, Stream output)
		{
			if (input == null)
			{
				throw new Exception("The input stream must not be null.");
			}
			byte[] buffer = new byte[8192];
			int readSize = 8192;
			this._TotalBytesRead = 0L;
			int count = input.Read(buffer, 0, readSize);
			if (output != null)
			{
				output.Write(buffer, 0, count);
			}
			this._TotalBytesRead += (long)count;
			while (count > 0)
			{
				this.SlurpBlock(buffer, 0, count);
				count = input.Read(buffer, 0, readSize);
				if (output != null)
				{
					output.Write(buffer, 0, count);
				}
				this._TotalBytesRead += (long)count;
			}
			return (int)(~(int)this._register);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0000A7AC File Offset: 0x000089AC
		public int ComputeCrc32(int W, byte B)
		{
			return this._InternalComputeCrc32((uint)W, B);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000A7B6 File Offset: 0x000089B6
		internal int _InternalComputeCrc32(uint W, byte B)
		{
			return (int)(this.crc32Table[(int)((W ^ (uint)B) & 255U)] ^ W >> 8);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000A7CC File Offset: 0x000089CC
		public void SlurpBlock(byte[] block, int offset, int count)
		{
			if (block == null)
			{
				throw new Exception("The data buffer must not be null.");
			}
			for (int i = 0; i < count; i++)
			{
				int x = offset + i;
				byte b = block[x];
				if (this.reverseBits)
				{
					uint temp = this._register >> 24 ^ (uint)b;
					this._register = (this._register << 8 ^ this.crc32Table[(int)temp]);
				}
				else
				{
					uint temp2 = (this._register & 255U) ^ (uint)b;
					this._register = (this._register >> 8 ^ this.crc32Table[(int)temp2]);
				}
			}
			this._TotalBytesRead += (long)count;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x0000A860 File Offset: 0x00008A60
		public void UpdateCRC(byte b)
		{
			if (this.reverseBits)
			{
				uint temp = this._register >> 24 ^ (uint)b;
				this._register = (this._register << 8 ^ this.crc32Table[(int)temp]);
				return;
			}
			uint temp2 = (this._register & 255U) ^ (uint)b;
			this._register = (this._register >> 8 ^ this.crc32Table[(int)temp2]);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000A8C0 File Offset: 0x00008AC0
		public void UpdateCRC(byte b, int n)
		{
			while (n-- > 0)
			{
				if (this.reverseBits)
				{
					uint temp = this._register >> 24 ^ (uint)b;
					this._register = (this._register << 8 ^ this.crc32Table[(int)((temp >= 0U) ? temp : (temp + 256U))]);
				}
				else
				{
					uint temp2 = (this._register & 255U) ^ (uint)b;
					this._register = (this._register >> 8 ^ this.crc32Table[(int)((temp2 >= 0U) ? temp2 : (temp2 + 256U))]);
				}
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x0000A948 File Offset: 0x00008B48
		private static uint ReverseBits(uint data)
		{
			uint ret = (data & 1431655765U) << 1 | (data >> 1 & 1431655765U);
			ret = ((ret & 858993459U) << 2 | (ret >> 2 & 858993459U));
			ret = ((ret & 252645135U) << 4 | (ret >> 4 & 252645135U));
			return ret << 24 | (ret & 65280U) << 8 | (ret >> 8 & 65280U) | ret >> 24;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000A9B4 File Offset: 0x00008BB4
		private static byte ReverseBits(byte data)
		{
			int num = (int)data * 131586;
			uint i = 17055760U;
			uint s = (uint)(num & (int)i);
			uint t = (uint)(num << 2 & (int)((int)i << 1));
			return (byte)(16781313U * (s + t) >> 24);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000A9E8 File Offset: 0x00008BE8
		private void GenerateLookupTable()
		{
			this.crc32Table = new uint[256];
			byte i = 0;
			do
			{
				uint dwCrc = (uint)i;
				for (byte j = 8; j > 0; j -= 1)
				{
					if ((dwCrc & 1U) == 1U)
					{
						dwCrc = (dwCrc >> 1 ^ this.dwPolynomial);
					}
					else
					{
						dwCrc >>= 1;
					}
				}
				if (this.reverseBits)
				{
					this.crc32Table[(int)CRC32.ReverseBits(i)] = CRC32.ReverseBits(dwCrc);
				}
				else
				{
					this.crc32Table[(int)i] = dwCrc;
				}
				i += 1;
			}
			while (i != 0);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000AA5C File Offset: 0x00008C5C
		private uint gf2_matrix_times(uint[] matrix, uint vec)
		{
			uint sum = 0U;
			int i = 0;
			while (vec != 0U)
			{
				if ((vec & 1U) == 1U)
				{
					sum ^= matrix[i];
				}
				vec >>= 1;
				i++;
			}
			return sum;
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000AA88 File Offset: 0x00008C88
		private void gf2_matrix_square(uint[] square, uint[] mat)
		{
			for (int i = 0; i < 32; i++)
			{
				square[i] = this.gf2_matrix_times(mat, mat[i]);
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000AAB0 File Offset: 0x00008CB0
		public void Combine(int crc, int length)
		{
			uint[] even = new uint[32];
			uint[] odd = new uint[32];
			if (length == 0)
			{
				return;
			}
			uint crc2 = ~this._register;
			odd[0] = this.dwPolynomial;
			uint row = 1U;
			for (int i = 1; i < 32; i++)
			{
				odd[i] = row;
				row <<= 1;
			}
			this.gf2_matrix_square(even, odd);
			this.gf2_matrix_square(odd, even);
			uint len2 = (uint)length;
			do
			{
				this.gf2_matrix_square(even, odd);
				if ((len2 & 1U) == 1U)
				{
					crc2 = this.gf2_matrix_times(even, crc2);
				}
				len2 >>= 1;
				if (len2 == 0U)
				{
					break;
				}
				this.gf2_matrix_square(odd, even);
				if ((len2 & 1U) == 1U)
				{
					crc2 = this.gf2_matrix_times(odd, crc2);
				}
				len2 >>= 1;
			}
			while (len2 != 0U);
			crc2 ^= (uint)crc;
			this._register = ~crc2;
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000AB67 File Offset: 0x00008D67
		public CRC32() : this(false)
		{
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000AB70 File Offset: 0x00008D70
		public CRC32(bool reverseBits) : this(-306674912, reverseBits)
		{
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000AB7E File Offset: 0x00008D7E
		public CRC32(int polynomial, bool reverseBits)
		{
			this.reverseBits = reverseBits;
			this.dwPolynomial = (uint)polynomial;
			this.GenerateLookupTable();
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000ABA1 File Offset: 0x00008DA1
		public void Reset()
		{
			this._register = uint.MaxValue;
		}

		// Token: 0x04000134 RID: 308
		private uint dwPolynomial;

		// Token: 0x04000135 RID: 309
		private long _TotalBytesRead;

		// Token: 0x04000136 RID: 310
		private bool reverseBits;

		// Token: 0x04000137 RID: 311
		private uint[] crc32Table;

		// Token: 0x04000138 RID: 312
		private const int BUFFER_SIZE = 8192;

		// Token: 0x04000139 RID: 313
		private uint _register = uint.MaxValue;
	}
}
