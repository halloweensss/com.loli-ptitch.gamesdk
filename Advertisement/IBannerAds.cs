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
        Task ShowBanner();
        Task HideBanner();
    }
}