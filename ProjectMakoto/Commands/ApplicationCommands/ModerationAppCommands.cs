namespace ProjectMakoto.ApplicationCommands;
public class ModerationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("poll", "Starts a poll.", (long)Permissions.ManageMessages, dmPermission: false)]
    public async Task Poll(InteractionContext ctx)
    {
        Task.Run(async () =>
        {
            await new PollCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("purge", "Deletes the specified amount of messages.", (long)Permissions.ManageMessages, dmPermission: false)]
    public async Task Purge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new PurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("guild-purge", "Scans all channels and deletes the specified user's messages.", (long)(Permissions.ManageMessages | Permissions.ManageChannels), dmPermission: false)]
    public async Task GuildPurge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new GuildPurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("clearbackup", "Clears the stored roles and nickname of a user.", (long)Permissions.ManageRoles, dmPermission: false)]
    public async Task ClearBackup(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new ClearBackupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("timeout", "Sets the specified user into a timeout.", (long)Permissions.ModerateMembers, dmPermission: false)]
    public async Task Timeout(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("duration", "The duration")] string duration = "", [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new TimeoutCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "duration", duration },
                { "reason", reason },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("remove-timeout", "Removes a timeout from the specified user.", (long)Permissions.ModerateMembers, dmPermission: false)]
    public async Task RemoveTimeout(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new RemoveTimeoutCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("kick", "Kicks the specified user.", (long)Permissions.KickMembers, dmPermission: false)]
    public async Task Kick(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new KickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "reason", reason },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("ban", "Bans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task Ban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("days", "Days of messages to delete")] int days = 0,  [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new BanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "days", days },
                { "reason", reason },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("softban", "Soft bans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task SoftBan(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("days", "Days of messages to delete")] int days = 0, [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new SoftBanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "days", days },
                { "reason", reason },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("unban", "Unbans the specified user.", (long)Permissions.BanMembers, dmPermission: false)]
    public async Task Unban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new UnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("follow", "Allows you to follow an announcement channel from our support server.", (long)Permissions.ManageWebhooks, dmPermission: false)]
    public async Task Follow(InteractionContext ctx, [Option("channel", "The channel")] FollowChannel channel)
    {
        Task.Run(async () =>
        {
            await new FollowUpdatesCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "channel", channel },
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("moveall", "Move all users in your Voice Channel to another Voice Channel", (long)Permissions.MoveMembers, dmPermission: false)]
    public async Task MoveAll(InteractionContext ctx, [Option("channel", "The channel to move to."), ChannelTypes(ChannelType.Voice)] DiscordChannel newChannel)
    {
        Task.Run(async () =>
        {
            await new MoveAllCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "newChannel", newChannel }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("movehere", "Move all users from another Voice Channel to your Voice Channel", (long)Permissions.MoveMembers, dmPermission: false)]
    public async Task MoveHere(InteractionContext ctx, [Option("channel", "The channel to move from."), ChannelTypes(ChannelType.Voice)] DiscordChannel oldChannel)
    {
        Task.Run(async () =>
        {
            await new MoveHereCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "oldChannel", oldChannel }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("customembed", "Create an embedded message", (long)Permissions.EmbedLinks, dmPermission: false)]
    public async Task CustomEmbed(InteractionContext ctx)
    {
        Task.Run(async () =>
        {
            await new CustomEmbedCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("override-bump-time", "Allows fixing of the last bump in case Disboard did not properly post a message.")]
    public async Task OverrideBumpTime(InteractionContext ctx)
    {
        Task.Run(async () =>
        {
            await new ManualBumpCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }
}