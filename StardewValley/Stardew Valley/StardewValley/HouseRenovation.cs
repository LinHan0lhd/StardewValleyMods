using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.HomeRenovations;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewValley
{
	// Token: 0x020000B3 RID: 179
	public class HouseRenovation : ISalable, IHaveItemTypeId
	{
		// Token: 0x06000C6D RID: 3181 RVA: 0x0008D47C File Offset: 0x0008B67C
		public bool ShouldDrawIcon()
		{
			return false;
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000C6E RID: 3182 RVA: 0x0008D47F File Offset: 0x0008B67F
		public string TypeDefinitionId
		{
			get
			{
				return "(Salable)";
			}
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x0008D486 File Offset: 0x0008B686
		public string QualifiedItemId
		{
			get
			{
				return this.TypeDefinitionId + "HouseRenovation";
			}
		}

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000C70 RID: 3184 RVA: 0x0008D498 File Offset: 0x0008B698
		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		// Token: 0x06000C71 RID: 3185 RVA: 0x0008D4A0 File Offset: 0x0008B6A0
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
		}

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000C72 RID: 3186 RVA: 0x0008D4A2 File Offset: 0x0008B6A2
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000C73 RID: 3187 RVA: 0x0008D4AA File Offset: 0x0008B6AA
		// (set) Token: 0x06000C74 RID: 3188 RVA: 0x0008D4AD File Offset: 0x0008B6AD
		public bool IsRecipe
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x0008D4AF File Offset: 0x0008B6AF
		public string getDescription()
		{
			return this._description;
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x0008D4B7 File Offset: 0x0008B6B7
		public int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06000C77 RID: 3191 RVA: 0x0008D4BA File Offset: 0x0008B6BA
		public int addToStack(Item stack)
		{
			return 0;
		}

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000C78 RID: 3192 RVA: 0x0008D4BD File Offset: 0x0008B6BD
		// (set) Token: 0x06000C79 RID: 3193 RVA: 0x0008D4C0 File Offset: 0x0008B6C0
		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000C7A RID: 3194 RVA: 0x0008D4C2 File Offset: 0x0008B6C2
		// (set) Token: 0x06000C7B RID: 3195 RVA: 0x0008D4C5 File Offset: 0x0008B6C5
		public int Quality
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x0008D4C7 File Offset: 0x0008B6C7
		public int sellToStorePrice(long specificPlayerID = -1L)
		{
			return -1;
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x0008D4CA File Offset: 0x0008B6CA
		public int salePrice(bool ignoreProfitMargins = false)
		{
			if (this.Price <= 0)
			{
				return 0;
			}
			return this.Price;
		}

		// Token: 0x06000C7E RID: 3198 RVA: 0x0008D4DD File Offset: 0x0008B6DD
		public bool appliesProfitMargins()
		{
			return false;
		}

		// Token: 0x06000C7F RID: 3199 RVA: 0x0008D4E0 File Offset: 0x0008B6E0
		public bool actionWhenPurchased(string shopId)
		{
			return false;
		}

		// Token: 0x06000C80 RID: 3200 RVA: 0x0008D4E3 File Offset: 0x0008B6E3
		public bool canStackWith(ISalable other)
		{
			return false;
		}

		// Token: 0x06000C81 RID: 3201 RVA: 0x0008D4E6 File Offset: 0x0008B6E6
		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x0008D4E9 File Offset: 0x0008B6E9
		public bool IsInfiniteStock()
		{
			return true;
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x0008D4EC File Offset: 0x0008B6EC
		public ISalable GetSalableInstance()
		{
			return this;
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x0008D4EF File Offset: 0x0008B6EF
		public void FixStackSize()
		{
		}

		// Token: 0x06000C85 RID: 3205 RVA: 0x0008D4F1 File Offset: 0x0008B6F1
		public void FixQuality()
		{
		}

		// Token: 0x06000C86 RID: 3206 RVA: 0x0008D4F3 File Offset: 0x0008B6F3
		public string GetItemTypeId()
		{
			return this.TypeDefinitionId;
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x0008D4FB File Offset: 0x0008B6FB
		public static void ShowRenovationMenu()
		{
			Game1.activeClickableMenu = new ShopMenu("HouseRenovations", HouseRenovation.GetAvailableRenovations(), 0, null, new ShopMenu.OnPurchaseDelegate(HouseRenovation.OnPurchaseRenovation), null, true)
			{
				purchaseSound = null
			};
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x0008D528 File Offset: 0x0008B728
		public static List<ISalable> GetAvailableRenovations()
		{
			FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>(Game1.player.homeLocation.Value, false);
			List<ISalable> available_renovations = new List<ISalable>();
			Dictionary<string, HomeRenovation> data = DataLoader.HomeRenovations(Game1.content);
			Action<HouseRenovation, int> <>9__2;
			foreach (string key in data.Keys)
			{
				HomeRenovation renovation_data = data[key];
				bool valid = true;
				foreach (RenovationValue requirement_data in renovation_data.Requirements)
				{
					if (requirement_data.Type == "Value")
					{
						string requirement_value = requirement_data.Value;
						bool match = true;
						if (requirement_value.Length > 0 && requirement_value[0] == '!')
						{
							requirement_value = requirement_value.Substring(1);
							match = false;
						}
						int value = int.Parse(requirement_value);
						try
						{
							NetInt field = (NetInt)farmhouse.GetType().GetField(requirement_data.Key).GetValue(farmhouse);
							if (field == null)
							{
								valid = false;
								break;
							}
							if (field.Value == value != match)
							{
								valid = false;
								break;
							}
							continue;
						}
						catch (Exception)
						{
							valid = false;
							break;
						}
					}
					if (requirement_data.Type == "Mail" && Game1.player.hasOrWillReceiveMail(requirement_data.Key) != (requirement_data.Value == "1"))
					{
						valid = false;
						break;
					}
				}
				if (valid)
				{
					HouseRenovation renovation = new HouseRenovation
					{
						location = farmhouse,
						_name = key
					};
					string[] split = Game1.content.LoadString(renovation_data.TextStrings).Split('/', StringSplitOptions.None);
					try
					{
						renovation._displayName = split[0];
						renovation._description = split[1];
						renovation.placementText = split[2];
					}
					catch (Exception)
					{
						renovation._displayName = "?";
						renovation._description = "?";
						renovation.placementText = "?";
					}
					if (renovation_data.CheckForObstructions)
					{
						HouseRenovation houseRenovation = renovation;
						houseRenovation.validate = (Func<HouseRenovation, int, bool>)Delegate.Combine(houseRenovation.validate, new Func<HouseRenovation, int, bool>(HouseRenovation.EnsureNoObstructions));
					}
					if (renovation_data.AnimationType == "destroy")
					{
						renovation.animationType = HouseRenovation.AnimationType.Destroy;
					}
					else
					{
						renovation.animationType = HouseRenovation.AnimationType.Build;
					}
					renovation.Price = renovation_data.Price;
					renovation.RoomId = ((!string.IsNullOrEmpty(renovation_data.RoomId)) ? renovation_data.RoomId : key);
					if (!string.IsNullOrEmpty(renovation_data.SpecialRect))
					{
						if (renovation_data.SpecialRect == "crib")
						{
							Rectangle? crib_bounds = farmhouse.GetCribBounds();
							if (!farmhouse.CanModifyCrib() || crib_bounds == null)
							{
								continue;
							}
							renovation.AddRenovationBound(crib_bounds.Value);
						}
					}
					else
					{
						foreach (RectGroup rectGroup in renovation_data.RectGroups)
						{
							List<Rectangle> rectangles = new List<Rectangle>();
							foreach (Rect rect in rectGroup.Rects)
							{
								rectangles.Add(new Rectangle
								{
									X = rect.X,
									Y = rect.Y,
									Width = rect.Width,
									Height = rect.Height
								});
							}
							renovation.AddRenovationBound(rectangles);
						}
					}
					using (List<RenovationValue>.Enumerator enumerator2 = renovation_data.RenovateActions.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							HouseRenovation.<>c__DisplayClass47_1 CS$<>8__locals2 = new HouseRenovation.<>c__DisplayClass47_1();
							CS$<>8__locals2.action_data = enumerator2.Current;
							if (CS$<>8__locals2.action_data.Type == "Value")
							{
								try
								{
									HouseRenovation.<>c__DisplayClass47_2 CS$<>8__locals3 = new HouseRenovation.<>c__DisplayClass47_2();
									CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals2;
									CS$<>8__locals3.field = (NetInt)farmhouse.GetType().GetField(CS$<>8__locals3.CS$<>8__locals1.action_data.Key).GetValue(farmhouse);
									if (CS$<>8__locals3.field == null)
									{
										valid = false;
										break;
									}
									HouseRenovation houseRenovation2 = renovation;
									houseRenovation2.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation2.onRenovation, new Action<HouseRenovation, int>(CS$<>8__locals3.<GetAvailableRenovations>g__ActionOnRenovation|1));
									continue;
								}
								catch (Exception)
								{
									valid = false;
									break;
								}
							}
							if (CS$<>8__locals2.action_data.Type == "Mail")
							{
								HouseRenovation houseRenovation3 = renovation;
								houseRenovation3.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation3.onRenovation, new Action<HouseRenovation, int>(CS$<>8__locals2.<GetAvailableRenovations>g__MailOnRenovation|0));
							}
						}
					}
					if (valid)
					{
						HouseRenovation houseRenovation4 = renovation;
						Delegate a2 = houseRenovation4.onRenovation;
						Action<HouseRenovation, int> b2;
						if ((b2 = <>9__2) == null)
						{
							b2 = (<>9__2 = delegate(HouseRenovation a, int b)
							{
								farmhouse.UpdateForRenovation();
							});
						}
						houseRenovation4.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(a2, b2);
						available_renovations.Add(renovation);
					}
				}
			}
			return available_renovations;
		}

		// Token: 0x06000C89 RID: 3209 RVA: 0x0008DB10 File Offset: 0x0008BD10
		public static bool EnsureNoObstructions(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location != null)
			{
				foreach (Rectangle rectangle in renovation.renovationBounds[selected_index])
				{
					foreach (Vector2 tile in rectangle.GetVectors())
					{
						if (renovation.location.isTileOccupiedByFarmer(tile) != null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"), true);
							return false;
						}
						if (renovation.location.IsTileOccupiedBy(tile, CollisionMask.All, CollisionMask.None, false))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"), true);
							return false;
						}
					}
					Rectangle world_box = new Rectangle(rectangle.X * 64, rectangle.Y * 64, rectangle.Width * 64, rectangle.Height * 64);
					DecoratableLocation decoratable_location = renovation.location as DecoratableLocation;
					if (decoratable_location != null)
					{
						using (List<Furniture>.Enumerator enumerator3 = decoratable_location.furniture.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								if (enumerator3.Current.GetBoundingBox().Intersects(world_box))
								{
									Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"), true);
									return false;
								}
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x0008DCD8 File Offset: 0x0008BED8
		public static void BuildCrib(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				farm_house.cribStyle.Value = 1;
			}
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x0008DD00 File Offset: 0x0008BF00
		public static void RemoveCrib(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				farm_house.cribStyle.Value = 0;
			}
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x0008DD28 File Offset: 0x0008BF28
		public static void OpenBedroom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Add("renovation_bedroom_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x0008DD60 File Offset: 0x0008BF60
		public static void CloseBedroom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Remove("renovation_bedroom_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x0008DD98 File Offset: 0x0008BF98
		public static void OpenSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Add("renovation_southern_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x0008DDD0 File Offset: 0x0008BFD0
		public static void CloseSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Remove("renovation_southern_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x0008DE08 File Offset: 0x0008C008
		public static void OpenCornernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Add("renovation_corner_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x0008DE40 File Offset: 0x0008C040
		public static void CloseCornerRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house = renovation.location as FarmHouse;
			if (farm_house != null)
			{
				Game1.player.mailReceived.Remove("renovation_corner_open");
				farm_house.UpdateForRenovation();
			}
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0008DE78 File Offset: 0x0008C078
		public static bool OnPurchaseRenovation(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
		{
			HouseRenovation renovation = salable as HouseRenovation;
			if (renovation != null)
			{
				who._money += salable.salePrice(false);
				Game1.activeClickableMenu = new RenovateMenu(renovation);
				return true;
			}
			return false;
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x0008DEB4 File Offset: 0x0008C0B4
		public virtual void AddRenovationBound(Rectangle bound)
		{
			List<Rectangle> bounds = new List<Rectangle>
			{
				bound
			};
			this.renovationBounds.Add(bounds);
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x0008DEDA File Offset: 0x0008C0DA
		public virtual void AddRenovationBound(List<Rectangle> bounds)
		{
			this.renovationBounds.Add(bounds);
		}

		// Token: 0x04000890 RID: 2192
		protected string _displayName;

		// Token: 0x04000891 RID: 2193
		protected string _name;

		// Token: 0x04000892 RID: 2194
		protected string _description;

		// Token: 0x04000893 RID: 2195
		public HouseRenovation.AnimationType animationType;

		// Token: 0x04000894 RID: 2196
		public List<List<Rectangle>> renovationBounds = new List<List<Rectangle>>();

		// Token: 0x04000895 RID: 2197
		public string placementText = "";

		// Token: 0x04000896 RID: 2198
		public GameLocation location;

		// Token: 0x04000897 RID: 2199
		public bool requireClearance = true;

		// Token: 0x04000898 RID: 2200
		public Action<HouseRenovation, int> onRenovation;

		// Token: 0x04000899 RID: 2201
		public Func<HouseRenovation, int, bool> validate;

		// Token: 0x0400089A RID: 2202
		public int Price;

		// Token: 0x0400089B RID: 2203
		public string RoomId;

		// Token: 0x02000464 RID: 1124
		public enum AnimationType
		{
			// Token: 0x04002820 RID: 10272
			Build,
			// Token: 0x04002821 RID: 10273
			Destroy
		}
	}
}
