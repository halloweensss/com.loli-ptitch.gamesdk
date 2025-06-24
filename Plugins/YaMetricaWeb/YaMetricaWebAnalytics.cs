using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Analytics;
using GameSDK.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace GameSDK.Plugins.YaMetricaWeb
{
    public class YaMetricaWebAnalytics : IAnalyticsApp
    {
        private static readonly YaMetricaWebAnalytics _instance = new();
        private readonly Dictionary<string, YaMetricaWebSettings> _settingsDictionary = new(2);

        private ConsentInfo _consentInfo;
        private YaMetricaWebSettings _mainSettings;
        private YaMetricaWebSettings[] _settings;

        private readonly YaMetricaErrorTracker _errorTracker;
        public string ServiceId => "YaMetricaWeb";
        public InitializationStatus InitializationStatus { get; private set; }
        public AnalyticsProviderType ProviderType => AnalyticsProviderType.YaGamesWeb;

        private YaMetricaWebAnalytics()
        {
            _errorTracker = new YaMetricaErrorTracker(this);
        }
        
        public async Task Initialize()
        {
            if (InitializationStatus == InitializationStatus.Initialized ||
                InitializationStatus == InitializationStatus.Waiting)
                return;

            await InitializeInternal();
        }

        public Task SetConsent(ConsentInfo consentInfo)
        {
            _consentInfo = consentInfo;
            return Task.CompletedTask;
        }

        public async Task<bool> SendEvent(string eventDataId, Dictionary<string, object> eventDataParameters)
        {
            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Analytics]: YaMetricaWeb is not initialized! Status: {InitializationStatus}");

                return false;
            }

            if (eventDataParameters == null)
                return await SendEvent(eventDataId);

            var json = JsonConvert.SerializeObject(eventDataParameters, Formatting.Indented);

#if !UNITY_EDITOR
            YaMetricaSendParamsEvent(eventDataId, json);
#else

#endif

            if (GameApp.IsDebugMode)
                Debug.Log(
                    $"[GameSDK.Analytics]: Send event {eventDataId} to YaMetricaWeb with parameters:\n {json}");

            return true;
        }

        public Task<bool> SendEvent(string eventDataId)
        {
            if (InitializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Analytics]: YaMetricaWeb is not initialized! Status: {InitializationStatus}");

                return Task.FromResult(false);
            }

#if !UNITY_EDITOR
            YaMetricaSendEvent(eventDataId);
#else

#endif
            if (GameApp.IsDebugMode)
                Debug.Log(
                    $"[GameSDK.Analytics]: Send event {eventDataId} to YaMetricaWeb");

            return Task.FromResult(true);
        }

        private async Task InitializeInternal()
        {
            _settings = Resources.LoadAll<YaMetricaWebSettings>(string.Empty);

            if (_settings.Length == 0)
            {
                InitializationStatus = InitializationStatus.Error;

                if (GameApp.IsDebugMode)
                    Debug.LogError("[GameSDK.Analytics]: YaMetricaWeb settings not found!");

                return;
            }

            _settingsDictionary.Clear();

            foreach (var setting in _settings)
                _settingsDictionary.Add(setting.CounterId, setting);

            _mainSettings = _settings[0];

#if !UNITY_EDITOR
            var json = _mainSettings.GetInitializeParameters();
            InitializationStatus = InitializationStatus.Waiting;

            YaMetricaInitialize(json, Callback);

            while (InitializationStatus == InitializationStatus.Waiting) 
                await Task.Yield();
#else
            InitializationStatus = InitializationStatus.Waiting;
            Callback();
            await Task.CompletedTask;
#endif

            if (_mainSettings.EnabledErrorTracking)
                _errorTracker.Initialize();
            
            [MonoPInvokeCallback(typeof(Action))]
            static void Callback()
            {
                _instance.InitializationStatus = InitializationStatus.Initialized;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Analytics.Analytics.Register(_instance);
        }

        [DllImport("__Internal")]
        private static extern void YaMetricaInitialize(string jsonData, Action callback);

        [DllImport("__Internal")]
        private static extern void YaMetricaSendParamsEvent(string eventName, string jsonData);

        [DllImport("__Internal")]
        private static extern void YaMetricaSendEvent(string eventName);
    }
}