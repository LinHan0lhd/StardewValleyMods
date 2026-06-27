using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000046 RID: 70
	public sealed class NetDouble : NetField<double, NetDouble>
	{
		// Token: 0x060002B9 RID: 697 RVA: 0x0000EBCC File Offset: 0x0000CDCC
		public NetDouble()
		{
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000EBD4 File Offset: 0x0000CDD4
		public NetDouble(double value) : base(value)
		{
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000EBDD File Offset: 0x0000CDDD
		public override void Set(double newValue)
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

		// Token: 0x060002BC RID: 700 RVA: 0x0000EC05 File Offset: 0x0000CE05
		protected override double interpolate(double startValue, double endValue, float factor)
		{
			return startValue + (endValue - startValue) * (double)factor;
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000EC10 File Offset: 0x0000CE10
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			double newValue = reader.ReadDouble();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000EC3A File Offset: 0x0000CE3A
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
