using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000048 RID: 72
	public class NetString : NetField<string, NetString>
	{
		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060002C4 RID: 708 RVA: 0x0000ECC0 File Offset: 0x0000CEC0
		public int Length
		{
			get
			{
				return base.Value.Length;
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000ECCD File Offset: 0x0000CECD
		public NetString() : base(null)
		{
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000ECD6 File Offset: 0x0000CED6
		public NetString(string value) : base(value)
		{
		}

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060002C7 RID: 711 RVA: 0x0000ECE0 File Offset: 0x0000CEE0
		// (remove) Token: 0x060002C8 RID: 712 RVA: 0x0000ED18 File Offset: 0x0000CF18
		public event NetString.FilterString FilterStringEvent;

		// Token: 0x060002C9 RID: 713 RVA: 0x0000ED4D File Offset: 0x0000CF4D
		public override void Set(string newValue)
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

		// Token: 0x060002CA RID: 714 RVA: 0x0000ED7A File Offset: 0x0000CF7A
		public bool Contains(string substr)
		{
			return base.Value != null && base.Value.Contains(substr);
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0000ED94 File Offset: 0x0000CF94
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			string newValue = null;
			if (reader.ReadBoolean())
			{
				newValue = reader.ReadString();
				if (this.FilterStringEvent != null)
				{
					newValue = this.FilterStringEvent(newValue);
				}
			}
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000EDDD File Offset: 0x0000CFDD
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value != null);
			if (this.value != null)
			{
				writer.Write(this.value);
			}
		}

		// Token: 0x020003E1 RID: 993
		// (Invoke) Token: 0x060039DB RID: 14811
		public delegate string FilterString(string newValue);
	}
}
