using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002D0 RID: 720
	public class FarmHouse : DecoratableLocation
	{
		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06002F0B RID: 12043 RVA: 0x0024E964 File Offset: 0x0024CB64
		[XmlIgnore]
		public virtual Farmer owner
		{
			get
			{
				return Game1.MasterPlayer;
			}
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06002F0C RID: 12044 RVA: 0x0024E96B File Offset: 0x0024CB6B
		[XmlIgnore]
		[MemberNotNullWhen(true, "owner")]
		public virtual bool HasOwner
		{
			[MemberNotNullWhen(true, "owner")]
			get
			{
				return this.owner != null;
			}
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06002F0D RID: 12045 RVA: 0x0024E976 File Offset: 0x0024CB76
		public virtual long OwnerId
		{
			get
			{
				Farmer owner = this.owner;
				if (owner == null)
				{
					return 0L;
				}
				return owner.UniqueMultiplayerID;
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06002F0E RID: 12046 RVA: 0x0024E98A File Offset: 0x0024CB8A
		[MemberNotNullWhen(true, "owner")]
		public bool IsOwnerActivated
		{
			[MemberNotNullWhen(true, "owner")]
			get
			{
				Farmer owner = this.owner;
				return owner != null && owner.isActive();
			}
		}

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06002F0F RID: 12047 RVA: 0x0024E9A0 File Offset: 0x0024CBA0
		[MemberNotNullWhen(true, "owner")]
		public bool IsOwnedByCurrentPlayer
		{
			[MemberNotNullWhen(true, "owner")]
			get
			{
				Farmer owner = this.owner;
				long? num = (owner != null) ? new long?(owner.UniqueMultiplayerID) : null;
				long uniqueMultiplayerID = Game1.player.UniqueMultiplayerID;
				return num.GetValueOrDefault() == uniqueMultiplayerID & num != null;
			}
		}

		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06002F10 RID: 12048 RVA: 0x0024E9EA File Offset: 0x0024CBEA
		// (set) Token: 0x06002F11 RID: 12049 RVA: 0x0024E9FD File Offset: 0x0024CBFD
		[XmlIgnore]
		public virtual int upgradeLevel
		{
			get
			{
				Farmer owner = this.owner;
				if (owner == null)
				{
					return 0;
				}
				return owner.HouseUpgradeLevel;
			}
			set
			{
				if (this.HasOwner)
				{
					this.owner.houseUpgradeLevel.Value = value;
				}
			}
		}

		// Token: 0x06002F12 RID: 12050 RVA: 0x0024EA18 File Offset: 0x0024CC18
		public FarmHouse()
		{
			this.fridge.Value.Location = this;
		}

		// Token: 0x06002F13 RID: 12051 RVA: 0x0024EAB4 File Offset: 0x0024CCB4
		public FarmHouse(string m, string name) : base(m, name)
		{
			this.fridge.Value.Location = this;
			this.ReadWallpaperAndFloorTileData();
			Farm farm = Game1.getFarm();
			this.AddStarterGiftBox(farm);
			this.AddStarterFurniture(farm);
			this.SetStarterFlooring(farm, null);
			this.SetStarterWallpaper(farm, null);
		}

		// Token: 0x06002F14 RID: 12052 RVA: 0x0024EB7C File Offset: 0x0024CD7C
		private void AddStarterGiftBox(Farm farm)
		{
			Chest box = new Chest(null, Vector2.Zero, true, 0, true);
			string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseStarterGift");
			for (int i = 0; i < fields.Length; i += 2)
			{
				string giftId;
				string error;
				int count;
				if (!ArgUtility.TryGet(fields, i, out giftId, out error, false, "string giftId") || !ArgUtility.TryGetOptionalInt(fields, i + 1, out count, out error, 0, "int count"))
				{
					farm.LogMapPropertyError("FarmHouseStarterGift", fields, error, ' ');
				}
				else
				{
					box.Items.Add(ItemRegistry.Create(giftId, count, 0, false));
				}
			}
			if (!box.Items.Any<Item>())
			{
				Item parsnipSeeds = ItemRegistry.Create("(O)472", 15, 0, false);
				box.Items.Add(parsnipSeeds);
			}
			Vector2 tile;
			if (!farm.TryGetMapPropertyAs("FarmHouseStarterSeedsPosition", out tile, false))
			{
				switch (Game1.whichFarm)
				{
				case 1:
				case 2:
				case 4:
					tile = new Vector2(4f, 7f);
					goto IL_127;
				case 3:
					tile = new Vector2(2f, 9f);
					goto IL_127;
				case 6:
					tile = new Vector2(8f, 6f);
					goto IL_127;
				}
				tile = new Vector2(3f, 7f);
			}
			IL_127:
			this.objects.Add(tile, box);
		}

		// Token: 0x06002F15 RID: 12053 RVA: 0x0024ECC0 File Offset: 0x0024CEC0
		private void AddStarterFurniture(Farm farm)
		{
			this.furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(9f, 8f)));
			string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseFurniture");
			if (fields.Any<string>())
			{
				for (int i = 0; i < fields.Length; i += 4)
				{
					int index;
					string error;
					Vector2 tile;
					int rotations;
					if (!ArgUtility.TryGetInt(fields, i, out index, out error, "int index") || !ArgUtility.TryGetVector2(fields, i + 1, out tile, out error, false, "Vector2 tile") || !ArgUtility.TryGetInt(fields, i + 3, out rotations, out error, "int rotations"))
					{
						farm.LogMapPropertyError("FarmHouseFurniture", fields, error, ' ');
					}
					else
					{
						Furniture newFurniture = ItemRegistry.Create<Furniture>("(F)" + index.ToString(), 1, 0, false);
						newFurniture.InitializeAtTile(tile);
						newFurniture.isOn.Value = true;
						for (int rotation = 0; rotation < rotations; rotation++)
						{
							newFurniture.rotate();
						}
						Furniture targetFurniture = base.GetFurnitureAt(tile);
						if (targetFurniture != null)
						{
							targetFurniture.heldObject.Value = newFurniture;
						}
						else
						{
							this.furniture.Add(newFurniture);
						}
					}
				}
				return;
			}
			switch (Game1.whichFarm)
			{
			case 0:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1120", 1, 0, false).SetPlacement(5, 4, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1364", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1376", 1, 0, false).SetPlacement(1, 10, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)0", 1, 0, false).SetPlacement(4, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1466", 1, 0, false).SetPlacement(1, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1618", 1, 0, false).SetPlacement(6, 8, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1602", 1, 0, false).SetPlacement(5, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				return;
			case 1:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1122", 1, 0, false).SetPlacement(1, 6, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1367", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)3", 1, 0, false).SetPlacement(1, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680", 1, 0, false).SetPlacement(5, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1673", 1, 0, false).SetPlacement(1, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1673", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1676", 1, 0, false).SetPlacement(5, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1737", 1, 0, false).SetPlacement(6, 8, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742", 1, 0, false).SetPlacement(5, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1675", 1, 0, false).SetPlacement(10, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				this.objects.Add(new Vector2(4f, 4f), ItemRegistry.Create<Object>("(BC)FishSmoker", 1, 0, false));
				return;
			case 2:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1134", 1, 0, false).SetPlacement(1, 7, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1748", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)3", 1, 0, false).SetPlacement(1, 6, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680", 1, 0, false).SetPlacement(6, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1296", 1, 0, false).SetPlacement(1, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1682", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1777", 1, 0, false).SetPlacement(6, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1745", 1, 0, false).SetPlacement(6, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1747", 1, 0, false).SetPlacement(5, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1296", 1, 0, false).SetPlacement(10, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				return;
			case 3:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1218", 1, 0, false).SetPlacement(1, 6, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1368", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1755", 1, 0, false).SetPlacement(1, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1755", 1, 0, false).SetPlacement(3, 6, 1));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680", 1, 0, false).SetPlacement(5, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1751", 1, 0, false).SetPlacement(5, 10, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1749", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1753", 1, 0, false).SetPlacement(5, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742", 1, 0, false).SetPlacement(5, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1794", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				return;
			case 4:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680", 1, 0, false).SetPlacement(1, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1628", 1, 0, false).SetPlacement(1, 5, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1393", 1, 0, false).SetPlacement(3, 4, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1369", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1678", 1, 0, false).SetPlacement(10, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1812", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1630", 1, 0, false).SetPlacement(1, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1811", 1, 0, false).SetPlacement(6, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1389", 1, 0, false).SetPlacement(10, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1758", 1, 0, false).SetPlacement(1, 10, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1794", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				return;
			case 5:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1466", 1, 0, false).SetPlacement(1, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(3, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(6, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1601", 1, 0, false).SetPlacement(10, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)202", 1, 0, false).SetPlacement(3, 4, 1));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1124", 1, 0, false).SetPlacement(4, 4, 1).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1379", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)202", 1, 0, false).SetPlacement(6, 4, 3));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1378", 1, 0, false).SetPlacement(10, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1377", 1, 0, false).SetPlacement(1, 9, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1445", 1, 0, false).SetPlacement(1, 10, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1618", 1, 0, false).SetPlacement(2, 9, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792", 1, 0, false).SetPlacement(this.getFireplacePoint(), 0));
				return;
			case 6:
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680", 1, 0, false).SetPlacement(4, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(7, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(3, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1283", 1, 0, false).SetPlacement(1, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(8, 1, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)202", 1, 0, false).SetPlacement(7, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(10, 4, 0));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)6", 1, 0, false).SetPlacement(2, 6, 1));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)6", 1, 0, false).SetPlacement(5, 7, 3));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1124", 1, 0, false).SetPlacement(3, 6, 0).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1362", 1, 0, false)));
				this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1228", 1, 0, false).SetPlacement(2, 9, 0));
				return;
			default:
				return;
			}
		}

		// Token: 0x06002F16 RID: 12054 RVA: 0x0024F79C File Offset: 0x0024D99C
		public static string GetStarterFlooring(Farm farm)
		{
			string id = (farm != null) ? farm.getMapProperty("FarmHouseFlooring") : null;
			if (id != null)
			{
				return id;
			}
			switch (Game1.whichFarm)
			{
			case 1:
				id = "1";
				break;
			case 2:
				id = "34";
				break;
			case 3:
				id = "18";
				break;
			case 4:
				id = "4";
				break;
			case 5:
				id = "5";
				break;
			case 6:
				id = "35";
				break;
			default:
				id = null;
				break;
			}
			return id;
		}

		// Token: 0x06002F17 RID: 12055 RVA: 0x0024F81C File Offset: 0x0024DA1C
		public static string GetStarterWallpaper(Farm farm)
		{
			string id = (farm != null) ? farm.getMapProperty("FarmHouseWallpaper") : null;
			if (id != null)
			{
				return id;
			}
			switch (Game1.whichFarm)
			{
			case 1:
				id = "11";
				break;
			case 2:
				id = "92";
				break;
			case 3:
				id = "12";
				break;
			case 4:
				id = "95";
				break;
			case 5:
				id = "65";
				break;
			case 6:
				id = "106";
				break;
			default:
				id = null;
				break;
			}
			return id;
		}

		// Token: 0x06002F18 RID: 12056 RVA: 0x0024F89C File Offset: 0x0024DA9C
		private void SetStarterFlooring(Farm farm, string styleToOverride = null)
		{
			string id = FarmHouse.GetStarterFlooring(farm);
			if (id != null)
			{
				base.SetFloor(id, null);
			}
		}

		// Token: 0x06002F19 RID: 12057 RVA: 0x0024F8BC File Offset: 0x0024DABC
		private void SetStarterWallpaper(Farm farm, string styleToOverride = null)
		{
			string id = FarmHouse.GetStarterWallpaper(farm);
			if (id != null)
			{
				base.SetWallpaper(id, null);
			}
		}

		// Token: 0x06002F1A RID: 12058 RVA: 0x0024F8DC File Offset: 0x0024DADC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.fridge, "fridge").AddField(this.cribStyle, "cribStyle").AddField(this.synchronizedDisplayedLevel, "synchronizedDisplayedLevel");
			this.cribStyle.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				if (this.map == null)
				{
					return;
				}
				if (this._appliedMapOverrides != null && this._appliedMapOverrides.Contains("crib"))
				{
					this._appliedMapOverrides.Remove("crib");
				}
				this.UpdateChildRoom();
				this.ReadWallpaperAndFloorTileData();
				this.setWallpapers();
				this.setFloors();
			};
			this.fridge.fieldChangeEvent += delegate(NetRef<Chest> field, Chest oldValue, Chest newValue)
			{
				newValue.Location = this;
			};
		}

		// Token: 0x06002F1B RID: 12059 RVA: 0x0024F954 File Offset: 0x0024DB54
		public List<Child> getChildren()
		{
			return this.characters.OfType<Child>().ToList<Child>();
		}

		// Token: 0x06002F1C RID: 12060 RVA: 0x0024F968 File Offset: 0x0024DB68
		public int getChildrenCount()
		{
			int count = 0;
			using (List<NPC>.Enumerator enumerator = this.characters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is Child)
					{
						count++;
					}
				}
			}
			return count;
		}

		// Token: 0x06002F1D RID: 12061 RVA: 0x0024F9C4 File Offset: 0x0024DBC4
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, false, false, false);
		}

		// Token: 0x06002F1E RID: 12062 RVA: 0x0024F9E8 File Offset: 0x0024DBE8
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			foreach (NPC c in this.characters)
			{
				if (c.isMarried())
				{
					if (c.getSpouse() == Game1.player)
					{
						c.checkForMarriageDialogue(timeOfDay, this);
					}
					if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && c.TilePoint != this.getSpouseBedSpot(c.Name) && (timeOfDay == 2200 || (c.controller == null && timeOfDay % 100 % 30 == 0)))
					{
						Point bed_spot = this.getSpouseBedSpot(c.Name);
						c.controller = null;
						PathFindController.endBehavior end_behavior = null;
						bool found_bed = this.GetSpouseBed() != null;
						if (found_bed)
						{
							end_behavior = new PathFindController.endBehavior(FarmHouse.spouseSleepEndFunction);
						}
						c.controller = new PathFindController(c, this, bed_spot, 0, end_behavior);
						if (c.controller.pathToEndPoint == null || !base.isTileOnMap(c.controller.pathToEndPoint.Last<Point>()))
						{
							c.controller = null;
						}
						else if (found_bed)
						{
							foreach (Furniture furniture in this.furniture)
							{
								BedFurniture bed = furniture as BedFurniture;
								if (bed != null && bed.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
								{
									bed.ReserveForNPC();
									break;
								}
							}
						}
					}
				}
				Child child = c as Child;
				if (child != null)
				{
					child.tenMinuteUpdate();
				}
			}
		}

		// Token: 0x06002F1F RID: 12063 RVA: 0x0024FBDC File Offset: 0x0024DDDC
		public static void spouseSleepEndFunction(Character c, GameLocation location)
		{
			NPC npc = c as NPC;
			if (npc != null)
			{
				if (DataLoader.AnimationDescriptions(Game1.content).ContainsKey(npc.name.Value.ToLower() + "_sleep"))
				{
					npc.playSleepingAnimation();
				}
				Microsoft.Xna.Framework.Rectangle npcBounds = npc.GetBoundingBox();
				foreach (Furniture furniture in location.furniture)
				{
					BedFurniture bed = furniture as BedFurniture;
					if (bed != null && bed.GetBoundingBox().Intersects(npcBounds))
					{
						bed.ReserveForNPC();
						break;
					}
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					if (Game1.random.NextDouble() < 0.8)
					{
						npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight0", npc.getTermOfSpousalEndearment(Game1.random.NextDouble() < 0.1)), null, 2, 3000, 0);
						return;
					}
					npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight1"), null, 2, 3000, 0);
				}
			}
		}

		// Token: 0x06002F20 RID: 12064 RVA: 0x0024FD24 File Offset: 0x0024DF24
		public virtual Point getFrontDoorSpot()
		{
			foreach (Warp warp in this.warps)
			{
				if (warp.TargetName == "Farm")
				{
					if (this is Cabin)
					{
						return new Point(warp.TargetX, warp.TargetY);
					}
					if (warp.TargetX == 64 && warp.TargetY == 15)
					{
						return Game1.getFarm().GetMainFarmHouseEntry();
					}
					return new Point(warp.TargetX, warp.TargetY);
				}
			}
			return Game1.getFarm().GetMainFarmHouseEntry();
		}

		// Token: 0x06002F21 RID: 12065 RVA: 0x0024FDE0 File Offset: 0x0024DFE0
		public virtual Point getPorchStandingSpot()
		{
			Point p = Game1.getFarm().GetMainFarmHouseEntry();
			p.X += 2;
			return p;
		}

		// Token: 0x06002F22 RID: 12066 RVA: 0x0024FE08 File Offset: 0x0024E008
		public Point getKitchenStandingSpot()
		{
			Point position;
			if (base.TryGetMapPropertyAs("KitchenStandingLocation", out position, false))
			{
				return position;
			}
			int upgradeLevel = this.upgradeLevel;
			if (upgradeLevel == 1)
			{
				return new Point(4, 5);
			}
			if (upgradeLevel - 2 > 1)
			{
				return new Point(-1000, -1000);
			}
			return new Point(22, 24);
		}

		// Token: 0x06002F23 RID: 12067 RVA: 0x0024FE5C File Offset: 0x0024E05C
		public virtual BedFurniture GetSpouseBed()
		{
			if (this.HasOwner)
			{
				NPC spouse = this.owner.getSpouse();
				if (((spouse != null) ? spouse.Name : null) == "Krobus")
				{
					return null;
				}
				if (this.owner.hasCurrentOrPendingRoommate() && this.GetBed(BedFurniture.BedType.Single, 0) != null)
				{
					return this.GetBed(BedFurniture.BedType.Single, 0);
				}
			}
			return this.GetBed(BedFurniture.BedType.Double, 0);
		}

		// Token: 0x06002F24 RID: 12068 RVA: 0x0024FEC0 File Offset: 0x0024E0C0
		public Point getSpouseBedSpot(string spouseName)
		{
			if (spouseName == "Krobus")
			{
				NPC characterFromName = Game1.getCharacterFromName(this.name.Value, true, false);
				if (characterFromName != null && characterFromName.isRoommate())
				{
					goto IL_35;
				}
			}
			if (this.GetSpouseBed() != null)
			{
				BedFurniture spouseBed = this.GetSpouseBed();
				Point bed_spot = this.GetSpouseBed().GetBedSpot();
				if (spouseBed.bedType == BedFurniture.BedType.Double)
				{
					bed_spot.X++;
				}
				return bed_spot;
			}
			IL_35:
			return this.GetSpouseRoomSpot();
		}

		// Token: 0x06002F25 RID: 12069 RVA: 0x0024FF30 File Offset: 0x0024E130
		public Point GetSpouseRoomSpot()
		{
			if (this.upgradeLevel == 0)
			{
				return new Point(-1000, -1000);
			}
			return this.spouseRoomSpot;
		}

		// Token: 0x06002F26 RID: 12070 RVA: 0x0024FF50 File Offset: 0x0024E150
		public BedFurniture GetBed(BedFurniture.BedType bed_type = BedFurniture.BedType.Any, int index = 0)
		{
			foreach (Furniture furniture in this.furniture)
			{
				BedFurniture bed = furniture as BedFurniture;
				if (bed != null && (bed_type == BedFurniture.BedType.Any || bed.bedType == bed_type))
				{
					if (index == 0)
					{
						return bed;
					}
					index--;
				}
			}
			return null;
		}

		// Token: 0x06002F27 RID: 12071 RVA: 0x0024FFC0 File Offset: 0x0024E1C0
		public Point GetPlayerBedSpot()
		{
			BedFurniture bed = this.GetPlayerBed();
			if (bed != null)
			{
				return bed.GetBedSpot();
			}
			return this.getEntryLocation();
		}

		// Token: 0x06002F28 RID: 12072 RVA: 0x0024FFE4 File Offset: 0x0024E1E4
		public BedFurniture GetPlayerBed()
		{
			if (this.upgradeLevel == 0)
			{
				return this.GetBed(BedFurniture.BedType.Single, 0);
			}
			return this.GetBed(BedFurniture.BedType.Double, 0);
		}

		// Token: 0x06002F29 RID: 12073 RVA: 0x00250000 File Offset: 0x0024E200
		public Point getBedSpot(BedFurniture.BedType bed_type = BedFurniture.BedType.Any)
		{
			BedFurniture bed = this.GetBed(bed_type, 0);
			if (bed != null)
			{
				return bed.GetBedSpot();
			}
			return new Point(-1000, -1000);
		}

		// Token: 0x06002F2A RID: 12074 RVA: 0x00250030 File Offset: 0x0024E230
		public Point getEntryLocation()
		{
			Point position;
			if (base.TryGetMapPropertyAs("EntryLocation", out position, false))
			{
				return position;
			}
			switch (this.upgradeLevel)
			{
			case 0:
				return new Point(3, 11);
			case 1:
				return new Point(9, 11);
			case 2:
			case 3:
				return new Point(27, 30);
			default:
				return new Point(-1000, -1000);
			}
		}

		// Token: 0x06002F2B RID: 12075 RVA: 0x0025009A File Offset: 0x0024E29A
		public BedFurniture GetChildBed(int index)
		{
			return this.GetBed(BedFurniture.BedType.Child, index);
		}

		// Token: 0x06002F2C RID: 12076 RVA: 0x002500A4 File Offset: 0x0024E2A4
		public Point GetChildBedSpot(int index)
		{
			BedFurniture child_bed = this.GetChildBed(index);
			if (child_bed != null)
			{
				return child_bed.GetBedSpot();
			}
			return Point.Zero;
		}

		// Token: 0x06002F2D RID: 12077 RVA: 0x002500C8 File Offset: 0x0024E2C8
		public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
		{
			return (!base.isTileOnMap(v) || base.getTileIndexAt((int)v.X, (int)v.Y, "Back", "indoor") != 0) && base.isTilePlaceable(v, itemIsPassable);
		}

		// Token: 0x06002F2E RID: 12078 RVA: 0x00250100 File Offset: 0x0024E300
		public Point getRandomOpenPointInHouse(Random r, int buffer = 0, int tries = 30)
		{
			for (int numTries = 0; numTries < tries; numTries++)
			{
				Point point = new Point(r.Next(this.map.Layers[0].LayerWidth), r.Next(this.map.Layers[0].LayerHeight));
				Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(point.X - buffer, point.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
				bool obstacleFound = false;
				foreach (Point point2 in rect.GetPoints())
				{
					int x = point2.X;
					int y = point2.Y;
					obstacleFound = (!base.hasTileAt(x, y, "Back", null) || !this.CanItemBePlacedHere(new Vector2((float)x, (float)y), false, CollisionMask.All, ~CollisionMask.Objects, false, false) || this.isTileOnWall(x, y));
					if (base.getTileIndexAt(x, y, "Back", "indoor") == 0)
					{
						obstacleFound = true;
					}
					if (obstacleFound)
					{
						break;
					}
				}
				if (!obstacleFound)
				{
					return point;
				}
			}
			return Point.Zero;
		}

		// Token: 0x06002F2F RID: 12079 RVA: 0x0025022C File Offset: 0x0024E42C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 173)
			{
				this.fridge.Value.fridge.Value = true;
				this.fridge.Value.checkForAction(who, false);
				return true;
			}
			if (base.getTileIndexAt(tileLocation, "Buildings", "indoor") == 2173)
			{
				if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse == "Emily")
				{
					EmilysParrot parrot = base.getTemporarySpriteByID(5858585) as EmilysParrot;
					if (parrot != null)
					{
						parrot.doAction();
					}
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002F30 RID: 12080 RVA: 0x002502E8 File Offset: 0x0024E4E8
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (this.HasOwner && Game1.IsMasterGame)
			{
				foreach (NPC spouse in this.characters)
				{
					Farmer spouse2 = spouse.getSpouse();
					long? num = (spouse2 != null) ? new long?(spouse2.UniqueMultiplayerID) : null;
					long ownerId = this.OwnerId;
					if ((num.GetValueOrDefault() == ownerId & num != null) && Game1.timeOfDay < 1500 && Game1.random.NextDouble() < 0.0006 && spouse.controller == null && spouse.Schedule == null && spouse.TilePoint != this.getSpouseBedSpot(Game1.player.spouse) && this.furniture.Count > 0)
					{
						Furniture f = this.furniture[Game1.random.Next(this.furniture.Count)];
						Microsoft.Xna.Framework.Rectangle b = f.boundingBox.Value;
						Vector2 possibleLocation = new Vector2((float)(b.X / 64), (float)(b.Y / 64));
						if (f.furniture_type.Value != 15 && f.furniture_type.Value != 12)
						{
							int tries = 0;
							int facingDirection = -3;
							while (tries < 3)
							{
								int xMove = Game1.random.Next(-1, 2);
								int yMove = Game1.random.Next(-1, 2);
								possibleLocation.X += (float)xMove;
								if (xMove == 0)
								{
									possibleLocation.Y += (float)yMove;
								}
								if (xMove != -1)
								{
									if (xMove != 1)
									{
										if (yMove != -1)
										{
											if (yMove == 1)
											{
												facingDirection = 0;
											}
										}
										else
										{
											facingDirection = 2;
										}
									}
									else
									{
										facingDirection = 3;
									}
								}
								else
								{
									facingDirection = 1;
								}
								if (this.CanItemBePlacedHere(possibleLocation, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
								{
									break;
								}
								tries++;
							}
							if (tries < 3)
							{
								spouse.controller = new PathFindController(spouse, this, new Point((int)possibleLocation.X, (int)possibleLocation.Y), facingDirection, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002F31 RID: 12081 RVA: 0x00250544 File Offset: 0x0024E744
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this.wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			this.fridge.Value.updateWhenCurrentLocation(time);
			if (Game1.player.isMarriedOrRoommates() && Game1.player.spouse != null)
			{
				NPC spouse = base.getCharacterFromName(Game1.player.spouse);
				if (spouse != null && !spouse.isEmoting)
				{
					Vector2 spousePos = spouse.Tile;
					foreach (Vector2 offset in Character.AdjacentTilesOffsets)
					{
						Vector2 v = spousePos + offset;
						Monster monster = base.isCharacterAtTile(v) as Monster;
						if (monster != null)
						{
							Microsoft.Xna.Framework.Rectangle monsterBounds = monster.GetBoundingBox();
							Point centerPixel = monsterBounds.Center;
							spouse.faceGeneralDirection(v * new Vector2(64f, 64f), 0, false);
							Game1.showSwordswipeAnimation(spouse.FacingDirection, spouse.Position, 60f, false);
							base.localSound("swordswipe", null, null, SoundContext.Default);
							spouse.shake(500);
							spouse.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:FarmHouse_SpouseAttacked" + (Game1.random.Next(12) + 1).ToString()), null, 2, 3000, 0);
							monster.takeDamage(50, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).X, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).Y, false, 1.0, Game1.player);
							if (monster.Health <= 0)
							{
								this.debris.Add(new Debris(monster.Sprite.textureName.Value, Game1.random.Next(6, 16), Utility.PointToVector2(centerPixel)));
								this.monsterDrop(monster, centerPixel.X, centerPixel.Y, this.owner);
								this.characters.Remove(monster);
								Stats stats = Game1.stats;
								uint monstersKilled = stats.MonstersKilled;
								stats.MonstersKilled = monstersKilled + 1U;
								Game1.player.changeFriendship(-10, spouse);
							}
							else
							{
								monster.shedChunks(4);
							}
							spouse.CurrentDialogue.Clear();
							spouse.CurrentDialogue.Push(spouse.TryGetDialogue("Spouse_MonstersInHouse") ?? new Dialogue(spouse, "Data\\ExtraDialogue:Spouse_MonstersInHouse", false));
						}
					}
				}
			}
		}

		// Token: 0x06002F32 RID: 12082 RVA: 0x002507BC File Offset: 0x0024E9BC
		public Point getFireplacePoint()
		{
			switch (this.upgradeLevel)
			{
			case 0:
				return new Point(8, 4);
			case 1:
				return new Point(26, 4);
			case 2:
			case 3:
				return new Point(17, 23);
			default:
				return new Point(-50, -50);
			}
		}

		// Token: 0x06002F33 RID: 12083 RVA: 0x0025080C File Offset: 0x0024EA0C
		public bool HasNpcSpouseOrRoommate()
		{
			Farmer owner = this.owner;
			return ((owner != null) ? owner.spouse : null) != null && this.owner.isMarriedOrRoommates();
		}

		// Token: 0x06002F34 RID: 12084 RVA: 0x0025082F File Offset: 0x0024EA2F
		public bool HasNpcSpouseOrRoommate(string spouseName)
		{
			if (spouseName != null)
			{
				Farmer owner = this.owner;
				if (((owner != null) ? owner.spouse : null) == spouseName)
				{
					return this.owner.isMarriedOrRoommates();
				}
			}
			return false;
		}

		// Token: 0x06002F35 RID: 12085 RVA: 0x0025085C File Offset: 0x0024EA5C
		public virtual void showSpouseRoom()
		{
			bool showSpouse = this.HasNpcSpouseOrRoommate();
			bool flag = this.displayingSpouseRoom;
			this.displayingSpouseRoom = showSpouse;
			this.updateMap();
			if (flag && !this.displayingSpouseRoom)
			{
				Point corner = this.GetSpouseRoomCorner();
				Microsoft.Xna.Framework.Rectangle sourceArea = CharacterSpouseRoomData.DefaultMapSourceRect;
				CharacterData spouseData;
				if (NPC.TryGetData(this.owner.spouse, out spouseData))
				{
					CharacterSpouseRoomData spouseRoom = spouseData.SpouseRoom;
					sourceArea = ((spouseRoom != null) ? spouseRoom.MapSourceRect : sourceArea);
				}
				Microsoft.Xna.Framework.Rectangle spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
				spouseRoomBounds.X--;
				List<Item> collected_items = new List<Item>();
				Microsoft.Xna.Framework.Rectangle room_bounds = new Microsoft.Xna.Framework.Rectangle(spouseRoomBounds.X * 64, spouseRoomBounds.Y * 64, spouseRoomBounds.Width * 64, spouseRoomBounds.Height * 64);
				foreach (Furniture placed_furniture in new List<Furniture>(this.furniture))
				{
					if (placed_furniture.GetBoundingBox().Intersects(room_bounds))
					{
						StorageFurniture storage_furniture = placed_furniture as StorageFurniture;
						if (storage_furniture != null)
						{
							collected_items.AddRange(storage_furniture.heldItems);
							storage_furniture.heldItems.Clear();
						}
						if (placed_furniture.heldObject.Value != null)
						{
							collected_items.Add(placed_furniture.heldObject.Value);
							placed_furniture.heldObject.Value = null;
						}
						collected_items.Add(placed_furniture);
						this.furniture.Remove(placed_furniture);
					}
				}
				for (int x = spouseRoomBounds.X; x <= spouseRoomBounds.Right; x++)
				{
					for (int y = spouseRoomBounds.Y; y <= spouseRoomBounds.Bottom; y++)
					{
						Object tile_object = base.getObjectAtTile(x, y, false);
						if (tile_object != null && !(tile_object is Furniture))
						{
							tile_object.performRemoveAction();
							Fence fence = tile_object as Fence;
							if (fence == null)
							{
								IndoorPot garden_pot = tile_object as IndoorPot;
								if (garden_pot == null)
								{
									Chest chest = tile_object as Chest;
									if (chest != null)
									{
										collected_items.AddRange(chest.Items);
										chest.Items.Clear();
									}
								}
								else
								{
									HoeDirt value = garden_pot.hoeDirt.Value;
									if (((value != null) ? value.crop : null) != null)
									{
										garden_pot.hoeDirt.Value.destroyCrop(false);
									}
								}
							}
							else
							{
								tile_object = new Object(fence.ItemId, 1, false, -1, 0);
							}
							tile_object.heldObject.Value = null;
							tile_object.minutesUntilReady.Value = -1;
							tile_object.readyForHarvest.Value = false;
							collected_items.Add(tile_object);
							this.objects.Remove(new Vector2((float)x, (float)y));
						}
					}
				}
				if (this.upgradeLevel >= 2)
				{
					Utility.createOverflowChest(this, new Vector2(39f, 32f), collected_items);
				}
				else
				{
					Utility.createOverflowChest(this, new Vector2(21f, 10f), collected_items);
				}
			}
			base.loadObjects();
			if (this.upgradeLevel == 3)
			{
				this.AddCellarTiles();
				this.createCellarWarps();
				Game1.player.craftingRecipes.TryAdd("Cask", 0);
			}
			if (showSpouse)
			{
				this.loadSpouseRoom();
			}
			Farmer owner = this.owner;
			this.lastSpouseRoom = ((owner != null) ? owner.spouse : null);
		}

		// Token: 0x06002F36 RID: 12086 RVA: 0x00250BB8 File Offset: 0x0024EDB8
		public virtual void AddCellarTiles()
		{
			if (this._appliedMapOverrides.Contains("cellar"))
			{
				this._appliedMapOverrides.Remove("cellar");
			}
			base.ApplyMapOverride("FarmHouse_Cellar", "cellar", null, null);
		}

		// Token: 0x06002F37 RID: 12087 RVA: 0x00250C0C File Offset: 0x0024EE0C
		public Cellar GetCellar()
		{
			string cellarName = this.GetCellarName();
			if (cellarName == null)
			{
				return null;
			}
			return Game1.RequireLocation<Cellar>(cellarName, false);
		}

		// Token: 0x06002F38 RID: 12088 RVA: 0x00250C2C File Offset: 0x0024EE2C
		public string GetCellarName()
		{
			int cellar_number = -1;
			if (this.HasOwner)
			{
				foreach (int i in Game1.player.team.cellarAssignments.Keys)
				{
					if (Game1.player.team.cellarAssignments[i] == this.OwnerId)
					{
						cellar_number = i;
					}
				}
			}
			if (cellar_number == 0 || cellar_number == 1)
			{
				return "Cellar";
			}
			if (cellar_number == -1)
			{
				return null;
			}
			return "Cellar" + cellar_number.ToString();
		}

		// Token: 0x06002F39 RID: 12089 RVA: 0x00250CD8 File Offset: 0x0024EED8
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (this.HasOwner)
			{
				if (Game1.timeOfDay >= 2200 && this.owner.spouse != null && base.getCharacterFromName(this.owner.spouse) != null && !this.owner.isEngaged())
				{
					Game1.player.team.requestSpouseSleepEvent.Fire(this.owner.UniqueMultiplayerID);
				}
				if (Game1.timeOfDay >= 2000 && this.IsOwnedByCurrentPlayer && Game1.getFarm().farmers.Count <= 1)
				{
					Game1.player.team.requestPetWarpHomeEvent.Fire(this.owner.UniqueMultiplayerID);
				}
			}
			if (Game1.IsMasterGame)
			{
				Farm farm = Game1.getFarm();
				for (int i = this.characters.Count - 1; i >= 0; i--)
				{
					Pet pet = this.characters[i] as Pet;
					if (pet != null)
					{
						Point tile = pet.TilePoint;
						Microsoft.Xna.Framework.Rectangle bounds = pet.GetBoundingBox();
						if (!base.isTileOnMap(tile.X, tile.Y) || base.hasTileAt(bounds.Left / 64, tile.Y, "Buildings", null) || base.hasTileAt(bounds.Right / 64, tile.Y, "Buildings", null))
						{
							pet.WarpToPetBowl();
							break;
						}
					}
				}
				for (int j = this.characters.Count - 1; j >= 0; j--)
				{
					for (int k = j - 1; k >= 0; k--)
					{
						if (j < this.characters.Count && k < this.characters.Count && (this.characters[k].Equals(this.characters[j]) || (this.characters[k].Name.Equals(this.characters[j].Name) && this.characters[k].IsVillager && this.characters[j].IsVillager)) && k != j)
						{
							this.characters.RemoveAt(k);
						}
					}
					for (int l = farm.characters.Count - 1; l >= 0; l--)
					{
						if (j < this.characters.Count && l < this.characters.Count && farm.characters[l].Equals(this.characters[j]))
						{
							farm.characters.RemoveAt(l);
						}
					}
				}
			}
		}

		// Token: 0x06002F3A RID: 12090 RVA: 0x00250F8B File Offset: 0x0024F18B
		public void UpdateForRenovation()
		{
			this.updateFarmLayout();
			this.setWallpapers();
			this.setFloors();
		}

		// Token: 0x06002F3B RID: 12091 RVA: 0x00250FA0 File Offset: 0x0024F1A0
		public void updateFarmLayout()
		{
			if (this.currentlyDisplayedUpgradeLevel != this.upgradeLevel)
			{
				this.setMapForUpgradeLevel(this.upgradeLevel);
			}
			this._ApplyRenovations();
			if (this.displayingSpouseRoom == this.HasNpcSpouseOrRoommate())
			{
				string a = this.lastSpouseRoom;
				Farmer owner = this.owner;
				if (!(a != ((owner != null) ? owner.spouse : null)))
				{
					goto IL_53;
				}
			}
			this.showSpouseRoom();
			IL_53:
			this.UpdateChildRoom();
			this.ReadWallpaperAndFloorTileData();
		}

		// Token: 0x06002F3C RID: 12092 RVA: 0x0025100C File Offset: 0x0024F20C
		protected virtual void _ApplyRenovations()
		{
			bool hasOwner = this.HasOwner;
			if (this.upgradeLevel >= 2)
			{
				if (this._appliedMapOverrides.Contains("bedroom_open"))
				{
					this._appliedMapOverrides.Remove("bedroom_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_bedroom_open"))
				{
					base.ApplyMapOverride("FarmHouse_Bedroom_Open", "bedroom_open", null, null);
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_Bedroom_Normal", "bedroom_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("southernroom_open"))
				{
					this._appliedMapOverrides.Remove("southernroom_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_southern_open"))
				{
					base.ApplyMapOverride("FarmHouse_SouthernRoom_Add", "southernroom_open", null, null);
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_SouthernRoom_Remove", "southernroom_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("cornerroom_open"))
				{
					this._appliedMapOverrides.Remove("cornerroom_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_corner_open"))
				{
					base.ApplyMapOverride("FarmHouse_CornerRoom_Add", "cornerroom_open", null, null);
					if (this.displayingSpouseRoom)
					{
						base.setMapTile(49, 19, 229, "Front", "untitled tile sheet", null, true);
					}
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_CornerRoom_Remove", "cornerroom_open", null, null);
					if (this.displayingSpouseRoom)
					{
						base.setMapTile(49, 19, 87, "Front", "untitled tile sheet", null, true);
					}
				}
				if (this._appliedMapOverrides.Contains("diningroom_open"))
				{
					this._appliedMapOverrides.Remove("diningroom_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_dining_open"))
				{
					base.ApplyMapOverride("FarmHouse_DiningRoom_Add", "diningroom_open", null, null);
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_DiningRoom_Remove", "diningroom_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("cubby_open"))
				{
					this._appliedMapOverrides.Remove("cubby_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_cubby_open"))
				{
					base.ApplyMapOverride("FarmHouse_Cubby_Add", "cubby_open", null, null);
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_Cubby_Remove", "cubby_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("farupperroom_open"))
				{
					this._appliedMapOverrides.Remove("farupperroom_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_farupperroom_open"))
				{
					base.ApplyMapOverride("FarmHouse_FarUpperRoom_Add", "farupperroom_open", null, null);
				}
				else
				{
					base.ApplyMapOverride("FarmHouse_FarUpperRoom_Remove", "farupperroom_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("extendedcorner_open"))
				{
					this._appliedMapOverrides.Remove("extendedcorner_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_extendedcorner_open"))
				{
					base.ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Add", "extendedcorner_open", null, null);
				}
				else if (hasOwner && this.owner.mailReceived.Contains("renovation_corner_open"))
				{
					base.ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Remove", "extendedcorner_open", null, null);
				}
				if (this._appliedMapOverrides.Contains("diningroomwall_open"))
				{
					this._appliedMapOverrides.Remove("diningroomwall_open");
				}
				if (hasOwner && this.owner.mailReceived.Contains("renovation_diningroomwall_open"))
				{
					base.ApplyMapOverride("FarmHouse_DiningRoomWall_Add", "diningroomwall_open", null, null);
				}
				else if (hasOwner && this.owner.mailReceived.Contains("renovation_dining_open"))
				{
					base.ApplyMapOverride("FarmHouse_DiningRoomWall_Remove", "diningroomwall_open", null, null);
				}
			}
			string propertyValue;
			if (base.TryGetMapProperty("AdditionalRenovations", out propertyValue))
			{
				string[] array = propertyValue.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					string[] data_split = ArgUtility.SplitBySpace(array[i]);
					if (data_split.Length >= 4)
					{
						string map_patch_id = data_split[0];
						string required_mail = data_split[1];
						string add_map_override = data_split[2];
						string remove_map_override = data_split[3];
						Microsoft.Xna.Framework.Rectangle? destination_rect = null;
						if (data_split.Length >= 8)
						{
							try
							{
								destination_rect = new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle
								{
									X = int.Parse(data_split[4]),
									Y = int.Parse(data_split[5]),
									Width = int.Parse(data_split[6]),
									Height = int.Parse(data_split[7])
								});
							}
							catch (Exception)
							{
								destination_rect = null;
							}
						}
						if (this._appliedMapOverrides.Contains(map_patch_id))
						{
							this._appliedMapOverrides.Remove(map_patch_id);
						}
						if (hasOwner && this.owner.mailReceived.Contains(required_mail))
						{
							base.ApplyMapOverride(add_map_override, map_patch_id, null, destination_rect);
						}
						else
						{
							base.ApplyMapOverride(remove_map_override, map_patch_id, null, destination_rect);
						}
					}
				}
			}
		}

		// Token: 0x06002F3D RID: 12093 RVA: 0x002515FC File Offset: 0x0024F7FC
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			this.updateFarmLayout();
			this.setWallpapers();
			this.setFloors();
			if (this.HasNpcSpouseOrRoommate("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				Point frog_spot = this.GetSpouseRoomCorner();
				frog_spot.X++;
				frog_spot.Y += 6;
				Vector2 spot = Utility.PointToVector2(frog_spot);
				base.removeTile((int)spot.X, (int)spot.Y - 1, "Front");
				base.removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
				base.removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
			}
		}

		// Token: 0x06002F3E RID: 12094 RVA: 0x002516C4 File Offset: 0x0024F8C4
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (this.HasNpcSpouseOrRoommate("Emily") && Game1.player.eventsSeen.Contains("463391"))
			{
				Vector2 parrotSpot = new Vector2(2064f, 160f);
				int upgradeLevel = this.upgradeLevel;
				if (upgradeLevel - 2 <= 1)
				{
					parrotSpot = new Vector2(3408f, 1376f);
				}
				this.temporarySprites.Add(new EmilysParrot(parrotSpot));
			}
			if (Game1.player.currentLocation == null || (!Game1.player.currentLocation.Equals(this) && !Game1.player.currentLocation.name.Value.StartsWith("Cellar")))
			{
				Game1.player.Position = Utility.PointToVector2(this.getEntryLocation()) * 64f;
				Game1.xLocationAfterWarp = Game1.player.TilePoint.X;
				Game1.yLocationAfterWarp = Game1.player.TilePoint.Y;
				Game1.player.currentLocation = this;
			}
			foreach (NPC i in this.characters)
			{
				Child child = i as Child;
				if (child != null)
				{
					child.resetForPlayerEntry(this);
				}
				if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && !(i is Pet))
				{
					i.controller = null;
					i.Halt();
				}
			}
			if (this.IsOwnedByCurrentPlayer && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID) != null && Game1.player.team.IsMarried(Game1.player.UniqueMultiplayerID) && !Game1.player.mailReceived.Contains("CF_Spouse"))
			{
				Vector2 chestPosition = Utility.PointToVector2(this.getEntryLocation()) + new Vector2(0f, -1f);
				Chest chest = new Chest(new List<Item>
				{
					ItemRegistry.Create("(O)434", 1, 0, false)
				}, chestPosition, true, 1, false);
				this.overlayObjects[chestPosition] = chest;
			}
			if (this.IsOwnedByCurrentPlayer && !Game1.player.activeDialogueEvents.ContainsKey("pennyRedecorating"))
			{
				int whichQuilt = -1;
				if (Game1.player.mailReceived.Contains("pennyQuilt0"))
				{
					whichQuilt = 0;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt1"))
				{
					whichQuilt = 1;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt2"))
				{
					whichQuilt = 2;
				}
				if (whichQuilt != -1 && !Game1.player.mailReceived.Contains("pennyRefurbished"))
				{
					List<Object> objectsPickedUp = new List<Object>();
					foreach (Furniture furniture in this.furniture)
					{
						BedFurniture bed_furniture = furniture as BedFurniture;
						if (bed_furniture != null && bed_furniture.bedType == BedFurniture.BedType.Double)
						{
							string bedId = null;
							if (this.owner.mailReceived.Contains("pennyQuilt0"))
							{
								bedId = "2058";
							}
							if (this.owner.mailReceived.Contains("pennyQuilt1"))
							{
								bedId = "2064";
							}
							if (this.owner.mailReceived.Contains("pennyQuilt2"))
							{
								bedId = "2070";
							}
							if (bedId != null)
							{
								Vector2 tile_location = bed_furniture.TileLocation;
								bed_furniture.performRemoveAction();
								objectsPickedUp.Add(bed_furniture);
								Guid guid = this.furniture.GuidOf(bed_furniture);
								this.furniture.Remove(guid);
								this.furniture.Add(new BedFurniture(bedId, new Vector2(tile_location.X, tile_location.Y)));
								break;
							}
							break;
						}
					}
					Game1.player.mailReceived.Add("pennyRefurbished");
					Microsoft.Xna.Framework.Rectangle roomToRedecorate = (this.upgradeLevel >= 2) ? new Microsoft.Xna.Framework.Rectangle(38, 20, 11, 13) : new Microsoft.Xna.Framework.Rectangle(20, 1, 8, 10);
					for (int x = roomToRedecorate.X; x <= roomToRedecorate.Right; x++)
					{
						for (int y = roomToRedecorate.Y; y <= roomToRedecorate.Bottom; y++)
						{
							if (base.getObjectAtTile(x, y, false) != null)
							{
								Object o = base.getObjectAtTile(x, y, false);
								if (o != null && !(o is Chest) && !(o is StorageFurniture) && !(o is IndoorPot) && !(o is BedFurniture))
								{
									if (o.heldObject.Value != null)
									{
										Furniture furniture2 = o as Furniture;
										bool? flag = (furniture2 != null) ? new bool?(furniture2.IsTable()) : null;
										if (flag != null && flag.GetValueOrDefault())
										{
											Object held_object = o.heldObject.Value;
											o.heldObject.Value = null;
											objectsPickedUp.Add(held_object);
										}
									}
									o.performRemoveAction();
									Fence fence = o as Fence;
									if (fence != null)
									{
										o = new Object(fence.ItemId, 1, false, -1, 0);
									}
									objectsPickedUp.Add(o);
									this.objects.Remove(new Vector2((float)x, (float)y));
									Furniture curFurniture = o as Furniture;
									if (curFurniture != null)
									{
										this.furniture.Remove(curFurniture);
									}
								}
							}
						}
					}
					this.decoratePennyRoom(whichQuilt, objectsPickedUp);
				}
			}
			if (this.HasNpcSpouseOrRoommate("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				Point frog_spot = this.GetSpouseRoomCorner();
				frog_spot.X++;
				frog_spot.Y += 6;
				Vector2 spot = Utility.PointToVector2(frog_spot);
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(641f, 1534f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2(0f, -5f) * 4f,
					scale = 4f,
					layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
				});
				if (Game1.random.NextDouble() < 0.85)
				{
					Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					base.TemporarySprites.Add(new SebsFrogs
					{
						texture = crittersText2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(64f, 224f),
						interval = 100f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2((float)Game1.random.Choose(22, 25), (float)Game1.random.Choose(2, 1)) * 4f,
						scale = 4f,
						flipped = Game1.random.NextBool(),
						layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
						Parent = this
					});
				}
				if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextBool())
				{
					Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					base.TemporarySprites.Add(new SebsFrogs
					{
						texture = crittersText3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(64f, 240f),
						interval = 150f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2(8f, 3f) * 4f,
						scale = 4f,
						layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
						flipped = Game1.random.NextBool(),
						pingPong = false,
						Parent = this
					});
					if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
					{
						DelayedAction.playSoundAfterDelay("croak", 1000, null, null, -1, false);
					}
				}
			}
		}

		// Token: 0x06002F3F RID: 12095 RVA: 0x00251FD8 File Offset: 0x002501D8
		private void addFurnitureIfSpaceIsFreePenny(List<Object> objectsToStoreInChests, Furniture f, Furniture heldObject = null)
		{
			bool fail = false;
			foreach (Furniture furniture in this.furniture)
			{
				if (f.GetBoundingBox().Intersects(furniture.GetBoundingBox()))
				{
					fail = true;
					break;
				}
			}
			if (this.objects.ContainsKey(f.TileLocation))
			{
				fail = true;
			}
			if (!fail)
			{
				this.furniture.Add(f);
				if (heldObject != null)
				{
					f.heldObject.Value = heldObject;
					return;
				}
			}
			else
			{
				objectsToStoreInChests.Add(f);
				if (heldObject != null)
				{
					objectsToStoreInChests.Add(heldObject);
				}
			}
		}

		// Token: 0x06002F40 RID: 12096 RVA: 0x00252088 File Offset: 0x00250288
		private void decoratePennyRoom(int whichStyle, List<Object> objectsToStoreInChests)
		{
			List<Chest> chests = new List<Chest>();
			List<Vector2> chest_positions = new List<Vector2>();
			Color chest_color = default(Color);
			switch (whichStyle)
			{
			case 0:
				if (this.upgradeLevel == 1)
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(20, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914", 1, 0, false).SetPlacement(21, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915", 1, 0, false).SetPlacement(22, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914", 1, 0, false).SetPlacement(23, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(24, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1682", 1, 0, false).SetPlacement(26, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747", 1, 0, false).SetPlacement(25, 4, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395", 1, 0, false).SetPlacement(26, 4, 0), ItemRegistry.Create<Furniture>("(F)1363", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443", 1, 0, false).SetPlacement(27, 4, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664", 1, 0, false).SetPlacement(27, 5, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978", 1, 0, false).SetPlacement(21, 6, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124", 1, 0, false).SetPlacement(26, 9, 0), ItemRegistry.Create<Furniture>("(F)1368", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6", 1, 0, false).SetPlacement(25, 10, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296", 1, 0, false).SetPlacement(28, 10, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747", 1, 0, false).SetPlacement(24, 10, 0), null);
					base.SetWallpaper("107", "Bedroom");
					base.SetFloor("2", "Bedroom");
					chest_color = new Color(85, 85, 255);
					chest_positions.Add(new Vector2(21f, 10f));
					chest_positions.Add(new Vector2(22f, 10f));
				}
				else
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(38, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914", 1, 0, false).SetPlacement(39, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1604", 1, 0, false).SetPlacement(41, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915", 1, 0, false).SetPlacement(43, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(45, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914", 1, 0, false).SetPlacement(47, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(48, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443", 1, 0, false).SetPlacement(38, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747", 1, 0, false).SetPlacement(39, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395", 1, 0, false).SetPlacement(40, 23, 0), ItemRegistry.Create<Furniture>("(F)1363", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)714", 1, 0, false).SetPlacement(46, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443", 1, 0, false).SetPlacement(48, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978", 1, 0, false).SetPlacement(42, 25, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664", 1, 0, false).SetPlacement(47, 25, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664", 1, 0, false).SetPlacement(38, 27, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124", 1, 0, false).SetPlacement(46, 31, 0), ItemRegistry.Create<Furniture>("(F)1368", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)416", 1, 0, false).SetPlacement(40, 32, 2), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296", 1, 0, false).SetPlacement(38, 32, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6", 1, 0, false).SetPlacement(45, 32, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296", 1, 0, false).SetPlacement(48, 32, 0), null);
					base.SetWallpaper("107", "Bedroom");
					base.SetFloor("2", "Bedroom");
					chest_color = new Color(85, 85, 255);
					chest_positions.Add(new Vector2(38f, 24f));
					chest_positions.Add(new Vector2(39f, 24f));
				}
				break;
			case 1:
				if (this.upgradeLevel == 1)
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678", 1, 0, false).SetPlacement(20, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(21, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(22, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(23, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907", 1, 0, false).SetPlacement(24, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400", 1, 0, false).SetPlacement(25, 4, 0), ItemRegistry.Create<Furniture>("(F)1365", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866", 1, 0, false).SetPlacement(26, 4, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909", 1, 0, false).SetPlacement(27, 6, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451", 1, 0, false).SetPlacement(21, 6, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138", 1, 0, false).SetPlacement(27, 9, 0), ItemRegistry.Create<Furniture>("(F)1378", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)12", 1, 0, false).SetPlacement(26, 10, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758", 1, 0, false).SetPlacement(24, 10, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618", 1, 0, false).SetPlacement(21, 9, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390", 1, 0, false).SetPlacement(22, 10, 0), null);
					base.SetWallpaper("84", "Bedroom");
					base.SetFloor("35", "Bedroom");
					chest_color = new Color(255, 85, 85);
					chest_positions.Add(new Vector2(21f, 10f));
					chest_positions.Add(new Vector2(23f, 10f));
				}
				else
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678", 1, 0, false).SetPlacement(39, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907", 1, 0, false).SetPlacement(40, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(42, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(43, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814", 1, 0, false).SetPlacement(44, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907", 1, 0, false).SetPlacement(45, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916", 1, 0, false).SetPlacement(48, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758", 1, 0, false).SetPlacement(38, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400", 1, 0, false).SetPlacement(40, 23, 0), ItemRegistry.Create<Furniture>("(F)1365", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390", 1, 0, false).SetPlacement(46, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866", 1, 0, false).SetPlacement(47, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1387", 1, 0, false).SetPlacement(38, 24, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909", 1, 0, false).SetPlacement(47, 24, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)719", 1, 0, false).SetPlacement(38, 25, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451", 1, 0, false).SetPlacement(42, 25, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909", 1, 0, false).SetPlacement(38, 27, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1389", 1, 0, false).SetPlacement(47, 29, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1377", 1, 0, false).SetPlacement(48, 29, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758", 1, 0, false).SetPlacement(41, 30, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)424", 1, 0, false).SetPlacement(42, 30, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618", 1, 0, false).SetPlacement(44, 30, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)536", 1, 0, false).SetPlacement(47, 30, 3), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138", 1, 0, false).SetPlacement(38, 31, 0), ItemRegistry.Create<Furniture>("(F)1378", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1383", 1, 0, false).SetPlacement(41, 31, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1449", 1, 0, false).SetPlacement(48, 32, 0), null);
					base.SetWallpaper("84", "Bedroom");
					base.SetFloor("35", "Bedroom");
					chest_color = new Color(255, 85, 85);
					chest_positions.Add(new Vector2(39f, 23f));
					chest_positions.Add(new Vector2(43f, 25f));
				}
				break;
			case 2:
				if (this.upgradeLevel == 1)
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673", 1, 0, false).SetPlacement(20, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547", 1, 0, false).SetPlacement(21, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675", 1, 0, false).SetPlacement(24, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900", 1, 0, false).SetPlacement(25, 1, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393", 1, 0, false).SetPlacement(25, 4, 0), ItemRegistry.Create<Furniture>("(F)1367", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798", 1, 0, false).SetPlacement(26, 4, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902", 1, 0, false).SetPlacement(25, 5, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751", 1, 0, false).SetPlacement(22, 6, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122", 1, 0, false).SetPlacement(26, 9, 0), ItemRegistry.Create<Furniture>("(F)1378", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197", 1, 0, false).SetPlacement(28, 9, 3), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3", 1, 0, false).SetPlacement(25, 10, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(20, 10, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(24, 10, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964", 1, 0, false).SetPlacement(21, 8, 0), null);
					base.SetWallpaper("95", "Bedroom");
					base.SetFloor("1", "Bedroom");
					chest_color = new Color(85, 85, 85);
					chest_positions.Add(new Vector2(22f, 10f));
					chest_positions.Add(new Vector2(23f, 10f));
				}
				else
				{
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673", 1, 0, false).SetPlacement(38, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675", 1, 0, false).SetPlacement(40, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547", 1, 0, false).SetPlacement(42, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900", 1, 0, false).SetPlacement(45, 20, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751", 1, 0, false).SetPlacement(38, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393", 1, 0, false).SetPlacement(40, 23, 0), ItemRegistry.Create<Furniture>("(F)1367", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798", 1, 0, false).SetPlacement(47, 23, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902", 1, 0, false).SetPlacement(46, 24, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964", 1, 0, false).SetPlacement(42, 25, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(38, 26, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3", 1, 0, false).SetPlacement(46, 29, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(38, 30, 0), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122", 1, 0, false).SetPlacement(46, 30, 0), ItemRegistry.Create<Furniture>("(F)1369", 1, 0, false));
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197", 1, 0, false).SetPlacement(48, 30, 3), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)709", 1, 0, false).SetPlacement(38, 31, 1), null);
					this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3", 1, 0, false).SetPlacement(47, 32, 2), null);
					base.SetWallpaper("95", "Bedroom");
					base.SetFloor("1", "Bedroom");
					chest_color = new Color(85, 85, 85);
					chest_positions.Add(new Vector2(39f, 23f));
					chest_positions.Add(new Vector2(46f, 23f));
				}
				break;
			}
			if (objectsToStoreInChests != null)
			{
				foreach (Object o in objectsToStoreInChests)
				{
					if (chests.Count == 0)
					{
						chests.Add(new Chest(true, "130"));
					}
					bool found_chest_to_stash_in = false;
					using (List<Chest>.Enumerator enumerator2 = chests.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current.addItem(o) == null)
							{
								found_chest_to_stash_in = true;
							}
						}
					}
					if (!found_chest_to_stash_in)
					{
						Chest new_chest = new Chest(true, "130");
						chests.Add(new_chest);
						new_chest.addItem(o);
					}
				}
			}
			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				chest.playerChoiceColor.Value = chest_color;
				Vector2 chest_position = chest_positions[Math.Min(i, chest_positions.Count - 1)];
				this.PlaceInNearbySpace(chest_position, chest);
			}
		}

		// Token: 0x06002F41 RID: 12097 RVA: 0x00253130 File Offset: 0x00251330
		public void PlaceInNearbySpace(Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int attempts = 0;
			Queue<Vector2> open_list = new Queue<Vector2>();
			HashSet<Vector2> closed_list = new HashSet<Vector2>();
			open_list.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			while (attempts < 100)
			{
				current = open_list.Dequeue();
				if (this.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					break;
				}
				closed_list.Add(current);
				foreach (Vector2 v in Utility.getAdjacentTileLocations(current))
				{
					if (!closed_list.Contains(v))
					{
						open_list.Enqueue(v);
					}
				}
				attempts++;
			}
			if (!current.Equals(Vector2.Zero) && this.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
			{
				o.TileLocation = current;
				this.objects.Add(current, o);
			}
		}

		// Token: 0x06002F42 RID: 12098 RVA: 0x00253228 File Offset: 0x00251428
		public virtual void RefreshFloorObjectNeighbors()
		{
			foreach (Vector2 key in this.terrainFeatures.Keys)
			{
				Flooring flooring = this.terrainFeatures[key] as Flooring;
				if (flooring != null)
				{
					flooring.OnAdded(this, key);
				}
			}
		}

		// Token: 0x06002F43 RID: 12099 RVA: 0x0025329C File Offset: 0x0025149C
		public void moveObjectsForHouseUpgrade(int whichUpgrade)
		{
			this.previousUpgradeLevel = this.upgradeLevel;
			this.overlayObjects.Clear();
			switch (whichUpgrade)
			{
			case 0:
				if (this.upgradeLevel == 1)
				{
					this.shiftContents(-6, 0, null);
					return;
				}
				break;
			case 1:
			{
				int upgradeLevel = this.upgradeLevel;
				if (upgradeLevel == 0)
				{
					this.shiftContents(6, 0, null);
					return;
				}
				if (upgradeLevel != 2)
				{
					return;
				}
				this.shiftContents(-3, 0, null);
				return;
			}
			case 2:
			case 3:
			{
				int upgradeLevel = this.upgradeLevel;
				if (upgradeLevel != 0)
				{
					if (upgradeLevel == 1)
					{
						this.shiftContents(18, 19, null);
						foreach (Furniture v in this.furniture)
						{
							if (v.tileLocation.X >= 25f && v.tileLocation.X <= 28f && v.tileLocation.Y >= 20f && v.tileLocation.Y <= 21f)
							{
								v.TileLocation = new Vector2(v.tileLocation.X - 3f, v.tileLocation.Y - 9f);
							}
						}
						base.moveFurniture(42, 23, 16, 14);
						base.moveFurniture(43, 23, 17, 14);
						base.moveFurniture(44, 23, 18, 14);
						base.moveFurniture(43, 24, 22, 14);
						base.moveFurniture(44, 24, 23, 14);
						base.moveFurniture(42, 24, 19, 14);
						base.moveFurniture(43, 25, 20, 14);
						base.moveFurniture(44, 26, 21, 14);
						return;
					}
				}
				else
				{
					this.shiftContents(24, 19, null);
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06002F44 RID: 12100 RVA: 0x00253464 File Offset: 0x00251664
		protected override LocalizedContentManager getMapLoader()
		{
			if (this.mapLoader == null)
			{
				this.mapLoader = Game1.game1.xTileContent.CreateTemporary();
			}
			return this.mapLoader;
		}

		// Token: 0x06002F45 RID: 12101 RVA: 0x0025348C File Offset: 0x0025168C
		protected override void _updateAmbientLighting()
		{
			if (Game1.isStartingToGetDarkOut(this) || this.lightLevel.Value > 0f)
			{
				int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + base.ExtraMillisecondsPerInGameMinute);
				float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
				Game1.ambientLight = new Color((int)((byte)Utility.Lerp((float)(Game1.isRaining ? this.rainLightingColor.R : 0), (float)this.nightLightingColor.R, lerp)), (int)((byte)Utility.Lerp((float)(Game1.isRaining ? this.rainLightingColor.G : 0), (float)this.nightLightingColor.G, lerp)), (int)((byte)Utility.Lerp(0f, (float)this.nightLightingColor.B, lerp)));
				return;
			}
			Game1.ambientLight = (Game1.isRaining ? this.rainLightingColor : Color.White);
		}

		// Token: 0x06002F46 RID: 12102 RVA: 0x00253588 File Offset: 0x00251788
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			base.drawAboveFrontLayer(b);
			if (this.fridge.Value.mutex.IsLocked())
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)this.fridgePosition.X, (float)(this.fridgePosition.Y - 1)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.fridgePosition.Y + 1) * 64 + 1) / 10000f);
			}
		}

		// Token: 0x06002F47 RID: 12103 RVA: 0x00253638 File Offset: 0x00251838
		public override void updateMap()
		{
			bool showSpouse = this.HasNpcSpouseOrRoommate();
			this.mapPath.Value = "Maps\\FarmHouse" + ((this.upgradeLevel == 0) ? "" : ((this.upgradeLevel == 3) ? "2" : (this.upgradeLevel.ToString() ?? ""))) + (showSpouse ? "_marriage" : "");
			base.updateMap();
		}

		// Token: 0x06002F48 RID: 12104 RVA: 0x002536AC File Offset: 0x002518AC
		public virtual void setMapForUpgradeLevel(int level)
		{
			this.upgradeLevel = level;
			int previous_synchronized_displayed_level = this.synchronizedDisplayedLevel.Value;
			this.currentlyDisplayedUpgradeLevel = level;
			this.synchronizedDisplayedLevel.Value = level;
			bool showSpouse = this.HasNpcSpouseOrRoommate();
			if (this.displayingSpouseRoom && !showSpouse)
			{
				this.displayingSpouseRoom = false;
			}
			this.updateMap();
			this.RefreshFloorObjectNeighbors();
			if (showSpouse)
			{
				this.showSpouseRoom();
			}
			base.loadObjects();
			if (level == 3)
			{
				this.AddCellarTiles();
				this.createCellarWarps();
				Game1.player.craftingRecipes.TryAdd("Cask", 0);
			}
			bool need_bed_upgrade = this.previousUpgradeLevel == 0 && this.upgradeLevel >= 0;
			if (this.previousUpgradeLevel >= 0)
			{
				if (this.previousUpgradeLevel < 2 && this.upgradeLevel >= 2)
				{
					for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
					{
						for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
						{
							if (this.doesTileHaveProperty(x, y, "DefaultChildBedPosition", "Back", false) != null)
							{
								string bedId = BedFurniture.CHILD_BED_INDEX;
								this.furniture.Add(new BedFurniture(bedId, new Vector2((float)x, (float)y)));
								break;
							}
						}
					}
				}
				Furniture bed_furniture = null;
				if (this.previousUpgradeLevel == 0)
				{
					using (List<Furniture>.Enumerator enumerator = this.furniture.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Furniture furniture = enumerator.Current;
							BedFurniture bed = furniture as BedFurniture;
							if (bed != null && bed.bedType == BedFurniture.BedType.Single)
							{
								bed_furniture = bed;
								break;
							}
						}
						goto IL_1E1;
					}
				}
				foreach (Furniture furniture2 in this.furniture)
				{
					BedFurniture bed2 = furniture2 as BedFurniture;
					if (bed2 != null && bed2.bedType == BedFurniture.BedType.Double)
					{
						bed_furniture = bed2;
						break;
					}
				}
				IL_1E1:
				if (this.upgradeLevel != 3 || need_bed_upgrade)
				{
					for (int x2 = 0; x2 < this.map.Layers[0].LayerWidth; x2++)
					{
						int y2 = 0;
						while (y2 < this.map.Layers[0].LayerHeight)
						{
							if (this.doesTileHaveProperty(x2, y2, "DefaultBedPosition", "Back", false) != null)
							{
								string bedId2 = BedFurniture.DEFAULT_BED_INDEX;
								if (this.previousUpgradeLevel == 1 && bed_furniture != null && (bed_furniture.tileLocation.X != 39f || bed_furniture.tileLocation.Y != 22f))
								{
									break;
								}
								if (bed_furniture != null)
								{
									bedId2 = bed_furniture.ItemId;
								}
								if (this.previousUpgradeLevel == 0 && bed_furniture != null)
								{
									bed_furniture.performRemoveAction();
									Guid guid = this.furniture.GuidOf(bed_furniture);
									this.furniture.Remove(guid);
									bedId2 = Utility.GetDoubleWideVersionOfBed(bedId2);
									this.furniture.Add(new BedFurniture(bedId2, new Vector2((float)x2, (float)y2)));
									break;
								}
								if (bed_furniture != null)
								{
									bed_furniture.performRemoveAction();
									Guid guid2 = this.furniture.GuidOf(bed_furniture);
									this.furniture.Remove(guid2);
									this.furniture.Add(new BedFurniture(bed_furniture.ItemId, new Vector2((float)x2, (float)y2)));
									break;
								}
								break;
							}
							else
							{
								y2++;
							}
						}
					}
				}
				this.previousUpgradeLevel = -1;
			}
			if (previous_synchronized_displayed_level != level)
			{
				this.lightGlows.Clear();
			}
			this.fridgePosition = (this.GetFridgePositionFromMap() ?? Point.Zero);
		}

		// Token: 0x06002F49 RID: 12105 RVA: 0x00253A54 File Offset: 0x00251C54
		public Point? GetFridgePositionFromMap()
		{
			Layer layer = this.map.RequireLayer("Buildings");
			for (int y = 0; y < layer.LayerHeight; y++)
			{
				for (int x = 0; x < layer.LayerWidth; x++)
				{
					if (layer.GetTileIndexAt(x, y, "untitled tile sheet") == 173)
					{
						return new Point?(new Point(x, y));
					}
				}
			}
			return null;
		}

		// Token: 0x06002F4A RID: 12106 RVA: 0x00253ABE File Offset: 0x00251CBE
		public void createCellarWarps()
		{
			this.updateCellarWarps();
		}

		// Token: 0x06002F4B RID: 12107 RVA: 0x00253AC8 File Offset: 0x00251CC8
		public void updateCellarWarps()
		{
			Layer back_layer = this.map.RequireLayer("Back");
			string cellarName = this.GetCellarName();
			if (cellarName == null)
			{
				return;
			}
			for (int x = 0; x < back_layer.LayerWidth; x++)
			{
				for (int y = 0; y < back_layer.LayerHeight; y++)
				{
					string[] touchAction = base.GetTilePropertySplitBySpaces("TouchAction", "Back", x, y);
					if (ArgUtility.Get(touchAction, 0, null, true) == "Warp" && ArgUtility.Get(touchAction, 1, "", true).StartsWith("Cellar"))
					{
						touchAction[1] = cellarName;
						base.setTileProperty(x, y, "Back", "TouchAction", string.Join(" ", touchAction));
					}
				}
			}
			if (this.cellarWarps == null)
			{
				return;
			}
			foreach (Warp warp in this.cellarWarps)
			{
				if (!this.warps.Contains(warp))
				{
					this.warps.Add(warp);
				}
				warp.TargetName = cellarName;
			}
		}

		// Token: 0x06002F4C RID: 12108 RVA: 0x00253BEC File Offset: 0x00251DEC
		public virtual Point GetSpouseRoomCorner()
		{
			Point position;
			if (base.TryGetMapPropertyAs("SpouseRoomPosition", out position, false))
			{
				return position;
			}
			if (this.upgradeLevel != 1)
			{
				return new Point(50, 20);
			}
			return new Point(29, 1);
		}

		// Token: 0x06002F4D RID: 12109 RVA: 0x00253C28 File Offset: 0x00251E28
		public virtual void loadSpouseRoom()
		{
			Farmer owner = this.owner;
			string text = (((owner != null) ? owner.spouse : null) != null && this.owner.isMarriedOrRoommates()) ? this.owner.spouse : null;
			CharacterData spouseData;
			CharacterSpouseRoomData roomData = NPC.TryGetData(text, out spouseData) ? ((spouseData != null) ? spouseData.SpouseRoom : null) : null;
			this.spouseRoomSpot = this.GetSpouseRoomCorner();
			this.spouseRoomSpot.X = this.spouseRoomSpot.X + 3;
			this.spouseRoomSpot.Y = this.spouseRoomSpot.Y + 4;
			if (text != null)
			{
				string assetName = ((roomData != null) ? roomData.MapAsset : null) ?? "spouseRooms";
				Microsoft.Xna.Framework.Rectangle sourceArea = (roomData != null) ? roomData.MapSourceRect : CharacterSpouseRoomData.DefaultMapSourceRect;
				Point corner = this.GetSpouseRoomCorner();
				Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
				Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + assetName);
				Point fromOrigin = sourceArea.Location;
				this.map.Properties.Remove("Light");
				this.map.Properties.Remove("DayTiles");
				this.map.Properties.Remove("NightTiles");
				List<KeyValuePair<Point, Tile>> bottom_row_tiles = new List<KeyValuePair<Point, Tile>>();
				Layer front_layer = this.map.RequireLayer("Front");
				for (int x = areaToRefurbish.Left; x < areaToRefurbish.Right; x++)
				{
					Point point = new Point(x, areaToRefurbish.Bottom - 1);
					Tile tile = front_layer.Tiles[point.X, point.Y];
					if (tile != null)
					{
						bottom_row_tiles.Add(new KeyValuePair<Point, Tile>(point, tile));
					}
				}
				if (this._appliedMapOverrides.Contains("spouse_room"))
				{
					this._appliedMapOverrides.Remove("spouse_room");
				}
				base.ApplyMapOverride(assetName, "spouse_room", new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height)), new Microsoft.Xna.Framework.Rectangle?(areaToRefurbish));
				Layer refurbishedBuildingsLayer = refurbishedMap.RequireLayer("Buildings");
				Layer refurbishedFrontLayer = refurbishedMap.RequireLayer("Front");
				for (int x2 = 0; x2 < areaToRefurbish.Width; x2++)
				{
					for (int y = 0; y < areaToRefurbish.Height; y++)
					{
						int tileIndex = refurbishedBuildingsLayer.GetTileIndexAt(fromOrigin.X + x2, fromOrigin.Y + y, null);
						if (tileIndex != -1)
						{
							base.adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + x2, areaToRefurbish.Y + y, "Buildings");
						}
						if (y < areaToRefurbish.Height - 1)
						{
							tileIndex = refurbishedFrontLayer.GetTileIndexAt(fromOrigin.X + x2, fromOrigin.Y + y, null);
							if (tileIndex != -1)
							{
								base.adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + x2, areaToRefurbish.Y + y, "Front");
							}
						}
					}
				}
				foreach (Point tile2 in areaToRefurbish.GetPoints())
				{
					if (base.getTileIndexAt(tile2, "Paths", null) == 7)
					{
						this.spouseRoomSpot = tile2;
						break;
					}
				}
				Point spouse_room_spot = this.GetSpouseRoomSpot();
				base.setTileProperty(spouse_room_spot.X, spouse_room_spot.Y, "Back", "NoFurniture", "T");
				foreach (KeyValuePair<Point, Tile> kvp in bottom_row_tiles)
				{
					front_layer.Tiles[kvp.Key.X, kvp.Key.Y] = kvp.Value;
				}
			}
		}

		// Token: 0x06002F4E RID: 12110 RVA: 0x0025400C File Offset: 0x0025220C
		public virtual Microsoft.Xna.Framework.Rectangle? GetCribBounds()
		{
			if (this.upgradeLevel < 2)
			{
				return null;
			}
			return new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(30, 12, 3, 4));
		}

		// Token: 0x06002F4F RID: 12111 RVA: 0x0025403C File Offset: 0x0025223C
		public virtual void UpdateChildRoom()
		{
			Microsoft.Xna.Framework.Rectangle? crib_location = this.GetCribBounds();
			if (crib_location != null)
			{
				if (this._appliedMapOverrides.Contains("crib"))
				{
					this._appliedMapOverrides.Remove("crib");
				}
				base.ApplyMapOverride("FarmHouse_Crib_" + this.cribStyle.Value.ToString(), "crib", null, crib_location);
			}
		}

		// Token: 0x06002F50 RID: 12112 RVA: 0x002540AE File Offset: 0x002522AE
		public void playerDivorced()
		{
			this.displayingSpouseRoom = false;
		}

		// Token: 0x06002F51 RID: 12113 RVA: 0x002540B8 File Offset: 0x002522B8
		public virtual List<Microsoft.Xna.Framework.Rectangle> getForbiddenPetWarpTiles()
		{
			List<Microsoft.Xna.Framework.Rectangle> forbidden_tiles = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (this.upgradeLevel)
			{
			case 0:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(2, 8, 3, 4));
				break;
			case 1:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(8, 8, 3, 4));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(17, 8, 4, 3));
				break;
			case 2:
			case 3:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 27, 3, 4));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(35, 27, 4, 3));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(27, 15, 4, 3));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 17, 2, 6));
				break;
			}
			return forbidden_tiles;
		}

		// Token: 0x06002F52 RID: 12114 RVA: 0x00254164 File Offset: 0x00252364
		public bool canPetWarpHere(Vector2 tile_position)
		{
			foreach (Microsoft.Xna.Framework.Rectangle rect in this.getForbiddenPetWarpTiles())
			{
				if (rect.Contains((int)tile_position.X, (int)tile_position.Y))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002F53 RID: 12115 RVA: 0x002541D0 File Offset: 0x002523D0
		public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			List<Microsoft.Xna.Framework.Rectangle> walls = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (this.upgradeLevel)
			{
			case 0:
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
				break;
			case 1:
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 17, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(18, 6, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(20, 1, 9, 3));
				break;
			case 2:
			case 3:
			{
				bool hasOwner = this.HasOwner;
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 12, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 3, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 10, 10, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 10, 8, 3));
				int bedroomWidthReduction = (hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open")) ? -3 : 0;
				if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_bedroom_open"))
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 0, 2));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 10, 13 + bedroomWidthReduction, 3));
				}
				else
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 2, 2));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 10, 11 + bedroomWidthReduction, 3));
				}
				if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_southern_open"))
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 24, 3, 3));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(31, 24, 3, 3));
				}
				else
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open"))
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(30, 1, 9, 3));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(28, 3, 2, 2));
				}
				else
				{
					walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				foreach (Microsoft.Xna.Framework.Rectangle r in walls)
				{
					r.Offset(15, 10);
				}
				break;
			}
			}
			return walls;
		}

		// Token: 0x06002F54 RID: 12116 RVA: 0x00254418 File Offset: 0x00252618
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			FarmHouse farmhouse = l as FarmHouse;
			if (farmhouse != null)
			{
				this.cribStyle.Value = farmhouse.cribStyle.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06002F55 RID: 12117 RVA: 0x0025444C File Offset: 0x0025264C
		public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			List<Microsoft.Xna.Framework.Rectangle> floors = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (this.upgradeLevel)
			{
			case 0:
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 10, 9));
				break;
			case 1:
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 6, 9));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(7, 3, 11, 9));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(18, 8, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
				break;
			case 2:
			case 3:
			{
				bool hasOwner = this.HasOwner;
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 12, 6));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(13, 5, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 12, 10, 11));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(10, 12, 11, 9));
				if (hasOwner && this.owner.mailReceived.Contains("renovation_bedroom_open"))
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 0, 2));
					floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 12, 14, 11));
				}
				else
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 2, 2));
					floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
				}
				if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_southern_open"))
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 26, 11, 8));
				}
				else
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open"))
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(28, 5, 2, 3));
					floors.Add(new Microsoft.Xna.Framework.Rectangle(30, 3, 9, 6));
				}
				else
				{
					floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				foreach (Microsoft.Xna.Framework.Rectangle r in floors)
				{
					r.Offset(15, 10);
				}
				break;
			}
			}
			return floors;
		}

		// Token: 0x06002F56 RID: 12118 RVA: 0x00254674 File Offset: 0x00252874
		public virtual bool CanModifyCrib()
		{
			if (!this.HasOwner)
			{
				return false;
			}
			if (this.owner.isMarriedOrRoommates() && this.owner.GetSpouseFriendship().DaysUntilBirthing != -1)
			{
				return false;
			}
			using (List<Child>.Enumerator enumerator = this.owner.getChildren().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Age < 3)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x04002020 RID: 8224
		[XmlElement("fridge")]
		public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(true, "130"));

		// Token: 0x04002021 RID: 8225
		[XmlIgnore]
		public readonly NetInt synchronizedDisplayedLevel = new NetInt(-1);

		// Token: 0x04002022 RID: 8226
		public Point fridgePosition = Point.Zero;

		// Token: 0x04002023 RID: 8227
		[XmlIgnore]
		public Point spouseRoomSpot = Point.Zero;

		// Token: 0x04002024 RID: 8228
		private string lastSpouseRoom;

		// Token: 0x04002025 RID: 8229
		[XmlIgnore]
		private LocalizedContentManager mapLoader;

		// Token: 0x04002026 RID: 8230
		public List<Warp> cellarWarps;

		// Token: 0x04002027 RID: 8231
		[XmlElement("cribStyle")]
		public readonly NetInt cribStyle = new NetInt(1)
		{
			InterpolationEnabled = false
		};

		// Token: 0x04002028 RID: 8232
		[XmlIgnore]
		public int previousUpgradeLevel = -1;

		// Token: 0x04002029 RID: 8233
		private int currentlyDisplayedUpgradeLevel;

		// Token: 0x0400202A RID: 8234
		private bool displayingSpouseRoom;

		// Token: 0x0400202B RID: 8235
		private Color nightLightingColor = new Color(180, 180, 0);

		// Token: 0x0400202C RID: 8236
		private Color rainLightingColor = new Color(90, 90, 0);
	}
}
