using System;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using AutoServerPro.Models;
using AutoServerPro.Utils;

namespace AutoServerPro.Core;

public class SaveBackupManager
{
    private readonly IMonitor _monitor;
    private ModConfig _config;
    private readonly SavePathManager _pathManager;

    public SaveBackupManager(IMonitor monitor, ModConfig config, SavePathManager pathManager)
    {
        _monitor = monitor;
        _config = config;
        _pathManager = pathManager;
    }

    public void UpdateConfig(ModConfig config) => _config = config;

    public void AutoBackupCheck(string currentSaveName)
    {
        if (!_config.AutoCleanOldBackup || string.IsNullOrEmpty(currentSaveName)) return;

        string saveBackupDir = Path.Combine(GetBackupRootPath(), currentSaveName);
        if (!Directory.Exists(saveBackupDir)) Directory.CreateDirectory(saveBackupDir);

        var backups = Directory.GetDirectories(saveBackupDir)
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTime)
            .ToList();

        bool needsBackup = backups.Count == 0 ||
            (DateTime.Now - backups[0].CreationTime).TotalDays >= _config.AutoBackupDayInterval;

        if (needsBackup)
            DoSaveBackup(currentSaveName);
    }

    public void ForceBackup(string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            _monitor.Log("请指定存档名称", LogLevel.Warn);
            return;
        }
        DoSaveBackup(saveName);
    }

    public void ManualBackupCommand(string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            _monitor.Log("请指定存档名称", LogLevel.Warn);
            return;
        }
        DoSaveBackup(saveName);
    }

    private void DoSaveBackup(string saveName)
    {
        try
        {
            string src = Path.Combine(_pathManager.CurrentSavesPath, saveName);
            string backupRoot = GetBackupRootPath();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dest = Path.Combine(backupRoot, saveName, timestamp);

            DirectoryHelper.CopyDirectory(src, dest, true);
            _monitor.Log($"存档已备份: {dest}", LogLevel.Info);
            CleanOldBackups(Path.Combine(backupRoot, saveName));
        }
        catch (Exception ex)
        {
            _monitor.Log($"备份失败: {ex.Message}", LogLevel.Error);
        }
    }

    private void CleanOldBackups(string root)
    {
        if (!Directory.Exists(root)) return;

        var dirs = Directory.GetDirectories(root)
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTime)
            .ToList();

        var excess = dirs.Skip(_config.MaxBackupCount).ToList();
        var cutoff = DateTime.Now.AddDays(-_config.AutoBackupDayInterval);
        var expired = dirs.Where(d => d.CreationTime < cutoff).ToList();

        var toDelete = excess.Union(expired).ToList();
        foreach (var dir in toDelete)
        {
            try { Directory.Delete(dir.FullName, true); }
            catch { }
        }

        if (toDelete.Count > 0)
            _monitor.Log($"清理备份: 删除 {toDelete.Count} 个", LogLevel.Debug);
    }

    private string GetBackupRootPath()
    {
        string p = _pathManager.BackupRootPath;
        if (!Directory.Exists(p)) Directory.CreateDirectory(p);
        return p;
    }
}
