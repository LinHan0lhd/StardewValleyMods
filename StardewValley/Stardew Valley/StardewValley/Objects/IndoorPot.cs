using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Objects
{
	// Token: 0x020001AE RID: 430
	public class IndoorPot : Object
	{
		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06001ED0 RID: 7888 RVA: 0x001631EF File Offset: 0x001613EF
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06001ED1 RID: 7889 RVA: 0x001631F6 File Offset: 0x001613F6
		// (set) Token: 0x06001ED2 RID: 7890 RVA: 0x00163200 File Offset: 0x00161400
		[XmlIgnore]
		public override GameLocation Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				if (this.hoeDirt.Value != null)
				{
					this.hoeDirt.Value.Location = value;
					this.hoeDirt.Value.Pot = this;
				}
				if (this.bush.Value != null)
				{
					this.bush.Value.Location = value;
				}
				base.Location = value;
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06001ED3 RID: 7891 RVA: 0x00163261 File Offset: 0x00161461
		// (set) Token: 0x06001ED4 RID: 7892 RVA: 0x0016326C File Offset: 0x0016146C
		public override Vector2 TileLocation
		{
			get
			{
				return base.TileLocation;
			}
			set
			{
				if (this.hoeDirt.Value != null)
				{
					this.hoeDirt.Value.Tile = value;
				}
				if (this.bush.Value != null)
				{
					this.bush.Value.Tile = value;
				}
				base.TileLocation = value;
			}
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x001632BC File Offset: 0x001614BC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.hoeDirt, "hoeDirt").AddField(this.bush, "bush").AddField(this.bushLoadDirty, "bushLoadDirty");
			this.bush.fieldChangeEvent += delegate(NetRef<Bush> field, Bush value, Bush newValue)
			{
				if (newValue != null)
				{
					newValue.Location = this.Location;
					newValue.inPot.Value = true;
				}
			};
		}

		// Token: 0x06001ED6 RID: 7894 RVA: 0x0016331D File Offset: 0x0016151D
		public IndoorPot()
		{
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x00163348 File Offset: 0x00161548
		public IndoorPot(Vector2 tileLocation) : base(tileLocation, "62", false)
		{
			GameLocation location = Game1.currentLocation;
			this.Location = location;
			this.hoeDirt.Value = new HoeDirt(0, location);
			if (location.IsRainingHere() && location.isOutdoors.Value)
			{
				this.Water();
			}
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x001633C0 File Offset: 0x001615C0
		public override void DayUpdate()
		{
			base.DayUpdate();
			this.hoeDirt.Value.dayUpdate();
			this.showNextIndex.Value = this.hoeDirt.Value.isWatered();
			GameLocation location = this.Location;
			if (location.isOutdoors.Value && location.IsRainingHere())
			{
				this.Water();
			}
			if (this.heldObject.Value != null)
			{
				this.readyForHarvest.Value = true;
			}
			Bush value = this.bush.Value;
			if (value == null)
			{
				return;
			}
			value.dayUpdate();
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x0016344E File Offset: 0x0016164E
		public void Water()
		{
			this.hoeDirt.Value.state.Value = 1;
			this.showNextIndex.Value = true;
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x00163474 File Offset: 0x00161674
		public bool IsPlantableItem(Item item)
		{
			if (item.HasTypeObject())
			{
				string qualifiedItemId = item.QualifiedItemId;
				if (qualifiedItemId == "(O)499" || qualifiedItemId == "(O)805")
				{
					return false;
				}
				if (item.Category == -19)
				{
					return true;
				}
				string cropItemId = Crop.ResolveSeedId(item.ItemId, this.Location);
				if (Game1.cropData.ContainsKey(cropItemId))
				{
					return true;
				}
				Object obj = item as Object;
				if (obj != null && obj.IsTeaSapling())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x001634F0 File Offset: 0x001616F0
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			if (who != null && dropInItem != null && this.bush.Value == null)
			{
				if (this.hoeDirt.Value.canPlantThisSeedHere(dropInItem.ItemId, dropInItem.Category == -19))
				{
					if (dropInItem.QualifiedItemId == "(O)805")
					{
						if (!probe)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
						}
						return false;
					}
					return probe || this.hoeDirt.Value.plant(dropInItem.ItemId, who, dropInItem.Category == -19);
				}
				else if (this.hoeDirt.Value.crop == null && dropInItem.QualifiedItemId == "(O)251")
				{
					if (!probe)
					{
						this.bush.Value = new Bush(this.tileLocation.Value, 3, this.Location, -1)
						{
							inPot = 
							{
								Value = true
							}
						};
						if (!this.Location.IsOutdoors)
						{
							this.bush.Value.loadSprite();
							Game1.playSound("coin", null);
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x00163618 File Offset: 0x00161818
		public override bool performToolAction(Tool t)
		{
			if (t != null)
			{
				this.hoeDirt.Value.performToolAction(t, -1, this.tileLocation.Value);
				if (this.bush.Value != null)
				{
					if (this.bush.Value.performToolAction(t, -1, this.tileLocation.Value))
					{
						this.bush.Value = null;
					}
					return false;
				}
			}
			if (this.hoeDirt.Value.isWatered())
			{
				this.Water();
			}
			return base.performToolAction(t);
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x001636A0 File Offset: 0x001618A0
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (who != null)
			{
				if (justCheckingForActivity)
				{
					return this.hoeDirt.Value.readyForHarvest() || this.heldObject.Value != null || (this.bush.Value != null && this.bush.Value.inBloom());
				}
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				if (this.heldObject.Value != null)
				{
					Object obj = this.heldObject.Value;
					int originalQuality = obj.Quality;
					obj.Quality = this.Location.GetHarvestSpawnedObjectQuality(who, obj.isForage(), this.TileLocation, null);
					bool flag = who.addItemToInventoryBool(obj, false);
					if (flag)
					{
						this.heldObject.Value = null;
						this.readyForHarvest.Value = false;
						Game1.playSound("coin", null);
						this.Location.OnHarvestedForage(who, obj);
						return flag;
					}
					this.heldObject.Value.Quality = originalQuality;
					return flag;
				}
				else
				{
					bool b = this.hoeDirt.Value.performUseAction(this.tileLocation.Value);
					if (b)
					{
						return b;
					}
					Crop crop = this.hoeDirt.Value.crop;
					if (crop != null && crop.currentPhase.Value > 0 && this.hoeDirt.Value.getMaxShake() == 0f)
					{
						this.hoeDirt.Value.shake(0.09817477f, 0.06283186f, Game1.random.NextBool());
						DelayedAction.playSoundAfterDelay("leafrustle", Game1.random.Next(100), null, null, -1, false);
					}
					Bush value = this.bush.Value;
					if (value != null)
					{
						value.performUseAction(this.tileLocation.Value);
					}
				}
			}
			return false;
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x0016386B File Offset: 0x00161A6B
		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			HoeDirt value = this.hoeDirt.Value;
			if (value == null)
			{
				return;
			}
			value.performPlayerEntryAction();
		}

		// Token: 0x06001EDF RID: 7903 RVA: 0x00163888 File Offset: 0x00161A88
		public override void updateWhenCurrentLocation(GameTime time)
		{
			base.updateWhenCurrentLocation(time);
			if (this.Location == null)
			{
				return;
			}
			this.hoeDirt.Value.tickUpdate(time);
			Bush value = this.bush.Value;
			if (value != null)
			{
				value.tickUpdate(time);
			}
			if (this.bushLoadDirty.Value)
			{
				Bush value2 = this.bush.Value;
				if (value2 != null)
				{
					value2.loadSprite();
				}
				this.bushLoadDirty.Value = false;
			}
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x00163900 File Offset: 0x00161B00
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 scaleFactor = this.getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
			Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect((this.showNextIndex.Value > false) ? 1 : 0, null)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f);
			if (this.hoeDirt.Value.HasFertilizer())
			{
				Rectangle fertilizer_rect = this.hoeDirt.Value.GetFertilizerSourceRect();
				fertilizer_rect.Width = 13;
				fertilizer_rect.Height = 13;
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileLocation.X * 64f + 4f, this.tileLocation.Y * 64f - 12f)), new Rectangle?(fertilizer_rect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.tileLocation.Y + 0.65f) * 64f / 10000f + (float)x * 1E-05f);
			}
			Crop crop = this.hoeDirt.Value.crop;
			if (crop != null)
			{
				crop.drawWithOffset(spriteBatch, this.tileLocation.Value, (this.hoeDirt.Value.isWatered() && this.hoeDirt.Value.crop.currentPhase.Value == 0 && !this.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, this.hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
			}
			Object value = this.heldObject.Value;
			if (value != null)
			{
				value.draw(spriteBatch, x * 64, y * 64 - 48, (this.tileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, 1f);
			}
			Bush value2 = this.bush.Value;
			if (value2 == null)
			{
				return;
			}
			value2.draw(spriteBatch, -24f);
		}

		// Token: 0x040012FC RID: 4860
		[XmlElement("hoeDirt")]
		public readonly NetRef<HoeDirt> hoeDirt = new NetRef<HoeDirt>();

		// Token: 0x040012FD RID: 4861
		[XmlElement("bush")]
		public readonly NetRef<Bush> bush = new NetRef<Bush>();

		// Token: 0x040012FE RID: 4862
		[XmlIgnore]
		public readonly NetBool bushLoadDirty = new NetBool(true);
	}
}
