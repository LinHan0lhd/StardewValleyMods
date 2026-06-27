using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Buildings;

namespace AutoServerPro
{
    public class ModEntry : Mod
    {
        private const int MAX_TIME = 2600;
        private bool _goneToSleep = false;
        private bool _isSleeping = false;
        private int _sleepRetryCount = 0;
        private ModConfig Config = null!;
        private Harmony _harmony = null!;
        private static ModEntry? Instance { get; set; }

        private long _syncSourceId = 0;
        private Queue<long> _joinOrder = new Queue<long>();
        private string? _lastSkippedEventId = null;
        private bool _ccDoorUnlocked = false;

        // 节日系统
        private struct FestivalInfo
        {
            public string Location;
            public int StartTime;
            public int EndTime;
            public bool HasCountdown;
            public int CountdownSeconds;
            public bool NeedsLuauSoup;
            public bool NeedsPostEventLeave;
        }

        private class FestivalState
        {
            public bool Active;
            public int CountdownTicks;
            public int FestivalTicks;
            public bool EventCommandUsed;
            public bool EventTriggered;
            public int PostEventTicks;
            public bool ForceLeaveTimer;
        }

        private static readonly Dictionary<SDate, FestivalInfo> FestivalConfigs = new()
        {
            [new SDate(13, "spring")] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
            [new SDate(24, "spring")] = new FestivalInfo { Location = "Forest", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
            [new SDate(11, "summer")] = new FestivalInfo { Location = "Beach", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120, NeedsLuauSoup = true },
            [new SDate(28, "summer")] = new FestivalInfo { Location = "Beach", StartTime = 2200, EndTime = 2400, HasCountdown = true, CountdownSeconds = 120 },
            [new SDate(16, "fall")] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1500, HasCountdown = true, CountdownSeconds = 120, NeedsPostEventLeave = true },
            [new SDate(27, "fall")] = new FestivalInfo { Location = "Town", StartTime = 2200, EndTime = 2350, HasCountdown = false },
            [new SDate(8, "winter")] = new FestivalInfo { Location = "Forest", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
            [new SDate(25, "winter")] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1400, HasCountdown = false }
        };

        private FestivalState? currentFestivalState;
        private SDate? currentFestivalDate;
        private int gameClockTicks;
        private bool _waitingForFestivalEndDialog = false;

        public bool IsAutomating => true;
        public bool HasPlayers => Game1.otherFarmers?.Values?.Any(f => f?.isActive() == true) == true;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = helper.ReadConfig<ModConfig>();

            // 场景同步玩家ID命令
            helper.ConsoleCommands.Add("sync_player",
                "设置场景同步优先跟随的玩家ID > 用法: sync_player <玩家ID>",
                (cmd, args) =>
                {
                    if (args.Length < 1 || !long.TryParse(args[0], out long id))
                    {
                        Monitor.Log("用法: sync_player <玩家ID>", LogLevel.Info);
                        return;
                    }
                    Config.SyncPlayerId = id;
                    Helper.WriteConfig(Config);
                    Monitor.Log($"已设置优先同步玩家ID: {id}", LogLevel.Info);
                });

            // 配置热重载命令
            helper.ConsoleCommands.Add("reload_config", "重新加载配置文件", (cmd, args) =>
            {
                Config = helper.ReadConfig<ModConfig>();
                Monitor.Log("配置文件已重新加载", LogLevel.Info);
            });

            // 停止服务器命令
            helper.ConsoleCommands.Add("stop", "停止服务器", (cmd, args) =>
            {
                Game1.quit = true;
            });

            // Harmony 补丁（聊天记录）
            try
            {
                _harmony = new Harmony(ModManifest.UniqueID);
                var original = AccessTools.Method(typeof(ChatBox), "receiveChatMessage",
                    new Type[] { typeof(long), typeof(int), typeof(LocalizedContentManager.LanguageCode), typeof(string) });
                if (original == null)
                {
                    Monitor.Log("无法找到 ChatBox.receiveChatMessage 方法 > 聊天记录功能不可用", LogLevel.Warn);
                }
                else
                {
                    var postfix = new HarmonyMethod(typeof(ModEntry), nameof(AfterReceiveChatMessage));
                    _harmony.Patch(original, postfix: postfix);
                    Monitor.Log("Harmony 补丁已安装：聊天消息将被实时记录", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"安装聊天记录补丁失败: {ex.Message}", LogLevel.Error);
            }

            helper.Events.GameLoop.UpdateTicked += OnAutoLoadCheck;
            helper.Events.GameLoop.UpdateTicked += OnServerLogic;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.DayStarted += (_, _) =>
            {
                _goneToSleep = false;
                _isSleeping = false;
                _sleepRetryCount = 0;
                if (Context.IsMainPlayer) AutoReadMail();
            };
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        // ========== 聊天记录处理 ==========
        private static string CleanMessage(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage)) return rawMessage;
            string replaced = Regex.Replace(rawMessage, @"\[\d+\]", "[表情]");
            replaced = Regex.Replace(replaced, @"(\[表情\]\s*)+", "[表情] ");
            return replaced.Trim();
        }

        private static void AfterReceiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            if (chatKind != 0) return;
            Farmer? sender = Game1.GetPlayer(sourceFarmer, true) ?? Game1.MasterPlayer;
            string senderName = sender?.Name ?? $"Unknown ({sourceFarmer})";
            string cleanText = CleanMessage(message);
            if (string.IsNullOrWhiteSpace(cleanText)) return;
            Instance?.Monitor.Log($"[聊天] {senderName}: {cleanText}", LogLevel.Debug);
        }

        // ========== 语言设置 ==========
        private void SetLanguage()
        {
            if (string.IsNullOrWhiteSpace(Config.Language)) return;
            if (Enum.TryParse(Config.Language, true, out LocalizedContentManager.LanguageCode lang))
            {
                LocalizedContentManager.CurrentLanguageCode = lang;
                Monitor.Log($"已设置 {lang} 作为当前游戏语言", LogLevel.Info);
            }
            else Monitor.Log($"配置中的语言 '{Config.Language}' 无效", LogLevel.Warn);
        }

        // ========== 玩家进出 ==========
        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            string playerName = "Unknown";
            try
            {
                Farmer? farmer = Game1.GetPlayer(e.Peer.PlayerID);
                if (farmer != null) playerName = farmer.Name;
                _joinOrder.Enqueue(e.Peer.PlayerID);
                Monitor.Log($"{playerName} [ID: {e.Peer.PlayerID}] 加入了游戏", LogLevel.Info);
            }
            catch (Exception ex) { Monitor.Log($"获取玩家昵称出错: {ex.Message}", LogLevel.Warn); }
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            string playerName = "Unknown";
            try
            {
                Farmer? farmer = Game1.GetPlayer(e.Peer.PlayerID);
                playerName = farmer?.Name ?? $"Player {e.Peer.PlayerID}";
                _joinOrder = new Queue<long>(_joinOrder.Where(id => id != e.Peer.PlayerID));
                Monitor.Log($"{playerName} [ID: {e.Peer.PlayerID}] 退出了游戏", LogLevel.Info);
            }
            catch (Exception ex) { Monitor.Log($"获取退出玩家昵称出错: {ex.Message}", LogLevel.Warn); }
        }

        // ========== 自动加载 ==========
        private void OnAutoLoadCheck(object? sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady) return;
            if (Game1.activeClickableMenu is not TitleMenu) return;
            SetLanguage();
            string? latestSave = GetLatestSave();
            if (string.IsNullOrEmpty(latestSave)) { Monitor.Log("未找到任何存档", LogLevel.Warn); return; }
            Monitor.Log($"加载存档：{latestSave}", LogLevel.Info);
            LoadSave(latestSave!);
            Game1.multiplayerMode = 2;
            Monitor.Log("游戏模式已设置为网络联机模式", LogLevel.Info);
        }

        private string? GetLatestSave()
        {
            string savesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves");
            if (!Directory.Exists(savesPath)) return null!;
            var dirs = Directory.GetDirectories(savesPath);
            if (dirs.Length == 0) return null!;
            return Path.GetFileName(dirs.OrderByDescending(d => Directory.GetLastWriteTime(d)).First());
        }

        private void LoadSave(string saveName)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", saveName);
            if (!Directory.Exists(path)) { Monitor.Log($"存档 {saveName} 不存在", LogLevel.Warn); return; }
            try
            {
                SaveGame.Load(saveName);
                if (Game1.activeClickableMenu is TitleMenu titleMenu) titleMenu.exitThisMenu(false);
                Monitor.Log($"存档 {saveName} 加载成功", LogLevel.Info);
            }
            catch (Exception ex) { Monitor.Log($"加载存档失败：{ex.Message}", LogLevel.Error); }
        }

        // ========== 菜单变化处理 ==========
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShippingMenu)
            {
                Monitor.Log("自动跳过出货结算界面", LogLevel.Debug);
                Helper.Reflection.GetMethod(e.NewMenu, "okClicked").Invoke();
            }
            if (e.NewMenu is LevelUpMenu menu)
            {
                Monitor.Log("自动跳过技能升级菜单", LogLevel.Debug);
                menu.isActive = false;
                menu.informationUp = false;
                menu.isProfessionChooser = false;
                menu.RemoveLevelFromLevelList();
            }

            // 节日结束弹窗消失 → 快速跳过收尾剧情
            if (_waitingForFestivalEndDialog && e.OldMenu is ReadyCheckDialog && e.NewMenu == null)
            {
                _waitingForFestivalEndDialog = false;
                if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    Game1.CurrentEvent.skipEvent();
                }
            }
        }

        // ========== 主逻辑 ==========
        private void OnServerLogic(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Config == null || Game1.player == null || Game1.currentLocation == null) return;
            if (Game1.ticks % 60 != 0) return;

            // 宠物名字持续修正
            if (Game1.player.hasPet())
            {
                Farm? farm = Game1.getFarm();
                if (farm != null)
                {
                    Pet? myPet = null;
                    foreach (Building building in farm.buildings)
                    {
                        if (building is PetBowl bowl && bowl.petId.Value != Guid.Empty)
                        {
                            myPet = farm.characters.OfType<Pet>()
                                .FirstOrDefault(p => p.petId.Value == bowl.petId.Value);
                                break;
                        }
                    }

                    if (myPet != null && myPet.Name != Config.petname)
                    {
                        myPet.Name = Config.petname;
                        myPet.displayName = Config.petname;
                    }
                }
            }

            // 全局自动跳过所有对话（包括选项）
            if (Game1.activeClickableMenu is DialogueBox db)
            {
                db.closeDialogue();
            }

            if (Game1.CurrentEvent == null) _lastSkippedEventId = null;

            if (Game1.CurrentEvent != null && Context.IsMainPlayer)
            {
                // 核心保护
                if (IsAnyFestivalActive || AnyPlayerInTemp() || Game1.CurrentEvent.isFestival || Game1.activeClickableMenu is ReadyCheckDialog)
                    return;

                if (_lastSkippedEventId != null && Game1.CurrentEvent.id == _lastSkippedEventId)
                    return;

                // 宠物命名事件
                if (Game1.CurrentEvent.id == "1590166" || Game1.CurrentEvent.id == "897405")
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(Config.petname))
                        {
                            Helper.Reflection.GetMethod(Game1.CurrentEvent, "namePet")?.Invoke(Config.petname);
                            Monitor.Log($"自动为宠物命名: {Config.petname}", LogLevel.Info);
                        }
                        Game1.CurrentEvent.skipEvent();
                        _lastSkippedEventId = Game1.CurrentEvent.id;
                    }
                    catch
                    {
                        Game1.CurrentEvent.skipEvent();
                        _lastSkippedEventId = Game1.CurrentEvent.id;
                    }
                }
                // 山洞选择
                else if (Game1.CurrentEvent.id == "65")
                {
                    if (Game1.MasterPlayer?.caveChoice != null)
                    {
                        if (Config.farmcavechoicemushrooms)
                        {
                            Game1.MasterPlayer.caveChoice.Value = 2;
                            (Game1.getLocationFromName("FarmCave") as FarmCave)?.setUpMushroomHouse();
                            Monitor.Log("自动选择山洞：蘑菇洞", LogLevel.Info);
                        }
                        else
                        {
                            Game1.MasterPlayer.caveChoice.Value = 1;
                            Monitor.Log("自动选择山洞：水果蝙蝠洞", LogLevel.Info);
                        }
                    }
                    Game1.CurrentEvent.skipEvent();
                    _lastSkippedEventId = Game1.CurrentEvent.id;
                }
                else
                {
                    Game1.CurrentEvent.skipEvent();
                    _lastSkippedEventId = Game1.CurrentEvent.id;
                    Monitor.Log("自动跳过剧情", LogLevel.Debug);
                }
            }

            // 暂停/恢复
            var farmhands = Game1.otherFarmers?.Values?.Where(f => f?.isActive() == true).ToList() ?? new List<Farmer>();
            if (!farmhands.Any())
            {
                if (!Game1.paused) { Game1.paused = true; Monitor.Log("全员离线 > 游戏已暂停", LogLevel.Trace); }
                return;
            }
            if (Game1.paused) { Game1.paused = false; Monitor.Log("成员在线 > 游戏已恢复", LogLevel.Trace); }
            if (!Context.IsMainPlayer || !Context.IsMultiplayer) return;

            // 社区中心解锁
            if (!_ccDoorUnlocked && Game1.year == 1 && Game1.currentSeason == "spring" && Game1.dayOfMonth >= 5 && Game1.timeOfDay >= 800)
            {
                if (Game1.player?.eventsSeen?.Contains("611439") == false)
                {
                    Game1.player.eventsSeen.Add("611439");
                    Game1.MasterPlayer?.mailReceived?.Add("ccDoorUnlock");
                    Monitor.Log("社区中心已解锁", LogLevel.Info);
                }
                _ccDoorUnlocked = true;
            }

            // 场景同步（睡觉及其重试中/节日中/事件中暂停）
            if (Config.EnableSceneSync
                && !IsAnyFestivalActive
                && !_isSleeping
                && !_goneToSleep
                && _sleepRetryCount == 0
                && Game1.CurrentEvent == null)
            {
                UpdateSyncSource();
                if (_syncSourceId != 0 && Game1.otherFarmers?.TryGetValue(_syncSourceId, out Farmer leader) == true && leader?.isActive() == true)
                {
                    if (leader.currentLocation != null
                        && Game1.currentLocation != leader.currentLocation
                        && !leader.currentLocation.Name.StartsWith("UndergroundMine")
                        && leader.currentLocation.Name != "Temp"
                        && leader.currentLocation is not FarmHouse
                        && leader.currentLocation is not Cabin)
                    {
                        Game1.warpFarmer(leader.currentLocation.NameOrUniqueName, 999, 999, false);
                    }
                }
            }

            // 睡觉
            if (!_goneToSleep && !_isSleeping && ShouldGoToSleep())
            {
                GoToBed();
            }
        }

        // ========== 节日处理 ==========
        private bool IsAnyFestivalActive => currentFestivalState?.Active == true;
        private bool AnyPlayerInTemp() => Game1.getOnlineFarmers()?.Any(f => f?.currentLocation?.Name == "Temp") == true;

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!IsAutomating || !HasPlayers) return;
            gameClockTicks++;
            if (gameClockTicks < 3) return;
            gameClockTicks = 0;

            SDate today = SDate.Now();
            int time = Game1.timeOfDay;
            if (IsAnyFestivalActive) return;

            if (FestivalConfigs.TryGetValue(today, out var info))
            {
                if (time >= info.StartTime && time <= info.EndTime)
                    StartFestival(info, today);
            }
        }

        private void StartFestival(FestivalInfo info, SDate date)
        {
            if (string.IsNullOrWhiteSpace(info.Location) || info.Location == "Temp")
            {
                Monitor.Log($"节日位置无效 ({info.Location}) 已跳过", LogLevel.Warn);
                return;
            }

            Monitor.Log($"节日开始：{date.Day} {date.Season}", LogLevel.Trace);
            Game1.netReady?.SetLocalReady("festivalStart", true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
            {
                Game1.exitActiveMenu();
                Game1.warpFarmer(info.Location, 1, 20, 1);
            });
            currentFestivalState = new FestivalState
            {
                Active = true,
                EventTriggered = false,
                ForceLeaveTimer = !info.HasCountdown
            };
            currentFestivalDate = date;
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!IsAutomating || currentFestivalState == null || !currentFestivalState.Active) return;

            if (currentFestivalState.EventTriggered && Game1.CurrentEvent == null)
            {
                currentFestivalState = null;
                currentFestivalDate = null;
                return;
            }

            if (currentFestivalDate == null || !FestivalConfigs.TryGetValue(currentFestivalDate, out var info))
            {
                LeaveFestival();
                return;
            }

            var state = currentFestivalState;
            if (state.EventCommandUsed)
            {
                state.CountdownTicks = info.CountdownSeconds;
                if (info.NeedsLuauSoup) AddLuauIngredient();
                state.EventCommandUsed = false;
            }

            state.CountdownTicks++;
            state.FestivalTicks++;

            if (state.FestivalTicks >= 600)
            {
                Monitor.Log("节日超时 > 强制离开", LogLevel.Warn);
                LeaveFestival();
                return;
            }

            if (state.ForceLeaveTimer)
            {
                if (state.CountdownTicks >= 10)
                {
                    LeaveFestival();
                }
                return;
            }

            if (Game1.CurrentEvent == null) return;

            if (state.CountdownTicks == info.CountdownSeconds && !state.EventTriggered)
            {
                if (info.NeedsLuauSoup) AddLuauIngredient();
                AnswerYesFromLewis();
                state.EventTriggered = true;
                state.PostEventTicks = 0;
            }

            if (state.EventTriggered)
            {
                if (info.NeedsPostEventLeave)
                {
                    state.PostEventTicks++;
                    if (state.PostEventTicks >= 10)
                    {
                        LeaveFestival();
                    }
                }
            }
        }

        private void LeaveFestival()
        {
            if (currentFestivalState?.EventTriggered != true && currentFestivalState?.ForceLeaveTimer != true)
                return;

            Game1.netReady?.SetLocalReady("festivalEnd", true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, who =>
            {
                Game1.exitActiveMenu();
            });
            _waitingForFestivalEndDialog = true;
            currentFestivalState = null;
            currentFestivalDate = null;
        }

        private void AnswerYesFromLewis()
        {
            if (Game1.CurrentEvent != null)
                Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                    ?.Invoke(Game1.getCharacterFromName("Lewis"), "yes");
        }

        private void AddLuauIngredient()
        {
            var item = ItemRegistry.Create("268", 1, 4);
            Helper.Reflection.GetMethod(new Event(), "addItemToLuauSoup")?.Invoke(item, Game1.player);
        }

        // ========== 场景同步源 ==========
        private void UpdateSyncSource()
        {
            while (_joinOrder.Count > 0 && Game1.otherFarmers?.ContainsKey(_joinOrder.Peek()) != true)
                _joinOrder.Dequeue();
            if (Config.SyncPlayerId != 0)
            {
                if (Game1.otherFarmers?.TryGetValue(Config.SyncPlayerId, out var named) == true && named?.isActive() == true)
                {
                    _syncSourceId = Config.SyncPlayerId;
                    return;
                }
            }
            _syncSourceId = _joinOrder.Count > 0 ? _joinOrder.Peek() : 0;
        }

        // ========== 睡觉 ==========
        private bool ShouldGoToSleep() => AllPlayersSleeping() || IsDayEnding();
        private bool AllPlayersSleeping() => Game1.getOnlineFarmers()?.Where(f => f != Game1.player).All(f => f?.timeWentToBed?.Value >= 1) == true;
        private bool IsDayEnding() => Game1.timeOfDay >= MAX_TIME - 1;

        public void GoToBed()
        {
            if (_isSleeping || _goneToSleep) return;

            _goneToSleep = true;

            bool isInOwnHome = false;
            if (Game1.currentLocation is FarmHouse house)
            {
                isInOwnHome = house.owner == Game1.player;
            }

            if (!isInOwnHome)
            {
                Monitor.Log("正在传送回农场主屋…", LogLevel.Info);
                Game1.warpFarmer("FarmHouse", 1, 1, false);
            }

            _isSleeping = true;

            BedFurniture? bed = null;
            var farmhouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
            if (farmhouse != null)
            {
                bed = farmhouse.furniture.OfType<BedFurniture>().FirstOrDefault();
            }

            if (bed != null)
            {
                _sleepRetryCount = 0;
                AttemptSleepOnBed(bed);
            }
            else
            {
                _sleepRetryCount++;
                if (_sleepRetryCount <= 3)
                {
                    Monitor.Log($"没有床 > 重试 {_sleepRetryCount}/3...", LogLevel.Warn);
                    _isSleeping = false;
                    _goneToSleep = false;
                }
                else
                {
                    Monitor.Log("尝试创建隐藏备用床...", LogLevel.Warn);
                    if (farmhouse != null)
                    {
                        var hiddenBed = new BedFurniture("2048", new Vector2(999, 999));
                        farmhouse.furniture.Add(hiddenBed);
                        // 防外挂偷床狗设计：隐藏床位于地图边缘 希望从不触发 玩家友善
                        Monitor.Log("已添加隐藏备用床 > 准备尝试入睡", LogLevel.Info);
                        _sleepRetryCount = 0;
                        AttemptSleepOnBed(hiddenBed);
                        return;
                    }
                    Monitor.Log("操作失败：等待凌晨2点强制过天", LogLevel.Error);
                    _sleepRetryCount = 0;
                }
            }
        }

        private void AttemptSleepOnBed(BedFurniture bed)
        {
            try
            {
                Point bedSpot = bed.GetBedSpot();
                Game1.player.Position = new Vector2(bedSpot.X * 64f, bedSpot.Y * 64f);
                BedFurniture.ShiftPositionForBed(Game1.player);
                var sleepMethod = Helper.Reflection.GetMethod(Game1.currentLocation, "startSleep");
                if (sleepMethod != null) { sleepMethod.Invoke(); Monitor.Log("主机玩家已上床", LogLevel.Info); }
                else Monitor.Log("无法找到上床方法 > 主机无法入睡", LogLevel.Warn);
            }
            catch (Exception ex) { Monitor.Log($"上床失败: {ex.Message}", LogLevel.Error); }
        }

        // ========== 邮件自动处理 ==========
        private void AutoReadMail()
        {
            while (Game1.player.mailbox.Count > 0)
            {
                string mailId = Game1.player.mailbox[0];
                Game1.player.mailbox.RemoveAt(0);
                Game1.player.mailReceived.Add(mailId);
            }
        }
    }
}