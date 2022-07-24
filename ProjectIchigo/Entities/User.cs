namespace ProjectIchigo.Entities;

public class User
{
    public User(Bot _bot, ulong userId)
    {
        Cooldown = new(_bot);
        UserId = userId;

        UrlSubmissions = new(this);
        AfkStatus = new(this);
        ScoreSaber = new(this);
        ExperienceUserSettings = new(this);
    }

    public ulong UserId { get; set; }

    public UrlSubmissions UrlSubmissions { get; set; }
    public AfkStatus AfkStatus { get; set; }
    public ScoreSaber ScoreSaber { get; set; }
    public ExperienceUserSettings ExperienceUserSettings { get; set; }
    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    public Cooldown Cooldown { get; set; }
}
