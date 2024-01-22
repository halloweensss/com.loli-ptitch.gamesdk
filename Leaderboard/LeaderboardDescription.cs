using System;

namespace GameSDK.Leaderboard
{
    [Serializable]
    public class LeaderboardDescription
    {
        public string Name;
        public Title Title;
    }
    
    [Serializable]
    public class Title
    {
        public string EN;
        public string RU;
    }
}