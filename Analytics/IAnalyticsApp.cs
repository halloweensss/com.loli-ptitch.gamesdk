using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Analytics
{
    public interface IAnalyticsApp : IGameSDKService
    {
        AnalyticsProviderType ProviderType { get; }
        Task Initialize();
        Task SetConsent(ConsentInfo consentInfo);
        Task<bool> SendEvent(string eventDataId, Dictionary<string, object> eventDataParameters);
        Task<bool> SendEvent(string eventDataId);
    }
}