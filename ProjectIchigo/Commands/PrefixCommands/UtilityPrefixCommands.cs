namespace ProjectIchigo.PrefixCommands;

public class UtilityPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("help"),
    CommandModule("utility"),
    Description("Sends you a list of all available commands, their usage and their description.")]
    public async Task Help(CommandContext ctx, [Description("Command")]string command = "")
    {
        Task.Run(async () =>
        {
            await new HelpCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "command", command }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("user-info"), Aliases("userinfo"),
    CommandModule("utility"),
    Description("Displays information the bot knows about you or the mentioned user.")]
    public async Task UserInfo(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new UserInfoCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot.watcher, ctx);
    }


    [Command("guild-info"),
    CommandModule("utility"),
    Description("Displays information this or the mentioned guild.")]
    public async Task GuildInfo(CommandContext ctx, [Description("GuildId")] ulong? guildId = null)
    {
        Task.Run(async () =>
        {
            await new GuildInfoCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "guildId", guildId }
            });
        }).Add(_bot.watcher, ctx);
    }


    [Command("reminders"),
    CommandModule("utility"),
    Description("Allows you to manage your reminders.")]
    public async Task Reminders(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            await new RemindersCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }


    [Command("avatar"), Aliases("pfp"),
    CommandModule("utility"),
    Description("Displays your or the mentioned user's avatar as an embedded image.")]
    public async Task Avatar(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new AvatarCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("banner"),
    CommandModule("utility"),
    Description("Displays your or the mentioned user's banner as an embedded image.")]
    public async Task Banner(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new BannerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("rank"), Aliases("level", "lvl"),
    CommandModule("utility"),
    Description("Shows your or the mentioned user's rank and rank progress.")]
    public async Task Rank(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new RankCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("leaderboard"),
    CommandModule("utility"),
    Description("Displays the current experience rankings on this server.")]
    public async Task Leaderboard(CommandContext ctx, [Description("3-50")] int ShowAmount = 10)
    {
        Task.Run(async () =>
        {
            await new LeaderboardCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "ShowAmount", ShowAmount }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Command("report-host"),
    CommandModule("utility"),
    Description("Allows you to contribute a new malicious host to our database.")]
    public async Task ReportHost(CommandContext ctx, [Description("Host")] string url)
    {
        Task.Run(async () =>
        {
            await new ReportHostCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "url", url }
            });
        }).Add(_bot.watcher, ctx);
    }




    [Command("emoji"), Aliases("emojis", "emote", "steal", "grab", "sticker", "stickers"),
    CommandModule("utility"),
    Description("Steals all emojis and stickers of a message. Reply to a message to select it.")]
    public async Task EmojiStealer(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            await new EmojiStealerCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }



    [Command("translate"),
    CommandModule("utility"),
    Description("Allows you to translate a message. Reply to a message to select it.")]
    public async Task Translate(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            await new TranslateCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }



    [Command("upload"),
    CommandModule("utility"),
    Description("Upload a file to the bot. Only use when instructed to.")]
    public async Task Upload(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.Message.Attachments.Any())
            {
                _ = ctx.SendSyntaxError("<File>");
                return;
            }

            await new UploadCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "stream", await new HttpClient().GetStreamAsync(ctx.Message.Attachments[0].Url) },
                { "filesize", ctx.Message.Attachments[0].FileSize }
            });
        }).Add(_bot.watcher, ctx);
    }
    
    
    
    [Command("urban-dictionary"),
    CommandModule("utility"),
    Description("Look up a term on Urban Dictionary.")]
    public async Task UrbanDictionary(CommandContext ctx, [RemainingText]string term)
    {
        Task.Run(async () =>
        {
            await new UrbanDictionaryCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "term", term }
            });
        }).Add(_bot.watcher, ctx);
    }



    [Group("data"),
    CommandModule("utility"),
    Description("Allows you to request or manage your user data.")]
    public class Join : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot.watcher, ctx);
        }

        [Command("request"), Description("Allows you to request your user data.")]
        public async Task Request(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.RequestCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("delete"), Description("Allows you to delete your user data.")]
        public async Task Delete(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.DeleteCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("object"), Description("Allows you to stop Ichigo from further processing of your user data.")]
        public async Task Object(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.ObjectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("policy"), Description("Allows you to view how Ichigo processes your data.")]
        public async Task Info(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }



    [Command("credits"),
    CommandModule("utility"),
    Description("Allows you to view who contributed the bot.")]
    public async Task Credits(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            await new CreditsCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
    }
}
