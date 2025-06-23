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
        private static readonly RemoteConfigs Instance = new();

        private readonly RemoteConfigInjector _injector;
        private readonly Dictionary<string, RemoteConfigValue> _remoteValues = new(16);

        private readonly Dictionary<PlatformServiceType, IRemoteConfigsApp> _services = new();
        private InitializationStatus _initializationStatus = InitializationStatus.None;

        private RemoteConfigs()
        {
            _injector = new RemoteConfigInjector(this);
        }

        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public static IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues => Instance._remoteValues;

        public static event Action OnInitialized;
        public static event Action OnInitializeError;

        public static void Register(IRemoteConfigsApp app) => Instance.RegisterInternal(app);
        
        private void RegisterInternal(IRemoteConfigsApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.RemoteConfigs]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.RemoteConfigs]: Platform {app.PlatformService} is registered!");
        }

        public static async Task Initialize()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.RemoteConfigs]: Before initialize remote configs, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            Instance._initializationStatus = InitializationStatus.Waiting;
            
            await Instance.InitializeDefaultConfigs();

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.Initialize();
                    Instance.InitializeValues(service.Value.RemoteValues, ConfigValueSource.RemoteValue);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.RemoteConfigs]: An initialize SDK error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }

        public static async Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters)
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.RemoteConfigs]: Before initialize remote configs, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            Instance._initializationStatus = InitializationStatus.Waiting;

            await Instance.InitializeDefaultConfigs();

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.InitializeWithUserParameters(parameters);
                    Instance.InitializeValues(service.Value.RemoteValues, ConfigValueSource.RemoteValue);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.RemoteConfigs]: An initialize SDK error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }

        public static void SetDefaultValue(string key, object value, bool updateRegisteredObjects = false)
        {
            Instance.SetDefaultValueInternal(key, value);

            if (updateRegisteredObjects)
                Instance._injector.UpdateValues(key);
        }

        public static bool TryGetValue<T>(string key, out T value) where T : unmanaged
        {
            return Instance.TryGetValueInternal(key, out value);
        }

        public static bool TryGetValue(string key, out RemoteConfigValue value)
        {
            return Instance.TryGetValueInternal(key, out value);
        }

        public static void Register(object target)
        {
            Instance._injector.Register(target);
        }

        public static void Register(params object[] targets)
        {
            Instance._injector.Register(targets);
        }

        private async Task InitializeDefaultConfigs()
        {
            var configs = Resources.LoadAll<DefaultRemoteValuesConfig>(string.Empty);
            await Task.Yield();

            foreach (var config in configs)
            {
                foreach (var value in config.DefaultValues)
                    SetDefaultValue(value.Key, value.Value);
                
                await Task.Yield();
            }
        }

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
                    var byteList = new List<byte>(bytes);
                    data = Encoding.UTF8.GetString(byteList.ToArray());
                    break;
                }
                case IEnumerable enumerable:
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var obj in enumerable)
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

            if (_remoteValues.TryGetValue(key, out var remoteValue))
            {
                if (remoteValue.Source == ConfigValueSource.DefaultValue)
                {
                    _remoteValues[key] =
                        new RemoteConfigValue(Encoding.UTF8.GetBytes(data), ConfigValueSource.DefaultValue);
                }
            }
            else
                _remoteValues.Add(key, new RemoteConfigValue
                {
                    Data = Encoding.UTF8.GetBytes(data),
                    Source = ConfigValueSource.DefaultValue
                });
        }

        private bool TryGetValueInternal<T>(string key, out T value) where T : unmanaged
        {
            value = default;

            if (_remoteValues.TryGetValue(key, out var remoteValue) == false)
                return false;

            return remoteValue.TryGetValue(out value);
        }

        internal bool TryGetValueInternal(string key, out RemoteConfigValue value)
        {
            return _remoteValues.TryGetValue(key, out value);
        }

        private void InitializeValues(IReadOnlyDictionary<string, RemoteConfigValue> values, ConfigValueSource source)
        {
            foreach (var (key, data) in values)
                TryAddOrReplace(key, data.StringValue, source);
        }

        private void TryAddOrReplace(string key, string value, ConfigValueSource source)
        {
            if (_remoteValues.ContainsKey(key))
                _remoteValues[key] = new RemoteConfigValue(Encoding.UTF8.GetBytes(value), source);
            else
                _remoteValues.Add(key, new RemoteConfigValue
                {
                    Data = Encoding.UTF8.GetBytes(value),
                    Source = source
                });

            _injector.UpdateValues(key);
        }
    }
}