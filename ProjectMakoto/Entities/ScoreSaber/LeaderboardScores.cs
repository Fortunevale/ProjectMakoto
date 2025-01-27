namespace ProjectMakoto.Entities.ScoreSaber;

public class LeaderboardScores
{
    /// <summary>
    /// The scores.
    /// </summary>
    [JsonProperty("scores")]
    public ScoreInfo[] Scores { get; internal set; }

    /// <summary>
    /// The metadata this request contains.
    /// </summary>
    [JsonProperty("metadata")]
    public MetadataInfo Metadata { get; internal set; }

    public class ScoreInfo
    {
        /// <summary>
        /// The id of the score set.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; internal set; }

        /// <summary>
        /// The player that set the score.
        /// </summary>
        [JsonProperty("leaderboardPlayerInfo")]
        public PlayerInfo Player { get; set; }

        /// <summary>
        /// The rank at which this score resides.
        /// </summary>
        [JsonProperty("rank")]
        public int Rank { get; internal set; }

        /// <summary>
        /// The score without any modifications.
        /// </summary>
        [JsonProperty("baseScore")]
        public int Score { get; internal set; }

        /// <summary>
        /// The score after modifications have been applied.
        /// </summary>
        [JsonProperty("modifiedScore")]
        public int ModifiedScore { get; internal set; }

        /// <summary>
        /// The pp achieved with this score.
        /// </summary>
        [JsonProperty("pp")]
        public float PP { get; internal set; }

        /// <summary>
        /// How much weight this score has in player's total pp.
        /// </summary>
        [JsonProperty("weight")]
        public float Weight { get; internal set; }

        /// <summary>
        /// Which modifiers were used.
        /// </summary>
        [JsonProperty("modifiers")]
        public string Modifiers { get; internal set; }

        /// <summary>
        /// The multiplier used to calculate the <see cref="ModifiedScore"/>
        /// </summary>
        [JsonProperty("multiplier")]
        public float Multiplier { get; internal set; }

        /// <summary>
        /// The amount of bad cuts.
        /// </summary>
        [JsonProperty("badCuts")]
        public int BadCuts { get; internal set; }

        /// <summary>
        /// The amount of missed notes.
        /// </summary>
        [JsonProperty("missedNotes")]
        public int MissedNotes { get; internal set; }

        /// <summary>
        /// The biggest combo achieved in this score.
        /// </summary>
        [JsonProperty("maxCombo")]
        public int MaxCombo { get; internal set; }

        /// <summary>
        /// Whether this score has no mistakes.
        /// </summary>
        [JsonProperty("fullCombo")]
        public bool FullCombo { get; internal set; }

        /// <summary>
        /// The index id of the head mounted display used.
        /// </summary>
        [JsonProperty("hmd")]
        public int HMD { get; internal set; }

        /// <summary>
        /// The time this score was set.
        /// </summary>
        [JsonProperty("timeSet")]
        public DateTime Timestamp { get; internal set; }


        /// <summary>
        /// Whether this score has a replay.
        /// </summary>
        [JsonProperty("hasReplay")]
        public bool HasReplay { get; internal set; }

        public class PlayerInfo
        {
            /// <summary>
            /// The name of this player.
            /// </summary>
            [JsonProperty("id")]
            public int Id { get; internal set; }

            /// <summary>
            /// The name of this player.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; internal set; }

            /// <summary>
            /// The avatar this player uses.
            /// </summary>
            [JsonProperty("profilePicture")]
            public string AvatarUrl { get; internal set; }

            /// <summary>
            /// The country this player is from.
            /// </summary>
            [JsonProperty("country")]
            public string Country { get; internal set; }

            /// <summary>
            /// The permissions this player has.
            /// </summary>
            [JsonProperty("permissions")]
            public int Permissions { get; internal set; }

            /// <summary>
            /// The role this player has.
            /// </summary>
            [JsonProperty("role")]
            public string Role { get; internal set; }
        }
    }

    public class MetadataInfo
    {
        /// <summary>
        /// The total amount of pages.
        /// </summary>
        [JsonProperty("total")]
        public int TotalPages { get; internal set; }

        /// <summary>
        /// The page that's been returned.
        /// </summary>
        [JsonProperty("page")]
        public int Page { get; internal set; }

        /// <summary>
        /// How many items this page contains.
        /// </summary>
        [JsonProperty("itemsPerPage")]
        public int ItemCount { get; internal set; }
    }
}
