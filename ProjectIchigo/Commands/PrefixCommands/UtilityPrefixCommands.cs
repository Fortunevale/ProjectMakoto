namespace ProjectIchigo.PrefixCommands;

public class UtilityPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("help"),
    CommandModule("utility"),
    Description("Sends you a list of all available commands, their usage and their description via Direct Messages.")]
    public async Task Help(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (await _bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                return;

            List<KeyValuePair<string, string>> Commands = new();


            foreach (var command in ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()))
            {
                if (command.Value.CustomAttributes.OfType<CommandModuleAttribute>() is null)
                    continue;

                var module = command.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString;

                switch (module)
                {
                    case "admin":
                        if (!ctx.Member.IsAdmin(_bot.status))
                            continue;
                        break;
                    case "maintainence":
                        if (!ctx.Member.IsMaintenance(_bot.status))
                            continue;
                        break;
                    case "hidden":
                        continue;
                    default:
                        break;
                }

                Commands.Add(new KeyValuePair<string, string>($"{module.FirstLetterToUpper()} Commands", $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_"));
            }

            var Fields = Commands.PrepareEmbedFields();
            var Embeds = Fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value
                .Replace("##Prefix##", ctx.Prefix)
                .Replace("##n##", "\n")))
                .ToList().PrepareEmbeds("", "All available commands will be listed below.\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                            "**Do not include the brackets when using commands, they're merely an indicator for requirement.**");

            try
            {
                foreach (var b in Embeds)
                    await ctx.Member.SendMessageAsync(embed: b.WithAuthor(ctx.Guild.Name, "", ctx.Guild.IconUrl).WithFooter(ctx.GenerateUsedByFooter().Text, ctx.GenerateUsedByFooter().IconUrl).WithTimestamp(DateTime.UtcNow).WithColor(EmbedColors.Info).Build());

                var successembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = ":mailbox_with_mail: `You got mail! Please check your dm's.`",
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Color = EmbedColors.Success
                };

                await ctx.Channel.SendMessageAsync(embed: successembed);
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = "`It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Color = EmbedColors.Error,
                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                };

                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                await ctx.Channel.SendMessageAsync(embed: errorembed);
            }
            catch (Exception)
            {
                throw;
            }
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



    [Command("submit-url"),
    CommandModule("utility"),
    Description("Allows you to contribute a new malicious domain to our database.")]
    public async Task UrlSubmit(CommandContext ctx, [Description("URL")] string url)
    {
        Task.Run(async () =>
        {
            await new UrlSubmitCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
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
    public async Task UrbanDictionary(CommandContext ctx, string term)
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
