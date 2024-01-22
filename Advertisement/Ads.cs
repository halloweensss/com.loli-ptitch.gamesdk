using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Ads
    {
        private static Ads _instance;
        private InitializationStatus _initializationStatus = InitializationStatus.None;

        private Dictionary<PlatformServiceType, IAdsApp> _services = new Dictionary<PlatformServiceType, IAdsApp>();
        internal static Ads Instance => _instance ??= new Ads();
        internal static Dictionary<PlatformServiceType, IAdsApp> Services => Instance._services;
        public static Interstitial Interstitial { get; } = new Interstitial();
        public static Rewarded Rewarded { get; } = new Rewarded();
        public static Banner Banner { get; } = new Banner();

        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        
        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        internal void Register(IAdsApp app)
        {
            if (_services.ContainsKey(app.PlatformService))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Advertisement]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Advertisement]: Platform {app.PlatformService} is registered!");
            }
        }
        
        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Advertisement]: SDK has already been initialized!");
                }
                
                return;
            }
            
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement]: Before initialize ads, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            _instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.Initialize();
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Advertisement]: An initialize SDK error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }
    }
}