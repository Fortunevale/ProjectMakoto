// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Entities;

public sealed class User : RequiresBotReference
{
    public User(Bot bot, ulong userId) : base(bot)
    {
        if (bot.objectedUsers.Contains(userId))
            throw new InvalidOperationException($"User {userId} has objected to having their data processed.");

        this.Id = userId;

        this.Cooldown = new(bot, this);

        this.UrlSubmissions = new(bot, this);
        this.AfkStatus = new(bot, this);
        this.ScoreSaber = new(bot, this);
        this.ExperienceUser = new(bot, this);
        this.Reminders = new(bot, this);
        this.Translation = new(bot, this);
        this.TranslationReports = new(bot, this);
    }

    internal ulong Id { get; set; }

    [JsonIgnore]
    public DataSettings Data { get; set; } = new();

    public UrlSubmissionSettings UrlSubmissions { get; set; }
    public AfkStatus AfkStatus { get; set; }
    public ScoreSaberSettings ScoreSaber { get; set; }
    public ExperienceUserSettings ExperienceUser { get; set; }
    public ReminderSettings Reminders { get; set; }
    public TranslationSettings Translation { get; set; }
    public TranslationReportSettings TranslationReports { get; set; }

    public List<ulong> BlockedUsers { get; set; } = new();
    public List<UserPlaylist> UserPlaylists { get; set; } = new();

    public string? CurrentLocale { get; set; } = null;
    public string? OverrideLocale { get; set; } = null;

    [JsonIgnore]
    public string? Locale
        => this.OverrideLocale ?? this.CurrentLocale ?? "en";

    [JsonIgnore]
    public Cooldown Cooldown { get; set; }

    [JsonIgnore]
    public DateTime LastSuccessful2FA { get; set; } = DateTime.MinValue;

    [JsonIgnore]
    public UserUpload? PendingUserUpload { get; set; } = null;
}
