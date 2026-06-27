using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Mods;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000149 RID: 329
	[XmlInclude(typeof(Flooring))]
	[XmlInclude(typeof(FruitTree))]
	[XmlInclude(typeof(Grass))]
	[XmlInclude(typeof(HoeDirt))]
	[XmlInclude(typeof(LargeTerrainFeature))]
	[XmlInclude(typeof(ResourceClump))]
	[XmlInclude(typeof(Tree))]
	public abstract class TerrainFeature : INetObject<NetFields>, IHaveModData
	{
		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06001A0E RID: 6670 RVA: 0x0013354F File Offset: 0x0013174F
		// (set) Token: 0x06001A0F RID: 6671 RVA: 0x00133557 File Offset: 0x00131757
		[XmlIgnore]
		public virtual GameLocation Location { get; set; }

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06001A10 RID: 6672 RVA: 0x00133560 File Offset: 0x00131760
		// (set) Token: 0x06001A11 RID: 6673 RVA: 0x00133568 File Offset: 0x00131768
		[XmlIgnore]
		public virtual Vector2 Tile { get; set; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06001A12 RID: 6674 RVA: 0x00133571 File Offset: 0x00131771
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06001A13 RID: 6675 RVA: 0x00133579 File Offset: 0x00131779
		// (set) Token: 0x06001A14 RID: 6676 RVA: 0x00133586 File Offset: 0x00131786
		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return this.modData.GetForSerialization();
			}
			set
			{
				this.modData.SetFromSerialization(value);
			}
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06001A16 RID: 6678 RVA: 0x001335B7 File Offset: 0x001317B7
		// (set) Token: 0x06001A15 RID: 6677 RVA: 0x00133594 File Offset: 0x00131794
		[XmlIgnore]
		public bool NeedsUpdate
		{
			get
			{
				return this._needsUpdate;
			}
			set
			{
				if (value != this._needsUpdate)
				{
					this._needsUpdate = value;
					GameLocation location = this.Location;
					if (location == null)
					{
						return;
					}
					location.UpdateTerrainFeatureUpdateSubscription(this);
				}
			}
		}

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06001A17 RID: 6679 RVA: 0x001335BF File Offset: 0x001317BF
		public NetFields NetFields { get; }

		// Token: 0x06001A18 RID: 6680 RVA: 0x001335C7 File Offset: 0x001317C7
		protected TerrainFeature(bool needsTick)
		{
			this.NetFields = new NetFields(NetFields.GetNameForInstance<TerrainFeature>(this));
			this.NeedsTick = needsTick;
			this.initNetFields();
		}

		// Token: 0x06001A19 RID: 6681 RVA: 0x001335FF File Offset: 0x001317FF
		public virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.modData, "modData");
		}

		// Token: 0x06001A1A RID: 6682 RVA: 0x00133620 File Offset: 0x00131820
		public virtual Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		// Token: 0x06001A1B RID: 6683 RVA: 0x00133651 File Offset: 0x00131851
		public virtual Rectangle getRenderBounds()
		{
			return this.getBoundingBox();
		}

		// Token: 0x06001A1C RID: 6684 RVA: 0x00133659 File Offset: 0x00131859
		public virtual void loadSprite()
		{
		}

		// Token: 0x06001A1D RID: 6685 RVA: 0x0013365B File Offset: 0x0013185B
		public virtual bool isPassable(Character c = null)
		{
			return this.isTemporarilyInvisible;
		}

		// Token: 0x06001A1E RID: 6686 RVA: 0x00133663 File Offset: 0x00131863
		public virtual void OnAddedToLocation(GameLocation location, Vector2 tile)
		{
			this.Location = location;
			this.Tile = tile;
		}

		// Token: 0x06001A1F RID: 6687 RVA: 0x00133673 File Offset: 0x00131873
		public virtual void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
		{
		}

		// Token: 0x06001A20 RID: 6688 RVA: 0x00133675 File Offset: 0x00131875
		public virtual bool performUseAction(Vector2 tileLocation)
		{
			return false;
		}

		// Token: 0x06001A21 RID: 6689 RVA: 0x00133678 File Offset: 0x00131878
		public virtual bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			return false;
		}

		// Token: 0x06001A22 RID: 6690 RVA: 0x0013367B File Offset: 0x0013187B
		public virtual bool tickUpdate(GameTime time)
		{
			return false;
		}

		// Token: 0x06001A23 RID: 6691 RVA: 0x0013367E File Offset: 0x0013187E
		public virtual void dayUpdate()
		{
		}

		// Token: 0x06001A24 RID: 6692 RVA: 0x00133680 File Offset: 0x00131880
		public virtual bool seasonUpdate(bool onLoad)
		{
			return false;
		}

		// Token: 0x06001A25 RID: 6693 RVA: 0x00133683 File Offset: 0x00131883
		public virtual bool isActionable()
		{
			return false;
		}

		// Token: 0x06001A26 RID: 6694 RVA: 0x00133686 File Offset: 0x00131886
		public virtual void performPlayerEntryAction()
		{
			this.isTemporarilyInvisible = false;
		}

		// Token: 0x06001A27 RID: 6695 RVA: 0x0013368F File Offset: 0x0013188F
		public virtual void draw(SpriteBatch spriteBatch)
		{
		}

		// Token: 0x06001A28 RID: 6696 RVA: 0x00133691 File Offset: 0x00131891
		public virtual bool forceDraw()
		{
			return false;
		}

		// Token: 0x06001A29 RID: 6697 RVA: 0x00133694 File Offset: 0x00131894
		public virtual void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
		}

		// Token: 0x04000FF6 RID: 4086
		[XmlIgnore]
		public readonly bool NeedsTick;

		// Token: 0x04000FF7 RID: 4087
		[XmlIgnore]
		public bool isTemporarilyInvisible;

		// Token: 0x04000FF8 RID: 4088
		[XmlIgnore]
		protected bool _needsUpdate = true;
	}
}
