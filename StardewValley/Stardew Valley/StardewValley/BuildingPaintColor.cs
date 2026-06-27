using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	// Token: 0x02000085 RID: 133
	public class BuildingPaintColor : INetObject<NetFields>
	{
		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060004F0 RID: 1264 RVA: 0x00019341 File Offset: 0x00017541
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("BuildingPaintColor");

		// Token: 0x060004F1 RID: 1265 RVA: 0x0001934C File Offset: 0x0001754C
		public BuildingPaintColor()
		{
			this.NetFields.SetOwner(this).AddField(this.ColorName, "ColorName").AddField(this.Color1Default, "Color1Default").AddField(this.Color2Default, "Color2Default").AddField(this.Color3Default, "Color3Default").AddField(this.Color1Hue, "Color1Hue").AddField(this.Color1Saturation, "Color1Saturation").AddField(this.Color1Lightness, "Color1Lightness").AddField(this.Color2Hue, "Color2Hue").AddField(this.Color2Saturation, "Color2Saturation").AddField(this.Color2Lightness, "Color2Lightness").AddField(this.Color3Hue, "Color3Hue").AddField(this.Color3Saturation, "Color3Saturation").AddField(this.Color3Lightness, "Color3Lightness");
			this.Color1Default.fieldChangeVisibleEvent += this.OnDefaultFlagChanged;
			this.Color2Default.fieldChangeVisibleEvent += this.OnDefaultFlagChanged;
			this.Color3Default.fieldChangeVisibleEvent += this.OnDefaultFlagChanged;
			this.Color1Hue.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color1Saturation.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color1Lightness.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color2Hue.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color2Saturation.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color2Lightness.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color3Hue.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color3Saturation.fieldChangeVisibleEvent += this.OnColorChanged;
			this.Color3Lightness.fieldChangeVisibleEvent += this.OnColorChanged;
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00019600 File Offset: 0x00017800
		public virtual void CopyFrom(BuildingPaintColor other)
		{
			this.ColorName.Value = other.ColorName.Value;
			this.Color1Default.Value = other.Color1Default.Value;
			this.Color1Hue.Value = other.Color1Hue.Value;
			this.Color1Saturation.Value = other.Color1Saturation.Value;
			this.Color1Lightness.Value = other.Color1Lightness.Value;
			this.Color2Default.Value = other.Color2Default.Value;
			this.Color2Hue.Value = other.Color2Hue.Value;
			this.Color2Saturation.Value = other.Color2Saturation.Value;
			this.Color2Lightness.Value = other.Color2Lightness.Value;
			this.Color3Default.Value = other.Color3Default.Value;
			this.Color3Hue.Value = other.Color3Hue.Value;
			this.Color3Saturation.Value = other.Color3Saturation.Value;
			this.Color3Lightness.Value = other.Color3Lightness.Value;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0001972B File Offset: 0x0001792B
		public virtual void OnDefaultFlagChanged(NetBool field, bool old_value, bool new_value)
		{
			this._dirty = true;
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00019734 File Offset: 0x00017934
		public virtual void OnColorChanged(NetInt field, int old_value, int new_value)
		{
			this._dirty = true;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0001973D File Offset: 0x0001793D
		public virtual void Poll(Action apply)
		{
			if (this._dirty)
			{
				if (apply != null)
				{
					apply();
				}
				this._dirty = false;
			}
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x00019757 File Offset: 0x00017957
		public bool IsDirty()
		{
			return this._dirty;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0001975F File Offset: 0x0001795F
		public bool RequiresRecolor()
		{
			return !this.Color1Default.Value || !this.Color2Default.Value || !this.Color3Default.Value;
		}

		// Token: 0x04000220 RID: 544
		public NetString ColorName = new NetString();

		// Token: 0x04000221 RID: 545
		public NetBool Color1Default = new NetBool(true);

		// Token: 0x04000222 RID: 546
		public NetInt Color1Hue = new NetInt();

		// Token: 0x04000223 RID: 547
		public NetInt Color1Saturation = new NetInt();

		// Token: 0x04000224 RID: 548
		public NetInt Color1Lightness = new NetInt();

		// Token: 0x04000225 RID: 549
		public NetBool Color2Default = new NetBool(true);

		// Token: 0x04000226 RID: 550
		public NetInt Color2Hue = new NetInt();

		// Token: 0x04000227 RID: 551
		public NetInt Color2Saturation = new NetInt();

		// Token: 0x04000228 RID: 552
		public NetInt Color2Lightness = new NetInt();

		// Token: 0x04000229 RID: 553
		public NetBool Color3Default = new NetBool(true);

		// Token: 0x0400022A RID: 554
		public NetInt Color3Hue = new NetInt();

		// Token: 0x0400022B RID: 555
		public NetInt Color3Saturation = new NetInt();

		// Token: 0x0400022C RID: 556
		public NetInt Color3Lightness = new NetInt();

		// Token: 0x0400022D RID: 557
		protected bool _dirty;
	}
}
