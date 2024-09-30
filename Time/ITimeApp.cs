using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Time
{
    internal interface ITimeApp : IGameSDKService
    {
        Task<long> GetTimestamp();
    }
}