using System;
using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Authentication
{
    internal interface IAuthApp : IGameSDKService
    {
        string Id { get; }
        string Name { get; }
        SignInType SignInType { get; }
        Task SignIn();
        Task SignInAsGuest();
    }
}