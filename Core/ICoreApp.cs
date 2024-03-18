using System.Threading.Tasks;

namespace GameSDK.Core
{
    internal interface ICoreApp : IGameSDKService
    {
        DeviceType DeviceType { get; }
        string AppId { get; }
        string Lang { get; }
        string Payload { get; }
        bool IsReady { get; }
        Task Initialize();
        Task Ready();
    }
}