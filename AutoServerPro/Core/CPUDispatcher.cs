#nullable disable
using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
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
                    PatchDrawMethod();

                if (_config.SkipGamepadInput)
                    DisableGamepadControls();

                ApplyFrameRateLimit();

                _installed = true;
                _monitor.Log($"CPU优化已启用 [目标帧率:{_config.TargetFPS}fps, 跳过绘制:{_config.SkipDrawing}, 跳过手柄:{_config.SkipGamepadInput}]", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"CPU优化安装失败：{ex.Message}", LogLevel.Error);
            }
        }

        private void PatchDrawMethod()
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

        private void ApplyFrameRateLimit()
        {
            try
            {
                var gameRunner = GameRunner.instance;
                if (gameRunner == null)
                {
                    _monitor.Log("GameRunner 未初始化，帧率设置将延后", LogLevel.Warn);
                    return;
                }

                int targetFps = Math.Max(10, Math.Min(60, _config.TargetFPS));
                TimeSpan targetElapsed = TimeSpan.FromMilliseconds(1000.0 / targetFps);

                gameRunner.TargetElapsedTime = targetElapsed;
                gameRunner.IsFixedTimeStep = true;

                if (Game1.graphics != null)
                {
                    Game1.graphics.SynchronizeWithVerticalRetrace = false;
                }

                _monitor.Log($"目标帧率: {targetFps} FPS ({targetElapsed.TotalMilliseconds:F1}ms), 垂直同步: 关闭", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"帧率设置失败：{ex.Message}", LogLevel.Warn);
            }
        }

        public void ReapplySettings()
        {
            if (!_installed) return;
            ApplyFrameRateLimit();
        }
    }
}