using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x0200014B RID: 331
	public class Tree : TerrainFeature
	{
		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06001A2B RID: 6699 RVA: 0x001336BB File Offset: 0x001318BB
		// (set) Token: 0x06001A2C RID: 6700 RVA: 0x001336C3 File Offset: 0x001318C3
		[XmlIgnore]
		public string TextureName { get; private set; }

		// Token: 0x06001A2D RID: 6701 RVA: 0x001336CC File Offset: 0x001318CC
		public Tree() : base(true)
		{
			this.resetTexture();
		}

		// Token: 0x06001A2E RID: 6702 RVA: 0x001337B4 File Offset: 0x001319B4
		public Tree(string id, int growthStage, bool isGreenRainTemporaryTree = false) : this()
		{
			this.growthStage.Value = growthStage;
			this.isTemporaryGreenRainTree.Value = isGreenRainTemporaryTree;
			this.treeType.Value = id;
			if (this.treeType.Value == "4")
			{
				this.treeType.Value = "1";
			}
			if (this.treeType.Value == "5")
			{
				this.treeType.Value = "2";
			}
			this.flipped.Value = Game1.random.NextBool();
			this.health.Value = 10f;
		}

		// Token: 0x06001A2F RID: 6703 RVA: 0x00133860 File Offset: 0x00131A60
		public Tree(string id) : this()
		{
			this.treeType.Value = id;
			if (this.treeType.Value == "4")
			{
				this.treeType.Value = "1";
			}
			if (this.treeType.Value == "5")
			{
				this.treeType.Value = "2";
			}
			this.flipped.Value = Game1.random.NextBool();
			this.health.Value = 10f;
		}

		// Token: 0x06001A30 RID: 6704 RVA: 0x001338F4 File Offset: 0x00131AF4
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.growthStage, "growthStage").AddField(this.treeType, "treeType").AddField(this.health, "health").AddField(this.flipped, "flipped").AddField(this.stump, "stump").AddField(this.tapped, "tapped").AddField(this.hasSeed, "hasSeed").AddField(this.fertilized, "fertilized").AddField(this.shakeLeft, "shakeLeft").AddField(this.falling, "falling").AddField(this.destroy, "destroy").AddField(this.lastPlayerToHit, "lastPlayerToHit").AddField(this.wasShakenToday, "wasShakenToday").AddField(this.hasMoss, "hasMoss").AddField(this.isTemporaryGreenRainTree, "isTemporaryGreenRainTree").AddField(this.stopGrowingMoss, "stopGrowingMoss");
			this.treeType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.CheckForNewTexture();
			};
		}

		// Token: 0x06001A31 RID: 6705 RVA: 0x00133A25 File Offset: 0x00131C25
		public static Dictionary<string, WildTreeData> GetWildTreeDataDictionary()
		{
			if (Tree._WildTreeData == null)
			{
				Tree._LoadWildTreeData();
			}
			return Tree._WildTreeData;
		}

		// Token: 0x06001A32 RID: 6706 RVA: 0x00133A38 File Offset: 0x00131C38
		public static Dictionary<string, List<string>> GetWildTreeSeedLookup()
		{
			if (Tree._WildTreeSeedLookup == null)
			{
				Tree._LoadWildTreeData();
			}
			return Tree._WildTreeSeedLookup;
		}

		// Token: 0x06001A33 RID: 6707 RVA: 0x00133A4C File Offset: 0x00131C4C
		protected static void _LoadWildTreeData()
		{
			Tree._WildTreeData = DataLoader.WildTrees(Game1.content);
			Tree._WildTreeSeedLookup = new Dictionary<string, List<string>>();
			foreach (KeyValuePair<string, WildTreeData> pair in Tree._WildTreeData)
			{
				string treeId = pair.Key;
				WildTreeData treeData = pair.Value;
				if (treeData.SeedPlantable && !string.IsNullOrWhiteSpace(treeData.SeedItemId))
				{
					ItemMetadata seedData = ItemRegistry.ResolveMetadata(treeData.SeedItemId);
					if (seedData != null)
					{
						List<string> itemIds;
						if (!Tree._WildTreeSeedLookup.TryGetValue(seedData.QualifiedItemId, out itemIds))
						{
							itemIds = (Tree._WildTreeSeedLookup[seedData.QualifiedItemId] = new List<string>());
						}
						itemIds.Add(treeId);
						if (!Tree._WildTreeSeedLookup.TryGetValue(seedData.LocalItemId, out itemIds))
						{
							itemIds = (Tree._WildTreeSeedLookup[seedData.LocalItemId] = new List<string>());
						}
						itemIds.Add(treeId);
					}
				}
			}
		}

		// Token: 0x06001A34 RID: 6708 RVA: 0x00133B5C File Offset: 0x00131D5C
		public static string ResolveTreeTypeFromSeed(string itemId)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			List<string> possibles;
			if (((metadata != null) ? metadata.TypeIdentifier : null) == "(O)" && Tree.GetWildTreeSeedLookup().TryGetValue(metadata.LocalItemId, out possibles))
			{
				return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("wildtreesplanted") + 1U, 0.0, 0.0, 0.0).ChooseFrom(possibles);
			}
			return null;
		}

		// Token: 0x06001A35 RID: 6709 RVA: 0x00133BDE File Offset: 0x00131DDE
		internal static void ClearCache()
		{
			Tree._WildTreeData = null;
			Tree._WildTreeSeedLookup = null;
		}

		// Token: 0x06001A36 RID: 6710 RVA: 0x00133BEC File Offset: 0x00131DEC
		public void CheckForNewTexture()
		{
			if (!this.texture.IsValueCreated)
			{
				return;
			}
			string textureName = this.ChooseTexture();
			if (textureName != null && textureName != this.TextureName)
			{
				this.resetTexture();
			}
		}

		// Token: 0x06001A37 RID: 6711 RVA: 0x00133C25 File Offset: 0x00131E25
		public void resetTexture()
		{
			this.texture = new Lazy<Texture2D>(new Func<Texture2D>(this.<resetTexture>g__LoadTexture|65_0));
		}

		// Token: 0x06001A38 RID: 6712 RVA: 0x00133C40 File Offset: 0x00131E40
		public WildTreeData GetData()
		{
			WildTreeData data;
			if (!Tree.TryGetData(this.treeType.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06001A39 RID: 6713 RVA: 0x00133C64 File Offset: 0x00131E64
		public static bool TryGetData(string id, out WildTreeData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return Tree.GetWildTreeDataDictionary().TryGetValue(id, out data);
		}

		// Token: 0x06001A3A RID: 6714 RVA: 0x00133C7C File Offset: 0x00131E7C
		protected string ChooseTexture()
		{
			WildTreeData data = this.GetData();
			if (data != null)
			{
				List<WildTreeTextureData> textures = data.Textures;
				int? num = (textures != null) ? new int?(textures.Count) : null;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					foreach (WildTreeTextureData entry in data.Textures)
					{
						if (this.Location != null && this.Location.IsGreenhouse && entry.Season != null)
						{
							Season? season = entry.Season;
							Season season2 = Season.Spring;
							if (season.GetValueOrDefault() == season2 & season != null)
							{
								return entry.Texture;
							}
						}
						else
						{
							if (entry.Season != null)
							{
								Season? season = entry.Season;
								Season? season3 = this.localSeason;
								if (!(season.GetValueOrDefault() == season3.GetValueOrDefault() & season != null == (season3 != null)))
								{
									continue;
								}
							}
							if (entry.Condition == null || GameStateQuery.CheckConditions(entry.Condition, this.Location, null, null, null, null, null))
							{
								return entry.Texture;
							}
						}
					}
					return data.Textures[0].Texture;
				}
			}
			return null;
		}

		// Token: 0x06001A3B RID: 6715 RVA: 0x00133DF0 File Offset: 0x00131FF0
		public override Microsoft.Xna.Framework.Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		// Token: 0x06001A3C RID: 6716 RVA: 0x00133E24 File Offset: 0x00132024
		public override Microsoft.Xna.Framework.Rectangle getRenderBounds()
		{
			Vector2 tileLocation = this.Tile;
			if (this.stump.Value || this.growthStage.Value < 5)
			{
				return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
		}

		// Token: 0x06001A3D RID: 6717 RVA: 0x00133EB0 File Offset: 0x001320B0
		public override bool performUseAction(Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			if (!this.tapped.Value)
			{
				if (this.maxShake == 0f && !this.stump.Value && this.growthStage.Value >= 3 && this.IsLeafy())
				{
					location.localSound("leafrustle", null, null, SoundContext.Default);
				}
				this.shake(tileLocation, false);
			}
			return Game1.player.ActiveObject == null || !Game1.player.ActiveObject.canBePlacedHere(location, tileLocation, CollisionMask.All, false);
		}

		// Token: 0x06001A3E RID: 6718 RVA: 0x00133F50 File Offset: 0x00132150
		private int extraWoodCalculator(Vector2 tileLocation)
		{
			Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
			int extraWood = 0;
			if (random.NextDouble() < Game1.player.DailyLuck)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.LuckLevel / 25.0)
			{
				extraWood++;
			}
			if (this.treeType.Value == "3")
			{
				extraWood++;
			}
			return extraWood;
		}

		// Token: 0x06001A3F RID: 6719 RVA: 0x00134038 File Offset: 0x00132238
		public override bool tickUpdate(GameTime time)
		{
			GameLocation location = this.Location;
			Season? season = this.localSeason;
			if (season == null)
			{
				this.setSeason();
				this.CheckForNewTexture();
			}
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (this.destroy.Value)
			{
				return true;
			}
			this.alpha = Math.Min(1f, this.alpha + 0.05f);
			Vector2 tileLocation = this.Tile;
			if (this.growthStage.Value >= 5 && !this.falling.Value && !this.stump.Value && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 5), 192, 288)))
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
						this.shakeRotation -= ((this.growthStage.Value >= 5) ? 0.005235988f : 0.015707964f);
						if (this.shakeRotation <= -this.maxShake)
						{
							this.shakeLeft.Value = false;
						}
					}
					else
					{
						this.shakeRotation += ((this.growthStage.Value >= 5) ? 0.005235988f : 0.015707964f);
						if (this.shakeRotation >= this.maxShake)
						{
							this.shakeLeft.Value = true;
						}
					}
				}
				if (this.maxShake > 0f)
				{
					this.maxShake = Math.Max(0f, this.maxShake - ((this.growthStage.Value >= 5) ? 0.0010226539f : 0.0030679617f));
				}
			}
			else
			{
				this.shakeRotation += (this.shakeLeft.Value ? (-(this.maxShake * this.maxShake)) : (this.maxShake * this.maxShake));
				this.maxShake += 0.0015339808f;
				WildTreeData data = this.GetData();
				if (data != null && Game1.random.NextDouble() < 0.01 && this.IsLeafy())
				{
					location.localSound("leafrustle", null, null, SoundContext.Default);
				}
				if ((double)Math.Abs(this.shakeRotation) > 1.5707963267948966)
				{
					this.falling.Value = false;
					this.maxShake = 0f;
					if (data != null)
					{
						location.localSound("treethud", null, null, SoundContext.Default);
						if (this.IsLeafy())
						{
							int leavesToAdd = Game1.random.Next(90, 120);
							for (int i = 0; i < leavesToAdd; i++)
							{
								this.leaves.Add(new Leaf(new Vector2((float)(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (this.shakeLeft.Value ? -320 : 256)), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
							}
						}
						Random r;
						if (Game1.IsMultiplayer)
						{
							Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
							r = Game1.recentMultiplayerRandom;
						}
						else
						{
							r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
						}
						Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
						if (data.DropWoodOnChop)
						{
							int numToDrop = (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + this.extraWoodCalculator(tileLocation)));
							if (lastHitBy.stats.Get("Book_Woodcutting") > 0U && r.NextDouble() < 0.05)
							{
								numToDrop *= 2;
							}
							Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, numToDrop, true, -1, false, null);
							Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + this.extraWoodCalculator(tileLocation))), false, -1, false, null);
						}
						if (data.DropWoodOnChop)
						{
							Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, 5, this.lastPlayerToHit.Value, location);
						}
						int numHardwood = 0;
						if (data.DropHardwoodOnLumberChop)
						{
							while (lastHitBy.professions.Contains(14) && r.NextBool())
							{
								numHardwood++;
							}
						}
						List<WildTreeChopItemData> chopItems = data.ChopItems;
						if (chopItems != null && chopItems.Count > 0)
						{
							bool addedAdditionalHardwood = false;
							foreach (WildTreeChopItemData drop in data.ChopItems)
							{
								Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems", null, new bool?(false));
								if (item != null)
								{
									if (drop.ItemId == "709")
									{
										numHardwood += item.Stack;
										addedAdditionalHardwood = true;
									}
									else
									{
										Game1.createMultipleItemDebris(item, new Vector2(tileLocation.X + (float)(this.shakeLeft.Value ? -4 : 4), tileLocation.Y) * 64f, -2, location, -1, false);
									}
								}
							}
							if (addedAdditionalHardwood && lastHitBy.professions.Contains(14))
							{
								numHardwood += (int)((float)numHardwood * 0.25f + 0.9f);
							}
						}
						if (numHardwood > 0)
						{
							Game1.createMultipleObjectDebris("(O)709", (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, numHardwood, this.lastPlayerToHit.Value, location);
						}
						float seedOnChopChance = data.SeedOnChopChance;
						if (lastHitBy.getEffectiveSkillLevel(2) >= 1 && data != null && data.SeedItemId != null && r.NextDouble() < (double)seedOnChopChance)
						{
							Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X + (this.shakeLeft.Value ? -4 : 4), (int)tileLocation.Y, r.Next(1, 3), this.lastPlayerToHit.Value, location);
						}
					}
					if (this.health.Value == -100f)
					{
						return true;
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

		// Token: 0x06001A40 RID: 6720 RVA: 0x001348F8 File Offset: 0x00132AF8
		public Item TryGetDrop(WildTreeItemData drop, Random r, Farmer targetFarmer, string fieldName, Func<string, string> formatItemId = null, bool? isStump = null)
		{
			if (!r.NextBool(drop.Chance))
			{
				return null;
			}
			if (drop.Season != null)
			{
				Season? season = drop.Season;
				Season season2 = this.Location.GetSeason();
				if (!(season.GetValueOrDefault() == season2 & season != null))
				{
					return null;
				}
			}
			if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, this.Location, targetFarmer, null, null, r, null))
			{
				return null;
			}
			WildTreeChopItemData chopItemData = drop as WildTreeChopItemData;
			if (chopItemData != null && !chopItemData.IsValidForGrowthStage(this.growthStage.Value, isStump ?? this.stump.Value))
			{
				return null;
			}
			ISpawnItemData drop2 = drop;
			GameLocation location = this.Location;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
			defaultInterpolatedStringHandler.AppendLiteral("wild tree '");
			defaultInterpolatedStringHandler.AppendFormatted(this.treeType.Value);
			defaultInterpolatedStringHandler.AppendLiteral("' > ");
			defaultInterpolatedStringHandler.AppendFormatted(fieldName);
			defaultInterpolatedStringHandler.AppendLiteral(" entry '");
			defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			return ItemQueryResolver.TryResolveRandomItem(drop2, new ItemQueryContext(location, targetFarmer, r, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, formatItemId, null, delegate(string query, string error)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(57, 5);
				defaultInterpolatedStringHandler2.AppendLiteral("Wild tree '");
				defaultInterpolatedStringHandler2.AppendFormatted(this.treeType.Value);
				defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
				defaultInterpolatedStringHandler2.AppendFormatted(query);
				defaultInterpolatedStringHandler2.AppendLiteral("' for ");
				defaultInterpolatedStringHandler2.AppendFormatted(fieldName);
				defaultInterpolatedStringHandler2.AppendLiteral(" entry '");
				defaultInterpolatedStringHandler2.AppendFormatted(drop.Id);
				defaultInterpolatedStringHandler2.AppendLiteral("': ");
				defaultInterpolatedStringHandler2.AppendFormatted(error);
				log.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			});
		}

		// Token: 0x06001A41 RID: 6721 RVA: 0x00134A88 File Offset: 0x00132C88
		public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
		{
			GameLocation location = this.Location;
			WildTreeData data = this.GetData();
			if ((this.maxShake == 0f || doEvenIfStillShaking) && this.growthStage.Value >= 3 && !this.stump.Value)
			{
				this.shakeLeft.Value = ((float)Game1.player.StandingPixel.X > (tileLocation.X + 0.5f) * 64f || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool()));
				this.maxShake = (float)((this.growthStage.Value >= 5) ? 0.02454369260617026 : 0.04908738521234052);
				if (this.growthStage.Value >= 5)
				{
					if (this.IsLeafy())
					{
						if (Game1.random.NextDouble() < 0.66)
						{
							int numberOfLeaves = Game1.random.Next(1, 6);
							for (int i = 0; i < numberOfLeaves; i++)
							{
								this.leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), (float)Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
							}
						}
						if (Game1.random.NextDouble() < 0.01)
						{
							Season? season = this.localSeason;
							Season season2 = Season.Spring;
							if ((season.GetValueOrDefault() == season2 & season != null) || this.localSeason.GetValueOrDefault() == Season.Summer)
							{
								bool isIslandButterfly = this.Location.InIslandContext();
								while (Game1.random.NextDouble() < 0.8)
								{
									location.addCritter(new Butterfly(location, new Vector2(tileLocation.X + (float)Game1.random.Next(1, 3), tileLocation.Y - 2f + (float)Game1.random.Next(-1, 2)), isIslandButterfly, false, -1, false));
								}
							}
						}
					}
					if (this.hasSeed.Value && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1))
					{
						bool dropDefaultSeed = true;
						if (data != null)
						{
							List<WildTreeSeedDropItemData> seedDropItems = data.SeedDropItems;
							int? num = (seedDropItems != null) ? new int?(seedDropItems.Count) : null;
							int num2 = 0;
							if (num.GetValueOrDefault() > num2 & num != null)
							{
								foreach (WildTreeSeedDropItemData drop in data.SeedDropItems)
								{
									Item seed = this.TryGetDrop(drop, Game1.random, Game1.player, "SeedDropItems", null, null);
									if (seed != null)
									{
										if (Game1.player.professions.Contains(16) && seed.HasContextTag("forage_item"))
										{
											seed.Quality = 4;
										}
										Game1.createItemDebris(seed, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y, false);
										if (!drop.ContinueOnDrop)
										{
											dropDefaultSeed = false;
											break;
										}
									}
								}
							}
						}
						if (dropDefaultSeed && data != null)
						{
							Item seed2 = ItemRegistry.Create(data.SeedItemId, 1, 0, false);
							if (Game1.player.professions.Contains(16) && seed2.HasContextTag("forage_item"))
							{
								seed2.Quality = 4;
							}
							Game1.createItemDebris(seed2, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y, false);
						}
						if (Utility.tryRollMysteryBox(0.03, null))
						{
							Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32, false);
						}
						Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, this.Location, 2.0, 1.0, Game1.player.StandingPixel.Y - 32, null);
						if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
						{
							Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
						}
						this.hasSeed.Value = false;
					}
					if (this.wasShakenToday.Value)
					{
						return;
					}
					this.wasShakenToday.Value = true;
					if (((data != null) ? data.ShakeItems : null) == null)
					{
						return;
					}
					using (List<WildTreeItemData>.Enumerator enumerator2 = data.ShakeItems.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							WildTreeItemData entry = enumerator2.Current;
							Item item = this.TryGetDrop(entry, Game1.random, Game1.player, "ShakeItems", null, null);
							if (item != null)
							{
								Game1.createItemDebris(item, tileLocation * 64f, -2, this.Location, -1, false);
							}
						}
						return;
					}
				}
				if (Game1.random.NextDouble() < 0.66)
				{
					int numberOfLeaves2 = Game1.random.Next(1, 3);
					for (int j = 0; j < numberOfLeaves2; j++)
					{
						this.leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 32f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
					}
					return;
				}
			}
			else if (this.stump.Value)
			{
				this.shakeTimer = 100f;
			}
		}

		// Token: 0x06001A42 RID: 6722 RVA: 0x00135188 File Offset: 0x00133388
		public override bool isPassable(Character c = null)
		{
			return this.health.Value <= -99f || this.growthStage.Value == 0;
		}

		// Token: 0x06001A43 RID: 6723 RVA: 0x001351AC File Offset: 0x001333AC
		public virtual int GetMaxSizeHere(bool ignoreSeason = false)
		{
			GameLocation location = this.Location;
			Vector2 tile = this.Tile;
			if (this.GetData() == null)
			{
				return this.growthStage.Value;
			}
			if (location.IsNoSpawnTile(tile, "Tree", false) && !location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"))
			{
				return this.growthStage.Value;
			}
			if (!ignoreSeason && !this.IsInSeason())
			{
				return this.growthStage.Value;
			}
			if (this.growthStage.Value == 0 && location.objects.ContainsKey(tile))
			{
				return 0;
			}
			if (this.IsGrowthBlockedByNearbyTree())
			{
				return 4;
			}
			return 15;
		}

		// Token: 0x06001A44 RID: 6724 RVA: 0x0013525C File Offset: 0x0013345C
		public bool IsInSeason()
		{
			if (this.localSeason.GetValueOrDefault() == Season.Winter && !this.fertilized.Value && !this.Location.SeedsIgnoreSeasonsHere())
			{
				WildTreeData data = this.GetData();
				return ((data != null) ? new bool?(data.GrowsInWinter) : null) ?? false;
			}
			return true;
		}

		// Token: 0x06001A45 RID: 6725 RVA: 0x001352C8 File Offset: 0x001334C8
		public bool IsGrowthBlockedByNearbyTree()
		{
			GameLocation location = this.Location;
			Vector2 tile = this.Tile;
			Microsoft.Xna.Framework.Rectangle growthRect = new Microsoft.Xna.Framework.Rectangle((int)((tile.X - 1f) * 64f), (int)((tile.Y - 1f) * 64f), 192, 192);
			foreach (KeyValuePair<Vector2, TerrainFeature> other in location.terrainFeatures.Pairs)
			{
				if (other.Key != tile)
				{
					Tree otherTree = other.Value as Tree;
					if (otherTree != null && otherTree.growthStage.Value >= 5 && otherTree.getBoundingBox().Intersects(growthRect))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001A46 RID: 6726 RVA: 0x001353AC File Offset: 0x001335AC
		public void onGreenRainDay(bool undo = false)
		{
			if (undo)
			{
				if (this.isTemporaryGreenRainTree.Value)
				{
					this.isTemporaryGreenRainTree.Value = false;
					if (this.treeType.Value == "10")
					{
						this.treeType.Value = "1";
					}
					else
					{
						this.treeType.Value = "2";
					}
					this.resetTexture();
					return;
				}
			}
			else if (this.Location != null && this.Location.IsOutdoors)
			{
				if (this.growthStage.Value < 5)
				{
					if (this.growthStage.Value == 0 && (Game1.random.NextDouble() < 0.5 || this.Location == null || this.Location.objects.ContainsKey(this.Tile)))
					{
						return;
					}
					this.growthStage.Value = 4;
					for (int i = 0; i < 3; i++)
					{
						this.dayUpdate();
					}
				}
				WildTreeData data = this.GetData();
				bool? flag = (data != null) ? new bool?(data.GrowsMoss) : null;
				if (flag != null && flag.GetValueOrDefault() && Game1.random.NextBool())
				{
					this.hasMoss.Value = true;
				}
				if ((this.treeType.Value == "1" || this.treeType.Value == "2") && this.growthStage.Value >= 5 && Game1.random.NextBool(0.75))
				{
					this.isTemporaryGreenRainTree.Value = true;
					if (this.treeType.Value == "1")
					{
						this.treeType.Value = "10";
					}
					else
					{
						this.treeType.Value = "11";
					}
					this.resetTexture();
				}
			}
		}

		// Token: 0x06001A47 RID: 6727 RVA: 0x00135588 File Offset: 0x00133788
		public override void dayUpdate()
		{
			GameLocation environment = this.Location;
			if (!Game1.IsFall && !Game1.IsWinter)
			{
				GameLocation location2 = this.Location;
				if ((location2 == null || !location2.IsGreenRainingHere()) && this.isTemporaryGreenRainTree.Value)
				{
					this.isTemporaryGreenRainTree.Value = false;
					if (this.treeType.Value == "10")
					{
						this.treeType.Value = "1";
					}
					else
					{
						this.treeType.Value = "2";
					}
					this.resetTexture();
				}
			}
			this.wasShakenToday.Value = false;
			this.setSeason();
			this.CheckForNewTexture();
			WildTreeData data = this.GetData();
			Vector2 tile = this.Tile;
			if (this.health.Value <= -100f)
			{
				this.destroy.Value = true;
			}
			if (this.tapped.Value)
			{
				Object tile_object = environment.getObjectAtTile((int)tile.X, (int)tile.Y, false);
				if (tile_object == null || !tile_object.IsTapper())
				{
					this.tapped.Value = false;
				}
				else if (tile_object.IsTapper() && tile_object.heldObject.Value == null)
				{
					this.UpdateTapperProduct(tile_object, null, false);
				}
			}
			if (this.GetMaxSizeHere(false) > this.growthStage.Value)
			{
				float chance = (data != null) ? data.GrowthChance : 0.2f;
				float fertilizedGrowthChance = (data != null) ? data.FertilizedGrowthChance : 1f;
				if (Game1.random.NextBool(chance) || (this.fertilized.Value && Game1.random.NextBool(fertilizedGrowthChance)))
				{
					NetInt netInt = this.growthStage;
					int value = netInt.Value;
					netInt.Value = value + 1;
				}
			}
			if (this.localSeason.GetValueOrDefault() == Season.Winter && data != null && data.IsStumpDuringWinter && !this.Location.SeedsIgnoreSeasonsHere())
			{
				this.stump.Value = true;
			}
			else if (data != null && data.IsStumpDuringWinter && Game1.dayOfMonth <= 1 && Game1.IsSpring)
			{
				this.stump.Value = false;
				this.health.Value = 10f;
				this.shakeRotation = 0f;
			}
			if (this.growthStage.Value >= 5 && !this.stump.Value && environment is Farm && Game1.random.NextBool((data != null) ? data.SeedSpreadChance : 0.15f))
			{
				int xCoord = Game1.random.Next(-3, 4) + (int)tile.X;
				int yCoord = Game1.random.Next(-3, 4) + (int)tile.Y;
				Vector2 location = new Vector2((float)xCoord, (float)yCoord);
				if (!environment.IsNoSpawnTile(location, "Tree", false) && environment.isTileLocationOpen(new Location(xCoord, yCoord)) && !environment.IsTileOccupiedBy(location, CollisionMask.All, CollisionMask.None, false) && !environment.isWaterTile(xCoord, yCoord) && environment.isTileOnMap(location))
				{
					environment.terrainFeatures.Add(location, new Tree(this.treeType.Value, 0, false));
				}
			}
			if (this.isTemporaryGreenRainTree.Value && environment.IsGreenhouse && (this.localSeason.GetValueOrDefault() == Season.Winter || this.localSeason.GetValueOrDefault() == Season.Fall))
			{
				this.hasSeed.Value = false;
			}
			else
			{
				this.hasSeed.Value = (data != null && data.SeedItemId != null && this.growthStage.Value >= 5 && Game1.random.NextBool(data.SeedOnShakeChance));
			}
			bool accelerateMoss = this.growthStage.Value >= 5 && !Game1.IsWinter && (this.treeType.Value == "10" || this.treeType.Value == "11") && !this.isTemporaryGreenRainTree.Value;
			if (this.growthStage.Value >= 5 && !Game1.IsWinter && !accelerateMoss)
			{
				int x = (int)tile.X - 2;
				while ((float)x <= tile.X + 2f)
				{
					int y = (int)tile.Y - 2;
					while ((float)y <= tile.Y + 2f)
					{
						Vector2 v = new Vector2((float)x, (float)y);
						Tree tree = this.Location.terrainFeatures.GetValueOrDefault(v, null) as Tree;
						if (tree != null && tree.growthStage.Value >= 5 && (tree.treeType.Value == "10" || tree.treeType.Value == "11") && !tree.isTemporaryGreenRainTree.Value && tree.hasMoss.Value)
						{
							accelerateMoss = true;
							break;
						}
						y++;
					}
					if (accelerateMoss)
					{
						break;
					}
					x++;
				}
			}
			float mossChance = Game1.isRaining ? 0.2f : 0.1f;
			if (accelerateMoss && Game1.random.NextDouble() < 0.5)
			{
				NetInt netInt2 = this.growthStage;
				int value = netInt2.Value;
				netInt2.Value = value + 1;
			}
			if (Game1.IsSummer && !Game1.isGreenRain && !Game1.isRaining)
			{
				mossChance = 0.033f;
			}
			if (accelerateMoss && Game1.random.NextDouble() < 0.5)
			{
				mossChance += 0.1f;
			}
			if (this.stopGrowingMoss.Value)
			{
				this.hasMoss.Value = false;
				return;
			}
			if (!environment.IsGreenhouse && (this.localSeason.GetValueOrDefault() == Season.Winter || this.stump.Value))
			{
				this.hasMoss.Value = false;
				return;
			}
			bool? flag = (data != null) ? new bool?(data.GrowsMoss) : null;
			if (flag != null && flag.GetValueOrDefault() && this.growthStage.Value >= 14 && !this.stump.Value && Game1.random.NextBool(mossChance))
			{
				this.hasMoss.Value = true;
			}
		}

		// Token: 0x06001A48 RID: 6728 RVA: 0x00135B91 File Offset: 0x00133D91
		public override void performPlayerEntryAction()
		{
			base.performPlayerEntryAction();
			this.setSeason();
			this.CheckForNewTexture();
		}

		// Token: 0x06001A49 RID: 6729 RVA: 0x00135BA8 File Offset: 0x00133DA8
		public override bool seasonUpdate(bool onLoad)
		{
			if (!onLoad && Game1.IsFall && Game1.random.NextDouble() < 0.05 && !this.tapped.Value && (this.treeType.Value == "1" || this.treeType.Value == "2") && this.growthStage.Value >= 5 && this.Location != null && !(this.Location is Town) && !this.Location.IsGreenhouse)
			{
				this.treeType.Value = ((this.treeType.Value == "1") ? "10" : "11");
				this.isTemporaryGreenRainTree.Value = true;
				this.resetTexture();
			}
			if (this.tapped.Value && this.Location != null)
			{
				Object tileObject = this.Location.getObjectAtTile((int)this.Tile.X, (int)this.Tile.Y, false);
				if (tileObject != null && tileObject.IsTapper())
				{
					this.UpdateTapperProduct(tileObject, null, true);
				}
			}
			this.loadSprite();
			return false;
		}

		// Token: 0x06001A4A RID: 6730 RVA: 0x00135CE0 File Offset: 0x00133EE0
		public override bool isActionable()
		{
			return !this.tapped.Value && this.growthStage.Value >= 3;
		}

		// Token: 0x06001A4B RID: 6731 RVA: 0x00135D04 File Offset: 0x00133F04
		public virtual bool IsLeafy()
		{
			WildTreeData data = this.GetData();
			return data != null && data.IsLeafy && (data.IsLeafyInWinter || !this.Location.IsWinterHere()) && (data.IsLeafyInFall || !this.Location.IsFallHere());
		}

		// Token: 0x06001A4C RID: 6732 RVA: 0x00135D54 File Offset: 0x00133F54
		public Color? GetChopDebrisColor()
		{
			return this.GetChopDebrisColor(this.GetData());
		}

		// Token: 0x06001A4D RID: 6733 RVA: 0x00135D64 File Offset: 0x00133F64
		public Color? GetChopDebrisColor(WildTreeData data)
		{
			string rawColor = (data != null) ? data.DebrisColor : null;
			if (rawColor == null)
			{
				return null;
			}
			int debrisType;
			if (!int.TryParse(rawColor, out debrisType))
			{
				return Utility.StringToColor(rawColor);
			}
			return new Color?(Debris.getColorForDebris(debrisType));
		}

		// Token: 0x06001A4E RID: 6734 RVA: 0x00135DA8 File Offset: 0x00133FA8
		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
		{
			GameLocation location = this.Location ?? Game1.currentLocation;
			if (explosion > 0)
			{
				this.tapped.Value = false;
			}
			if (this.health.Value <= -99f)
			{
				return false;
			}
			if (this.growthStage.Value >= 5)
			{
				if (this.hasMoss.Value)
				{
					Item moss = Tree.CreateMossItem();
					if (((t != null) ? t.getLastFarmerToUse() : null) != null)
					{
						t.getLastFarmerToUse().gainExperience(2, moss.Stack);
					}
					this.hasMoss.Value = false;
					Game1.createMultipleItemDebris(moss, new Vector2(tileLocation.X, tileLocation.Y - 1f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32, false);
					Game1.stats.Increment("mossHarvested", 1U);
					this.shake(tileLocation, true);
					this.growthStage.Value = 12 - moss.Stack;
					Game1.playSound("moss_cut", null);
					for (int i = 0; i < 6; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Microsoft.Xna.Framework.Rectangle(Game1.random.Choose(16, 0), 96, 16, 16), new Vector2(tileLocation.X + (float)Game1.random.NextDouble() - 0.15f, tileLocation.Y - 1f + (float)Game1.random.NextDouble()) * 64f, false, 0.025f, Color.Green)
						{
							drawAboveAlwaysFront = true,
							motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -4f),
							acceleration = new Vector2(0f, 0.3f + (float)Game1.random.Next(-10, 11) / 200f),
							animationLength = 1,
							interval = 1000f,
							sourceRectStartingPos = new Vector2(0f, 96f),
							alpha = 1f,
							layerDepth = 1f,
							scale = 4f
						});
					}
				}
				if (this.tapped.Value)
				{
					return false;
				}
				if (t is Axe)
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					this.lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
					location.debris.Add(new Debris(12, Game1.random.Next(1, 3), t.getLastFarmerToUse().GetToolLocation(false) + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0, this.GetChopDebrisColor()));
					if (location is Town && tileLocation.X < 100f && !this.isTemporaryGreenRainTree.Value)
					{
						int pathsIndex = location.getTileIndexAt((int)tileLocation.X, (int)tileLocation.Y, "Paths", null);
						if (pathsIndex == 9 || pathsIndex == 10 || pathsIndex == 11)
						{
							this.shake(tileLocation, true);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:TownTreeWarning"));
							return false;
						}
					}
					if (!this.stump.Value && t.getLastFarmerToUse() != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.005)
					{
						Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
						if (o != null)
						{
							Game1.createItemDebris(o, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32, false);
						}
					}
					else if (!this.stump.Value && t.getLastFarmerToUse() != null && Utility.tryRollMysteryBox(0.005, null))
					{
						Game1.createItemDebris(ItemRegistry.Create((t.getLastFarmerToUse().stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32, false);
					}
					else if (!this.stump.Value && t.getLastFarmerToUse() != null && t.getLastFarmerToUse().stats.Get("TreesChopped") > 20U && Game1.random.NextDouble() < 0.0003 + (t.getLastFarmerToUse().mailReceived.Contains("GotWoodcuttingBook") ? 0.0007 : (t.getLastFarmerToUse().stats.Get("TreesChopped") * 1E-05)))
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)Book_Woodcutting", 1, 0, false), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32, false);
						t.getLastFarmerToUse().mailReceived.Add("GotWoodcuttingBook");
					}
					else if (!this.stump.Value)
					{
						Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, this.Location, 0.33, 1.0, Game1.player.StandingPixel.Y - 32, null);
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
					if (location is Town && tileLocation.X < 100f)
					{
						return false;
					}
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
				if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage / 5f))
				{
					string value = this.treeType.Value;
					Debris d;
					if (!(value == "12"))
					{
						if (!(value == "7"))
						{
							if (!(value == "8"))
							{
								d = new Debris("388", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
							}
							else
							{
								d = new Debris("(O)709", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
							}
						}
						else
						{
							d = new Debris("(O)420", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
						}
					}
					else
					{
						d = new Debris("(O)259", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
					}
					d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
					d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
					location.debris.Add(d);
				}
				this.health.Value -= damage;
				if (this.health.Value <= 0f && this.performTreeFall(t, explosion, tileLocation))
				{
					return true;
				}
			}
			else if (this.growthStage.Value >= 3)
			{
				if (t != null && t.Name.Contains("Ax"))
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					if (this.IsLeafy())
					{
						location.playSound("leafrustle", null, null, SoundContext.Default);
					}
					location.debris.Add(new Debris(12, Game1.random.Next(t.upgradeLevel.Value * 2, t.upgradeLevel.Value * 4), t.getLastFarmerToUse().GetToolLocation(false) + new Vector2(16f, 0f), Utility.PointToVector2(t.getLastFarmerToUse().StandingPixel), 0, null));
				}
				else if (explosion <= 0)
				{
					return false;
				}
				this.shake(tileLocation, true);
				float damage2;
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
					default:
						damage2 = (float)(10 + (t.upgradeLevel.Value - 4));
						break;
					}
				}
				this.health.Value -= damage2;
				if (this.health.Value <= 0f)
				{
					this.performBushDestroy(tileLocation);
					return true;
				}
			}
			else if (this.growthStage.Value >= 1)
			{
				if (explosion > 0)
				{
					location.playSound("cut", null, null, SoundContext.Default);
					return true;
				}
				if (t != null && t.Name.Contains("Axe"))
				{
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), false, -1, false, null);
				}
				if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
				{
					location.playSound("cut", null, null, SoundContext.Default);
					this.performSproutDestroy(t, tileLocation);
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
					this.performSeedDestroy(t, tileLocation);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A4F RID: 6735 RVA: 0x0013691C File Offset: 0x00134B1C
		public static Item CreateMossItem()
		{
			Random rand = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("mossHarvested") * 50U, 0.0, 0.0, 0.0);
			return ItemRegistry.Create("(O)Moss", rand.Next(1, 3), 0, false);
		}

		// Token: 0x06001A50 RID: 6736 RVA: 0x0013697C File Offset: 0x00134B7C
		public bool fertilize()
		{
			GameLocation location = this.Location;
			if (this.growthStage.Value >= 5)
			{
				Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer1", true);
				location.playSound("cancel", null, null, SoundContext.Default);
				return false;
			}
			if (this.fertilized.Value)
			{
				Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer2", true);
				location.playSound("cancel", null, null, SoundContext.Default);
				return false;
			}
			this.fertilized.Value = true;
			location.playSound("dirtyHit", null, null, SoundContext.Default);
			return true;
		}

		// Token: 0x06001A51 RID: 6737 RVA: 0x00136A2C File Offset: 0x00134C2C
		public bool instantDestroy(Vector2 tileLocation)
		{
			if (this.growthStage.Value >= 5)
			{
				return this.performTreeFall(null, 0, tileLocation);
			}
			if (this.growthStage.Value >= 3)
			{
				this.performBushDestroy(tileLocation);
				return true;
			}
			if (this.growthStage.Value >= 1)
			{
				this.performSproutDestroy(null, tileLocation);
				return true;
			}
			this.performSeedDestroy(null, tileLocation);
			return true;
		}

		// Token: 0x06001A52 RID: 6738 RVA: 0x00136A8C File Offset: 0x00134C8C
		protected void performSeedDestroy(Tool t, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
			});
			WildTreeData data = this.GetData();
			if (data != null && data.SeedItemId != null)
			{
				Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
				if (this.lastPlayerToHit.Value != 0L && lastHitBy.getEffectiveSkillLevel(2) >= 1)
				{
					Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, t.getLastFarmerToUse().UniqueMultiplayerID, location);
					return;
				}
				if (Game1.player.getEffectiveSkillLevel(2) >= 1)
				{
					Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, (t == null) ? Game1.player.UniqueMultiplayerID : t.getLastFarmerToUse().UniqueMultiplayerID, location);
				}
			}
		}

		// Token: 0x06001A53 RID: 6739 RVA: 0x00136B90 File Offset: 0x00134D90
		public void UpdateTapperProduct(Object tapper, Object previousOutput = null, bool onlyPerformRemovals = false)
		{
			if (tapper == null)
			{
				return;
			}
			WildTreeData data = this.GetData();
			if (data == null)
			{
				return;
			}
			float timeMultiplier = 1f;
			foreach (string contextTag in tapper.GetContextTags())
			{
				float multiplier;
				if (contextTag.StartsWithIgnoreCase("tapper_multiplier_") && float.TryParse(contextTag.Substring("tapper_multiplier_".Length), out multiplier))
				{
					timeMultiplier = 1f / multiplier;
					break;
				}
			}
			Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 73137.0, (double)this.Tile.X * 9.0, (double)this.Tile.Y * 13.0);
			Object output;
			int minutesUntilReady;
			if (this.TryGetTapperOutput(data.TapItems, (previousOutput != null) ? previousOutput.ItemId : null, random, timeMultiplier, out output, out minutesUntilReady) && (!onlyPerformRemovals || output == null))
			{
				tapper.heldObject.Value = output;
				tapper.minutesUntilReady.Value = minutesUntilReady;
			}
		}

		// Token: 0x06001A54 RID: 6740 RVA: 0x00136CB4 File Offset: 0x00134EB4
		protected bool TryGetTapperOutput(List<WildTreeTapItemData> tapItems, string previousItemId, Random r, float timeMultiplier, out Object output, out int minutesUntilReady)
		{
			if (tapItems != null)
			{
				previousItemId = ((previousItemId != null) ? ItemRegistry.QualifyItemId(previousItemId) : null);
				Func<string, string> <>9__0;
				foreach (WildTreeTapItemData tapData in tapItems)
				{
					if (GameStateQuery.CheckConditions(tapData.Condition, this.Location, null, null, null, null, null))
					{
						if (tapData.PreviousItemId != null)
						{
							bool found = false;
							foreach (string expectedPrevId in tapData.PreviousItemId)
							{
								found = (string.IsNullOrEmpty(expectedPrevId) ? (previousItemId == null) : previousItemId.EqualsIgnoreCase(ItemRegistry.QualifyItemId(expectedPrevId)));
								if (found)
								{
									break;
								}
							}
							if (!found)
							{
								continue;
							}
						}
						if (tapData.Season != null)
						{
							Season? season = tapData.Season;
							Season? season2 = this.localSeason;
							if (!(season.GetValueOrDefault() == season2.GetValueOrDefault() & season != null == (season2 != null)))
							{
								continue;
							}
						}
						Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
						WildTreeItemData drop = tapData;
						Farmer targetFarmer = lastHitBy;
						string fieldName = "TapItems";
						Func<string, string> formatItemId;
						if ((formatItemId = <>9__0) == null)
						{
							formatItemId = (<>9__0 = ((string id) => id.Replace("PREVIOUS_OUTPUT_ID", previousItemId)));
						}
						Item item = this.TryGetDrop(drop, r, targetFarmer, fieldName, formatItemId, null);
						if (item != null)
						{
							Object obj = item as Object;
							if (obj != null)
							{
								int daysUntilReady = (int)Utility.ApplyQuantityModifiers((float)tapData.DaysUntilReady, tapData.DaysUntilReadyModifiers, tapData.DaysUntilReadyModifierMode, this.Location, Game1.player, null, null, null);
								output = obj;
								minutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor((double)((float)daysUntilReady * timeMultiplier))));
								return true;
							}
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Wild tree '");
							defaultInterpolatedStringHandler.AppendFormatted(this.treeType.Value);
							defaultInterpolatedStringHandler.AppendLiteral("' can't produce item '");
							defaultInterpolatedStringHandler.AppendFormatted(item.ItemId);
							defaultInterpolatedStringHandler.AppendLiteral("': must be an object-type item.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				if (previousItemId != null)
				{
					return this.TryGetTapperOutput(tapItems, null, r, timeMultiplier, out output, out minutesUntilReady);
				}
			}
			output = null;
			minutesUntilReady = 0;
			return false;
		}

		// Token: 0x06001A55 RID: 6741 RVA: 0x00136F74 File Offset: 0x00135174
		protected void performSproutDestroy(Tool t, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), false, -1, false, null);
			if (t != null && t.Name.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1);
			}
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
			});
		}

		// Token: 0x06001A56 RID: 6742 RVA: 0x00137040 File Offset: 0x00135240
		protected void performBushDestroy(Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			WildTreeData data = this.GetData();
			if (data == null)
			{
				return;
			}
			Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), false, -1, false, this.GetChopDebrisColor(data));
			if (data.DropWoodOnChop || data.DropHardwoodOnLumberChop)
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 4.0), location);
			}
			List<WildTreeChopItemData> chopItems = data.ChopItems;
			if (chopItems != null && chopItems.Count > 0)
			{
				Random r;
				if (Game1.IsMultiplayer)
				{
					Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
					r = Game1.recentMultiplayerRandom;
				}
				else
				{
					r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
				}
				foreach (WildTreeChopItemData drop in data.ChopItems)
				{
					Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems", null, null);
					if (item != null)
					{
						Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location, -1, false);
					}
				}
			}
		}

		// Token: 0x06001A57 RID: 6743 RVA: 0x00137220 File Offset: 0x00135420
		protected bool performTreeFall(Tool t, int explosion, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			WildTreeData data = this.GetData();
			this.Location.objects.Remove(this.Tile);
			this.tapped.Value = false;
			if (!this.stump.Value)
			{
				if (t != null || explosion > 0)
				{
					location.playSound("treecrack", null, null, SoundContext.Default);
				}
				this.stump.Value = true;
				this.health.Value = 5f;
				this.falling.Value = true;
				if (t != null && t.getLastFarmerToUse().IsLocalPlayer)
				{
					if (t != null)
					{
						t.getLastFarmerToUse().gainExperience(2, 14);
					}
					if (((t != null) ? t.getLastFarmerToUse() : null) == null)
					{
						this.shakeLeft.Value = true;
					}
					else
					{
						this.shakeLeft.Value = ((float)t.getLastFarmerToUse().StandingPixel.X > (tileLocation.X + 0.5f) * 64f);
					}
					t.getLastFarmerToUse().stats.Increment("TreesChopped", 1);
				}
			}
			else
			{
				if (t != null && this.health.Value != -100f && t.getLastFarmerToUse().IsLocalPlayer && t != null)
				{
					t.getLastFarmerToUse().gainExperience(2, 2);
				}
				this.health.Value = -100f;
				if (data != null)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), false, -1, false, this.GetChopDebrisColor(data));
					Random r;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 2000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
						r = Game1.recentMultiplayerRandom;
					}
					else
					{
						r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
					}
					if (((t != null) ? t.getLastFarmerToUse() : null) == null)
					{
						if (location.Equals(Game1.currentLocation))
						{
							Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 2, location);
						}
						else
						{
							for (int i = 0; i < 2; i++)
							{
								Game1.createItemDebris(ItemRegistry.Create("(O)92", 1, 0, false), tileLocation * 64f, 2, location, -1, false);
							}
						}
					}
					else
					{
						Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value, false) ?? Game1.MasterPlayer;
						if (Game1.IsMultiplayer)
						{
							if (data.DropWoodOnChop)
							{
								Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 4.0), true, -1, false, null);
							}
							List<WildTreeChopItemData> chopItems = data.ChopItems;
							if (chopItems == null || chopItems.Count <= 0)
							{
								goto IL_50A;
							}
							using (List<WildTreeChopItemData>.Enumerator enumerator = data.ChopItems.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									WildTreeChopItemData drop = enumerator.Current;
									Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems", null, null);
									if (item != null)
									{
										if (item.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
										{
											item = ItemRegistry.Create("(O)422", item.Stack, item.Quality, false);
										}
										Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location, -1, false);
									}
								}
								goto IL_50A;
							}
						}
						if (data.DropWoodOnChop)
						{
							Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(5 + this.extraWoodCalculator(tileLocation))), true, -1, false, null);
						}
						List<WildTreeChopItemData> chopItems2 = data.ChopItems;
						if (chopItems2 != null && chopItems2.Count > 0)
						{
							foreach (WildTreeChopItemData drop2 in data.ChopItems)
							{
								Item item2 = this.TryGetDrop(drop2, r, lastHitBy, "ChopItems", null, null);
								if (item2 != null)
								{
									if (item2.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
									{
										item2 = ItemRegistry.Create("(O)422", item2.Stack, item2.Quality, false);
									}
									Game1.createMultipleItemDebris(item2, tileLocation * 64f, -2, location, -1, false);
								}
							}
						}
					}
					IL_50A:
					if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
					{
						Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
					}
					location.playSound("treethud", null, null, SoundContext.Default);
				}
				if (!this.falling.Value)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A58 RID: 6744 RVA: 0x001377DC File Offset: 0x001359DC
		protected void setSeason()
		{
			GameLocation location = this.Location;
			this.localSeason = new Season?((location is Desert || location is MineShaft) ? Season.Spring : Game1.GetSeasonForLocation(location));
		}

		// Token: 0x06001A59 RID: 6745 RVA: 0x00137814 File Offset: 0x00135A14
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			if (this.growthStage.Value < 5)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect;
				switch (this.growthStage.Value)
				{
				case 0:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16);
					break;
				case 1:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16);
					break;
				case 2:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16);
					break;
				default:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32);
					break;
				}
				spriteBatch.Draw(this.texture.Value, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), new Microsoft.Xna.Framework.Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
				return;
			}
			if (!this.falling.Value)
			{
				spriteBatch.Draw(this.texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32)), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
			}
			if (!this.stump.Value || this.falling.Value)
			{
				spriteBatch.Draw(this.texture.Value, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96)), Color.White, this.shakeRotation, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
			}
		}

		// Token: 0x06001A5A RID: 6746 RVA: 0x00137A34 File Offset: 0x00135C34
		public override void draw(SpriteBatch spriteBatch)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			Vector2 tileLocation = this.Tile;
			float baseSortPosition = (float)this.getBoundingBox().Bottom;
			WildTreeData data;
			if (this.texture.Value == null || !Tree.TryGetData(this.treeType.Value, out data))
			{
				IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
				spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Microsoft.Xna.Framework.Rectangle?(itemType.GetErrorSourceRect()), Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
				return;
			}
			if (this.growthStage.Value < 5)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect;
				switch (this.growthStage.Value)
				{
				case 0:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16);
					break;
				case 1:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16);
					break;
				case 2:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16);
					break;
				default:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32);
					break;
				}
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - (float)(sourceRect.Height * 4 - 64) + (float)((this.growthStage.Value >= 3) ? 128 : 64))), new Microsoft.Xna.Framework.Rectangle?(sourceRect), this.fertilized.Value ? Color.HotPink : Color.White, this.shakeRotation, new Vector2(8f, (float)((this.growthStage.Value >= 3) ? 32 : 16)), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.growthStage.Value == 0) ? 0.0001f : (baseSortPosition / 10000f));
			}
			else
			{
				if (!this.stump.Value || this.falling.Value)
				{
					if (this.IsLeafy())
					{
						spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Microsoft.Xna.Framework.Rectangle?(Tree.shadowSourceRect), Color.White * (1.5707964f - Math.Abs(this.shakeRotation)), 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
					}
					else
					{
						spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(469, 298, 42, 31)), Color.White * (1.5707964f - Math.Abs(this.shakeRotation)), 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
					}
					Microsoft.Xna.Framework.Rectangle source_rect = Tree.treeTopSourceRect;
					if ((data.UseAlternateSpriteWhenSeedReady && this.hasSeed.Value) || (data.UseAlternateSpriteWhenNotShaken && !this.wasShakenToday.Value))
					{
						source_rect.X = 48;
					}
					else
					{
						source_rect.X = 0;
					}
					if (this.hasMoss.Value)
					{
						source_rect.X = 96;
					}
					spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Microsoft.Xna.Framework.Rectangle?(source_rect), Color.White * this.alpha, this.shakeRotation, new Vector2(24f, 96f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 2f) / 10000f - tileLocation.X / 1000000f);
				}
				Microsoft.Xna.Framework.Rectangle stumpSource = Tree.stumpSourceRect;
				if (this.hasMoss.Value)
				{
					stumpSource.X += 96;
				}
				if (this.health.Value >= 1f || (!this.falling.Value && this.health.Value > -99f))
				{
					spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f - 64f)), new Microsoft.Xna.Framework.Rectangle?(stumpSource), Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, baseSortPosition / 10000f);
				}
				if (this.stump.Value && this.health.Value < 4f && this.health.Value > -99f)
				{
					spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Math.Min(2, (int)(3f - this.health.Value)) * 16, 144, 16, 16)), Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
				}
			}
			foreach (Leaf i in this.leaves)
			{
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, i.position), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(16 + i.type % 2 * 8, 112 + i.type / 2 * 8, 8, 8)), Color.White, i.rotation, Vector2.Zero, 4f, SpriteEffects.None, baseSortPosition / 10000f + 0.01f);
			}
		}

		// Token: 0x06001A5D RID: 6749 RVA: 0x001381F4 File Offset: 0x001363F4
		[CompilerGenerated]
		private Texture2D <resetTexture>g__LoadTexture|65_0()
		{
			this.TextureName = this.ChooseTexture();
			if (this.TextureName == null)
			{
				return null;
			}
			return Game1.content.Load<Texture2D>(this.TextureName);
		}

		// Token: 0x04001002 RID: 4098
		protected static Dictionary<string, WildTreeData> _WildTreeData;

		// Token: 0x04001003 RID: 4099
		protected static Dictionary<string, List<string>> _WildTreeSeedLookup;

		// Token: 0x04001004 RID: 4100
		public const float chanceForDailySeed = 0.05f;

		// Token: 0x04001005 RID: 4101
		public const float shakeRate = 0.015707964f;

		// Token: 0x04001006 RID: 4102
		public const float shakeDecayRate = 0.0030679617f;

		// Token: 0x04001007 RID: 4103
		public const int minWoodDebrisForFallenTree = 12;

		// Token: 0x04001008 RID: 4104
		public const int minWoodDebrisForStump = 5;

		// Token: 0x04001009 RID: 4105
		public const int startingHealth = 10;

		// Token: 0x0400100A RID: 4106
		public const int leafFallRate = 3;

		// Token: 0x0400100B RID: 4107
		public const int stageForMossGrowth = 14;

		// Token: 0x0400100C RID: 4108
		public const string bushyTree = "1";

		// Token: 0x0400100D RID: 4109
		public const string leafyTree = "2";

		// Token: 0x0400100E RID: 4110
		public const string pineTree = "3";

		// Token: 0x0400100F RID: 4111
		public const string winterTree1 = "4";

		// Token: 0x04001010 RID: 4112
		public const string winterTree2 = "5";

		// Token: 0x04001011 RID: 4113
		public const string palmTree = "6";

		// Token: 0x04001012 RID: 4114
		public const string mushroomTree = "7";

		// Token: 0x04001013 RID: 4115
		public const string mahoganyTree = "8";

		// Token: 0x04001014 RID: 4116
		public const string palmTree2 = "9";

		// Token: 0x04001015 RID: 4117
		public const string greenRainTreeBushy = "10";

		// Token: 0x04001016 RID: 4118
		public const string greenRainTreeLeafy = "11";

		// Token: 0x04001017 RID: 4119
		public const string greenRainTreeFern = "12";

		// Token: 0x04001018 RID: 4120
		public const string mysticTree = "13";

		// Token: 0x04001019 RID: 4121
		public const int seedStage = 0;

		// Token: 0x0400101A RID: 4122
		public const int sproutStage = 1;

		// Token: 0x0400101B RID: 4123
		public const int saplingStage = 2;

		// Token: 0x0400101C RID: 4124
		public const int bushStage = 3;

		// Token: 0x0400101D RID: 4125
		public const int treeStage = 5;

		// Token: 0x0400101F RID: 4127
		[XmlIgnore]
		public Lazy<Texture2D> texture;

		// Token: 0x04001020 RID: 4128
		protected Season? localSeason;

		// Token: 0x04001021 RID: 4129
		[XmlElement("growthStage")]
		public readonly NetInt growthStage = new NetInt();

		// Token: 0x04001022 RID: 4130
		[XmlElement("treeType")]
		public readonly NetString treeType = new NetString();

		// Token: 0x04001023 RID: 4131
		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		// Token: 0x04001024 RID: 4132
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		// Token: 0x04001025 RID: 4133
		[XmlElement("stump")]
		public readonly NetBool stump = new NetBool();

		// Token: 0x04001026 RID: 4134
		[XmlElement("tapped")]
		public readonly NetBool tapped = new NetBool();

		// Token: 0x04001027 RID: 4135
		[XmlElement("hasSeed")]
		public readonly NetBool hasSeed = new NetBool();

		// Token: 0x04001028 RID: 4136
		[XmlElement("hasMoss")]
		public readonly NetBool hasMoss = new NetBool();

		// Token: 0x04001029 RID: 4137
		[XmlElement("isTemporaryGreenRainTree")]
		public readonly NetBool isTemporaryGreenRainTree = new NetBool();

		// Token: 0x0400102A RID: 4138
		[XmlIgnore]
		public readonly NetBool wasShakenToday = new NetBool();

		// Token: 0x0400102B RID: 4139
		[XmlElement("fertilized")]
		public readonly NetBool fertilized = new NetBool();

		// Token: 0x0400102C RID: 4140
		[XmlIgnore]
		public readonly NetBool shakeLeft = new NetBool().Interpolated(false, false);

		// Token: 0x0400102D RID: 4141
		[XmlIgnore]
		public readonly NetBool falling = new NetBool();

		// Token: 0x0400102E RID: 4142
		[XmlIgnore]
		public readonly NetBool destroy = new NetBool();

		// Token: 0x0400102F RID: 4143
		[XmlIgnore]
		public float shakeRotation;

		// Token: 0x04001030 RID: 4144
		[XmlIgnore]
		public float maxShake;

		// Token: 0x04001031 RID: 4145
		[XmlIgnore]
		public float alpha = 1f;

		// Token: 0x04001032 RID: 4146
		private List<Leaf> leaves = new List<Leaf>();

		// Token: 0x04001033 RID: 4147
		[XmlIgnore]
		public readonly NetLong lastPlayerToHit = new NetLong();

		// Token: 0x04001034 RID: 4148
		[XmlIgnore]
		public float shakeTimer;

		// Token: 0x04001035 RID: 4149
		[XmlElement("stopGrowingMoss")]
		public readonly NetBool stopGrowingMoss = new NetBool();

		// Token: 0x04001036 RID: 4150
		public static Microsoft.Xna.Framework.Rectangle treeTopSourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96);

		// Token: 0x04001037 RID: 4151
		public static Microsoft.Xna.Framework.Rectangle stumpSourceRect = new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32);

		// Token: 0x04001038 RID: 4152
		public static Microsoft.Xna.Framework.Rectangle shadowSourceRect = new Microsoft.Xna.Framework.Rectangle(663, 1011, 41, 30);
	}
}
