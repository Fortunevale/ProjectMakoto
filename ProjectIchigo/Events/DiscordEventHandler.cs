namespace ProjectIchigo.Events;

internal class DiscordEventHandler
{
    internal DiscordEventHandler(Bot _bot)
    {
        this._bot = _bot;

        genericGuildEvents = new(_bot);
        commandEvents = new(_bot);
        afkEvents = new(_bot);
        crosspostEvents = new(_bot);
        phishingProtectionEvents = new(_bot);
        submissionEvents = new(_bot);
        discordEvents = new(_bot);
        actionlogEvents = new(_bot);
        joinEvents = new(_bot);
        bumpReminderEvents = new(_bot);
        experienceEvents = new(_bot);
        reactionRoleEvents = new(_bot);
        voicePrivacyEvents = new(_bot);
        inviteTrackerEvents = new(_bot);
        inviteNoteEvents = new(_bot);
        autoUnarchiveEvents = new(_bot);
        nameNormalizerEvents = new(_bot);
        embedMessagesEvents = new(_bot);
        tokenLeakEvents = new(_bot);
    }


    public Bot _bot { get; private set; }

    GenericGuildEvents genericGuildEvents { get; set; }
    CommandEvents commandEvents { get; set; }
    AfkEvents afkEvents { get; set; }
    CrosspostEvents crosspostEvents { get; set; }
    PhishingProtectionEvents phishingProtectionEvents { get; set; }
    PhishingSubmissionEvents submissionEvents { get; set; }
    DiscordEvents discordEvents { get; set; }
    ActionlogEvents actionlogEvents { get; set; }
    JoinEvents joinEvents { get; set; }
    BumpReminderEvents bumpReminderEvents { get; set; }
    ExperienceEvents experienceEvents { get; set; }
    ReactionRoleEvents reactionRoleEvents { get; set; }
    VoicePrivacyEvents voicePrivacyEvents { get; set; }
    InviteTrackerEvents inviteTrackerEvents { get; set; }
    InviteNoteEvents inviteNoteEvents { get; set; }
    AutoUnarchiveEvents autoUnarchiveEvents { get; set; }
    NameNormalizerEvents nameNormalizerEvents { get; set; }
    EmbedMessagesEvents embedMessagesEvents { get; set; }
    TokenLeakEvents tokenLeakEvents { get; set; }

    internal void FillDatabase(DiscordGuild guild = null, DiscordMember member = null, DiscordUser user = null)
    {
        if (guild is not null)
        {
            if (!_bot.guilds.ContainsKey(guild.Id))
                _bot.guilds.Add(guild.Id, new Guild(guild.Id, _bot));

            if (guild.Members is not null && guild.Members.Count > 0)
                foreach (var b in guild.Members)
                    if (!_bot.guilds[guild.Id].Members.ContainsKey(b.Key))
                        _bot.guilds[guild.Id].Members.Add(b.Key, new(_bot.guilds[guild.Id], b.Key));

            if (member is not null)
                if (!_bot.guilds[guild.Id].Members.ContainsKey(member.Id))
                    _bot.guilds[guild.Id].Members.Add(member.Id, new(_bot.guilds[guild.Id], member.Id));

            if (user is not null)
                if (!_bot.guilds[guild.Id].Members.ContainsKey(user.Id))
                    _bot.guilds[guild.Id].Members.Add(user.Id, new(_bot.guilds[guild.Id], user.Id));
        }

        if (member is not null)
            if (!_bot.users.ContainsKey(member.Id) && !_bot.objectedUsers.Contains(member.Id))
                _bot.users.Add(member.Id, new(_bot, member.Id));

        if (user is not null)
            if (!_bot.users.ContainsKey(user.Id) && !_bot.objectedUsers.Contains(user.Id))
                _bot.users.Add(user.Id, new(_bot, user.Id));
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            _ = genericGuildEvents.GuildMemberAdded(sender, e);
            _ = actionlogEvents.UserJoined(sender, e);
            _ = joinEvents.GuildMemberAdded(sender, e);
            _ = inviteTrackerEvents.GuildMemberAdded(sender, e);
            _ = nameNormalizerEvents.GuildMemberAdded(sender, e);
        });
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            _ = genericGuildEvents.GuildMemberRemoved(sender, e);
            _ = actionlogEvents.UserLeft(sender, e);
            _ = joinEvents.GuildMemberRemoved(sender, e);
        });
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            _ = genericGuildEvents.GuildMemberUpdated(sender, e);
            _ = actionlogEvents.MemberUpdated(sender, e);
            _ = nameNormalizerEvents.GuildMemberUpdated(sender, e);
        });
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            _ = genericGuildEvents.GuildMemberBanned(sender, e);
            _ = actionlogEvents.BanAdded(sender, e);
        });
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);

            _ = commandEvents.CommandExecuted(sender, e);
        });
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);
            _ = commandEvents.CommandError(sender, e);
        });
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            _ = afkEvents.MessageCreated(sender, e);
            _ = crosspostEvents.MessageCreated(sender, e);
            _ = phishingProtectionEvents.MessageCreated(sender, e);
            _ = bumpReminderEvents.MessageCreated(sender, e);
            _ = experienceEvents.MessageCreated(sender, e);
            _ = embedMessagesEvents.MessageCreated(sender, e);
            _ = tokenLeakEvents.MessageCreated(sender, e);

            if (!e.Message.Content.IsNullOrWhiteSpace() && (e.Message.Content == $"<@{sender.CurrentUser.Id}>" || e.Message.Content == $"<@!{sender.CurrentUser.Id}>"))
            {
                _ = e.Message.RespondAsync($"Hi {e.Author.Mention}, i'm Ichigo. I support Slash Commands, but additionally you can use me via `;;`. To get a list of all commands, type `;;help` or do a `/` and filter by me.\n" +
                                $"If you need help, feel free to join our Support and Development Server: <{_bot.status.DevelopmentServerInvite}>\n\n" +
                                $"To find out more about me, check my Github Repo: <https://s.aitsys.dev/ichigo>.");
            }
        });
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            _ = phishingProtectionEvents.MessageUpdated(sender, e);
            _ = actionlogEvents.MessageUpdated(sender, e);
            _ = tokenLeakEvents.MessageUpdated(sender, e);
        });
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Interaction.User);

            _ = submissionEvents.ComponentInteractionCreated(sender, e);
            _ = embedMessagesEvents.ComponentInteractionCreated(sender, e);
        });
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = discordEvents.GuildCreated(sender, e);
            _ = inviteTrackerEvents.GuildCreated(sender, e);
        });
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            _ = actionlogEvents.MessageDeleted(sender, e);
            _ = bumpReminderEvents.MessageDeleted(sender, e);
        });
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.MessageBulkDeleted(sender, e);
        });
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.RoleCreated(sender, e);
        });
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.RoleModified(sender, e);
        });
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.RoleDeleted(sender, e);
        });
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            _ = actionlogEvents.BanRemoved(sender, e);
        });
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.GuildAfter);

            _ = actionlogEvents.GuildUpdated(sender, e);
        });
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.ChannelCreated(sender, e);
            _ = voicePrivacyEvents.ChannelCreated(sender, e);
        });
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.ChannelDeleted(sender, e);
        });
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.ChannelUpdated(sender, e);
        });
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.InviteCreated(sender, e);
            _ = inviteTrackerEvents.InviteCreated(sender, e);
        });
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = actionlogEvents.InviteDeleted(sender, e);
            _ = inviteTrackerEvents.InviteDeleted(sender, e);
            _ = inviteNoteEvents.InviteDeleted(sender, e);
        });
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            _ = bumpReminderEvents.ReactionAdded(sender, e);
            _ = reactionRoleEvents.MessageReactionAdded(sender, e);
        });
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            _ = bumpReminderEvents.ReactionRemoved(sender, e);
            _ = reactionRoleEvents.MessageReactionRemoved(sender, e);
        });
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            _ = actionlogEvents.VoiceStateUpdated(sender, e);
            _ = voicePrivacyEvents.VoiceStateUpdated(sender, e);
        });
    }

    internal async Task ThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = e.Thread.JoinAsync();
        });
    }

    internal async Task ThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);
        });
    }

    internal async Task ThreadMemberUpdated(DiscordClient sender, ThreadMemberUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (e.Thread.Guild is not null)
                FillDatabase(e.Thread.Guild);

            _ = e.Thread.JoinAsync();
        });
    }

    internal async Task ThreadMembersUpdated(DiscordClient sender, ThreadMembersUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = e.Thread.JoinAsync();
        });
    }

    internal async Task ThreadListSynced(DiscordClient sender, ThreadListSyncEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            foreach (var b in e.Threads)
                _ = b.JoinAsync();
        });
    }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            _ = autoUnarchiveEvents.ThreadUpdated(sender, e);
        });
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            FillDatabase(user: e.UserAfter);

            _ = nameNormalizerEvents.UserUpdated(sender, e);
        });
    }
}
