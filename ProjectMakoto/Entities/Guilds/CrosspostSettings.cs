// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class CrosspostSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("crosspostdelay"), ColumnType(ColumnTypes.Int), Default("5")]
    public int DelayBeforePosting
    {
        get => this.Bot.DatabaseClient.GetValue<int>("guilds", "serverid", this.Parent.Id, "crosspostdelay", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "crosspostdelay", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("crosspostexcludebots"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool ExcludeBots
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "crosspostexcludebots", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "crosspostexcludebots", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("crosspostchannels"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public ulong[] CrosspostChannels
    {
        get => JsonConvert.DeserializeObject<ulong[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "crosspostchannels", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "crosspostchannels", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("crosspost_ratelimits"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public CrosspostRatelimit[] CrosspostRatelimits
    {
        get => JsonConvert.DeserializeObject<CrosspostRatelimit[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "crosspost_ratelimits", this.Bot.DatabaseClient.mainDatabaseConnection))
            .Select(x =>
            {
                x.Bot = this.Bot;
                x.Parent = this.Parent;

                return x;
            }).ToArray();
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "crosspost_ratelimits", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    private bool QueueInitialized = false;
    private Dictionary<DiscordMessage, DiscordChannel> _queue = new();

    public async Task CrosspostQueue()
    {
        this.QueueInitialized = true;
        _logger.LogDebug("Initializing crosspost queue for '{Guild}'", this.Parent.Id);

        while (true)
        {
            DiscordChannel channel;
            DiscordMessage message;

            try
            {
                while (this._queue.Count == 0)
                    await Task.Delay(1000);

                var keyValuePair = this._queue.First();
                channel = keyValuePair.Value;
                message = keyValuePair.Key;
            }
            catch (Exception)
            {
                this._queue ??= new();
                continue;
            }

            try
            {
                if (!this.CrosspostRatelimits.Any(x => x.Id == channel.Id))
                {
                    _logger.LogDebug("Initialized new crosspost ratelimit for '{Channel}'", channel.Id);
                    this.CrosspostRatelimits = this.CrosspostRatelimits.Add(new()
                    {
                        Id = channel.Id,
                    });
                }

                var r = this.CrosspostRatelimits.First(x => x.Id == channel.Id);

                _logger.LogDebug("Crosspost Ratelimit '{Channel}': First: {First}; Remaining: {Remaining}", channel.Id, r.FirstPost, r.PostsRemaining);

                async Task Crosspost()
                {
                    if (message.Flags?.HasMessageFlag(MessageFlags.Crossposted) ?? false)
                        return;

                    r.PostsRemaining--;
                    var crossPostTask = channel.CrosspostMessageAsync(message);

                    Stopwatch sw = new();
                    sw.Start();
                    while (!crossPostTask.IsCompleted && sw.ElapsedMilliseconds < 3000)
                        await Task.Delay(50);
                    sw.Stop();

                    _logger.LogDebug("It took {Milliseconds}ms to process a crosspost", sw.ElapsedMilliseconds);

                    if (!crossPostTask.IsCompleted)
                    {
                        _logger.LogWarn("Crosspost Ratelimit tripped for '{Channel}': {Message}", channel.Id, message.Id);

                        r.FirstPost = DateTime.UtcNow;
                        r.PostsRemaining = 0;
                    }

                    _ = await crossPostTask;

                    _ = this._queue.Remove(message);
                    _logger.LogDebug("Crossposted message in '{Channel}': {Message}", channel.Id, message.Id);
                }

                void ResetLimits()
                {
                    r.PostsRemaining = 10;
                    r.FirstPost = DateTime.UtcNow;
                }

                if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() <= 0)
                {
                    _logger.LogDebug("First crosspost for '{Channel}' was at {FirstPost}, resetting crosspost availability", channel.Id, r.FirstPost.AddHours(1));
                    ResetLimits();
                }

                if (r.PostsRemaining > 0)
                {
                    _logger.LogDebug("{Remaining} crossposts available for '{Channel}', allowing request", r.PostsRemaining, channel.Id);
                    await Crosspost();
                    continue;
                }

                if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
                {
                    _logger.LogDebug("No crossposts available for '{Channel}', waiting until {WaitUntil} ({WaitUntilSec} seconds)", channel.Id, r.FirstPost.AddHours(1), r.FirstPost.AddHours(1).GetTotalSecondsUntil());
                    await Task.Delay(r.FirstPost.AddHours(1).GetTimespanUntil());
                }

                ResetLimits();

                _logger.LogDebug("Crossposts for '{Channel}' available again, allowing request. {Remaining} requests remaining, first post at {First}.", channel.Id, r.PostsRemaining, r.FirstPost);
                await Crosspost();
                continue;
            }
            catch (Exception ex)
            {
                _ = this._queue.Remove(message);
                _logger.LogError("Failed to process crosspost queue", ex);
            }
        }
    }

    public async Task CrosspostWithRatelimit(DiscordClient client, DiscordMessage message)
    {
        if (message.Reference is not null || message.MessageType is not MessageType.Default)
            return;

        if (this.Parent.Crosspost.ExcludeBots)
            if (message.WebhookMessage || message.Author.IsBot)
                return;

        var ReactionAdded = false;

        if (!this.QueueInitialized)
            _ = this.CrosspostQueue();

        this._queue.Add(message, message.Channel);

        await Task.Delay(5000);

        if (this._queue.ContainsKey(message))
        {
            if (!ReactionAdded)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(client, 974029756355977216));
                ReactionAdded = true;
            }
        }

        while (this._queue.ContainsKey(message))
        {
            await Task.Delay(1000);
        }

        if (ReactionAdded)
            _ = message.DeleteReactionsEmojiAsync(DiscordEmoji.FromGuildEmote(client, 974029756355977216));
    }
}
