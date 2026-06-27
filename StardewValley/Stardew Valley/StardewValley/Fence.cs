using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData.Fences;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x020000ED RID: 237
	public class Fence : Object
	{
		// Token: 0x06001290 RID: 4752 RVA: 0x000DBDC8 File Offset: 0x000D9FC8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.health, "health").AddField(this.maxHealth, "maxHealth").AddField(this.gatePosition, "gatePosition").AddField(this.isGate, "isGate").AddField(this.repairQueued, "repairQueued");
			this.itemId.fieldChangeVisibleEvent += delegate(NetString field, string oldValue, string newValue)
			{
				this.OnIdChanged();
			};
			this.isGate.fieldChangeVisibleEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				this.OnIdChanged();
			};
		}

		// Token: 0x06001291 RID: 4753 RVA: 0x000DBE60 File Offset: 0x000DA060
		public Fence(Vector2 tileLocation, string itemId, bool isGate) : base(itemId, 1, false, -1, 0)
		{
			if (Fence.fenceDrawGuide == null)
			{
				Fence.populateFenceDrawGuide();
			}
			base.Type = "Crafting";
			this.isGate.Value = isGate;
			this.TileLocation = tileLocation;
			this.canBeSetDown.Value = true;
			this.canBeGrabbed.Value = true;
			this.price.Value = 1;
			this.ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
			if (isGate)
			{
				this.health.Value *= 2f;
			}
			this.OnIdChanged();
		}

		// Token: 0x06001292 RID: 4754 RVA: 0x000DBF38 File Offset: 0x000DA138
		public Fence() : this(Vector2.Zero, "322", false)
		{
		}

		// Token: 0x06001293 RID: 4755 RVA: 0x000DBF4C File Offset: 0x000DA14C
		public virtual void ResetHealth(float amount_adjustment)
		{
			FenceData data = this.GetData();
			float base_health = (float)((data != null) ? data.Health : 100);
			if (this.isGate.Value)
			{
				amount_adjustment = 0f;
			}
			this.health.Value = base_health + amount_adjustment;
			this.health.Value *= 2f;
			this.maxHealth.Value = this.health.Value;
		}

		// Token: 0x06001294 RID: 4756 RVA: 0x000DBFC0 File Offset: 0x000DA1C0
		protected override void MigrateLegacyItemId()
		{
			switch (this.obsolete_whichType.GetValueOrDefault(1))
			{
			case 2:
				base.ItemId = "323";
				break;
			case 3:
				base.ItemId = "324";
				break;
			case 4:
				base.ItemId = "325";
				break;
			case 5:
				base.ItemId = "298";
				break;
			default:
				base.ItemId = "322";
				break;
			}
			this.obsolete_whichType = null;
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x000DC03F File Offset: 0x000DA23F
		protected virtual void OnIdChanged()
		{
			if (this.fenceTexture == null || this.fenceTexture.IsValueCreated)
			{
				this.fenceTexture = new Lazy<Texture2D>(new Func<Texture2D>(this.loadFenceTexture));
			}
			this._data = null;
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x000DC075 File Offset: 0x000DA275
		public virtual void repair()
		{
			this.ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x000DC094 File Offset: 0x000DA294
		public static void populateFenceDrawGuide()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			dictionary[0] = 5;
			dictionary[10] = 9;
			dictionary[100] = 10;
			dictionary[1000] = 3;
			dictionary[500] = 5;
			dictionary[1010] = 8;
			dictionary[1100] = 6;
			dictionary[1500] = 3;
			dictionary[600] = 0;
			dictionary[510] = 2;
			dictionary[110] = 7;
			dictionary[1600] = 0;
			dictionary[1610] = 4;
			dictionary[1510] = 2;
			dictionary[1110] = 7;
			dictionary[610] = 4;
			Fence.fenceDrawGuide = dictionary;
		}

		// Token: 0x06001298 RID: 4760 RVA: 0x000DC160 File Offset: 0x000DA360
		public virtual void PerformRepairIfNecessary()
		{
			if (Game1.IsMasterGame && this.repairQueued.Value)
			{
				this.ResetHealth(this.GetRepairHealthAdjustment());
				this.repairQueued.Value = false;
			}
		}

		// Token: 0x06001299 RID: 4761 RVA: 0x000DC190 File Offset: 0x000DA390
		public override void updateWhenCurrentLocation(GameTime time)
		{
			this.PerformRepairIfNecessary();
			int gatePosition = this.gatePosition.Get();
			gatePosition += this.gateMotion;
			if (gatePosition == 88)
			{
				int drawSum = this.getDrawSum();
				if (drawSum != 110 && drawSum != 1500 && drawSum != 1000 && drawSum != 500 && drawSum != 100 && drawSum != 10)
				{
					this.toggleGate(Game1.player, false, false);
				}
			}
			this.gatePosition.Set(gatePosition);
			if (gatePosition >= 88 || gatePosition <= 0)
			{
				this.gateMotion = 0;
			}
			Object @object = this.heldObject.Get();
			if (@object == null)
			{
				return;
			}
			@object.updateWhenCurrentLocation(time);
		}

		// Token: 0x0600129A RID: 4762 RVA: 0x000DC22A File Offset: 0x000DA42A
		public static Dictionary<string, FenceData> GetFenceLookup()
		{
			if (Fence._FenceLookup == null)
			{
				Fence._LoadFenceData();
			}
			return Fence._FenceLookup;
		}

		// Token: 0x0600129B RID: 4763 RVA: 0x000DC23D File Offset: 0x000DA43D
		public FenceData GetData()
		{
			if (this._data == null)
			{
				Fence.TryGetData(base.ItemId, out this._data);
			}
			return this._data;
		}

		// Token: 0x0600129C RID: 4764 RVA: 0x000DC25F File Offset: 0x000DA45F
		public static bool TryGetData(string itemId, out FenceData data)
		{
			if (itemId == null)
			{
				data = null;
				return false;
			}
			return Fence.GetFenceLookup().TryGetValue(itemId, out data);
		}

		// Token: 0x0600129D RID: 4765 RVA: 0x000DC275 File Offset: 0x000DA475
		protected static void _LoadFenceData()
		{
			Fence._FenceLookup = DataLoader.Fences(Game1.content);
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x000DC288 File Offset: 0x000DA488
		public int getDrawSum()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return 0;
			}
			int drawSum = 0;
			Vector2 surroundingLocations = this.tileLocation.Value;
			surroundingLocations.X += 1f;
			Object rightObj;
			if (location.objects.TryGetValue(surroundingLocations, out rightObj))
			{
				Fence rightFence = rightObj as Fence;
				if (rightFence != null && rightFence.countsForDrawing(base.ItemId))
				{
					drawSum += 100;
				}
			}
			surroundingLocations.X -= 2f;
			Object leftObj;
			if (location.objects.TryGetValue(surroundingLocations, out leftObj))
			{
				Fence leftFence = leftObj as Fence;
				if (leftFence != null && leftFence.countsForDrawing(base.ItemId))
				{
					drawSum += 10;
				}
			}
			surroundingLocations.X += 1f;
			surroundingLocations.Y += 1f;
			Object downObj;
			if (location.objects.TryGetValue(surroundingLocations, out downObj))
			{
				Fence downFence = downObj as Fence;
				if (downFence != null && downFence.countsForDrawing(base.ItemId))
				{
					drawSum += 500;
				}
			}
			surroundingLocations.Y -= 2f;
			Object upObj;
			if (location.objects.TryGetValue(surroundingLocations, out upObj))
			{
				Fence upFence = upObj as Fence;
				if (upFence != null && upFence.countsForDrawing(base.ItemId))
				{
					drawSum += 1000;
				}
			}
			return drawSum;
		}

		// Token: 0x0600129F RID: 4767 RVA: 0x000DC3CC File Offset: 0x000DA5CC
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (!justCheckingForActivity && who != null)
			{
				Point playerTile = who.TilePoint;
				Object upObj;
				Object downObj;
				Object rightObj;
				Object leftObj;
				if (location.objects.TryGetValue(new Vector2((float)playerTile.X, (float)(playerTile.Y - 1)), out upObj) && location.objects.TryGetValue(new Vector2((float)playerTile.X, (float)(playerTile.Y + 1)), out downObj) && location.objects.TryGetValue(new Vector2((float)(playerTile.X + 1), (float)playerTile.Y), out rightObj) && location.objects.TryGetValue(new Vector2((float)(playerTile.X - 1), (float)playerTile.Y), out leftObj) && !upObj.isPassable() && !downObj.isPassable() && !leftObj.isPassable() && !rightObj.isPassable())
				{
					this.performToolAction(null);
				}
			}
			if (this.health.Value <= 1f)
			{
				return false;
			}
			if (this.isGate.Value)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				if (this.isGate.Value)
				{
					this.toggleGate(who, this.gatePosition.Value == 0, false);
				}
				return true;
			}
			else
			{
				if (justCheckingForActivity)
				{
					return false;
				}
				foreach (Vector2 v in Utility.getAdjacentTileLocations(this.tileLocation.Value))
				{
					Object obj;
					if (location.objects.TryGetValue(v, out obj))
					{
						Fence fence = obj as Fence;
						if (fence != null && fence.isGate.Value)
						{
							fence.checkForAction(who, false);
							return true;
						}
					}
				}
				return this.health.Value <= 0f;
			}
		}

		// Token: 0x060012A0 RID: 4768 RVA: 0x000DC5A4 File Offset: 0x000DA7A4
		public virtual void toggleGate(bool open, bool is_toggling_counterpart = false, Farmer who = null)
		{
			if (this.health.Value <= 1f)
			{
				return;
			}
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			int drawSum = this.getDrawSum();
			if (drawSum == 110 || drawSum == 1500 || drawSum == 1000 || drawSum == 500 || drawSum == 100 || drawSum == 10)
			{
				if (who != null)
				{
					who.TemporaryPassableTiles.Add(new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64));
				}
				if (open)
				{
					this.gatePosition.Value = 88;
				}
				else
				{
					this.gatePosition.Value = 0;
				}
				if (!is_toggling_counterpart && location != null)
				{
					location.playSound("doorClose", null, null, SoundContext.Default);
				}
			}
			else
			{
				if (who != null)
				{
					who.TemporaryPassableTiles.Add(new Rectangle((int)this.tileLocation.X * 64, (int)this.tileLocation.Y * 64, 64, 64));
				}
				this.gatePosition.Value = 0;
			}
			if (!is_toggling_counterpart)
			{
				if (drawSum <= 100)
				{
					if (drawSum != 10)
					{
						if (drawSum != 100)
						{
							return;
						}
						Vector2 neighborTile = this.tileLocation.Value + new Vector2(-1f, 0f);
						Object neighbor;
						if (location.objects.TryGetValue(neighborTile, out neighbor))
						{
							Fence fence = neighbor as Fence;
							if (fence != null && fence.isGate.Value && fence.getDrawSum() == 10)
							{
								fence.toggleGate(this.gatePosition.Value != 0, true, who);
								return;
							}
						}
					}
					else
					{
						Vector2 neighborTile2 = this.tileLocation.Value + new Vector2(1f, 0f);
						Object neighbor2;
						if (location.objects.TryGetValue(neighborTile2, out neighbor2))
						{
							Fence fence2 = neighbor2 as Fence;
							if (fence2 != null && fence2.isGate.Value && fence2.getDrawSum() == 100)
							{
								fence2.toggleGate(this.gatePosition.Value != 0, true, who);
								return;
							}
						}
					}
				}
				else if (drawSum != 500)
				{
					if (drawSum != 1000)
					{
						return;
					}
					Vector2 neighborTile3 = this.tileLocation.Value + new Vector2(0f, 1f);
					Object neighbor3;
					if (location.objects.TryGetValue(neighborTile3, out neighbor3))
					{
						Fence fence3 = neighbor3 as Fence;
						if (fence3 != null && fence3.isGate.Value && fence3.getDrawSum() == 500)
						{
							fence3.toggleGate(this.gatePosition.Value != 0, true, who);
							return;
						}
					}
				}
				else
				{
					Vector2 neighborTile4 = this.tileLocation.Value + new Vector2(0f, -1f);
					Object neighbor4;
					if (location.objects.TryGetValue(neighborTile4, out neighbor4))
					{
						Fence fence4 = neighbor4 as Fence;
						if (fence4 != null && fence4.isGate.Value && fence4.getDrawSum() == 1000)
						{
							fence4.toggleGate(this.gatePosition.Value != 0, true, who);
						}
					}
				}
			}
		}

		// Token: 0x060012A1 RID: 4769 RVA: 0x000DC8D0 File Offset: 0x000DAAD0
		public void toggleGate(Farmer who, bool open, bool is_toggling_counterpart = false)
		{
			this.toggleGate(open, is_toggling_counterpart, who);
		}

		// Token: 0x060012A2 RID: 4770 RVA: 0x000DC8DB File Offset: 0x000DAADB
		public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
		{
			location.debris.Add(new Debris(base.ItemId, origin, destination));
		}

		// Token: 0x060012A3 RID: 4771 RVA: 0x000DC8F8 File Offset: 0x000DAAF8
		public override bool performToolAction(Tool t)
		{
			GameLocation location = this.Location;
			if (this.heldObject.Value != null && t != null && !(t is MeleeWeapon) && t.isHeavyHitter())
			{
				Item value = this.heldObject.Value;
				this.heldObject.Value.performRemoveAction();
				this.heldObject.Value = null;
				Game1.createItemDebris(value.getOne(), this.TileLocation * 64f, -1, null, -1, false);
				base.playNearbySoundAll("axchop", null, SoundContext.Default);
			}
			else if (this.isGate.Value && (t is Axe || t is Pickaxe))
			{
				base.playNearbySoundAll("axchop", null, SoundContext.Default);
				Game1.createObjectDebris("(O)325", (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.player.UniqueMultiplayerID, location);
				location.objects.Remove(this.tileLocation.Value);
				Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, 6, false, -1, false, null);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0)
				});
			}
			else if (!this.isGate.Value && this.IsValidRemovalTool(t))
			{
				FenceData data = this.GetData();
				string text;
				if ((text = ((data != null) ? data.RemovalSound : null)) == null)
				{
					text = (((data != null) ? data.PlacementSound : null) ?? "hammer");
				}
				string sound = text;
				int removalDebrisType = (data != null) ? data.RemovalDebrisType : 14;
				base.playNearbySoundAll(sound, null, SoundContext.Default);
				location.objects.Remove(this.tileLocation.Value);
				for (int i = 0; i < 4; i++)
				{
					location.temporarySprites.Add(new CosmeticDebris(this.fenceTexture.Value, new Vector2(this.tileLocation.X * 64f + 32f, this.tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((this.tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, Game1.soundBank.GetCue("shiny4"), null, 0, 200));
				}
				Game1.createRadialDebris(location, removalDebrisType, (int)this.tileLocation.X, (int)this.tileLocation.Y, 6, false, -1, false, null);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
				});
				if (this.maxHealth.Value - this.health.Value < 0.5f)
				{
					location.debris.Add(new Debris(new Object(base.ItemId, 1, false, -1, 0), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
			}
			return false;
		}

		// Token: 0x060012A4 RID: 4772 RVA: 0x000DCD20 File Offset: 0x000DAF20
		public virtual bool IsValidRemovalTool(Tool tool)
		{
			if (tool == null)
			{
				return !this.isGate.Value;
			}
			FenceData data = this.GetData();
			List<string> removalToolIds = (data != null) ? data.RemovalToolIds : null;
			List<string> removalToolTypes = (data != null) ? data.RemovalToolTypes : null;
			bool allowAnyTool = true;
			if (removalToolIds != null && removalToolIds.Count > 0)
			{
				allowAnyTool = false;
				string toolName = tool.Name;
				foreach (string requiredName in removalToolIds)
				{
					if (toolName == requiredName)
					{
						return true;
					}
				}
			}
			if (removalToolTypes != null && removalToolTypes.Count > 0)
			{
				allowAnyTool = false;
				string toolType = tool.GetType().FullName;
				foreach (string requiredType in removalToolTypes)
				{
					if (toolType == requiredType)
					{
						return true;
					}
				}
			}
			return allowAnyTool;
		}

		// Token: 0x060012A5 RID: 4773 RVA: 0x000DCE2C File Offset: 0x000DB02C
		public override bool minutesElapsed(int minutes)
		{
			if (!Game1.IsMasterGame)
			{
				return false;
			}
			this.PerformRepairIfNecessary();
			if (!Game1.IsBuildingConstructed("Gold Clock") || Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
			{
				this.health.Value -= (float)minutes / 1440f;
				if (this.health.Value <= -1f && (Game1.timeOfDay <= 610 || Game1.timeOfDay > 1800))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x000DCEB4 File Offset: 0x000DB0B4
		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			if (this.heldObject.Value != null)
			{
				this.heldObject.Value.TileLocation = this.tileLocation.Value;
				this.heldObject.Value.Location = this.Location;
				this.heldObject.Value.actionOnPlayerEntry();
				this.heldObject.Value.isOn.Value = true;
				this.heldObject.Value.initializeLightSource(this.tileLocation.Value, false);
			}
		}

		// Token: 0x060012A7 RID: 4775 RVA: 0x000DCF48 File Offset: 0x000DB148
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (dropInItem.HasTypeObject() && dropInItem.ItemId == "325")
			{
				if (probe)
				{
					return false;
				}
				if (!this.isGate.Value)
				{
					int drawSum = this.getDrawSum();
					if (drawSum == 1500 || drawSum == 110 || drawSum == 1000 || drawSum == 10 || drawSum == 100 || drawSum == 500)
					{
						Vector2 neighbor = default(Vector2);
						if (drawSum <= 100)
						{
							if (drawSum != 10)
							{
								if (drawSum == 100)
								{
									neighbor = this.tileLocation.Value + new Vector2(-1f, 0f);
									Fence leftFence = location.objects.GetValueOrDefault(neighbor, null) as Fence;
									if (leftFence != null && leftFence.isGate.Value)
									{
										int neighborSum = leftFence.getDrawSum();
										if (neighborSum != 10 && neighborSum != 110)
										{
											return false;
										}
									}
								}
							}
							else
							{
								neighbor = this.tileLocation.Value + new Vector2(1f, 0f);
								Fence rightFence = location.objects.GetValueOrDefault(neighbor, null) as Fence;
								if (rightFence != null && rightFence.isGate.Value)
								{
									int neighborSum2 = rightFence.getDrawSum();
									if (neighborSum2 != 100 && neighborSum2 != 110)
									{
										return false;
									}
								}
							}
						}
						else if (drawSum != 500)
						{
							if (drawSum == 1000)
							{
								neighbor = this.tileLocation.Value + new Vector2(0f, 1f);
								Fence downFence = location.objects.GetValueOrDefault(neighbor, null) as Fence;
								if (downFence != null && downFence.isGate.Value)
								{
									int neighborSum3 = downFence.getDrawSum();
									if (neighborSum3 != 500 && neighborSum3 != 1500)
									{
										return false;
									}
								}
							}
						}
						else
						{
							neighbor = this.tileLocation.Value + new Vector2(0f, -1f);
							Fence upFence = location.objects.GetValueOrDefault(neighbor, null) as Fence;
							if (upFence != null && upFence.isGate.Value)
							{
								int neighborSum4 = upFence.getDrawSum();
								if (neighborSum4 != 1000 && neighborSum4 != 1500)
								{
									return false;
								}
							}
						}
						foreach (Vector2 adjacent_tile in new Vector2[]
						{
							this.tileLocation.Value + new Vector2(1f, 0f),
							this.tileLocation.Value + new Vector2(-1f, 0f),
							this.tileLocation.Value + new Vector2(0f, -1f),
							this.tileLocation.Value + new Vector2(0f, 1f)
						})
						{
							Object adjacent;
							if (!(adjacent_tile == neighbor) && location.objects.TryGetValue(adjacent_tile, out adjacent))
							{
								Fence fence = adjacent as Fence;
								if (fence != null && fence.isGate.Value && fence.Type == base.Type)
								{
									return false;
								}
							}
						}
						if (this.heldObject.Value != null)
						{
							Item value = this.heldObject.Value;
							this.heldObject.Value.performRemoveAction();
							this.heldObject.Value = null;
							Game1.createItemDebris(value.getOne(), this.TileLocation * 64f, -1, null, -1, false);
						}
						this.isGate.Value = true;
						FenceData gateData;
						if (Fence.TryGetData("325", out gateData))
						{
							location.playSound(gateData.PlacementSound, null, null, SoundContext.Default);
						}
						return true;
					}
				}
			}
			else if (dropInItem.QualifiedItemId == "(O)93" && this.heldObject.Value == null && !this.isGate.Value)
			{
				if (!probe)
				{
					this.heldObject.Value = new Torch();
					location.playSound("axe", null, null, SoundContext.Default);
					this.heldObject.Value.Location = this.Location;
					this.heldObject.Value.initializeLightSource(this.tileLocation.Value, false);
				}
				return true;
			}
			if (this.health.Value <= 1f && !this.repairQueued.Value && this.CanRepairWithThisItem(dropInItem))
			{
				if (!probe)
				{
					string repair_sound = this.GetRepairSound();
					if (!string.IsNullOrEmpty(repair_sound))
					{
						location.playSound(repair_sound, null, null, SoundContext.Default);
					}
					this.repairQueued.Value = true;
				}
				return true;
			}
			return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
		}

		// Token: 0x060012A8 RID: 4776 RVA: 0x000DD444 File Offset: 0x000DB644
		public virtual float GetRepairHealthAdjustment()
		{
			FenceData data = this.GetData();
			if (data == null)
			{
				return 0f;
			}
			return Utility.RandomFloat(data.RepairHealthAdjustmentMinimum, data.RepairHealthAdjustmentMaximum, null);
		}

		// Token: 0x060012A9 RID: 4777 RVA: 0x000DD473 File Offset: 0x000DB673
		public virtual string GetRepairSound()
		{
			FenceData data = this.GetData();
			return ((data != null) ? data.PlacementSound : null) ?? "";
		}

		// Token: 0x060012AA RID: 4778 RVA: 0x000DD490 File Offset: 0x000DB690
		public virtual bool CanRepairWithThisItem(Item item)
		{
			return this.health.Value <= 1f && item != null && item.QualifiedItemId == base.QualifiedItemId;
		}

		// Token: 0x060012AB RID: 4779 RVA: 0x000DD4BC File Offset: 0x000DB6BC
		public override bool performDropDownAction(Farmer who)
		{
			return false;
		}

		// Token: 0x060012AC RID: 4780 RVA: 0x000DD4C0 File Offset: 0x000DB6C0
		public virtual Texture2D loadFenceTexture()
		{
			if (base.ItemId == "325")
			{
				this.isGate.Value = true;
			}
			FenceData data = this.GetData();
			if (data == null)
			{
				return ItemRegistry.RequireTypeDefinition(this.TypeDefinitionId).GetErrorTexture();
			}
			return Game1.content.Load<Texture2D>(data.Texture);
		}

		// Token: 0x060012AD RID: 4781 RVA: 0x000DD518 File Offset: 0x000DB718
		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			spriteBatch.Draw(this.fenceTexture.Value, objectPosition - new Vector2(0f, 64f), new Rectangle?(new Rectangle(5 * Fence.fencePieceWidth % this.fenceTexture.Value.Bounds.Width, 5 * Fence.fencePieceWidth / this.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(f.StandingPixel.Y + 1) / 10000f);
		}

		// Token: 0x060012AE RID: 4782 RVA: 0x000DD5C8 File Offset: 0x000DB7C8
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			location.Y -= 64f * scale;
			int drawSum = this.getDrawSum();
			int sourceRectPosition = Fence.fenceDrawGuide[drawSum];
			if (this.isGate.Value)
			{
				if (drawSum == 110)
				{
					spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle?(new Rectangle(0, 512, 88, 24)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					return;
				}
				if (drawSum == 1500)
				{
					spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle?(new Rectangle(112, 512, 16, 64)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					return;
				}
			}
			spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(32f, 32f) * scale, new Rectangle?(Game1.getArbitrarySourceRect(this.fenceTexture.Value, 64, 128, sourceRectPosition)), color * transparency, 0f, new Vector2(32f, 32f) * scale, scale, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060012AF RID: 4783 RVA: 0x000DD724 File Offset: 0x000DB924
		public bool countsForDrawing(string otherItemId)
		{
			return (this.health.Value > 1f || this.repairQueued.Value) && !this.isGate.Value && (otherItemId == base.ItemId || otherItemId == "325");
		}

		// Token: 0x060012B0 RID: 4784 RVA: 0x000DD77A File Offset: 0x000DB97A
		public override bool isPassable()
		{
			return this.isGate.Value && this.gatePosition.Value >= 88;
		}

		// Token: 0x060012B1 RID: 4785 RVA: 0x000DD7A0 File Offset: 0x000DB9A0
		public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
		{
			int sourceRectPosition = 1;
			FenceData data = this.GetData();
			if (data == null)
			{
				IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition(this.TypeDefinitionId);
				b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f)), new Rectangle?(itemType.GetErrorSourceRect()), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
				return;
			}
			if (this.health.Value > 1f || this.repairQueued.Value)
			{
				int drawSum = this.getDrawSum();
				sourceRectPosition = Fence.fenceDrawGuide[drawSum];
				if (this.isGate.Value)
				{
					Vector2 offset = new Vector2(0f, 0f);
					if (drawSum <= 110)
					{
						if (drawSum == 10)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 128))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 192, 24, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
							return;
						}
						if (drawSum == 100)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 128))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 240, 24, 48)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
							return;
						}
						if (drawSum == 110)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 64))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 128, 24, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
							return;
						}
					}
					else
					{
						if (drawSum == 500)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 320, 24, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
							return;
						}
						if (drawSum == 1000)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 288, 24, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
							return;
						}
						if (drawSum == 1500)
						{
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 16 : 0, 160, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
							b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 + 44))), new Rectangle?(new Rectangle((this.gatePosition.Value == 88) ? 16 : 0, 176, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
							return;
						}
					}
					sourceRectPosition = 17;
				}
				else if (this.heldObject.Value != null)
				{
					Vector2 offset2 = Vector2.Zero;
					offset2 += data.HeldObjectDrawOffset;
					if (drawSum != 10)
					{
						if (drawSum == 100)
						{
							offset2.X = data.LeftEndHeldObjectDrawX;
						}
					}
					else
					{
						offset2.X = data.RightEndHeldObjectDrawX;
					}
					offset2 *= 4f;
					this.heldObject.Value.draw(b, x * 64 + (int)offset2.X, y * 64 + (int)offset2.Y, (float)(y * 64 + 64) / 10000f, 1f);
				}
			}
			b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64))), new Rectangle?(new Rectangle(sourceRectPosition * Fence.fencePieceWidth % this.fenceTexture.Value.Bounds.Width, sourceRectPosition * Fence.fencePieceWidth / this.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);
		}

		// Token: 0x04000B0E RID: 2830
		public const int debrisPieces = 4;

		// Token: 0x04000B0F RID: 2831
		public static int fencePieceWidth = 16;

		// Token: 0x04000B10 RID: 2832
		public static int fencePieceHeight = 32;

		// Token: 0x04000B11 RID: 2833
		public const int gateClosedPosition = 0;

		// Token: 0x04000B12 RID: 2834
		public const int gateOpenedPosition = 88;

		// Token: 0x04000B13 RID: 2835
		public const int sourceRectForSoloGate = 17;

		// Token: 0x04000B14 RID: 2836
		public const int globalHealthMultiplier = 2;

		// Token: 0x04000B15 RID: 2837
		public const int N = 1000;

		// Token: 0x04000B16 RID: 2838
		public const int E = 100;

		// Token: 0x04000B17 RID: 2839
		public const int S = 500;

		// Token: 0x04000B18 RID: 2840
		public const int W = 10;

		// Token: 0x04000B19 RID: 2841
		public const string woodFenceId = "322";

		// Token: 0x04000B1A RID: 2842
		public const string stoneFenceId = "323";

		// Token: 0x04000B1B RID: 2843
		public const string ironFenceId = "324";

		// Token: 0x04000B1C RID: 2844
		public const string hardwoodFenceId = "298";

		// Token: 0x04000B1D RID: 2845
		public const string gateId = "325";

		// Token: 0x04000B1E RID: 2846
		[XmlIgnore]
		public Lazy<Texture2D> fenceTexture;

		// Token: 0x04000B1F RID: 2847
		public static Dictionary<int, int> fenceDrawGuide;

		// Token: 0x04000B20 RID: 2848
		[XmlElement("health")]
		public new readonly NetFloat health = new NetFloat();

		// Token: 0x04000B21 RID: 2849
		[XmlElement("maxHealth")]
		public readonly NetFloat maxHealth = new NetFloat();

		// Token: 0x04000B22 RID: 2850
		[XmlElement("whichType")]
		public int? obsolete_whichType;

		// Token: 0x04000B23 RID: 2851
		[XmlElement("gatePosition")]
		public readonly NetInt gatePosition = new NetInt();

		// Token: 0x04000B24 RID: 2852
		public int gateMotion;

		// Token: 0x04000B25 RID: 2853
		[XmlElement("isGate")]
		public readonly NetBool isGate = new NetBool();

		// Token: 0x04000B26 RID: 2854
		[XmlIgnore]
		public readonly NetBool repairQueued = new NetBool();

		// Token: 0x04000B27 RID: 2855
		protected static Dictionary<string, FenceData> _FenceLookup;

		// Token: 0x04000B28 RID: 2856
		protected FenceData _data;
	}
}
