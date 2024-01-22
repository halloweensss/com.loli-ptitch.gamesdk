using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Shortcut
{
    public class Shortcut
    {
        private static Shortcut _instance;

        private Dictionary<PlatformServiceType, IShortcutApp> _services = new Dictionary<PlatformServiceType, IShortcutApp>();
        internal static Shortcut Instance => _instance ??= new Shortcut();

        internal void Register(IShortcutApp app)
        {
            if (_services.ContainsKey(app.PlatformService))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Shortcut]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Shortcut]: Platform {app.PlatformService} is registered!");
            }
        }

        public static async Task<bool> Create()
        {
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Shortcut]: Before create shortcut, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return false;
            }
            

            List<bool> created = new List<bool>();

            foreach (var service in _instance._services)
            {
                try
                {
                    created.Add(await service.Value.Create());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Shortcut]: An error occurred while creating the shortcut {e.Message}!");
                    }
                    
                    return false;
                }
            }

            if (created.Count == 0)
            {
                return false;
            }
            
            foreach (var shortcutCreated in created)
            {
                if (shortcutCreated)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static async Task<bool> CanCreate()
        {
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Shortcut]: Before check create shortcut, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return false;
            }
            

            List<bool> created = new List<bool>();

            foreach (var service in _instance._services)
            {
                try
                {
                    created.Add(await service.Value.CanCreate());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Shortcut]: An error occurred while check creating the shortcut {e.Message}!");
                    }
                    
                    return false;
                }
            }

            if (created.Count == 0)
            {
                return false;
            }
            
            foreach (var shortcutCreated in created)
            {
                if (shortcutCreated)
                {
                    return true;
                }
            }

            return false;
        }
    }
}