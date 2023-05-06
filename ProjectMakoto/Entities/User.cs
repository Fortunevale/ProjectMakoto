// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

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
        Translation = new(this);
    }

    [JsonIgnore]
    public ulong UserId { get; set; }

    [JsonIgnore]
    public DataSettings Data { get; set; } = new();

    public UrlSubmissionSettings UrlSubmissions { get; set; }
    public AfkStatus AfkStatus { get; set; }
    public ScoreSaberSettings ScoreSaber { get; set; }
    public ExperienceUserSettings ExperienceUser { get; set; }
    public ReminderSettings Reminders { get; set; }
    public TranslationSettings Translation { get; set; }

    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    public string? CurrentLocale { get; set; } = null;
    public string? OverrideLocale { get; set; } = null;

    [JsonIgnore]
    public Cooldown Cooldown { get; set; }
}
