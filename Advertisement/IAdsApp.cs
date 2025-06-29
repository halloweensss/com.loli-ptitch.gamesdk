using System.Threading.Tasks;
using IServiceProvider = GameSDK.Core.IServiceProvider;

namespace GameSDK.Advertisement
{
    public interface IAdsApp : IServiceProvider
    {
        Task Initialize();
    }
}