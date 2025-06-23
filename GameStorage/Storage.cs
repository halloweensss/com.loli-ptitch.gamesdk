using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.GameStorage
{
    public class Storage
    {
        private static readonly Storage Instance = new ();

        private readonly Dictionary<PlatformServiceType, IStorageApp> _services = new(2);
        public static event Action<string> OnSaved;
        public static event Action<string> OnFailSaving;
        public static event Action<string, string> OnLoaded;
        public static event Action<string> OnFailLoading;

        public static void Register(IStorageApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IStorageApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Storage]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Storage]: Platform {app.PlatformService} is registered!");
        }

        public static async Task<StorageStatus> Save(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Storage]: The key cannot be empty");

                OnFailSaving?.Invoke(key);
                return StorageStatus.Error;
            }

            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Storage]: Before save, initialize the sdk\nGameApp.Initialize()!");

                OnFailSaving?.Invoke(key);
                return StorageStatus.Error;
            }

            var statuses = new List<StorageStatus>();

            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.Save(key, value);

                    if (GameApp.IsDebugMode)
                        Debug.Log($"[GameSDK.Storage]: Saving status {service.Key}: {result}");

                    statuses.Add(result);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Storage]: Saving occurred with an error {e.Message}!");
                }

            var isSaved = statuses.Any(status => status == StorageStatus.Success);

            switch (isSaved)
            {
                case true:
                    OnSaved?.Invoke(key);
                    return StorageStatus.Success;
                default:
                    OnFailSaving?.Invoke(key);
                    return StorageStatus.Error;
            }
        }

        public static async Task<(StorageStatus, string)> Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Storage]: The key cannot be empty");

                OnFailLoading?.Invoke(key);
                return (StorageStatus.Error, string.Empty);
            }

            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Storage]: Before load, initialize the sdk\nGameApp.Initialize()!");

                OnFailLoading?.Invoke(key);
                return (StorageStatus.Error, string.Empty);
            }

            var statuses = new List<(StorageStatus, string)>();

            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.Load(key);

                    if (GameApp.IsDebugMode)
                        Debug.Log($"[GameSDK.Storage]: Load result {service.Key}: {result}");

                    statuses.Add(result);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Storage]: Loading occurred with an error {e.Message}!");
                }

            var loadedData = statuses.FirstOrDefault(status => status.Item1 == StorageStatus.Success);

            if (loadedData.Equals(default))
            {
                OnFailLoading?.Invoke(key);
                return (StorageStatus.Error, string.Empty);
            }

            OnLoaded?.Invoke(key, loadedData.Item2);
            return loadedData;
        }
    }
}