using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Buildings
{
	// Token: 0x02000381 RID: 897
	public class FishPond : Building
	{
		// Token: 0x06003771 RID: 14193 RVA: 0x002BDD20 File Offset: 0x002BBF20
		public FishPond(Vector2 tileLocation) : base("Fish Pond", tileLocation)
		{
			this.UpdateMaximumOccupancy();
			this.fadeWhenPlayerIsBehind.Value = false;
			this.Reseed();
		}

		// Token: 0x06003772 RID: 14194 RVA: 0x002BDE31 File Offset: 0x002BC031
		public FishPond() : this(Vector2.Zero)
		{
		}

		// Token: 0x06003773 RID: 14195 RVA: 0x002BDE40 File Offset: 0x002BC040
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.fishType, "fishType").AddField(this.output, "output").AddField(this.daysSinceSpawn, "daysSinceSpawn").AddField(this.lastUnlockedPopulationGate, "lastUnlockedPopulationGate").AddField(this.animateHappyFishEvent, "animateHappyFishEvent").AddField(this.hasCompletedRequest, "hasCompletedRequest").AddField(this.goldenAnimalCracker, "goldenAnimalCracker").AddField(this.isPlayingGoldenCrackerAnimation, "isPlayingGoldenCrackerAnimation").AddField(this.neededItem, "neededItem").AddField(this.seedOffset, "seedOffset").AddField(this.hasSpawnedFish, "hasSpawnedFish").AddField(this.needsMutex.NetFields, "needsMutex.NetFields").AddField(this.neededItemCount, "neededItemCount").AddField(this.overrideWaterColor, "overrideWaterColor").AddField(this.sign, "sign").AddField(this.nettingStyle, "nettingStyle");
			this.animateHappyFishEvent.onEvent += this.AnimateHappyFish;
			this.fishType.fieldChangeVisibleEvent += this.OnFishTypeChanged;
		}

		// Token: 0x06003774 RID: 14196 RVA: 0x002BDF8E File Offset: 0x002BC18E
		public virtual void OnFishTypeChanged(NetString field, string old_value, string new_value)
		{
			this._fishSilhouettes.Clear();
			this._jumpingFish.Clear();
			this._fishObject = null;
		}

		// Token: 0x06003775 RID: 14197 RVA: 0x002BDFB0 File Offset: 0x002BC1B0
		public virtual void Reseed()
		{
			this.seedOffset.Value = DateTime.UtcNow.Millisecond;
		}

		// Token: 0x06003776 RID: 14198 RVA: 0x002BDFD5 File Offset: 0x002BC1D5
		public List<PondFishSilhouette> GetFishSilhouettes()
		{
			return this._fishSilhouettes;
		}

		// Token: 0x06003777 RID: 14199 RVA: 0x002BDFE0 File Offset: 0x002BC1E0
		public void UpdateMaximumOccupancy()
		{
			this.GetFishPondData();
			if (this._fishPondData != null)
			{
				if (this._fishPondData.MaxPopulation > 0)
				{
					this.maxOccupants.Value = this._fishPondData.MaxPopulation;
					return;
				}
				for (int i = 1; i <= 10; i++)
				{
					if (i <= this.lastUnlockedPopulationGate.Value)
					{
						this.maxOccupants.Set(i);
					}
					else
					{
						Dictionary<int, List<string>> populationGates = this._fishPondData.PopulationGates;
						if (((populationGates != null) ? new bool?(populationGates.ContainsKey(i)) : null) ?? false)
						{
							break;
						}
						this.maxOccupants.Set(i);
					}
				}
			}
		}

		// Token: 0x06003778 RID: 14200 RVA: 0x002BE094 File Offset: 0x002BC294
		public FishPondData GetFishPondData()
		{
			FishPondData data_entry = FishPond.GetRawData(this.fishType.Value);
			if (data_entry == null)
			{
				return null;
			}
			this._fishPondData = data_entry;
			if (this._fishPondData.SpawnTime == -1)
			{
				int value = this.GetFishObject().Price;
				if (value <= 30)
				{
					this._fishPondData.SpawnTime = 1;
				}
				else if (value <= 80)
				{
					this._fishPondData.SpawnTime = 2;
				}
				else if (value <= 120)
				{
					this._fishPondData.SpawnTime = 3;
				}
				else if (value <= 250)
				{
					this._fishPondData.SpawnTime = 4;
				}
				else
				{
					this._fishPondData.SpawnTime = 5;
				}
			}
			return this._fishPondData;
		}

		// Token: 0x06003779 RID: 14201 RVA: 0x002BE13C File Offset: 0x002BC33C
		public static FishPondData GetRawData(string itemId)
		{
			if (itemId == null)
			{
				return null;
			}
			HashSet<string> contextTags = ItemContextTagManager.GetBaseContextTags(itemId);
			if (contextTags.Contains("fish_pond_ignore"))
			{
				return null;
			}
			FishPondData selected = null;
			foreach (FishPondData data in DataLoader.FishPondData(Game1.content))
			{
				int? num = (selected != null) ? new int?(selected.Precedence) : null;
				int precedence = data.Precedence;
				if (!(num.GetValueOrDefault() <= precedence & num != null) && ItemContextTagManager.DoAllTagsMatch(data.RequiredTags, contextTags))
				{
					selected = data;
				}
			}
			return selected;
		}

		// Token: 0x0600377A RID: 14202 RVA: 0x002BE1F8 File Offset: 0x002BC3F8
		public Item GetFishProduce(Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			FishPondData data = this.GetFishPondData();
			if (data == null)
			{
				return null;
			}
			GameLocation location = base.GetParentLocation();
			Object fish = this.GetFishObject();
			FishPondReward selectedOutput = null;
			foreach (FishPondReward itemData in data.ProducedItems)
			{
				FishPondReward selectedOutput3 = selectedOutput;
				int? num = (selectedOutput3 != null) ? new int?(selectedOutput3.Precedence) : null;
				int num2 = itemData.Precedence;
				if (!(num.GetValueOrDefault() <= num2 & num != null) && this.currentOccupants.Value >= itemData.RequiredPopulation && random.NextBool(itemData.Chance) && GameStateQuery.CheckConditions(itemData.Condition, location, null, null, fish, null, null))
				{
					selectedOutput = itemData;
				}
			}
			Item item = null;
			if (selectedOutput != null)
			{
				ISpawnItemData selectedOutput2 = selectedOutput;
				GameLocation location2 = location;
				Farmer player = null;
				Random random2 = null;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
				defaultInterpolatedStringHandler.AppendLiteral("fish pond data '");
				defaultInterpolatedStringHandler.AppendFormatted(this.fishType.Value);
				defaultInterpolatedStringHandler.AppendLiteral("' > reward '");
				defaultInterpolatedStringHandler.AppendFormatted(selectedOutput.Id);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				item = ItemQueryResolver.TryResolveRandomItem(selectedOutput2, new ItemQueryContext(location2, player, random2, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, delegate(string id)
				{
					if (!(ItemRegistry.QualifyItemId(selectedOutput.ItemId) == "(O)812"))
					{
						return id;
					}
					return "FLAVORED_ITEM Roe " + fish.QualifiedItemId;
				}, fish, null);
			}
			if (item != null)
			{
				if (item.Name.Contains("Roe"))
				{
					while (random.NextDouble() < 0.2)
					{
						Item item2 = item;
						int num2 = item2.Stack;
						item2.Stack = num2 + 1;
					}
				}
				if (this.goldenAnimalCracker.Value)
				{
					item.Stack *= 2;
				}
			}
			return item;
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x0600377B RID: 14203 RVA: 0x002BE3F0 File Offset: 0x002BC5F0
		public int FishCount
		{
			get
			{
				return this.currentOccupants.Value;
			}
		}

		// Token: 0x0600377C RID: 14204 RVA: 0x002BE3FD File Offset: 0x002BC5FD
		private Item CreateFishInstance()
		{
			return new Object(this.fishType.Value, 1, false, -1, 0);
		}

		// Token: 0x0600377D RID: 14205 RVA: 0x002BE414 File Offset: 0x002BC614
		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (this.daysOfConstructionLeft.Value <= 0 && base.occupiesTile(tileLocation, false))
			{
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				if (who.ActiveObject != null && this.performActiveObjectDropInAction(who, false))
				{
					return true;
				}
				if (this.output.Value != null)
				{
					Item item = this.output.Value;
					this.output.Value = null;
					if (who.addItemToInventoryBool(item, false))
					{
						Game1.playSound("coin", null);
						int bonusExperience = 0;
						Object obj = item as Object;
						if (obj != null)
						{
							bonusExperience = (int)((float)obj.sellToStorePrice(-1L) * FishPond.HARVEST_OUTPUT_EXP_MULTIPLIER);
						}
						who.gainExperience(1, bonusExperience + FishPond.HARVEST_BASE_EXP);
					}
					else
					{
						this.output.Value = item;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					}
					return true;
				}
				if (who.ActiveObject != null && this.HasUnresolvedNeeds() && who.ActiveObject.QualifiedItemId == this.neededItem.Value.QualifiedItemId)
				{
					if (this.neededItemCount.Value == 1)
					{
						this.showObjectThrownIntoPondAnimation(who, who.ActiveObject, delegate
						{
							if (this.neededItemCount.Value <= 0)
							{
								Game1.playSound("jingle1", null);
							}
						});
					}
					else
					{
						this.showObjectThrownIntoPondAnimation(who, who.ActiveObject, null);
					}
					who.reduceActiveItemByOne();
					if (who == Game1.player)
					{
						NetIntDelta netIntDelta = this.neededItemCount;
						int value = netIntDelta.Value;
						netIntDelta.Value = value - 1;
						if (this.neededItemCount.Value <= 0)
						{
							this.needsMutex.RequestLock(delegate
							{
								this.needsMutex.ReleaseLock();
								this.ResolveNeeds(who);
							}, null);
							this.neededItemCount.Value = -1;
						}
					}
					if (this.neededItemCount.Value <= 0)
					{
						this.animateHappyFishEvent.Fire();
					}
					return true;
				}
				if (who.ActiveObject != null && (who.ActiveObject.Category == -4 || who.ActiveObject.QualifiedItemId == "(O)393" || who.ActiveObject.QualifiedItemId == "(O)397"))
				{
					if (this.fishType.Value != null)
					{
						if (!this.isLegalFishForPonds(this.fishType.Value))
						{
							string heldFishName = who.ActiveObject.DisplayName;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:CantPutInPonds", heldFishName.ToLower()));
							return true;
						}
						if (who.ActiveObject.ItemId != this.fishType.Value)
						{
							string heldFishName2 = who.ActiveObject.DisplayName;
							if (who.ActiveObject.QualifiedItemId == "(O)393" || who.ActiveObject.QualifiedItemId == "(O)397")
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishTypeCoral", heldFishName2));
							}
							else
							{
								string displayName = ItemRegistry.GetDataOrErrorItem(this.fishType.Value).DisplayName;
								if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishType", heldFishName2, displayName));
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishType", heldFishName2.ToLower(), displayName.ToLower()));
								}
							}
							return true;
						}
						if (this.currentOccupants.Value >= this.maxOccupants.Value)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PondFull"));
							return true;
						}
						return this.addFishToPond(who, who.ActiveObject);
					}
					else
					{
						if (!this.isLegalFishForPonds(who.ActiveObject.ItemId))
						{
							string heldFishName3 = who.ActiveObject.DisplayName;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:CantPutInPonds", heldFishName3));
							return true;
						}
						return this.addFishToPond(who, who.ActiveObject);
					}
				}
				else if (this.fishType.Value != null)
				{
					if (Game1.didPlayerJustRightClick(true))
					{
						Game1.playSound("bigSelect", null);
						Game1.activeClickableMenu = new PondQueryMenu(this);
						return true;
					}
				}
				else if (Game1.didPlayerJustRightClick(true))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:NoFish"));
					return true;
				}
			}
			return base.doAction(tileLocation, who);
		}

		// Token: 0x0600377E RID: 14206 RVA: 0x002BE8CD File Offset: 0x002BCACD
		public void AnimateHappyFish()
		{
			this._numberOfFishToJump = this.currentOccupants.Value;
			this._timeUntilFishHop = 1f;
		}

		// Token: 0x0600377F RID: 14207 RVA: 0x002BE8EB File Offset: 0x002BCAEB
		public Vector2 GetItemBucketTile()
		{
			return new Vector2((float)(this.tileX.Value + 4), (float)(this.tileY.Value + 4));
		}

		// Token: 0x06003780 RID: 14208 RVA: 0x002BE90E File Offset: 0x002BCB0E
		public Vector2 GetRequestTile()
		{
			return new Vector2((float)(this.tileX.Value + 2), (float)(this.tileY.Value + 2));
		}

		// Token: 0x06003781 RID: 14209 RVA: 0x002BE931 File Offset: 0x002BCB31
		public Vector2 GetCenterTile()
		{
			return new Vector2((float)(this.tileX.Value + 2), (float)(this.tileY.Value + 2));
		}

		// Token: 0x06003782 RID: 14210 RVA: 0x002BE954 File Offset: 0x002BCB54
		public void ResolveNeeds(Farmer who)
		{
			this.Reseed();
			this.hasCompletedRequest.Value = true;
			this.lastUnlockedPopulationGate.Value = this.maxOccupants.Value + 1;
			this.UpdateMaximumOccupancy();
			this.daysSinceSpawn.Value = 0;
			int bonusExperience = 0;
			FishPondData fishData = this.GetFishPondData();
			if (fishData != null)
			{
				bonusExperience = (int)((float)fishData.SpawnTime * FishPond.QUEST_SPAWNRATE_EXP_MULTIPIER);
			}
			who.gainExperience(1, bonusExperience + FishPond.QUEST_BASE_EXP);
			Random r = Utility.CreateDaySaveRandom((double)this.seedOffset.Value, 0.0, 0.0);
			Game1.showGlobalMessage(PondQueryMenu.getCompletedRequestString(this, this.GetFishObject(), r));
		}

		// Token: 0x06003783 RID: 14211 RVA: 0x002BE9FC File Offset: 0x002BCBFC
		public override void resetLocalState()
		{
			base.resetLocalState();
			this._jumpingFish.Clear();
			while (this._fishSilhouettes.Count < this.currentOccupants.Value)
			{
				PondFishSilhouette silhouette = new PondFishSilhouette(this);
				this._fishSilhouettes.Add(silhouette);
				silhouette.position = (this.GetCenterTile() + new Vector2(Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)(this.tilesWide.Value - 2), Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)(this.tilesHigh.Value - 2))) * 64f;
			}
		}

		// Token: 0x06003784 RID: 14212 RVA: 0x002BEABF File Offset: 0x002BCCBF
		private bool isLegalFishForPonds(string itemId)
		{
			return FishPond.GetRawData(itemId) != null;
		}

		// Token: 0x06003785 RID: 14213 RVA: 0x002BEACC File Offset: 0x002BCCCC
		private void showObjectThrownIntoPondAnimation(Farmer who, Object whichObject, Action callback = null)
		{
			who.faceGeneralDirection(this.GetCenterTile() * 64f + new Vector2(32f, 32f), 0, false);
			if (who.FacingDirection == 1 || who.FacingDirection == 3)
			{
				float distance = Vector2.Distance(who.Position, this.GetCenterTile() * 64f);
				float verticalDistance = this.GetCenterTile().Y * 64f + 32f - who.position.Y;
				distance -= 8f;
				float gravity = 0.0025f;
				float velocity = (float)((double)distance * Math.Sqrt((double)(gravity / (2f * (distance + 96f)))));
				float t = 2f * (velocity / gravity) + (float)((Math.Sqrt((double)(velocity * velocity + 2f * gravity * 96f)) - (double)velocity) / (double)gravity);
				t += verticalDistance;
				float xVelocityReduction = 0f;
				if (verticalDistance > 0f)
				{
					xVelocityReduction = verticalDistance / 832f;
					t += xVelocityReduction * 200f;
				}
				Game1.playSound("throwDownITem", null);
				TemporaryAnimatedSpriteList fishTossSprites = new TemporaryAnimatedSpriteList();
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(whichObject.QualifiedItemId);
				fishTossSprites.Add(new TemporaryAnimatedSprite(itemData.GetTextureName(), itemData.GetSourceRect(0, null), who.Position + new Vector2(0f, -64f), false, 0f, Color.White)
				{
					scale = 4f,
					layerDepth = 1f,
					totalNumberOfLoops = 1,
					interval = t,
					motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * (velocity - xVelocityReduction), -velocity * 3f / 2f),
					acceleration = new Vector2(0f, gravity),
					timeBasedMotion = true
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, this.GetCenterTile() * 64f, false, false)
				{
					delayBeforeAnimationStart = (int)t,
					layerDepth = (((float)this.tileY.Value + 0.5f) * 64f + 2f) / 10000f
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, this.GetCenterTile() * 64f, false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
				{
					delayBeforeAnimationStart = (int)t
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, this.GetCenterTile() * 64f + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
				{
					delayBeforeAnimationStart = (int)t
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, this.GetCenterTile() * 64f + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
				{
					delayBeforeAnimationStart = (int)t
				});
				if (who.IsLocalPlayer)
				{
					DelayedAction.playSoundAfterDelay("waterSlosh", (int)t, who.currentLocation, null, -1, false);
					if (callback != null)
					{
						DelayedAction.functionAfterDelay(callback, (int)t);
					}
				}
				if (this.fishType.Value != null && whichObject.ItemId == this.fishType.Value)
				{
					this._delayUntilFishSilhouetteAdded = t / 1000f;
				}
				Game1.multiplayer.broadcastSprites(who.currentLocation, fishTossSprites);
				return;
			}
			float distance2 = Vector2.Distance(who.Position, this.GetCenterTile() * 64f);
			float height = Math.Abs(distance2);
			if (who.FacingDirection == 0)
			{
				distance2 = -distance2;
				height += 64f;
			}
			float horizontalDistance = this.GetCenterTile().X * 64f - who.position.X;
			float gravity2 = 0.0025f;
			float velocity2 = (float)Math.Sqrt((double)(2f * gravity2 * height));
			float t2 = (float)(Math.Sqrt((double)(2f * (height - distance2) / gravity2)) + (double)(velocity2 / gravity2));
			t2 *= 1.05f;
			if (who.FacingDirection == 0)
			{
				t2 *= 0.7f;
			}
			else
			{
				t2 *= 2.5f;
			}
			t2 -= Math.Abs(horizontalDistance) / ((who.FacingDirection == 0) ? 100f : 2f);
			Game1.playSound("throwDownITem", null);
			TemporaryAnimatedSpriteList fishTossSprites2 = new TemporaryAnimatedSpriteList();
			ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(whichObject.QualifiedItemId);
			fishTossSprites2.Add(new TemporaryAnimatedSprite(itemData2.GetTextureName(), itemData2.GetSourceRect(0, null), who.Position + new Vector2(0f, -64f), false, 0f, Color.White)
			{
				scale = 4f,
				layerDepth = 1f,
				totalNumberOfLoops = 1,
				interval = t2,
				motion = new Vector2(horizontalDistance / ((who.FacingDirection == 0) ? 900f : 1000f), -velocity2),
				acceleration = new Vector2(0f, gravity2),
				timeBasedMotion = true
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, this.GetCenterTile() * 64f, false, false)
			{
				delayBeforeAnimationStart = (int)t2,
				layerDepth = (((float)this.tileY.Value + 0.5f) * 64f + 2f) / 10000f
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, this.GetCenterTile() * 64f, false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
			{
				delayBeforeAnimationStart = (int)t2
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, this.GetCenterTile() * 64f + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
			{
				delayBeforeAnimationStart = (int)t2
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, this.GetCenterTile() * 64f + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false)
			{
				delayBeforeAnimationStart = (int)t2
			});
			if (who.IsLocalPlayer)
			{
				DelayedAction.playSoundAfterDelay("waterSlosh", (int)t2, who.currentLocation, null, -1, false);
				if (callback != null)
				{
					DelayedAction.functionAfterDelay(callback, (int)t2);
				}
			}
			if (this.fishType.Value != null && whichObject.ItemId == this.fishType.Value)
			{
				this._delayUntilFishSilhouetteAdded = t2 / 1000f;
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, fishTossSprites2);
		}

		// Token: 0x06003786 RID: 14214 RVA: 0x002BF3EC File Offset: 0x002BD5EC
		private bool addFishToPond(Farmer who, Object fish)
		{
			who.reduceActiveItemByOne();
			NetInt currentOccupants = this.currentOccupants;
			int value = currentOccupants.Value;
			currentOccupants.Value = value + 1;
			if (this.currentOccupants.Value == 1)
			{
				this.fishType.Value = fish.ItemId;
				this._fishPondData = null;
				this.UpdateMaximumOccupancy();
			}
			this.showObjectThrownIntoPondAnimation(who, fish, null);
			return true;
		}

		// Token: 0x06003787 RID: 14215 RVA: 0x002BF44C File Offset: 0x002BD64C
		public override void dayUpdate(int dayOfMonth)
		{
			this.hasSpawnedFish.Value = false;
			this._hasAnimatedSpawnedFish = false;
			if (this.hasCompletedRequest.Value)
			{
				this.neededItem.Value = null;
				this.neededItemCount.Set(-1);
				this.hasCompletedRequest.Value = false;
			}
			FishPondData data = this.GetFishPondData();
			if (this.currentOccupants.Value > 0 && data != null)
			{
				Random r = Utility.CreateDaySaveRandom((double)(this.tileX.Value * 1000), (double)(this.tileY.Value * 2000), 0.0);
				if ((data.BaseMinProduceChance >= data.BaseMaxProduceChance) ? r.NextBool(data.BaseMinProduceChance) : (r.NextDouble() < (double)Utility.Lerp(data.BaseMinProduceChance, data.BaseMaxProduceChance, (float)this.currentOccupants.Value / 10f)))
				{
					this.output.Value = this.GetFishProduce(r);
				}
				NetInt netInt = this.daysSinceSpawn;
				int value = netInt.Value;
				netInt.Value = value + 1;
				if (this.daysSinceSpawn.Value > data.SpawnTime)
				{
					this.daysSinceSpawn.Value = data.SpawnTime;
				}
				if (this.daysSinceSpawn.Value >= data.SpawnTime)
				{
					string itemId;
					int count;
					if (this.TryGetNeededItemData(out itemId, out count))
					{
						if (this.currentOccupants.Value >= this.maxOccupants.Value && this.neededItem.Value == null)
						{
							this.neededItem.Value = ItemRegistry.Create(itemId, 1, 0, false);
							this.neededItemCount.Value = count;
						}
					}
					else
					{
						this.SpawnFish();
					}
				}
				if (this.currentOccupants.Value == 10 && this.fishType.Value == "717")
				{
					foreach (Farmer f in Game1.getAllFarmers())
					{
						if (f.mailReceived.Add("FullCrabPond"))
						{
							f.activeDialogueEvents["FullCrabPond"] = 14;
						}
					}
				}
				this.doFishSpecificWaterColoring();
			}
			base.dayUpdate(dayOfMonth);
		}

		// Token: 0x06003788 RID: 14216 RVA: 0x002BF688 File Offset: 0x002BD888
		private void doFishSpecificWaterColoring()
		{
			FishPondData data = this.GetFishPondData();
			Color? customColor = null;
			if (data != null)
			{
				List<FishPondWaterColor> waterColor = data.WaterColor;
				int? num = (waterColor != null) ? new int?(waterColor.Count) : null;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					foreach (FishPondWaterColor entry in data.WaterColor)
					{
						if (this.currentOccupants.Value >= entry.MinPopulation && this.lastUnlockedPopulationGate.Value >= entry.MinUnlockedPopulationGate && (entry.Condition == null || GameStateQuery.CheckConditions(entry.Condition, base.GetParentLocation(), null, null, this.GetFishObject(), null, null)))
						{
							if (entry.Color.EqualsIgnoreCase("CopyFromInput"))
							{
								Object fish = this.GetFishObject();
								ColoredObject coloredObject = fish as ColoredObject;
								customColor = ((coloredObject != null) ? new Color?(coloredObject.color.Value) : ItemContextTagManager.GetColorFromTags(fish));
								break;
							}
							customColor = Utility.StringToColor(entry.Color);
							break;
						}
					}
				}
			}
			this.overrideWaterColor.Value = (customColor ?? Color.White);
		}

		// Token: 0x06003789 RID: 14217 RVA: 0x002BF7F4 File Offset: 0x002BD9F4
		public override Color? GetWaterColor(Vector2 tile)
		{
			if (!(this.overrideWaterColor.Value != Color.White))
			{
				return null;
			}
			return new Color?(this.overrideWaterColor.Value);
		}

		// Token: 0x0600378A RID: 14218 RVA: 0x002BF834 File Offset: 0x002BDA34
		public bool JumpFish()
		{
			if (this._fishSilhouettes.Count == 0)
			{
				return false;
			}
			PondFishSilhouette fish_silhouette = Game1.random.ChooseFrom(this._fishSilhouettes);
			this._fishSilhouettes.Remove(fish_silhouette);
			this._jumpingFish.Add(new JumpingFish(this, fish_silhouette.position, (this.GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f));
			return true;
		}

		// Token: 0x0600378B RID: 14219 RVA: 0x002BF8AC File Offset: 0x002BDAAC
		public void SpawnFish()
		{
			if (this.currentOccupants.Value < this.maxOccupants.Value && this.currentOccupants.Value > 0)
			{
				this.hasSpawnedFish.Value = true;
				this.daysSinceSpawn.Value = 0;
				this.currentOccupants.Value = this.currentOccupants.Value + 1;
				if (this.currentOccupants.Value > this.maxOccupants.Value)
				{
					this.currentOccupants.Value = this.maxOccupants.Value;
				}
			}
		}

		// Token: 0x0600378C RID: 14220 RVA: 0x002BF940 File Offset: 0x002BDB40
		public override bool performActiveObjectDropInAction(Farmer who, bool probe)
		{
			Object heldObj = who.ActiveObject;
			if (this.IsValidSignItem(heldObj) && (this.sign.Value == null || heldObj.QualifiedItemId != this.sign.Value.QualifiedItemId))
			{
				if (probe)
				{
					return true;
				}
				Object oldSign = this.sign.Value;
				this.sign.Value = (Object)heldObj.getOne();
				who.reduceActiveItemByOne();
				if (oldSign != null)
				{
					Game1.createItemDebris(oldSign, new Vector2((float)this.tileX.Value + 0.5f, (float)(this.tileY.Value + this.tilesHigh.Value)) * 64f, 3, who.currentLocation, -1, false);
				}
				who.currentLocation.playSound("axe", null, null, SoundContext.Default);
				return true;
			}
			else
			{
				if (!(((heldObj != null) ? heldObj.QualifiedItemId : null) == "(O)GoldenAnimalCracker") || this.goldenAnimalCracker.Value || this.currentOccupants.Value <= 0)
				{
					return base.performActiveObjectDropInAction(who, probe);
				}
				if (probe)
				{
					return true;
				}
				who.reduceActiveItemByOne();
				this.goldenAnimalCracker.Value = true;
				this.isPlayingGoldenCrackerAnimation.Value = true;
				this.showObjectThrownIntoPondAnimation(who, heldObj, delegate
				{
					this.isPlayingGoldenCrackerAnimation.Value = false;
				});
				return true;
			}
		}

		// Token: 0x0600378D RID: 14221 RVA: 0x002BFAA0 File Offset: 0x002BDCA0
		public override void performToolAction(Tool t, int tileX, int tileY)
		{
			if ((t is Axe || t is Pickaxe) && this.sign.Value != null)
			{
				if (t.getLastFarmerToUse() != null)
				{
					Game1.createItemDebris(this.sign.Value, new Vector2((float)this.tileX.Value + 0.5f, (float)(this.tileY.Value + this.tilesHigh.Value)) * 64f, 3, t.getLastFarmerToUse().currentLocation, -1, false);
				}
				this.sign.Value = null;
				t.getLastFarmerToUse().currentLocation.playSound("hammer", new Vector2?(new Vector2((float)tileX, (float)tileY)), null, SoundContext.Default);
			}
			base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x0600378E RID: 14222 RVA: 0x002BFB72 File Offset: 0x002BDD72
		public override void performActionOnConstruction(GameLocation location, Farmer who)
		{
			base.performActionOnConstruction(location, who);
			this.nettingStyle.Value = (this.tileX.Value / 3 + this.tileY.Value / 3) % 3;
		}

		// Token: 0x0600378F RID: 14223 RVA: 0x002BFBA4 File Offset: 0x002BDDA4
		public override void performActionOnBuildingPlacement()
		{
			base.performActionOnBuildingPlacement();
			this.nettingStyle.Value = (this.tileX.Value / 3 + this.tileY.Value / 3) % 3;
		}

		// Token: 0x06003790 RID: 14224 RVA: 0x002BFBD4 File Offset: 0x002BDDD4
		public bool HasUnresolvedNeeds()
		{
			string text;
			int num;
			return this.neededItem.Value != null && this.TryGetNeededItemData(out text, out num) && !this.hasCompletedRequest.Value;
		}

		// Token: 0x06003791 RID: 14225 RVA: 0x002BFC0C File Offset: 0x002BDE0C
		private bool TryGetNeededItemData(out string itemId, out int count)
		{
			itemId = null;
			count = 1;
			if (this.currentOccupants.Value < this.maxOccupants.Value)
			{
				return false;
			}
			this.GetFishPondData();
			FishPondData fishPondData = this._fishPondData;
			if (((fishPondData != null) ? fishPondData.PopulationGates : null) != null)
			{
				if (this.maxOccupants.Value + 1 <= this.lastUnlockedPopulationGate.Value)
				{
					return false;
				}
				List<string> gate;
				if (this._fishPondData.PopulationGates.TryGetValue(this.maxOccupants.Value + 1, out gate))
				{
					Random r = Utility.CreateDaySaveRandom((double)Utility.CreateRandomSeed((double)(this.tileX.Value * 1000), (double)(this.tileY.Value * 2000), 0.0, 0.0, 0.0), 0.0, 0.0);
					string[] split_data = ArgUtility.SplitBySpace(r.ChooseFrom(gate));
					if (split_data.Length >= 1)
					{
						itemId = split_data[0];
					}
					if (split_data.Length >= 3)
					{
						count = r.Next(Convert.ToInt32(split_data[1]), Convert.ToInt32(split_data[2]) + 1);
					}
					else if (split_data.Length >= 2)
					{
						count = Convert.ToInt32(split_data[1]);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003792 RID: 14226 RVA: 0x002BFD44 File Offset: 0x002BDF44
		public void ClearPond()
		{
			Rectangle r = base.GetBoundingBox();
			for (int i = 0; i < this.currentOccupants.Value; i++)
			{
				Vector2 pos = Utility.PointToVector2(r.Center);
				int direction = Game1.random.Next(4);
				switch (direction)
				{
				case 0:
					pos = new Vector2((float)Game1.random.Next(r.Left, r.Right), (float)r.Top);
					break;
				case 1:
					pos = new Vector2((float)r.Right, (float)Game1.random.Next(r.Top, r.Bottom));
					break;
				case 2:
					pos = new Vector2((float)Game1.random.Next(r.Left, r.Right), (float)r.Bottom);
					break;
				case 3:
					pos = new Vector2((float)r.Left, (float)Game1.random.Next(r.Top, r.Bottom));
					break;
				}
				Game1.createItemDebris(this.CreateFishInstance(), pos, direction, Game1.currentLocation, -1, true);
			}
			this._hasAnimatedSpawnedFish = false;
			this.hasSpawnedFish.Value = false;
			this._fishSilhouettes.Clear();
			this._jumpingFish.Clear();
			this.goldenAnimalCracker.Value = false;
			this.isPlayingGoldenCrackerAnimation.Value = false;
			this._fishObject = null;
			this.currentOccupants.Value = 0;
			this.daysSinceSpawn.Value = 0;
			this.neededItem.Value = null;
			this.neededItemCount.Value = -1;
			this.lastUnlockedPopulationGate.Value = 0;
			this.fishType.Value = null;
			this.Reseed();
			this.overrideWaterColor.Value = Color.White;
		}

		// Token: 0x06003793 RID: 14227 RVA: 0x002BFF09 File Offset: 0x002BE109
		public Object CatchFish()
		{
			if (this.currentOccupants.Value == 0)
			{
				return null;
			}
			this.currentOccupants.Value--;
			return (Object)this.CreateFishInstance();
		}

		// Token: 0x06003794 RID: 14228 RVA: 0x002BFF38 File Offset: 0x002BE138
		public Object GetFishObject()
		{
			if (this._fishObject == null)
			{
				this._fishObject = new Object(this.fishType.Value, 1, false, -1, 0);
			}
			return this._fishObject;
		}

		// Token: 0x06003795 RID: 14229 RVA: 0x002BFF64 File Offset: 0x002BE164
		public override void Update(GameTime time)
		{
			this.needsMutex.Update(base.GetParentLocation());
			this.animateHappyFishEvent.Poll();
			if (!this._hasAnimatedSpawnedFish && this.hasSpawnedFish.Value && this._numberOfFishToJump <= 0 && Utility.isOnScreen((this.GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f, 64))
			{
				this._hasAnimatedSpawnedFish = true;
				if (this.fishType.Value != "393" && this.fishType.Value != "397")
				{
					this._numberOfFishToJump = 1;
					this._timeUntilFishHop = Utility.RandomFloat(2f, 5f, null);
				}
			}
			if (this._delayUntilFishSilhouetteAdded > 0f)
			{
				this._delayUntilFishSilhouetteAdded -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._delayUntilFishSilhouetteAdded < 0f)
				{
					this._delayUntilFishSilhouetteAdded = 0f;
				}
			}
			if (this._numberOfFishToJump > 0 && this._timeUntilFishHop > 0f)
			{
				this._timeUntilFishHop -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._timeUntilFishHop <= 0f && this.JumpFish())
				{
					this._numberOfFishToJump--;
					this._timeUntilFishHop = Utility.RandomFloat(0.15f, 0.25f, null);
				}
			}
			while (this._fishSilhouettes.Count > this.currentOccupants.Value - this._jumpingFish.Count)
			{
				this._fishSilhouettes.RemoveAt(0);
			}
			if (this._delayUntilFishSilhouetteAdded <= 0f)
			{
				while (this._fishSilhouettes.Count < this.currentOccupants.Value - this._jumpingFish.Count)
				{
					this._fishSilhouettes.Add(new PondFishSilhouette(this));
				}
			}
			for (int i = 0; i < this._fishSilhouettes.Count; i++)
			{
				this._fishSilhouettes[i].Update((float)time.ElapsedGameTime.TotalSeconds);
			}
			for (int j = 0; j < this._jumpingFish.Count; j++)
			{
				if (this._jumpingFish[j].Update((float)time.ElapsedGameTime.TotalSeconds))
				{
					PondFishSilhouette new_silhouette = new PondFishSilhouette(this);
					new_silhouette.position = this._jumpingFish[j].position;
					this._fishSilhouettes.Add(new_silhouette);
					this._jumpingFish.RemoveAt(j);
					j--;
				}
			}
			base.Update(time);
		}

		// Token: 0x06003796 RID: 14230 RVA: 0x002C0204 File Offset: 0x002BE404
		public override bool isTileFishable(Vector2 tile)
		{
			return this.daysOfConstructionLeft.Value <= 0 && (tile.X > (float)this.tileX.Value && tile.X < (float)(this.tileX.Value + this.tilesWide.Value - 1) && tile.Y > (float)this.tileY.Value) && tile.Y < (float)(this.tileY.Value + this.tilesHigh.Value - 1);
		}

		// Token: 0x06003797 RID: 14231 RVA: 0x002C028F File Offset: 0x002BE48F
		public override bool CanRefillWateringCan()
		{
			return this.daysOfConstructionLeft.Value <= 0;
		}

		// Token: 0x06003798 RID: 14232 RVA: 0x002C02A2 File Offset: 0x002BE4A2
		public override Rectangle? getSourceRectForMenu()
		{
			return new Rectangle?(new Rectangle(0, 0, 80, 80));
		}

		// Token: 0x06003799 RID: 14233 RVA: 0x002C02B4 File Offset: 0x002BE4B4
		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			BuildingData data = this.GetData();
			y += 32;
			if (base.ShouldDrawShadow(data))
			{
				this.drawShadow(b, x, y);
			}
			b.Draw(this.texture.Value, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(0, 80, 80, 80)), new Color(60, 126, 150) * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.75f);
			for (int yWater = this.tileY.Value; yWater < this.tileY.Value + 5; yWater++)
			{
				for (int xWater = this.tileX.Value; xWater < this.tileX.Value + 4; xWater++)
				{
					bool flag = yWater == this.tileY.Value + 4;
					bool topY = yWater == this.tileY.Value;
					if (flag)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(x + xWater * 64 + 32), (float)(y + (yWater + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32)), new Rectangle?(new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5)), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(x + xWater * 64 + 32), (float)(y + yWater * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f))), new Rectangle?(new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(-(int)Game1.currentLocation.waterPosition)) : 0))), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
					}
				}
			}
			b.Draw(this.texture.Value, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(0, 0, 80, 80)), this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.9f);
			b.Draw(this.texture.Value, new Vector2((float)(x + 64), (float)(y + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0))), new Rectangle?(new Rectangle(16, 160, 48, 7)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.95f);
			b.Draw(this.texture.Value, new Vector2((float)x, (float)(y - 128)), new Rectangle?(new Rectangle(80, 0, 80, 48)), this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
		}

		// Token: 0x0600379A RID: 14234 RVA: 0x002C068C File Offset: 0x002BE88C
		public override void OnEndMove()
		{
			foreach (PondFishSilhouette pondFishSilhouette in this._fishSilhouettes)
			{
				pondFishSilhouette.position = (this.GetCenterTile() + new Vector2(Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)(this.tilesWide.Value - 2), Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)(this.tilesHigh.Value - 2))) * 64f;
			}
		}

		// Token: 0x0600379B RID: 14235 RVA: 0x002C074C File Offset: 0x002BE94C
		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if (this.daysOfConstructionLeft.Value > 0)
			{
				this.drawInConstruction(b);
				return;
			}
			BuildingData data = this.GetData();
			for (int i = this.animations.Count - 1; i >= 0; i--)
			{
				this.animations[i].draw(b, false, 0, 0, 1f);
			}
			if (base.ShouldDrawShadow(data))
			{
				this.drawShadow(b, -1, -1);
			}
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64))), new Rectangle?(new Rectangle(0, 80, 80, 80)), ((this.overrideWaterColor.Value == Color.White) ? new Color(60, 126, 150) : this.overrideWaterColor.Value) * this.alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f - 3f) / 10000f);
			for (int y = this.tileY.Value; y < this.tileY.Value + 5; y++)
			{
				for (int x = this.tileX.Value; x < this.tileX.Value + 4; x++)
				{
					bool flag = y == this.tileY.Value + 4;
					bool topY = y == this.tileY.Value;
					if (flag)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)((y + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32))), new Rectangle?(new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5)), this.overrideWaterColor.Equals(Color.White) ? Game1.currentLocation.waterColor.Value : (this.overrideWaterColor.Value * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f - 2f) / 10000f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f)))), new Rectangle?(new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(-(int)Game1.currentLocation.waterPosition)) : 0))), (this.overrideWaterColor.Value == Color.White) ? Game1.currentLocation.waterColor.Value : (this.overrideWaterColor.Value * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f - 2f) / 10000f);
					}
				}
			}
			if (this.overrideWaterColor.Value.Equals(Color.White))
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 64), (float)(this.tileY.Value * 64 + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0)))), new Rectangle?(new Rectangle(16, 160, 48, 7)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f);
			}
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64))), new Rectangle?(new Rectangle(0, 0, 80, 80)), this.color * this.alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, ((float)this.tileY.Value + 0.5f) * 64f / 10000f);
			if (this.nettingStyle.Value < 3)
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 128))), new Rectangle?(new Rectangle(80, this.nettingStyle.Value * 48, 80, 48)), this.color * this.alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 2f) / 10000f);
			}
			if (this.sign.Value != null)
			{
				ParsedItemData signDraw = ItemRegistry.GetDataOrErrorItem(this.sign.Value.QualifiedItemId);
				b.Draw(signDraw.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 8), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 128 - 32))), new Rectangle?(signDraw.GetSourceRect(0, null)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 2f) / 10000f);
				if (this.fishType.Value != null)
				{
					ParsedItemData fishDraw = ItemRegistry.GetData(this.fishType.Value);
					if (fishDraw != null)
					{
						Texture2D fishTexture = fishDraw.GetTexture();
						Rectangle fishSourceRect = fishDraw.GetSourceRect(0, null);
						float yOffset = (this.maxOccupants.Value == 1) ? 6f : 0f;
						b.Draw(fishTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 8 + 8 - 4), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 128 - 8 + 4) + yOffset)), new Rectangle?(fishSourceRect), Color.Black * 0.4f * this.alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 3f) / 10000f);
						b.Draw(fishTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 8 + 8 - 1), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 128 - 8 + 1) + yOffset)), new Rectangle?(fishSourceRect), this.color * this.alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 4f) / 10000f);
						if (this.maxOccupants.Value > 1)
						{
							Utility.drawTinyDigits(this.currentOccupants.Value, b, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 32 + 8 + ((this.currentOccupants.Value < 10) ? 8 : 0)), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - 96))), 3f, (((float)this.tileY.Value + 0.5f) * 64f + 5f) / 10000f, Color.LightYellow * this.alpha);
						}
					}
				}
			}
			if (this._fishObject != null && (this._fishObject.QualifiedItemId == "(O)393" || this._fishObject.QualifiedItemId == "(O)397"))
			{
				for (int j = 0; j < this.currentOccupants.Value; j++)
				{
					Vector2 drawOffset = Vector2.Zero;
					int drawI = (j + this.seedOffset.Value) % 10;
					switch (drawI)
					{
					case 0:
						drawOffset = new Vector2(0f, 0f);
						break;
					case 1:
						drawOffset = new Vector2(48f, 32f);
						break;
					case 2:
						drawOffset = new Vector2(80f, 72f);
						break;
					case 3:
						drawOffset = new Vector2(140f, 28f);
						break;
					case 4:
						drawOffset = new Vector2(96f, 0f);
						break;
					case 5:
						drawOffset = new Vector2(0f, 96f);
						break;
					case 6:
						drawOffset = new Vector2(140f, 80f);
						break;
					case 7:
						drawOffset = new Vector2(64f, 120f);
						break;
					case 8:
						drawOffset = new Vector2(140f, 140f);
						break;
					case 9:
						drawOffset = new Vector2(0f, 150f);
						break;
					}
					b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 64 + 7), (float)(this.tileY.Value * 64 + 64 + 32)) + drawOffset), new Rectangle?(Game1.shadowTexture.Bounds), this.color * this.alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f - 2f) / 10000f - 1.1E-05f);
					ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + this.fishType.Value);
					Texture2D sprite = dataOrErrorItem.GetTexture();
					Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
					b.Draw(sprite, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 64), (float)(this.tileY.Value * 64 + 64)) + drawOffset), new Rectangle?(sourceRect), this.color * this.alpha * 0.75f, 0f, Vector2.Zero, 3f, (drawI % 3 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f - 2f) / 10000f - 1E-05f);
				}
			}
			else
			{
				for (int k = 0; k < this._fishSilhouettes.Count; k++)
				{
					this._fishSilhouettes[k].Draw(b);
				}
			}
			for (int l = 0; l < this._jumpingFish.Count; l++)
			{
				this._jumpingFish[l].Draw(b);
			}
			if (this.HasUnresolvedNeeds())
			{
				Vector2 drawn_position = this.GetRequestTile() * 64f;
				drawn_position += 64f * new Vector2(0.5f, 0.5f);
				float y_offset = 3f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				float bubble_layer_depth = (drawn_position.Y + 160f) / 10000f + 1E-06f;
				drawn_position.Y += y_offset - 32f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, drawn_position), new Rectangle?(new Rectangle(403, 496, 5, 14)), Color.White * 0.75f, 0f, new Vector2(2f, 14f), 4f, SpriteEffects.None, bubble_layer_depth);
			}
			bool showGoldenCracker = this.goldenAnimalCracker.Value && !this.isPlayingGoldenCrackerAnimation.Value;
			if (showGoldenCracker)
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64)) + new Vector2(65f, 59f) * 4f), new Rectangle?(new Rectangle(130, 160, 15, 16)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 2f) / 10000f);
			}
			if (this.output.Value != null)
			{
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64)) + new Vector2(65f, 59f) * 4f), new Rectangle?(new Rectangle(0, 160, 15, 16)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 1f) / 10000f);
				if (showGoldenCracker)
				{
					b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64)) + new Vector2(65f, 59f) * 4f), new Rectangle?(new Rectangle(145, 160, 15, 16)), this.color * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)this.tileY.Value + 0.5f) * 64f + 3f) / 10000f);
				}
				Vector2 vector = this.GetItemBucketTile() * 64f;
				float y_offset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Vector2 bubble_draw_position = vector + new Vector2(0f, -2f) * 64f + new Vector2(0f, y_offset2);
				Vector2 item_relative_to_bubble = new Vector2(40f, 36f);
				float bubble_layer_depth2 = (vector.Y + 64f) / 10000f + 1E-06f;
				float item_layer_depth = (vector.Y + 64f) / 10000f + 1E-05f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, bubble_layer_depth2);
				ParsedItemData outputDraw = ItemRegistry.GetDataOrErrorItem(this.output.Value.QualifiedItemId);
				Texture2D outputTexture = outputDraw.GetTexture();
				b.Draw(outputTexture, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), new Rectangle?(outputDraw.GetSourceRect(0, null)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth);
				ColoredObject coloredObj = this.output.Value as ColoredObject;
				if (coloredObj != null)
				{
					Rectangle colored_source_rect = ItemRegistry.GetDataOrErrorItem(this.output.Value.QualifiedItemId).GetSourceRect(1, null);
					b.Draw(outputTexture, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), new Rectangle?(colored_source_rect), coloredObj.color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth + 1E-05f);
				}
				if (this.output.Value.Stack > 1)
				{
					Utility.drawTinyDigits(this.output.Value.Stack, b, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble + new Vector2(16f, 12f)), 3f, item_layer_depth + 2E-05f, Color.LightYellow * this.alpha);
				}
			}
		}

		// Token: 0x0600379C RID: 14236 RVA: 0x002C1A15 File Offset: 0x002BFC15
		public bool IsValidSignItem(Item item)
		{
			return item != null && (item.HasContextTag("sign_item") || item.QualifiedItemId == "(BC)34");
		}

		// Token: 0x040023FB RID: 9211
		public const int MAXIMUM_OCCUPANCY = 10;

		// Token: 0x040023FC RID: 9212
		public static readonly float FISHING_MILLISECONDS = 1000f;

		// Token: 0x040023FD RID: 9213
		public static readonly int HARVEST_BASE_EXP = 10;

		// Token: 0x040023FE RID: 9214
		public static readonly float HARVEST_OUTPUT_EXP_MULTIPLIER = 0.04f;

		// Token: 0x040023FF RID: 9215
		public static readonly int QUEST_BASE_EXP = 20;

		// Token: 0x04002400 RID: 9216
		public static readonly float QUEST_SPAWNRATE_EXP_MULTIPIER = 5f;

		// Token: 0x04002401 RID: 9217
		public const int NUMBER_OF_NETTING_STYLE_TYPES = 4;

		// Token: 0x04002402 RID: 9218
		[XmlArrayItem("int")]
		public readonly NetString fishType = new NetString();

		// Token: 0x04002403 RID: 9219
		public readonly NetInt lastUnlockedPopulationGate = new NetInt(0);

		// Token: 0x04002404 RID: 9220
		public readonly NetBool hasCompletedRequest = new NetBool(false);

		// Token: 0x04002405 RID: 9221
		public readonly NetBool goldenAnimalCracker = new NetBool(false);

		// Token: 0x04002406 RID: 9222
		[XmlIgnore]
		public readonly NetBool isPlayingGoldenCrackerAnimation = new NetBool(false);

		// Token: 0x04002407 RID: 9223
		public readonly NetRef<Object> sign = new NetRef<Object>();

		// Token: 0x04002408 RID: 9224
		public readonly NetColor overrideWaterColor = new NetColor(Color.White);

		// Token: 0x04002409 RID: 9225
		public readonly NetRef<Item> output = new NetRef<Item>();

		// Token: 0x0400240A RID: 9226
		public readonly NetRef<Item> neededItem = new NetRef<Item>();

		// Token: 0x0400240B RID: 9227
		public readonly NetIntDelta neededItemCount = new NetIntDelta(0);

		// Token: 0x0400240C RID: 9228
		public readonly NetInt daysSinceSpawn = new NetInt(0);

		// Token: 0x0400240D RID: 9229
		public readonly NetInt nettingStyle = new NetInt(0);

		// Token: 0x0400240E RID: 9230
		public readonly NetInt seedOffset = new NetInt(0);

		// Token: 0x0400240F RID: 9231
		public readonly NetBool hasSpawnedFish = new NetBool(false);

		// Token: 0x04002410 RID: 9232
		[XmlIgnore]
		public readonly NetMutex needsMutex = new NetMutex();

		// Token: 0x04002411 RID: 9233
		[XmlIgnore]
		protected bool _hasAnimatedSpawnedFish;

		// Token: 0x04002412 RID: 9234
		[XmlIgnore]
		protected float _delayUntilFishSilhouetteAdded;

		// Token: 0x04002413 RID: 9235
		[XmlIgnore]
		protected int _numberOfFishToJump;

		// Token: 0x04002414 RID: 9236
		[XmlIgnore]
		protected float _timeUntilFishHop;

		// Token: 0x04002415 RID: 9237
		[XmlIgnore]
		protected Object _fishObject;

		// Token: 0x04002416 RID: 9238
		[XmlIgnore]
		public List<PondFishSilhouette> _fishSilhouettes = new List<PondFishSilhouette>();

		// Token: 0x04002417 RID: 9239
		[XmlIgnore]
		public List<JumpingFish> _jumpingFish = new List<JumpingFish>();

		// Token: 0x04002418 RID: 9240
		[XmlIgnore]
		private readonly NetEvent0 animateHappyFishEvent = new NetEvent0(false);

		// Token: 0x04002419 RID: 9241
		[XmlIgnore]
		public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

		// Token: 0x0400241A RID: 9242
		[XmlIgnore]
		protected FishPondData _fishPondData;
	}
}
