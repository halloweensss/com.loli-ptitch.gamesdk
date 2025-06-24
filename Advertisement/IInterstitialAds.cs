using System;
using System.Threading.Tasks;
using GameSDK.Core;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IInterstitialAds : IServiceProvider
    {
        event Action<IInterstitialAds> OnShownInterstitial;
        event Action<IInterstitialAds> OnClosedInterstitial;
        event Action<IInterstitialAds> OnShownFailedInterstitial;
        event Action<IInterstitialAds> OnErrorInterstitial;
        event Action<IInterstitialAds> OnClickedInterstitial;

        Task ShowInterstitial();
    }
}