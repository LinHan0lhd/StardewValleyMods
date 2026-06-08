using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;

        public override void Entry(IModHelper helper)
        {
            // 读取配置文件
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // 注册控制台命令
            helper.ConsoleCommands.Add("asp_mark", "标记一个玩家获得无限送礼权\n用法: asp_mark <玩家名字>", this.MarkPlayer);
            helper.ConsoleCommands.Add("asp_unmark", "移除玩家的无限送礼标记\n用法: asp_unmark <玩家名字或ID>", this.UnmarkPlayer);
            helper.ConsoleCommands.Add("asp_list", "列出所有已标记的玩家", this.ListMarkedPlayers);

            // 事件：加载存档后 & 每天开始时 → 补全NPC + 重置送礼次数
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            this.Monitor.Log("InfiniteGifts 已就绪", LogLevel.Info);
        }

        /// <summary>保存配置到文件</summary>
        private void SaveConfig()
        {
            this.Helper.WriteConfig(this.Config);
        }

        // ==================== 控制台指令 ====================

        private void MarkPlayer(string command, string[] args)
        {
            if (args.Length == 0)
            {
                this.Monitor.Log("请输入玩家名字例如: asp_mark 刘易斯", LogLevel.Warn);
                return;
            }

            string targetName = string.Join(" ", args).Trim();
            Farmer? player = Game1.getOnlineFarmers()
                .FirstOrDefault(f => f.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (player == null)
            {
                this.Monitor.Log($"未找到名为 '{targetName}' 的在线玩家", LogLevel.Warn);
                return;
            }

            long id = player.UniqueMultiplayerID;
            if (this.Config.MarkedPlayerIDs.Contains(id))
            {
                this.Monitor.Log($"玩家 {player.Name} (ID: {id}) 已经在无限送礼列表中", LogLevel.Info);
                return;
            }

            // 加入标记列表并保存
            this.Config.MarkedPlayerIDs = this.Config.MarkedPlayerIDs.Append(id).ToArray();
            this.SaveConfig();
            this.Monitor.Log($"已将玩家 {player.Name} (ID: {id}) 加入无限送礼列表", LogLevel.Info);

            // ★ 立即应用无限送礼效果（仅限主机且游戏已加载）
            if (Context.IsMainPlayer && Context.IsWorldReady)
            {
                this.ApplyInfiniteGiftsToPlayer(player);
                this.Monitor.Log($"已立即为 {player.Name} 应用无限送礼", LogLevel.Info);
            }
        }

        private void UnmarkPlayer(string command, string[] args)
        {
            if (args.Length == 0)
            {
                this.Monitor.Log("请提供玩家名字或ID例如: asp_unmark 刘易斯 或 asp_unmark 123456789", LogLevel.Warn);
                return;
            }

            string input = string.Join(" ", args).Trim();
            long targetId;

            // 尝试解析为ID，否则按名字查找
            if (long.TryParse(input, out targetId))
            {
                if (!this.Config.MarkedPlayerIDs.Contains(targetId))
                {
                    this.Monitor.Log($"ID {targetId} 不在无限送礼列表中", LogLevel.Info);
                    return;
                }

                this.Config.MarkedPlayerIDs = this.Config.MarkedPlayerIDs.Where(id => id != targetId).ToArray();
                this.SaveConfig();
                this.Monitor.Log($"已移除 ID {targetId} 的无限送礼标记", LogLevel.Info);
            }
            else
            {
                // 按名字查找在线玩家
                Farmer? player = Game1.getOnlineFarmers()
                    .FirstOrDefault(f => f.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    this.Monitor.Log($"未找到名为 '{input}' 的在线玩家，无法移除请使用ID", LogLevel.Warn);
                    return;
                }

                long id = player.UniqueMultiplayerID;
                if (!this.Config.MarkedPlayerIDs.Contains(id))
                {
                    this.Monitor.Log($"玩家 {player.Name} (ID: {id}) 不在无限送礼列表中", LogLevel.Info);
                    return;
                }

                this.Config.MarkedPlayerIDs = this.Config.MarkedPlayerIDs.Where(pid => pid != id).ToArray();
                this.SaveConfig();
                this.Monitor.Log($"已移除玩家 {player.Name} (ID: {id}) 的无限送礼标记", LogLevel.Info);
            }
        }

        private void ListMarkedPlayers(string command, string[] args)
        {
            if (this.Config.MarkedPlayerIDs.Length == 0)
            {
                this.Monitor.Log("当前没有标记任何玩家", LogLevel.Info);
                return;
            }

            this.Monitor.Log("已标记的无限送礼玩家：", LogLevel.Info);
            foreach (long id in this.Config.MarkedPlayerIDs)
            {
                Farmer? farmer = Game1.GetPlayer(id);
                string name = farmer?.Name ?? "（离线或未知）";
                this.Monitor.Log($"  - {name} (ID: {id})", LogLevel.Info);
            }
        }

        // ==================== 事件处理 ====================

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.ApplyInfiniteGifts();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            this.ApplyInfiniteGifts();
        }

        // ==================== 核心逻辑 ====================

        /// <summary>对所有标记的玩家执行补全NPC和重置送礼次数</summary>
        private void ApplyInfiniteGifts()
        {
            if (!Context.IsMainPlayer)
                return; // 只在主机端执行

            foreach (long playerId in this.Config.MarkedPlayerIDs)
            {
                Farmer? farmer = Game1.GetPlayer(playerId);
                if (farmer != null)
                    this.ApplyInfiniteGiftsToPlayer(farmer);
            }

            this.Monitor.Log($"已为 {this.Config.MarkedPlayerIDs.Length} 名标记玩家补全NPC并重置无限送礼次数", LogLevel.Trace);
        }

        /// <summary>针对单个玩家：补全所有NPC的友谊数据，并将送礼次数设为 -999</summary>
        private void ApplyInfiniteGiftsToPlayer(Farmer farmer)
        {
            // 1. 补全所有NPC的友谊数据（如果缺失）
            foreach (string npcName in Game1.NPCGiftTastes.Keys)
            {
                if (!farmer.friendshipData.ContainsKey(npcName))
                {
                    farmer.friendshipData.Add(npcName, new Friendship());
                }
            }

            // 2. 将所有NPC的每日/每周送礼次数设为 -999
            foreach (var pair in farmer.friendshipData.Pairs)
            {
                pair.Value.GiftsToday = -999;
                pair.Value.GiftsThisWeek = -999;
            }
        }
    }
}