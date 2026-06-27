using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000045 RID: 69
	public class NetFloat : NetField<float, NetFloat>
	{
		// Token: 0x060002B3 RID: 691 RVA: 0x0000EB50 File Offset: 0x0000CD50
		public NetFloat()
		{
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000EB58 File Offset: 0x0000CD58
		public NetFloat(float value) : base(value)
		{
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000EB61 File Offset: 0x0000CD61
		public override void Set(float newValue)
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

		// Token: 0x060002B6 RID: 694 RVA: 0x0000EB89 File Offset: 0x0000CD89
		protected override float interpolate(float startValue, float endValue, float factor)
		{
			return startValue + (endValue - startValue) * factor;
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000EB94 File Offset: 0x0000CD94
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			float newValue = reader.ReadSingle();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000EBBE File Offset: 0x0000CDBE
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
