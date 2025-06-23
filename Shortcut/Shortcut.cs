using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Shortcut
{
    public class Shortcut
    {
        private static readonly Shortcut Instance = new();

        private readonly Dictionary<PlatformServiceType, IShortcutApp> _services = new();

        public static void Register(IShortcutApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IShortcutApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Shortcut]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Shortcut]: Platform {app.PlatformService} is registered!");
        }

        public static async Task<bool> Create()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Shortcut]: Before create shortcut, initialize the sdk\nGameApp.Initialize()!");

                return false;
            }


            var created = new List<bool>();

            foreach (var service in Instance._services)
                try
                {
                    created.Add(await service.Value.Create());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError(
                            $"[GameSDK.Shortcut]: An error occurred while creating the shortcut {e.Message}!");

                    return false;
                }

            if (created.Count == 0)
                return false;

            foreach (var shortcutCreated in created)
                if (shortcutCreated)
                    return true;

            return false;
        }

        public static async Task<bool> CanCreate()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Shortcut]: Before check create shortcut, initialize the sdk\nGameApp.Initialize()!");

                return false;
            }


            var created = new List<bool>();

            foreach (var service in Instance._services)
                try
                {
                    created.Add(await service.Value.CanCreate());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError(
                            $"[GameSDK.Shortcut]: An error occurred while check creating the shortcut {e.Message}!");

                    return false;
                }

            if (created.Count == 0)
                return false;

            foreach (var shortcutCreated in created)
                if (shortcutCreated)
                    return true;

            return false;
        }
    }
}