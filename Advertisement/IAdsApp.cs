using System;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Advertisement
{
    public interface IAdsApp : IGameSDKService
    {
        Task Initialize();
        Task ShowInterstitial();
        Task ShowRewarded();
        Task ShowBanner();
        Task HideBanner();
    }
}