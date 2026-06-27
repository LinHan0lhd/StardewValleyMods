using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000044 RID: 68
	public class NetRotation : NetField<float, NetRotation>
	{
		// Token: 0x060002AD RID: 685 RVA: 0x0000EAA0 File Offset: 0x0000CCA0
		public NetRotation()
		{
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000EAA8 File Offset: 0x0000CCA8
		public NetRotation(float value) : base(value)
		{
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000EAB1 File Offset: 0x0000CCB1
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

		// Token: 0x060002B0 RID: 688 RVA: 0x0000EADC File Offset: 0x0000CCDC
		protected override float interpolate(float startValue, float endValue, float factor)
		{
			float num = Math.Abs(endValue - startValue);
			float period = 6.2831855f;
			if (num > 180f)
			{
				if (endValue > startValue)
				{
					startValue += period;
				}
				else
				{
					endValue += period;
				}
			}
			return (startValue + (endValue - startValue) * factor) % period;
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000EB18 File Offset: 0x0000CD18
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			float newValue = reader.ReadSingle();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000EB42 File Offset: 0x0000CD42
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
