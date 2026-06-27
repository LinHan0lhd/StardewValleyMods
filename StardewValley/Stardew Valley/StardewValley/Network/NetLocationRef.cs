using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E3 RID: 483
	public class NetLocationRef : INetObject<NetFields>
	{
		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06002163 RID: 8547 RVA: 0x001733E6 File Offset: 0x001715E6
		public string LocationName
		{
			get
			{
				return this.locationName.Value;
			}
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06002164 RID: 8548 RVA: 0x001733F3 File Offset: 0x001715F3
		public bool IsStructure
		{
			get
			{
				return this.isStructure.Value;
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06002165 RID: 8549 RVA: 0x00173400 File Offset: 0x00171600
		// (set) Token: 0x06002166 RID: 8550 RVA: 0x00173408 File Offset: 0x00171608
		[XmlIgnore]
		public GameLocation Value
		{
			get
			{
				return this.Get();
			}
			set
			{
				this.Set(value);
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06002167 RID: 8551 RVA: 0x00173411 File Offset: 0x00171611
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("NetLocationRef");

		// Token: 0x06002168 RID: 8552 RVA: 0x0017341C File Offset: 0x0017161C
		public NetLocationRef()
		{
			this.NetFields.SetOwner(this).AddField(this.locationName, "locationName").AddField(this.isStructure, "isStructure");
			this.locationName.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._dirty = true;
			};
			this.isStructure.fieldChangeVisibleEvent += delegate(NetBool <p0>, bool <p1>, bool <p2>)
			{
				this._dirty = true;
			};
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x001734B7 File Offset: 0x001716B7
		public NetLocationRef(GameLocation value) : this()
		{
			this.Set(value);
		}

		// Token: 0x0600216A RID: 8554 RVA: 0x001734C6 File Offset: 0x001716C6
		public bool IsChanging()
		{
			return this.locationName.IsChanging() || this.isStructure.IsChanging();
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x001734E2 File Offset: 0x001716E2
		public void Update(bool forceUpdate = false)
		{
			if (forceUpdate)
			{
				this._dirty = true;
			}
			this.ApplyChangesIfDirty();
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x001734F4 File Offset: 0x001716F4
		public void ApplyChangesIfDirty()
		{
			if (this._usedLocalLocation && this._gameLocation != Game1.currentLocation)
			{
				this._dirty = true;
				this._usedLocalLocation = false;
			}
			if (this._dirty)
			{
				this._gameLocation = Game1.getLocationFromName(this.locationName.Value, this.isStructure.Value);
				this._dirty = false;
				Action onLocationChanged = this.OnLocationChanged;
				if (onLocationChanged != null)
				{
					onLocationChanged();
				}
			}
			if (!this._usedLocalLocation && this._gameLocation != Game1.currentLocation && this.IsCurrentlyViewedLocation())
			{
				this._usedLocalLocation = true;
				this._gameLocation = Game1.currentLocation;
			}
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x00173594 File Offset: 0x00171794
		public GameLocation Get()
		{
			this.ApplyChangesIfDirty();
			return this._gameLocation;
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x001735A4 File Offset: 0x001717A4
		public void Set(GameLocation location)
		{
			if (location == null)
			{
				this.isStructure.Value = false;
				this.locationName.Value = "";
			}
			else
			{
				this.isStructure.Value = location.isStructure.Value;
				this.locationName.Value = location.NameOrUniqueName;
			}
			if (this.IsCurrentlyViewedLocation())
			{
				this._usedLocalLocation = true;
				this._gameLocation = Game1.currentLocation;
			}
			else
			{
				this._gameLocation = location;
			}
			GameLocation gameLocation = this._gameLocation;
			bool? flag = (gameLocation != null) ? new bool?(gameLocation.IsTemporary) : null;
			if (flag != null && flag.GetValueOrDefault())
			{
				this._gameLocation = null;
			}
			this._dirty = false;
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x0017365C File Offset: 0x0017185C
		public bool IsCurrentlyViewedLocation()
		{
			return Game1.currentLocation != null && this.locationName.Value == Game1.currentLocation.NameOrUniqueName;
		}

		// Token: 0x040013F7 RID: 5111
		public readonly NetString locationName = new NetString();

		// Token: 0x040013F8 RID: 5112
		public readonly NetBool isStructure = new NetBool();

		// Token: 0x040013F9 RID: 5113
		protected GameLocation _gameLocation;

		// Token: 0x040013FA RID: 5114
		protected bool _dirty = true;

		// Token: 0x040013FB RID: 5115
		protected bool _usedLocalLocation;

		// Token: 0x040013FD RID: 5117
		[XmlIgnore]
		public Action OnLocationChanged;
	}
}
