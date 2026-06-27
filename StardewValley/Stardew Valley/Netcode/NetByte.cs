using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000042 RID: 66
	public sealed class NetByte : NetField<byte, NetByte>
	{
		// Token: 0x060002A3 RID: 675 RVA: 0x0000E9B8 File Offset: 0x0000CBB8
		public NetByte()
		{
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000E9C0 File Offset: 0x0000CBC0
		public NetByte(byte value) : base(value)
		{
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000E9C9 File Offset: 0x0000CBC9
		public override void Set(byte newValue)
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

		// Token: 0x060002A6 RID: 678 RVA: 0x0000E9F4 File Offset: 0x0000CBF4
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			byte newValue = reader.ReadByte();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000EA1E File Offset: 0x0000CC1E
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(base.Value);
		}
	}
}
