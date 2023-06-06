// Project Makoto
// Copyright (C) 2023  Fortunevale
// handler program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// handler program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class DiscordEventHandler
{
    private DiscordEventHandler() { }

    public static void SetupEvents(Bot _bot)
    {
        DiscordEventHandler handler = new();

        _logger.LogDebug("Registering DisCatSharp EventHandler..");
        handler._bot = _bot;

        handler.genericGuildEvents = new(_bot);
        handler.commandEvents = new(_bot);
        handler.afkEvents = new(_bot);
        handler.crosspostEvents = new(_bot);
        handler.phishingProtectionEvents = new(_bot);
        handler.submissionEvents = new(_bot);
        handler.discordEvents = new(_bot);
        handler.actionlogEvents = new(_bot);
        handler.joinEvents = new(_bot);
        handler.bumpReminderEvents = new(_bot);
        handler.experienceEvents = new(_bot);
        handler.reactionRoleEvents = new(_bot);
        handler.voicePrivacyEvents = new(_bot);
        handler.inviteTrackerEvents = new(_bot);
        handler.inviteNoteEvents = new(_bot);
        handler.autoUnarchiveEvents = new(_bot);
        handler.nameNormalizerEvents = new(_bot);
        handler.embedMessagesEvents = new(_bot);
        handler.tokenLeakEvents = new(_bot);
        handler.vcCreatorEvents = new(_bot);
        handler.reminderEvents = new(_bot);

        _bot.discordClient.GuildCreated += handler.GuildCreated;
        _bot.discordClient.GuildUpdated += handler.GuildUpdated;

        _bot.discordClient.ChannelCreated += handler.ChannelCreated;
        _bot.discordClient.ChannelDeleted += handler.ChannelDeleted;
        _bot.discordClient.ChannelUpdated += handler.ChannelUpdated;

        _bot.discordClient.GuildMemberAdded += handler.GuildMemberAdded;
        _bot.discordClient.GuildMemberRemoved += handler.GuildMemberRemoved;
        _bot.discordClient.GuildMemberUpdated += handler.GuildMemberUpdated;
        _bot.discordClient.GuildBanAdded += handler.GuildBanAdded;
        _bot.discordClient.GuildBanRemoved += handler.GuildBanRemoved;

        _bot.discordClient.InviteCreated += handler.InviteCreated;
        _bot.discordClient.InviteDeleted += handler.InviteDeleted;

        _bot.discordClient.MessageCreated += handler.MessageCreated;
        _bot.discordClient.MessageDeleted += handler.MessageDeleted;
        _bot.discordClient.MessagesBulkDeleted += handler.MessagesBulkDeleted;
        _bot.discordClient.MessageUpdated += handler.MessageUpdated;

        _bot.discordClient.MessageReactionAdded += handler.MessageReactionAdded;
        _bot.discordClient.MessageReactionRemoved += handler.MessageReactionRemoved;

        _bot.discordClient.ComponentInteractionCreated += handler.ComponentInteractionCreated;

        _bot.discordClient.GuildRoleCreated += handler.GuildRoleCreated;
        _bot.discordClient.GuildRoleDeleted += handler.GuildRoleDeleted;
        _bot.discordClient.GuildRoleUpdated += handler.GuildRoleUpdated;

        _bot.discordClient.VoiceStateUpdated += handler.VoiceStateUpdated;

        _bot.discordClient.ThreadCreated += handler.ThreadCreated;
        _bot.discordClient.ThreadDeleted += handler.ThreadDeleted;
        _bot.discordClient.ThreadMemberUpdated += handler.ThreadMemberUpdated;
        _bot.discordClient.ThreadMembersUpdated += handler.ThreadMembersUpdated;
        _bot.discordClient.ThreadUpdated += handler.ThreadUpdated;
        _bot.discordClient.ThreadListSynced += handler.ThreadListSynced;
        _bot.discordClient.UserUpdated += handler.UserUpdated;

        _bot.discordClient.GetCommandsNext().CommandExecuted += handler.CommandExecuted;
        _bot.discordClient.GetCommandsNext().CommandErrored += handler.CommandError;
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
    VcCreatorEvents vcCreatorEvents { get; set; }
    AutoUnarchiveEvents autoUnarchiveEvents { get; set; }
    NameNormalizerEvents nameNormalizerEvents { get; set; }
    EmbedMessagesEvents embedMessagesEvents { get; set; }
    TokenLeakEvents tokenLeakEvents { get; set; }
    ReminderEvents reminderEvents { get; set; }

    private void FillDatabase(DiscordGuild guild = null, DiscordMember member = null, DiscordUser user = null)
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
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            genericGuildEvents.GuildMemberAdded(sender, e).Add(_bot.watcher);
            actionlogEvents.UserJoined(sender, e).Add(_bot.watcher);
            joinEvents.GuildMemberAdded(sender, e).Add(_bot.watcher);
            inviteTrackerEvents.GuildMemberAdded(sender, e).Add(_bot.watcher);
            nameNormalizerEvents.GuildMemberAdded(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            genericGuildEvents.GuildMemberRemoved(sender, e).Add(_bot.watcher);
            actionlogEvents.UserLeft(sender, e).Add(_bot.watcher);
            joinEvents.GuildMemberRemoved(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            genericGuildEvents.GuildMemberUpdated(sender, e).Add(_bot.watcher);
            actionlogEvents.MemberUpdated(sender, e).Add(_bot.watcher);
            nameNormalizerEvents.GuildMemberUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            genericGuildEvents.GuildMemberBanned(sender, e).Add(_bot.watcher);
            actionlogEvents.BanAdded(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);

            commandEvents.CommandExecuted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);
            commandEvents.CommandError(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            afkEvents.MessageCreated(sender, e).Add(_bot.watcher);
            crosspostEvents.MessageCreated(sender, e).Add(_bot.watcher);
            phishingProtectionEvents.MessageCreated(sender, e).Add(_bot.watcher);
            bumpReminderEvents.MessageCreated(sender, e).Add(_bot.watcher);
            experienceEvents.MessageCreated(sender, e).Add(_bot.watcher);
            embedMessagesEvents.MessageCreated(sender, e).Add(_bot.watcher);
            tokenLeakEvents.MessageCreated(sender, e).Add(_bot.watcher);

            if (!e.Message.Content.IsNullOrWhiteSpace() && (e.Message.Content == $"<@{sender.CurrentUser.Id}>" || e.Message.Content == $"<@!{sender.CurrentUser.Id}>"))
            {
                string prefix;

                try
                {
                    prefix = _bot.guilds[e.Guild.Id].PrefixSettings.Prefix.IsNullOrWhiteSpace() ? ";;" : _bot.guilds[e.Guild.Id].PrefixSettings.Prefix;
                }
                catch (Exception)
                {
                    prefix = ";;";
                }

                _ = e.Message.RespondAsync($"Hi {e.Author.Mention}, i'm Makoto. I support Slash Commands, but additionally you can use me via `{prefix}`. To get a list of all commands, type `;;help` or do a `/` and filter by me.\n" +
                                $"If you need help, feel free to join our Support and Development Server: <{_bot.status.DevelopmentServerInvite}>\n\n" +
                                $"To find out more about me, check my Github Repo: <https://s.aitsys.dev/makoto>.");
            }
        }).Add(_bot.watcher);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            phishingProtectionEvents.MessageUpdated(sender, e).Add(_bot.watcher);
            actionlogEvents.MessageUpdated(sender, e).Add(_bot.watcher);
            tokenLeakEvents.MessageUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Interaction.User);

            submissionEvents.ComponentInteractionCreated(sender, e).Add(_bot.watcher);
            embedMessagesEvents.ComponentInteractionCreated(sender, e).Add(_bot.watcher);
            reminderEvents.ComponentInteractionCreated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            discordEvents.GuildCreated(sender, e).Add(_bot.watcher);
            inviteTrackerEvents.GuildCreated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            actionlogEvents.MessageDeleted(sender, e).Add(_bot.watcher);
            bumpReminderEvents.MessageDeleted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.MessageBulkDeleted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.RoleCreated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.RoleModified(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.RoleDeleted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            actionlogEvents.BanRemoved(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.GuildAfter);

            actionlogEvents.GuildUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.ChannelCreated(sender, e).Add(_bot.watcher);
            voicePrivacyEvents.ChannelCreated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.ChannelDeleted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.ChannelUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.InviteCreated(sender, e).Add(_bot.watcher);
            inviteTrackerEvents.InviteCreated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            actionlogEvents.InviteDeleted(sender, e).Add(_bot.watcher);
            inviteTrackerEvents.InviteDeleted(sender, e).Add(_bot.watcher);
            inviteNoteEvents.InviteDeleted(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            bumpReminderEvents.ReactionAdded(sender, e).Add(_bot.watcher);
            reactionRoleEvents.MessageReactionAdded(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            bumpReminderEvents.ReactionRemoved(sender, e).Add(_bot.watcher);
            reactionRoleEvents.MessageReactionRemoved(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            actionlogEvents.VoiceStateUpdated(sender, e).Add(_bot.watcher);
            voicePrivacyEvents.VoiceStateUpdated(sender, e).Add(_bot.watcher);
            vcCreatorEvents.VoiceStateUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            e.Thread.JoinWithQueue(_bot.threadJoinClient);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadMemberUpdated(DiscordClient sender, ThreadMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Thread.Guild is not null)
                FillDatabase(e.Thread.Guild);

            e.Thread.JoinWithQueue(_bot.threadJoinClient);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadMembersUpdated(DiscordClient sender, ThreadMembersUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            e.Thread.JoinWithQueue(_bot.threadJoinClient);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadListSynced(DiscordClient sender, ThreadListSyncEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            if (_bot.status.DiscordGuildDownloadCompleted)
                foreach (var b in e.Threads)
                    b.JoinWithQueue(_bot.threadJoinClient);
        }).Add(_bot.watcher);
    }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            autoUnarchiveEvents.ThreadUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(user: e.UserAfter);

            nameNormalizerEvents.UserUpdated(sender, e).Add(_bot.watcher);
        }).Add(_bot.watcher);
    }
}
