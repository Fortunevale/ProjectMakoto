namespace ProjectMakoto.Entities.ScoreSaber;

public class PlayerInfo
{
    public string id { get; set; }
    public string name { get; set; }
    public string profilePicture { get; set; }
    public string country { get; set; }
    public decimal pp { get; set; }
    public int rank { get; set; }
    public int countryRank { get; set; }
    public string role { get; set; }
    public Badge[] badges { get; set; }
    public string histories { get; set; }
    public Scorestats scoreStats { get; set; }
    public int permissions { get; set; }
    public bool banned { get; set; }
    public bool inactive { get; set; }

    public class Scorestats
    {
        public long totalScore { get; set; }
        public long totalRankedScore { get; set; }
        public float averageRankedAccuracy { get; set; }
        public int totalPlayCount { get; set; }
        public int rankedPlayCount { get; set; }
        public int replaysWatched { get; set; }
    }

    public class Badge
    {
        public string description { get; set; }
        public string image { get; set; }
    }
}
