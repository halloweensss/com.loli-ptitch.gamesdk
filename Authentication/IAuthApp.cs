using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Authentication
{
    internal interface IAuthApp : IGameSDKService
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