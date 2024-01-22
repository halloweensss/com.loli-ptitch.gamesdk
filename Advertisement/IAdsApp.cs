using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Advertisement
{
    internal interface IAdsApp : IGameSDKService
    {
        Task Initialize();
        Task ShowInterstitial();
        Task ShowRewarded();
        Task ShowBanner();
        Task HideBanner();
    }
}