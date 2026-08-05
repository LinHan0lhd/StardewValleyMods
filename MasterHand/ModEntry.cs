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
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

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
        helper.ConsoleCommands.Add("mh_buildings", "查询建筑列表 (中文名+英文ID) > mh_buildings [关键词]", ListBuildings);
        helper.ConsoleCommands.Add("mh_build", "自动查询空地建造建筑 > mh_build <建筑ID> [near <玩家ID>|<玩家ID>] [wait] [loc <地点>]", BuildBuilding);
        helper.ConsoleCommands.Add("mh_buildat", "以玩家位置建造建筑 > mh_buildat <玩家ID> <偏移x> <偏移y> <建筑ID> [wait]", BuildBuildingAt);

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
        {
            // 离线时也尝试从所有 farmer 中取名称用于提示
            var any = GetAnyPlayer(id, false);
            Mon.Log(any != null
                ? $"[警告] 玩家 {any.Name} [ID: {id}] 当前不在线"
                : $"[警告] 玩家 ID {id} 不存在", LogLevel.Warn);
        }
        return farmer;
    }

    private static Farmer GetAnyPlayer(long id, bool logError = true)
    {
        foreach (var f in Game1.getAllFarmers())
        {
            if (f != null && f.UniqueMultiplayerID == id)
                return f;
        }
        if (logError)
            Mon.Log($"[警告] 玩家 ID {id} 不存在", LogLevel.Warn);
        return null;
    }

    private static string PlayerName(long id)
    {
        if (id <= 0) return null;
        var f = Game1.GetPlayer(id, true);
        if (f != null) return f.Name;
        foreach (var x in Game1.getAllFarmers())
        {
            if (x != null && x.UniqueMultiplayerID == id)
                return x.Name;
        }
        return null;
    }

    private static Farmer GetPlayerByName(string name, bool logError = true)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        foreach (var f in Game1.getAllFarmers())
        {
            if (f != null && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                return f;
        }
        if (logError)
            Mon.Log($"[警告] 找不到名为 '{name}' 的玩家", LogLevel.Warn);
        return null;
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
        if (arg.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            long hostId = Game1.player?.UniqueMultiplayerID ?? 0;
            if (hostId == 0 && logError) Mon.Log("[错误] 主机玩家不可用", LogLevel.Warn);
            return hostId;
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
                if (FavoredPlayerId == 0)
                {
                    Mon.Log("[眷者] 未设置", LogLevel.Info);
                }
                else
                {
                    string name = PlayerName(FavoredPlayerId);
                    bool online = Game1.GetPlayer(FavoredPlayerId, true) != null;
                    Mon.Log(name != null
                        ? $"[眷者] {name} [ID: {FavoredPlayerId}]{(online ? "" : " (离线)")}"
                        : $"[眷者] 未找到此玩家 (ID:{FavoredPlayerId})", LogLevel.Info);
                }
                break;

            default:
                if (long.TryParse(cmd, out long id) && id > 0)
                {
                    Config.FavoredPlayerId = id;
                    SaveConfig();
                    string name = PlayerName(id);
                    bool online = Game1.GetPlayer(id, true) != null;
                    Mon.Log(name != null
                        ? $"[眷者] 已设置为 {name} [ID: {id}]{(online ? "" : " (离线)")}"
                        : $"[眷者] 已设置 [ID: {id}] (玩家不存在)", LogLevel.Info);
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
        // 列出所有存档玩家（包括离线 farmhand），排除主机自身，标注在线状态
        long hostId = Game1.player?.UniqueMultiplayerID ?? long.MinValue;
        var all = Game1.getAllFarmers()?
            .Where(f => f != null && f.UniqueMultiplayerID != hostId)
            .ToList();
        if (all == null || all.Count == 0)
        {
            Mon.Log("[列表] 不存在其他在线玩家", LogLevel.Info);
            return;
        }
        var onlineIds = new HashSet<long>(Game1.getOnlineFarmers()
            .Where(f => f != null)
            .Select(f => f.UniqueMultiplayerID));
        Mon.Log($"[列表] 共 {all.Count} 名玩家（在线 {all.Count(f => onlineIds.Contains(f.UniqueMultiplayerID))}）", LogLevel.Info);
        foreach (var f in all)
        {
            bool online = onlineIds.Contains(f.UniqueMultiplayerID);
            Mon.Log($"  {f.Name} [ID: {f.UniqueMultiplayerID}]{(online ? "" : " (离线)")}", LogLevel.Info);
        }
    }

    private static void GiveItem(string _, string[] args)
    {
        if (!RequireWorldReady() || args.Length < 2)
        {
            Mon.Log("用法: mh_give <玩家ID> <物品ID> [数量] [品质]", LogLevel.Info);
            Mon.Log("      玩家ID 可填: 数字ID | ~ (眷者) | admin (主机)", LogLevel.Info);
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
            Mon.Log("      玩家ID 可填: 数字ID | ~ (眷者) | admin (主机)", LogLevel.Info);
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
            Mon.Log("      玩家ID 可填: 数字ID | ~ (眷者) | admin (主机)", LogLevel.Info);
            return;
        }

        long playerId = ResolvePlayerId(args[0]);
        if (playerId == 0) return;
        var farmer = GetOnlinePlayer(playerId);
        if (farmer == null) return;

        if (!int.TryParse(args[1], out int offX) || !int.TryParse(args[2], out int offY))
        {
            Mon.Log("[错误] 偏移量必须是整数", LogLevel.Warn);
            return;
        }

        // 计算目标坐标
        int tx = (int)farmer.Tile.X + offX;
        int ty = (int)farmer.Tile.Y + offY;
        Vector2 targetTile = new Vector2(tx, ty);

        // 优先查玩家当前所在地图
        GameLocation loc = farmer.currentLocation;
        Building b = loc.getBuildingAt(targetTile);

        // 回退到主农场
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
            Mon.Log("[错误] 该类建筑不允许拆除", LogLevel.Warn);
            return;
        }

        // 走原生拆除流程
        b.BeforeDemolish();
        bool ok = loc.destroyStructure(b);

        Mon.Log(ok
            ? $"[拆除] 已拆除 {farmer.Name} 偏移 ({offX},{offY}) 处的 {bType} @({tx},{ty})"
            : $"[错误] 拆除 {bType} 失败", LogLevel.Info);
    }

    private void ListBuildings(string _, string[] args)
    {
        if (!RequireWorldReady()) return;

        string filter = args.Length > 0 ? string.Join(" ", args).ToLowerInvariant() : null;
        bool any = false;

        Mon.Log("=== 建筑列表 ===", LogLevel.Info);
        // 黑名单：场地原生唯一建筑，建造会出问题（农舍进不去/温室重复无意义）
        HashSet<string> blacklist = new(StringComparer.OrdinalIgnoreCase) { "Farmhouse", "Greenhouse" };
        foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
        {
            string id = pair.Key;
            BuildingData data = pair.Value;
            if (data == null) continue;
            if (blacklist.Contains(id)) continue;

            string displayName = TokenParser.ParseText(data.Name, null, null, null) ?? id;
            string desc = TokenParser.ParseText(data.Description, null, null, null);

            // 过滤：同时匹配名称、英文ID、描述
            if (filter != null
                && !displayName.ToLowerInvariant().Contains(filter)
                && !id.ToLowerInvariant().Contains(filter)
                && (desc == null || !desc.ToLowerInvariant().Contains(filter)))
                continue;

            string cn = displayName == id ? displayName : $"{displayName} / {id}";
            string size = $"{data.Size.X}x{data.Size.Y}";
            string cost = data.BuildCost > 0 ? $"{data.BuildCost}g" : "免费";
            string days = data.BuildDays > 0 ? $"{data.BuildDays}天" : "即时";
            string builder = data.Builder ?? "?";
            string upgrade = !string.IsNullOrEmpty(data.BuildingToUpgrade) ? $" 升级自:{data.BuildingToUpgrade}" : "";

            Mon.Log($"  [{cn}] 大小:{size} 费用:{cost} 工期:{days} 建造者:{builder}{upgrade}", LogLevel.Info);
            any = true;

            // Cabin 额外列出 7 种风格（可直接用 mh_build "Stone Cabin" 指定）
            if (id == "Cabin" && (filter == null
                || "stone".Contains(filter) || "log".Contains(filter) || "plank".Contains(filter)
                || "rustic".Contains(filter) || "trailer".Contains(filter) || "neighbor".Contains(filter)
                || "beach".Contains(filter)
                || displayName.ToLowerInvariant().Contains(filter)))
            {
                string[] cabinStyles = { "Stone Cabin", "Log Cabin", "Plank Cabin", "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin" };
                foreach (string s in cabinStyles)
                {
                    if (filter == null || s.ToLowerInvariant().Contains(filter)
                        || "石".Contains(filter) || "木".Contains(filter) || "小屋".Contains(filter) || "风格".Contains(filter))
                    {
                        string styleCn = TokenParser.ParseText($"[{s}]", null, null, null);
                        styleCn = styleCn != null && styleCn.StartsWith("[") && styleCn.EndsWith("]")
                            ? styleCn.Substring(1, styleCn.Length - 2) : s;
                        Mon.Log($"    风格: {styleCn} / {s}  (用 mh_build \"{s}\" 直接建造)", LogLevel.Info);
                    }
                }
            }
        }
        if (!any)
            Mon.Log(filter == null ? "  (无建筑数据)" : $"  (未匹配到包含 '{filter}' 的建筑)", LogLevel.Info);
        else
            Mon.Log(">> 提示: 使用 mh_build <英文ID> 小屋风格可直接当 ID 使用", LogLevel.Info);
    }

    private void BuildBuilding(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        if (args.Length == 0)
        {
            Mon.Log("用法: mh_build <建筑ID> [near <玩家ID>|<玩家ID>] [wait] [loc <地点>]", LogLevel.Info);
            Mon.Log("常见地点: Farm(农场) | IslandWest(姜岛农场)", LogLevel.Info);
            Mon.Log("玩家ID 可填: 数字ID | ~ (眷者) | admin (主机)", LogLevel.Info);
            Mon.Log("小屋风格: Stone Cabin | Log Cabin | Plank Cabin | Rustic Cabin | Trailer Cabin | Neighbor Cabin | Beach Cabin", LogLevel.Info);
            Mon.Log("示例:", LogLevel.Info);
            Mon.Log("  mh_build Mill                       自动在农场找空地建磨坊(即时)", LogLevel.Info);
            Mon.Log("  mh_build Cabin                      建小屋(自动按现有数量循环分配风格)", LogLevel.Info);
            Mon.Log("  mh_build \"Stone Cabin\"            建石屋(指定风格)", LogLevel.Info);
            Mon.Log("  mh_build Silo near 1145140          在指定玩家附近找空地", LogLevel.Info);
            Mon.Log("  mh_build Silo 1145140               上方指令简写形式", LogLevel.Info);
            Mon.Log("  mh_build Silo near admin            在主机附近找空地", LogLevel.Info);
            Mon.Log("  mh_build Mill wait                  走正常工期(不即时)", LogLevel.Info);
            Mon.Log("  mh_build Mill loc Farm              指定建造地点(默认 Farm)", LogLevel.Info);
            Mon.Log("  mh_build \"Shipping Bin\" loc IslandWest  在姜岛农场建额外出货箱", LogLevel.Info);
            return;
        }

        string typeId = args[0];

        // 解析可选参数
        bool instant = true;        // 默认即时
        long nearPlayerId = 0;      // 0 = 不指定玩家
        string locName = "Farm";    // 默认地点

        for (int i = 1; i < args.Length; i++)
        {
            string a = args[i];
            if (a.Equals("wait", StringComparison.OrdinalIgnoreCase))
            {
                instant = false;
            }
            else if (a.Equals("near", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                nearPlayerId = ResolvePlayerId(args[++i], logError: false);
                if (nearPlayerId == 0)
                {
                    Mon.Log($"[错误] 无效的玩家ID: {args[i]}", LogLevel.Warn);
                    return;
                }
            }
            else if (a.Equals("loc", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                locName = args[++i];
            }
            else if (long.TryParse(a, out long pid) && pid > 0)
            {
                // 简写: mh_build <id> <玩家ID>
                nearPlayerId = pid;
            }
            else
            {
                Mon.Log($"[警告] 忽略未知参数: {a}", LogLevel.Warn);
            }
        }

        // 允许风格小屋作为别名
        string forceSkinId = null;
        string[] cabinStyles = { "Stone Cabin", "Log Cabin", "Plank Cabin", "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin" };
        if (cabinStyles.Contains(typeId, StringComparer.OrdinalIgnoreCase))
        {
            forceSkinId = typeId;
            typeId = "Cabin";
        }

        // 取建筑数据
        if (!Building.TryGetData(typeId, out BuildingData data) || data == null)
        {
            Mon.Log($"[错误] 找不到建筑 '{typeId}'，使用 mh_buildings 查询可用建筑", LogLevel.Warn);
            return;
        }

        // 黑名单：禁止建造唯一建筑
        string[] forbiddenIds = { "Farmhouse", "Greenhouse" };
        if (forbiddenIds.Contains(typeId, StringComparer.OrdinalIgnoreCase))
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 为场地原生唯一建筑，禁止建造", LogLevel.Warn);
            return;
        }

        // Cabin 必须建在 Farm
        if (typeId.Equals("Cabin", StringComparison.OrdinalIgnoreCase) && locName != "Farm")
        {
            Mon.Log("[错误] Cabin 只能建造在 Farm", LogLevel.Warn);
            return;
        }

        // 取目标地点
        GameLocation loc = Game1.getLocationFromName(locName);
        if (loc == null)
        {
            Mon.Log($"[错误] 找不到地点 '{locName}'", LogLevel.Warn);
            return;
        }
        if (!loc.IsBuildableLocation())
        {
            Mon.Log($"[错误] 地点 '{locName}' 不允许建造建筑", LogLevel.Warn);
            return;
        }

        // 游戏内建 BuildCondition
        if (!string.IsNullOrEmpty(data.BuildCondition)
            && !GameStateQuery.CheckConditions(data.BuildCondition, loc, Game1.player, null, null, null, null))
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 不满足建造条件", LogLevel.Warn);
            return;
        }

        // 升级类建筑：要求已建造 BuildingToUpgrade 的前一级
        if (!string.IsNullOrEmpty(data.BuildingToUpgrade)
            && loc.getNumberBuildingsConstructed(data.BuildingToUpgrade, false) == 0)
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 为升级建筑 & 需先建造 {data.BuildingToUpgrade}", LogLevel.Warn);
            return;
        }

        string displayName = TokenParser.ParseText(data.Name, null, null, null) ?? typeId;
        int w = Math.Max(1, data.Size.X);
        int h = Math.Max(1, data.Size.Y);

        // 决定搜索起点
        Vector2 center;
        if (nearPlayerId != 0)
        {
            Farmer farmer = GetOnlinePlayer(nearPlayerId);
            if (farmer == null) return;
            // 玩家若在农场内则用其位置；否则在农场可建区域内找近玩家位置
            if (farmer.currentLocation == loc)
                center = new Vector2((int)farmer.Tile.X, (int)farmer.Tile.Y);
            else
                center = GetBuildableCenter(loc, data);
        }
        else
        {
            center = GetBuildableCenter(loc, data);
        }

        // Cabin 自动分配风格
        bool isCabin = typeId.Equals("Cabin", StringComparison.OrdinalIgnoreCase);
        if (isCabin && forceSkinId == null)
        {
            string[] defaultOrder = { "Stone Cabin", "Log Cabin", "Plank Cabin", "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin" };
            int cabinCount = 0;
            foreach (Building b in loc.buildings)
                if (b.buildingType.Value == "Cabin") cabinCount++;
            forceSkinId = defaultOrder[cabinCount % defaultOrder.Length];
        }

        Mon.Log($"[建造] 准备在 {locName} 自动选址建造 {displayName}"
            + (forceSkinId != null ? $" [{forceSkinId}]" : "")
            + $" ({typeId}) 大小 {w}x{h} ...", LogLevel.Info);

        Vector2 found = Vector2.Zero;
        Building built = null;
        bool foundSpot = false;

        // 阶段 1：以起点为中心螺旋搜索
        const int spiralRadius = 255;
        foreach (Vector2 tile in EnumerateSpiralTiles(center, spiralRadius))
        {
            if (!IsWithinBuildableRect(loc, tile, w, h)) continue;
            if (!CanPlaceBuilding(loc, data, tile)) continue;
            if (TryPlace(loc, typeId, data, forceSkinId, instant, tile, out built, out found))
            {
                foundSpot = true;
                break;
            }
        }

        // 阶段 2：螺旋没覆盖到则对整个可建造矩形做逐格兜底扫描
        if (!foundSpot)
        {
            Rectangle rect = loc.GetBuildableRectangle();
            if (rect == Rectangle.Empty)
            {
                try { rect = new Rectangle(0, 0, Math.Min(255, loc.Map.Layers[0].LayerWidth), Math.Min(255, loc.Map.Layers[0].LayerHeight)); }
                catch { rect = new Rectangle(0, 0, 255, 255); }
            }
            for (int y = rect.Y; y + h <= rect.Y + rect.Height; y++)
                for (int x = rect.X; x + w <= rect.X + rect.Width; x++)
                {
                    Vector2 tile = new Vector2(x, y);
                    if (!CanPlaceBuilding(loc, data, tile)) continue;
                    if (TryPlace(loc, typeId, data, forceSkinId, instant, tile, out built, out found))
                    {
                        foundSpot = true;
                        break;
                    }
                }
            if (foundSpot)
                Mon.Log("[建造] 螺旋搜索未命中 & 已在兜底扫描阶段找到空地", LogLevel.Trace);
        }

        if (!foundSpot)
        {
            Mon.Log($"[错误] 在 {locName} 范围内未找到可建造 {displayName} ({w}x{h}) 的空地", LogLevel.Warn);
            return;
        }

        Mon.Log($"[成功] 已在 {locName} ({(int)found.X},{(int)found.Y}) 建造 {displayName}"
            + (forceSkinId != null ? $" [{forceSkinId}]" : "")
            + (instant ? " (即时)" : " (工期中)"), LogLevel.Info);
    }

    private static bool TryPlace(GameLocation loc, string typeId, BuildingData data, string forceSkinId, bool instant, Vector2 tile, out Building built, out Vector2 outPos)
    {
        built = null;
        outPos = tile;
        if (forceSkinId != null)
        {
            Building building = Building.CreateInstanceFromId(typeId, tile);
            building.owner.Value = Game1.player.UniqueMultiplayerID;
            building.skinId.Value = forceSkinId;
            if (instant)
            {
                building.magical.Value = true;
                building.daysOfConstructionLeft.Value = 0;
            }
            if (loc.buildStructure(building, tile, Game1.player, skipSafetyChecks: false))
            {
                built = building;
                return true;
            }
            return false;
        }
        if (loc.buildStructure(typeId, tile, Game1.player, out Building b, magicalConstruction: instant, skipSafetyChecks: false))
        {
            built = b;
            if (instant && b != null) b.daysOfConstructionLeft.Value = 0;
            return true;
        }
        return false;
    }

    private void BuildBuildingAt(string _, string[] args)
    {
        if (!RequireWorldReady() || !RequireHost()) return;
        if (args.Length < 4)
        {
            Mon.Log("用法: mh_buildat <玩家ID> <偏移x> <偏移y> <建筑ID> [wait]", LogLevel.Info);
            Mon.Log("说明: 以玩家当前所在瓦片为基准并偏移 (x,y) 放置建筑左上角", LogLevel.Info);
            Mon.Log("      坐标系统: (0,0)=地图左上角 | X 向右为正 | Y 向下为正", LogLevel.Info);
            Mon.Log("      玩家ID 可填: 数字ID | ~ (眷者) | admin (主机)", LogLevel.Info);
            Mon.Log("示例:", LogLevel.Info);
            Mon.Log("  mh_buildat 1145140 0 1 Mill       建磨坊（即时）", LogLevel.Info);
            Mon.Log("  mh_buildat 1145140 0 1 Silo wait  建筒仓（不即时）", LogLevel.Info);
            Mon.Log("  mh_buildat ~ 1 2 \"Shipping Bin\"  以眷者为基准偏移(1,2)建出货箱", LogLevel.Info);
            return;
        }

        long pid = ResolvePlayerId(args[0]);
        if (pid == 0) return;
        Farmer farmer = GetOnlinePlayer(pid);
        if (farmer == null) return;

        if (!int.TryParse(args[1], out int offX) || !int.TryParse(args[2], out int offY))
        {
            Mon.Log("[错误] 偏移 x/y 必须是整数", LogLevel.Warn);
            return;
        }

        string typeId = args[3];
        bool instant = !(args.Length >= 5 && args[4].Equals("wait", StringComparison.OrdinalIgnoreCase));

        GameLocation loc = farmer.currentLocation;
        if (loc == null)
        {
            Mon.Log("[错误] 玩家所在地点无效", LogLevel.Warn);
            return;
        }
        if (!loc.IsBuildableLocation())
        {
            Mon.Log($"[错误] 玩家当前地点 '{loc.NameOrUniqueName}' 不允许建造建筑；请先到允许建造的地点再执行", LogLevel.Warn);
            return;
        }

        // 允许风格小屋作别名：Stone Cabin / Log Cabin / ... 映射为 Cabin + skinId
        string forceSkinId = null;
        string[] cabinStyles = { "Stone Cabin", "Log Cabin", "Plank Cabin", "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin" };
        if (cabinStyles.Contains(typeId, StringComparer.OrdinalIgnoreCase))
        {
            forceSkinId = typeId;
            typeId = "Cabin";
        }

        if (!Building.TryGetData(typeId, out BuildingData data) || data == null)
        {
            Mon.Log($"[错误] 找不到建筑 '{typeId}'，使用 mh_buildings 查询可用建筑", LogLevel.Warn);
            return;
        }

        // 黑名单：唯一建筑禁止
        string[] forbiddenIds = { "Farmhouse", "Greenhouse" };
        if (forbiddenIds.Contains(typeId, StringComparer.OrdinalIgnoreCase))
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 为场地原生唯一建筑，禁止建造", LogLevel.Warn);
            return;
        }
        // Cabin 必须在 Farm
        if (typeId.Equals("Cabin", StringComparison.OrdinalIgnoreCase) && !loc.NameOrUniqueName.Equals("Farm", StringComparison.OrdinalIgnoreCase))
        {
            Mon.Log("[错误] Cabin 只能建造在 Farm；请玩家先到农场再使用此命令", LogLevel.Warn);
            return;
        }

        // BuildCondition + 升级前置校验
        if (!string.IsNullOrEmpty(data.BuildCondition)
            && !GameStateQuery.CheckConditions(data.BuildCondition, loc, Game1.player, null, null, null, null))
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 不满足建造条件", LogLevel.Warn);
            return;
        }
        if (!string.IsNullOrEmpty(data.BuildingToUpgrade)
            && loc.getNumberBuildingsConstructed(data.BuildingToUpgrade, false) == 0)
        {
            Mon.Log($"[错误] 建筑 '{typeId}' 为升级建筑 & 需先建造 {data.BuildingToUpgrade}", LogLevel.Warn);
            return;
        }

        // Cabin 自动分配风格
        bool isCabin = typeId.Equals("Cabin", StringComparison.OrdinalIgnoreCase);
        if (isCabin && forceSkinId == null)
        {
            string[] defaultOrder = { "Stone Cabin", "Log Cabin", "Plank Cabin", "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin" };
            int cabinCount = 0;
            foreach (Building b in loc.buildings)
                if (b.buildingType.Value == "Cabin") cabinCount++;
            forceSkinId = defaultOrder[cabinCount % defaultOrder.Length];
        }

        int tileX = (int)farmer.Tile.X + offX;
        int tileY = (int)farmer.Tile.Y + offY;
        Vector2 tile = new Vector2(tileX, tileY);
        int w = Math.Max(1, data.Size.X);
        int h = Math.Max(1, data.Size.Y);
        string displayName = TokenParser.ParseText(data.Name, null, null, null) ?? typeId;

        // 范围 + 占地 + 门 校验
        if (!IsWithinBuildableRect(loc, tile, w, h))
        {
            Rectangle rect = loc.GetBuildableRectangle();
            Mon.Log($"[错误] 偏移后位置 ({tileX},{tileY}) 超出可建造区域 {rect} (建筑 {w}x{h})", LogLevel.Warn);
            return;
        }
        // 防卡死优先提示：建筑主占地内有玩家则拒绝
        if (WouldTrapPlayer(loc, tile, w, h))
        {
            string trapNames = string.Join(", ", Game1.getOnlineFarmers()
                .Where(f => f != null && f.currentLocation == loc
                    && (int)f.Tile.X >= tileX && (int)f.Tile.X < tileX + w
                    && (int)f.Tile.Y >= tileY && (int)f.Tile.Y < tileY + h)
                .Select(f => f.Name));
            Mon.Log($"[错误] 位置 ({tileX},{tileY}) 内有玩家 [{trapNames}] 建造会卡死玩家；请调整偏移或让玩家移开", LogLevel.Warn);
            return;
        }
        if (!CanPlaceBuilding(loc, data, tile))
        {
            Mon.Log($"[错误] 位置 ({tileX},{tileY}) 无法放置 {displayName} (有障碍物/地形不可建/门下方不可通行)", LogLevel.Warn);
            return;
        }

        // 执行建造
        Building built = null;
        bool ok = TryPlace(loc, typeId, data, forceSkinId, instant, tile, out built, out Vector2 _);
        if (!ok)
        {
            Mon.Log($"[错误] 放置失败：{displayName} @ ({tileX},{tileY})", LogLevel.Warn);
            return;
        }

        Mon.Log($"[精准建造] {farmer.Name} [{farmer.UniqueMultiplayerID}] 瓦片({(int)farmer.Tile.X},{(int)farmer.Tile.Y}) "
            + $"偏移 ({offX},{offY}) → ({tileX},{tileY}) 放置 {displayName}"
            + (forceSkinId != null ? $" [{forceSkinId}]" : "")
            + (instant ? " (即时)" : " (工期中)"), LogLevel.Info);
    }

    private static Vector2 GetBuildableCenter(GameLocation loc, BuildingData data)
    {
        Rectangle rect = loc.GetBuildableRectangle();
        if (rect != Rectangle.Empty && rect.Width > 0 && rect.Height > 0)
        {
            int w = Math.Max(1, data.Size.X);
            int h = Math.Max(1, data.Size.Y);
            int cx = rect.X + (rect.Width - w) / 2;
            int cy = rect.Y + (rect.Height - h) / 2;
            return new Vector2(cx, cy);
        }
        return new Vector2(10, 10);
    }

    private static bool IsWithinBuildableRect(GameLocation loc, Vector2 tile, int w, int h)
    {
        Rectangle rect = loc.GetBuildableRectangle();
        if (rect == Rectangle.Empty) return true; // 无限制
        return tile.X >= rect.X
            && tile.Y >= rect.Y
            && tile.X + w <= rect.X + rect.Width
            && tile.Y + h <= rect.Y + rect.Height;
    }

    private static bool CanPlaceBuilding(GameLocation loc, BuildingData data, Vector2 tile)
    {
        int w = Math.Max(1, data.Size.X);
        int h = Math.Max(1, data.Size.Y);

        // 主占地
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Vector2 t = new Vector2(tile.X + x, tile.Y + y);
                if (!loc.isBuildable(t, false)) return false;
            }

        // 额外占地
        if (data.AdditionalPlacementTiles != null)
        {
            foreach (BuildingPlacementTile pt in data.AdditionalPlacementTiles)
            {
                foreach (Point p in pt.TileArea.GetPoints())
                {
                    Vector2 t = new Vector2(tile.X + p.X, tile.Y + p.Y);
                    if (!loc.isBuildable(t, pt.OnlyNeedsToBePassable)) return false;
                }
            }
        }

        // 人类门下方需可通行
        if (data.HumanDoor != new Point(-1, -1))
        {
            Vector2 doorPos = tile + new Vector2(data.HumanDoor.X, data.HumanDoor.Y + 1);
            if (!loc.isBuildable(doorPos, true) && !loc.isPath(doorPos)) return false;
        }

        // 防卡死：建筑主占地内不得有任何在线玩家
        if (WouldTrapPlayer(loc, tile, w, h)) return false;

        return true;
    }
    
    private static bool WouldTrapPlayer(GameLocation loc, Vector2 tile, int w, int h)
    {
        var farmers = Game1.getOnlineFarmers();
        if (farmers == null) return false;
        foreach (var f in farmers)
        {
            if (f == null) continue;
            if (f.currentLocation != loc) continue;
            int px = (int)f.Tile.X;
            int py = (int)f.Tile.Y;
            if (px >= tile.X && px < tile.X + w && py >= tile.Y && py < tile.Y + h)
                return true;
        }
        return false;
    }

    private static IEnumerable<Vector2> EnumerateSpiralTiles(Vector2 center, int maxRadius)
    {
        yield return center;
        for (int r = 1; r <= maxRadius; r++)
        {
            // 上边: 从 (-r, -r) 到 (r-1, -r)
            for (int x = -r; x < r; x++)
                yield return new Vector2(center.X + x, center.Y - r);
            // 右边: 从 (r, -r) 到 (r, r-1)
            for (int y = -r; y < r; y++)
                yield return new Vector2(center.X + r, center.Y + y);
            // 下边: 从 (r, r) 到 (-r+1, r)
            for (int x = r; x > -r; x--)
                yield return new Vector2(center.X + x, center.Y + r);
            // 左边: 从 (-r, r) 到 (-r, -r+1)
            for (int y = r; y > -r; y--)
                yield return new Vector2(center.X - r, center.Y + y);
        }
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
                    bool online = Game1.GetPlayer(id, true) != null;
                    Mon.Log($"  {PlayerName(id) ?? $"ID:{id}"} [ID: {id}]{(online ? "" : " (离线)")}", LogLevel.Info);
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
                    Mon.Log($"[无限送礼] 已添加 {PlayerName(id) ?? $"ID:{id}"} [ID: {id}]", LogLevel.Info);
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
                        Mon.Log($"[无限送礼] 已移除 {PlayerName(id) ?? $"ID:{id}"} [ID: {id}]", LogLevel.Info);
                    }
                    else Mon.Log($"[无限送礼] {PlayerName(id) ?? $"ID:{id}"} [ID: {id}] 不在白名单", LogLevel.Warn);
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