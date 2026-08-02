#nullable disable
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;

namespace MasterHand;

// ───── 配置模型 ─────
public class ModConfig
{
    public long FavoredPlayerId { get; set; }
    public bool InfiniteGiftsEnabled { get; set; }
    public List<long> InfiniteGiftsWhitelist { get; set; } = new();
}

// ───── 辅助类：用于包裹 Item ─────
[XmlRoot("Items")]
public class ItemsWrapper
{
    [XmlElement("Item")]
    public Item[] Items { get; set; }
}

// ───── 主入口 ─────
public class ModEntry : Mod
{
    private const string ItemPoolFolder = "ItemPool";
    private const string ModDataResetKey = "MasterHand/ResetOnDisconnect";
    private const string ModDataPoolItemNameKey = "MasterHand/PoolItemName";
    private const string ModDataOrigStackKey = "MasterHand/OriginalStack";
    private const string ModDataOrigQualityKey = "MasterHand/OriginalQuality";

    internal static IMonitor Mon { get; private set; }
    private static new IModHelper Helper { get; set; }
    private static ModConfig Config { get; set; }
    private static long FavoredPlayerId => Config?.FavoredPlayerId ?? 0;

    // 全部 Item 派生类型
    private static readonly Type[] ItemDerivedTypes;

    // 用于包裹 <Items> 反序列化的序列化器
    private static readonly XmlSerializer ItemsWrapperSerializer;

    private bool isTimeFrozen;

    static ModEntry()
    {
        ItemDerivedTypes = Assembly.GetAssembly(typeof(Item))
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract)
            .ToArray();

        ItemsWrapperSerializer = new XmlSerializer(typeof(ItemsWrapper), ItemDerivedTypes);
    }

    public override void Entry(IModHelper helper)
    {
        Mon = Monitor;
        Helper = helper;
        Config = Helper.Data.ReadJsonFile<ModConfig>("config.json") ?? new ModConfig();

        // 注册命令
        helper.ConsoleCommands.Add("mh_list", "列出在线玩家", (_, _) => ListPlayers());
        helper.ConsoleCommands.Add("mh_give", "赠送物品 > mh_give <玩家ID> <物品ID> [数量] [品质]", GiveItem);
        helper.ConsoleCommands.Add("mh_poolitem", "赠送 poolitem 物品 > mh_poolitem list / <玩家ID> <物品名称> [数量] [品质] [reset]", GivePoolItem);
        helper.ConsoleCommands.Add("mh_money", "修改主机金钱 > mh_money <金额>", SetMoney);
        helper.ConsoleCommands.Add("mh_time", "设置时间 > mh_time <600-2600>", SetTime);
        helper.ConsoleCommands.Add("mh_pause", "暂停/继续时间", (_, _) => TogglePause());
        helper.ConsoleCommands.Add("mh_weather", "设置明天天气 > mh_weather <0-5> [地点/all]", SetWeather);
        helper.ConsoleCommands.Add("mh_season", "设置季节 > mh_season <0-3>", SetSeason);
        helper.ConsoleCommands.Add("mh_day", "设置日期 > mh_day <1-28>", SetDay);
        helper.ConsoleCommands.Add("mh_year", "设置年份 > mh_year <年份>", SetYear);
        helper.ConsoleCommands.Add("mh_kick", "踢出玩家 > mh_kick <玩家ID>", KickPlayer);
        helper.ConsoleCommands.Add("mh_favored", "设置/清除眷者 > mh_favored <玩家ID> | clear | show", SetFavoredPlayer);
        helper.ConsoleCommands.Add("mh_giftwl", "无限送礼 > mh_giftwl on|off|list|add|remove|clear", GiftWhitelist);
        helper.ConsoleCommands.Add("mh_demolish", "拆除指定玩家偏移位置的建筑 > mh_demolish <玩家ID> <偏移x> <偏移y>", DemolishBuilding);

        // 事件
        helper.Events.GameLoop.SaveLoaded += (_, _) => ApplyInfiniteGiftsToAllWhitelistedFarmers("存档加载");
        helper.Events.GameLoop.DayStarted += (_, _) => ApplyInfiniteGiftsToAllWhitelistedFarmers("新一天");
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

        // 物品池
        LoadItemPool();

        // Harmony 补丁
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateFriendshipGifts)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_UpdateFriendshipGifts))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Multiplayer), "saveFarmhand", new[] { typeof(NetFarmerRoot) }),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_SaveFarmhand))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(NetWorldState), nameof(NetWorldState.SaveFarmhand)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Postfix_NetWorldState_SaveFarmhand))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Multiplayer), nameof(Multiplayer.playerDisconnected)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_PlayerDisconnected))
        );
    }

    // ─── 工具方法 ───

    private static void SaveConfig() => Helper.Data.WriteJsonFile("config.json", Config);

    private static bool RequireWorldReady()
    {
        if (!Context.IsWorldReady)
            Mon.Log("[警告] 尚未载入存档，无法执行命令", LogLevel.Warn);
        return Context.IsWorldReady;
    }

    private static bool RequireHost()
    {
        if (!Context.IsMainPlayer)
            Mon.Log("[提示] 仅限主机执行此命令", LogLevel.Warn);
        return Context.IsMainPlayer;
    }

    private static Farmer GetOnlinePlayer(long id, bool logError = true)
    {
        var farmer = Game1.GetPlayer(id, true);
        if (farmer == null && logError)
            Mon.Log($"[警告] 玩家 ID {id} 不在线或不存在", LogLevel.Warn);
        return farmer;
    }

    private static int? TryParseIntArg(string[] args, int index, int? min = null, int? max = null, int? fallback = null)
    {
        if (args.Length <= index || args[index] == "~")
            return fallback;
        if (int.TryParse(args[index], out int val))
        {
            if (min.HasValue) val = Math.Max(min.Value, val);
            if (max.HasValue) val = Math.Min(max.Value, val);
            return val;
        }
        return fallback;
    }

    private static bool? TryParseBoolArg(string[] args, int index, bool? fallback = null)
    {
        if (args.Length <= index || args[index] == "~")
            return fallback;
        if (bool.TryParse(args[index], out bool b)) return b;
        if (int.TryParse(args[index], out int i)) return i != 0;
        return fallback;
    }

    private static long ResolvePlayerId(string arg, bool logError = true)
    {
        if (arg == "~")
        {
            if (FavoredPlayerId == 0)
            {
                if (logError) Mon.Log("[错误] 眷者尚未设置", LogLevel.Warn);
                return 0;
            }
            return FavoredPlayerId;
        }
        if (long.TryParse(arg, out long id) && id > 0)
            return id;
        if (logError) Mon.Log($"[错误] 无效的玩家ID: {arg}", LogLevel.Warn);
        return 0;
    }

    // ─── 眷者管理 ───

    private static void SetFavoredPlayer(string _, string[] args)
    {
        if (args.Length == 0)
        {
            Mon.Log("用法: mh_favored <玩家ID> | clear | show", LogLevel.Info);
            return;
        }

        string cmd = args[0].ToLowerInvariant();
        switch (cmd)
        {
            case "clear":
                Config.FavoredPlayerId = 0;
                SaveConfig();
                Mon.Log("[眷者] 已清除", LogLevel.Info);
                break;

            case "show":
                var f = FavoredPlayerId != 0 ? Game1.GetPlayer(FavoredPlayerId, true) : null;
                Mon.Log(f != null
                    ? $"[眷者] {f.Name} [ID: {FavoredPlayerId}]"
                    : $"[眷者] 未设置或不在线 (ID:{FavoredPlayerId})", LogLevel.Info);
                break;

            default:
                if (long.TryParse(cmd, out long id) && id > 0)
                {
                    var farmer = Game1.GetPlayer(id, true);
                    Config.FavoredPlayerId = id;
                    SaveConfig();
                    Mon.Log($"[眷者] 已设置为 {farmer?.Name ?? "未知"} [ID: {id}]", LogLevel.Info);
                }
                else Mon.Log("[错误] 无效的玩家 ID", LogLevel.Warn);
                break;
        }
    }

    // ─── 物品池 ───

    private void LoadItemPool()
    {
        var poolDir = Path.Combine(Helper.DirectoryPath, ItemPoolFolder);
        Directory.CreateDirectory(poolDir);
        var samplePath = Path.Combine(poolDir, "Stardrop.xml");
        if (!File.Exists(samplePath))
        {
            File.WriteAllText(samplePath,
                "<Item xsi:type=\"Object\">\n" +
                "  <name>Stardrop</name>\n" +
                "  <parentSheetIndex>434</parentSheetIndex>\n" +
                "  <itemId>434</itemId>\n" +
                "  <price>7777</price>\n" +
                "  <edibility>100</edibility>\n" +
                "  <category>0</category>\n" +
                "  <type>Crafting</type>\n" +
                "</Item>");
        }
    }

    private static Item LoadPoolItemInternal(string itemName, int stack, int quality)
    {
        string filePath = Path.Combine(Helper.DirectoryPath, ItemPoolFolder, itemName + ".xml");
        if (!File.Exists(filePath))
        {
            Mon.Log($"[警告] 物品 '{itemName}' 不存在于物品池", LogLevel.Warn);
            return null;
        }

        try
        {
            string xml = File.ReadAllText(filePath);
            xml = RemoveXsiNamespace(xml);

            string wrappedXml = $"<Items xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">{xml}</Items>";

            ItemsWrapper wrapper;
            using (var sr = new StringReader(wrappedXml))
                wrapper = (ItemsWrapper)ItemsWrapperSerializer.Deserialize(sr);

            if (wrapper?.Items == null || wrapper.Items.Length == 0)
            {
                Mon.Log($"[错误] 物品 '{itemName}' 的 XML 中没有找到 Item 元素", LogLevel.Warn);
                return null;
            }

            Item item = wrapper.Items[0];

            if (item is not StardewValley.Object obj)
                return item;

            if (stack > 0) obj.Stack = stack;
            if (quality >= 0) obj.Quality = quality;
            return obj;
        }
        catch (Exception ex)
        {
            Mon.Log($"[内部错误] 处理物品 '{itemName}' 时异常: {ex.Message}", LogLevel.Error);
            return null;
        }
    }

    private static string RemoveXsiNamespace(string xml)
        => Regex.Replace(xml, @"\s+xmlns:xsi\s*=\s*[""'][^""']*[""']", "");

    // ─── 送礼 ───

    internal static class GiftProposalManager
    {
        public static bool Send(Farmer target, Item item, string poolItemName = null, bool resetOnDisconnect = false, int? originalStack = null, int? originalQuality = null)
        {
            if (target == null || item == null) return false;

            if (resetOnDisconnect && !string.IsNullOrEmpty(poolItemName))
            {
                item.modData[ModDataResetKey] = "1";
                item.modData[ModDataPoolItemNameKey] = poolItemName;
                item.modData[ModDataOrigStackKey] = (originalStack ?? item.Stack).ToString();
                item.modData[ModDataOrigQualityKey] = (originalQuality ?? item.Quality).ToString();
            }

            if (target.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
            {
                bool added = Game1.player.addItemToInventoryBool(item, true);
                Mon?.Log(added
                    ? $"[成功] 已将 {item.DisplayName} x{item.Stack} 加入主机背包"
                    : $"[失败] 无法添加 {item.DisplayName} (背包可能已满)",
                    added ? LogLevel.Info : LogLevel.Warn);
                return added;
            }

            if (!target.isActive())
            {
                Mon?.Log($"[警告] 目标玩家 {target.Name} 当前离线", LogLevel.Warn);
                return false;
            }

            Game1.player.team.SendProposal(target, ProposalType.Gift, item);
            Mon?.Log($"[赠送] 已向 {target.Name} 发送 {item.DisplayName} x{item.Stack} 提议请求{(resetOnDisconnect ? " [下线重置]" : "")}", LogLevel.Info);
            return true;
        }
    }

    // ─── 命令 ───

    private static void ListPlayers()
    {
        var farmers = Game1.getOnlineFarmers();
        if (!farmers.Any())
        {
            Mon.Log("[列表] 当前无在线玩家", LogLevel.Info);
            return;
        }
        foreach (var f in farmers)
            Mon.Log($"  {f.Name} [ID: {f.UniqueMultiplayerID}]", LogLevel.Info);
    }

    private static void GiveItem(string _, string[] args)
    {
        if (!RequireWorldReady() || args.Length < 2)
        {
            Mon.Log("用法: mh_give <玩家ID> <物品ID> [数量] [品质]", LogLevel.Info);
            return;
        }
        long playerId = ResolvePlayerId(args[0]);
        if (playerId == 0) return;
        var farmer = GetOnlinePlayer(playerId);
        if (farmer == null) return;

        string itemId = args[1];
        int amount = TryParseIntArg(args, 2, 1, 999, 1) ?? 1;
        int quality = TryParseIntArg(args, 3, 0, 4, 0) ?? 0;

        var item = ItemRegistry.Create(itemId, amount, quality);
        if (item == null)
        {
            Mon.Log($"[错误] 物品 ID '{itemId}' 无效", LogLevel.Warn);
            return;
        }
        GiftProposalManager.Send(farmer, item);
    }

    private void GivePoolItem(string _, string[] args)
    {
        if (!RequireWorldReady() || args.Length == 0)
        {
            Mon.Log("用法: mh_poolitem <玩家ID> <物品名称> [数量] [品质] [reset] | list", LogLevel.Info);
            return;
        }

        if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            ListPoolItems();
            return;
        }

        long playerId = ResolvePlayerId(args[0]);
        if (playerId == 0) return;
        var farmer = GetOnlinePlayer(playerId);
        if (farmer == null) return;

        if (args.Length < 2)
        {
            Mon.Log("[警告] 未指定物品名称", LogLevel.Warn);
            return;
        }

        string itemName = args[1];
        int? qty = TryParseIntArg(args, 2, 1, null);
        int? qua = TryParseIntArg(args, 3, 0, 4);
        bool reset = TryParseBoolArg(args, 4, false) ?? false;

        var item = LoadPoolItemInternal(itemName, qty ?? 0, qua ?? -1);
        if (item == null) return;

        GiftProposalManager.Send(farmer, item, itemName, reset, qty, qua);
    }

    private static void ListPoolItems()
    {
        var poolDir = Path.Combine(Helper.DirectoryPath, ItemPoolFolder);
        if (!Directory.Exists(poolDir))
        {
            Mon.Log("[列表] 物品池文件夹不存在", LogLevel.Warn);
            return;
        }
        var files = Directory.GetFiles(poolDir, "*.xml");
        if (files.Length == 0)
        {
            Mon.Log("[列表] 物品池为空", LogLevel.Info);
            return;
        }
        Mon.Log($"━━━ 物品池（共 {files.Length} 件）━━━", LogLevel.Info);
        for (int i = 0; i < files.Length; i++)
            Mon.Log($"  {i + 1,2}. {Path.GetFileNameWithoutExtension(files[i])}", LogLevel.Info);
    }

    // 金钱

    private void SetMoney(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        int? amount = TryParseIntArg(args, 0, 0);
        if (amount == null)
        {
            Mon.Log("用法: mh_money <金额>", LogLevel.Info);
            return;
        }
        Game1.player.Money = amount.Value;
        Mon.Log($"[金钱] 主机金钱已更新为 {amount.Value} 金", LogLevel.Info);
    }

    // 时间

    private void SetTime(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        int? time = TryParseIntArg(args, 0, 600, 2600);
        if (time == null)
        {
            Mon.Log("用法: mh_time <600-2600>", LogLevel.Info);
            return;
        }
        ApplyTimeChange(time.Value);
        Mon.Log($"[时间] 已调整为 {Game1.timeOfDay / 100:D2}:{Game1.timeOfDay % 100:D2}", LogLevel.Info);
    }

    private static void ApplyTimeChange(int targetTime)
    {
        int intervals = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, targetTime) / 10;
        if (intervals > 0)
        {
            for (int i = 0; i < intervals; i++)
                Game1.performTenMinuteClockUpdate();
        }
        else
        {
            for (int i = 0; i < -intervals; i++)
            {
                Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, -20);
                Game1.performTenMinuteClockUpdate();
            }
        }
        Game1.outdoorLight = Color.White;
        Game1.ambientLight = Color.White;
        Game1.gameTimeInterval = 0;
        Game1.UpdateGameClock(Game1.currentGameTime);
    }

    private void TogglePause()
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        isTimeFrozen = !isTimeFrozen;
        Mon.Log(isTimeFrozen ? "[暂停] 时间已冻结" : "[继续] 时间已恢复流动", LogLevel.Info);
    }

    private void OnUpdateTicked(object _, UpdateTickedEventArgs e)
    {
        if (Context.IsWorldReady && isTimeFrozen)
            Game1.gameTimeInterval = 0;
    }

    // 下线重置特殊物品

    public static void Prefix_PlayerDisconnected(long id)
    {
        if (!Context.IsMainPlayer) return;

        // 获取 Multiplayer 实例
        var multiplayerField = AccessTools.Field(typeof(Game1), "multiplayer");
        var multiplayer = multiplayerField?.GetValue(null) as Multiplayer;
        if (multiplayer == null) return;

        // 获取私有的 disconnectingFarmers 列表
        var disconnectingField = AccessTools.Field(typeof(Multiplayer), "disconnectingFarmers");
        var disconnectingList = disconnectingField?.GetValue(multiplayer) as List<long>;

        // 如果玩家 ID 不在“正在断线”列表中 说明已经处理过 直接跳过
        if (disconnectingList == null || !disconnectingList.Contains(id))
            return;

        // 重置在线对象中的物品（不输出日志）
        var onlineFarmer = Game1.GetPlayer(id, true);
        if (onlineFarmer != null)
            ResetMarkedItemsOnDisconnect(onlineFarmer, logOnReset: false);

        // 重置 farmhandData 中的持久化数据
        var farmhandData = Game1.netWorldState?.Value?.farmhandData;
        if (farmhandData != null && farmhandData.FieldDict.TryGetValue(id, out var farmhandRef) && farmhandRef?.Value != null)
        {
            ResetMarkedItemsOnDisconnect(farmhandRef.Value, logOnReset: false);
            farmhandRef.MarkDirty();
            farmhandData.MarkDirty();
            Game1.netWorldState.MarkDirty();
        }
    }

    private static int ResetMarkedItemsOnDisconnect(Farmer farmer, bool logOnReset = false)
    {
        int resetCount = 0;
        for (int i = 0; i < farmer.Items.Count; i++)
        {
            var item = farmer.Items[i];
            if (item == null || !item.modData.TryGetValue(ModDataResetKey, out var v) || v != "1")
                continue;
            if (!item.modData.TryGetValue(ModDataPoolItemNameKey, out var poolName))
                continue;

            int origStack = int.TryParse(item.modData.GetValueOrDefault(ModDataOrigStackKey, "1"), out var s) ? s : 1;
            int origQuality = int.TryParse(item.modData.GetValueOrDefault(ModDataOrigQualityKey, "0"), out var q) ? q : 0;

            var fresh = LoadPoolItemInternal(poolName, origStack, origQuality);
            if (fresh == null)
            {
                Mon.Log($"[重置] 重新加载 {poolName} 失败 保留原物品", LogLevel.Warn);
                continue;
            }

            fresh.modData[ModDataResetKey] = "1";
            fresh.modData[ModDataPoolItemNameKey] = poolName;
            fresh.modData[ModDataOrigStackKey] = origStack.ToString();
            fresh.modData[ModDataOrigQualityKey] = origQuality.ToString();

            farmer.Items[i] = fresh;
            resetCount++;
        }

        if (resetCount > 0 && logOnReset)
            Mon.Log($"[重置] {farmer.Name} 的 {resetCount} 个特殊物品已复原", LogLevel.Info);

        return resetCount;
    }

    // 天气

    private void SetWeather(string _, string[] args)
    {
        if (!RequireWorldReady() || Game1.netWorldState?.Value == null)
        {
            Mon.Log("[错误] 网络世界状态未就绪", LogLevel.Error);
            return;
        }
        if (!RequireHost()) return;

        int? weatherId = TryParseIntArg(args, 0, 0, 5);
        if (weatherId == null)
        {
            Mon.Log("用法: mh_weather <天气代码> [地点]", LogLevel.Info);
            Mon.Log("代码：晴天[0] 刮风[1] 降雨[2] 雷雨[3] 落雪[4] 苔雨[5]", LogLevel.Info);
            Mon.Log("地点: default / island / desert / all", LogLevel.Info);
            return;
        }

        List<string> targets = args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase)
            ? new() { "Default", "Island", "Desert" }
            : new() { ResolveLocationKey(args.ElementAtOrDefault(1)) };

        string weatherStr = weatherId.Value switch
        {
            1 => "Wind",
            2 => "Rain",
            3 => "Storm",
            4 => "Snow",
            5 => "GreenRain",
            _ => "Sun"
        };
        string weatherCn = weatherId.Value switch
        {
            1 => "刮风",
            2 => "雨天",
            3 => "雷雨",
            4 => "降雪",
            5 => "苔雨",
            _ => "晴天"
        };

        try
        {
            foreach (var key in targets)
            {
                var weather = Game1.netWorldState.Value.GetWeatherForLocation(key);
                if (weather == null)
                {
                    Mon.Log($"[警告] 无法获取 {key} 天气数据", LogLevel.Warn);
                    continue;
                }
                weather.WeatherForTomorrow = weatherStr;
                if (key == "Default") Game1.weatherForTomorrow = weatherStr;
                Mon.Log($"[天气] {key} 明日天气: {weatherCn} ({weatherStr})", LogLevel.Info);
            }
        }
        catch (Exception ex)
        {
            Mon.Log($"[异常] 设置天气失败: {ex.Message}", LogLevel.Error);
        }
    }

    private static string ResolveLocationKey(string raw) => raw?.ToLowerInvariant() switch
    {
        "island" or "ginger" => "Island",
        "desert" => "Desert",
        _ => "Default"
    };

    // 日期/季节/年份

    private void SetSeason(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        int? season = TryParseIntArg(args, 0, 0, 3);
        if (season == null)
        {
            Mon.Log("用法: mh_season <0-3>", LogLevel.Info);
            return;
        }
        string seasonStr = season.Value switch { 0 => "spring", 1 => "summer", 2 => "fall", 3 => "winter", _ => "spring" };
        ApplyDate(Game1.dayOfMonth, seasonStr, Game1.year);
        Mon.Log($"[季节] 已切换为 {GetSeasonCnName(season.Value)}", LogLevel.Info);
    }

    private void SetDay(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        int? day = TryParseIntArg(args, 0, 1, 28);
        if (day == null)
        {
            Mon.Log("用法: mh_day <1-28>", LogLevel.Info);
            return;
        }
        ApplyDate(day.Value, Game1.currentSeason, Game1.year);
        Mon.Log($"[日期] 已设为第 {day.Value} 天", LogLevel.Info);
    }

    private void SetYear(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        int? year = TryParseIntArg(args, 0, 1);
        if (year == null)
        {
            Mon.Log("用法: mh_year <年份>", LogLevel.Info);
            return;
        }
        ApplyDate(Game1.dayOfMonth, Game1.currentSeason, year.Value);
        Mon.Log($"[年份] 已设为第 {year.Value} 年", LogLevel.Info);
    }

    private static void ApplyDate(int day, string season, int year)
    {
        bool seasonChanged = season != Game1.currentSeason;
        Game1.dayOfMonth = day;
        Game1.currentSeason = season;
        Game1.year = year;
        Game1.stats.DaysPlayed = (uint)SDate.Now().DaysSinceStart;

        if (Context.IsMainPlayer && Game1.netWorldState?.Value != null)
            Game1.netWorldState.Value.UpdateFromGame1();

        if (seasonChanged)
            Game1.setGraphicsForSeason();
    }

    private static string GetSeasonCnName(int season) => season switch
    {
        0 => "春季",
        1 => "夏季",
        2 => "秋季",
        3 => "冬季",
        _ => "春季"
    };

    // 玩家管理

    private void KickPlayer(string _, string[] args)
    {
        if (!RequireHost()) return;
        if (args.Length < 1 || !long.TryParse(args[0], out long id) || id <= 0)
        {
            Mon.Log("用法: mh_kick <玩家ID>", LogLevel.Info);
            return;
        }
        if (Game1.server == null)
        {
            Mon.Log("[错误] 服务器未运行", LogLevel.Warn);
            return;
        }
        Game1.server.kick(id);
        Mon.Log($"[踢出] 已踢出玩家 ID {id}", LogLevel.Info);
    }

    // 建筑

    private void DemolishBuilding(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        if (args.Length < 3)
        {
            Mon.Log("用法: mh_demolish <玩家ID> <偏移x> <偏移y>", LogLevel.Info);
            Mon.Log("示例: mh_demolish 765611989 0 0  (拆除玩家脚下的建筑)", LogLevel.Info);
            return;
        }

        long playerId = ResolvePlayerId(args[0]);
        if (playerId == 0) return;
        var farmer = GetOnlinePlayer(playerId);
        if (farmer == null) return;

        if (!int.TryParse(args[1], out int offX) || !int.TryParse(args[2], out int offY))
        {
            Mon.Log("[错误] 偏移量必须是整数 (单位: 格)", LogLevel.Warn);
            return;
        }

        // 计算目标坐标
        int tx = (int)farmer.Tile.X + offX;
        int ty = (int)farmer.Tile.Y + offY;
        Vector2 targetTile = new Vector2(tx, ty);

        // 优先查玩家当前所在地图
        GameLocation loc = farmer.currentLocation;
        Building b = loc.getBuildingAt(targetTile);

        // 如果当前地图没有，回退到主农场
        if (b == null && loc != Game1.getFarm())
        {
            loc = Game1.getFarm();
            b = loc.getBuildingAt(targetTile);
        }

        if (b == null)
        {
            Mon.Log($"[错误] 在 ({tx}, {ty}) 没有找到建筑", LogLevel.Warn);
            return;
        }

        // 保护主农舍和温室
        string bType = b.buildingType.Value;
        if ((bType == "Farmhouse" && b.HasIndoorsName("FarmHouse"))
            || (bType == "Greenhouse" && b.HasIndoorsName("Greenhouse")))
        {
            Mon.Log("[错误] 该建筑不允许拆除", LogLevel.Warn);
            return;
        }

        // 走原生拆除流程
        b.BeforeDemolish();                    // 收集任务物品 → 失物招领
        bool ok = loc.destroyStructure(b);     // 原生拆除（Remove + performActionOnDemolition + 网络同步）

        Mon.Log(ok
            ? $"[拆除] 已拆除 {farmer.Name} 偏移({offX},{offY}) 处的 {bType} @({tx},{ty})"
            : $"[错误] 拆除 {bType} 失败", LogLevel.Info);
    }

    // 无限送礼

    private static bool IsInfiniteGiftsEnabled(Farmer farmer) =>
        Config.InfiniteGiftsEnabled && farmer != null && Config.InfiniteGiftsWhitelist.Contains(farmer.UniqueMultiplayerID);

    private static void GiftWhitelist(string _, string[] args)
    {
        if (args.Length == 0)
        {
            Mon.Log("用法: mh_giftwl on|off|list|add <ID>|remove <ID>|clear", LogLevel.Info);
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "on":
                Config.InfiniteGiftsEnabled = true; SaveConfig();
                Mon.Log("[无限送礼] 已开启", LogLevel.Info);
                break;
            case "off":
                Config.InfiniteGiftsEnabled = false; SaveConfig();
                Mon.Log("[无限送礼] 已关闭", LogLevel.Info);
                break;
            case "list":
                Mon.Log($"[无限送礼] 状态{(Config.InfiniteGiftsEnabled ? "开启" : "关闭")}：白名单 {Config.InfiniteGiftsWhitelist.Count} 人", LogLevel.Info);
                foreach (var id in Config.InfiniteGiftsWhitelist)
                {
                    var f = Game1.GetPlayer(id, true);
                    Mon.Log($"  {f?.Name ?? "未知"} [ID: {id}]", LogLevel.Info);
                }
                break;
            case "add":
                {
                    if (args.Length < 2) { Mon.Log("用法: mh_giftwl add <玩家ID>", LogLevel.Warn); return; }
                    long id = ResolvePlayerId(args[1], logError: false);
                    if (id == 0 && long.TryParse(args[1], out long raw) && raw > 0) id = raw;
                    if (id == 0) { Mon.Log("[错误] 无效的玩家 ID", LogLevel.Warn); return; }
                    if (!Config.InfiniteGiftsWhitelist.Contains(id))
                        Config.InfiniteGiftsWhitelist.Add(id);
                    SaveConfig();
                    Mon.Log($"[无限送礼] 已添加 {Game1.GetPlayer(id, true)?.Name ?? "未知"} [ID: {id}]", LogLevel.Info);
                    break;
                }
            case "remove":
                {
                    if (args.Length < 2) { Mon.Log("用法: mh_giftwl remove <玩家ID>", LogLevel.Warn); return; }
                    long id = ResolvePlayerId(args[1], logError: false);
                    if (id == 0 && long.TryParse(args[1], out long raw) && raw > 0) id = raw;
                    if (Config.InfiniteGiftsWhitelist.Remove(id))
                    {
                        SaveConfig();
                        Mon.Log($"[无限送礼] 已移除 [ID: {id}]", LogLevel.Info);
                    }
                    else Mon.Log($"[无限送礼] [ID: {id}] 不在白名单", LogLevel.Warn);
                    break;
                }
            case "clear":
                Config.InfiniteGiftsWhitelist.Clear(); SaveConfig();
                Mon.Log("[无限送礼] 白名单已清空", LogLevel.Info);
                break;
            default:
                Mon.Log($"[错误] 未知子命令: {args[0]}", LogLevel.Warn);
                break;
        }
    }

    private static void ApplyInfiniteGiftsToAllWhitelistedFarmers(string context)
    {
        if (!Context.IsMainPlayer) return;

        if (IsInfiniteGiftsEnabled(Game1.player))
        {
            ReplaceAllFriendships(Game1.player);
        }

        foreach (var farmhand in Game1.netWorldState?.Value?.farmhandData?.Values ?? Enumerable.Empty<Farmer>())
        {
            if (farmhand != null && IsInfiniteGiftsEnabled(farmhand))
            {
                ReplaceAllFriendships(farmhand);
            }
        }
    }

    public static bool Prefix_UpdateFriendshipGifts(Farmer __instance)
    {
        if (__instance == Game1.player && IsInfiniteGiftsEnabled(__instance))
        {
            ReplaceAllFriendships(__instance);
            return false;
        }
        return true;
    }

    public static void Prefix_SaveFarmhand(NetFarmerRoot farmhand)
    {
        if (farmhand?.Value != null && IsInfiniteGiftsEnabled(farmhand.Value))
        {
            ReplaceAllFriendships(farmhand.Value);
        }
    }

    public static void Postfix_NetWorldState_SaveFarmhand(NetFarmerRoot farmhand)
    {
        if (!Context.IsMainPlayer || farmhand?.Value == null) return;
        long id = farmhand.Value.UniqueMultiplayerID;
        var farmhandData = Game1.netWorldState?.Value?.farmhandData;
        if (farmhandData?.FieldDict.TryGetValue(id, out var farmhandRef) != true || farmhandRef?.Value == null)
            return;

        ResetMarkedItemsOnDisconnect(farmhandRef.Value, logOnReset: true);
        farmhandRef.MarkDirty();
        farmhandData.MarkDirty();
    }

    public static void ReplaceAllFriendships(Farmer farmer)
    {
        if (farmer?.friendshipData == null || Game1.NPCGiftTastes == null) return;

        foreach (var npcName in Game1.NPCGiftTastes.Keys)
        {
            var npc = Game1.getCharacterFromName(npcName, true, false);
            if (npc == null || !npc.CanReceiveGifts()) continue;

            if (!farmer.friendshipData.TryGetValue(npcName, out var friendship))
            {
                friendship = new Friendship(0);
                farmer.friendshipData[npcName] = friendship;
            }

            friendship.GiftsToday = -999;
            friendship.GiftsThisWeek = -999;
            friendship.LastGiftDate = new WorldDate(Game1.Date);
        }
    }
}