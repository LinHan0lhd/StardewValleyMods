using System;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
	// Token: 0x0200006D RID: 109
	internal class DeepCloneState
	{
		// Token: 0x06000452 RID: 1106 RVA: 0x00014C24 File Offset: 0x00012E24
		public object GetKnownRef(object from)
		{
			object[] baseFromTo = this._baseFromTo;
			if (from == baseFromTo[0])
			{
				return baseFromTo[3];
			}
			if (from == baseFromTo[1])
			{
				return baseFromTo[4];
			}
			if (from == baseFromTo[2])
			{
				return baseFromTo[5];
			}
			DeepCloneState.MiniDictionary loops = this._loops;
			if (loops == null)
			{
				return null;
			}
			return loops.FindEntry(from);
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x00014C68 File Offset: 0x00012E68
		public void AddKnownRef(object from, object to)
		{
			if (this._idx < 3)
			{
				this._baseFromTo[this._idx] = from;
				this._baseFromTo[this._idx + 3] = to;
				this._idx++;
				return;
			}
			if (this._loops == null)
			{
				this._loops = new DeepCloneState.MiniDictionary();
			}
			this._loops.Insert(from, to);
		}

		// Token: 0x040001A0 RID: 416
		private DeepCloneState.MiniDictionary _loops;

		// Token: 0x040001A1 RID: 417
		private readonly object[] _baseFromTo = new object[6];

		// Token: 0x040001A2 RID: 418
		private int _idx;

		// Token: 0x020003FE RID: 1022
		private class MiniDictionary
		{
			// Token: 0x06003A36 RID: 14902 RVA: 0x002D838A File Offset: 0x002D658A
			public MiniDictionary() : this(5)
			{
			}

			// Token: 0x06003A37 RID: 14903 RVA: 0x002D8393 File Offset: 0x002D6593
			public MiniDictionary(int capacity)
			{
				if (capacity > 0)
				{
					this.Initialize(capacity);
				}
			}

			// Token: 0x06003A38 RID: 14904 RVA: 0x002D83A8 File Offset: 0x002D65A8
			public object FindEntry(object key)
			{
				if (this._buckets != null)
				{
					int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
					DeepCloneState.MiniDictionary.Entry[] entries = this._entries;
					for (int i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = entries[i].Next)
					{
						if (entries[i].HashCode == hashCode && entries[i].Key == key)
						{
							return entries[i].Value;
						}
					}
				}
				return null;
			}

			// Token: 0x06003A39 RID: 14905 RVA: 0x002D8424 File Offset: 0x002D6624
			private static int GetPrime(int min)
			{
				for (int i = 0; i < DeepCloneState.MiniDictionary._primes.Length; i++)
				{
					int prime = DeepCloneState.MiniDictionary._primes[i];
					if (prime >= min)
					{
						return prime;
					}
				}
				for (int j = min | 1; j < 2147483647; j += 2)
				{
					if (DeepCloneState.MiniDictionary.IsPrime(j) && (j - 1) % 101 != 0)
					{
						return j;
					}
				}
				return min;
			}

			// Token: 0x06003A3A RID: 14906 RVA: 0x002D8478 File Offset: 0x002D6678
			private static bool IsPrime(int candidate)
			{
				if ((candidate & 1) != 0)
				{
					int limit = (int)Math.Sqrt((double)candidate);
					for (int divisor = 3; divisor <= limit; divisor += 2)
					{
						if (candidate % divisor == 0)
						{
							return false;
						}
					}
					return true;
				}
				return candidate == 2;
			}

			// Token: 0x06003A3B RID: 14907 RVA: 0x002D84AC File Offset: 0x002D66AC
			private static int ExpandPrime(int oldSize)
			{
				int newSize = 2 * oldSize;
				if (newSize > 2146435069 && 2146435069 > oldSize)
				{
					return 2146435069;
				}
				return DeepCloneState.MiniDictionary.GetPrime(newSize);
			}

			// Token: 0x06003A3C RID: 14908 RVA: 0x002D84DC File Offset: 0x002D66DC
			private void Initialize(int size)
			{
				this._buckets = new int[size];
				for (int i = 0; i < this._buckets.Length; i++)
				{
					this._buckets[i] = -1;
				}
				this._entries = new DeepCloneState.MiniDictionary.Entry[size];
			}

			// Token: 0x06003A3D RID: 14909 RVA: 0x002D8520 File Offset: 0x002D6720
			public void Insert(object key, object value)
			{
				if (this._buckets == null)
				{
					this.Initialize(0);
				}
				int hashCode = RuntimeHelpers.GetHashCode(key) & int.MaxValue;
				int targetBucket = hashCode % this._buckets.Length;
				DeepCloneState.MiniDictionary.Entry[] entries = this._entries;
				if (this._count == entries.Length)
				{
					this.Resize();
					entries = this._entries;
					targetBucket = hashCode % this._buckets.Length;
				}
				int index = this._count;
				this._count++;
				entries[index].HashCode = hashCode;
				entries[index].Next = this._buckets[targetBucket];
				entries[index].Key = key;
				entries[index].Value = value;
				this._buckets[targetBucket] = index;
			}

			// Token: 0x06003A3E RID: 14910 RVA: 0x002D85D7 File Offset: 0x002D67D7
			private void Resize()
			{
				this.Resize(DeepCloneState.MiniDictionary.ExpandPrime(this._count));
			}

			// Token: 0x06003A3F RID: 14911 RVA: 0x002D85EC File Offset: 0x002D67EC
			private void Resize(int newSize)
			{
				int[] newBuckets = new int[newSize];
				for (int i = 0; i < newBuckets.Length; i++)
				{
					newBuckets[i] = -1;
				}
				DeepCloneState.MiniDictionary.Entry[] newEntries = new DeepCloneState.MiniDictionary.Entry[newSize];
				Array.Copy(this._entries, 0, newEntries, 0, this._count);
				for (int j = 0; j < this._count; j++)
				{
					if (newEntries[j].HashCode >= 0)
					{
						int bucket = newEntries[j].HashCode % newSize;
						newEntries[j].Next = newBuckets[bucket];
						newBuckets[bucket] = j;
					}
				}
				this._buckets = newBuckets;
				this._entries = newEntries;
			}

			// Token: 0x040026EE RID: 9966
			private int[] _buckets;

			// Token: 0x040026EF RID: 9967
			private DeepCloneState.MiniDictionary.Entry[] _entries;

			// Token: 0x040026F0 RID: 9968
			private int _count;

			// Token: 0x040026F1 RID: 9969
			private static readonly int[] _primes = new int[]
			{
				3,
				7,
				11,
				17,
				23,
				29,
				37,
				47,
				59,
				71,
				89,
				107,
				131,
				163,
				197,
				239,
				293,
				353,
				431,
				521,
				631,
				761,
				919,
				1103,
				1327,
				1597,
				1931,
				2333,
				2801,
				3371,
				4049,
				4861,
				5839,
				7013,
				8419,
				10103,
				12143,
				14591,
				17519,
				21023,
				25229,
				30293,
				36353,
				43627,
				52361,
				62851,
				75431,
				90523,
				108631,
				130363,
				156437,
				187751,
				225307,
				270371,
				324449,
				389357,
				467237,
				560689,
				672827,
				807403,
				968897,
				1162687,
				1395263,
				1674319,
				2009191,
				2411033,
				2893249,
				3471899,
				4166287,
				4999559,
				5999471,
				7199369
			};

			// Token: 0x020006DF RID: 1759
			private struct Entry
			{
				// Token: 0x040030C4 RID: 12484
				public int HashCode;

				// Token: 0x040030C5 RID: 12485
				public int Next;

				// Token: 0x040030C6 RID: 12486
				public object Key;

				// Token: 0x040030C7 RID: 12487
				public object Value;
			}
		}
	}
}
