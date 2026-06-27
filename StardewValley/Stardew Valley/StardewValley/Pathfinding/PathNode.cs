using System;

namespace StardewValley.Pathfinding
{
	// Token: 0x0200019D RID: 413
	public class PathNode : IEquatable<PathNode>
	{
		// Token: 0x06001D5E RID: 7518 RVA: 0x00150D4A File Offset: 0x0014EF4A
		public PathNode(int x, int y, PathNode parent)
		{
			this.x = x;
			this.y = y;
			this.parent = parent;
			this.id = PathNode.ComputeHash(x, y);
		}

		// Token: 0x06001D5F RID: 7519 RVA: 0x00150D74 File Offset: 0x0014EF74
		public PathNode(int x, int y, byte g, PathNode parent)
		{
			this.x = x;
			this.y = y;
			this.g = g;
			this.parent = parent;
			this.id = PathNode.ComputeHash(x, y);
		}

		// Token: 0x06001D60 RID: 7520 RVA: 0x00150DA6 File Offset: 0x0014EFA6
		public bool Equals(PathNode obj)
		{
			return obj != null && this.x == obj.x && this.y == obj.y;
		}

		// Token: 0x06001D61 RID: 7521 RVA: 0x00150DCC File Offset: 0x0014EFCC
		public override bool Equals(object obj)
		{
			PathNode other = obj as PathNode;
			return other != null && this.x == other.x && this.y == other.y;
		}

		// Token: 0x06001D62 RID: 7522 RVA: 0x00150E01 File Offset: 0x0014F001
		public override int GetHashCode()
		{
			return this.id;
		}

		// Token: 0x06001D63 RID: 7523 RVA: 0x00150E09 File Offset: 0x0014F009
		public static int ComputeHash(int x, int y)
		{
			return 100000 * x + y;
		}

		// Token: 0x04001229 RID: 4649
		public readonly int x;

		// Token: 0x0400122A RID: 4650
		public readonly int y;

		// Token: 0x0400122B RID: 4651
		public readonly int id;

		// Token: 0x0400122C RID: 4652
		public byte g;

		// Token: 0x0400122D RID: 4653
		public PathNode parent;
	}
}
