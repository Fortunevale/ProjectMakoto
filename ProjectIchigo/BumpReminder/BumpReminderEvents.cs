namespace ProjectIchigo;

internal class BumpReminderEvents
{
    internal BumpReminderEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild is null)
                return;

            if (!_bot._guilds.List.ContainsKey(e.Guild.Id))
                _bot._guilds.List.Add(e.Guild.Id, new Guilds.ServerSettings());

            if (e.Channel.IsPrivate || !_bot._guilds.List[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _bot._guilds.List[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            DiscordUser bUser = null;

            var getuser = sender.GetUserAsync(_bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastUserId, true).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                    bUser = x.Result;
            });

            getuser.Wait(10000);

            if (!(e.Author.Id == sender.CurrentUser.Id && e.Message.Embeds.Any()))
                _bot._bumpReminder.SendPersistentMessage(sender, e.Channel, bUser);

            if (e.Author.Id != Resources.AccountIds.Disboard || !e.Message.Embeds.Any())
                return;

            if (e.Message.Embeds[0].Description.ToLower().Contains(":thumbsup:"))
            {
                _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow;
                _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow;
                _bot._guilds.List[e.Guild.Id].BumpReminderSettings.BumpsMissed = 0;

                try
                {
                    DiscordMember _bumper;

                    if (e.Message.MessageType is MessageType.ChatInputCommand)
                    {
                        _bumper = await e.Message.Interaction.User.ConvertToMember(e.Guild);
                    }
                    else
                    {
                        List<string> Mentions = e.Message.Embeds[0].Description.ToLower().GetMentions();

                        if (Mentions is null || Mentions.Count is 0)
                            throw new Exception("No mentions in message");

                        _bumper = await e.Guild.GetMemberAsync(Convert.ToUInt64(Regex.Match(Mentions.First(), @"\d+").Value));
                    }

                    _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastUserId = _bumper.Id;

                    e.Channel.SendMessageAsync($"**{_bumper.Mention} Thanks a lot for supporting the server!**\n\n" +
                                               $"_**You can subscribe and unsubscribe to the bump reminder notifications at any time by reacting to the pinned message!**_").Add(_bot._watcher);

                    try
                    {
                        if (_bot._guilds.List[e.Guild.Id].ExperienceSettings.UseExperience && _bot._guilds.List[e.Guild.Id].ExperienceSettings.BoostXpForBumpReminder)
                            _bot._experienceHandler.ModifyExperience(_bumper, e.Guild, e.Channel, 50);
                    }
                    catch { }

                    _bot._bumpReminder.ScheduleBump(sender, e.Guild.Id);
                }
                catch (Exception)
                {
                    _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastUserId = 0;
                    throw;
                }
            }
            else
            {
                if (_bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastBump < DateTime.UtcNow.AddHours(-2))
                {
                    if (e.Message.Embeds[0].Description.ToLower().Contains("please wait another"))
                    {
                        string _embedDescription = e.Message.Embeds[0].Description.ToLower();

                        try
                        {
                            _embedDescription = _embedDescription.Remove(0, _embedDescription.IndexOf(">"));
                            int _minutes = Int32.Parse(Regex.Match(_embedDescription, @"\d+").Value);

                            _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddMinutes(_minutes - 120);
                            _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddMinutes(_minutes - 120);
                            _bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastUserId = 0;

                            e.Channel.SendMessageAsync($"⚠ It seems the last bump was not registered properly.\n" +
                                $"The last time the server was bumped was determined to be around {Formatter.Timestamp(_bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}.").Add(_bot._watcher);

                            _bot._bumpReminder.ScheduleBump(sender, e.Guild.Id);
                        }
                        catch (Exception ex) { _logger.LogDebug(ex.ToString()); }
                    }
                }
            }
        }).Add(_bot._watcher);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_bot._guilds.List[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _bot._guilds.List[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _bot._guilds.List[e.Guild.Id].BumpReminderSettings.PersistentMessageId)
            {
                DiscordUser bUser = null;

                var getuser = sender.GetUserAsync(_bot._guilds.List[e.Guild.Id].BumpReminderSettings.LastUserId, true).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                        bUser = x.Result;
                });

                getuser.Wait(10000);

                _bot._bumpReminder.SendPersistentMessage(sender, e.Channel, bUser);
            }
        }).Add(_bot._watcher);
    }

    internal async Task ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_bot._guilds.List[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _bot._guilds.List[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _bot._guilds.List[e.Guild.Id].BumpReminderSettings.MessageId && e.Emoji.GetDiscordName() == "✅")
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);

                await member.GrantRoleAsync(e.Guild.GetRole(_bot._guilds.List[e.Guild.Id].BumpReminderSettings.RoleId));
            }
        }).Add(_bot._watcher);
    }

    internal async Task ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || !_bot._guilds.List[e.Guild.Id].BumpReminderSettings.Enabled || e.Channel.Id != _bot._guilds.List[e.Guild.Id].BumpReminderSettings.ChannelId)
                return;

            if (e.Message.Id == _bot._guilds.List[e.Guild.Id].BumpReminderSettings.MessageId && e.Emoji.GetDiscordName() == "✅")
            {
                var member = await e.Guild.GetMemberAsync(e.User.Id);

                await member.RevokeRoleAsync(e.Guild.GetRole(_bot._guilds.List[e.Guild.Id].BumpReminderSettings.RoleId));
            }
        }).Add(_bot._watcher);
    }
}
