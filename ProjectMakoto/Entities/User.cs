// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Entities;

[TableName("users")]
public sealed class User : RequiresBotReference
{
    public User(Bot bot, ulong userId) : base(bot)
    {
        if (bot.objectedUsers.Contains(userId))
            throw new InvalidOperationException($"User {userId} has objected to having their data processed.");

        this.Id = userId;

        _ = this.Bot.DatabaseClient.CreateRow("users", typeof(User), userId, this.Bot.DatabaseClient.mainDatabaseConnection);

        this.Cooldown = new(bot, this);

        this.UrlSubmissions = new(bot, this);
        this.AfkStatus = new(bot, this);
        this.ScoreSaber = new(bot, this);
        this.ExperienceUser = new(bot, this);
        this.Reminders = new(bot, this);
        this.Translation = new(bot, this);
        this.TranslationReports = new(bot, this);
        this.Data = new(bot, this);
    }

    [ColumnName("userid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }

    [ContainsValues]
    public UrlSubmissionSettings UrlSubmissions { get; init; }

    [ContainsValues]
    public AfkStatus AfkStatus { get; init; }

    [ContainsValues]
    public ScoreSaberSettings ScoreSaber { get; init; }

    [ContainsValues]
    public ExperienceUserSettings ExperienceUser { get; init; }

    [ContainsValues]
    public ReminderSettings Reminders { get; init; }

    [ContainsValues]
    public TranslationSettings Translation { get; init; }

    [ContainsValues]
    public TranslationReportSettings TranslationReports { get; init; }

    [ContainsValues]
    public DataSettings Data { get; init; }

    [ColumnName("blocked_users"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public ulong[] BlockedUsers
    {
        get => JsonConvert.DeserializeObject<ulong[]>(this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Id, "blocked_users", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => this.Bot.DatabaseClient.SetValue("users", "userid", this.Id, "blocked_users", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("playlists"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public UserPlaylist[] UserPlaylists
    {
        get
        {
            return JsonConvert.DeserializeObject<UserPlaylist[]>(this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Id, "playlists", this.Bot.DatabaseClient.mainDatabaseConnection))
                .Select(x =>
                {
                    x.Bot = this.Bot;
                    x.Parent = this;

                    return x;
                }).ToArray();
        }
        set => this.Bot.DatabaseClient.SetValue("users", "userid", this.Id, "playlists", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("current_locale"), ColumnType(ColumnTypes.Text), Nullable]
    public string? CurrentLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Id, "current_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Id, "current_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("override_locale"), ColumnType(ColumnTypes.Text), Nullable]
    public string? OverrideLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Id, "override_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Id, "override_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
    
    [ColumnName("timezone"), ColumnType(ColumnTypes.Text), Nullable]
    public string? Timezone
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Id, "timezone", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Id, "timezone", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [JsonIgnore]
    public string? Locale
        => this.OverrideLocale ?? this.CurrentLocale ?? "en";

    [JsonIgnore]
    public Cooldown Cooldown { get; init; }

    [JsonIgnore]
    public DateTime LastSuccessful2FA { get; set; } = DateTime.MinValue;

    [JsonIgnore]
    public UserUpload? PendingUserUpload { get; set; } = null;
}
