namespace ProjectIchigo.Entities;

public class PollSettings
{
    public PollSettings(Guild guild, Bot bot)
    {
        this._bot = bot;
        this.Parent = guild;

        _RunningPolls.ItemsChanged += RunningPollsUpdatedAsync;
    }

    private Guild Parent { get; set; }

    private Bot _bot { get; set; }

    public ObservableList<PollEntry> RunningPolls { get => _RunningPolls; set { _RunningPolls = value; _RunningPolls.ItemsChanged += RunningPollsUpdatedAsync; } }
    private ObservableList<PollEntry> _RunningPolls { get; set; } = new();

    private async void RunningPollsUpdatedAsync(object? sender, ObservableListUpdate<PollEntry> e)
    {
        while (RunningPolls.Count > 10)
            RunningPolls.RemoveAt(0);

        while (!_bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        foreach (var b in RunningPolls.ToList())
            if (!GetScheduleTasks().ToList().Any(x => x.Value.customId == $"{Parent.ServerId}; {b.SelectUUID}; poll"))
            {
                string taskuid = "";
                CancellationTokenSource cancellationTokenSource = new();

                async Task VoteHandling(DiscordClient sender, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message?.Id == b.MessageId && e.Channel?.Id == b.ChannelId)
                        {
                            if (e.GetCustomId() == b.SelectUUID)
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
                            else if (e.GetCustomId() == b.EndEarlyUUID)
                            {
                                if (!(await e.User.ConvertToMember(e.Guild)).Permissions.HasPermission(Permissions.ManageMessages))
                                {
                                    _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ `You don't have the necessary permissions to end this poll early.`"));
                                    return;
                                }
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                                this.RunningPolls.Remove(b);
                                b.DueTime = DateTime.UtcNow;
                                await Task.Delay(5000);
                                this.RunningPolls.Add(b);
                                cancellationTokenSource.Cancel();
                                _ = e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"✅ `Poll ended.`"));
                            }
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
                    cancellationTokenSource.Cancel();

                    await Task.Delay(1000);

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

                    try
                    { _ = message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds?.ElementAt(0))); }
                    catch { }
                    await message.RespondAsync(new DiscordEmbedBuilder()
                        .WithDescription($"`Poll ended.`\n\n**Results**\n{(votes.Count <= 0 ? "`No one voted on this poll.`" : string.Join("\n\n", votes.OrderByDescending(x => x.Value).Select(x => $"> **{b.Options[x.Key].Sanitize()}**\n`{x.Value} Votes`")))}")
                        .WithAuthor($"Poll • {channel.Guild.Name}", null, channel.Guild.IconUrl)
                        .WithColor(EmbedColors.Success));
                });

                Task.Run(async () =>
                {
                    int LastTotalVotes = -1;

                    while (this.RunningPolls.Contains(b))
                    {
                        if (LastTotalVotes != b.Votes.Count)
                        {
                            LastTotalVotes = b.Votes.Count;

                            var channel = await _bot.discordClient.GetChannelAsync(b.ChannelId);
                            var message = await channel.GetMessageAsync(b.MessageId, true);

                            await message.ModifyAsync(new DiscordEmbedBuilder(message.Embeds.ElementAt(0)).WithDescription($"> **{b.PollText}**\n\n_This poll will end {b.DueTime.ToTimestamp()}._\n\n`{b.Votes.Count} Total Votes`").Build()); 
                        }

                        if (cancellationTokenSource.IsCancellationRequested)
                            return;

                        try { await Task.Delay(TimeSpan.FromMinutes(2), cancellationTokenSource.Token); } catch { }
                    }
                }).Add(_bot.watcher);

                _bot.discordClient.ComponentInteractionCreated += VoteHandling;

                task.Add(_bot.watcher);
                taskuid = task.CreateScheduleTask(b.DueTime.ToUniversalTime(), $"{Parent.ServerId}; {b.SelectUUID}; poll");

                _bot.discordClient.MessageDeleted += MessageDeletionHandling;
                _bot.discordClient.ChannelDeleted += ChannelDeletionHandling;

                _logger.LogDebug($"Created scheduled task for poll by '{Parent.ServerId}'");
            }

        foreach (var b in GetScheduleTasks().ToList())
            if (b.Value.customId.StartsWith($"{Parent.ServerId};") && b.Value.customId.EndsWith($"poll"))
            {
                var uuid = b.Value.customId[..b.Value.customId.LastIndexOf(";")];
                uuid = uuid[(uuid.IndexOf(";") + 2)..];

                if (!RunningPolls.Any(x => x.SelectUUID == uuid))
                {
                    DeleteScheduleTask(b.Key);
                    _logger.LogDebug($"Deleted scheduled task for poll by '{Parent.ServerId}'");
                }
            }

        _ = Bot.DatabaseClient.FullSyncDatabase();
    }
}
