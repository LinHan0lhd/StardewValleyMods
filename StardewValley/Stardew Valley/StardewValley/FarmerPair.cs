using System;

namespace StardewValley
{
	// Token: 0x020000AC RID: 172
	public struct FarmerPair
	{
		// Token: 0x060009ED RID: 2541 RVA: 0x0006C344 File Offset: 0x0006A544
		public static FarmerPair MakePair(long f1, long f2)
		{
			return new FarmerPair
			{
				Farmer1 = Math.Min(f1, f2),
				Farmer2 = Math.Max(f1, f2)
			};
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x0006C376 File Offset: 0x0006A576
		public bool Contains(long f)
		{
			return this.Farmer1 == f || this.Farmer2 == f;
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x0006C38C File Offset: 0x0006A58C
		public long GetOther(long f)
		{
			if (this.Farmer1 == f)
			{
				return this.Farmer2;
			}
			return this.Farmer1;
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x0006C3A4 File Offset: 0x0006A5A4
		public bool Equals(FarmerPair other)
		{
			return this.Farmer1 == other.Farmer1 && this.Farmer2 == other.Farmer2;
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x0006C3C4 File Offset: 0x0006A5C4
		public override bool Equals(object obj)
		{
			if (obj is FarmerPair)
			{
				FarmerPair pair = (FarmerPair)obj;
				return this.Equals(pair);
			}
			return false;
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x0006C3E9 File Offset: 0x0006A5E9
		public override int GetHashCode()
		{
			return this.Farmer1.GetHashCode() ^ this.Farmer2.GetHashCode() << 16;
		}

		// Token: 0x04000636 RID: 1590
		public long Farmer1;

		// Token: 0x04000637 RID: 1591
		public long Farmer2;
	}
}
