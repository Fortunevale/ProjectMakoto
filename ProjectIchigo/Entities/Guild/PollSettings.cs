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
            if (!GetScheduleTasks().ToList().Any(x => x.Value.customId == $"{Parent.ServerId}; {b.ComponentUUID}; poll"))
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

                    try
                    { _ = message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds?.ElementAt(0))); }
                    catch { }
                    await message.RespondAsync(new DiscordEmbedBuilder()
                        .WithDescription($"`Poll ended.`\n\n**Results**\n{(votes.Count <= 0 ? "`No one voted on this poll.`" : string.Join("\n\n", votes.OrderByDescending(x => x.Value).Select(x => $"> **{b.Options[x.Key].Sanitize()}**\n`{x.Value} Votes`")))}")
                        .WithAuthor($"Poll • {channel.Guild.Name}", null, channel.Guild.IconUrl)
                        .WithColor(EmbedColors.Success));
                });

                _bot.discordClient.ComponentInteractionCreated += VoteHandling;

                task.Add(_bot.watcher);
                taskuid = task.CreateScheduleTask(b.DueTime.ToUniversalTime(), $"{Parent.ServerId}; {b.ComponentUUID}; poll");

                _bot.discordClient.MessageDeleted += MessageDeletionHandling;
                _bot.discordClient.ChannelDeleted += ChannelDeletionHandling;

                _logger.LogDebug($"Created scheduled task for poll by '{Parent.ServerId}'");
            }

        foreach (var b in GetScheduleTasks().ToList())
            if (b.Value.customId.StartsWith($"{Parent.ServerId};") && b.Value.customId.EndsWith($"poll"))
            {
                var uuid = b.Value.customId[..b.Value.customId.LastIndexOf(";")];
                uuid = uuid[(uuid.IndexOf(";") + 2)..];

                if (!RunningPolls.Any(x => x.ComponentUUID == uuid))
                {
                    DeleteScheduleTask(b.Key);
                    _logger.LogDebug($"Deleted scheduled task for poll by '{Parent.ServerId}'");
                }
            }

        _ = Bot.DatabaseClient.FullSyncDatabase();
    }
}
