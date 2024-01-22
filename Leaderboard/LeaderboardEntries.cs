using System;

namespace GameSDK.Leaderboard
{
    [Serializable]
    public class LeaderboardEntries
    {
        public LeaderboardDescription Leaderboard;
        public LeaderboardRange[] Ranges;
        public int UserRank;
        public LeaderboardPlayerData[] Entries;
    }

    [Serializable]
    public class LeaderboardRange
    {
        public int Start;
        public int Size;
    }
}