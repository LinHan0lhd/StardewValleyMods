using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Netcode;

namespace MasterHand
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.ConsoleCommands.Add("mh_give", "给玩家物品 > 用法: mh_give <玩家ID> <物品ID> [数量] [品质]", GiveItem);
            helper.ConsoleCommands.Add("mh_money", "修改玩家金钱 > 用法: mh_money <玩家ID> <金额>", SetMoney);
            helper.ConsoleCommands.Add("mh_drop", "私有掉落物品 > 用法: mh_drop <玩家ID> <物品ID> [数量] [品质]", PrivateDrop);
            helper.ConsoleCommands.Add("mh_heal", "治疗玩家 > 用法: mh_heal <玩家ID>", HealPlayer);
            helper.ConsoleCommands.Add("mh_tp", "传送玩家 > 用法: mh_tp <玩家ID> <目标玩家ID 或 地图名 X Y>", TeleportPlayer);
            Monitor.Log("MasterHand 已加载", LogLevel.Info);
        }

        private Farmer? GetOnlinePlayer(long playerId, bool logError = true)
        {
            Farmer? farmer = Game1.GetPlayer(playerId, true);
            if (farmer == null && logError)
                Monitor.Log($"玩家 ID {playerId} 不在线或不存在。", LogLevel.Warn);
            return farmer;
        }

        // ---------- 命令实现 ----------

        private void GiveItem(string command, string[] args)
        {
            if (args.Length < 2 || !long.TryParse(args[0], out long playerId))
            { Monitor.Log("用法: mh_give <玩家ID> <物品ID> [数量] [品质]", LogLevel.Info); return; }
            Farmer? farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            string itemId = args[1];
            int amount = args.Length >= 3 && int.TryParse(args[2], out int a) ? Math.Max(1, a) : 1;
            int quality = args.Length >= 4 && int.TryParse(args[3], out int q) ? Math.Clamp(q, 0, 4) : 0;

            Item? item = ItemRegistry.Create(itemId, amount, quality);
            if (item == null) { Monitor.Log($"物品 ID '{itemId}' 无效。", LogLevel.Warn); return; }

            if (farmer.addItemToInventoryBool(item))
                Monitor.Log($"已给 {farmer.Name} 添加 {item.DisplayName} x{amount} (品质 {quality})。", LogLevel.Info);
            else
            {
                Game1.createItemDebris(item, farmer.getStandingPosition(), farmer.FacingDirection, farmer.currentLocation, (int)farmer.UniqueMultiplayerID);
                Monitor.Log($"{farmer.Name} 背包已满，物品 {item.DisplayName} 已掉落为私有物品。", LogLevel.Info);
            }
        }

        private void SetMoney(string command, string[] args)
        {
            if (args.Length < 2 || !long.TryParse(args[0], out long playerId) || !int.TryParse(args[1], out int amount))
            { Monitor.Log("用法: mh_money <玩家ID> <金额>", LogLevel.Info); return; }
            Farmer? farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            if (Game1.player.team.useSeparateWallets.Value)
            {
                farmer.Money = Math.Max(0, amount);
                Monitor.Log($"已将玩家 {farmer.Name} 的独立钱包设置为 {amount} 金。", LogLevel.Info);
            }
            else
            {
                Game1.player.Money = Math.Max(0, amount);
                Monitor.Log($"已将团队共享金钱设置为 {amount} 金。", LogLevel.Info);
            }
        }

        private void PrivateDrop(string command, string[] args)
        {
            if (args.Length < 2 || !long.TryParse(args[0], out long playerId))
            { Monitor.Log("用法: mh_drop <玩家ID> <物品ID> [数量] [品质]", LogLevel.Info); return; }
            Farmer? farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            string itemId = args[1];
            int amount = args.Length >= 3 && int.TryParse(args[2], out int amt) ? Math.Max(1, amt) : 1;
            int quality = args.Length >= 4 && int.TryParse(args[3], out int qua) ? Math.Clamp(qua, 0, 4) : 0;

            Item? item = ItemRegistry.Create(itemId, amount, quality);
            if (item == null) { Monitor.Log($"物品 ID '{itemId}' 无效。", LogLevel.Warn); return; }

            Game1.createItemDebris(item, farmer.getStandingPosition(), farmer.FacingDirection, farmer.currentLocation, (int)farmer.UniqueMultiplayerID);
            Monitor.Log($"已在 {farmer.Name} 脚下生成私有物品：{item.DisplayName} x{amount}。", LogLevel.Info);
        }

        private void HealPlayer(string command, string[] args)
        {
            if (args.Length < 1 || !long.TryParse(args[0], out long playerId))
            {
                Monitor.Log("用法: mh_heal <玩家ID>", LogLevel.Info);
                return;
            }
            Farmer? farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            // 记录旧值，用于后续生成同步消息
            int oldHealth = farmer.health;
            float oldStamina = farmer.Stamina;

            // 直接设置满血满体力（与吃食物的逻辑一致）
            farmer.health = farmer.maxHealth;
            farmer.Stamina = farmer.MaxStamina;

            // 模拟食物恢复后同步消息的发送，让客机能够收到属性更新
            // 这段逻辑直接取自 doneEating 源码，确保网络同步生效
            if (oldHealth < farmer.health)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", farmer.health - oldHealth), 5));
            }
            if (oldStamina < farmer.Stamina)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(farmer.Stamina - oldStamina)), 4));
            }

            Monitor.Log($"已治疗 {farmer.Name}（满血满体力，已同步）。", LogLevel.Info);
        }

        private void TeleportPlayer(string command, string[] args)
        {
            if (args.Length < 2 || !long.TryParse(args[0], out long playerId))
            { Monitor.Log("用法: mh_tp <玩家ID> <目标玩家ID 或 地图名 X Y>", LogLevel.Info); return; }
            Farmer? farmer = GetOnlinePlayer(playerId);
            if (farmer == null) return;

            if (long.TryParse(args[1], out long targetPlayerId))
            {
                Farmer? target = GetOnlinePlayer(targetPlayerId);
                if (target == null) return;
                Game1.warpFarmer(target.currentLocation.NameOrUniqueName, target.TilePoint.X, target.TilePoint.Y, false);
                Monitor.Log($"已将 {farmer.Name} 传送到 {target.Name} 身边。", LogLevel.Info);
            }
            else if (args.Length >= 4 && int.TryParse(args[2], out int x) && int.TryParse(args[3], out int y))
            {
                string locationName = args[1];
                if (Game1.getLocationFromName(locationName) == null)
                { Monitor.Log($"地图 '{locationName}' 不存在。", LogLevel.Warn); return; }
                Game1.warpFarmer(locationName, x, y, false);
                Monitor.Log($"已将 {farmer.Name} 传送到 {locationName} ({x}, {y})。", LogLevel.Info);
            }
            else
            { Monitor.Log("传送参数无效。用法: mh_tp <玩家ID> <目标玩家ID 或 地图名 X Y>", LogLevel.Warn); }
        }
    }
}