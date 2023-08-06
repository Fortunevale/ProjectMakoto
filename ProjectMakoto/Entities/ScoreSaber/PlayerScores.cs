namespace ProjectMakoto.Entities.ScoreSaber;

public class PlayerScores
{
    public Playerscore[] playerScores { get; set; }
    public Metadata metadata { get; set; }

    public class Metadata
    {
        public int total { get; set; }
        public int page { get; set; }
        public int itemsPerPage { get; set; }
    }

    public class Playerscore
    {
        public Score score { get; set; }
        public Leaderboard leaderboard { get; set; }
    }

    public class Score
    {
        public int id { get; set; }
        public int rank { get; set; }
        public int baseScore { get; set; }
        public int modifiedScore { get; set; }
        public float pp { get; set; }
        public float weight { get; set; }
        public string modifiers { get; set; }
        public float multiplier { get; set; }
        public int badCuts { get; set; }
        public int missedNotes { get; set; }
        public int maxCombo { get; set; }
        public bool fullCombo { get; set; }
        public int hmd { get; set; }
        public DateTime timeSet { get; set; }
        public bool hasReplay { get; set; }
    }

    public class Leaderboard
    {
        public int id { get; set; }
        public string songHash { get; set; }
        public string songName { get; set; }
        public string songSubName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public Difficulty difficulty { get; set; }
        public int maxScore { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? rankedDate { get; set; }
        public DateTime? qualifiedDate { get; set; }
        public DateTime? lovedDate { get; set; }
        public bool ranked { get; set; }
        public bool qualified { get; set; }
        public bool loved { get; set; }
        public int maxPP { get; set; }
        public float stars { get; set; }
        public int plays { get; set; }
        public int dailyPlays { get; set; }
        public bool positiveModifiers { get; set; }
        public object playerScore { get; set; }
        public string coverImage { get; set; }
        public object difficulties { get; set; }
    }

    public class Difficulty
    {
        public int leaderboardId { get; set; }
        public int difficulty { get; set; }
        public string gameMode { get; set; }
        public string difficultyRaw { get; set; }
    }
}
