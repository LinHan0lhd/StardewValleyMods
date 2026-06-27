using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Fences;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley
{
	// Token: 0x020000EE RID: 238
	[XmlInclude(typeof(BreakableContainer))]
	[XmlInclude(typeof(Cask))]
	[XmlInclude(typeof(Chest))]
	[XmlInclude(typeof(ColoredObject))]
	[XmlInclude(typeof(CrabPot))]
	[XmlInclude(typeof(Fence))]
	[XmlInclude(typeof(Furniture))]
	[XmlInclude(typeof(IndoorPot))]
	[XmlInclude(typeof(ItemPedestal))]
	[XmlInclude(typeof(Mannequin))]
	[XmlInclude(typeof(MiniJukebox))]
	[XmlInclude(typeof(Phone))]
	[XmlInclude(typeof(Sign))]
	[XmlInclude(typeof(Torch))]
	[XmlInclude(typeof(Trinket))]
	[XmlInclude(typeof(Wallpaper))]
	[XmlInclude(typeof(WoodChipper))]
	[XmlInclude(typeof(Workbench))]
	public class Object : Item
	{
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x060012B5 RID: 4789 RVA: 0x000DDDD6 File Offset: 0x000DBFD6
		// (set) Token: 0x060012B6 RID: 4790 RVA: 0x000DDDE3 File Offset: 0x000DBFE3
		public bool destroyOvernight
		{
			get
			{
				return this._destroyOvernight.Value;
			}
			set
			{
				this._destroyOvernight.Value = value;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x060012B7 RID: 4791 RVA: 0x000DDDF1 File Offset: 0x000DBFF1
		// (set) Token: 0x060012B8 RID: 4792 RVA: 0x000DDDFE File Offset: 0x000DBFFE
		[XmlIgnore]
		public LightSource lightSource
		{
			get
			{
				return this.netLightSource.Value;
			}
			set
			{
				this.netLightSource.Value = value;
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x060012B9 RID: 4793 RVA: 0x000DDE0C File Offset: 0x000DC00C
		// (set) Token: 0x060012BA RID: 4794 RVA: 0x000DDE14 File Offset: 0x000DC014
		[XmlIgnore]
		public virtual GameLocation Location { get; set; }

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x060012BB RID: 4795 RVA: 0x000DDE1D File Offset: 0x000DC01D
		// (set) Token: 0x060012BC RID: 4796 RVA: 0x000DDE2A File Offset: 0x000DC02A
		[XmlIgnore]
		public virtual Vector2 TileLocation
		{
			get
			{
				return this.tileLocation.Value;
			}
			set
			{
				if (this.tileLocation.Value != value)
				{
					this.tileLocation.Value = value;
					this.RecalculateBoundingBox();
				}
			}
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x060012BD RID: 4797 RVA: 0x000DDE51 File Offset: 0x000DC051
		// (set) Token: 0x060012BE RID: 4798 RVA: 0x000DDE5E File Offset: 0x000DC05E
		[XmlIgnore]
		public string name
		{
			get
			{
				return this.netName.Value;
			}
			set
			{
				this.netName.Value = value;
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x060012BF RID: 4799 RVA: 0x000DDE6C File Offset: 0x000DC06C
		// (set) Token: 0x060012C0 RID: 4800 RVA: 0x000DDE79 File Offset: 0x000DC079
		[XmlElement("displayNameFormat")]
		public string displayNameFormat
		{
			get
			{
				return this.netDisplayNameFormat.Value;
			}
			set
			{
				this.netDisplayNameFormat.Value = value;
			}
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x060012C1 RID: 4801 RVA: 0x000DDE87 File Offset: 0x000DC087
		public override string TypeDefinitionId
		{
			get
			{
				if (!this.bigCraftable.Value)
				{
					return "(O)";
				}
				return "(BC)";
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x060012C2 RID: 4802 RVA: 0x000DDEA4 File Offset: 0x000DC0A4
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				this.displayName = this.loadDisplayName();
				if (this.orderData.Value == "QI_COOKING")
				{
					this.displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Fresh_Prefix", this.displayName);
				}
				if (this.isRecipe.Value)
				{
					string label = this.displayName;
					string rawCraftingData;
					if (CraftingRecipe.craftingRecipes.TryGetValue(this.displayName, out rawCraftingData))
					{
						string count = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(rawCraftingData.Split('/', StringSplitOptions.None), 2, null, true), 1, null);
						if (count != null)
						{
							label = label + " x" + count;
						}
					}
					return label + Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12657");
				}
				return this.displayName;
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x060012C3 RID: 4803 RVA: 0x000DDF5E File Offset: 0x000DC15E
		// (set) Token: 0x060012C4 RID: 4804 RVA: 0x000DDF84 File Offset: 0x000DC184
		[XmlIgnore]
		public override string Name
		{
			get
			{
				if (!this.isRecipe.Value)
				{
					return this.name;
				}
				return this.name + " Recipe";
			}
			set
			{
				this.name = value;
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x060012C5 RID: 4805 RVA: 0x000DDF8D File Offset: 0x000DC18D
		public override string BaseName
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x060012C6 RID: 4806 RVA: 0x000DDF95 File Offset: 0x000DC195
		// (set) Token: 0x060012C7 RID: 4807 RVA: 0x000DDFA2 File Offset: 0x000DC1A2
		[XmlIgnore]
		public string Type
		{
			get
			{
				return this.type.Value;
			}
			set
			{
				this.type.Value = value;
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060012C8 RID: 4808 RVA: 0x000DDFB0 File Offset: 0x000DC1B0
		// (set) Token: 0x060012C9 RID: 4809 RVA: 0x000DDFBD File Offset: 0x000DC1BD
		[XmlIgnore]
		public bool CanBeSetDown
		{
			get
			{
				return this.canBeSetDown.Value;
			}
			set
			{
				this.canBeSetDown.Value = value;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060012CA RID: 4810 RVA: 0x000DDFCB File Offset: 0x000DC1CB
		// (set) Token: 0x060012CB RID: 4811 RVA: 0x000DDFD8 File Offset: 0x000DC1D8
		[XmlIgnore]
		public bool CanBeGrabbed
		{
			get
			{
				return this.canBeGrabbed.Value;
			}
			set
			{
				this.canBeGrabbed.Value = value;
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060012CC RID: 4812 RVA: 0x000DDFE6 File Offset: 0x000DC1E6
		// (set) Token: 0x060012CD RID: 4813 RVA: 0x000DDFF3 File Offset: 0x000DC1F3
		[XmlIgnore]
		public bool IsOn
		{
			get
			{
				return this.isOn.Value;
			}
			set
			{
				this.isOn.Value = value;
			}
		}

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060012CE RID: 4814 RVA: 0x000DE001 File Offset: 0x000DC201
		// (set) Token: 0x060012CF RID: 4815 RVA: 0x000DE00E File Offset: 0x000DC20E
		[XmlIgnore]
		public bool IsSpawnedObject
		{
			get
			{
				return this.isSpawnedObject.Value;
			}
			set
			{
				this.isSpawnedObject.Value = value;
			}
		}

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060012D0 RID: 4816 RVA: 0x000DE01C File Offset: 0x000DC21C
		// (set) Token: 0x060012D1 RID: 4817 RVA: 0x000DE029 File Offset: 0x000DC229
		[XmlIgnore]
		public bool Flipped
		{
			get
			{
				return this.flipped.Value;
			}
			set
			{
				this.flipped.Value = value;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060012D2 RID: 4818 RVA: 0x000DE037 File Offset: 0x000DC237
		// (set) Token: 0x060012D3 RID: 4819 RVA: 0x000DE044 File Offset: 0x000DC244
		[XmlIgnore]
		public int Price
		{
			get
			{
				return this.price.Value;
			}
			set
			{
				this.price.Value = value;
			}
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x060012D4 RID: 4820 RVA: 0x000DE052 File Offset: 0x000DC252
		// (set) Token: 0x060012D5 RID: 4821 RVA: 0x000DE05F File Offset: 0x000DC25F
		[XmlIgnore]
		public int Edibility
		{
			get
			{
				return this.edibility.Value;
			}
			set
			{
				this.edibility.Value = value;
			}
		}

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x060012D6 RID: 4822 RVA: 0x000DE06D File Offset: 0x000DC26D
		// (set) Token: 0x060012D7 RID: 4823 RVA: 0x000DE07A File Offset: 0x000DC27A
		[XmlIgnore]
		public int Fragility
		{
			get
			{
				return this.fragility.Value;
			}
			set
			{
				this.fragility.Value = value;
			}
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x060012D8 RID: 4824 RVA: 0x000DE088 File Offset: 0x000DC288
		// (set) Token: 0x060012D9 RID: 4825 RVA: 0x000DE090 File Offset: 0x000DC290
		[XmlIgnore]
		public Vector2 Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
			}
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x060012DA RID: 4826 RVA: 0x000DE099 File Offset: 0x000DC299
		// (set) Token: 0x060012DB RID: 4827 RVA: 0x000DE0A6 File Offset: 0x000DC2A6
		[XmlIgnore]
		public int MinutesUntilReady
		{
			get
			{
				return this.minutesUntilReady.Value;
			}
			set
			{
				this.minutesUntilReady.Value = value;
			}
		}

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x060012DC RID: 4828 RVA: 0x000DE0B4 File Offset: 0x000DC2B4
		// (set) Token: 0x060012DD RID: 4829 RVA: 0x000DE0BC File Offset: 0x000DC2BC
		[XmlIgnore]
		public string SignText { get; private set; }

		// Token: 0x060012DE RID: 4830 RVA: 0x000DE0C8 File Offset: 0x000DC2C8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.tileLocation, "tileLocation").AddField(this.owner, "owner").AddField(this.type, "type").AddField(this.canBeSetDown, "canBeSetDown").AddField(this.canBeGrabbed, "canBeGrabbed").AddField(this.isSpawnedObject, "isSpawnedObject").AddField(this.questItem, "questItem").AddField(this.questId, "questId").AddField(this.isOn, "isOn").AddField(this.fragility, "fragility").AddField(this.price, "price").AddField(this.edibility, "edibility").AddField(this.uses, "uses").AddField(this.bigCraftable, "bigCraftable").AddField(this.setOutdoors, "setOutdoors").AddField(this.setIndoors, "setIndoors").AddField(this.readyForHarvest, "readyForHarvest").AddField(this.showNextIndex, "showNextIndex").AddField(this.flipped, "flipped").AddField(this.isLamp, "isLamp").AddField(this.heldObject, "heldObject").AddField(this.lastInputItem, "lastInputItem").AddField(this.lastOutputRuleId, "lastOutputRuleId").AddField(this.minutesUntilReady, "minutesUntilReady").AddField(this.boundingBox, "boundingBox").AddField(this.preserve, "preserve").AddField(this.preservedParentSheetIndex, "preservedParentSheetIndex").AddField(this.netDisplayNameFormat, "netDisplayNameFormat").AddField(this.netLightSource, "netLightSource").AddField(this.orderData, "orderData").AddField(this._destroyOvernight, "_destroyOvernight").AddField(this.signText, "signText");
			this.heldObject.fieldChangeVisibleEvent += delegate(NetRef<Object> field, Object oldValue, Object newValue)
			{
				this._hasHeldObject = (this.heldObject.Value != null);
			};
			this.netLightSource.fieldChangeVisibleEvent += delegate(NetRef<LightSource> field, LightSource oldValue, LightSource newValue)
			{
				this._hasLightSource = (this.netLightSource.Value != null);
			};
			this.bigCraftable.fieldChangeVisibleEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				this._qualifiedItemId = null;
				base.MarkContextTagsDirty();
			};
			this.signText.fieldChangeVisibleEvent += delegate(NetString field, string oldValue, string newValue)
			{
				newValue = TokenParser.ParseText(newValue, null, null, null);
				this.SignText = Utility.FilterDirtyWords(newValue);
			};
			this.preserve.fieldChangeVisibleEvent += delegate(NetNullableEnum<Object.PreserveType> <p0>, Object.PreserveType? <p1>, Object.PreserveType? <p2>)
			{
				base.MarkContextTagsDirty();
			};
			this.preservedParentSheetIndex.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				base.MarkContextTagsDirty();
			};
		}

		// Token: 0x060012DF RID: 4831 RVA: 0x000DE36C File Offset: 0x000DC56C
		public Object()
		{
		}

		// Token: 0x060012E0 RID: 4832 RVA: 0x000DE4FC File Offset: 0x000DC6FC
		public Object(Vector2 tileLocation, string itemId, bool isRecipe = false) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			this.isRecipe.Value = isRecipe;
			base.ItemId = itemId;
			this.canBeSetDown.Value = true;
			this.bigCraftable.Value = true;
			BigCraftableData data;
			if (Game1.bigCraftableData.TryGetValue(itemId, out data))
			{
				this.name = (data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName);
				this.price.Value = data.Price;
				this.type.Value = "Crafting";
				base.Category = -9;
				this.setOutdoors.Value = data.CanBePlacedOutdoors;
				this.setIndoors.Value = data.CanBePlacedIndoors;
				this.fragility.Value = data.Fragility;
				this.isLamp.Value = data.IsLamp;
			}
			base.ResetParentSheetIndex();
			this.TileLocation = tileLocation;
			this.initializeLightSource(this.tileLocation.Value, false);
		}

		// Token: 0x060012E1 RID: 4833 RVA: 0x000DE604 File Offset: 0x000DC804
		public Object(string itemId, int initialStack, bool isRecipe = false, int price = -1, int quality = 0) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			this.stack.Value = initialStack;
			this.isRecipe.Value = isRecipe;
			this.quality.Value = quality;
			base.ItemId = itemId;
			base.ResetParentSheetIndex();
			ObjectData data;
			if (Game1.objectData.TryGetValue(itemId, out data))
			{
				this.name = (data.Name ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName);
				this.price.Value = data.Price;
				this.edibility.Value = data.Edibility;
				this.type.Value = data.Type;
				base.Category = data.Category;
			}
			if (price != -1)
			{
				this.price.Value = price;
			}
			this.canBeSetDown.Value = true;
			this.canBeGrabbed.Value = true;
			this.isSpawnedObject.Value = false;
			if (Game1.random.NextBool() && Utility.IsLegacyIdAbove(itemId, 52) && !Utility.IsLegacyIdBetween(itemId, 8, 15) && !Utility.IsLegacyIdBetween(itemId, 384, 391))
			{
				this.flipped.Value = true;
			}
			if (base.QualifiedItemId == "(O)463" || base.QualifiedItemId == "(O)464")
			{
				this.scale = new Vector2(1f, 1f);
			}
			if (itemId == "449" || this.IsWeeds() || this.IsTwig())
			{
				this.fragility.Value = 2;
			}
			else if (this.name.Contains("Fence"))
			{
				this.scale = new Vector2(10f, 0f);
			}
			else if (this.IsBreakableStone())
			{
				if (!(itemId == "8"))
				{
					if (!(itemId == "10"))
					{
						if (!(itemId == "12"))
						{
							if (!(itemId == "14"))
							{
								if (!(itemId == "25"))
								{
									this.minutesUntilReady.Value = 1;
								}
								else
								{
									this.minutesUntilReady.Value = 8;
								}
							}
							else
							{
								this.minutesUntilReady.Value = 12;
							}
						}
						else
						{
							this.minutesUntilReady.Value = 16;
						}
					}
					else
					{
						this.minutesUntilReady.Value = 8;
					}
				}
				else
				{
					this.minutesUntilReady.Value = 4;
				}
			}
			if (base.Category == -22)
			{
				this.scale.Y = 1f;
			}
		}

		// Token: 0x060012E2 RID: 4834 RVA: 0x000DE884 File Offset: 0x000DCA84
		[Obsolete("This is used for specialized game behavior and only supports vanilla objects. New code should place a new object instance instead.")]
		public virtual void SetIdAndSprite(int spriteIndex)
		{
			base.ParentSheetIndex = spriteIndex;
			base.ItemId = spriteIndex.ToString();
		}

		// Token: 0x060012E3 RID: 4835 RVA: 0x000DE89C File Offset: 0x000DCA9C
		public virtual void RecalculateBoundingBox()
		{
			Vector2 tile = this.TileLocation;
			this.boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
		}

		// Token: 0x060012E4 RID: 4836 RVA: 0x000DE8D8 File Offset: 0x000DCAD8
		public virtual bool IsHeldOverHead()
		{
			return true;
		}

		// Token: 0x060012E5 RID: 4837 RVA: 0x000DE8DC File Offset: 0x000DCADC
		protected override void _PopulateContextTags(HashSet<string> tags)
		{
			base._PopulateContextTags(tags);
			if (this.orderData.Value == "QI_COOKING")
			{
				tags.Add("quality_qi");
			}
			if (this.preserve != null && this.preserve.Value != null)
			{
				Object.PreserveType? value = this.preserve.Value;
				if (value != null)
				{
					switch (value.GetValueOrDefault())
					{
					case Object.PreserveType.Wine:
						tags.Add("wine_item");
						break;
					case Object.PreserveType.Jelly:
						tags.Add("jelly_item");
						break;
					case Object.PreserveType.Pickle:
						tags.Add("pickle_item");
						break;
					case Object.PreserveType.Juice:
						tags.Add("juice_item");
						break;
					case Object.PreserveType.Honey:
						tags.Add("honey_item");
						break;
					}
				}
			}
			if (this.preservedParentSheetIndex.Value != null)
			{
				tags.Add("preserve_sheet_index_" + ItemContextTagManager.SanitizeContextTag(this.preservedParentSheetIndex.Value));
			}
		}

		// Token: 0x060012E6 RID: 4838 RVA: 0x000DE9EF File Offset: 0x000DCBEF
		protected virtual string loadDisplayName()
		{
			return Object.GetObjectDisplayName(base.QualifiedItemId, this.preserve.Value, this.preservedParentSheetIndex.Value, this.displayNameFormat, null);
		}

		// Token: 0x060012E7 RID: 4839 RVA: 0x000DEA1C File Offset: 0x000DCC1C
		public static string GetObjectDisplayName(string itemId, Object.PreserveType? preserveType, string preservedId, string displayNameFormat = null, string defaultBaseName = null)
		{
			string text;
			if (defaultBaseName == null)
			{
				text = ItemRegistry.GetDataOrErrorItem(itemId).DisplayName;
			}
			else
			{
				ParsedItemData data = ItemRegistry.GetData(itemId);
				text = (((data != null) ? data.DisplayName : null) ?? defaultBaseName);
			}
			string baseName = text;
			string preservedItemId = Object.GetPreservedItemId(preserveType, preservedId);
			ParsedItemData preservedData = (preservedItemId != null) ? ItemRegistry.GetDataOrErrorItem(preservedItemId) : null;
			string preservedName = (preservedData != null) ? preservedData.DisplayName : null;
			string lowerPreservedName = (preservedName != null) ? preservedName.ToLowerInvariant() : null;
			if (displayNameFormat != null)
			{
				string result = TokenParser.ParseText(displayNameFormat, null, null, null);
				if (result.Contains('%'))
				{
					result = result.Replace("%DISPLAY_NAME_LOWERCASE", baseName).Replace("%DISPLAY_NAME", baseName).Replace("%PRESERVED_DISPLAY_NAME_LOWERCASE", lowerPreservedName).Replace("%PRESERVED_DISPLAY_NAME", preservedName);
				}
				return result;
			}
			if (preserveType != null)
			{
				switch (preserveType.GetValueOrDefault())
				{
				case Object.PreserveType.Wine:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Wine_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:Wine_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.Jelly:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Jelly_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:Jelly_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.Pickle:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Pickles_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:Pickles_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.Juice:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Juice_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:Juice_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.Roe:
				{
					string result2;
					if ((result2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Roe_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false)) == null)
					{
						result2 = Game1.content.LoadString("Strings\\Objects:Roe_Flavored_Name", (preservedName != null) ? preservedName.TrimEnd('鱼') : null, (lowerPreservedName != null) ? lowerPreservedName.TrimEnd('鱼') : null);
					}
					return result2;
				}
				case Object.PreserveType.AgedRoe:
					if (preservedItemId != null)
					{
						string result3;
						if ((result3 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:AgedRoe_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false)) == null)
						{
							result3 = Game1.content.LoadString("Strings\\Objects:AgedRoe_Flavored_Name", (preservedName != null) ? preservedName.TrimEnd('鱼') : null, (lowerPreservedName != null) ? lowerPreservedName.TrimEnd('鱼') : null);
						}
						return result3;
					}
					break;
				case Object.PreserveType.Honey:
					if (preservedId == "-1")
					{
						return Game1.content.LoadString("Strings\\Objects:Honey_Wild_Name");
					}
					if (preservedName == null)
					{
						return baseName;
					}
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:Honey_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:Honey_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.Bait:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:SpecificBait_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:SpecificBait_Flavored_Name", preservedName, lowerPreservedName);
				case Object.PreserveType.DriedFruit:
				case Object.PreserveType.DriedMushroom:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:DriedFruit_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Lexicon.makePlural(Game1.content.LoadString("Strings\\Objects:DriedFruit_Flavored_Name", preservedName, lowerPreservedName), false);
				case Object.PreserveType.SmokedFish:
					return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:SmokedFish_Flavored_" + ((preservedData != null) ? preservedData.QualifiedItemId : null) + "_Name", preservedName, lowerPreservedName, false) ?? Game1.content.LoadString("Strings\\Objects:SmokedFish_Flavored_Name", preservedName, lowerPreservedName);
				}
			}
			return baseName;
		}

		// Token: 0x060012E8 RID: 4840 RVA: 0x000DEE0E File Offset: 0x000DD00E
		public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
		{
			return new Vector2(this.tileLocation.X * 64f - (float)viewport.X, this.tileLocation.Y * 64f - (float)viewport.Y);
		}

		// Token: 0x060012E9 RID: 4841 RVA: 0x000DEE49 File Offset: 0x000DD049
		public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(int index)
		{
			return Object.getSourceRectForBigCraftable(Game1.bigCraftableSpriteSheet, index);
		}

		// Token: 0x060012EA RID: 4842 RVA: 0x000DEE56 File Offset: 0x000DD056
		public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(Texture2D texture, int index)
		{
			return new Microsoft.Xna.Framework.Rectangle(index % (texture.Width / 16) * 16, index * 16 / texture.Width * 16 * 2, 16, 32);
		}

		// Token: 0x060012EB RID: 4843 RVA: 0x000DEE80 File Offset: 0x000DD080
		public virtual bool performToolAction(Tool t)
		{
			GameLocation location = this.Location;
			if (this.isTemporarilyInvisible)
			{
				return false;
			}
			if (base.QualifiedItemId == "(BC)165")
			{
				Chest chest2 = this.heldObject.Value as Chest;
				if (chest2 != null && !chest2.isEmpty())
				{
					chest2.clearNulls();
					if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
					{
						this.playNearbySoundAll("hammer", null, SoundContext.Default);
						this.shakeTimer = 100;
					}
					return false;
				}
			}
			if (t == null)
			{
				Object tileObj;
				if (location.objects.TryGetValue(this.tileLocation.Value, out tileObj) && tileObj.Equals(this))
				{
					if (location.farmers.Count > 0)
					{
						Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 10), false, -1, false, null);
					}
					location.objects.Remove(this.tileLocation.Value);
				}
				return false;
			}
			if (this.IsBreakableStone() && t is Pickaxe)
			{
				int damage = t.upgradeLevel.Value + 1;
				if ((base.QualifiedItemId == "(O)12" && t.upgradeLevel.Value == 1) || ((base.QualifiedItemId == "(O)12" || base.QualifiedItemId == "(O)14") && t.upgradeLevel.Value == 0))
				{
					damage = 0;
					this.playNearbySoundAll("crafting", null, SoundContext.Default);
				}
				this.MinutesUntilReady -= damage;
				if (this.MinutesUntilReady <= 0)
				{
					return true;
				}
				this.playNearbySoundAll("hammer", null, SoundContext.Default);
				this.shakeTimer = 100;
				return false;
			}
			else
			{
				if (this.IsBreakableStone() && t is Pickaxe)
				{
					return false;
				}
				if (this.name.Equals("Boulder") && (t.upgradeLevel.Value < 4 || !(t is Pickaxe)))
				{
					if (t.isHeavyHitter())
					{
						this.playNearbySoundAll("hammer", null, SoundContext.Default);
					}
					return false;
				}
				if (this.IsWeeds() && t.isHeavyHitter())
				{
					int damage2 = 1;
					if (t is MeleeWeapon && t.isScythe() && t.QualifiedItemId != "(W)47")
					{
						damage2 = 2;
					}
					if (this.shakeTimer <= 0)
					{
						this.minutesUntilReady.Value -= damage2;
					}
					if (this.minutesUntilReady.Value <= 0)
					{
						if (!(base.QualifiedItemId == "(O)319") && !(base.QualifiedItemId == "(O)320") && !(base.QualifiedItemId == "(O)321") && t.getLastFarmerToUse() != null)
						{
							foreach (BaseEnchantment baseEnchantment in t.getLastFarmerToUse().enchantments)
							{
								baseEnchantment.OnCutWeed(this.tileLocation.Value, location, t.getLastFarmerToUse());
							}
						}
						this.cutWeed(t.getLastFarmerToUse());
						return true;
					}
					if (this.shakeTimer <= 0)
					{
						Game1.playSound("weed_cut", null);
						this.shakeTimer = 200;
						return false;
					}
				}
				else
				{
					if (this.IsTwig() && t is Axe)
					{
						this.fragility.Value = 2;
						this.playNearbySoundAll("axchop", null, SoundContext.Default);
						location.debris.Add(new Debris(ItemRegistry.Create("(O)388", 1, 0, false), this.tileLocation.Value * 64f));
						Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 10), false, -1, false, null);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
						});
						t.getLastFarmerToUse().gainExperience(2, 1);
						return true;
					}
					if (this.name.Contains("SupplyCrate") && t.isHeavyHitter())
					{
						this.MinutesUntilReady -= t.upgradeLevel.Value + 1;
						if (this.MinutesUntilReady <= 0)
						{
							this.fragility.Value = 2;
							this.playNearbySoundAll("barrelBreak", null, SoundContext.Default);
							Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)this.tileLocation.X * 777.0, (double)this.tileLocation.Y * 7.0, 0.0, 0.0);
							int houseLevel = t.getLastFarmerToUse().HouseUpgradeLevel;
							int x = (int)this.tileLocation.X;
							int y = (int)this.tileLocation.Y;
							if (houseLevel != 0)
							{
								if (houseLevel != 1)
								{
									switch (r.Next(9))
									{
									case 0:
										Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
										break;
									case 1:
										Game1.createMultipleObjectDebris("(O)920", x, y, r.Next(5, 8), location);
										break;
									case 2:
										Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(2, 5), location);
										break;
									case 3:
										Game1.createMultipleObjectDebris("(O)253", x, y, r.Next(2, 4), location);
										break;
									case 4:
										Game1.createMultipleObjectDebris(r.Choose("(O)904", "(O)905"), x, y, r.Next(1, 3), location);
										break;
									case 5:
										Game1.createMultipleObjectDebris("(O)246", x, y, r.Next(4, 8), location);
										Game1.createMultipleObjectDebris("(O)247", x, y, r.Next(2, 5), location);
										Game1.createMultipleObjectDebris("(O)245", x, y, r.Next(4, 8), location);
										break;
									case 6:
										Game1.createMultipleObjectDebris("(O)275", x, y, 2, location);
										break;
									case 7:
										Game1.createMultipleObjectDebris("(O)288", x, y, r.Next(3, 6), location);
										break;
									default:
										Game1.createMultipleObjectDebris("MixedFlowerSeeds", x, y, r.Next(5, 6), location);
										break;
									}
								}
								else
								{
									switch (r.Next(10))
									{
									case 0:
										Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
										break;
									case 1:
										Game1.createMultipleObjectDebris("(O)371", x, y, r.Next(5, 8), location);
										break;
									case 2:
										Game1.createMultipleObjectDebris("(O)749", x, y, r.Next(2, 5), location);
										break;
									case 3:
										Game1.createMultipleObjectDebris("(O)253", x, y, r.Next(1, 3), location);
										break;
									case 4:
										Game1.createMultipleObjectDebris("(O)237", x, y, r.Next(1, 3), location);
										break;
									case 5:
										Game1.createMultipleObjectDebris("(O)246", x, y, r.Next(4, 8), location);
										break;
									case 6:
										Game1.createMultipleObjectDebris("(O)247", x, y, r.Next(2, 5), location);
										break;
									case 7:
										Game1.createMultipleObjectDebris("(O)245", x, y, r.Next(4, 8), location);
										break;
									case 8:
										Game1.createMultipleObjectDebris("(O)287", x, y, r.Next(3, 6), location);
										break;
									default:
										Game1.createMultipleObjectDebris("MixedFlowerSeeds", x, y, r.Next(4, 6), location);
										break;
									}
								}
							}
							else
							{
								switch (r.Next(7))
								{
								case 0:
									Game1.createMultipleObjectDebris("(O)770", x, y, r.Next(3, 6), location);
									break;
								case 1:
									Game1.createMultipleObjectDebris("(O)371", x, y, r.Next(5, 8), location);
									break;
								case 2:
									Game1.createMultipleObjectDebris("(O)535", x, y, r.Next(2, 5), location);
									break;
								case 3:
									Game1.createMultipleObjectDebris("(O)241", x, y, r.Next(1, 3), location);
									break;
								case 4:
									Game1.createMultipleObjectDebris("(O)395", x, y, r.Next(1, 3), location);
									break;
								case 5:
									Game1.createMultipleObjectDebris("(O)286", x, y, r.Next(3, 6), location);
									break;
								default:
									Game1.createMultipleObjectDebris("(O)286", x, y, r.Next(3, 6), location);
									break;
								}
							}
							Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 10), false, -1, false, null);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							});
							return true;
						}
						this.shakeTimer = 200;
						this.playNearbySoundAll("woodWhack", null, SoundContext.Default);
						return false;
					}
				}
				if (base.QualifiedItemId == "(O)590" || base.QualifiedItemId == "(O)SeedSpot")
				{
					if (t is Hoe)
					{
						Random r2 = Utility.CreateDaySaveRandom((double)(-(double)this.tileLocation.X * 7f), (double)(this.tileLocation.Y * 777f), (double)(Game1.netWorldState.Value.TreasureTotemsUsed * 777));
						t.getLastFarmerToUse().stats.Increment("ArtifactSpotsDug", 1);
						if (t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") > 2U && r2.NextDouble() < 0.008 + ((!t.getLastFarmerToUse().mailReceived.Contains("DefenseBookDropped")) ? (t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") * 0.002) : 0.005))
						{
							t.getLastFarmerToUse().mailReceived.Add("DefenseBookDropped");
							Vector2 position = this.TileLocation * 64f;
							Game1.createMultipleItemDebris(ItemRegistry.Create("(O)Book_Defense", 1, 0, false), position, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location, -1, false);
						}
						if (base.QualifiedItemId == "(O)SeedSpot")
						{
							Item raccoonSeedForCurrentTimeOfYear = Utility.getRaccoonSeedForCurrentTimeOfYear(t.getLastFarmerToUse(), r2, -1);
							Vector2 position2 = this.TileLocation * 64f;
							Game1.createMultipleItemDebris(raccoonSeedForCurrentTimeOfYear, position2, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location, -1, false);
						}
						else
						{
							location.digUpArtifactSpot((int)this.tileLocation.X, (int)this.tileLocation.Y, t.getLastFarmerToUse());
						}
						location.makeHoeDirt(this.tileLocation.Value, true);
						this.playNearbySoundAll("hoeHit", null, SoundContext.Default);
						t.getLastFarmerToUse().gainExperience(2, 15);
						location.objects.Remove(this.tileLocation.Value);
					}
					return false;
				}
				if (this.bigCraftable.Value && !(t is MeleeWeapon) && t.isHeavyHitter() && ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).IsErrorItem)
				{
					this.playNearbySoundAll("hammer", null, SoundContext.Default);
					this.performRemoveAction();
					location.objects.Remove(this.tileLocation.Value);
					return false;
				}
				if (this.fragility.Value == 2)
				{
					return false;
				}
				if (!(this.Type == "Crafting") || t is MeleeWeapon || !t.isHeavyHitter())
				{
					return false;
				}
				if (t is Hoe && this.IsSprinkler())
				{
					return false;
				}
				this.playNearbySoundAll("hammer", null, SoundContext.Default);
				if (this.fragility.Value == 1)
				{
					Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(3, 6), false, -1, false, null);
					Game1.createRadialDebris(location, 14, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(3, 6), false, -1, false, null);
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(2, 5), false, -1, false, null);
						Game1.createRadialDebris(location, 14, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(2, 5), false, -1, false, null);
					}, 80);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
					});
					this.performRemoveAction();
					location.objects.Remove(this.tileLocation.Value);
					return false;
				}
				TerrainFeature terrainFeature;
				if (this.IsTapper() && location.terrainFeatures.TryGetValue(this.tileLocation.Value, out terrainFeature))
				{
					Tree tree = terrainFeature as Tree;
					if (tree != null)
					{
						tree.tapped.Value = false;
					}
				}
				string qualifiedItemId = base.QualifiedItemId;
				if (qualifiedItemId == "(BC)254")
				{
					if (this.heldObject.Value != null)
					{
						base.ResetParentSheetIndex();
						location.debris.Add(new Debris(this.heldObject.Value, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
						this.heldObject.Value = null;
					}
					return true;
				}
				if (qualifiedItemId == "(BC)21")
				{
					if (this.heldObject.Value != null)
					{
						location.debris.Add(new Debris(this.heldObject.Value, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
						this.heldObject.Value = null;
					}
				}
				if (!this.IsSprinkler() || this.heldObject.Value == null)
				{
					if (this.IsSprinkler() && base.SpecialVariable == 999999)
					{
						location.debris.Add(new Debris(ItemRegistry.Create("(O)93", 1, 0, false), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
					}
					if (this.heldObject.Value != null && this.readyForHarvest.Value)
					{
						location.debris.Add(new Debris(this.heldObject.Value, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
					}
					if (base.QualifiedItemId == "(BC)156")
					{
						base.ResetParentSheetIndex();
						this.heldObject.Value = null;
						this.minutesUntilReady.Value = -1;
					}
					if (this.name.Contains("Seasonal"))
					{
						base.ParentSheetIndex -= base.ParentSheetIndex % 4;
					}
					return true;
				}
				if (this.heldObject.Value.heldObject.Value != null)
				{
					Object value = this.heldObject.Value.heldObject.Value;
					Chest chest = value as Chest;
					if (chest != null)
					{
						chest.GetMutex().RequestLock(delegate
						{
							List<Item> list = new List<Item>(chest.Items);
							chest.Items.Clear();
							foreach (Item item in list)
							{
								if (item != null)
								{
									location.debris.Add(new Debris(item, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
								}
							}
							Object held_object = this.heldObject.Value;
							this.heldObject.Value = null;
							location.debris.Add(new Debris(held_object, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
							chest.GetMutex().ReleaseLock();
						}, null);
					}
					return false;
				}
				location.debris.Add(new Debris(this.heldObject.Value, this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
				this.heldObject.Value = null;
				return false;
			}
		}

		// Token: 0x060012EC RID: 4844 RVA: 0x000E0088 File Offset: 0x000DE288
		public virtual void cutWeed(Farmer who)
		{
			GameLocation location = this.Location;
			Color c = Color.Green;
			string sound = "cut";
			int animation = 50;
			this.fragility.Value = 2;
			string toDrop = null;
			if (Game1.random.NextBool())
			{
				toDrop = "771";
			}
			else if (Game1.random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") > 0U) ? 0.04 : 0.0))
			{
				toDrop = "770";
			}
			else if (Game1.currentSeason == "summer" && Game1.random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") > 0U) ? 0.04 : 0.0))
			{
				toDrop = "MixedFlowerSeeds";
			}
			if (this.name.Contains("GreenRainWeeds") && Game1.random.NextDouble() < 0.1)
			{
				toDrop = "Moss";
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length != 6)
				{
					if (length == 15)
					{
						switch (qualifiedItemId[14])
						{
						case '0':
							if (!(qualifiedItemId == "GreenRainWeeds0"))
							{
								goto IL_4E1;
							}
							break;
						case '1':
							if (!(qualifiedItemId == "GreenRainWeeds1"))
							{
								goto IL_4E1;
							}
							break;
						case '2':
						case '3':
							goto IL_4E1;
						case '4':
							if (!(qualifiedItemId == "GreenRainWeeds4"))
							{
								goto IL_4E1;
							}
							break;
						default:
							goto IL_4E1;
						}
						sound = "weed_cut";
					}
				}
				else
				{
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(O)320"))
						{
							goto IL_4E1;
						}
						c = new Color(175, 143, 255);
						sound = "breakingGlass";
						animation = 47;
						this.playNearbySoundAll("drumkit2", null, SoundContext.Default);
						toDrop = null;
						goto IL_4E1;
					case '1':
						if (!(qualifiedItemId == "(O)321"))
						{
							goto IL_4E1;
						}
						c = new Color(73, 255, 158);
						sound = "breakingGlass";
						animation = 47;
						this.playNearbySoundAll("drumkit2", null, SoundContext.Default);
						toDrop = null;
						goto IL_4E1;
					case '2':
						if (qualifiedItemId == "(O)792")
						{
							goto IL_43A;
						}
						if (!(qualifiedItemId == "(O)882"))
						{
							goto IL_4E1;
						}
						goto IL_446;
					case '3':
						if (!(qualifiedItemId == "(O)313"))
						{
							if (qualifiedItemId == "(O)793")
							{
								goto IL_43A;
							}
							if (!(qualifiedItemId == "(O)883"))
							{
								goto IL_4E1;
							}
							goto IL_446;
						}
						break;
					case '4':
						if (!(qualifiedItemId == "(O)314"))
						{
							if (qualifiedItemId == "(O)794")
							{
								goto IL_43A;
							}
							if (!(qualifiedItemId == "(O)884"))
							{
								goto IL_4E1;
							}
							goto IL_446;
						}
						break;
					case '5':
						if (!(qualifiedItemId == "(O)315"))
						{
							goto IL_4E1;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(O)316"))
						{
							goto IL_4E1;
						}
						goto IL_374;
					case '7':
						if (!(qualifiedItemId == "(O)317"))
						{
							goto IL_4E1;
						}
						goto IL_374;
					case '8':
						if (qualifiedItemId == "(O)678")
						{
							c = new Color(228, 109, 159);
							goto IL_4E1;
						}
						if (!(qualifiedItemId == "(O)318"))
						{
							goto IL_4E1;
						}
						goto IL_374;
					case '9':
						if (qualifiedItemId == "(O)679")
						{
							c = new Color(253, 191, 46);
							goto IL_4E1;
						}
						if (!(qualifiedItemId == "(O)319"))
						{
							goto IL_4E1;
						}
						c = new Color(30, 216, 255);
						sound = "breakingGlass";
						animation = 47;
						this.playNearbySoundAll("drumkit2", null, SoundContext.Default);
						toDrop = null;
						goto IL_4E1;
					default:
						goto IL_4E1;
					}
					c = new Color(84, 101, 27);
					goto IL_4E1;
					IL_374:
					c = new Color(109, 49, 196);
					goto IL_4E1;
					IL_43A:
					toDrop = "770";
					goto IL_4E1;
					IL_446:
					c = new Color(30, 97, 68);
					if (Game1.MasterPlayer.hasOrWillReceiveMail("islandNorthCaveOpened") && Game1.random.NextDouble() < 0.1 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMummifiedFrog"))
					{
						Game1.addMailForTomorrow("gotMummifiedFrog", true, true);
						toDrop = "828";
					}
					else if (Game1.random.NextDouble() < 0.01)
					{
						toDrop = "828";
					}
					else if (Game1.random.NextDouble() < 0.08)
					{
						toDrop = "831";
					}
				}
			}
			IL_4E1:
			if (sound.Equals("breakingGlass") && Game1.random.NextDouble() < 0.0025)
			{
				toDrop = "338";
			}
			this.playNearbySoundAll(sound, null, SoundContext.Default);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(animation, this.tileLocation.Value * 64f, c, 8, false, 100f, 0, -1, -1f, -1, 0)
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(animation, this.tileLocation.Value * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), c * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					scale = 0.75f,
					flipped = true
				}
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(animation, this.tileLocation.Value * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), c * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					scale = 0.75f,
					delayBeforeAnimationStart = 50
				}
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(animation, this.tileLocation.Value * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), c * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					scale = 0.75f,
					flipped = true,
					delayBeforeAnimationStart = 100
				}
			});
			if (!sound.Equals("breakingGlass"))
			{
				if (Game1.random.NextDouble() < 1E-05)
				{
					location.debris.Add(new Debris(ItemRegistry.Create("(H)40", 1, 0, false), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
				if (Game1.random.NextDouble() <= 0.01 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
				{
					location.debris.Add(new Debris(ItemRegistry.Create("(O)890", 1, 0, false), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
			}
			if (toDrop != null)
			{
				location.debris.Add(new Debris(new Object(toDrop, 1, false, -1, 0), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				location.addJumperFrog(this.tileLocation.Value);
			}
			if (location.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.009)
			{
				Object o = location.tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					Game1.createItemDebris(o, new Vector2(this.tileLocation.X + 0.5f, this.tileLocation.Y + 0.75f) * 64f, Game1.player.FacingDirection, location, -1, false);
				}
			}
		}

		// Token: 0x060012ED RID: 4845 RVA: 0x000E0942 File Offset: 0x000DEB42
		public virtual bool isAnimalProduct()
		{
			return base.Category == -18 || base.Category == -5 || base.Category == -6 || base.QualifiedItemId == "(O)430";
		}

		// Token: 0x060012EE RID: 4846 RVA: 0x000E0974 File Offset: 0x000DEB74
		public virtual bool onExplosion(Farmer who)
		{
			if (who == null)
			{
				return false;
			}
			GameLocation location = this.Location;
			if (this.IsWeeds())
			{
				this.fragility.Value = 0;
				this.cutWeed(who);
				location.removeObject(this.tileLocation.Value, false);
			}
			if (this.IsTwig())
			{
				this.fragility.Value = 0;
				Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 10), false, -1, false, null);
				location.debris.Add(new Debris(ItemRegistry.Create("(O)388", 1, 0, false), this.tileLocation.Value * 64f));
			}
			if (this.IsBreakableStone())
			{
				this.fragility.Value = 0;
			}
			this.performRemoveAction();
			return true;
		}

		// Token: 0x060012EF RID: 4847 RVA: 0x000E0A58 File Offset: 0x000DEC58
		public override bool canBeShipped()
		{
			return !this.bigCraftable.Value && this.type.Value != null && this.Type != "Quest" && this.canBeTrashed() && !(this is Furniture) && !(this is Wallpaper);
		}

		// Token: 0x060012F0 RID: 4848 RVA: 0x000E0AB0 File Offset: 0x000DECB0
		public virtual void ApplySprinkler(Vector2 tile)
		{
			GameLocation location = this.Location;
			if (location.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") == "T")
			{
				return;
			}
			TerrainFeature terrainFeature;
			if (location.terrainFeatures.TryGetValue(tile, out terrainFeature))
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null && dirt.state.Value != 2)
				{
					dirt.state.Value = 1;
				}
			}
		}

		// Token: 0x060012F1 RID: 4849 RVA: 0x000E0B24 File Offset: 0x000DED24
		public virtual void ApplySprinklerAnimation()
		{
			GameLocation location = this.Location;
			int radius = this.GetModifiedRadiusForSprinkler();
			int tileX = (int)this.tileLocation.X;
			int tileY = (int)this.tileLocation.Y;
			if (radius == 0)
			{
				int delay = Game1.random.Next(1000);
				location.temporarySprites.Add(new TemporaryAnimatedSprite(29, this.tileLocation.Value * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = delay,
					id = tileX * 4000 + tileY
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(29, this.tileLocation.Value * 64f + new Vector2(48f, 0f), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
				{
					rotation = 1.5707964f,
					delayBeforeAnimationStart = delay,
					id = tileX * 4000 + tileY
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(29, this.tileLocation.Value * 64f + new Vector2(0f, 48f), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
				{
					rotation = 3.1415927f,
					delayBeforeAnimationStart = delay,
					id = tileX * 4000 + tileY
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(29, this.tileLocation.Value * 64f + new Vector2(-48f, 0f), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
				{
					rotation = 4.712389f,
					delayBeforeAnimationStart = delay,
					id = tileX * 4000 + tileY
				});
				return;
			}
			if (radius != 1)
			{
				if (radius > 0)
				{
					float scale = (float)radius / 2f;
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2176, 320, 320), 60f, 4, 100, this.tileLocation.Value * 64f + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
					{
						color = Color.White * 0.4f,
						delayBeforeAnimationStart = Game1.random.Next(1000),
						id = tileX * 4000 + tileY,
						scale = scale
					});
				}
				return;
			}
			location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1984, 192, 192), 60f, 3, 100, this.tileLocation.Value * 64f + new Vector2(-64f, -64f), false, false)
			{
				color = Color.White * 0.4f,
				delayBeforeAnimationStart = Game1.random.Next(1000),
				id = tileX * 4000 + tileY
			});
		}

		// Token: 0x060012F2 RID: 4850 RVA: 0x000E0EC4 File Offset: 0x000DF0C4
		public virtual List<Vector2> GetSprinklerTiles()
		{
			int radius = this.GetModifiedRadiusForSprinkler();
			if (radius == 0)
			{
				return Utility.getAdjacentTileLocations(this.tileLocation.Value);
			}
			if (radius > 0)
			{
				List<Vector2> tiles = new List<Vector2>();
				int x = (int)this.tileLocation.X - radius;
				while ((float)x <= this.tileLocation.X + (float)radius)
				{
					int y = (int)this.tileLocation.Y - radius;
					while ((float)y <= this.tileLocation.Y + (float)radius)
					{
						tiles.Add(new Vector2((float)x, (float)y));
						y++;
					}
					x++;
				}
				return tiles;
			}
			return new List<Vector2>();
		}

		// Token: 0x060012F3 RID: 4851 RVA: 0x000E0F5C File Offset: 0x000DF15C
		public virtual bool IsInSprinklerRangeBroadphase(Vector2 target)
		{
			int radius = this.GetModifiedRadiusForSprinkler();
			if (radius == 0)
			{
				radius = 1;
			}
			return Math.Abs(target.X - this.TileLocation.X) <= (float)radius && Math.Abs(target.Y - this.TileLocation.Y) <= (float)radius;
		}

		// Token: 0x060012F4 RID: 4852 RVA: 0x000E0FB0 File Offset: 0x000DF1B0
		public virtual void DayUpdate()
		{
			GameLocation location = this.Location;
			this.health = 10;
			if (this.IsSprinkler() && (!location.isOutdoors.Value || !location.IsRainingHere()) && this.GetModifiedRadiusForSprinkler() >= 0)
			{
				location.postFarmEventOvernightActions.Add(delegate
				{
					if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
					{
						return;
					}
					foreach (Vector2 v2 in this.GetSprinklerTiles())
					{
						this.ApplySprinkler(v2);
					}
					this.ApplySprinklerAnimation();
				});
			}
			MachineData machineData = this.GetMachineData();
			if (machineData != null)
			{
				if (machineData.ClearContentsOvernightCondition != null)
				{
					string clearContentsOvernightCondition = machineData.ClearContentsOvernightCondition;
					GameLocation location2 = location;
					Farmer player = null;
					Item value = this.lastInputItem.Value;
					if (GameStateQuery.CheckConditions(clearContentsOvernightCondition, location2, player, this.heldObject.Value, value, null, null))
					{
						base.ResetParentSheetIndex();
						this.heldObject.Value = null;
						this.readyForHarvest.Value = false;
						this.showNextIndex.Value = false;
						this.minutesUntilReady.Value = -1;
					}
				}
				MachineOutputRule outputRule;
				MachineOutputTriggerRule machineOutputTriggerRule;
				MachineOutputRule machineOutputRule;
				MachineOutputTriggerRule machineOutputTriggerRule2;
				if (this.heldObject.Value == null && MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.DayUpdate, null, null, location, out outputRule, out machineOutputTriggerRule, out machineOutputRule, out machineOutputTriggerRule2))
				{
					this.OutputMachine(machineData, outputRule, null, null, location, false, false);
				}
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int num = qualifiedItemId.Length;
				if (num <= 7)
				{
					if (num != 6)
					{
						if (num != 7)
						{
							goto IL_8AE;
						}
						switch (qualifiedItemId[6])
						{
						case '2':
						{
							if (!(qualifiedItemId == "(BC)272"))
							{
								goto IL_8AE;
							}
							AnimalHouse ah = location as AnimalHouse;
							if (ah != null)
							{
								using (NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection.Enumerator enumerator = ah.animals.Pairs.GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										KeyValuePair<long, FarmAnimal> kvp = enumerator.Current;
										kvp.Value.pet(Game1.player, true);
									}
									goto IL_8AE;
								}
								goto IL_37A;
							}
							goto IL_8AE;
						}
						case '3':
						case '7':
							goto IL_8AE;
						case '4':
							if (qualifiedItemId == "(BC)104")
							{
								this.minutesUntilReady.Value = (location.IsWinterHere() ? 9999 : -1);
								goto IL_8AE;
							}
							if (!(qualifiedItemId == "(BC)164"))
							{
								goto IL_8AE;
							}
							if (!(location is Town))
							{
								goto IL_8AE;
							}
							if (Game1.random.NextDouble() < 0.9)
							{
								GameLocation manorHouse = Game1.RequireLocation("ManorHouse", false);
								if (manorHouse.CanItemBePlacedHere(new Vector2(22f, 6f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
								{
									if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
									{
										Game1.mailbox.Add("lewisStatue");
									}
									this.rot();
									manorHouse.objects.Add(new Vector2(22f, 6f), ItemRegistry.Create<Object>("(BC)164", 1, 0, false));
									goto IL_8AE;
								}
								goto IL_8AE;
							}
							else
							{
								GameLocation animalShop = Game1.RequireLocation("AnimalShop", false);
								if (animalShop.CanItemBePlacedHere(new Vector2(11f, 6f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
								{
									if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
									{
										Game1.mailbox.Add("lewisStatue");
									}
									this.rot();
									animalShop.objects.Add(new Vector2(11f, 6f), ItemRegistry.Create<Object>("(BC)164", 1, 0, false));
									goto IL_8AE;
								}
								goto IL_8AE;
							}
							break;
						case '5':
						{
							if (!(qualifiedItemId == "(BC)165"))
							{
								goto IL_8AE;
							}
							AnimalHouse animalHouse = location as AnimalHouse;
							if (animalHouse == null)
							{
								goto IL_8AE;
							}
							Chest chest = this.heldObject.Value as Chest;
							if (chest == null)
							{
								goto IL_8AE;
							}
							using (NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.ValuesCollection.Enumerator enumerator2 = animalHouse.animals.Values.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									FarmAnimal animal = enumerator2.Current;
									if (animal.GetHarvestType().GetValueOrDefault() == FarmAnimalHarvestType.HarvestWithTool && animal.currentProduce.Value != null)
									{
										Object produce = ItemRegistry.Create<Object>("(O)" + animal.currentProduce.Value, 1, 0, false);
										produce.CanBeSetDown = false;
										produce.Quality = animal.produceQuality.Value;
										if (animal.hasEatenAnimalCracker.Value)
										{
											produce.Stack = 2;
										}
										if (chest.addItem(produce) == null)
										{
											animal.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
											animal.currentProduce.Value = null;
											animal.ReloadTextureIfNeeded(false);
											this.showNextIndex.Value = true;
										}
									}
								}
								goto IL_8AE;
							}
							break;
						}
						case '6':
							if (!(qualifiedItemId == "(BC)156"))
							{
								goto IL_8AE;
							}
							break;
						case '8':
						{
							if (!(qualifiedItemId == "(BC)108"))
							{
								goto IL_8AE;
							}
							base.ResetParentSheetIndex();
							Season season = location.GetSeason();
							if (this.Location.IsOutdoors && (season == Season.Winter || season == Season.Fall))
							{
								base.ParentSheetIndex = 109;
								goto IL_8AE;
							}
							goto IL_8AE;
						}
						default:
							goto IL_8AE;
						}
						if (this.MinutesUntilReady > 0 || this.heldObject.Value == null)
						{
							goto IL_8AE;
						}
						if (!location.canSlimeHatchHere())
						{
							this.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
							this.readyForHarvest.Value = false;
							goto IL_8AE;
						}
						GreenSlime slime = null;
						Vector2 v = new Vector2((float)((int)this.tileLocation.X), (float)((int)this.tileLocation.Y + 1)) * 64f;
						string qualifiedItemId2 = this.heldObject.Value.QualifiedItemId;
						if (!(qualifiedItemId2 == "(O)680"))
						{
							if (!(qualifiedItemId2 == "(O)413"))
							{
								if (!(qualifiedItemId2 == "(O)437"))
								{
									if (!(qualifiedItemId2 == "(O)439"))
									{
										if (qualifiedItemId2 == "(O)857")
										{
											slime = new GreenSlime(v, 121);
											slime.makeTigerSlime(false);
										}
									}
									else
									{
										slime = new GreenSlime(v, 121);
									}
								}
								else
								{
									slime = new GreenSlime(v, 80);
								}
							}
							else
							{
								slime = new GreenSlime(v, 40);
							}
						}
						else
						{
							slime = new GreenSlime(v, 0);
						}
						if (slime != null)
						{
							Game1.showGlobalMessage(slime.cute.Value ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
							Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(slime, location, this.tileLocation.Value + new Vector2(0f, 1f), 10, false);
							slime.setTilePosition((int)openSpot.X, (int)openSpot.Y);
							location.characters.Add(slime);
							base.ResetParentSheetIndex();
							this.heldObject.Value = null;
							this.minutesUntilReady.Value = -1;
							goto IL_8AE;
						}
						goto IL_8AE;
					}
					else
					{
						switch (qualifiedItemId[5])
						{
						case '4':
							if (qualifiedItemId == "(O)784")
							{
								goto IL_822;
							}
							if (!(qualifiedItemId == "(O)674"))
							{
								goto IL_8AE;
							}
							goto IL_856;
						case '5':
							if (qualifiedItemId == "(O)785")
							{
								goto IL_822;
							}
							if (!(qualifiedItemId == "(O)675"))
							{
								goto IL_8AE;
							}
							goto IL_856;
						case '6':
							if (!(qualifiedItemId == "(O)746"))
							{
								if (!(qualifiedItemId == "(O)676"))
								{
									goto IL_8AE;
								}
								goto IL_883;
							}
							else
							{
								if (location.IsWinterHere())
								{
									this.rot();
									goto IL_8AE;
								}
								goto IL_8AE;
							}
							break;
						case '7':
							if (!(qualifiedItemId == "(O)747"))
							{
								if (!(qualifiedItemId == "(O)677"))
								{
									goto IL_8AE;
								}
								goto IL_883;
							}
							break;
						case '8':
							if (!(qualifiedItemId == "(O)748"))
							{
								goto IL_8AE;
							}
							break;
						default:
							goto IL_8AE;
						}
						this.destroyOvernight = true;
						goto IL_8AE;
						IL_822:
						if (Game1.dayOfMonth == 1 && !location.IsSpringHere() && location.isOutdoors.Value)
						{
							num = base.ParentSheetIndex;
							base.ParentSheetIndex = num + 1;
							goto IL_8AE;
						}
						goto IL_8AE;
						IL_856:
						if (Game1.dayOfMonth == 1 && location.IsSummerHere() && location.isOutdoors.Value)
						{
							base.ParentSheetIndex += 2;
							goto IL_8AE;
						}
						goto IL_8AE;
						IL_883:
						if (Game1.dayOfMonth == 1 && location.IsFallHere() && location.isOutdoors.Value)
						{
							base.ParentSheetIndex += 2;
							goto IL_8AE;
						}
						goto IL_8AE;
					}
				}
				else if (num != 15)
				{
					if (num != 21)
					{
						goto IL_8AE;
					}
					if (!(qualifiedItemId == "(BC)StatueOfBlessings"))
					{
						goto IL_8AE;
					}
				}
				else
				{
					if (!(qualifiedItemId == "(BC)MushroomLog"))
					{
						goto IL_8AE;
					}
					if (Game1.IsRainingHere(location))
					{
						this.minutesUntilReady.Value -= Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
						goto IL_8AE;
					}
					goto IL_8AE;
				}
				IL_37A:
				this.showNextIndex.Value = false;
			}
			IL_8AE:
			if (this.bigCraftable.Value && this.name.Contains("Seasonal"))
			{
				int baseIndex = base.ParentSheetIndex - base.ParentSheetIndex % 4;
				base.ParentSheetIndex = baseIndex + location.GetSeasonIndex();
			}
		}

		// Token: 0x060012F5 RID: 4853 RVA: 0x000E18C8 File Offset: 0x000DFAC8
		public virtual void rot()
		{
			Random r = Utility.CreateRandom((double)Game1.year * 999.0, (double)Game1.dayOfMonth, (double)Game1.seasonIndex, 0.0, 0.0);
			this.SetIdAndSprite(r.Choose(747, 748));
			this.price.Value = 0;
			this.quality.Value = 0;
			this.name = "Rotten Plant";
			this.displayName = null;
			this.lightSource = null;
			this.bigCraftable.Value = false;
		}

		// Token: 0x060012F6 RID: 4854 RVA: 0x000E195C File Offset: 0x000DFB5C
		public override void actionWhenBeingHeld(Farmer who)
		{
			GameLocation location = who.currentLocation;
			if (location != null)
			{
				if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					GameLocation gameLocation = location;
					LightSource lightSource = this.lightSource;
					gameLocation.removeLightSource((lightSource != null) ? lightSource.Id : null);
					base.actionWhenBeingHeld(who);
					return;
				}
				if (this.lightSource != null && (!this.bigCraftable.Value || this.isLamp.Value))
				{
					if (!location.hasLightSource(this.lightSource.Id))
					{
						location.sharedLights.AddLight(new LightSource(this.lightSource.Id, this.lightSource.textureIndex.Value, this.lightSource.position.Value, this.lightSource.radius.Value, this.lightSource.color.Value, LightSource.LightContext.None, who.UniqueMultiplayerID, location.NameOrUniqueName));
					}
					location.repositionLightSource(this.lightSource.Id, who.Position + new Vector2(32f, -64f));
				}
			}
			base.actionWhenBeingHeld(who);
		}

		// Token: 0x060012F7 RID: 4855 RVA: 0x000E1A85 File Offset: 0x000DFC85
		public override void actionWhenStopBeingHeld(Farmer who)
		{
			GameLocation currentLocation = who.currentLocation;
			if (currentLocation != null)
			{
				LightSource lightSource = this.lightSource;
				currentLocation.removeLightSource((lightSource != null) ? lightSource.Id : null);
			}
			base.actionWhenStopBeingHeld(who);
		}

		// Token: 0x060012F8 RID: 4856 RVA: 0x000E1AB1 File Offset: 0x000DFCB1
		public static void ConsumeInventoryItem(Farmer who, Item drop_in, int amount)
		{
			if (drop_in.ConsumeStack(amount) == null)
			{
				(Object.autoLoadFrom ?? who.Items).RemoveButKeepEmptySlot(drop_in);
				IInventory inventory = Object.autoLoadFrom;
				if (inventory == null)
				{
					return;
				}
				inventory.RemoveEmptySlots();
			}
		}

		// Token: 0x060012F9 RID: 4857 RVA: 0x000E1AE4 File Offset: 0x000DFCE4
		public virtual bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			if (this.isTemporarilyInvisible)
			{
				return false;
			}
			Object dropIn = dropInItem as Object;
			if (dropIn == null)
			{
				return false;
			}
			GameLocation location = this.Location;
			if (this.IsSprinkler())
			{
				if (this.heldObject.Value == null && (dropIn.QualifiedItemId == "(O)915" || dropIn.QualifiedItemId == "(O)913"))
				{
					if (probe)
					{
						return true;
					}
					if (location is MineShaft || (location is VolcanoDungeon && dropIn.QualifiedItemId == "(O)913"))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
						return false;
					}
					Object attached_object = dropIn.getOne() as Object;
					if (((attached_object != null) ? attached_object.QualifiedItemId : null) == "(O)913" && attached_object.heldObject.Value == null)
					{
						Chest chest = new Chest();
						chest.SpecialChestType = Chest.SpecialChestTypes.Enricher;
						attached_object.heldObject.Value = chest;
					}
					location.playSound("axe", null, null, SoundContext.Default);
					this.heldObject.Value = attached_object;
					this.minutesUntilReady.Value = -1;
					return true;
				}
				else if (dropIn.QualifiedItemId == "(O)93" && base.SpecialVariable != 999999)
				{
					if (probe)
					{
						return true;
					}
					base.SpecialVariable = 999999;
					Game1.playSound("woodyStep", null);
					this.lightSource = new LightSource(this.GenerateLightSourceId(this.TileLocation), 4, new Vector2(this.tileLocation.X * 64f + 16f, this.tileLocation.Y * 64f + 16f), 1.25f, new Color(1, 1, 1) * 0.9f, LightSource.LightContext.None, 0L, location.NameOrUniqueName);
					return true;
				}
			}
			if (dropIn.QualifiedItemId == "(O)872" && Object.autoLoadFrom == null && this.TryApplyFairyDust(probe))
			{
				return true;
			}
			MachineData machineData = this.GetMachineData();
			if (machineData != null)
			{
				return (this.heldObject.Value == null || machineData.AllowLoadWhenFull) && (!probe || this.MinutesUntilReady <= 0) && this.PlaceInMachine(machineData, dropInItem, probe, who, true, true) && (!returnFalseIfItemConsumed || probe);
			}
			if (base.QualifiedItemId == "(BC)99" && dropIn.QualifiedItemId == "(O)178")
			{
				GameLocation rootLocation = location.GetRootLocation();
				if (rootLocation.GetHayCapacity() <= 0)
				{
					if (Object.autoLoadFrom == null && !probe)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"), true);
					}
					return false;
				}
				if (probe)
				{
					return true;
				}
				location.playSound("Ship", null, null, SoundContext.Default);
				DelayedAction.playSoundAfterDelay("grassyStep", 100, null, null, -1, false);
				if (dropIn.Stack == 0)
				{
					dropIn.Stack = 1;
				}
				int old = rootLocation.piecesOfHay.Value;
				int numLeft = rootLocation.tryToAddHay(dropIn.Stack);
				int now = rootLocation.piecesOfHay.Value;
				if (old <= 0 && now > 0)
				{
					this.showNextIndex.Value = true;
				}
				else if (now <= 0)
				{
					this.showNextIndex.Value = false;
				}
				dropIn.Stack = numLeft;
				if (numLeft <= 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x000E1E48 File Offset: 0x000E0048
		public virtual bool TryApplyFairyDust(bool probe = false)
		{
			if (this.MinutesUntilReady > 0)
			{
				MachineData machineData = this.GetMachineData();
				bool? flag = (machineData != null) ? new bool?(machineData.AllowFairyDust) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					if (!probe)
					{
						Utility.addSprinklesToLocation(this.Location, (int)this.tileLocation.X, (int)this.tileLocation.Y, 1, 2, 400, 40, Color.White, null, false);
						Game1.playSound("yoba", null);
						this.MinutesUntilReady = 10;
						DelayedAction.functionAfterDelay(delegate
						{
							this.minutesElapsed(10);
						}, 50);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x000E1EFC File Offset: 0x000E00FC
		public static Item OutputSolarPanel(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			int minutes = machine.MinutesUntilReady;
			GameLocation location = machine.Location;
			Object held_object = machine.heldObject.Value;
			if (held_object == null)
			{
				held_object = ItemRegistry.Create<Object>("(O)787", 1, 0, false);
				held_object.CanBeSetDown = false;
				minutes = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 7);
			}
			if (minutes > 0 && location.IsOutdoors && !location.IsRainingHere())
			{
				minutes = Math.Max(0, minutes - 2400);
			}
			overrideMinutesUntilReady = ((minutes != machine.MinutesUntilReady) ? new int?(minutes) : null);
			return held_object;
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x000E1F8C File Offset: 0x000E018C
		public static Item OutputStatueOfEndlessFortune(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			NPC todaysBirthdayNPC = Utility.getTodaysBirthdayNPC();
			Item item = (todaysBirthdayNPC != null) ? todaysBirthdayNPC.getFavoriteItem() : null;
			if (item != null)
			{
				return item;
			}
			string index = "80";
			switch (Game1.random.Next(4))
			{
			case 0:
				index = "72";
				break;
			case 1:
				index = "337";
				break;
			case 2:
				index = "749";
				break;
			case 3:
				index = "336";
				break;
			}
			return new Object(index, 1, false, -1, 0);
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x000E200C File Offset: 0x000E020C
		public static Item OutputDeconstructor(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			if (!inputItem.HasTypeObject() && !inputItem.HasTypeBigCraftable())
			{
				return null;
			}
			string recipe;
			if (!CraftingRecipe.craftingRecipes.TryGetValue(inputItem.Name, out recipe))
			{
				return null;
			}
			string[] fields = recipe.Split('/', StringSplitOptions.None);
			if (ArgUtility.SplitBySpace(ArgUtility.Get(fields, 2, null, true)).Length > 1)
			{
				return null;
			}
			if (inputItem.QualifiedItemId == "(O)710")
			{
				return ItemRegistry.Create("(O)334", 2, 0, false);
			}
			Object bestIngredient = null;
			string[] ingredients = ArgUtility.SplitBySpace(ArgUtility.Get(fields, 0, null, true));
			for (int i = 0; i < ingredients.Length; i += 2)
			{
				string itemId = ArgUtility.Get(ingredients, i, null, true);
				int count = ArgUtility.GetInt(ingredients, i + 1, 1);
				Object ingredient = new Object(itemId, count, false, -1, 0);
				if (bestIngredient == null || ingredient.sellToStorePrice(-1L) * ingredient.Stack > bestIngredient.sellToStorePrice(-1L) * bestIngredient.Stack)
				{
					bestIngredient = ingredient;
				}
			}
			return bestIngredient;
		}

		// Token: 0x060012FE RID: 4862 RVA: 0x000E20F8 File Offset: 0x000E02F8
		public static Item OutputAnvil(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			Trinket t = inputItem as Trinket;
			if (t == null)
			{
				return null;
			}
			if (!t.GetTrinketData().CanBeReforged)
			{
				if (!probe)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Anvil_wrongtrinket"), true);
				}
				return null;
			}
			Trinket output = (Trinket)inputItem.getOne();
			if (!output.RerollStats(Game1.random.Next(9999999)))
			{
				if (!probe && player != null)
				{
					player.doEmote(40);
				}
				return null;
			}
			if (!probe)
			{
				Game1.currentLocation.playSound("metal_tap", null, null, SoundContext.Default);
				DelayedAction.playSoundAfterDelay("metal_tap", 250, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("metal_tap", 500, null, null, -1, false);
			}
			overrideMinutesUntilReady = new int?(10);
			return output;
		}

		// Token: 0x060012FF RID: 4863 RVA: 0x000E21E0 File Offset: 0x000E03E0
		public static Item OutputGeodeCrusher(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			if (!Utility.IsGeode(inputItem, true))
			{
				return null;
			}
			Item treasureFromGeode = Utility.getTreasureFromGeode(inputItem);
			if (!probe)
			{
				GameLocation location = machine.Location;
				Vector2 pixelPos = machine.tileLocation.Value * 64f;
				Utility.addSmokePuff(location, pixelPos + new Vector2(4f, -48f), 200, 2f, 0.02f, 0.75f, 0.002f);
				Utility.addSmokePuff(location, pixelPos + new Vector2(-16f, -56f), 300, 2f, 0.02f, 0.75f, 0.002f);
				Utility.addSmokePuff(location, pixelPos + new Vector2(16f, -52f), 400, 2f, 0.02f, 0.75f, 0.002f);
				Utility.addSmokePuff(location, pixelPos + new Vector2(32f, -56f), 200, 2f, 0.02f, 0.75f, 0.002f);
				Utility.addSmokePuff(location, pixelPos + new Vector2(40f, -44f), 500, 2f, 0.02f, 0.75f, 0.002f);
			}
			return treasureFromGeode;
		}

		// Token: 0x06001300 RID: 4864 RVA: 0x000E232C File Offset: 0x000E052C
		public static Item OutputIncubator(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			Building parentBuilding = machine.Location.ParentBuilding;
			BuildingData buildingData = (parentBuilding != null) ? parentBuilding.GetData() : null;
			if (buildingData == null)
			{
				overrideMinutesUntilReady = null;
				return null;
			}
			FarmAnimalData animalData = FarmAnimal.GetAnimalDataFromEgg(inputItem, machine.Location);
			if (animalData == null || !buildingData.ValidOccupantTypes.Contains(animalData.House))
			{
				overrideMinutesUntilReady = null;
				return null;
			}
			overrideMinutesUntilReady = new int?((animalData.IncubationTime > 0) ? animalData.IncubationTime : 9000);
			return inputItem.getOne();
		}

		// Token: 0x06001301 RID: 4865 RVA: 0x000E23B4 File Offset: 0x000E05B4
		public static Item OutputSeedMaker(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			if (!inputItem.HasTypeObject())
			{
				return null;
			}
			string seed = null;
			foreach (KeyValuePair<string, CropData> v in Game1.cropData)
			{
				if (ItemRegistry.HasItemId(inputItem, v.Value.HarvestItemId))
				{
					seed = v.Key;
					break;
				}
			}
			if (seed == null)
			{
				return null;
			}
			Vector2 tile = machine.tileLocation.Value;
			Random r = Utility.CreateDaySaveRandom((double)tile.X, (double)(tile.Y * 77f), (double)Game1.timeOfDay);
			Item output;
			if (r.NextDouble() < 0.005)
			{
				output = new Object("499", 1, false, -1, 0);
			}
			else if (r.NextDouble() < 0.02)
			{
				output = new Object("770", r.Next(1, 5), false, -1, 0);
			}
			else
			{
				output = new Object(seed, r.Next(1, 4), false, -1, 0);
			}
			return output;
		}

		// Token: 0x06001302 RID: 4866 RVA: 0x000E24C0 File Offset: 0x000E06C0
		public static Item OutputMushroomLog(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			List<Tree> nearbyTrees = new List<Tree>();
			for (int x = (int)machine.TileLocation.X - 3; x < (int)machine.TileLocation.X + 4; x++)
			{
				for (int y = (int)machine.TileLocation.Y - 3; y < (int)machine.TileLocation.Y + 4; y++)
				{
					Vector2 v = new Vector2((float)x, (float)y);
					Tree nearbyTree = machine.Location.terrainFeatures.GetValueOrDefault(v, null) as Tree;
					if (nearbyTree != null)
					{
						nearbyTrees.Add(nearbyTree);
					}
				}
			}
			int treeCount = nearbyTrees.Count;
			List<string> mushroomPossibilities = new List<string>();
			int mossyCount = 0;
			foreach (Tree t in nearbyTrees)
			{
				if (t.growthStage.Value >= 5)
				{
					string mushroomType = Game1.random.NextBool(0.05) ? "(O)422" : (Game1.random.NextBool(0.15) ? "(O)420" : "(O)404");
					string value = t.treeType.Value;
					if (!(value == "2"))
					{
						if (!(value == "1"))
						{
							if (!(value == "3"))
							{
								if (value == "13")
								{
									mushroomType = "(O)422";
								}
							}
							else
							{
								mushroomType = "(O)281";
							}
						}
						else
						{
							mushroomType = "(O)257";
						}
					}
					else
					{
						mushroomType = (Game1.random.NextBool(0.1) ? "(O)422" : "(O)420");
					}
					mushroomPossibilities.Add(mushroomType);
					if (t.hasMoss.Value)
					{
						mossyCount++;
					}
				}
			}
			for (int i = 0; i < Math.Max(1, (int)((float)nearbyTrees.Count * 0.75f)); i++)
			{
				mushroomPossibilities.Add(Game1.random.NextBool(0.05) ? "(O)422" : (Game1.random.NextBool(0.15) ? "(O)420" : "(O)404"));
			}
			int amount = Math.Max(1, Math.Min(5, Game1.random.Next(1, 3) * (nearbyTrees.Count / 2)));
			int quality = 0;
			float qualityBoostChance = (float)mossyCount * 0.025f + (float)treeCount * 0.025f;
			while (Game1.random.NextDouble() < (double)qualityBoostChance)
			{
				quality++;
				if (quality == 3)
				{
					quality = 4;
					break;
				}
			}
			return ItemRegistry.Create(Game1.random.ChooseFrom(mushroomPossibilities), amount, quality, false);
		}

		// Token: 0x06001303 RID: 4867 RVA: 0x000E2788 File Offset: 0x000E0988
		public bool ParseItemCount(string[] query, out string replacement, Random random, Farmer player)
		{
			if (query[0] == "ItemCount")
			{
				replacement = Object.CurrentParsedItemCount.ToString();
				return true;
			}
			replacement = null;
			return false;
		}

		// Token: 0x06001304 RID: 4868 RVA: 0x000E27AC File Offset: 0x000E09AC
		public bool PlaceInMachine(MachineData machineData, Item inputItem, bool probe, Farmer who, bool showMessages = true, bool playSounds = true)
		{
			if (machineData == null || inputItem == null)
			{
				return false;
			}
			if (this.heldObject.Value != null)
			{
				if (!machineData.AllowLoadWhenFull)
				{
					return false;
				}
				string qualifiedItemId = inputItem.QualifiedItemId;
				Item value = this.lastInputItem.Value;
				if (qualifiedItemId == ((value != null) ? value.QualifiedItemId : null))
				{
					return false;
				}
			}
			MachineItemAdditionalConsumedItems failedRequirement;
			if (!MachineDataUtility.HasAdditionalRequirements(Object.autoLoadFrom ?? who.Items, machineData.AdditionalConsumedItems, out failedRequirement))
			{
				if (showMessages && failedRequirement.InvalidCountMessage != null && !probe && Object.autoLoadFrom == null)
				{
					Object.CurrentParsedItemCount = failedRequirement.RequiredCount;
					Game1.showRedMessage(TokenParser.ParseText(failedRequirement.InvalidCountMessage, null, new TokenParserDelegate(this.ParseItemCount), null), true);
					who.ignoreItemConsumptionThisFrame = true;
				}
				return false;
			}
			GameLocation location = this.Location;
			MachineOutputRule outputRule;
			MachineOutputTriggerRule triggerRule;
			MachineOutputRule outputRuleIgnoringCount;
			MachineOutputTriggerRule triggerIgnoringCount;
			if (!MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out outputRule, out triggerRule, out outputRuleIgnoringCount, out triggerIgnoringCount))
			{
				if (showMessages && !probe && Object.autoLoadFrom == null)
				{
					if (outputRuleIgnoringCount != null)
					{
						string invalidCountMessage = outputRuleIgnoringCount.InvalidCountMessage ?? machineData.InvalidCountMessage;
						if (!string.IsNullOrWhiteSpace(invalidCountMessage))
						{
							Object.CurrentParsedItemCount = triggerIgnoringCount.RequiredCount;
							Game1.showRedMessage(TokenParser.ParseText(invalidCountMessage, null, new TokenParserDelegate(this.ParseItemCount), null), true);
							who.ignoreItemConsumptionThisFrame = true;
						}
					}
					else if (machineData.InvalidItemMessage != null && GameStateQuery.CheckConditions(machineData.InvalidItemMessageCondition, location, who, null, who.ActiveObject, null, null))
					{
						Game1.showRedMessage(TokenParser.ParseText(machineData.InvalidItemMessage, null, null, null), true);
						who.ignoreItemConsumptionThisFrame = true;
					}
				}
				return false;
			}
			if (probe)
			{
				return true;
			}
			if (!this.OutputMachine(machineData, outputRule, inputItem, who, location, probe, false))
			{
				return false;
			}
			if (machineData.AdditionalConsumedItems != null)
			{
				IInventory inventory = Object.autoLoadFrom ?? who.Items;
				foreach (MachineItemAdditionalConsumedItems additionalRequirement in machineData.AdditionalConsumedItems)
				{
					inventory.ReduceId(additionalRequirement.ItemId, additionalRequirement.RequiredCount);
				}
			}
			if (triggerRule.RequiredCount > 0)
			{
				Object.ConsumeInventoryItem(who, inputItem, triggerRule.RequiredCount);
			}
			if (machineData.LoadEffects != null)
			{
				foreach (MachineEffects effect in machineData.LoadEffects)
				{
					if (this.PlayMachineEffect(effect, playSounds))
					{
						this._machineAnimation = effect;
						this._machineAnimationLoop = false;
						this._machineAnimationIndex = 0;
						this._machineAnimationFrame = -1;
						this._machineAnimationInterval = 0;
						break;
					}
				}
			}
			this.playCustomMachineLoadEffects();
			MachineDataUtility.UpdateStats(machineData.StatsToIncrementWhenLoaded, inputItem, 1);
			return true;
		}

		// Token: 0x06001305 RID: 4869 RVA: 0x000E2A5C File Offset: 0x000E0C5C
		private void playCustomMachineLoadEffects()
		{
			if (base.ItemId == "FishSmoker")
			{
				for (int i = 0; i < 12; i++)
				{
					this.Location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), 9999f, 1, 1, new Vector2((float)((int)this.TileLocation.X * 64) + 18f, ((float)((int)this.TileLocation.Y) - 1.15f) * 64f), false, false)
					{
						color = new Color(60, 60, 60),
						alphaFade = -0.02f,
						alpha = 0.01f,
						alphaFadeFade = -0.0003f,
						motion = new Vector2(0.25f, -0.1f),
						acceleration = new Vector2(0f, -0.01f),
						rotationChange = (float)Game1.random.Next(-10, 10) / 500f,
						scale = 1.5f,
						scaleChange = 0.024f,
						layerDepth = Math.Max(0f, ((this.tileLocation.Y + 1f) * 64f - 24f + (float)i) / 10000f) + this.tileLocation.X * 1E-05f,
						delayBeforeAnimationStart = i * 550
					});
				}
			}
		}

		// Token: 0x06001306 RID: 4870 RVA: 0x000E2BE0 File Offset: 0x000E0DE0
		public virtual bool OutputMachine(MachineData machine, MachineOutputRule outputRule, Item inputItem, Farmer who, GameLocation location, bool probe, bool heldObjectOnly = false)
		{
			who = (who ?? Game1.MasterPlayer);
			if (machine == null || (this.heldObject.Value != null && !machine.AllowLoadWhenFull))
			{
				return false;
			}
			MachineOutputTriggerRule machineOutputTriggerRule;
			MachineOutputRule machineOutputRule;
			MachineOutputTriggerRule machineOutputTriggerRule2;
			if (outputRule == null && !MachineDataUtility.TryGetMachineOutputRule(this, machine, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out outputRule, out machineOutputTriggerRule, out machineOutputRule, out machineOutputTriggerRule2))
			{
				return false;
			}
			MachineItemOutput outputData = MachineDataUtility.GetOutputData(this, machine, outputRule, inputItem, who, location);
			int? overrideMinutesUntilReady;
			Item newHeldItem = MachineDataUtility.GetOutputItem(this, outputData, inputItem, who, heldObjectOnly || probe, out overrideMinutesUntilReady);
			if (newHeldItem == null)
			{
				return false;
			}
			if (probe)
			{
				return true;
			}
			newHeldItem.FixQuality();
			newHeldItem.FixStackSize();
			this.heldObject.Value = (Object)newHeldItem;
			if (!heldObjectOnly)
			{
				int minutesUntilReady = 0;
				int? num = overrideMinutesUntilReady;
				int num2 = 0;
				if (num.GetValueOrDefault() >= num2 & num != null)
				{
					minutesUntilReady = overrideMinutesUntilReady.Value;
				}
				else if (outputRule.MinutesUntilReady >= 0 || outputRule.DaysUntilReady >= 0)
				{
					minutesUntilReady = ((outputRule.DaysUntilReady >= 0) ? Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, outputRule.DaysUntilReady) : outputRule.MinutesUntilReady);
				}
				minutesUntilReady = (int)Utility.ApplyQuantityModifiers((float)minutesUntilReady, machine.ReadyTimeModifiers, machine.ReadyTimeModifierMode, location, who, this.heldObject.Value, inputItem, null);
				this.MinutesUntilReady = minutesUntilReady;
				if (this.MinutesUntilReady == 0)
				{
					this.readyForHarvest.Value = true;
				}
				this.lastOutputRuleId.Value = outputRule.Id;
				if (inputItem != null)
				{
					this.lastInputItem.Value = inputItem.getOne();
					this.lastInputItem.Value.Stack = inputItem.Stack;
				}
				else
				{
					this.lastInputItem.Value = null;
				}
				if (machine.IsIncubator)
				{
					AnimalHouse animalHouse = location as AnimalHouse;
					if (animalHouse != null)
					{
						animalHouse.hasShownIncubatorBuildingFullMessage = false;
					}
				}
				base.ResetParentSheetIndex();
				base.ParentSheetIndex += outputData.IncrementMachineParentSheetIndex;
				if (machine.LightWhileWorking != null)
				{
					this.initializeLightSource(this.tileLocation.Value, false);
				}
				if (machine.ShowNextIndexWhileWorking)
				{
					this.showNextIndex.Value = true;
				}
				if (machine.WobbleWhileWorking)
				{
					this.scale.X = 5f;
				}
				this.minutesElapsed(0);
			}
			return true;
		}

		// Token: 0x06001307 RID: 4871 RVA: 0x000E2DF2 File Offset: 0x000E0FF2
		public virtual bool PlayMachineEffect(MachineEffects effect, bool playSounds = true)
		{
			return MachineDataUtility.PlayEffects(this, effect, playSounds);
		}

		// Token: 0x06001308 RID: 4872 RVA: 0x000E2DFC File Offset: 0x000E0FFC
		public virtual void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (environment == null)
			{
				return;
			}
			if (this.readyForHarvest.Value && !this._hasHeldObject)
			{
				this.readyForHarvest.Value = false;
			}
			if (this._hasLightSource)
			{
				LightSource light = this.netLightSource.Get();
				if (light != null && this.isOn.Value && !environment.hasLightSource(light.Id))
				{
					environment.sharedLights.AddLight(light.Clone());
				}
			}
			if (this._machineAnimation != null)
			{
				List<int> frames = this._machineAnimation.Frames;
				if (frames != null && frames.Count > 0)
				{
					this._machineAnimationInterval += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this._machineAnimation.Interval > 0 && this._machineAnimationInterval >= this._machineAnimation.Interval)
					{
						this._machineAnimationIndex += this._machineAnimationInterval / this._machineAnimation.Interval;
						this._machineAnimationInterval %= this._machineAnimation.Interval;
						if (this._machineAnimationIndex >= this._machineAnimation.Frames.Count)
						{
							if (this._machineAnimationLoop)
							{
								this._machineAnimationIndex %= this._machineAnimation.Frames.Count;
							}
							else
							{
								this._machineAnimation = null;
								this._machineAnimationFrame = -1;
							}
						}
					}
					if (this._machineAnimation != null)
					{
						this._machineAnimationFrame = this._machineAnimation.Frames[this._machineAnimationIndex];
					}
				}
				else
				{
					this._machineAnimationFrame = -1;
				}
			}
			if (this._hasHeldObject)
			{
				Object heldObject = this.heldObject.Get();
				if (heldObject.QualifiedItemId == "(O)913" && this.IsSprinkler())
				{
					Chest chest = heldObject.heldObject.Value as Chest;
					if (chest != null)
					{
						chest.mutex.Update(environment);
						if (Game1.activeClickableMenu == null && chest.GetMutex().IsLockHeld())
						{
							chest.GetMutex().ReleaseLock();
						}
					}
				}
				if (heldObject._hasLightSource)
				{
					this.lightSource = heldObject.netLightSource.Get();
					if (this.lightSource != null && !environment.hasLightSource(this.lightSource.Id))
					{
						environment.sharedLights.AddLight(this.lightSource.Clone());
					}
				}
				if (!this.readyForHarvest.Value)
				{
					if (this._machineAnimation != null)
					{
						goto IL_3FF;
					}
					MachineData data = this.GetMachineData();
					if (((data != null) ? data.WorkingEffects : null) == null)
					{
						goto IL_3FF;
					}
					using (List<MachineEffects>.Enumerator enumerator = data.WorkingEffects.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							MachineEffects effect = enumerator.Current;
							if (effect != null)
							{
								string condition = effect.Condition;
								GameLocation location = this.Location;
								Farmer player = null;
								Item value = this.lastInputItem.Value;
								if (GameStateQuery.CheckConditions(condition, location, player, heldObject, value, null, null))
								{
									this._machineAnimation = effect;
									this._machineAnimationLoop = true;
									this._machineAnimationIndex = 0;
									this._machineAnimationFrame = -1;
									MachineEffects machineAnimation = this._machineAnimation;
									bool flag;
									if (machineAnimation == null)
									{
										flag = false;
									}
									else
									{
										List<int> frames2 = machineAnimation.Frames;
										int? num = (frames2 != null) ? new int?(frames2.Count) : null;
										int num2 = 0;
										flag = (num.GetValueOrDefault() > num2 & num != null);
									}
									this._machineAnimationInterval = ((flag && this._machineAnimation.Interval > 0) ? ((int)(((double)((long)(this.tileLocation.X * (float)(this._machineAnimation.Interval / 2) + this.tileLocation.Y * (float)(this._machineAnimation.Interval / 2 * 10))) + time.TotalGameTime.TotalMilliseconds) % (double)(this._machineAnimation.Interval * this._machineAnimation.Frames.Count))) : 0);
									break;
								}
							}
						}
						goto IL_3FF;
					}
				}
				if (this._machineAnimation != null && this._machineAnimationLoop)
				{
					this._machineAnimation = null;
				}
			}
			else if (this._machineAnimation != null && this._machineAnimationLoop)
			{
				this._machineAnimation = null;
			}
			IL_3FF:
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shakeTimer <= 0)
				{
					this.health = 10;
				}
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(O)590") && !(qualifiedItemId == "(O)SeedSpot"))
			{
				if (qualifiedItemId == "(BC)56")
				{
					base.ResetParentSheetIndex();
					base.ParentSheetIndex += (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0);
				}
			}
			else if (Game1.random.NextDouble() < 0.01)
			{
				this.shakeTimer = 100;
			}
			if (this.IsTextSign())
			{
				if (this.shouldShowSign)
				{
					this.shouldShowSign = false;
					this.lastNoteBlockSoundTime += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.lastNoteBlockSoundTime > 125)
					{
						this.lastNoteBlockSoundTime = 125;
						return;
					}
				}
				else if (this.lastNoteBlockSoundTime > 0)
				{
					this.lastNoteBlockSoundTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.lastNoteBlockSoundTime < 0)
					{
						this.lastNoteBlockSoundTime = 0;
					}
				}
			}
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x000E3360 File Offset: 0x000E1560
		public virtual void actionOnPlayerEntry()
		{
			this.isTemporarilyInvisible = false;
			this.health = 10;
			if (base.QualifiedItemId == "(BC)99")
			{
				this.showNextIndex.Value = (this.Location.GetRootLocation().piecesOfHay.Value > 0);
			}
		}

		// Token: 0x0600130A RID: 4874 RVA: 0x000E33B4 File Offset: 0x000E15B4
		public override bool canBeTrashed()
		{
			ObjectData data;
			return !this.questItem.Value && base.canBeTrashed() && (!Game1.objectData.TryGetValue(base.ItemId, out data) || data.CanBeTrashed);
		}

		// Token: 0x0600130B RID: 4875 RVA: 0x000E33F4 File Offset: 0x000E15F4
		public virtual bool isForage()
		{
			return base.Category == -79 || base.Category == -81 || base.Category == -80 || base.Category == -75 || base.Category == -23 || base.HasContextTag("forage_item") || base.QualifiedItemId == "(O)430";
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x000E3454 File Offset: 0x000E1654
		public virtual void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
		{
			if (this.name == "Error Item")
			{
				return;
			}
			Furniture furniture = this as Furniture;
			if (furniture != null && furniture.furniture_type.Value == 14 && furniture.isOn.Value)
			{
				string id = this.GenerateLightSourceId(tileLocation);
				int textureIndex = 4;
				Vector2 position = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f);
				float radius = 2.5f;
				Color color = new Color(0, 80, 160);
				LightSource.LightContext lightContext = LightSource.LightContext.None;
				long playerID = 0L;
				GameLocation location = this.Location;
				this.lightSource = new LightSource(id, textureIndex, position, radius, color, lightContext, playerID, (location != null) ? location.NameOrUniqueName : null);
				return;
			}
			if (furniture != null && furniture.furniture_type.Value == 16 && furniture.isOn.Value)
			{
				string id2 = this.GenerateLightSourceId(tileLocation);
				int textureIndex2 = 4;
				Vector2 position2 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f);
				float radius2 = 1.5f;
				Color color2 = new Color(0, 80, 160);
				LightSource.LightContext lightContext2 = LightSource.LightContext.None;
				long playerID2 = 0L;
				GameLocation location2 = this.Location;
				this.lightSource = new LightSource(id2, textureIndex2, position2, radius2, color2, lightContext2, playerID2, (location2 != null) ? location2.NameOrUniqueName : null);
				return;
			}
			if (this.bigCraftable.Value)
			{
				if (this is Torch && this.isOn.Value)
				{
					float y_offset = -64f;
					if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "campfire_item"))
					{
						y_offset = 32f;
					}
					string id3 = this.GenerateLightSourceId(tileLocation);
					int textureIndex3 = 4;
					Vector2 position3 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + y_offset);
					float radius3 = 2.5f;
					Color color3 = new Color(0, 80, 160);
					LightSource.LightContext lightContext3 = LightSource.LightContext.None;
					long playerID3 = 0L;
					GameLocation location3 = this.Location;
					this.lightSource = new LightSource(id3, textureIndex3, position3, radius3, color3, lightContext3, playerID3, (location3 != null) ? location3.NameOrUniqueName : null);
					return;
				}
				if (this.isLamp.Value)
				{
					string id4 = this.GenerateLightSourceId(tileLocation);
					int textureIndex4 = 4;
					Vector2 position4 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f);
					float radius4 = 3f;
					Color color4 = new Color(0, 40, 80);
					LightSource.LightContext lightContext4 = LightSource.LightContext.None;
					long playerID4 = 0L;
					GameLocation location4 = this.Location;
					this.lightSource = new LightSource(id4, textureIndex4, position4, radius4, color4, lightContext4, playerID4, (location4 != null) ? location4.NameOrUniqueName : null);
					return;
				}
				string a = base.QualifiedItemId;
				if (a == "(BC)74")
				{
					string id5 = this.GenerateLightSourceId(tileLocation);
					int textureIndex5 = 4;
					Vector2 position5 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f);
					float radius5 = 1.5f;
					Color darkCyan = Color.DarkCyan;
					LightSource.LightContext lightContext5 = LightSource.LightContext.None;
					long playerID5 = 0L;
					GameLocation location5 = this.Location;
					this.lightSource = new LightSource(id5, textureIndex5, position5, radius5, darkCyan, lightContext5, playerID5, (location5 != null) ? location5.NameOrUniqueName : null);
					return;
				}
				if (a == "(BC)96")
				{
					string id6 = this.GenerateLightSourceId(tileLocation);
					int textureIndex6 = 4;
					Vector2 position6 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f);
					float radius6 = 1f;
					Color color5 = Color.HotPink * 0.75f;
					LightSource.LightContext lightContext6 = LightSource.LightContext.None;
					long playerID6 = 0L;
					GameLocation location6 = this.Location;
					this.lightSource = new LightSource(id6, textureIndex6, position6, radius6, color5, lightContext6, playerID6, (location6 != null) ? location6.NameOrUniqueName : null);
					return;
				}
			}
			else if (Utility.IsNormalObjectAtParentSheetIndex(this, base.ItemId) || this is Torch)
			{
				if (base.QualifiedItemId == "(O)95" || ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "torch_item"))
				{
					string a = base.ItemId;
					Color c;
					if (!(a == "94"))
					{
						if (!(a == "95"))
						{
							c = new Color(1, 1, 1) * 0.9f;
						}
						else
						{
							c = new Color(70, 0, 150) * 0.9f;
						}
					}
					else
					{
						c = Color.Yellow;
					}
					string id7 = this.GenerateLightSourceId(tileLocation);
					int textureIndex7 = 4;
					Vector2 position7 = new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f);
					float radius7 = mineShaft ? 1.5f : 1.25f;
					Color color6 = c;
					LightSource.LightContext lightContext7 = LightSource.LightContext.None;
					long playerID7 = 0L;
					GameLocation location7 = this.Location;
					this.lightSource = new LightSource(id7, textureIndex7, position7, radius7, color6, lightContext7, playerID7, (location7 != null) ? location7.NameOrUniqueName : null);
					return;
				}
				if (base.QualifiedItemId == "(O)746")
				{
					string id8 = this.GenerateLightSourceId(tileLocation);
					int textureIndex8 = 4;
					Vector2 position8 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 48f);
					float radius8 = 0.5f;
					Color color7 = new Color(1, 1, 1) * 0.65f;
					LightSource.LightContext lightContext8 = LightSource.LightContext.None;
					long playerID8 = 0L;
					GameLocation location8 = this.Location;
					this.lightSource = new LightSource(id8, textureIndex8, position8, radius8, color7, lightContext8, playerID8, (location8 != null) ? location8.NameOrUniqueName : null);
					return;
				}
				if (this.IsSprinkler() && base.SpecialVariable == 999999)
				{
					string id9 = this.GenerateLightSourceId(tileLocation);
					int textureIndex9 = 4;
					Vector2 position9 = new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f);
					float radius9 = 1.25f;
					Color color8 = new Color(1, 1, 1) * 0.9f;
					LightSource.LightContext lightContext9 = LightSource.LightContext.None;
					long playerID9 = 0L;
					GameLocation location9 = this.Location;
					this.lightSource = new LightSource(id9, textureIndex9, position9, radius9, color8, lightContext9, playerID9, (location9 != null) ? location9.NameOrUniqueName : null);
				}
			}
			if (this.MinutesUntilReady > 0)
			{
				MachineData machineData = this.GetMachineData();
				MachineLight light = (machineData != null) ? machineData.LightWhileWorking : null;
				if (light != null)
				{
					string id10 = this.GenerateLightSourceId(tileLocation);
					int textureIndex10 = 4;
					Vector2 position10 = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f);
					float radius10 = light.Radius;
					Color color9 = Utility.StringToColor(light.Color) ?? Color.White;
					LightSource.LightContext lightContext10 = LightSource.LightContext.None;
					long playerID10 = 0L;
					GameLocation location10 = this.Location;
					this.lightSource = new LightSource(id10, textureIndex10, position10, radius10, color9, lightContext10, playerID10, (location10 != null) ? location10.NameOrUniqueName : null);
				}
			}
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x000E3A04 File Offset: 0x000E1C04
		public virtual void performRemoveAction()
		{
			GameLocation environment = this.Location;
			Vector2 tileLocation = this.TileLocation;
			if (environment != null)
			{
				GameLocation gameLocation = environment;
				LightSource lightSource = this.lightSource;
				gameLocation.removeLightSource((lightSource != null) ? lightSource.Id : null);
				TerrainFeature terrainFeature;
				if (this.IsTapper() && environment.terrainFeatures != null && environment.terrainFeatures.TryGetValue(tileLocation, out terrainFeature))
				{
					Tree tree = terrainFeature as Tree;
					if (tree != null)
					{
						tree.tapped.Value = false;
					}
				}
				if (this.IsSprinkler())
				{
					environment.removeTemporarySpritesWithID((int)tileLocation.X * 4000 + (int)tileLocation.Y);
				}
			}
			if (base.QualifiedItemId == "(BC)126")
			{
				string id = (this.quality.Value != 0) ? (this.quality.Value - 1).ToString() : this.preservedParentSheetIndex.Value;
				if (id != null)
				{
					Game1.createItemDebris(new Hat(id), tileLocation * 64f, (Game1.player.FacingDirection + 2) % 4, null, -1, false);
					this.quality.Value = 0;
					this.preservedParentSheetIndex.Value = null;
				}
			}
			if (this.name.Contains("Seasonal") && this.bigCraftable.Value)
			{
				base.ResetParentSheetIndex();
			}
		}

		// Token: 0x0600130E RID: 4878 RVA: 0x000E3B44 File Offset: 0x000E1D44
		public virtual void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
		{
			if ((this.Type == "Crafting" || this.Type == "interactive") && this.fragility.Value != 2)
			{
				location.debris.Add(new Debris(base.QualifiedItemId, origin, destination));
			}
		}

		// Token: 0x0600130F RID: 4879 RVA: 0x000E3B9C File Offset: 0x000E1D9C
		public virtual bool isPassable()
		{
			if (this.isTemporarilyInvisible)
			{
				return true;
			}
			if (this.bigCraftable.Value)
			{
				return false;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length <= 6)
				{
					if (length != 5)
					{
						if (length != 6)
						{
							goto IL_13C;
						}
						switch (qualifiedItemId[5])
						{
						case '0':
							if (!(qualifiedItemId == "(O)590"))
							{
								goto IL_13C;
							}
							break;
						case '1':
						case '2':
							goto IL_13C;
						case '3':
							if (!(qualifiedItemId == "(O)893"))
							{
								goto IL_13C;
							}
							break;
						case '4':
							if (!(qualifiedItemId == "(O)894"))
							{
								goto IL_13C;
							}
							break;
						case '5':
							if (!(qualifiedItemId == "(O)895"))
							{
								goto IL_13C;
							}
							break;
						case '6':
							if (!(qualifiedItemId == "(O)286"))
							{
								goto IL_13C;
							}
							break;
						case '7':
							if (!(qualifiedItemId == "(O)287") && !(qualifiedItemId == "(O)297"))
							{
								goto IL_13C;
							}
							break;
						case '8':
							if (!(qualifiedItemId == "(O)288"))
							{
								goto IL_13C;
							}
							break;
						default:
							goto IL_13C;
						}
					}
					else if (!(qualifiedItemId == "(O)93"))
					{
						goto IL_13C;
					}
				}
				else if (length != 11)
				{
					if (length != 19)
					{
						goto IL_13C;
					}
					if (!(qualifiedItemId == "(O)BlueGrassStarter"))
					{
						goto IL_13C;
					}
				}
				else if (!(qualifiedItemId == "(O)SeedSpot"))
				{
					goto IL_13C;
				}
				return true;
			}
			IL_13C:
			if (this.IsFloorPathItem())
			{
				return true;
			}
			if (base.Category != -74 && base.Category != -19)
			{
				return false;
			}
			if (this.isSapling())
			{
				return false;
			}
			qualifiedItemId = base.QualifiedItemId;
			return !(qualifiedItemId == "(O)301") && !(qualifiedItemId == "(O)302") && !(qualifiedItemId == "(O)473");
		}

		// Token: 0x06001310 RID: 4880 RVA: 0x000E3D40 File Offset: 0x000E1F40
		public virtual void reloadSprite()
		{
			this.initializeLightSource(this.tileLocation.Value, false);
		}

		// Token: 0x06001311 RID: 4881 RVA: 0x000E3D54 File Offset: 0x000E1F54
		public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			Vector2 tile = this.tileLocation.Value;
			return this.GetBoundingBoxAt((int)tile.X, (int)tile.Y);
		}

		// Token: 0x06001312 RID: 4882 RVA: 0x000E3D84 File Offset: 0x000E1F84
		public virtual Microsoft.Xna.Framework.Rectangle GetBoundingBoxAt(int x, int y)
		{
			Microsoft.Xna.Framework.Rectangle bounds = this.boundingBox.Value;
			if ((this is Torch && !this.bigCraftable.Value) || base.QualifiedItemId == "(O)590")
			{
				bounds.X = (int)this.tileLocation.X * 64 + 24;
				bounds.Y = (int)this.tileLocation.Y * 64 + 24;
			}
			else
			{
				bounds.X = (int)this.tileLocation.X * 64;
				bounds.Y = (int)this.tileLocation.Y * 64;
			}
			if (this.boundingBox.Value != bounds)
			{
				this.boundingBox.Value = bounds;
			}
			return bounds;
		}

		// Token: 0x06001313 RID: 4883 RVA: 0x000E3E44 File Offset: 0x000E2044
		public override bool canBeGivenAsGift()
		{
			ObjectData data;
			return !this.bigCraftable.Value && !(this is Furniture) && !(this is Wallpaper) && (!Game1.objectData.TryGetValue(base.ItemId, out data) || data.CanBeGivenAsGift);
		}

		// Token: 0x06001314 RID: 4884 RVA: 0x000E3E8C File Offset: 0x000E208C
		public virtual bool performDropDownAction(Farmer who)
		{
			if (who == null)
			{
				who = (Game1.GetPlayer(this.owner.Value, false) ?? Game1.player);
			}
			GameLocation location = this.Location;
			MachineData machineData = this.GetMachineData();
			MachineOutputRule outputRule;
			MachineOutputTriggerRule machineOutputTriggerRule;
			MachineOutputRule machineOutputRule;
			MachineOutputTriggerRule machineOutputTriggerRule2;
			if (MachineDataUtility.TryGetMachineOutputRule(this, machineData, MachineOutputTrigger.MachinePutDown, null, who, location, out outputRule, out machineOutputTriggerRule, out machineOutputRule, out machineOutputTriggerRule2))
			{
				this.OutputMachine(machineData, outputRule, null, who, location, false, false);
				return false;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(BC)96"))
			{
				if (qualifiedItemId == "(BC)99")
				{
					this.showNextIndex.Value = (location.GetRootLocation().piecesOfHay.Value >= 0);
				}
			}
			else
			{
				this.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 3);
			}
			return false;
		}

		// Token: 0x06001315 RID: 4885 RVA: 0x000E3F4C File Offset: 0x000E214C
		private void totemWarp(Farmer who)
		{
			GameLocation location = who.currentLocation;
			for (int i = 0; i < 12; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), (float)Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), false, Game1.random.NextBool())
				});
			}
			who.playNearbySoundAll("wand", null, SoundContext.Default);
			Game1.displayFarmer = false;
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = -2000;
			Game1.player.freezePause = 1000;
			Game1.flashAlpha = 1f;
			DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.totemWarpForReal), 1000);
			Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
			r.Inflate(192, 192);
			int j = 0;
			Point playerTile = who.TilePoint;
			for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(6, new Vector2((float)x, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = j * 25,
						motion = new Vector2(-0.25f, 0f)
					}
				});
				j++;
			}
		}

		// Token: 0x06001316 RID: 4886 RVA: 0x000E414C File Offset: 0x000E234C
		private void totemWarpForReal()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(O)688"))
			{
				if (!(qualifiedItemId == "(O)689"))
				{
					if (!(qualifiedItemId == "(O)690"))
					{
						if (!(qualifiedItemId == "(O)261"))
						{
							if (qualifiedItemId == "(O)886")
							{
								Game1.warpFarmer("IslandSouth", 11, 11, false);
							}
						}
						else
						{
							Game1.warpFarmer("Desert", 35, 43, false);
						}
					}
					else
					{
						Game1.warpFarmer("Beach", 20, 4, false);
					}
				}
				else
				{
					Game1.warpFarmer("Mountain", 31, 20, false);
				}
			}
			else
			{
				Point warp_location;
				if (!Game1.getFarm().TryGetMapPropertyAs("WarpTotemEntry", out warp_location, false))
				{
					int whichFarm = Game1.whichFarm;
					if (whichFarm != 5)
					{
						if (whichFarm == 6)
						{
							warp_location = new Point(82, 29);
						}
						else
						{
							warp_location = new Point(48, 7);
						}
					}
					else
					{
						warp_location = new Point(48, 39);
					}
				}
				Game1.warpFarmer("Farm", warp_location.X, warp_location.Y, false);
			}
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			Game1.player.temporarilyInvincible = false;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}

		// Token: 0x06001317 RID: 4887 RVA: 0x000E4280 File Offset: 0x000E2480
		public void MonsterMusk(Farmer who)
		{
			GameLocation location = who.currentLocation;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.StopAnimation();
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(104, 350, false, false, null, false),
				new FarmerSprite.AnimationFrame(105, 350, false, false, null, false)
			}, null);
			location.playSound("croak", null, null, SoundContext.Default);
			who.applyBuff("24");
		}

		// Token: 0x06001318 RID: 4888 RVA: 0x000E4344 File Offset: 0x000E2544
		public override void ModifyItemBuffs(BuffEffects effects)
		{
			if (effects != null && base.Category == -7)
			{
				int buff_bonus = 0;
				if (base.Quality != 0)
				{
					buff_bonus = 1;
				}
				if (buff_bonus > 0)
				{
					foreach (NetFloat effect in new NetFloat[]
					{
						effects.FarmingLevel,
						effects.FishingLevel,
						effects.MiningLevel,
						effects.LuckLevel,
						effects.ForagingLevel,
						effects.MaxStamina,
						effects.MagneticRadius,
						effects.Defense,
						effects.Attack
					})
					{
						if (effect.Value != 0f)
						{
							effect.Value += (float)buff_bonus;
						}
					}
				}
			}
			base.ModifyItemBuffs(effects);
		}

		// Token: 0x06001319 RID: 4889 RVA: 0x000E4408 File Offset: 0x000E2608
		private void treasureTotem(Farmer who, GameLocation gameLocation)
		{
			Game1.playSound("treasure_totem", null);
			NetWorldState value = Game1.netWorldState.Value;
			int treasureTotemsUsed = value.TreasureTotemsUsed;
			value.TreasureTotemsUsed = treasureTotemsUsed + 1;
			Vector2 center = who.Tile;
			int radius = 4;
			int x = (int)center.X - radius;
			while ((float)x < center.X + (float)radius)
			{
				int y = (int)center.Y - radius;
				while ((float)y < center.Y + (float)radius)
				{
					if (Math.Round((double)Utility.distance((float)x, center.X, (float)y, center.Y)) == (double)(radius - 1))
					{
						Vector2 location = new Vector2((float)x, (float)y);
						if (gameLocation.CanItemBePlacedHere(location, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && !gameLocation.IsTileOccupiedBy(location, CollisionMask.All, CollisionMask.None, false) && !gameLocation.hasTileAt(x, y, "AlwaysFront", null) && !gameLocation.hasTileAt(x, y, "Front", null) && !gameLocation.isBehindBush(location) && (gameLocation.doesTileHaveProperty(x, y, "Diggable", "Back", false) != null || (gameLocation.GetSeason() == Season.Winter && gameLocation.doesTileHaveProperty(x, y, "Type", "Back", false) == "Grass")))
						{
							if ((this.name.Equals("Forest") && x >= 93 && y <= 22) || !gameLocation.IsOutdoors)
							{
								goto IL_299;
							}
							gameLocation.objects.Add(location, ItemRegistry.Create<Object>("(O)590", 1, 0, false));
						}
						Utility.addRainbowStarExplosion(gameLocation, new Vector2((float)x, (float)y) * 64f, 1);
						Utility.addStarsAndSpirals(gameLocation, x, y, 1, 1, 100, 100, Color.White, null, false);
						goto IL_1A8;
					}
					goto IL_1A8;
					IL_299:
					y++;
					continue;
					IL_1A8:
					if (Math.Round((double)Utility.distance((float)x, center.X, (float)y, center.Y)) <= (double)(radius - 1))
					{
						Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(144, 249, 7, 7), (float)Game1.random.Next(100, 200), 6, 1, new Vector2((float)x, (float)y) * 64f + new Vector2((float)(32 + Game1.random.Next(-16, 16)), (float)(32 + Game1.random.Next(-16, 16))), false, false, 0.001f, 0f, (Game1.random.NextDouble() < 0.5) ? new Color(255, 255, 100) : Color.White, 4f, 0f, 0f, 0f, false), gameLocation, 4, 64, 64);
						goto IL_299;
					}
					goto IL_299;
				}
				x++;
			}
		}

		// Token: 0x0600131A RID: 4890 RVA: 0x000E46DC File Offset: 0x000E28DC
		private void rainTotem(Farmer who)
		{
			GameLocation location = who.currentLocation;
			string contextId = location.GetLocationContextId();
			LocationContextData context = location.GetLocationContext();
			if (!context.AllowRainTotem)
			{
				Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere", true);
				return;
			}
			if (context.RainTotemAffectsContext != null)
			{
				contextId = context.RainTotemAffectsContext;
			}
			bool applied = false;
			if (contextId == "Default")
			{
				if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
				{
					Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = "Rain");
					applied = true;
				}
			}
			else
			{
				location.GetWeather().WeatherForTomorrow = "Rain";
				applied = true;
			}
			if (applied)
			{
				Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"));
			}
			Game1.screenGlow = false;
			location.playSound("thunder", null, null, SoundContext.Default);
			who.canMove = false;
			Game1.screenGlowOnce(Color.SlateBlue, false, 0.005f, 0.3f);
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(57, 2000, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true)
			}, null);
			for (int i = 0; i < 6; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f, false)
					{
						motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
						delayBeforeAnimationStart = i * 200
					}
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f, false)
					{
						motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
						delayBeforeAnimationStart = 100 + i * 200
					}
				});
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f, false)
					{
						motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
						delayBeforeAnimationStart = 200 + i * 200
					}
				});
			}
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), false, false, false, 0f)
			{
				motion = new Vector2(0f, -7f),
				acceleration = new Vector2(0f, 0.1f),
				scaleChange = 0.015f,
				alpha = 1f,
				alphaFade = 0.0075f,
				shakeIntensity = 1f,
				initialPosition = Game1.player.Position + new Vector2(0f, -96f),
				xPeriodic = true,
				xPeriodicLoopTime = 1000f,
				xPeriodicRange = 4f,
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(base.QualifiedItemId, 0);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				sprite
			});
			DelayedAction.playSoundAfterDelay("rainsound", 2000, null, null, -1, false);
		}

		// Token: 0x0600131B RID: 4891 RVA: 0x000E4B7C File Offset: 0x000E2D7C
		private void readBook(GameLocation location)
		{
			Game1.player.canMove = false;
			Game1.player.freezePause = 1030;
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(57, 1000, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true)
				{
					frameEndBehavior = delegate(Farmer x)
					{
						location.removeTemporarySpritesWithID(1987654);
						Utility.addRainbowStarExplosion(location, Game1.player.getStandingPosition() + new Vector2(-40f, -156f), 8);
					}
				}
			}, null);
			Game1.MusicDuckTimer = 4000f;
			Game1.playSound("book_read", null);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), false, false, Game1.player.getDrawLayer() + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					holdLastFrame = true,
					id = 1987654
				}
			});
			Color? c = ItemContextTagManager.GetColorFromTags(this);
			if (c != null)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 20, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), false, false, Game1.player.getDrawLayer() + 0.0012f, 0f, c.Value, 4f, 0f, 0f, 0f, false)
					{
						holdLastFrame = true,
						id = 1987654
					}
				});
			}
			if (base.ItemId.StartsWith("SkillBook_"))
			{
				int current = Game1.player.newLevels.Count;
				Game1.player.gainExperience(Convert.ToInt32(base.ItemId.Last<char>().ToString() ?? ""), 250);
				if (Game1.player.newLevels.Count == current || (Game1.player.newLevels.Count > 1 && current >= 1))
				{
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:SkillBookMessage", Game1.content.LoadString("Strings\\1_6_Strings:SkillName_" + this.ItemId.Last<char>().ToString()).ToLower()));
					}, 1000);
					return;
				}
			}
			else if (Game1.player.stats.Get(this.itemId.Value) > 0U && base.ItemId != "Book_PriceCatalogue" && base.ItemId != "Book_AnimalCatalogue")
			{
				if (!Game1.player.mailReceived.Contains("read_a_book"))
				{
					Game1.player.mailReceived.Add("read_a_book");
				}
				bool foundAny = false;
				foreach (string tag in base.GetContextTags())
				{
					if (tag.StartsWithIgnoreCase("book_xp_"))
					{
						foundAny = true;
						string whichSkill = tag.Split('_', StringSplitOptions.None)[2];
						Game1.player.gainExperience(Farmer.getSkillNumberFromName(whichSkill), 100);
						break;
					}
				}
				if (!foundAny)
				{
					for (int i = 0; i < 5; i++)
					{
						Game1.player.gainExperience(i, 20);
					}
					return;
				}
			}
			else
			{
				string itemId = base.ItemId;
				if (itemId == "Book_QueenOfSauce")
				{
					Dictionary<string, string> dictionary = DataLoader.Tv_CookingChannel(Game1.content);
					int num = 0;
					foreach (KeyValuePair<string, string> s in dictionary)
					{
						if (Game1.player.cookingRecipes.TryAdd(s.Value.Split("/", StringSplitOptions.None)[0], 0))
						{
							num++;
						}
					}
					Game1.player.stats.Increment(this.itemId.Value, 1U);
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:QoS_Cookbook", num.ToString() ?? ""));
					return;
				}
				if (itemId == "PurpleBook")
				{
					Game1.player.gainExperience(0, 250);
					Game1.player.gainExperience(1, 250);
					Game1.player.gainExperience(2, 250);
					Game1.player.gainExperience(3, 250);
					Game1.player.gainExperience(4, 250);
					return;
				}
				Game1.player.stats.Increment(this.itemId.Value, 1U);
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:LearnedANewPower"));
				}, 1000);
				if (!Game1.player.mailReceived.Contains("read_a_book"))
				{
					Game1.player.mailReceived.Add("read_a_book");
				}
				Game1.stats.checkForBooksReadAchievement();
			}
		}

		// Token: 0x0600131C RID: 4892 RVA: 0x000E50D0 File Offset: 0x000E32D0
		public virtual bool performUseAction(GameLocation location)
		{
			if (!Game1.player.canMove || this.isTemporarilyInvisible)
			{
				return false;
			}
			bool normal_gameplay = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming.Value && !Game1.player.bathingClothes.Value && !Game1.player.onBridge.Value;
			if (normal_gameplay && (base.Category == -102 || base.Category == -103))
			{
				this.readBook(location);
				return true;
			}
			if (this.name.Contains("Totem"))
			{
				if (normal_gameplay)
				{
					string qualifiedItemId = base.QualifiedItemId;
					if (qualifiedItemId != null)
					{
						int length = qualifiedItemId.Length;
						if (length == 6)
						{
							char c = qualifiedItemId[5];
							if (c != '0')
							{
								if (c != '1')
								{
									switch (c)
									{
									case '6':
										if (!(qualifiedItemId == "(O)886"))
										{
											goto IL_789;
										}
										break;
									case '7':
										goto IL_789;
									case '8':
										if (!(qualifiedItemId == "(O)688"))
										{
											goto IL_789;
										}
										break;
									case '9':
										if (!(qualifiedItemId == "(O)689"))
										{
											goto IL_789;
										}
										break;
									default:
										goto IL_789;
									}
								}
								else
								{
									if (qualifiedItemId == "(O)681")
									{
										this.rainTotem(Game1.player);
										return true;
									}
									if (!(qualifiedItemId == "(O)261"))
									{
										goto IL_789;
									}
								}
							}
							else if (!(qualifiedItemId == "(O)690"))
							{
								goto IL_789;
							}
							Game1.player.jitterStrength = 1f;
							Color sprinkleColor = (base.QualifiedItemId == "(O)681") ? Color.SlateBlue : ((base.QualifiedItemId == "(O)688") ? Color.LimeGreen : ((base.QualifiedItemId == "(O)689") ? Color.OrangeRed : ((base.QualifiedItemId == "(O)261") ? new Color(255, 200, 0) : Color.LightBlue)));
							location.playSound("warrior", null, null, SoundContext.Default);
							Game1.player.faceDirection(2);
							Game1.player.CanMove = false;
							Game1.player.temporarilyInvincible = true;
							Game1.player.temporaryInvincibilityTimer = -4000;
							Game1.changeMusicTrack("silence", false, MusicContext.Default);
							if (base.QualifiedItemId == "(O)681")
							{
								Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
								{
									new FarmerSprite.AnimationFrame(57, 2000, false, false, null, false),
									new FarmerSprite.AnimationFrame((int)((short)Game1.player.FarmerSprite.CurrentFrame), 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.rainTotem), true)
								}, null);
							}
							else
							{
								Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
								{
									new FarmerSprite.AnimationFrame(57, 2000, false, false, null, false),
									new FarmerSprite.AnimationFrame((int)((short)Game1.player.FarmerSprite.CurrentFrame), 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.totemWarp), true)
								}, null);
							}
							TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), false, false, false, 0f)
							{
								motion = new Vector2(0f, -1f),
								scaleChange = 0.01f,
								alpha = 1f,
								alphaFade = 0.0075f,
								shakeIntensity = 1f,
								initialPosition = Game1.player.Position + new Vector2(0f, -96f),
								xPeriodic = true,
								xPeriodicLoopTime = 1000f,
								xPeriodicRange = 4f,
								layerDepth = 1f
							};
							sprite.CopyAppearanceFromItemId(base.QualifiedItemId, 0);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								sprite
							});
							sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), false, false, false, 0f)
							{
								motion = new Vector2(0f, -0.5f),
								scaleChange = 0.005f,
								scale = 0.5f,
								alpha = 1f,
								alphaFade = 0.0075f,
								shakeIntensity = 1f,
								delayBeforeAnimationStart = 10,
								initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
								xPeriodic = true,
								xPeriodicLoopTime = 1000f,
								xPeriodicRange = 4f,
								layerDepth = 0.9999f
							};
							sprite.CopyAppearanceFromItemId(base.QualifiedItemId, 0);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								sprite
							});
							sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), false, false, false, 0f)
							{
								motion = new Vector2(0f, -0.5f),
								scaleChange = 0.005f,
								scale = 0.5f,
								alpha = 1f,
								alphaFade = 0.0075f,
								delayBeforeAnimationStart = 20,
								shakeIntensity = 1f,
								initialPosition = Game1.player.Position + new Vector2(64f, -96f),
								xPeriodic = true,
								xPeriodicLoopTime = 1000f,
								xPeriodicRange = 4f,
								layerDepth = 0.9988f
							};
							sprite.CopyAppearanceFromItemId(base.QualifiedItemId, 0);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								sprite
							});
							Game1.screenGlowOnce(sprinkleColor, false, 0.005f, 0.3f);
							Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, true);
							return true;
						}
						if (length == 16)
						{
							if (qualifiedItemId == "(O)TreasureTotem")
							{
								if (!location.IsOutdoors)
								{
									Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:Object.cs.13053", true);
									return false;
								}
								this.treasureTotem(Game1.player, location);
								return true;
							}
						}
					}
				}
			}
			else if (base.QualifiedItemId == "(O)79" || base.QualifiedItemId == "(O)842")
			{
				bool isJournal = base.QualifiedItemId == "(O)842";
				int length;
				int[] unseenNotes = Utility.GetUnseenSecretNotes(Game1.player, isJournal, out length);
				if (unseenNotes.Length == 0)
				{
					return false;
				}
				Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.player.UniqueMultiplayerID, (double)(unseenNotes.Length * 777), 0.0, 0.0);
				int which = isJournal ? unseenNotes.Min() : r.ChooseFrom(unseenNotes);
				if (!Game1.player.secretNotesSeen.Add(which))
				{
					return false;
				}
				if (which != 10)
				{
					if (which == 23 && !Game1.player.eventsSeen.Contains("2120303"))
					{
						Game1.player.addQuest("29");
					}
				}
				else if (!Game1.player.mailReceived.Contains("qiCave"))
				{
					Game1.player.addQuest("30");
				}
				Game1.activeClickableMenu = new LetterViewerMenu(which);
				return true;
			}
			IL_789:
			if (base.QualifiedItemId == "(O)911")
			{
				if (!normal_gameplay)
				{
					return false;
				}
				string warpError = Utility.GetHorseWarpErrorMessage(Utility.GetHorseWarpRestrictionsForFarmer(Game1.player));
				if (warpError == null)
				{
					Horse horse = null;
					foreach (NPC npc in location.characters)
					{
						Horse curHorse = npc as Horse;
						if (curHorse != null && curHorse.getOwner() == Game1.player)
						{
							horse = curHorse;
							break;
						}
					}
					if (horse == null || Math.Abs(Game1.player.TilePoint.X - horse.TilePoint.X) > 1 || Math.Abs(Game1.player.TilePoint.Y - horse.TilePoint.Y) > 1)
					{
						Game1.player.faceDirection(2);
						Game1.MusicDuckTimer = 2000f;
						Game1.playSound("horse_flute", null);
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
						{
							new FarmerSprite.AnimationFrame(98, 400, true, false, null, false),
							new FarmerSprite.AnimationFrame(99, 200, true, false, null, false),
							new FarmerSprite.AnimationFrame(100, 200, true, false, null, false),
							new FarmerSprite.AnimationFrame(99, 200, true, false, null, false),
							new FarmerSprite.AnimationFrame(98, 400, true, false, null, false),
							new FarmerSprite.AnimationFrame(99, 200, true, false, null, false)
						}, null);
						Game1.player.freezePause = 1500;
						DelayedAction.functionAfterDelay(delegate
						{
							string error = Utility.GetHorseWarpErrorMessage(Utility.GetHorseWarpRestrictionsForFarmer(Game1.player));
							if (error != null)
							{
								Game1.showRedMessage(error, true);
								return;
							}
							Game1.player.team.requestHorseWarpEvent.Fire(Game1.player.UniqueMultiplayerID);
						}, 1500);
					}
					this.stack.Value = this.stack.Value + 1;
					return true;
				}
				Game1.showRedMessage(warpError, true);
			}
			if (!(base.QualifiedItemId == "(O)879"))
			{
				return false;
			}
			if (!normal_gameplay)
			{
				return false;
			}
			Game1.player.faceDirection(2);
			Game1.player.freezePause = 1750;
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new FarmerSprite.AnimationFrame(57, 750, false, false, null, false),
				new FarmerSprite.AnimationFrame((int)((short)Game1.player.FarmerSprite.CurrentFrame), 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.MonsterMusk), true)
			}, null);
			for (int i = 0; i < 3; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, new Vector2(16f, (float)(-64 + 32 * i)), Color.Purple, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						motion = new Vector2(Utility.RandomFloat(-1f, 1f, null), -0.5f),
						scaleChange = 0.005f,
						scale = 0.5f,
						alpha = 1f,
						alphaFade = 0.0075f,
						shakeIntensity = 1f,
						delayBeforeAnimationStart = 100 * i,
						layerDepth = 0.9999f,
						positionFollowsAttachedCharacter = true,
						attachedCharacter = Game1.player
					}
				});
			}
			location.playSound("steam", null, null, SoundContext.Default);
			return true;
		}

		// Token: 0x0600131D RID: 4893 RVA: 0x000E5BF8 File Offset: 0x000E3DF8
		public override Color getCategoryColor()
		{
			if (this.type.Value == "Arch")
			{
				return new Color(110, 0, 90);
			}
			return base.getCategoryColor();
		}

		// Token: 0x0600131E RID: 4894 RVA: 0x000E5C24 File Offset: 0x000E3E24
		public override string getCategoryName()
		{
			Furniture furniture = this as Furniture;
			if (furniture != null)
			{
				int placementRestriction = furniture.placementRestriction;
				if (placementRestriction == 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors");
				}
				if (placementRestriction != 2)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12847");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration");
			}
			else
			{
				if (this.Type == "Arch")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12849");
				}
				return base.getCategoryName();
			}
		}

		// Token: 0x0600131F RID: 4895 RVA: 0x000E5CA4 File Offset: 0x000E3EA4
		public static string GetCategoryDisplayName(int category)
		{
			switch (category)
			{
			case -103:
				return Game1.content.LoadString("Strings\\1_6_Strings:skillBook_Category");
			case -102:
				return Game1.content.LoadString("Strings\\1_6_Strings:Book_Category");
			case -101:
			case -98:
				break;
			case -100:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:category_clothes");
			case -99:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14307");
			case -97:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12501");
			case -96:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ring.cs.1");
			default:
				switch (category)
				{
				case -81:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12869");
				case -80:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12866");
				case -79:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12854");
				case -78:
				case -77:
				case -76:
					break;
				case -75:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12851");
				case -74:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12855");
				default:
					switch (category)
					{
					case -28:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12867");
					case -27:
					case -26:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12862");
					case -25:
					case -7:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12853");
					case -24:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12859");
					case -22:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12858");
					case -21:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12857");
					case -20:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12860");
					case -19:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12856");
					case -18:
					case -14:
					case -6:
					case -5:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12864");
					case -16:
					case -15:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12868");
					case -12:
					case -2:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12850");
					case -8:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12863");
					case -4:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12852");
					}
					break;
				}
				break;
			}
			return "";
		}

		// Token: 0x06001320 RID: 4896 RVA: 0x000E5F04 File Offset: 0x000E4104
		public static Color GetCategoryColor(int category)
		{
			if (category <= -102)
			{
				if (category == -103)
				{
					return new Color(122, 93, 39);
				}
				if (category == -102)
				{
					return new Color(85, 47, 27);
				}
			}
			else
			{
				switch (category)
				{
				case -81:
					return new Color(10, 130, 50);
				case -80:
					return new Color(219, 54, 211);
				case -79:
					return Color.DeepPink;
				case -78:
				case -77:
				case -76:
					break;
				case -75:
					return Color.Green;
				case -74:
					return Color.Brown;
				default:
					switch (category)
					{
					case -28:
						return new Color(50, 10, 70);
					case -27:
					case -26:
						return new Color(0, 155, 111);
					case -24:
						return new Color(150, 80, 190);
					case -22:
						return Color.DarkCyan;
					case -21:
						return Color.DarkRed;
					case -20:
						return Color.DimGray;
					case -19:
						return Color.SlateGray;
					case -18:
					case -14:
					case -6:
					case -5:
						return new Color(255, 0, 100);
					case -16:
					case -15:
						return new Color(64, 102, 114);
					case -12:
					case -2:
						return new Color(110, 0, 90);
					case -8:
						return new Color(148, 61, 40);
					case -7:
						return new Color(220, 60, 0);
					case -4:
						return Color.DarkBlue;
					}
					break;
				}
			}
			return Color.Black;
		}

		// Token: 0x06001321 RID: 4897 RVA: 0x000E60AA File Offset: 0x000E42AA
		public virtual bool isActionable(Farmer who)
		{
			return !this.isTemporarilyInvisible && this.checkForAction(who, true);
		}

		// Token: 0x06001322 RID: 4898 RVA: 0x000E60BE File Offset: 0x000E42BE
		public int getHealth()
		{
			return this.health;
		}

		// Token: 0x06001323 RID: 4899 RVA: 0x000E60C6 File Offset: 0x000E42C6
		public void setHealth(int health)
		{
			this.health = health;
		}

		// Token: 0x06001324 RID: 4900 RVA: 0x000E60D0 File Offset: 0x000E42D0
		protected virtual void grabItemFromAutoGrabber(Item item, Farmer who)
		{
			Chest chest = this.heldObject.Value as Chest;
			if (chest != null)
			{
				if (who.couldInventoryAcceptThisItem(item))
				{
					chest.Items.Remove(item);
					chest.clearNulls();
					Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromAutoGrabber), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				}
				if (chest.isEmpty())
				{
					this.showNextIndex.Value = false;
				}
			}
		}

		// Token: 0x06001325 RID: 4901 RVA: 0x000E6165 File Offset: 0x000E4365
		public static bool HighlightFertilizers(Item i)
		{
			return i.Category == -19;
		}

		// Token: 0x06001326 RID: 4902 RVA: 0x000E6174 File Offset: 0x000E4374
		public override int healthRecoveredOnConsumption()
		{
			if (this.Edibility < 0)
			{
				return 0;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(O)874")
			{
				return (int)((float)this.staminaRecoveredOnConsumption() * 0.68f);
			}
			if (qualifiedItemId == "(O)434" || qualifiedItemId == "(O)349")
			{
				return 0;
			}
			if (!(qualifiedItemId == "(O)773"))
			{
				return (int)((float)this.staminaRecoveredOnConsumption() * 0.45f);
			}
			return 999;
		}

		// Token: 0x06001327 RID: 4903 RVA: 0x000E61F0 File Offset: 0x000E43F0
		public override int staminaRecoveredOnConsumption()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(O)773")
			{
				return 0;
			}
			if (!(qualifiedItemId == "(O)434"))
			{
				return (int)Math.Ceiling((double)this.Edibility * 2.5) + base.Quality * this.Edibility;
			}
			return 999;
		}

		// Token: 0x06001328 RID: 4904 RVA: 0x000E6250 File Offset: 0x000E4450
		public virtual bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (this.isTemporarilyInvisible)
			{
				return true;
			}
			if (!justCheckingForActivity && who != null)
			{
				GameLocation location = this.Location;
				Point tile = who.TilePoint;
				if (location.isObjectAtTile(tile.X, tile.Y - 1) && location.isObjectAtTile(tile.X, tile.Y + 1) && location.isObjectAtTile(tile.X + 1, tile.Y) && location.isObjectAtTile(tile.X - 1, tile.Y) && !location.getObjectAtTile(tile.X, tile.Y - 1, false).isPassable() && !location.getObjectAtTile(tile.X, tile.Y + 1, false).isPassable() && !location.getObjectAtTile(tile.X - 1, tile.Y, false).isPassable() && !location.getObjectAtTile(tile.X + 1, tile.Y, false).isPassable())
				{
					this.performToolAction(null);
				}
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				switch (length)
				{
				case 5:
					switch (qualifiedItemId[4])
					{
					case '0':
						if (!(qualifiedItemId == "(BC)0"))
						{
							goto IL_62A;
						}
						break;
					case '1':
						if (!(qualifiedItemId == "(BC)1"))
						{
							goto IL_62A;
						}
						break;
					case '2':
						if (!(qualifiedItemId == "(BC)2"))
						{
							goto IL_62A;
						}
						break;
					case '3':
						if (!(qualifiedItemId == "(BC)3"))
						{
							goto IL_62A;
						}
						break;
					case '4':
						if (!(qualifiedItemId == "(BC)4"))
						{
							goto IL_62A;
						}
						break;
					case '5':
						if (!(qualifiedItemId == "(BC)5"))
						{
							goto IL_62A;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(BC)6"))
						{
							goto IL_62A;
						}
						break;
					case '7':
						if (!(qualifiedItemId == "(BC)7"))
						{
							goto IL_62A;
						}
						break;
					default:
						goto IL_62A;
					}
					return this.CheckForActionOnHousePlant(who, justCheckingForActivity);
				case 6:
					switch (qualifiedItemId[5])
					{
					case '1':
						if (qualifiedItemId == "(BC)71")
						{
							return this.CheckForActionOnStaircase(who, justCheckingForActivity);
						}
						break;
					case '3':
						if (qualifiedItemId == "(O)463")
						{
							return this.CheckForActionOnDrumBlock(who, justCheckingForActivity);
						}
						break;
					case '4':
						if (qualifiedItemId == "(BC)94")
						{
							return this.CheckForActionOnSingingStone(who, justCheckingForActivity);
						}
						if (qualifiedItemId == "(O)464")
						{
							return this.CheckForActionOnFluteBlock(who, justCheckingForActivity);
						}
						break;
					case '6':
						if (qualifiedItemId == "(BC)56")
						{
							return this.CheckForActionOnSlimeBall(who, justCheckingForActivity);
						}
						break;
					case '9':
						if (qualifiedItemId == "(BC)99")
						{
							return this.CheckForActionOnFeedHopper(who, justCheckingForActivity);
						}
						break;
					}
					break;
				case 7:
					switch (qualifiedItemId[6])
					{
					case '1':
						if (qualifiedItemId == "(BC)141")
						{
							return this.CheckForActionOnPrairieKingArcadeSystem(who, justCheckingForActivity);
						}
						break;
					case '5':
						if (qualifiedItemId == "(BC)165")
						{
							return this.CheckForActionOnAutoGrabber(who, justCheckingForActivity);
						}
						break;
					case '7':
						if (qualifiedItemId == "(BC)247")
						{
							return this.CheckForActionOnSewingMachine(who, justCheckingForActivity);
						}
						break;
					case '8':
						if (qualifiedItemId == "(BC)238")
						{
							return this.CheckForActionOnMiniObelisk(who, justCheckingForActivity);
						}
						break;
					case '9':
						if (qualifiedItemId == "(BC)159")
						{
							return this.CheckForActionOnJunimoKartArcadeSystem(who, justCheckingForActivity);
						}
						if (qualifiedItemId == "(BC)239")
						{
							return this.CheckForActionOnFarmComputer(who, justCheckingForActivity);
						}
						break;
					}
					break;
				case 8:
				case 9:
				case 10:
				case 11:
					break;
				case 12:
					if (qualifiedItemId == "(O)PotOfGold")
					{
						if (!justCheckingForActivity)
						{
							Game1.playSound("hammer", null);
							Game1.playSound("moneyDial", null);
							Game1.createMultipleItemDebris(ItemRegistry.Create("(O)GoldCoin", Math.Min(100, 7 + Game1.year), 0, false), this.TileLocation * 64f + new Vector2(32f), 1, null, -1, false);
							Game1.createMultipleItemDebris(ItemRegistry.Create("(H)LeprechuanHat", 1, 0, false), this.TileLocation * 64f + new Vector2(32f), 1, null, -1, false);
							this.Location.removeObject(this.TileLocation, false);
							Utility.addDirtPuffs(this.Location, (int)this.TileLocation.X, (int)this.TileLocation.Y, 1, 1, 3);
							Utility.addStarsAndSpirals(this.Location, (int)this.TileLocation.X, (int)this.TileLocation.Y, 1, 1, 100, 30, Color.White, null, false);
						}
						return true;
					}
					break;
				case 13:
					if (qualifiedItemId == "(BC)MiniForge")
					{
						if (!justCheckingForActivity)
						{
							Game1.activeClickableMenu = new ForgeMenu();
						}
						return true;
					}
					break;
				default:
					if (length != 21)
					{
						if (length == 24)
						{
							if (qualifiedItemId == "(BC)StatueOfTheDwarfKing")
							{
								if (!justCheckingForActivity)
								{
									if (who.stats.Get(StatKeys.Mastery(3)) < 1U)
									{
										Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
										Game1.playSound("cancel", null);
									}
									else if (!who.hasBuffWithNameContainingString("dwarfStatue"))
									{
										Game1.activeClickableMenu = new ChooseFromIconsMenu("dwarfStatue");
										(Game1.activeClickableMenu as ChooseFromIconsMenu).sourceObject = this;
									}
									else
									{
										this.shakeTimer = 400;
										Game1.playSound("cancel", null);
									}
								}
								return true;
							}
						}
					}
					else if (qualifiedItemId == "(BC)StatueOfBlessings")
					{
						return this.CheckForActionOnBlessedStatue(who, who.currentLocation, justCheckingForActivity);
					}
					break;
				}
			}
			IL_62A:
			return (this.IsSprinkler() && this.CheckForActionOnSprinkler(who, justCheckingForActivity)) || (this.IsScarecrow() && this.CheckForActionOnScarecrow(who, justCheckingForActivity)) || (this.IsTextSign() && this.CheckForActionOnTextSign(who, justCheckingForActivity)) || this.CheckForActionOnMachine(who, justCheckingForActivity);
		}

		// Token: 0x06001329 RID: 4905 RVA: 0x000E68CB File Offset: 0x000E4ACB
		protected virtual bool CheckForActionOnSewingMachine(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Game1.activeClickableMenu = new TailoringMenu();
			return true;
		}

		// Token: 0x0600132A RID: 4906 RVA: 0x000E68E0 File Offset: 0x000E4AE0
		protected virtual bool CheckForActionOnAutoGrabber(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Chest chest = this.heldObject.Value as Chest;
			if (chest != null && !chest.isEmpty())
			{
				Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromAutoGrabber), false, true, true, true, true, 1, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				return true;
			}
			return false;
		}

		// Token: 0x0600132B RID: 4907 RVA: 0x000E6958 File Offset: 0x000E4B58
		protected virtual bool CheckForActionOnFarmComputer(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			this.shakeTimer = 500;
			this.Location.localSound("DwarvishSentry", null, null, SoundContext.Default);
			who.freezePause = 500;
			DelayedAction.functionAfterDelay(delegate
			{
				this.ShowFarmComputerReport(who);
			}, 500);
			return true;
		}

		// Token: 0x0600132C RID: 4908 RVA: 0x000E69D4 File Offset: 0x000E4BD4
		protected virtual void ShowFarmComputerReport(Farmer who)
		{
			GameLocation location = (this.Location ?? who.currentLocation).GetRootLocation();
			Farm farm = location as Farm;
			bool flag = location.IsBuildableLocation() || location.buildings.Any<Building>();
			string locationDisplayName = location.GetDisplayName();
			int totalCrops = location.getTotalCrops();
			int totalOpenHoeDirt = location.getTotalOpenHoeDirt();
			int numCropsToHarvest = location.getTotalCropsReadyForHarvest();
			int numUnwateredCrops = location.getTotalUnwateredCrops();
			int? numGreenhouseCropsToHarvest = location.HasMinBuildings("Greenhouse", 1) ? location.getTotalGreenhouseCropsReadyForHarvest() : null;
			int numForage = location.getTotalForageItems();
			int anyMachinesReady = location.getNumberOfMachinesReadyForHarvest();
			bool? farmCaveNeedsHarvest = (farm != null) ? new bool?(farm.doesFarmCaveNeedHarvesting()) : null;
			StringBuilder report = new StringBuilder();
			if (location is Farm)
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_Farm", Game1.player.farmName.Value));
			}
			else if (!string.IsNullOrWhiteSpace(locationDisplayName))
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_NamedLocation", locationDisplayName));
			}
			else
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro_Generic"));
			}
			report.Append("^--------------^");
			if (flag)
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_PiecesHay", location.piecesOfHay, location.GetHayCapacity())).Append(" ^");
			}
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalCrops", totalCrops)).Append("  ^").Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest", numCropsToHarvest)).Append("  ^").Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsUnwatered", numUnwateredCrops)).Append("  ^");
			if (numGreenhouseCropsToHarvest != null)
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest_Greenhouse", numGreenhouseCropsToHarvest)).Append("  ^");
			}
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalOpenHoeDirt", totalOpenHoeDirt)).Append("  ^");
			if (farm == null || farm.SpawnsForage())
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalForage", numForage)).Append("  ^");
			}
			report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_MachinesReady", anyMachinesReady)).Append("  ^");
			if (farmCaveNeedsHarvest != null)
			{
				report.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_FarmCave", farmCaveNeedsHarvest.Value ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
			}
			Game1.multipleDialogues(new string[]
			{
				report.ToString()
			});
		}

		// Token: 0x0600132D RID: 4909 RVA: 0x000E6CBC File Offset: 0x000E4EBC
		protected virtual bool CheckForActionOnMiniObelisk(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			GameLocation location = this.Location;
			Vector2 obelisk = Vector2.Zero;
			Vector2 obelisk2 = Vector2.Zero;
			foreach (KeyValuePair<Vector2, Object> o in location.objects.Pairs)
			{
				if (o.Value.bigCraftable.Value && o.Value.QualifiedItemId == "(BC)238")
				{
					if (obelisk == Vector2.Zero)
					{
						obelisk = o.Key;
					}
					else if (obelisk2 == Vector2.Zero)
					{
						obelisk2 = o.Key;
						break;
					}
				}
			}
			if (obelisk2 == Vector2.Zero)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsPair"), true);
				return false;
			}
			Vector2 target = (Vector2.Distance(who.Tile, obelisk) > Vector2.Distance(who.Tile, obelisk2)) ? obelisk : obelisk2;
			Vector2[] array = new Vector2[]
			{
				new Vector2(target.X, target.Y + 1f),
				new Vector2(target.X - 1f, target.Y),
				new Vector2(target.X + 1f, target.Y),
				new Vector2(target.X, target.Y - 1f)
			};
			for (int k = 0; k < array.Length; k++)
			{
				Vector2 v = array[k];
				if (!location.IsTileBlockedBy(v, CollisionMask.All, CollisionMask.All, false))
				{
					for (int i = 0; i < 12; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), (float)Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), false, Game1.random.NextBool()));
					}
					location.playSound("wand", null, null, SoundContext.Default);
					Game1.displayFarmer = false;
					Game1.player.freezePause = 50;
					Game1.flashAlpha = 1f;
					DelayedAction.fadeAfterDelay(delegate
					{
						who.setTileLocation(v);
						Game1.displayFarmer = true;
						Game1.globalFadeToClear(null, 0.02f);
					}, 50);
					Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
					Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
					r.Inflate(192, 192);
					int j = 0;
					Point playerTile = who.TilePoint;
					for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)x, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = j * 25,
							motion = new Vector2(-0.25f, 0f)
						});
						j++;
					}
					return true;
				}
			}
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsSpace"), true);
			return false;
		}

		// Token: 0x0600132E RID: 4910 RVA: 0x000E70DC File Offset: 0x000E52DC
		protected virtual bool CheckForActionOnPrairieKingArcadeSystem(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			this.Location.showPrairieKingMenu();
			return true;
		}

		// Token: 0x0600132F RID: 4911 RVA: 0x000E70F0 File Offset: 0x000E52F0
		protected virtual bool CheckForActionOnJunimoKartArcadeSystem(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Response[] responses = new Response[]
			{
				new Response("Progress", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12873")),
				new Response("Endless", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12875")),
				new Response("Exit", Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11738"))
			};
			this.Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), responses, "MinecartGame");
			return true;
		}

		// Token: 0x06001330 RID: 4912 RVA: 0x000E7180 File Offset: 0x000E5380
		protected virtual bool CheckForActionOnStaircase(Farmer who, bool justCheckingForActivity = false)
		{
			MineShaft mine = this.Location as MineShaft;
			if (mine != null && mine.shouldCreateLadderOnThisLevel())
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Game1.enterMine(Game1.CurrentMineLevel + 1, null);
				Game1.playSound("stairsdown", null);
			}
			else if (this.Location.Name.Equals("ManorHouse"))
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Game1.warpFarmer("LewisBasement", 4, 4, 2);
				Game1.playSound("stairsdown", null);
			}
			return false;
		}

		// Token: 0x06001331 RID: 4913 RVA: 0x000E7214 File Offset: 0x000E5414
		protected virtual bool CheckForActionOnSlimeBall(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			GameLocation location = this.Location;
			location.objects.Remove(this.tileLocation.Value);
			DelayedAction.playSoundAfterDelay("slimedead", 40, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("slimeHit", 100, null, null, -1, false);
			location.playSound("slimeHit", null, null, SoundContext.Default);
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, (double)this.tileLocation.X * 77.0, (double)this.tileLocation.Y * 777.0, 2.0);
			Game1.createMultipleObjectDebris("(O)766", (int)this.tileLocation.X, (int)this.tileLocation.Y, r.Next(10, 21), 1f + ((who.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, this.tileLocation.Value * 64f, Color.Lime, 10, false, 100f, 0, -1, -1f, -1, 0)
			{
				interval = 70f,
				holdLastFrame = true,
				alphaFade = 0.01f
			}, location, 4, 64, 64);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, this.tileLocation.Value * 64f + new Vector2(-16f, 0f), Color.Lime, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 0,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, this.tileLocation.Value * 64f + new Vector2(0f, 16f), Color.Lime, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 100,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, this.tileLocation.Value * 64f + new Vector2(16f, 0f), Color.Lime, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 200,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			while (r.NextDouble() < 0.33)
			{
				Game1.createObjectDebris("(O)557", (int)this.tileLocation.X, (int)this.tileLocation.Y, who.UniqueMultiplayerID);
			}
			return true;
		}

		// Token: 0x06001332 RID: 4914 RVA: 0x000E7544 File Offset: 0x000E5744
		protected virtual bool CheckForActionOnBlessedStatue(Farmer who, GameLocation location, bool justCheckingForActivitiy = false)
		{
			if (who.stats.Get(StatKeys.Mastery(0)) < 1U && !justCheckingForActivitiy)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
				Game1.playSound("cancel", null);
				return true;
			}
			if (who.hasBuffWithNameContainingString("statue_of_blessings_") || who.hasBeenBlessedByStatueToday)
			{
				return false;
			}
			if (justCheckingForActivitiy)
			{
				return true;
			}
			who.hasBeenBlessedByStatueToday = true;
			Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 777U, 0.0, 0.0);
			for (int i = 0; i < 8; i++)
			{
				r.Next();
			}
			who.applyBuff("statue_of_blessings_" + r.Next((Game1.isRaining || Utility.isFestivalDay()) ? 6 : 7).ToString());
			Game1.playSound("statue_of_blessings", null);
			this.showNextIndex.Value = true;
			if (location.critters == null)
			{
				location.critters = new List<Critter>();
			}
			location.critters.Add(new Butterfly(location, this.TileLocation + new Vector2(1f, 0f), false, false, 163, false));
			location.critters.Add(new Butterfly(location, this.TileLocation + new Vector2(0.33f, 0.25f), false, false, 163, false));
			location.critters.Add(new Butterfly(location, this.TileLocation + new Vector2(1.58f, 0.25f), false, false, 163, false));
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(221, 225, 15, 31), 9000f, 1, 1, this.TileLocation * 64f + new Vector2(1f, -16f) * 4f, false, false, Math.Max(0f, ((this.TileLocation.Y + 1f) * 64f - 20f) / 10000f) + this.TileLocation.X * 1E-05f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false));
			for (int j = 0; j < 6; j++)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(144, 249, 7, 7), (float)Game1.random.Next(100, 200), 6, 1, this.TileLocation * 64f + new Vector2((float)(32 + Game1.random.Next(-64, 64)), (float)Game1.random.Next(-64, 64)), false, false, Math.Max(0f, ((this.TileLocation.Y + 1f) * 64f - 24f) / 10000f) + this.TileLocation.X * 1E-05f, 0f, (Game1.random.NextDouble() < 0.5) ? new Color(255, 180, 210) : Color.White, 4f, 0f, 0f, 0f, false), location, 4, 64, 64);
			}
			return true;
		}

		// Token: 0x06001333 RID: 4915 RVA: 0x000E78C8 File Offset: 0x000E5AC8
		protected virtual bool CheckForActionOnHousePlant(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			int parentSheetIndex = base.ParentSheetIndex;
			base.ParentSheetIndex = parentSheetIndex + 1;
			int total = -1;
			int baseIndex = -1;
			if (this.name == "House Plant")
			{
				total = 8;
				baseIndex = 0;
			}
			if (base.ParentSheetIndex == baseIndex + total)
			{
				base.ParentSheetIndex -= total;
				return false;
			}
			return true;
		}

		// Token: 0x06001334 RID: 4916 RVA: 0x000E7920 File Offset: 0x000E5B20
		protected virtual bool CheckForActionOnFluteBlock(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			int preservedParentSheetInt;
			int.TryParse(this.preservedParentSheetIndex.Value, out preservedParentSheetInt);
			if (preservedParentSheetInt != 2300)
			{
				if (preservedParentSheetInt != 2400)
				{
					preservedParentSheetInt = (preservedParentSheetInt + 100) % 2400;
				}
				else
				{
					preservedParentSheetInt = 0;
				}
			}
			else
			{
				preservedParentSheetInt = 2400;
			}
			this.preservedParentSheetIndex.Value = preservedParentSheetInt.ToString();
			this.shakeTimer = 200;
			string sound = "flute";
			if (who.ActiveObject != null)
			{
				sound = this.getFluteBlockSoundFromHeldObject(who.ActiveObject);
			}
			ICue cue = this.internalSound;
			if (cue != null)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			Game1.playSound(sound, preservedParentSheetInt, out this.internalSound);
			this.scale.Y = 1.3f;
			this.shakeTimer = 200;
			return true;
		}

		// Token: 0x06001335 RID: 4917 RVA: 0x000E79E4 File Offset: 0x000E5BE4
		protected virtual bool CheckForActionOnDrumBlock(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			int preservedParentSheetInt;
			int.TryParse(this.preservedParentSheetIndex.Value, out preservedParentSheetInt);
			preservedParentSheetInt = (preservedParentSheetInt + 1) % 7;
			this.preservedParentSheetIndex.Value = preservedParentSheetInt.ToString();
			this.shakeTimer = 200;
			ICue cue = this.internalSound;
			if (cue != null)
			{
				cue.Stop(AudioStopOptions.Immediate);
			}
			Game1.playSound("drumkit" + preservedParentSheetInt.ToString(), out this.internalSound);
			this.scale.Y = 1.3f;
			this.shakeTimer = 200;
			return true;
		}

		// Token: 0x06001336 RID: 4918 RVA: 0x000E7A78 File Offset: 0x000E5C78
		protected bool CheckForActionOnSprinkler(Farmer who, bool justCheckingForActivity = false)
		{
			if (this.heldObject.Value != null && this.heldObject.Value.QualifiedItemId == "(O)913")
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				if (!Game1.didPlayerJustRightClick(true))
				{
					return false;
				}
				Chest chest = this.heldObject.Value.heldObject.Value as Chest;
				if (chest != null)
				{
					chest.GetMutex().RequestLock(new Action(chest.ShowMenu), null);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001337 RID: 4919 RVA: 0x000E7AF8 File Offset: 0x000E5CF8
		protected bool CheckForActionOnScarecrow(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (base.QualifiedItemId == "(BC)126")
			{
				Hat hat = who.CurrentItem as Hat;
				if (hat != null)
				{
					this.shakeTimer = 100;
					if (this.quality.Value != 0)
					{
						Game1.createItemDebris(ItemRegistry.Create("(H)" + (this.quality.Value - 1).ToString(), 1, 0, false), this.tileLocation.Value * 64f, (who.FacingDirection + 2) % 4, null, -1, false);
						this.quality.Value = 0;
					}
					if (this.preservedParentSheetIndex.Value != null)
					{
						Game1.createItemDebris(new Hat(this.preservedParentSheetIndex.Value), this.tileLocation.Value * 64f, (who.FacingDirection + 2) % 4, null, -1, false);
					}
					this.preservedParentSheetIndex.Value = hat.ItemId;
					who.Items[who.CurrentToolIndex] = null;
					this.Location.playSound("dirtyHit", null, null, SoundContext.Default);
					return true;
				}
			}
			if (!Game1.didPlayerJustRightClick(true))
			{
				return false;
			}
			this.shakeTimer = 100;
			if (base.SpecialVariable == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
			}
			else
			{
				Game1.drawObjectDialogue((base.SpecialVariable == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", base.SpecialVariable));
			}
			return true;
		}

		// Token: 0x06001338 RID: 4920 RVA: 0x000E7C98 File Offset: 0x000E5E98
		protected bool CheckForActionOnSingingStone(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			int pitch = Game1.random.Next(2400);
			pitch -= pitch % 100;
			Game1.playSound("crystal", new int?(pitch));
			this.shakeTimer = 100;
			return true;
		}

		// Token: 0x06001339 RID: 4921 RVA: 0x000E7CDC File Offset: 0x000E5EDC
		protected virtual bool CheckForActionOnTextSign(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (Game1.activeClickableMenu == null)
			{
				TitleTextInputMenu signMenu = new TitleTextInputMenu(Game1.content.LoadString("Strings\\UI:TextSignEntry"), null, this.SignText, null, true);
				signMenu.pasteButton.visible = false;
				signMenu.doneNaming = delegate(string text)
				{
					this.signText.Value = text.Trim();
					signMenu.exitThisMenu(true);
					this.showNextIndex.Value = string.IsNullOrEmpty(this.SignText);
				};
				signMenu.textBox.textLimit = 60;
				Game1.activeClickableMenu = signMenu;
				return true;
			}
			return false;
		}

		// Token: 0x0600133A RID: 4922 RVA: 0x000E7D70 File Offset: 0x000E5F70
		protected bool CheckForActionOnFeedHopper(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (who.ActiveObject != null)
			{
				return false;
			}
			if (who.freeSpotsInInventory() > 0)
			{
				GameLocation location = this.Location;
				GameLocation rootLocation = location.GetRootLocation();
				int piecesHay = rootLocation.piecesOfHay.Value;
				if (piecesHay > 0)
				{
					AnimalHouse i = location as AnimalHouse;
					if (i != null)
					{
						int piecesOfHayToRemove = Math.Min(i.animalsThatLiveHere.Count, piecesHay);
						piecesOfHayToRemove = Math.Max(1, piecesOfHayToRemove);
						int alreadyHay = i.numberOfObjectsWithName("Hay");
						piecesOfHayToRemove = Math.Min(piecesOfHayToRemove, i.animalLimit.Value - alreadyHay);
						if (piecesOfHayToRemove != 0 && Game1.player.couldInventoryAcceptThisItem("(O)178", piecesOfHayToRemove, 0))
						{
							rootLocation.piecesOfHay.Value -= Math.Max(1, piecesOfHayToRemove);
							who.addItemToInventoryBool(ItemRegistry.Create("(O)178", piecesOfHayToRemove, 0, false), false);
							Game1.playSound("shwip", null);
						}
					}
					else if (Game1.player.couldInventoryAcceptThisItem("(O)178", 1, 0))
					{
						NetInt piecesOfHay = rootLocation.piecesOfHay;
						int value = piecesOfHay.Value;
						piecesOfHay.Value = value - 1;
						who.addItemToInventoryBool(ItemRegistry.Create("(O)178", 1, 0, false), false);
						Game1.playSound("shwip", null);
					}
					if (rootLocation.piecesOfHay.Value <= 0)
					{
						this.showNextIndex.Value = false;
					}
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
			}
			return true;
		}

		// Token: 0x0600133B RID: 4923 RVA: 0x000E7F14 File Offset: 0x000E6114
		protected bool CheckForActionOnMachine(Farmer who, bool justCheckingForActivity = false)
		{
			GameLocation location = this.Location;
			if (!this.readyForHarvest.Value)
			{
				MachineData machineData = this.GetMachineData();
				if (machineData != null && machineData.InteractMethod != null)
				{
					MachineInteractDelegate method;
					string error;
					if (StaticDelegateBuilder.TryCreateDelegate<MachineInteractDelegate>(machineData.InteractMethod, out method, out error))
					{
						return justCheckingForActivity || method(this, location, who);
					}
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Machine ");
					defaultInterpolatedStringHandler.AppendFormatted(base.ItemId);
					defaultInterpolatedStringHandler.AppendLiteral(" has invalid interaction method '");
					defaultInterpolatedStringHandler.AppendFormatted(machineData.InteractMethod);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return false;
			}
			if (justCheckingForActivity)
			{
				return true;
			}
			if (who.isMoving())
			{
				Game1.haltAfterCheck = false;
			}
			MachineData machineData2 = this.GetMachineData();
			Object outputObj = this.heldObject.Value;
			if (this.lastOutputRuleId.Value != null)
			{
				List<MachineOutputRule> outputRules = machineData2.OutputRules;
				MachineOutputRule outputRule = (outputRules != null) ? outputRules.FirstOrDefault((MachineOutputRule p) => p.Id == this.lastOutputRuleId.Value) : null;
				if (outputRule != null && outputRule.RecalculateOnCollect)
				{
					this.heldObject.Value = null;
					this.OutputMachine(machineData2, outputRule, this.lastInputItem.Value, who, location, false, true);
					if (this.heldObject.Value != null)
					{
						outputObj = this.heldObject.Value;
					}
					else
					{
						this.heldObject.Value = outputObj;
					}
				}
			}
			bool checkForReload = false;
			if (who.IsLocalPlayer)
			{
				this.heldObject.Value = null;
				if (!who.addItemToInventoryBool(outputObj, false))
				{
					this.heldObject.Value = outputObj;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					return false;
				}
				Game1.playSound("coin", null);
				checkForReload = true;
				MachineDataUtility.UpdateStats((machineData2 != null) ? machineData2.StatsToIncrementWhenHarvested : null, outputObj, outputObj.Stack);
			}
			this.heldObject.Value = null;
			this.readyForHarvest.Value = false;
			this.showNextIndex.Value = false;
			base.ResetParentSheetIndex();
			MachineOutputRule outputCollectedRule;
			MachineOutputTriggerRule machineOutputTriggerRule;
			MachineOutputRule machineOutputRule;
			MachineOutputTriggerRule machineOutputTriggerRule2;
			if (MachineDataUtility.TryGetMachineOutputRule(this, machineData2, MachineOutputTrigger.OutputCollected, outputObj.getOne(), who, location, out outputCollectedRule, out machineOutputTriggerRule, out machineOutputRule, out machineOutputTriggerRule2))
			{
				this.OutputMachine(machineData2, outputCollectedRule, this.lastInputItem.Value, who, location, false, false);
			}
			TerrainFeature terrainFeature;
			if (this.IsTapper() && location.terrainFeatures.TryGetValue(this.tileLocation.Value, out terrainFeature))
			{
				Tree tree = terrainFeature as Tree;
				if (tree != null)
				{
					tree.UpdateTapperProduct(this, outputObj, false);
				}
			}
			if (machineData2 != null && machineData2.ExperienceGainOnHarvest != null)
			{
				string[] expSplit = machineData2.ExperienceGainOnHarvest.Split(' ', StringSplitOptions.None);
				for (int i = 0; i < expSplit.Length; i += 2)
				{
					int skill = Farmer.getSkillNumberFromName(expSplit[i]);
					int amount;
					string text;
					if (skill != -1 && ArgUtility.TryGetInt(expSplit, i + 1, out amount, out text, "int amount"))
					{
						who.gainExperience(skill, amount);
					}
				}
			}
			if (checkForReload)
			{
				this.AttemptAutoLoad(who);
			}
			return true;
		}

		// Token: 0x0600133C RID: 4924 RVA: 0x000E81FC File Offset: 0x000E63FC
		public void playNearbySoundLocal(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			ICue cue;
			Game1.sounds.PlayLocal(audioName, this.Location, new Vector2?(this.tileLocation.Value), pitch, context, out cue);
		}

		// Token: 0x0600133D RID: 4925 RVA: 0x000E822F File Offset: 0x000E642F
		public void playNearbySoundAll(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			Game1.sounds.PlayAll(audioName, this.Location, new Vector2?(this.tileLocation.Value), pitch, context);
		}

		// Token: 0x0600133E RID: 4926 RVA: 0x000E8254 File Offset: 0x000E6454
		public virtual bool IsScarecrow()
		{
			return base.HasContextTag("crow_scare") || this.Name.Contains("arecrow");
		}

		// Token: 0x0600133F RID: 4927 RVA: 0x000E8278 File Offset: 0x000E6478
		public virtual int GetRadiusForScarecrow()
		{
			foreach (string contextTag in base.GetContextTags())
			{
				int radius;
				if (contextTag.StartsWithIgnoreCase("crow_scare_radius_") && int.TryParse(contextTag.Substring("crow_scare_radius_".Length), out radius))
				{
					return radius;
				}
			}
			if (this.Name.StartsWith("Deluxe"))
			{
				return 17;
			}
			return 9;
		}

		// Token: 0x06001340 RID: 4928 RVA: 0x000E8308 File Offset: 0x000E6508
		public virtual Task<bool> AttemptAutoLoad(Farmer who)
		{
			GameLocation location = this.Location;
			Object fromObj;
			if (location != null && location.objects.TryGetValue(new Vector2(this.TileLocation.X, this.TileLocation.Y - 1f), out fromObj))
			{
				Chest chest = fromObj as Chest;
				if (chest != null && chest.specialChestType.Value == Chest.SpecialChestTypes.AutoLoader)
				{
					TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
					chest.GetMutex().RequestLock(delegate
					{
						try
						{
							chest.GetMutex().ReleaseLock();
							bool loaded = this.AttemptAutoLoad(chest.Items, who);
							taskSource.SetResult(loaded);
						}
						catch (Exception ex)
						{
							taskSource.SetException(ex);
						}
					}, null);
					return taskSource.Task;
				}
			}
			return Task.FromResult<bool>(false);
		}

		// Token: 0x06001341 RID: 4929 RVA: 0x000E83C4 File Offset: 0x000E65C4
		public virtual bool AttemptAutoLoad(IInventory inventory, Farmer who)
		{
			if (this.heldObject.Value != null)
			{
				return false;
			}
			Object.autoLoadFrom = inventory;
			foreach (Item item in inventory)
			{
				if (this.performObjectDropInAction(item, false, who, false))
				{
					Object.autoLoadFrom = null;
					return true;
				}
			}
			Object.autoLoadFrom = null;
			return false;
		}

		// Token: 0x06001342 RID: 4930 RVA: 0x000E843C File Offset: 0x000E663C
		private string getFluteBlockSoundFromHeldObject(Object o)
		{
			string qualifiedItemId = o.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				switch (qualifiedItemId.Length)
				{
				case 5:
				{
					char c = qualifiedItemId[3];
					if (c != '6')
					{
						if (c != '8')
						{
							goto IL_193;
						}
						if (!(qualifiedItemId == "(O)80"))
						{
							goto IL_193;
						}
					}
					else
					{
						if (!(qualifiedItemId == "(O)66"))
						{
							goto IL_193;
						}
						return "miniharp_note";
					}
					break;
				}
				case 6:
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(O)430"))
						{
							goto IL_193;
						}
						return "pig";
					case '1':
					case '3':
					case '5':
						goto IL_193;
					case '2':
						if (!(qualifiedItemId == "(O)372"))
						{
							if (!(qualifiedItemId == "(O)382"))
							{
								goto IL_193;
							}
							return "dustMeep";
						}
						break;
					case '4':
						if (!(qualifiedItemId == "(O)444"))
						{
							goto IL_193;
						}
						return "Duck";
					case '6':
						if (!(qualifiedItemId == "(O)746"))
						{
							goto IL_193;
						}
						goto IL_187;
					case '7':
						if (!(qualifiedItemId == "(O)797"))
						{
							if (!(qualifiedItemId == "(O)577"))
							{
								goto IL_193;
							}
							goto IL_17B;
						}
						break;
					case '8':
						if (!(qualifiedItemId == "(O)578") && !(qualifiedItemId == "(O)338"))
						{
							goto IL_193;
						}
						goto IL_17B;
					case '9':
						if (!(qualifiedItemId == "(O)769"))
						{
							goto IL_193;
						}
						goto IL_187;
					default:
						goto IL_193;
					}
					return "clam_tone";
					IL_187:
					return "toyPiano";
				case 7:
					if (!(qualifiedItemId == "(BC)214"))
					{
						goto IL_193;
					}
					return "telephone_buttonPush";
				default:
					goto IL_193;
				}
				IL_17B:
				return "crystal";
			}
			IL_193:
			return "flute";
		}

		// Token: 0x06001343 RID: 4931 RVA: 0x000E85E4 File Offset: 0x000E67E4
		public virtual void farmerAdjacentAction(Farmer who, bool diagonal = false)
		{
			if (this.name == "Error Item" || this.isTemporarilyInvisible)
			{
				return;
			}
			GameLocation location = this.Location;
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(O)464"))
			{
				if (!(qualifiedItemId == "(O)463"))
				{
					if (!(qualifiedItemId == "(BC)29"))
					{
						if (this.IsTextSign())
						{
							this.hovering = true;
							return;
						}
						if (!diagonal)
						{
							Vector2 v = new Vector2(this.TileLocation.X, this.TileLocation.Y - 1f);
							Object tileObj;
							if (this.Location.objects.TryGetValue(v, out tileObj) && tileObj.IsTextSign())
							{
								tileObj.hovering = true;
							}
						}
					}
					else if (!diagonal)
					{
						this.scale.X = this.scale.X + 1f;
						if (this.scale.X > 30f)
						{
							base.ParentSheetIndex = ((base.ParentSheetIndex == 29) ? 30 : 29);
							this.scale.X = 0f;
							this.scale.Y = this.scale.Y + 2f;
						}
						if (this.scale.Y >= 20f && Game1.random.NextDouble() < 0.0001 && location.characters.Count < 4)
						{
							Vector2 playerPos = Game1.player.Tile;
							foreach (Vector2 offset in Character.AdjacentTilesOffsets)
							{
								Vector2 v2 = playerPos + offset;
								if (!location.IsTileOccupiedBy(v2, CollisionMask.All, CollisionMask.None, false) && location.isTilePassable(new Location((int)v2.X, (int)v2.Y), Game1.viewport) && location.isCharacterAtTile(v2) == null)
								{
									if (Game1.random.NextDouble() < 0.1)
									{
										location.characters.Add(new GreenSlime(v2 * new Vector2(64f, 64f)));
									}
									else if (Game1.random.NextBool())
									{
										location.characters.Add(new ShadowGuy(v2 * new Vector2(64f, 64f)));
									}
									else
									{
										location.characters.Add(new ShadowGirl(v2 * new Vector2(64f, 64f)));
									}
									((Monster)location.characters[location.characters.Count - 1]).moveTowardPlayerThreshold.Value = 4;
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(352, 400f, 2, 1, v2 * new Vector2(64f, 64f), false, false)
									});
									location.playSound("shadowpeep", null, null, SoundContext.Default);
									return;
								}
							}
							return;
						}
					}
				}
				else if ((this.internalSound == null || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - (double)this.lastNoteBlockSoundTime >= 1000.0 && !this.internalSound.IsPlaying)) && !Game1.dialogueUp && !diagonal)
				{
					int preservedParentSheetInt;
					int.TryParse(this.preservedParentSheetIndex.Value, out preservedParentSheetInt);
					Game1.playSound("drumkit" + preservedParentSheetInt.ToString(), out this.internalSound);
					this.scale.Y = 1.3f;
					this.shakeTimer = 200;
					this.lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
					return;
				}
			}
			else if ((this.internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - this.lastNoteBlockSoundTime >= 1000 && !this.internalSound.IsPlaying)) && !Game1.dialogueUp && !diagonal)
			{
				int preservedParentSheetInt2;
				int.TryParse(this.preservedParentSheetIndex.Value, out preservedParentSheetInt2);
				string sound = "flute";
				if (who.ActiveObject != null)
				{
					sound = this.getFluteBlockSoundFromHeldObject(who.ActiveObject);
				}
				Game1.playSound(sound, preservedParentSheetInt2, out this.internalSound);
				this.scale.Y = 1.3f;
				this.shakeTimer = 200;
				this.lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
				IslandSouthEast islandSouthEast = location as IslandSouthEast;
				if (islandSouthEast != null)
				{
					islandSouthEast.OnFlutePlayed(preservedParentSheetInt2);
					return;
				}
			}
		}

		// Token: 0x06001344 RID: 4932 RVA: 0x000E8A80 File Offset: 0x000E6C80
		public virtual void addWorkingAnimation()
		{
			GameLocation environment = this.Location;
			if (environment == null || !environment.farmers.Any())
			{
				return;
			}
			MachineData machineData = this.GetMachineData();
			if (((machineData != null) ? machineData.WorkingEffects : null) != null)
			{
				foreach (MachineEffects effect in machineData.WorkingEffects)
				{
					if (this.PlayMachineEffect(effect, true))
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001345 RID: 4933 RVA: 0x000E8B08 File Offset: 0x000E6D08
		public virtual void onReadyForHarvest()
		{
		}

		// Token: 0x06001346 RID: 4934 RVA: 0x000E8B0C File Offset: 0x000E6D0C
		public virtual bool minutesElapsed(int minutes)
		{
			GameLocation environment = this.Location;
			if (environment == null)
			{
				return false;
			}
			if (this.heldObject.Value != null && base.QualifiedItemId != "(BC)165")
			{
				if (this.IsSprinkler())
				{
					return false;
				}
				MachineData machineData = this.GetMachineData();
				if (Game1.IsMasterGame && (machineData == null || this.ShouldTimePassForMachine()))
				{
					this.minutesUntilReady.Value -= minutes;
				}
				if (this.MinutesUntilReady <= 0 && (machineData == null || !machineData.OnlyCompleteOvernight || Game1.newDaySync.hasInstance()))
				{
					if (!this.readyForHarvest.Value && (!Game1.newDaySync.hasInstance() || Game1.newDaySync.hasFinished()))
					{
						environment.playSound("dwop", null, null, SoundContext.Default);
					}
					this.readyForHarvest.Value = true;
					this.minutesUntilReady.Value = 0;
					this.onReadyForHarvest();
					this.showNextIndex.Value = (machineData != null && machineData.ShowNextIndexWhenReady);
					if (this.lightSource != null)
					{
						environment.removeLightSource(this.lightSource.Id);
						this.lightSource = null;
					}
				}
				if (machineData != null)
				{
					if (!this.readyForHarvest.Value && machineData.WorkingEffects != null && Game1.random.NextDouble() < (double)machineData.WorkingEffectChance)
					{
						this.addWorkingAnimation();
					}
				}
				else if (!this.readyForHarvest.Value && Game1.random.NextDouble() < 0.33)
				{
					this.addWorkingAnimation();
				}
			}
			else
			{
				string qualifiedItemId = base.QualifiedItemId;
				if (!(qualifiedItemId == "(BC)29"))
				{
					if (!(qualifiedItemId == "(BC)96"))
					{
						if (!(qualifiedItemId == "(BC)141"))
						{
							if (qualifiedItemId == "(BC)83")
							{
								this.showNextIndex.Value = false;
								environment.removeLightSource(this.GenerateLightSourceId(this.tileLocation.Value));
							}
						}
						else
						{
							this.showNextIndex.Value = !this.showNextIndex.Value;
						}
					}
					else
					{
						this.MinutesUntilReady -= minutes;
						this.showNextIndex.Value = !this.showNextIndex.Value;
						if (this.MinutesUntilReady <= 0)
						{
							this.performRemoveAction();
							environment.objects.Remove(this.tileLocation.Value);
							environment.objects.Add(this.tileLocation.Value, ItemRegistry.Create<Object>("(BC)98", 1, 0, false));
							Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "Capsule_Broken", MailType.Received, true, null);
						}
					}
				}
				else
				{
					this.scale.Y = Math.Max(0f, this.scale.Y = this.scale.Y - (float)(minutes / 2 + 1));
				}
			}
			return false;
		}

		// Token: 0x06001347 RID: 4935 RVA: 0x000E8E0C File Offset: 0x000E700C
		public virtual bool ShouldTimePassForMachine()
		{
			GameLocation location = this.Location;
			MachineData data = this.GetMachineData();
			if (location == null || data == null)
			{
				return false;
			}
			if (data.PreventTimePass != null)
			{
				using (List<MachineTimeBlockers>.Enumerator enumerator = data.PreventTimePass.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						switch (enumerator.Current)
						{
						case MachineTimeBlockers.Outside:
							if (location.IsOutdoors)
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Inside:
							if (!location.IsOutdoors)
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Spring:
							if (location.IsSpringHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Summer:
							if (location.IsSummerHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Fall:
							if (location.IsFallHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Winter:
							if (location.IsWinterHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Sun:
							if (!location.IsRainingHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Rain:
							if (location.IsRainingHere())
							{
								return false;
							}
							break;
						case MachineTimeBlockers.Always:
							return false;
						}
					}
				}
				return true;
			}
			return true;
		}

		// Token: 0x06001348 RID: 4936 RVA: 0x000E8F20 File Offset: 0x000E7120
		public override string checkForSpecialItemHoldUpMeessage()
		{
			if (!this.bigCraftable.Value && this.Type == "Arch")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12993");
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(O)102")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12994");
			}
			if (qualifiedItemId == "(O)535")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12995");
			}
			if (!(qualifiedItemId == "(BC)160"))
			{
				return base.checkForSpecialItemHoldUpMeessage();
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12996");
		}

		// Token: 0x06001349 RID: 4937 RVA: 0x000E8FC4 File Offset: 0x000E71C4
		public virtual bool countsForShippedCollection()
		{
			if (string.IsNullOrWhiteSpace(this.type.Value) || this.Type == "Arch" || this.bigCraftable.Value)
			{
				return false;
			}
			if (base.QualifiedItemId == "(O)433")
			{
				return true;
			}
			int category = base.Category;
			if (category <= -14)
			{
				if (category <= -74)
				{
					if (category != -999 && category != -74)
					{
						goto IL_C0;
					}
				}
				else
				{
					switch (category)
					{
					case -29:
					case -24:
					case -22:
					case -21:
					case -20:
					case -19:
						break;
					case -28:
					case -27:
					case -26:
					case -25:
					case -23:
						goto IL_C0;
					default:
						if (category != -14)
						{
							goto IL_C0;
						}
						break;
					}
				}
			}
			else if (category <= -7)
			{
				if (category != -12 && category - -8 > 1)
				{
					goto IL_C0;
				}
			}
			else if (category != -2 && category != 0)
			{
				goto IL_C0;
			}
			return false;
			IL_C0:
			ObjectData data;
			return !Game1.objectData.TryGetValue(base.ItemId, out data) || !data.ExcludeFromShippingCollection;
		}

		// Token: 0x0600134A RID: 4938 RVA: 0x000E90B0 File Offset: 0x000E72B0
		public static bool isPotentialBasicShipped(string itemId, int category, string objectType)
		{
			if (itemId == "433")
			{
				return true;
			}
			if (objectType == "Arch" || objectType == "Fish" || objectType == "Minerals" || objectType == "Cooking")
			{
				return false;
			}
			if (category <= -19)
			{
				if (category <= -102)
				{
					if (category != -999 && category - -103 > 1)
					{
						goto IL_C3;
					}
				}
				else if (category != -96 && category != -74)
				{
					switch (category)
					{
					case -29:
					case -24:
					case -22:
					case -21:
					case -20:
					case -19:
						break;
					case -28:
					case -27:
					case -26:
					case -25:
					case -23:
						goto IL_C3;
					default:
						goto IL_C3;
					}
				}
			}
			else if (category <= -12)
			{
				if (category != -14 && category != -12)
				{
					goto IL_C3;
				}
			}
			else if (category - -8 > 1 && category != -2 && category != 0)
			{
				goto IL_C3;
			}
			return false;
			IL_C3:
			ObjectData data;
			return !Game1.objectData.TryGetValue(itemId, out data) || !data.ExcludeFromShippingCollection;
		}

		// Token: 0x0600134B RID: 4939 RVA: 0x000E919A File Offset: 0x000E739A
		public override IEnumerable<Buff> GetFoodOrDrinkBuffs()
		{
			foreach (Buff buff in base.GetFoodOrDrinkBuffs())
			{
				yield return buff;
			}
			IEnumerator<Buff> enumerator = null;
			if (this.customBuff != null)
			{
				Buff buff2 = this.customBuff();
				if (buff2 != null)
				{
					yield return buff2;
				}
			}
			ObjectData data;
			if (this.edibility.Value > -300 && Game1.objectData.TryGetValue(base.ItemId, out data))
			{
				List<ObjectBuffData> buffs = data.Buffs;
				if (buffs != null && buffs.Count > 0)
				{
					float durationMultiplier = (base.Quality != 0) ? 1.5f : 1f;
					foreach (Buff buff3 in Object.TryCreateBuffsFromData(data, this.Name, this.DisplayName, durationMultiplier, new Action<BuffEffects>(this.ModifyItemBuffs)))
					{
						yield return buff3;
					}
					enumerator = null;
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x0600134C RID: 4940 RVA: 0x000E91AA File Offset: 0x000E73AA
		public static IEnumerable<Buff> TryCreateBuffsFromData(ObjectData obj, string name, string displayName, float durationMultiplier = 1f, Action<BuffEffects> adjustEffects = null)
		{
			List<ObjectBuffData> buffs = obj.Buffs;
			if (buffs == null || buffs.Count <= 0)
			{
				yield break;
			}
			foreach (ObjectBuffData data in obj.Buffs)
			{
				if (data != null)
				{
					string id = data.BuffId;
					bool flag = !string.IsNullOrWhiteSpace(id);
					if (!flag)
					{
						id = (obj.IsDrink ? "drink" : "food");
					}
					BuffEffects effects = new BuffEffects(data.CustomAttributes);
					if (adjustEffects != null)
					{
						adjustEffects(effects);
					}
					Texture2D texture = null;
					int spriteIndex = -1;
					if (data.IconTexture != null)
					{
						texture = Game1.content.Load<Texture2D>(data.IconTexture);
						spriteIndex = data.IconSpriteIndex;
					}
					int millisecondsDuration = -1;
					if (data.Duration == -2)
					{
						millisecondsDuration = -2;
					}
					else if (data.Duration != 0)
					{
						millisecondsDuration = (int)((float)data.Duration * durationMultiplier) * Game1.realMilliSecondsPerGameMinute;
					}
					bool isDebuff = data.IsDebuff;
					Color? glowColor = Utility.StringToColor(data.GlowColor);
					if (flag || ((millisecondsDuration > 0 || millisecondsDuration == -2) && effects.HasAnyValue()))
					{
						Buff buff = new Buff(id, name, displayName, millisecondsDuration, texture, spriteIndex, effects, new bool?(isDebuff), null, null);
						buff.customFields.TryAddMany(data.CustomFields);
						if (glowColor != null)
						{
							buff.glow = glowColor.Value;
						}
						yield return buff;
					}
				}
			}
			List<ObjectBuffData>.Enumerator enumerator = default(List<ObjectBuffData>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x0600134D RID: 4941 RVA: 0x000E91D8 File Offset: 0x000E73D8
		public virtual bool ShouldWobble()
		{
			if (this.minutesUntilReady.Value > 0 && !this.readyForHarvest.Value)
			{
				MachineData machineData = this.GetMachineData();
				if (machineData != null)
				{
					return machineData.WobbleWhileWorking && this.heldObject.Value != null;
				}
				if (this.bigCraftable.Value)
				{
					string qualifiedItemId = base.QualifiedItemId;
					return !(qualifiedItemId == "(BC)22") && !(qualifiedItemId == "(BC)23") && !(qualifiedItemId == "(BC)65") && !(qualifiedItemId == "(BC)66") && !(qualifiedItemId == "(BC)165");
				}
			}
			return false;
		}

		// Token: 0x0600134E RID: 4942 RVA: 0x000E9280 File Offset: 0x000E7480
		public virtual Vector2 getScale()
		{
			if (base.Category == -22)
			{
				return Vector2.Zero;
			}
			if (!this.bigCraftable.Value)
			{
				this.scale.Y = Math.Max(4f, this.scale.Y - 0.04f);
				return this.scale;
			}
			if (!this.ShouldWobble())
			{
				return Vector2.Zero;
			}
			if (base.QualifiedItemId.Equals("(BC)17"))
			{
				this.scale.X = (float)((double)(this.scale.X + 0.04f) % 6.283185307179586);
				return Vector2.Zero;
			}
			this.scale.X = this.scale.X - 0.1f;
			this.scale.Y = this.scale.Y + 0.1f;
			if (this.scale.X <= 0f)
			{
				this.scale.X = 10f;
			}
			if (this.scale.Y >= 10f)
			{
				this.scale.Y = 0f;
			}
			return new Vector2(Math.Abs(this.scale.X - 5f), Math.Abs(this.scale.Y - 5f));
		}

		// Token: 0x0600134F RID: 4943 RVA: 0x000E93C8 File Offset: 0x000E75C8
		public virtual void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			float drawLayer = Math.Max(0f, (float)(f.StandingPixel.Y + 3) / 10000f);
			Texture2D texture = itemData.GetTexture();
			int offset = 0;
			if (this is Mannequin)
			{
				offset = 2;
			}
			spriteBatch.Draw(texture, objectPosition, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(offset, new int?(base.ParentSheetIndex))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer);
		}

		// Token: 0x06001350 RID: 4944 RVA: 0x000E9448 File Offset: 0x000E7648
		public virtual void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
		{
			if (this.isPlaceable() && !(this is Wallpaper))
			{
				Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
				int x = (int)Game1.GetPlacementGrabTile().X * 64;
				int y = (int)Game1.GetPlacementGrabTile().Y * 64;
				if (Game1.isCheckingNonMousePlacement)
				{
					Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, x, y);
					x = (int)nearbyValidPlacementPosition.X;
					y = (int)nearbyValidPlacementPosition.Y;
				}
				Vector2 tile = new Vector2((float)(x / 64), (float)(y / 64));
				if (this.Equals(Game1.player.ActiveObject))
				{
					this.TileLocation = tile;
				}
				if (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y))
				{
					Object obj;
					bool flag;
					if (location.objects.TryGetValue(tile, out obj))
					{
						IndoorPot pot = obj as IndoorPot;
						if (pot != null)
						{
							flag = pot.IsPlantableItem(this);
							goto IL_C2;
						}
					}
					flag = false;
					IL_C2:
					if (!flag)
					{
						return;
					}
				}
				bool canPlaceHere = Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player, false) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player));
				Game1.isCheckingNonMousePlacement = false;
				int width = 1;
				int height = 1;
				Furniture furniture = this as Furniture;
				if (furniture != null)
				{
					width = furniture.getTilesWide();
					height = furniture.getTilesHigh();
				}
				for (int x_offset = 0; x_offset < width; x_offset++)
				{
					for (int y_offset = 0; y_offset < height; y_offset++)
					{
						spriteBatch.Draw(Game1.mouseCursors, new Vector2((tile.X + (float)x_offset) * 64f - (float)Game1.viewport.X, (tile.Y + (float)y_offset) * 64f - (float)Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
					}
				}
				if (this.bigCraftable.Value || this is Furniture || (this.category.Value != -74 && this.category.Value != -19))
				{
					this.draw(spriteBatch, (int)tile.X, (int)tile.Y, 0.5f);
				}
			}
		}

		// Token: 0x06001351 RID: 4945 RVA: 0x000E9674 File Offset: 0x000E7874
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			if (drawShadow && !this.bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
			{
				this.DrawShadow(spriteBatch, location, color, layerDepth);
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			float drawnScale = scaleSize;
			if (this.bigCraftable.Value && drawnScale > 0.2f)
			{
				drawnScale /= 2f;
			}
			int offset = 0;
			if (this is Mannequin)
			{
				offset = 2;
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = itemData.GetSourceRect(offset, new int?(base.ParentSheetIndex));
			spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle?(sourceRect), color * transparency, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), 4f * drawnScale, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001352 RID: 4946 RVA: 0x000E9780 File Offset: 0x000E7980
		public virtual void DrawShadow(SpriteBatch spriteBatch, Vector2 position, Color color, float layerDepth)
		{
			spriteBatch.Draw(Game1.shadowTexture, position + new Vector2(32f, 48f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
		}

		// Token: 0x06001353 RID: 4947 RVA: 0x000E9810 File Offset: 0x000E7A10
		public override void DrawIconBar(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color)
		{
			if (base.Category == -22 && this.uses.Value > 0)
			{
				float health = ((float)(FishingRod.maxTackleUses - this.uses.Value) + 0f) / (float)FishingRod.maxTackleUses;
				spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), Utility.getRedToGreenLerpColor(health));
			}
		}

		// Token: 0x06001354 RID: 4948 RVA: 0x000E9894 File Offset: 0x000E7A94
		public virtual void drawAsProp(SpriteBatch b)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			int x = (int)this.tileLocation.X;
			int y = (int)this.tileLocation.Y;
			if (this.bigCraftable.Value)
			{
				int indexOffset = 0;
				if (this.showNextIndex.Value)
				{
					indexOffset = 1;
				}
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Texture2D texture = itemData.GetTexture();
				Vector2 scaleFactor = this.getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
				Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				b.Draw(texture, destination, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(indexOffset, new int?(base.ParentSheetIndex))), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f) + (this.IsTapper() ? 0.0015f : 0f));
				if (base.QualifiedItemId == "(BC)17" && this.MinutesUntilReady > 0)
				{
					b.Draw(Game1.objectSpriteSheet, this.getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, -1, -1)), Color.White, this.scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f + 0.0001f));
					return;
				}
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBoxAt(x, y);
				if (base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)742" && base.QualifiedItemId != "(O)SeedSpot")
				{
					b.Draw(Game1.shadowTexture, this.getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)bounds.Bottom / 15000f);
				}
				ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				b.Draw(itemData2.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 32))), new Microsoft.Xna.Framework.Rectangle?(itemData2.GetSourceRect(0, null)), Color.White, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
			}
		}

		// Token: 0x06001355 RID: 4949 RVA: 0x000E9C0A File Offset: 0x000E7E0A
		public virtual void drawAboveFrontLayer(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
		}

		// Token: 0x06001356 RID: 4950 RVA: 0x000E9C0C File Offset: 0x000E7E0C
		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			GameLocation location = this.Location;
			if (this.hovering)
			{
				if (this.IsTextSign() && !string.IsNullOrEmpty(this.SignText))
				{
					Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64)));
					SpriteText.drawSmallTextBubble(spriteBatch, this.SignText, position, 256, 0.98f + this.TileLocation.X * 0.0001f + this.TileLocation.Y * 1E-06f, false);
				}
				this.hovering = false;
			}
			if (this.bigCraftable.Value)
			{
				Vector2 scaleFactor = this.getScale();
				scaleFactor *= 4f;
				Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
				Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position2.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position2.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
				int offset = 0;
				if (this.showNextIndex.Value)
				{
					offset = 1;
				}
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				if (this.heldObject.Value != null)
				{
					MachineData machineData = this.GetMachineData();
					if (machineData != null && machineData.IsIncubator)
					{
						FarmAnimalData animalDataFromEgg = FarmAnimal.GetAnimalDataFromEgg(this.heldObject.Value, location);
						offset = ((animalDataFromEgg != null) ? animalDataFromEgg.IncubatorParentSheetOffset : 1);
					}
				}
				if (this._machineAnimationFrame >= 0 && this._machineAnimation != null)
				{
					offset = this._machineAnimationFrame;
				}
				Mannequin mannequin = this as Mannequin;
				if (mannequin != null)
				{
					offset = mannequin.facing.Value;
				}
				if (this.IsTapper())
				{
					draw_layer = Math.Max(0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
				}
				if (base.QualifiedItemId == "(BC)272")
				{
					Texture2D texture = itemData.GetTexture();
					spriteBatch.Draw(texture, destination, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(1, new int?(base.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
					spriteBatch.Draw(texture, position2 + new Vector2(8.5f, 12f) * 4f, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(2, new int?(base.ParentSheetIndex))), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
					return;
				}
				spriteBatch.Draw(itemData.GetTexture(), destination, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(offset, new int?(base.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				if (base.QualifiedItemId == "(BC)17" && this.MinutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, this.getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16)), Color.White * alpha, this.scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
				}
				if (this.isLamp.Value && Game1.isDarkOut(this.Location))
				{
					spriteBatch.Draw(Game1.mouseCursors, position2 + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
				}
				if (base.QualifiedItemId == "(BC)126")
				{
					string hatId = (this.quality.Value != 0) ? (this.quality.Value - 1).ToString() : this.preservedParentSheetIndex.Value;
					if (hatId != null)
					{
						ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(H)" + hatId);
						Texture2D texture2 = dataOrErrorItem.GetTexture();
						int spriteIndex = dataOrErrorItem.SpriteIndex;
						bool isPrismatic = ItemContextTagManager.HasBaseTag("(H)" + hatId, "Prismatic");
						spriteBatch.Draw(texture2, position2 + new Vector2(-3f, -6f) * 4f, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(spriteIndex * 20 % texture2.Width, spriteIndex * 20 / texture2.Width * 20 * 4, 20, 20)), (isPrismatic ? Utility.GetPrismaticColor(0, 1f) : Color.White) * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
					}
				}
			}
			else if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
			{
				Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBoxAt(x, y);
				string qualifiedItemId = base.QualifiedItemId;
				if (qualifiedItemId == "(O)590")
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(368 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 32, 16, 16)), Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? bounds.Top : bounds.Bottom) / 10000f);
					return;
				}
				if (qualifiedItemId == "(O)SeedSpot")
				{
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(160 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 800.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 0, 17, 16)), Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1600.0 <= 400.0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? bounds.Top : bounds.Bottom) / 10000f);
					return;
				}
				if (this.fragility.Value != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 51 + 4))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)bounds.Bottom / 15000f);
				}
				ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				spriteBatch.Draw(itemData2.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(itemData2.GetSourceRect(0, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? bounds.Top : bounds.Center.Y) / 10000f);
				if (this.IsSprinkler())
				{
					if (this.heldObject.Value != null)
					{
						Vector2 offset2 = Vector2.Zero;
						if (this.heldObject.Value.QualifiedItemId == "(O)913")
						{
							offset2 = new Vector2(0f, -20f);
						}
						ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(this.heldObject.Value.QualifiedItemId);
						spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))) + offset2), new Microsoft.Xna.Framework.Rectangle?(heldItemData.GetSourceRect(1, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? bounds.Top : bounds.Bottom) / 10000f + 1E-05f);
					}
					if (base.SpecialVariable == 999999)
					{
						if (this.heldObject.Value != null && this.heldObject.Value.QualifiedItemId == "(O)913")
						{
							Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f, (float)(y * 64 - 32), (float)bounds.Bottom / 10000f + 1E-06f, 1f);
						}
						else
						{
							Torch.drawBasicTorch(spriteBatch, (float)(x * 64) - 2f, (float)(y * 64 - 32 + 12), (float)(bounds.Bottom + 2) / 10000f, 1f);
						}
					}
				}
			}
			if (this.readyForHarvest.Value)
			{
				float base_sort = (float)((y + 1) * 64) / 10000f + this.tileLocation.X / 50000f;
				if (this.IsTapper() || base.QualifiedItemId.Equals("(BC)MushroomLog"))
				{
					base_sort += 0.02f;
				}
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 - 8), (float)(y * 64 - 96 - 16) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort + 1E-06f);
				if (this.heldObject.Value != null)
				{
					ParsedItemData heldItemData2 = ItemRegistry.GetDataOrErrorItem(this.heldObject.Value.QualifiedItemId);
					Texture2D texture3 = heldItemData2.GetTexture();
					ColoredObject coloredObj = this.heldObject.Value as ColoredObject;
					if (coloredObj != null)
					{
						coloredObj.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64) - 96f - 8f + yOffset)), 1f, 0.75f, base_sort + 1.1E-05f);
						return;
					}
					spriteBatch.Draw(texture3, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(heldItemData2.GetSourceRect(0, null)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1E-05f);
					if (this.heldObject.Value.Stack > 1)
					{
						this.heldObject.Value.DrawMenuIcons(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 - 32) + yOffset - 4f)), 1f, 1f, base_sort + 1.2E-05f, StackDrawType.Draw, Color.White);
						return;
					}
					if (this.heldObject.Value.Quality > 0)
					{
						this.heldObject.Value.DrawMenuIcons(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64 - 32) + yOffset - 4f)), 1f, 1f, base_sort + 1.2E-05f, StackDrawType.HideButShowQuality, Color.White);
					}
				}
			}
		}

		// Token: 0x06001357 RID: 4951 RVA: 0x000EAB28 File Offset: 0x000E8D28
		public virtual void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			if (this.bigCraftable.Value)
			{
				Vector2 scaleFactor = this.getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
				Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				int indexOffset = 0;
				if (this.showNextIndex.Value)
				{
					indexOffset = 1;
				}
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				spriteBatch.Draw(itemData.GetTexture(), destination, new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(indexOffset, new int?(base.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				if (base.QualifiedItemId == "(BC)17" && this.MinutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(position) + new Vector2(32f, 0f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16)), Color.White * alpha, this.scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
				}
				if (this.isLamp.Value && Game1.isDarkOut(this.Location))
				{
					spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					return;
				}
			}
			else if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
			{
				if (base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot" && this.fragility.Value != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32), (float)(yNonTile + 51 + 4))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
				}
				ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				spriteBatch.Draw(itemData2.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(yNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(itemData2.GetSourceRect(0, new int?(base.ParentSheetIndex))), Color.White * alpha, 0f, new Vector2(8f, 8f), (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		// Token: 0x06001358 RID: 4952 RVA: 0x000EAF08 File Offset: 0x000E9108
		public override int maximumStackSize()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(O)79" || qualifiedItemId == "(O)842" || qualifiedItemId == "(O)911")
			{
				return 1;
			}
			if (base.Category == -22)
			{
				return 1;
			}
			return 999;
		}

		// Token: 0x06001359 RID: 4953 RVA: 0x000EAF56 File Offset: 0x000E9156
		public virtual void hoverAction()
		{
			this.hovering = true;
		}

		// Token: 0x0600135A RID: 4954 RVA: 0x000EAF5F File Offset: 0x000E915F
		public virtual bool clicked(Farmer who)
		{
			return false;
		}

		// Token: 0x0600135B RID: 4955 RVA: 0x000EAF62 File Offset: 0x000E9162
		protected override Item GetOneNew()
		{
			if (!this.bigCraftable.Value)
			{
				return new Object(base.ItemId, 1, false, -1, 0);
			}
			return new Object(this.tileLocation.Value, base.ItemId, false);
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000EAF98 File Offset: 0x000E9198
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Object fromObj = source as Object;
			if (fromObj != null)
			{
				this.Scale = fromObj.scale;
				this.IsSpawnedObject = fromObj.isSpawnedObject.Value;
				this.Price = fromObj.price.Value;
				this.Edibility = fromObj.edibility.Value;
				this.name = fromObj.name;
				this.displayNameFormat = fromObj.displayNameFormat;
				this.TileLocation = fromObj.TileLocation;
				this.uses.Value = fromObj.uses.Value;
				this.questItem.Value = fromObj.questItem.Value;
				this.questId.Value = fromObj.questId.Value;
				this.preserve.Value = fromObj.preserve.Value;
				this.preservedParentSheetIndex.Value = fromObj.preservedParentSheetIndex.Value;
				this.orderData.Value = fromObj.orderData.Value;
				this.owner.Value = fromObj.owner.Value;
			}
		}

		// Token: 0x0600135D RID: 4957 RVA: 0x000EB0B8 File Offset: 0x000E92B8
		public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
		{
			if (base.QualifiedItemId == "(O)710")
			{
				return CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y);
			}
			if (this.IsTapper())
			{
				Tree tree = l.terrainFeatures.GetValueOrDefault(tile, null) as Tree;
				if (tree != null && !l.objects.ContainsKey(tile))
				{
					WildTreeData data = tree.GetData();
					bool? flag = (data != null) ? new bool?(data.CanBeTapped()) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						return true;
					}
				}
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(O)805"))
			{
				if (qualifiedItemId == "(O)419")
				{
					Tree tree2 = l.terrainFeatures.GetValueOrDefault(tile, null) as Tree;
					return tree2 != null && !tree2.stopGrowingMoss.Value;
				}
			}
			else if (l.terrainFeatures.GetValueOrDefault(tile, null) is Tree)
			{
				return true;
			}
			if (Object.isWildTreeSeed(base.ItemId))
			{
				if (!l.CanItemBePlacedHere(tile, true, collisionMask, ~CollisionMask.Objects, false, false))
				{
					return false;
				}
				string deniedMessage;
				if (!this.canPlaceWildTreeSeed(l, tile, out deniedMessage))
				{
					if (showError && deniedMessage != null)
					{
						Game1.showRedMessage(deniedMessage, true);
					}
					return false;
				}
				return true;
			}
			else
			{
				int value = this.category.Value;
				if (value != -74)
				{
					if (value != -19)
					{
						if (l != null)
						{
							Vector2 nonTile = tile * 64f * 64f;
							nonTile.X += 32f;
							nonTile.Y += 32f;
							foreach (Furniture f in l.furniture)
							{
								if (f.furniture_type.Value == 11 && f.GetBoundingBox().Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null)
								{
									return true;
								}
							}
						}
						if (this.IsFloorPathItem())
						{
							collisionMask &= ~CollisionMask.Buildings;
						}
						return l.CanItemBePlacedHere(tile, this.isPassable(), collisionMask, ~CollisionMask.Objects, false, false);
					}
					HoeDirt dirt = l.GetHoeDirtAtTile(tile);
					if (dirt != null && dirt.CanApplyFertilizer(base.QualifiedItemId))
					{
						IndoorPot pot = l.getObjectAtTile((int)tile.X, (int)tile.Y, false) as IndoorPot;
						return pot == null || pot.IsPlantableItem(this);
					}
					return false;
				}
				else
				{
					HoeDirt dirt2 = l.GetHoeDirtAtTile(tile);
					Object obj = l.getObjectAtTile((int)tile.X, (int)tile.Y, false);
					IndoorPot pot2 = obj as IndoorPot;
					TerrainFeature terrainFeature;
					if (((dirt2 != null) ? dirt2.crop : null) != null || (dirt2 == null && l.terrainFeatures.TryGetValue(tile, out terrainFeature)))
					{
						return false;
					}
					if (this.IsFruitTreeSapling())
					{
						if (obj != null)
						{
							return false;
						}
						if (dirt2 != null)
						{
							return false;
						}
						if (FruitTree.IsTooCloseToAnotherTree(tile, l, !this.IsFruitTreeSapling()))
						{
							if (showError)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"), true);
							}
							return false;
						}
						if (FruitTree.IsGrowthBlocked(tile, l))
						{
							if (showError)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", this.DisplayName), true);
							}
							return false;
						}
						if (!l.CanItemBePlacedHere(tile, true, collisionMask, ~CollisionMask.Objects, false, false))
						{
							return false;
						}
						string deniedMessage2;
						if (!l.CanPlantTreesHere(base.ItemId, (int)tile.X, (int)tile.Y, out deniedMessage2))
						{
							if (showError && deniedMessage2 != null)
							{
								Game1.showRedMessage(deniedMessage2, true);
							}
							return false;
						}
						return true;
					}
					else if (this.IsTeaSapling())
					{
						bool isFreeGardenPot = pot2 != null && pot2.bush.Value == null && pot2.hoeDirt.Value.crop == null;
						if (isFreeGardenPot)
						{
							if (!l.IsOutdoors)
							{
								return true;
							}
						}
						else
						{
							if (obj != null || dirt2 != null)
							{
								return false;
							}
							if (!l.CanItemBePlacedHere(tile, true, collisionMask, ~CollisionMask.Objects, false, false))
							{
								return false;
							}
							if (l.IsGreenhouse && l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back", false) == null)
							{
								return false;
							}
						}
						string deniedMessage3;
						if (!l.CheckItemPlantRules(base.QualifiedItemId, isFreeGardenPot, l.isOutdoors.Value || l.IsGreenhouse, out deniedMessage3))
						{
							if (showError && deniedMessage3 != null)
							{
								Game1.showRedMessage(Game1.content.LoadString(deniedMessage3), true);
							}
							return false;
						}
						return true;
					}
					else
					{
						if (!this.IsWildTreeSapling())
						{
							if (this.HasTypeObject())
							{
								if (pot2 != null)
								{
									return pot2.IsPlantableItem(this) && pot2.bush.Value == null && pot2.hoeDirt.Value.canPlantThisSeedHere(base.ItemId, false);
								}
								if (dirt2 != null && l.CanItemBePlacedHere(tile, true, collisionMask, ~CollisionMask.Objects, false, false) && dirt2.canPlantThisSeedHere(base.ItemId, false))
								{
									return true;
								}
							}
							return false;
						}
						if (obj != null)
						{
							return false;
						}
						if (FruitTree.IsTooCloseToAnotherTree(tile, l, true))
						{
							if (showError)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060_Fruit"), true);
							}
							return false;
						}
						return l.CanItemBePlacedHere(tile, true, collisionMask, ~CollisionMask.Objects, false, false);
					}
				}
			}
		}

		// Token: 0x0600135E RID: 4958 RVA: 0x000EB5D4 File Offset: 0x000E97D4
		public override bool isPlaceable()
		{
			return base.HasContextTag("placeable") || (!base.HasContextTag("not_placeable") && (this.type.Value != null && (base.Category == -8 || base.Category == -9 || this.Type == "Crafting" || this.isSapling() || base.QualifiedItemId == "(O)710" || base.Category == -74 || base.Category == -19) && (this.edibility.Value < 0 || this.IsWildTreeSapling())));
		}

		// Token: 0x0600135F RID: 4959 RVA: 0x000EB67C File Offset: 0x000E987C
		public bool IsConsideredReadyMachineForComputer()
		{
			if (this.bigCraftable.Value && this.heldObject.Value != null)
			{
				Chest chest = this.heldObject.Value as Chest;
				if (chest == null)
				{
					return this.minutesUntilReady.Value <= 0;
				}
				if (!chest.isEmpty())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001360 RID: 4960 RVA: 0x000EB6D4 File Offset: 0x000E98D4
		public MachineData GetMachineData()
		{
			return DataLoader.Machines(Game1.content).GetValueOrDefault(base.QualifiedItemId);
		}

		// Token: 0x06001361 RID: 4961 RVA: 0x000EB6EB File Offset: 0x000E98EB
		public virtual bool isSapling()
		{
			return this.IsTeaSapling() || this.IsWildTreeSapling() || this.IsFruitTreeSapling();
		}

		// Token: 0x06001362 RID: 4962 RVA: 0x000EB705 File Offset: 0x000E9905
		public virtual bool IsTeaSapling()
		{
			return base.QualifiedItemId == "(O)251";
		}

		// Token: 0x06001363 RID: 4963 RVA: 0x000EB717 File Offset: 0x000E9917
		public virtual bool IsFruitTreeSapling()
		{
			return this.HasTypeObject() && Game1.fruitTreeData.ContainsKey(base.ItemId);
		}

		// Token: 0x06001364 RID: 4964 RVA: 0x000EB733 File Offset: 0x000E9933
		public virtual bool IsWildTreeSapling()
		{
			return this.HasTypeObject() && Object.isWildTreeSeed(base.ItemId);
		}

		// Token: 0x06001365 RID: 4965 RVA: 0x000EB74A File Offset: 0x000E994A
		public virtual bool IsFloorPathItem()
		{
			return this.HasTypeObject() && Object.IsFloorPathItem(base.ItemId);
		}

		// Token: 0x06001366 RID: 4966 RVA: 0x000EB761 File Offset: 0x000E9961
		public static bool IsFloorPathItem(string itemId)
		{
			return itemId != null && Flooring.GetFloorPathItemLookup().ContainsKey(itemId);
		}

		// Token: 0x06001367 RID: 4967 RVA: 0x000EB773 File Offset: 0x000E9973
		public virtual bool IsFenceItem()
		{
			return this.HasTypeObject() && Fence.GetFenceLookup().ContainsKey(base.ItemId);
		}

		// Token: 0x06001368 RID: 4968 RVA: 0x000EB78F File Offset: 0x000E998F
		public static bool isWildTreeSeed(string itemId)
		{
			return itemId != null && Tree.GetWildTreeSeedLookup().ContainsKey(itemId);
		}

		// Token: 0x06001369 RID: 4969 RVA: 0x000EB7A4 File Offset: 0x000E99A4
		private bool canPlaceWildTreeSeed(GameLocation location, Vector2 tile, out string deniedMessage)
		{
			if (location.IsNoSpawnTile(tile, "Tree", true))
			{
				deniedMessage = null;
				return false;
			}
			if (location.IsNoSpawnTile(tile, "Tree", false) && !location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"))
			{
				deniedMessage = null;
				return false;
			}
			if (location.objects.ContainsKey(tile))
			{
				deniedMessage = null;
				return false;
			}
			TerrainFeature terrainFeature;
			if (location.terrainFeatures.TryGetValue(tile, out terrainFeature) && !(terrainFeature is HoeDirt))
			{
				deniedMessage = null;
				return false;
			}
			return location.CanPlantTreesHere(base.ItemId, (int)tile.X, (int)tile.Y, out deniedMessage) && location.CheckItemPlantRules(base.QualifiedItemId, false, location is Farm || location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back", false) != null || location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"), out deniedMessage);
		}

		// Token: 0x0600136A RID: 4970 RVA: 0x000EB8AA File Offset: 0x000E9AAA
		public virtual bool IsSprinkler()
		{
			return this.GetBaseRadiusForSprinkler() >= 0;
		}

		// Token: 0x0600136B RID: 4971 RVA: 0x000EB8B8 File Offset: 0x000E9AB8
		public bool IsBreakableStone()
		{
			return base.Category == -999 && this.Name == "Stone";
		}

		// Token: 0x0600136C RID: 4972 RVA: 0x000EB8D9 File Offset: 0x000E9AD9
		public virtual bool IsTextSign()
		{
			return base.ItemId == "TextSign";
		}

		// Token: 0x0600136D RID: 4973 RVA: 0x000EB8EB File Offset: 0x000E9AEB
		public bool IsTwig()
		{
			return base.Category == -999 && this.Name == "Twig";
		}

		// Token: 0x0600136E RID: 4974 RVA: 0x000EB90C File Offset: 0x000E9B0C
		public bool isDebrisOrForage()
		{
			return this.IsWeeds() || this.IsBreakableStone() || this.IsTwig() || this.isForage();
		}

		// Token: 0x0600136F RID: 4975 RVA: 0x000EB92E File Offset: 0x000E9B2E
		public bool IsWeeds()
		{
			return base.Category == -999 && this.Name.ContainsIgnoreCase("weeds");
		}

		// Token: 0x06001370 RID: 4976 RVA: 0x000EB94F File Offset: 0x000E9B4F
		public virtual bool IsTapper()
		{
			return base.HasContextTag("tapper_item");
		}

		// Token: 0x06001371 RID: 4977 RVA: 0x000EB95C File Offset: 0x000E9B5C
		public virtual bool IsBar()
		{
			return base.QualifiedItemId == "(O)334" || base.QualifiedItemId == "(O)335" || base.QualifiedItemId == "(O)336" || base.QualifiedItemId == "(O)337" || base.QualifiedItemId == "(O)910";
		}

		// Token: 0x06001372 RID: 4978 RVA: 0x000EB9C3 File Offset: 0x000E9BC3
		public string GetPreservedItemId()
		{
			return Object.GetPreservedItemId(this.preserve.Value, this.preservedParentSheetIndex.Value);
		}

		// Token: 0x06001373 RID: 4979 RVA: 0x000EB9E0 File Offset: 0x000E9BE0
		public static string GetPreservedItemId(Object.PreserveType? preserveType, string preservedId)
		{
			if (preservedId == "-1" && preserveType.GetValueOrDefault() == Object.PreserveType.Honey)
			{
				preservedId = null;
			}
			return preservedId;
		}

		// Token: 0x06001374 RID: 4980 RVA: 0x000EBA00 File Offset: 0x000E9C00
		public virtual int GetModifiedRadiusForSprinkler()
		{
			int radius = this.GetBaseRadiusForSprinkler();
			if (radius < 0)
			{
				return -1;
			}
			if (this.heldObject.Value != null && this.heldObject.Value.QualifiedItemId == "(O)915")
			{
				radius++;
			}
			return radius;
		}

		// Token: 0x06001375 RID: 4981 RVA: 0x000EBA48 File Offset: 0x000E9C48
		public virtual int GetBaseRadiusForSprinkler()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(O)599")
			{
				return 0;
			}
			if (qualifiedItemId == "(O)621")
			{
				return 1;
			}
			if (!(qualifiedItemId == "(O)645"))
			{
				return -1;
			}
			return 2;
		}

		// Token: 0x06001376 RID: 4982 RVA: 0x000EBA8C File Offset: 0x000E9C8C
		public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
			this.health = 10;
			this.Location = location;
			this.TileLocation = placementTile;
			NetFieldBase<long, NetLong> netFieldBase = this.owner;
			Farmer who2 = who;
			netFieldBase.Value = ((who2 != null) ? who2.UniqueMultiplayerID : Game1.player.UniqueMultiplayerID);
			if (!this.bigCraftable.Value && !(this is Furniture))
			{
				if (this.IsSprinkler() && location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSprinklers", "Back") == "T")
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NoSprinklers"), true);
					return false;
				}
				if (this.IsWildTreeSapling())
				{
					string deniedMessage;
					if (!this.canPlaceWildTreeSeed(location, placementTile, out deniedMessage))
					{
						if (deniedMessage == null)
						{
							deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021");
						}
						Game1.showRedMessage(deniedMessage, true);
						return false;
					}
					string treeType = Tree.ResolveTreeTypeFromSeed(base.QualifiedItemId);
					if (treeType != null)
					{
						Game1.stats.Increment("wildtreesplanted", 1U);
						location.terrainFeatures.Remove(placementTile);
						location.terrainFeatures.Add(placementTile, new Tree(treeType, 0, false));
						location.playSound("dirtyHit", null, null, SoundContext.Default);
						return true;
					}
					return false;
				}
				else if (this.IsFloorPathItem())
				{
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					string key = Flooring.GetFloorPathItemLookup()[base.ItemId];
					location.terrainFeatures.Add(placementTile, new Flooring(key));
					FloorPathData floorData;
					if (Game1.floorPathData.TryGetValue(key, out floorData) && floorData.PlacementSound != null)
					{
						location.playSound(floorData.PlacementSound, null, null, SoundContext.Default);
					}
					return true;
				}
				else if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "torch_item"))
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.removeLightSource(this.GenerateLightSourceId(this.tileLocation.Value));
					GameLocation location2 = location;
					LightSource lightSource = this.lightSource;
					location2.removeLightSource((lightSource != null) ? lightSource.Id : null);
					new Torch(1, base.ItemId).placementAction(location, x, y, who ?? Game1.player);
					return true;
				}
				else if (this.IsFenceItem())
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					FenceData fenceData = Fence.GetFenceLookup()[base.ItemId];
					location.objects.Add(placementTile, new Fence(placementTile, base.ItemId, base.ItemId == "325"));
					if (fenceData.PlacementSound != null)
					{
						location.playSound(fenceData.PlacementSound, null, null, SoundContext.Default);
					}
					return true;
				}
				else
				{
					string qualifiedItemId = base.QualifiedItemId;
					if (qualifiedItemId != null)
					{
						int length = qualifiedItemId.Length;
						if (length == 6)
						{
							int idNum;
							switch (qualifiedItemId[5])
							{
							case '0':
								if (!(qualifiedItemId == "(O)710"))
								{
									goto IL_24C6;
								}
								if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
								{
									return false;
								}
								new CrabPot().placementAction(location, x, y, who);
								return true;
							case '1':
							case '2':
								goto IL_24C6;
							case '3':
								if (!(qualifiedItemId == "(O)893"))
								{
									goto IL_24C6;
								}
								break;
							case '4':
								if (!(qualifiedItemId == "(O)894"))
								{
									goto IL_24C6;
								}
								break;
							case '5':
								if (!(qualifiedItemId == "(O)895"))
								{
									if (!(qualifiedItemId == "(O)805"))
									{
										goto IL_24C6;
									}
									TerrainFeature terrainFeature;
									if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature))
									{
										Tree tree = terrainFeature as Tree;
										if (tree != null)
										{
											return tree.fertilize();
										}
									}
									return false;
								}
								break;
							case '6':
								if (!(qualifiedItemId == "(O)926"))
								{
									if (!(qualifiedItemId == "(O)286"))
									{
										goto IL_24C6;
									}
									using (IEnumerator<TemporaryAnimatedSprite> enumerator = location.temporarySprites.GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											if (enumerator.Current.position.Equals(placementTile * 64f))
											{
												return false;
											}
										}
									}
									idNum = Game1.random.Next();
									location.playSound("thudStep", null, null, SoundContext.Default);
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, true, false, location, who)
										{
											shakeIntensity = 0.5f,
											shakeIntensityChange = 0.002f,
											extraInfoForEndBehavior = idNum,
											endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID)
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
										{
											id = idNum
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, true, true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
										{
											delayBeforeAnimationStart = 100,
											id = idNum
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
										{
											delayBeforeAnimationStart = 200,
											id = idNum
										}
									});
									location.netAudio.StartPlaying("fuse");
									return true;
								}
								else
								{
									if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
									{
										return false;
									}
									location.objects.Add(placementTile, new Torch("278", true)
									{
										Fragility = 1,
										destroyOvernight = true
									});
									Utility.addSmokePuff(location, new Vector2((float)x, (float)y), 0, 2f, 0.02f, 0.75f, 0.002f);
									Utility.addSmokePuff(location, new Vector2((float)(x + 16), (float)(y + 16)), 0, 2f, 0.02f, 0.75f, 0.002f);
									Utility.addSmokePuff(location, new Vector2((float)(x + 32), (float)y), 0, 2f, 0.02f, 0.75f, 0.002f);
									Utility.addSmokePuff(location, new Vector2((float)(x + 48), (float)(y + 16)), 0, 2f, 0.02f, 0.75f, 0.002f);
									Utility.addSmokePuff(location, new Vector2((float)(x + 32), (float)(y + 32)), 0, 2f, 0.02f, 0.75f, 0.002f);
									Game1.playSound("fireball", null);
									return true;
								}
								break;
							case '7':
								if (qualifiedItemId == "(O)287")
								{
									using (IEnumerator<TemporaryAnimatedSprite> enumerator = location.temporarySprites.GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											if (enumerator.Current.position.Equals(placementTile * 64f))
											{
												return false;
											}
										}
									}
									idNum = Game1.random.Next();
									location.playSound("thudStep", null, null, SoundContext.Default);
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, true, false, location, who)
										{
											shakeIntensity = 0.5f,
											shakeIntensityChange = 0.002f,
											extraInfoForEndBehavior = idNum,
											endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID)
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
										{
											id = idNum
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
										{
											delayBeforeAnimationStart = 100,
											id = idNum
										}
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
										{
											delayBeforeAnimationStart = 200,
											id = idNum
										}
									});
									location.netAudio.StartPlaying("fuse");
									return true;
								}
								if (!(qualifiedItemId == "(O)297"))
								{
									goto IL_24C6;
								}
								if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
								{
									return false;
								}
								location.terrainFeatures.Add(placementTile, new Grass(1, 4));
								location.playSound("dirtyHit", null, null, SoundContext.Default);
								return true;
							case '8':
								if (!(qualifiedItemId == "(O)288"))
								{
									goto IL_24C6;
								}
								using (IEnumerator<TemporaryAnimatedSprite> enumerator = location.temporarySprites.GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										if (enumerator.Current.position.Equals(placementTile * 64f))
										{
											return false;
										}
									}
								}
								idNum = Game1.random.Next();
								location.playSound("thudStep", null, null, SoundContext.Default);
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(base.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, true, false, location, who)
									{
										shakeIntensity = 0.5f,
										shakeIntensityChange = 0.002f,
										extraInfoForEndBehavior = idNum,
										endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID)
									}
								});
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
									{
										id = idNum
									}
								});
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
									{
										delayBeforeAnimationStart = 100,
										id = idNum
									}
								});
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
									{
										delayBeforeAnimationStart = 200,
										id = idNum
									}
								});
								location.netAudio.StartPlaying("fuse");
								return true;
							case '9':
							{
								if (!(qualifiedItemId == "(O)419"))
								{
									goto IL_24C6;
								}
								TerrainFeature terrainFeature2;
								if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature2))
								{
									Tree tree2 = terrainFeature2 as Tree;
									if (tree2 != null && !tree2.stopGrowingMoss.Value)
									{
										tree2.hasMoss.Value = false;
										tree2.stopGrowingMoss.Value = true;
										Game1.playSound("slosh", null);
										Game1.playSound("glug", null);
										Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(21, tree2.Tile * 64f + new Vector2(0f, -64f), new Color(165, 100, 255), 8, false, 80f, 1, -1, (tree2.Tile.Y + 1.25f) * 64f / 10000f, 128, 0), location, 4, 64, 64);
										return true;
									}
								}
								return false;
							}
							default:
								goto IL_24C6;
							}
							int fireworkType = base.ParentSheetIndex - 893;
							int spriteX = 256 + fireworkType * 16;
							using (IEnumerator<TemporaryAnimatedSprite> enumerator = location.temporarySprites.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									if (enumerator.Current.position.Equals(placementTile * 64f))
									{
										return false;
									}
								}
							}
							idNum = Game1.random.Next();
							int idNumFirework = Game1.random.Next();
							location.playSound("thudStep", null, null, SoundContext.Default);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(spriteX, 397, 16, 16), 2400f, 1, 1, placementTile * 64f, false, false, -1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									shakeIntensity = 0.5f,
									shakeIntensityChange = 0.002f,
									extraInfoForEndBehavior = idNum,
									endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID),
									layerDepth = (placementTile.Y * 64f + 64f - 16f) / 10000f
								}
							});
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(spriteX, 397, 16, 16), 800f, 1, 0, placementTile * 64f, false, false, -1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									fireworkType = fireworkType,
									delayBeforeAnimationStart = 2400,
									acceleration = new Vector2(0f, -0.36f + (float)Game1.random.Next(10) / 100f),
									drawAboveAlwaysFront = true,
									startSound = "firework",
									shakeIntensity = 0.5f,
									shakeIntensityChange = 0.002f,
									extraInfoForEndBehavior = idNumFirework,
									endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID),
									id = Game1.random.Next(20, 31),
									Parent = location,
									owner = who
								}
							});
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
								{
									id = idNum
								}
							});
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, true, true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
								{
									delayBeforeAnimationStart = 100,
									id = idNum
								}
							});
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 40f, 5, 5, placementTile * 64f + new Vector2(11f, 12f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
								{
									delayBeforeAnimationStart = 200,
									id = idNum
								}
							});
							location.netAudio.StartPlaying("fuse");
							DelayedAction.functionAfterDelay(delegate
							{
								location.netAudio.StopPlaying("fuse");
							}, 2400);
							return true;
						}
						if (length != 10)
						{
							if (length != 19)
							{
								goto IL_24C6;
							}
							if (!(qualifiedItemId == "(O)BlueGrassStarter"))
							{
								goto IL_24C6;
							}
							if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
							{
								return false;
							}
							location.terrainFeatures.Add(placementTile, new Grass(7, 4));
							location.playSound("dirtyHit", null, null, SoundContext.Default);
							return true;
						}
						else
						{
							if (!(qualifiedItemId == "(O)TentKit"))
							{
								goto IL_24C6;
							}
							if (location == null || !location.IsOutdoors)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors_Message"), true);
								return false;
							}
							if (Utility.isFestivalDay((Game1.dayOfMonth + 1) % 28, (Game1.dayOfMonth == 28) ? ((Game1.season + 1) % (Season)4) : Game1.season, location.GetLocationContextId()))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"), true);
								return false;
							}
							PassiveFestivalData passiveFestival = null;
							string passiveFestivalID = null;
							if (Utility.TryGetPassiveFestivalDataForDay((Game1.dayOfMonth + 1) % 28, (Game1.dayOfMonth == 28) ? ((Game1.season + 1) % (Season)4) : Game1.season, null, out passiveFestivalID, out passiveFestival, false) && passiveFestival != null)
							{
								if (passiveFestival.MapReplacements != null)
								{
									using (Dictionary<string, string>.KeyCollection.Enumerator enumerator2 = passiveFestival.MapReplacements.Keys.GetEnumerator())
									{
										while (enumerator2.MoveNext())
										{
											if (enumerator2.Current.Equals(location.Name))
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"), true);
												return false;
											}
										}
									}
								}
								if (((passiveFestivalID.Equals("TroutDerby") && location.Name.Equals("Forest")) || (passiveFestivalID.Equals("SquidFest") && location.Name.Equals("Beach"))) && passiveFestival.StartDay > Game1.dayOfMonth)
								{
									Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:FestivalTentWarning"), true);
									return false;
								}
							}
							if (who == null)
							{
								goto IL_24C6;
							}
							Microsoft.Xna.Framework.Rectangle area = Microsoft.Xna.Framework.Rectangle.Empty;
							switch (Utility.getDirectionFromChange(placementTile, who.Tile))
							{
							case 0:
								area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 1f), (int)(placementTile.Y - 1f), 3, 2);
								break;
							case 1:
								area = new Microsoft.Xna.Framework.Rectangle((int)placementTile.X, (int)(placementTile.Y - 1f), 3, 2);
								break;
							case 2:
								area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 1f), (int)placementTile.Y, 3, 2);
								break;
							case 3:
								area = new Microsoft.Xna.Framework.Rectangle((int)(placementTile.X - 2f), (int)(placementTile.Y - 1f), 3, 2);
								break;
							}
							if (area != Microsoft.Xna.Framework.Rectangle.Empty && location.isAreaClear(area))
							{
								location.largeTerrainFeatures.Add(new Tent(new Vector2((float)(area.X + 1), (float)(area.Y + 1))));
								Game1.playSound("moss_cut", null);
								Game1.playSound("woodyHit", null);
								new Microsoft.Xna.Framework.Rectangle(area.X * 64, area.Y * 64, 192, 128);
								Utility.addDirtPuffs(location, area.X, area.Y, 3, 2, 9);
								return true;
							}
							Game1.showRedMessage(Game1.content.LoadString("Strings\\1_6_Strings:Tent_Blocked"), true);
							return false;
						}
						bool result;
						return result;
					}
				}
			}
			else
			{
				if (this.IsTapper())
				{
					TerrainFeature terrainFeature3;
					if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature3))
					{
						Tree tree3 = terrainFeature3 as Tree;
						if (tree3 != null && tree3.growthStage.Value >= 5 && !tree3.stump.Value && !location.objects.ContainsKey(placementTile) && (!tree3.isTemporaryGreenRainTree.Value || Game1.season != Season.Summer))
						{
							WildTreeData data = tree3.GetData();
							if (data != null && data.CanBeTapped())
							{
								Object tapper = (Object)base.getOne();
								tapper.heldObject.Value = null;
								tapper.TileLocation = placementTile;
								location.objects.Add(placementTile, tapper);
								tree3.tapped.Value = true;
								tree3.UpdateTapperProduct(tapper, null, false);
								location.playSound("axe", null, null, SoundContext.Default);
								return true;
							}
						}
					}
					return false;
				}
				if (base.HasContextTag("sign_item"))
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Sign(placementTile, base.ItemId));
					location.playSound("axe", null, null, SoundContext.Default);
					return true;
				}
				else if (base.HasContextTag("torch_item"))
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					new Torch(base.ItemId, true)
					{
						shakeTimer = 25
					}.placementAction(location, x, y, who);
					return true;
				}
				else
				{
					string qualifiedItemId = base.QualifiedItemId;
					if (qualifiedItemId != null)
					{
						int length = qualifiedItemId.Length;
						if (length <= 7)
						{
							if (length != 6)
							{
								if (length == 7)
								{
									switch (qualifiedItemId[6])
									{
									case '0':
										if (!(qualifiedItemId == "(BC)130"))
										{
											goto IL_24C6;
										}
										break;
									case '1':
									{
										if (!(qualifiedItemId == "(BC)211"))
										{
											goto IL_24C6;
										}
										WoodChipper wood_chipper = (this as WoodChipper) ?? new WoodChipper(placementTile);
										wood_chipper.placementAction(location, x, y, null);
										location.objects.Add(placementTile, wood_chipper);
										location.playSound("hammer", null, null, SoundContext.Default);
										return true;
									}
									case '2':
										if (!(qualifiedItemId == "(BC)232"))
										{
											goto IL_24C6;
										}
										break;
									case '3':
										if (!(qualifiedItemId == "(BC)163"))
										{
											goto IL_24C6;
										}
										location.objects.Add(placementTile, new Cask(placementTile));
										location.playSound("hammer", null, null, SoundContext.Default);
										goto IL_24C6;
									case '4':
									{
										if (qualifiedItemId == "(BC)214")
										{
											Phone phone = (this as Phone) ?? new Phone(placementTile);
											location.objects.Add(placementTile, phone);
											location.playSound("hammer", null, null, SoundContext.Default);
											return true;
										}
										if (!(qualifiedItemId == "(BC)254"))
										{
											goto IL_24C6;
										}
										AnimalHouse animalHouse = location as AnimalHouse;
										if (animalHouse == null || !animalHouse.name.Value.Contains("Barn"))
										{
											Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MustBePlacedInBarn"), true);
											return false;
										}
										goto IL_24C6;
									}
									case '5':
									{
										if (qualifiedItemId == "(BC)165")
										{
											Object autoGrabber = ItemRegistry.Create<Object>("(BC)165", 1, 0, false);
											location.objects.Add(placementTile, autoGrabber);
											autoGrabber.heldObject.Value = new Chest();
											location.playSound("axe", null, null, SoundContext.Default);
											return true;
										}
										if (!(qualifiedItemId == "(BC)275"))
										{
											goto IL_24C6;
										}
										if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
										{
											Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
											return false;
										}
										Chest chest2 = new Chest(true, placementTile, base.ItemId)
										{
											name = this.name,
											shakeTimer = 50
										};
										chest2.lidFrameCount.Value = 2;
										location.objects.Add(placementTile, chest2);
										location.playSound("axe", null, null, SoundContext.Default);
										return true;
									}
									case '6':
										if (!(qualifiedItemId == "(BC)216"))
										{
											if (!(qualifiedItemId == "(BC)256"))
											{
												goto IL_24C6;
											}
											if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
												return false;
											}
											location.objects.Add(placementTile, new Chest(true, placementTile, base.ItemId)
											{
												name = this.name,
												shakeTimer = 50
											});
											location.playSound("axe", null, null, SoundContext.Default);
											return true;
										}
										else
										{
											if (location.objects.ContainsKey(placementTile))
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
												return false;
											}
											bool allowPlacement;
											if (!location.TryGetMapPropertyAs("AllowMiniFridges", out allowPlacement, false))
											{
												FarmHouse farmHouse = location as FarmHouse;
												if (farmHouse != null && farmHouse.upgradeLevel < 1)
												{
													Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"), true);
													return false;
												}
												allowPlacement = (location is FarmHouse || location is IslandFarmHouse);
											}
											if (!allowPlacement)
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
												return false;
											}
											Chest fridge = new Chest("216", placementTile, 217, 2)
											{
												shakeTimer = 50
											};
											fridge.fridge.Value = true;
											location.objects.Add(placementTile, fridge);
											location.playSound("hammer", null, null, SoundContext.Default);
											return true;
										}
										break;
									case '7':
										goto IL_24C6;
									case '8':
										if (qualifiedItemId == "(BC)108")
										{
											Object tub = (Object)base.getOne();
											tub.ResetParentSheetIndex();
											Season season = location.GetSeason();
											if (this.Location.IsOutdoors && (season == Season.Winter || season == Season.Fall))
											{
												tub.ParentSheetIndex = 109;
											}
											location.Objects.Add(placementTile, tub);
											Game1.playSound("axe", null);
											return true;
										}
										if (qualifiedItemId == "(BC)208")
										{
											location.objects.Add(placementTile, new Workbench(placementTile));
											location.playSound("axe", null, null, SoundContext.Default);
											return true;
										}
										if (!(qualifiedItemId == "(BC)248"))
										{
											if (!(qualifiedItemId == "(BC)238"))
											{
												goto IL_24C6;
											}
											if (!(location is Farm))
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceOnFarm"), true);
												return false;
											}
											Vector2 obelisk = Vector2.Zero;
											Vector2 obelisk2 = Vector2.Zero;
											foreach (KeyValuePair<Vector2, Object> o in location.objects.Pairs)
											{
												if (o.Value.QualifiedItemId == "(BC)238")
												{
													if (obelisk.Equals(Vector2.Zero))
													{
														obelisk = o.Key;
													}
													else if (obelisk2.Equals(Vector2.Zero))
													{
														obelisk2 = o.Key;
														break;
													}
												}
											}
											if (!obelisk.Equals(Vector2.Zero) && !obelisk2.Equals(Vector2.Zero))
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceTwo"), true);
												return false;
											}
											goto IL_24C6;
										}
										else
										{
											if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
											{
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
												return false;
											}
											location.objects.Add(placementTile, new Chest(true, placementTile, base.ItemId)
											{
												name = this.name,
												shakeTimer = 50
											});
											location.playSound("axe", null, null, SoundContext.Default);
											return true;
										}
										break;
									case '9':
									{
										if (!(qualifiedItemId == "(BC)209"))
										{
											goto IL_24C6;
										}
										MiniJukebox mini_jukebox = (this as MiniJukebox) ?? new MiniJukebox(placementTile);
										location.objects.Add(placementTile, mini_jukebox);
										mini_jukebox.RegisterToLocation();
										location.playSound("hammer", null, null, SoundContext.Default);
										return true;
									}
									default:
										goto IL_24C6;
									}
									if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
									{
										Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
										return false;
									}
									location.objects.Add(placementTile, new Chest(true, placementTile, base.ItemId)
									{
										name = this.name,
										shakeTimer = 50
									});
									location.playSound((base.QualifiedItemId == "(BC)130") ? "axe" : "hammer", null, null, SoundContext.Default);
									return true;
								}
							}
							else
							{
								char c = qualifiedItemId[4];
								if (c != '6')
								{
									if (c == '7')
									{
										if (qualifiedItemId == "(BC)71")
										{
											MineShaft mine = location as MineShaft;
											if (mine != null)
											{
												if (mine.shouldCreateLadderOnThisLevel() && mine.recursiveTryToCreateLadderDown(placementTile, "hoeHit", 16))
												{
													MineShaft.numberOfCraftedStairsUsedThisRun++;
													return true;
												}
												Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
											}
											else if (location.Name.Equals("ManorHouse") && x >= 1088)
											{
												Game1.warpFarmer("LewisBasement", 4, 4, 2);
												Game1.playSound("stairsdown", null);
												Game1.screenGlowOnce(Color.Black, true, 1f, 1f);
												return true;
											}
											return false;
										}
									}
								}
								else if (qualifiedItemId == "(BC)62")
								{
									location.objects.Add(placementTile, new IndoorPot(placementTile));
								}
							}
						}
						else
						{
							if (length != 12)
							{
								if (length != 17)
								{
									goto IL_24C6;
								}
								if (!(qualifiedItemId == "(BC)BigStoneChest"))
								{
									goto IL_24C6;
								}
							}
							else if (!(qualifiedItemId == "(BC)BigChest"))
							{
								goto IL_24C6;
							}
							if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
								return false;
							}
							Chest bigchest = new Chest(true, placementTile, base.ItemId)
							{
								shakeTimer = 50
							};
							location.objects.Add(placementTile, bigchest);
							location.playSound((base.QualifiedItemId == "(BC)BigChest") ? "axe" : "hammer", null, null, SoundContext.Default);
							return true;
						}
					}
				}
			}
			IL_24C6:
			TerrainFeature terrainFeature4;
			if (base.Category == -19 && location.terrainFeatures.TryGetValue(placementTile, out terrainFeature4))
			{
				HoeDirt dirt3 = terrainFeature4 as HoeDirt;
				if (dirt3 != null && dirt3.crop != null && (base.QualifiedItemId == "(O)369" || base.QualifiedItemId == "(O)368") && dirt3.crop.currentPhase.Value != 0)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"), true);
					return false;
				}
			}
			if (this.isSapling())
			{
				if (this.IsWildTreeSapling() || this.IsFruitTreeSapling())
				{
					if (FruitTree.IsTooCloseToAnotherTree(new Vector2((float)(x / 64), (float)(y / 64)), location, false))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"), true);
						return false;
					}
					if (FruitTree.IsGrowthBlocked(new Vector2((float)(x / 64), (float)(y / 64)), location))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", this.DisplayName), true);
						return false;
					}
				}
				TerrainFeature terrainFeature5;
				if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature5))
				{
					HoeDirt dirt2 = terrainFeature5 as HoeDirt;
					if (dirt2 == null || dirt2.crop != null)
					{
						return false;
					}
					location.terrainFeatures.Remove(placementTile);
				}
				string deniedMessage2 = null;
				bool canDig = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back", false) != null;
				string tileType = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Type", "Back", false);
				bool canPlantTrees = location.doesEitherTileOrTileIndexPropertyEqual((int)placementTile.X, (int)placementTile.Y, "CanPlantTrees", "Back", "T");
				if ((!(location is Farm) || (!canDig && !(tileType == "Grass") && !(tileType == "Dirt") && !canPlantTrees) || (location.IsNoSpawnTile(placementTile, "Tree", false) && !canPlantTrees)) && ((!canDig && !(tileType == "Stone")) || !location.CanPlantTreesHere(base.ItemId, (int)placementTile.X, (int)placementTile.Y, out deniedMessage2)))
				{
					if (deniedMessage2 == null)
					{
						deniedMessage2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068");
					}
					Game1.showRedMessage(deniedMessage2, true);
					return false;
				}
				location.playSound("dirtyHit", null, null, SoundContext.Default);
				DelayedAction.playSoundAfterDelay("coin", 100, null, null, -1, false);
				if (this.IsTeaSapling())
				{
					location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location, -1));
					return true;
				}
				FruitTree fruitTree = new FruitTree(base.ItemId, 0)
				{
					GreenHouseTileTree = (location.IsGreenhouse && tileType == "Stone")
				};
				fruitTree.growthRate.Value = Math.Max(1, base.Quality + 1);
				location.terrainFeatures.Add(placementTile, fruitTree);
				return true;
			}
			else
			{
				if (base.Category == -74 || base.Category == -19)
				{
					TerrainFeature terrainFeature6;
					if (location.terrainFeatures.TryGetValue(placementTile, out terrainFeature6))
					{
						HoeDirt dirt = terrainFeature6 as HoeDirt;
						if (dirt != null)
						{
							string seedId = Crop.ResolveSeedId(who.ActiveObject.ItemId, location);
							if (!dirt.canPlantThisSeedHere(seedId, who.ActiveObject.Category == -19))
							{
								return false;
							}
							if (dirt.plant(seedId, who, who.ActiveObject.Category == -19) && who.IsLocalPlayer)
							{
								if (base.Category == -74)
								{
									foreach (Object o2 in location.Objects.Values)
									{
										if (o2.IsSprinkler() && o2.heldObject.Value != null && o2.heldObject.Value.QualifiedItemId == "(O)913" && o2.IsInSprinklerRangeBroadphase(placementTile))
										{
											if (o2.GetSprinklerTiles().Contains(placementTile))
											{
												Object value = o2.heldObject.Value.heldObject.Value;
												Chest chest = value as Chest;
												if (chest != null)
												{
													IInventory items = chest.Items;
													if (items.Count > 0 && items[0] != null && !chest.GetMutex().IsLocked())
													{
														chest.GetMutex().RequestLock(delegate
														{
															if (items.Count > 0 && items[0] != null)
															{
																Item item = items[0];
																if (item.Category == -19 && dirt.plant(item.ItemId, who, true))
																{
																	items[0] = item.ConsumeStack(1);
																}
															}
															chest.GetMutex().ReleaseLock();
														}, null);
														break;
													}
												}
											}
										}
									}
								}
								Game1.haltAfterCheck = false;
								return true;
							}
							return false;
						}
					}
					return false;
				}
				if (!this.performDropDownAction(who))
				{
					Object toPlace = (Object)base.getOne();
					bool place_furniture_instance_instead = false;
					if (toPlace.GetType() == typeof(Furniture) && Furniture.GetFurnitureInstance(base.ItemId, new Vector2?(new Vector2((float)(x / 64), (float)(y / 64)))).GetType() != toPlace.GetType())
					{
						StorageFurniture storageFurniture = new StorageFurniture(base.ItemId, new Vector2((float)(x / 64), (float)(y / 64)));
						storageFurniture.currentRotation.Value = (this as Furniture).currentRotation.Value;
						storageFurniture.updateRotation();
						toPlace = storageFurniture;
						place_furniture_instance_instead = true;
					}
					toPlace.shakeTimer = 50;
					toPlace.Location = location;
					toPlace.TileLocation = placementTile;
					toPlace.performDropDownAction(who);
					if (this.IsTextSign())
					{
						toPlace.signText.Value = null;
						toPlace.showNextIndex.Value = (toPlace.QualifiedItemId == "(BC)TextSign");
					}
					if (toPlace.name.Contains("Seasonal"))
					{
						int baseIndex = toPlace.ParentSheetIndex - toPlace.ParentSheetIndex % 4;
						toPlace.ParentSheetIndex = baseIndex + location.GetSeasonIndex();
					}
					Object tileObj;
					if (!(toPlace is Furniture) && location.objects.TryGetValue(placementTile, out tileObj))
					{
						if (tileObj.QualifiedItemId != base.QualifiedItemId)
						{
							Game1.createItemDebris(tileObj, placementTile * 64f, Game1.random.Next(4), null, -1, false);
							location.objects[placementTile] = toPlace;
						}
					}
					else
					{
						Furniture furniture = toPlace as Furniture;
						if (furniture != null)
						{
							if (place_furniture_instance_instead)
							{
								location.furniture.Add(furniture);
							}
							else
							{
								location.furniture.Add(this as Furniture);
							}
						}
						else
						{
							location.objects.Add(placementTile, toPlace);
						}
					}
					toPlace.initializeLightSource(placementTile, false);
				}
				location.playSound("woodyStep", null, null, SoundContext.Default);
				return true;
			}
		}

		// Token: 0x06001377 RID: 4983 RVA: 0x000EE7D8 File Offset: 0x000EC9D8
		protected override void MigrateLegacyItemId()
		{
			if (this.bigCraftable.Value && !Game1.bigCraftableData.ContainsKey(base.ParentSheetIndex.ToString()))
			{
				if (base.ParentSheetIndex >= 56 && base.ParentSheetIndex <= 61)
				{
					base.ItemId = "56";
					return;
				}
				if (base.ParentSheetIndex >= 101 && base.ParentSheetIndex <= 103)
				{
					this.SetIdAndSprite(101);
					return;
				}
				if (this.name.Contains("Seasonal"))
				{
					base.ItemId = (base.ParentSheetIndex - base.ParentSheetIndex % 4).ToString();
					return;
				}
				if (Game1.bigCraftableData.ContainsKey((base.ParentSheetIndex - 1).ToString()))
				{
					base.ItemId = (base.ParentSheetIndex - 1).ToString();
					return;
				}
			}
			base.MigrateLegacyItemId();
		}

		// Token: 0x06001378 RID: 4984 RVA: 0x000EE8B8 File Offset: 0x000ECAB8
		public override bool actionWhenPurchased(string shopId)
		{
			if (base.QualifiedItemId == "(O)434")
			{
				if (!Game1.isFestival())
				{
					Game1.player.mailReceived.Add("CF_Sewer");
				}
				else
				{
					Game1.player.mailReceived.Add("CF_Fair");
				}
				Game1.exitActiveMenu();
				Game1.player.eatObject(this, true);
			}
			return base.actionWhenPurchased(shopId) || this.isRecipe.Value;
		}

		// Token: 0x06001379 RID: 4985 RVA: 0x000EE931 File Offset: 0x000ECB31
		public virtual bool needsToBeDonated()
		{
			return LibraryMuseum.IsItemSuitableForDonation(base.QualifiedItemId, true);
		}

		// Token: 0x0600137A RID: 4986 RVA: 0x000EE940 File Offset: 0x000ECB40
		public override string getDescription()
		{
			if (base.Category == -102 && Game1.player.stats.Get(this.itemId.Value) > 0U && base.ItemId != "Book_PriceCatalogue" && base.ItemId != "Book_AnimalCatalogue")
			{
				foreach (string tag in base.GetContextTags())
				{
					if (tag.StartsWithIgnoreCase("book_xp_"))
					{
						string whichSkill = tag.Split('_', StringSplitOptions.None)[2];
						return Game1.parseText(Game1.content.LoadString("Strings\\1_6_Strings:alreadyreadbook", Farmer.getSkillDisplayNameFromIndex(Farmer.getSkillNumberFromName(whichSkill)).ToLower()), Game1.smallFont, this.getDescriptionWidth());
					}
				}
				return Game1.parseText(Game1.content.LoadString("Strings\\1_6_Strings:alreadyreadbook_random"), Game1.smallFont, this.getDescriptionWidth());
			}
			if (this.isRecipe.Value)
			{
				if (base.Category == -7)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13073", this.loadDisplayName());
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13074", this.loadDisplayName());
			}
			else
			{
				if (this.needsToBeDonated())
				{
					return Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13078"), Game1.smallFont, this.getDescriptionWidth());
				}
				string text = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
				string preservedId = this.GetPreservedItemId();
				if (preservedId != null)
				{
					ParsedItemData preservedData = ItemRegistry.GetDataOrErrorItem(preservedId);
					text = string.Format(text, preservedData.DisplayName, preservedData.DisplayName.ToLower());
				}
				return Game1.parseText(text, Game1.smallFont, this.getDescriptionWidth());
			}
		}

		// Token: 0x0600137B RID: 4987 RVA: 0x000EEB14 File Offset: 0x000ECD14
		public virtual string GenerateLightSourceId(Vector2 position)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (this.Location == null)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral("_Held_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(this.Location.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<float>(position.X);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<float>(position.Y);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600137C RID: 4988 RVA: 0x000EEBDC File Offset: 0x000ECDDC
		public override int sellToStorePrice(long specificPlayerID = -1L)
		{
			if (this is Fence)
			{
				return this.price.Value;
			}
			if (base.Category == -22)
			{
				return (int)((float)this.price.Value * (1f + (float)this.quality.Value * 0.25f) * (((float)(FishingRod.maxTackleUses - this.uses.Value) + 0f) / (float)FishingRod.maxTackleUses));
			}
			float salePrice = (float)((int)((float)this.price.Value * (1f + (float)base.Quality * 0.25f)));
			salePrice = this.getPriceAfterMultipliers(salePrice, specificPlayerID);
			if (base.QualifiedItemId == "(O)493")
			{
				salePrice /= 2f;
			}
			if (salePrice > 0f)
			{
				salePrice = Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier);
			}
			return (int)salePrice;
		}

		// Token: 0x0600137D RID: 4989 RVA: 0x000EECB8 File Offset: 0x000ECEB8
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			if (this is Fence)
			{
				return this.price.Value;
			}
			if (this.isRecipe.Value)
			{
				return this.price.Value * 10;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(O)388"))
			{
				if (!(qualifiedItemId == "(O)390"))
				{
					if (!(qualifiedItemId == "(O)382"))
					{
						if (!(qualifiedItemId == "(O)378"))
						{
							if (!(qualifiedItemId == "(O)380"))
							{
								if (!(qualifiedItemId == "(O)384"))
								{
									float salePrice = (float)((int)((float)(this.price.Value * 2) * (1f + (float)this.quality.Value * 0.25f)));
									if (!ignoreProfitMargins && this.appliesProfitMargins())
									{
										salePrice = (float)((int)Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier));
									}
									return (int)salePrice;
								}
								if (Game1.year <= 1)
								{
									return 350;
								}
								return 750;
							}
							else
							{
								if (Game1.year <= 1)
								{
									return 150;
								}
								return 250;
							}
						}
						else
						{
							if (Game1.year <= 1)
							{
								return 80;
							}
							return 160;
						}
					}
					else
					{
						if (Game1.year <= 1)
						{
							return 120;
						}
						return 250;
					}
				}
				else
				{
					if (Game1.year <= 1)
					{
						return 20;
					}
					return 100;
				}
			}
			else
			{
				if (Game1.year <= 1)
				{
					return 10;
				}
				return 50;
			}
		}

		// Token: 0x0600137E RID: 4990 RVA: 0x000EEE02 File Offset: 0x000ED002
		public override bool appliesProfitMargins()
		{
			return this.category.Value == -74 || this.isSapling() || base.appliesProfitMargins();
		}

		// Token: 0x0600137F RID: 4991 RVA: 0x000EEE24 File Offset: 0x000ED024
		protected virtual float getPriceAfterMultipliers(float startPrice, long specificPlayerID = -1L)
		{
			string lowerName = this.name.ToLower();
			bool animalGood = lowerName.Contains("mayonnaise") || lowerName.Contains("cheese") || lowerName.Contains("cloth") || lowerName.Contains("wool");
			float saleMultiplier = 1f;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (Game1.player.useSeparateWallets)
				{
					if (specificPlayerID == -1L)
					{
						if (player.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							continue;
						}
						if (!player.isActive())
						{
							continue;
						}
					}
					else if (player.UniqueMultiplayerID != specificPlayerID)
					{
						continue;
					}
				}
				else if (!player.isActive())
				{
					continue;
				}
				float multiplier = 1f;
				if (player.professions.Contains(0) && (animalGood || base.Category == -5 || base.Category == -6 || base.Category == -18))
				{
					multiplier *= 1.2f;
				}
				if (player.professions.Contains(1) && (base.Category == -75 || base.Category == -80 || (base.Category == -79 && !this.isSpawnedObject.Value)))
				{
					multiplier *= 1.1f;
				}
				if (player.professions.Contains(4) && base.Category == -26)
				{
					multiplier *= 1.4f;
				}
				if (player.professions.Contains(6) && (base.Category == -4 || (this.preserve != null && this.preserve.Value != null && this.preserve.Value.GetValueOrDefault() == Object.PreserveType.SmokedFish)))
				{
					multiplier *= (player.professions.Contains(8) ? 1.5f : 1.25f);
				}
				if (player.professions.Contains(15) && base.Category == -27)
				{
					multiplier *= 1.25f;
				}
				if (player.professions.Contains(20) && this.IsBar())
				{
					multiplier *= 1.5f;
				}
				if (player.professions.Contains(23) && (base.Category == -2 || base.Category == -12))
				{
					multiplier *= 1.3f;
				}
				if (player.eventsSeen.Contains("2120303") && (base.QualifiedItemId == "(O)296" || base.QualifiedItemId == "(O)410"))
				{
					multiplier *= 3f;
				}
				if (player.eventsSeen.Contains("3910979") && base.QualifiedItemId == "(O)399")
				{
					multiplier *= 5f;
				}
				if (player.stats.Get("Book_Artifact") > 0U && this.Type != null && this.Type.Equals("Arch"))
				{
					multiplier *= 3f;
				}
				saleMultiplier = Math.Max(saleMultiplier, multiplier);
			}
			return startPrice * saleMultiplier;
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x000EF15C File Offset: 0x000ED35C
		public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToField<Object>(this.heldObject, handler, getPath, null);
		}

		// Token: 0x04000B29 RID: 2857
		public const int wood = 388;

		// Token: 0x04000B2A RID: 2858
		public const int stone = 390;

		// Token: 0x04000B2B RID: 2859
		public const int copper = 378;

		// Token: 0x04000B2C RID: 2860
		public const int iron = 380;

		// Token: 0x04000B2D RID: 2861
		public const int coal = 382;

		// Token: 0x04000B2E RID: 2862
		public const int gold = 384;

		// Token: 0x04000B2F RID: 2863
		public const int iridium = 386;

		// Token: 0x04000B30 RID: 2864
		public const string artifactSpotID = "590";

		// Token: 0x04000B31 RID: 2865
		public const string hayID = "178";

		// Token: 0x04000B32 RID: 2866
		public const string iridiumBarID = "337";

		// Token: 0x04000B33 RID: 2867
		public const string woodID = "388";

		// Token: 0x04000B34 RID: 2868
		public const string stoneID = "390";

		// Token: 0x04000B35 RID: 2869
		public const string copperID = "378";

		// Token: 0x04000B36 RID: 2870
		public const string ironID = "380";

		// Token: 0x04000B37 RID: 2871
		public const string coalID = "382";

		// Token: 0x04000B38 RID: 2872
		public const string goldID = "384";

		// Token: 0x04000B39 RID: 2873
		public const string iridiumID = "386";

		// Token: 0x04000B3A RID: 2874
		public const string amethystClusterID = "66";

		// Token: 0x04000B3B RID: 2875
		public const string aquamarineID = "62";

		// Token: 0x04000B3C RID: 2876
		public const string bobberID = "133";

		// Token: 0x04000B3D RID: 2877
		public const string caveCarrotID = "78";

		// Token: 0x04000B3E RID: 2878
		public const string diamondID = "72";

		// Token: 0x04000B3F RID: 2879
		public const string emeraldID = "60";

		// Token: 0x04000B40 RID: 2880
		public const string prismaticShardID = "74";

		// Token: 0x04000B41 RID: 2881
		public const string quartzID = "80";

		// Token: 0x04000B42 RID: 2882
		public const string rubyID = "64";

		// Token: 0x04000B43 RID: 2883
		public const string sapphireID = "70";

		// Token: 0x04000B44 RID: 2884
		public const string stardropID = "434";

		// Token: 0x04000B45 RID: 2885
		public const string topazID = "68";

		// Token: 0x04000B46 RID: 2886
		public const string artifactSpotQID = "(O)590";

		// Token: 0x04000B47 RID: 2887
		public const string hayQID = "(O)178";

		// Token: 0x04000B48 RID: 2888
		public const string copperBarQID = "(O)334";

		// Token: 0x04000B49 RID: 2889
		public const string ironBarQID = "(O)335";

		// Token: 0x04000B4A RID: 2890
		public const string goldBarQID = "(O)336";

		// Token: 0x04000B4B RID: 2891
		public const string iridiumBarQID = "(O)337";

		// Token: 0x04000B4C RID: 2892
		public const string woodQID = "(O)388";

		// Token: 0x04000B4D RID: 2893
		public const string stoneQID = "(O)390";

		// Token: 0x04000B4E RID: 2894
		public const string copperQID = "(O)378";

		// Token: 0x04000B4F RID: 2895
		public const string ironQID = "(O)380";

		// Token: 0x04000B50 RID: 2896
		public const string coalQID = "(O)382";

		// Token: 0x04000B51 RID: 2897
		public const string goldQID = "(O)384";

		// Token: 0x04000B52 RID: 2898
		public const string iridiumQID = "(O)386";

		// Token: 0x04000B53 RID: 2899
		public const string amethystClusterQID = "(O)66";

		// Token: 0x04000B54 RID: 2900
		public const string aquamarineQID = "(O)62";

		// Token: 0x04000B55 RID: 2901
		public const string caveCarrotQID = "(O)78";

		// Token: 0x04000B56 RID: 2902
		public const string diamondQID = "(O)72";

		// Token: 0x04000B57 RID: 2903
		public const string emeraldQID = "(O)60";

		// Token: 0x04000B58 RID: 2904
		public const string prismaticShardQID = "(O)74";

		// Token: 0x04000B59 RID: 2905
		public const string rubyQID = "(O)64";

		// Token: 0x04000B5A RID: 2906
		public const string sapphireQID = "(O)70";

		// Token: 0x04000B5B RID: 2907
		public const string stardropQID = "(O)434";

		// Token: 0x04000B5C RID: 2908
		public const string topazQID = "(O)68";

		// Token: 0x04000B5D RID: 2909
		public const int inedible = -300;

		// Token: 0x04000B5E RID: 2910
		public const int GreensCategory = -81;

		// Token: 0x04000B5F RID: 2911
		public const int GemCategory = -2;

		// Token: 0x04000B60 RID: 2912
		public const int VegetableCategory = -75;

		// Token: 0x04000B61 RID: 2913
		public const int FishCategory = -4;

		// Token: 0x04000B62 RID: 2914
		public const int EggCategory = -5;

		// Token: 0x04000B63 RID: 2915
		public const int MilkCategory = -6;

		// Token: 0x04000B64 RID: 2916
		public const int CookingCategory = -7;

		// Token: 0x04000B65 RID: 2917
		public const int CraftingCategory = -8;

		// Token: 0x04000B66 RID: 2918
		public const int BigCraftableCategory = -9;

		// Token: 0x04000B67 RID: 2919
		public const int FruitsCategory = -79;

		// Token: 0x04000B68 RID: 2920
		public const int SeedsCategory = -74;

		// Token: 0x04000B69 RID: 2921
		public const int mineralsCategory = -12;

		// Token: 0x04000B6A RID: 2922
		public const int flowersCategory = -80;

		// Token: 0x04000B6B RID: 2923
		public const int meatCategory = -14;

		// Token: 0x04000B6C RID: 2924
		public const int metalResources = -15;

		// Token: 0x04000B6D RID: 2925
		public const int buildingResources = -16;

		// Token: 0x04000B6E RID: 2926
		public const int sellAtPierres = -17;

		// Token: 0x04000B6F RID: 2927
		public const int sellAtPierresAndMarnies = -18;

		// Token: 0x04000B70 RID: 2928
		public const int fertilizerCategory = -19;

		// Token: 0x04000B71 RID: 2929
		public const int junkCategory = -20;

		// Token: 0x04000B72 RID: 2930
		public const int baitCategory = -21;

		// Token: 0x04000B73 RID: 2931
		public const int tackleCategory = -22;

		// Token: 0x04000B74 RID: 2932
		public const int sellAtFishShopCategory = -23;

		// Token: 0x04000B75 RID: 2933
		public const int furnitureCategory = -24;

		// Token: 0x04000B76 RID: 2934
		public const int ingredientsCategory = -25;

		// Token: 0x04000B77 RID: 2935
		public const int artisanGoodsCategory = -26;

		// Token: 0x04000B78 RID: 2936
		public const int syrupCategory = -27;

		// Token: 0x04000B79 RID: 2937
		public const int monsterLootCategory = -28;

		// Token: 0x04000B7A RID: 2938
		public const int equipmentCategory = -29;

		// Token: 0x04000B7B RID: 2939
		public const int clothingCategorySortValue = -94;

		// Token: 0x04000B7C RID: 2940
		public const int hatCategory = -95;

		// Token: 0x04000B7D RID: 2941
		public const int ringCategory = -96;

		// Token: 0x04000B7E RID: 2942
		public const int weaponCategory = -98;

		// Token: 0x04000B7F RID: 2943
		public const int bootsCategory = -97;

		// Token: 0x04000B80 RID: 2944
		public const int toolCategory = -99;

		// Token: 0x04000B81 RID: 2945
		public const int clothingCategory = -100;

		// Token: 0x04000B82 RID: 2946
		public const int trinketCategory = -101;

		// Token: 0x04000B83 RID: 2947
		public const int booksCategory = -102;

		// Token: 0x04000B84 RID: 2948
		public const int skillBooksCategory = -103;

		// Token: 0x04000B85 RID: 2949
		public const int litterCategory = -999;

		// Token: 0x04000B86 RID: 2950
		public const int WildHorseradishIndex = 16;

		// Token: 0x04000B87 RID: 2951
		public const int LeekIndex = 20;

		// Token: 0x04000B88 RID: 2952
		public const int DandelionIndex = 22;

		// Token: 0x04000B89 RID: 2953
		public const int HandCursorIndex = 26;

		// Token: 0x04000B8A RID: 2954
		public const int WaterAnimationIndex = 28;

		// Token: 0x04000B8B RID: 2955
		public const int LumberIndex = 30;

		// Token: 0x04000B8C RID: 2956
		public const int mineStoneGrey1Index = 32;

		// Token: 0x04000B8D RID: 2957
		public const int mineStoneBlue1Index = 34;

		// Token: 0x04000B8E RID: 2958
		public const int mineStoneBlue2Index = 36;

		// Token: 0x04000B8F RID: 2959
		public const int mineStoneGrey2Index = 38;

		// Token: 0x04000B90 RID: 2960
		public const int mineStoneBrown1Index = 40;

		// Token: 0x04000B91 RID: 2961
		public const int mineStoneBrown2Index = 42;

		// Token: 0x04000B92 RID: 2962
		public const int mineStonePurpleIndex = 44;

		// Token: 0x04000B93 RID: 2963
		public const int mineStoneMysticIndex = 46;

		// Token: 0x04000B94 RID: 2964
		public const int mineStoneSnow1 = 48;

		// Token: 0x04000B95 RID: 2965
		public const int mineStoneSnow3 = 52;

		// Token: 0x04000B96 RID: 2966
		public const int mineStoneRed1Index = 56;

		// Token: 0x04000B97 RID: 2967
		public const int mineStoneRed2Index = 58;

		// Token: 0x04000B98 RID: 2968
		public const int emeraldIndex = 60;

		// Token: 0x04000B99 RID: 2969
		public const int aquamarineIndex = 62;

		// Token: 0x04000B9A RID: 2970
		public const int rubyIndex = 64;

		// Token: 0x04000B9B RID: 2971
		public const int amethystClusterIndex = 66;

		// Token: 0x04000B9C RID: 2972
		public const int topazIndex = 68;

		// Token: 0x04000B9D RID: 2973
		public const int sapphireIndex = 70;

		// Token: 0x04000B9E RID: 2974
		public const int diamondIndex = 72;

		// Token: 0x04000B9F RID: 2975
		public const int prismaticShardIndex = 74;

		// Token: 0x04000BA0 RID: 2976
		public const int stardrop = 434;

		// Token: 0x04000BA1 RID: 2977
		public const string WildHoneyPreservedId = "-1";

		// Token: 0x04000BA2 RID: 2978
		public const int lowQuality = 0;

		// Token: 0x04000BA3 RID: 2979
		public const int medQuality = 1;

		// Token: 0x04000BA4 RID: 2980
		public const int highQuality = 2;

		// Token: 0x04000BA5 RID: 2981
		public const int bestQuality = 4;

		// Token: 0x04000BA6 RID: 2982
		public const int fragility_Removable = 0;

		// Token: 0x04000BA7 RID: 2983
		public const int fragility_Delicate = 1;

		// Token: 0x04000BA8 RID: 2984
		public const int fragility_Indestructable = 2;

		// Token: 0x04000BA9 RID: 2985
		public const int spriteSheetTileSize = 16;

		// Token: 0x04000BAA RID: 2986
		public const float wobbleAmountWhenWorking = 10f;

		// Token: 0x04000BAB RID: 2987
		public const string RecipeNameSuffix = " Recipe";

		// Token: 0x04000BAC RID: 2988
		[XmlElement("tileLocation")]
		public readonly NetVector2 tileLocation = new NetVector2();

		// Token: 0x04000BAD RID: 2989
		[XmlElement("owner")]
		public readonly NetLong owner = new NetLong();

		// Token: 0x04000BAE RID: 2990
		[XmlElement("type")]
		public readonly NetString type = new NetString();

		// Token: 0x04000BAF RID: 2991
		[XmlElement("canBeSetDown")]
		public readonly NetBool canBeSetDown = new NetBool(false);

		// Token: 0x04000BB0 RID: 2992
		[XmlElement("canBeGrabbed")]
		public readonly NetBool canBeGrabbed = new NetBool(true);

		// Token: 0x04000BB1 RID: 2993
		[XmlElement("isSpawnedObject")]
		public readonly NetBool isSpawnedObject = new NetBool(false);

		// Token: 0x04000BB2 RID: 2994
		[XmlElement("questItem")]
		public readonly NetBool questItem = new NetBool(false);

		// Token: 0x04000BB3 RID: 2995
		[XmlElement("questId")]
		public readonly NetString questId = new NetString();

		// Token: 0x04000BB4 RID: 2996
		[XmlElement("isOn")]
		public readonly NetBool isOn = new NetBool(true);

		// Token: 0x04000BB5 RID: 2997
		[XmlElement("fragility")]
		public readonly NetInt fragility = new NetInt(0);

		// Token: 0x04000BB6 RID: 2998
		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		// Token: 0x04000BB7 RID: 2999
		[XmlElement("edibility")]
		public readonly NetInt edibility = new NetInt(-300);

		// Token: 0x04000BB8 RID: 3000
		[XmlElement("bigCraftable")]
		public readonly NetBool bigCraftable = new NetBool();

		// Token: 0x04000BB9 RID: 3001
		[XmlElement("setOutdoors")]
		public readonly NetBool setOutdoors = new NetBool();

		// Token: 0x04000BBA RID: 3002
		[XmlElement("setIndoors")]
		public readonly NetBool setIndoors = new NetBool();

		// Token: 0x04000BBB RID: 3003
		[XmlElement("readyForHarvest")]
		public readonly NetBool readyForHarvest = new NetBool();

		// Token: 0x04000BBC RID: 3004
		[XmlElement("showNextIndex")]
		public readonly NetBool showNextIndex = new NetBool();

		// Token: 0x04000BBD RID: 3005
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		// Token: 0x04000BBE RID: 3006
		[XmlElement("isLamp")]
		public readonly NetBool isLamp = new NetBool();

		// Token: 0x04000BBF RID: 3007
		[XmlElement("heldObject")]
		public readonly NetRef<Object> heldObject = new NetRef<Object>();

		// Token: 0x04000BC0 RID: 3008
		[XmlElement("lastOutputRuleId")]
		public readonly NetString lastOutputRuleId = new NetString();

		// Token: 0x04000BC1 RID: 3009
		[XmlElement("lastInputItem")]
		public readonly NetRef<Item> lastInputItem = new NetRef<Item>();

		// Token: 0x04000BC2 RID: 3010
		[XmlElement("minutesUntilReady")]
		public readonly NetIntDelta minutesUntilReady = new NetIntDelta();

		// Token: 0x04000BC3 RID: 3011
		[XmlElement("boundingBox")]
		public readonly NetRectangle boundingBox = new NetRectangle();

		// Token: 0x04000BC4 RID: 3012
		public Vector2 scale;

		// Token: 0x04000BC5 RID: 3013
		[XmlElement("uses")]
		public readonly NetInt uses = new NetInt();

		// Token: 0x04000BC6 RID: 3014
		[XmlIgnore]
		private readonly NetRef<LightSource> netLightSource = new NetRef<LightSource>();

		// Token: 0x04000BC7 RID: 3015
		[XmlIgnore]
		public readonly NetString netDisplayNameFormat = new NetString();

		// Token: 0x04000BC8 RID: 3016
		[XmlIgnore]
		public bool isTemporarilyInvisible;

		// Token: 0x04000BC9 RID: 3017
		[XmlIgnore]
		protected NetBool _destroyOvernight = new NetBool(false);

		// Token: 0x04000BCA RID: 3018
		[XmlIgnore]
		public bool shouldShowSign;

		// Token: 0x04000BCB RID: 3019
		[XmlIgnore]
		public Func<Buff> customBuff;

		// Token: 0x04000BCC RID: 3020
		[XmlElement("signText")]
		public readonly NetString signText = new NetString();

		// Token: 0x04000BCD RID: 3021
		protected MachineEffects _machineAnimation;

		// Token: 0x04000BCE RID: 3022
		protected bool _machineAnimationLoop;

		// Token: 0x04000BCF RID: 3023
		protected int _machineAnimationIndex;

		// Token: 0x04000BD0 RID: 3024
		protected int _machineAnimationFrame = -1;

		// Token: 0x04000BD1 RID: 3025
		protected int _machineAnimationInterval;

		// Token: 0x04000BD2 RID: 3026
		[XmlElement("orderData")]
		public readonly NetString orderData = new NetString();

		// Token: 0x04000BD3 RID: 3027
		[XmlIgnore]
		public static IInventory autoLoadFrom;

		// Token: 0x04000BD4 RID: 3028
		[XmlIgnore]
		public int shakeTimer;

		// Token: 0x04000BD5 RID: 3029
		[XmlIgnore]
		public int lastNoteBlockSoundTime;

		// Token: 0x04000BD6 RID: 3030
		[XmlIgnore]
		public ICue internalSound;

		// Token: 0x04000BD8 RID: 3032
		[XmlElement("preserve")]
		public readonly NetNullableEnum<Object.PreserveType> preserve = new NetNullableEnum<Object.PreserveType>();

		// Token: 0x04000BD9 RID: 3033
		[XmlElement("preservedParentSheetIndex")]
		public readonly NetString preservedParentSheetIndex = new NetString();

		// Token: 0x04000BDA RID: 3034
		[XmlElement("honeyType")]
		public string obsolete_honeyType;

		// Token: 0x04000BDB RID: 3035
		[XmlIgnore]
		public string displayName;

		// Token: 0x04000BDD RID: 3037
		protected bool _hasHeldObject;

		// Token: 0x04000BDE RID: 3038
		protected bool _hasLightSource;

		// Token: 0x04000BDF RID: 3039
		public static int CurrentParsedItemCount;

		// Token: 0x04000BE0 RID: 3040
		protected int health = 10;

		// Token: 0x04000BE1 RID: 3041
		[XmlIgnore]
		public bool hovering;

		// Token: 0x020004C1 RID: 1217
		public enum PreserveType
		{
			// Token: 0x0400294F RID: 10575
			Wine,
			// Token: 0x04002950 RID: 10576
			Jelly,
			// Token: 0x04002951 RID: 10577
			Pickle,
			// Token: 0x04002952 RID: 10578
			Juice,
			// Token: 0x04002953 RID: 10579
			Roe,
			// Token: 0x04002954 RID: 10580
			AgedRoe,
			// Token: 0x04002955 RID: 10581
			Honey,
			// Token: 0x04002956 RID: 10582
			Bait,
			// Token: 0x04002957 RID: 10583
			DriedFruit,
			// Token: 0x04002958 RID: 10584
			DriedMushroom,
			// Token: 0x04002959 RID: 10585
			SmokedFish
		}
	}
}
