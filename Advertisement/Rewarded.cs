using System;
using System.Collections.Generic;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Rewarded
    {
        private readonly Dictionary<string, IRewardedAds> _services = new(2);

        internal Rewarded()
        {
        }

        public event Action OnShowed;
        public event Action OnClosed;
        public event Action OnError;

        public event Action OnShowFailed;
        public event Action OnClicked;
        public event Action OnRewarded;

        public void Register(IRewardedAds service)
        {
            if (_services.TryAdd(service.ServiceId, service) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Rewarded]: The platform {service.ServiceId} has already been registered!");

                return;
            }

            service.OnShownRewarded += OnShowedHandler;
            service.OnClosedRewarded += OnClosedHandler;
            service.OnShownFailedRewarded += OnShowFailedHandler;
            service.OnErrorRewarded += OnErrorHandler;
            service.OnClickedRewarded += OnClickedHandler;
            service.OnRewardedRewarded += OnRewardedHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Rewarded]: Platform {service.ServiceId} is registered!");
        }

        public void Unregister(IRewardedAds service)
        {
            if (_services.Remove(service.ServiceId) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Rewarded]: The platform {service.ServiceId} has not been registered!");

                return;
            }

            service.OnShownRewarded -= OnShowedHandler;
            service.OnClosedRewarded -= OnClosedHandler;
            service.OnShownFailedRewarded -= OnShowFailedHandler;
            service.OnErrorRewarded -= OnErrorHandler;
            service.OnClickedRewarded -= OnClickedHandler;
            service.OnRewardedRewarded -= OnRewardedHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Rewarded]: Platform {service.ServiceId} is unregistered!");
        }

        public async void Show()
        {
            try
            {
                if (Ads.IsInitialized == false)
                    await Ads.Initialize();

                if (Ads.IsInitialized == false)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogWarning(
                            "[GameSDK.Advertisement]: Before show rewarded, initialize the ads\nAds.Initialize()!");

                    return;
                }

                await GameApp.GameStop();

                foreach (var service in _services)
                    try
                    {
                        await service.Value.ShowRewarded();
                    }
                    catch (Exception e)
                    {
                        if (GameApp.IsDebugMode)
                            Debug.LogError(
                                $"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");

                        OnErrorHandler(service.Value);
                        return;
                    }
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");
            }
        }

        internal void OnShowedHandler(IRewardedAds platform)
        {
            OnShowed?.Invoke();
        }

        internal async void OnClosedHandler(IRewardedAds platform)
        {
            try
            {
                OnClosed?.Invoke();

                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");
            }
        }

        internal void OnRewardedHandler(IRewardedAds platform)
        {
            OnRewarded?.Invoke();
        }

        internal async void OnErrorHandler(IRewardedAds platform)
        {
            try
            {
                OnError?.Invoke();

                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");
            }
        }

        private async void OnShowFailedHandler(IRewardedAds platform)
        {
            try
            {
                OnShowFailed?.Invoke();

                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");
            }
        }

        private void OnClickedHandler(IRewardedAds platform)
        {
            OnClicked?.Invoke();
        }
    }
}