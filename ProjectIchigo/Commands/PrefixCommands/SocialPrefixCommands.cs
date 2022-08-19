namespace ProjectIchigo.PrefixCommands;

internal class SocialPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Command("afk"),
    CommandModule("social"),
    Description("Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.")]
    public async Task Afk(CommandContext ctx, [RemainingText][Description("Text (<128 characters)")] string reason = "-")
    {
        Task.Run(async () =>
        {
            await new AfkCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "reason", reason }
            });
        }).Add(_bot.watcher, ctx);
    }

    [Command("cuddle"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Cuddle with another user.")]
    public async Task Cuddle(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new CuddleCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("kiss"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kiss another user.")]
    public async Task Kiss(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new KissCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("slap"), Aliases("bonk", "punch"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Slap another user.")]
    public async Task Slap(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new SlapCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("kill"), Aliases("waste"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kill another user..?")]
    public async Task Kill(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new KillCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("boop"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give another user a boop!")]
    public async Task Boop(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new BoopCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("highfive"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give a high five!")]
    public async Task Highfive(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new HighFiveCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("hug"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Hug another user!")]
    public async Task Hug(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new HugCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("pat"), Aliases("pet", "headpat", "headpet"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give someone some headpats!")]
    public async Task Pat(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new PatCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            });
        }).Add(_bot.watcher, ctx);
    }
}
