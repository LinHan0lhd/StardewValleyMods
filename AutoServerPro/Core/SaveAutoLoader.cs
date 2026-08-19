using System;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SaveAutoLoader
{
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private readonly SavePathManager _pathManager;

    public string? CurrentSaveName { get; private set; }

    public SaveAutoLoader(IMonitor monitor, ModConfig config, SavePathManager pathManager)
    {
        _monitor = monitor;
        _config = config;
        _pathManager = pathManager;
    }

    public bool AutoLoadSave()
    {
        string? saveName = !string.IsNullOrWhiteSpace(_config.NewSaveName)
            ? _config.NewSaveName
            : GetLatestSave();

        if (string.IsNullOrEmpty(saveName))
        {
            _monitor.Log("未找到存档", LogLevel.Info);
            return false;
        }

        string src = DetermineLoadSource(saveName);
        if (src == "TempSaves")
            _pathManager.RedirectSavesToTemp();
        else
            _pathManager.RedirectSavesToOriginal();

        _monitor.Log($"存档目录：{src}", LogLevel.Info);
        _monitor.Log($"加载存档：{saveName}", LogLevel.Info);
        LoadSave(saveName);
        Game1.multiplayerMode = 2;
        return true;
    }

    private string? GetLatestSave()
    {
        if (Directory.Exists(_pathManager.SavesRootPath))
        {
            var dirs = Directory.GetDirectories(_pathManager.SavesRootPath)
                .OrderByDescending(d => Directory.GetLastWriteTime(d))
                .ToList();

            if (dirs.Count > 0)
            {
                string name = Path.GetFileName(dirs[0]);
                string tempDir = Path.Combine(_pathManager.TempSavesRootPath, name);
                if (Directory.Exists(tempDir) && Directory.GetLastWriteTime(tempDir) > Directory.GetLastWriteTime(dirs[0]))
                    return name;
                return name;
            }
        }

        if (Directory.Exists(_pathManager.TempSavesRootPath))
        {
            var dirs = Directory.GetDirectories(_pathManager.TempSavesRootPath)
                .OrderByDescending(d => Directory.GetLastWriteTime(d))
                .ToList();
            if (dirs.Count > 0) return Path.GetFileName(dirs[0]);
        }

        return null;
    }

    private void LoadSave(string saveName)
    {
        string path = Path.Combine(_pathManager.CurrentSavesPath, saveName);
        if (!Directory.Exists(path))
        {
            _monitor.Log($"存档 {saveName} 不存在", LogLevel.Warn);
            return;
        }

        try
        {
            SaveGame.Load(saveName);
            CurrentSaveName = saveName;
            if (Game1.activeClickableMenu is TitleMenu menu)
                menu.exitThisMenu(false);
            _monitor.Log($"存档 {saveName} 加载成功", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"加载存档失败：{ex.Message}", LogLevel.Error);
        }
    }

    private string DetermineLoadSource(string saveName)
    {
        string savesDir = Path.Combine(_pathManager.SavesRootPath, saveName);
        string tempDir = Path.Combine(_pathManager.TempSavesRootPath, saveName);
        bool hasSaves = Directory.Exists(savesDir);
        bool hasTemp = Directory.Exists(tempDir);

        if (!hasTemp) return "Saves";
        if (!hasSaves) return "TempSaves";

        var savesTime = GetDirectoryLatestTime(savesDir);
        var tempTime = GetDirectoryLatestTime(tempDir);
        if (!savesTime.HasValue) return "TempSaves";
        if (!tempTime.HasValue) return "Saves";
        return savesTime.Value >= tempTime.Value ? "Saves" : "TempSaves";
    }

    private DateTime? GetDirectoryLatestTime(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return null;
        try
        {
            var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            if (files.Length == 0) return null;
            DateTime latest = DateTime.MinValue;
            foreach (var f in files)
            {
                var t = File.GetLastWriteTime(f);
                if (t > latest) latest = t;
            }
            return latest;
        }
        catch { return null; }
    }
}
