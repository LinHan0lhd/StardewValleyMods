using System;
using System.Collections.Generic;

namespace StardewValley.Pathfinding
{
	// Token: 0x0200019E RID: 414
	public class PriorityQueue
	{
		// Token: 0x06001D64 RID: 7524 RVA: 0x00150E14 File Offset: 0x0014F014
		public PriorityQueue()
		{
			this.nodes = new SortedDictionary<int, Queue<PathNode>>();
			this.total_size = 0;
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x00150E2E File Offset: 0x0014F02E
		public bool IsEmpty()
		{
			return this.total_size == 0;
		}

		// Token: 0x06001D66 RID: 7526 RVA: 0x00150E3C File Offset: 0x0014F03C
		public void Clear()
		{
			this.total_size = 0;
			foreach (KeyValuePair<int, Queue<PathNode>> i in this.nodes)
			{
				i.Value.Clear();
			}
		}

		// Token: 0x06001D67 RID: 7527 RVA: 0x00150E9C File Offset: 0x0014F09C
		public bool Contains(PathNode p, int priority)
		{
			Queue<PathNode> v;
			return this.nodes.TryGetValue(priority, out v) && v.Contains(p);
		}

		// Token: 0x06001D68 RID: 7528 RVA: 0x00150EC4 File Offset: 0x0014F0C4
		public PathNode Dequeue()
		{
			if (!this.IsEmpty())
			{
				foreach (Queue<PathNode> q in this.nodes.Values)
				{
					if (q.Count > 0)
					{
						this.total_size--;
						return q.Dequeue();
					}
				}
			}
			return null;
		}

		// Token: 0x06001D69 RID: 7529 RVA: 0x00150F40 File Offset: 0x0014F140
		public object Peek()
		{
			if (!this.IsEmpty())
			{
				foreach (Queue<PathNode> q in this.nodes.Values)
				{
					if (q.Count > 0)
					{
						return q.Peek();
					}
				}
			}
			return null;
		}

		// Token: 0x06001D6A RID: 7530 RVA: 0x00150FB0 File Offset: 0x0014F1B0
		public object Dequeue(int priority)
		{
			this.total_size--;
			return this.nodes[priority].Dequeue();
		}

		// Token: 0x06001D6B RID: 7531 RVA: 0x00150FD4 File Offset: 0x0014F1D4
		public void Enqueue(PathNode item, int priority)
		{
			Queue<PathNode> node;
			if (!this.nodes.TryGetValue(priority, out node))
			{
				this.nodes.Add(priority, new Queue<PathNode>());
				this.Enqueue(item, priority);
				return;
			}
			node.Enqueue(item);
			this.total_size++;
		}

		// Token: 0x0400122E RID: 4654
		private int total_size;

		// Token: 0x0400122F RID: 4655
		private SortedDictionary<int, Queue<PathNode>> nodes;
	}
}
