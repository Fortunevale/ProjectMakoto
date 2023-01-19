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

        [Command("delete"), Description("Allows you to delete your user data and stop Ichigo from further processing of your user data.")]
        public async Task Delete(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Data.DeleteCommand().ExecuteCommand(ctx, _bot);
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



    [Command("language"),
    CommandModule("utility"),
    Description("Change the language Ichigo uses.")]
    public async Task Language(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            await new LanguageCommand().ExecuteCommand(ctx, _bot);
        }).Add(_bot.watcher, ctx);
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


    [Group("vcc"),
    CommandModule("utility"),
    Description("Allows you to modify your own voice channel.")]
    public class VcCreatorManagement : BaseCommandModule
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

        [Command("open"), Description("Opens your channel so new users can freely join.")]
        public async Task Open(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.OpenCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("close"), Description("Closes your channel. You have to invite people for them to join.")]
        public async Task Close(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.CloseCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("name"), Description("Changes the name of your channel.")]
        public async Task Name(CommandContext ctx, [RemainingText]string newName)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.NameCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "newName", newName },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("limit"), Description("Changes the user limit of your channel.")]
        public async Task Limit(CommandContext ctx, uint newLimit)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.LimitCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "newLimit", newLimit },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("invite"), Description("Invites a new person to your channel.")]
        public async Task Invite(CommandContext ctx, DiscordMember victim)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.InviteCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("kick"), Description("Kicks person from your channel.")]
        public async Task Kick(CommandContext ctx, DiscordMember victim)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.KickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("ban"), Description("Bans person from your channel.")]
        public async Task Ban(CommandContext ctx, DiscordMember victim)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.BanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("unban"), Description("Unbans person from your channel.")]
        public async Task Unban(CommandContext ctx, DiscordMember victim)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.UnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("change-owner"), Description("Sets a new person to be the owner of your channel.")]
        public async Task ChangeOwner(CommandContext ctx, DiscordMember victim)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreator.ChangeOwnerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }
    }
}
