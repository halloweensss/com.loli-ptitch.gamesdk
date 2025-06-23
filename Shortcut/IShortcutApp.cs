using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Shortcut
{
    public interface IShortcutApp : IGameSDKService
    {
        Task<bool> Create();
        Task<bool> CanCreate();
    }
}