// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Entities;

[TableName("guilds")]
public sealed class Guild : RequiresBotReference
{
    public Guild(Bot bot, ulong serverId) : base(bot)
    {
        this.Id = serverId;

        this.TokenLeakDetection = new(bot, this);
        this.PhishingDetection = new(bot, this);
        this.BumpReminder = new(bot, this);
        this.Join = new(bot, this);
        this.Experience = new(bot, this);
        this.Crosspost = new(bot, this);
        this.ActionLog = new(bot, this);
        this.InVoiceTextPrivacy = new(bot, this);
        this.InviteTracker = new(bot, this);
        this.InviteNotes = new(bot, this);
        this.NameNormalizer = new(bot, this);
        this.EmbedMessage = new(bot, this);
        this.MusicModule = new(bot, this);
        this.Polls = new(bot, this);
        this.VcCreator = new(bot, this);
        this.PrefixSettings = new(bot, this);

        this.Members = new((id) =>
        {
            return new Member(bot, this, id);
        });
    }

    [ColumnName("serverid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; set; }

    [ContainsValues]
    public TokenLeakDetectionSettings TokenLeakDetection { get; set; }

    [ContainsValues]
    public PhishingDetectionSettings PhishingDetection { get; set; }

    [ContainsValues]
    public BumpReminderSettings BumpReminder { get; set; }

    [ContainsValues]
    public JoinSettings Join { get; set; }

    [ContainsValues]
    public ExperienceSettings Experience { get; set; }

    [ContainsValues]
    public CrosspostSettings Crosspost { get; set; }

    [ContainsValues]
    public ActionLogSettings ActionLog { get; set; }

    [ContainsValues]
    public InVoiceTextPrivacySettings InVoiceTextPrivacy { get; set; }

    [ContainsValues]
    public InviteTrackerSettings InviteTracker { get; set; }

    [ContainsValues]
    public InviteNotesSettings InviteNotes { get; set; }

    [ContainsValues]
    public NameNormalizerSettings NameNormalizer { get; set; }

    [ContainsValues]
    public EmbedMessageSettings EmbedMessage { get; set; }

    [ContainsValues]
    public PollSettings Polls { get; set; }

    [ContainsValues]
    public VcCreatorSettings VcCreator { get; set; } // todo

    [ContainsValues]
    public PrefixSettings PrefixSettings { get; set; }  // todo

    [ContainsValues]
    public Lavalink MusicModule { get; set; }  // todo

    [ColumnName("autounarchivelist"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Default("[]")]
    public ulong[] AutoUnarchiveThreads { get; set; }

    [ColumnName("levelrewards"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Default("[]")]
    public LevelRewardEntry[] LevelRewards { get; set; }

    [ColumnName("levelrewards"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Default("[]")]
    public KeyValuePair<ulong, ReactionRoleEntry>[] ReactionRoles { get; set; }

    [ColumnName("current_locale"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Nullable]
    public string? CurrentLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "current_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "current_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("override_locale"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Nullable]
    public string? OverrideLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "override_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "override_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    public SelfFillingDictionary<Member> Members { get; set; }
}
