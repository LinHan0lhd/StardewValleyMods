using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Delegates;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.Objects
{
	// Token: 0x020001B0 RID: 432
	public class Mannequin : Object
	{
		// Token: 0x06001EF1 RID: 7921 RVA: 0x001641F4 File Offset: 0x001623F4
		public Mannequin()
		{
		}

		// Token: 0x06001EF2 RID: 7922 RVA: 0x00164260 File Offset: 0x00162460
		public Mannequin(string itemId) : this()
		{
			base.ItemId = itemId;
			base.name = itemId;
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
			base.ParentSheetIndex = data.SpriteIndex;
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
			this.setIndoors.Value = true;
			this.setOutdoors.Value = true;
			base.Type = "interactive";
			this.facing.Value = 2;
		}

		// Token: 0x06001EF3 RID: 7923 RVA: 0x001642DC File Offset: 0x001624DC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.changeMutex.NetFields, "changeMutex.NetFields").AddField(this.hat, "hat").AddField(this.shirt, "shirt").AddField(this.pants, "pants").AddField(this.boots, "boots").AddField(this.facing, "facing").AddField(this.swappedWithFarmerTonight, "swappedWithFarmerTonight");
			this.hat.fieldChangeVisibleEvent += this.OnMannequinUpdated<NetRef<Hat>, Hat>;
			this.shirt.fieldChangeVisibleEvent += this.OnMannequinUpdated<NetRef<Clothing>, Clothing>;
			this.pants.fieldChangeVisibleEvent += this.OnMannequinUpdated<NetRef<Clothing>, Clothing>;
			this.boots.fieldChangeVisibleEvent += this.OnMannequinUpdated<NetRef<Boots>, Boots>;
		}

		// Token: 0x06001EF4 RID: 7924 RVA: 0x001643C7 File Offset: 0x001625C7
		private void OnMannequinUpdated<TNetField, TValue>(TNetField field, TValue oldValue, TValue newValue)
		{
			this.renderCache = null;
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x001643D0 File Offset: 0x001625D0
		protected internal MannequinData GetMannequinData()
		{
			if (this._data == null)
			{
				this._data = DataLoader.Mannequins(Game1.content).GetValueOrDefault(base.ItemId);
			}
			return this._data;
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x001643FC File Offset: 0x001625FC
		protected override string loadDisplayName()
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
			if (this.displayNameOverride == null)
			{
				return data.DisplayName;
			}
			return this.displayNameOverride;
		}

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06001EF7 RID: 7927 RVA: 0x0016442A File Offset: 0x0016262A
		public override string TypeDefinitionId { get; } = "(M)";

		// Token: 0x06001EF8 RID: 7928 RVA: 0x00164434 File Offset: 0x00162634
		public override string getDescription()
		{
			if (this._description == null)
			{
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
				this._description = Game1.parseText(TokenParser.ParseText(data.Description, null, null, null), Game1.smallFont, this.getDescriptionWidth());
			}
			return this._description;
		}

		// Token: 0x06001EF9 RID: 7929 RVA: 0x0016447F File Offset: 0x0016267F
		public override bool isPlaceable()
		{
			return true;
		}

		// Token: 0x06001EFA RID: 7930 RVA: 0x00164484 File Offset: 0x00162684
		public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToField<Hat>(this.hat, handler, getPath, null) && ForEachItemHelper.ApplyToField<Clothing>(this.shirt, handler, getPath, null) && ForEachItemHelper.ApplyToField<Clothing>(this.pants, handler, getPath, null) && ForEachItemHelper.ApplyToField<Boots>(this.boots, handler, getPath, null);
		}

		// Token: 0x06001EFB RID: 7931 RVA: 0x001644DC File Offset: 0x001626DC
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
			Mannequin toPlace = base.getOne() as Mannequin;
			location.Objects.Add(placementTile, toPlace);
			location.playSound("woodyStep", null, null, SoundContext.Default);
			return true;
		}

		// Token: 0x06001EFC RID: 7932 RVA: 0x00164534 File Offset: 0x00162734
		private void emitGhost()
		{
			this.Location.temporarySprites.Add(new TemporaryAnimatedSprite(this.GetMannequinData().Texture, new Rectangle((Game1.random.NextDouble() < 0.5) ? 0 : 64, 64, 16, 32), this.TileLocation * 64f + new Vector2(0f, -1f) * 64f, false, 0.004f, Color.White)
			{
				scale = 4f,
				layerDepth = 1f,
				motion = new Vector2((float)(7 + Game1.random.Next(-1, 6)), (float)(-8 + Game1.random.Next(-1, 5))),
				acceleration = new Vector2(-0.4f + (float)Game1.random.Next(10) / 100f, 0f),
				animationLength = 4,
				totalNumberOfLoops = 99,
				interval = 80f,
				scaleChangeChange = 0.01f
			});
			this.Location.playSound("cursed_mannequin", null, null, SoundContext.Default);
		}

		// Token: 0x06001EFD RID: 7933 RVA: 0x00164670 File Offset: 0x00162870
		public override bool minutesElapsed(int minutes)
		{
			if (Game1.random.NextDouble() < 0.001 && this.GetMannequinData().Cursed)
			{
				if (Game1.timeOfDay > Game1.getTrulyDarkTime(this.Location) && Game1.random.NextDouble() < 0.1)
				{
					this.emitGhost();
				}
				else if (Game1.random.NextDouble() < 0.66)
				{
					if (Game1.random.NextDouble() < 0.5)
					{
						using (FarmerCollection.Enumerator enumerator = this.Location.farmers.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Farmer f = enumerator.Current;
								this.facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(this.TileLocation, f.Tile));
								this.renderCache = null;
							}
							goto IL_12A;
						}
					}
					this.eyeTimer = 2500;
				}
				else
				{
					this.Location.playSound("cursed_mannequin", null, null, SoundContext.Default);
					this.shakeTimer = Game1.random.Next(500, 4000);
				}
			}
			IL_12A:
			return base.minutesElapsed(minutes);
		}

		// Token: 0x06001EFE RID: 7934 RVA: 0x001647C0 File Offset: 0x001629C0
		public override void actionOnPlayerEntry()
		{
			if (Game1.random.NextDouble() < 0.001 && this.GetMannequinData().Cursed)
			{
				this.shakeTimer = Game1.random.Next(500, 1000);
			}
			base.actionOnPlayerEntry();
		}

		// Token: 0x06001EFF RID: 7935 RVA: 0x00164810 File Offset: 0x00162A10
		public override void DayUpdate()
		{
			base.DayUpdate();
			if (Game1.IsMasterGame && this.GetMannequinData().Cursed && this.Location != null && (this.Location is FarmHouse || this.Location is IslandFarmHouse || this.Location is Shed))
			{
				if (Game1.random.NextDouble() < 0.05)
				{
					Vector2 oldTile = this.TileLocation;
					Utility.spawnObjectAround(this.TileLocation, this, this.Location, false, delegate(Object x)
					{
						if (!this.TileLocation.Equals(oldTile))
						{
							this.Location.objects.Remove(oldTile);
						}
					});
					return;
				}
				if (this.swappedWithFarmerTonight.Value)
				{
					this.swappedWithFarmerTonight.Value = false;
					return;
				}
				if (Game1.random.NextDouble() < 0.005)
				{
					if (this.Location.farmers.Count <= 0)
					{
						return;
					}
					using (FarmerCollection.Enumerator enumerator = this.Location.farmers.GetEnumerator())
					{
						if (!enumerator.MoveNext())
						{
							return;
						}
						Farmer who = enumerator.Current;
						Vector2 oldTile = this.TileLocation;
						Vector2 bedTile = who.mostRecentBed / 64f;
						bedTile.X = (float)((int)bedTile.X);
						bedTile.Y = (float)((int)bedTile.Y);
						if (Utility.spawnObjectAround(bedTile, this, this.Location, false, delegate(Object x)
						{
							if (!this.TileLocation.Equals(oldTile))
							{
								this.Location.objects.Remove(oldTile);
							}
						}))
						{
							this.facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(this.TileLocation, who.Tile));
							this.renderCache = null;
							this.eyeTimer = 2000;
						}
						return;
					}
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					DecoratableLocation dec_location = this.Location as DecoratableLocation;
					string floorID = dec_location.GetFloorID((int)this.TileLocation.X, (int)this.TileLocation.Y);
					string wallpaperID = null;
					for (int y = (int)this.TileLocation.Y; y > 0; y--)
					{
						wallpaperID = dec_location.GetWallpaperID((int)this.TileLocation.X, y);
						if (wallpaperID != null)
						{
							break;
						}
					}
					if (floorID != null)
					{
						dec_location.SetFloor("MoreFloors:6", floorID);
					}
					if (wallpaperID != null)
					{
						dec_location.SetWallpaper("MoreWalls:21", wallpaperID);
					}
					this.shakeTimer = 10000;
					return;
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					DecoratableLocation dec_location2 = this.Location as DecoratableLocation;
					if (Game1.random.NextDouble() < 0.33)
					{
						for (int i = 0; i < 30; i++)
						{
							int xPos = Game1.random.Next(2, this.Location.Map.Layers[0].LayerWidth - 2);
							for (int y2 = 1; y2 < this.Location.Map.Layers[0].LayerHeight; y2++)
							{
								Vector2 spot = new Vector2((float)xPos, (float)y2);
								if (this.Location.isTileLocationOpen(spot) && this.Location.isTilePlaceable(spot, false) && !dec_location2.isTileOnWall(xPos, y2) && !this.Location.IsTileOccupiedBy(spot, CollisionMask.All, CollisionMask.None, false))
								{
									this.facing.Value = 2;
									this.renderCache = null;
									this.Location.objects.Remove(this.TileLocation);
									this.TileLocation = spot;
									this.Location.objects.Add(this.TileLocation, this);
									return;
								}
							}
						}
						return;
					}
					int xStartingPoint;
					int xEndingPoint;
					int xDirection;
					if (Game1.random.NextDouble() < 0.5)
					{
						xStartingPoint = 1;
						xEndingPoint = this.Location.Map.Layers[0].LayerWidth - 1;
						xDirection = 1;
					}
					else
					{
						xStartingPoint = this.Location.Map.Layers[0].LayerWidth - 1;
						xEndingPoint = 1;
						xDirection = -1;
					}
					for (int j = 0; j < 30; j++)
					{
						int yPos = Game1.random.Next(2, this.Location.Map.Layers[0].LayerHeight - 2);
						for (int x2 = xStartingPoint; x2 != xEndingPoint; x2 += xDirection)
						{
							Vector2 spot2 = new Vector2((float)x2, (float)yPos);
							if (this.Location.isTileLocationOpen(spot2) && this.Location.isTilePlaceable(spot2, false) && !dec_location2.isTileOnWall(x2, yPos) && !this.Location.IsTileOccupiedBy(spot2, CollisionMask.All, CollisionMask.None, false))
							{
								this.facing.Value = ((xDirection == 1) ? 1 : 3);
								this.renderCache = null;
								this.Location.objects.Remove(this.TileLocation);
								this.TileLocation = spot2;
								this.Location.objects.Add(this.TileLocation, this);
								return;
							}
						}
					}
					return;
				}
			}
			else if (Game1.IsMasterGame && this.Location is SeedShop && this.TileLocation.X > 33f && this.TileLocation.Y > 14f)
			{
				if (base.ItemId.Equals("CursedMannequinMale"))
				{
					base.ItemId = "MannequinMale";
				}
				else if (base.ItemId.Equals("CursedMannequinFemale"))
				{
					base.ItemId = "MannequinFemale";
				}
				base.ResetParentSheetIndex();
				this.renderCache = null;
				this._data = null;
			}
		}

		// Token: 0x06001F00 RID: 7936 RVA: 0x00164DD8 File Offset: 0x00162FD8
		public override void updateWhenCurrentLocation(GameTime time)
		{
			base.updateWhenCurrentLocation(time);
			this.changeMutex.Update(this.Location);
			if (this.eyeTimer > 0)
			{
				this.eyeTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x06001F01 RID: 7937 RVA: 0x00164E24 File Offset: 0x00163024
		public override bool performToolAction(Tool t)
		{
			if (t == null)
			{
				return false;
			}
			if (t is MeleeWeapon || !t.isHeavyHitter())
			{
				return false;
			}
			if (this.hat.Value != null || this.shirt.Value != null || this.pants.Value != null || this.boots.Value != null)
			{
				if (this.hat.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.hat.Value));
					this.hat.Value = null;
				}
				else if (this.shirt.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.shirt.Value));
					this.shirt.Value = null;
				}
				else if (this.pants.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.pants.Value));
					this.pants.Value = null;
				}
				else if (this.boots.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.boots.Value));
					this.boots.Value = null;
				}
				this.Location.playSound("hammer", null, null, SoundContext.Default);
				this.shakeTimer = 100;
				return false;
			}
			this.Location.objects.Remove(this.TileLocation);
			this.Location.playSound("hammer", null, null, SoundContext.Default);
			this.DropItem(new Mannequin(base.ItemId));
			return false;
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x00164FC4 File Offset: 0x001631C4
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (who.CurrentItem is Hat || who.CurrentItem is Clothing || who.CurrentItem is Boots)
			{
				return false;
			}
			if (justCheckingForActivity)
			{
				return true;
			}
			if (this.hat.Value == null && this.shirt.Value == null && this.pants.Value == null && this.boots.Value == null)
			{
				this.facing.Value = (this.facing.Value + 1) % 4;
				this.renderCache = null;
				Game1.playSound("shwip", null);
			}
			else
			{
				this.changeMutex.RequestLock(delegate
				{
					this.hat.Value = who.Equip<Hat>(this.hat.Value, who.hat);
					this.shirt.Value = who.Equip<Clothing>(this.shirt.Value, who.shirtItem);
					this.pants.Value = who.Equip<Clothing>(this.pants.Value, who.pantsItem);
					this.boots.Value = who.Equip<Boots>(this.boots.Value, who.boots);
					this.changeMutex.ReleaseLock();
				}, null);
				Game1.playSound("coin", null);
			}
			if (this.GetMannequinData().Cursed && Game1.random.NextDouble() < 0.001)
			{
				this.emitGhost();
			}
			return true;
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x001650E4 File Offset: 0x001632E4
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			Hat newHat = dropInItem as Hat;
			if (newHat == null)
			{
				Clothing newClothing = dropInItem as Clothing;
				if (newClothing == null)
				{
					Boots newBoots = dropInItem as Boots;
					if (newBoots == null)
					{
						return false;
					}
					if (!probe)
					{
						this.DropItem(this.boots.Value);
						this.boots.Value = (Boots)newBoots.getOne();
					}
				}
				else if (!probe)
				{
					if (newClothing.clothesType.Value == Clothing.ClothesType.SHIRT)
					{
						this.DropItem(this.shirt.Value);
						this.shirt.Value = (Clothing)newClothing.getOne();
					}
					else
					{
						this.DropItem(this.pants.Value);
						this.pants.Value = (Clothing)newClothing.getOne();
					}
				}
			}
			else if (!probe)
			{
				this.DropItem(this.hat.Value);
				this.hat.Value = (Hat)newHat.getOne();
			}
			if (!probe)
			{
				Game1.playSound("dirtyHit", null);
			}
			return true;
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x001651F4 File Offset: 0x001633F4
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (this.eyeTimer > 0 && this.facing.Value != 0)
			{
				float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1.1E-05f;
				Vector2 pos = Game1.GlobalToLocal(new Vector2((float)x, (float)y) * 64f + new Vector2(20f, -40f));
				int value = this.facing.Value;
				if (value != 1)
				{
					if (value == 3)
					{
						pos.X += 4f;
					}
				}
				else
				{
					pos.X += 12f;
				}
				if (this.facing.Value != 2)
				{
					pos.Y -= 4f;
				}
				spriteBatch.Draw(Game1.mouseCursors_1_6, pos, new Rectangle?(new Rectangle(377 + 5 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1620.0 / 60.0), 330, 5 + ((this.facing.Value != 2) ? -3 : 0), 3)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
			}
			float drawLayer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			Farmer fakeFarmer = this.GetFarmerForRendering();
			fakeFarmer.position.Value = new Vector2((float)(x * 64), (float)(y * 64 - 4 + (this.GetMannequinData().DisplaysClothingAsMale ? 20 : 16)));
			if (this.shakeTimer > 0)
			{
				fakeFarmer.position.Value += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
			}
			fakeFarmer.FarmerRenderer.draw(spriteBatch, fakeFarmer.FarmerSprite, fakeFarmer.FarmerSprite.SourceRect, fakeFarmer.getLocalPosition(Game1.viewport), new Vector2(0f, (float)fakeFarmer.GetBoundingBox().Height), drawLayer + 0.0001f, Color.White, 0f, fakeFarmer);
			FarmerRenderer.FarmerSpriteLayers armLayer = FarmerRenderer.FarmerSpriteLayers.Arms;
			if (fakeFarmer.facingDirection.Value == 0)
			{
				armLayer = FarmerRenderer.FarmerSpriteLayers.ArmsUp;
			}
			if (fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset > 0)
			{
				Rectangle sourceRect = fakeFarmer.FarmerSprite.SourceRect;
				sourceRect.Offset(-288 + fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset * 16, 0);
				spriteBatch.Draw(fakeFarmer.FarmerRenderer.baseTexture, fakeFarmer.getLocalPosition(Game1.viewport) + new Vector2(0f, (float)fakeFarmer.GetBoundingBox().Height) + fakeFarmer.FarmerRenderer.positionOffset + fakeFarmer.armOffset, new Rectangle?(sourceRect), Color.White, 0f, new Vector2(0f, (float)fakeFarmer.GetBoundingBox().Height), 4f * this.scale, fakeFarmer.FarmerSprite.CurrentAnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(drawLayer + 0.0001f, armLayer, false));
			}
		}

		// Token: 0x06001F05 RID: 7941 RVA: 0x00165537 File Offset: 0x00163737
		protected override Item GetOneNew()
		{
			return new Mannequin(base.ItemId);
		}

		// Token: 0x06001F06 RID: 7942 RVA: 0x00165544 File Offset: 0x00163744
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Mannequin from = source as Mannequin;
			if (from != null)
			{
				NetFieldBase<Hat, NetRef<Hat>> netFieldBase = this.hat;
				Hat value = from.hat.Value;
				netFieldBase.Value = (((value != null) ? value.getOne() : null) as Hat);
				NetFieldBase<Clothing, NetRef<Clothing>> netFieldBase2 = this.shirt;
				Clothing value2 = from.shirt.Value;
				netFieldBase2.Value = (((value2 != null) ? value2.getOne() : null) as Clothing);
				NetFieldBase<Clothing, NetRef<Clothing>> netFieldBase3 = this.pants;
				Clothing value3 = from.pants.Value;
				netFieldBase3.Value = (((value3 != null) ? value3.getOne() : null) as Clothing);
				NetFieldBase<Boots, NetRef<Boots>> netFieldBase4 = this.boots;
				Boots value4 = from.boots.Value;
				netFieldBase4.Value = (((value4 != null) ? value4.getOne() : null) as Boots);
			}
		}

		// Token: 0x06001F07 RID: 7943 RVA: 0x00165604 File Offset: 0x00163804
		private void DropItem(Item item)
		{
			if (item == null)
			{
				return;
			}
			Vector2 position = new Vector2((this.TileLocation.X + 0.5f) * 64f, (this.TileLocation.Y + 0.5f) * 64f);
			this.Location.debris.Add(new Debris(item, position));
		}

		// Token: 0x06001F08 RID: 7944 RVA: 0x00165661 File Offset: 0x00163861
		private Farmer GetFarmerForRendering()
		{
			this.renderCache = (this.renderCache ?? this.<GetFarmerForRendering>g__CreateInstance|37_0());
			return this.renderCache;
		}

		// Token: 0x06001F09 RID: 7945 RVA: 0x00165680 File Offset: 0x00163880
		[CompilerGenerated]
		private Farmer <GetFarmerForRendering>g__CreateInstance|37_0()
		{
			MannequinData data = this.GetMannequinData();
			Farmer farmer = new Farmer();
			farmer.changeGender(data.DisplaysClothingAsMale);
			farmer.faceDirection(this.facing.Value);
			farmer.changeHairColor(Color.Transparent);
			farmer.skin.Set(farmer.FarmerRenderer.recolorSkin(-12345, false));
			farmer.hat.Value = this.hat.Value;
			farmer.shirtItem.Value = this.shirt.Value;
			if (this.shirt.Value != null)
			{
				farmer.changeShirt("-1");
			}
			farmer.pantsItem.Value = this.pants.Value;
			if (this.pants.Value != null)
			{
				farmer.changePantStyle("-1");
			}
			farmer.boots.Value = this.boots.Value;
			if (this.boots.Value != null)
			{
				farmer.changeShoeColor(this.boots.Value.GetBootsColorString());
			}
			farmer.FarmerRenderer.textureName.Value = data.FarmerTexture;
			farmer.FarmerSprite.PauseForSingleAnimation = true;
			farmer.currentEyes = 0;
			return farmer;
		}

		// Token: 0x04001307 RID: 4871
		protected string _description;

		// Token: 0x04001308 RID: 4872
		protected MannequinData _data;

		// Token: 0x04001309 RID: 4873
		public string displayNameOverride;

		// Token: 0x0400130A RID: 4874
		public readonly NetMutex changeMutex = new NetMutex();

		// Token: 0x0400130B RID: 4875
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		// Token: 0x0400130C RID: 4876
		public readonly NetRef<Clothing> shirt = new NetRef<Clothing>();

		// Token: 0x0400130D RID: 4877
		public readonly NetRef<Clothing> pants = new NetRef<Clothing>();

		// Token: 0x0400130E RID: 4878
		public readonly NetRef<Boots> boots = new NetRef<Boots>();

		// Token: 0x0400130F RID: 4879
		public readonly NetDirection facing = new NetDirection();

		// Token: 0x04001310 RID: 4880
		public readonly NetBool swappedWithFarmerTonight = new NetBool();

		// Token: 0x04001311 RID: 4881
		private Farmer renderCache;

		// Token: 0x04001312 RID: 4882
		internal int eyeTimer;
	}
}
