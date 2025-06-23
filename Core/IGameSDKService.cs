using GameSDK.Core.Properties;

namespace GameSDK.Core
{
    public interface IGameSDKService
    {
        PlatformServiceType PlatformService { get; }
        InitializationStatus InitializationStatus { get; }
    }
}