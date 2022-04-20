namespace Project_Ichigo.Events;

internal class DisCatSharpEventHandler
{
    internal DisCatSharpEventHandler(Bot _bot)
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



    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        _ = genericGuildEvents.GuildMemberAdded(sender, e);
        _ = actionlogEvents.UserJoined(sender, e);
        _ = joinEvents.GuildMemberAdded(sender, e);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        _ = genericGuildEvents.GuildMemberRemoved(sender, e);
        _ = actionlogEvents.UserLeft(sender, e);
        _ = joinEvents.GuildMemberRemoved(sender, e);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        _ = genericGuildEvents.GuildMemberUpdated(sender, e);
        _ = actionlogEvents.MemberUpdated(sender, e);
    }

    internal async Task GuildBanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        _ = genericGuildEvents.GuildMemberBanned(sender, e);
        _ = actionlogEvents.BanAdded(sender, e);
    }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        _ = commandEvents.CommandExecuted(sender, e);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        _ = commandEvents.CommandError(sender, e);
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = afkEvents.MessageCreated(sender, e);
        _ = crosspostEvents.MessageCreated(sender, e);
        _ = phishingProtectionEvents.MessageCreated(sender, e);
        _ = bumpReminderEvents.MessageCreated(sender, e);
        _ = experienceEvents.MessageCreated(sender, e);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        _ = phishingProtectionEvents.MessageUpdated(sender, e);
        _ = actionlogEvents.MessageUpdated(sender, e);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = submissionEvents.ComponentInteractionCreated(sender, e);
    }

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        _ = discordEvents.GuildCreated(sender, e);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        _ = actionlogEvents.MessageDeleted(sender, e);
        _ = bumpReminderEvents.MessageDeleted(sender, e);
    }

    internal async Task MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        _ = actionlogEvents.MessageBulkDeleted(sender, e);
    }

    internal async Task GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        _ = actionlogEvents.RoleCreated(sender, e);
    }

    internal async Task GuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        _ = actionlogEvents.RoleModified(sender, e);
    }

    internal async Task GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        _ = actionlogEvents.RoleDeleted(sender, e);
    }

    internal async Task GuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        _ = actionlogEvents.BanRemoved(sender, e);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        _ = actionlogEvents.GuildUpdated(sender, e);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        _ = actionlogEvents.ChannelCreated(sender, e);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        _ = actionlogEvents.ChannelDeleted(sender, e);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        _ = actionlogEvents.ChannelUpdated(sender, e);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        _ = actionlogEvents.InviteCreated(sender, e);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        _ = actionlogEvents.InviteDeleted(sender, e);
    }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        _ = bumpReminderEvents.ReactionAdded(sender, e);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        _ = bumpReminderEvents.ReactionRemoved(sender, e);
    }
}
