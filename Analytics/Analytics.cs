using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;
using UnityEngine.Pool;

namespace GameSDK.Analytics
{
    public sealed class Analytics
    {
        private static readonly Analytics Instance = new();
        private readonly LinkedList<EventData> _cachedEvents = new();
        private readonly ObjectPool<EventData> _eventDataPool;
        private readonly Dictionary<AnalyticsProviderType, IAnalyticsApp> _services = new();
        private ConsentInfo _consentInfo = new() { IsConsentGranted = false };
        private InitializationStatus _initializationStatus = InitializationStatus.None;
        private AnalyticsProviderType _registeredProviders = AnalyticsProviderType.None;

        private Analytics()
        {
            _eventDataPool = new ObjectPool<EventData>(EventData.Create, EventData.Clear, defaultCapacity: 32);
        }

        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public static event Action OnInitialized;
        public static event Action OnInitializeError;

        public static void Register(IAnalyticsApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IAnalyticsApp app)
        {
            if (_services.TryAdd(app.ProviderType, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Analytics]: The platform {app.ProviderType} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Analytics]: Platform {app.ProviderType} is registered!");

            _registeredProviders |= app.ProviderType;
        }

        public static async Task Initialize()
        {
            await Instance.InitializeInternal();
        }

        public static async Task SetConsent(ConsentInfo consentInfo)
        {
            await Instance.SetConsentInternal(consentInfo);
        }

        public static async Task<bool> SendEvent(string key, Dictionary<string, object> parameters = null,
            AnalyticsProviderType providers = AnalyticsProviderType.Default)
        {
            return await Instance.SendEventInternal(key, parameters, providers);
        }

        private async Task InitializeInternal()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Analytics]: SDK has already been initialized!");

                return;
            }

            _initializationStatus = InitializationStatus.Waiting;

            foreach (var service in _services)
                try
                {
                    await service.Value.Initialize();

                    if (service.Value.InitializationStatus == InitializationStatus.Initialized) continue;

                    _initializationStatus = service.Value.InitializationStatus;
                    return;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Analytics]: An initialize SDK error has occurred {e.Message}!");

                    _initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            _initializationStatus = InitializationStatus.Initialized;

            if (GameApp.IsDebugMode)
                Debug.Log("[GameSDK.Analytics]: SDK has been initialized!");

            OnInitialized?.Invoke();
        }

        private async Task SetConsentInternal(ConsentInfo consentInfo)
        {
            _consentInfo = consentInfo;

            foreach (var service in _services)
                await service.Value.SetConsent(consentInfo);

            if (_consentInfo.IsConsentGranted == false)
                return;

            await FlushEventsInternal();
        }

        private async Task<bool> SendEventInternal(string key, Dictionary<string, object> parameters,
            AnalyticsProviderType provider = AnalyticsProviderType.Default)
        {
            var node = CreateElement(key, parameters, provider);

            if (_consentInfo.IsConsentGranted == false)
                return false;

            return await SendEventInternal(node);
        }

        private LinkedListNode<EventData> CreateElement(string key, Dictionary<string, object> parameters,
            AnalyticsProviderType provider = AnalyticsProviderType.Default)
        {
            var eventData = _eventDataPool.Get();

            if (provider == AnalyticsProviderType.Default)
                provider = _registeredProviders;

            eventData.Initialize(key, parameters, provider);
            var node = _cachedEvents.AddLast(eventData);

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Analytics]: Event {key} has been created!");

            return node;
        }

        private void ClearElement(LinkedListNode<EventData> node)
        {
            _cachedEvents.Remove(node);
            _eventDataPool.Release(node.Value);
        }

        private async Task<bool> SendEventInternal(LinkedListNode<EventData> node)
        {
            if (_initializationStatus != InitializationStatus.Initialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError("[GameSDK.Analytics]: SDK is not initialized! Status: " + _initializationStatus);

                return false;
            }

            var eventData = node.Value;

            var dispatchedProviders = AnalyticsProviderType.None;
            var providers = eventData.Providers;

            foreach (var (providerType, service) in _services)
            {
                if (service.InitializationStatus != InitializationStatus.Initialized)
                    continue;

                if ((providers & providerType) == 0)
                    continue;

                var sendResult = false;

                var parameters = eventData.Parameters;

                if (parameters != null && parameters.Count > 0)
                    sendResult = await service.SendEvent(eventData.Id, eventData.Parameters);
                else
                    sendResult = await service.SendEvent(eventData.Id);

                if (sendResult == false)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogWarning(
                            $"[GameSDK.Analytics]: Event {eventData.Id} has not been sent to {providerType} provider!");

                    continue;
                }

                eventData.RemoveProvider(providerType);
                dispatchedProviders |= providerType;
            }

            var result = eventData.Providers == AnalyticsProviderType.None;

            if (result)
            {
                ClearElement(node);

                if (GameApp.IsDebugMode)
                    Debug.Log(
                        $"[GameSDK.Analytics]: Event {eventData.Id} has been sent successfully to {dispatchedProviders}!");
            }
            else
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Analytics]: Event {eventData.Id} has not been sent to {providers} left providers {providers}!");
            }

            return result;
        }

        private async Task FlushEventsInternal()
        {
            var currentNode = _cachedEvents.First;

            while (currentNode != null)
            {
                var next = currentNode.Next;
                await SendEventInternal(currentNode);

                currentNode = next;

                await Task.Yield();
            }
        }
    }
}