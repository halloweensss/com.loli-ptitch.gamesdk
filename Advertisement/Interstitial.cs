using System;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Interstitial
    {
        public event Action OnShowed;
        public event Action OnClosed;
        public event Action OnShowFailed;
        public event Action OnError;
        public event Action OnClicked;
        
        internal Interstitial()
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
                        $"[GameSDK.Advertisement]: Before show interstitial, initialize the ads\nAds.Initialize()!");
                }
                
                return;
            }
            
            foreach (var service in Ads.Services)
            {
                try
                {
                    await service.Value.ShowInterstitial();
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
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

        internal void OnClosedHandler(PlatformServiceType platform)
        {
            OnClosed?.Invoke();
        }

        internal void OnShowFailedHandler(PlatformServiceType platform)
        {
            OnShowFailed?.Invoke();
        }
        
        internal void OnErrorHandler(PlatformServiceType platform)
        {
            OnError?.Invoke();
        }
        
        internal void OnClickedHandler(PlatformServiceType platform)
        {
            OnClicked?.Invoke();
        }
    }
}