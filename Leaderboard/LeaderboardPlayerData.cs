using System;

namespace GameSDK.Leaderboard
{
    [Serializable]
    public class LeaderboardPlayerData
    {
        /// <summary>
        /// Stable player identifier provided by the platform, when available.
        /// </summary>
        public string PlayerId;
        public string Name;
        /// <summary>
        /// URL of the player's avatar provided by the platform, when available.
        /// </summary>
        public string AvatarUrl;
        public long Rank;
        public long Score;
        /// <summary>
        /// Arbitrary data saved together with this leaderboard entry, when supported.
        /// </summary>
        public string ExtraData;
    }
}
