using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000040 RID: 64
	public sealed class NetIntDelta : NetField<int, NetIntDelta>
	{
		// Token: 0x06000294 RID: 660 RVA: 0x0000E7E0 File Offset: 0x0000C9E0
		public NetIntDelta()
		{
			base.Interpolated(false, false);
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0000E7F1 File Offset: 0x0000C9F1
		public NetIntDelta(int value) : base(value)
		{
			base.Interpolated(false, false);
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000E804 File Offset: 0x0000CA04
		private int fixRange(int value)
		{
			if (this.Minimum != null)
			{
				value = Math.Max(this.Minimum.Value, value);
			}
			if (this.Maximum != null)
			{
				value = Math.Min(this.Maximum.Value, value);
			}
			return value;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000E852 File Offset: 0x0000CA52
		public override void Set(int newValue)
		{
			newValue = this.fixRange(newValue);
			if (newValue != this.value)
			{
				base.cleanSet(newValue);
				if (Math.Abs(newValue - this.networkValue) > this.DirtyThreshold)
				{
					base.MarkDirty();
				}
			}
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000E888 File Offset: 0x0000CA88
		protected override int interpolate(int startValue, int endValue, float factor)
		{
			return startValue + (int)((float)(endValue - startValue) * factor);
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000E894 File Offset: 0x0000CA94
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			int delta = reader.ReadInt32();
			this.networkValue = this.fixRange(this.networkValue + delta);
			base.setInterpolationTarget(this.fixRange(this.targetValue + delta));
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0000E8D0 File Offset: 0x0000CAD0
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.targetValue - this.networkValue);
			this.networkValue = this.targetValue;
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0000E8F4 File Offset: 0x0000CAF4
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			int fullValue = reader.ReadInt32();
			base.cleanSet(fullValue);
			this.networkValue = fullValue;
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000E922 File Offset: 0x0000CB22
		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(this.targetValue);
			this.networkValue = this.targetValue;
		}

		// Token: 0x0400016E RID: 366
		private int networkValue;

		// Token: 0x0400016F RID: 367
		public int DirtyThreshold;

		// Token: 0x04000170 RID: 368
		public int? Minimum;

		// Token: 0x04000171 RID: 369
		public int? Maximum;
	}
}
