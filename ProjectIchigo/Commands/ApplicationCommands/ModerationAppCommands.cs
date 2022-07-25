namespace ProjectIchigo.ApplicationCommands;
internal class ModerationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("purge", "Deletes the specified amount of messages", (long)Permissions.ManageMessages) ]
    public async Task Purge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new PurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("guild-purge", "Scans the specified amount of messages for the given user's messages and deletes them.", (long)(Permissions.ManageMessages | Permissions.ManageChannels))]
    public async Task GuildPurge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new GuildPurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("clearbackup", "Clears the stored roles of a user.", (long)Permissions.ManageRoles)]
    public async Task ClearBackup(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new ClearBackupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("timeout", "Times the user for the specified amount of time out", (long)Permissions.ModerateMembers)]
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
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("remove-timeout", "Removes the timeout for the specified user", (long)Permissions.ModerateMembers)]
    public async Task RemoveTimeout(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new RemoveTimeoutCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("kick", "Kicks the specified user", (long)Permissions.KickMembers)]
    public async Task Kick(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new KickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "reason", reason },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("ban", "Bans the specified user", (long)Permissions.BanMembers)]
    public async Task Ban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim, [Option("reason", "The reason")] string reason = "")
    {
        Task.Run(async () =>
        {
            await new BanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
                { "reason", reason },
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("unban", "Unbans the specified user", (long)Permissions.BanMembers)]
    public async Task Unban(InteractionContext ctx, [Option("user", "The user")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new UnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }
}
