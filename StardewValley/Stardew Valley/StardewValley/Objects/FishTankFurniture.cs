using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001AA RID: 426
	public class FishTankFurniture : StorageFurniture
	{
		// Token: 0x06001E3C RID: 7740 RVA: 0x0015B820 File Offset: 0x00159A20
		public FishTankFurniture()
		{
			this.generationSeed.Value = Game1.random.Next();
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x0015B8A8 File Offset: 0x00159AA8
		public FishTankFurniture(string itemId, Vector2 tile, int initialRotations) : base(itemId, tile, initialRotations)
		{
			this.generationSeed.Value = Game1.random.Next();
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x0015B934 File Offset: 0x00159B34
		public FishTankFurniture(string itemId, Vector2 tile) : base(itemId, tile)
		{
			this.generationSeed.Value = Game1.random.Next();
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x0015B9BE File Offset: 0x00159BBE
		public override void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
		{
			base.actionOnPlayerEntryOrPlacement(environment, dropDown);
			this.ResetFish();
			this.UpdateFish();
		}

		// Token: 0x06001E40 RID: 7744 RVA: 0x0015B9D4 File Offset: 0x00159BD4
		public virtual void ResetFish()
		{
			this.bubbles.Clear();
			this.tankFish.Clear();
			this._fishLookup.Clear();
			this.UpdateFish();
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x0015B9FD File Offset: 0x00159BFD
		public Texture2D GetAquariumTexture()
		{
			if (this._aquariumTexture == null)
			{
				this._aquariumTexture = Game1.content.Load<Texture2D>("LooseSprites\\AquariumFish");
			}
			return this._aquariumTexture;
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x0015BA24 File Offset: 0x00159C24
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.generationSeed, "generationSeed").AddField(this.refreshFishEvent, "refreshFishEvent");
			this.refreshFishEvent.onEvent += this.UpdateDecorAndFish;
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x0015BA76 File Offset: 0x00159C76
		protected override Item GetOneNew()
		{
			return new FishTankFurniture(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x0015BA90 File Offset: 0x00159C90
		public virtual int GetCapacityForCategory(FishTankFurniture.FishTankCategories category)
		{
			int extra = 0;
			if (base.QualifiedItemId.Equals("(F)JungleTank"))
			{
				extra++;
			}
			switch (category)
			{
			case FishTankFurniture.FishTankCategories.Swim:
				return this.getTilesWide() - 1;
			case FishTankFurniture.FishTankCategories.Ground:
				return this.getTilesWide() - 1 + extra;
			case FishTankFurniture.FishTankCategories.Decoration:
				if (this.getTilesWide() <= 2)
				{
					return 1;
				}
				return -1;
			default:
				return 0;
			}
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x0015BAF0 File Offset: 0x00159CF0
		public FishTankFurniture.FishTankCategories GetCategoryFromItem(Item item)
		{
			Dictionary<string, string> aquarium_data = this.GetAquariumData();
			if (!this.CanBeDeposited(item))
			{
				return FishTankFurniture.FishTankCategories.None;
			}
			if (item.QualifiedItemId == "(TR)FrogEgg")
			{
				return FishTankFurniture.FishTankCategories.Ground;
			}
			string rawData;
			if (!aquarium_data.TryGetValue(item.ItemId, out rawData))
			{
				return FishTankFurniture.FishTankCategories.Decoration;
			}
			string a = ArgUtility.Get(rawData.Split('/', StringSplitOptions.None), 1, null, true);
			if (a == "crawl" || a == "ground" || a == "front_crawl" || a == "static")
			{
				return FishTankFurniture.FishTankCategories.Ground;
			}
			return FishTankFurniture.FishTankCategories.Swim;
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x0015BB80 File Offset: 0x00159D80
		public bool HasRoomForThisItem(Item item)
		{
			if (!this.CanBeDeposited(item))
			{
				return false;
			}
			FishTankFurniture.FishTankCategories category = this.GetCategoryFromItem(item);
			int capacity = this.GetCapacityForCategory(category);
			if (item is Hat)
			{
				capacity = 999;
			}
			if (capacity < 0)
			{
				foreach (Item held_item in this.heldItems)
				{
					if (held_item != null && held_item.QualifiedItemId == item.QualifiedItemId)
					{
						return false;
					}
				}
				return true;
			}
			int current_count = 0;
			foreach (Item held_item2 in this.heldItems)
			{
				if (held_item2 != null)
				{
					if (this.GetCategoryFromItem(held_item2) == category)
					{
						current_count++;
					}
					if (current_count >= capacity)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x0015BC78 File Offset: 0x00159E78
		public override string GetShopMenuContext()
		{
			return "FishTank";
		}

		// Token: 0x06001E48 RID: 7752 RVA: 0x0015BC7F File Offset: 0x00159E7F
		public override void ShowMenu()
		{
			this.ShowShopMenu();
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x0015BC88 File Offset: 0x00159E88
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
			if (this.mutex.IsLocked())
			{
				return true;
			}
			if (who.ActiveObject == null && !(who.CurrentItem is Hat))
			{
				Item currentItem = who.CurrentItem;
				if (!(((currentItem != null) ? currentItem.QualifiedItemId : null) == "(TR)FrogEgg"))
				{
					goto IL_13D;
				}
			}
			if (this.localDepositedItem == null && this.CanBeDeposited(who.CurrentItem))
			{
				if (!this.HasRoomForThisItem(who.CurrentItem))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishTank_Full"), true);
					return true;
				}
				this.localDepositedItem = who.CurrentItem.getOne();
				if (who.CurrentItem.ConsumeStack(1) == null)
				{
					who.removeItemFromInventory(who.CurrentItem);
					who.showNotCarrying();
				}
				this.mutex.RequestLock(delegate
				{
					location.playSound("dropItemInWater", null, null, SoundContext.Default);
					this.heldItems.Add(this.localDepositedItem);
					this.localDepositedItem = null;
					this.refreshFishEvent.Fire();
					this.mutex.ReleaseLock();
				}, delegate
				{
					this.localDepositedItem = who.addItemToInventory(this.localDepositedItem);
					if (this.localDepositedItem != null)
					{
						Game1.createItemDebris(this.localDepositedItem, new Vector2(this.TileLocation.X + (float)this.getTilesWide() / 2f + 0.5f, this.TileLocation.Y + 0.5f) * 64f, -1, location, -1, false);
					}
					this.localDepositedItem = null;
				});
				return true;
			}
			IL_13D:
			this.mutex.RequestLock(new Action(this.ShowMenu), null);
			return true;
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x0015BDEC File Offset: 0x00159FEC
		public virtual bool CanBeDeposited(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item.QualifiedItemId == "(TR)FrogEgg")
			{
				return true;
			}
			if (!(item is Hat) && !Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId))
			{
				return false;
			}
			if (item.QualifiedItemId == "(O)152" || item.QualifiedItemId == "(O)393" || item.QualifiedItemId == "(O)390" || item.QualifiedItemId == "(O)117" || item.QualifiedItemId == "(O)166" || item.QualifiedItemId == "(O)832" || item.QualifiedItemId == "(O)109" || item.QualifiedItemId == "(O)709" || item.QualifiedItemId == "(O)392" || item.QualifiedItemId == "(O)394" || item.QualifiedItemId == "(O)167" || item.QualifiedItemId == "(O)789" || item.QualifiedItemId == "(O)330" || item.QualifiedItemId == "(O)797")
			{
				return true;
			}
			if (item is Hat)
			{
				int numHatWearers = 0;
				int numHats = 0;
				using (List<TankFish>.Enumerator enumerator = this.tankFish.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.CanWearHat())
						{
							numHatWearers++;
						}
					}
				}
				using (NetList<Item, NetRef<Item>>.Enumerator enumerator2 = this.heldItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current is Hat)
						{
							numHats++;
						}
					}
				}
				return numHats < numHatWearers;
			}
			return this.GetAquariumData().ContainsKey(item.ItemId);
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x0015BFEC File Offset: 0x0015A1EC
		public override void DayUpdate()
		{
			this.ResetFish();
			base.DayUpdate();
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x0015BFFC File Offset: 0x0015A1FC
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (Game1.currentLocation == environment)
			{
				if (this.fishDirty)
				{
					this.fishDirty = false;
					this.UpdateDecorAndFish();
				}
				foreach (TankFish tankFish in this.tankFish)
				{
					tankFish.Update(time);
				}
				for (int i = 0; i < this.bubbles.Count; i++)
				{
					Vector4 bubble = this.bubbles[i];
					bubble.W += 0.05f;
					if (bubble.W > 1f)
					{
						bubble.W = 1f;
					}
					bubble.Y += bubble.W;
					this.bubbles[i] = bubble;
					if (bubble.Y >= (float)this.GetTankBounds().Height)
					{
						this.bubbles.RemoveAt(i);
						i--;
					}
				}
			}
			base.updateWhenCurrentLocation(time);
			this.refreshFishEvent.Poll();
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x0015C118 File Offset: 0x0015A318
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			this.generationSeed.Value = Game1.random.Next();
			this.fishDirty = true;
			return base.placementAction(location, x, y, who);
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x0015C141 File Offset: 0x0015A341
		public Dictionary<string, string> GetAquariumData()
		{
			return DataLoader.AquariumFish(Game1.content);
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x0015C14D File Offset: 0x0015A34D
		public override bool onDresserItemWithdrawn(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
		{
			bool result = base.onDresserItemWithdrawn(salable, who, countTaken, stock);
			this.refreshFishEvent.Fire();
			return result;
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x0015C168 File Offset: 0x0015A368
		public virtual void UpdateFish()
		{
			List<Item> fish_items = new List<Item>();
			Dictionary<string, string> aquarium_data = this.GetAquariumData();
			foreach (Item item in this.heldItems)
			{
				if (item != null)
				{
					Object o = item as Object;
					if (o != null)
					{
						o.reloadSprite();
					}
					bool forceValid = item.QualifiedItemId == "(TR)FrogEgg";
					if ((forceValid || Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId)) && (forceValid || aquarium_data.ContainsKey(item.ItemId)))
					{
						fish_items.Add(item);
					}
				}
			}
			List<Item> items_to_remove = new List<Item>();
			foreach (Item key in this._fishLookup.Keys)
			{
				if (!this.heldItems.Contains(key))
				{
					items_to_remove.Add(key);
				}
			}
			for (int i = 0; i < fish_items.Count; i++)
			{
				Item item2 = fish_items[i];
				if (!this._fishLookup.ContainsKey(item2))
				{
					TankFish fish = new TankFish(this, item2);
					this.tankFish.Add(fish);
					this._fishLookup[item2] = fish;
				}
			}
			foreach (Item removed_item in items_to_remove)
			{
				this.tankFish.Remove(this._fishLookup[removed_item]);
				this.heldItems.Remove(removed_item);
			}
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x0015C330 File Offset: 0x0015A530
		public virtual void UpdateDecorAndFish()
		{
			Random r = Utility.CreateRandom((double)this.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			this.UpdateFish();
			this.decorationSlots.Clear();
			for (int y = 0; y < 3; y++)
			{
				int x = 0;
				while (x < this.getTilesWide())
				{
					Vector2 slot_position = default(Vector2);
					if (y % 2 != 0)
					{
						slot_position.X = (float)(8 + x * 16);
						goto IL_86;
					}
					if (x != this.getTilesWide() - 1)
					{
						slot_position.X = (float)(16 + x * 16);
						goto IL_86;
					}
					IL_B2:
					x++;
					continue;
					IL_86:
					slot_position.Y = 4f;
					slot_position.Y += 3.3333333f * (float)y;
					this.decorationSlots.Add(slot_position);
					goto IL_B2;
				}
			}
			this.floorDecorationIndices.Clear();
			this.floorDecorations.Clear();
			this._currentDecorationIndex = 0;
			for (int i = 0; i < this.decorationSlots.Count; i++)
			{
				this.floorDecorationIndices.Add(i);
				this.floorDecorations.Add(null);
			}
			Utility.Shuffle<int>(r, this.floorDecorationIndices);
			Random decoration_random = Utility.CreateRandom((double)r.Next(), 0.0, 0.0, 0.0, 0.0);
			bool add_decoration = this.GetItemCount("393") > 0;
			for (int j = 0; j < 1; j++)
			{
				if (add_decoration)
				{
					this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 5), 256, 16, 16));
				}
				else
				{
					this._AdvanceDecorationIndex();
				}
			}
			decoration_random = Utility.CreateRandom((double)r.Next(), 0.0, 0.0, 0.0, 0.0);
			bool add_decoration2 = this.GetItemCount("152") > 0;
			for (int k = 0; k < 4; k++)
			{
				if (add_decoration2)
				{
					this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 288, 16, 16));
				}
				else
				{
					this._AdvanceDecorationIndex();
				}
			}
			decoration_random = Utility.CreateRandom((double)r.Next(), 0.0, 0.0, 0.0, 0.0);
			bool add_decoration3 = this.GetItemCount("390") > 0;
			for (int l = 0; l < 2; l++)
			{
				if (add_decoration3)
				{
					this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 272, 16, 16));
				}
				else
				{
					this._AdvanceDecorationIndex();
				}
			}
			if (this.GetItemCount("117") > 0)
			{
				this.AddFloorDecoration(new Rectangle(48, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("166") > 0)
			{
				this.AddFloorDecoration(new Rectangle(64, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("797") > 0)
			{
				this.AddFloorDecoration(new Rectangle(80, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("832") > 0)
			{
				this.AddFloorDecoration(new Rectangle(96, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("109") > 0)
			{
				this.AddFloorDecoration(new Rectangle(112, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("709") > 0)
			{
				this.AddFloorDecoration(new Rectangle(128, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("392") > 0)
			{
				this.AddFloorDecoration(new Rectangle(144, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("394") > 0)
			{
				this.AddFloorDecoration(new Rectangle(160, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("167") > 0)
			{
				this.AddFloorDecoration(new Rectangle(176, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("789") > 0)
			{
				this.AddFloorDecoration(new Rectangle(192, 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
			if (this.GetItemCount("330") > 0)
			{
				this.AddFloorDecoration(new Rectangle(208, 288, 16, 16));
				return;
			}
			this._AdvanceDecorationIndex();
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x0015C7E8 File Offset: 0x0015A9E8
		public virtual void AddFloorDecoration(Rectangle source_rect)
		{
			if (this._currentDecorationIndex == -1)
			{
				return;
			}
			int index = this.floorDecorationIndices[this._currentDecorationIndex];
			this._AdvanceDecorationIndex();
			int center_x = (int)this.decorationSlots[index].X;
			int center_y = (int)this.decorationSlots[index].Y;
			if (center_x < source_rect.Width / 2)
			{
				center_x = source_rect.Width / 2;
			}
			if (center_x > this.GetTankBounds().Width / 4 - source_rect.Width / 2)
			{
				center_x = this.GetTankBounds().Width / 4 - source_rect.Width / 2;
			}
			KeyValuePair<Rectangle, Vector2> decoration = new KeyValuePair<Rectangle, Vector2>(source_rect, new Vector2((float)center_x, (float)center_y));
			this.floorDecorations[index] = new KeyValuePair<Rectangle, Vector2>?(decoration);
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x0015C8A4 File Offset: 0x0015AAA4
		protected virtual void _AdvanceDecorationIndex()
		{
			for (int i = 0; i < this.decorationSlots.Count; i++)
			{
				this._currentDecorationIndex++;
				if (this._currentDecorationIndex >= this.decorationSlots.Count)
				{
					this._currentDecorationIndex = 0;
				}
				if (this.floorDecorations[this.floorDecorationIndices[this._currentDecorationIndex]] == null)
				{
					return;
				}
			}
			this._currentDecorationIndex = 1;
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x0015C91D File Offset: 0x0015AB1D
		public override void OnMenuClose()
		{
			this.refreshFishEvent.Fire();
			base.OnMenuClose();
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x0015C930 File Offset: 0x0015AB30
		public Vector2 GetFishSortRegion()
		{
			return new Vector2(this.GetBaseDrawLayer() + 1E-06f, this.GetGlassDrawLayer() - 1E-06f);
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x0015C94F File Offset: 0x0015AB4F
		public float GetGlassDrawLayer()
		{
			return this.GetBaseDrawLayer() + 0.0001f;
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x0015C960 File Offset: 0x0015AB60
		public float GetBaseDrawLayer()
		{
			if (this.furniture_type.Value != 12)
			{
				return (float)(this.boundingBox.Value.Bottom - ((this.furniture_type.Value == 6 || this.furniture_type.Value == 13) ? 48 : 8)) / 10000f;
			}
			return 2E-09f;
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x0015C9C0 File Offset: 0x0015ABC0
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 shake = Vector2.Zero;
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			Vector2 draw_position = this.drawPosition.Value;
			if (!Furniture.isDrawingLocationFurniture)
			{
				draw_position = new Vector2((float)x, (float)y) * 64f;
				draw_position.Y -= (float)(this.sourceRect.Height * 4 - this.boundingBox.Height);
			}
			if (this.shakeTimer > 0)
			{
				shake = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Rectangle mainSourceRect = itemData.GetSourceRect(0, null);
			spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position + shake), new Rectangle?(new Rectangle(mainSourceRect.X + mainSourceRect.Width, mainSourceRect.Y, mainSourceRect.Width, mainSourceRect.Height)), Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.GetGlassDrawLayer());
			if (Furniture.isDrawingLocationFurniture)
			{
				for (int i = 0; i < this.tankFish.Count; i++)
				{
					TankFish fish = this.tankFish[i];
					float fish_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, fish.zPosition / 20f);
					fish_layer += 1E-07f * (float)i;
					fish.Draw(spriteBatch, alpha, fish_layer);
				}
				for (int j = 0; j < this.floorDecorations.Count; j++)
				{
					if (this.floorDecorations[j] != null)
					{
						KeyValuePair<Rectangle, Vector2> decoration = this.floorDecorations[j].Value;
						Vector2 decoration_position = decoration.Value;
						Rectangle decoration_source_rect = decoration.Key;
						float decoration_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
						spriteBatch.Draw(this.GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)this.GetTankBounds().Left + decoration_position.X * 4f, (float)(this.GetTankBounds().Bottom - 4) - decoration_position.Y * 4f)), new Rectangle?(decoration_source_rect), Color.White * alpha, 0f, new Vector2((float)(decoration_source_rect.Width / 2), (float)(decoration_source_rect.Height - 4)), 4f, SpriteEffects.None, decoration_layer);
					}
				}
				foreach (Vector4 bubble in this.bubbles)
				{
					float layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
					spriteBatch.Draw(this.GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)this.GetTankBounds().Left + bubble.X, (float)(this.GetTankBounds().Bottom - 4) - bubble.Y - bubble.Z * 4f)), new Rectangle?(new Rectangle(0, 240, 16, 16)), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * bubble.W, SpriteEffects.None, layer);
				}
			}
			base.draw(spriteBatch, x, y, alpha);
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x0015CDA0 File Offset: 0x0015AFA0
		public int GetItemCount(string itemId)
		{
			int count = 0;
			foreach (Item item in this.heldItems)
			{
				if (Utility.IsNormalObjectAtParentSheetIndex(item, itemId))
				{
					count += item.Stack;
				}
			}
			return count;
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x0015CE04 File Offset: 0x0015B004
		public virtual Rectangle GetTankBounds()
		{
			Rectangle sourceRect = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetSourceRect(0, null);
			int height = sourceRect.Height / 16;
			int width = sourceRect.Width / 16;
			Rectangle tank_rect = new Rectangle((int)this.TileLocation.X * 64, (int)((this.TileLocation.Y - (float)this.getTilesHigh() - 1f) * 64f), width * 64, height * 64);
			tank_rect.X += 4;
			tank_rect.Width -= 8;
			if (base.QualifiedItemId == "(F)CCFishTank")
			{
				tank_rect.X += 24;
				tank_rect.Width -= 76;
			}
			tank_rect.Height -= 28;
			tank_rect.Y += 64;
			tank_rect.Height -= 64;
			return tank_rect;
		}

		// Token: 0x04001294 RID: 4756
		public const int TANK_DEPTH = 10;

		// Token: 0x04001295 RID: 4757
		public const int FLOOR_DECORATION_OFFSET = 4;

		// Token: 0x04001296 RID: 4758
		public const int TANK_SORT_REGION = 20;

		// Token: 0x04001297 RID: 4759
		[XmlIgnore]
		public List<Vector4> bubbles = new List<Vector4>();

		// Token: 0x04001298 RID: 4760
		[XmlIgnore]
		public List<TankFish> tankFish = new List<TankFish>();

		// Token: 0x04001299 RID: 4761
		[XmlIgnore]
		public NetEvent0 refreshFishEvent = new NetEvent0(false);

		// Token: 0x0400129A RID: 4762
		[XmlIgnore]
		public bool fishDirty = true;

		// Token: 0x0400129B RID: 4763
		[XmlIgnore]
		private Texture2D _aquariumTexture;

		// Token: 0x0400129C RID: 4764
		[XmlIgnore]
		public List<KeyValuePair<Rectangle, Vector2>?> floorDecorations = new List<KeyValuePair<Rectangle, Vector2>?>();

		// Token: 0x0400129D RID: 4765
		[XmlIgnore]
		public List<Vector2> decorationSlots = new List<Vector2>();

		// Token: 0x0400129E RID: 4766
		[XmlIgnore]
		public List<int> floorDecorationIndices = new List<int>();

		// Token: 0x0400129F RID: 4767
		public NetInt generationSeed = new NetInt();

		// Token: 0x040012A0 RID: 4768
		[XmlIgnore]
		public Item localDepositedItem;

		// Token: 0x040012A1 RID: 4769
		[XmlIgnore]
		protected int _currentDecorationIndex;

		// Token: 0x040012A2 RID: 4770
		protected Dictionary<Item, TankFish> _fishLookup = new Dictionary<Item, TankFish>();

		// Token: 0x02000554 RID: 1364
		public enum FishTankCategories
		{
			// Token: 0x04002B3E RID: 11070
			None,
			// Token: 0x04002B3F RID: 11071
			Swim,
			// Token: 0x04002B40 RID: 11072
			Ground,
			// Token: 0x04002B41 RID: 11073
			Decoration
		}
	}
}
