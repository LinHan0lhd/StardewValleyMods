using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x0200004A RID: 74
	public sealed class NetPoint : NetField<Point, NetPoint>
	{
		// Token: 0x060002DC RID: 732 RVA: 0x0000F3EE File Offset: 0x0000D5EE
		public NetPoint()
		{
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000F3F6 File Offset: 0x0000D5F6
		public NetPoint(Point value) : base(value)
		{
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0000F3FF File Offset: 0x0000D5FF
		// (set) Token: 0x060002DF RID: 735 RVA: 0x0000F40C File Offset: 0x0000D60C
		public int X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Point point = this.value;
				if (point.X != value)
				{
					Point newValue = new Point(value, point.Y);
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

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x0000F454 File Offset: 0x0000D654
		// (set) Token: 0x060002E1 RID: 737 RVA: 0x0000F464 File Offset: 0x0000D664
		public int Y
		{
			get
			{
				return base.Value.Y;
			}
			set
			{
				Point point = this.value;
				if (point.Y != value)
				{
					Point newValue = new Point(point.X, value);
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

		// Token: 0x060002E2 RID: 738 RVA: 0x0000F4AC File Offset: 0x0000D6AC
		public void Set(int x, int y)
		{
			this.Set(new Point(x, y));
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000F4BB File Offset: 0x0000D6BB
		public override void Set(Point newValue)
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

		// Token: 0x060002E4 RID: 740 RVA: 0x0000F4E8 File Offset: 0x0000D6E8
		protected override Point interpolate(Point startValue, Point endValue, float factor)
		{
			Point delta = new Point(endValue.X - startValue.X, endValue.Y - startValue.Y);
			delta.X = (int)((float)delta.X * factor);
			delta.Y = (int)((float)delta.Y * factor);
			return new Point(startValue.X + delta.X, startValue.Y + delta.Y);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0000F558 File Offset: 0x0000D758
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newX = reader.ReadInt32();
			int newY = reader.ReadInt32();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(new Point(newX, newY));
			}
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000F58F File Offset: 0x0000D78F
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(base.Value.X);
			writer.Write(base.Value.Y);
		}
	}
}
