#nullable disable
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using AutoServerPro.Utils;
using AutoServerPro.Models;

namespace AutoServerPro.Core
{
    public class SaveManager
    {
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private readonly IModHelper _helper;
        private string _currentSaveName = "";

        public SaveManager(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            _monitor = monitor;
            _config = config;
            _helper = helper;
        }

        public void UpdateConfig(ModConfig config) => _config = config;

        public string SavesRootPath => string.IsNullOrWhiteSpace(_config.CustomSavesPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves")
            : _config.CustomSavesPath;

        public string BackupRootPath => string.IsNullOrWhiteSpace(_config.BackupPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "AutoServerBackups")
            : _config.BackupPath;

        private string GetBackupRootPath()
        {
            string path = BackupRootPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        // ========== 自动加载 ==========
        public bool AutoLoadSave()
        {
            if (!string.IsNullOrWhiteSpace(_config.NewSaveName))
            {
                string dir = Path.Combine(SavesRootPath, _config.NewSaveName);
                if (Directory.Exists(dir))
                {
                    _monitor.Log($"加载指定存档：{_config.NewSaveName}", LogLevel.Info);
                    LoadSave(_config.NewSaveName);
                    Game1.multiplayerMode = 2;
                    return true;
                }
            }

            string latest = GetLatestSave();
            if (string.IsNullOrEmpty(latest))
            {
                _monitor.Log("未找到存档", LogLevel.Info);
                return false;
            }
            _monitor.Log($"加载最新存档：{latest}", LogLevel.Info);
            LoadSave(latest);
            Game1.multiplayerMode = 2;
            return true;
        }

        private string GetLatestSave()
        {
            if (!Directory.Exists(SavesRootPath)) return null;
            var dirs = Directory.GetDirectories(SavesRootPath);
            if (dirs.Length == 0) return null;
            return Path.GetFileName(dirs.OrderByDescending(d => Directory.GetLastWriteTime(d)).First());
        }

        private void LoadSave(string saveName)
        {
            string path = Path.Combine(SavesRootPath, saveName);
            if (!Directory.Exists(path))
            {
                _monitor.Log($"存档 {saveName} 不存在", LogLevel.Warn);
                return;
            }
            try
            {
                SaveGame.Load(saveName);
                _currentSaveName = saveName;
                if (Game1.activeClickableMenu is TitleMenu menu) menu.exitThisMenu(false);
                _monitor.Log($"存档 {saveName} 加载成功", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"加载存档失败：{ex.Message}", LogLevel.Error);
            }
        }

        // ========== 备份系统 ==========
        public void AutoBackupCheck()
        {
            if (!Context.IsWorldReady || string.IsNullOrEmpty(_currentSaveName)) return;
            string saveDir = Path.Combine(SavesRootPath, _currentSaveName);
            if (!Directory.Exists(saveDir)) return;

            string backupRoot = GetBackupRootPath();

            string marker = Path.Combine(saveDir, ".last_backup");
            int today = Game1.Date.TotalDays;

            if (File.Exists(marker) && int.TryParse(File.ReadAllText(marker).Trim(), out int last) && (today - last) < _config.AutoBackupDayInterval)
                return;

            DoSaveBackup(_currentSaveName);
            File.WriteAllText(marker, today.ToString());

            if (_config.AutoCleanOldBackup) CleanOldBackups(backupRoot);
        }

        private void DoSaveBackup(string saveName)
        {
            string src = Path.Combine(SavesRootPath, saveName);
            string time = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dst = Path.Combine(BackupRootPath, $"{saveName}_{time}");
            DirectoryHelper.CopyDirectory(src, dst, true);
            _monitor.Log($"备份完成：{Path.GetFileName(dst)}", LogLevel.Info);
        }

        private void CleanOldBackups(string root)
        {
            if (!Directory.Exists(root)) return;
            var dirs = Directory.GetDirectories(root)
                .Select(d => new DirectoryInfo(d))
                .OrderByDescending(d => d.CreationTime)
                .ToList();
            if (dirs.Count <= _config.MaxBackupCount) return;
            foreach (var d in dirs.Skip(_config.MaxBackupCount))
            {
                Directory.Delete(d.FullName, true);
                _monitor.Log($"清理过期备份：{d.Name}", LogLevel.Debug);
            }
        }

        public void ManualBackupCommand(string _, string[] __)
        {
            if (!Context.IsWorldReady || string.IsNullOrEmpty(_currentSaveName))
            {
                _monitor.Log("请先加载存档", LogLevel.Warn);
                return;
            }
            DoSaveBackup(_currentSaveName);
        }

        // ========== 创建存档 ==========
        public void CreateNewWorld(string saveName, string hostName = null)
        {
            hostName ??= _config.DefaultHostName;

            string targetPath = Path.Combine(SavesRootPath, saveName);
            if (Directory.Exists(targetPath))
            {
                _monitor.Log($"存档 {saveName} 已存在", LogLevel.Error);
                return;
            }

            _monitor.Log($"创建存档：{saveName} ; 农场类型：{_config.FarmType}", LogLevel.Info);

            try
            {
                // ─── 农场类型 ───
                int whichFarm = _config.FarmType;
                if (whichFarm < 0 || whichFarm > 7)
                {
                    _monitor.Log($"无效农场类型 {whichFarm} 使用标准农场", LogLevel.Warn);
                    whichFarm = 0;
                }
                Game1.whichFarm = whichFarm;
                Game1.whichModFarm = null;

                // ─── 玩家身份 ───
                Game1.player.Name = hostName;
                Game1.player.displayName = hostName;
                Game1.player.favoriteThing.Value = "Stardrop";
                Game1.player.farmName.Value = saveName;
                Game1.player.isCustomized.Value = true;
                Game1.player.ConvertClothingOverrideToClothesItems();

                // ─── 宠物品种（支持10种：0-4猫，5-9狗） ───
                const int dogIndex = 5;
                int petBreed = _config.PetBreed;
                if (petBreed >= 0 && petBreed <= 9)
                {
                    if (petBreed < dogIndex)
                    {
                        Game1.player.whichPetType = "Cat";
                        Game1.player.whichPetBreed = petBreed.ToString();
                    }
                    else
                    {
                        Game1.player.whichPetType = "Dog";
                        Game1.player.whichPetBreed = (petBreed - dogIndex).ToString();
                    }
                }

                // ─── 游戏全局选项 ───
                Game1.player.team.useSeparateWallets.Value = _config.UseSeparateWallets;
                Game1.cabinsSeparate = !_config.CabinLayoutNearby;
                Game1.bundleType = _config.BundlesRemix ? Game1.BundleType.Remixed : Game1.BundleType.Default;

                // ─── 设置存档选项 ───
                try
                {
                    var optionsDict = _helper.Reflection.GetField<Dictionary<string, object>>(Game1.game1, "newGameSetupOptions")?.GetValue();
                    if (optionsDict != null)
                    {
                        optionsDict["YearOneCompletable"] = _config.CommunityCenterYear1;

                        var mineChestType = typeof(Game1).GetNestedType("MineChestType",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (mineChestType != null)
                        {
                            object mineChestValue = _config.MinesRemix
                                ? Enum.Parse(mineChestType, "Remixed")
                                : Enum.Parse(mineChestType, "Default");
                            optionsDict["MineChests"] = mineChestValue;
                        }
                        else
                        {
                            optionsDict["MineChests"] = _config.MinesRemix ? "Remixed" : "Default";
                            _monitor.Log("MineChestType 枚举未找到", LogLevel.Warn);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"设置 newGameSetupOptions 失败：{ex.Message}", LogLevel.Warn);
                }

                Game1.UseLegacyRandom = _config.UseLegacyRandom;
                if (_config.RandomSeed.HasValue)
                {
                    Game1.startingGameSeed = _config.RandomSeed.Value;
                }

                Game1.startingCabins = Math.Max(1, _config.StartingCabins);

                if (_config.SpawnMonstersAtNight.HasValue)
                    Game1.spawnMonstersAtNight = _config.SpawnMonstersAtNight.Value;
                else if (whichFarm == 4)
                    Game1.spawnMonstersAtNight = true;

                // ─── 服务器模式 ───
                Game1.multiplayerMode = 2;

                // ─── 核心创建 ───
                Game1.game1.loadForNewGame(false);

                // 利润设置
                Game1.player.difficultyModifier = _config.ProfitMargin;

                // ─── 首日模拟 ───
                Game1.saveOnNewDay = true;
                Game1.player.eventsSeen.Add("60367");
                Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
                Game1.player.Position = new Vector2(9f, 9f) * 64f;
                Game1.player.isInBed.Value = true;
                Game1.NewDay(0f);

                Game1.exitActiveMenu();
                Game1.setGameMode(3);

                _currentSaveName = saveName;
                _monitor.Log($"存档 {saveName} 创建成功", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"创建失败：{ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                Game1.activeClickableMenu = new TitleMenu();
            }
        }
    }
}