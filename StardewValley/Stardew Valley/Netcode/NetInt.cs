using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200003F RID: 63
	public sealed class NetInt : NetField<int, NetInt>
	{
		// Token: 0x0600028C RID: 652 RVA: 0x0000E748 File Offset: 0x0000C948
		public NetInt()
		{
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000E750 File Offset: 0x0000C950
		public NetInt(int value) : base(value)
		{
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000E759 File Offset: 0x0000C959
		public override void Set(int newValue)
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

		// Token: 0x0600028F RID: 655 RVA: 0x0000E781 File Offset: 0x0000C981
		public new bool Equals(NetInt other)
		{
			return this.value == other.value;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0000E791 File Offset: 0x0000C991
		public bool Equals(int other)
		{
			return this.value == other;
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0000E79C File Offset: 0x0000C99C
		protected override int interpolate(int startValue, int endValue, float factor)
		{
			return startValue + (int)((float)(endValue - startValue) * factor);
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0000E7A8 File Offset: 0x0000C9A8
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int newValue = reader.ReadInt32();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x06000293 RID: 659 RVA: 0x0000E7D2 File Offset: 0x0000C9D2
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
