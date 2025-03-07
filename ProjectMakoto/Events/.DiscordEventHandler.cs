// Project Makoto
// Copyright (C) 2024  Fortunevale
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

    Translations.events.genericEvent tKey
        => this.Bot.LoadedTranslations.Events.GenericEvent;

    public static void SetupEvents(Bot _bot)
    {
        DiscordEventHandler handler = new(_bot);

        Log.Debug("Registering DisCatSharp EventHandler..");
        handler.genericGuildEvents = new(_bot);
        handler.commandEvents = new(_bot);
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

        _bot.DiscordClient.GetFirstShard().GetCommandsNext().CommandExecuted += handler.CommandExecuted;
        _bot.DiscordClient.GetFirstShard().GetCommandsNext().CommandErrored += handler.CommandError;
    }

    GenericGuildEvents genericGuildEvents { get; set; }
    CommandEvents commandEvents { get; set; }
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

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.genericGuildEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            _ = this.actionlogEvents.UserJoined(sender, e).Add(this.Bot);
            _ = this.joinEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            _ = this.inviteTrackerEvents.GuildMemberAdded(sender, e).Add(this.Bot);
            _ = this.nameNormalizerEvents.GuildMemberAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.genericGuildEvents.GuildMemberRemoved(sender, e).Add(this.Bot);
            _ = this.actionlogEvents.UserLeft(sender, e).Add(this.Bot);
            _ = this.joinEvents.GuildMemberRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.genericGuildEvents.GuildMemberUpdated(sender, e).Add(this.Bot);
            _ = this.actionlogEvents.MemberUpdated(sender, e).Add(this.Bot);
            _ = this.nameNormalizerEvents.GuildMemberUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.genericGuildEvents.GuildMemberBanned(sender, e).Add(this.Bot);
            _ = this.actionlogEvents.BanAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.commandEvents.CommandExecuted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.commandEvents.CommandError(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.crosspostEvents.MessageCreated(sender, e).Add(this.Bot);
            _ = this.phishingProtectionEvents.MessageCreated(sender, e).Add(this.Bot);
            _ = this.bumpReminderEvents.MessageCreated(sender, e).Add(this.Bot);
            _ = this.experienceEvents.MessageCreated(sender, e).Add(this.Bot);
            _ = this.embedMessagesEvents.MessageCreated(sender, e).Add(this.Bot);
            _ = this.tokenLeakEvents.MessageCreated(sender, e).Add(this.Bot);

            if (!e.Message.Content.IsNullOrWhiteSpace() && (e.Message.Content == $"<@{sender.CurrentUser.Id}>" || e.Message.Content == $"<@!{sender.CurrentUser.Id}>"))
            {
                var prefix = e.Guild.GetGuildPrefix(this.Bot);

                _ = e.Message.RespondAsync(this.tKey.PingMessage.Get(this.Bot.Guilds[e.Guild.Id]).Build(false, true,
                    new TVar("User", e.Author.Mention),
                    new TVar("Bot", sender.CurrentUser.GetUsername()),
                    new TVar("BotMention", sender.CurrentUser.Mention),
                    new TVar("Help", sender.GetCommandMention(this.Bot, "help")),
                    new TVar("Invite", $"<{this.Bot.status.DevelopmentServerInvite}>"),
                    new TVar("GithubRepo", "<https://s.aitsys.dev/makoto>")));
            }
        }).Add(this.Bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.phishingProtectionEvents.MessageUpdated(sender, e).Add(this.Bot);
            _ = this.actionlogEvents.MessageUpdated(sender, e).Add(this.Bot);
            _ = this.tokenLeakEvents.MessageUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.submissionEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
            _ = this.embedMessagesEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
            _ = this.reminderEvents.ComponentInteractionCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.discordEvents.GuildCreated(sender, e).Add(this.Bot);
            _ = this.inviteTrackerEvents.GuildCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.MessageDeleted(sender, e).Add(this.Bot);
            _ = this.bumpReminderEvents.MessageDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.MessageBulkDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.RoleCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.RoleModified(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.RoleDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.BanRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.GuildUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.ChannelCreated(sender, e).Add(this.Bot);
            _ = this.voicePrivacyEvents.ChannelCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.ChannelDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.ChannelUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.InviteCreated(sender, e).Add(this.Bot);
            _ = this.inviteTrackerEvents.InviteCreated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.InviteDeleted(sender, e).Add(this.Bot);
            _ = this.inviteTrackerEvents.InviteDeleted(sender, e).Add(this.Bot);
            _ = this.inviteNoteEvents.InviteDeleted(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.bumpReminderEvents.ReactionAdded(sender, e).Add(this.Bot);
            _ = this.reactionRoleEvents.MessageReactionAdded(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.bumpReminderEvents.ReactionRemoved(sender, e).Add(this.Bot);
            _ = this.reactionRoleEvents.MessageReactionRemoved(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.actionlogEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
            _ = this.voicePrivacyEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
            _ = this.vcCreatorEvents.VoiceStateUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task ThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal Task ThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
    {
        return Task.CompletedTask;
        //_ = Task.Run(async () =>
        //{

        //}).Add(this.Bot);
    }

    internal async Task ThreadMemberUpdated(DiscordClient sender, ThreadMemberUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadMembersUpdated(DiscordClient sender, ThreadMembersUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            e.Thread.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadListSynced(DiscordClient sender, ThreadListSyncEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (this.Bot.status.DiscordGuildDownloadCompleted)
                foreach (var b in e.Threads)
                    b.JoinWithQueue(this.Bot.ThreadJoinClient);
        }).Add(this.Bot);
    }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.autoUnarchiveEvents.ThreadUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _ = this.nameNormalizerEvents.UserUpdated(sender, e).Add(this.Bot);
        }).Add(this.Bot);
    }
}
