namespace Project_Ichigo.BumpReminder;

internal class BumpReminderEvents
{
    TaskWatcher.TaskWatcher _watcher { get; set; }
    ServerInfo _guilds { get; set; }
    BumpReminder _reminder { get; set; }

    internal BumpReminderEvents(TaskWatcher.TaskWatcher watcher, ServerInfo _guilds, BumpReminder _reminder)
    {
        _watcher = watcher;
        this._guilds = _guilds;
        this._reminder = _reminder;
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null)
                return;

            if (!_guilds.Servers.ContainsKey(e.Guild.Id))
                _guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (e.Channel.IsPrivate || !_guilds.Servers[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _guilds.Servers[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            DiscordUser bUser = null;

            var getuser = sender.GetUserAsync(_guilds.Servers[e.Guild.Id].BumpReminderSettings.LastUserId, true).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                    bUser = x.Result;
            });

            getuser.Wait(10000);

            if (!(e.Author.Id == sender.CurrentUser.Id && e.Message.Embeds.Any()))
                _reminder.SendPersistentMessage(sender, e.Channel, bUser);

            if (e.Author.Id != Resources.AccountIds.Disboard || !e.Message.Embeds.Any())
                return;

            if (e.Message.Embeds[0].Description.ToLower().Contains("bump done"))
            {
                List<string> Mentions = e.Message.Embeds[0].Description.ToLower().GetMentions();

                if (Mentions.Count != 1)
                    return;

                var _bumper = await e.Guild.GetMemberAsync(Convert.ToUInt64(Regex.Match(Mentions.First(), @"\d+").Value));

                _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow;
                _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow;
                _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastUserId = _bumper.Id;

                e.Channel.SendMessageAsync($"**{_bumper.Mention} Thanks a lot for supporting the server!**\n\n" +
                                           $"_**You can subscribe and unsubscribe to the bump reminder notifications at any time by reacting to the pinned message!**_").Add(_watcher);

                _reminder.ScheduleBump(sender, e.Guild.Id);
            }
            else
            {
                if (_guilds.Servers[e.Guild.Id].BumpReminderSettings.LastBump > DateTime.UtcNow.AddHours(2))
                {
                    if (e.Message.Embeds[0].Description.ToLower().Contains("please wait another"))
                    {
                        string _embedDescription = e.Message.Embeds[0].Description.ToLower();

                        try
                        {
                            _embedDescription = _embedDescription.Remove(0, _embedDescription.IndexOf(">"));
                            int _minutes = Int32.Parse(Regex.Match(_embedDescription, @"\d+").Value);

                            _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddMinutes(_minutes - 120);
                            _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddMinutes(_minutes - 120);
                            _guilds.Servers[e.Guild.Id].BumpReminderSettings.LastUserId = 0;

                            e.Channel.SendMessageAsync($":warning: It seems the last bump was not registered properly.\n" +
                                $"The last time the server was bumped was determined to be around {Formatter.Timestamp(_guilds.Servers[e.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}.").Add(_watcher);

                            _reminder.ScheduleBump(sender, e.Guild.Id);
                        }
                        catch (Exception ex) { LogDebug(ex.ToString()); }
                    }
                }
            }
        }).Add(_watcher);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_guilds.Servers[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _guilds.Servers[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _guilds.Servers[e.Guild.Id].BumpReminderSettings.PersistentMessageId)
            {
                DiscordUser bUser = null;

                var getuser = sender.GetUserAsync(_guilds.Servers[e.Guild.Id].BumpReminderSettings.LastUserId, true).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                        bUser = x.Result;
                });

                getuser.Wait(10000);

                _reminder.SendPersistentMessage(sender, e.Channel, bUser);
            }
        }).Add(_watcher);
    }

    internal async Task ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_guilds.Servers[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _guilds.Servers[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _guilds.Servers[e.Guild.Id].BumpReminderSettings.MessageId && e.Emoji.GetDiscordName() == ":white_check_mark:")
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);

                await member.GrantRoleAsync(e.Guild.GetRole(_guilds.Servers[e.Guild.Id].BumpReminderSettings.RoleId));
            }
        }).Add(_watcher);
    }

    internal async Task ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_guilds.Servers[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _guilds.Servers[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _guilds.Servers[e.Guild.Id].BumpReminderSettings.MessageId && e.Emoji.GetDiscordName() == ":white_check_mark:")
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);

                await member.RevokeRoleAsync(e.Guild.GetRole(_guilds.Servers[e.Guild.Id].BumpReminderSettings.RoleId));
            }
        }).Add(_watcher);
    }
}
