#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using AutoServerPro.Core;
using AutoServerPro.Utils;
using AutoServerPro.Models;

namespace AutoServerPro
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }

        private ModConfig _config;
        private SaveManager _saveManager;
        private FestivalManager _festivalManager;
        private AutoSleepManager _sleepManager;
        private SceneSyncManager _syncManager;
        private ChatLogger _chatLogger;
        private CPUDispatcher _cpuDispatcher;

        private bool _hasAutoLoaded = false;
        private bool _hasAutoCreated = false;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            _config = helper.ReadConfig<ModConfig>();
            ReflectionHelper.Initialize(helper);

            _saveManager = new SaveManager(Monitor, _config, helper);
            _festivalManager = new FestivalManager(Monitor);
            _sleepManager = new AutoSleepManager(Monitor, _config, helper);
            _syncManager = new SceneSyncManager(Monitor, _config);
            _chatLogger = new ChatLogger(Monitor, ModManifest.UniqueID);
            _cpuDispatcher = new CPUDispatcher(Monitor, _config);

            _chatLogger.Install();

            if (_config.EnableCPUOptimization)
                _cpuDispatcher.Install();

            RegisterCommands();
            BindEvents();
        }

        private void RegisterCommands()
        {
            // 同步玩家
            Helper.ConsoleCommands.Add("sync_player", "设置同步玩家ID", (_, args) =>
            {
                if (args.Length < 1 || !long.TryParse(args[0], out long id))
                {
                    Monitor.Log("用法: sync_player <玩家ID>", LogLevel.Info);
                    return;
                }
                _config.SyncPlayerId = id;
                Helper.WriteConfig(_config);
                Monitor.Log($"已设置优先同步玩家ID: {id}", LogLevel.Info);
            });

            // 重载配置
            Helper.ConsoleCommands.Add("reload_config", "重新加载配置", (_, __) =>
            {
                _config = Helper.ReadConfig<ModConfig>();
                _saveManager.UpdateConfig(_config);
                _sleepManager.UpdateConfig(_config);
                _syncManager.UpdateConfig(_config);
                _cpuDispatcher.UpdateConfig(_config);
                _cpuDispatcher.ReapplySettings();
                Monitor.Log("配置文件已重新加载", LogLevel.Info);
            });

            // 停止服务器
            Helper.ConsoleCommands.Add("stop", "停止服务器", (_, __) => Game1.quit = true);

            // 手动备份
            Helper.ConsoleCommands.Add("save_backup", "手动备份当前存档", _saveManager.ManualBackupCommand);

            // 聊天指令: chat tell "消息" 或 chat <聊天指令> [参数]
            Helper.ConsoleCommands.Add("chat",
                "聊天: chat tell \"消息\" 广播聊天 | chat <聊天指令> [参数] 执行聊天指令",
                (_, args) => HandleChatCommand(args));
        }

        private void HandleChatCommand(string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("世界未加载，无法使用聊天指令", LogLevel.Warn);
                return;
            }
            if (args.Length < 1)
            {
                Monitor.Log("用法: chat tell \"消息内容\"  或  chat <聊天指令> [参数]", LogLevel.Info);
                Monitor.Log("示例: chat tell 你好 | chat h | chat list", LogLevel.Info);
                return;
            }

            // chat tell "消息" — 向所有玩家广播聊天消息
            if (string.Equals(args[0], "tell", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 2)
                {
                    Monitor.Log("用法: chat tell \"消息内容\"", LogLevel.Info);
                    return;
                }
                string message = string.Join(" ", args.Skip(1));
                if (string.IsNullOrWhiteSpace(message))
                {
                    Monitor.Log("消息内容不能为空", LogLevel.Warn);
                    return;
                }
                Game1.Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, Multiplayer.AllPlayers);
                Game1.chatBox?.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, message);
                Monitor.Log($"已广播聊天: {message}", LogLevel.Info);
                return;
            }

            // chat <聊天指令> [参数] — 执行游戏内聊天指令
            if (Game1.chatBox == null)
            {
                Monitor.Log("聊天框未就绪", LogLevel.Warn);
                return;
            }

            // 记录执行前的消息数，用于提取新产生的输出
            int msgBefore = Game1.chatBox.messages?.Count ?? 0;
            bool handled = ChatCommands.TryHandle(args, Game1.chatBox);

            if (!handled)
            {
                Monitor.Log($"未知的聊天指令: {args[0]}", LogLevel.Warn);
                return;
            }

            // 将新产生的聊天消息输出到控制台
            if (Game1.chatBox.messages != null)
            {
                for (int i = msgBefore; i < Game1.chatBox.messages.Count; i++)
                {
                    var msg = Game1.chatBox.messages[i];
                    string text = ChatMessage.makeMessagePlaintext(msg.message, false);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        Monitor.Log(text, LogLevel.Info);
                    }
                }
            }
        }

        private void BindEvents()
        {
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdate;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            Helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnGameLaunched(object _, GameLaunchedEventArgs __)
        {
            if (_config.EnableCPUOptimization)
                _cpuDispatcher.ReapplySettings();
        }

        private void OnDayStarted(object _, DayStartedEventArgs __)
        {
            _sleepManager.ResetSleepState();
            _festivalManager.ResetOnNewDay();
            if (Context.IsMainPlayer)
            {
                AutoReadMail();
                _saveManager.AutoBackupCheck();
            }
        }

        private void OnUpdateTicked(object _, UpdateTickedEventArgs __)
        {
            if (!Context.IsWorldReady && Game1.activeClickableMenu is TitleMenu)
            {
                if (!_hasAutoLoaded)
                {
                    SetLanguage();
                    bool hasSave = _saveManager.AutoLoadSave();
                    _hasAutoLoaded = true;

                    if (!hasSave && !_hasAutoCreated)
                    {
                        _hasAutoCreated = true;
                        string farmName = _config.DefaultFarmName;
                        string hostName = _config.DefaultHostName;
                        Monitor.Log($"正在自动创建存档 '{farmName}'...", LogLevel.Info);
                        try
                        {
                            _saveManager.CreateNewWorld(farmName, hostName);
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"自动创建失败：{ex.Message}", LogLevel.Error);
                        }
                    }
                }
                return;
            }

            if (!Context.IsWorldReady) return;
            if (Game1.ticks % 60 != 0) return;

            _sleepManager.FixPetName();

            if (Game1.activeClickableMenu is DialogueBox db)
                db.closeDialogue();

            if (Game1.CurrentEvent != null && Context.IsMainPlayer)
            {
                if (_festivalManager.IsFestivalActive || _syncManager.AnyPlayerInTemp() || Game1.CurrentEvent.isFestival || Game1.activeClickableMenu is ReadyCheckDialog)
                    return;
                _sleepManager.HandleEventSkipping();
            }

            var farmhands = Game1.otherFarmers?.Values?.Where(f => f?.isActive() == true).ToList() ?? new List<Farmer>();
            if (!farmhands.Any())
            {
                if (!Game1.paused) { Game1.paused = true; Monitor.Log("全员离线 > 游戏暂停", LogLevel.Trace); }
                return;
            }
            if (Game1.paused) { Game1.paused = false; Monitor.Log("成员在线 > 游戏恢复", LogLevel.Trace); }

            if (!Context.IsMainPlayer || !Context.IsMultiplayer) return;

            if (!_sleepManager.IsCcDoorUnlocked && Game1.year == 1 && Game1.currentSeason == "spring" && Game1.dayOfMonth >= 5 && Game1.timeOfDay >= 800)
            {
                if (Game1.player?.eventsSeen?.Contains("611439") == false)
                {
                    Game1.player.eventsSeen.Add("611439");
                    Game1.MasterPlayer?.mailReceived?.Add("ccDoorUnlock");
                    Monitor.Log("社区中心已解锁", LogLevel.Trace);
                }
                _sleepManager.SetCcDoorUnlocked(true);
            }

            if (_config.EnableSceneSync && !_festivalManager.IsFestivalActive && !_sleepManager.IsSleepingOrGone)
            {
                _syncManager.UpdateSyncSource();
                _syncManager.SyncLocation();
            }

            if (!_sleepManager.IsSleepingOrGone && _sleepManager.ShouldGoToSleep())
                _sleepManager.GoToBed();
        }

        private void OnTimeChanged(object _, TimeChangedEventArgs __)
        {
            _festivalManager.OnTimeChanged();
        }

        private void OnOneSecondUpdate(object _, OneSecondUpdateTickedEventArgs __)
        {
            _festivalManager.OnOneSecondUpdate();
        }

        private void OnMenuChanged(object _, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShippingMenu)
            {
                Monitor.Log("跳过出货结算", LogLevel.Debug);
                Helper.Reflection.GetMethod(e.NewMenu, "okClicked").Invoke();
            }
            if (e.NewMenu is LevelUpMenu menu)
            {
                Monitor.Log("跳过升级菜单", LogLevel.Debug);
                menu.isActive = false;
                menu.informationUp = false;
                menu.isProfessionChooser = false;
                menu.RemoveLevelFromLevelList();
            }
            _festivalManager.HandleFestivalEndDialog(e);
        }

        private void OnPeerConnected(object _, PeerConnectedEventArgs e)
        {
            string name = "Unknown";
            try { name = Game1.GetPlayer(e.Peer.PlayerID)?.Name ?? name; }
            catch { }
            _syncManager.AddPlayer(e.Peer.PlayerID);
            Monitor.Log($"{name} [ID:{e.Peer.PlayerID}] 加入", LogLevel.Debug);
        }

        private void OnPeerDisconnected(object _, PeerDisconnectedEventArgs e)
        {
            string name = "Unknown";
            try { name = Game1.GetPlayer(e.Peer.PlayerID)?.Name ?? name; }
            catch { }
            _syncManager.RemovePlayer(e.Peer.PlayerID);
            Monitor.Log($"{name} [ID:{e.Peer.PlayerID}] 离开", LogLevel.Debug);
        }

        private void SetLanguage()
        {
            if (string.IsNullOrWhiteSpace(_config.Language)) return;
            if (Enum.TryParse(_config.Language, true, out LocalizedContentManager.LanguageCode lang))
            {
                LocalizedContentManager.CurrentLanguageCode = lang;
                Monitor.Log($"设置 {lang} 作为游戏语言", LogLevel.Info);
            }
            else Monitor.Log($"语言 '{_config.Language}' 无效", LogLevel.Warn);
        }

        private void AutoReadMail()
        {
            while (Game1.player.mailbox.Count > 0)
            {
                string id = Game1.player.mailbox[0];
                Game1.player.mailbox.RemoveAt(0);
                Game1.player.mailReceived.Add(id);
            }
        }
    }
}