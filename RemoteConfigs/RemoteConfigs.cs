using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.RemoteConfigs
{
    public class RemoteConfigs
    {
        private static RemoteConfigs _instance;
        private InitializationStatus _initializationStatus = InitializationStatus.None;
        
        private Dictionary<PlatformServiceType, IRemoteConfigsApp> _services = new Dictionary<PlatformServiceType, IRemoteConfigsApp>();
        
        private Dictionary<string, RemoteConfigValue> _remoteValues = new Dictionary<string, RemoteConfigValue>(16);
        
        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        public static RemoteConfigs Instance => _instance ??= new RemoteConfigs();
        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public static IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues => Instance._remoteValues;
        
        internal void Register(IRemoteConfigsApp app)
        {
            if (_services.ContainsKey(app.PlatformService)) 
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.RemoteConfigs]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.RemoteConfigs]: Platform {app.PlatformService} is registered!");
            }
        }
        
        public static async Task Initialize()
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
                        $"[GameSDK.RemoteConfigs]: Before initialize remote configs, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            _instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.Initialize();
                    _instance.InitializeValues(service.Value.RemoteValues, ConfigValueSource.RemoteValue);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.RemoteConfigs]: An initialize SDK error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }
        
        public static async Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters)
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
                        $"[GameSDK.RemoteConfigs]: Before initialize remote configs, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            _instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.InitializeWithUserParameters(parameters);
                    _instance.InitializeValues(service.Value.RemoteValues, ConfigValueSource.RemoteValue);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.RemoteConfigs]: An initialize SDK error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }
        
        public static void SetDefaultValue(string key, object value) => _instance.SetDefaultValueInternal(key, value);

        public static bool TryGetValue<T>(string key, out T value) where T : unmanaged => _instance.TryGetValueInternal(key, out value);

        public static bool TryGetValue(string key, out RemoteConfigValue value) => _instance.TryGetValueInternal(key, out value);

        private void SetDefaultValueInternal(string key, object value)
        {
            string data;
            switch (value)
            {
                case string s:
                    data = s;
                    break;
                case IEnumerable<byte> bytes:
                {
                    List<byte> byteList = new List<byte>(bytes);
                    data = Encoding.UTF8.GetString(byteList.ToArray());
                    break;
                }
                case IEnumerable enumerable:
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (object obj in enumerable)
                        stringBuilder.Append(Convert.ToString(obj, CultureInfo.InvariantCulture));
                    data = stringBuilder.ToString();
                    break;
                }
                default:
                    data = Convert.ToString(value, CultureInfo.InvariantCulture);
                    break;
            }

            if (data == null)
                return;
            
            if (_remoteValues.TryGetValue(key, out var remoteValue) && remoteValue.Source == ConfigValueSource.DefaultValue)
            {
                _remoteValues[key] = new RemoteConfigValue(Encoding.UTF8.GetBytes(data), ConfigValueSource.DefaultValue);
            }
            else
            {
                _remoteValues.Add(key, new RemoteConfigValue
                {
                    Data = Encoding.UTF8.GetBytes(data),
                    Source = ConfigValueSource.DefaultValue
                });
            }
        }

        private bool TryGetValueInternal<T>(string key, out T value) where T : unmanaged
        {
            value = default;
            
            if (_remoteValues.TryGetValue(key, out var remoteValue) == false)
                return false;

            var type = typeof(T);
            
            if(type == typeof(string))
            {
                value = (T)(object)remoteValue.StringValue;
            }
            else if (type == typeof(byte[]))
            {
                value = (T)remoteValue.ByteArrayValue;
            }
            else if (type == typeof(long) || type == typeof(ulong) || type == typeof(int) || type == typeof(uint))
            {
                value = (T)(object)remoteValue.LongValue;
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                value = (T)(object)remoteValue.DoubleValue;
            }
            else if (type == typeof(bool))
            {
                value = (T)(object)remoteValue.BooleanValue;
            }
            else
            {
                return false;
            }
            
            return true;
        }
        
        private bool TryGetValueInternal(string key, out RemoteConfigValue value) => _remoteValues.TryGetValue(key, out value) != false;

        private void InitializeValues(IReadOnlyDictionary<string, RemoteConfigValue> values, ConfigValueSource source)
        {
            foreach (var (key, data) in values)
            {
                TryAddOrReplace(key, data.StringValue, source);
            }
        }
        
        private void TryAddOrReplace(string key, string value, ConfigValueSource source)
        {
            if (_remoteValues.ContainsKey(key))
            {
                _remoteValues[key] = new RemoteConfigValue(Encoding.UTF8.GetBytes(value), source);
            }
            else
            {
                _remoteValues.Add(key, new RemoteConfigValue
                {
                    Data = Encoding.UTF8.GetBytes(value),
                    Source = source
                });
            }
        }
    }
}