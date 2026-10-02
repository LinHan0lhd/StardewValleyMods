#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace FarmhouseMoveIn;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; }

    private MoveInManager _moveInManager;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        _moveInManager = new MoveInManager(Monitor);

        helper.ConsoleCommands.Add("movein",
            "让指定联机玩家搬进主屋: movein <玩家ID|名字>  |  movein list",
            (_, args) => OnMoveInCommand(args));
    }

    private void OnMoveInCommand(string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Monitor.Log("世界未加载，无法执行", LogLevel.Warn);
            return;
        }

        if (args.Length < 1)
        {
            Monitor.Log("用法: movein <玩家ID|名字>  |  movein list", LogLevel.Info);
            return;
        }

        if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            ListPlayers();
            return;
        }

        Farmer who = ResolvePlayer(args[0]);
        if (who == null)
        {
            Monitor.Log($"找不到玩家: {args[0]}", LogLevel.Warn);
            return;
        }

        if (who.IsMainPlayer)
        {
            Monitor.Log($"{who.Name} 是主机，已住主屋", LogLevel.Info);
            return;
        }

        bool success = _moveInManager.MoveIn(who);
        if (success)
            Monitor.Log($"{who.Name} (ID: {who.UniqueMultiplayerID}) 已搬进主屋", LogLevel.Info);
        else
            Monitor.Log($"{who.Name} 搬家失败，请查看日志", LogLevel.Warn);
    }

    /// <summary>列出所有玩家（排除主机），标注在线状态和住处</summary>
    private void ListPlayers()
    {
        long hostId = Game1.player?.UniqueMultiplayerID ?? long.MinValue;
        var all = Game1.getAllFarmers()?
            .Where(f => f != null && f.UniqueMultiplayerID != hostId)
            .ToList();

        if (all == null || all.Count == 0)
        {
            Monitor.Log("不存在其他玩家", LogLevel.Info);
            return;
        }

        var onlineIds = new HashSet<long>(Game1.getOnlineFarmers()
            .Where(f => f != null)
            .Select(f => f.UniqueMultiplayerID));

        Monitor.Log($"共 {all.Count} 名玩家（在线 {all.Count(f => onlineIds.Contains(f.UniqueMultiplayerID))}）", LogLevel.Info);
        foreach (var f in all)
        {
            bool online = onlineIds.Contains(f.UniqueMultiplayerID);
            string home = string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase)
                ? "主屋" : f.homeLocation.Value;
            Monitor.Log($"  {f.Name} [ID: {f.UniqueMultiplayerID}] 住处:{home}{(online ? "" : " (离线)")}", LogLevel.Info);
        }
    }

    // ─── 玩家解析 ───

    private static Farmer GetOnlinePlayer(long id) => Game1.GetPlayer(id, true);

    private static Farmer GetAnyPlayer(long id)
    {
        foreach (var f in Game1.getAllFarmers())
            if (f != null && f.UniqueMultiplayerID == id)
                return f;
        return null;
    }

    private static Farmer GetPlayerByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        foreach (var f in Game1.getAllFarmers())
            if (f != null && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                return f;
        return null;
    }

    /// <summary>解析玩家参数：数字ID | 名字</summary>
    private static Farmer ResolvePlayer(string arg)
    {
        if (long.TryParse(arg, out long id))
        {
            var f = GetOnlinePlayer(id);
            if (f != null) return f;
            return GetAnyPlayer(id);
        }
        return GetPlayerByName(arg);
    }
}
