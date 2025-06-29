using System;
using System.Threading.Tasks;
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
        event Action<IRewardedAds> OnLoadedRewarded;
        event Action<IRewardedAds> OnFailedToLoadRewarded;
        
        Task ShowRewarded(string placement = null);
        void LoadRewarded();
        bool IsLoadedRewarded(string placement = null);
        double GetRewardedEcpm(string placement = null);
    }
}