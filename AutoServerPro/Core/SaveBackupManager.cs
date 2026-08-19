using System;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SaveBackupManager
{
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private readonly SavePathManager _pathManager;

    public SaveBackupManager(IMonitor monitor, ModConfig config, SavePathManager pathManager)
    {
        _monitor = monitor;
        _config = config;
        _pathManager = pathManager;
    }

    public void AutoBackupCheck(string currentSaveName)
    {
        if (!_config.AutoCleanOldBackup) return;
        if (string.IsNullOrEmpty(currentSaveName)) return;

        string backupRoot = GetBackupRootPath();
        string saveBackupDir = Path.Combine(backupRoot, currentSaveName);
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

    private void DoSaveBackup(string saveName)
    {
        try
        {
            string src = Path.Combine(_pathManager.CurrentSavesPath, saveName);
            string backupRoot = GetBackupRootPath();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dest = Path.Combine(backupRoot, saveName, timestamp);

            CopyDirectory(src, dest);
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
            .Skip(_config.MaxBackupCount)
            .ToList();

        foreach (var dir in dirs)
        {
            try { Directory.Delete(dir.FullName, true); }
            catch { }
        }
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

    private string GetBackupRootPath()
    {
        string p = _pathManager.BackupRootPath;
        if (!Directory.Exists(p)) Directory.CreateDirectory(p);
        return p;
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        foreach (var dir in Directory.GetDirectories(sourceDir))
            CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
    }
}