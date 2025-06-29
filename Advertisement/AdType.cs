using System;

namespace GameSDK.Advertisement
{
    [Flags]
    public enum AdType
    {
        None = 0,
        Interstitial = 1,
        Banner = 2,
        RewardedVideo = 4,
        Mrec = 8
    }
}