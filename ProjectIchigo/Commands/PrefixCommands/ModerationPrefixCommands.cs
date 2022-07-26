namespace ProjectIchigo.PrefixCommands;
internal class ModerationPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("purge"), Aliases("clear"),
    CommandModule("moderation"),
    Description("Deletes the specified amount of messages.")]
    public async Task Purge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim = null)
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



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    CommandModule("moderation"),
    Description("Scans all channels and deletes the specified user's messages.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim)
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



    [Command("clearbackup"), Aliases("clearroles", "clearrole", "clearbackuproles", "clearbackuprole"),
    CommandModule("moderation"),
    Description($"Clears the stored roles and nickname of a user.")]
    public async Task ClearBackup(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await new ClearBackupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("timeout"), Aliases("time-out", "mute"),
    CommandModule("moderation"),
    Description("Sets the specified user into a timeout.")]
    public async Task Timeout(CommandContext ctx, DiscordUser victim, [Description("Duration")] string duration = "", [Description("Reason")] string reason = "")
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



    [Command("remove-timeout"), Aliases("rm-timeout", "rmtimeout", "removetimeout", "unmute"),
    CommandModule("moderation"),
    Description("Removes a timeout from the specified user.")]
    public async Task RemoveTimeout(CommandContext ctx, DiscordMember victim)
    {
        Task.Run(async () =>
        {
            await new KickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("kick"),
    CommandModule("moderation"),
    Description("Kicks the specified user.")]
    public async Task Kick(CommandContext ctx, DiscordMember victim, [Description("Reason")][RemainingText] string reason = "")
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



    [Command("ban"),
    CommandModule("moderation"),
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Reason")][RemainingText] string reason = "")
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



    [Command("unban"),
    CommandModule("moderation"),
    Description("Unbans the specified user.")]
    public async Task Unban(CommandContext ctx, DiscordUser victim)
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
