﻿using System;
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
        
        private readonly Dictionary<string, IInterstitialAds> _services = new(2);
        
        internal Interstitial()
        {
            
        }

        public void Register(IInterstitialAds service)
        {
            if (_services.TryAdd(service.ServiceId, service) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Interstitial]: The platform {service.ServiceId} has already been registered!");

                return;
            }

            service.OnShownInterstitial += OnShowedHandler;
            service.OnClosedInterstitial += OnClosedHandler;
            service.OnShownFailedInterstitial += OnShowFailedHandler;
            service.OnErrorInterstitial += OnErrorHandler;
            service.OnClickedInterstitial += OnClickedHandler;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Interstitial]: Platform {service.ServiceId} is registered!");
        }
        
        public void Unregister(IInterstitialAds service)
        {
            if (_services.Remove(service.ServiceId) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Interstitial]: The platform {service.ServiceId} has not been registered!");

                return;
            }

            service.OnShownInterstitial -= OnShowedHandler;
            service.OnClosedInterstitial -= OnClosedHandler;
            service.OnShownFailedInterstitial -= OnShowFailedHandler;
            service.OnErrorInterstitial -= OnErrorHandler;
            service.OnClickedInterstitial -= OnClickedHandler;
            
            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Interstitial]: Platform {service.ServiceId} is unregistered!");
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

        private async void OnShowedHandler(IInterstitialAds platform)
        {
            try
            {
                OnShowed?.Invoke();

                await GameApp.GameStop();
            }
            catch (Exception e)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogError($"[GameSDK.Advertisement]: An show interstitial error has occurred {e.Message}!");
            }
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