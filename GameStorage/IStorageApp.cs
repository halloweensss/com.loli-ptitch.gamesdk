using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.GameStorage
{
    public interface IStorageApp : IGameSDKService
    {
        Task<StorageStatus> Save(string key, string value);
        Task<(StorageStatus, string)> Load(string key);
    }
}