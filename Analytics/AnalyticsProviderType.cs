using System;

namespace GameSDK.Analytics
{
    [Flags]
    public enum AnalyticsProviderType
    {
        Default = -1,
        None = 0,
        YaGamesWeb = 1,
    }
}