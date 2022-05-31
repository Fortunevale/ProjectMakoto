namespace Project_Ichigo.Objects;

public class Lavalink
{
    public List<QueueInfo> SongQueue = new();

    public List<ulong> collectedSkips = new();
    public List<ulong> collectedDisconnectVotes = new();
    public List<ulong> collectedClearQueueVotes = new();

    public bool Repeat = false;
    public bool Shuffle = false;

    public bool IsPaused = false;

    public class QueueInfo
    {
        public QueueInfo(string VideoTitle, string Url, DiscordGuild guild, DiscordUser user)
        {
            this.VideoTitle = VideoTitle;
            this.Url = Url;
            this.guild = guild;
            this.user = user;
        }

        public string VideoTitle { get; set; }
        public string Url { get; set; }
        public DiscordGuild guild { get; set; }
        public DiscordUser user { get; set; }
    }
}
