#nullable disable
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using AutoServerPro.Utils;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class AutoSleepManager
{
    private const int MAX_TIME = 2600;
    private readonly IMonitor _monitor;
    private ModConfig _config;
    private readonly IModHelper _helper;

    private bool _goneToSleep = false;
    private bool _isSleeping = false;
    private int _sleepRetryCount = 0;
    private bool _ccDoorUnlocked = false;
    private string _lastSkippedEventId = null;

    public AutoSleepManager(IMonitor monitor, ModConfig config, IModHelper helper)
    {
        _monitor = monitor;
        _config = config;
        _helper = helper;
    }

    public void UpdateConfig(ModConfig config) => _config = config;
    public bool IsSleepingOrGone => _goneToSleep || _isSleeping;
    public bool IsCcDoorUnlocked => _ccDoorUnlocked;
    public void SetCcDoorUnlocked(bool val) => _ccDoorUnlocked = val;
    public void ResetSleepState()
    {
        _goneToSleep = false;
        _isSleeping = false;
        _sleepRetryCount = 0;
        // 清除房主的睡觉播报标记，使第二天可以正常触发（原版每天也会重置 team 状态）
        try
        {
            Game1.player.team?.announcedSleepingFarmers?.Clear();
        }
        catch { }
    }

    public void FixPetName()
    {
        if (!Game1.player.hasPet()) return;
        var farm = Game1.getFarm();
        if (farm == null) return;

        Pet pet = null;
        foreach (var b in farm.buildings)
            if (b is PetBowl bowl && bowl.petId.Value != Guid.Empty)
            {
                pet = farm.characters.OfType<Pet>().FirstOrDefault(p => p.petId.Value == bowl.petId.Value);
                break;
            }
        if (pet != null && pet.Name != _config.petname)
        {
            pet.Name = _config.petname;
            pet.displayName = _config.petname;
        }
    }

    public void HandleEventSkipping()
    {
        if (Game1.CurrentEvent == null) return;
        if (_lastSkippedEventId == Game1.CurrentEvent.id) return;

        // 婚礼事件不能跳过，需要让剧情自然播放到 end wedding，否则配偶不会搬入、婚姻对话不会设置，导致卡住。
        // 婚礼对话由 ModEntry 中的推进逻辑自动点击完成。
        if (Game1.CurrentEvent.isWedding || Game1.CurrentEvent.id == "-2") return;

        if (Game1.CurrentEvent.id == "1590166" || Game1.CurrentEvent.id == "897405")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_config.petname))
                    ReflectionHelper.InvokeMethod(Game1.CurrentEvent, "namePet", _config.petname);
                Game1.CurrentEvent.skipEvent();
                _lastSkippedEventId = Game1.CurrentEvent.id;
            }
            catch { Game1.CurrentEvent.skipEvent(); _lastSkippedEventId = Game1.CurrentEvent.id; }
        }
        else if (Game1.CurrentEvent.id == "65")
        {
            if (Game1.MasterPlayer?.caveChoice != null)
            {
                Game1.MasterPlayer.caveChoice.Value = _config.CreateMushroomCave ? 2 : 1;
                if (_config.CreateMushroomCave)
                    (Game1.getLocationFromName("FarmCave") as FarmCave)?.setUpMushroomHouse();
            }
            Game1.CurrentEvent.skipEvent();
            _lastSkippedEventId = Game1.CurrentEvent.id;
        }
        else
        {
            Game1.CurrentEvent.skipEvent();
            _lastSkippedEventId = Game1.CurrentEvent.id;
        }
    }

    public bool ShouldGoToSleep() => AllPlayersSleeping() || IsDayEnding();

    private bool AllPlayersSleeping() =>
        Game1.getOnlineFarmers()?.Where(f => f != Game1.player).All(f => f?.timeWentToBed?.Value >= 1) == true;

    private bool IsDayEnding() => Game1.timeOfDay >= MAX_TIME - 1;

    public void GoToBed()
    {
        if (_isSleeping || _goneToSleep) return;
        _goneToSleep = true;

        bool inOwnHome = Game1.currentLocation is FarmHouse house && house.owner == Game1.player;
        if (!inOwnHome)
        {
            _monitor.Log("传送回主屋", LogLevel.Info);
            Game1.warpFarmer("FarmHouse", 1, 1, false);
        }

        _isSleeping = true;
        var farmhouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
        if (farmhouse == null)
        {
            _monitor.Log("上床失败：找不到主屋", LogLevel.Error);
            _isSleeping = false;
            _goneToSleep = false;
            return;
        }

        // 房主（机器人）优先使用隐藏床，不占用正常床
        var hiddenBed = farmhouse.furniture.OfType<BedFurniture>()
            .FirstOrDefault(b => b.TileLocation.X == 999 && b.TileLocation.Y == 999);

        if (hiddenBed != null)
        {
            _sleepRetryCount = 0;
            AttemptSleepOnBed(hiddenBed);
            return;
        }

        // 没有隐藏床则创建一个
        _monitor.Log("创建隐藏床供房主使用", LogLevel.Debug);
        hiddenBed = new BedFurniture("2048", new Vector2(999, 999));
        farmhouse.furniture.Add(hiddenBed);
        _sleepRetryCount = 0;
        AttemptSleepOnBed(hiddenBed);
    }

    private void AttemptSleepOnBed(BedFurniture bed)
    {
        try
        {
            Point spot = bed.GetBedSpot();
            Game1.player.Position = new Vector2(spot.X * 64f, spot.Y * 64f);
            BedFurniture.ShiftPositionForBed(Game1.player);

            // 房主是机器人，不播报睡觉提示。
            // startSleep 内部会检查 announcedSleepingFarmers，已存在则跳过播报。
            // 预先把房主加入列表，即可只抑制房主的播报，不影响其他联机玩家。
            if (Game1.player.team?.announcedSleepingFarmers != null &&
                !Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
            {
                Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
            }

            var method = _helper.Reflection.GetMethod(Game1.currentLocation, "startSleep");
            if (method != null)
            {
                method.Invoke();
                _monitor.Log("已上床", LogLevel.Trace);
            }
            else
                _monitor.Log("找不到上床方法", LogLevel.Warn);
        }
        catch (Exception ex)
        {
            _monitor.Log($"上床异常：{ex.Message}", LogLevel.Error);
        }
    }
}
