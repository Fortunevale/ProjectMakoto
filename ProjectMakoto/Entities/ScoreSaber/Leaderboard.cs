namespace ProjectMakoto.Entities.ScoreSaber;

public class Leaderboard
{
    public int requestId { get; set; }
    public string requestDescription { get; set; }
    public Leaderboardinfo leaderboardInfo { get; set; }
    public string created_at { get; set; }
    public Rankvotes rankVotes { get; set; }
    public Qatvotes qatVotes { get; set; }
    public Rankcomment[] rankComments { get; set; }
    public Qatcomment[] qatComments { get; set; }
    public int requestType { get; set; }
    public int approved { get; set; }
    public Difficulty2[] difficulties { get; set; }

    public class Leaderboardinfo
    {
        public int id { get; set; }
        public string songHash { get; set; }
        public string songName { get; set; }
        public string songSubName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public Difficulty difficulty { get; set; }
        public int maxScore { get; set; }
        public DateTime createdDate { get; set; }
        public string rankedDate { get; set; }
        public string qualifiedDate { get; set; }
        public string lovedDate { get; set; }
        public bool ranked { get; set; }
        public bool qualified { get; set; }
        public bool loved { get; set; }
        public float maxPP { get; set; }
        public float stars { get; set; }
        public bool positiveModifiers { get; set; }
        public int plays { get; set; }
        public int dailyPlays { get; set; }
        public string coverImage { get; set; }
        public Playerscore playerScore { get; set; }
        public Difficulty1[] difficulties { get; set; }
    }

    public class Difficulty
    {
        public int leaderboardId { get; set; }
        public int difficulty { get; set; }
        public string gameMode { get; set; }
        public string difficultyRaw { get; set; }
    }

    public class Playerscore
    {
        public int id { get; set; }
        public Leaderboardplayerinfo leaderboardPlayerInfo { get; set; }
        public int rank { get; set; }
        public int baseScore { get; set; }
        public int modifiedScore { get; set; }
        public float pp { get; set; }
        public int weight { get; set; }
        public string modifiers { get; set; }
        public int multiplier { get; set; }
        public int badCuts { get; set; }
        public int missedNotes { get; set; }
        public int maxCombo { get; set; }
        public bool fullCombo { get; set; }
        public int hmd { get; set; }
        public bool hasReplay { get; set; }
        public DateTime timeSet { get; set; }
    }

    public class Leaderboardplayerinfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string profilePicture { get; set; }
        public string country { get; set; }
        public int permissions { get; set; }
        public string role { get; set; }
    }

    public class Difficulty1
    {
        public int leaderboardId { get; set; }
        public int difficulty { get; set; }
        public string gameMode { get; set; }
        public string difficultyRaw { get; set; }
    }

    public class Rankvotes
    {
        public int upvotes { get; set; }
        public int downvotes { get; set; }
        public bool myVote { get; set; }
        public int neutral { get; set; }
    }

    public class Qatvotes
    {
        public int upvotes { get; set; }
        public int downvotes { get; set; }
        public bool myVote { get; set; }
        public int neutral { get; set; }
    }

    public class Rankcomment
    {
        public string username { get; set; }
        public string userId { get; set; }
        public string comment { get; set; }
        public string timeStamp { get; set; }
    }

    public class Qatcomment
    {
        public string username { get; set; }
        public string userId { get; set; }
        public string comment { get; set; }
        public string timeStamp { get; set; }
    }

    public class Difficulty2
    {
        public int requestId { get; set; }
        public int difficulty { get; set; }
    }

}
