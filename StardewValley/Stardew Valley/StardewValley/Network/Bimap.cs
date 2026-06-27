using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	// Token: 0x020001C8 RID: 456
	public class Bimap<L, R> : IEnumerable<KeyValuePair<!0, !1>>, IEnumerable
	{
		// Token: 0x17000344 RID: 836
		public R this[L l]
		{
			get
			{
				return this.leftToRight[l];
			}
			set
			{
				R rightKey;
				if (this.leftToRight.TryGetValue(l, out rightKey))
				{
					this.rightToLeft.Remove(rightKey);
				}
				L leftKey;
				if (this.rightToLeft.TryGetValue(value, out leftKey))
				{
					this.leftToRight.Remove(leftKey);
				}
				this.leftToRight[l] = value;
				this.rightToLeft[value] = l;
			}
		}

		// Token: 0x17000345 RID: 837
		public L this[R r]
		{
			get
			{
				return this.rightToLeft[r];
			}
			set
			{
				L leftKey;
				if (this.rightToLeft.TryGetValue(r, out leftKey))
				{
					this.leftToRight.Remove(leftKey);
				}
				R rightKey;
				if (this.leftToRight.TryGetValue(value, out rightKey))
				{
					this.rightToLeft.Remove(rightKey);
				}
				this.rightToLeft[r] = value;
				this.leftToRight[value] = r;
			}
		}

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x06002026 RID: 8230 RVA: 0x0016E6A1 File Offset: 0x0016C8A1
		public ICollection<L> LeftValues
		{
			get
			{
				return this.leftToRight.Keys;
			}
		}

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x06002027 RID: 8231 RVA: 0x0016E6AE File Offset: 0x0016C8AE
		public ICollection<R> RightValues
		{
			get
			{
				return this.rightToLeft.Keys;
			}
		}

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06002028 RID: 8232 RVA: 0x0016E6BB File Offset: 0x0016C8BB
		public int Count
		{
			get
			{
				return this.rightToLeft.Count;
			}
		}

		// Token: 0x06002029 RID: 8233 RVA: 0x0016E6C8 File Offset: 0x0016C8C8
		public void Clear()
		{
			this.leftToRight.Clear();
			this.rightToLeft.Clear();
		}

		// Token: 0x0600202A RID: 8234 RVA: 0x0016E6E0 File Offset: 0x0016C8E0
		public void Add(L l, R r)
		{
			if (this.leftToRight.ContainsKey(l) || this.rightToLeft.ContainsKey(r))
			{
				throw new ArgumentException();
			}
			this.leftToRight.Add(l, r);
			this.rightToLeft.Add(r, l);
		}

		// Token: 0x0600202B RID: 8235 RVA: 0x0016E71E File Offset: 0x0016C91E
		public bool ContainsLeft(L l)
		{
			return this.leftToRight.ContainsKey(l);
		}

		// Token: 0x0600202C RID: 8236 RVA: 0x0016E72C File Offset: 0x0016C92C
		public bool ContainsRight(R r)
		{
			return this.rightToLeft.ContainsKey(r);
		}

		// Token: 0x0600202D RID: 8237 RVA: 0x0016E73C File Offset: 0x0016C93C
		public void RemoveLeft(L l)
		{
			R rightKey;
			if (this.leftToRight.TryGetValue(l, out rightKey))
			{
				this.rightToLeft.Remove(rightKey);
			}
			this.leftToRight.Remove(l);
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x0016E774 File Offset: 0x0016C974
		public void RemoveRight(R r)
		{
			L leftKey;
			if (this.rightToLeft.TryGetValue(r, out leftKey))
			{
				this.leftToRight.Remove(leftKey);
			}
			this.rightToLeft.Remove(r);
		}

		// Token: 0x0600202F RID: 8239 RVA: 0x0016E7AB File Offset: 0x0016C9AB
		public L GetLeft(R r)
		{
			return this.rightToLeft[r];
		}

		// Token: 0x06002030 RID: 8240 RVA: 0x0016E7B9 File Offset: 0x0016C9B9
		public R GetRight(L l)
		{
			return this.leftToRight[l];
		}

		// Token: 0x06002031 RID: 8241 RVA: 0x0016E7C7 File Offset: 0x0016C9C7
		public IEnumerator<KeyValuePair<L, R>> GetEnumerator()
		{
			return this.leftToRight.GetEnumerator();
		}

		// Token: 0x06002032 RID: 8242 RVA: 0x0016E7D9 File Offset: 0x0016C9D9
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x040013A7 RID: 5031
		private Dictionary<L, R> leftToRight = new Dictionary<L, R>();

		// Token: 0x040013A8 RID: 5032
		private Dictionary<R, L> rightToLeft = new Dictionary<R, L>();
	}
}
