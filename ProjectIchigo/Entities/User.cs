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
        ExperienceUser = new(this);
        Reminders = new(this, _bot);
    }

    [JsonIgnore]
    public ulong UserId { get; set; }

    public UrlSubmissionSettings UrlSubmissions { get; set; }
    public AfkStatus AfkStatus { get; set; }
    public ScoreSaberSettings ScoreSaber { get; set; }
    public ExperienceUserSettings ExperienceUser { get; set; }
    public ReminderSettings Reminders { get; set; }

    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    [JsonIgnore]
    public Cooldown Cooldown { get; set; }
}
