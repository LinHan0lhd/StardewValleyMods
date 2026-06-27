using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002C9 RID: 713
	public class CommunityCenter : GameLocation
	{
		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06002E2C RID: 11820 RVA: 0x00241178 File Offset: 0x0023F378
		[XmlElement("bundles")]
		public NetBundles bundles
		{
			get
			{
				return Game1.netWorldState.Value.Bundles;
			}
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06002E2D RID: 11821 RVA: 0x00241189 File Offset: 0x0023F389
		[XmlElement("bundleRewards")]
		public NetIntDictionary<bool, NetBool> bundleRewards
		{
			get
			{
				return Game1.netWorldState.Value.BundleRewards;
			}
		}

		// Token: 0x06002E2E RID: 11822 RVA: 0x0024119C File Offset: 0x0023F39C
		public CommunityCenter()
		{
			this.initAreaBundleConversions();
			this.refreshBundlesIngredientsInfo();
		}

		// Token: 0x06002E2F RID: 11823 RVA: 0x0024124C File Offset: 0x0023F44C
		public CommunityCenter(string map_path, string name) : base(map_path, name)
		{
			this.initAreaBundleConversions();
			this.refreshBundlesIngredientsInfo();
		}

		// Token: 0x06002E30 RID: 11824 RVA: 0x00241300 File Offset: 0x0023F500
		public CommunityCenter(string name) : base("Maps\\CommunityCenter_Ruins", name)
		{
			this.initAreaBundleConversions();
			this.refreshBundlesIngredientsInfo();
		}

		// Token: 0x06002E31 RID: 11825 RVA: 0x002413B8 File Offset: 0x0023F5B8
		public void refreshBundlesIngredientsInfo()
		{
			this.bundlesIngredientsInfo = new Dictionary<string, List<List<int>>>();
			Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;
			Dictionary<int, bool[]> bundlesD = this.bundlesDict();
			foreach (KeyValuePair<string, string> v in bundleData)
			{
				string[] array = v.Key.Split('/', StringSplitOptions.None);
				int bundleIndex = Convert.ToInt32(array[1]);
				string areaName = array[0];
				string[] ingredientSplit = ArgUtility.SplitBySpace(v.Value.Split('/', StringSplitOptions.None)[2]);
				if (this.shouldNoteAppearInArea(CommunityCenter.getAreaNumberFromName(areaName)))
				{
					for (int i = 0; i < ingredientSplit.Length; i += 3)
					{
						if (bundlesD.ContainsKey(bundleIndex) && !bundlesD[bundleIndex][i / 3])
						{
							int categoryOrId;
							string key;
							if (int.TryParse(ingredientSplit[i], out categoryOrId) && categoryOrId < 0)
							{
								key = categoryOrId.ToString();
							}
							else
							{
								ParsedItemData data = ItemRegistry.GetData(ingredientSplit[i]);
								key = ((data != null) ? data.QualifiedItemId : ("(O)" + ingredientSplit[i]));
							}
							int itemStack = Convert.ToInt32(ingredientSplit[i + 1]);
							int itemQuality = Convert.ToInt32(ingredientSplit[i + 2]);
							List<List<int>> ingredients;
							if (!this.bundlesIngredientsInfo.TryGetValue(key, out ingredients))
							{
								ingredients = (this.bundlesIngredientsInfo[key] = new List<List<int>>());
							}
							ingredients.Add(new List<int>
							{
								bundleIndex,
								itemStack,
								itemQuality
							});
						}
					}
				}
			}
		}

		// Token: 0x06002E32 RID: 11826 RVA: 0x0024155C File Offset: 0x0023F75C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.warehouse, "warehouse").AddField(this.areasComplete, "areasComplete").AddField(this.numberOfStarsOnPlaque, "numberOfStarsOnPlaque").AddField(this.newJunimoNoteCheckEvent, "newJunimoNoteCheckEvent").AddField(this.restoreAreaCutsceneEvent, "restoreAreaCutsceneEvent").AddField(this.areaCompleteRewardEvent, "areaCompleteRewardEvent").AddField(this.missedRewardsChest, "missedRewardsChest").AddField(this.showMissedRewardsChestEvent, "showMissedRewardsChestEvent").AddField(this.missedRewardsChestVisible, "missedRewardsChestVisible");
			this.newJunimoNoteCheckEvent.onEvent += this.doCheckForNewJunimoNotes;
			this.restoreAreaCutsceneEvent.onEvent += this.doRestoreAreaCutscene;
			this.areaCompleteRewardEvent.onEvent += this.doAreaCompleteReward;
			this.showMissedRewardsChestEvent.onEvent += this.doShowMissedRewardsChest;
		}

		// Token: 0x06002E33 RID: 11827 RVA: 0x00241664 File Offset: 0x0023F864
		private void initAreaBundleConversions()
		{
			this.areaToBundleDictionary = new Dictionary<int, List<int>>();
			this.bundleToAreaDictionary = new Dictionary<int, int>();
			for (int i = 0; i < 7; i++)
			{
				this.areaToBundleDictionary.Add(i, new List<int>());
				NetMutex j = new NetMutex();
				this.bundleMutexes.Add(j);
				base.NetFields.AddField(j.NetFields, "m.NetFields");
			}
			foreach (KeyValuePair<string, string> v in Game1.netWorldState.Value.BundleData)
			{
				int bundleIndex = Convert.ToInt32(v.Key.Split('/', StringSplitOptions.None)[1]);
				this.areaToBundleDictionary[CommunityCenter.getAreaNumberFromName(v.Key.Split('/', StringSplitOptions.None)[0])].Add(bundleIndex);
				this.bundleToAreaDictionary.Add(bundleIndex, CommunityCenter.getAreaNumberFromName(v.Key.Split('/', StringSplitOptions.None)[0]));
			}
		}

		// Token: 0x06002E34 RID: 11828 RVA: 0x00241778 File Offset: 0x0023F978
		public static int getAreaNumberFromName(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 5:
					if (!(name == "Vault"))
					{
						return -1;
					}
					return 4;
				case 6:
					if (!(name == "Pantry"))
					{
						return -1;
					}
					return 0;
				case 7:
				case 12:
				case 15:
				case 16:
				case 17:
				case 18:
					return -1;
				case 8:
				{
					char c = name[0];
					if (c != 'B')
					{
						if (c != 'F')
						{
							return -1;
						}
						if (!(name == "FishTank"))
						{
							return -1;
						}
						return 2;
					}
					else
					{
						if (!(name == "Bulletin"))
						{
							return -1;
						}
						return 5;
					}
					break;
				}
				case 9:
					if (!(name == "Fish Tank"))
					{
						return -1;
					}
					return 2;
				case 10:
				{
					char c = name[0];
					if (c != 'B')
					{
						if (c != 'C')
						{
							return -1;
						}
						if (!(name == "CraftsRoom"))
						{
							return -1;
						}
					}
					else
					{
						if (!(name == "BoilerRoom"))
						{
							return -1;
						}
						return 3;
					}
					break;
				}
				case 11:
				{
					char c = name[0];
					if (c != 'B')
					{
						if (c != 'C')
						{
							return -1;
						}
						if (!(name == "Crafts Room"))
						{
							return -1;
						}
					}
					else
					{
						if (!(name == "Boiler Room"))
						{
							return -1;
						}
						return 3;
					}
					break;
				}
				case 13:
					if (!(name == "BulletinBoard"))
					{
						return -1;
					}
					return 5;
				case 14:
					if (!(name == "Bulletin Board"))
					{
						return -1;
					}
					return 5;
				case 19:
					if (!(name == "Abandoned Joja Mart"))
					{
						return -1;
					}
					return 6;
				default:
					return -1;
				}
				return 1;
			}
			return -1;
		}

		// Token: 0x06002E35 RID: 11829 RVA: 0x002418FC File Offset: 0x0023FAFC
		private Point getNotePosition(int area)
		{
			switch (area)
			{
			case 0:
				return new Point(14, 5);
			case 1:
				return new Point(14, 23);
			case 2:
				return new Point(40, 10);
			case 3:
				return new Point(63, 14);
			case 4:
				return new Point(55, 6);
			case 5:
				return new Point(46, 11);
			default:
				return Point.Zero;
			}
		}

		// Token: 0x06002E36 RID: 11830 RVA: 0x00241968 File Offset: 0x0023FB68
		public void addJunimoNote(int area)
		{
			Point position = this.getNotePosition(area);
			if (!position.Equals(Vector2.Zero))
			{
				StaticTile[] tileFrames = CommunityCenter.getJunimoNoteTileFrames(area, this.map);
				string layer = (area == 5) ? "Front" : "Buildings";
				this.map.RequireLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(this.map.RequireLayer(layer), tileFrames, 70L);
				IDictionary<string, LightSource> currentLightSources = Game1.currentLightSources;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
				defaultInterpolatedStringHandler.AppendFormatted("CommunityCenter");
				defaultInterpolatedStringHandler.AppendLiteral("_Area");
				defaultInterpolatedStringHandler.AppendFormatted<int>(area);
				currentLightSources.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f)
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64 - 12), (float)(position.Y * 64 - 12)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					scale = 0.75f,
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 50
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64 - 12), (float)(position.Y * 64 + 12)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 100
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					layerDepth = 1f,
					scale = 0.75f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 150
				});
			}
		}

		// Token: 0x06002E37 RID: 11831 RVA: 0x00241C90 File Offset: 0x0023FE90
		public int numberOfCompleteBundles()
		{
			int number = 0;
			foreach (KeyValuePair<int, bool[]> v in this.bundles.Pairs)
			{
				number++;
				for (int i = 0; i < v.Value.Length; i++)
				{
					if (!v.Value[i])
					{
						number--;
						break;
					}
				}
			}
			return number;
		}

		// Token: 0x06002E38 RID: 11832 RVA: 0x00241D18 File Offset: 0x0023FF18
		public void addStarToPlaque()
		{
			NetInt netInt = this.numberOfStarsOnPlaque;
			int value = netInt.Value;
			netInt.Value = value + 1;
		}

		// Token: 0x06002E39 RID: 11833 RVA: 0x00241D3C File Offset: 0x0023FF3C
		private string getMessageForAreaCompletion()
		{
			int areasComplete = this.getNumberOfAreasComplete();
			if (areasComplete >= 1 && areasComplete <= 6)
			{
				return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaCompletion" + areasComplete.ToString(), Game1.player.Name);
			}
			return "";
		}

		// Token: 0x06002E3A RID: 11834 RVA: 0x00241D84 File Offset: 0x0023FF84
		private int getNumberOfAreasComplete()
		{
			int complete = 0;
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				if (this.areasComplete[i])
				{
					complete++;
				}
			}
			return complete;
		}

		// Token: 0x06002E3B RID: 11835 RVA: 0x00241DBC File Offset: 0x0023FFBC
		public Dictionary<int, bool[]> bundlesDict()
		{
			return (from kvp in this.bundles.Pairs
			select new KeyValuePair<int, bool[]>(kvp.Key, kvp.Value.ToArray<bool>())).ToDictionary((KeyValuePair<int, bool[]> x) => x.Key, (KeyValuePair<int, bool[]> y) => y.Value);
		}

		// Token: 0x06002E3C RID: 11836 RVA: 0x00241E40 File Offset: 0x00240040
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (who.IsLocalPlayer && ArgUtility.Get(action, 0, null, true) == "MissedRewards")
			{
				this.missedRewardsChest.Value.mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new ItemGrabMenu(this.missedRewardsChest.Value.Items, false, true, null, null, null, new ItemGrabMenu.behaviorOnItemSelect(this.rewardGrabbed), false, true, true, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
					Game1.activeClickableMenu.exitFunction = delegate()
					{
						this.missedRewardsChest.Value.mutex.ReleaseLock();
						this.checkForMissedRewards();
					};
				}, null);
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06002E3D RID: 11837 RVA: 0x00241E97 File Offset: 0x00240097
		private void rewardGrabbed(Item item, Farmer who)
		{
			this.bundleRewards[item.SpecialVariable] = false;
		}

		// Token: 0x06002E3E RID: 11838 RVA: 0x00241EAC File Offset: 0x002400AC
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "indoors");
			if (tileIndexAt == 1799)
			{
				if (this.numberOfCompleteBundles() > 2)
				{
					this.checkBundle(5);
				}
				return true;
			}
			if (tileIndexAt - 1824 > 9)
			{
				return base.checkAction(tileLocation, viewport, who);
			}
			this.checkBundle(this.getAreaNumberFromLocation(who.Tile));
			return true;
		}

		// Token: 0x06002E3F RID: 11839 RVA: 0x00241F10 File Offset: 0x00240110
		public void checkBundle(int area)
		{
			this.bundleMutexes[area].RequestLock(delegate
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(area, this.bundlesDict());
			}, null);
		}

		// Token: 0x06002E40 RID: 11840 RVA: 0x00241F54 File Offset: 0x00240154
		public void addJunimoNoteViewportTarget(int area)
		{
			if (this.junimoNotesViewportTargets == null)
			{
				this.junimoNotesViewportTargets = new List<int>();
			}
			if (!this.junimoNotesViewportTargets.Contains(area))
			{
				this.junimoNotesViewportTargets.Add(area);
			}
		}

		// Token: 0x06002E41 RID: 11841 RVA: 0x00241F83 File Offset: 0x00240183
		public void checkForNewJunimoNotes()
		{
			this.newJunimoNoteCheckEvent.Fire();
		}

		// Token: 0x06002E42 RID: 11842 RVA: 0x00241F90 File Offset: 0x00240190
		private void doCheckForNewJunimoNotes()
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				if (!this.isJunimoNoteAtArea(i) && this.shouldNoteAppearInArea(i))
				{
					this.addJunimoNoteViewportTarget(i);
				}
			}
		}

		// Token: 0x06002E43 RID: 11843 RVA: 0x00241FD8 File Offset: 0x002401D8
		public bool isJunimoNoteAtArea(int area)
		{
			Point p = this.getNotePosition(area);
			if (area == 5)
			{
				return this.map.RequireLayer("Front").Tiles[p.X, p.Y] != null;
			}
			return this.map.RequireLayer("Buildings").Tiles[p.X, p.Y] != null;
		}

		// Token: 0x06002E44 RID: 11844 RVA: 0x00242044 File Offset: 0x00240244
		public bool shouldNoteAppearInArea(int area)
		{
			bool isAreaComplete = true;
			for (int i = 0; i < this.areaToBundleDictionary[area].Count; i++)
			{
				foreach (int bundleIndex in this.areaToBundleDictionary[area])
				{
					bool[] bundleEntries;
					if (this.bundles.TryGetValue(bundleIndex, out bundleEntries))
					{
						int bundleLength = bundleEntries.Length / 3;
						for (int j = 0; j < bundleLength; j++)
						{
							if (!bundleEntries[j])
							{
								isAreaComplete = false;
								break;
							}
						}
					}
					if (!isAreaComplete)
					{
						break;
					}
				}
			}
			if (area >= 0 && !isAreaComplete)
			{
				switch (area)
				{
				case 0:
				case 2:
					if (this.numberOfCompleteBundles() > 0)
					{
						return true;
					}
					break;
				case 1:
					return true;
				case 3:
					if (this.numberOfCompleteBundles() > 1)
					{
						return true;
					}
					break;
				case 4:
					if (this.numberOfCompleteBundles() > 3)
					{
						return true;
					}
					break;
				case 5:
					if (this.numberOfCompleteBundles() > 2)
					{
						return true;
					}
					break;
				case 6:
					if (Utility.HasAnyPlayerSeenEvent("191393"))
					{
						return true;
					}
					break;
				}
			}
			return false;
		}

		// Token: 0x06002E45 RID: 11845 RVA: 0x00242158 File Offset: 0x00240358
		public override void updateMap()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				this.warehouse.Value = true;
				this.mapPath.Value = "Maps\\CommunityCenter_Joja";
			}
			base.updateMap();
		}

		// Token: 0x06002E46 RID: 11846 RVA: 0x00242192 File Offset: 0x00240392
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (this.areAllAreasComplete())
			{
				this.mapPath.Value = "Maps\\CommunityCenter_Refurbished";
				this.updateMap();
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06002E47 RID: 11847 RVA: 0x002421BC File Offset: 0x002403BC
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (this.areAllAreasComplete())
			{
				this.mapPath.Value = "Maps\\CommunityCenter_Refurbished";
				this.addFishTank();
			}
			this._isWatchingJunimoGoodbye = false;
			if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !this.areAllAreasComplete())
			{
				for (int i = 0; i < this.areasComplete.Count; i++)
				{
					if (this.shouldNoteAppearInArea(i))
					{
						this.characters.Add(new Junimo(new Vector2((float)this.getNotePosition(i).X, (float)(this.getNotePosition(i).Y + 2)) * 64f, i, false));
					}
				}
			}
			this.numberOfStarsOnPlaque.Value = 0;
			for (int j = 0; j < this.areasComplete.Count; j++)
			{
				if (this.areasComplete[j])
				{
					NetInt netInt = this.numberOfStarsOnPlaque;
					int value = netInt.Value;
					netInt.Value = value + 1;
				}
			}
			this.checkForMissedRewards();
		}

		// Token: 0x06002E48 RID: 11848 RVA: 0x002422BC File Offset: 0x002404BC
		private void doShowMissedRewardsChest(bool isVisible)
		{
			int tileX = (int)this.missedRewardsChestTile.X;
			int tileY = (int)this.missedRewardsChestTile.Y;
			base.removeMapTile(tileX, tileY, "Buildings");
			if (isVisible)
			{
				base.setMapTile(tileX, tileY, 5, "Buildings", "indoors2", "MissedRewards", true);
			}
		}

		// Token: 0x06002E49 RID: 11849 RVA: 0x00242310 File Offset: 0x00240510
		private void checkForMissedRewards()
		{
			HashSet<int> visited_areas = new HashSet<int>();
			bool hasUnclaimedRewards = false;
			this.missedRewardsChest.Value.Items.Clear();
			List<Item> rewards = new List<Item>();
			foreach (int key in this.bundleRewards.Keys)
			{
				int area = this.bundleToAreaDictionary[key];
				if (this.bundleRewards[key] && this.areasComplete.Count > area && this.areasComplete[area] && !visited_areas.Contains(area))
				{
					visited_areas.Add(area);
					hasUnclaimedRewards = true;
					rewards.Clear();
					JunimoNoteMenu.GetBundleRewards(area, rewards);
					foreach (Item item in rewards)
					{
						this.missedRewardsChest.Value.addItem(item);
					}
				}
			}
			if (hasUnclaimedRewards != this.missedRewardsChestVisible.Value)
			{
				this.showMissedRewardsChestEvent.Fire(hasUnclaimedRewards);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(Game1.random.Choose(5, 46), this.missedRewardsChestTile * 64f + new Vector2(16f, 16f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f
					}
				});
				this.missedRewardsChestVisible.Value = hasUnclaimedRewards;
			}
		}

		// Token: 0x06002E4A RID: 11850 RVA: 0x002424D0 File Offset: 0x002406D0
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !this.areAllAreasComplete())
			{
				for (int i = 0; i < this.areasComplete.Count; i++)
				{
					if (this.areasComplete[i])
					{
						this.loadArea(i, false);
					}
					else if (this.shouldNoteAppearInArea(i))
					{
						this.addJunimoNote(i);
					}
				}
			}
			this.doShowMissedRewardsChest(this.missedRewardsChestVisible.Value);
		}

		// Token: 0x06002E4B RID: 11851 RVA: 0x00242551 File Offset: 0x00240751
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.eventUp && !this.areAllAreasComplete())
			{
				Game1.changeMusicTrack("communityCenter", false, MusicContext.Default);
			}
		}

		// Token: 0x06002E4C RID: 11852 RVA: 0x00242574 File Offset: 0x00240774
		private int getAreaNumberFromLocation(Vector2 tileLocation)
		{
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				if (this.getAreaBounds(i).Contains((int)tileLocation.X, (int)tileLocation.Y))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06002E4D RID: 11853 RVA: 0x002425BC File Offset: 0x002407BC
		private Microsoft.Xna.Framework.Rectangle getAreaBounds(int area)
		{
			switch (area)
			{
			case 0:
				return new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 11);
			case 1:
				return new Microsoft.Xna.Framework.Rectangle(0, 12, 21, 17);
			case 2:
				return new Microsoft.Xna.Framework.Rectangle(35, 4, 9, 9);
			case 3:
				return new Microsoft.Xna.Framework.Rectangle(52, 9, 16, 12);
			case 4:
				return new Microsoft.Xna.Framework.Rectangle(45, 0, 15, 9);
			case 5:
				return new Microsoft.Xna.Framework.Rectangle(22, 13, 28, 9);
			case 7:
				return new Microsoft.Xna.Framework.Rectangle(44, 10, 6, 3);
			case 8:
				return new Microsoft.Xna.Framework.Rectangle(22, 4, 13, 9);
			}
			return Microsoft.Xna.Framework.Rectangle.Empty;
		}

		// Token: 0x06002E4E RID: 11854 RVA: 0x00242662 File Offset: 0x00240862
		protected void removeJunimo()
		{
			this.characters.RemoveWhere((NPC npc) => npc is Junimo);
		}

		// Token: 0x06002E4F RID: 11855 RVA: 0x0024268F File Offset: 0x0024088F
		public override void cleanupBeforeSave()
		{
			this.removeJunimo();
		}

		// Token: 0x06002E50 RID: 11856 RVA: 0x00242697 File Offset: 0x00240897
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (this.farmers.Count <= 1)
			{
				this.removeJunimo();
			}
		}

		// Token: 0x06002E51 RID: 11857 RVA: 0x002426B4 File Offset: 0x002408B4
		public bool isBundleComplete(int bundleIndex)
		{
			for (int i = 0; i < this.bundles[bundleIndex].Length; i++)
			{
				if (!this.bundles[bundleIndex][i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002E52 RID: 11858 RVA: 0x002426F0 File Offset: 0x002408F0
		public bool couldThisIngredienteBeUsedInABundle(Object o)
		{
			if (!o.bigCraftable.Value)
			{
				List<List<int>> ingredientsById;
				if (this.bundlesIngredientsInfo.TryGetValue(o.QualifiedItemId, out ingredientsById))
				{
					foreach (List<int> i in ingredientsById)
					{
						if (o.Quality >= i[2])
						{
							return true;
						}
					}
				}
				List<List<int>> ingredientsByCategory;
				if (o.Category < 0 && this.bundlesIngredientsInfo.TryGetValue(o.Category.ToString(), out ingredientsByCategory))
				{
					foreach (List<int> j in ingredientsByCategory)
					{
						if (o.Quality >= j[2])
						{
							return true;
						}
					}
					return false;
				}
				return false;
			}
			return false;
		}

		// Token: 0x06002E53 RID: 11859 RVA: 0x002427EC File Offset: 0x002409EC
		public void areaCompleteReward(int whichArea)
		{
			this.areaCompleteRewardEvent.Fire(whichArea);
		}

		// Token: 0x06002E54 RID: 11860 RVA: 0x002427FC File Offset: 0x002409FC
		private void doAreaCompleteReward(int whichArea)
		{
			string mailReceivedID = "";
			switch (whichArea)
			{
			case 0:
				mailReceivedID = "ccPantry";
				break;
			case 1:
				mailReceivedID = "ccCraftsRoom";
				break;
			case 2:
				mailReceivedID = "ccFishTank";
				break;
			case 3:
				mailReceivedID = "ccBoilerRoom";
				break;
			case 4:
				mailReceivedID = "ccVault";
				break;
			case 5:
				mailReceivedID = "ccBulletin";
				Game1.addMailForTomorrow("ccBulletinThankYou", false, false);
				break;
			}
			if (mailReceivedID.Length > 0 && !Game1.player.mailReceived.Contains(mailReceivedID))
			{
				Game1.player.mailForTomorrow.Add(mailReceivedID + "%&NL&%");
			}
		}

		// Token: 0x06002E55 RID: 11861 RVA: 0x002428A0 File Offset: 0x00240AA0
		public void loadArea(int area, bool showEffects = true)
		{
			Microsoft.Xna.Framework.Rectangle areaToRefurbish = this.getAreaBounds(area);
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\CommunityCenter_Refurbished");
			Map override_map = refurbishedMap;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler.AppendLiteral("CommunityCenter_Refurbished");
			defaultInterpolatedStringHandler.AppendFormatted<int>(area);
			base.ApplyMapOverride(override_map, defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Rectangle?(areaToRefurbish), new Microsoft.Xna.Framework.Rectangle?(areaToRefurbish), null);
			Layer refurbishedBuildingsLayer = refurbishedMap.RequireLayer("Buildings");
			Layer refurbishedFrontLayer = refurbishedMap.RequireLayer("Front");
			Layer refurbishedPathsLayer = refurbishedMap.RequireLayer("Paths");
			foreach (Point tile in areaToRefurbish.GetPoints())
			{
				int x = tile.X;
				int y = tile.Y;
				Tile fromTile = refurbishedBuildingsLayer.Tiles[x, y];
				if (fromTile != null)
				{
					base.adjustMapLightPropertiesForLamp(fromTile.TileIndex, x, y, "Buildings");
					if (Game1.player.currentLocation == this && Game1.player.TilePoint.X == x && Game1.player.TilePoint.Y == y)
					{
						Game1.player.Position = new Vector2(2080f, 576f);
					}
				}
				fromTile = refurbishedFrontLayer.Tiles[x, y];
				if (fromTile != null)
				{
					base.adjustMapLightPropertiesForLamp(fromTile.TileIndex, x, y, "Front");
				}
				fromTile = refurbishedPathsLayer.Tiles[x, y];
				if (fromTile != null && fromTile.TileIndex == 8)
				{
					IDictionary<string, LightSource> currentLightSources = Game1.currentLightSources;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 4);
					defaultInterpolatedStringHandler.AppendFormatted("CommunityCenter");
					defaultInterpolatedStringHandler.AppendLiteral("_Area");
					defaultInterpolatedStringHandler.AppendFormatted<int>(area);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(tile.X);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(tile.Y);
					currentLightSources.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)(x * 64), (float)(y * 64)), 2f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				}
				if (showEffects && Game1.random.NextDouble() < 0.58 && refurbishedBuildingsLayer.Tiles[x, y] == null)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(x * 64), (float)(y * 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2((float)Game1.random.Next(17) / 10f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = Game1.random.Next(500)
					});
				}
			}
			if ((area == 5 || area == 8) && this.missedRewardsChestVisible.Value)
			{
				this.doShowMissedRewardsChest(true);
			}
			if (area != 2)
			{
				if (area == 5)
				{
					this.loadArea(7, true);
				}
			}
			else
			{
				this.addFishTank();
			}
			base.addLightGlows();
		}

		// Token: 0x06002E56 RID: 11862 RVA: 0x00242BEC File Offset: 0x00240DEC
		public void addFishTank()
		{
			bool found = false;
			foreach (Furniture f in this.furniture)
			{
				if (f.QualifiedItemId == "(F)CCFishTank")
				{
					f.AllowLocalRemoval = false;
					found = true;
					break;
				}
			}
			if (!found)
			{
				FishTankFurniture f2 = new FishTankFurniture("CCFishTank", new Vector2(38f, 9f));
				f2.CanBeGrabbed = false;
				f2.AllowLocalRemoval = false;
				f2.Fragility = 2;
				f2.heldItems.Add(ItemRegistry.Create("(O)143", 1, 0, false));
				f2.heldItems.Add(ItemRegistry.Create("(O)145", 1, 0, false));
				f2.heldItems.Add(ItemRegistry.Create("(O)721", 1, 0, false));
				this.furniture.Add(f2);
			}
		}

		// Token: 0x06002E57 RID: 11863 RVA: 0x00242CE0 File Offset: 0x00240EE0
		public void restoreAreaCutscene(int whichArea)
		{
			this.restoreAreaCutsceneEvent.Fire(whichArea);
		}

		// Token: 0x06002E58 RID: 11864 RVA: 0x00242CEE File Offset: 0x00240EEE
		public void markAreaAsComplete(int area)
		{
			if (Game1.currentLocation == this)
			{
				this.areasComplete[area] = true;
			}
			if (this.areAllAreasComplete() && Game1.currentLocation == this)
			{
				this._isWatchingJunimoGoodbye = true;
			}
		}

		// Token: 0x06002E59 RID: 11865 RVA: 0x00242D1C File Offset: 0x00240F1C
		private void doRestoreAreaCutscene(int whichArea)
		{
			this.markAreaAsComplete(whichArea);
			this.restoreAreaIndex = whichArea;
			this.restoreAreaPhase = 0;
			this.restoreAreaTimer = 1000;
			if (Game1.player.currentLocation == this)
			{
				Game1.freezeControls = true;
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			this.checkForMissedRewards();
		}

		// Token: 0x06002E5A RID: 11866 RVA: 0x00242D70 File Offset: 0x00240F70
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.restoreAreaCutsceneEvent.Poll();
			this.newJunimoNoteCheckEvent.Poll();
			this.areaCompleteRewardEvent.Poll();
			this.showMissedRewardsChestEvent.Poll();
			foreach (NetMutex i in this.bundleMutexes)
			{
				i.Update(this);
				if (i.IsLockHeld() && Game1.activeClickableMenu == null)
				{
					i.ReleaseLock();
				}
			}
		}

		// Token: 0x06002E5B RID: 11867 RVA: 0x00242E0C File Offset: 0x0024100C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.missedRewardsChest.Value.updateWhenCurrentLocation(time);
			if (this.restoreAreaTimer > 0)
			{
				int old = this.restoreAreaTimer;
				this.restoreAreaTimer -= time.ElapsedGameTime.Milliseconds;
				switch (this.restoreAreaPhase)
				{
				case 0:
					if (this.restoreAreaTimer <= 0)
					{
						this.restoreAreaTimer = 3000;
						this.restoreAreaPhase = 1;
						if (Game1.player.currentLocation == this)
						{
							Game1.player.faceDirection(2);
							Game1.player.jump();
							Game1.player.jitterStrength = 1f;
							Game1.player.showFrame(94, false);
							return;
						}
					}
					break;
				case 1:
					if (Game1.IsMasterGame && Game1.random.NextDouble() < 0.4)
					{
						Vector2 v = Utility.getRandomPositionInThisRectangle(this.getAreaBounds(this.restoreAreaIndex), Game1.random);
						Junimo i = new Junimo(v * 64f, this.restoreAreaIndex, true);
						if (!base.isCollidingPosition(i.GetBoundingBox(), Game1.viewport, i))
						{
							this.characters.Add(i);
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(Game1.random.Choose(5, 46), v * 64f + new Vector2(16f, 16f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
								{
									layerDepth = 1f
								}
							});
							base.localSound("tinyWhip", null, null, SoundContext.Default);
						}
					}
					if (this.restoreAreaTimer <= 0)
					{
						this.restoreAreaTimer = 999999;
						this.restoreAreaPhase = 2;
						if (Game1.player.currentLocation != this)
						{
							break;
						}
						Game1.screenGlowOnce(Color.White, true, 0.005f, 1f);
						Game1.playSound("wind", out this.buildUpSound);
						this.buildUpSound.SetVariable("Volume", 0f);
						this.buildUpSound.SetVariable("Frequency", 0f);
						Game1.player.jitterStrength = 2f;
						Game1.player.stopShowingFrame();
					}
					Game1.drawLighting = false;
					return;
				case 2:
					if (this.buildUpSound != null)
					{
						this.buildUpSound.SetVariable("Volume", Game1.screenGlowAlpha * 150f);
						this.buildUpSound.SetVariable("Frequency", Game1.screenGlowAlpha * 150f);
					}
					if (Game1.screenGlowAlpha >= Game1.screenGlowMax)
					{
						this.messageAlpha += 0.008f;
						this.messageAlpha = Math.Min(this.messageAlpha, 1f);
					}
					if ((Game1.screenGlowAlpha == Game1.screenGlowMax || Game1.currentLocation != this) && this.restoreAreaTimer > 5200)
					{
						this.restoreAreaTimer = 5200;
					}
					if (this.restoreAreaTimer < 5200 && Game1.random.NextDouble() < (double)((float)(5200 - this.restoreAreaTimer) / 10000f))
					{
						base.localSound(Game1.random.Choose("dustMeep", "junimoMeep1"), null, null, SoundContext.Default);
					}
					if (this.restoreAreaTimer <= 0)
					{
						this.restoreAreaTimer = 2000;
						this.messageAlpha = 0f;
						this.restoreAreaPhase = 3;
						if (Game1.IsMasterGame)
						{
							this.characters.RemoveWhere(delegate(NPC npc)
							{
								Junimo junimo = npc as Junimo;
								return junimo != null && junimo.temporaryJunimo.Value;
							});
						}
						if (Game1.player.currentLocation == this)
						{
							Game1.screenGlowHold = false;
							this.loadArea(this.restoreAreaIndex, true);
							if (Game1.IsMasterGame)
							{
								this._mapSeatsDirty = true;
							}
							ICue cue = this.buildUpSound;
							if (cue != null)
							{
								cue.Stop(AudioStopOptions.Immediate);
							}
							base.localSound("wand", null, null, SoundContext.Default);
							Game1.changeMusicTrack("junimoStarSong", false, MusicContext.Default);
							base.localSound("woodyHit", null, null, SoundContext.Default);
							Game1.flashAlpha = 1f;
							Game1.player.stopJittering();
							Game1.drawLighting = true;
							return;
						}
						if (Game1.IsMasterGame)
						{
							this.loadArea(this.restoreAreaIndex, true);
							this._mapSeatsDirty = true;
							return;
						}
					}
					break;
				case 3:
					if (old > 1000 && this.restoreAreaTimer <= 1000)
					{
						Junimo j = this.getJunimoForArea(this.restoreAreaIndex);
						if (j != null && Game1.IsMasterGame)
						{
							if (!j.holdingBundle.Value)
							{
								j.Position = Utility.getRandomAdjacentOpenTile(Utility.PointToVector2(this.getNotePosition(this.restoreAreaIndex)), this) * 64f;
								int iter = 0;
								while (base.isCollidingPosition(j.GetBoundingBox(), Game1.viewport, j) && iter < 20)
								{
									Microsoft.Xna.Framework.Rectangle area_bounds = this.getAreaBounds(this.restoreAreaIndex);
									if (this.restoreAreaIndex == 5)
									{
										area_bounds = new Microsoft.Xna.Framework.Rectangle(44, 13, 6, 2);
									}
									j.Position = Utility.getRandomPositionInThisRectangle(area_bounds, Game1.random) * 64f;
									iter++;
								}
								if (iter < 20)
								{
									j.fadeBack();
								}
							}
							j.returnToJunimoHutToFetchStar(this);
						}
					}
					if (this.restoreAreaTimer <= 0 && !this._isWatchingJunimoGoodbye)
					{
						Game1.freezeControls = false;
						return;
					}
					break;
				default:
					return;
				}
			}
			else if (Game1.activeClickableMenu == null)
			{
				List<int> list = this.junimoNotesViewportTargets;
				if (list != null && list.Count > 0 && !Game1.isViewportOnCustomPath())
				{
					this.setViewportToNextJunimoNoteTarget();
				}
			}
		}

		// Token: 0x06002E5C RID: 11868 RVA: 0x002433C8 File Offset: 0x002415C8
		private void setViewportToNextJunimoNoteTarget()
		{
			if (this.junimoNotesViewportTargets.Count > 0)
			{
				Game1.freezeControls = true;
				int area = this.junimoNotesViewportTargets[0];
				Point p = this.getNotePosition(area);
				Game1.moveViewportTo(new Vector2((float)p.X, (float)p.Y) * 64f, 5f, 2000, new Game1.afterFadeFunction(this.afterViewportGetsToJunimoNotePosition), new Game1.afterFadeFunction(this.setViewportToNextJunimoNoteTarget));
				return;
			}
			Game1.viewportFreeze = true;
			Game1.viewportHold = 10000;
			Game1.globalFadeToBlack(new Game1.afterFadeFunction(Game1.afterFadeReturnViewportToPlayer), 0.02f);
			Game1.freezeControls = false;
			Game1.afterViewport = null;
		}

		// Token: 0x06002E5D RID: 11869 RVA: 0x00243478 File Offset: 0x00241678
		private void afterViewportGetsToJunimoNotePosition()
		{
			int area = this.junimoNotesViewportTargets[0];
			this.junimoNotesViewportTargets.RemoveAt(0);
			this.addJunimoNote(area);
			base.localSound("reward", null, null, SoundContext.Default);
		}

		// Token: 0x06002E5E RID: 11870 RVA: 0x002434C4 File Offset: 0x002416C4
		public Junimo getJunimoForArea(int whichArea)
		{
			foreach (NPC npc in this.characters)
			{
				Junimo junimo = npc as Junimo;
				if (junimo != null && junimo.whichArea.Value == whichArea)
				{
					return junimo;
				}
			}
			Junimo i = new Junimo(Vector2.Zero, whichArea, false);
			base.addCharacter(i);
			return i;
		}

		// Token: 0x06002E5F RID: 11871 RVA: 0x00243544 File Offset: 0x00241744
		public bool areAllAreasComplete()
		{
			using (IEnumerator<bool> enumerator = this.areasComplete.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002E60 RID: 11872 RVA: 0x00243594 File Offset: 0x00241794
		public void junimoGoodbyeDance()
		{
			this.getJunimoForArea(0).Position = new Vector2(23f, 11f) * 64f;
			this.getJunimoForArea(1).Position = new Vector2(27f, 11f) * 64f;
			this.getJunimoForArea(2).Position = new Vector2(24f, 12f) * 64f;
			this.getJunimoForArea(4).Position = new Vector2(26f, 12f) * 64f;
			this.getJunimoForArea(3).Position = new Vector2(28f, 12f) * 64f;
			this.getJunimoForArea(5).Position = new Vector2(25f, 11f) * 64f;
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				this.getJunimoForArea(i).stayStill();
				this.getJunimoForArea(i).faceDirection(1);
				this.getJunimoForArea(i).fadeBack();
				this.getJunimoForArea(i).IsInvisible = false;
				this.getJunimoForArea(i).setAlpha(1f);
			}
			Point playerPixel = Game1.player.StandingPixel;
			Game1.moveViewportTo(new Vector2((float)playerPixel.X, (float)playerPixel.Y), 2f, 5000, new Game1.afterFadeFunction(this.startGoodbyeDance), new Game1.afterFadeFunction(this.endGoodbyeDance));
			Game1.viewportFreeze = false;
			Game1.freezeControls = true;
		}

		// Token: 0x06002E61 RID: 11873 RVA: 0x0024372C File Offset: 0x0024192C
		public void prepareForJunimoDance()
		{
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				Junimo junimoForArea = this.getJunimoForArea(i);
				junimoForArea.holdingBundle.Value = false;
				junimoForArea.holdingStar.Value = false;
				junimoForArea.controller = null;
				junimoForArea.Halt();
				junimoForArea.IsInvisible = true;
			}
			this.numberOfStarsOnPlaque.Value = 0;
			for (int j = 0; j < this.areasComplete.Count; j++)
			{
				if (this.areasComplete[j])
				{
					NetInt netInt = this.numberOfStarsOnPlaque;
					int value = netInt.Value;
					netInt.Value = value + 1;
				}
			}
		}

		// Token: 0x06002E62 RID: 11874 RVA: 0x002437C8 File Offset: 0x002419C8
		private void startGoodbyeDance()
		{
			Game1.freezeControls = true;
			this.getJunimoForArea(0).Position = new Vector2(23f, 11f) * 64f;
			this.getJunimoForArea(1).Position = new Vector2(27f, 11f) * 64f;
			this.getJunimoForArea(2).Position = new Vector2(24f, 12f) * 64f;
			this.getJunimoForArea(4).Position = new Vector2(26f, 12f) * 64f;
			this.getJunimoForArea(3).Position = new Vector2(28f, 12f) * 64f;
			this.getJunimoForArea(5).Position = new Vector2(25f, 11f) * 64f;
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				this.getJunimoForArea(i).stayStill();
				this.getJunimoForArea(i).faceDirection(1);
				this.getJunimoForArea(i).fadeBack();
				this.getJunimoForArea(i).IsInvisible = false;
				this.getJunimoForArea(i).setAlpha(1f);
				this.getJunimoForArea(i).sayGoodbye();
			}
		}

		// Token: 0x06002E63 RID: 11875 RVA: 0x00243920 File Offset: 0x00241B20
		private void endGoodbyeDance()
		{
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				this.getJunimoForArea(i).fadeAway();
			}
			Game1.pauseThenDoFunction(3600, new Game1.afterFadeFunction(this.loadJunimoHut));
			Game1.freezeControls = true;
		}

		// Token: 0x06002E64 RID: 11876 RVA: 0x0024396C File Offset: 0x00241B6C
		private void loadJunimoHut()
		{
			for (int i = 0; i < this.areasComplete.Count; i++)
			{
				this.getJunimoForArea(i).clearTextAboveHead();
			}
			this.loadArea(8, true);
			Game1.flashAlpha = 1f;
			base.localSound("wand", null, null, SoundContext.Default);
			Game1.freezeControls = false;
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:CommunityCenter_JunimosReturned"));
		}

		// Token: 0x06002E65 RID: 11877 RVA: 0x002439E8 File Offset: 0x00241BE8
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			for (int i = 0; i < this.numberOfStarsOnPlaque.Value; i++)
			{
				switch (i)
				{
				case 0:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 324f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 364f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 384f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 3:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 364f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 4:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 324f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 5:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 308f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				}
			}
			if (Game1.eventUp)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture f in this.furniture)
				{
					if (f.QualifiedItemId == "(F)CCFishTank")
					{
						f.draw(b, -1, -1, 1f);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
		}

		// Token: 0x06002E66 RID: 11878 RVA: 0x00243CBC File Offset: 0x00241EBC
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (this.messageAlpha > 0f)
			{
				Junimo i = this.getJunimoForArea(0);
				if (i != null)
				{
					b.Draw(i.Sprite.Texture, new Vector2((float)(Game1.viewport.Width / 2 - 32), (float)(Game1.viewport.Height * 2) / 3f - 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800.0) / 100 * 16, 0, 16, 16)), Color.Lime * this.messageAlpha, 0f, new Vector2((float)(i.Sprite.SpriteWidth * 4 / 2), (float)(i.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, 1f) * 4f, i.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
				}
				b.DrawString(Game1.dialogueFont, "\"" + Game1.parseText(this.getMessageForAreaCompletion() + "\"", Game1.dialogueFont, 640), new Vector2((float)(Game1.viewport.Width / 2 - 320), (float)(Game1.viewport.Height * 2) / 3f), Game1.textColor * this.messageAlpha * 0.6f);
			}
		}

		// Token: 0x06002E67 RID: 11879 RVA: 0x00243E4C File Offset: 0x0024204C
		public static string getAreaNameFromNumber(int areaNumber)
		{
			switch (areaNumber)
			{
			case 0:
				return "Pantry";
			case 1:
				return "Crafts Room";
			case 2:
				return "Fish Tank";
			case 3:
				return "Boiler Room";
			case 4:
				return "Vault";
			case 5:
				return "Bulletin Board";
			case 6:
				return "Abandoned Joja Mart";
			default:
				return "";
			}
		}

		// Token: 0x06002E68 RID: 11880 RVA: 0x00243EAC File Offset: 0x002420AC
		public static string getAreaEnglishDisplayNameFromNumber(int areaNumber)
		{
			return Game1.content.LoadBaseString("Strings\\Locations:CommunityCenter_AreaName_" + CommunityCenter.getAreaNameFromNumber(areaNumber).Replace(" ", ""));
		}

		// Token: 0x06002E69 RID: 11881 RVA: 0x00243ED7 File Offset: 0x002420D7
		public static string getAreaDisplayNameFromNumber(int areaNumber)
		{
			return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_" + CommunityCenter.getAreaNameFromNumber(areaNumber).Replace(" ", ""));
		}

		// Token: 0x06002E6A RID: 11882 RVA: 0x00243F04 File Offset: 0x00242104
		public static StaticTile[] getJunimoNoteTileFrames(int area, Map map)
		{
			TileSheet tileSheet = map.GetTileSheet("indoor") ?? map.RequireTileSheet(0, "indoors");
			if (area == 5)
			{
				Layer layer = map.RequireLayer("Front");
				return new StaticTile[]
				{
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1741),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1773),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1805),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1805),
					new StaticTile(layer, tileSheet, BlendMode.Alpha, 1773)
				};
			}
			Layer layer2 = map.RequireLayer("Buildings");
			return new StaticTile[]
			{
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1832),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1824),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1825),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1826),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1827),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1828),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1829),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1830),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1831),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1832),
				new StaticTile(layer2, tileSheet, BlendMode.Alpha, 1833)
			};
		}

		// Token: 0x04001F8D RID: 8077
		public const int AREA_Pantry = 0;

		// Token: 0x04001F8E RID: 8078
		public const int AREA_FishTank = 2;

		// Token: 0x04001F8F RID: 8079
		public const int AREA_CraftsRoom = 1;

		// Token: 0x04001F90 RID: 8080
		public const int AREA_BoilerRoom = 3;

		// Token: 0x04001F91 RID: 8081
		public const int AREA_Vault = 4;

		// Token: 0x04001F92 RID: 8082
		public const int AREA_Bulletin = 5;

		// Token: 0x04001F93 RID: 8083
		public const int AREA_AbandonedJojaMart = 6;

		// Token: 0x04001F94 RID: 8084
		public const int AREA_Bulletin2 = 7;

		// Token: 0x04001F95 RID: 8085
		public const int AREA_JunimoHut = 8;

		// Token: 0x04001F96 RID: 8086
		[XmlElement("warehouse")]
		private readonly NetBool warehouse = new NetBool();

		// Token: 0x04001F97 RID: 8087
		[XmlIgnore]
		public List<NetMutex> bundleMutexes = new List<NetMutex>();

		// Token: 0x04001F98 RID: 8088
		public readonly NetArray<bool, NetBool> areasComplete = new NetArray<bool, NetBool>(6);

		// Token: 0x04001F99 RID: 8089
		[XmlElement("numberOfStarsOnPlaque")]
		public readonly NetInt numberOfStarsOnPlaque = new NetInt();

		// Token: 0x04001F9A RID: 8090
		[XmlIgnore]
		private readonly NetEvent0 newJunimoNoteCheckEvent = new NetEvent0(false);

		// Token: 0x04001F9B RID: 8091
		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> restoreAreaCutsceneEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04001F9C RID: 8092
		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> areaCompleteRewardEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04001F9D RID: 8093
		private float messageAlpha;

		// Token: 0x04001F9E RID: 8094
		private List<int> junimoNotesViewportTargets;

		// Token: 0x04001F9F RID: 8095
		private Dictionary<int, List<int>> areaToBundleDictionary;

		// Token: 0x04001FA0 RID: 8096
		private Dictionary<int, int> bundleToAreaDictionary;

		// Token: 0x04001FA1 RID: 8097
		private Dictionary<string, List<List<int>>> bundlesIngredientsInfo;

		// Token: 0x04001FA2 RID: 8098
		private bool _isWatchingJunimoGoodbye;

		// Token: 0x04001FA3 RID: 8099
		private Vector2 missedRewardsChestTile = new Vector2(22f, 10f);

		// Token: 0x04001FA4 RID: 8100
		private const string missedRewardsTileSheetId = "indoors2";

		// Token: 0x04001FA5 RID: 8101
		[XmlIgnore]
		public readonly NetRef<Chest> missedRewardsChest = new NetRef<Chest>(new Chest(true, "130"));

		// Token: 0x04001FA6 RID: 8102
		[XmlIgnore]
		public readonly NetBool missedRewardsChestVisible = new NetBool(false);

		// Token: 0x04001FA7 RID: 8103
		[XmlIgnore]
		public readonly NetEvent1Field<bool, NetBool> showMissedRewardsChestEvent = new NetEvent1Field<bool, NetBool>();

		// Token: 0x04001FA8 RID: 8104
		public const int PHASE_firstPause = 0;

		// Token: 0x04001FA9 RID: 8105
		public const int PHASE_junimoAppear = 1;

		// Token: 0x04001FAA RID: 8106
		public const int PHASE_junimoDance = 2;

		// Token: 0x04001FAB RID: 8107
		public const int PHASE_restore = 3;

		// Token: 0x04001FAC RID: 8108
		private int restoreAreaTimer;

		// Token: 0x04001FAD RID: 8109
		private int restoreAreaPhase;

		// Token: 0x04001FAE RID: 8110
		private int restoreAreaIndex;

		// Token: 0x04001FAF RID: 8111
		private ICue buildUpSound;
	}
}
