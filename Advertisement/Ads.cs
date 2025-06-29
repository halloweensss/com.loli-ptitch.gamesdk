using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Ads : IGameService
    {
        private static readonly Ads Instance = new();

        private readonly Dictionary<string, IAdsApp> _services = new(2);

        private InitializationStatus _initializationStatus = InitializationStatus.None;
        public static Interstitial Interstitial { get; } = new();
        public static Rewarded Rewarded { get; } = new();
        public static Banner Banner { get; } = new();
        
        public static Revenue Revenue { get; } = new();

        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public string ServiceName => "Advertisement";

        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        public static event Action<AdRevenueData> OnAdRevenuePaid;

        public static void Register(IAdsApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IAdsApp app)
        {
            if (_services.TryAdd(app.ServiceId, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement]: The platform {app.ServiceId} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement]: Platform {app.ServiceId} is registered!");
        }

        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Advertisement]: SDK has already been initialized!");

                return;
            }

            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Advertisement]: Before initialize ads, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            Instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.Initialize();
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Advertisement]: An initialize SDK error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }
    }
}