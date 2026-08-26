#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Triggers;
using AutoServerPro.Core;
using AutoServerPro.Utils;
using AutoServerPro.Models;

namespace AutoServerPro;

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
    private bool _hasPlayerConnected = false;
    private bool _savedAfterAllPlayersOffline = false;

    private bool _hasTeleportedAfterLoad = false;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        _config = helper.ReadConfig<ModConfig>();
        ReflectionHelper.Initialize(helper);

        _festivalManager = new FestivalManager(Monitor);
        _saveManager = new SaveManager(Monitor, _config, helper, _festivalManager);
        _sleepManager = new AutoSleepManager(Monitor, _config, helper);
        _syncManager = new SceneSyncManager(_config, helper);
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

        Helper.ConsoleCommands.Add("stop", "停止服务器（先保存临时存档）", (_, __) =>
        {
            _saveManager.ForceSaveAndQuit();
        });

        Helper.ConsoleCommands.Add("save_backup", "备份当前存档", _saveManager.ManualBackupCommand);

        Helper.ConsoleCommands.Add("save_now", "保存当前进度", (_, __) =>
        {
            _saveManager.ForceSaveNow(allowFestivalQueue: false);
        });

        Helper.ConsoleCommands.Add("chat",
            "聊天: chat tell \"消息\" 广播聊天 | chat <聊天指令> [参数] 执行聊天指令",
            (_, args) => HandleChatCommand(args));
    }

    private void HandleChatCommand(string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Monitor.Log("世界未加载 > 无法使用聊天指令", LogLevel.Warn);
            return;
        }
        if (args.Length < 1)
        {
            Monitor.Log("用法: chat tell \"消息内容\"  或  chat <聊天指令> [参数]", LogLevel.Info);
            Monitor.Log("示例: chat tell 你好 | chat h | chat list", LogLevel.Info);
            return;
        }

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

        if (Game1.chatBox == null)
        {
            Monitor.Log("聊天框未就绪", LogLevel.Warn);
            return;
        }

        int msgBefore = Game1.chatBox.messages?.Count ?? 0;
        bool handled = ChatCommands.TryHandle(args, Game1.chatBox);

        if (!handled)
        {
            Monitor.Log($"未知的聊天指令: {args[0]}", LogLevel.Warn);
            return;
        }

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
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdate;
        Helper.Events.Display.MenuChanged += OnMenuChanged;
        Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
        Helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Helper.Events.GameLoop.DayStarted += OnDayStarted;
    }

    private void OnDayStarted(object _, DayStartedEventArgs __)
    {
        _sleepManager.ResetSleepState();
        _festivalManager.ResetOnNewDay();
        if (Context.IsMainPlayer)
        {
            AutoReadMail();
            _saveManager.AutoBackupCheck();
            if (_config.EnableSceneSync)
                _syncManager.TeleportToFarm();
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
                _hasTeleportedAfterLoad = false;

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

        _saveManager.TickFestivalSaveFlow();

        if (_saveManager.IsWaitingFestivalEnd) return;

        _saveManager.UpdateSave();

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
            if (_config.SaveWhenAllPlayersOffline && Context.IsMainPlayer && _hasPlayerConnected && !_savedAfterAllPlayersOffline)
            {
                _savedAfterAllPlayersOffline = true;
                Monitor.Log("全员离线 > 保存进度", LogLevel.Info);
                _saveManager.ForceSaveNow();
            }
            return;
        }
        if (Game1.paused) { Game1.paused = false; Monitor.Log("成员在线 > 游戏恢复", LogLevel.Trace); }
        _savedAfterAllPlayersOffline = false;

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
            if (!_hasTeleportedAfterLoad)
            {
                _hasTeleportedAfterLoad = true;
                _syncManager.TeleportToFarm();
            }
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
        if (e.NewMenu is SaveGameMenu saveMenu)
        {
            Monitor.Log("跳过保存菜单", LogLevel.Debug);
            saveMenu.hasDrawn = true;
        }
        _festivalManager.HandleFestivalEndDialog(e);
    }

    private void OnPeerConnected(object _, PeerConnectedEventArgs e)
    {
        _hasPlayerConnected = true;
        string name = "NewPlayer";
        try
        {
            name = string.IsNullOrEmpty(Game1.GetPlayer(e.Peer.PlayerID)?.Name)
                ? "NewPlayer"
                : Game1.GetPlayer(e.Peer.PlayerID).Name;
        }
        catch { }

        _syncManager.AddPlayer(e.Peer.PlayerID);
        Monitor.Log($"{name} [ID:{e.Peer.PlayerID}] 加入", LogLevel.Debug);
    }

    private void OnPeerDisconnected(object _, PeerDisconnectedEventArgs e)
    {
        string name = "NewPlayer";
        try
        {
            name = string.IsNullOrEmpty(Game1.GetPlayer(e.Peer.PlayerID)?.Name)
                ? "NewPlayer"
                : Game1.GetPlayer(e.Peer.PlayerID).Name;
        }
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
        var mailData = DataLoader.Mail(Game1.content);
        if (mailData == null) return;

        while (Game1.player.mailbox.Count > 0)
        {
            string mailId = Game1.player.mailbox[0];
            Game1.player.mailbox.RemoveAt(0);
            if (Game1.player.mailReceived.Contains(mailId)) continue;

            try { ProcessMail(mailId, mailData, Game1.player); }
            catch (Exception ex) { Monitor.Log($"[邮件] 处理 '{mailId}' 失败: {ex.Message}", LogLevel.Error); }

            Game1.player.mailReceived.Add(mailId);
        }
    }

    private void ProcessMail(string mailId, IDictionary<string, string> mailData, Farmer target)
    {
        if (mailData == null || !mailData.TryGetValue(mailId, out string mailText) || string.IsNullOrWhiteSpace(mailText)) return;

        foreach (string action in ExtractCommands(mailText, "%action"))
        {
            if (!TriggerActionManager.TryRunAction(action, out string error, out Exception ex))
                Monitor.Log($"[邮件] '{mailId}' 动作失败: {error}", LogLevel.Warn);
        }

        foreach (string itemCmd in ExtractCommands(mailText, "%item"))
        {
            HandleItemCommand(itemCmd, target);
        }

        if (mailId == "winter_18")
        {
            string key = "sawSecretSanta" + Game1.year;
            if (!target.mailReceived.Contains(key)) target.mailReceived.Add(key);
        }
    }

    private IEnumerable<string> ExtractCommands(string text, string prefix)
    {
        int searchFrom = 0;
        while (true)
        {
            int start = text.IndexOf(prefix, searchFrom, StringComparison.InvariantCulture);
            if (start < 0) yield break;
            int end = text.IndexOf("%%", start, StringComparison.InvariantCulture);
            if (end < 0) yield break;
            searchFrom = end + 2;
            yield return text.Substring(start + prefix.Length, end - start - prefix.Length).Trim();
        }
    }

    private void HandleItemCommand(string itemCmd, Farmer target)
    {
        int firstSpace = itemCmd.IndexOf(' ');
        string type = firstSpace >= 0 ? itemCmd.Substring(0, firstSpace).ToLower() : itemCmd.ToLower();
        string argsStr = firstSpace >= 0 ? itemCmd.Substring(firstSpace + 1) : "";
        string[] args = argsStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (string.IsNullOrWhiteSpace(type)) return;

        switch (type)
        {
            case "quest":
                if (args.Length > 0 && !target.mailReceived.Contains("NOQUEST_" + args[0]))
                    target.addQuest(args[0]);
                break;
            case "specialorder":
                if (args.Length > 0 && !target.mailReceived.Contains("NOSPECIALORDER_" + args[0]))
                    target.team.AddSpecialOrder(args[0], null, false);
                break;
            case "conversationtopic":
                if (args.Length >= 2 && int.TryParse(args[1], out int days))
                    target.activeDialogueEvents[args[0]] = days;
                break;
        }
    }
}