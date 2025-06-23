using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Time
{
    public interface ITimeApp : IGameSDKService
    {
        Task<long> GetTimestamp();
    }
}