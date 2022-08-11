namespace ProjectIchigo.ApplicationCommands;
internal class UtilityAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("user-info", "Displays information the bot knows about you or the mentioned user.", dmPermission: false)]
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

    [SlashCommand("avatar", "Displays your or the mentioned user's avatar as an embedded image.", dmPermission: false)]
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
    
    [SlashCommand("banner", "Displays your or the mentioned user's banner as an embedded image.", dmPermission: false)]
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
    
    [SlashCommand("rank", "Shows your or the mentioned user's rank and rank progress.", dmPermission: false)]
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

    [SlashCommand("leaderboard", "Displays the current experience rankings on this server.", dmPermission: false)]
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
    
    [SlashCommand("submit-url", "Allows you to contribute a new malicious domain to our database.", dmPermission: false)]
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

#if DEBUG
    [SlashCommandGroup("data", "[WIP] Allows you to request or manage your user data.", dmPermission: false)]
    public class Data : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("request", "[WIP] Allows you to request your user data.", dmPermission: false)]
        public async Task Request(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.RequestCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
        
        [SlashCommand("delete", "[WIP] Allows you to delete your user data. Temporarily redirects to our support guild as this command is not yet finished.", dmPermission: false)]
        public async Task Delete(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.DeleteCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
        
        [SlashCommand("object", "[WIP] Allows you to stop Ichigo from further processing of your user data.", dmPermission: false)]
        public async Task Object(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.ObjectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
        
        [SlashCommand("info", "[WIP] Allows you to view how Ichigo processes your data.", dmPermission: false)]
        public async Task Info(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
#endif

    [ContextMenu(ApplicationCommandType.Message, "Steal Emojis")]
    public async Task EmojiStealer(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new EmojiStealerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "message", ctx.TargetMessage }
            });
        }).Add(_bot._watcher, ctx);
    }
    
    [ContextMenu(ApplicationCommandType.Message, "Translate Message")]
    public async Task Translate(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new TranslateCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "message", ctx.TargetMessage }
            });
        }).Add(_bot._watcher, ctx);
    }
}
