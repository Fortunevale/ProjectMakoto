// Project Makoto
// Copyright (C) 2024  Fortunevale
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
    
    Description("Starts a poll.")]
    public async Task Poll(CommandContext ctx)
        => _ = new PollCommand().ExecuteCommand(ctx, this._bot);



    [Command("purge"), Aliases("clear"),
    
    Description("Deletes the specified amount of messages.")]
    public async Task Purge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim = null)
        => _ = new PurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        });



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    
    Description("Scans all channels and deletes the specified user's messages.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim)
        => _ = new GuildPurgeCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "number", number },
            { "victim", victim },
        });



    [Command("clearbackup"), Aliases("clearroles", "clearrole", "clearbackuproles", "clearbackuprole"),
    
    Description($"Clears the stored roles and nickname of a user.")]
    public async Task ClearBackup(CommandContext ctx, DiscordUser victim)
        => _ = new ClearBackupCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });



    [Command("timeout"), Aliases("time-out", "mute"),
    
    Description("Sets the specified user into a timeout.")]
    public async Task Timeout(CommandContext ctx, DiscordUser victim, [Description("Duration")] string duration = "", [Description("Reason")][RemainingText] string reason = "")
        => _ = new TimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "duration", duration },
            { "reason", reason },
        });



    [Command("remove-timeout"), Aliases("rm-timeout", "rmtimeout", "removetimeout", "unmute"),
    
    Description("Removes a timeout from the specified user.")]
    public async Task RemoveTimeout(CommandContext ctx, DiscordMember victim)
        => _ = new RemoveTimeoutCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });



    [Command("kick"),
    
    Description("Kicks the specified user.")]
    public async Task Kick(CommandContext ctx, DiscordMember victim, [Description("Reason")][RemainingText] string reason = "")
        => _ = new KickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "reason", reason },
        });



    [Command("ban"),
    
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Message deletion days")] int days = 0, [Description("Reason")][RemainingText] string reason = "")
        => _ = new BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        });

    [Command("ban"),
    
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Reason")][RemainingText] string reason)
        => _ = new BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", 0 },
            { "reason", reason },
        });

    [Command("softban"),
    
    Description("Soft bans the specified user.")]
    public async Task SoftBan(CommandContext ctx, DiscordUser victim, [Description("Message deletion days")] int days = 0, [Description("Reason")][RemainingText] string reason = "")
        => _ = new SoftBanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
            { "days", days },
            { "reason", reason },
        });



    [Command("unban"),
    
    Description("Unbans the specified user.")]
    public async Task Unban(CommandContext ctx, DiscordUser victim)
        => _ = new UnbanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim },
        });



    [Group("follow"),
    
    Description("Allows you to follow an announcement channel from our support server.")]
    public sealed class MessageEmbedding : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "follow");

        [Command("githubupdates"),
        Description("Follow the github updates channel.")]
        public async Task GithubUpdates(CommandContext ctx)
            => _ = new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.GithubUpdates },
            });

        [Command("globalbans"),
        Description("Follow the global bans channel.")]
        public async Task GlobalBans(CommandContext ctx)
            => _ = new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.GlobalBans },
            });

        [Command("news"),
        Description("Follow the news channel.")]
        public async Task News(CommandContext ctx)
            => _ = new FollowUpdatesCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "channel", FollowChannel.News },
            });
    }



    [Command("moveall"),
    
    Description("Move all users in your Voice Channel to another Voice Channel")]
    public async Task MoveAll(CommandContext ctx, DiscordChannel newChannel)
        => _ = new MoveAllCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "newChannel", newChannel }
        });



    [Command("movehere"),
    
    Description("Move all users from another Voice Channel to your Voice Channel")]
    public async Task MoveHere(CommandContext ctx, DiscordChannel oldChannel)
        => _ = new MoveHereCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "oldChannel", oldChannel }
        });



    [Command("customembed"),
    
    Description("Create an embedded message")]
    public async Task CustomEmbed(CommandContext ctx)
        => _ = new CustomEmbedCommand().ExecuteCommand(ctx, this._bot);



    [Command("override-bump-time"),
    
    Description("Allows fixing of the last bump in case Disboard did not properly post a message.")]
    public async Task OverrideBumpTime(CommandContext ctx)
        => _ = new ManualBumpCommand().ExecuteCommand(ctx, this._bot);
}
