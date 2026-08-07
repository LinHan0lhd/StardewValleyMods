#nullable disable
using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using AutoServerPro.Models;

namespace AutoServerPro.Core
{
    public class CPUDispatcher
    {
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private Harmony _harmony;
        private bool _installed;

        public CPUDispatcher(IMonitor monitor, ModConfig config)
        {
            _monitor = monitor;
            _config = config;
        }

        public void UpdateConfig(ModConfig config) => _config = config;

        public void Install()
        {
            if (_installed) return;

            try
            {
                _harmony = new Harmony("LinHan.AutoServerPro.CPUOptimizer");

                if (_config.SkipDrawing)
                    InstallDrawPatch();

                if (_config.DisableAudio)
                    InstallAudioPatch();

                if (_config.DisableWeatherParticles)
                    InstallWeatherPatch();

                InstallInputPatches();

                _installed = true;

                var features = new List<string>();
                if (_config.SkipDrawing) features.Add("跳过绘制");
                if (_config.DisableAudio) features.Add("禁用音频");
                if (_config.DisableWeatherParticles) features.Add("禁用天气粒子");
                if (_config.DisableKeyboardInput) features.Add("禁键盘");
                if (_config.DisableMouseInput) features.Add("禁鼠标");
                if (_config.DisableGamepadInput) features.Add("禁手柄");
                if (features.Count > 0)
                    _monitor.Log($"CPU优化已启用: {string.Join(", ", features)}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"CPU优化安装失败：{ex.Message}", LogLevel.Error);
            }
        }

        private void InstallDrawPatch()
        {
            var drawMethod = AccessTools.Method(typeof(Game1), nameof(Game1.Draw),
                new[] { typeof(GameTime) });
            if (drawMethod == null)
            {
                _monitor.Log("无法找到 Game1.Draw 方法", LogLevel.Warn);
                return;
            }

            var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(DrawPrefix));
            _harmony.Patch(drawMethod, prefix: prefix);
            _monitor.Log("Draw补丁已安装（无头服务器跳过渲染）", LogLevel.Debug);
        }

        private static bool DrawPrefix()
        {
            if (Game1.game1 != null)
                Game1.game1.isDrawing = false;
            return false;
        }

        private void InstallAudioPatch()
        {
            var audioUpdate = AccessTools.Method(typeof(AudioEngineWrapper), "Update");
            if (audioUpdate == null)
            {
                _monitor.Log("无法找到 AudioEngineWrapper.Update 方法", LogLevel.Warn);
                return;
            }

            var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(AudioUpdatePrefix));
            _harmony.Patch(audioUpdate, prefix: prefix);

            var updateMusic = AccessTools.Method(typeof(Game1), nameof(Game1.updateMusic));
            if (updateMusic != null)
            {
                var musicPrefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateMusicPrefix));
                _harmony.Patch(updateMusic, prefix: musicPrefix);
            }

            _monitor.Log("音频补丁已安装", LogLevel.Debug);
        }

        private static bool AudioUpdatePrefix() => false;
        private static bool UpdateMusicPrefix() => false;

        private void InstallWeatherPatch()
        {
            var rainMethod = AccessTools.Method(typeof(Game1), nameof(Game1.updateRaindropPosition));
            if (rainMethod != null)
            {
                var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateRaindropPrefix));
                _harmony.Patch(rainMethod, prefix: prefix);
            }

            var weatherDebris = AccessTools.Method(typeof(Game1), "updateDebrisWeatherForMovement",
                new[] { typeof(List<WeatherDebris>) });
            if (weatherDebris != null)
            {
                var prefix2 = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateDebrisWeatherPrefix));
                _harmony.Patch(weatherDebris, prefix: prefix2);
            }

            _monitor.Log("天气粒子补丁已安装", LogLevel.Debug);
        }

        private static bool UpdateRaindropPrefix() => false;
        private static bool UpdateDebrisWeatherPrefix() => false;

        private void InstallInputPatches()
        {
            if (_config.DisableKeyboardInput)
            {
                var kbMethod = AccessTools.Method(typeof(InputState), nameof(InputState.GetKeyboardState));
                if (kbMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(GetKeyboardStatePrefix));
                    _harmony.Patch(kbMethod, prefix: prefix);
                    _monitor.Log("键盘输入补丁已安装", LogLevel.Debug);
                }
                else
                {
                    _monitor.Log("无法找到 InputState.GetKeyboardState 方法", LogLevel.Warn);
                }
            }

            if (_config.DisableMouseInput)
            {
                var mouseMethod = AccessTools.Method(typeof(InputState), nameof(InputState.GetMouseState));
                if (mouseMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(GetMouseStatePrefix));
                    _harmony.Patch(mouseMethod, prefix: prefix);
                    _monitor.Log("鼠标输入补丁已安装", LogLevel.Debug);
                }
                else
                {
                    _monitor.Log("无法找到 InputState.GetMouseState 方法", LogLevel.Warn);
                }
            }

            if (_config.DisableGamepadInput)
            {
                var padMethod = AccessTools.Method(typeof(InputState), nameof(InputState.GetGamePadState));
                if (padMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(GetGamePadStatePrefix));
                    _harmony.Patch(padMethod, prefix: prefix);
                    _monitor.Log("手柄输入补丁已安装", LogLevel.Debug);
                }
                else
                {
                    _monitor.Log("无法找到 InputState.GetGamePadState 方法", LogLevel.Warn);
                }
            }
        }

        private static bool GetKeyboardStatePrefix(ref KeyboardState __result)
        {
            __result = default(KeyboardState);
            return false;
        }

        private static bool GetMouseStatePrefix(ref MouseState __result)
        {
            __result = new MouseState(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            return false;
        }

        private static bool GetGamePadStatePrefix(ref GamePadState __result)
        {
            __result = default(GamePadState);
            return false;
        }

        public void ReapplySettings()
        {
            if (!_installed) return;
        }
    }
}