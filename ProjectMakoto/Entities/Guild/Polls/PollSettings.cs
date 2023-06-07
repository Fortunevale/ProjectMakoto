// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class PollSettings
{
    public PollSettings(Guild guild, Bot bot)
    {
        this._bot = bot;
        this.Parent = guild;

        this._RunningPolls.ItemsChanged += RunningPollsUpdatedAsync;
    }

    private Guild Parent { get; set; }

    private Bot _bot { get; set; }

    public ObservableList<PollEntry> RunningPolls { get => this._RunningPolls; set { this._RunningPolls = value; this._RunningPolls.ItemsChanged += RunningPollsUpdatedAsync; } }
    private ObservableList<PollEntry> _RunningPolls { get; set; } = new();

    private async void RunningPollsUpdatedAsync(object? sender, ObservableListUpdate<PollEntry> e)
    {
        var CommandKey = this._bot.loadedTranslations.Commands.Moderation.Poll;

        while (this.RunningPolls.Count > 10)
            this.RunningPolls.RemoveAt(0);

        while (!this._bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        foreach (var b in this.RunningPolls.ToList())
            if (!UniversalExtensions.GetScheduledTasks().Any(x =>
            {
                if (x.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier ||
                scheduledTaskIdentifier.Snowflake != this.Parent.ServerId ||
                scheduledTaskIdentifier.Type != "poll" ||
                scheduledTaskIdentifier.Id != b.SelectUUID)
                    return false;

                return true;
            }))
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

                                if (b.Votes.TryGetValue(e.User.Id, out List<string> currentVotes))
                                {
                                    b.Votes[e.User.Id] = new List<string>(e.Values);
                                    _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"🔁 {CommandKey.VoteUpdated.Get(this.Parent).Build(true, new TVar("Options", string.Join(", ", e.Values.Select(x => $"'{currentVotes}'"))))}"));
                                    return;
                                }

                                b.Votes.Add(e.User.Id, new List<string>(e.Values));
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"✅ {CommandKey.Voted.Get(this.Parent).Build(true, new TVar("Options", string.Join(", ", e.Values.Select(x => $"'{b.Options[x]}'"))))}"));
                            }
                            else if (e.GetCustomId() == b.EndEarlyUUID)
                            {
                                if (!(await e.User.ConvertToMember(e.Guild)).Permissions.HasPermission(Permissions.ManageMessages))
                                {
                                    _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ {CommandKey.NoPerms.Get(this._bot.users[e.User.Id]).Build(true)}"));
                                    return;
                                }
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                                this.RunningPolls.Remove(b);
                                b.DueTime = DateTime.UtcNow;
                                await Task.Delay(5000);
                                this.RunningPolls.Add(b);
                                cancellationTokenSource.Cancel();
                                _ = e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"✅ {CommandKey.PollEnded.Get(this._bot.users[e.User.Id]).Build(true)}"));
                            }
                        }
                    }).Add(this._bot);
                }

                async Task MessageDeletionHandling(DiscordClient client, MessageDeleteEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message?.Id == b.MessageId)
                        {
                            this._bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                            this._bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                            this._bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;
                            this.RunningPolls.Remove(b);
                        }
                    }).Add(this._bot);
                }

                async Task ChannelDeletionHandling(DiscordClient client, ChannelDeleteEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Channel?.Id == b.ChannelId)
                        {
                            this._bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                            this._bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                            this._bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;
                            this.RunningPolls.Remove(b);
                        }
                    }).Add(this._bot);
                }

                Task task = new(async () =>
                {
                    cancellationTokenSource.Cancel();

                    await Task.Delay(1000);

                    this._bot.discordClient.ComponentInteractionCreated -= VoteHandling;
                    this._bot.discordClient.MessageDeleted -= MessageDeletionHandling;
                    this._bot.discordClient.ChannelDeleted -= ChannelDeletionHandling;

                    this.RunningPolls.Remove(b);

                    var channel = await this._bot.discordClient.GetChannelAsync(b.ChannelId);
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
                        .WithDescription($"{CommandKey.PollEnded.Get(this.Parent).Build(true)}\n\n**{CommandKey.Results.Get(this.Parent).Build()}**\n{(votes.Count <= 0 ? CommandKey.NoVotes.Get(this.Parent).Build(true) : string.Join("\n\n", votes.OrderByDescending(x => x.Value).Select(x => $"> **{b.Options[x.Key].FullSanitize()}**\n{CommandKey.Votes.Get(this.Parent).Build(true, new TVar("Count", x.Value))}")))}")
                        .WithAuthor($"{CommandKey.Poll.Get(this.Parent)} • {channel.Guild.Name}", null, channel.Guild.IconUrl)
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

                            var channel = await this._bot.discordClient.GetChannelAsync(b.ChannelId);
                            var message = await channel.GetMessageAsync(b.MessageId, true);

                            await message.ModifyAsync(new DiscordEmbedBuilder(message.Embeds.ElementAt(0)).WithDescription($"> **{b.PollText}**\n\n_{CommandKey.PollEnding.Get(this.Parent).Build(new TVar("Timestamp", b.DueTime.ToTimestamp()))}._\n\n{CommandKey.TotalVotes.Get(this.Parent).Build(true, new TVar("Count", b.Votes.Count))}").Build());
                        }

                        if (cancellationTokenSource.IsCancellationRequested)
                            return;

                        try
                        { await Task.Delay(TimeSpan.FromMinutes(2), cancellationTokenSource.Token); }
                        catch { }
                    }
                }).Add(this._bot);

                this._bot.discordClient.ComponentInteractionCreated += VoteHandling;

                task.Add(this._bot);
                taskuid = task.CreateScheduledTask(b.DueTime.ToUniversalTime(), new ScheduledTaskIdentifier(this.Parent.ServerId, b.SelectUUID, "poll"));

                this._bot.discordClient.MessageDeleted += MessageDeletionHandling;
                this._bot.discordClient.ChannelDeleted += ChannelDeletionHandling;

                _logger.LogDebug("Created scheduled task for poll by '{Guild}'", this.Parent.ServerId);
            }

        foreach (var b in UniversalExtensions.GetScheduledTasks())
        {
            if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                continue;

            if (scheduledTaskIdentifier.Snowflake == this.Parent.ServerId && scheduledTaskIdentifier.Type == "poll" && !this.RunningPolls.Any(x => x.SelectUUID == ((ScheduledTaskIdentifier)b.CustomData).Id))
            {
                b.Delete();

                _logger.LogDebug("Deleted scheduled task for poll by '{Guild}'", this.Parent.ServerId);
            }
        }
    }
}
