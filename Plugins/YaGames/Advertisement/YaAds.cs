using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Advertisement;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.Plugins.YaGames.Advertisement
{
    public class YaAds : IAdsApp
    {
        private static readonly YaAds _instance = new YaAds();
        
        private InitializationStatus _status = InitializationStatus.None;
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => _status;
        public Task Initialize()
        {
            _status = InitializationStatus.Initialized;
            return Task.CompletedTask;
        }

        public Task ShowInterstitial()
        {
#if !UNITY_EDITOR
            YaGamesShowInterstitial(OnOpen, OnClose, OnError, OnOffline);
#else
            OnOpen();
            OnClose(true);
#endif
            return Task.CompletedTask;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Ads.Interstitial.OnShowedHandler(_instance.PlatformService);
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement interstitial opened!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnClose(bool wasShown)
            {
                if (wasShown)
                {
                    Ads.Interstitial.OnClosedHandler(_instance.PlatformService);
                    
                    if (GameApp.IsDebugMode)
                    {
                        Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement interstitial closed!");
                    }
                }
                else
                {
                    Ads.Interstitial.OnShowFailedHandler(_instance.PlatformService);
                    
                    if (GameApp.IsDebugMode)
                    {
                        Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad!");
                    }
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(string error)
            {
                Ads.Interstitial.OnErrorHandler(_instance.PlatformService);
                    
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad with error {error}!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnOffline()
            {
                Ads.Interstitial.OnErrorHandler(_instance.PlatformService);
                    
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an ad, the player switched to offline mode!");
                }
            }
        }

        public Task ShowRewarded()
        {
#if !UNITY_EDITOR
            YaGamesShowRewarded(OnOpen, OnClose, OnError, OnRewarded);
#else
            OnOpen();
            OnRewarded();
            OnClose();
#endif
            return Task.CompletedTask;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Ads.Rewarded.OnShowedHandler(_instance.PlatformService);
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement rewarded opened!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnClose()
            {
                Ads.Rewarded.OnClosedHandler(_instance.PlatformService);
                    
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement rewarded closed!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(string error)
            {
                Ads.Rewarded.OnErrorHandler(_instance.PlatformService);
                    
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an rewarded ad with error {error}!");
                }
            }
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnRewarded()
            {
                Ads.Rewarded.OnRewardedHandler(_instance.PlatformService);
                    
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement you can get a reward for watching a video!");
                }
            }
        }

        public Task ShowBanner()
        {
#if !UNITY_EDITOR
            YaGamesShowBanner(OnOpen, OnError);
#else
            OnOpen();
#endif
            return Task.CompletedTask;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnOpen()
            {
                Ads.Banner.OnShowedHandler(_instance.PlatformService);
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement banner opened!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(int error)
            {
                Ads.Banner.OnErrorHandler(_instance.PlatformService);

                var type = (BannerErrors)error;
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while displaying an banner ad with error {type}!");
                }
            }
        }
        
        public Task HideBanner()
        {
#if !UNITY_EDITOR
            YaGamesHideBanner(OnHided, OnError);
#else
            OnHided();
#endif
            return Task.CompletedTask;
            
            [MonoPInvokeCallback(typeof(Action))]
            static void OnHided()
            {
                Ads.Banner.OnHiddenHandler(_instance.PlatformService);
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement banner is hidden!");
                }
            }

            [MonoPInvokeCallback(typeof(Action))]
            static void OnError(int error)
            {
                Ads.Banner.OnErrorHandler(_instance.PlatformService);

                var type = (BannerErrors)error;
                
                if (GameApp.IsDebugMode)
                {
                    Debug.Log($"[GameSDK.Advertisement]: YaAdvertisement an error occurred while hidden an banner ad with error {type}!");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            Ads.Instance.Register(_instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaGamesShowInterstitial(Action onOpen, Action<bool> onClose, Action<string> onError, Action onOffline);
        [DllImport("__Internal")]
        private static extern void YaGamesShowRewarded(Action onOpen, Action onClose, Action<string> onError, Action onRewarded);
        [DllImport("__Internal")]
        private static extern void YaGamesShowBanner(Action onOpen, Action<int> onError);
        [DllImport("__Internal")]
        private static extern void YaGamesHideBanner(Action onHided, Action<int> onError);
    }
}