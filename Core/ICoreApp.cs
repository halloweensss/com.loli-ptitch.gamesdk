using System.Threading.Tasks;

namespace GameSDK.Core
{
    public interface ICoreApp : IGameSDKService
    {
        DeviceType DeviceType { get; }
        string AppId { get; }
        string Lang { get; }
        string Payload { get; }
        bool IsReady { get; }
        bool IsStarted { get; }
        Task Initialize();
        Task Ready();
        Task Start();
        Task Stop();
    }
}