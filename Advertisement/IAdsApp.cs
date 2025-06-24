using System;
using System.Threading.Tasks;
using GameSDK.Core;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IAdsApp : IServiceProvider
    {
        Task Initialize();
        Task ShowInterstitial();
        Task ShowRewarded();
        Task ShowBanner();
        Task HideBanner();
    }
}