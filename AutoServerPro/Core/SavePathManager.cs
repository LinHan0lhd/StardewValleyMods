using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SavePathManager
{
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private Harmony? _harmony;
    private static string? _tempSavesPathOverride;

    public string CurrentSavesPath { get; private set; }

    public string SavesRootPath => string.IsNullOrWhiteSpace(_config.CustomSavesPath)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves")
        : _config.CustomSavesPath;

    public string BackupRootPath => string.IsNullOrWhiteSpace(_config.BackupPath)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "FarmArchives")
        : _config.BackupPath;

    public string TempSavesRootPath => string.IsNullOrWhiteSpace(_config.CustomTempSavesPath)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "TempSaves")
        : _config.CustomTempSavesPath;

    public SavePathManager(IMonitor monitor, ModConfig config)
    {
        _monitor = monitor;
        _config = config;
        CurrentSavesPath = SavesRootPath;

        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.SavePathRedirect");
            SavePatch.Monitor = monitor;
        }
        catch (Exception ex)
        {
            _monitor.Log($"注册 NPC 补丁失败: {ex.Message}", LogLevel.Warn);
        }
    }

    public string ExtraDataPath(string saveName) => Path.Combine(CurrentSavesPath, saveName, "EXTRADATA");

    public void RedirectSavesToTemp()
    {
        try
        {
            _harmony ??= new Harmony("LinHan.AutoServerPro.SavePathRedirect");
            var method = AccessTools.Method(typeof(Program), "GetSavesFolder");
            if (method == null)
            {
                _monitor.Log("无法找到 Program.GetSavesFolder 方法", LogLevel.Error);
                return;
            }

            var patchInfo = Harmony.GetPatchInfo(method);
            bool hasPrefix = patchInfo?.Prefixes?.Any(p => p.owner == "LinHan.AutoServerPro.SavePathRedirect") ?? false;
            if (!hasPrefix)
                _harmony.Patch(method, prefix: new HarmonyMethod(typeof(SavePathManager), nameof(GetSavesFolderPrefix)));

            _tempSavesPathOverride = TempSavesRootPath;
            CurrentSavesPath = TempSavesRootPath;
        }
        catch (Exception ex)
        {
            _monitor.Log($"重定向存档路径失败: {ex.Message}", LogLevel.Error);
        }
    }

    public void RedirectSavesToOriginal()
    {
        try
        {
            if (_harmony != null)
            {
                var method = AccessTools.Method(typeof(Program), "GetSavesFolder");
                if (method != null)
                    _harmony.Unpatch(method, HarmonyPatchType.Prefix, "LinHan.AutoServerPro.SavePathRedirect");
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"恢复存档路径失败: {ex.Message}", LogLevel.Warn);
        }
        finally
        {
            CurrentSavesPath = SavesRootPath;
        }
    }

    private static bool GetSavesFolderPrefix(ref string __result)
    {
        __result = _tempSavesPathOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StardewValley", "TempSaves");
        return false;
    }
}
