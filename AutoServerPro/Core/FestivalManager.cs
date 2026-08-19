using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using AutoServerPro.Models;
using AutoServerPro.Utils;

namespace AutoServerPro.Core;

public class FestivalManager
{
    private readonly IMonitor _monitor;
    private FestivalState? _currentState = null;
    private SDate? _currentFestivalDate;
    private int _gameClockTicks;
    private bool _waitingForFestivalEndDialog;

    private static readonly Dictionary<(int Day, Season Season), FestivalInfo> FestivalConfigs = new()
    {
        [(13, Season.Spring)] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
        [(24, Season.Spring)] = new FestivalInfo { Location = "Forest", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
        [(11, Season.Summer)] = new FestivalInfo { Location = "Beach", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120, NeedsLuauSoup = true },
        [(28, Season.Summer)] = new FestivalInfo { Location = "Beach", StartTime = 2200, EndTime = 2400, HasCountdown = true, CountdownSeconds = 120 },
        [(16, Season.Fall)] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1500, HasCountdown = true, CountdownSeconds = 120, NeedsPostEventLeave = true },
        [(27, Season.Fall)] = new FestivalInfo { Location = "Town", StartTime = 2200, EndTime = 2350, HasCountdown = false },
        [(8, Season.Winter)] = new FestivalInfo { Location = "Forest", StartTime = 900, EndTime = 1400, HasCountdown = true, CountdownSeconds = 120 },
        [(25, Season.Winter)] = new FestivalInfo { Location = "Town", StartTime = 900, EndTime = 1400, HasCountdown = false }
    };

    public FestivalManager(IMonitor monitor) => _monitor = monitor;

    public bool IsFestivalActive => _currentState?.Active == true;

    public void OnTimeChanged()
    {
        if (!Context.IsWorldReady || !HasPlayers()) return;

        _gameClockTicks++;
        if (_gameClockTicks < 3) return;
        _gameClockTicks = 0;

        SDate today = SDate.Now();
        int now = Game1.timeOfDay;

        if (FestivalConfigs.TryGetValue((today.Day, today.Season), out var festivalInfo) && _currentState == null)
        {
            if (now >= festivalInfo.StartTime && now <= festivalInfo.EndTime)
                StartFestival(festivalInfo, today, now);
        }
    }

    public void OnOneSecondUpdate()
    {
        if (_currentState == null || !_currentState.Active) return;
        var state = _currentState;
        var info = state.LinkedFestivalInfo;
        int now = Game1.timeOfDay;

        if (now < info.StartTime || now > info.EndTime || _currentFestivalDate is null || _currentFestivalDate != SDate.Now())
        {
            LeaveFestival(true);
            return;
        }

        if (state.EventTriggered && Game1.CurrentEvent == null)
        {
            ClearFestivalState();
            return;
        }

        if (state.EventCommandUsed)
        {
            state.EventCommandUsed = false;
            if (info.NeedsLuauSoup) AddLuauIngredient();
        }

        state.ElapsedSeconds++;

        if (!state.EventTriggered && info.HasCountdown && state.ElapsedSeconds >= state.TargetCountdownSeconds)
        {
            if (info.NeedsLuauSoup) AddLuauIngredient();
            AnswerYesFromLewis();
            state.EventTriggered = true;
            state.PostEventSeconds = 0;
        }

        if (state.EventTriggered && info.NeedsPostEventLeave)
        {
            state.PostEventSeconds++;
            if (state.PostEventSeconds >= 10) LeaveFestival(true);
        }

        if (state.ForceLeaveTimer && state.ElapsedSeconds >= 300)
        {
            LeaveFestival(true);
            return;
        }

        if (state.ElapsedSeconds >= 900)
        {
            _monitor.Log("节日超时 > 强制离开", LogLevel.Warn);
            LeaveFestival(true);
        }
    }

    public void HandleFestivalEndDialog(MenuChangedEventArgs e)
    {
        if (_waitingForFestivalEndDialog && e.OldMenu is ReadyCheckDialog && e.NewMenu == null)
        {
            _waitingForFestivalEndDialog = false;
            if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                Game1.CurrentEvent.skipEvent();
        }
    }

    public void ResetOnNewDay()
    {
        _gameClockTicks = 0;
        ClearFestivalState();
    }

    private void StartFestival(FestivalInfo info, SDate date, int enterTime)
    {
        if (string.IsNullOrWhiteSpace(info.Location) || info.Location == "Temp")
        { _monitor.Log($"节日位置无效", LogLevel.Warn); return; }

        _monitor.Log($"节日开始：{date.Day} {date.Season}", LogLevel.Trace);
        Game1.netReady?.SetLocalReady("festivalStart", true);
        Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
        {
            Game1.exitActiveMenu();
            Game1.warpFarmer(info.Location, 1, 20, 1);
        });

        _currentState = new FestivalState
        {
            Active = true,
            FestivalStartTimeOfDay = enterTime,
            TargetCountdownSeconds = info.CountdownSeconds,
            ElapsedSeconds = 0,
            PostEventSeconds = 0,
            EventCommandUsed = false,
            EventTriggered = false,
            ForceLeaveTimer = !info.HasCountdown,
            LinkedFestivalInfo = info
        };
        _currentFestivalDate = date;
    }

    private void LeaveFestival(bool force = false)
    {
        if (_currentState == null) return;
        if (!force && !_currentState.EventTriggered && !_currentState.ForceLeaveTimer) return;

        Game1.netReady?.SetLocalReady("festivalEnd", true);
        Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, who => Game1.exitActiveMenu());
        _waitingForFestivalEndDialog = true;
        ClearFestivalState();
    }

    private void ClearFestivalState()
    {
        _currentState = null;
        _currentFestivalDate = null;
        _waitingForFestivalEndDialog = false;
    }

    private static bool HasPlayers() => Game1.otherFarmers?.Values?.Any(f => f?.isActive() == true) == true;

    private static void AnswerYesFromLewis()
    {
        if (Game1.CurrentEvent != null)
            ReflectionHelper.InvokeMethod(Game1.CurrentEvent, "answerDialogueQuestion", Game1.getCharacterFromName("Lewis"), "yes");
    }

    private static void AddLuauIngredient()
    {
        var item = ItemRegistry.Create("268", 1, 4);
        ReflectionHelper.InvokeMethod(new Event(), "addItemToLuauSoup", item, Game1.player);
    }

    private class FestivalState
    {
        public bool Active;
        public int FestivalStartTimeOfDay;
        public int TargetCountdownSeconds;
        public int ElapsedSeconds;
        public int PostEventSeconds;
        public bool EventCommandUsed;
        public bool EventTriggered;
        public bool ForceLeaveTimer;
        public FestivalInfo LinkedFestivalInfo;
    }
}
