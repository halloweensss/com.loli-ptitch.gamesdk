using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;
using DeviceType = GameSDK.Core.DeviceType;

namespace GameSDK.Plugins.YaGames.Core
{
    internal sealed class YaGamesApp : ICoreApp
    {
        private static readonly YaGamesApp _instance = new YaGamesApp();

        private InitializationStatus _status = InitializationStatus.None;
        private DeviceType _device = DeviceType.Undefined;
        private YaEnvironment _environment = new();
        private bool _ready;

        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => _status;

        public DeviceType DeviceType
        {
            get
            {
                if (_device == DeviceType.Undefined)
                {
                    InitializeDeviceType();
                }

                return _device;
            }
        }

        public string AppId => _environment.app.id;
        public string Lang => _environment.i18n.lang;
        public string Payload => _environment.payload;
        public bool IsReady => _ready;

        public async Task Initialize()
        {
            if (_status == InitializationStatus.Initialized || _status == InitializationStatus.Waiting)
            {
                return;
            }

            await _instance.InitializeInternal();
        }

        public async Task Ready()
        {
            if (_ready)
                return;

            await _instance.GameReadyInternal();
        }

        private async Task InitializeInternal()
        {
#if !UNITY_EDITOR
            YaGamesInitialize(OnSuccess, OnError);
            _status = InitializationStatus.Waiting;
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();
#else
            _status = InitializationStatus.Waiting;
            OnSuccess();
            await Task.CompletedTask;
#endif

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._status = InitializationStatus.Initialized;
                
                _instance.InitializeDeviceType();
                _instance.InitializeEnvironment();
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }
        
        private async Task GameReadyInternal()
        {
#if !UNITY_EDITOR
            YaGamesReady(OnSuccess, OnError);
#else
            await Task.CompletedTask;
            OnSuccess();
#endif
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._ready = true;
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp ready!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._ready = false;
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: An error occurred while ready the YaGamesApp!");
                }
            }
        }

        private void InitializeDeviceType()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp is not initialized!");
                }

                return;
            }

#if !UNITY_EDITOR
            _device = (DeviceType)YaGamesGetDeviceType();
#else
            _device = SystemInfo.deviceType switch
            {
                UnityEngine.DeviceType.Unknown => DeviceType.Undefined,
                UnityEngine.DeviceType.Handheld => DeviceType.Mobile,
                UnityEngine.DeviceType.Console => DeviceType.Console,
                UnityEngine.DeviceType.Desktop => DeviceType.Desktop,
                _ => DeviceType.Undefined
            };
#endif
        }
        
        private void InitializeEnvironment()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK]: YaGamesApp is not initialized!");
                }

                return;
            }

#if !UNITY_EDITOR
            _instance._environment = JsonUtility.FromJson<YaEnvironment>(YaGamesGetEnvironment());
#else
            _instance._environment = new YaEnvironment
            {
                app = new YaApp()
                {
                    id = "-1"
                },
                browser = new YaBrowser()
                {
                    lang = "en"
                },
                i18n = new YaI18n()
                {
                    lang = "en",
                    tld = "com"
                }
            };
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameApp.Instance.Register(_instance);
        }

        [DllImport("__Internal")]
        private static extern void YaGamesInitialize(Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern void YaGamesReady(Action onSuccess, Action onError);

        [DllImport("__Internal")]
        private static extern int YaGamesGetDeviceType();
        
        [DllImport("__Internal")]
        private static extern string YaGamesGetEnvironment();
    }
}
