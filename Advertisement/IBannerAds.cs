using System;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Advertisement
{
    public interface IBannerAds : IGameSDKService
    {
        event Action<IBannerAds> OnShownBanner;
        event Action<IBannerAds> OnHiddenBanner;
        event Action<IBannerAds> OnErrorBanner;
        Task ShowBanner();
        Task HideBanner();
    }
}