using System;
using System.Collections.Generic;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Banner
    {
        public event Action OnShowed;
        public event Action OnHidden;
        public event Action OnError;
        
        private readonly Dictionary<string, IBannerAds> _services = new(2);
        
        internal Banner()
        {
            
        }
        
        public void Register(IBannerAds service)
        {
            if (_services.TryAdd(service.ServiceId, service) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Banner]: The platform {service.ServiceId} has already been registered!");

                return;
            }
            
            service.OnShownBanner += OnShowedHandler;
            service.OnHiddenBanner += OnHiddenHandler;
            service.OnErrorBanner += OnErrorHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Banner]: Platform {service.ServiceId} is registered!");
        }
        
        public void Unregister(IBannerAds service)
        {
            if (_services.Remove(service.ServiceId) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Banner]: The platform {service.ServiceId} has not been registered!");

                return;
            }
            
            service.OnShownBanner -= OnShowedHandler;
            service.OnHiddenBanner -= OnHiddenHandler;
            service.OnErrorBanner -= OnErrorHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Banner]: Platform {service.ServiceId} is unregistered!");
        }
        
        public async void Show()
        {
            try
            {
                if (Ads.IsInitialized == false)
                {
                    await Ads.Initialize();
                }

                if (Ads.IsInitialized == false)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogWarning(
                            $"[GameSDK.Advertisement]: Before show banner, initialize the ads\nAds.Initialize()!");
                    }

                    return;
                }

                foreach (var service in _services)
                {
                    try
                    {
                        await service.Value.ShowBanner();
                    }
                    catch (Exception e)
                    {
                        if (GameApp.IsDebugMode)
                        {
                            Debug.LogError(
                                $"[GameSDK.Advertisement]: An show banner error has occurred {e.Message}!");
                        }

                        OnErrorHandler(service.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show banner error has occurred {e.Message}!");
            }
        }
        
        public async void Hide()
        {
            try
            {
                if (Ads.IsInitialized == false)
                {
                    await Ads.Initialize();
                }

                if (Ads.IsInitialized == false)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogWarning(
                            $"[GameSDK.Advertisement]: Before hide banner, initialize the ads\nAds.Initialize()!");
                    }

                    return;
                }

                foreach (var service in _services)
                {
                    try
                    {
                        await service.Value.HideBanner();
                    }
                    catch (Exception e)
                    {
                        if (GameApp.IsDebugMode)
                        {
                            Debug.LogError(
                                $"[GameSDK.Advertisement]: An hide banner error has occurred {e.Message}!");
                        }

                        OnErrorHandler(service.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An hide banner error has occurred {e.Message}!");
            }
        }

        internal void OnShowedHandler(IBannerAds platform)
        {
            OnShowed?.Invoke();
        }

        internal void OnHiddenHandler(IBannerAds platform)
        {
            OnHidden?.Invoke();
        }
        
        internal void OnErrorHandler(IBannerAds platform)
        {
            OnError?.Invoke();
        }
    }
}