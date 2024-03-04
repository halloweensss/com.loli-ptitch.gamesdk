using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Core.Properties;
using GameSDK.Core.Tools;
using GameSDK.RemoteConfigs;
using UnityEngine;

namespace Plugins.YaGames.RemoteConfis
{
    public class YaRemoteConfigs : IRemoteConfigsApp
    {
        private static readonly YaRemoteConfigs _instance = new YaRemoteConfigs();
        
        private InitializationStatus _status = InitializationStatus.None;
        
        private Dictionary<string, RemoteConfigValue> _remoteValues = new Dictionary<string, RemoteConfigValue>(16);
        
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => _status;
        
        public IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues => _remoteValues;

        public async Task Initialize()
        {
#if !UNITY_EDITOR
            YaRemoteConfigsInitialize(OnSuccess, OnError);
            _status = InitializationStatus.Waiting;
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();
#else
            _status = InitializationStatus.Waiting;

            var data = new List<KeyValueStruct<string>>
            {
                new("1", "1"),
                new("2", "2"),
                new("4", "3"),
            };

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };
            
            var json = JsonUtility.ToJson(serializableList);
                
            OnSuccess(json);
            await Task.CompletedTask;
#endif

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string value)
            {
                _instance._status = InitializationStatus.Initialized;

                var values = JsonUtility.FromJson<SerializableList<KeyValueStruct<string>>>(value);

                foreach (var data in values.data)
                {
                    _instance.TryAddOrReplace(data.key, data.value, ConfigValueSource.RemoteValue);
                }
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.RemoteConfigs]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string value)
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.RemoteConfigs]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }
        
        public async Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters)
        {
#if !UNITY_EDITOR
            var data = new List<KeyValueStruct<string>>(parameters.Length);

            foreach (var parameter in parameters)
            {
                data.Add(new KeyValueStruct<string>(parameter.Key, parameter.Value));
            }

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };
            
            var json = JsonUtility.ToJson(serializableList);
            YaRemoteConfigsInitializeWithClientParameters(json, OnSuccess, OnError);
            _status = InitializationStatus.Waiting;
            
            while (_status == InitializationStatus.Waiting)
                await Task.Yield();
#else
            _status = InitializationStatus.Waiting;
            
            var data = new List<KeyValueStruct<string>>
            {
                new("1", "4"),
                new("2", "5"),
                new("4", "6"),
            };

            var serializableList = new SerializableList<KeyValueStruct<string>>
            {
                data = data
            };
            
            var json = JsonUtility.ToJson(serializableList);

            OnSuccess(json);
            
            await Task.CompletedTask;
#endif

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnSuccess(string value)
            {
                _instance._status = InitializationStatus.Initialized;

                var values = JsonUtility.FromJson<SerializableList<KeyValueStruct<string>>>(value);

                foreach (var data in values.data)
                {
                    _instance.TryAddOrReplace(data.key, data.value, ConfigValueSource.RemoteValue);
                }
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.RemoteConfigs]: YaGamesApp initialized!");
                }
            }

            [MonoPInvokeCallback(typeof(Action<string>))]
            static void OnError(string value)
            {
                _instance._status = InitializationStatus.Error;
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.RemoteConfigs]: An error occurred while initializing the YaGamesApp!");
                }
            }
        }

        private void TryAddOrReplace(string key, string value, ConfigValueSource source)
        {
            if (_remoteValues.ContainsKey(key))
            {
                _remoteValues[key] = new RemoteConfigValue(System.Text.Encoding.UTF8.GetBytes(value), source);
            }
            else
            {
                _remoteValues.Add(key, new RemoteConfigValue
                {
                    Data = System.Text.Encoding.UTF8.GetBytes(value),
                    Source = source
                });
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            RemoteConfigs.Instance.Register(_instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaRemoteConfigsInitialize(Action<string> onSuccess, Action<string> onError);
        
        [DllImport("__Internal")]
        private static extern void YaRemoteConfigsInitializeWithClientParameters(string parameters, Action<string> onSuccess, Action<string> onError);
    }
}