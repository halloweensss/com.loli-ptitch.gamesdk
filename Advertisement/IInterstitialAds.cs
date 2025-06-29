using System;
using System.Threading.Tasks;
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
        event Action<IInterstitialAds> OnLoadedInterstitial;
        event Action<IInterstitialAds> OnFailedToLoadInterstitial;

        Task ShowInterstitial(string placement = null);
        void LoadInterstitial();
        bool IsLoadedInterstitial(string placement = null);
        double GetInterstitialEcpm(string placement = null);

    }
}