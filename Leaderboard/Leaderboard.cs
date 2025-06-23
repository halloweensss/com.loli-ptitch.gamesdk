using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.Leaderboard
{
    public class Leaderboard
    {
        private static readonly Leaderboard Instance = new();

        private readonly Dictionary<PlatformServiceType, ILeaderboardApp> _services = new(2);

        private InitializationStatus _initializationStatus = InitializationStatus.None;
        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public static event Action OnInitialized;
        public static event Action OnInitializeError;

        public static void Register(ILeaderboardApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(ILeaderboardApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Leaderboard]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Leaderboard]: Platform {app.PlatformService} is registered!");
        }

        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Leaderboard]: SDK has already been initialized!");

                return;
            }

            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before initialize ads, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            Instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.Initialize();
                    if (service.Value.InitializationStatus == InitializationStatus.Initialized) continue;

                    Instance._initializationStatus = service.Value.InitializationStatus;
                    return;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Leaderboard]: An initialize SDK error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }

        public static async Task<LeaderboardDescription> GetDescription(string id)
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before get description leaderboard, initialize the leaderboards\nLeaderboard.Initialize()!");

                return null;
            }

            var descriptions = new List<LeaderboardDescription>();
            foreach (var service in Instance._services)
                try
                {
                    var description = await service.Value.GetDescription(id);
                    if (description != null)
                        descriptions.Add(description);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError(
                            $"[GameSDK.Leaderboard]: An get leaderboard description error has occurred {e.Message}!");

                    return null;
                }

            if (descriptions.Count > 0)
                return descriptions.First();

            if (GameApp.IsDebugMode)
                Debug.LogError("[GameSDK.Leaderboard]: No leaderboard description found!");

            return null;
        }

        public static async Task SetScore(string id, int score)
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before set score leaderboard, initialize the leaderboards\nLeaderboard.Initialize()!");

                return;
            }

            if (Auth.SignInType != SignInType.Account)
                await Auth.SignIn();

            if (Auth.SignInType != SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before set score leaderboard, sign in to account\nAuth.SignIn()!");

                return;
            }

            foreach (var service in Instance._services)
                try
                {
                    var status = await service.Value.SetScore(id, score);
                    if (status != LeaderboardStatus.Success)
                        if (GameApp.IsDebugMode)
                            Debug.LogError($"[GameSDK.Leaderboard]: Error writing data to the leaderboard {id}!");
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Leaderboard]: Error writing data to the leaderboard {e.Message}!");

                    return;
                }
        }

        public static async Task<(bool, LeaderboardPlayerData)> GetPlayerData(string id)
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before get player data leaderboard, initialize the leaderboards\nLeaderboard.Initialize()!");

                return (false, null);
            }

            if (Auth.SignInType != SignInType.Account)
                await Auth.SignIn();

            if (Auth.SignInType != SignInType.Account)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before get player data leaderboard, sign in to account\nAuth.SignIn()!");

                return (false, null);
            }

            var results =
                new List<(LeaderboardStatus, LeaderboardPlayerData)>();
            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.GetPlayerData(id);
                    if (result.Item1 != LeaderboardStatus.Success)
                        if (GameApp.IsDebugMode)
                            Debug.LogError($"[GameSDK.Leaderboard]: Error getting data from the leaderboard {id}!");

                    results.Add(result);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Leaderboard]: Error getting data from the leaderboard {e.Message}!");

                    return (false, null);
                }

            foreach (var result in results)
                if (result.Item1 == LeaderboardStatus.Success)
                    return (true, result.Item2);

            return (false, null);
        }

        public static async Task<(bool, LeaderboardEntries)> GetEntries(LeaderboardParameters parameters)
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Leaderboard]: Before get entries leaderboard, initialize the leaderboards\nLeaderboard.Initialize()!");

                return (false, null);
            }

            var results =
                new List<(LeaderboardStatus, LeaderboardEntries)>();
            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.GetEntries(parameters);
                    if (result.Item1 != LeaderboardStatus.Success)
                        if (GameApp.IsDebugMode)
                            Debug.LogError(
                                $"[GameSDK.Leaderboard]: Error getting entries from the leaderboard {parameters.id}!");

                    results.Add(result);
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError(
                            $"[GameSDK.Leaderboard]: Error getting entries from the leaderboard {e.Message}!");

                    return (false, null);
                }

            foreach (var result in results)
                if (result.Item1 == LeaderboardStatus.Success)
                    return (true, result.Item2);

            return (false, null);
        }
    }
}