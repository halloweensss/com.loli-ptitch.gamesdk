using System;
using System.Collections.Generic;
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
        
        private readonly Dictionary<PlatformServiceType, IInterstitialAds> _services = new(2);
        
        internal Interstitial()
        {
            
        }

        public void Register(IInterstitialAds service)
        {
            if (_services.TryAdd(service.PlatformService, service) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Interstitial]: The platform {service.PlatformService} has already been registered!");

                return;
            }

            service.OnShownInterstitial += OnShowedHandler;
            service.OnClosedInterstitial += OnClosedHandler;
            service.OnShownFailedInterstitial += OnShowFailedHandler;
            service.OnErrorInterstitial += OnErrorHandler;
            service.OnClickedInterstitial += OnClickedHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Interstitial]: Platform {service.PlatformService} is registered!");
        }
        
        public void Unregister(IInterstitialAds service)
        {
            if (_services.Remove(service.PlatformService) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Interstitial]: The platform {service.PlatformService} has not been registered!");

                return;
            }

            service.OnShownInterstitial -= OnShowedHandler;
            service.OnClosedInterstitial -= OnClosedHandler;
            service.OnShownFailedInterstitial -= OnShowFailedHandler;
            service.OnErrorInterstitial -= OnErrorHandler;
            service.OnClickedInterstitial -= OnClickedHandler;
            
            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Interstitial]: Platform {service.PlatformService} is unregistered!");
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
                    {
                        Debug.LogWarning(
                            $"[GameSDK.Advertisement]: Before show interstitial, initialize the ads\nAds.Initialize()!");
                    }
                
                    return;
                }

                await GameApp.GameStop();
            
                foreach (var service in _services)
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

                        OnErrorHandler(service.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
            }
        }

        private void OnShowedHandler(IInterstitialAds platform)
        {
            OnShowed?.Invoke();
        }

        private async void OnClosedHandler(IInterstitialAds platform)
        {
            try
            {
                OnClosed?.Invoke();
            
                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
            }
        }

        private async void OnShowFailedHandler(IInterstitialAds platform)
        {
            try
            {
                OnShowFailed?.Invoke();
            
                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
            }
        }
        
        private async void OnErrorHandler(IInterstitialAds platform)
        {
            try
            {
                OnError?.Invoke();
            
                await GameApp.GameStart();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
            }
        }

        private void OnClickedHandler(IInterstitialAds platform)
        {
            OnClicked?.Invoke();
        }
    }
}