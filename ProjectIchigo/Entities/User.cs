namespace ProjectIchigo.Entities;

public class User
{
    public User(Bot _bot, ulong userId)
    {
        if (_bot.objectedUsers.Contains(userId))
            throw new InvalidOperationException($"User {userId} has objected to having their data processed.");

        Cooldown = new(_bot);
        UserId = userId;

        UrlSubmissions = new(this);
        AfkStatus = new(this);
        ScoreSaber = new(this);
        ExperienceUserSettings = new(this);
        ReminderSettings = new(this);
    }

    [JsonIgnore]
    public ulong UserId { get; set; }

    public UrlSubmissionSettings UrlSubmissions { get; set; }
    public AfkStatus AfkStatus { get; set; }
    public ScoreSaberSettings ScoreSaber { get; set; }
    public ExperienceUserSettings ExperienceUserSettings { get; set; }
    public ReminderSettings ReminderSettings { get; set; }

    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    [JsonIgnore]
    public Cooldown Cooldown { get; set; }
}
