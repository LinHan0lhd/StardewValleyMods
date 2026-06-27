using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000146 RID: 326
	[XmlInclude(typeof(Bush))]
	public abstract class LargeTerrainFeature : TerrainFeature
	{
		// Token: 0x170002CB RID: 715
		// (get) Token: 0x060019ED RID: 6637 RVA: 0x00131846 File Offset: 0x0012FA46
		// (set) Token: 0x060019EE RID: 6638 RVA: 0x00131853 File Offset: 0x0012FA53
		[XmlIgnore]
		public override Vector2 Tile
		{
			get
			{
				return this.netTilePosition.Value;
			}
			set
			{
				this.netTilePosition.Value = value;
			}
		}

		// Token: 0x060019EF RID: 6639 RVA: 0x00131861 File Offset: 0x0012FA61
		protected LargeTerrainFeature(bool needsTick) : base(needsTick)
		{
		}

		// Token: 0x060019F0 RID: 6640 RVA: 0x00131875 File Offset: 0x0012FA75
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.netTilePosition, "netTilePosition");
		}

		// Token: 0x060019F1 RID: 6641 RVA: 0x00131894 File Offset: 0x0012FA94
		public virtual void onDestroy()
		{
		}

		// Token: 0x04000FDB RID: 4059
		[XmlElement("tilePosition")]
		public readonly NetVector2 netTilePosition = new NetVector2();

		// Token: 0x04000FDC RID: 4060
		public bool isDestroyedByNPCTrample;
	}
}
