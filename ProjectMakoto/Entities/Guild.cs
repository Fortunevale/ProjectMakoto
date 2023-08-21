// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Entities;

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

        this.Members = new();
    }

    internal ulong Id { get; set; }

    public TokenLeakDetectionSettings TokenLeakDetection { get; set; }
    public PhishingDetectionSettings PhishingDetection { get; set; }
    public BumpReminderSettings BumpReminder { get; set; }
    public JoinSettings Join { get; set; }
    public ExperienceSettings Experience { get; set; }
    public CrosspostSettings Crosspost { get; set; }
    public ActionLogSettings ActionLog { get; set; }
    public InVoiceTextPrivacySettings InVoiceTextPrivacy { get; set; }
    public InviteTrackerSettings InviteTracker { get; set; }
    public InviteNotesSettings InviteNotes { get; set; }
    public NameNormalizerSettings NameNormalizer { get; set; }
    public EmbedMessageSettings EmbedMessage { get; set; }
    public PollSettings Polls { get; set; }
    public VcCreatorSettings VcCreator { get; set; }
    public PrefixSettings PrefixSettings { get; set; }

    public List<ulong> AutoUnarchiveThreads { get; set; } = new();
    public List<LevelRewardEntry> LevelRewards { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoleEntry>> ReactionRoles { get; set; } = new();

    public Dictionary<ulong, Member> Members { get; set; }

    public Lavalink MusicModule { get; set; }

    public string? CurrentLocale { get; set; } = null;
    public string? OverrideLocale { get; set; } = null;
}
