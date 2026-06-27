using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x0200004C RID: 76
	public sealed class NetColor : NetField<Color, NetColor>
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060002FA RID: 762 RVA: 0x0000F870 File Offset: 0x0000DA70
		// (set) Token: 0x060002FB RID: 763 RVA: 0x0000F88B File Offset: 0x0000DA8B
		public byte R
		{
			get
			{
				return base.Value.R;
			}
			set
			{
				base.Value = new Color(value, this.G, this.B, this.A);
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060002FC RID: 764 RVA: 0x0000F8AC File Offset: 0x0000DAAC
		// (set) Token: 0x060002FD RID: 765 RVA: 0x0000F8C7 File Offset: 0x0000DAC7
		public byte G
		{
			get
			{
				return base.Value.G;
			}
			set
			{
				base.Value = new Color(this.R, value, this.B, this.A);
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060002FE RID: 766 RVA: 0x0000F8E8 File Offset: 0x0000DAE8
		// (set) Token: 0x060002FF RID: 767 RVA: 0x0000F903 File Offset: 0x0000DB03
		public byte B
		{
			get
			{
				return base.Value.B;
			}
			set
			{
				base.Value = new Color(this.R, this.G, value, this.A);
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0000F924 File Offset: 0x0000DB24
		// (set) Token: 0x06000301 RID: 769 RVA: 0x0000F93F File Offset: 0x0000DB3F
		public byte A
		{
			get
			{
				return base.Value.A;
			}
			set
			{
				base.Value = new Color(this.R, this.G, this.B, value);
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000F95F File Offset: 0x0000DB5F
		public NetColor()
		{
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000F967 File Offset: 0x0000DB67
		public NetColor(Color value) : base(value)
		{
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000F970 File Offset: 0x0000DB70
		public override void Set(Color newValue)
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

		// Token: 0x06000305 RID: 773 RVA: 0x0000F99D File Offset: 0x0000DB9D
		public new bool Equals(NetColor other)
		{
			return this.value == other.value;
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0000F9B0 File Offset: 0x0000DBB0
		public bool Equals(Color other)
		{
			return this.value == other;
		}

		// Token: 0x06000307 RID: 775 RVA: 0x0000F9C0 File Offset: 0x0000DBC0
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			Color newValue = default(Color);
			newValue.PackedValue = reader.ReadUInt32();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0000F9F8 File Offset: 0x0000DBF8
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value.PackedValue);
		}
	}
}
