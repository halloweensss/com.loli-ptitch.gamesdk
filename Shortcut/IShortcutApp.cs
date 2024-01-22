using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Shortcut
{
    internal interface IShortcutApp : IGameSDKService
    {
        Task<bool> Create();
        Task<bool> CanCreate();
    }
}