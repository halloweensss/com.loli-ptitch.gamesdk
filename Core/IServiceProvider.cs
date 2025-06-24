namespace GameSDK.Core
{
    public interface IServiceProvider
    {
        string ServiceId { get; }
        InitializationStatus InitializationStatus { get; }
    }
}