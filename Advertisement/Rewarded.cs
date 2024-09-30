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

            await GameApp.GameStop();

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

        internal async void OnClosedHandler(PlatformServiceType platform)
        {
            OnClosed?.Invoke();
            
            await GameApp.GameStart();
        }

        internal void OnRewardedHandler(PlatformServiceType platform)
        {
            OnRewarded?.Invoke();
        }

        internal async void OnErrorHandler(PlatformServiceType platform)
        {
            OnError?.Invoke();
            
            await GameApp.GameStart();
        }

        internal void OnClickedHandler(PlatformServiceType platform)
        {
            OnClicked?.Invoke();
        }
    }
}