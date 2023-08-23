// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class PollSettings : RequiresParent<Guild>
{
    public PollSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("polls"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public PollEntry[] RunningPolls
    {
        get => JsonConvert.DeserializeObject<PollEntry[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "polls", this.Bot.DatabaseClient.mainDatabaseConnection));
        set
        {
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "polls", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
            this.RunningPollsUpdatedAsync();
        }
    }

    private void RunningPollsUpdatedAsync()
    {
        _ = Task.Run(async () =>
        {
            var CommandKey = this.Bot.LoadedTranslations.Commands.Moderation.Poll;

            while (this.RunningPolls.Length > 10)
                this.RunningPolls = this.RunningPolls.Take(10).ToArray();

            while (!this.Bot.status.DiscordGuildDownloadCompleted)
                await Task.Delay(1000);

            foreach (var b in this.RunningPolls.ToList())
                if (!ScheduledTaskExtensions.GetScheduledTasks().ContainsTask("poll", this.Parent.Id, b.SelectUUID))
                {
                    var taskuid = "";
                    CancellationTokenSource cancellationTokenSource = new();

                    async Task VoteHandling(DiscordClient sender, ComponentInteractionCreateEventArgs e)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (e.Message?.Id == b.MessageId && e.Channel?.Id == b.ChannelId)
                            {
                                if (e.GetCustomId() == b.SelectUUID)
                                {

                                    if (b.Votes.TryGetValue(e.User.Id, out var currentVotes))
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
                                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ {CommandKey.NoPerms.Get(this.Bot.Users[e.User.Id]).Build(true)}"));
                                        return;
                                    }
                                    _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                                    this.RunningPolls = this.RunningPolls.Remove(x => x.SelectUUID, b);
                                    b.DueTime = DateTime.UtcNow;
                                    await Task.Delay(5000);
                                    this.RunningPolls = this.RunningPolls.Add(b);
                                    cancellationTokenSource.Cancel();
                                    _ = e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"✅ {CommandKey.PollEnded.Get(this.Bot.Users[e.User.Id]).Build(true)}"));
                                }
                            }
                        }).Add(this.Bot);
                    }

                    async Task MessageDeletionHandling(DiscordClient client, MessageDeleteEventArgs e)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (e.Message?.Id == b.MessageId)
                            {
                                this.Bot.DiscordClient.ComponentInteractionCreated -= VoteHandling;
                                this.Bot.DiscordClient.MessageDeleted -= MessageDeletionHandling;
                                this.Bot.DiscordClient.ChannelDeleted -= ChannelDeletionHandling;
                                this.RunningPolls = this.RunningPolls.Remove(x => x.SelectUUID, b);
                            }
                        }).Add(this.Bot);
                    }

                    async Task ChannelDeletionHandling(DiscordClient client, ChannelDeleteEventArgs e)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (e.Channel?.Id == b.ChannelId)
                            {
                                this.Bot.DiscordClient.ComponentInteractionCreated -= VoteHandling;
                                this.Bot.DiscordClient.MessageDeleted -= MessageDeletionHandling;
                                this.Bot.DiscordClient.ChannelDeleted -= ChannelDeletionHandling;
                                this.RunningPolls = this.RunningPolls.Remove(x => x.SelectUUID, b);
                            }
                        }).Add(this.Bot);
                    }

                    Func<Task> task = new(async () =>
                    {
                        cancellationTokenSource.Cancel();

                        await Task.Delay(1000);

                        this.Bot.DiscordClient.ComponentInteractionCreated -= VoteHandling;
                        this.Bot.DiscordClient.MessageDeleted -= MessageDeletionHandling;
                        this.Bot.DiscordClient.ChannelDeleted -= ChannelDeletionHandling;

                        this.RunningPolls = this.RunningPolls.Remove(x => x.SelectUUID, b);

                        var channel = await this.Bot.DiscordClient.GetChannelAsync(b.ChannelId);
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
                        _ = await message.RespondAsync(new DiscordEmbedBuilder()
                            .WithDescription($"{CommandKey.PollEnded.Get(this.Parent).Build(true)}\n\n**{CommandKey.Results.Get(this.Parent).Build()}**\n{(votes.Count <= 0 ? CommandKey.NoVotes.Get(this.Parent).Build(true) : string.Join("\n\n", votes.OrderByDescending(x => x.Value).Select(x => $"> **{b.Options[x.Key].FullSanitize()}**\n{CommandKey.Votes.Get(this.Parent).Build(true, new TVar("Count", x.Value))}")))}")
                            .WithAuthor($"{CommandKey.Poll.Get(this.Parent)} • {channel.Guild.Name}", null, channel.Guild.IconUrl)
                            .WithColor(EmbedColors.Success));
                    });

                    _ = Task.Run(async () =>
                    {
                        var LastTotalVotes = -1;

                        while (this.RunningPolls.Contains(b))
                        {
                            if (LastTotalVotes != b.Votes.Count)
                            {
                                LastTotalVotes = b.Votes.Count;

                                var channel = await this.Bot.DiscordClient.GetChannelAsync(b.ChannelId);
                                var message = await channel.GetMessageAsync(b.MessageId, true);

                                _ = await message.ModifyAsync(new DiscordEmbedBuilder(message.Embeds.ElementAt(0)).WithDescription($"> **{b.PollText}**\n\n_{CommandKey.PollEnding.Get(this.Parent).Build(new TVar("Timestamp", b.DueTime.ToTimestamp()))}._\n\n{CommandKey.TotalVotes.Get(this.Parent).Build(true, new TVar("Count", b.Votes.Count))}").Build());
                            }

                            if (cancellationTokenSource.IsCancellationRequested)
                                return;

                            try
                            { await Task.Delay(TimeSpan.FromMinutes(2), cancellationTokenSource.Token); }
                            catch { }
                        }
                    }).Add(this.Bot);

                    this.Bot.DiscordClient.ComponentInteractionCreated += VoteHandling;

                    taskuid = task.CreateScheduledTask(b.DueTime.ToUniversalTime(), new ScheduledTaskIdentifier(this.Parent.Id, b.SelectUUID, "poll"));

                    this.Bot.DiscordClient.MessageDeleted += MessageDeletionHandling;
                    this.Bot.DiscordClient.ChannelDeleted += ChannelDeletionHandling;

                    _logger.LogDebug("Created scheduled task for poll by '{Guild}'", this.Parent.Id);
                }

            foreach (var b in ScheduledTaskExtensions.GetScheduledTasks())
            {
                if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                    continue;

                if (scheduledTaskIdentifier.Snowflake == this.Parent.Id && scheduledTaskIdentifier.Type == "poll" && !this.RunningPolls.Any(x => x.SelectUUID == ((ScheduledTaskIdentifier)b.CustomData).Id))
                {
                    b.Delete();

                    _logger.LogDebug("Deleted scheduled task for poll by '{Guild}'", this.Parent.Id);
                }
            }
        });
    }
}
