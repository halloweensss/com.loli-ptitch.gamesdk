using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.Core.Properties;
using GameSDK.GameStorage;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.PlayerData
{
    public class YaPlayerData : IAuthApp, IStorageApp
    {
        private static readonly YaPlayerData _instance = new YaPlayerData();
        private InitializationStatus _status = InitializationStatus.None;
        private StorageStatus _lastStorageStatus = StorageStatus.None;
        private string _lastStorageData = string.Empty;
        private SignInType _signInType = SignInType.None;
        private string _id = string.Empty;
        private string _name = string.Empty;
        private Coroutine _coroutineDelayedSave = null;
        
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => _status;

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    InitializeId();
                }
                
                return _id;
            }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    InitializeName();
                }
                
                return _name;
            }
        }

        public SignInType SignInType
        {
            get
            {
                if (_signInType == SignInType.None && _status == InitializationStatus.Initialized)
                {
                    InitializeMode();
                }
                
                return _signInType;
            }
        }

        public async Task SignIn()
        {
#if !UNITY_EDITOR
            _status = InitializationStatus.Waiting;
            YaGamesGetPlayer(true, OnSuccess, OnError);
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Error)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: Attempt to get a local id!");
                }
                
                _status = InitializationStatus.Waiting;
                YaGamesGetPlayer(false, OnSuccess, OnError);
            }
            else
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
                return;
            }
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
            }
#else
            _status = InitializationStatus.Waiting;
            _signInType = SignInType.Account;
            OnSuccess();
            await Task.CompletedTask;
#endif
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._status = InitializationStatus.Initialized;
                _instance.InitializeId();
                _instance.InitializeName();
                _instance.InitializeMode();
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: An error occurred while sign in the YaPlayerData!");
                }
            }
        }

        public async Task SignInAsGuest()
        {
#if !UNITY_EDITOR
            _status = InitializationStatus.Waiting;
            YaGamesGetPlayer(false, OnSuccess, OnError);

            while (_status == InitializationStatus.Waiting)
                await Task.Yield();

            if (_status == InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You are logged in as a {_signInType}!");
                }
            }
#else
            _status = InitializationStatus.Waiting;
            _signInType = SignInType.Guest;
            OnSuccess();
            await Task.CompletedTask;
#endif
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._status = InitializationStatus.Initialized;
                _instance.InitializeId();
                _instance.InitializeName();
                _instance.InitializeMode();
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: An error occurred while sign in the YaPlayerData!");
                }
            }
        }

        private void InitializeId()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

#if !UNITY_EDITOR
            _id = YaGamesGetId();
#else
            _id = $"Id [{_signInType.ToString()}]";
#endif
        }

        private void InitializeName()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

#if !UNITY_EDITOR
            _name = YaGamesGetName();
#else
            _name = $"Name [{_signInType.ToString()}]";
#endif
        }
        
        private void InitializeMode()
        {
            if (_status != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Authentication]: YaPlayerData is not initialized!");
                }

                return;
            }

#if !UNITY_EDITOR
            var result = YaGamesGetMode();
            
            _signInType = result switch
            {
                1 => SignInType.Account,
                0 => SignInType.Guest,
                _ => SignInType.None
            };

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Authentication]: Result auth: {result}");
            }
#else
            _signInType = SignInType.Account;
#endif
        }

        public async Task<StorageStatus> Save(string key, string value)
        {
#if !UNITY_EDITOR

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                await Auth.SignInAsGuest();
            }

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: YaPlayerData is not initialized!");
                }

                return StorageStatus.Error;
            }

            _lastStorageStatus = StorageStatus.Waiting;
            YaGamesSaveData(key, value, OnSuccess, OnError);

            while (_lastStorageStatus == StorageStatus.Waiting)
                await Task.Yield();

            return _lastStorageStatus;
#else
            _lastStorageStatus = StorageStatus.Waiting;
            PlayerPrefs.SetString(key, value);
            OnSuccess();
            await Task.CompletedTask;
            return _lastStorageStatus;
#endif

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccess()
            {
                _instance._lastStorageStatus = StorageStatus.Success;

                var runner = GameApp.Runner;

                if (runner != null)
                {
                    _instance._coroutineDelayedSave ??= runner.StartCoroutine(DelayedSave());
                    return;
                }


#if !UNITY_EDITOR
                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                return;
#endif
                OnSuccessAll();
                return;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._lastStorageStatus = StorageStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to save data in the YaPlayerData!");
                }
            }
            
            static IEnumerator DelayedSave()
            {
                yield return new WaitForSeconds(1f);

#if !UNITY_EDITOR
                YaGamesSaveDataAll(OnSuccessAll, OnErrorAll);
                _instance._coroutineDelayedSave = null;
                yield break;
#endif
                OnSuccessAll();
                _instance._coroutineDelayedSave = null;
                yield break;
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnSuccessAll()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: Data saved all in the YaPlayerData!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnErrorAll()
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to save data all in the YaPlayerData!");
                }
            }
        }

        public async Task<(StorageStatus, string)> Load(string key)
        {
#if !UNITY_EDITOR

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                await Auth.SignInAsGuest();
            }

            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: YaPlayerData is not initialized!");
                }

                return (StorageStatus.Error, string.Empty);
            }

            _lastStorageStatus = StorageStatus.Waiting;
            YaGamesLoadData(key, OnSuccess, OnError);

            while (_lastStorageStatus == StorageStatus.Waiting)
                await Task.Yield();

            if (_lastStorageStatus == StorageStatus.Success)
            {
                return (_lastStorageStatus, _lastStorageData);
            }

            return (_lastStorageStatus, string.Empty);
#else
            _lastStorageStatus = StorageStatus.Waiting;
            if (PlayerPrefs.HasKey(key) == false)
            {
                OnError();
            }
            else
            {
                OnSuccess(PlayerPrefs.GetString(key));
            }
            await Task.CompletedTask;
            return (_lastStorageStatus, _lastStorageData);
#endif
            
            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string data)
            {
                _instance._lastStorageStatus = StorageStatus.Success;
                _instance._lastStorageData = data;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Storage]: Data loaded from the YaPlayerData!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError()
            {
                _instance._lastStorageStatus = StorageStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Storage]: Failed to load data from the YaPlayerData!");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Auth.Instance.Register(_instance);
            Storage.Instance.Register(_instance);
        }


        [DllImport("__Internal")]
        private static extern void YaGamesGetPlayer(bool isSigned, Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern string YaGamesGetId();
        
        [DllImport("__Internal")]
        private static extern string YaGamesGetName();
        
        [DllImport("__Internal")]
        private static extern int YaGamesGetMode();
        
        [DllImport("__Internal")]
        private static extern string YaGamesSaveData(string key, string value, Action onSuccess, Action onError);
        [DllImport("__Internal")]
        private static extern string YaGamesSaveDataAll(Action onSuccess, Action onError);
        
        [DllImport("__Internal")]
        private static extern string YaGamesLoadData(string key, Action<string> onSuccess, Action onError);
    }
}