using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000043 RID: 67
	public sealed class NetBool : NetField<bool, NetBool>
	{
		// Token: 0x060002A8 RID: 680 RVA: 0x0000EA2C File Offset: 0x0000CC2C
		public NetBool()
		{
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000EA34 File Offset: 0x0000CC34
		public NetBool(bool value) : base(value)
		{
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000EA3D File Offset: 0x0000CC3D
		public override void Set(bool newValue)
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

		// Token: 0x060002AB RID: 683 RVA: 0x0000EA68 File Offset: 0x0000CC68
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			bool newValue = reader.ReadBoolean();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000EA92 File Offset: 0x0000CC92
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
