using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley
{
	// Token: 0x0200010B RID: 267
	public class TemporaryAnimatedSpriteList : IList<TemporaryAnimatedSprite>, ICollection<TemporaryAnimatedSprite>, IEnumerable<TemporaryAnimatedSprite>, IEnumerable
	{
		// Token: 0x1700027B RID: 635
		public TemporaryAnimatedSprite this[int index]
		{
			get
			{
				return this.AnimatedSprites[index];
			}
			set
			{
				this.AnimatedSprites[index] = value;
			}
		}

		// Token: 0x0600157B RID: 5499 RVA: 0x000FE93F File Offset: 0x000FCB3F
		public void AddRange(IEnumerable<TemporaryAnimatedSprite> values)
		{
			this.AnimatedSprites.AddRange(values);
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x0600157C RID: 5500 RVA: 0x000FE94D File Offset: 0x000FCB4D
		public int Count
		{
			get
			{
				return this.AnimatedSprites.Count;
			}
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x0600157D RID: 5501 RVA: 0x000FE95A File Offset: 0x000FCB5A
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600157E RID: 5502 RVA: 0x000FE95D File Offset: 0x000FCB5D
		public void Add(TemporaryAnimatedSprite item)
		{
			this.AnimatedSprites.Add(item);
		}

		// Token: 0x0600157F RID: 5503 RVA: 0x000FE96C File Offset: 0x000FCB6C
		public void Clear()
		{
			foreach (TemporaryAnimatedSprite sprite in this.AnimatedSprites)
			{
				if (sprite.Pooled)
				{
					sprite.Pool();
				}
			}
			this.AnimatedSprites.Clear();
		}

		// Token: 0x06001580 RID: 5504 RVA: 0x000FE9D4 File Offset: 0x000FCBD4
		public bool Contains(TemporaryAnimatedSprite item)
		{
			return this.AnimatedSprites.Contains(item);
		}

		// Token: 0x06001581 RID: 5505 RVA: 0x000FE9E2 File Offset: 0x000FCBE2
		public void CopyTo(TemporaryAnimatedSprite[] array, int index)
		{
			this.AnimatedSprites.CopyTo(array, index);
		}

		// Token: 0x06001582 RID: 5506 RVA: 0x000FE9F1 File Offset: 0x000FCBF1
		public IEnumerator<TemporaryAnimatedSprite> GetEnumerator()
		{
			return this.AnimatedSprites.GetEnumerator();
		}

		// Token: 0x06001583 RID: 5507 RVA: 0x000FEA03 File Offset: 0x000FCC03
		public int IndexOf(TemporaryAnimatedSprite item)
		{
			return this.AnimatedSprites.IndexOf(item);
		}

		// Token: 0x06001584 RID: 5508 RVA: 0x000FEA11 File Offset: 0x000FCC11
		public void Insert(int index, TemporaryAnimatedSprite item)
		{
			this.AnimatedSprites.Insert(index, item);
		}

		// Token: 0x06001585 RID: 5509 RVA: 0x000FEA20 File Offset: 0x000FCC20
		public bool Remove(TemporaryAnimatedSprite item)
		{
			if (this.AnimatedSprites.Remove(item))
			{
				if (item.Pooled)
				{
					item.Pool();
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001586 RID: 5510 RVA: 0x000FEA44 File Offset: 0x000FCC44
		public void RemoveAt(int index)
		{
			TemporaryAnimatedSprite item = this.AnimatedSprites[index];
			this.AnimatedSprites.RemoveAt(index);
			if (item.Pooled)
			{
				item.Pool();
			}
		}

		// Token: 0x06001587 RID: 5511 RVA: 0x000FEA78 File Offset: 0x000FCC78
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04000DE3 RID: 3555
		public List<TemporaryAnimatedSprite> AnimatedSprites = new List<TemporaryAnimatedSprite>();
	}
}
