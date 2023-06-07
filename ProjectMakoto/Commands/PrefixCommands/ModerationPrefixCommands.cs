// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;
public sealed class ModerationPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Command("poll"),
    CommandModule("moderation"),
    Description("Starts a poll.")]
    public async Task Poll(CommandContext ctx)
        => new PollCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);



    [Command("purge"), Aliases("clear"),
    CommandModule("moderation"),
    Description("Deletes the specified amount of messages.")]
    public async Task Purge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim = null)
        => new PurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        }).Add(this._bot.watcher, ctx);



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    CommandModule("moderation"),
    Description("Scans all channels and deletes the specified user's messages.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim)
        => new GuildPurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        }).Add(this._bot.watcher, ctx);



    [Command("clearbackup"), Aliases("clearroles", "clearrole", "clearbackuproles", "clearbackuprole"),
    CommandModule("moderation"),
    Description($"Clears the stored roles and nickname of a user.")]
    public async Task ClearBackup(CommandContext ctx, DiscordUser victim)
        => new ClearBackupCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        }).Add(this._bot.watcher, ctx);



    [Command("timeout"), Aliases("time-out", "mute"),
    CommandModule("moderation"),
    Description("Sets the specified user into a timeout.")]
    public async Task Timeout(CommandContext ctx, DiscordUser victim, [Description("Duration")] string duration = "", [Description("Reason")][RemainingText] string reason = "")
        => new TimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "duration", duration },
            { "reason", reason },
        }).Add(this._bot.watcher, ctx);



    [Command("remove-timeout"), Aliases("rm-timeout", "rmtimeout", "removetimeout", "unmute"),
    CommandModule("moderation"),
    Description("Removes a timeout from the specified user.")]
    public async Task RemoveTimeout(CommandContext ctx, DiscordMember victim)
        => new RemoveTimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        }).Add(this._bot.watcher, ctx);



    [Command("kick"),
    CommandModule("moderation"),
    Description("Kicks the specified user.")]
    public async Task Kick(CommandContext ctx, DiscordMember victim, [Description("Reason")][RemainingText] string reason = "")
        => new KickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "reason", reason },
        }).Add(this._bot.watcher, ctx);



    [Command("ban"),
    CommandModule("moderation"),
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Message deletion days")] int days = 0, [Description("Reason")][RemainingText] string reason = "")
        => new BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        }).Add(this._bot.watcher, ctx);

    [Command("ban"),
    CommandModule("moderation"),
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Reason")][RemainingText] string reason)
        => new BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", 0 },
            { "reason", reason },
        }).Add(this._bot.watcher, ctx);

    [Command("softban"),
    CommandModule("moderation"),
    Description("Soft bans the specified user.")]
    public async Task SoftBan(CommandContext ctx, DiscordUser victim, [Description("Message deletion days")] int days = 0, [Description("Reason")][RemainingText] string reason = "")
        => new SoftBanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        }).Add(this._bot.watcher, ctx);



    [Command("unban"),
    CommandModule("moderation"),
    Description("Unbans the specified user.")]
    public async Task Unban(CommandContext ctx, DiscordUser victim)
        => new UnbanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        }).Add(this._bot.watcher, ctx);



    [Group("follow"),
    CommandModule("moderation"),
    Description("Allows you to follow an announcement channel from our support server.")]
    public sealed class MessageEmbedding : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "follow").Add(this._bot.watcher, ctx);

        [Command("githubupdates"),
        Description("Follow the github updates channel.")]
        public async Task GithubUpdates(CommandContext ctx)
            => new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.GithubUpdates },
            }).Add(this._bot.watcher, ctx);

        [Command("globalbans"),
        Description("Follow the global bans channel.")]
        public async Task GlobalBans(CommandContext ctx)
            => new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.GlobalBans },
            }).Add(this._bot.watcher, ctx);

        [Command("news"),
        Description("Follow the news channel.")]
        public async Task News(CommandContext ctx)
            => new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.News },
            }).Add(this._bot.watcher, ctx);
    }



    [Command("moveall"),
    CommandModule("moderation"),
    Description("Move all users in your Voice Channel to another Voice Channel")]
    public async Task MoveAll(CommandContext ctx, DiscordChannel newChannel)
        => new MoveAllCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "newChannel", newChannel }
        }).Add(this._bot.watcher, ctx);



    [Command("movehere"),
    CommandModule("moderation"),
    Description("Move all users from another Voice Channel to your Voice Channel")]
    public async Task MoveHere(CommandContext ctx, DiscordChannel oldChannel)
        => new MoveHereCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "oldChannel", oldChannel }
        }).Add(this._bot.watcher, ctx);



    [Command("customembed"),
    CommandModule("moderation"),
    Description("Create an embedded message")]
    public async Task CustomEmbed(CommandContext ctx)
        => new CustomEmbedCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);



    [Command("override-bump-time"),
    CommandModule("moderation"),
    Description("Allows fixing of the last bump in case Disboard did not properly post a message.")]
    public async Task OverrideBumpTime(CommandContext ctx)
        => new ManualBumpCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
}
