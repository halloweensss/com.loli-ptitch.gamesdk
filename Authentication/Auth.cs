using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.Authentication
{
    public class Auth
    {
        private static readonly Auth Instance = new();

        private readonly Dictionary<PlatformServiceType, IAuthApp> _services = new(2);
        private InitializationStatus _initializationStatus = InitializationStatus.None;
        private SignInType _signInType = SignInType.None;

        public static bool IsAuthorized => Instance._initializationStatus == InitializationStatus.Initialized &&
                                           Instance._signInType != SignInType.None;

        public static SignInType SignInType => Instance._signInType;
        public static string Id => Instance.GetId();
        public static string Name => Instance.GetName();
        public static PayingStatusType PayingStatus => Instance.GetPayingStatus();

        public static event Action OnInitialized;
        public static event Action<SignInType> OnSignIn;
        public static event Action OnInitializeError;

        public static void Register(IAuthApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IAuthApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Authentication]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Authentication]: Platform {app.PlatformService} is registered!");
        }

        private string GetId()
        {
            if (_services.Count > 0)
                return _services.First().Value.Id;

            return string.Empty;
        }

        private string GetName()
        {
            if (_services.Count > 0)
                return _services.First().Value.Name;

            return string.Empty;
        }

        public static async Task SignIn()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            if (Instance._signInType == SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Authentication]: You have already been logged in!");

                return;
            }

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.SignIn();

                    if (service.Value.InitializationStatus != InitializationStatus.Initialized)
                    {
                        Instance._initializationStatus = service.Value.InitializationStatus;
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Authentication]: An authorization error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._signInType = SignInType.Account;

            foreach (var service in Instance._services)
            {
                var signInType = service.Value.SignInType;
                if (signInType < Instance._signInType)
                    Instance._signInType = signInType;
            }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
            OnSignIn?.Invoke(Instance._signInType);
        }

        public static async Task SignInAsGuest()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            if (Instance._signInType is SignInType.Guest or SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Authentication]: You have already been logged in!");

                return;
            }

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.SignInAsGuest();

                    if (service.Value.InitializationStatus != InitializationStatus.Initialized)
                    {
                        Instance._initializationStatus = service.Value.InitializationStatus;
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Authentication]: An authorization error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._signInType = SignInType.Guest;

            foreach (var service in Instance._services)
            {
                var signInType = service.Value.SignInType;
                if (signInType > Instance._signInType)
                    Instance._signInType = signInType;
            }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
            OnSignIn?.Invoke(Instance._signInType);
        }

        public static async Task<string> GetAvatar(AvatarSizeType size)
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");

                return string.Empty;
            }

            if (Instance._signInType == SignInType.None)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Authentication]: You are not logged in!");

                return string.Empty;
            }

            foreach (var service in Instance._services)
            {
                var avatar = await service.Value.GetAvatar(size);

                if (string.IsNullOrEmpty(avatar) == false)
                    return avatar;
            }

            return string.Empty;
        }

        public PayingStatusType GetPayingStatus()
        {
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");

                return PayingStatusType.None;
            }

            if (Instance._signInType == SignInType.None)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Authentication]: You are not logged in!");

                return PayingStatusType.None;
            }

            foreach (var service in Instance._services)
            {
                var payingStatus = service.Value.PayingStatus;

                if (payingStatus != PayingStatusType.None)
                    return payingStatus;
            }

            return PayingStatusType.None;
        }
    }
}