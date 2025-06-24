using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Time
{
    public class Time : IGameService
    {
        private static readonly Time Instance = new();

        private readonly Dictionary<string, ITimeApp> _services = new(2);

        public string ServiceName => "Time";

        public static void Register(ITimeApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(ITimeApp app)
        {
            if (_services.TryAdd(app.ServiceId, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning($"[GameSDK.Time]: The platform {app.ServiceId} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Time]: Platform {app.ServiceId} is registered!");
        }

        public static async Task<long> GetTimestamp()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Time]: Before request time, initialize the sdk\nGameApp.Initialize()!");

                return 0;
            }

            long timestamp = 0;

            foreach (var service in Instance._services)
                try
                {
                    timestamp = await service.Value.GetTimestamp();

                    if (timestamp > 0)
                        break;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Time]: An get time error has occurred {e.Message}!");

                    return 0;
                }

            if (timestamp <= 0)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Time]: Get time failed!");

                return 0;
            }

            return timestamp;
        }
    }
}