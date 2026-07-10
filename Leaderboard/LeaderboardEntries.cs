using System;

namespace GameSDK.Leaderboard
{
    [Serializable]
    public class LeaderboardEntries
    {
        public LeaderboardDescription Leaderboard;
        public LeaderboardRange[] Ranges;
        public long UserRank;
        /// <summary>
        /// Whether the provider returned a rank for the current player.
        /// </summary>
        public bool HasUserRank;
        public LeaderboardPlayerData[] Entries;
    }

    [Serializable]
    public class LeaderboardRange
    {
        public long Start;
        public long Size;
    }
}
