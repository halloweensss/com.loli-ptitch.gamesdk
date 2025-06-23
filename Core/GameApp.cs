using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.Core
{
    public class GameApp
    {
        private static readonly GameApp Instance = new();

        private GameAppRunner _runner;
        private bool _isReady = false;
        private bool _isStarted = false;
        private bool _manuallyStarted = false;
        private int _startedCounter = 0;

        private bool _isVisible = true;
        private InitializationStatus _initializationStatus = InitializationStatus.None;
        private readonly Dictionary<PlatformServiceType, ICoreApp> _services = new Dictionary<PlatformServiceType, ICoreApp>();
        public static GameAppRunner Runner => Instance._runner;
        public static DeviceType DeviceType => Instance.GetDeviceType();
        public static string Lang => Instance.GetLang();
        public static string AppId => Instance.GetAppId();
        public static string Payload => Instance.GetPayload();
        public static bool IsDebugMode { get; set; } = true;
        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;
        public static bool IsReady => Instance._isReady;
        public static bool IsStarted => Instance._isStarted;
        public static bool IsVisible => Instance._isVisible;
        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        public static event Action<bool> OnStartChanged;
        public static event Action<bool> OnVisibilityChanged;
        
        public static void Register(ICoreApp app) => Instance.RegisterInternal(app);
        
        public static void RegisterRunner(GameAppRunner runner) => Instance.RegisterRunnerInternal(runner);
        
        public static void OnVisibilityChange(bool isVisible) => Instance.OnVisibilityChangeInternal(isVisible);
        
        private void RegisterInternal(ICoreApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Platform {app.PlatformService} is registered!");
            }
        }
        
        private void RegisterRunnerInternal(GameAppRunner runner)
        {
            _runner = runner;

            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Runner is registered!");
            }
        }

        private async void OnVisibilityChangeInternal(bool isVisible)
        {
            _isVisible = isVisible;

            if (_isVisible)
            {
                await GameStart();
            }
            else
            {
                await GameStop();
            }
            
            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Visibility changed to {_isVisible}");
            }

            OnVisibilityChanged?.Invoke(_isVisible);
        }

        private DeviceType GetDeviceType()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.DeviceType;
            }

            return SystemInfo.deviceType switch
            {
                UnityEngine.DeviceType.Unknown => DeviceType.Undefined,
                UnityEngine.DeviceType.Handheld => DeviceType.Mobile,
                UnityEngine.DeviceType.Console => DeviceType.Console,
                UnityEngine.DeviceType.Desktop => DeviceType.Desktop,
                _ => DeviceType.Undefined
            };
        }
        
        private string GetLang()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.Lang;
            }

            return "en";
        }
        
        private string GetPayload()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.Payload;
            }

            return string.Empty;
        }
        
        private string GetAppId()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.AppId;
            }

            return "-1";
        }
    
        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK]: SDK has already been initialized!");
                }
                
                return;
            }
            
            Instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in Instance._services)
            {
                try
                {
                    await service.Value.Initialize();
                    
                    if (service.Value.InitializationStatus == InitializationStatus.Initialized) continue;
                    
                    Instance._initializationStatus = service.Value.InitializationStatus;
                    return;
                }
                catch (Exception e)
                {
                    if (IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK]: An initialize SDK error has occurred {e.Message}!");
                    }
                    
                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }
        
        public static async Task GameReady()
        {
            if (IsReady)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: SDK has already been ready!");
                }

                return;
            }
            
            if (IsInitialized == false)
            {
                await Initialize();
            }
            
            if (IsInitialized == false)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: Before game ready, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }

            foreach (var service in Instance._services)
            {
                try
                {
                    await service.Value.Ready();
                    
                    if (service.Value.IsReady) continue;
                    
                    Instance._isReady = false;
                    return;
                }
                catch (Exception e)
                {
                    if (IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK]: An game ready SDK error has occurred {e.Message}!");
                    }
                    
                    return;
                }
            }

            Instance._isReady = true;
            
            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Game ready!");
            }
        }

        public static async Task Start()
        {
            Instance._manuallyStarted = true;
            await GameStart();
        }
        
        public static async Task Stop()
        {
            await GameStop();
            Instance._manuallyStarted = false;
        }
        
        public static async Task GameStart()
        {
            if(Instance._manuallyStarted == false)
                return;
            
            if(Instance._startedCounter > 1)
            {
                Instance._startedCounter--;
            }
            
            if(Instance._startedCounter > 1)
                return;
            
            if (IsStarted)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: SDK has already been started!");
                }

                return;
            }
            
            if (IsInitialized == false)
            {
                await Initialize();
            }
            
            if (IsInitialized == false)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: Before game start, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }

            foreach (var service in Instance._services)
            {
                try
                {
                    await service.Value.Start();
                }
                catch (Exception e)
                {
                    if (IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK]: An game start SDK error has occurred {e.Message}!");
                    }
                    
                    return;
                }
            }

            Instance._isStarted = true;
            Instance._startedCounter = 1;
            
            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Game start!");
            }
            
            OnStartChanged?.Invoke(IsStarted);
        }
        
        public static async Task GameStop()
        {
            if(Instance._manuallyStarted == false)
                return;
            
            if (IsStarted == false)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: SDK has already been stopped!");
                }

                if (Instance._startedCounter >= 1)
                {
                    Instance._startedCounter++;
                }

                return;
            }
            
            if (IsInitialized == false)
            {
                await Initialize();
            }
            
            if (IsInitialized == false)
            {
                if (IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK]: Before game stop, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }

            foreach (var service in Instance._services)
            {
                try
                {
                    await service.Value.Stop();
                }
                catch (Exception e)
                {
                    if (IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK]: An game stop SDK error has occurred {e.Message}!");
                    }
                    
                    return;
                }
            }

            Instance._isStarted = false;
            Instance._startedCounter++;
            
            if (IsDebugMode)
            {
                Debug.Log($"[GameSDK]: Game stopped!");
            }
            
            OnStartChanged?.Invoke(IsStarted);
        }
    }
}
