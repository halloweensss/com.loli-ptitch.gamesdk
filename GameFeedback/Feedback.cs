using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.Core.Properties;
using UnityEngine;

namespace GameSDK.GameFeedback
{
    public class Feedback
    {
        private static Feedback _instance;

        private ReviewStatus _status;

        private Dictionary<PlatformServiceType, IFeedbackApp> _services = new Dictionary<PlatformServiceType, IFeedbackApp>();
        internal static Feedback Instance => _instance ??= new Feedback();
        
        public static event Action OnCreated;

        internal void Register(IFeedbackApp app)
        {
            if (_services.ContainsKey(app.PlatformService))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Feedback]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Feedback]: Platform {app.PlatformService} is registered!");
            }
        }
        
        public static async Task<(bool, FailReviewReason)> CanReview()
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
                        $"[GameSDK.Feedback]: Before getting the opportunity to review, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return (false, FailReviewReason.NoAuth);
            }
            
            _instance._status = ReviewStatus.Waiting;

            List<(bool, FailReviewReason)> reviews = new List<(bool, FailReviewReason)>();

            foreach (var service in _instance._services)
            {
                try
                {
                    reviews.Add(await service.Value.CanReview());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Feedback]: An get possible review error has occurred {e.Message}!");
                    }
                    
                    _instance._status = ReviewStatus.Error;
                    return (false, FailReviewReason.Unknown);
                }
            }

            if (reviews.Count == 0)
            {
                return (false, FailReviewReason.Unknown);
            }
            
            foreach (var review in reviews)
            {
                if (review.Item1)
                {
                    _instance._status = ReviewStatus.Success;
                    return (true, FailReviewReason.Unknown);
                }
            }

            return (false, reviews[0].Item2);
        }
        
        public static async Task<(bool, FailReviewReason)> RequestReview()
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
                        $"[GameSDK.Feedback]: Before request review, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return (false, FailReviewReason.NoAuth);
            }
            
            _instance._status = ReviewStatus.Waiting;

            List<(bool, FailReviewReason)> reviews = new List<(bool, FailReviewReason)>();

            foreach (var service in _instance._services)
            {
                try
                {
                    reviews.Add(await service.Value.RequestReview());
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Feedback]: An request review error has occurred {e.Message}!");
                    }
                    
                    _instance._status = ReviewStatus.Error;
                    return (false, FailReviewReason.Unknown);
                }
            }

            if (reviews.Count == 0)
            {
                return (false, FailReviewReason.Unknown);
            }
            
            foreach (var review in reviews)
            {
                if (review.Item1)
                {
                    _instance._status = ReviewStatus.Success;
                    return (true, FailReviewReason.Unknown);
                }
            }

            return (false, reviews[0].Item2);
        }
    }
}