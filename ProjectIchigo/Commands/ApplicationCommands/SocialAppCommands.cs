namespace ProjectIchigo.ApplicationCommands;
internal class SocialAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("afk", "Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.", dmPermission: false)]
    public async Task UserInfo(InteractionContext ctx, [Option("reason", "The reason")]string reason = "-")
    {
        Task.Run(async () =>
        {
            await new AfkCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "reason", reason }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("cuddle", "Cuddle with another user.", dmPermission: false)]
    public async Task Cuddle(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new CuddleCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("kiss", "Kiss another user.", dmPermission: false)]
    public async Task Kiss(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new KissCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("slap", "Slap another user.", dmPermission: false)]
    public async Task Slap(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new SlapCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("kill", "Kill another user..?", dmPermission: false)]
    public async Task Kill(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new KillCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("boop", "Give another user a boop!", dmPermission: false)]
    public async Task Boop(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new BoopCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("highfive", "Give a high five!", dmPermission: false)]
    public async Task Highfive(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new HighFiveCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("hug", "Hug another user!", dmPermission: false)]
    public async Task Hug(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new HugCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }



    [SlashCommand("pat", "Give someone some headpats!", dmPermission: false)]
    public async Task Pat(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
    {
        Task.Run(async () =>
        {
            await new PatCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "user", user }
            }, false, true);
        }).Add(_bot.watcher, ctx);
    }
}
