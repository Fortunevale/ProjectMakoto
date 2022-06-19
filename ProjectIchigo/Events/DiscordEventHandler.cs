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

    internal void FillDatabase(DiscordGuild guild = null, DiscordMember member = null, DiscordUser user = null)
    {
        if (guild is not null)
            if (!_bot._guilds.List.ContainsKey(guild.Id))
                _bot._guilds.List.Add(guild.Id, new Guilds.ServerSettings());

        if (guild.Members is not null && guild.Members.Count > 0)
            foreach(var b in guild.Members)
                if (!_bot._guilds.List[ guild.Id ].Members.ContainsKey(b.Key))
                    _bot._guilds.List[ guild.Id ].Members.Add(b.Key, new());

        if (member is not null && guild is not null)
            if (!_bot._guilds.List[ guild.Id ].Members.ContainsKey(member.Id))
                _bot._guilds.List[ guild.Id ].Members.Add(member.Id, new());

        if (user is not null && guild is not null)
            if (!_bot._guilds.List[ guild.Id ].Members.ContainsKey(user.Id))
                _bot._guilds.List[ guild.Id ].Members.Add(user.Id, new());

        if (member is not null)
            if (!_bot._users.List.ContainsKey(member.Id))
                _bot._users.List.Add(member.Id, new(_bot));

        if (user is not null)
            if (!_bot._users.List.ContainsKey(user.Id))
                _bot._users.List.Add(user.Id, new(_bot));
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        FillDatabase(e.Guild, e.Member);

        _ = genericGuildEvents.GuildMemberAdded(sender, e);
        _ = actionlogEvents.UserJoined(sender, e);
        _ = joinEvents.GuildMemberAdded(sender, e);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        FillDatabase(e.Guild, e.Member);

        _ = genericGuildEvents.GuildMemberRemoved(sender, e);
        _ = actionlogEvents.UserLeft(sender, e);
        _ = joinEvents.GuildMemberRemoved(sender, e);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        FillDatabase(e.Guild, e.Member);

        _ = genericGuildEvents.GuildMemberUpdated(sender, e);
        _ = actionlogEvents.MemberUpdated(sender, e);
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        FillDatabase(e.Guild, e.Member);

        _ = genericGuildEvents.GuildMemberBanned(sender, e);
        _ = actionlogEvents.BanAdded(sender, e);
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        FillDatabase(e.Context.Guild, e.Context.Member);

        _ = commandEvents.CommandExecuted(sender, e);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        FillDatabase(e.Context.Guild, e.Context.Member);
        _ = commandEvents.CommandError(sender, e);
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        FillDatabase(e.Guild, user: e.Message.Author);

        _ = afkEvents.MessageCreated(sender, e);
        _ = crosspostEvents.MessageCreated(sender, e);
        _ = phishingProtectionEvents.MessageCreated(sender, e);
        _ = bumpReminderEvents.MessageCreated(sender, e);
        _ = experienceEvents.MessageCreated(sender, e);

        if (!e.Message.Content.IsNullOrWhiteSpace() && (e.Message.Content == $"<@{sender.CurrentUser.Id}>" || e.Message.Content == $"<@!{sender.CurrentUser.Id}>"))
        {
            _ = e.Message.RespondAsync($"Hi {e.Author.Mention}, i'm Project Ichigo. My prefix is `;;`. To get help, type `;;help`.\n" +
                                    $"If you need help, feel free to join our Support and Development Server: https://discord.gg/SaHT4GPGyW\n\n" +
                                    $"To find out more about me, check my Github Repo: <https://bit.ly/38vWpaj>.");
        }
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        FillDatabase(e.Guild, user: e.Message.Author);

        _ = phishingProtectionEvents.MessageUpdated(sender, e);
        _ = actionlogEvents.MessageUpdated(sender, e);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        FillDatabase(e.Guild, user: e.Interaction.User);

        _ = submissionEvents.ComponentInteractionCreated(sender, e);
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = discordEvents.GuildCreated(sender, e);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        FillDatabase(e.Guild, user: e.Message.Author);

        _ = actionlogEvents.MessageDeleted(sender, e);
        _ = bumpReminderEvents.MessageDeleted(sender, e);
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.MessageBulkDeleted(sender, e);
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.RoleCreated(sender, e);
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.RoleModified(sender, e);
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.RoleDeleted(sender, e);
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        FillDatabase(e.Guild, e.Member);

        _ = actionlogEvents.BanRemoved(sender, e);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        FillDatabase(e.GuildAfter);

        _ = actionlogEvents.GuildUpdated(sender, e);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.ChannelCreated(sender, e);
        _ = voicePrivacyEvents.ChannelCreated(sender, e);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.ChannelDeleted(sender, e);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.ChannelUpdated(sender, e);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.InviteCreated(sender, e);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        FillDatabase(e.Guild);

        _ = actionlogEvents.InviteDeleted(sender, e);
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        FillDatabase(e.Guild, user: e.User);

        _ = bumpReminderEvents.ReactionAdded(sender, e);
        _ = reactionRoleEvents.MessageReactionAdded(sender, e);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        FillDatabase(e.Guild, user: e.User);

        _ = bumpReminderEvents.ReactionRemoved(sender, e);
        _ = reactionRoleEvents.MessageReactionRemoved(sender, e);
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        FillDatabase(e.Guild, user: e.User);

        _ = actionlogEvents.VoiceStateUpdated(sender, e);
        _ = voicePrivacyEvents.VoiceStateUpdated(sender, e);
    }
}
