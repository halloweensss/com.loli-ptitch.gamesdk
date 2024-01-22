using System;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Banner
    {
        public event Action OnShowed;
        public event Action OnHidden;
        public event Action OnError;

        internal Banner()
        {
            
        }
        
        public async void Show()
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

            foreach (var service in Ads.Services)
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

                    OnErrorHandler(service.Key);
                    return;
                }
            }
        }
        
        public async void Hide()
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

            foreach (var service in Ads.Services)
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

                    OnErrorHandler(service.Key);
                    return;
                }
            }
        }

        internal void OnShowedHandler(PlatformServiceType platform)
        {
            OnShowed?.Invoke();
        }

        internal void OnHiddenHandler(PlatformServiceType platform)
        {
            OnHidden?.Invoke();
        }
        
        internal void OnErrorHandler(PlatformServiceType platform)
        {
            OnError?.Invoke();
        }
    }
}