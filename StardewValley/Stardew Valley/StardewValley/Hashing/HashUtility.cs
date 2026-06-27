using System;
using System.Data.HashFunction;
using System.Text;

namespace StardewValley.Hashing
{
	// Token: 0x02000318 RID: 792
	public class HashUtility : IHashUtility
	{
		// Token: 0x06003443 RID: 13379 RVA: 0x0029C464 File Offset: 0x0029A664
		public int GetDeterministicHashCode(string value)
		{
			byte[] data = Encoding.UTF8.GetBytes(value);
			return this.GetDeterministicHashCode(data);
		}

		// Token: 0x06003444 RID: 13380 RVA: 0x0029C484 File Offset: 0x0029A684
		public int GetDeterministicHashCode(params int[] values)
		{
			byte[] data = new byte[values.Length * 4];
			Buffer.BlockCopy(values, 0, data, 0, data.Length);
			return this.GetDeterministicHashCode(data);
		}

		// Token: 0x06003445 RID: 13381 RVA: 0x0029C4AF File Offset: 0x0029A6AF
		public int GetDeterministicHashCode(byte[] data)
		{
			return BitConverter.ToInt32(HashUtility.Hasher.ComputeHash(data), 0);
		}

		// Token: 0x0400223B RID: 8763
		private static readonly IHashFunction Hasher = new xxHash(32);
	}
}
