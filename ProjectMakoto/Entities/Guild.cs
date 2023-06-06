// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Guild
{
    public Guild(ulong serverId, Bot bot)
    {
        this._bot = bot;

        this.ServerId = serverId;

        this.TokenLeakDetection = new(this);
        this.PhishingDetection = new(this);
        this.BumpReminder = new(this);
        this.Join = new(this);
        this.Experience = new(this);
        this.Crosspost = new(this);
        this.ActionLog = new(this);
        this.InVoiceTextPrivacy = new(this);
        this.InviteTracker = new(this);
        this.InviteNotes = new(this);
        this.NameNormalizer = new(this);
        this.EmbedMessage = new(this);
        this.MusicModule = new(this);
        this.Polls = new(this, bot);
        this.VcCreator = new(this, bot);
        this.PrefixSettings = new(this);
    }

    public Bot _bot { get; set; }
    public ulong ServerId { get; set; }

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

    public Dictionary<ulong, Member> Members { get; set; } = new();

    public Lavalink MusicModule { get; set; }

    public string? CurrentLocale { get; set; } = null;
    public string? OverrideLocale { get; set; } = null;
}
