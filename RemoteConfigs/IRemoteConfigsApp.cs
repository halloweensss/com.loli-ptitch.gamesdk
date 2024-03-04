using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.RemoteConfigs
{
    internal interface IRemoteConfigsApp : IGameSDKService
    {
        IReadOnlyDictionary<string, RemoteConfigValue> RemoteValues { get; }
        Task Initialize();
        Task InitializeWithUserParameters(params KeyValuePair<string, string>[] parameters);
    }
}