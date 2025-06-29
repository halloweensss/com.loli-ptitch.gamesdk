using System;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IAdRevenueSource : IServiceProvider
    {
        event Action<AdRevenueData> OnAdRevenuePaid;
    }
}