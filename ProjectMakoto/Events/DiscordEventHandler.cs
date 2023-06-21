// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class DiscordEventHandler : RequiresBotReference
{
    private DiscordEventHandler(Bot bot) : base(bot) { }

    public static void SetupEvents(Bot _bot)
    {
        DiscordEventHandler handler = new(_bot);

        _logger.LogDebug("Registering DisCatSharp EventHandler..");
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
        handler.userBlockEvents = new(_bot);

        _bot.DiscordClient.GuildCreated += handler.GuildCreated;
        _bot.DiscordClient.GuildUpdated += handler.GuildUpdated;

        _bot.DiscordClient.ChannelCreated += handler.ChannelCreated;
        _bot.DiscordClient.ChannelDeleted += handler.ChannelDeleted;
        _bot.DiscordClient.ChannelUpdated += handler.ChannelUpdated;

        _bot.DiscordClient.GuildMemberAdded += handler.GuildMemberAdded;
        _bot.DiscordClient.GuildMemberRemoved += handler.GuildMemberRemoved;
        _bot.DiscordClient.GuildMemberUpdated += handler.GuildMemberUpdated;
        _bot.DiscordClient.GuildBanAdded += handler.GuildBanAdded;
        _bot.DiscordClient.GuildBanRemoved += handler.GuildBanRemoved;

        _bot.DiscordClient.InviteCreated += handler.InviteCreated;
        _bot.DiscordClient.InviteDeleted += handler.InviteDeleted;

        _bot.DiscordClient.MessageCreated += handler.MessageCreated;
        _bot.DiscordClient.MessageDeleted += handler.MessageDeleted;
        _bot.DiscordClient.MessagesBulkDeleted += handler.MessagesBulkDeleted;
        _bot.DiscordClient.MessageUpdated += handler.MessageUpdated;

        _bot.DiscordClient.MessageReactionAdded += handler.MessageReactionAdded;
        _bot.DiscordClient.MessageReactionRemoved += handler.MessageReactionRemoved;

        _bot.DiscordClient.ComponentInteractionCreated += handler.ComponentInteractionCreated;

        _bot.DiscordClient.GuildRoleCreated += handler.GuildRoleCreated;
        _bot.DiscordClient.GuildRoleDeleted += handler.GuildRoleDeleted;
        _bot.DiscordClient.GuildRoleUpdated += handler.GuildRoleUpdated;

        _bot.DiscordClient.VoiceStateUpdated += handler.VoiceStateUpdated;

        _bot.DiscordClient.ThreadCreated += handler.ThreadCreated;
        _bot.DiscordClient.ThreadDeleted += handler.ThreadDeleted;
        _bot.DiscordClient.ThreadMemberUpdated += handler.ThreadMemberUpdated;
        _bot.DiscordClient.ThreadMembersUpdated += handler.ThreadMembersUpdated;
        _bot.DiscordClient.ThreadUpdated += handler.ThreadUpdated;
        _bot.DiscordClient.ThreadListSynced += handler.ThreadListSynced;
        _bot.DiscordClient.UserUpdated += handler.UserUpdated;

        _bot.DiscordClient.GetCommandsNext().CommandExecuted += handler.CommandExecuted;
        _bot.DiscordClient.GetCommandsNext().CommandErrored += handler.CommandError;
    }

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
    UserBlockEvents userBlockEvents { get; set; }

    private void FillDatabase(DiscordGuild guild = null, DiscordMember member = null, DiscordUser user = null)
    {
        if (guild is not null)
        {
            if (!this.Bot.Guilds.ContainsKey(guild.Id))
                this.Bot.Guilds.Add(guild.Id, new Guild(guild.Id, this.Bot));

            if (guild.Members is not null && guild.Members.Count > 0)
                foreach (var b in guild.Members)
                    if (!this.Bot.Guilds[guild.Id].Members.ContainsKey(b.Key))
                        this.Bot.Guilds[guild.Id].Members.Add(b.Key, new(this.Bot, this.Bot.Guilds[guild.Id], b.Key));

            if (member is not null)
                if (!this.Bot.Guilds[guild.Id].Members.ContainsKey(member.Id))
                    this.Bot.Guilds[guild.Id].Members.Add(member.Id, new(this.Bot, this.Bot.Guilds[guild.Id], member.Id));

            if (user is not null)
                if (!this.Bot.Guilds[guild.Id].Members.ContainsKey(user.Id))
                    this.Bot.Guilds[guild.Id].Members.Add(user.Id, new(this.Bot, this.Bot.Guilds[guild.Id], user.Id));
        }

        if (member is not null)
            if (!this.Bot.Users.ContainsKey(member.Id) && !this.Bot.objectedUsers.Contains(member.Id))
                this.Bot.Users.Add(member.Id, new(this.Bot, member.Id));

        if (user is not null)
            if (!this.Bot.Users.ContainsKey(user.Id) && !this.Bot.objectedUsers.Contains(user.Id))
                this.Bot.Users.Add(user.Id, new(this.Bot, user.Id));
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            this.genericGuildEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            this.actionlogEvents.UserJoined(sender, e).Add(this.Bot);
            this.joinEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            this.inviteTrackerEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            this.nameNormalizerEvents.GuildMemberAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            this.genericGuildEvents.GuildMemberRemoved(sender, e).Add(this.Bot);
            this.actionlogEvents.UserLeft(sender, e).Add(this.Bot);
            this.joinEvents.GuildMemberRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            this.genericGuildEvents.GuildMemberUpdated(sender, e).Add(this.Bot);
            this.actionlogEvents.MemberUpdated(sender, e).Add(this.Bot);
            this.nameNormalizerEvents.GuildMemberUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            this.genericGuildEvents.GuildMemberBanned(sender, e).Add(this.Bot);
            this.actionlogEvents.BanAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);

            this.commandEvents.CommandExecuted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Context.Guild, e.Context.Member);
            this.commandEvents.CommandError(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            this.afkEvents.MessageCreated(sender, e).Add(this.Bot);
            this.crosspostEvents.MessageCreated(sender, e).Add(this.Bot);
            this.phishingProtectionEvents.MessageCreated(sender, e).Add(this.Bot);
            this.bumpReminderEvents.MessageCreated(sender, e).Add(this.Bot);
            this.experienceEvents.MessageCreated(sender, e).Add(this.Bot);
            this.embedMessagesEvents.MessageCreated(sender, e).Add(this.Bot);
            this.tokenLeakEvents.MessageCreated(sender, e).Add(this.Bot);

            if (!e.Message.Content.IsNullOrWhiteSpace() && (e.Message.Content == $"<@{sender.CurrentUser.Id}>" || e.Message.Content == $"<@!{sender.CurrentUser.Id}>"))
            {
                string prefix = e.Guild.GetGuildPrefix(this.Bot);

                _ = e.Message.RespondAsync($"Hi {e.Author.Mention}, i'm Makoto. I support Slash Commands, but additionally you can use me via `{prefix}`. To get a list of all commands, type `;;help` or do a `/` and filter by me.\n" +
                                $"If you need help, feel free to join our Support and Development Server: <{this.Bot.status.DevelopmentServerInvite}>\n\n" +
                                $"To find out more about me, check my Github Repo: <https://s.aitsys.dev/makoto>.");
            }
        }).Add(this.Bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            this.phishingProtectionEvents.MessageUpdated(sender, e).Add(this.Bot);
            this.actionlogEvents.MessageUpdated(sender, e).Add(this.Bot);
            this.tokenLeakEvents.MessageUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Interaction.User);

            this.submissionEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
            this.embedMessagesEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
            this.reminderEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.discordEvents.GuildCreated(sender, e).Add(this.Bot);
            this.inviteTrackerEvents.GuildCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.Message.Author);

            this.actionlogEvents.MessageDeleted(sender, e).Add(this.Bot);
            this.bumpReminderEvents.MessageDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.MessageBulkDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.RoleCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.RoleModified(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.RoleDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, e.Member);

            this.actionlogEvents.BanRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.GuildAfter);

            this.actionlogEvents.GuildUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.ChannelCreated(sender, e).Add(this.Bot);
            this.voicePrivacyEvents.ChannelCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.ChannelDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.ChannelUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.InviteCreated(sender, e).Add(this.Bot);
            this.inviteTrackerEvents.InviteCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.actionlogEvents.InviteDeleted(sender, e).Add(this.Bot);
            this.inviteTrackerEvents.InviteDeleted(sender, e).Add(this.Bot);
            this.inviteNoteEvents.InviteDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            this.bumpReminderEvents.ReactionAdded(sender, e).Add(this.Bot);
            this.reactionRoleEvents.MessageReactionAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            this.bumpReminderEvents.ReactionRemoved(sender, e).Add(this.Bot);
            this.reactionRoleEvents.MessageReactionRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild, user: e.User);

            this.actionlogEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
            this.voicePrivacyEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
            this.vcCreatorEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
            this.userBlockEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);
        }).Add(this.Bot);
    }

    internal async Task ThreadMemberUpdated(DiscordClient sender, ThreadMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Thread.Guild is not null)
                FillDatabase(e.Thread.Guild);

            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadMembersUpdated(DiscordClient sender, ThreadMembersUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadListSynced(DiscordClient sender, ThreadListSyncEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            if (this.Bot.status.DiscordGuildDownloadCompleted)
                foreach (var b in e.Threads)
                    b.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(e.Guild);

            this.autoUnarchiveEvents.ThreadUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            FillDatabase(user: e.UserAfter);

            this.nameNormalizerEvents.UserUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }
}
