#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Locations;
using AutoServerPro.Models;

namespace AutoServerPro.Core
{
    public class CPUDispatcher
    {
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private Harmony _harmony;
        private bool _installed;

        private static readonly List<string> _patchedMethods = new();

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

                if (_config.DisableViewportUpdate)
                    InstallViewportPatch();

                if (_config.DisableWeatherParticles)
                    InstallWeatherPatch();

                if (_config.DisableGamepadInput)
                    DisableGamepadControls();

                if (_config.DisableAllInputProcessing)
                    InstallInputPatch();

                _installed = true;

                var features = new List<string>();
                if (_config.SkipDrawing) features.Add("跳过绘制");
                if (_config.DisableAudio) features.Add("禁用音频");
                if (_config.DisableViewportUpdate) features.Add("禁用视口更新");
                if (_config.DisableWeatherParticles) features.Add("禁用天气粒子");
                if (_config.DisableGamepadInput) features.Add("禁手柄");
                if (_config.DisableAllInputProcessing) features.Add("禁用输入处理");
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
            _patchedMethods.Add("Game1.Draw");
            _monitor.Log("Draw补丁已安装", LogLevel.Debug);
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

            _patchedMethods.Add("AudioEngineWrapper.Update");
            _patchedMethods.Add("Game1.updateMusic");
            _monitor.Log("音频补丁已安装", LogLevel.Debug);
        }

        private static bool AudioUpdatePrefix() => false;
        private static bool UpdateMusicPrefix() => false;

        private void InstallViewportPatch()
        {
            var vpMethod = AccessTools.Method(typeof(Game1), nameof(Game1.UpdateViewPort),
                new[] { typeof(bool), typeof(Point) });
            if (vpMethod == null)
            {
                _monitor.Log("无法找到 Game1.UpdateViewPort 方法", LogLevel.Warn);
                return;
            }

            var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateViewPortPrefix));
            _harmony.Patch(vpMethod, prefix: prefix);
            _patchedMethods.Add("Game1.UpdateViewPort");
            _monitor.Log("视口更新补丁已安装", LogLevel.Debug);
        }

        private static bool UpdateViewPortPrefix()
        {
            if (Game1.game1 != null && Game1.player != null)
            {
                Game1.viewport.X = (int)Game1.player.Position.X - Game1.viewport.Width / 2;
                Game1.viewport.Y = (int)Game1.player.Position.Y - Game1.viewport.Height / 2;
            }
            return false;
        }

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

            _patchedMethods.Add("Game1.updateRaindropPosition");
            _patchedMethods.Add("Game1.updateDebrisWeatherForMovement");
            _monitor.Log("天气粒子补丁已安装", LogLevel.Debug);
        }

        private static bool UpdateRaindropPrefix() => false;
        private static bool UpdateDebrisWeatherPrefix() => false;

        private void DisableGamepadControls()
        {
            try
            {
                if (Game1.options != null)
                {
                    Game1.options.gamepadControls = false;
                    _monitor.Log("手柄控制已禁用", LogLevel.Debug);
                }
            }
            catch { }
        }

        private void InstallInputPatch()
        {
            var updateInput = AccessTools.Method(typeof(Game1), nameof(Game1.UpdateControlInput),
                new[] { typeof(GameTime) });
            if (updateInput == null)
            {
                _monitor.Log("无法找到 Game1.UpdateControlInput 方法", LogLevel.Warn);
                return;
            }

            var prefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateControlInputPrefix));
            _harmony.Patch(updateInput, prefix: prefix);

            var updateChatBox = AccessTools.Method(typeof(Game1), nameof(Game1.UpdateChatBox));
            if (updateChatBox != null)
            {
                var chatPrefix = new HarmonyMethod(typeof(CPUDispatcher), nameof(UpdateChatBoxPrefix));
                _harmony.Patch(updateChatBox, prefix: chatPrefix);
            }

            _patchedMethods.Add("Game1.UpdateControlInput");
            _patchedMethods.Add("Game1.UpdateChatBox");
            _monitor.Log("输入处理补丁已安装", LogLevel.Debug);
        }

        private static void UpdateControlInputPrefix(ref GameTime time)
        {
            if (Game1.player != null && Game1.inputSimulator != null)
            {
                Game1.inputSimulator.Update(time);
            }
        }

        private static bool UpdateControlInputPrefix(ref GameTime time, ref IEnumerable<Keys> __state1, ref IEnumerable<Buttons> __state2, ref IEnumerable<Buttons> __state3)
        {
            return false;
        }

        private static bool UpdateChatBoxPrefix() => false;

        public void ReapplySettings()
        {
            if (!_installed) return;

            if (_config.DisableGamepadInput && Game1.options != null)
            {
                Game1.options.gamepadControls = false;
            }
        }
    }
}