using System;

namespace GameSDK.Leaderboard
{
    [Serializable]
    public class LeaderboardDescription
    {
        /// <summary>
        /// Platform-specific application identifier, when available.
        /// </summary>
        public string AppId;
        public string Name;
        public Title Title;
        /// <summary>
        /// All localized titles supplied by the platform.
        /// </summary>
        public LeaderboardLocalizedTitle[] LocalizedTitles;
        /// <summary>
        /// Whether this is the platform's default leaderboard, when available.
        /// </summary>
        public bool IsDefault;
        /// <summary>
        /// Sort direction, for example <c>ASC</c> or <c>DESC</c>.
        /// </summary>
        public string SortOrder;
        /// <summary>
        /// Whether the platform treats a lower score as better.
        /// </summary>
        public bool IsInvertedSortOrder;
        public LeaderboardScoreFormat ScoreFormat;
    }
    
    [Serializable]
    public class Title
    {
        public string EN;
        public string RU;
    }

    [Serializable]
    public class LeaderboardLocalizedTitle
    {
        public string Locale;
        public string Value;
    }

    [Serializable]
    public class LeaderboardScoreFormat
    {
        /// <summary>
        /// Format used to display scores, for example <c>numeric</c> or <c>time</c>.
        /// </summary>
        public string Type;
        /// <summary>
        /// Number of decimal places implied by an integer score.
        /// </summary>
        public int DecimalOffset;
    }
}
