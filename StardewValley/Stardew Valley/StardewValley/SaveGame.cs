using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Locations;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SaveSerialization;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Util;

namespace StardewValley
{
	// Token: 0x020000FA RID: 250
	public class SaveGame
	{
		// Token: 0x06001435 RID: 5173 RVA: 0x000F4A7A File Offset: 0x000F2C7A
		public bool HasSaveFix(SaveFixes fix)
		{
			return this.lastAppliedSaveFix >= (int)fix;
		}

		// Token: 0x06001436 RID: 5174 RVA: 0x000F4A88 File Offset: 0x000F2C88
		public static IEnumerator<int> Save()
		{
			SaveGame.IsProcessing = true;
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				IEnumerator<int> save = SaveGame.getSaveEnumerator();
				while (save.MoveNext())
				{
					int num = save.Current;
					yield return num;
				}
				yield return 100;
				save = null;
			}
			else
			{
				SaveGame.<>c__DisplayClass92_0 CS$<>8__locals1 = new SaveGame.<>c__DisplayClass92_0();
				SaveGame.LogVerbose("SaveGame.Save() called.");
				yield return 1;
				CS$<>8__locals1.loader = SaveGame.getSaveEnumerator();
				Task saveTask = new Task(delegate()
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					if (CS$<>8__locals1.loader != null)
					{
						while (CS$<>8__locals1.loader.MoveNext() && CS$<>8__locals1.loader.Current < 100)
						{
						}
						return;
					}
				});
				Game1.hooks.StartTask(saveTask, "Save");
				while (!saveTask.IsCanceled && !saveTask.IsCompleted && !saveTask.IsFaulted)
				{
					yield return 1;
				}
				SaveGame.IsProcessing = false;
				if (saveTask.IsFaulted)
				{
					Exception e = saveTask.Exception.GetBaseException();
					SaveGame.LogError("saveTask failed with an exception", e);
					if (e is TaskCanceledException)
					{
						Game1.ExitToTitle(null);
						yield break;
					}
					throw e;
				}
				else
				{
					SaveGame.LogVerbose("SaveGame.Save() completed without exceptions.");
					yield return 100;
					CS$<>8__locals1 = null;
					saveTask = null;
				}
			}
			yield break;
		}

		// Token: 0x06001437 RID: 5175 RVA: 0x000F4A90 File Offset: 0x000F2C90
		public static string FilterFileName(string fileName)
		{
			StringBuilder sb = new StringBuilder(fileName.Length);
			foreach (char c in fileName)
			{
				if (char.IsLetterOrDigit(c))
				{
					sb.Append(c);
				}
			}
			fileName = sb.ToString();
			return fileName;
		}

		// Token: 0x06001438 RID: 5176 RVA: 0x000F4ADD File Offset: 0x000F2CDD
		public static IEnumerator<int> getSaveEnumerator()
		{
			if (SaveGame.CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 1;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				farmer.UnapplyAllTrinketEffects();
			}
			Game1.player.gameVersion = Game1.version;
			Game1.player.gameVersionLabel = Game1.versionLabel;
			foreach (GameLocation gameLocation in Game1.locations)
			{
				gameLocation.cleanupBeforeSave();
			}
			Game1.player.team.globalInventories.RemoveWhere((KeyValuePair<string, Inventory> p) => !p.Value.HasAny());
			SaveGame saveGame = new SaveGame();
			saveGame.player = Game1.player;
			saveGame.farmhands = new List<Farmer>(Game1.netWorldState.Value.farmhandData.Values);
			saveGame.locations = new List<GameLocation>(Game1.locations);
			saveGame.currentSeason = Game1.currentSeason;
			saveGame.samBandName = Game1.samBandName;
			saveGame.broadcastedMail = new HashSet<string>(Game1.player.team.broadcastedMail);
			saveGame.constructedBuildings = new HashSet<string>(Game1.player.team.constructedBuildings);
			saveGame.bannedUsers = Game1.bannedUsers.ToSaveableArray<string, string>();
			saveGame.skullCavesDifficulty = Game1.netWorldState.Value.SkullCavesDifficulty;
			saveGame.minesDifficulty = Game1.netWorldState.Value.MinesDifficulty;
			saveGame.visitsUntilY1Guarantee = Game1.netWorldState.Value.VisitsUntilY1Guarantee;
			saveGame.shuffleMineChests = Game1.netWorldState.Value.ShuffleMineChests;
			saveGame.elliottBookName = Game1.elliottBookName;
			saveGame.dayOfMonth = Game1.dayOfMonth;
			saveGame.year = Game1.year;
			saveGame.dailyLuck = Game1.player.team.sharedDailyLuck.Value;
			saveGame.isRaining = Game1.isRaining;
			saveGame.isLightning = Game1.isLightning;
			saveGame.isSnowing = Game1.isSnowing;
			saveGame.isDebrisWeather = Game1.isDebrisWeather;
			saveGame.shouldSpawnMonsters = Game1.spawnMonstersAtNight;
			saveGame.specialOrders = Game1.player.team.specialOrders.ToList<SpecialOrder>();
			saveGame.availableSpecialOrders = Game1.player.team.availableSpecialOrders.ToList<SpecialOrder>();
			saveGame.completedSpecialOrders = Game1.player.team.completedSpecialOrders.ToList<string>();
			saveGame.collectedNutTracker = Game1.player.team.collectedNutTracker.ToList<string>();
			saveGame.acceptedSpecialOrderTypes = Game1.player.team.acceptedSpecialOrderTypes.ToList<string>();
			saveGame.returnedDonations = Game1.player.team.returnedDonations.ToList<Item>();
			saveGame.weddingToday = Game1.weddingToday;
			saveGame.weddingsToday = Game1.weddingsToday.ToList<long>();
			saveGame.shippingBin = Game1.getFarm().getShippingBin(Game1.player).ToArray<Item>();
			saveGame.globalInventories = DictionarySaver<string, Item[]>.ArrayFrom<NetRef<Inventory>>(Game1.player.team.globalInventories.FieldDict, (NetRef<Inventory> value) => value.Value.ToArray<Item>());
			saveGame.whichFarm = Game1.GetFarmTypeID();
			saveGame.junimoKartLeaderboards = Game1.player.team.junimoKartScores;
			saveGame.lastAppliedSaveFix = 98;
			saveGame.locationWeather = SerializableDictionary<string, LocationWeather>.BuildFrom<NetRef<LocationWeather>>(Game1.netWorldState.Value.LocationWeather.FieldDict, (NetRef<LocationWeather> value) => value.Value);
			saveGame.builders = DictionarySaver<string, BuilderData>.ArrayFrom<NetRef<BuilderData>>(Game1.netWorldState.Value.Builders.FieldDict, (NetRef<BuilderData> value) => value.Value);
			saveGame.cellarAssignments = DictionarySaver<int, long>.ArrayFrom<NetLong>(Game1.player.team.cellarAssignments.FieldDict, (NetLong value) => value.Value);
			saveGame.uniqueIDForThisGame = Game1.uniqueIDForThisGame;
			saveGame.musicVolume = Game1.options.musicVolumeLevel;
			saveGame.soundVolume = Game1.options.soundVolumeLevel;
			saveGame.mine_lowestLevelReached = Game1.netWorldState.Value.LowestMineLevel;
			saveGame.mine_lowestLevelReachedForOrder = Game1.netWorldState.Value.LowestMineLevelForOrder;
			saveGame.currentGemBirdIndex = Game1.currentGemBirdIndex;
			saveGame.mine_permanentMineChanges = MineShaft.permanentMineChanges.ToSaveableArray<int, MineInfo>();
			saveGame.dishOfTheDay = Game1.dishOfTheDay;
			saveGame.latestID = (long)Game1.multiplayer.latestID;
			saveGame.highestPlayerLimit = Game1.netWorldState.Value.HighestPlayerLimit;
			saveGame.options = Game1.options;
			saveGame.splitscreenOptions = Game1.splitscreenOptions.ToSaveableArray<long, Options>();
			saveGame.CustomData = Game1.CustomData;
			saveGame.worldStateIDs = Game1.worldStateIDs;
			saveGame.weatherForTomorrow = Game1.weatherForTomorrow;
			saveGame.goldenWalnuts = Game1.netWorldState.Value.GoldenWalnuts;
			saveGame.goldenWalnutsFound = Game1.netWorldState.Value.GoldenWalnutsFound;
			saveGame.miniShippingBinsObtained = Game1.netWorldState.Value.MiniShippingBinsObtained;
			saveGame.goldenCoconutCracked = Game1.netWorldState.Value.GoldenCoconutCracked;
			saveGame.parrotPlatformsUnlocked = Game1.netWorldState.Value.ParrotPlatformsUnlocked;
			saveGame.farmPerfect = Game1.player.team.farmPerfect.Value;
			saveGame.lostBooksFound = Game1.netWorldState.Value.LostBooksFound;
			saveGame.foundBuriedNuts = Game1.netWorldState.Value.FoundBuriedNuts.ToList<string>();
			saveGame.checkedGarbage = Game1.netWorldState.Value.CheckedGarbage.ToList<string>();
			saveGame.mineShrineActivated = Game1.player.team.mineShrineActivated.Value;
			saveGame.skullShrineActivated = Game1.player.team.skullShrineActivated.Value;
			saveGame.timesFedRaccoons = Game1.netWorldState.Value.TimesFedRaccoons;
			saveGame.treasureTotemsUsed = Game1.netWorldState.Value.TreasureTotemsUsed;
			saveGame.perfectionWaivers = Game1.netWorldState.Value.PerfectionWaivers;
			saveGame.seasonOfCurrentRaccoonBundle = Game1.netWorldState.Value.SeasonOfCurrentRacconBundle;
			saveGame.raccoonBundles = Game1.netWorldState.Value.raccoonBundles.ToArray<bool>();
			saveGame.activatedGoldenParrot = Game1.netWorldState.Value.ActivatedGoldenParrot;
			saveGame.daysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished;
			saveGame.gameVersion = Game1.version;
			saveGame.gameVersionLabel = Game1.versionLabel;
			saveGame.limitedNutDrops = DictionarySaver<string, int>.ArrayFrom<NetInt>(Game1.player.team.limitedNutDrops.FieldDict, (NetInt value) => value.Value);
			saveGame.bundleData = Game1.netWorldState.Value.BundleData.ToSaveableArray<string, string>();
			saveGame.moveBuildingPermissionMode = (int)Game1.player.team.farmhandsCanMoveBuildings.Value;
			saveGame.useLegacyRandom = Game1.player.team.useLegacyRandom.Value;
			saveGame.allowChatCheats = Game1.player.team.allowChatCheats.Value;
			saveGame.hasDedicatedHost = Game1.player.team.hasDedicatedHost.Value;
			saveGame.hasApplied1_3_UpdateChanges = true;
			saveGame.hasApplied1_4_UpdateChanges = true;
			saveGame.farmerFriendships = DictionarySaver<FarmerPair, Friendship>.ArrayFrom<NetRef<Friendship>>(Game1.player.team.friendshipData.FieldDict, (NetRef<Friendship> value) => value.Value);
			SaveGame saveData = saveGame;
			string finalDataName = SaveGame.FilterFileName(Game1.GetSaveGameName(true)) + "_" + Game1.uniqueIDForThisGame.ToString();
			string saveDirPath = Path.Combine(Program.GetSavesFolder(), finalDataName + Path.DirectorySeparatorChar.ToString());
			string finalFarmerPath = Path.Combine(saveDirPath, "SaveGameInfo");
			string finalDataPath = Path.Combine(saveDirPath, finalDataName);
			string tempFarmerPath = finalFarmerPath + "_STARDEWVALLEYSAVETMP";
			string tempDataPath = finalDataPath + "_STARDEWVALLEYSAVETMP";
			SaveGame.ensureFolderStructureExists();
			Stream fstream = null;
			try
			{
				fstream = File.Open(tempDataPath, FileMode.Create);
			}
			catch (IOException ex)
			{
				if (fstream != null)
				{
					fstream.Close();
					fstream.Dispose();
				}
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(ex.Message);
				yield break;
			}
			MemoryStream mstream = new MemoryStream(1024);
			MemoryStream mstream2 = new MemoryStream(1024);
			byte[] buffer = null;
			if (SaveGame.CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			SaveGame.LogVerbose("Saving without compression...");
			MemoryStream memoryStream = mstream;
			XmlWriterSettings settings = new XmlWriterSettings
			{
				CloseOutput = false
			};
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
			xmlWriter.WriteStartDocument();
			SaveSerializer.Serialize<SaveGame>(xmlWriter, saveData);
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			xmlWriter.Close();
			memoryStream.Close();
			buffer = mstream.ToArray();
			mstream = null;
			if (SaveGame.CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			fstream.Write(buffer, 0, buffer.Length);
			fstream.Close();
			buffer = null;
			mstream = null;
			Game1.player.saveTime = (int)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalMinutes;
			try
			{
				fstream = File.Open(tempFarmerPath, FileMode.Create);
			}
			catch (IOException ex2)
			{
				Stream stream3 = fstream;
				if (stream3 != null)
				{
					stream3.Close();
				}
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(ex2.Message);
				yield break;
			}
			Stream stream2 = fstream;
			XmlWriter xmlWriter2 = XmlWriter.Create(stream2, settings);
			xmlWriter2.WriteStartDocument();
			SaveSerializer.Serialize<Farmer>(xmlWriter2, Game1.player);
			xmlWriter2.WriteEndDocument();
			xmlWriter2.Flush();
			xmlWriter2.Close();
			stream2.Close();
			fstream.Close();
			if (SaveGame.CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			string oldDataPath = finalDataPath + "_old";
			string oldFarmerPath = finalFarmerPath + "_old";
			try
			{
				LegacyShims.MoveFileWithOverwrite(finalDataPath, oldDataPath);
				LegacyShims.MoveFileWithOverwrite(finalFarmerPath, oldFarmerPath);
			}
			catch
			{
			}
			LegacyShims.MoveFileWithOverwrite(tempDataPath, finalDataPath);
			LegacyShims.MoveFileWithOverwrite(tempFarmerPath, finalFarmerPath);
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				farmer2.resetAllTrinketEffects();
			}
			Game1.player.sleptInTemporaryBed.Value = false;
			if (SaveGame.CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 100;
			yield break;
		}

		// Token: 0x06001439 RID: 5177 RVA: 0x000F4AE8 File Offset: 0x000F2CE8
		public static bool IsNewGameSaveNameCollision(string save_name)
		{
			string filename = SaveGame.FilterFileName(save_name) + "_" + Game1.uniqueIDForThisGame.ToString();
			return Directory.Exists(Path.Combine(Program.GetSavesFolder(), filename));
		}

		// Token: 0x0600143A RID: 5178 RVA: 0x000F4B20 File Offset: 0x000F2D20
		public static void ensureFolderStructureExists()
		{
			string folderName = SaveGame.FilterFileName(Game1.GetSaveGameName(true)) + "_" + Game1.uniqueIDForThisGame.ToString();
			Directory.CreateDirectory(Path.Combine(Program.GetSavesFolder(), folderName));
		}

		// Token: 0x0600143B RID: 5179 RVA: 0x000F4B5E File Offset: 0x000F2D5E
		public static void Load(string filename)
		{
			Game1.gameMode = 6;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
			Game1.currentLoader = SaveGame.getLoadEnumerator(filename);
		}

		// Token: 0x0600143C RID: 5180 RVA: 0x000F4B88 File Offset: 0x000F2D88
		public static void LoadFarmType()
		{
			List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
			Game1.whichFarm = -1;
			if (farm_types != null)
			{
				foreach (ModFarmType farm_type in farm_types)
				{
					if (farm_type.Id == SaveGame.loaded.whichFarm)
					{
						Game1.whichModFarm = farm_type;
						Game1.whichFarm = 7;
						break;
					}
				}
			}
			if (SaveGame.loaded.whichFarm == null)
			{
				Game1.whichFarm = 0;
			}
			if (Game1.whichFarm < 0)
			{
				int farmType;
				if (int.TryParse(SaveGame.loaded.whichFarm, out farmType))
				{
					Game1.whichFarm = farmType;
					return;
				}
				SaveGame.LogWarn("Ignored unknown farm type '" + SaveGame.loaded.whichFarm + "' which no longer exists in the data.");
				Game1.whichFarm = 0;
				Game1.whichModFarm = null;
			}
		}

		// Token: 0x0600143D RID: 5181 RVA: 0x000F4C68 File Offset: 0x000F2E68
		public static SaveGame TryReadSaveFile(string file, string fileNameSuffix, out string error)
		{
			string fullFilePath = Path.Combine(Program.GetSavesFolder(), file, file + fileNameSuffix);
			if (!File.Exists(fullFilePath))
			{
				fullFilePath += ".xml";
				if (!File.Exists(fullFilePath))
				{
					return SaveGame.<TryReadSaveFile>g__FileDoesNotExist|99_0(out error);
				}
			}
			Stream stream = null;
			try
			{
				stream = new MemoryStream(File.ReadAllBytes(fullFilePath), false);
			}
			catch (IOException e)
			{
				error = e.Message;
				if (stream != null)
				{
					stream.Close();
				}
				return null;
			}
			byte firstByte = (byte)stream.ReadByte();
			stream.Position -= 1L;
			if (firstByte == 120)
			{
				SaveGame.LogVerbose("zlib stream detected...");
				stream = new ZlibStream(stream, CompressionMode.Decompress);
			}
			SaveGame result;
			try
			{
				error = null;
				result = SaveSerializer.Deserialize<SaveGame>(stream);
			}
			catch (Exception ex)
			{
				error = ex.Message;
				result = null;
			}
			finally
			{
				stream.Dispose();
			}
			return result;
		}

		// Token: 0x0600143E RID: 5182 RVA: 0x000F4D50 File Offset: 0x000F2F50
		public static SaveGame TryReadSaveFileWithFallback(string file, out string error, out bool autoRecovered)
		{
			SaveGame data = SaveGame.TryReadSaveFile(file, null, out error);
			if (data != null)
			{
				error = null;
				autoRecovered = false;
				return data;
			}
			string text;
			data = SaveGame.TryReadSaveFile(file, "_old", out text);
			if (data != null)
			{
				error = null;
				autoRecovered = true;
				return data;
			}
			data = SaveGame.TryReadSaveFile(file, "_STARDEWVALLEYSAVETMP", out text);
			if (data != null)
			{
				error = null;
				autoRecovered = true;
				return data;
			}
			error = (error ?? "Save could not be loaded");
			autoRecovered = false;
			return null;
		}

		// Token: 0x0600143F RID: 5183 RVA: 0x000F4DB4 File Offset: 0x000F2FB4
		public static IEnumerator<int> getLoadEnumerator(string file)
		{
			SaveGame.LogVerbose("getLoadEnumerator('" + file + "')");
			Stopwatch stopwatch = Stopwatch.StartNew();
			Game1.SetSaveName(Path.GetFileNameWithoutExtension(file).Split('_', StringSplitOptions.None).FirstOrDefault<string>());
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
			SaveGame.IsProcessing = true;
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 1;
			string error = null;
			bool autoRecovered = false;
			Task readSaveTask = new Task(delegate()
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				SaveGame.loaded = SaveGame.TryReadSaveFileWithFallback(file, out error, out autoRecovered);
			});
			Game1.hooks.StartTask(readSaveTask, "Load_ReadSave");
			while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
			{
				yield return 15;
			}
			if (SaveGame.loaded == null)
			{
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(error);
				yield break;
			}
			if (autoRecovered)
			{
				SaveGame.LogWarn("Save file " + file + " was corrupted; auto-recovered it from the backup.");
			}
			readSaveTask = null;
			yield return 19;
			Game1.hasApplied1_3_UpdateChanges = SaveGame.loaded.hasApplied1_3_UpdateChanges;
			Game1.hasApplied1_4_UpdateChanges = SaveGame.loaded.hasApplied1_4_UpdateChanges;
			Game1.lastAppliedSaveFix = (SaveFixes)SaveGame.loaded.lastAppliedSaveFix;
			Game1.player.team.useLegacyRandom.Value = SaveGame.loaded.useLegacyRandom;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4697");
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 20;
			SaveGame.LoadFarmType();
			Game1.year = SaveGame.loaded.year;
			Game1.netWorldState.Value.CurrentPlayerLimit = Game1.multiplayer.playerLimit;
			if (SaveGame.loaded.highestPlayerLimit >= 0)
			{
				Game1.netWorldState.Value.HighestPlayerLimit = SaveGame.loaded.highestPlayerLimit;
			}
			else
			{
				Game1.netWorldState.Value.HighestPlayerLimit = Math.Max(Game1.netWorldState.Value.HighestPlayerLimit, Game1.multiplayer.MaxPlayers);
			}
			Game1.uniqueIDForThisGame = SaveGame.loaded.uniqueIDForThisGame;
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				Game1.game1.loadForNewGame(true);
			}
			else
			{
				readSaveTask = new Task(delegate()
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					Game1.game1.loadForNewGame(true);
				});
				Game1.hooks.StartTask(readSaveTask, "Load_LoadForNewGame");
				while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
				{
					yield return 24;
				}
				if (readSaveTask.IsFaulted)
				{
					Exception e = readSaveTask.Exception.GetBaseException();
					SaveGame.LogError("loadNewGameTask failed with an exception.", e);
					throw e;
				}
				if (SaveGame.CancelToTitle)
				{
					Game1.ExitToTitle(null);
				}
				yield return 25;
				readSaveTask = null;
			}
			int legacyWeather;
			Game1.weatherForTomorrow = (int.TryParse(SaveGame.loaded.weatherForTomorrow, out legacyWeather) ? Utility.LegacyWeatherToWeather(legacyWeather) : SaveGame.loaded.weatherForTomorrow);
			Game1.dayOfMonth = SaveGame.loaded.dayOfMonth;
			Game1.year = SaveGame.loaded.year;
			Game1.currentSeason = SaveGame.loaded.currentSeason;
			Game1.worldStateIDs = SaveGame.loaded.worldStateIDs;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4698");
			if (SaveGame.loaded.mine_permanentMineChanges != null)
			{
				MineShaft.permanentMineChanges = new SerializableDictionary<int, MineInfo>(SaveGame.loaded.mine_permanentMineChanges.ToDictionary<int, MineInfo>());
				Game1.netWorldState.Value.LowestMineLevel = SaveGame.loaded.mine_lowestLevelReached;
				Game1.netWorldState.Value.LowestMineLevelForOrder = SaveGame.loaded.mine_lowestLevelReachedForOrder;
			}
			Game1.currentGemBirdIndex = SaveGame.loaded.currentGemBirdIndex;
			if (SaveGame.loaded.bundleData.Length != 0)
			{
				Dictionary<string, string> bundleData = SaveGame.loaded.bundleData.ToDictionary<string, string>();
				if (!SaveGame.loaded.HasSaveFix(SaveFixes.StandardizeBundleFields))
				{
					SaveMigrator_1_6.StandardizeBundleFields(bundleData);
				}
				Game1.netWorldState.Value.SetBundleData(bundleData);
			}
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 26;
			Game1.isRaining = SaveGame.loaded.isRaining;
			Game1.isLightning = SaveGame.loaded.isLightning;
			Game1.isSnowing = SaveGame.loaded.isSnowing;
			Game1.isGreenRain = Utility.isGreenRainDay();
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			if (SaveGame.loaded.locationWeather != null)
			{
				Game1.netWorldState.Value.LocationWeather.Clear();
				foreach (KeyValuePair<string, LocationWeather> pair in SaveGame.loaded.locationWeather)
				{
					Game1.netWorldState.Value.LocationWeather[pair.Key] = pair.Value;
				}
			}
			if (SaveGame.loaded.builders != null)
			{
				foreach (SaveablePair<string, BuilderData> pair2 in SaveGame.loaded.builders)
				{
					Game1.netWorldState.Value.Builders[pair2.Key] = pair2.Value;
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				SaveGame.loadDataToFarmer(SaveGame.loaded.player);
			}
			else
			{
				readSaveTask = new Task(delegate()
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					SaveGame.loadDataToFarmer(SaveGame.loaded.player);
				});
				Game1.hooks.StartTask(readSaveTask, "Load_Farmer");
				while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
				{
					yield return 1;
				}
				if (readSaveTask.IsFaulted)
				{
					Exception e2 = readSaveTask.Exception.GetBaseException();
					SaveGame.LogError("loadFarmerTask failed with an exception", e2);
					throw e2;
				}
				readSaveTask = null;
			}
			Game1.player = SaveGame.loaded.player;
			Game1.player.team.useLegacyRandom.Value = SaveGame.loaded.useLegacyRandom;
			Game1.player.team.allowChatCheats.Value = SaveGame.loaded.allowChatCheats;
			Game1.player.team.hasDedicatedHost.Value = SaveGame.loaded.hasDedicatedHost;
			Game1.netWorldState.Value.farmhandData.Clear();
			if (Game1.lastAppliedSaveFix < SaveFixes.MigrateFarmhands)
			{
				SaveMigrator_1_6.MigrateFarmhands(SaveGame.loaded.locations);
			}
			if (SaveGame.loaded.farmhands != null)
			{
				foreach (Farmer farmhand in SaveGame.loaded.farmhands)
				{
					Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
				}
			}
			foreach (Farmer target in Game1.netWorldState.Value.farmhandData.Values)
			{
				SaveGame.loadDataToFarmer(target);
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved"))
			{
				Mountain mountain = Game1.getLocationFromName("Mountain") as Mountain;
				if (mountain != null)
				{
					mountain.reloadMap();
					mountain.ApplyTreehouseIfNecessary();
					if (mountain.treehouseDoorDirty)
					{
						mountain.treehouseDoorDirty = false;
						WarpPathfindingCache.PopulateCache();
					}
				}
			}
			if (SaveGame.loaded.farmerFriendships != null)
			{
				foreach (SaveablePair<FarmerPair, Friendship> pair3 in SaveGame.loaded.farmerFriendships)
				{
					Game1.player.team.friendshipData[pair3.Key] = pair3.Value;
				}
			}
			Game1.spawnMonstersAtNight = SaveGame.loaded.shouldSpawnMonsters;
			Game1.player.team.limitedNutDrops.Clear();
			if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
			{
				Game1.netWorldState.Value.RegisterSpecialCurrencies();
			}
			if (SaveGame.loaded.limitedNutDrops != null)
			{
				foreach (SaveablePair<string, int> pair4 in SaveGame.loaded.limitedNutDrops)
				{
					if (pair4.Value > 0)
					{
						Game1.player.team.limitedNutDrops[pair4.Key] = pair4.Value;
					}
				}
			}
			Game1.player.team.completedSpecialOrders.Clear();
			Game1.player.team.completedSpecialOrders.AddRange(SaveGame.loaded.completedSpecialOrders);
			Game1.player.team.specialOrders.Clear();
			foreach (SpecialOrder order in SaveGame.loaded.specialOrders)
			{
				if (order != null)
				{
					Game1.player.team.specialOrders.Add(order);
				}
			}
			Game1.player.team.availableSpecialOrders.Clear();
			foreach (SpecialOrder order2 in SaveGame.loaded.availableSpecialOrders)
			{
				if (order2 != null)
				{
					Game1.player.team.availableSpecialOrders.Add(order2);
				}
			}
			Game1.player.team.acceptedSpecialOrderTypes.Clear();
			Game1.player.team.acceptedSpecialOrderTypes.AddRange(SaveGame.loaded.acceptedSpecialOrderTypes);
			Game1.player.team.collectedNutTracker.Clear();
			Game1.player.team.collectedNutTracker.AddRange(SaveGame.loaded.collectedNutTracker);
			Game1.player.team.globalInventories.Clear();
			if (SaveGame.loaded.globalInventories != null)
			{
				foreach (SaveablePair<string, Item[]> pair5 in SaveGame.loaded.globalInventories)
				{
					Game1.player.team.GetOrCreateGlobalInventory(pair5.Key).AddRange(pair5.Value);
				}
			}
			List<Item> list = SaveGame.loaded.junimoChest;
			if (list != null && list.Count > 0)
			{
				Game1.player.team.GetOrCreateGlobalInventory("JunimoChests").AddRange(SaveGame.loaded.junimoChest);
			}
			Game1.player.team.returnedDonations.Clear();
			foreach (Item donatedItem in SaveGame.loaded.returnedDonations)
			{
				Game1.player.team.returnedDonations.Add(donatedItem);
			}
			if (SaveGame.loaded.obsolete_stats != null)
			{
				Game1.player.stats = SaveGame.loaded.obsolete_stats;
			}
			if (SaveGame.loaded.obsolete_mailbox != null && !Game1.player.mailbox.Any())
			{
				Game1.player.mailbox.AddRange(SaveGame.loaded.obsolete_mailbox);
			}
			Game1.random = Utility.CreateDaySaveRandom(1.0, 0.0, 0.0);
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4699");
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 36;
			Game1.UpdatePassiveFestivalStates();
			if (SaveGame.loaded.cellarAssignments != null)
			{
				foreach (SaveablePair<int, long> pair6 in SaveGame.loaded.cellarAssignments)
				{
					Game1.player.team.cellarAssignments[pair6.Key] = pair6.Value;
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				SaveGame.loadDataToLocations(SaveGame.loaded.locations);
			}
			else
			{
				readSaveTask = new Task(delegate()
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					SaveGame.loadDataToLocations(SaveGame.loaded.locations);
				});
				Game1.hooks.StartTask(readSaveTask, "Load_Locations");
				while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
				{
					yield return 1;
				}
				if (readSaveTask.IsFaulted)
				{
					Exception e3 = readSaveTask.Exception.GetBaseException();
					SaveGame.LogError("loadLocationsTask failed with an exception", e3);
					throw readSaveTask.Exception.GetBaseException();
				}
				readSaveTask = null;
			}
			if (SaveGame.loaded.shippingBin != null)
			{
				Game1.getFarm().getShippingBin(Game1.player).Clear();
				Game1.getFarm().getShippingBin(Game1.player).AddRange(SaveGame.loaded.shippingBin);
			}
			Railroad railroad = Game1.getLocationFromName("Railroad") as Railroad;
			if (railroad != null)
			{
				railroad.ResetTrainForNewDay();
			}
			HashSet<long> validFarmhands = new HashSet<long>();
			Utility.ForEachBuilding(delegate(Building building)
			{
				Cabin cabin = ((building != null) ? building.GetIndoors() : null) as Cabin;
				if (cabin != null)
				{
					validFarmhands.Add(cabin.farmhandReference.UID);
				}
				return true;
			}, true);
			List<Farmer> orphanedFarmhands = new List<Farmer>();
			foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
			{
				if (!farmer.isCustomized.Value && !validFarmhands.Contains(farmer.UniqueMultiplayerID))
				{
					orphanedFarmhands.Add(farmer);
				}
			}
			foreach (Farmer farmer2 in orphanedFarmhands)
			{
				Game1.player.team.DeleteFarmhand(farmer2);
			}
			foreach (Farmer farmer3 in Game1.getAllFarmers())
			{
				int farmerMoney = farmer3.Money;
				NetIntDelta moneyField;
				if (!Game1.player.team.individualMoney.TryGetValue(farmer3.UniqueMultiplayerID, out moneyField))
				{
					moneyField = (Game1.player.team.individualMoney[farmer3.UniqueMultiplayerID] = new NetIntDelta(farmerMoney));
				}
				moneyField.Value = farmerMoney;
			}
			Game1.updateCellarAssignments();
			foreach (GameLocation location in Game1.locations)
			{
				foreach (Building building2 in location.buildings)
				{
					GameLocation indoors = building2.GetIndoors();
					if (indoors != null)
					{
						FarmHouse house = indoors as FarmHouse;
						if (house != null)
						{
							house.updateCellarWarps();
						}
						indoors.parentLocationName.Value = location.NameOrUniqueName;
					}
				}
				FarmHouse farmHouse = location as FarmHouse;
				if (farmHouse != null)
				{
					farmHouse.updateCellarWarps();
				}
			}
			foreach (Farmer farmhand2 in Game1.netWorldState.Value.farmhandData.Values)
			{
				Game1.netWorldState.Value.ResetFarmhandState(farmhand2);
			}
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 50;
			yield return 51;
			Game1.isDebrisWeather = SaveGame.loaded.isDebrisWeather;
			if (Game1.isDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
			else
			{
				Game1.debrisWeather.Clear();
			}
			yield return 53;
			Game1.player.team.sharedDailyLuck.Value = SaveGame.loaded.dailyLuck;
			yield return 54;
			yield return 55;
			Game1.setGraphicsForSeason(true);
			yield return 56;
			Game1.samBandName = SaveGame.loaded.samBandName;
			Game1.elliottBookName = SaveGame.loaded.elliottBookName;
			yield return 63;
			Game1.weddingToday = SaveGame.loaded.weddingToday;
			Game1.weddingsToday = SaveGame.loaded.weddingsToday.ToList<long>();
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4700");
			yield return 64;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4701");
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			yield return 79;
			Game1.options.musicVolumeLevel = SaveGame.loaded.musicVolume;
			Game1.options.soundVolumeLevel = SaveGame.loaded.soundVolume;
			yield return 83;
			if (SaveGame.loaded.countdownToWedding != null && SaveGame.loaded.countdownToWedding.Value != 0 && !string.IsNullOrEmpty(SaveGame.loaded.player.spouse))
			{
				WorldDate weddingDate = WorldDate.Now();
				weddingDate.TotalDays += SaveGame.loaded.countdownToWedding.Value;
				Friendship friendship = SaveGame.loaded.player.friendshipData[SaveGame.loaded.player.spouse];
				friendship.Status = FriendshipStatus.Engaged;
				friendship.WeddingDate = weddingDate;
			}
			yield return 85;
			yield return 87;
			yield return 88;
			yield return 95;
			Game1.fadeToBlack = true;
			Game1.fadeIn = false;
			Game1.fadeToBlackAlpha = 0.99f;
			if (Game1.player.mostRecentBed.X <= 0f)
			{
				Game1.player.Position = new Vector2(192f, 384f);
			}
			Game1.addNewFarmBuildingMaps();
			GameLocation last_sleep_location = null;
			if (Game1.player.lastSleepLocation.Value != null && Game1.isLocationAccessible(Game1.player.lastSleepLocation.Value))
			{
				last_sleep_location = Game1.getLocationFromName(Game1.player.lastSleepLocation.Value);
			}
			bool apply_default_bed_position = true;
			if (last_sleep_location != null && last_sleep_location.CanWakeUpHere(Game1.player, null))
			{
				Game1.currentLocation = last_sleep_location;
				Game1.player.currentLocation = Game1.currentLocation;
				Game1.player.Position = Utility.PointToVector2(Game1.player.lastSleepPoint.Value) * 64f;
				apply_default_bed_position = false;
			}
			if (apply_default_bed_position)
			{
				Game1.currentLocation = Game1.RequireLocation("FarmHouse", false);
			}
			Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.player.CanMove = true;
			Game1.player.ReequipEnchantments();
			if (SaveGame.loaded.junimoKartLeaderboards != null)
			{
				Game1.player.team.junimoKartScores.LoadScores(SaveGame.loaded.junimoKartLeaderboards.GetScores());
			}
			Game1.options = SaveGame.loaded.options;
			Game1.splitscreenOptions = new SerializableDictionary<long, Options>(SaveGame.loaded.splitscreenOptions.ToDictionary<long, Options>());
			Game1.CustomData = SaveGame.loaded.CustomData;
			Game1.player.team.broadcastedMail.Clear();
			if (SaveGame.loaded.broadcastedMail != null)
			{
				Game1.player.team.broadcastedMail.AddRange(SaveGame.loaded.broadcastedMail);
			}
			Game1.player.team.constructedBuildings.Clear();
			if (SaveGame.loaded.constructedBuildings != null)
			{
				Game1.player.team.constructedBuildings.AddRange(SaveGame.loaded.constructedBuildings);
			}
			if (Game1.options == null)
			{
				Game1.options = new Options();
				Game1.options.LoadDefaultOptions();
			}
			else
			{
				if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
				{
					Game1.options.loadChineseFonts();
				}
				else
				{
					Game1.options.dialogueFontScale = 1f;
				}
				Game1.options.platformClampValues();
				Game1.options.SaveDefaultOptions();
			}
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(false, false);
				Game1.options.gamepadMode = startupPreferences.gamepadMode;
			}
			catch
			{
			}
			Game1.initializeVolumeLevels();
			Game1.multiplayer.latestID = (ulong)SaveGame.loaded.latestID;
			Game1.netWorldState.Value.SkullCavesDifficulty = SaveGame.loaded.skullCavesDifficulty;
			Game1.netWorldState.Value.MinesDifficulty = SaveGame.loaded.minesDifficulty;
			Game1.netWorldState.Value.VisitsUntilY1Guarantee = SaveGame.loaded.visitsUntilY1Guarantee;
			Game1.netWorldState.Value.ShuffleMineChests = SaveGame.loaded.shuffleMineChests;
			Game1.netWorldState.Value.DishOfTheDay = SaveGame.loaded.dishOfTheDay;
			if (Game1.IsRainingHere(null))
			{
				Game1.changeMusicTrack("rain", true, MusicContext.Default);
			}
			Game1.updateWeatherIcon();
			Game1.netWorldState.Value.MiniShippingBinsObtained = SaveGame.loaded.miniShippingBinsObtained;
			Game1.netWorldState.Value.LostBooksFound = SaveGame.loaded.lostBooksFound;
			Game1.netWorldState.Value.GoldenWalnuts = SaveGame.loaded.goldenWalnuts;
			Game1.netWorldState.Value.GoldenWalnutsFound = SaveGame.loaded.goldenWalnutsFound;
			Game1.netWorldState.Value.GoldenCoconutCracked = SaveGame.loaded.goldenCoconutCracked;
			Game1.netWorldState.Value.FoundBuriedNuts.Clear();
			Game1.netWorldState.Value.FoundBuriedNuts.AddRange(SaveGame.loaded.foundBuriedNuts);
			Game1.netWorldState.Value.CheckedGarbage.Clear();
			Game1.netWorldState.Value.CheckedGarbage.AddRange(SaveGame.loaded.checkedGarbage);
			IslandSouth.SetupIslandSchedules();
			Game1.netWorldState.Value.TimesFedRaccoons = SaveGame.loaded.timesFedRaccoons;
			Game1.netWorldState.Value.TreasureTotemsUsed = SaveGame.loaded.treasureTotemsUsed;
			Game1.netWorldState.Value.PerfectionWaivers = SaveGame.loaded.perfectionWaivers;
			Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = SaveGame.loaded.seasonOfCurrentRaccoonBundle;
			Game1.netWorldState.Value.raccoonBundles.Set(SaveGame.loaded.raccoonBundles);
			Game1.netWorldState.Value.ActivatedGoldenParrot = SaveGame.loaded.activatedGoldenParrot;
			Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = SaveGame.loaded.daysPlayedWhenLastRaccoonBundleWasFinished;
			Game1.PerformPassiveFestivalSetup();
			Game1.player.team.farmhandsCanMoveBuildings.Value = (FarmerTeam.RemoteBuildingPermissions)SaveGame.loaded.moveBuildingPermissionMode;
			Game1.player.team.mineShrineActivated.Value = SaveGame.loaded.mineShrineActivated;
			Game1.player.team.skullShrineActivated.Value = SaveGame.loaded.skullShrineActivated;
			if (Game1.multiplayerMode == 2)
			{
				if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.InviteOnly)
				{
					Game1.options.setServerMode("invite");
				}
				else if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.FriendsOnly)
				{
					Game1.options.setServerMode("friends");
				}
				else
				{
					Game1.options.setServerMode("friends");
				}
			}
			Game1.bannedUsers = new SerializableDictionary<string, string>(SaveGame.loaded.bannedUsers.ToDictionary<string, string>());
			bool flag = SaveGame.loaded.lostBooksFound < 0;
			SaveGame.loaded = null;
			Game1.currentLocation.lastTouchActionLocation = Game1.player.Tile;
			if (Game1.player.horseName.Value == null)
			{
				Horse horse = Utility.findHorse(Guid.Empty);
				if (horse != null && horse.displayName != "")
				{
					Game1.player.horseName.Value = horse.displayName;
					horse.ownerId.Value = Game1.player.UniqueMultiplayerID;
				}
			}
			SaveMigrator.ApplySaveFixes();
			if (flag)
			{
				SaveMigrator_1_4.RecalculateLostBookCount();
			}
			foreach (Item item in Game1.player.Items)
			{
				Object @object = item as Object;
				if (@object != null)
				{
					@object.reloadSprite();
				}
			}
			foreach (Trinket trinket in Game1.player.trinketItems)
			{
				trinket.reloadSprite();
			}
			Game1.gameMode = 3;
			Game1.AddNPCs();
			Game1.AddModNPCs();
			Game1.RefreshQuestOfTheDay();
			try
			{
				Game1.fixProblems();
			}
			catch (Exception ex)
			{
				Game1.log.Error("Failed to fix problems.", ex);
			}
			Utility.ForEachBuilding(delegate(Building building)
			{
				Stable stable = building as Stable;
				if (stable != null)
				{
					stable.grabHorse();
				}
				else
				{
					GameLocation indoors2 = building.GetIndoors();
					Cabin cabin = indoors2 as Cabin;
					if (cabin == null)
					{
						Shed shed = indoors2 as Shed;
						if (shed != null)
						{
							shed.updateLayout();
							building.updateInteriorWarps(shed);
						}
					}
					else
					{
						cabin.updateFarmLayout();
					}
				}
				return true;
			}, true);
			Game1.UpdateHorseOwnership();
			Game1.UpdateFarmPerfection();
			Game1.doMorningStuff();
			if (apply_default_bed_position)
			{
				FarmHouse farmhouse = Game1.player.currentLocation as FarmHouse;
				if (farmhouse != null)
				{
					Game1.player.Position = Utility.PointToVector2(farmhouse.GetPlayerBedSpot()) * 64f;
				}
			}
			BedFurniture.ShiftPositionForBed(Game1.player);
			Game1.stats.checkForAchievements();
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			SaveGame.LogVerbose("getLoadEnumerator() exited, elapsed = '" + stopwatch.Elapsed.ToString() + "'");
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle(null);
			}
			SaveGame.IsProcessing = false;
			Game1.player.currentLocation.lastTouchActionLocation = Game1.player.Tile;
			if (Game1.IsMasterGame)
			{
				Game1.player.currentLocation.hostSetup();
				Game1.player.currentLocation.interiorDoors.ResetSharedState();
			}
			Game1.player.currentLocation.resetForPlayerEntry();
			Game1.player.sleptInTemporaryBed.Value = false;
			Game1.player.showToolUpgradeAvailability();
			Game1.player.resetAllTrinketEffects();
			Game1.dayTimeMoneyBox.questsDirty = true;
			yield return 100;
			yield break;
		}

		// Token: 0x06001440 RID: 5184 RVA: 0x000F4DC4 File Offset: 0x000F2FC4
		public static void loadDataToFarmer(Farmer target)
		{
			target.gameVersion = target.gameVersion;
			target.Items.OverwriteWith(target.Items);
			target.canMove = true;
			target.Sprite = new FarmerSprite(null);
			target.songsHeard.Add("title_day");
			target.songsHeard.Add("title_night");
			target.maxItems.Value = target.maxItems.Value;
			for (int i = 0; i < target.maxItems.Value; i++)
			{
				if (target.Items.Count <= i)
				{
					target.Items.Add(null);
				}
			}
			if (target.FarmerRenderer == null)
			{
				target.FarmerRenderer = new FarmerRenderer(target.getTexture(), target);
			}
			target.changeGender(target.IsMale);
			target.changeAccessory(target.accessory.Value);
			target.changeShirt(target.shirt.Value);
			target.changePantsColor(target.GetPantsColor());
			target.changeSkinColor(target.skin.Value, false);
			target.changeHairColor(target.hairstyleColor.Value);
			target.changeHairStyle(target.hair.Value);
			target.changeShoeColor(target.shoes.Value);
			target.changeEyeColor(target.newEyeColor.Value);
			target.Stamina = target.Stamina;
			target.health = target.health;
			target.maxStamina.Value = target.maxStamina.Value;
			target.mostRecentBed = target.mostRecentBed;
			target.Position = target.mostRecentBed;
			target.position.X -= 64f;
			if (!Game1.hasApplied1_3_UpdateChanges)
			{
				SaveMigrator_1_3.MigrateFriendshipData(target);
			}
			target.questLog.RemoveWhere((Quest quest) => quest == null);
			target.ConvertClothingOverrideToClothesItems();
			target.UpdateClothing();
			target._lastEquippedTool = target.CurrentTool;
		}

		// Token: 0x06001441 RID: 5185 RVA: 0x000F4FC4 File Offset: 0x000F31C4
		public static void loadDataToLocations(List<GameLocation> fromLocations)
		{
			Dictionary<string, string> formerLocationNames = SaveGame.GetFormerLocationNames();
			if (formerLocationNames.Count > 0)
			{
				foreach (GameLocation gameLocation in fromLocations)
				{
					foreach (NPC npc in gameLocation.characters)
					{
						string curHome = npc.DefaultMap;
						string newHome;
						if (curHome != null && formerLocationNames.TryGetValue(curHome, out newHome))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Updated ");
							defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
							defaultInterpolatedStringHandler.AppendLiteral("'s home from '");
							defaultInterpolatedStringHandler.AppendFormatted(curHome);
							defaultInterpolatedStringHandler.AppendLiteral("' to '");
							defaultInterpolatedStringHandler.AppendFormatted(newHome);
							defaultInterpolatedStringHandler.AppendLiteral("'.");
							SaveGame.LogDebug(defaultInterpolatedStringHandler.ToStringAndClear());
							npc.DefaultMap = newHome;
						}
					}
				}
			}
			Game1.netWorldState.Value.ParrotPlatformsUnlocked = SaveGame.loaded.parrotPlatformsUnlocked;
			Game1.player.team.farmPerfect.Value = SaveGame.loaded.farmPerfect;
			List<GameLocation> loadedLocations = new List<GameLocation>();
			Dictionary<string, Tuple<NPC, GameLocation>> lostVillagers = new Dictionary<string, Tuple<NPC, GameLocation>>();
			foreach (GameLocation fromLocation in fromLocations)
			{
				GameLocation realLocation = Game1.getLocationFromName(fromLocation.name.Value);
				if (realLocation == null)
				{
					if (fromLocation is Cellar)
					{
						realLocation = Game1.CreateGameLocation("Cellar");
						if (realLocation == null)
						{
							SaveGame.LogError("Couldn't create 'Cellar' location. Was it removed from Data/Locations?", null);
							continue;
						}
						realLocation.name.Value = fromLocation.name.Value;
						Game1.locations.Add(realLocation);
					}
					string realLocationName;
					if (realLocation == null && formerLocationNames.TryGetValue(fromLocation.name.Value, out realLocationName))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Mapped legacy location '");
						defaultInterpolatedStringHandler.AppendFormatted(fromLocation.Name);
						defaultInterpolatedStringHandler.AppendLiteral("' to '");
						defaultInterpolatedStringHandler.AppendFormatted(realLocationName);
						defaultInterpolatedStringHandler.AppendLiteral("'.");
						SaveGame.LogDebug(defaultInterpolatedStringHandler.ToStringAndClear());
						realLocation = Game1.getLocationFromName(realLocationName);
					}
					if (realLocation == null)
					{
						List<string> npcNames = new List<string>();
						foreach (NPC npc2 in fromLocation.characters)
						{
							if (npc2.IsVillager && npc2.Name != null)
							{
								npcNames.Add(npc2.Name);
								lostVillagers[npc2.Name] = Tuple.Create<NPC, GameLocation>(npc2, fromLocation);
							}
						}
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Ignored unknown location '");
						defaultInterpolatedStringHandler.AppendFormatted(fromLocation.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral("' in save data");
						string value;
						if (npcNames.Count <= 0)
						{
							value = "";
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
							defaultInterpolatedStringHandler2.AppendLiteral(", including NPC");
							defaultInterpolatedStringHandler2.AppendFormatted((npcNames.Count > 1) ? "s" : "");
							defaultInterpolatedStringHandler2.AppendLiteral(" '");
							defaultInterpolatedStringHandler2.AppendFormatted(string.Join("', '", from p in npcNames
							orderby p
							select p));
							defaultInterpolatedStringHandler2.AppendLiteral("'");
							value = defaultInterpolatedStringHandler2.ToStringAndClear();
						}
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						continue;
					}
				}
				Farm farm = realLocation as Farm;
				if (farm == null)
				{
					FarmHouse farmHouse = realLocation as FarmHouse;
					if (farmHouse == null)
					{
						Forest forest = realLocation as Forest;
						if (forest == null)
						{
							MovieTheater theater = realLocation as MovieTheater;
							if (theater == null)
							{
								Town town = realLocation as Town;
								if (town == null)
								{
									Beach beach = realLocation as Beach;
									if (beach == null)
									{
										Woods woods = realLocation as Woods;
										if (woods == null)
										{
											CommunityCenter communityCenter = realLocation as CommunityCenter;
											if (communityCenter == null)
											{
												ShopLocation shopLocation = realLocation as ShopLocation;
												if (shopLocation != null)
												{
													ShopLocation fromShopLocation = fromLocation as ShopLocation;
													if (fromShopLocation != null)
													{
														shopLocation.itemsFromPlayerToSell.MoveFrom(fromShopLocation.itemsFromPlayerToSell);
														shopLocation.itemsToStartSellingTomorrow.MoveFrom(fromShopLocation.itemsToStartSellingTomorrow);
													}
												}
											}
											else
											{
												CommunityCenter fromCommunityCenter = fromLocation as CommunityCenter;
												if (fromCommunityCenter != null)
												{
													communityCenter.areasComplete.Set(fromCommunityCenter.areasComplete);
												}
											}
										}
										else
										{
											Woods fromWoods = fromLocation as Woods;
											if (fromWoods != null)
											{
												woods.hasUnlockedStatue.Value = fromWoods.hasUnlockedStatue.Value;
											}
										}
									}
									else
									{
										Beach fromBeach = fromLocation as Beach;
										if (fromBeach != null)
										{
											beach.bridgeFixed.Value = fromBeach.bridgeFixed.Value;
										}
									}
								}
								else
								{
									Town fromTown = fromLocation as Town;
									if (fromTown != null)
									{
										town.daysUntilCommunityUpgrade.Value = fromTown.daysUntilCommunityUpgrade.Value;
									}
								}
							}
							else
							{
								MovieTheater fromTheater = fromLocation as MovieTheater;
								if (fromTheater != null)
								{
									theater.dayFirstEntered.Set(fromTheater.dayFirstEntered.Value);
								}
							}
						}
						else
						{
							Forest fromForest = fromLocation as Forest;
							if (fromForest != null)
							{
								forest.stumpFixed.Value = fromForest.stumpFixed.Value;
								forest.obsolete_log = fromForest.obsolete_log;
							}
						}
					}
					else
					{
						FarmHouse fromFarmHouse = fromLocation as FarmHouse;
						if (fromFarmHouse != null)
						{
							farmHouse.setMapForUpgradeLevel(farmHouse.upgradeLevel);
							farmHouse.fridge.Value = fromFarmHouse.fridge.Value;
							farmHouse.ReadWallpaperAndFloorTileData();
						}
					}
				}
				else
				{
					Farm fromFarm = fromLocation as Farm;
					if (fromFarm != null)
					{
						farm.greenhouseUnlocked.Value = fromFarm.greenhouseUnlocked.Value;
						farm.greenhouseMoved.Value = fromFarm.greenhouseMoved.Value;
						farm.hasSeenGrandpaNote = fromFarm.hasSeenGrandpaNote;
						farm.grandpaScore.Value = fromFarm.grandpaScore.Value;
						farm.UpdatePatio();
					}
				}
				realLocation.TransferDataFromSavedLocation(fromLocation);
				realLocation.animals.MoveFrom(fromLocation.animals);
				realLocation.buildings.Set(fromLocation.buildings);
				realLocation.characters.Set(fromLocation.characters);
				realLocation.furniture.Set(fromLocation.furniture);
				realLocation.largeTerrainFeatures.Set(fromLocation.largeTerrainFeatures);
				realLocation.miniJukeboxCount.Value = fromLocation.miniJukeboxCount.Value;
				realLocation.miniJukeboxTrack.Value = fromLocation.miniJukeboxTrack.Value;
				realLocation.netObjects.Set(fromLocation.netObjects.Pairs);
				realLocation.numberOfSpawnedObjectsOnMap = fromLocation.numberOfSpawnedObjectsOnMap;
				realLocation.piecesOfHay.Value = fromLocation.piecesOfHay.Value;
				realLocation.resourceClumps.Set(new List<ResourceClump>(fromLocation.resourceClumps));
				realLocation.terrainFeatures.Set(fromLocation.terrainFeatures.Pairs);
				if (!SaveGame.loaded.HasSaveFix(SaveFixes.MigrateBuildingsToData))
				{
					SaveMigrator_1_6.ConvertBuildingsToData(realLocation);
				}
				loadedLocations.Add(realLocation);
			}
			SaveGame.MigrateLostVillagers(lostVillagers);
			foreach (GameLocation realLocation2 in loadedLocations)
			{
				realLocation2.AddDefaultBuildings(false);
				foreach (Building b in realLocation2.buildings)
				{
					b.load();
					if (b.GetIndoorsType() == IndoorsType.Instanced)
					{
						GameLocation indoors = b.GetIndoors();
						if (indoors != null)
						{
							indoors.addLightGlows();
						}
					}
				}
				foreach (FarmAnimal farmAnimal in realLocation2.animals.Values)
				{
					farmAnimal.reload(null);
				}
				foreach (Furniture furniture in realLocation2.furniture)
				{
					furniture.updateDrawPosition();
				}
				foreach (LargeTerrainFeature largeTerrainFeature in realLocation2.largeTerrainFeatures)
				{
					largeTerrainFeature.Location = realLocation2;
					largeTerrainFeature.loadSprite();
				}
				foreach (TerrainFeature terrainFeature in realLocation2.terrainFeatures.Values)
				{
					terrainFeature.Location = realLocation2;
					terrainFeature.loadSprite();
					HoeDirt hoe_dirt = terrainFeature as HoeDirt;
					if (hoe_dirt != null)
					{
						hoe_dirt.updateNeighbors();
					}
				}
				foreach (KeyValuePair<Vector2, Object> v in realLocation2.objects.Pairs)
				{
					v.Value.initializeLightSource(v.Key, false);
					v.Value.reloadSprite();
				}
				realLocation2.addLightGlows();
				IslandLocation islandLocation = realLocation2 as IslandLocation;
				if (islandLocation == null)
				{
					FarmCave farmCave = realLocation2 as FarmCave;
					if (farmCave != null)
					{
						farmCave.UpdateReadyFlag();
					}
				}
				else
				{
					islandLocation.AddAdditionalWalnutBushes();
				}
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.characters.Count > 0)
				{
					foreach (NPC npc3 in location.characters.ToArray<NPC>())
					{
						SaveGame.initializeCharacter(npc3, location);
						npc3.reloadSprite(false);
					}
				}
				return true;
			}, true, false);
			Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
		}

		// Token: 0x06001442 RID: 5186 RVA: 0x000F5AA8 File Offset: 0x000F3CA8
		public static void initializeCharacter(NPC c, GameLocation location)
		{
			c.currentLocation = location;
			c.reloadData();
			if (!c.DefaultPosition.Equals(Vector2.Zero))
			{
				c.Position = c.DefaultPosition;
			}
		}

		// Token: 0x06001443 RID: 5187 RVA: 0x000F5AE4 File Offset: 0x000F3CE4
		public static void MigrateLostVillagers(Dictionary<string, Tuple<NPC, GameLocation>> lostVillagers)
		{
			Dictionary<string, string> npcNamesByFormerName = SaveGame.GetFormerNpcNames((string newName, CharacterData _) => Game1.getCharacterFromName(newName, true, false) == null);
			foreach (KeyValuePair<string, Tuple<NPC, GameLocation>> pair in lostVillagers)
			{
				NPC npc = pair.Value.Item1;
				GameLocation lostLocation = pair.Value.Item2;
				string newName3;
				CharacterData characterData;
				if (Game1.getCharacterFromName(npc.Name, true, false) == null && (!npcNamesByFormerName.TryGetValue(npc.Name, out newName3) || Game1.getCharacterFromName(newName3, true, false) == null) && NPC.TryGetData(newName3 ?? npc.Name, out characterData))
				{
					GameLocation home = null;
					string oldName = npc.Name;
					npc.Name = (newName3 ?? oldName);
					string defaultMap = npc.DefaultMap;
					npc.reloadDefaultLocation();
					try
					{
						home = npc.getHome();
					}
					catch (Exception)
					{
						continue;
					}
					npc.Name = oldName;
					if (home != null)
					{
						home.characters.Add(npc);
						npc.currentLocation = home;
						npc.position.Value = npc.DefaultPosition * 64f;
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Moved NPC '");
						defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
						defaultInterpolatedStringHandler.AppendLiteral("' from deleted location '");
						defaultInterpolatedStringHandler.AppendFormatted(lostLocation.Name);
						defaultInterpolatedStringHandler.AppendLiteral("' to their new home in '");
						defaultInterpolatedStringHandler.AppendFormatted(npc.currentLocation.Name);
						defaultInterpolatedStringHandler.AppendLiteral("'.");
						log.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			foreach (KeyValuePair<string, string> pair2 in npcNamesByFormerName)
			{
				string oldName2 = pair2.Key;
				string newName2 = pair2.Value;
				NPC npc2 = Game1.getCharacterFromName(oldName2, true, false);
				if (npc2 != null)
				{
					npc2.Name = newName2;
					foreach (Farmer player in Game1.getAllFarmers())
					{
						if (player.spouse == oldName2)
						{
							player.spouse = newName2;
						}
						Friendship friendship;
						if (player.friendshipData.TryGetValue(oldName2, out friendship))
						{
							player.friendshipData.Remove(oldName2);
							player.friendshipData.TryAdd(newName2, friendship);
						}
						SerializableDictionary<string, int> giftedItems;
						if (player.giftedItems.TryGetValue(oldName2, out giftedItems))
						{
							player.giftedItems.Remove(oldName2);
							player.giftedItems.TryAdd(newName2, giftedItems);
						}
					}
					IGameLogger log2 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Migrated legacy NPC '");
					defaultInterpolatedStringHandler.AppendFormatted(oldName2);
					defaultInterpolatedStringHandler.AppendLiteral("' in save data to '");
					defaultInterpolatedStringHandler.AppendFormatted(newName2);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log2.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}

		// Token: 0x06001444 RID: 5188 RVA: 0x000F5E5C File Offset: 0x000F405C
		public static Dictionary<string, string> GetFormerLocationNames()
		{
			Dictionary<string, string> formerNames = new Dictionary<string, string>();
			foreach (KeyValuePair<string, LocationData> pair in Game1.locationData)
			{
				LocationData data = pair.Value;
				List<string> formerLocationNames = data.FormerLocationNames;
				if (formerLocationNames != null && formerLocationNames.Count > 0)
				{
					foreach (string formerName in data.FormerLocationNames)
					{
						string conflictingId;
						if (Game1.locationData.ContainsKey(formerName))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(129, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Location '");
							defaultInterpolatedStringHandler.AppendFormatted(pair.Key);
							defaultInterpolatedStringHandler.AppendLiteral("' in Data/Locations has former name '");
							defaultInterpolatedStringHandler.AppendFormatted(formerName);
							defaultInterpolatedStringHandler.AppendLiteral("', which can't be added because there's a location with that ID in Data/Locations.");
							SaveGame.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else if (formerNames.TryGetValue(formerName, out conflictingId))
						{
							if (conflictingId != pair.Key)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(113, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Location '");
								defaultInterpolatedStringHandler.AppendFormatted(pair.Key);
								defaultInterpolatedStringHandler.AppendLiteral("' in Data/Locations has former name '");
								defaultInterpolatedStringHandler.AppendFormatted(formerName);
								defaultInterpolatedStringHandler.AppendLiteral("', which can't be added because that name is already mapped to '");
								defaultInterpolatedStringHandler.AppendFormatted(conflictingId);
								defaultInterpolatedStringHandler.AppendLiteral("'.");
								SaveGame.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							}
						}
						else
						{
							formerNames[formerName] = pair.Key;
						}
					}
				}
			}
			return formerNames;
		}

		// Token: 0x06001445 RID: 5189 RVA: 0x000F6028 File Offset: 0x000F4228
		public static Dictionary<string, string> GetFormerNpcNames(Func<string, CharacterData, bool> filter)
		{
			Dictionary<string, string> formerNames = new Dictionary<string, string>();
			foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
			{
				CharacterData data = pair.Value;
				List<string> formerCharacterNames = data.FormerCharacterNames;
				if (formerCharacterNames != null && formerCharacterNames.Count > 0 && filter(pair.Key, data))
				{
					foreach (string formerName in data.FormerCharacterNames)
					{
						string conflictingId;
						if (Game1.characterData.ContainsKey(formerName))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(122, 2);
							defaultInterpolatedStringHandler.AppendLiteral("NPC '");
							defaultInterpolatedStringHandler.AppendFormatted(pair.Key);
							defaultInterpolatedStringHandler.AppendLiteral("' in Data/Characters has former name '");
							defaultInterpolatedStringHandler.AppendFormatted(formerName);
							defaultInterpolatedStringHandler.AppendLiteral("', which can't be added because there's an NPC with that ID in Data/Characters.");
							SaveGame.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else if (formerNames.TryGetValue(formerName, out conflictingId))
						{
							if (conflictingId != pair.Key)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(109, 3);
								defaultInterpolatedStringHandler.AppendLiteral("NPC '");
								defaultInterpolatedStringHandler.AppendFormatted(pair.Key);
								defaultInterpolatedStringHandler.AppendLiteral("' in Data/Characters has former name '");
								defaultInterpolatedStringHandler.AppendFormatted(formerName);
								defaultInterpolatedStringHandler.AppendLiteral("', which can't be added because that name is already mapped to '");
								defaultInterpolatedStringHandler.AppendFormatted(conflictingId);
								defaultInterpolatedStringHandler.AppendLiteral("'.");
								SaveGame.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							}
						}
						else
						{
							formerNames[formerName] = pair.Key;
						}
					}
				}
			}
			return formerNames;
		}

		// Token: 0x06001446 RID: 5190 RVA: 0x000F6204 File Offset: 0x000F4404
		private static void LogVerbose(string message)
		{
			Game1.log.Verbose(message);
		}

		// Token: 0x06001447 RID: 5191 RVA: 0x000F6211 File Offset: 0x000F4411
		private static void LogDebug(string message)
		{
			Game1.log.Debug(message);
		}

		// Token: 0x06001448 RID: 5192 RVA: 0x000F621E File Offset: 0x000F441E
		private static void LogWarn(string message)
		{
			Game1.log.Warn(message);
		}

		// Token: 0x06001449 RID: 5193 RVA: 0x000F622B File Offset: 0x000F442B
		private static void LogError(string message, Exception exception = null)
		{
			Game1.log.Error(message, exception);
		}

		// Token: 0x0600144C RID: 5196 RVA: 0x000F645D File Offset: 0x000F465D
		[CompilerGenerated]
		internal static SaveGame <TryReadSaveFile>g__FileDoesNotExist|99_0(out string outError)
		{
			outError = "File does not exist";
			return null;
		}

		// Token: 0x04000CB2 RID: 3250
		public const string TempNameSuffix = "_STARDEWVALLEYSAVETMP";

		// Token: 0x04000CB3 RID: 3251
		public const string BackupNameSuffix = "_old";

		// Token: 0x04000CB4 RID: 3252
		public const bool PlatformSupportsBackups = true;

		// Token: 0x04000CB5 RID: 3253
		[InstancedStatic]
		public static bool IsProcessing;

		// Token: 0x04000CB6 RID: 3254
		[InstancedStatic]
		public static bool CancelToTitle;

		// Token: 0x04000CB7 RID: 3255
		public Farmer player;

		// Token: 0x04000CB8 RID: 3256
		public List<Farmer> farmhands;

		// Token: 0x04000CB9 RID: 3257
		public List<GameLocation> locations;

		// Token: 0x04000CBA RID: 3258
		public string currentSeason;

		// Token: 0x04000CBB RID: 3259
		public string samBandName;

		// Token: 0x04000CBC RID: 3260
		public string elliottBookName;

		// Token: 0x04000CBD RID: 3261
		[XmlArray("mailbox")]
		public List<string> obsolete_mailbox;

		// Token: 0x04000CBE RID: 3262
		public HashSet<string> broadcastedMail;

		// Token: 0x04000CBF RID: 3263
		public HashSet<string> constructedBuildings;

		// Token: 0x04000CC0 RID: 3264
		public HashSet<string> worldStateIDs;

		// Token: 0x04000CC1 RID: 3265
		public int lostBooksFound = -1;

		// Token: 0x04000CC2 RID: 3266
		public int goldenWalnuts = -1;

		// Token: 0x04000CC3 RID: 3267
		public int goldenWalnutsFound;

		// Token: 0x04000CC4 RID: 3268
		public int miniShippingBinsObtained;

		// Token: 0x04000CC5 RID: 3269
		public bool mineShrineActivated;

		// Token: 0x04000CC6 RID: 3270
		public bool skullShrineActivated;

		// Token: 0x04000CC7 RID: 3271
		public bool goldenCoconutCracked;

		// Token: 0x04000CC8 RID: 3272
		public bool parrotPlatformsUnlocked;

		// Token: 0x04000CC9 RID: 3273
		public bool farmPerfect;

		// Token: 0x04000CCA RID: 3274
		public List<string> foundBuriedNuts = new List<string>();

		// Token: 0x04000CCB RID: 3275
		public List<string> checkedGarbage = new List<string>();

		// Token: 0x04000CCC RID: 3276
		public int visitsUntilY1Guarantee = -1;

		// Token: 0x04000CCD RID: 3277
		public Game1.MineChestType shuffleMineChests;

		// Token: 0x04000CCE RID: 3278
		public int dayOfMonth;

		// Token: 0x04000CCF RID: 3279
		public int year;

		// Token: 0x04000CD0 RID: 3280
		public int? countdownToWedding;

		// Token: 0x04000CD1 RID: 3281
		public double dailyLuck;

		// Token: 0x04000CD2 RID: 3282
		public ulong uniqueIDForThisGame;

		// Token: 0x04000CD3 RID: 3283
		public bool weddingToday;

		// Token: 0x04000CD4 RID: 3284
		public bool isRaining;

		// Token: 0x04000CD5 RID: 3285
		public bool isDebrisWeather;

		// Token: 0x04000CD6 RID: 3286
		public bool isLightning;

		// Token: 0x04000CD7 RID: 3287
		public bool isSnowing;

		// Token: 0x04000CD8 RID: 3288
		public bool shouldSpawnMonsters;

		// Token: 0x04000CD9 RID: 3289
		public bool hasApplied1_3_UpdateChanges;

		// Token: 0x04000CDA RID: 3290
		public bool hasApplied1_4_UpdateChanges;

		// Token: 0x04000CDB RID: 3291
		public List<long> weddingsToday;

		// Token: 0x04000CDC RID: 3292
		[XmlElement("stats")]
		public Stats obsolete_stats;

		// Token: 0x04000CDD RID: 3293
		[InstancedStatic]
		public static SaveGame loaded;

		// Token: 0x04000CDE RID: 3294
		public float musicVolume;

		// Token: 0x04000CDF RID: 3295
		public float soundVolume;

		// Token: 0x04000CE0 RID: 3296
		public Object dishOfTheDay;

		// Token: 0x04000CE1 RID: 3297
		public int highestPlayerLimit = -1;

		// Token: 0x04000CE2 RID: 3298
		public int moveBuildingPermissionMode;

		// Token: 0x04000CE3 RID: 3299
		public bool useLegacyRandom;

		// Token: 0x04000CE4 RID: 3300
		public bool allowChatCheats;

		// Token: 0x04000CE5 RID: 3301
		public bool hasDedicatedHost;

		// Token: 0x04000CE6 RID: 3302
		public SerializableDictionary<string, LocationWeather> locationWeather;

		// Token: 0x04000CE7 RID: 3303
		[XmlArrayItem("item")]
		public SaveablePair<string, BuilderData>[] builders;

		// Token: 0x04000CE8 RID: 3304
		[XmlArrayItem("item")]
		public SaveablePair<string, string>[] bannedUsers = LegacyShims.EmptyArray<SaveablePair<string, string>>();

		// Token: 0x04000CE9 RID: 3305
		[XmlArrayItem("item")]
		public SaveablePair<string, string>[] bundleData = LegacyShims.EmptyArray<SaveablePair<string, string>>();

		// Token: 0x04000CEA RID: 3306
		[XmlArrayItem("item")]
		public SaveablePair<string, int>[] limitedNutDrops = LegacyShims.EmptyArray<SaveablePair<string, int>>();

		// Token: 0x04000CEB RID: 3307
		public long latestID;

		// Token: 0x04000CEC RID: 3308
		public Options options;

		// Token: 0x04000CED RID: 3309
		[XmlArrayItem("item")]
		public SaveablePair<long, Options>[] splitscreenOptions = LegacyShims.EmptyArray<SaveablePair<long, Options>>();

		// Token: 0x04000CEE RID: 3310
		public SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

		// Token: 0x04000CEF RID: 3311
		[XmlArrayItem("item")]
		public SaveablePair<int, MineInfo>[] mine_permanentMineChanges;

		// Token: 0x04000CF0 RID: 3312
		public int mine_lowestLevelReached;

		// Token: 0x04000CF1 RID: 3313
		public string weatherForTomorrow;

		// Token: 0x04000CF2 RID: 3314
		public string whichFarm;

		// Token: 0x04000CF3 RID: 3315
		public int mine_lowestLevelReachedForOrder = -1;

		// Token: 0x04000CF4 RID: 3316
		public int skullCavesDifficulty;

		// Token: 0x04000CF5 RID: 3317
		public int minesDifficulty;

		// Token: 0x04000CF6 RID: 3318
		public int currentGemBirdIndex;

		// Token: 0x04000CF7 RID: 3319
		public NetLeaderboards junimoKartLeaderboards;

		// Token: 0x04000CF8 RID: 3320
		public List<SpecialOrder> specialOrders;

		// Token: 0x04000CF9 RID: 3321
		public List<SpecialOrder> availableSpecialOrders;

		// Token: 0x04000CFA RID: 3322
		public List<string> completedSpecialOrders;

		// Token: 0x04000CFB RID: 3323
		public List<string> acceptedSpecialOrderTypes = new List<string>();

		// Token: 0x04000CFC RID: 3324
		public List<Item> returnedDonations;

		// Token: 0x04000CFD RID: 3325
		public List<Item> junimoChest;

		// Token: 0x04000CFE RID: 3326
		public Item[] shippingBin = LegacyShims.EmptyArray<Item>();

		// Token: 0x04000CFF RID: 3327
		[XmlArrayItem("item")]
		public SaveablePair<string, Item[]>[] globalInventories = LegacyShims.EmptyArray<SaveablePair<string, Item[]>>();

		// Token: 0x04000D00 RID: 3328
		public List<string> collectedNutTracker = new List<string>();

		// Token: 0x04000D01 RID: 3329
		[XmlArrayItem("item")]
		public SaveablePair<FarmerPair, Friendship>[] farmerFriendships = LegacyShims.EmptyArray<SaveablePair<FarmerPair, Friendship>>();

		// Token: 0x04000D02 RID: 3330
		[XmlArrayItem("item")]
		public SaveablePair<int, long>[] cellarAssignments = LegacyShims.EmptyArray<SaveablePair<int, long>>();

		// Token: 0x04000D03 RID: 3331
		public int timesFedRaccoons;

		// Token: 0x04000D04 RID: 3332
		public int treasureTotemsUsed;

		// Token: 0x04000D05 RID: 3333
		public int perfectionWaivers;

		// Token: 0x04000D06 RID: 3334
		public int seasonOfCurrentRaccoonBundle;

		// Token: 0x04000D07 RID: 3335
		public bool[] raccoonBundles = new bool[2];

		// Token: 0x04000D08 RID: 3336
		public bool activatedGoldenParrot;

		// Token: 0x04000D09 RID: 3337
		public int daysPlayedWhenLastRaccoonBundleWasFinished;

		// Token: 0x04000D0A RID: 3338
		public int lastAppliedSaveFix;

		// Token: 0x04000D0B RID: 3339
		public string gameVersion = Game1.version;

		// Token: 0x04000D0C RID: 3340
		public string gameVersionLabel;

		// Token: 0x04000D0D RID: 3341
		public static XmlSerializer serializer = new XmlSerializer(typeof(SaveGame), new Type[]
		{
			typeof(Character),
			typeof(GameLocation),
			typeof(Item),
			typeof(Quest),
			typeof(TerrainFeature)
		});

		// Token: 0x04000D0E RID: 3342
		public static XmlSerializer farmerSerializer = new XmlSerializer(typeof(Farmer), new Type[]
		{
			typeof(Item)
		});

		// Token: 0x04000D0F RID: 3343
		public static XmlSerializer locationSerializer = new XmlSerializer(typeof(GameLocation), new Type[]
		{
			typeof(Character),
			typeof(Item),
			typeof(TerrainFeature)
		});

		// Token: 0x04000D10 RID: 3344
		public static XmlSerializer descriptionElementSerializer = new XmlSerializer(typeof(DescriptionElement), new Type[]
		{
			typeof(Character),
			typeof(Item)
		});

		// Token: 0x04000D11 RID: 3345
		public static XmlSerializer legacyDescriptionElementSerializer = new XmlSerializer(typeof(SaveMigrator_1_6.LegacyDescriptionElement), new Type[]
		{
			typeof(DescriptionElement),
			typeof(Character),
			typeof(Item)
		});
	}
}
