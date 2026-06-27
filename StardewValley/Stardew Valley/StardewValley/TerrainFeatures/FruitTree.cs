using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.FruitTrees;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000141 RID: 321
	public class FruitTree : TerrainFeature
	{
		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06001969 RID: 6505 RVA: 0x0012A3CE File Offset: 0x001285CE
		// (set) Token: 0x0600196A RID: 6506 RVA: 0x0012A3D6 File Offset: 0x001285D6
		[XmlIgnore]
		public string textureName { get; private set; }

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x0600196B RID: 6507 RVA: 0x0012A3DF File Offset: 0x001285DF
		// (set) Token: 0x0600196C RID: 6508 RVA: 0x0012A3EC File Offset: 0x001285EC
		[XmlIgnore]
		public bool GreenHouseTileTree
		{
			get
			{
				return this.greenHouseTileTree.Value;
			}
			set
			{
				this.greenHouseTileTree.Value = value;
			}
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x0012A3FA File Offset: 0x001285FA
		public FruitTree() : this(null, 0)
		{
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x0012A404 File Offset: 0x00128604
		public FruitTree(string id, int growthStage = 0) : base(true)
		{
			this.treeId.Value = id;
			this.growthStage.Value = growthStage;
			this.daysUntilMature.Value = FruitTree.GrowthStageToDaysUntilMature(growthStage);
			this.flipped.Value = Game1.random.NextBool();
			this.loadSprite();
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x0012A50C File Offset: 0x0012870C
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.growthStage, "this.growthStage").AddField(this.treeId, "treeId").AddField(this.daysUntilMature, "daysUntilMature").AddField(this.fruit, "fruit").AddField(this.struckByLightningCountdown, "struckByLightningCountdown").AddField(this.health, "health").AddField(this.flipped, "flipped").AddField(this.stump, "stump").AddField(this.greenHouseTileTree, "greenHouseTileTree").AddField(this.shakeLeft, "shakeLeft").AddField(this.falling, "falling").AddField(this.lastPlayerToHit, "lastPlayerToHit").AddField(this.growthRate, "growthRate");
			this.treeId.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this.loadSprite();
			};
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x0012A60D File Offset: 0x0012880D
		public int GetSpriteRowNumber()
		{
			FruitTreeData data = this.GetData();
			if (data == null)
			{
				return 0;
			}
			return data.TextureSpriteRow;
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x0012A620 File Offset: 0x00128820
		public override void loadSprite()
		{
			FruitTreeData data = this.GetData();
			string assetName = ((data != null) ? data.Texture : null) ?? "TileSheets\\fruitTrees";
			if (this.texture == null || this.textureName != assetName)
			{
				try
				{
					this.texture = Game1.content.Load<Texture2D>(assetName);
					this.textureName = assetName;
				}
				catch (Exception ex)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Fruit tree '");
					defaultInterpolatedStringHandler.AppendFormatted(this.treeId.Value);
					defaultInterpolatedStringHandler.AppendLiteral("' failed to load spritesheet '");
					defaultInterpolatedStringHandler.AppendFormatted(assetName);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
			}
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x0012A6E8 File Offset: 0x001288E8
		public override bool isActionable()
		{
			return true;
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x0012A6EC File Offset: 0x001288EC
		public bool IgnoresSeasonsHere()
		{
			GameLocation location = this.Location;
			return ((location != null) ? new bool?(location.SeedsIgnoreSeasonsHere()) : null) ?? false;
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x0012A72C File Offset: 0x0012892C
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x0012A760 File Offset: 0x00128960
		public override Rectangle getRenderBounds()
		{
			Vector2 tileLocation = this.Tile;
			if (this.stump.Value || this.growthStage.Value < 4)
			{
				return new Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
			}
			return new Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x0012A7EC File Offset: 0x001289EC
		public override bool performUseAction(Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			if (this.maxShake == 0f && !this.stump.Value && this.growthStage.Value >= 3 && !this.IsWinterTreeHere())
			{
				location.playSound("leafrustle", null, null, SoundContext.Default);
			}
			this.shake(tileLocation, false);
			return true;
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x0012A858 File Offset: 0x00128A58
		public override bool tickUpdate(GameTime time)
		{
			if (this.destroy)
			{
				return true;
			}
			GameLocation location = this.Location;
			Vector2 tileLocation = this.Tile;
			this.alpha = Math.Min(1f, this.alpha + 0.05f);
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (this.growthStage.Value >= 4 && !this.falling.Value && !this.stump.Value && Game1.player.GetBoundingBox().Intersects(new Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 4), 192, 224)))
			{
				this.alpha = Math.Max(0.4f, this.alpha - 0.09f);
			}
			if (!this.falling.Value)
			{
				if ((double)Math.Abs(this.shakeRotation) > 1.5707963267948966 && this.leaves.Count <= 0 && this.health.Value <= 0f)
				{
					return true;
				}
				if (this.maxShake > 0f)
				{
					if (this.shakeLeft.Value)
					{
						this.shakeRotation -= ((this.growthStage.Value >= 4) ? 0.005235988f : 0.015707964f);
						if (this.shakeRotation <= -this.maxShake)
						{
							this.shakeLeft.Value = false;
						}
					}
					else
					{
						this.shakeRotation += ((this.growthStage.Value >= 4) ? 0.005235988f : 0.015707964f);
						if (this.shakeRotation >= this.maxShake)
						{
							this.shakeLeft.Value = true;
						}
					}
				}
				if (this.maxShake > 0f)
				{
					this.maxShake = Math.Max(0f, this.maxShake - ((this.growthStage.Value >= 4) ? 0.0010226539f : 0.0030679617f));
				}
				if (this.struckByLightningCountdown.Value > 0 && Game1.random.NextDouble() < 0.01)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(tileLocation.X * 64f + (float)Game1.random.Next(-64, 96), tileLocation.Y * 64f - 192f + (float)Game1.random.Next(-64, 128)), false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							interval = 99999f,
							layerDepth = 1f,
							scale = 2f,
							scaleChange = 0.01f
						}
					});
				}
			}
			else
			{
				this.shakeRotation += (this.shakeLeft.Value ? (-(this.maxShake * this.maxShake)) : (this.maxShake * this.maxShake));
				this.maxShake += 0.0015339808f;
				if (Game1.random.NextDouble() < 0.01 && !this.IsWinterTreeHere())
				{
					location.localSound("leafrustle", null, null, SoundContext.Default);
				}
				if ((double)Math.Abs(this.shakeRotation) > 1.5707963267948966)
				{
					this.falling.Value = false;
					this.maxShake = 0f;
					location.localSound("treethud", null, null, SoundContext.Default);
					int leavesToAdd = Game1.random.Next(90, 120);
					for (int i = 0; i < leavesToAdd; i++)
					{
						this.leaves.Add(new Leaf(new Vector2((float)(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (this.shakeLeft.Value ? -320 : 256)), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
					}
					Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
					Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 12.0), true, -1, false, null);
					Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 12.0), false, -1, false, null);
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
					}
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, 10, this.lastPlayerToHit.Value, location);
					}
					else
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, 10, location);
					}
					if (this.health.Value <= 0f)
					{
						this.health.Value = -100f;
					}
				}
			}
			for (int j = this.leaves.Count - 1; j >= 0; j--)
			{
				Leaf leaf = this.leaves[j];
				Leaf leaf2 = leaf;
				leaf2.position.Y = leaf2.position.Y - (leaf.yVelocity - 3f);
				leaf.yVelocity = Math.Max(0f, leaf.yVelocity - 0.01f);
				leaf.rotation += leaf.rotationRate;
				if (leaf.position.Y >= tileLocation.Y * 64f + 64f)
				{
					this.leaves.RemoveAt(j);
				}
			}
			return false;
		}

		// Token: 0x06001978 RID: 6520 RVA: 0x0012AF70 File Offset: 0x00129170
		public int GetQuality()
		{
			if (this.struckByLightningCountdown.Value > 0 || this.daysUntilMature.Value >= 0)
			{
				return 0;
			}
			switch (this.daysUntilMature.Value / -112)
			{
			case 0:
				return 0;
			case 1:
				return 1;
			case 2:
				return 2;
			default:
				return 4;
			}
		}

		// Token: 0x06001979 RID: 6521 RVA: 0x0012AFC8 File Offset: 0x001291C8
		public virtual void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
		{
			if ((this.maxShake == 0f || doEvenIfStillShaking) && this.growthStage.Value >= 3 && !this.stump.Value)
			{
				Vector2 playerPixel = Game1.player.getStandingPosition();
				this.shakeLeft.Value = (playerPixel.X > (tileLocation.X + 0.5f) * 64f || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool()));
				this.maxShake = (float)((this.growthStage.Value >= 4) ? 0.02454369260617026 : 0.04908738521234052);
				if (this.growthStage.Value >= 4)
				{
					if (Game1.random.NextDouble() < 0.66 && !this.IsWinterTreeHere())
					{
						int numberOfLeaves = Game1.random.Next(1, 6);
						for (int i = 0; i < numberOfLeaves; i++)
						{
							this.leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), (float)Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
						}
					}
					int fruitQuality = this.GetQuality();
					TerrainFeature terrainFeature;
					if (this.Location.terrainFeatures.TryGetValue(tileLocation, out terrainFeature) && terrainFeature.Equals(this))
					{
						for (int j = 0; j < this.fruit.Count; j++)
						{
							Vector2 offset = new Vector2(0f, 0f);
							switch (j)
							{
							case 0:
								offset.X = -64f;
								break;
							case 1:
								offset.X = 64f;
								offset.Y = -32f;
								break;
							case 2:
								offset.Y = 32f;
								break;
							}
							Debris d;
							if (this.struckByLightningCountdown.Value <= 0)
							{
								Item item = this.fruit[j];
								this.fruit[j] = null;
								d = new Debris(item, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, playerPixel)
								{
									itemQuality = fruitQuality
								};
							}
							else
							{
								d = new Debris(382.ToString(), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, playerPixel)
								{
									itemQuality = fruitQuality
								};
							}
							d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
							d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
							this.Location.debris.Add(d);
						}
						this.fruit.Clear();
						return;
					}
				}
				else if (Game1.random.NextDouble() < 0.66 && !this.IsWinterTreeHere())
				{
					int numberOfLeaves2 = Game1.random.Next(1, 3);
					for (int k = 0; k < numberOfLeaves2; k++)
					{
						this.leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 96f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
					}
					return;
				}
			}
			else if (this.stump.Value)
			{
				this.shakeTimer = 100f;
			}
		}

		// Token: 0x0600197A RID: 6522 RVA: 0x0012B443 File Offset: 0x00129643
		public override bool isPassable(Character c = null)
		{
			return this.health.Value <= -99f;
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x0012B45C File Offset: 0x0012965C
		public static bool IsTooCloseToAnotherTree(Vector2 tileLocation, GameLocation environment, bool fruitTreesOnly = false)
		{
			Vector2 v = default(Vector2);
			for (int i = (int)tileLocation.X - 2; i <= (int)tileLocation.X + 2; i++)
			{
				for (int j = (int)tileLocation.Y - 2; j <= (int)tileLocation.Y + 2; j++)
				{
					v.X = (float)i;
					v.Y = (float)j;
					TerrainFeature feature;
					if (environment.terrainFeatures.TryGetValue(v, out feature) && (feature is FruitTree || (!fruitTreesOnly && feature is Tree)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x0012B4E4 File Offset: 0x001296E4
		public static bool IsGrowthBlocked(Vector2 tileLocation, GameLocation environment)
		{
			foreach (Vector2 v in Utility.getSurroundingTileLocationsArray(tileLocation))
			{
				if (environment.IsTileOccupiedBy(v, CollisionMask.Objects, CollisionMask.None, false))
				{
					Object valueOrDefault = environment.objects.GetValueOrDefault(v, null);
					string a = (valueOrDefault != null) ? valueOrDefault.QualifiedItemId : null;
					if (!(a == "(O)590") && !(a == "(O)SeedSpot"))
					{
						return true;
					}
				}
				if (environment.IsTileOccupiedBy(v, CollisionMask.TerrainFeatures, CollisionMask.None, false))
				{
					TerrainFeature valueOrDefault2 = environment.terrainFeatures.GetValueOrDefault(v, null);
					HoeDirt dirt = valueOrDefault2 as HoeDirt;
					if (dirt == null)
					{
						if (!(valueOrDefault2 is Grass))
						{
							return true;
						}
					}
					else if (dirt.crop != null)
					{
						return true;
					}
				}
				if (environment.IsTileOccupiedBy(v, CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.Furniture | CollisionMask.LocationSpecific, CollisionMask.None, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600197D RID: 6525 RVA: 0x0012B5AC File Offset: 0x001297AC
		public FruitTreeData GetData()
		{
			FruitTreeData data;
			if (!FruitTree.TryGetData(this.treeId.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x0600197E RID: 6526 RVA: 0x0012B5D0 File Offset: 0x001297D0
		public static bool TryGetData(string id, out FruitTreeData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return Game1.fruitTreeData.TryGetValue(id, out data);
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x0012B5E6 File Offset: 0x001297E6
		public string GetDisplayName()
		{
			FruitTreeData data = this.GetData();
			return TokenParser.ParseText((data != null) ? data.DisplayName : null, null, null, null) ?? ItemRegistry.GetErrorItemName();
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x0012B60C File Offset: 0x0012980C
		public override void dayUpdate()
		{
			GameLocation environment = this.Location;
			if (this.health.Value <= -99f)
			{
				this.destroy = true;
			}
			if (this.struckByLightningCountdown.Value > 0)
			{
				NetInt netInt = this.struckByLightningCountdown;
				int value = netInt.Value;
				netInt.Value = value - 1;
				if (this.struckByLightningCountdown.Value <= 0)
				{
					this.fruit.Clear();
				}
			}
			bool foundSomething = FruitTree.IsGrowthBlocked(this.Tile, environment);
			if (!foundSomething || this.daysUntilMature.Value <= 0)
			{
				if (this.daysUntilMature.Value > 28)
				{
					this.daysUntilMature.Value = 28;
				}
				if (this.growthRate.Value > 1)
				{
					int value2 = this.growthRate.Value;
				}
				this.daysUntilMature.Value -= this.growthRate.Value;
				this.growthStage.Value = FruitTree.DaysUntilMatureToGrowthStage(this.daysUntilMature.Value);
			}
			else if (foundSomething && this.growthStage.Value != 4)
			{
				FruitTreeData data = this.GetData();
				string tokenizedDisplayName = ((data != null) ? data.DisplayName : null) ?? this.GetDisplayName();
				Game1.multiplayer.broadcastGlobalMessage("Strings\\UI:FruitTree_Warning", true, null, new string[]
				{
					tokenizedDisplayName
				});
			}
			if (this.stump.Value)
			{
				this.fruit.Clear();
				return;
			}
			this.TryAddFruit();
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x0012B76C File Offset: 0x0012996C
		public static int GrowthStageToDaysUntilMature(int growthStage)
		{
			if (growthStage > 4)
			{
				growthStage = 4;
			}
			switch (growthStage)
			{
			case 1:
				return 21;
			case 2:
				return 14;
			case 3:
				return 7;
			case 4:
				return 0;
			default:
				return 28;
			}
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x0012B79C File Offset: 0x0012999C
		public static int DaysUntilMatureToGrowthStage(int daysUntilMature)
		{
			for (int stage = 4; stage >= 0; stage--)
			{
				if (daysUntilMature <= FruitTree.GrowthStageToDaysUntilMature(stage))
				{
					return stage;
				}
			}
			return 0;
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x0012B7C4 File Offset: 0x001299C4
		public bool TryAddFruit()
		{
			if (!this.stump.Value && this.growthStage.Value >= 4 && (this.IsInSeasonHere() || (this.struckByLightningCountdown.Value > 0 && !this.IsWinterTreeHere())) && this.fruit.Count < 3)
			{
				FruitTreeData data = this.GetData();
				if (((data != null) ? data.Fruit : null) != null)
				{
					foreach (FruitTreeFruitData entry in data.Fruit)
					{
						Item item = this.TryCreateFruit(entry);
						if (item != null)
						{
							this.fruit.Add(item);
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x0012B894 File Offset: 0x00129A94
		private Item TryCreateFruit(FruitTreeFruitData drop)
		{
			if (!Game1.random.NextBool(drop.Chance))
			{
				return null;
			}
			if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, this.Location, null, null, null, null, this.IgnoresSeasonsHere() ? GameStateQuery.SeasonQueryKeys : null))
			{
				return null;
			}
			if (drop.Season != null && !this.IgnoresSeasonsHere())
			{
				Season? season = drop.Season;
				Season seasonForLocation = Game1.GetSeasonForLocation(this.Location);
				if (!(season.GetValueOrDefault() == seasonForLocation & season != null))
				{
					return null;
				}
			}
			ISpawnItemData drop2 = drop;
			GameLocation location = this.Location;
			Farmer player = null;
			Random random = null;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
			defaultInterpolatedStringHandler.AppendLiteral("fruit tree '");
			defaultInterpolatedStringHandler.AppendFormatted(this.treeId.Value);
			defaultInterpolatedStringHandler.AppendLiteral("' > fruit '");
			defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			Item item = ItemQueryResolver.TryResolveRandomItem(drop2, new ItemQueryContext(location, player, random, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, delegate(string query, string error)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(57, 4);
				defaultInterpolatedStringHandler2.AppendLiteral("Fruit tree '");
				defaultInterpolatedStringHandler2.AppendFormatted(this.treeId.Value);
				defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
				defaultInterpolatedStringHandler2.AppendFormatted(query);
				defaultInterpolatedStringHandler2.AppendLiteral("' for fruit '");
				defaultInterpolatedStringHandler2.AppendFormatted(drop.Id);
				defaultInterpolatedStringHandler2.AppendLiteral("': ");
				defaultInterpolatedStringHandler2.AppendFormatted(error);
				log.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			});
			if (item != null)
			{
				item.Quality = this.GetQuality();
			}
			return item;
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x0012B9E6 File Offset: 0x00129BE6
		public virtual bool IsWinterTreeHere()
		{
			return !this.IgnoresSeasonsHere() && Game1.GetSeasonForLocation(this.Location) == Season.Winter;
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x0012BA00 File Offset: 0x00129C00
		public virtual bool IsInSeasonHere()
		{
			if (this.IgnoresSeasonsHere())
			{
				return true;
			}
			FruitTreeData data = this.GetData();
			List<Season> growSeasons = (data != null) ? data.Seasons : null;
			if (growSeasons != null && growSeasons.Count > 0)
			{
				Season curSeason = Game1.GetSeasonForLocation(this.Location);
				foreach (Season growSeason in growSeasons)
				{
					if (curSeason == growSeason)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x0012BA8C File Offset: 0x00129C8C
		public virtual Season GetCosmeticSeason()
		{
			if (!this.IgnoresSeasonsHere())
			{
				return this.Location.GetSeason();
			}
			return Season.Summer;
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x0012BAA3 File Offset: 0x00129CA3
		public override bool seasonUpdate(bool onLoad)
		{
			if (!this.IsInSeasonHere() && !onLoad)
			{
				this.fruit.Clear();
			}
			return false;
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x0012BABC File Offset: 0x00129CBC
		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
		{
			if (this.health.Value <= -99f)
			{
				return false;
			}
			if (t is MeleeWeapon)
			{
				return false;
			}
			GameLocation location = this.Location;
			if (this.growthStage.Value >= 4)
			{
				if (t is Axe)
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					location.debris.Add(new Debris(12, Game1.random.Next(t.upgradeLevel.Value * 2, t.upgradeLevel.Value * 4), t.getLastFarmerToUse().GetToolLocation(false) + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0, null));
					this.lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
					int fruitQuality = this.GetQuality();
					TerrainFeature terrainFeature;
					if (location.terrainFeatures.TryGetValue(tileLocation, out terrainFeature) && terrainFeature.Equals(this))
					{
						for (int i = 0; i < this.fruit.Count; i++)
						{
							Vector2 offset = new Vector2(0f, 0f);
							switch (i)
							{
							case 0:
								offset.X = -64f;
								break;
							case 1:
								offset.X = 64f;
								offset.Y = -32f;
								break;
							case 2:
								offset.Y = 32f;
								break;
							}
							Debris d;
							if (this.struckByLightningCountdown.Value <= 0)
							{
								Item item = this.fruit[i];
								this.fruit[i] = null;
								d = new Debris(item, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, Game1.player.getStandingPosition())
								{
									itemQuality = fruitQuality
								};
							}
							else
							{
								d = new Debris(382.ToString(), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, Game1.player.getStandingPosition())
								{
									itemQuality = fruitQuality
								};
							}
							d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
							d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
							location.debris.Add(d);
						}
						this.fruit.Clear();
					}
				}
				else if (explosion <= 0)
				{
					return false;
				}
				this.shake(tileLocation, true);
				float damage;
				if (explosion > 0)
				{
					damage = (float)explosion;
				}
				else
				{
					if (t == null)
					{
						return false;
					}
					switch (t.upgradeLevel.Value)
					{
					case 0:
						damage = 1f;
						break;
					case 1:
						damage = 1.25f;
						break;
					case 2:
						damage = 1.67f;
						break;
					case 3:
						damage = 2.5f;
						break;
					case 4:
						damage = 5f;
						break;
					default:
						damage = (float)(t.upgradeLevel.Value + 1);
						break;
					}
				}
				this.health.Value -= damage;
				if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage / 5f))
				{
					Debris d2 = new Debris("388", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
					d2.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
					d2.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
					location.debris.Add(d2);
				}
				if (this.health.Value <= 0f)
				{
					if (!this.stump.Value)
					{
						location.playSound("treecrack", new Vector2?(tileLocation), null, SoundContext.Default);
						this.stump.Value = true;
						this.health.Value = 5f;
						this.falling.Value = true;
						if (((t != null) ? t.getLastFarmerToUse() : null) == null)
						{
							this.shakeLeft.Value = true;
						}
						else
						{
							this.shakeLeft.Value = ((float)t.getLastFarmerToUse().StandingPixel.X > (tileLocation.X + 0.5f) * 64f);
						}
					}
					else
					{
						this.health.Value = -100f;
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), false, -1, false, null);
						if (Game1.IsMultiplayer)
						{
							Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 2000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
						}
						if (((t != null) ? t.getLastFarmerToUse() : null) == null)
						{
							Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 2, location);
						}
						else
						{
							Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
							if (Game1.IsMultiplayer)
							{
								Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 1, this.lastPlayerToHit.Value, location);
								Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, lastHitBy.professions.Contains(12) ? 5 : 4, true, -1, false, null);
							}
							else
							{
								Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 5.0), true, -1, false, null);
								Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 1, location);
							}
						}
						if (this.treeId.Value != null)
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)" + this.treeId.Value, 1, this.GetQuality(), false), tileLocation * 64f, 2, location, -1, false);
						}
					}
				}
			}
			else if (this.growthStage.Value >= 3)
			{
				if (t != null && t.Name.Contains("Ax"))
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					location.playSound("leafrustle", new Vector2?(tileLocation), null, SoundContext.Default);
					location.debris.Add(new Debris(12, Game1.random.Next(t.upgradeLevel.Value * 2, t.upgradeLevel.Value * 4), t.getLastFarmerToUse().GetToolLocation(false) + new Vector2(16f, 0f), t.getLastFarmerToUse().getStandingPosition(), 0, null));
				}
				else if (explosion <= 0)
				{
					return false;
				}
				this.shake(tileLocation, true);
				float damage2 = 1f;
				Random debrisRandom;
				if (Game1.IsMultiplayer)
				{
					debrisRandom = Game1.recentMultiplayerRandom;
				}
				else
				{
					debrisRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, Game1.stats.DaysPlayed, (double)this.health.Value);
				}
				if (explosion > 0)
				{
					damage2 = (float)explosion;
				}
				else
				{
					switch (t.upgradeLevel.Value)
					{
					case 0:
						damage2 = 2f;
						break;
					case 1:
						damage2 = 2.5f;
						break;
					case 2:
						damage2 = 3.34f;
						break;
					case 3:
						damage2 = 5f;
						break;
					case 4:
						damage2 = 10f;
						break;
					}
				}
				int debris = 0;
				while (t != null && debrisRandom.NextDouble() < (double)damage2 * 0.08 + (double)((float)t.getLastFarmerToUse().ForagingLevel / 200f))
				{
					debris++;
				}
				this.health.Value -= damage2;
				if (debris > 0)
				{
					Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, debris, location);
				}
				if (this.health.Value <= 0f)
				{
					if (this.treeId.Value != null)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)" + this.treeId.Value, 1, 0, false), tileLocation * 64f, 2, location, -1, false);
					}
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), false, -1, false, null);
					return true;
				}
			}
			else if (this.growthStage.Value >= 1)
			{
				if (explosion > 0)
				{
					return true;
				}
				if (t != null && t.Name.Contains("Axe"))
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), false, -1, false, null);
				}
				if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), false, -1, false, null);
					if (t.Name.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
					{
						Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
					}
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					});
					if (this.treeId.Value != null)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)" + this.treeId.Value, 1, 0, false), tileLocation * 64f, 2, location, -1, false);
					}
					return true;
				}
			}
			else
			{
				if (explosion > 0)
				{
					return true;
				}
				if (t.Name.Contains("Axe") || t.Name.Contains("Pick") || t.Name.Contains("Hoe"))
				{
					location.playSound("woodyHit", new Vector2?(tileLocation), null, SoundContext.Default);
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					});
					if (this.treeId.Value != null)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)" + this.treeId.Value, 1, 0, false), tileLocation * 64f, 2, location, -1, false);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x0012C69C File Offset: 0x0012A89C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			if (this.growthStage.Value < 4)
			{
				Rectangle sourceRect;
				switch (this.growthStage.Value)
				{
				case 0:
					sourceRect = new Rectangle(128, 512, 64, 64);
					break;
				case 1:
					sourceRect = new Rectangle(0, 512, 64, 64);
					break;
				case 2:
					sourceRect = new Rectangle(64, 512, 64, 64);
					break;
				default:
					sourceRect = new Rectangle(0, 384, 64, 128);
					break;
				}
				spriteBatch.Draw(this.texture, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
				return;
			}
			if (!this.falling.Value)
			{
				spriteBatch.Draw(this.texture, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle?(new Rectangle(128, 384, 64, 128)), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
			}
			if (!this.stump.Value || this.falling.Value)
			{
				spriteBatch.Draw(this.texture, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Rectangle?(new Rectangle(0, 0, 192, 384)), Color.White, this.shakeRotation, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
			}
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x0012C8C4 File Offset: 0x0012AAC4
		public override void draw(SpriteBatch spriteBatch)
		{
			int seasonIndex = Game1.GetSeasonIndexForLocation(this.Location);
			int spriteRow = this.GetSpriteRowNumber();
			Vector2 tileLocation = this.Tile;
			Rectangle boundingBox = this.getBoundingBox();
			if (this.greenHouseTileTree.Value)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(new Rectangle(669, 1957, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
			}
			if (this.growthStage.Value < 4)
			{
				Vector2 positionOffset = new Vector2((float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / 6.283185307179586) * -16.0)), (float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / 6.283185307179586) * -16.0))) / 2f;
				Rectangle sourceRect;
				switch (this.growthStage.Value)
				{
				case 0:
					sourceRect = new Rectangle(0, spriteRow * 5 * 16, 48, 80);
					break;
				case 1:
					sourceRect = new Rectangle(48, spriteRow * 5 * 16, 48, 80);
					break;
				case 2:
					sourceRect = new Rectangle(96, spriteRow * 5 * 16, 48, 80);
					break;
				default:
					sourceRect = new Rectangle(144, spriteRow * 5 * 16, 48, 80);
					break;
				}
				spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + positionOffset.X, tileLocation.Y * 64f - (float)sourceRect.Height + 128f + positionOffset.Y)), new Rectangle?(sourceRect), Color.White, this.shakeRotation, new Vector2(24f, 80f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)boundingBox.Bottom / 10000f - tileLocation.X / 1000000f);
			}
			else
			{
				if (!this.stump.Value || this.falling.Value)
				{
					Season cosmeticSeason = this.GetCosmeticSeason();
					if (!this.falling.Value)
					{
						spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle?(new Rectangle((int)(((Season)12 + (int)(cosmeticSeason * Season.Winter)) * (Season)16), spriteRow * 5 * 16 + 64, 48, 16)), (this.struckByLightningCountdown.Value > 0) ? (Color.Gray * this.alpha) : (Color.White * this.alpha), 0f, new Vector2(24f, 16f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-07f);
					}
					spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle?(new Rectangle((int)(((Season)12 + (int)(cosmeticSeason * Season.Winter)) * (Season)16), spriteRow * 5 * 16, 48, 64)), (this.struckByLightningCountdown.Value > 0) ? (Color.Gray * this.alpha) : (Color.White * this.alpha), this.shakeRotation, new Vector2(24f, 80f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)boundingBox.Bottom / 10000f + 0.001f - tileLocation.X / 1000000f);
				}
				if (this.health.Value >= 1f || (!this.falling.Value && this.health.Value > -99f))
				{
					spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 2f) : 0f), tileLocation.Y * 64f + 64f)), new Rectangle?(new Rectangle(384, spriteRow * 5 * 16 + 48, 48, 32)), (this.struckByLightningCountdown.Value > 0) ? (Color.Gray * this.alpha) : (Color.White * this.alpha), 0f, new Vector2(24f, 32f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.stump.Value && !this.falling.Value) ? ((float)boundingBox.Bottom / 10000f) : ((float)boundingBox.Bottom / 10000f - 0.001f - tileLocation.X / 1000000f));
				}
				for (int i = 0; i < this.fruit.Count; i++)
				{
					ParsedItemData parsedItemData = (this.struckByLightningCountdown.Value > 0) ? ItemRegistry.GetDataOrErrorItem("(O)382") : ItemRegistry.GetDataOrErrorItem(this.fruit[i].QualifiedItemId);
					Texture2D texture = parsedItemData.GetTexture();
					Rectangle sourceRect2 = parsedItemData.GetSourceRect(0, null);
					switch (i)
					{
					case 0:
						spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 64f + tileLocation.X * 200f % 64f / 2f, tileLocation.Y * 64f - 192f - tileLocation.X % 64f / 3f)), new Rectangle?(sourceRect2), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					case 1:
						spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 256f + tileLocation.X * 232f % 64f / 3f)), new Rectangle?(sourceRect2), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					case 2:
						spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + tileLocation.X * 200f % 64f / 3f, tileLocation.Y * 64f - 160f + tileLocation.X * 200f % 64f / 3f)), new Rectangle?(sourceRect2), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (float)boundingBox.Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					}
				}
			}
			foreach (Leaf j in this.leaves)
			{
				spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, j.position), new Rectangle?(new Rectangle((24 + seasonIndex) * 16, spriteRow * 5 * 16, 8, 8)), Color.White, j.rotation, Vector2.Zero, 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f + 0.01f);
			}
		}

		// Token: 0x04000F5A RID: 3930
		public const string DefaultTextureName = "TileSheets\\fruitTrees";

		// Token: 0x04000F5B RID: 3931
		public const float shakeRate = 0.015707964f;

		// Token: 0x04000F5C RID: 3932
		public const float shakeDecayRate = 0.0030679617f;

		// Token: 0x04000F5D RID: 3933
		public const int minWoodDebrisForFallenTree = 12;

		// Token: 0x04000F5E RID: 3934
		public const int minWoodDebrisForStump = 5;

		// Token: 0x04000F5F RID: 3935
		public const int startingHealth = 10;

		// Token: 0x04000F60 RID: 3936
		public const int leafFallRate = 3;

		// Token: 0x04000F61 RID: 3937
		public const int DaysUntilMaturity = 28;

		// Token: 0x04000F62 RID: 3938
		public const int maxFruitsOnTrees = 3;

		// Token: 0x04000F63 RID: 3939
		public const int seedStage = 0;

		// Token: 0x04000F64 RID: 3940
		public const int sproutStage = 1;

		// Token: 0x04000F65 RID: 3941
		public const int saplingStage = 2;

		// Token: 0x04000F66 RID: 3942
		public const int bushStage = 3;

		// Token: 0x04000F67 RID: 3943
		public const int treeStage = 4;

		// Token: 0x04000F69 RID: 3945
		[XmlIgnore]
		public Texture2D texture;

		// Token: 0x04000F6A RID: 3946
		[XmlElement("growthStage")]
		public readonly NetInt growthStage = new NetInt();

		// Token: 0x04000F6B RID: 3947
		[XmlElement("treeType")]
		public string obsolete_treeType;

		// Token: 0x04000F6C RID: 3948
		[XmlElement("treeId")]
		public readonly NetString treeId = new NetString();

		// Token: 0x04000F6D RID: 3949
		[XmlElement("daysUntilMature")]
		public readonly NetInt daysUntilMature = new NetInt(28);

		// Token: 0x04000F6E RID: 3950
		[XmlElement("fruitsOnTree")]
		public int? obsolete_fruitsOnTree;

		// Token: 0x04000F6F RID: 3951
		[XmlElement("fruit")]
		public readonly NetList<Item, NetRef<Item>> fruit = new NetList<Item, NetRef<Item>>();

		// Token: 0x04000F70 RID: 3952
		[XmlElement("struckByLightningCountdown")]
		public readonly NetInt struckByLightningCountdown = new NetInt();

		// Token: 0x04000F71 RID: 3953
		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat(10f);

		// Token: 0x04000F72 RID: 3954
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		// Token: 0x04000F73 RID: 3955
		[XmlElement("stump")]
		public readonly NetBool stump = new NetBool();

		// Token: 0x04000F74 RID: 3956
		[XmlElement("greenHouseTileTree")]
		public readonly NetBool greenHouseTileTree = new NetBool();

		// Token: 0x04000F75 RID: 3957
		[XmlIgnore]
		public readonly NetBool shakeLeft = new NetBool();

		// Token: 0x04000F76 RID: 3958
		[XmlIgnore]
		public readonly NetBool falling = new NetBool();

		// Token: 0x04000F77 RID: 3959
		[XmlIgnore]
		public bool destroy;

		// Token: 0x04000F78 RID: 3960
		[XmlIgnore]
		public float shakeRotation;

		// Token: 0x04000F79 RID: 3961
		[XmlIgnore]
		public float maxShake;

		// Token: 0x04000F7A RID: 3962
		[XmlIgnore]
		public float alpha = 1f;

		// Token: 0x04000F7B RID: 3963
		private List<Leaf> leaves = new List<Leaf>();

		// Token: 0x04000F7C RID: 3964
		[XmlIgnore]
		public readonly NetLong lastPlayerToHit = new NetLong();

		// Token: 0x04000F7D RID: 3965
		[XmlIgnore]
		public float shakeTimer;

		// Token: 0x04000F7E RID: 3966
		[XmlElement("growthRate")]
		public readonly NetInt growthRate = new NetInt(1);
	}
}
