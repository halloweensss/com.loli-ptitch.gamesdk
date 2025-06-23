using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.GameFeedback
{
    public class Feedback
    {
        private static readonly Feedback Instance = new ();
        private readonly Dictionary<PlatformServiceType, IFeedbackApp> _services = new(2);
        private ReviewStatus _status;
        public static ReviewStatus Status => Instance._status;

        public static void Register(IFeedbackApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IFeedbackApp app)
        {
            if (_services.TryAdd(app.PlatformService, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Feedback]: The platform {app.PlatformService} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Feedback]: Platform {app.PlatformService} is registered!");
        }

        public static async Task<(bool, FailReviewReason)> CanReview()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Feedback]: Before getting the opportunity to review, initialize the sdk\nGameApp.Initialize()!");

                return (false, FailReviewReason.NoAuth);
            }

            Instance._status = ReviewStatus.Waiting;

            var reviews = new List<(bool, FailReviewReason)>();

            foreach (var service in Instance._services)
                try
                {
                    reviews.Add(await service.Value.CanReview());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Feedback]: An get possible review error has occurred {e.Message}!");

                    Instance._status = ReviewStatus.Error;
                    return (false, FailReviewReason.Unknown);
                }

            if (reviews.Count == 0)
                return (false, FailReviewReason.Unknown);

            foreach (var review in reviews)
                if (review.Item1)
                {
                    Instance._status = ReviewStatus.Success;
                    return (true, FailReviewReason.Unknown);
                }

            return (false, reviews[0].Item2);
        }

        public static async Task<(bool, FailReviewReason)> RequestReview()
        {
            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Feedback]: Before request review, initialize the sdk\nGameApp.Initialize()!");

                return (false, FailReviewReason.NoAuth);
            }

            Instance._status = ReviewStatus.Waiting;

            var reviews = new List<(bool, FailReviewReason)>();

            foreach (var service in Instance._services)
                try
                {
                    reviews.Add(await service.Value.RequestReview());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Feedback]: An request review error has occurred {e.Message}!");

                    Instance._status = ReviewStatus.Error;
                    return (false, FailReviewReason.Unknown);
                }

            if (reviews.Count == 0)
                return (false, FailReviewReason.Unknown);

            foreach (var review in reviews)
                if (review.Item1)
                {
                    Instance._status = ReviewStatus.Success;
                    return (true, FailReviewReason.Unknown);
                }

            return (false, reviews[0].Item2);
        }
    }
}