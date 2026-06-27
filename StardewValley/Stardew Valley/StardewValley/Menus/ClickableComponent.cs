using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	// Token: 0x0200025E RID: 606
	public class ClickableComponent : IScreenReadable
	{
		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06002830 RID: 10288 RVA: 0x001D4012 File Offset: 0x001D2212
		// (set) Token: 0x06002831 RID: 10289 RVA: 0x001D401A File Offset: 0x001D221A
		public string ScreenReaderText { get; set; }

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06002832 RID: 10290 RVA: 0x001D4023 File Offset: 0x001D2223
		// (set) Token: 0x06002833 RID: 10291 RVA: 0x001D402B File Offset: 0x001D222B
		public string ScreenReaderDescription { get; set; }

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06002834 RID: 10292 RVA: 0x001D4034 File Offset: 0x001D2234
		// (set) Token: 0x06002835 RID: 10293 RVA: 0x001D403C File Offset: 0x001D223C
		public bool ScreenReaderIgnore { get; set; }

		// Token: 0x06002836 RID: 10294 RVA: 0x001D4048 File Offset: 0x001D2248
		public ClickableComponent(Rectangle bounds, string name)
		{
			this.bounds = bounds;
			this.name = name;
		}

		// Token: 0x06002837 RID: 10295 RVA: 0x001D40B0 File Offset: 0x001D22B0
		public ClickableComponent(Rectangle bounds, string name, string label)
		{
			this.bounds = bounds;
			this.name = name;
			this.label = label;
		}

		// Token: 0x06002838 RID: 10296 RVA: 0x001D411C File Offset: 0x001D231C
		public ClickableComponent(Rectangle bounds, Item item)
		{
			this.bounds = bounds;
			this.item = item;
		}

		// Token: 0x06002839 RID: 10297 RVA: 0x001D4181 File Offset: 0x001D2381
		public virtual bool containsPoint(int x, int y)
		{
			if (!this.visible)
			{
				return false;
			}
			if (this.bounds.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
				return true;
			}
			return false;
		}

		// Token: 0x0600283A RID: 10298 RVA: 0x001D41A4 File Offset: 0x001D23A4
		public virtual bool containsPoint(int x, int y, int extraMargin)
		{
			if (!this.visible)
			{
				return false;
			}
			Rectangle r = new Rectangle(this.bounds.X - extraMargin, this.bounds.Y - extraMargin, this.bounds.Width + extraMargin * 2, this.bounds.Height + extraMargin * 2);
			if (r.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
				return true;
			}
			return false;
		}

		// Token: 0x0600283B RID: 10299 RVA: 0x001D420D File Offset: 0x001D240D
		public void snapMouseCursorToCenter()
		{
			Game1.setMousePosition(this.bounds.Center.X, this.bounds.Center.Y);
		}

		// Token: 0x0600283C RID: 10300 RVA: 0x001D4234 File Offset: 0x001D2434
		public static void SetUpNeighbors<T>(List<T> components, int id) where T : ClickableComponent
		{
			for (int i = 0; i < components.Count; i++)
			{
				T item = components[i];
				if (item != null)
				{
					item.upNeighborID = id;
				}
			}
		}

		// Token: 0x0600283D RID: 10301 RVA: 0x001D4270 File Offset: 0x001D2470
		public static void ChainNeighborsLeftRight<T>(List<T> components) where T : ClickableComponent
		{
			ClickableComponent old_neighbor = null;
			for (int i = 0; i < components.Count; i++)
			{
				T item = components[i];
				if (item != null)
				{
					item.rightNeighborID = -1;
					item.leftNeighborID = -1;
					if (old_neighbor != null)
					{
						item.leftNeighborID = old_neighbor.myID;
						old_neighbor.rightNeighborID = item.myID;
					}
					old_neighbor = item;
				}
			}
		}

		// Token: 0x0600283E RID: 10302 RVA: 0x001D42E4 File Offset: 0x001D24E4
		public static void ChainNeighborsUpDown<T>(List<T> components) where T : ClickableComponent
		{
			ClickableComponent old_neighbor = null;
			for (int i = 0; i < components.Count; i++)
			{
				T item = components[i];
				if (item != null)
				{
					item.downNeighborID = -1;
					item.upNeighborID = -1;
					if (old_neighbor != null)
					{
						item.upNeighborID = old_neighbor.myID;
						old_neighbor.downNeighborID = item.myID;
					}
					old_neighbor = item;
				}
			}
		}

		// Token: 0x040019DE RID: 6622
		public const int ID_ignore = -500;

		// Token: 0x040019DF RID: 6623
		public const int CUSTOM_SNAP_BEHAVIOR = -7777;

		// Token: 0x040019E0 RID: 6624
		public const int SNAP_AUTOMATIC = -99998;

		// Token: 0x040019E1 RID: 6625
		public const int SNAP_TO_DEFAULT = -99999;

		// Token: 0x040019E2 RID: 6626
		public Rectangle bounds;

		// Token: 0x040019E3 RID: 6627
		public string name;

		// Token: 0x040019E4 RID: 6628
		public string label;

		// Token: 0x040019E5 RID: 6629
		public float scale = 1f;

		// Token: 0x040019E6 RID: 6630
		public Item item;

		// Token: 0x040019E7 RID: 6631
		public bool visible = true;

		// Token: 0x040019E8 RID: 6632
		public bool leftNeighborImmutable;

		// Token: 0x040019E9 RID: 6633
		public bool rightNeighborImmutable;

		// Token: 0x040019EA RID: 6634
		public bool upNeighborImmutable;

		// Token: 0x040019EB RID: 6635
		public bool downNeighborImmutable;

		// Token: 0x040019EC RID: 6636
		public bool fullyImmutable;

		// Token: 0x040019ED RID: 6637
		public int myID = -500;

		// Token: 0x040019EE RID: 6638
		public int myAlternateID = -500;

		// Token: 0x040019EF RID: 6639
		public int leftNeighborID = -1;

		// Token: 0x040019F0 RID: 6640
		public int rightNeighborID = -1;

		// Token: 0x040019F1 RID: 6641
		public int upNeighborID = -1;

		// Token: 0x040019F2 RID: 6642
		public int downNeighborID = -1;

		// Token: 0x040019F3 RID: 6643
		public int region;

		// Token: 0x040019F4 RID: 6644
		public bool tryDefaultIfNoRightNeighborExists;

		// Token: 0x040019F5 RID: 6645
		public bool tryDefaultIfNoDownNeighborExists;
	}
}
