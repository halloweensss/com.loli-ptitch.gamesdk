using System;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Advertisement
{
    public interface IRewardedAds : IGameSDKService
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