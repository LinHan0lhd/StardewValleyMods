using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x0200004B RID: 75
	public sealed class NetRectangle : NetField<Rectangle, NetRectangle>
	{
		// Token: 0x060002E7 RID: 743 RVA: 0x0000F5B3 File Offset: 0x0000D7B3
		public NetRectangle()
		{
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000F5BB File Offset: 0x0000D7BB
		public NetRectangle(Rectangle value) : base(value)
		{
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060002E9 RID: 745 RVA: 0x0000F5C4 File Offset: 0x0000D7C4
		// (set) Token: 0x060002EA RID: 746 RVA: 0x0000F5D4 File Offset: 0x0000D7D4
		public int X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Rectangle rect = this.value;
				if (rect.X != value)
				{
					Rectangle newValue = new Rectangle(value, rect.Y, rect.Width, rect.Height);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060002EB RID: 747 RVA: 0x0000F628 File Offset: 0x0000D828
		// (set) Token: 0x060002EC RID: 748 RVA: 0x0000F638 File Offset: 0x0000D838
		public int Y
		{
			get
			{
				return base.Value.Y;
			}
			set
			{
				Rectangle rect = this.value;
				if (rect.Y != value)
				{
					Rectangle newValue = new Rectangle(rect.X, value, rect.Width, rect.Height);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060002ED RID: 749 RVA: 0x0000F68C File Offset: 0x0000D88C
		// (set) Token: 0x060002EE RID: 750 RVA: 0x0000F69C File Offset: 0x0000D89C
		public int Width
		{
			get
			{
				return base.Value.Width;
			}
			set
			{
				Rectangle rect = this.value;
				if (rect.Width != value)
				{
					Rectangle newValue = new Rectangle(rect.X, rect.Y, value, rect.Height);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060002EF RID: 751 RVA: 0x0000F6F0 File Offset: 0x0000D8F0
		// (set) Token: 0x060002F0 RID: 752 RVA: 0x0000F700 File Offset: 0x0000D900
		public int Height
		{
			get
			{
				return base.Value.Height;
			}
			set
			{
				Rectangle rect = this.value;
				if (rect.Height != value)
				{
					Rectangle newValue = new Rectangle(rect.X, rect.Y, rect.Width, value);
					if (base.canShortcutSet())
					{
						this.value = newValue;
						return;
					}
					base.cleanSet(newValue);
					base.MarkDirty();
				}
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x0000F754 File Offset: 0x0000D954
		public Point Center
		{
			get
			{
				return this.value.Center;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000F761 File Offset: 0x0000D961
		public int Top
		{
			get
			{
				return this.value.Top;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x0000F76E File Offset: 0x0000D96E
		public int Bottom
		{
			get
			{
				return this.value.Bottom;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0000F77B File Offset: 0x0000D97B
		public int Left
		{
			get
			{
				return this.value.Left;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x0000F788 File Offset: 0x0000D988
		public int Right
		{
			get
			{
				return this.value.Right;
			}
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0000F795 File Offset: 0x0000D995
		public void Set(int x, int y, int width, int height)
		{
			this.Set(new Rectangle(x, y, width, height));
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000F7A7 File Offset: 0x0000D9A7
		public override void Set(Rectangle newValue)
		{
			if (base.canShortcutSet())
			{
				this.value = newValue;
				return;
			}
			if (newValue != this.value)
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000F7D4 File Offset: 0x0000D9D4
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newX = reader.ReadInt32();
			int newY = reader.ReadInt32();
			int newWidth = reader.ReadInt32();
			int newHeight = reader.ReadInt32();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(new Rectangle(newX, newY, newWidth, newHeight));
			}
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x0000F81C File Offset: 0x0000DA1C
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value.X);
			writer.Write(this.value.Y);
			writer.Write(this.value.Width);
			writer.Write(this.value.Height);
		}
	}
}
