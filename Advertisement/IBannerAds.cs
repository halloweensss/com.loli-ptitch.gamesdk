using System;
using System.Threading.Tasks;
using GameSDK.Core;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IBannerAds : IServiceProvider
    {
        event Action<IBannerAds> OnShownBanner;
        event Action<IBannerAds> OnHiddenBanner;
        event Action<IBannerAds> OnErrorBanner;
        Task ShowBanner(BannerPosition position = BannerPosition.None, string placement = null);
        void LoadBanner();
        bool IsLoadedBanner(string placement = null);
        double GetBannerEcpm(string placement = null);
        Task HideBanner();
    }
}