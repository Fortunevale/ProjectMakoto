// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Util.Initializers;
internal static class SyncTasks
{
    internal static async Task GuildDownloadCompleted(Bot bot, DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            bot.status.DiscordGuildDownloadCompleted = true;

            Log.Information("I'm on {GuildsCount} guilds.", e.Guilds.Count);

            _ = Task.Run(async () =>
            {
                foreach (var user in bot.Users)
                {
                    var userCache = new Dictionary<ulong, DiscordUser?>();

                    if (user.Value.LegacyBlockedUsers.Length > 0)
                    {
                        for (var i = 0; i < user.Value.LegacyBlockedUsers.Length; i++)
                        {
                            var b = user.Value.LegacyBlockedUsers[i];

                            if (!userCache.TryGetValue(b, out var victim))
                            {
                                if (bot.DiscordClient.GetFirstShard().TryGetUser(b, out var fetched))
                                    userCache.Add(b, fetched);
                                else
                                    userCache.Add(b, null);

                                victim = userCache[b];
                            }

                            if (victim is null || victim.Id == bot.DiscordClient.CurrentUser.Id || victim.Id == user.Key || victim.IsBot || (victim.Flags?.HasFlag(UserFlags.Staff) ?? false))
                            {
                                Log.Debug("Removing '{victim}' from '{owner}' blocklist", b, user.Value.Id);
                                i--;
                                user.Value.LegacyBlockedUsers = user.Value.LegacyBlockedUsers.Remove(x => x.ToString(), b);
                            }
                        }
                    }
                }
            }).Add(bot);

            for (var i = 0; i < 501; i++)
            {
                _ = bot.ExperienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!bot.Guilds.ContainsKey(guild.Key))
                    bot.Guilds.Add(guild.Key, new Guild(bot, guild.Key));

                if (bot.Guilds[guild.Key].BumpReminder.ChannelId != 0)
                {
                    bot.BumpReminder.ScheduleBump(sender, guild.Key);
                }

                if (bot.Guilds[guild.Key].Crosspost.CrosspostChannels.Length != 0)
                {
                    _ = Task.Run(async () =>
                    {
                        for (var i = 0; i < bot.Guilds[guild.Key].Crosspost.CrosspostChannels.Length; i++)
                        {
                            if (guild.Value is null)
                                return;

                            var ChannelId = bot.Guilds[guild.Key].Crosspost.CrosspostChannels[i];

                            Log.Debug("Checking channel '{ChannelId}' for missing crossposts..", ChannelId);

                            if (!guild.Value.Channels.ContainsKey(ChannelId))
                                return;

                            var Messages = await guild.Value.GetChannel(ChannelId).GetMessagesAsync(20);

                            if (Messages.Any(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                foreach (var msg in Messages.Where(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                {
                                    Log.Debug("Handling missing crosspost message '{msg}' in '{ChannelId}' for '{guild}'..", msg.Id, msg.ChannelId, guild.Key);

                                    var WaitTime = bot.Guilds[guild.Value.Id].Crosspost.DelayBeforePosting - msg.Id.GetSnowflakeTime().GetTotalSecondsSince();

                                    if (WaitTime > 0)
                                        await Task.Delay(TimeSpan.FromSeconds(WaitTime));

                                    if (bot.Guilds[guild.Value.Id].Crosspost.DelayBeforePosting > 3)
                                        _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode("🕒"));

                                    await bot.Guilds[guild.Key].Crosspost.CrosspostWithRatelimit(sender, msg);
                                }
                        }
                    }).Add(bot);
                }
            }

            _ = BasePlugin.RaisePreSyncTasksExecution(bot, e.Guilds.Values.Where(x => x != null));

            try
            {
                await ExecuteSyncTasks(bot, bot.DiscordClient);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to run sync tasks");
            }

            _ = BasePlugin.RaisePostSyncTasksExecution(bot, e.Guilds.Values.Where(x => x != null));
        }).Add(bot);
    }

    internal static async Task ExecuteSyncTasks(Bot bot, DiscordShardedClient shardedClient)
    {
        var Guilds = shardedClient.GetGuilds();

        ObservableList<Task> runningTasks = new();

        void runningTasksUpdated(object sender, ObservableListUpdate<Task> e)
        {
            if (e is not null && e.NewItems is not null)
                foreach (var b in e.NewItems)
                {
                    _ = b.Add(bot);
                }
        }

        runningTasks.ItemsChanged += runningTasksUpdated;

        var startupTasksSuccess = 0;

        foreach (var guild in Guilds)
        {
            while (runningTasks.Count >= 4 && !runningTasks.Any(x => x.IsCompleted))
                await Task.Delay(100);

            foreach (var task in runningTasks.ToList())
                if (task.IsCompleted)
                    _ = runningTasks.Remove(task);

            runningTasks.Add(Task.Run(async () =>
            {
                Log.Debug("Performing sync tasks for '{guild}'..", guild.Key);

                if (bot.objectedUsers.Contains(guild.Value.OwnerId.Value) || bot.bannedUsers.ContainsKey(guild.Value.OwnerId.Value) || bot.bannedGuilds.ContainsKey(guild.Key))
                {
                    Log.Information("Leaving guild '{guild}'..", guild.Key);
                    await guild.Value.LeaveAsync();
                    return;
                }

                var guildMembers = await guild.Value.GetAllMembersAsync();
                var guildBans = await guild.Value.GetBansAsync();

                foreach (var member in guildMembers)
                {
                    bot.ExperienceHandler.CheckExperience(member.Id, guild.Value);

                    if (bot.Guilds[guild.Key].Members[member.Id].FirstJoinDate == DateTime.MinValue)
                        bot.Guilds[guild.Key].Members[member.Id].FirstJoinDate = member.JoinedAt.UtcDateTime;

                    if (bot.Guilds[guild.Key].Members[member.Id].LastLeaveDate != DateTime.MinValue)
                        bot.Guilds[guild.Key].Members[member.Id].LastLeaveDate = DateTime.MinValue;

                    bot.Guilds[guild.Key].Members[member.Id].MemberRoles = member.Roles.Select(x => new MemberRole()
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToArray();

                    bot.Guilds[guild.Key].Members[member.Id].SavedNickname = member.Nickname;

                    await bot.Guilds[guild.Key].Members[member.Id].PerformAutoKickChecks(guild.Value, member);
                }

                foreach (var databaseMember in bot.Guilds[guild.Key].Members)
                {
                    if (!guildMembers.Any(x => x.Id == databaseMember.Key))
                    {
                        if (bot.Guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate == DateTime.MinValue)
                            bot.Guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate = DateTime.UtcNow;
                    }
                }

                foreach (var banEntry in guildBans)
                {
                    if (!bot.Guilds[guild.Key].Members.ContainsKey(banEntry.User.Id))
                        continue;

                    if (bot.Guilds[guild.Key].Members[banEntry.User.Id].MemberRoles.Length > 0)
                        bot.Guilds[guild.Key].Members[banEntry.User.Id].MemberRoles = Array.Empty<MemberRole>();

                    if (bot.Guilds[guild.Key].Members[banEntry.User.Id].SavedNickname != "")
                        bot.Guilds[guild.Key].Members[banEntry.User.Id].SavedNickname = "";
                }

                if (bot.Guilds[guild.Key].InviteTracker.Enabled)
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

                    Log.Debug("Requesting more threads for '{guild}'", guild.Key);
                }

                foreach (var b in Threads.Where(x => x.CurrentMember is null))
                {
                    Log.Debug("Joining thread on '{guild}': {thread}", guild.Key, b.Id);
                    b.JoinWithQueue(bot.ThreadJoinClient);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to join threads on '{guild}'", guild.Key);
            }
        }

        while (runningTasks.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        runningTasks.ItemsChanged -= runningTasksUpdated;
        runningTasks.Clear();

        Log.Information("Sync Tasks successfully finished for {startupTasksSuccess}/{GuildCount} guilds.", startupTasksSuccess, Guilds.Count);
    }
}
