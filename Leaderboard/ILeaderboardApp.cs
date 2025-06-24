using System.Threading.Tasks;
using GameSDK.Core;

namespace GameSDK.Leaderboard
{
    public interface ILeaderboardApp : IServiceProvider
    {
        Task Initialize();
        Task<LeaderboardDescription> GetDescription(string id);
        Task<LeaderboardStatus> SetScore(string id, int score);
        Task<(LeaderboardStatus, LeaderboardPlayerData)> GetPlayerData(string id);
        Task<(LeaderboardStatus, LeaderboardEntries)> GetEntries(LeaderboardParameters parameters);
    }
}