using GameSDK.Core.Properties;

namespace GameSDK.Core
{
    internal interface IGameSDKService
    {
        PlatformServiceType PlatformService { get; }
        InitializationStatus InitializationStatus { get; }
    }
}