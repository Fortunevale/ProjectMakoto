namespace ProjectIchigo.ApplicationCommands;
internal class User : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("user-info", "Shows information about you or the mentioned user")]
    public async Task UserInfo(InteractionContext ctx, [Option("User", "The User")]DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new UserInfoCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("avatar", "Sends your or the mentioned user's avatar as an embedded image")]
    public async Task Avatar(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new AvatarCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }
    
    [SlashCommand("banner", "Sends your or the mentioned user's banner as an embedded image")]
    public async Task Banner(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new BannerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }
    
    [SlashCommand("rank", "Shows your or the mentioned user's rank and rank progress")]
    public async Task Rank(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new RankCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommand("leaderboard", "Shows the current experience leaderboard")]
    public async Task Leaderboard(InteractionContext ctx, [Option("amount", "The amount of rankings to show"), MinimumValue(3), MaximumValue(50)] int ShowAmount = 10)
    {
        Task.Run(async () =>
        {
            await new LeaderboardCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "ShowAmount", ShowAmount }
            });
        }).Add(_bot._watcher, ctx);
    }
    
    [SlashCommand("submit-url", "Allows submission of new malicous urls to our database")]
    public async Task UrlSubmit(InteractionContext ctx, [Option("url", "The url")] string url)
    {
        Task.Run(async () =>
        {
            await new UrlSubmitCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "url", url }
            });
        }).Add(_bot._watcher, ctx);
    }
}
