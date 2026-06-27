using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley.Objects
{
	// Token: 0x020001A1 RID: 417
	public class BedFurniture : Furniture
	{
		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06001D73 RID: 7539 RVA: 0x00151460 File Offset: 0x0014F660
		// (set) Token: 0x06001D74 RID: 7540 RVA: 0x001514D9 File Offset: 0x0014F6D9
		[XmlElement("bedType")]
		public BedFurniture.BedType bedType
		{
			get
			{
				if (this._bedType.Value == BedFurniture.BedType.Any)
				{
					BedFurniture.BedType bed_type = BedFurniture.BedType.Single;
					string[] data = base.getData();
					if (data != null && data.Length > 1)
					{
						string[] tokens = ArgUtility.SplitBySpace(data[1]);
						if (tokens.Length > 1)
						{
							string a = tokens[1];
							if (!(a == "double"))
							{
								if (a == "child")
								{
									bed_type = BedFurniture.BedType.Child;
								}
							}
							else
							{
								bed_type = BedFurniture.BedType.Double;
							}
						}
					}
					this._bedType.Value = bed_type;
				}
				return this._bedType.Value;
			}
			set
			{
				this._bedType.Value = value;
			}
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x001514E7 File Offset: 0x0014F6E7
		public BedFurniture()
		{
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x00151506 File Offset: 0x0014F706
		public BedFurniture(string itemId, Vector2 tile, int initialRotations) : base(itemId, tile, initialRotations)
		{
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x00151528 File Offset: 0x0014F728
		public BedFurniture(string itemId, Vector2 tile) : base(itemId, tile)
		{
		}

		// Token: 0x06001D78 RID: 7544 RVA: 0x00151549 File Offset: 0x0014F749
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this._bedType, "_bedType").AddField(this.mutex.NetFields, "mutex.NetFields");
		}

		// Token: 0x06001D79 RID: 7545 RVA: 0x00151580 File Offset: 0x0014F780
		public virtual bool IsBeingSleptIn()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (this.mutex.IsLocked())
			{
				return true;
			}
			Rectangle bedBounds = base.GetBoundingBox();
			using (FarmerCollection.Enumerator enumerator = location.farmers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetBoundingBox().Intersects(bedBounds))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001D7A RID: 7546 RVA: 0x00151608 File Offset: 0x0014F808
		public override void DayUpdate()
		{
			base.DayUpdate();
			this.mutex.ReleaseLock();
		}

		// Token: 0x06001D7B RID: 7547 RVA: 0x0015161B File Offset: 0x0014F81B
		public virtual void ReserveForNPC()
		{
			this.mutex.RequestLock(null, null);
		}

		// Token: 0x06001D7C RID: 7548 RVA: 0x0015162C File Offset: 0x0014F82C
		public override void AttemptRemoval(Action<Furniture> removal_action)
		{
			if (this._alreadyAttempingRemoval)
			{
				this._alreadyAttempingRemoval = false;
				return;
			}
			this._alreadyAttempingRemoval = true;
			this.mutex.RequestLock(delegate
			{
				this._alreadyAttempingRemoval = false;
				if (removal_action != null)
				{
					removal_action(this);
					this.mutex.ReleaseLock();
				}
			}, delegate
			{
				this._alreadyAttempingRemoval = false;
			});
		}

		// Token: 0x06001D7D RID: 7549 RVA: 0x00151688 File Offset: 0x0014F888
		public static BedFurniture GetBedAtTile(GameLocation location, int x, int y)
		{
			if (location == null)
			{
				return null;
			}
			foreach (Furniture furniture in location.furniture)
			{
				if (Utility.doesRectangleIntersectTile(furniture.GetBoundingBox(), x, y))
				{
					BedFurniture bedFurniture = furniture as BedFurniture;
					if (bedFurniture != null)
					{
						return bedFurniture;
					}
				}
			}
			return null;
		}

		// Token: 0x06001D7E RID: 7550 RVA: 0x001516FC File Offset: 0x0014F8FC
		public static void ApplyWakeUpPosition(Farmer who)
		{
			string lastSleptInName = who.lastSleepLocation.Value;
			GameLocation lastSleptIn = (lastSleptInName != null && Game1.isLocationAccessible(lastSleptInName)) ? Game1.getLocationFromName(lastSleptInName) : null;
			GameLocation disconnectLocation = Game1.getLocationFromName(who.disconnectLocation.Value);
			if (disconnectLocation != null && (long)who.disconnectDay.Value == (long)((ulong)Game1.MasterPlayer.stats.DaysPlayed) && !Game1.newDaySync.hasInstance())
			{
				who.currentLocation = disconnectLocation;
				who.Position = who.disconnectPosition.Value;
			}
			else
			{
				bool? flag = (lastSleptIn != null) ? new bool?(lastSleptIn.CanWakeUpHere(who, null)) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					who.Position = Utility.PointToVector2(who.lastSleepPoint.Value) * 64f;
					who.currentLocation = lastSleptIn;
					BedFurniture.ShiftPositionForBed(who);
				}
				else
				{
					if (lastSleptIn != null)
					{
						Game1.log.Verbose("Can't wake up in last sleep location '" + lastSleptIn.NameOrUniqueName + "' because it has no bed and doesn't have the 'AllowWakeUpWithoutBed: true' map property set.");
					}
					else if (lastSleptInName != null)
					{
						Game1.log.Verbose("Can't wake up in last sleep location '" + lastSleptInName + "' because no such location was found.");
					}
					FarmHouse home = Game1.RequireLocation<FarmHouse>(who.homeLocation.Value, false);
					who.currentLocation = home;
					who.Position = Utility.PointToVector2(home.GetPlayerBedSpot()) * 64f;
					BedFurniture.ShiftPositionForBed(who);
				}
			}
			if (who == Game1.player)
			{
				Game1.currentLocation = who.currentLocation;
			}
		}

		// Token: 0x06001D7F RID: 7551 RVA: 0x00151880 File Offset: 0x0014FA80
		public static void ShiftPositionForBed(Farmer who)
		{
			GameLocation location = who.currentLocation;
			BedFurniture bed = BedFurniture.GetBedAtTile(location, (int)(who.position.X / 64f), (int)(who.position.Y / 64f));
			if (bed != null)
			{
				who.Position = Utility.PointToVector2(bed.GetBedSpot()) * 64f;
				if (bed.bedType != BedFurniture.BedType.Double)
				{
					if (location.map == null)
					{
						location.reloadMap();
					}
					if (!location.CanItemBePlacedHere(new Vector2(bed.TileLocation.X - 1f, bed.TileLocation.Y + 1f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						who.faceDirection(3);
					}
					else
					{
						who.position.X -= 64f;
						who.faceDirection(1);
					}
				}
				else
				{
					bool should_wake_up_in_spouse_spot = false;
					FarmHouse farmhouse = location as FarmHouse;
					if (farmhouse != null && farmhouse.HasOwner)
					{
						long? spouse = farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID);
						long uniqueMultiplayerID = who.UniqueMultiplayerID;
						if (spouse.GetValueOrDefault() == uniqueMultiplayerID & spouse != null)
						{
							should_wake_up_in_spouse_spot = true;
						}
						else if (farmhouse.owner != who && !farmhouse.owner.isMarriedOrRoommates())
						{
							should_wake_up_in_spouse_spot = true;
						}
					}
					if (should_wake_up_in_spouse_spot)
					{
						who.position.X += 64f;
						who.faceDirection(3);
					}
					else
					{
						who.position.X -= 64f;
						who.faceDirection(1);
					}
				}
			}
			who.position.Y += 32f;
			NetRoot<Farmer> netRoot = who.NetFields.Root as NetRoot<Farmer>;
			if (netRoot == null)
			{
				return;
			}
			netRoot.CancelInterpolation();
		}

		// Token: 0x06001D80 RID: 7552 RVA: 0x00151A40 File Offset: 0x0014FC40
		public virtual bool CanModifyBed(Farmer who)
		{
			if (who == null)
			{
				return false;
			}
			GameLocation location = who.currentLocation;
			if (location == null)
			{
				return false;
			}
			FarmHouse farmhouse = location as FarmHouse;
			if (farmhouse != null && farmhouse.owner != who)
			{
				long? spouse = farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID);
				long uniqueMultiplayerID = who.UniqueMultiplayerID;
				if (!(spouse.GetValueOrDefault() == uniqueMultiplayerID & spouse != null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001D81 RID: 7553 RVA: 0x00151AAC File Offset: 0x0014FCAC
		public override int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
		{
			BedFurniture.<>c__DisplayClass24_0 CS$<>8__locals1;
			CS$<>8__locals1.x = x;
			CS$<>8__locals1.y = y;
			CS$<>8__locals1.location = location;
			if (this.bedType == BedFurniture.BedType.Double)
			{
				if (!BedFurniture.<GetAdditionalFurniturePlacementStatus>g__IsBedsideClear|24_0(-1, ref CS$<>8__locals1))
				{
					return -1;
				}
			}
			else if (!BedFurniture.<GetAdditionalFurniturePlacementStatus>g__IsBedsideClear|24_0(-1, ref CS$<>8__locals1) && !BedFurniture.<GetAdditionalFurniturePlacementStatus>g__IsBedsideClear|24_0(this.getTilesWide(), ref CS$<>8__locals1))
			{
				return -1;
			}
			return base.GetAdditionalFurniturePlacementStatus(CS$<>8__locals1.location, CS$<>8__locals1.x, CS$<>8__locals1.y, who);
		}

		// Token: 0x06001D82 RID: 7554 RVA: 0x00151B1C File Offset: 0x0014FD1C
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			this._alreadyAttempingRemoval = false;
			this.Location = location;
			if (!this.CanModifyBed(who))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"), true);
				return false;
			}
			FarmHouse farmhouse = location as FarmHouse;
			if (farmhouse != null && ((this.bedType == BedFurniture.BedType.Child && farmhouse.upgradeLevel < 2) || (this.bedType == BedFurniture.BedType.Double && farmhouse.upgradeLevel < 1)))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_NeedsUpgrade"), true);
				return false;
			}
			return base.placementAction(location, x, y, who);
		}

		// Token: 0x06001D83 RID: 7555 RVA: 0x00151BA8 File Offset: 0x0014FDA8
		public override void performRemoveAction()
		{
			this._alreadyAttempingRemoval = false;
			base.performRemoveAction();
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x00151BB8 File Offset: 0x0014FDB8
		public override void hoverAction()
		{
			if (Game1.player.GetBoundingBox().Intersects(base.GetBoundingBox()))
			{
				return;
			}
			base.hoverAction();
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x00151BE8 File Offset: 0x0014FDE8
		public override bool canBeRemoved(Farmer who)
		{
			if (this.Location == null)
			{
				return false;
			}
			if (!this.CanModifyBed(who))
			{
				if (!Game1.player.GetBoundingBox().Intersects(base.GetBoundingBox()))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"), true);
				}
				return false;
			}
			if (this.IsBeingSleptIn())
			{
				if (!Game1.player.GetBoundingBox().Intersects(base.GetBoundingBox()))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_InUse"), true);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06001D86 RID: 7558 RVA: 0x00151C73 File Offset: 0x0014FE73
		protected override Item GetOneNew()
		{
			return new BedFurniture(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001D87 RID: 7559 RVA: 0x00151C8C File Offset: 0x0014FE8C
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			BedFurniture fromBed = source as BedFurniture;
			if (fromBed != null)
			{
				this.bedType = fromBed.bedType;
			}
		}

		// Token: 0x06001D88 RID: 7560 RVA: 0x00151CB6 File Offset: 0x0014FEB6
		public virtual Point GetBedSpot()
		{
			return new Point((int)this.tileLocation.X + 1, (int)this.tileLocation.Y + 1);
		}

		// Token: 0x06001D89 RID: 7561 RVA: 0x00151CD9 File Offset: 0x0014FED9
		public override void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
		{
			base.actionOnPlayerEntryOrPlacement(environment, dropDown);
			this.UpdateBedTile(false);
		}

		// Token: 0x06001D8A RID: 7562 RVA: 0x00151CEC File Offset: 0x0014FEEC
		public virtual void UpdateBedTile(bool check_bounds)
		{
			Rectangle bounding_box = base.GetBoundingBox();
			if (this.bedType == BedFurniture.BedType.Double)
			{
				this.bedTileOffset = 1;
				return;
			}
			if (!check_bounds || !bounding_box.Intersects(Game1.player.GetBoundingBox()))
			{
				if (Game1.player.Position.X > (float)bounding_box.Center.X)
				{
					this.bedTileOffset = 0;
					return;
				}
				this.bedTileOffset = 1;
			}
		}

		// Token: 0x06001D8B RID: 7563 RVA: 0x00151D54 File Offset: 0x0014FF54
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.Location != null)
			{
				this.mutex.Update(Game1.getOnlineFarmers());
				this.UpdateBedTile(true);
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x06001D8C RID: 7564 RVA: 0x00151D7C File Offset: 0x0014FF7C
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			if (Furniture.isDrawingLocationFurniture)
			{
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle drawSourceRect = dataOrErrorItem.GetSourceRect(0, null);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition.Value + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawSourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Top + 1) / 10000f);
				drawSourceRect.X += drawSourceRect.Width;
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition.Value + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawSourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 1) / 10000f);
				return;
			}
			base.draw(spriteBatch, x, y, alpha);
		}

		// Token: 0x06001D8D RID: 7565 RVA: 0x00151F17 File Offset: 0x00150117
		public override bool AllowPlacementOnThisTile(int x, int y)
		{
			return (this.bedType == BedFurniture.BedType.Child && (float)y == this.TileLocation.Y + 1f) || base.AllowPlacementOnThisTile(x, y);
		}

		// Token: 0x06001D8E RID: 7566 RVA: 0x00151F44 File Offset: 0x00150144
		public override bool IntersectsForCollision(Rectangle rect)
		{
			Rectangle bounds = base.GetBoundingBox();
			Rectangle current_rect = bounds;
			current_rect.Height = 64;
			if (current_rect.Intersects(rect))
			{
				return true;
			}
			current_rect = bounds;
			current_rect.Y += 128;
			current_rect.Height -= 128;
			return current_rect.Intersects(rect);
		}

		// Token: 0x06001D8F RID: 7567 RVA: 0x00151F9E File Offset: 0x0015019E
		public override int GetAdditionalTilePropertyRadius()
		{
			return 1;
		}

		// Token: 0x06001D90 RID: 7568 RVA: 0x00151FA1 File Offset: 0x001501A1
		public static bool IsBedHere(GameLocation location, int x, int y)
		{
			if (location == null)
			{
				return false;
			}
			BedFurniture.ignoreContextualBedSpotOffset = true;
			if (location.doesTileHaveProperty(x, y, "Bed", "Back", false) != null)
			{
				BedFurniture.ignoreContextualBedSpotOffset = false;
				return true;
			}
			BedFurniture.ignoreContextualBedSpotOffset = false;
			return false;
		}

		// Token: 0x06001D91 RID: 7569 RVA: 0x00151FD4 File Offset: 0x001501D4
		public override bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (this.bedType == BedFurniture.BedType.Double && (float)tile_x == this.tileLocation.X - 1f && (float)tile_y == this.tileLocation.Y + 1f && layer_name == "Back" && property_name == "NoFurniture")
			{
				property_value = "T";
				return true;
			}
			if ((float)tile_x >= this.tileLocation.X && (float)tile_x < this.tileLocation.X + (float)this.getTilesWide() && (float)tile_y == this.tileLocation.Y + 1f && layer_name == "Back")
			{
				if (property_name == "Bed")
				{
					property_value = "T";
					return true;
				}
				if (this.bedType != BedFurniture.BedType.Child)
				{
					int bed_spot_x = (int)this.tileLocation.X + this.bedTileOffset;
					if (BedFurniture.ignoreContextualBedSpotOffset)
					{
						bed_spot_x = (int)this.tileLocation.X + 1;
					}
					if (tile_x == bed_spot_x && property_name == "TouchAction")
					{
						property_value = "Sleep";
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001D93 RID: 7571 RVA: 0x00152114 File Offset: 0x00150314
		[CompilerGenerated]
		internal static bool <GetAdditionalFurniturePlacementStatus>g__IsBedsideClear|24_0(int offsetX, ref BedFurniture.<>c__DisplayClass24_0 A_1)
		{
			Vector2 tile = new Vector2((float)(A_1.x / 64 + offsetX), (float)(A_1.y / 64 + 1));
			return A_1.location.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, true);
		}

		// Token: 0x0400123B RID: 4667
		public static string DEFAULT_BED_INDEX = "2048";

		// Token: 0x0400123C RID: 4668
		public static string DOUBLE_BED_INDEX = "2052";

		// Token: 0x0400123D RID: 4669
		public static string CHILD_BED_INDEX = "2076";

		// Token: 0x0400123E RID: 4670
		[XmlIgnore]
		public int bedTileOffset;

		// Token: 0x0400123F RID: 4671
		[XmlIgnore]
		protected bool _alreadyAttempingRemoval;

		// Token: 0x04001240 RID: 4672
		[XmlIgnore]
		public static bool ignoreContextualBedSpotOffset = false;

		// Token: 0x04001241 RID: 4673
		[XmlIgnore]
		protected NetEnum<BedFurniture.BedType> _bedType = new NetEnum<BedFurniture.BedType>(BedFurniture.BedType.Any);

		// Token: 0x04001242 RID: 4674
		[XmlIgnore]
		public NetMutex mutex = new NetMutex();

		// Token: 0x0200054B RID: 1355
		public enum BedType
		{
			// Token: 0x04002B23 RID: 11043
			Any = -1,
			// Token: 0x04002B24 RID: 11044
			Single,
			// Token: 0x04002B25 RID: 11045
			Double,
			// Token: 0x04002B26 RID: 11046
			Child
		}
	}
}
