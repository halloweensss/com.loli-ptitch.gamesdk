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
        private static Auth _instance;
        private InitializationStatus _initializationStatus = InitializationStatus.None;
        private SignInType _signInType = SignInType.None;
        
        private Dictionary<PlatformServiceType, IAuthApp> _services = new Dictionary<PlatformServiceType, IAuthApp>();
        internal static Auth Instance => _instance ??= new Auth();
        public static bool IsAuthorized => Instance._initializationStatus == InitializationStatus.Initialized && Instance._signInType != SignInType.None;
        public static SignInType SignInType => Instance._signInType;
        public static string Id => Instance.GetId();
        public static string Name => Instance.GetName();
        
        public static event Action OnInitialized;
        public static event Action<SignInType> OnSignIn;
        public static event Action OnInitializeError;

        internal void Register(IAuthApp app)
        {
            if (_services.ContainsKey(app.PlatformService))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Authentication]: Platform {app.PlatformService} is registered!");
            }
        }

        private string GetId()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.Id;
            }

            return string.Empty;
        }
        
        private string GetName()
        {
            if (_services.Count > 0)
            {
                return _services.First().Value.Name;
            }

            return string.Empty;
        }
        
        public static async Task SignIn()
        {
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            if (_instance._signInType == SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You have already been logged in!");
                }
                
                return;
            }

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.SignIn();

                    if (service.Value.InitializationStatus != InitializationStatus.Initialized)
                    {
                        _instance._initializationStatus = service.Value.InitializationStatus;
                        return;
                    }
                    
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Authentication]: An authorization error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._signInType = SignInType.Account;

            foreach (var service in _instance._services)
            {
                var signInType = service.Value.SignInType;
                if (signInType < _instance._signInType)
                {
                    _instance._signInType = signInType;
                }
            }
            
            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
            OnSignIn?.Invoke(_instance._signInType);
        }

        public static async Task SignInAsGuest()
        {
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Authentication]: Before logging in, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            if (_instance._signInType is SignInType.Guest or SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Authentication]: You have already been logged in!");
                }
                
                return;
            }

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.SignInAsGuest();

                    if (service.Value.InitializationStatus != InitializationStatus.Initialized)
                    {
                        _instance._initializationStatus = service.Value.InitializationStatus;
                        return;
                    }
                    
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Authentication]: An authorization error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._signInType = SignInType.Guest;
            
            foreach (var service in _instance._services)
            {
                var signInType = service.Value.SignInType;
                if (signInType > _instance._signInType)
                {
                    _instance._signInType = signInType;
                }
            }
            
            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
            OnSignIn?.Invoke(_instance._signInType);
        }
    }
}
