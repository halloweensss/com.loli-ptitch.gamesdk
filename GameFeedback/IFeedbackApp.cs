using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.GameFeedback
{
    internal interface IFeedbackApp : IGameSDKService
    {
        Task<(bool, FailReviewReason)> CanReview();
        Task<(bool, FailReviewReason)> RequestReview();
    }
}