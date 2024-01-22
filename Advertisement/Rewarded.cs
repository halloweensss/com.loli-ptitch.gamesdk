using System;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Rewarded
    {
        public event Action OnShowed;
        public event Action OnClosed;
        public event Action OnError;
        public event Action OnClicked;
        public event Action OnRewarded;

        internal Rewarded()
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
                        $"[GameSDK.Advertisement]: Before show rewarded, initialize the ads\nAds.Initialize()!");
                }

                return;
            }

            foreach (var service in Ads.Services)
            {
                try
                {
                    await service.Value.ShowRewarded();
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError(
                            $"[GameSDK.Advertisement]: An show rewarded error has occurred {e.Message}!");
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

        internal void OnRewardedHandler(PlatformServiceType platform)
        {
            OnRewarded?.Invoke();
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