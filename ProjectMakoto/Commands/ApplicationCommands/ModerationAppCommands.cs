// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(995)]
public sealed class ModerationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("poll", "Starts a poll.", (long)Permissions.ManageMessages, dmPermission: false)]
    public async Task Poll(InteractionContext ctx)
        => _ = new PollCommand().ExecuteCommand(ctx, this._bot);

    [SlashCommand("purge", "Deletes the specified amount of messages.", (long)Permissions.ManageMessages, dmPermission: false)]
    public async Task Purge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim = null)
        => _ = new PurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        });

    [SlashCommand("guild-purge", "Scans all channels and deletes the specified user's messages.", (long)(Permissions.ManageMessages | Permissions.ManageChannels), dmPermission: false)]
    public async Task GuildPurge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim)
        => _ = new GuildPurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        });

    [SlashCommand("clearbackup", "Clears the stored roles and nickname of a user.", (long)Permissions.ManageRoles, dmPermission: false)]
    public async Task ClearBackup(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
        => _ = new ClearBackupCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });

    [SlashCommand("timeout", "Sets the specified user into a timeout.", (long)Permissions.ModerateMembers, dmPermission: false)]
    public async Task Timeout(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("duration", "The duration")] string duration = "", [Option("reason", "The reason")] string reason = "")
        => _ = new TimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "duration", duration },
            { "reason", reason },
        });

    [SlashCommand("remove-timeout", "Removes a timeout from the specified user.", (long)Permissions.ModerateMembers, dmPermission: false)]
    public async Task RemoveTimeout(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
        => _ = new RemoveTimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });

    [SlashCommand("kick", "Kicks the specified user.", (long)Permissions.KickMembers, dmPermission: false)]
    public async Task Kick(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("reason", "The reason")] string reason = "")
        => _ = new KickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        { { "victim", victim },
            { "reason", reason },
        });

    [SlashCommand("ban", "Bans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task Ban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("days", "Days of messages to delete")] int days = 0, [Option("reason", "The reason")] string reason = "")
        => _ = new BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        });

    [SlashCommand("softban", "Soft bans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task SoftBan(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("days", "Days of messages to delete")] int days = 0, [Option("reason", "The reason")] string reason = "")
        => _ = new SoftBanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        });

    [SlashCommand("unban", "Unbans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task Unban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
        => _ = new UnbanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });

    [SlashCommand("follow", "Allows you to follow an announcement channel from our support server.", (long)Permissions.ManageWebhooks, dmPermission: false)]
    public async Task Follow(InteractionContext ctx, [Option("channel", "The channel")] FollowChannel channel)
        => _ = new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "channel", channel },
        });

    [SlashCommand("moveall", "Move all users in your Voice Channel to another Voice Channel", (long)Permissions.MoveMembers, dmPermission: false)]
    public async Task MoveAll(InteractionContext ctx, [Option("channel", "The channel to move to."), ChannelTypes(ChannelType.Voice)] DiscordChannel newChannel)
        => _ = new MoveAllCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "newChannel", newChannel }
        });

    [SlashCommand("movehere", "Move all users from another Voice Channel to your Voice Channel", (long)Permissions.MoveMembers, dmPermission: false)]
    public async Task MoveHere(InteractionContext ctx, [Option("channel", "The channel to move from."), ChannelTypes(ChannelType.Voice)] DiscordChannel oldChannel)
        => _ = new MoveHereCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "oldChannel", oldChannel }
        });

    [SlashCommand("customembed", "Create an embedded message", (long)Permissions.EmbedLinks, dmPermission: false)]
    public async Task CustomEmbed(InteractionContext ctx)
        => _ = new CustomEmbedCommand().ExecuteCommand(ctx, this._bot);

    [SlashCommand("override-bump-time", "Allows fixing of the last bump in case Disboard did not properly post a message.", dmPermission: false)]
    public async Task OverrideBumpTime(InteractionContext ctx)
        => _ = new ManualBumpCommand().ExecuteCommand(ctx, this._bot);
}