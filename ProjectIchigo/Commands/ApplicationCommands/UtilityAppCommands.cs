namespace ProjectIchigo.ApplicationCommands;
public class UtilityAppCommands : ApplicationCommandsModule
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
    }
    
    [SlashCommand("report-host", "Allows you to contribute a new malicious host to our database.", dmPermission: false)]
    public async Task ReportHost(InteractionContext ctx, [Option("url", "The host")] string url)
    {
        Task.Run(async () =>
        {
            await new ReportHostCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "url", url }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("upload", "Upload a file to the bot. Only use when instructed to.", dmPermission: false)]
    public async Task Upload(InteractionContext ctx, [Option("file", "The file you want to upload.")] DiscordAttachment attachment)
    {
        Task.Run(async () =>
        {
            await new UploadCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "stream", await new HttpClient().GetStreamAsync(attachment.Url) },
                { "filesize", attachment.FileSize }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommand("urban-dictionary", "Look up a term on Urban Dictionary.", dmPermission: false)]
    public async Task UrbanDictionary(InteractionContext ctx, [Option("term", "The term you want to look up.")] string term)
    {
        Task.Run(async () =>
        {
            await new UrbanDictionaryCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "term", term }
            });
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommandGroup("data", "Allows you to request or manage your user data.", dmPermission: false)]
    public class Data : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("request", "Allows you to request your user data.", dmPermission: false)]
        public async Task Request(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.RequestCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [SlashCommand("delete", "Allows you to delete your user data.", dmPermission: false)]
        public async Task Delete(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.DeleteCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [SlashCommand("object", "Allows you to stop Ichigo from further processing of your user data.", dmPermission: false)]
        public async Task Object(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.ObjectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [SlashCommand("policy", "Allows you to view how Ichigo processes your data.", dmPermission: false)]
        public async Task Info(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommand("credits", "Allows you to view who contributed the bot.", dmPermission: false)]
    public async Task Credits(InteractionContext ctx)
    {
        Task.Run(async () =>
        {
            await new CreditsCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }

    [ContextMenu(ApplicationCommandType.Message, "Steal Emojis")]
    public async Task EmojiStealer(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new EmojiStealerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "message", ctx.TargetMessage }
            });
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
    }
}
