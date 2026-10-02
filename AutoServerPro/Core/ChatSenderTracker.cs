using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AutoServerPro.Core;

/// <summary>
/// 追踪最近一条聊天消息的发送者 ID，供聊天指令使用。
/// 因为原版 ChatCommands handler 只接收 command 和 chat，无法直接获取发送指令的玩家。
/// </summary>
public static class ChatSenderTracker
{
    public static long LastSenderId { get; private set; } = -1L;

    public static void Install(IMonitor monitor)
    {
        try
        {
            var harmony = new Harmony("LinHan.AutoServerPro.ChatSenderTracker");
            var method = AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage));
            if (method == null)
            {
                monitor.Log("无法找到 ChatBox.receiveChatMessage 方法", LogLevel.Warn);
                return;
            }

            var patchInfo = Harmony.GetPatchInfo(method);
            bool hasPrefix = patchInfo?.Prefixes?.Any(p => p.owner == "LinHan.AutoServerPro.ChatSenderTracker") ?? false;
            if (!hasPrefix)
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(ChatSenderTracker), nameof(ReceiveChatMessagePrefix)));
        }
        catch (Exception ex)
        {
            monitor.Log($"安装聊天发送者追踪失败: {ex.Message}", LogLevel.Warn);
        }
    }

    private static void ReceiveChatMessagePrefix(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
    {
        // 只记录玩家聊天（chatKind 0），不记录系统消息
        if (chatKind == 0 && sourceFarmer != 0L)
        {
            LastSenderId = sourceFarmer;
        }
    }
}
