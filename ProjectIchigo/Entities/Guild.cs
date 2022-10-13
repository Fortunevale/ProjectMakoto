namespace ProjectIchigo.Entities;

public class Guild
{
    public Guild(ulong serverId, Bot bot)
    {
        _bot = bot;

        ServerId = serverId;

        TokenLeakDetectionSettings = new(this);
        PhishingDetectionSettings = new(this);
        BumpReminderSettings = new(this);
        JoinSettings = new(this);
        ExperienceSettings = new(this);
        CrosspostSettings = new(this);
        ActionLogSettings = new(this);
        InVoiceTextPrivacySettings = new(this);
        InviteTrackerSettings = new(this);
        NameNormalizerSettings = new(this);
        EmbedMessageSettings = new(this);
        Lavalink = new(this);

        _ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated;
        _RunningPolls.ItemsChanged += RunningPollsUpdatedAsync;
    }

    private Bot _bot { get; set; }

    public ulong ServerId { get; set; }

    public TokenLeakDetectionSettings TokenLeakDetectionSettings { get; set; }
    public PhishingDetectionSettings PhishingDetectionSettings { get; set; }
    public BumpReminderSettings BumpReminderSettings { get; set; }
    public JoinSettings JoinSettings { get; set; }
    public ExperienceSettings ExperienceSettings { get; set; }
    public CrosspostSettings CrosspostSettings { get; set; }
    public ActionLogSettings ActionLogSettings { get; set; }
    public InVoiceTextPrivacySettings InVoiceTextPrivacySettings { get; set; }
    public InviteTrackerSettings InviteTrackerSettings { get; set; }
    public NameNormalizerSettings NameNormalizerSettings { get; set; }
    public EmbedMessageSettings EmbedMessageSettings { get; set; }

    public List<ulong> AutoUnarchiveThreads { get; set; } = new();
    public List<LevelRewardEntry> LevelRewards { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoleEntry>> ReactionRoles { get; set; } = new();
    public ObservableList<ulong> ProcessedAuditLogs { get => _ProcessedAuditLogs; set { _ProcessedAuditLogs = value; _ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated; } }
    public ObservableList<PollEntry> RunningPolls { get => _RunningPolls; set { _RunningPolls = value; _RunningPolls.ItemsChanged += RunningPollsUpdatedAsync; } }

    public Dictionary<ulong, Member> Members { get; set; } = new();

    public Lavalink Lavalink { get; set; }
    
    private ObservableList<PollEntry> _RunningPolls { get; set; } = new();
    private ObservableList<ulong> _ProcessedAuditLogs { get; set; } = new();

    private void AuditLogCollectionUpdated(object? sender, ObservableListUpdate<ulong> e)
    {
        while (ProcessedAuditLogs.Count > 50)
        {
            ProcessedAuditLogs.RemoveAt(0);
        }
    }

    private async void RunningPollsUpdatedAsync(object? sender, ObservableListUpdate<PollEntry> e)
    {
        while (RunningPolls.Count > 10)
            RunningPolls.RemoveAt(0);

        while (!_bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        foreach (var b in RunningPolls.ToList())
            if (!GetScheduleTasks().ToList().Any(x => x.Value.customId == $"{ServerId}; {b.ComponentUUID}; poll"))
            {
                string taskuid = "";

                async Task VoteHandling(DiscordClient sender, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message?.Id == b.MessageId && e.Channel?.Id == b.ChannelId && e.GetCustomId() == b.ComponentUUID)
                        {
                            if (b.Votes.ContainsKey(e.User.Id))
                            {
                                b.Votes[e.User.Id] = new List<string>(e.Values);
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"🔁 `Updated your vote to {string.Join(", ", e.Values.Select(x => $"'{b.Options[x]}'"))}.`"));
                                return;
                            }

                            b.Votes.Add(e.User.Id, new List<string>(e.Values));
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"✅ `Voted for {string.Join(", ", e.Values.Select(x => $"'{b.Options[x]}'"))}.`"));
                        }
                    }).Add(_bot.watcher);
                }

                async Task MessageDeletionHandling(DiscordClient client, MessageDeleteEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message?.Id == b.MessageId)
                        {
                            _bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                            _bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                            _bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;
                            this.RunningPolls.Remove(b);
                        }
                    }).Add(_bot.watcher);
                }

                async Task ChannelDeletionHandling(DiscordClient client, ChannelDeleteEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Channel?.Id == b.ChannelId)
                        {
                            _bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                            _bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                            _bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;
                            this.RunningPolls.Remove(b);
                        }
                    }).Add(_bot.watcher);
                }

                Task task = new(async () =>
                {
                    _bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                    _bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                    _bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;

                    this.RunningPolls.Remove(b);

                    var channel = await _bot.discordClient.GetChannelAsync(b.ChannelId);
                    var message = await channel.GetMessageAsync(b.MessageId, true);

                    Dictionary<string, int> votes = new();

                    foreach (var user in b.Votes)
                        foreach (var vote in user.Value)
                        {
                            if (!votes.ContainsKey(vote))
                                votes.Add(vote, 0);

                            votes[vote]++;
                        }

                    try { _ = message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds?.ElementAt(0))); } catch { }
                    await message.RespondAsync(new DiscordEmbedBuilder()
                        .WithDescription($"`Poll ended.`\n\n**Results**\n{(votes.Count <= 0 ? "`No one voted on this poll.`" : string.Join("\n\n", votes.OrderByDescending(x => x.Value).Select(x => $"> **{b.Options[x.Key].Sanitize()}**\n`{x.Value} Votes`")))}")
                        .WithAuthor($"Poll • {channel.Guild.Name}", null, channel.Guild.IconUrl)
                        .WithColor(EmbedColors.Success));
                });

                _bot.discordClient.ComponentInteractionCreated += VoteHandling;

                task.Add(_bot.watcher);
                taskuid = task.CreateScheduleTask(b.DueTime.ToUniversalTime(), $"{ServerId}; {b.ComponentUUID}; poll");

                _bot.discordClient.MessageDeleted += MessageDeletionHandling;
                _bot.discordClient.ChannelDeleted += ChannelDeletionHandling;

                _logger.LogDebug($"Created scheduled task for poll by '{ServerId}'");
            }

        foreach (var b in GetScheduleTasks().ToList())
            if (b.Value.customId.StartsWith($"{ServerId};") && b.Value.customId.EndsWith($"poll"))
            {
                var uuid = b.Value.customId[..b.Value.customId.LastIndexOf(";")];
                uuid = uuid[(uuid.IndexOf(";") + 2)..];

                if (!RunningPolls.Any(x => x.ComponentUUID == uuid))
                {
                    DeleteScheduleTask(b.Key);
                    _logger.LogDebug($"Deleted scheduled task for poll by '{ServerId}'");
                }
            }

        _ = Bot.DatabaseClient.FullSyncDatabase();
    }
}
