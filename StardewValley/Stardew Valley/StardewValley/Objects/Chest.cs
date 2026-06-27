using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Objects
{
	// Token: 0x020001A5 RID: 421
	public class Chest : Object
	{
		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06001DC6 RID: 7622 RVA: 0x00154900 File Offset: 0x00152B00
		private ChestHitTimer HitTimerInstance
		{
			get
			{
				if (this.hitTimerInstance != null)
				{
					return this.hitTimerInstance;
				}
				this.hitTimerInstance = new ChestHitTimer();
				if (Game1.IsMasterGame || this.Location == null)
				{
					return this.hitTimerInstance;
				}
				Dictionary<ulong, ChestHitTimer> localTimers;
				if (!Game1.player.team.chestHit.SavedTimers.TryGetValue(this.Location.NameOrUniqueName, out localTimers))
				{
					return this.hitTimerInstance;
				}
				ulong tileHash = ChestHitSynchronizer.HashPosition((int)this.TileLocation.X, (int)this.TileLocation.Y);
				ChestHitTimer timer;
				if (localTimers.TryGetValue(tileHash, out timer))
				{
					this.hitTimerInstance = timer;
					localTimers.Remove(tileHash);
					if (timer.SavedTime >= 0 && Game1.currentGameTime != null)
					{
						timer.Milliseconds -= (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - timer.SavedTime;
						timer.SavedTime = -1;
					}
				}
				return this.hitTimerInstance;
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06001DC7 RID: 7623 RVA: 0x001549E8 File Offset: 0x00152BE8
		// (set) Token: 0x06001DC8 RID: 7624 RVA: 0x001549F5 File Offset: 0x00152BF5
		[XmlIgnore]
		public Chest.SpecialChestTypes SpecialChestType
		{
			get
			{
				return this.specialChestType.Value;
			}
			set
			{
				this.specialChestType.Value = value;
			}
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06001DC9 RID: 7625 RVA: 0x00154A03 File Offset: 0x00152C03
		// (set) Token: 0x06001DCA RID: 7626 RVA: 0x00154A10 File Offset: 0x00152C10
		[XmlIgnore]
		public string GlobalInventoryId
		{
			get
			{
				return this.globalInventoryId.Value;
			}
			set
			{
				this.globalInventoryId.Value = value;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06001DCB RID: 7627 RVA: 0x00154A1E File Offset: 0x00152C1E
		// (set) Token: 0x06001DCC RID: 7628 RVA: 0x00154A2B File Offset: 0x00152C2B
		[XmlIgnore]
		public Color Tint
		{
			get
			{
				return this.tint.Value;
			}
			set
			{
				this.tint.Value = value;
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06001DCD RID: 7629 RVA: 0x00154A39 File Offset: 0x00152C39
		[XmlIgnore]
		public Inventory Items
		{
			get
			{
				return this.netItems.Value;
			}
		}

		// Token: 0x06001DCE RID: 7630 RVA: 0x00154A48 File Offset: 0x00152C48
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.startingLidFrame, "startingLidFrame").AddField(this.frameCounter, "frameCounter").AddField(this.netItems, "netItems").AddField(this.tint, "tint").AddField(this.playerChoiceColor, "playerChoiceColor").AddField(this.playerChest, "playerChest").AddField(this.fridge, "fridge").AddField(this.giftbox, "giftbox").AddField(this.giftboxIndex, "giftboxIndex").AddField(this.giftboxIsStarterGift, "giftboxIsStarterGift").AddField(this.mutex.NetFields, "mutex.NetFields").AddField(this.lidFrameCount, "lidFrameCount").AddField(this.bigCraftableSpriteIndex, "bigCraftableSpriteIndex").AddField(this.dropContents, "dropContents").AddField(this.openChestEvent.NetFields, "openChestEvent.NetFields").AddField(this.synchronized, "synchronized").AddField(this.specialChestType, "specialChestType").AddField(this.kickStartTile, "kickStartTile").AddField(this.separateWalletItems, "separateWalletItems").AddField(this.globalInventoryId, "globalInventoryId");
			this.openChestEvent.onEvent += this.performOpenChest;
			this.kickStartTile.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 old_value, Vector2 new_value)
			{
				if (Game1.gameMode == 6)
				{
					return;
				}
				if (new_value.X != -1000f && new_value.Y != -1000f)
				{
					this.localKickStartTile = new Vector2?(this.kickStartTile.Value);
					this.kickProgress = 0f;
				}
			};
		}

		// Token: 0x06001DCF RID: 7631 RVA: 0x00154BDC File Offset: 0x00152DDC
		public Chest()
		{
			this.Name = "Chest";
			this.type.Value = "interactive";
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x00154D1C File Offset: 0x00152F1C
		public Chest(bool playerChest, Vector2 tileLocation, string itemId = "130") : base(tileLocation, itemId, false)
		{
			this.Name = "Chest";
			this.type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				this.startingLidFrame.Value = base.ParentSheetIndex + 1;
				this.bigCraftable.Value = true;
				this.canBeSetDown.Value = true;
			}
			else
			{
				this.lidFrameCount.Value = 3;
			}
			this.SetSpecialChestType();
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x00154EAC File Offset: 0x001530AC
		public Chest(bool playerChest, string itemId = "130") : base(Vector2.Zero, itemId, false)
		{
			this.Name = "Chest";
			this.type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				this.startingLidFrame.Value = base.ParentSheetIndex + 1;
				this.bigCraftable.Value = true;
				this.canBeSetDown.Value = true;
				return;
			}
			this.lidFrameCount.Value = 3;
		}

		// Token: 0x06001DD2 RID: 7634 RVA: 0x00155038 File Offset: 0x00153238
		public Chest(string itemId, Vector2 tile_location, int starting_lid_frame, int lid_frame_count) : base(tile_location, itemId, false)
		{
			this.playerChest.Value = true;
			this.startingLidFrame.Value = starting_lid_frame;
			this.lidFrameCount.Value = lid_frame_count;
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x0015519C File Offset: 0x0015339C
		public Chest(List<Item> items, Vector2 location, bool giftbox = false, int giftboxIndex = 0, bool giftboxIsStarterGift = false)
		{
			base.name = "Chest";
			this.type.Value = "interactive";
			this.giftbox.Value = giftbox;
			this.giftboxIndex.Value = giftboxIndex;
			this.giftboxIsStarterGift.Value = giftboxIsStarterGift;
			if (!this.giftbox.Value)
			{
				this.lidFrameCount.Value = 3;
			}
			if (items != null)
			{
				this.Items.OverwriteWith(items);
			}
			this.TileLocation = location;
		}

		// Token: 0x06001DD4 RID: 7636 RVA: 0x0015532F File Offset: 0x0015352F
		public void resetLidFrame()
		{
			this.currentLidFrame = this.startingLidFrame.Value;
		}

		// Token: 0x06001DD5 RID: 7637 RVA: 0x00155344 File Offset: 0x00153544
		public void fixLidFrame()
		{
			if (this.currentLidFrame == 0)
			{
				this.currentLidFrame = this.startingLidFrame.Value;
			}
			if (this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				return;
			}
			if (this.playerChest.Value)
			{
				if (this.GetMutex().IsLocked() && !this.GetMutex().IsLockHeld())
				{
					this.currentLidFrame = this.getLastLidFrame();
					return;
				}
				if (!this.GetMutex().IsLocked())
				{
					this.currentLidFrame = this.startingLidFrame.Value;
					return;
				}
			}
			else if (this.currentLidFrame == this.startingLidFrame.Value && this.GetMutex().IsLocked() && !this.GetMutex().IsLockHeld())
			{
				this.currentLidFrame = this.getLastLidFrame();
			}
		}

		// Token: 0x06001DD6 RID: 7638 RVA: 0x00155400 File Offset: 0x00153600
		public int getLastLidFrame()
		{
			return this.startingLidFrame.Value + this.lidFrameCount.Value - 1;
		}

		// Token: 0x06001DD7 RID: 7639 RVA: 0x0015541C File Offset: 0x0015361C
		public void HandleChestHit(ChestHitArgs args)
		{
			if (!Game1.IsMasterGame)
			{
				Game1.log.Warn("Attempted to call Chest::HandleChestHit as a farmhand.");
				return;
			}
			if (this.TileLocation.X == 0f && this.TileLocation.Y == 0f)
			{
				this.TileLocation = Utility.PointToVector2(args.ChestTile);
			}
			this.GetMutex().RequestLock(delegate
			{
				this.clearNulls();
				if (this.isEmpty())
				{
					this.performRemoveAction();
					if (this.Location.Objects.Remove(Utility.PointToVector2(args.ChestTile)) && this.Type == "Crafting" && this.fragility.Value != 2)
					{
						this.Location.debris.Add(new Debris(this.QualifiedItemId, args.ToolPosition, Utility.PointToVector2(args.StandingPixel)));
					}
					Game1.player.team.chestHit.SignalDelete(this.Location, args.ChestTile.X, args.ChestTile.Y);
				}
				else if (args.ToolCanHit)
				{
					if (args.HoldDownClick || args.RecentlyHit)
					{
						if (this.kickStartTile.Value == this.TileLocation)
						{
							this.kickStartTile.Value = new Vector2(-1000f, -1000f);
						}
						this.TryMoveToSafePosition(new int?(args.Direction));
						Game1.player.team.chestHit.SignalMove(this.Location, args.ChestTile.X, args.ChestTile.Y, (int)this.TileLocation.X, (int)this.TileLocation.Y);
					}
					else
					{
						this.kickStartTile.Value = this.TileLocation;
					}
				}
				this.GetMutex().ReleaseLock();
			}, null);
		}

		// Token: 0x06001DD8 RID: 7640 RVA: 0x001554A8 File Offset: 0x001536A8
		public override bool performToolAction(Tool t)
		{
			if (((t != null) ? t.getLastFarmerToUse() : null) != null && t.getLastFarmerToUse() != Game1.player)
			{
				return false;
			}
			if (this.playerChest.Value)
			{
				if (t == null)
				{
					return false;
				}
				if (t is MeleeWeapon || !t.isHeavyHitter())
				{
					return false;
				}
				if (base.performToolAction(t))
				{
					GameLocation location = this.Location;
					Farmer player = t.getLastFarmerToUse();
					if (player != null)
					{
						Vector2 c = this.TileLocation;
						if (c.X == 0f && c.Y == 0f)
						{
							bool found = false;
							foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
							{
								if (pair.Value == this)
								{
									c.X = (float)((int)pair.Key.X);
									c.Y = (float)((int)pair.Key.Y);
									found = true;
									break;
								}
							}
							if (!found)
							{
								c = player.GetToolLocation(false) / 64f;
								c.X = (float)((int)c.X);
								c.Y = (float)((int)c.Y);
							}
						}
						if (!this.GetMutex().IsLocked())
						{
							ChestHitArgs args = new ChestHitArgs();
							args.Location = location;
							args.ChestTile = new Point((int)this.TileLocation.X, (int)this.TileLocation.Y);
							args.ToolPosition = player.GetToolLocation(false);
							args.StandingPixel = player.StandingPixel;
							args.Direction = player.FacingDirection;
							args.HoldDownClick = (t != player.CurrentTool);
							args.ToolCanHit = (t.isHeavyHitter() && !(t is MeleeWeapon));
							args.RecentlyHit = (this.HitTimerInstance.Milliseconds > 0);
							if (args.ToolCanHit)
							{
								this.shakeTimer = 100;
								this.HitTimerInstance.Milliseconds = 10000;
							}
							if (args.ChestTile.X == 0 && args.ChestTile.Y == 0)
							{
								if (location.getObjectAtTile((int)c.X, (int)c.Y, false) != this)
								{
									return false;
								}
								args.ChestTile = new Point((int)c.X, (int)c.Y);
							}
							Game1.player.team.chestHit.Sync(args);
						}
					}
				}
				return false;
			}
			else
			{
				if (t is Pickaxe && this.currentLidFrame == this.getLastLidFrame() && this.frameCounter.Value == -1 && this.isEmpty())
				{
					this.Location.playSound("woodWhack", null, null, SoundContext.Default);
					for (int i = 0; i < 8; i++)
					{
						Game1.multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, this.tileLocation.Value * 64f + new Vector2(32f, 64f), false, Game1.random.NextDouble() < 0.5, (this.tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 8f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 64f, false)
							{
								motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, (float)Game1.random.Next(-11, -8)),
								acceleration = new Vector2(0f, 0.3f)
							}
						});
					}
					Game1.createRadialDebris(this.Location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 7), false, -1, false, new Color?(new Color(204, 132, 87)));
					return true;
				}
				return false;
			}
		}

		// Token: 0x06001DD9 RID: 7641 RVA: 0x00155948 File Offset: 0x00153B48
		public bool TryMoveToSafePosition(int? preferDirection = null)
		{
			Chest.<>c__DisplayClass53_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.location = this.Location;
			Vector2? prioritizeDirection;
			if (preferDirection != null)
			{
				switch (preferDirection.GetValueOrDefault())
				{
				case 0:
					prioritizeDirection = new Vector2?(new Vector2(0f, -1f));
					goto IL_9C;
				case 1:
					prioritizeDirection = new Vector2?(new Vector2(1f, 0f));
					goto IL_9C;
				case 3:
					prioritizeDirection = new Vector2?(new Vector2(-1f, 0f));
					goto IL_9C;
				}
			}
			prioritizeDirection = new Vector2?(new Vector2(0f, 1f));
			IL_9C:
			return this.<TryMoveToSafePosition>g__TryMoveRecursively|53_0(this.tileLocation.Value, 0, prioritizeDirection, ref CS$<>8__locals1);
		}

		// Token: 0x06001DDA RID: 7642 RVA: 0x00155A06 File Offset: 0x00153C06
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			this.localKickStartTile = null;
			this.kickProgress = -1f;
			return base.placementAction(location, x, y, who);
		}

		// Token: 0x06001DDB RID: 7643 RVA: 0x00155A2C File Offset: 0x00153C2C
		public void SetSpecialChestType()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(BC)BigChest" || qualifiedItemId == "(BC)BigStoneChest")
			{
				this.SpecialChestType = Chest.SpecialChestTypes.BigChest;
				return;
			}
			if (qualifiedItemId == "(BC)248")
			{
				this.SpecialChestType = Chest.SpecialChestTypes.MiniShippingBin;
				return;
			}
			if (qualifiedItemId == "(BC)256")
			{
				this.SpecialChestType = Chest.SpecialChestTypes.JunimoChest;
				return;
			}
			if (!(qualifiedItemId == "(BC)275"))
			{
				return;
			}
			this.SpecialChestType = Chest.SpecialChestTypes.AutoLoader;
		}

		// Token: 0x06001DDC RID: 7644 RVA: 0x00155AA4 File Offset: 0x00153CA4
		public void destroyAndDropContents(Vector2 pointToDropAt)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			List<Item> item_list = new List<Item>();
			item_list.AddRange(this.Items);
			if (this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				foreach (Inventory separate_wallet_item_list in this.separateWalletItems.Values)
				{
					item_list.AddRange(separate_wallet_item_list);
				}
			}
			if (item_list.Count > 0)
			{
				location.playSound("throwDownITem", null, null, SoundContext.Default);
			}
			foreach (Item item in item_list)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location, -1, false);
				}
			}
			this.Items.Clear();
			this.separateWalletItems.Clear();
			this.clearNulls();
		}

		// Token: 0x06001DDD RID: 7645 RVA: 0x00155BC0 File Offset: 0x00153DC0
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			if (dropInItem != null && dropInItem.QualifiedItemId != base.QualifiedItemId && dropInItem.HasContextTag("swappable_chest") && base.HasContextTag("swappable_chest") && this.Location != null)
			{
				if (!probe)
				{
					if (this.GetMutex().IsLocked())
					{
						return false;
					}
					Chest newChest = new Chest(true, this.TileLocation, dropInItem.ItemId);
					int newCapacity = newChest.GetActualCapacity();
					if (newCapacity < this.GetActualCapacity() && newCapacity < this.Items.CountItemStacks())
					{
						return false;
					}
					if (newCapacity < this.Items.Count)
					{
						this.clearNulls();
					}
					newChest.netItems.Value = this.netItems.Value;
					newChest.playerChoiceColor.Value = this.playerChoiceColor.Value;
					newChest.Tint = this.Tint;
					newChest.modData.CopyFrom(base.modData);
					GameLocation location = this.Location;
					location.Objects.Remove(this.TileLocation);
					location.Objects.Add(this.TileLocation, newChest);
					Game1.createMultipleItemDebris(ItemRegistry.Create(base.QualifiedItemId, 1, 0, false), this.TileLocation * 64f + new Vector2(32f), -1, null, -1, false);
					this.Location.playSound("axchop", null, null, SoundContext.Default);
				}
				return true;
			}
			return base.performObjectDropInAction(dropInItem, probe, who, false);
		}

		// Token: 0x06001DDE RID: 7646 RVA: 0x00155D48 File Offset: 0x00153F48
		public void dumpContents()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			IInventory items = this.Items;
			if (this.synchronized.Value && (this.GetMutex().IsLocked() || !Game1.IsMasterGame) && !this.GetMutex().IsLockHeld())
			{
				return;
			}
			if (items.Count > 0 && (this.GetMutex().IsLockHeld() || !this.playerChest.Value))
			{
				if (this.giftbox.Value && this.giftboxIsStarterGift.Value)
				{
					FarmHouse house = location as FarmHouse;
					if (house != null)
					{
						if (!house.IsOwnedByCurrentPlayer)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Objects:ParsnipSeedPackage_SomeoneElse"));
							return;
						}
						Game1.player.addQuest((Game1.GetFarmTypeID() == "MeadowlandsFarm") ? "132" : "6");
						Game1.dayTimeMoneyBox.PingQuestLog();
					}
				}
				foreach (Item item in items)
				{
					if (item != null)
					{
						item.SetTempData<bool>("FromStarterGiftBox", true);
						if (item.QualifiedItemId == "(O)434")
						{
							if (Game1.player.mailReceived.Add((location is FarmHouse) ? "CF_Spouse" : "CF_Mines"))
							{
								Game1.player.eatObject(items[0] as Object, true);
							}
						}
						else if (this.dropContents.Value)
						{
							Game1.createItemDebris(item, this.tileLocation.Value * 64f, -1, location, -1, false);
							if (location is VolcanoDungeon)
							{
								int value = this.bigCraftableSpriteIndex.Value;
								if (value != 223)
								{
									if (value == 227)
									{
										Game1.player.team.RequestLimitedNutDrops("VolcanoRareChest", location, (int)this.tileLocation.Value.X * 64, (int)this.tileLocation.Value.Y * 64, 1, 1);
									}
								}
								else
								{
									Game1.player.team.RequestLimitedNutDrops("VolcanoNormalChest", location, (int)this.tileLocation.Value.X * 64, (int)this.tileLocation.Value.Y * 64, 1, 1);
								}
							}
						}
						else if (!this.synchronized.Value || this.GetMutex().IsLockHeld())
						{
							item.onDetachedFromParent();
							ItemGrabMenu grabMenu3 = Game1.activeClickableMenu as ItemGrabMenu;
							if (grabMenu3 != null)
							{
								grabMenu3.ItemsToGrabMenu.actualInventory.Add(item);
							}
							else
							{
								Game1.player.addItemByMenuIfNecessaryElseHoldUp(item, null, false);
							}
							if (this.mailToAddOnItemDump != null)
							{
								Game1.player.mailReceived.Add(this.mailToAddOnItemDump);
							}
							if (location is Caldera || Game1.player.currentLocation is Caldera)
							{
								Game1.player.mailReceived.Add("CalderaTreasure");
							}
						}
					}
				}
				items.Clear();
				this.clearNulls();
				MineShaft mine = Game1.mine;
				if (mine != null)
				{
					mine.chestConsumed();
				}
				IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
				ItemGrabMenu grabMenu = activeClickableMenu as ItemGrabMenu;
				if (grabMenu != null)
				{
					ItemGrabMenu grabMenu2 = grabMenu;
					grabMenu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(grabMenu2.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu _)
					{
						grabMenu.DropRemainingItems();
					}));
				}
			}
			Game1.player.gainExperience(5, 25 + Game1.CurrentMineLevel);
			if (this.giftbox.Value)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Microsoft.Xna.Framework.Rectangle(0, this.giftboxIndex.Value * 32, 16, 32), 80f, 11, 1, this.tileLocation.Value * 64f - new Vector2(0f, 52f), false, false, this.tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					destroyable = false,
					holdLastFrame = true
				};
				Object tileObj;
				if (location.netObjects.TryGetValue(this.tileLocation.Value, out tileObj) && tileObj == this)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						sprite
					});
					location.removeObject(this.tileLocation.Value, false);
					return;
				}
				location.temporarySprites.Add(sprite);
			}
		}

		// Token: 0x06001DDF RID: 7647 RVA: 0x001561E8 File Offset: 0x001543E8
		public NetMutex GetMutex()
		{
			if (this.GlobalInventoryId != null)
			{
				return Game1.player.team.GetOrCreateGlobalInventoryMutex(this.GlobalInventoryId);
			}
			if (this.specialChestType.Value == Chest.SpecialChestTypes.JunimoChest)
			{
				return Game1.player.team.GetOrCreateGlobalInventoryMutex("JunimoChests");
			}
			return this.mutex;
		}

		// Token: 0x06001DE0 RID: 7648 RVA: 0x0015623C File Offset: 0x0015443C
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			GameLocation location = this.Location;
			IInventory items = this.GetItemsForPlayer();
			if (this.giftbox.Value)
			{
				Game1.player.Halt();
				Game1.player.freezePause = 1000;
				location.playSound("Ship", null, null, SoundContext.Default);
				this.dumpContents();
			}
			else if (this.playerChest.Value)
			{
				if (!Game1.didPlayerJustRightClick(true))
				{
					return false;
				}
				this.GetMutex().RequestLock(delegate
				{
					if (this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
					{
						this.OpenMiniShippingMenu();
						return;
					}
					this.frameCounter.Value = 5;
					Game1.playSound(this.fridge.Value ? "doorCreak" : "openChest", null);
					Game1.player.Halt();
					Game1.player.freezePause = 1000;
				}, null);
			}
			else if (!this.playerChest.Value)
			{
				if (this.currentLidFrame == this.startingLidFrame.Value && this.frameCounter.Value <= -1)
				{
					location.playSound("openChest", null, null, SoundContext.Default);
					if (this.synchronized.Value)
					{
						this.GetMutex().RequestLock(new Action(this.openChestEvent.Fire), null);
					}
					else
					{
						this.performOpenChest();
					}
				}
				else if (this.currentLidFrame == this.getLastLidFrame() && items.Count > 0 && !this.synchronized.Value)
				{
					Item item = items[0];
					items.RemoveAt(0);
					if (Game1.mine != null)
					{
						Game1.mine.chestConsumed();
					}
					who.addItemByMenuIfNecessaryElseHoldUp(item, null, false);
					IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
					ItemGrabMenu grab_menu = activeClickableMenu as ItemGrabMenu;
					if (grab_menu != null)
					{
						ItemGrabMenu grab_menu2 = grab_menu;
						grab_menu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(grab_menu2.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu menu)
						{
							grab_menu.DropRemainingItems();
						}));
					}
				}
			}
			if (items.Count == 0 && (!this.playerChest.Value || this.giftbox.Value))
			{
				location.removeObject(this.TileLocation, false);
				location.playSound("woodWhack", null, null, SoundContext.Default);
				for (int i = 0; i < 8; i++)
				{
					Game1.multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, this.tileLocation.Value * 64f + new Vector2(32f, 64f), false, Game1.random.NextDouble() < 0.5, (this.tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 8f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 64f, false)
						{
							motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, (float)Game1.random.Next(-11, -8)),
							acceleration = new Vector2(0f, 0.3f)
						}
					});
				}
				Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 7), false, -1, false, new Color?(new Color(204, 132, 87)));
			}
			return true;
		}

		// Token: 0x06001DE1 RID: 7649 RVA: 0x00156618 File Offset: 0x00154818
		public virtual void OpenMiniShippingMenu()
		{
			Game1.playSound("shwip", null);
			this.ShowMenu();
		}

		// Token: 0x06001DE2 RID: 7650 RVA: 0x0015663F File Offset: 0x0015483F
		public virtual void performOpenChest()
		{
			this.frameCounter.Value = 5;
		}

		// Token: 0x06001DE3 RID: 7651 RVA: 0x0015664D File Offset: 0x0015484D
		public virtual void grabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				this.GetItemsForPlayer().Remove(item);
				this.clearNulls();
				this.ShowMenu();
			}
		}

		// Token: 0x06001DE4 RID: 7652 RVA: 0x00156674 File Offset: 0x00154874
		public virtual Item addItem(Item item)
		{
			item.resetState();
			this.clearNulls();
			IInventory item_list = this.GetItemsForPlayer();
			for (int i = 0; i < item_list.Count; i++)
			{
				if (item_list[i] != null && item_list[i].canStackWith(item))
				{
					int toRemove = item.Stack - item_list[i].addToStack(item);
					if (item.ConsumeStack(toRemove) == null)
					{
						return null;
					}
				}
			}
			if (item_list.Count < this.GetActualCapacity())
			{
				item_list.Add(item);
				return null;
			}
			return item;
		}

		// Token: 0x06001DE5 RID: 7653 RVA: 0x001566F8 File Offset: 0x001548F8
		public virtual int GetActualCapacity()
		{
			switch (this.SpecialChestType)
			{
			case Chest.SpecialChestTypes.MiniShippingBin:
			case Chest.SpecialChestTypes.JunimoChest:
				return 9;
			case Chest.SpecialChestTypes.Enricher:
				return 1;
			case Chest.SpecialChestTypes.BigChest:
				return 70;
			}
			return 36;
		}

		// Token: 0x06001DE6 RID: 7654 RVA: 0x00156738 File Offset: 0x00154938
		public virtual void CheckAutoLoad(Farmer who)
		{
			GameLocation location = this.Location;
			Vector2 tile = this.TileLocation;
			if (location != null)
			{
				Object beneath_object;
				if (!location.objects.TryGetValue(new Vector2(tile.X, tile.Y + 1f), out beneath_object))
				{
					return;
				}
				if (beneath_object != null)
				{
					beneath_object.AttemptAutoLoad(who);
				}
			}
		}

		// Token: 0x06001DE7 RID: 7655 RVA: 0x00156788 File Offset: 0x00154988
		public virtual void ShowMenu()
		{
			ItemGrabMenu oldMenu = Game1.activeClickableMenu as ItemGrabMenu;
			switch (this.SpecialChestType)
			{
			case Chest.SpecialChestTypes.MiniShippingBin:
				Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), false, true, new InventoryMenu.highlightThisItem(Utility.highlightShippableObjects), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, false, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				break;
			case Chest.SpecialChestTypes.JunimoChest:
				Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				break;
			case Chest.SpecialChestTypes.AutoLoader:
			{
				ItemGrabMenu itemGrabMenu;
				IClickableMenu activeClickableMenu = itemGrabMenu = new ItemGrabMenu(this.GetItemsForPlayer(), false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				itemGrabMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(itemGrabMenu.exitFunction, new IClickableMenu.onExit(delegate()
				{
					this.CheckAutoLoad(Game1.player);
				}));
				Game1.activeClickableMenu = activeClickableMenu;
				break;
			}
			case Chest.SpecialChestTypes.Enricher:
				Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), false, true, new InventoryMenu.highlightThisItem(Object.HighlightFertilizers), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				break;
			default:
				Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				break;
			}
			if (oldMenu != null)
			{
				ItemGrabMenu newMenu = Game1.activeClickableMenu as ItemGrabMenu;
				if (newMenu != null)
				{
					newMenu.inventory.moveItemSound = oldMenu.inventory.moveItemSound;
					newMenu.inventory.highlightMethod = oldMenu.inventory.highlightMethod;
				}
			}
		}

		// Token: 0x06001DE8 RID: 7656 RVA: 0x00156988 File Offset: 0x00154B88
		public virtual void grabItemFromInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item tmp = this.addItem(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				tmp = who.addItemToInventory(tmp);
			}
			this.clearNulls();
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
			this.ShowMenu();
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06001DE9 RID: 7657 RVA: 0x00156A1E File Offset: 0x00154C1E
		public IInventory GetItemsForPlayer()
		{
			return this.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x06001DEA RID: 7658 RVA: 0x00156A30 File Offset: 0x00154C30
		public IInventory GetItemsForPlayer(long id)
		{
			if (this.GlobalInventoryId != null)
			{
				return Game1.player.team.GetOrCreateGlobalInventory(this.GlobalInventoryId);
			}
			Chest.SpecialChestTypes specialChestTypes = this.SpecialChestType;
			if (specialChestTypes != Chest.SpecialChestTypes.MiniShippingBin)
			{
				if (specialChestTypes == Chest.SpecialChestTypes.JunimoChest)
				{
					return Game1.player.team.GetOrCreateGlobalInventory("JunimoChests");
				}
			}
			else if (Game1.player.team.useSeparateWallets.Value && this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				Inventory items;
				if (!this.separateWalletItems.TryGetValue(id, out items))
				{
					items = (this.separateWalletItems[id] = new Inventory());
				}
				return items;
			}
			return this.Items;
		}

		// Token: 0x06001DEB RID: 7659 RVA: 0x00156AE4 File Offset: 0x00154CE4
		public virtual bool isEmpty()
		{
			if (this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				using (NetDictionary<long, Inventory, NetRef<Inventory>, SerializableDictionary<long, Inventory>, NetLongDictionary<Inventory, NetRef<Inventory>>>.ValuesCollection.Enumerator enumerator = this.separateWalletItems.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.HasAny())
						{
							return false;
						}
					}
				}
				return true;
			}
			return !this.GetItemsForPlayer().HasAny();
		}

		// Token: 0x06001DEC RID: 7660 RVA: 0x00156B74 File Offset: 0x00154D74
		public virtual void clearNulls()
		{
			this.GetItemsForPlayer().RemoveEmptySlots();
		}

		// Token: 0x06001DED RID: 7661 RVA: 0x00156B84 File Offset: 0x00154D84
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (environment == null)
			{
				return;
			}
			if (this.synchronized.Value)
			{
				this.openChestEvent.Poll();
			}
			if (this.localKickStartTile != null)
			{
				if (Game1.currentLocation == environment)
				{
					if (this.kickProgress == 0f)
					{
						if (Utility.isOnScreen((this.localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
						{
							Game1.playSound("clubhit", null);
						}
						this.shakeTimer = 100;
					}
				}
				else
				{
					this.localKickStartTile = null;
					this.kickProgress = -1f;
				}
				if (this.kickProgress >= 0f)
				{
					float move_duration = 0.25f;
					this.kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / (double)move_duration);
					if (this.kickProgress >= 1f)
					{
						this.kickProgress = -1f;
						this.localKickStartTile = null;
					}
				}
			}
			else
			{
				this.kickProgress = -1f;
			}
			this.fixLidFrame();
			this.mutex.Update(environment);
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shakeTimer <= 0)
				{
					this.health = 10;
				}
			}
			ChestHitTimer chestHitTimer = this.hitTimerInstance;
			if (chestHitTimer != null)
			{
				chestHitTimer.Update(time);
			}
			if (this.playerChest.Value)
			{
				if (this.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
				{
					this.UpdateFarmerNearby(true);
					if (this._shippingBinFrameCounter > -1)
					{
						this._shippingBinFrameCounter--;
						if (this._shippingBinFrameCounter <= 0)
						{
							this._shippingBinFrameCounter = 5;
							if (this._farmerNearby && this.currentLidFrame < this.getLastLidFrame())
							{
								this.currentLidFrame++;
							}
							else if (!this._farmerNearby && this.currentLidFrame > this.startingLidFrame.Value)
							{
								this.currentLidFrame--;
							}
							else
							{
								this._shippingBinFrameCounter = -1;
							}
						}
					}
					if (Game1.activeClickableMenu == null && this.GetMutex().IsLockHeld())
					{
						this.GetMutex().ReleaseLock();
						return;
					}
				}
				else if (this.frameCounter.Value > -1 && this.currentLidFrame < this.getLastLidFrame() + 1)
				{
					NetInt netInt = this.frameCounter;
					int value = netInt.Value;
					netInt.Value = value - 1;
					if (this.frameCounter.Value <= 0 && this.GetMutex().IsLockHeld())
					{
						if (this.currentLidFrame == this.getLastLidFrame())
						{
							this.ShowMenu();
							this.frameCounter.Value = -1;
							return;
						}
						this.frameCounter.Value = 5;
						this.currentLidFrame++;
						return;
					}
				}
				else if (((this.frameCounter.Value == -1 && this.currentLidFrame > this.startingLidFrame.Value) || this.currentLidFrame >= this.getLastLidFrame()) && Game1.activeClickableMenu == null && this.GetMutex().IsLockHeld())
				{
					this.GetMutex().ReleaseLock();
					this.currentLidFrame = this.getLastLidFrame();
					this.frameCounter.Value = 2;
					environment.localSound("doorCreakReverse", null, null, SoundContext.Default);
					return;
				}
			}
			else if (this.frameCounter.Value > -1 && this.currentLidFrame <= this.getLastLidFrame())
			{
				NetInt netInt2 = this.frameCounter;
				int value = netInt2.Value;
				netInt2.Value = value - 1;
				if (this.frameCounter.Value <= 0)
				{
					if (this.currentLidFrame == this.getLastLidFrame())
					{
						this.dumpContents();
						this.frameCounter.Value = -1;
						return;
					}
					this.frameCounter.Value = 10;
					this.currentLidFrame++;
					if (this.currentLidFrame == this.getLastLidFrame())
					{
						this.frameCounter.Value += 5;
					}
				}
			}
		}

		// Token: 0x06001DEE RID: 7662 RVA: 0x00156F8C File Offset: 0x0015518C
		public virtual void UpdateFarmerNearby(bool animate = true)
		{
			GameLocation location = this.Location;
			bool should_open = false;
			Vector2 curTile = this.tileLocation.Value;
			foreach (Farmer farmer in location.farmers)
			{
				Point playerTile = farmer.TilePoint;
				if (Math.Abs((float)playerTile.X - curTile.X) <= 1f && Math.Abs((float)playerTile.Y - curTile.Y) <= 1f)
				{
					should_open = true;
					break;
				}
			}
			if (should_open != this._farmerNearby)
			{
				this._farmerNearby = should_open;
				this._shippingBinFrameCounter = 5;
				if (!animate)
				{
					this._shippingBinFrameCounter = -1;
					if (this._farmerNearby)
					{
						this.currentLidFrame = this.getLastLidFrame();
						return;
					}
					this.currentLidFrame = this.startingLidFrame.Value;
					return;
				}
				else if (Game1.gameMode != 6)
				{
					if (this._farmerNearby)
					{
						location.localSound("doorCreak", null, null, SoundContext.Default);
						return;
					}
					location.localSound("doorCreakReverse", null, null, SoundContext.Default);
				}
			}
		}

		// Token: 0x06001DEF RID: 7663 RVA: 0x001570CC File Offset: 0x001552CC
		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			this.fixLidFrame();
			if (this.specialChestType.Value == Chest.SpecialChestTypes.MiniShippingBin)
			{
				this.UpdateFarmerNearby(false);
			}
			this.kickProgress = -1f;
			this.localKickStartTile = null;
			if (!this.playerChest.Value && this.GetItemsForPlayer().Count == 0)
			{
				this.currentLidFrame = this.getLastLidFrame();
			}
		}

		// Token: 0x06001DF0 RID: 7664 RVA: 0x00157137 File Offset: 0x00155337
		public virtual void SetBigCraftableSpriteIndex(int sprite_index, int starting_lid_frame = -1, int lid_frame_count = 3)
		{
			this.bigCraftableSpriteIndex.Value = sprite_index;
			if (starting_lid_frame >= 0)
			{
				this.startingLidFrame.Value = starting_lid_frame;
			}
			else
			{
				this.startingLidFrame.Value = sprite_index + 1;
			}
			this.lidFrameCount.Value = lid_frame_count;
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x00157174 File Offset: 0x00155374
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			float draw_x = (float)x;
			float draw_y = (float)y;
			if (this.localKickStartTile != null)
			{
				draw_x = Utility.Lerp(this.localKickStartTile.Value.X, draw_x, this.kickProgress);
				draw_y = Utility.Lerp(this.localKickStartTile.Value.Y, draw_y, this.kickProgress);
			}
			float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
			if (this.localKickStartTile != null)
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.Black * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
				draw_y -= (float)Math.Sin((double)this.kickProgress * 3.141592653589793) * 0.5f;
			}
			if (this.playerChest.Value && (base.QualifiedItemId == "(BC)130" || base.QualifiedItemId == "(BC)232" || base.QualifiedItemId.Equals("(BC)BigChest") || base.QualifiedItemId.Equals("(BC)BigStoneChest")))
			{
				if (this.playerChoiceColor.Value.Equals(Color.Black))
				{
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
					Texture2D texture = itemData.GetTexture();
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(0, null)), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(0, new int?(this.currentLidFrame))), this.tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
					return;
				}
				ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Texture2D texture2 = itemData2.GetTexture();
				int spriteIndex = base.ParentSheetIndex;
				int lidIndex = this.currentLidFrame + 8;
				int coloredLidIndex = this.currentLidFrame;
				string qualifiedItemId = base.QualifiedItemId;
				if (!(qualifiedItemId == "(BC)130"))
				{
					if (qualifiedItemId == "(BC)BigChest")
					{
						spriteIndex = 312;
						lidIndex = this.currentLidFrame + 16;
						coloredLidIndex = this.currentLidFrame + 8;
					}
				}
				else
				{
					spriteIndex = 168;
					lidIndex = this.currentLidFrame + 46;
					coloredLidIndex = this.currentLidFrame + 38;
				}
				Microsoft.Xna.Framework.Rectangle drawRect = itemData2.GetSourceRect(0, new int?(spriteIndex));
				Microsoft.Xna.Framework.Rectangle lidRect = itemData2.GetSourceRect(0, new int?(lidIndex));
				Microsoft.Xna.Framework.Rectangle coloredLidRect = itemData2.GetSourceRect(0, new int?(coloredLidIndex));
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle?(drawRect), this.playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + 20f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, spriteIndex / 8 * 32 + 53, 16, 11)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle?(lidRect), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
				spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle?(coloredLidRect), this.playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			else
			{
				if (this.playerChest.Value)
				{
					ParsedItemData itemData3 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
					Texture2D texture3 = itemData3.GetTexture();
					spriteBatch.Draw(texture3, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(itemData3.GetSourceRect(0, null)), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
					spriteBatch.Draw(texture3, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(itemData3.GetSourceRect(0, new int?(this.currentLidFrame))), this.tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
					return;
				}
				if (this.giftbox.Value)
				{
					spriteBatch.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
					if (this.GetItemsForPlayer().Count > 0)
					{
						int textureY = this.giftboxIndex.Value * 32;
						spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), draw_y * 64f - 52f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, textureY, 16, 32)), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
						return;
					}
				}
				else
				{
					int sprite_index = 500;
					Texture2D sprite_sheet = Game1.objectSpriteSheet;
					int sprite_sheet_height = 16;
					int y_offset = 0;
					if (this.bigCraftableSpriteIndex.Value >= 0)
					{
						sprite_index = this.bigCraftableSpriteIndex.Value;
						sprite_sheet = Game1.bigCraftableSpriteSheet;
						sprite_sheet_height = 32;
						y_offset = -64;
					}
					if (this.bigCraftableSpriteIndex.Value < 0)
					{
						spriteBatch.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
					}
					spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset)), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(sprite_sheet, sprite_index, 16, sprite_sheet_height)), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
					Vector2 lidPosition = new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset);
					if (this.bigCraftableSpriteIndex.Value < 0)
					{
						switch (this.currentLidFrame)
						{
						case 501:
							lidPosition.Y -= 32f;
							break;
						case 502:
							lidPosition.Y -= 40f;
							break;
						case 503:
							lidPosition.Y -= 60f;
							break;
						}
					}
					spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(sprite_sheet, this.currentLidFrame, 16, sprite_sheet_height)), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				}
				return;
			}
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x00157B34 File Offset: 0x00155D34
		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
		{
			if (this.playerChest.Value)
			{
				if (this.playerChoiceColor.Equals(Color.Black))
				{
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
					spriteBatch.Draw(itemData.GetTexture(), local ? new Vector2((float)x, (float)(y - 64)) : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)((y - 1) * 64))), new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(0, null)), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
					return;
				}
				ParsedItemData itemData2 = ItemRegistry.GetData(base.QualifiedItemId);
				if (itemData2 != null)
				{
					int drawIndex = base.ParentSheetIndex;
					int overlayIndex = this.currentLidFrame + 8;
					int coloredLidIndex = this.currentLidFrame;
					string qualifiedItemId = base.QualifiedItemId;
					if (!(qualifiedItemId == "(BC)130"))
					{
						if (!(qualifiedItemId == "(BC)BigChest"))
						{
							if (qualifiedItemId == "(BC)BigStoneChest")
							{
								overlayIndex = this.currentLidFrame + 8;
								coloredLidIndex = this.currentLidFrame;
							}
						}
						else
						{
							drawIndex = 312;
							overlayIndex = this.currentLidFrame + 16;
							coloredLidIndex = this.currentLidFrame + 8;
						}
					}
					else
					{
						drawIndex = 168;
						overlayIndex = this.currentLidFrame + 46;
						coloredLidIndex = this.currentLidFrame + 38;
					}
					Microsoft.Xna.Framework.Rectangle drawRect = itemData2.GetSourceRect(0, new int?(drawIndex));
					Microsoft.Xna.Framework.Rectangle lidRect = itemData2.GetSourceRect(0, new int?(overlayIndex));
					Microsoft.Xna.Framework.Rectangle coloredLidRect = itemData2.GetSourceRect(0, new int?(coloredLidIndex));
					Texture2D texture = itemData2.GetTexture();
					spriteBatch.Draw(texture, local ? new Vector2((float)x, (float)(y - 64)) : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y - 1) * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(drawRect), this.playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
					spriteBatch.Draw(texture, local ? new Vector2((float)x, (float)(y - 64)) : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y - 1) * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(coloredLidRect), this.playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
					spriteBatch.Draw(texture, local ? new Vector2((float)x, (float)(y + 20)) : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 + 20))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, drawIndex / 8 * 32 + 53, 16, 11)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
					spriteBatch.Draw(texture, local ? new Vector2((float)x, (float)(y - 64)) : Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y - 1) * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(lidRect), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
				}
			}
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x00157F27 File Offset: 0x00156127
		public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToList<Item>(this.Items, handler, getPath, false, null);
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x00157F98 File Offset: 0x00156198
		[CompilerGenerated]
		private bool <TryMoveToSafePosition>g__TryMoveRecursively|53_0(Vector2 tile_position, int depth, Vector2? prioritize_direction, ref Chest.<>c__DisplayClass53_0 A_4)
		{
			List<Vector2> offsets = new List<Vector2>();
			offsets.AddRange(new Vector2[]
			{
				new Vector2(1f, 0f),
				new Vector2(-1f, 0f),
				new Vector2(0f, -1f),
				new Vector2(0f, 1f)
			});
			Utility.Shuffle<Vector2>(Game1.random, offsets);
			if (prioritize_direction != null)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			foreach (Vector2 offset in offsets)
			{
				Vector2 new_position = tile_position + offset;
				if (this.canBePlacedHere(A_4.location, new_position, CollisionMask.All, false) && A_4.location.CanItemBePlacedHere(new_position, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					if (!A_4.location.objects.ContainsKey(new_position) && A_4.location.objects.Remove(this.TileLocation))
					{
						this.kickStartTile.Value = this.TileLocation;
						this.TileLocation = new_position;
						A_4.location.objects[new_position] = this;
					}
					return true;
				}
			}
			Utility.Shuffle<Vector2>(Game1.random, offsets);
			if (prioritize_direction != null)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			if (depth < 3)
			{
				foreach (Vector2 offset2 in offsets)
				{
					Vector2 new_position2 = tile_position + offset2;
					if (A_4.location.isPointPassable(new Location((int)(new_position2.X + 0.5f) * 64, (int)(new_position2.Y + 0.5f) * 64), Game1.viewport) && this.<TryMoveToSafePosition>g__TryMoveRecursively|53_0(new_position2, depth + 1, prioritize_direction, ref A_4))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0400125C RID: 4700
		public const int capacity = 36;

		// Token: 0x0400125D RID: 4701
		internal ChestHitTimer hitTimerInstance;

		// Token: 0x0400125E RID: 4702
		[XmlElement("currentLidFrame")]
		public readonly NetInt startingLidFrame = new NetInt(501);

		// Token: 0x0400125F RID: 4703
		public readonly NetInt lidFrameCount = new NetInt(5);

		// Token: 0x04001260 RID: 4704
		private int currentLidFrame;

		// Token: 0x04001261 RID: 4705
		[XmlElement("frameCounter")]
		public readonly NetInt frameCounter = new NetInt(-1);

		// Token: 0x04001262 RID: 4706
		[XmlElement("items")]
		public NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

		// Token: 0x04001263 RID: 4707
		public readonly NetLongDictionary<Inventory, NetRef<Inventory>> separateWalletItems = new NetLongDictionary<Inventory, NetRef<Inventory>>();

		// Token: 0x04001264 RID: 4708
		[XmlElement("tint")]
		public readonly NetColor tint = new NetColor(Color.White);

		// Token: 0x04001265 RID: 4709
		[XmlElement("playerChoiceColor")]
		public readonly NetColor playerChoiceColor = new NetColor(Color.Black);

		// Token: 0x04001266 RID: 4710
		[XmlElement("playerChest")]
		public readonly NetBool playerChest = new NetBool();

		// Token: 0x04001267 RID: 4711
		[XmlElement("fridge")]
		public readonly NetBool fridge = new NetBool();

		// Token: 0x04001268 RID: 4712
		[XmlElement("giftbox")]
		public readonly NetBool giftbox = new NetBool();

		// Token: 0x04001269 RID: 4713
		[XmlElement("giftboxIndex")]
		public readonly NetInt giftboxIndex = new NetInt();

		// Token: 0x0400126A RID: 4714
		public readonly NetBool giftboxIsStarterGift = new NetBool();

		// Token: 0x0400126B RID: 4715
		[XmlElement("spriteIndexOverride")]
		public readonly NetInt bigCraftableSpriteIndex = new NetInt(-1);

		// Token: 0x0400126C RID: 4716
		[XmlElement("dropContents")]
		public readonly NetBool dropContents = new NetBool(false);

		// Token: 0x0400126D RID: 4717
		[XmlIgnore]
		public string mailToAddOnItemDump;

		// Token: 0x0400126E RID: 4718
		[XmlElement("synchronized")]
		public readonly NetBool synchronized = new NetBool(false);

		// Token: 0x0400126F RID: 4719
		[XmlIgnore]
		public int _shippingBinFrameCounter;

		// Token: 0x04001270 RID: 4720
		[XmlIgnore]
		public bool _farmerNearby;

		// Token: 0x04001271 RID: 4721
		[XmlIgnore]
		public NetVector2 kickStartTile = new NetVector2(new Vector2(-1000f, -1000f));

		// Token: 0x04001272 RID: 4722
		[XmlIgnore]
		public Vector2? localKickStartTile;

		// Token: 0x04001273 RID: 4723
		[XmlIgnore]
		public float kickProgress = -1f;

		// Token: 0x04001274 RID: 4724
		[XmlIgnore]
		public readonly NetEvent0 openChestEvent = new NetEvent0(false);

		// Token: 0x04001275 RID: 4725
		[XmlElement("specialChestType")]
		public readonly NetEnum<Chest.SpecialChestTypes> specialChestType = new NetEnum<Chest.SpecialChestTypes>();

		// Token: 0x04001276 RID: 4726
		public readonly NetString globalInventoryId = new NetString();

		// Token: 0x04001277 RID: 4727
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		// Token: 0x0200054E RID: 1358
		public enum SpecialChestTypes
		{
			// Token: 0x04002B2D RID: 11053
			None,
			// Token: 0x04002B2E RID: 11054
			MiniShippingBin,
			// Token: 0x04002B2F RID: 11055
			JunimoChest,
			// Token: 0x04002B30 RID: 11056
			AutoLoader,
			// Token: 0x04002B31 RID: 11057
			Enricher,
			// Token: 0x04002B32 RID: 11058
			[Obsolete("This value is only used in mobile versions of the game.")]
			Mill,
			// Token: 0x04002B33 RID: 11059
			BigChest
		}
	}
}
