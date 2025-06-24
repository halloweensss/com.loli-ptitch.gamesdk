using System;
using System.Threading.Tasks;
using GameSDK.Core;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IRewardedAds : IServiceProvider
    {
        event Action<IRewardedAds> OnShownRewarded;
        event Action<IRewardedAds> OnClosedRewarded;
        event Action<IRewardedAds> OnShownFailedRewarded;
        event Action<IRewardedAds> OnErrorRewarded;
        event Action<IRewardedAds> OnClickedRewarded;
        event Action<IRewardedAds> OnRewardedRewarded;
        
        Task ShowRewarded();
    }
}