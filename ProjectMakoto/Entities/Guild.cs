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

        _ = this.Bot.DatabaseClient.CreateRow("guilds", typeof(Guild), serverId, this.Bot.DatabaseClient.mainDatabaseConnection);
        _ = this.Bot.DatabaseClient.CreateTable(serverId.ToString(), typeof(Guild), this.Bot.DatabaseClient.guildDatabaseConnection);

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
    internal ulong Id { get; init; }

    [ContainsValues]
    public TokenLeakDetectionSettings TokenLeakDetection { get; init; }

    [ContainsValues]
    public PhishingDetectionSettings PhishingDetection { get; init; }

    [ContainsValues]
    public BumpReminderSettings BumpReminder { get; init; }

    [ContainsValues]
    public JoinSettings Join { get; init; }

    [ContainsValues]
    public ExperienceSettings Experience { get; init; }

    [ContainsValues]
    public CrosspostSettings Crosspost { get; init; }

    [ContainsValues]
    public ActionLogSettings ActionLog { get; init; }

    [ContainsValues]
    public InVoiceTextPrivacySettings InVoiceTextPrivacy { get; init; }

    [ContainsValues]
    public InviteTrackerSettings InviteTracker { get; init; }

    [ContainsValues]
    public InviteNotesSettings InviteNotes { get; init; }

    [ContainsValues]
    public NameNormalizerSettings NameNormalizer { get; init; }

    [ContainsValues]
    public EmbedMessageSettings EmbedMessage { get; init; }

    [ContainsValues]
    public PollSettings Polls { get; init; }

    [ContainsValues]
    public VcCreatorSettings VcCreator { get; init; }

    [ContainsValues]
    public PrefixSettings PrefixSettings { get; init; }

    [ContainsValues]
    public Lavalink MusicModule { get; set; }

    [ColumnName("autounarchivelist"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Default("[]")]
    public ulong[] AutoUnarchiveThreads
    {
        get => JsonConvert.DeserializeObject<ulong[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "autounarchivelist", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "autounarchivelist", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("levelrewards"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Default("[]")]
    public LevelRewardEntry[] LevelRewards // todo
    {
        get => JsonConvert.DeserializeObject<LevelRewardEntry[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "levelrewards", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "levelrewards", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("reactionroles"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Default("[]")]
    public ReactionRoleEntry[] ReactionRoles
    {
        get => JsonConvert.DeserializeObject<ReactionRoleEntry[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "reactionroles", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "reactionroles", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("current_locale"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Nullable]
    public string? CurrentLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "current_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "current_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("override_locale"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Nullable]
    public string? OverrideLocale
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Id, "override_locale", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Id, "override_locale", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    public SelfFillingDictionary<Member> Members { get; init; }
}
