using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Time
{
    public class Time
    {
        private static Time _instance;

        private readonly Dictionary<PlatformServiceType, ITimeApp> _services = new(2);
        internal static Time Instance => _instance ??= new Time();
        
        internal void Register(ITimeApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Time]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Time]: Platform {app.PlatformService} is registered!");
            }
        }
        
        public static async Task<long> GetTimestamp()
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
                        $"[GameSDK.Time]: Before request time, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return default;
            }
            
            long timestamp = 0;
            
            foreach (var service in _instance._services)
            {
                try
                {
                    timestamp = await service.Value.GetTimestamp();
                    
                    if(timestamp > 0)
                        break;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Time]: An get time error has occurred {e.Message}!");
                    }
                    
                    return default;
                }
            }

            if (timestamp <= 0)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning("[GameSDK.Time]: Get time failed!");
                }
                
                return default;
            }

            return timestamp;
        }
    }
}