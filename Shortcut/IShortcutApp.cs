using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Shortcut
{
    public interface IShortcutApp : IServiceProvider
    {
        Task<bool> Create();
        Task<bool> CanCreate();
    }
}