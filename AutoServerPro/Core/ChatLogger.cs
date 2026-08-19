#nullable disable
using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AutoServerPro.Core;

public class ChatLogger
{
    private readonly IMonitor _monitor;
    private readonly string _modId;
    private Harmony _harmony;

    public ChatLogger(IMonitor monitor, string modId)
    {
        _monitor = monitor;
        _modId = modId;
    }

    public void Install()
    {
        try
        {
            _harmony = new Harmony(_modId);
            var original = AccessTools.Method(typeof(ChatBox), "receiveChatMessage",
                new Type[] { typeof(long), typeof(int), typeof(LocalizedContentManager.LanguageCode), typeof(string) });
            if (original == null)
            {
                _monitor.Log("无法找到 ChatBox.receiveChatMessage", LogLevel.Warn);
                return;
            }
            _harmony.Patch(original, postfix: new HarmonyMethod(typeof(ChatLogger), nameof(AfterReceiveChatMessage)));
            _monitor.Log("聊天记录补丁已安装", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装聊天补丁失败：{ex.Message}", LogLevel.Error);
        }
    }

    private static string CleanMessage(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        string replaced = Regex.Replace(raw, @"\[\d+\]", "[表情]");
        replaced = Regex.Replace(replaced, @"(\[表情\]\s*)+", "[表情] ");
        return replaced.Trim();
    }

    private static void AfterReceiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
    {
        if (chatKind != 0) return;
        var sender = Game1.GetPlayer(sourceFarmer, true) ?? Game1.MasterPlayer;
        string name = sender?.Name ?? $"Unknown ({sourceFarmer})";
        string clean = CleanMessage(message);
        if (string.IsNullOrWhiteSpace(clean)) return;
        ModEntry.Instance?.Monitor.Log($"[聊天] {name}: {clean}", LogLevel.Debug);
    }
}
