// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util.Initializers;
internal sealed class SyncTasks
{
    internal static async Task GuildDownloadCompleted(Bot bot, DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Run(async () =>
        {
            bot.status.DiscordGuildDownloadCompleted = true;

            _logger.LogInfo("I'm on {GuildsCount} guilds.", e.Guilds.Count);

            Task.Run(async () =>
            {
                while (!bot.status.LavalinkInitialized)
                    await Task.Delay(1000);

                Dictionary<string, TimeSpan> VideoLengthCache = new();

                foreach (var user in bot.users)
                {
                    foreach (var list in user.Value.UserPlaylists)
                    {
                        foreach (var b in list.List.ToList())
                        {
                            if (b.Length is null || !b.Length.HasValue)
                            {
                                if (!VideoLengthCache.ContainsKey(b.Url))
                                {
                                    _logger.LogInfo("Fetching video length for '{Url}'", b.Url);

                                    var track = await bot.discordClient.GetLavalink().ConnectedNodes.First(x => x.Value.IsConnected).Value.Rest.GetTracksAsync(b.Url, LavalinkSearchType.Plain);

                                    if (track.LoadResultType != LavalinkLoadResultType.TrackLoaded)
                                    {
                                        list.List.Remove(b);
                                        _logger.LogError("Failed to load video length for '{Url}'", track.Exception, b.Url);
                                        continue;
                                    }

                                    VideoLengthCache.Add(b.Url, track.Tracks.First().Length);
                                    await Task.Delay(100);
                                }

                                b.Length = VideoLengthCache[b.Url];
                            }
                        }
                    }
                }
            }).Add(bot);

            for (int i = 0; i < 501; i++)
            {
                bot.experienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!bot.guilds.ContainsKey(guild.Key))
                    bot.guilds.Add(guild.Key, new Guild(guild.Key, bot));

                if (bot.guilds[guild.Key].BumpReminder.Enabled)
                {
                    bot.bumpReminder.ScheduleBump(sender, guild.Key);
                }

                foreach (var member in guild.Value.Members)
                {
                    if (!bot.guilds[guild.Key].Members.ContainsKey(member.Value.Id))
                    {
                        bot.guilds[guild.Key].Members.Add(member.Value.Id, new(bot.guilds[guild.Key], member.Value.Id));
                    }

                    bot.experienceHandler.CheckExperience(member.Key, guild.Value);
                }

                if (bot.guilds[guild.Key].Crosspost.CrosspostChannels.Any())
                {
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < bot.guilds[guild.Key].Crosspost.CrosspostChannels.Count; i++)
                        {
                            if (guild.Value is null)
                                return;

                            var ChannelId = bot.guilds[guild.Key].Crosspost.CrosspostChannels[i];

                            _logger.LogDebug("Checking channel '{ChannelId}' for missing crossposts..", ChannelId);

                            if (!guild.Value.Channels.ContainsKey(ChannelId))
                                return;

                            var Messages = await guild.Value.GetChannel(ChannelId).GetMessagesAsync(20);

                            if (Messages.Any(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                foreach (var msg in Messages.Where(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                {
                                    _logger.LogDebug("Handling missing crosspost message '{msg}' in '{ChannelId}' for '{guild}'..", msg.Id, msg.ChannelId, guild.Key);

                                    var WaitTime = bot.guilds[guild.Value.Id].Crosspost.DelayBeforePosting - msg.Id.GetSnowflakeTime().GetTotalSecondsSince();

                                    if (WaitTime > 0)
                                        await Task.Delay(TimeSpan.FromSeconds(WaitTime));

                                    if (bot.guilds[guild.Value.Id].Crosspost.DelayBeforePosting > 3)
                                        _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode("🕒"));

                                    bool ReactionAdded = false;

                                    var task = bot.guilds[guild.Value.Id].Crosspost.CrosspostWithRatelimit(msg.Channel, msg).ContinueWith(s =>
                                    {
                                        if (ReactionAdded)
                                            _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                    });

                                    await Task.Delay(5000);

                                    if (!task.IsCompleted)
                                    {
                                        await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                        ReactionAdded = true;
                                    }

                                    while (!task.IsCompleted)
                                        task.Wait();
                                }
                        }
                    }).Add(bot);
                }
            }

            try
            {
                await ExecuteSyncTasks(bot, bot.discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            List<DiscordUser> UserCache = new();

            await Task.Delay(5000);

            while (!bot.status.LavalinkInitialized)
                await Task.Delay(1000);

            foreach (var guild in e.Guilds)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (bot.guilds[guild.Key].MusicModule.ChannelId != 0)
                        {
                            if (!guild.Value.Channels.ContainsKey(bot.guilds[guild.Key].MusicModule.ChannelId))
                                throw new Exception("Channel no longer exists");

                            if (bot.guilds[guild.Key].MusicModule.CurrentVideo.ToLower().Contains("localhost") || bot.guilds[guild.Key].MusicModule.CurrentVideo.ToLower().Contains("127.0.0.1"))
                                throw new Exception("Localhost?");

                            var channel = guild.Value.GetChannel(bot.guilds[guild.Key].MusicModule.ChannelId);

                            if (!channel.Users.Where(x => !x.IsBot).Any())
                                throw new Exception("Channel empty");

                            if (bot.guilds[guild.Key].MusicModule.SongQueue.Count > 0)
                            {
                                for (var i = 0; i < bot.guilds[guild.Key].MusicModule.SongQueue.Count; i++)
                                {
                                    Lavalink.QueueInfo b = bot.guilds[guild.Key].MusicModule.SongQueue[i];

                                    _logger.LogDebug("Fixing queue info for '{Url}'", b.Url);

                                    b.guild = guild.Value;

                                    if (!UserCache.Any(x => x.Id == b.UserId))
                                    {
                                        _logger.LogDebug("Fetching user '{UserId}'", b.UserId);
                                        UserCache.Add(await bot.discordClient.GetUserAsync(b.UserId));
                                    }

                                    b.user = UserCache.First(x => x.Id == b.UserId);
                                }
                            }

                            var lava = bot.discordClient.GetLavalink();

                            while (!lava.ConnectedNodes.Values.Any(x => x.IsConnected))
                                await Task.Delay(1000);

                            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
                            var conn = node.GetGuildConnection(guild.Value);

                            if (conn is null)
                            {
                                if (!lava.ConnectedNodes.Any())
                                {
                                    throw new Exception("Lavalink connection isn't established.");
                                }

                                conn = await node.ConnectAsync(channel);
                            }

                            var loadResult = await node.Rest.GetTracksAsync(bot.guilds[guild.Key].MusicModule.CurrentVideo, LavalinkSearchType.Plain);

                            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                                return;

                            await conn.PlayAsync(loadResult.Tracks.First());

                            await Task.Delay(2000);
                            await conn.SeekAsync(TimeSpan.FromSeconds(bot.guilds[guild.Key].MusicModule.CurrentVideoPosition));

                            bot.guilds[guild.Key].MusicModule.QueueHandler(bot, bot.discordClient, node, conn);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred while trying to continue music playback for '{guild}'", ex, guild.Key);
                        bot.guilds[guild.Key].MusicModule = new(bot.guilds[guild.Key]);
                    }
                });

                await Task.Delay(1000);
            }
        }).Add(bot);
    }

    internal static async Task ExecuteSyncTasks(Bot bot, IReadOnlyDictionary<ulong, DiscordGuild> Guilds)
    {
        ObservableList<Task> runningTasks = new();

        void runningTasksUpdated(object sender, ObservableListUpdate<Task> e)
        {
            if (e is not null && e.NewItems is not null)
                foreach (Task b in e.NewItems)
                {
                    b.Add(bot);
                }
        }

        runningTasks.ItemsChanged += runningTasksUpdated;

        int startupTasksSuccess = 0;

        foreach (var guild in Guilds)
        {
            while (runningTasks.Count >= 4 && !runningTasks.Any(x => x.IsCompleted))
                await Task.Delay(100);

            foreach (var task in runningTasks.ToList())
                if (task.IsCompleted)
                    runningTasks.Remove(task);

            runningTasks.Add(Task.Run(async () =>
            {
                _logger.LogDebug("Performing sync tasks for '{guild}'..", guild.Key);

                if (bot.objectedUsers.Contains(guild.Value.OwnerId) || bot.bannedUsers.ContainsKey(guild.Value.OwnerId) || bot.bannedGuilds.ContainsKey(guild.Key))
                {
                    _logger.LogInfo("Leaving guild '{guild}'..", guild.Key);
                    await guild.Value.LeaveAsync();
                    return;
                }

                var guildMembers = await guild.Value.GetAllMembersAsync();
                var guildBans = await guild.Value.GetBansAsync();

                foreach (var member in guildMembers)
                {
                    if (!bot.guilds[guild.Key].Members.ContainsKey(member.Id))
                        bot.guilds[guild.Key].Members.Add(member.Id, new(bot.guilds[guild.Key], member.Id));

                    if (bot.guilds[guild.Key].Members[member.Id].FirstJoinDate == DateTime.UnixEpoch)
                        bot.guilds[guild.Key].Members[member.Id].FirstJoinDate = member.JoinedAt.UtcDateTime;

                    if (bot.guilds[guild.Key].Members[member.Id].LastLeaveDate != DateTime.UnixEpoch)
                        bot.guilds[guild.Key].Members[member.Id].LastLeaveDate = DateTime.UnixEpoch;

                    bot.guilds[guild.Key].Members[member.Id].MemberRoles = member.Roles.Select(x => new MemberRole
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToList();

                    bot.guilds[guild.Key].Members[member.Id].SavedNickname = member.Nickname;
                }

                foreach (var databaseMember in bot.guilds[guild.Key].Members.ToList())
                {
                    if (!guildMembers.Any(x => x.Id == databaseMember.Key))
                    {
                        if (bot.guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate == DateTime.UnixEpoch)
                            bot.guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate = DateTime.UtcNow;
                    }
                }

                foreach (var banEntry in guildBans)
                {
                    if (!bot.guilds[guild.Key].Members.ContainsKey(banEntry.User.Id))
                        continue;

                    if (bot.guilds[guild.Key].Members[banEntry.User.Id].MemberRoles.Count > 0)
                        bot.guilds[guild.Key].Members[banEntry.User.Id].MemberRoles.Clear();

                    if (bot.guilds[guild.Key].Members[banEntry.User.Id].SavedNickname != "")
                        bot.guilds[guild.Key].Members[banEntry.User.Id].SavedNickname = "";
                }

                if (bot.guilds[guild.Key].InviteTracker.Enabled)
                {
                    await InviteTrackerEvents.UpdateCachedInvites(bot, guild.Value);
                }

                startupTasksSuccess++;
            }));
        }

        foreach (var guild in Guilds)
        {
            try
            {
                List<DiscordThreadChannel> Threads = new();

                while (true)
                {
                    var t = await guild.Value.GetActiveThreadsAsync();

                    foreach (var b in t.ReturnedThreads.Values)
                    {
                        if (!Threads.Contains(b) && b is not null)
                            Threads.Add(b);
                    }

                    if (!t.HasMore)
                        break;

                    _logger.LogDebug("Requesting more threads for '{guild}'", guild.Key);
                }

                foreach (var b in Threads.Where(x => x.CurrentMember is null))
                {
                    _logger.LogDebug("Joining thread on '{guild}': {thread}", guild.Key, b.Id);
                    b.JoinWithQueue(bot.threadJoinClient);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to join threads on '{guild}'", ex, guild.Key);
            }
        }

        while (runningTasks.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        runningTasks.ItemsChanged -= runningTasksUpdated;
        runningTasks.Clear();

        _logger.LogInfo("Sync Tasks successfully finished for {startupTasksSuccess}/{GuildCount} guilds.", startupTasksSuccess, Guilds.Count);
        _ = bot.databaseClient.FullSyncDatabase();
    }
}
