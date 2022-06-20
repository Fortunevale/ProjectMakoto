namespace ProjectIchigo.Entities;

internal class Users
{
    public Dictionary<ulong, Info> List { get; set; } = new();

    internal class Info
    {
        internal Info(Bot _bot)
        {
            Cooldown = new(_bot);
        }

        public UrlSubmissions UrlSubmissions { get; set; } = new();
        public AfkStatus AfkStatus { get; set; } = new();
        public ScoreSaber ScoreSaber { get; set; } = new();
        public ExperienceUserSettings ExperienceUserSettings { get; set; } = new();
        public List<UserPlaylist> UserPlaylists { get; set; } = new();

        public Cooldown Cooldown { get; set; }
    }
}
