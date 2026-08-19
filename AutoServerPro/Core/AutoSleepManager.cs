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
    public void ResetSleepState() { _goneToSleep = false; _isSleeping = false; _sleepRetryCount = 0; }

    // 宠物名字修正
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

    // 事件跳过（宠物命名、山洞选择）
    public void HandleEventSkipping()
    {
        if (Game1.CurrentEvent == null) return;
        if (_lastSkippedEventId == Game1.CurrentEvent.id) return;

        // 宠物命名事件
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
        // 山洞选择事件
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

    // 睡觉判断
    public bool ShouldGoToSleep()
    {
        return AllPlayersSleeping() || IsDayEnding();
    }

    private bool AllPlayersSleeping()
    {
        return Game1.getOnlineFarmers()?.Where(f => f != Game1.player).All(f => f?.timeWentToBed?.Value >= 1) == true;
    }

    private bool IsDayEnding() => Game1.timeOfDay >= MAX_TIME - 1;

    // 核心上床逻辑
    public void GoToBed()
    {
        RemoveHiddenBedIfExists();

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
        var bed = farmhouse?.furniture.OfType<BedFurniture>().FirstOrDefault();

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
                _monitor.Log($"没有床 > 重试 {_sleepRetryCount}/3", LogLevel.Warn);
                _isSleeping = false;
                _goneToSleep = false;
            }
            else
            {
                _monitor.Log("创建隐藏备用床", LogLevel.Warn);
                if (farmhouse != null)
                {
                    var hidden = new BedFurniture("2048", new Vector2(999, 999));
                    farmhouse.furniture.Add(hidden);
                    _sleepRetryCount = 0;
                    AttemptSleepOnBed(hidden);
                    return;
                }
                _monitor.Log("上床失败：等待强制过天", LogLevel.Error);
                _sleepRetryCount = 0;
            }
        }
    }

    // 隐藏床自动清理
    private void RemoveHiddenBedIfExists()
    {
        var farmhouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
        if (farmhouse == null) return;

        // 检查是否有任何非隐藏的正常床
        bool hasNormalBed = farmhouse.furniture.OfType<BedFurniture>().Any(b =>
            !(b.TileLocation.X == 999 && b.TileLocation.Y == 999)
        );

        if (hasNormalBed)
        {
            // 移除所有隐藏床（位置 (999,999)）
            var hiddenBeds = farmhouse.furniture.OfType<BedFurniture>()
                .Where(b => b.TileLocation.X == 999 && b.TileLocation.Y == 999)
                .ToList();

            if (hiddenBeds.Any())
            {
                farmhouse.furniture.Remove(hiddenBeds.First());
                _monitor.Log("移除隐藏备用床", LogLevel.Debug);
            }
        }
    }

    // 上床操作
    private void AttemptSleepOnBed(BedFurniture bed)
    {
        try
        {
            Point spot = bed.GetBedSpot();
            Game1.player.Position = new Vector2(spot.X * 64f, spot.Y * 64f);
            BedFurniture.ShiftPositionForBed(Game1.player);
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