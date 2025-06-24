using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.GameStorage
{
    public interface IStorageApp : IServiceProvider
    {
        Task<StorageStatus> Save(string key, string value);
        Task<(StorageStatus, string)> Load(string key);
    }
}