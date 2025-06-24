using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Authentication
{
    public interface IAuthApp : IServiceProvider
    {
        string Id { get; }
        string Name { get; }
        SignInType SignInType { get; }
        PayingStatusType PayingStatus { get; }
        Task<string> GetAvatar(AvatarSizeType size);
        Task SignIn();
        Task SignInAsGuest();
    }
}