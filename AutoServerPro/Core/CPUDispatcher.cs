#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class CPUDispatcher
{
    private readonly IMonitor _monitor;
    private static IMonitor _staticMonitor;
    private ModConfig _config;
    private Harmony _harmony;
    private bool _installed;
    private Harmony _saveGameMenuHarmony;

    public CPUDispatcher(IMonitor monitor, ModConfig config)
    {
        _monitor = monitor;
        _staticMonitor = monitor;
        _config = config;
    }

    public void UpdateConfig(ModConfig config) => _config = config;

    public void Install()
    {
        if (_installed) return;

        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.CPUOptimizer");

            if (_config.SkipDrawing) InstallDrawPatch();
            if (_config.DisableAudio) InstallAudioPatch();
            if (_config.DisableWeatherParticles) InstallWeatherPatch();

            InstallInputPatches();
            InstallDayEndPatches();

            _installed = true;

            var features = new List<string>();
            if (_config.SkipDrawing) features.Add("跳过绘制");
            if (_config.DisableAudio) features.Add("禁用音频");
            if (_config.DisableWeatherParticles) features.Add("禁用天气粒子");
            if (_config.DisableKeyboardInput) features.Add("禁用键盘");
            if (_config.DisableMouseInput) features.Add("禁用鼠标");
            if (_config.DisableGamepadInput) features.Add("禁用手柄");
            if (features.Count > 0)
                _monitor.Log($"CPU优化已启用: {string.Join(" ", features)}", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"CPU优化安装失败：{ex.Message}", LogLevel.Error);
        }
    }

    private void InstallDrawPatch()
    {
        var drawMethod = AccessTools.Method(typeof(Game1), "Draw", new[] { typeof(GameTime) });
        if (drawMethod == null)
        {
            _monitor.Log("无法找到 Game1.Draw 方法", LogLevel.Warn);
            return;
        }
        _harmony.Patch(drawMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(DrawPrefix)));
    }

    private static bool DrawPrefix()
    {
        if (Game1.game1 != null) Game1.game1.isDrawing = false;
        return false;
    }

    private void InstallAudioPatch()
    {
        var audioType = AccessTools.TypeByName("StardewValley.Audio.AudioEngineWrapper");
        if (audioType == null)
        {
            _monitor.Log("无法找到 AudioEngineWrapper 类型", LogLevel.Warn);
            return;
        }

        var audioUpdate = AccessTools.Method(audioType, "Update");
        if (audioUpdate != null)
            _harmony.Patch(audioUpdate, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(AudioUpdatePrefix)));

        var updateMusic = AccessTools.Method(typeof(Game1), "updateMusic");
        if (updateMusic != null)
            _harmony.Patch(updateMusic, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateMusicPrefix)));
    }

    private static bool AudioUpdatePrefix() => false;
    private static bool UpdateMusicPrefix() => false;

    private void InstallWeatherPatch()
    {
        var rainMethod = AccessTools.Method(typeof(Game1), "updateRaindropPosition");
        if (rainMethod != null)
            _harmony.Patch(rainMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateRaindropPrefix)));

        var weatherDebrisType = AccessTools.TypeByName("StardewValley.WeatherDebris");
        var weatherDebris = AccessTools.Method(typeof(Game1), "updateDebrisWeatherForMovement",
            new[] { typeof(List<>).MakeGenericType(weatherDebrisType) });
        if (weatherDebris != null)
            _harmony.Patch(weatherDebris, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateDebrisWeatherPrefix)));
    }

    private static bool UpdateRaindropPrefix() => false;
    private static bool UpdateDebrisWeatherPrefix() => false;

    private void InstallInputPatches()
    {
        if (_config.DisableKeyboardInput)
        {
            var kbMethod = AccessTools.Method(typeof(InputState), "GetKeyboardState");
            if (kbMethod != null)
                _harmony.Patch(kbMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(GetKeyboardStatePrefix)));
            else
                _monitor.Log("无法找到 InputState.GetKeyboardState 方法", LogLevel.Warn);
        }

        if (_config.DisableMouseInput)
        {
            var mouseMethod = AccessTools.Method(typeof(InputState), "GetMouseState");
            if (mouseMethod != null)
                _harmony.Patch(mouseMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(GetMouseStatePrefix)));
            else
                _monitor.Log("无法找到 InputState.GetMouseState 方法", LogLevel.Warn);
        }

        if (_config.DisableGamepadInput)
        {
            var padMethod = AccessTools.Method(typeof(InputState), "GetGamePadState");
            if (padMethod != null)
                _harmony.Patch(padMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(GetGamePadStatePrefix)));
            else
                _monitor.Log("无法找到 InputState.GetGamePadState 方法", LogLevel.Warn);
        }
    }

    private static bool GetKeyboardStatePrefix(ref KeyboardState __result)
    {
        __result = default;
        return false;
    }

    private static bool GetMouseStatePrefix(ref MouseState __result)
    {
        __result = new MouseState(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        return false;
    }

    private static bool GetGamePadStatePrefix(ref GamePadState __result)
    {
        __result = default;
        return false;
    }

    private void InstallDayEndPatches()
    {
        _saveGameMenuHarmony = new Harmony("LinHan.AutoServerPro.SaveGameMenu");
        var updateMethod = AccessTools.Method(typeof(SaveGameMenu), "update");
        if (updateMethod != null)
            _saveGameMenuHarmony.Patch(updateMethod, prefix: new HarmonyMethod(typeof(CPUDispatcher), nameof(SaveGameMenuUpdatePrefix)));
        else
            _monitor.Log("无法找到 SaveGameMenu.update 方法", LogLevel.Warn);

    }

    private static bool SaveGameMenuUpdatePrefix(SaveGameMenu __instance)
    {
        if (!__instance.hasDrawn) __instance.hasDrawn = true;
        return true;
    }

    public void ReapplySettings()
    {
        if (!_installed) return;
    }
}
