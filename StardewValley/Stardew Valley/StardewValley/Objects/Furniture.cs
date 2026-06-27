using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace StardewValley.Objects
{
	// Token: 0x020001AC RID: 428
	[XmlInclude(typeof(BedFurniture))]
	[XmlInclude(typeof(RandomizedPlantFurniture))]
	[XmlInclude(typeof(StorageFurniture))]
	[XmlInclude(typeof(TV))]
	public class Furniture : Object, ISittable
	{
		// Token: 0x06001E64 RID: 7780 RVA: 0x0015E7F8 File Offset: 0x0015C9F8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.furniture_type, "furniture_type").AddField(this.rotations, "rotations").AddField(this.currentRotation, "currentRotation").AddField(this.sourceIndexOffset, "sourceIndexOffset").AddField(this.drawPosition, "drawPosition").AddField(this.sourceRect, "sourceRect").AddField(this.defaultSourceRect, "defaultSourceRect").AddField(this.defaultBoundingBox, "defaultBoundingBox").AddField(this.drawHeldObjectLow, "drawHeldObjectLow").AddField(this.sittingFarmers, "sittingFarmers");
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06001E65 RID: 7781 RVA: 0x0015E8B4 File Offset: 0x0015CAB4
		[XmlIgnore]
		public int placementRestriction
		{
			get
			{
				if (this._placementRestriction < 0)
				{
					bool use_default = true;
					string[] data = this.getData();
					if (data != null && data.Length > 6 && int.TryParse(data[6], out this._placementRestriction) && this._placementRestriction >= 0)
					{
						use_default = false;
					}
					if (use_default)
					{
						if (base.name.Contains("TV"))
						{
							this._placementRestriction = 0;
						}
						else if (this.IsTable() || this.furniture_type.Value == 1 || this.furniture_type.Value == 0 || this.furniture_type.Value == 8 || this.furniture_type.Value == 16)
						{
							this._placementRestriction = 2;
						}
						else
						{
							this._placementRestriction = 0;
						}
					}
				}
				return this._placementRestriction;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06001E66 RID: 7782 RVA: 0x0015E96E File Offset: 0x0015CB6E
		[XmlIgnore]
		public string description
		{
			get
			{
				if (this._description == null)
				{
					this._description = this.loadDescription();
				}
				return this._description;
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06001E67 RID: 7783 RVA: 0x0015E98A File Offset: 0x0015CB8A
		public override string TypeDefinitionId { get; } = "(F)";

		// Token: 0x06001E68 RID: 7784 RVA: 0x0015E994 File Offset: 0x0015CB94
		public Furniture()
		{
			this.updateDrawPosition();
			this.isOn.Value = false;
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x0015EA40 File Offset: 0x0015CC40
		public Furniture(string itemId, Vector2 tile, int initialRotations) : this(itemId, tile)
		{
			for (int i = 0; i < initialRotations; i++)
			{
				this.rotate();
			}
			this.isOn.Value = false;
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x0015EA74 File Offset: 0x0015CC74
		public virtual void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			if (this.IntersectsForCollision(Game1.player.GetBoundingBox()))
			{
				Game1.player.TemporaryPassableTiles.Add(this.GetBoundingBoxAt((int)tilePos.X, (int)tilePos.Y));
			}
			if (this.furniture_type.Value == 13)
			{
				if (loc != null && loc.IsRainingHere())
				{
					this.sourceRect.Value = this.defaultSourceRect.Value;
					this.sourceIndexOffset.Value = 1;
				}
				else
				{
					this.sourceRect.Value = this.defaultSourceRect.Value;
					this.sourceIndexOffset.Value = 0;
					this.AddLightGlow();
				}
			}
			this.minutesElapsed(1);
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x0015EB24 File Offset: 0x0015CD24
		public void OnRemoved(GameLocation loc, Vector2 tilePos)
		{
			this.RemoveLightGlow();
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x0015EB2C File Offset: 0x0015CD2C
		public override bool IsHeldOverHead()
		{
			return false;
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x0015EB30 File Offset: 0x0015CD30
		public virtual bool IsTable()
		{
			int furnitureType = this.furniture_type.Value;
			return furnitureType == 11 || furnitureType == 5;
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x0015EB54 File Offset: 0x0015CD54
		public static Rectangle GetDefaultSourceRect(string itemId, Texture2D texture = null)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(F)" + itemId);
			string[] rawData = Furniture.getData(itemId);
			if (rawData == null)
			{
				return itemData.GetSourceRect(0, null);
			}
			if (rawData[2].Equals("-1"))
			{
				return Furniture.getDefaultSourceRectForType(itemData, Furniture.getTypeNumberFromName(rawData[1]), texture);
			}
			string[] array = ArgUtility.SplitBySpace(rawData[2]);
			int width = Convert.ToInt32(array[0]);
			int height = Convert.ToInt32(array[1]);
			return Furniture.getDefaultSourceRect(itemData, width, height, texture);
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x0015EBCF File Offset: 0x0015CDCF
		public Furniture SetPlacement(int x, int y, int rotations = 0)
		{
			return this.SetPlacement(new Vector2((float)x, (float)y), rotations);
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x0015EBE1 File Offset: 0x0015CDE1
		public Furniture SetPlacement(Point tile, int rotations = 0)
		{
			return this.SetPlacement(Utility.PointToVector2(tile), rotations);
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x0015EBF0 File Offset: 0x0015CDF0
		public Furniture SetPlacement(Vector2 tile, int rotations = 0)
		{
			this.InitializeAtTile(tile);
			for (int i = 0; i < rotations; i++)
			{
				this.rotate();
			}
			return this;
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x0015EC18 File Offset: 0x0015CE18
		public Furniture SetHeldObject(Object obj)
		{
			this.heldObject.Value = obj;
			if (obj != null)
			{
				Furniture furniture = obj as Furniture;
				if (furniture != null)
				{
					furniture.InitializeAtTile(this.TileLocation);
				}
				else
				{
					obj.TileLocation = this.TileLocation;
				}
			}
			return this;
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x0015EC5C File Offset: 0x0015CE5C
		public void InitializeAtTile(Vector2 tile)
		{
			Texture2D texture = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetTexture();
			string[] data = this.getData();
			if (data != null)
			{
				this.furniture_type.Value = Furniture.getTypeNumberFromName(data[1]);
				this.defaultSourceRect.Value = new Rectangle(base.ParentSheetIndex * 16 % texture.Width, base.ParentSheetIndex * 16 / texture.Width * 16, 1, 1);
				this.drawHeldObjectLow.Value = this.Name.ContainsIgnoreCase("tea");
				this.sourceRect.Value = Furniture.GetDefaultSourceRect(base.ItemId, null);
				this.defaultSourceRect.Value = this.sourceRect.Value;
				this.rotations.Value = Convert.ToInt32(data[4]);
				this.price.Value = Convert.ToInt32(data[5]);
			}
			else
			{
				this.defaultSourceRect.Value = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetSourceRect(0, null);
			}
			if (tile != this.TileLocation)
			{
				this.TileLocation = tile;
				return;
			}
			this.RecalculateBoundingBox(data);
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x0015ED84 File Offset: 0x0015CF84
		public Furniture(string itemId, Vector2 tile)
		{
			this.isOn.Value = false;
			base.ItemId = itemId;
			base.ResetParentSheetIndex();
			base.name = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
			this.InitializeAtTile(tile);
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x0015EE54 File Offset: 0x0015D054
		public override void RecalculateBoundingBox()
		{
			this.RecalculateBoundingBox(this.getData());
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x0015EE64 File Offset: 0x0015D064
		private void RecalculateBoundingBox(string[] data)
		{
			string rawSize = ArgUtility.Get(data, 3, null, true);
			Rectangle box;
			if (rawSize != null)
			{
				if (!(rawSize == "-1"))
				{
					string[] sizeParts = ArgUtility.SplitBySpace(data[3]);
					box = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, Convert.ToInt32(sizeParts[0]) * 64, Convert.ToInt32(sizeParts[1]) * 64);
				}
				else
				{
					box = this.getDefaultBoundingBoxForType(this.furniture_type.Value);
				}
			}
			else
			{
				box = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64);
			}
			this.defaultBoundingBox.Value = box;
			this.boundingBox.Value = box;
			this.updateRotation();
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x0015EF2E File Offset: 0x0015D12E
		protected string[] getData()
		{
			return Furniture.getData(base.ItemId);
		}

		// Token: 0x06001E78 RID: 7800 RVA: 0x0015EF3C File Offset: 0x0015D13C
		protected static string[] getData(string itemId)
		{
			string rawData;
			if (!DataLoader.Furniture(Game1.content).TryGetValue(itemId, out rawData))
			{
				return null;
			}
			return rawData.Split('/', StringSplitOptions.None);
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x0015EF68 File Offset: 0x0015D168
		protected override string loadDisplayName()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
		}

		// Token: 0x06001E7A RID: 7802 RVA: 0x0015EF7C File Offset: 0x0015D17C
		protected virtual string loadDescription()
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			if (itemData.IsErrorItem)
			{
				return itemData.Description;
			}
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length != 7)
				{
					switch (length)
					{
					case 16:
						if (qualifiedItemId == "(F)JojaCatalogue")
						{
							return Game1.content.LoadString("Strings\\1_6_Strings:JojaCatalogueDescription");
						}
						break;
					case 17:
					{
						char c = qualifiedItemId[3];
						if (c != 'R')
						{
							if (c == 'T')
							{
								if (qualifiedItemId == "(F)TrashCatalogue")
								{
									return Game1.content.LoadString("Strings\\1_6_Strings:TrashCatalogueDescription");
								}
							}
						}
						else if (qualifiedItemId == "(F)RetroCatalogue")
						{
							return Game1.content.LoadString("Strings\\1_6_Strings:RetroCatalogueDescription");
						}
						break;
					}
					case 18:
					{
						char c = qualifiedItemId[3];
						if (c != 'J')
						{
							if (c == 'W')
							{
								if (qualifiedItemId == "(F)WizardCatalogue")
								{
									return Game1.content.LoadString("Strings\\1_6_Strings:WizardCatalogueDescription");
								}
							}
						}
						else if (qualifiedItemId == "(F)JunimoCatalogue")
						{
							return Game1.content.LoadString("Strings\\1_6_Strings:JunimoCatalogueDescription");
						}
						break;
					}
					}
				}
				else
				{
					char c = qualifiedItemId[4];
					if (c != '2')
					{
						if (c == '3')
						{
							if (qualifiedItemId == "(F)1308")
							{
								return Game1.parseText(Game1.content.LoadString("Strings\\Objects:CatalogueDescription"), Game1.smallFont, 320);
							}
						}
					}
					else if (qualifiedItemId == "(F)1226")
					{
						return Game1.parseText(Game1.content.LoadString("Strings\\Objects:FurnitureCatalogueDescription"), Game1.smallFont, 320);
					}
				}
			}
			switch (this.placementRestriction)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_NotOutdoors");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors_Description");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration_Description");
			default:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12623");
			}
		}

		// Token: 0x06001E7B RID: 7803 RVA: 0x0015F181 File Offset: 0x0015D381
		public override string getDescription()
		{
			return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x06001E7C RID: 7804 RVA: 0x0015F199 File Offset: 0x0015D399
		public override Color getCategoryColor()
		{
			return new Color(100, 25, 190);
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x0015F1A9 File Offset: 0x0015D3A9
		public override bool performDropDownAction(Farmer who)
		{
			this.actionOnPlayerEntryOrPlacement(this.Location, true);
			return false;
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x0015F1B9 File Offset: 0x0015D3B9
		public override void hoverAction()
		{
			base.hoverAction();
			if (!Game1.player.isInventoryFull())
			{
				Game1.mouseCursor = Game1.cursor_grab;
			}
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x0015F1D8 File Offset: 0x0015D3D8
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (justCheckingForActivity)
			{
				return true;
			}
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
							char c = qualifiedItemId[6];
							if (c != '2')
							{
								switch (c)
								{
								case '6':
									if (qualifiedItemId == "(F)1226")
									{
										Utility.TryOpenShopMenu("Furniture Catalogue", location, null, null, false, true, null);
										return true;
									}
									break;
								case '8':
									if (qualifiedItemId == "(F)1308")
									{
										Utility.TryOpenShopMenu("Catalogue", location, null, null, false, true, null);
										return true;
									}
									break;
								case '9':
									if (qualifiedItemId == "(F)1309")
									{
										Game1.playSound("openBox", null);
										this.shakeTimer = 500;
										if (Game1.getMusicTrackName(MusicContext.Default).Equals("sam_acoustic1"))
										{
											Game1.changeMusicTrack("none", true, MusicContext.Default);
										}
										else
										{
											Game1.changeMusicTrack("sam_acoustic1", false, MusicContext.Default);
										}
										return true;
									}
									break;
								}
							}
							else if (qualifiedItemId == "(F)1402")
							{
								Game1.activeClickableMenu = new Billboard(false);
								return true;
							}
						}
					}
					else
					{
						char c = qualifiedItemId[4];
						if (c != '0')
						{
							if (c == '1')
							{
								if (!(qualifiedItemId == "(F)714") && !(qualifiedItemId == "(F)719"))
								{
								}
							}
						}
						else if (!(qualifiedItemId == "(F)704") && !(qualifiedItemId == "(F)709"))
						{
						}
					}
				}
				else if (length != 11)
				{
					switch (length)
					{
					case 16:
						if (qualifiedItemId == "(F)JojaCatalogue")
						{
							if (!Game1.player.mailReceived.Contains("JojaThriveTerms"))
							{
								Game1.player.mailReceived.Add("JojaThriveTerms");
								Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\1_6_Strings:JojaCatalogueDescriptionTerms"))
								{
									whichBG = 4
								};
							}
							else
							{
								Utility.TryOpenShopMenu("JojaFurnitureCatalogue", location, null, null, false, true, null);
							}
							return true;
						}
						break;
					case 17:
					{
						char c = qualifiedItemId[3];
						if (c != 'R')
						{
							if (c == 'T')
							{
								if (qualifiedItemId == "(F)TrashCatalogue")
								{
									Utility.TryOpenShopMenu("TrashFurnitureCatalogue", location, null, null, false, true, null);
								}
							}
						}
						else if (qualifiedItemId == "(F)RetroCatalogue")
						{
							Utility.TryOpenShopMenu("RetroFurnitureCatalogue", location, null, null, false, true, null);
						}
						break;
					}
					case 18:
					{
						char c = qualifiedItemId[3];
						if (c != 'J')
						{
							if (c == 'W')
							{
								if (qualifiedItemId == "(F)WizardCatalogue")
								{
									if (!Game1.player.mailReceived.Contains("WizardCatalogue"))
									{
										Game1.player.mailReceived.Add("WizardCatalogue");
										Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\1_6_Strings:WizardCatalogueLetter"))
										{
											whichBG = 2
										};
									}
									else
									{
										Utility.TryOpenShopMenu("WizardFurnitureCatalogue", location, null, null, false, true, null);
									}
									return true;
								}
							}
						}
						else if (qualifiedItemId == "(F)JunimoCatalogue")
						{
							Utility.TryOpenShopMenu("JunimoFurnitureCatalogue", location, null, null, false, true, null);
						}
						break;
					}
					}
				}
				else if (qualifiedItemId == "(F)Cauldron")
				{
					base.IsOn = !base.IsOn;
					base.SpecialVariable = (base.IsOn ? 388859 : 0);
					if (base.IsOn)
					{
						location.playSound("fireball", null, null, SoundContext.Default);
						location.playSound("bubbles", null, null, SoundContext.Default);
						for (int i = 0; i < 13; i++)
						{
							this.addCauldronBubbles(-0.5f - (float)i * 0.2f);
						}
					}
				}
			}
			if (this.furniture_type.Value == 14 || this.furniture_type.Value == 16)
			{
				this.isOn.Value = !this.isOn.Value;
				this.initializeLightSource(this.tileLocation.Value, false);
				this.setFireplace(true, true);
				return true;
			}
			if (this.GetSeatCapacity() > 0)
			{
				who.BeginSitting(this);
				return true;
			}
			return this.clicked(who);
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x0015F6E8 File Offset: 0x0015D8E8
		public virtual void setFireplace(bool playSound = true, bool broadcast = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (this.isOn.Value)
			{
				if (base.lightSource == null)
				{
					this.initializeLightSource(this.tileLocation.Value, false);
				}
				if (base.lightSource != null && this.isOn.Value && !location.hasLightSource(base.lightSource.Id))
				{
					location.sharedLights.AddLight(base.lightSource.Clone());
				}
				if (playSound)
				{
					location.localSound("fireball", null, null, SoundContext.Default);
				}
				AmbientLocationSounds.addSound(new Vector2(this.tileLocation.X, this.tileLocation.Y), 1);
				return;
			}
			if (playSound)
			{
				location.localSound("fireball", null, null, SoundContext.Default);
			}
			base.performRemoveAction();
			AmbientLocationSounds.removeSound(new Vector2(this.tileLocation.X, this.tileLocation.Y));
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x0015F7F2 File Offset: 0x0015D9F2
		public virtual void AttemptRemoval(Action<Furniture> removal_action)
		{
			if (removal_action != null)
			{
				removal_action(this);
			}
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x0015F800 File Offset: 0x0015DA00
		public virtual bool canBeRemoved(Farmer who)
		{
			if (!this.AllowLocalRemoval)
			{
				return false;
			}
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (this.HasSittingFarmers())
			{
				return false;
			}
			if (this.heldObject.Value != null)
			{
				return false;
			}
			Rectangle bounds = base.GetBoundingBox();
			if (this.isPassable())
			{
				for (int x = bounds.Left / 64; x < bounds.Right / 64; x++)
				{
					for (int y = bounds.Top / 64; y < bounds.Bottom / 64; y++)
					{
						Furniture tileFurniture = location.GetFurnitureAt(new Vector2((float)x, (float)y));
						if (tileFurniture != null && tileFurniture != this)
						{
							return false;
						}
						if (location.objects.ContainsKey(new Vector2((float)x, (float)y)))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x0015F8BC File Offset: 0x0015DABC
		public override bool clicked(Farmer who)
		{
			Game1.haltAfterCheck = false;
			if (this.furniture_type.Value == 11 && who.ActiveObject != null && this.heldObject.Value == null)
			{
				return false;
			}
			if (this.heldObject.Value != null)
			{
				Object item = this.heldObject.Value;
				this.heldObject.Value = null;
				if (who.addItemToInventoryBool(item, false))
				{
					item.performRemoveAction();
					Game1.playSound("coin", null);
					return true;
				}
				this.heldObject.Value = item;
			}
			return false;
		}

		// Token: 0x06001E84 RID: 7812 RVA: 0x0015F950 File Offset: 0x0015DB50
		public virtual int GetSeatCapacity()
		{
			if (base.QualifiedItemId.Equals("(F)UprightPiano") || base.QualifiedItemId.Equals("(F)DarkPiano"))
			{
				return 1;
			}
			switch (this.furniture_type.Value)
			{
			case 0:
				return 1;
			case 1:
				return 2;
			case 2:
				return this.defaultBoundingBox.Width / 64 - 1;
			case 3:
				return 1;
			default:
				return 0;
			}
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x0015F9BF File Offset: 0x0015DBBF
		public virtual bool IsSeatHere(GameLocation location)
		{
			return location.furniture.Contains(this);
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x0015F9CD File Offset: 0x0015DBCD
		public virtual bool IsSittingHere(Farmer who)
		{
			return this.sittingFarmers.ContainsKey(who.UniqueMultiplayerID);
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x0015F9E0 File Offset: 0x0015DBE0
		public virtual Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
		{
			int key;
			if (this.sittingFarmers.TryGetValue(who.UniqueMultiplayerID, out key))
			{
				return new Vector2?(this.GetSeatPositions(ignore_offsets)[key]);
			}
			return null;
		}

		// Token: 0x06001E88 RID: 7816 RVA: 0x0015FA1E File Offset: 0x0015DC1E
		public virtual bool HasSittingFarmers()
		{
			return this.sittingFarmers.Length > 0;
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x0015FA2E File Offset: 0x0015DC2E
		public virtual void RemoveSittingFarmer(Farmer farmer)
		{
			this.sittingFarmers.Remove(farmer.UniqueMultiplayerID);
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x0015FA42 File Offset: 0x0015DC42
		public virtual int GetSittingFarmerCount()
		{
			return this.sittingFarmers.Length;
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x0015FA50 File Offset: 0x0015DC50
		public virtual Rectangle GetSeatBounds()
		{
			Rectangle bounds = base.GetBoundingBox();
			return new Rectangle(bounds.X / 64, bounds.Y / 64, bounds.Width / 64, bounds.Height / 64);
		}

		// Token: 0x06001E8C RID: 7820 RVA: 0x0015FA90 File Offset: 0x0015DC90
		public virtual int GetSittingDirection()
		{
			if (this.Name.Contains("Stool"))
			{
				return Game1.player.FacingDirection;
			}
			if (base.QualifiedItemId.Equals("(F)UprightPiano") || base.QualifiedItemId.Equals("(F)DarkPiano"))
			{
				return 0;
			}
			switch (this.currentRotation.Value)
			{
			case 0:
				return 2;
			case 1:
				return 1;
			case 2:
				return 0;
			case 3:
				return 3;
			default:
				return 2;
			}
		}

		// Token: 0x06001E8D RID: 7821 RVA: 0x0015FB10 File Offset: 0x0015DD10
		public virtual Vector2? AddSittingFarmer(Farmer who)
		{
			List<Vector2> seat_positions = this.GetSeatPositions(false);
			int seat_index = -1;
			Vector2? sit_position = null;
			float distance = 96f;
			Vector2 playerPixel = who.getStandingPosition();
			for (int i = 0; i < seat_positions.Count; i++)
			{
				if (!this.sittingFarmers.Values.Contains(i))
				{
					float curr_distance = ((seat_positions[i] + new Vector2(0.5f, 0.5f)) * 64f - playerPixel).Length();
					if (curr_distance < distance)
					{
						distance = curr_distance;
						sit_position = new Vector2?(seat_positions[i]);
						seat_index = i;
					}
				}
			}
			if (sit_position != null)
			{
				this.sittingFarmers[who.UniqueMultiplayerID] = seat_index;
			}
			return sit_position;
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x0015FBDC File Offset: 0x0015DDDC
		public virtual List<Vector2> GetSeatPositions(bool ignore_offsets = false)
		{
			List<Vector2> seat_positions = new List<Vector2>();
			if (base.QualifiedItemId.Equals("(F)UprightPiano") || base.QualifiedItemId.Equals("(F)DarkPiano"))
			{
				seat_positions.Add(this.TileLocation + new Vector2(1.5f, 0f));
			}
			switch (this.furniture_type.Value)
			{
			case 0:
				seat_positions.Add(this.TileLocation);
				break;
			case 1:
				for (int x = 0; x < this.getTilesWide(); x++)
				{
					for (int y = 0; y < this.getTilesHigh(); y++)
					{
						seat_positions.Add(this.TileLocation + new Vector2((float)x, (float)y));
					}
				}
				break;
			case 2:
			{
				int width = this.defaultBoundingBox.Width / 64 - 1;
				switch (this.currentRotation.Value)
				{
				case 0:
				case 2:
					seat_positions.Add(this.TileLocation + new Vector2(0.5f, 0f));
					for (int i = 1; i < width - 1; i++)
					{
						seat_positions.Add(this.TileLocation + new Vector2((float)i + 0.5f, 0f));
					}
					seat_positions.Add(this.TileLocation + new Vector2((float)(width - 1) + 0.5f, 0f));
					break;
				case 1:
					for (int j = 0; j < width; j++)
					{
						seat_positions.Add(this.TileLocation + new Vector2(1f, (float)j));
					}
					break;
				default:
					for (int k = 0; k < width; k++)
					{
						seat_positions.Add(this.TileLocation + new Vector2(0f, (float)k));
					}
					break;
				}
				break;
			}
			case 3:
				if (this.currentRotation.Value == 0 || this.currentRotation.Value == 2)
				{
					seat_positions.Add(this.TileLocation + new Vector2(0.5f, 0f));
				}
				else if (this.currentRotation.Value == 1)
				{
					seat_positions.Add(this.TileLocation + new Vector2(1f, 0f));
				}
				else
				{
					seat_positions.Add(this.TileLocation + new Vector2(0f, 0f));
				}
				break;
			}
			return seat_positions;
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x0015FE5B File Offset: 0x0015E05B
		public bool timeToTurnOnLights()
		{
			return this.Location != null && (this.Location.IsRainingHere() || Game1.timeOfDay >= Game1.getTrulyDarkTime(this.Location) - 100);
		}

		// Token: 0x06001E90 RID: 7824 RVA: 0x0015FE90 File Offset: 0x0015E090
		public override void DayUpdate()
		{
			base.DayUpdate();
			this.sittingFarmers.Clear();
			if (this.Location.IsRainingHere())
			{
				this.addLights();
			}
			else if (!this.timeToTurnOnLights() || Game1.newDay)
			{
				this.removeLights();
			}
			else
			{
				this.addLights();
			}
			this.RemoveLightGlow();
			if (Game1.IsMasterGame && Game1.season == Season.Winter && Game1.dayOfMonth == 25 && (this.furniture_type.Value == 11 || this.furniture_type.Value == 5) && this.heldObject.Value != null)
			{
				if (this.heldObject.Value.QualifiedItemId == "(O)223" && !Game1.player.mailReceived.Contains("CookiePresent_year" + Game1.year.ToString()))
				{
					this.heldObject.Value = ItemRegistry.Create<Object>("(O)MysteryBox", 1, 0, false);
					Game1.player.mailReceived.Add("CookiePresent_year" + Game1.year.ToString());
					return;
				}
				if (this.heldObject.Value.Category == -6 && !Game1.player.mailReceived.Contains("MilkPresent_year" + Game1.year.ToString()))
				{
					this.heldObject.Value = ItemRegistry.Create<Object>("(O)MysteryBox", 1, 0, false);
					Game1.player.mailReceived.Add("MilkPresent_year" + Game1.year.ToString());
				}
			}
		}

		// Token: 0x06001E91 RID: 7825 RVA: 0x0016002C File Offset: 0x0015E22C
		public virtual void AddLightGlow()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (this.lightGlowPosition != null)
			{
				return;
			}
			Vector2 light_glow_position = new Vector2((float)(this.boundingBox.X + 32), (float)(this.boundingBox.Y + 64));
			if (!location.lightGlows.Contains(light_glow_position))
			{
				this.lightGlowPosition = new Vector2?(light_glow_position);
				location.lightGlows.Add(light_glow_position);
			}
		}

		// Token: 0x06001E92 RID: 7826 RVA: 0x001600A0 File Offset: 0x0015E2A0
		public virtual void RemoveLightGlow()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (this.lightGlowPosition != null && location.lightGlows.Contains(this.lightGlowPosition.Value))
			{
				location.lightGlows.Remove(this.lightGlowPosition.Value);
			}
			location.lightGlowLayerCache.Clear();
			this.lightGlowPosition = null;
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x0016010C File Offset: 0x0015E30C
		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			this.actionOnPlayerEntryOrPlacement(this.Location, false);
			if (this.Location != null && base.QualifiedItemId.Equals("(F)BirdHouse") && this.Location.isOutdoors.Value && !Game1.isRaining && Game1.timeOfDay < Game1.getStartingToGetDarkTime(this.Location))
			{
				Random r = Utility.CreateDaySaveRandom((double)(this.TileLocation.X * 74797f), (double)(this.TileLocation.Y * 77f), (double)(Game1.timeOfDay * 99));
				int doves = (int)Game1.stats.Get("childrenTurnedToDoves");
				if (r.NextDouble() < 0.06)
				{
					this.Location.instantiateCrittersList();
					int whichBird = (Game1.season == Season.Fall) ? 45 : 25;
					int yOffset = 0;
					if (Game1.random.NextBool() && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
					{
						whichBird = ((Game1.season == Season.Fall) ? 135 : 125);
					}
					if (whichBird == 25 && Game1.random.NextDouble() < 0.05)
					{
						whichBird = 165;
					}
					if (r.NextDouble() < (double)doves * 0.08)
					{
						whichBird = 175;
						yOffset = 12;
					}
					this.Location.critters.Add(new Birdie(this.TileLocation * 64f + new Vector2(32f, (float)(64 + Game1.random.Next(3) * 4 + yOffset)), -160f, whichBird, true));
				}
			}
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x001602B4 File Offset: 0x0015E4B4
		public virtual void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
		{
			if (this.Location == null)
			{
				this.Location = environment;
			}
			this.RemoveLightGlow();
			this.removeLights();
			if (this.furniture_type.Value == 14 || this.furniture_type.Value == 16)
			{
				this.setFireplace(false, false);
			}
			if (this.timeToTurnOnLights())
			{
				this.addLights();
				Furniture furniture = this.heldObject.Value as Furniture;
				if (furniture != null)
				{
					furniture.addLights();
				}
			}
			if (base.QualifiedItemId == "(F)1971" && !dropDown)
			{
				environment.instantiateCrittersList();
				environment.addCritter(new Butterfly(environment, environment.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
				while (Game1.random.NextBool())
				{
					environment.addCritter(new Butterfly(environment, environment.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
				}
			}
		}

		// Token: 0x06001E95 RID: 7829 RVA: 0x00160390 File Offset: 0x0015E590
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			Object dropIn = dropInItem as Object;
			if (dropIn == null)
			{
				return false;
			}
			if (this.IsTable() && this.heldObject.Value == null && !dropIn.bigCraftable.Value && !(dropIn is Wallpaper))
			{
				Furniture furniture = dropIn as Furniture;
				if (furniture == null || (furniture.getTilesWide() == 1 && furniture.getTilesHigh() == 1))
				{
					if (!probe)
					{
						this.heldObject.Value = (Object)dropIn.getOne();
						this.heldObject.Value.Location = this.Location;
						this.heldObject.Value.TileLocation = this.tileLocation.Value;
						this.heldObject.Value.boundingBox.X = this.boundingBox.X;
						this.heldObject.Value.boundingBox.Y = this.boundingBox.Y;
						this.heldObject.Value.performDropDownAction(who);
						location.playSound("woodyStep", null, null, SoundContext.Default);
						if (who != null)
						{
							who.reduceActiveItemByOne();
							if (returnFalseIfItemConsumed)
							{
								return false;
							}
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001E96 RID: 7830 RVA: 0x001604DD File Offset: 0x0015E6DD
		protected virtual string GenerateLightSourceId()
		{
			return base.GenerateLightSourceId(this.tileLocation.Value);
		}

		// Token: 0x06001E97 RID: 7831 RVA: 0x001604F0 File Offset: 0x0015E6F0
		private bool isLampStyleLightSource()
		{
			return this.furniture_type.Value == 7 || this.furniture_type.Value == 17 || base.QualifiedItemId == "(F)1369";
		}

		// Token: 0x06001E98 RID: 7832 RVA: 0x00160524 File Offset: 0x0015E724
		public virtual void addLights()
		{
			GameLocation environment = this.Location;
			if (environment == null)
			{
				return;
			}
			Furniture furniture = this.heldObject.Value as Furniture;
			if (furniture != null)
			{
				this.heldObject.Value.Location = this.Location;
				furniture.addLights();
			}
			if (this.isLampStyleLightSource())
			{
				this.sourceRect.Value = this.defaultSourceRect.Value;
				this.sourceIndexOffset.Value = 1;
				if (base.lightSource == null)
				{
					base.lightSource = new LightSource(this.GenerateLightSourceId(), 4, new Vector2((float)(this.boundingBox.X + 32), (float)(this.boundingBox.Y + ((this.furniture_type.Value == 7) ? -64 : 64))), (this.furniture_type.Value == 7) ? 2f : 1f, (base.QualifiedItemId == "(F)1369") ? (Color.RoyalBlue * 0.7f) : Color.Black, LightSource.LightContext.None, 0L, environment.NameOrUniqueName);
					environment.sharedLights.AddLight(base.lightSource.Clone());
					return;
				}
			}
			else
			{
				if (base.QualifiedItemId == "(F)1440")
				{
					base.lightSource = new LightSource(this.GenerateLightSourceId(), 4, new Vector2((float)(this.boundingBox.X + 96), (float)this.boundingBox.Y - 32f), 1.5f, Color.Black, LightSource.LightContext.None, 0L, environment.NameOrUniqueName);
					environment.sharedLights.AddLight(base.lightSource.Clone());
					return;
				}
				if (this.furniture_type.Value == 13)
				{
					this.sourceRect.Value = this.defaultSourceRect.Value;
					this.sourceIndexOffset.Value = 1;
					this.RemoveLightGlow();
					return;
				}
				if (this is FishTankFurniture && base.lightSource == null)
				{
					string lightSourceId = this.GenerateLightSourceId();
					Vector2 lightPosition = new Vector2(this.tileLocation.X * 64f + 32f + 2f, this.tileLocation.Y * 64f + 12f);
					for (int i = 0; i < this.getTilesWide(); i++)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
						defaultInterpolatedStringHandler.AppendFormatted(lightSourceId);
						defaultInterpolatedStringHandler.AppendLiteral("_tile");
						defaultInterpolatedStringHandler.AppendFormatted<int>(i);
						base.lightSource = new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 8, lightPosition, 2f, Color.Black, LightSource.LightContext.None, 0L, environment.NameOrUniqueName);
						environment.sharedLights.AddLight(base.lightSource.Clone());
						lightPosition.X += 64f;
					}
				}
			}
		}

		// Token: 0x06001E99 RID: 7833 RVA: 0x001607DC File Offset: 0x0015E9DC
		public virtual void removeLights()
		{
			GameLocation environment = this.Location;
			Furniture furniture = this.heldObject.Value as Furniture;
			if (furniture != null)
			{
				furniture.removeLights();
			}
			if (this.isLampStyleLightSource() || base.QualifiedItemId == "(F)1440")
			{
				this.sourceRect.Value = this.defaultSourceRect.Value;
				this.sourceIndexOffset.Value = 0;
				if (environment != null)
				{
					environment.removeLightSource(this.GenerateLightSourceId());
				}
				base.lightSource = null;
				return;
			}
			if (this.furniture_type.Value != 13)
			{
				if (this is FishTankFurniture)
				{
					string lightSourceId = this.GenerateLightSourceId();
					for (int i = 0; i < this.getTilesWide(); i++)
					{
						if (environment != null)
						{
							GameLocation gameLocation = environment;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
							defaultInterpolatedStringHandler.AppendFormatted(lightSourceId);
							defaultInterpolatedStringHandler.AppendLiteral("_tile");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							gameLocation.removeLightSource(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					base.lightSource = null;
				}
				return;
			}
			if (environment != null && environment.IsRainingHere())
			{
				this.sourceRect.Value = this.defaultSourceRect.Value;
				this.sourceIndexOffset.Value = 1;
				return;
			}
			this.sourceRect.Value = this.defaultSourceRect.Value;
			this.sourceIndexOffset.Value = 0;
			this.AddLightGlow();
		}

		// Token: 0x06001E9A RID: 7834 RVA: 0x0016091F File Offset: 0x0015EB1F
		public override bool minutesElapsed(int minutes)
		{
			if (this.Location == null)
			{
				return false;
			}
			if (this.timeToTurnOnLights())
			{
				this.addLights();
			}
			else
			{
				this.removeLights();
			}
			return false;
		}

		// Token: 0x06001E9B RID: 7835 RVA: 0x00160944 File Offset: 0x0015EB44
		public override void performRemoveAction()
		{
			this.removeLights();
			if (this.Location == null)
			{
				return;
			}
			if (this.furniture_type.Value == 14 || this.furniture_type.Value == 16)
			{
				this.isOn.Value = false;
				this.setFireplace(false, false);
			}
			this.RemoveLightGlow();
			base.performRemoveAction();
			if (this.furniture_type.Value == 14 || this.furniture_type.Value == 16)
			{
				base.lightSource = null;
			}
			if (base.QualifiedItemId == "(F)1309" && Game1.getMusicTrackName(MusicContext.Default).Equals("sam_acoustic1"))
			{
				Game1.changeMusicTrack("none", true, MusicContext.Default);
			}
			this.sittingFarmers.Clear();
		}

		// Token: 0x06001E9C RID: 7836 RVA: 0x00160A00 File Offset: 0x0015EC00
		public virtual void rotate()
		{
			if (this.rotations.Value < 2)
			{
				return;
			}
			int rotationAmount = (this.rotations.Value == 4) ? 1 : 2;
			this.currentRotation.Value += rotationAmount;
			this.currentRotation.Value %= 4;
			this.updateRotation();
		}

		// Token: 0x06001E9D RID: 7837 RVA: 0x00160A5C File Offset: 0x0015EC5C
		public virtual void updateRotation()
		{
			this.flipped.Value = false;
			if (this.currentRotation.Value > 0)
			{
				Point specialRotationOffsets;
				switch (this.furniture_type.Value)
				{
				case 2:
					specialRotationOffsets = new Point(-1, 1);
					goto IL_6C;
				case 3:
					specialRotationOffsets = new Point(-1, 1);
					goto IL_6C;
				case 5:
					specialRotationOffsets = new Point(-1, 0);
					goto IL_6C;
				}
				specialRotationOffsets = Point.Zero;
				IL_6C:
				bool differentSizesFor2Rotations = (this.IsTable() || this.furniture_type.Value == 12 || base.QualifiedItemId == "(F)724" || base.QualifiedItemId == "(F)727") && !base.name.Contains("End Table") && !base.name.Contains("EndTable");
				bool sourceRectRotate = this.defaultBoundingBox.Width != this.defaultBoundingBox.Height;
				if (differentSizesFor2Rotations && this.currentRotation.Value == 2)
				{
					this.currentRotation.Value = 1;
				}
				if (sourceRectRotate)
				{
					int oldBoundingBoxHeight = this.boundingBox.Height;
					switch (this.currentRotation.Value)
					{
					case 0:
					case 2:
						this.boundingBox.Height = this.defaultBoundingBox.Height;
						this.boundingBox.Width = this.defaultBoundingBox.Width;
						break;
					case 1:
					case 3:
						this.boundingBox.Height = this.boundingBox.Width + specialRotationOffsets.X * 64;
						this.boundingBox.Width = oldBoundingBoxHeight + specialRotationOffsets.Y * 64;
						break;
					}
				}
				Point specialSpecialSourceRectOffset = (this.furniture_type.Value == 12) ? new Point(1, -1) : Point.Zero;
				if (sourceRectRotate)
				{
					switch (this.currentRotation.Value)
					{
					case 0:
						this.sourceRect.Value = this.defaultSourceRect.Value;
						break;
					case 1:
						this.sourceRect.Value = new Rectangle(this.defaultSourceRect.X + this.defaultSourceRect.Width, this.defaultSourceRect.Y, this.defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, this.defaultSourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
						break;
					case 2:
						this.sourceRect.Value = new Rectangle(this.defaultSourceRect.X + this.defaultSourceRect.Width + this.defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, this.defaultSourceRect.Y, this.defaultSourceRect.Width, this.defaultSourceRect.Height);
						break;
					case 3:
						this.sourceRect.Value = new Rectangle(this.defaultSourceRect.X + this.defaultSourceRect.Width, this.defaultSourceRect.Y, this.defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, this.defaultSourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
						this.flipped.Value = true;
						break;
					}
				}
				else
				{
					this.flipped.Value = (this.currentRotation.Value == 3);
					if (this.rotations.Value == 2)
					{
						this.sourceRect.Value = new Rectangle(this.defaultSourceRect.X + ((this.currentRotation.Value == 2) ? 1 : 0) * this.defaultSourceRect.Width, this.defaultSourceRect.Y, this.defaultSourceRect.Width, this.defaultSourceRect.Height);
					}
					else
					{
						this.sourceRect.Value = new Rectangle(this.defaultSourceRect.X + ((this.currentRotation.Value == 3) ? 1 : this.currentRotation.Value) * this.defaultSourceRect.Width, this.defaultSourceRect.Y, this.defaultSourceRect.Width, this.defaultSourceRect.Height);
					}
				}
				if (differentSizesFor2Rotations && this.currentRotation.Value == 1)
				{
					this.currentRotation.Value = 2;
				}
			}
			else
			{
				this.sourceRect.Value = this.defaultSourceRect.Value;
				this.boundingBox.Value = this.defaultBoundingBox.Value;
			}
			this.updateDrawPosition();
		}

		// Token: 0x06001E9E RID: 7838 RVA: 0x00160F20 File Offset: 0x0015F120
		public virtual bool isGroundFurniture()
		{
			return this.furniture_type.Value != 13 && this.furniture_type.Value != 6 && this.furniture_type.Value != 17 && this.furniture_type.Value != 13;
		}

		// Token: 0x06001E9F RID: 7839 RVA: 0x00160F6D File Offset: 0x0015F16D
		public override bool canBeGivenAsGift()
		{
			return false;
		}

		// Token: 0x06001EA0 RID: 7840 RVA: 0x00160F70 File Offset: 0x0015F170
		public static Furniture GetFurnitureInstance(string itemId, Vector2? position = null)
		{
			if (position == null)
			{
				position = new Vector2?(Vector2.Zero);
			}
			if (itemId == "1466" || itemId == "1468" || itemId == "1680" || itemId == "2326" || itemId == "RetroTV")
			{
				return new TV(itemId, position.Value);
			}
			string furnitureType = ArgUtility.Get(Furniture.getData(itemId), 1, null, true);
			if (furnitureType == "fishtank")
			{
				return new FishTankFurniture(itemId, position.Value);
			}
			if (furnitureType == "dresser")
			{
				return new StorageFurniture(itemId, position.Value);
			}
			if (furnitureType == "randomized_plant")
			{
				return new RandomizedPlantFurniture(itemId, position.Value);
			}
			bool? flag = (furnitureType != null) ? new bool?(furnitureType.StartsWith("bed")) : null;
			if (flag != null && flag.GetValueOrDefault())
			{
				return new BedFurniture(itemId, position.Value);
			}
			return new Furniture(itemId, position.Value);
		}

		// Token: 0x06001EA1 RID: 7841 RVA: 0x00161090 File Offset: 0x0015F290
		public virtual bool IsCloseEnoughToFarmer(Farmer f, int? override_tile_x = null, int? override_tile_y = null)
		{
			Rectangle furniture_rect = new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, this.getTilesWide() * 64, this.getTilesHigh() * 64);
			if (override_tile_x != null)
			{
				furniture_rect.X = override_tile_x.Value * 64;
			}
			if (override_tile_y != null)
			{
				furniture_rect.Y = override_tile_y.Value * 64;
			}
			furniture_rect.Inflate(96, 96);
			return furniture_rect.Contains(Game1.player.StandingPixel);
		}

		// Token: 0x06001EA2 RID: 7842 RVA: 0x00161124 File Offset: 0x0015F324
		public virtual int GetModifiedWallTilePosition(GameLocation l, int tile_x, int tile_y)
		{
			if (this.isGroundFurniture())
			{
				return tile_y;
			}
			if (l != null)
			{
				DecoratableLocation decoratableLocation = l as DecoratableLocation;
				if (decoratableLocation != null)
				{
					int top_y = decoratableLocation.GetWallTopY(tile_x, tile_y);
					if (top_y != -1)
					{
						return top_y;
					}
				}
				return tile_y;
			}
			return tile_y;
		}

		// Token: 0x06001EA3 RID: 7843 RVA: 0x0016115C File Offset: 0x0015F35C
		public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
		{
			if (!l.CanPlaceThisFurnitureHere(this))
			{
				return false;
			}
			if (!this.isGroundFurniture())
			{
				tile.Y = (float)this.GetModifiedWallTilePosition(l, (int)tile.X, (int)tile.Y);
			}
			CollisionMask ignorePassables = CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.TerrainFeatures;
			bool passable = this.isPassable();
			if (passable)
			{
				ignorePassables |= (CollisionMask.Characters | CollisionMask.Farmers);
			}
			collisionMask &= ~(CollisionMask.Furniture | CollisionMask.Objects);
			int tilesWide = this.getTilesWide();
			int tilesHigh = this.getTilesHigh();
			for (int x = 0; x < tilesWide; x++)
			{
				for (int y = 0; y < tilesHigh; y++)
				{
					Vector2 curTile = new Vector2(tile.X + (float)x, tile.Y + (float)y);
					Vector2 curPixel = new Vector2(curTile.X + 0.5f, curTile.Y + 0.5f) * 64f;
					if (!l.isTilePlaceable(curTile, passable))
					{
						return false;
					}
					foreach (Furniture f in l.furniture)
					{
						if (f.furniture_type.Value == 11 && f.GetBoundingBox().Contains((int)curPixel.X, (int)curPixel.Y) && f.heldObject.Value == null && tilesWide == 1 && tilesHigh == 1)
						{
							return true;
						}
						if ((f.furniture_type.Value != 12 || this.furniture_type.Value == 12) && f.GetBoundingBox().Contains((int)curPixel.X, (int)curPixel.Y) && !f.AllowPlacementOnThisTile((int)tile.X + x, (int)tile.Y + y))
						{
							return false;
						}
					}
					Object tileObj;
					if (l.objects.TryGetValue(curTile, out tileObj) && (!tileObj.isPassable() || !this.isPassable()))
					{
						return false;
					}
					if (!this.isGroundFurniture())
					{
						if (l.IsTileOccupiedBy(curTile, collisionMask, ignorePassables, false))
						{
							return false;
						}
					}
					else if (this.furniture_type.Value == 15 && y == 0)
					{
						if (l.IsTileOccupiedBy(curTile, collisionMask, ignorePassables, false))
						{
							return false;
						}
					}
					else
					{
						if (l.IsTileBlockedBy(curTile, collisionMask, ignorePassables, false))
						{
							return false;
						}
						HoeDirt dirt = l.terrainFeatures.GetValueOrDefault(curTile, null) as HoeDirt;
						if (dirt != null && dirt.crop != null)
						{
							return false;
						}
					}
				}
			}
			return this.GetAdditionalFurniturePlacementStatus(l, (int)tile.X * 64, (int)tile.Y * 64, null) == 0;
		}

		// Token: 0x06001EA4 RID: 7844 RVA: 0x001613F4 File Offset: 0x0015F5F4
		public virtual void updateDrawPosition()
		{
			this.drawPosition.Value = new Vector2((float)this.boundingBox.X, (float)(this.boundingBox.Y - (this.sourceRect.Height * 4 - this.boundingBox.Height)));
		}

		// Token: 0x06001EA5 RID: 7845 RVA: 0x00161443 File Offset: 0x0015F643
		public virtual int getTilesWide()
		{
			return this.boundingBox.Width / 64;
		}

		// Token: 0x06001EA6 RID: 7846 RVA: 0x00161453 File Offset: 0x0015F653
		public virtual int getTilesHigh()
		{
			return this.boundingBox.Height / 64;
		}

		// Token: 0x06001EA7 RID: 7847 RVA: 0x00161464 File Offset: 0x0015F664
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			if (!this.isGroundFurniture())
			{
				y = this.GetModifiedWallTilePosition(location, x / 64, y / 64) * 64;
			}
			if (this.GetAdditionalFurniturePlacementStatus(location, x, y, who) != 0)
			{
				return false;
			}
			Vector2 tile = new Vector2((float)(x / 64), (float)(y / 64));
			if (this.TileLocation != tile)
			{
				this.TileLocation = tile;
			}
			else
			{
				this.RecalculateBoundingBox();
			}
			foreach (Furniture f in location.furniture)
			{
				if (f.furniture_type.Value == 11 && f.heldObject.Value == null && f.GetBoundingBox().Intersects(this.boundingBox.Value))
				{
					f.performObjectDropInAction(this, false, who ?? Game1.player, false);
					return true;
				}
			}
			return base.placementAction(location, x, y, who);
		}

		// Token: 0x06001EA8 RID: 7848 RVA: 0x00161568 File Offset: 0x0015F768
		public virtual int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
		{
			if (location.CanPlaceThisFurnitureHere(this))
			{
				Point anchor = new Point(x / 64, y / 64);
				this.tileLocation.Value = new Vector2((float)anchor.X, (float)anchor.Y);
				bool paintingAtRightPlace = false;
				if (this.furniture_type.Value == 6 || this.furniture_type.Value == 17 || this.furniture_type.Value == 13 || base.QualifiedItemId == "(F)1293")
				{
					int offset = (base.QualifiedItemId == "(F)1293") ? 3 : 0;
					bool foundWall = false;
					DecoratableLocation decoratable_location = location as DecoratableLocation;
					if (decoratable_location != null)
					{
						if ((this.furniture_type.Value == 6 || this.furniture_type.Value == 17 || this.furniture_type.Value == 13 || offset != 0) && decoratable_location.isTileOnWall(anchor.X, anchor.Y - offset) && decoratable_location.GetWallTopY(anchor.X, anchor.Y - offset) + offset == anchor.Y)
						{
							foundWall = true;
						}
						else if (!this.isGroundFurniture() && decoratable_location.isTileOnWall(anchor.X, anchor.Y - 1) && decoratable_location.GetWallTopY(anchor.X, anchor.Y) + 1 == anchor.Y)
						{
							foundWall = true;
						}
					}
					if (!foundWall)
					{
						return 1;
					}
					paintingAtRightPlace = true;
				}
				int tiles_high_to_check = this.getTilesHigh();
				if (this.furniture_type.Value == 6 && tiles_high_to_check > 2)
				{
					tiles_high_to_check = 2;
				}
				for (int furnitureX = anchor.X; furnitureX < anchor.X + this.getTilesWide(); furnitureX++)
				{
					int furnitureY = anchor.Y;
					while (furnitureY < anchor.Y + tiles_high_to_check)
					{
						if (location.doesTileHaveProperty(furnitureX, furnitureY, "NoFurniture", "Back", false) != null)
						{
							return 2;
						}
						if (paintingAtRightPlace)
						{
							goto IL_1D4;
						}
						DecoratableLocation decoratableLocation = location as DecoratableLocation;
						if (decoratableLocation == null || !decoratableLocation.isTileOnWall(furnitureX, furnitureY))
						{
							goto IL_1D4;
						}
						if (!(this is BedFurniture) || furnitureY != anchor.Y)
						{
							return 3;
						}
						IL_222:
						furnitureY++;
						continue;
						IL_1D4:
						int buildings_index = location.getTileIndexAt(furnitureX, furnitureY, "Buildings", null);
						if (buildings_index != -1 && (!(location is IslandFarmHouse) || buildings_index < 192 || buildings_index > 194 || !(location.getTileSheetIDAt(furnitureX, furnitureY, "Buildings") == "untitled tile sheet")))
						{
							return -1;
						}
						goto IL_222;
					}
				}
				return 0;
			}
			return 4;
		}

		// Token: 0x06001EA9 RID: 7849 RVA: 0x001617CB File Offset: 0x0015F9CB
		public override bool isPassable()
		{
			return this.furniture_type.Value == 12 || base.isPassable();
		}

		// Token: 0x06001EAA RID: 7850 RVA: 0x001617E4 File Offset: 0x0015F9E4
		public override bool isPlaceable()
		{
			return true;
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x001617E7 File Offset: 0x0015F9E7
		public virtual bool AllowPlacementOnThisTile(int tile_x, int tile_y)
		{
			return false;
		}

		// Token: 0x06001EAC RID: 7852 RVA: 0x001617EA File Offset: 0x0015F9EA
		public override Rectangle GetBoundingBoxAt(int x, int y)
		{
			if (this.isTemporarilyInvisible)
			{
				return Rectangle.Empty;
			}
			return this.boundingBox.Value;
		}

		// Token: 0x06001EAD RID: 7853 RVA: 0x00161808 File Offset: 0x0015FA08
		protected static Rectangle getDefaultSourceRectForType(ParsedItemData itemData, int type, Texture2D texture = null)
		{
			int width;
			int height;
			switch (type)
			{
			case 0:
				width = 1;
				height = 2;
				goto IL_B4;
			case 1:
				width = 2;
				height = 2;
				goto IL_B4;
			case 2:
				width = 3;
				height = 2;
				goto IL_B4;
			case 3:
				width = 2;
				height = 2;
				goto IL_B4;
			case 4:
				width = 2;
				height = 2;
				goto IL_B4;
			case 5:
				width = 5;
				height = 3;
				goto IL_B4;
			case 6:
				width = 2;
				height = 2;
				goto IL_B4;
			case 7:
				width = 1;
				height = 3;
				goto IL_B4;
			case 8:
				width = 1;
				height = 2;
				goto IL_B4;
			case 10:
				width = 2;
				height = 3;
				goto IL_B4;
			case 11:
				width = 2;
				height = 3;
				goto IL_B4;
			case 12:
				width = 3;
				height = 2;
				goto IL_B4;
			case 13:
				width = 1;
				height = 2;
				goto IL_B4;
			case 14:
				width = 2;
				height = 5;
				goto IL_B4;
			case 16:
				width = 1;
				height = 2;
				goto IL_B4;
			case 17:
				width = 1;
				height = 2;
				goto IL_B4;
			}
			width = 1;
			height = 2;
			IL_B4:
			return Furniture.getDefaultSourceRect(itemData, width, height, texture);
		}

		// Token: 0x06001EAE RID: 7854 RVA: 0x001618D2 File Offset: 0x0015FAD2
		protected static Rectangle getDefaultSourceRect(ParsedItemData itemData, int spriteWidth, int spriteHeight, Texture2D texture = null)
		{
			texture = (texture ?? itemData.GetTexture());
			return new Rectangle(itemData.SpriteIndex * 16 % texture.Width, itemData.SpriteIndex * 16 / texture.Width * 16, spriteWidth * 16, spriteHeight * 16);
		}

		// Token: 0x06001EAF RID: 7855 RVA: 0x00161914 File Offset: 0x0015FB14
		protected virtual Rectangle getDefaultBoundingBoxForType(int type)
		{
			int width;
			int height;
			switch (type)
			{
			case 0:
				width = 1;
				height = 1;
				goto IL_B4;
			case 1:
				width = 2;
				height = 1;
				goto IL_B4;
			case 2:
				width = 3;
				height = 1;
				goto IL_B4;
			case 3:
				width = 2;
				height = 1;
				goto IL_B4;
			case 4:
				width = 2;
				height = 1;
				goto IL_B4;
			case 5:
				width = 5;
				height = 2;
				goto IL_B4;
			case 6:
				width = 2;
				height = 2;
				goto IL_B4;
			case 7:
				width = 1;
				height = 1;
				goto IL_B4;
			case 8:
				width = 1;
				height = 1;
				goto IL_B4;
			case 10:
				width = 2;
				height = 1;
				goto IL_B4;
			case 11:
				width = 2;
				height = 2;
				goto IL_B4;
			case 12:
				width = 3;
				height = 2;
				goto IL_B4;
			case 13:
				width = 1;
				height = 2;
				goto IL_B4;
			case 14:
				width = 2;
				height = 1;
				goto IL_B4;
			case 16:
				width = 1;
				height = 1;
				goto IL_B4;
			case 17:
				width = 1;
				height = 2;
				goto IL_B4;
			}
			width = 1;
			height = 1;
			IL_B4:
			return new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, width * 64, height * 64);
		}

		// Token: 0x06001EB0 RID: 7856 RVA: 0x00161A00 File Offset: 0x0015FC00
		public static int getTypeNumberFromName(string typeName)
		{
			if (typeName.StartsWithIgnoreCase("bed"))
			{
				return 15;
			}
			string text = typeName.ToLower();
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
					if (text == "rug")
					{
						return 12;
					}
					break;
				case 4:
					if (text == "lamp")
					{
						return 7;
					}
					break;
				case 5:
				{
					char c = text[2];
					if (c <= 'n')
					{
						switch (c)
						{
						case 'a':
							if (text == "chair")
							{
								return 0;
							}
							break;
						case 'b':
							if (text == "table")
							{
								return 11;
							}
							break;
						case 'c':
							if (text == "decor")
							{
								return 8;
							}
							break;
						default:
							if (c == 'n')
							{
								if (text == "bench")
								{
									return 1;
								}
							}
							break;
						}
					}
					else if (c != 'r')
					{
						if (c == 'u')
						{
							if (text == "couch")
							{
								return 2;
							}
						}
					}
					else if (text == "torch")
					{
						return 16;
					}
					break;
				}
				case 6:
				{
					char c = text[0];
					if (c != 's')
					{
						if (c == 'w')
						{
							if (text == "window")
							{
								return 13;
							}
						}
					}
					else if (text == "sconce")
					{
						return 17;
					}
					break;
				}
				case 7:
					if (text == "dresser")
					{
						return 4;
					}
					break;
				case 8:
				{
					char c = text[0];
					if (c != 'a')
					{
						if (c != 'b')
						{
							if (c == 'p')
							{
								if (text == "painting")
								{
									return 6;
								}
							}
						}
						else if (text == "bookcase")
						{
							return 10;
						}
					}
					else if (text == "armchair")
					{
						return 3;
					}
					break;
				}
				case 9:
					if (text == "fireplace")
					{
						return 14;
					}
					break;
				case 10:
					if (text == "long table")
					{
						return 5;
					}
					break;
				}
			}
			return 9;
		}

		// Token: 0x06001EB1 RID: 7857 RVA: 0x00161C30 File Offset: 0x0015FE30
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			return this.price.Value;
		}

		// Token: 0x06001EB2 RID: 7858 RVA: 0x00161C3D File Offset: 0x0015FE3D
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06001EB3 RID: 7859 RVA: 0x00161C40 File Offset: 0x0015FE40
		public override string Name
		{
			get
			{
				return base.name;
			}
		}

		// Token: 0x06001EB4 RID: 7860 RVA: 0x00161C48 File Offset: 0x0015FE48
		protected virtual float getScaleSize()
		{
			int tilesWide = this.defaultSourceRect.Width / 16;
			int tilesHigh = this.defaultSourceRect.Height / 16;
			if (tilesWide >= 7)
			{
				return 0.5f;
			}
			if (tilesWide >= 6)
			{
				return 0.66f;
			}
			if (tilesWide >= 5)
			{
				return 0.75f;
			}
			if (tilesHigh >= 5)
			{
				return 0.8f;
			}
			if (tilesHigh >= 3)
			{
				return 1f;
			}
			if (tilesWide <= 2)
			{
				return 2f;
			}
			if (tilesWide <= 4)
			{
				return 1f;
			}
			return 0.1f;
		}

		// Token: 0x06001EB5 RID: 7861 RVA: 0x00161CC0 File Offset: 0x0015FEC0
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.Location == null)
			{
				return;
			}
			if (Game1.IsMasterGame && this.sittingFarmers.Length > 0)
			{
				List<long> ids_to_remove = null;
				foreach (long uid in this.sittingFarmers.Keys)
				{
					if (!Game1.player.team.playerIsOnline(uid))
					{
						if (ids_to_remove == null)
						{
							ids_to_remove = new List<long>();
						}
						ids_to_remove.Add(uid);
					}
				}
				if (ids_to_remove != null)
				{
					foreach (long uid2 in ids_to_remove)
					{
						this.sittingFarmers.Remove(uid2);
					}
				}
			}
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (base.IsOn && base.SpecialVariable == 388859)
			{
				this.lastNoteBlockSoundTime += (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this.lastNoteBlockSoundTime > 500)
				{
					this.lastNoteBlockSoundTime = 0;
					this.addCauldronBubbles(-0.5f);
				}
			}
		}

		// Token: 0x06001EB6 RID: 7862 RVA: 0x00161E1C File Offset: 0x0016001C
		private void addCauldronBubbles(float speed = -0.5f)
		{
			this.Location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), this.TileLocation * 64f + new Vector2(41.6f, -21f) + new Vector2((float)Game1.random.Next(-12, 21), (float)Game1.random.Next(16)), false, 0.002f, Color.Lime)
			{
				alphaFade = 0.001f - speed / 300f,
				alpha = 0.75f,
				motion = new Vector2(0f, speed),
				acceleration = new Vector2(0f, 0f),
				interval = 99999f,
				layerDepth = (float)(this.boundingBox.Bottom - 3 - Game1.random.Next(5)) / 10000f,
				scale = 3f,
				scaleChange = 0.01f,
				rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
			});
		}

		// Token: 0x06001EB7 RID: 7863 RVA: 0x00161F56 File Offset: 0x00160156
		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
		}

		// Token: 0x06001EB8 RID: 7864 RVA: 0x00161F58 File Offset: 0x00160158
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Rectangle sourceRect = itemData.GetSourceRect(0, null);
			spriteBatch.Draw(itemData.GetTexture(), location + new Vector2(32f, 32f), new Rectangle?(itemData.GetSourceRect(0, null)), color * transparency, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), 1f * this.getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001EB9 RID: 7865 RVA: 0x0016200C File Offset: 0x0016020C
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			Rectangle drawn_source_rect = this.sourceRect.Value;
			drawn_source_rect.X += drawn_source_rect.Width * this.sourceIndexOffset.Value;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			string textureName = itemData.TextureName;
			if (itemData.IsErrorItem)
			{
				drawn_source_rect = itemData.GetSourceRect(0, null);
			}
			if (Furniture._frontTextureName == null)
			{
				Furniture._frontTextureName = new Dictionary<string, string>();
			}
			if (Furniture.isDrawingLocationFurniture)
			{
				string frontTexturePath;
				if (!Furniture._frontTextureName.TryGetValue(textureName, out frontTexturePath))
				{
					frontTexturePath = textureName + "Front";
					Furniture._frontTextureName[textureName] = frontTexturePath;
				}
				Texture2D frontTexture = null;
				if (this.HasSittingFarmers() || base.SpecialVariable == 388859)
				{
					try
					{
						frontTexture = Game1.content.Load<Texture2D>(frontTexturePath);
					}
					catch
					{
						frontTexture = null;
					}
				}
				Vector2 actualDrawPosition = Game1.GlobalToLocal(Game1.viewport, this.drawPosition.Value + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero));
				SpriteEffects spriteEffects = this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				Color color = Color.White * alpha;
				if (this.HasSittingFarmers())
				{
					spriteBatch.Draw(texture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (float)(this.boundingBox.Value.Top + 16) / 10000f);
					if (frontTexture != null && drawn_source_rect.Right <= frontTexture.Width && drawn_source_rect.Bottom <= frontTexture.Height)
					{
						spriteBatch.Draw(frontTexture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (float)(this.boundingBox.Value.Bottom - 8) / 10000f);
					}
				}
				else
				{
					spriteBatch.Draw(texture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (this.furniture_type.Value == 12) ? (2E-09f + this.tileLocation.Y / 100000f) : ((float)(this.boundingBox.Value.Bottom - ((this.furniture_type.Value == 6 || this.furniture_type.Value == 17 || this.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
					if (base.SpecialVariable == 388859 && frontTexture != null && drawn_source_rect.Right <= frontTexture.Width && drawn_source_rect.Bottom <= frontTexture.Height)
					{
						spriteBatch.Draw(frontTexture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (float)(this.boundingBox.Value.Bottom - 2) / 10000f);
					}
				}
			}
			else
			{
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (drawn_source_rect.Height * 4 - this.boundingBox.Height) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.furniture_type.Value == 12) ? (2E-09f + this.tileLocation.Y / 100000f) : ((float)(this.boundingBox.Value.Bottom - ((this.furniture_type.Value == 6 || this.furniture_type.Value == 17 || this.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
			}
			if (this.heldObject.Value != null)
			{
				Furniture furniture = this.heldObject.Value as Furniture;
				if (furniture != null)
				{
					furniture.drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32), (float)(this.boundingBox.Center.Y - furniture.sourceRect.Height * 4 - (this.drawHeldObjectLow.Value ? -16 : 16)))), (float)(this.boundingBox.Bottom - 7) / 10000f, alpha);
				}
				else
				{
					ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(this.heldObject.Value.QualifiedItemId);
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32), (float)(this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85)))) + new Vector2(32f, 53f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.boundingBox.Bottom / 10000f);
					if (this.heldObject.Value is ColoredObject)
					{
						this.heldObject.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32), (float)(this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85)))), 1f, 1f, (float)(this.boundingBox.Bottom + 1) / 10000f, StackDrawType.Hide, Color.White, false);
					}
					else
					{
						spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32), (float)(this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85)))), new Rectangle?(heldItemData.GetSourceRect(0, null)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.boundingBox.Bottom + 1) / 10000f);
					}
				}
			}
			if (this.isOn.Value && this.furniture_type.Value == 14)
			{
				Rectangle bounds = this.GetBoundingBoxAt(x, y);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 12), (float)(this.boundingBox.Center.Y - 64))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(bounds.Bottom - 2) / 10000f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 32 - 4), (float)(this.boundingBox.Center.Y - 64))), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(bounds.Bottom - 1) / 10000f);
			}
			else if (this.isOn.Value && this.furniture_type.Value == 16)
			{
				Rectangle bounds2 = this.GetBoundingBoxAt(x, y);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - 20), (float)this.boundingBox.Center.Y - 105.6f)), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(bounds2.Bottom - 2) / 10000f);
			}
			if (Game1.debugMode)
			{
				spriteBatch.DrawString(Game1.smallFont, base.QualifiedItemId, Game1.GlobalToLocal(Game1.viewport, this.drawPosition.Value), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06001EBA RID: 7866 RVA: 0x001629EC File Offset: 0x00160BEC
		public virtual void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Rectangle drawn_source_rect = this.sourceRect.Value;
			drawn_source_rect.X += drawn_source_rect.Width * this.sourceIndexOffset.Value;
			if (itemData.IsErrorItem)
			{
				drawn_source_rect = itemData.GetSourceRect(0, null);
			}
			spriteBatch.Draw(itemData.GetTexture(), location, new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		// Token: 0x06001EBB RID: 7867 RVA: 0x00162A87 File Offset: 0x00160C87
		public virtual int GetAdditionalTilePropertyRadius()
		{
			return 0;
		}

		// Token: 0x06001EBC RID: 7868 RVA: 0x00162A8A File Offset: 0x00160C8A
		public virtual bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			return false;
		}

		// Token: 0x06001EBD RID: 7869 RVA: 0x00162A90 File Offset: 0x00160C90
		public virtual bool IntersectsForCollision(Rectangle rect)
		{
			return base.GetBoundingBox().Intersects(rect);
		}

		// Token: 0x06001EBE RID: 7870 RVA: 0x00162AAC File Offset: 0x00160CAC
		protected override Item GetOneNew()
		{
			return new Furniture(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001EBF RID: 7871 RVA: 0x00162AC4 File Offset: 0x00160CC4
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Furniture fromFurniture = source as Furniture;
			if (fromFurniture != null)
			{
				this.drawPosition.Value = fromFurniture.drawPosition.Value;
				this.defaultBoundingBox.Value = fromFurniture.defaultBoundingBox.Value;
				this.boundingBox.Value = fromFurniture.boundingBox.Value;
				this.isOn.Value = false;
				this.rotations.Value = fromFurniture.rotations.Value;
				this.currentRotation.Value = fromFurniture.currentRotation.Value - ((this.rotations.Value == 4) ? 1 : 2);
				this.rotate();
			}
		}

		// Token: 0x040012C4 RID: 4804
		public const int chair = 0;

		// Token: 0x040012C5 RID: 4805
		public const int bench = 1;

		// Token: 0x040012C6 RID: 4806
		public const int couch = 2;

		// Token: 0x040012C7 RID: 4807
		public const int armchair = 3;

		// Token: 0x040012C8 RID: 4808
		public const int dresser = 4;

		// Token: 0x040012C9 RID: 4809
		public const int longTable = 5;

		// Token: 0x040012CA RID: 4810
		public const int painting = 6;

		// Token: 0x040012CB RID: 4811
		public const int lamp = 7;

		// Token: 0x040012CC RID: 4812
		public const int decor = 8;

		// Token: 0x040012CD RID: 4813
		public const int other = 9;

		// Token: 0x040012CE RID: 4814
		public const int bookcase = 10;

		// Token: 0x040012CF RID: 4815
		public const int table = 11;

		// Token: 0x040012D0 RID: 4816
		public const int rug = 12;

		// Token: 0x040012D1 RID: 4817
		public const int window = 13;

		// Token: 0x040012D2 RID: 4818
		public const int fireplace = 14;

		// Token: 0x040012D3 RID: 4819
		public const int bed = 15;

		// Token: 0x040012D4 RID: 4820
		public const int torch = 16;

		// Token: 0x040012D5 RID: 4821
		public const int sconce = 17;

		// Token: 0x040012D6 RID: 4822
		public const string furnitureTextureName = "TileSheets\\furniture";

		// Token: 0x040012D7 RID: 4823
		[XmlElement("furniture_type")]
		public readonly NetInt furniture_type = new NetInt();

		// Token: 0x040012D8 RID: 4824
		[XmlElement("rotations")]
		public readonly NetInt rotations = new NetInt();

		// Token: 0x040012D9 RID: 4825
		[XmlElement("currentRotation")]
		public readonly NetInt currentRotation = new NetInt();

		// Token: 0x040012DA RID: 4826
		[XmlElement("sourceIndexOffset")]
		private readonly NetInt sourceIndexOffset = new NetInt();

		// Token: 0x040012DB RID: 4827
		[XmlElement("drawPosition")]
		protected readonly NetVector2 drawPosition = new NetVector2();

		// Token: 0x040012DC RID: 4828
		[XmlElement("sourceRect")]
		public readonly NetRectangle sourceRect = new NetRectangle();

		// Token: 0x040012DD RID: 4829
		[XmlElement("defaultSourceRect")]
		public readonly NetRectangle defaultSourceRect = new NetRectangle();

		// Token: 0x040012DE RID: 4830
		[XmlElement("defaultBoundingBox")]
		public readonly NetRectangle defaultBoundingBox = new NetRectangle();

		// Token: 0x040012DF RID: 4831
		[XmlElement("drawHeldObjectLow")]
		public readonly NetBool drawHeldObjectLow = new NetBool();

		// Token: 0x040012E0 RID: 4832
		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

		// Token: 0x040012E1 RID: 4833
		[XmlIgnore]
		public Vector2? lightGlowPosition;

		// Token: 0x040012E2 RID: 4834
		[XmlIgnore]
		public bool AllowLocalRemoval = true;

		// Token: 0x040012E3 RID: 4835
		public static bool isDrawingLocationFurniture;

		// Token: 0x040012E4 RID: 4836
		protected static Dictionary<string, string> _frontTextureName;

		// Token: 0x040012E5 RID: 4837
		[XmlIgnore]
		private int _placementRestriction = -1;

		// Token: 0x040012E6 RID: 4838
		[XmlIgnore]
		private string _description;
	}
}
