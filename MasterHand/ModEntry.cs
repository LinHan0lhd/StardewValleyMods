#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;

namespace MasterHand
{
    public class ModConfig
    {
        public long FavoredPlayerId { get; set; } = 0;
    }

    public class ModEntry : Mod
    {
        internal static IMonitor _monitor;
        private static IModHelper _helper;
        private const string ItemPoolFolder = "ItemPool";
        private static readonly XmlSerializer ItemSerializer;
        private bool isTimeFrozen;
        private static long _favoredPlayerId = 0;
        private static ModConfig Config;

        static ModEntry()
        {
            var itemTypes = Assembly.GetAssembly(typeof(Item))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract)
                .ToArray();
            ItemSerializer = new XmlSerializer(typeof(Item), itemTypes);
        }

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;

            // ---------- 加载配置 ----------
            Config = _helper.Data.ReadJsonFile<ModConfig>("config.json") ?? new ModConfig();
            _favoredPlayerId = Config.FavoredPlayerId;

            // ---------- 注册命令 ----------
            helper.ConsoleCommands.Add("mh_list", "列出所有在线玩家", (_, _) => ListPlayers());
            helper.ConsoleCommands.Add("mh_give", "赠送物品 > mh_give <玩家ID> <物品ID> [数量] [品质]", GiveItem);
            helper.ConsoleCommands.Add("mh_poolitem", "赠送 poolitem 物品 > mh_poolitem list / <玩家ID> <物品名称> [数量] [品质]", GivePoolItem);
            helper.ConsoleCommands.Add("mh_money", "修改主机金钱 > mh_money <金额>", SetMoney);
            helper.ConsoleCommands.Add("mh_time", "设置时间 > mh_time <600-2600>", SetTime);
            helper.ConsoleCommands.Add("mh_pause", "暂停/继续时间", (_, _) => TogglePause());
            helper.ConsoleCommands.Add("mh_weather", "设置明天天气 > mh_weather <0-5> [地点/all]", SetWeather);
            helper.ConsoleCommands.Add("mh_season", "设置季节 > mh_season <0-3>", SetSeason);
            helper.ConsoleCommands.Add("mh_day", "设置日期 > mh_day <1-28>", SetDay);
            helper.ConsoleCommands.Add("mh_year", "设置年份 > mh_year <年份>", SetYear);
            helper.ConsoleCommands.Add("mh_kick", "踢出玩家 > mh_kick <玩家ID>", KickPlayer);
            helper.ConsoleCommands.Add("mh_favored", "设置/清除眷者 > mh_favored <玩家ID> | clear | show", SetFavoredPlayer);

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            LoadItemPool();
        }

        // ========== 配置保存 ==========
        private static void SaveConfig()
        {
            _helper.Data.WriteJsonFile("config.json", Config);
        }

        // ========== 核心工具方法 ==========

        private static bool RequireWorldReady()
        {
            if (!Context.IsWorldReady)
                _monitor.Log("[警告] 尚未载入存档因此无法执行命令", LogLevel.Warn);
            return Context.IsWorldReady;
        }

        private static bool RequireHost()
        {
            if (!Context.IsMainPlayer)
                _monitor.Log("[提示] 仅限主机可执行此命令", LogLevel.Warn);
            return Context.IsMainPlayer;
        }

        private static Farmer GetOnlinePlayer(long playerId, bool logError = true)
        {
            var farmer = Game1.GetPlayer(playerId, true);
            if (farmer == null && logError)
                _monitor.Log($"[警告] 玩家 ID {playerId} 不在线或不存在", LogLevel.Warn);
            return farmer;
        }

        private static bool TryParseArg(string[] args, int index, out int value, int min = int.MinValue, int max = int.MaxValue, int fallback = 0)
        {
            value = fallback;
            if (args.Length > index && int.TryParse(args[index], out int parsed))
            {
                value = Math.Clamp(parsed, min, max);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 解析玩家 ID：支持 ~ 代指眷者
        /// </summary>
        private static long ResolvePlayerId(string arg, bool logError = true)
        {
            if (arg == "~")
            {
                if (_favoredPlayerId == 0)
                {
                    if (logError) _monitor.Log("[错误] 请先使用 mh_favored 设置眷者", LogLevel.Warn);
                    return 0;
                }
                return _favoredPlayerId;
            }
            if (long.TryParse(arg, out long id) && id > 0)
                return id;
            if (logError) _monitor.Log($"[错误] 无效的玩家ID: {arg}", LogLevel.Warn);
            return 0;
        }

        // ========== 眷者管理 ==========

        private static void SetFavoredPlayer(string _, string[] args)
        {
            if (args.Length == 0)
            {
                _monitor.Log("描述：可用 ~ 代指眷者", LogLevel.Info);
                _monitor.Log("用法: mh_favored <玩家ID> | clear | show", LogLevel.Info);
                return;
            }

            string cmd = args[0].ToLower();
            if (cmd == "clear")
            {
                _favoredPlayerId = 0;
                Config.FavoredPlayerId = 0;
                SaveConfig();
                _monitor.Log("[眷者] 已清除", LogLevel.Info);
                return;
            }
            if (cmd == "show")
            {
                if (_favoredPlayerId == 0)
                {
                    _monitor.Log("[眷者] 未设置", LogLevel.Info);
                    return;
                }
                var farmer = Game1.GetPlayer(_favoredPlayerId, true);
                if (farmer != null)
                    _monitor.Log($"[眷者] {farmer.Name} [ID: {_favoredPlayerId}]", LogLevel.Info);
                else
                    _monitor.Log($"[眷者] 已设置 [ID: {_favoredPlayerId}] 暂时不在线或不存在", LogLevel.Warn);
                return;
            }

            if (long.TryParse(cmd, out long id) && id > 0)
            {
                var farmer = Game1.GetPlayer(id, true);
                if (farmer == null)
                {
                    _monitor.Log($"[警告] 已设置 [ID: {id}] 暂时不在线", LogLevel.Warn);
                }
                _favoredPlayerId = id;
                Config.FavoredPlayerId = id;
                SaveConfig();
                _monitor.Log($"[眷者] 已设置为 {farmer?.Name ?? "未知"} [ID: {id}]", LogLevel.Info);
            }
            else
            {
                _monitor.Log("[错误] 无效的玩家 ID 或 参数不正确", LogLevel.Warn);
            }
        }

        // ========== 物品池初始化 ==========

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

        // ========== 送礼管理器 ==========

        internal static class GiftProposalManager
        {
            public static bool Send(Farmer target, Item item)
            {
                if (target == null || item == null) return false;

                if (target.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
                {
                    bool added = Game1.player.addItemToInventoryBool(item, true);
                    _monitor?.Log(added
                        ? $"[成功] 已将 {item.DisplayName} x{item.Stack} 加入主机背包"
                        : $"[失败] 无法添加 {item.DisplayName} 检查背包再尝试",
                        added ? LogLevel.Info : LogLevel.Warn);
                    return added;
                }

                if (!target.isActive())
                {
                    _monitor?.Log($"[警告] 目标玩家 {target.Name} 当前离线", LogLevel.Warn);
                    return false;
                }

                Game1.player.team.SendProposal(target, ProposalType.Gift, item);

                _monitor?.Log($"[赠送] 已向 {target.Name} 发送 {item.DisplayName} x{item.Stack} 提议请求", LogLevel.Info);
                return true;
            }
        }

        // ========== 命令实现 ==========

        private static void ListPlayers()
        {
            var farmers = Game1.getOnlineFarmers();
            if (!farmers.Any()) return;

            foreach (var f in farmers)
                _monitor.Log($"{f.Name}  [ID: {f.UniqueMultiplayerID}]", LogLevel.Info);
        }

        private static void GiveItem(string _, string[] args)
        {
            if (!RequireWorldReady()) return;
            if (args.Length < 2)
            {
                _monitor.Log("用法: mh_give <玩家ID> <物品ID> [数量] [品质]", LogLevel.Info);
                return;
            }

            long playerId = ResolvePlayerId(args[0]);
            if (playerId == 0) return;

            var farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            string itemId = args[1];
            TryParseArg(args, 2, out int amount, min: 1, max: 999, fallback: 1);
            TryParseArg(args, 3, out int quality, min: 0, max: 4);

            var item = ItemRegistry.Create(itemId, amount, quality);
            if (item == null)
            {
                _monitor.Log($"[错误] 物品 ID '{itemId}' 无效", LogLevel.Warn);
                return;
            }
            GiftProposalManager.Send(farmer, item);
        }

        private void GivePoolItem(string _, string[] args)
        {
            if (!RequireWorldReady()) return;
            var poolDir = Path.Combine(Helper.DirectoryPath, ItemPoolFolder);

            if (args.Length == 0 || (args.Length == 1 && args[0].ToLower() != "list"))
            {
                _monitor.Log("用法: mh_poolitem <玩家ID> <物品名称> [数量] [品质]", LogLevel.Info);
                return;
            }

            long playerId = ResolvePlayerId(args[0]);
            if (playerId == 0) return;

            var farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            if (args.Length < 2)
            {
                _monitor.Log("[警告] 未指定有效的物品名称（不含 .xml）", LogLevel.Warn);
                return;
            }

            string itemName = args[1];
            string filePath = Path.Combine(poolDir, itemName + ".xml");
            if (!File.Exists(filePath))
            {
                _monitor.Log($"[警告] 物品 '{itemName}' 不存在于物品池", LogLevel.Warn);
                return;
            }

            TryParseArg(args, 2, out int qty, min: 1, fallback: 0);
            TryParseArg(args, 3, out int qua, min: 0, max: 4, fallback: -1);

            try
            {
                string xml = File.ReadAllText(filePath);
                // 清理命名空间并补全必要的命名空间声明
                xml = RemoveXsiNamespace(xml);
                if (xml.Contains("xsi:type"))
                {
                    int tagEnd = xml.IndexOf('>');
                    if (tagEnd > 0)
                        xml = xml.Insert(tagEnd, " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                }

                var doc = XDocument.Parse(xml);
                if (doc.Root == null)
                {
                    _monitor.Log($"[错误] 物品 '{itemName}' 的 XML 根元素无效", LogLevel.Warn);
                    return;
                }

                Item item;
                using (var sr = new StringReader(doc.Root.ToString()))
                {
                    item = ItemSerializer.Deserialize(sr) as Item;
                }

                if (item == null)
                {
                    _monitor.Log($"[错误] 无法从 '{itemName}' 解析出有效物品", LogLevel.Warn);
                    return;
                }

                // 应用可选覆盖
                if (item is StardewValley.Object obj)
                {
                    if (qty > 0) obj.Stack = qty;
                    if (qua >= 0) obj.Quality = qua;
                }

                GiftProposalManager.Send(farmer, item);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[内部错误] 处理物品 '{itemName}' 时异常: {ex.Message}", LogLevel.Error);
            }
        }

        private static string RemoveXsiNamespace(string xml)
            => Regex.Replace(xml, @"\s+xmlns:xsi\s*=\s*[""'][^""']*[""']", "");

        private void SetMoney(string _, string[] args)
        {
            if (!RequireWorldReady() || !RequireHost()) return;
            if (!TryParseArg(args, 0, out int amount, min: 0))
            {
                _monitor.Log("用法: mh_money <金额>", LogLevel.Info);
                return;
            }
            Game1.player.Money = amount;
            _monitor.Log($"[金钱] 主机金钱已更新为 {amount} 金", LogLevel.Info);
        }

        // ========== 时间控制 ==========

        private void SetTime(string _, string[] args)
        {
            if (!RequireWorldReady() || !RequireHost()) return;
            if (!TryParseArg(args, 0, out int time, min: 600, max: 2600, fallback: -1))
            {
                _monitor.Log("用法: mh_time <600-2600>  (例如 800 代表 08:00)", LogLevel.Info);
                return;
            }
            ApplyTimeChange(time);
            _monitor.Log($"[时间] 已调整 {Game1.timeOfDay / 100:D2}:{Game1.timeOfDay % 100:D2} 作为游戏时间", LogLevel.Info);
        }

        private static void ApplyTimeChange(int targetTime)
        {
            int intervals = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, targetTime) / 10;
            if (intervals > 0)
            {
                for (int i = 0; i < intervals; i++)
                    Game1.performTenMinuteClockUpdate();
            }
            else if (intervals < 0)
            {
                for (int i = 0; i > intervals; i--)
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
            _monitor.Log(isTimeFrozen ? "[暂停] 时间已冻结" : "[继续] 时间已恢复流动", LogLevel.Info);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && isTimeFrozen)
                Game1.gameTimeInterval = 0;
        }

        // ========== 天气设置 ==========

        private void SetWeather(string _, string[] args)
        {
            if (!RequireWorldReady() || Game1.netWorldState?.Value == null)
            {
                _monitor.Log("[错误] 网络世界状态未就绪", LogLevel.Error);
                return;
            }
            if (!RequireHost()) return;

            if (!TryParseArg(args, 0, out int weatherId, min: 0, max: 5, fallback: -1))
            {
                _monitor.Log("用法: mh_weather <天气代码> [地点]", LogLevel.Info);
                _monitor.Log("代码：晴天[0] 刮风[1] 降雨[2] 雷雨[3] 落雪[4] 苔雨[5]", LogLevel.Info);
                _monitor.Log("地点: default / island / desert / all", LogLevel.Info);
                return;
            }

            // 解析目标区域
            List<string> targets = args.Length >= 2 && args[1].ToLowerInvariant() == "all"
                ? new() { "Default", "Island", "Desert" }
                : new() { ResolveLocationKey(args.ElementAtOrDefault(1)) };

            string weatherStr = weatherId switch
            {
                1 => "Wind", 2 => "Rain", 3 => "Storm", 4 => "Snow", 5 => "GreenRain", _ => "Sun"
            };
            string weatherCn = weatherId switch
            {
                1 => "刮风", 2 => "雨天", 3 => "雷雨", 4 => "降雪", 5 => "苔雨", _ => "晴天"
            };

            try
            {
                foreach (var key in targets)
                {
                    var weather = Game1.netWorldState.Value.GetWeatherForLocation(key);
                    if (weather == null)
                    {
                        _monitor.Log($"[警告] 无法获取地点 {key} 的天气数据", LogLevel.Warn);
                        continue;
                    }
                    weather.WeatherForTomorrow = weatherStr;
                    if (key == "Default") Game1.weatherForTomorrow = weatherStr;
                    _monitor.Log($"[天气] 已设置 {key} 次日天气为 {weatherCn} ({weatherStr})", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"[异常] 设置天气失败: {ex.Message}", LogLevel.Error);
            }
        }

        private static string ResolveLocationKey(string raw)
        {
            return raw?.ToLowerInvariant() switch
            {
                "island" or "ginger" => "Island",
                "desert" => "Desert",
                "default" or "main" => "Default",
                _ => "Default"
            };
        }

        // ========== 日期/季节/年份 ==========

        private void SetSeason(string _, string[] args)
        {
            if (!RequireWorldReady() || !RequireHost()) return;
            if (!TryParseArg(args, 0, out int season, min: 0, max: 3, fallback: -1))
            {
                _monitor.Log("用法: mh_season <0-3>", LogLevel.Info);
                return;
            }
            string seasonStr = season switch { 0 => "spring", 1 => "summer", 2 => "fall", 3 => "winter", _ => "spring" };
            ApplyDate(Game1.dayOfMonth, seasonStr, Game1.year);
            _monitor.Log($"[季节] 已切换 {GetSeasonCnName(season)} 作为主旋律", LogLevel.Info);
        }

        private void SetDay(string _, string[] args)
        {
            if (!RequireWorldReady() || !RequireHost()) return;
            if (!TryParseArg(args, 0, out int day, min: 1, max: 28, fallback: -1))
            {
                _monitor.Log("用法: mh_day <1-28>", LogLevel.Info);
                return;
            }
            ApplyDate(day, Game1.currentSeason, Game1.year);
            _monitor.Log($"[日期] 已设为第 {day} 天", LogLevel.Info);
        }

        private void SetYear(string _, string[] args)
        {
            if (!RequireWorldReady() || !RequireHost()) return;
            if (!TryParseArg(args, 0, out int year, min: 1, fallback: -1))
            {
                _monitor.Log("用法: mh_year <年份>", LogLevel.Info);
                return;
            }
            ApplyDate(Game1.dayOfMonth, Game1.currentSeason, year);
            _monitor.Log($"[年份] 已设为第 {year} 年", LogLevel.Info);
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
            0 => "春季", 1 => "夏季", 2 => "秋季", 3 => "冬季", _ => "春季"
        };

        // ========== 玩家管理 ==========

        private void KickPlayer(string _, string[] args)
        {
            if (!RequireHost()) return;
            if (args.Length < 1 || !long.TryParse(args[0], out long playerId))
            {
                _monitor.Log("用法: mh_kick <玩家ID>", LogLevel.Info);
                return;
            }
            if (Game1.server == null)
            {
                _monitor.Log("[错误] 服务器未运行", LogLevel.Warn);
                return;
            }
            Game1.server.kick(playerId);
            _monitor.Log($"[踢出] 已踢出玩家 [ID: {playerId}]", LogLevel.Info);
        }
    }
}