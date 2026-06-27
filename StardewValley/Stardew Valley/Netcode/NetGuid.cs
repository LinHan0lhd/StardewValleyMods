using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000047 RID: 71
	public sealed class NetGuid : NetField<Guid, NetGuid>
	{
		// Token: 0x060002BF RID: 703 RVA: 0x0000EC48 File Offset: 0x0000CE48
		public NetGuid()
		{
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000EC50 File Offset: 0x0000CE50
		public NetGuid(Guid value) : base(value)
		{
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000EC59 File Offset: 0x0000CE59
		public override void Set(Guid newValue)
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

		// Token: 0x060002C2 RID: 706 RVA: 0x0000EC88 File Offset: 0x0000CE88
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			Guid newValue = reader.ReadGuid();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000ECB2 File Offset: 0x0000CEB2
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.WriteGuid(this.value);
		}
	}
}
