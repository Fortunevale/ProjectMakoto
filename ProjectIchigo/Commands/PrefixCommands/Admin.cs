namespace ProjectIchigo.PrefixCommands;

internal class Admin : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("join"), Aliases("joinsettings", "join-settings"),
    CommandModule("admin"), 
    Description("Allows to review and change settings in the event somebody joins the server")]
    public class JoinSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows the currently used settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modification of the currently used settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("experience"), Aliases("experiencesettings", "experience-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings related to experience")]
    public class ExperienceSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows the currently used settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modification of the currently used settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("levelrewards"), Aliases("level-rewards", "rewards"),
    CommandModule("admin"),
    Description("Allows to review, add, remove and modify Level Rewards")]
    public class LevelRewards : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Level Rewards")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows adding, removing and modifying currently defined Level Rewards")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("phishing"), Aliases("phishingsettings", "phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the phishing detection")]
    public class PhishingSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Phishing Protection settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Phishing Protection settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("admin"),
    Description("Allows to review, set up and change settings for the Bump Reminder")]
    public class BumpReminder : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Bump Reminder settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Bump Reminder settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("actionlog"), Aliases("action-log"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the actionlog")]
    public class ActionLog : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Actionlog Settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }


        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Actionlog settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("autocrosspost"), Aliases("auto-crosspost", "crosspost"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the automatic crossposts")]
    public class AutoCrosspost : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            return $"`Exclude Bots             `: {_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots.BoolToEmote(ctx.Client)}\n" +
                   $"`Delay before crossposting`: `{TimeSpan.FromSeconds(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting).GetHumanReadable()}`\n\n" +
                   $"{(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count != 0 ? string.Join("\n\n", _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : "`No Auto Crosspost Channels set up.`")}";
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Auto Crosspost Channels")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Auto Crosspost Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined Auto Crosspost Channels and settings related to it")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                foreach (var b in _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                    if (!ctx.Guild.Channels.ContainsKey(b))
                        _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set delay", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                var ExcludeBots = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Exclude Bots", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));
                var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    ExcludeBots,
                    SetDelayButton
                })
                .AddComponents(new List<DiscordComponent>
                {
                    AddButton,
                    RemoveButton
                }).AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == ExcludeBots.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots = !_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == SetDelayButton.CustomId)
                {
                    embed.Description = "Please specify how long to delay the crossposting:\n" +
                    "`m` - _Minutes_\n" +
                    "`s` - _Seconds_\n\n" +
                    "For example, `10s` would result to 10 seconds.";
                    embed.Color = EmbedColors.AwaitingInput;

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    var interactivity = ctx.Client.GetInteractivity();
                    var reason = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                    if (reason.TimedOut)
                    {
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        return;
                    }

                    _ = Task.Delay(2000).ContinueWith(x =>
                    {
                        _ = reason.Result.DeleteAsync();
                    });

                    if (reason.Result.Content.ToLower() is "cancel" or ".")
                    {
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    try
                    {
                        if (!TimeSpan.TryParse(reason.Result.Content, out TimeSpan length))
                        {
                            switch (reason.Result.Content[^1..])
                            {
                                case "d":
                                    length = TimeSpan.FromDays(Convert.ToInt32(reason.Result.Content.Replace("d", "")));
                                    break;
                                case "h":
                                    length = TimeSpan.FromHours(Convert.ToInt32(reason.Result.Content.Replace("h", "")));
                                    break;
                                case "m":
                                    length = TimeSpan.FromMinutes(Convert.ToInt32(reason.Result.Content.Replace("m", "")));
                                    break;
                                case "s":
                                    length = TimeSpan.FromSeconds(Convert.ToInt32(reason.Result.Content.Replace("s", "")));
                                    break;
                                default:
                                    length = TimeSpan.FromSeconds(Convert.ToInt32(reason.Result.Content));
                                    return;
                            }
                        }

                        if (length > TimeSpan.FromMinutes(5) || length < TimeSpan.FromSeconds(1))
                        {
                            embed.Description = "`The duration has to be between 1 second and 5 minutes.`";
                            embed.Color = EmbedColors.Error;
                            embed.Author.IconUrl = Resources.LogIcons.Error;

                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            _ = ctx.Command.ExecuteAsync(ctx);
                            return;
                        }

                        _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting = Convert.ToInt32(length.TotalSeconds);

                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                    }
                    catch (Exception)
                    {
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == AddButton.CustomId)
                {
                    if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                    {
                        embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {_bot._status.DevelopmentServerInvite}";
                        embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    DiscordChannel channel;

                    try
                    {
                        channel = await GenericSelectors.PromptChannelSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                    }
                    catch (ArgumentException)
                    {
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        return;
                    }

                    if (channel.Type != ChannelType.News)
                    {
                        embed.Description = "`The channel you selected is not an announcement channel.`";
                        embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                    {
                        embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {_bot._status.DevelopmentServerInvite}";
                        embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (!_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(channel.Id))
                        _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Add(channel.Id);

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;

                }
                else if (e.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
                {
                    if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count == 0)
                    {
                        embed.Description = $"`No Crosspost Channels are set up.`";
                        embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    ulong ChannelToRemove;

                    try
                    {
                        var channel = await GenericSelectors.PromptCustomSelection(_bot, _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels
                            .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList(),
                            ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        ChannelToRemove = Convert.ToUInt64(channel);
                    }
                    catch (ArgumentException)
                    {
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        return;
                    }

                    if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(ChannelToRemove))
                        _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(ChannelToRemove);

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }
            }).Add(_bot._watcher, ctx);
        }
    }


    [Group("reactionroles"), Aliases("reactionrole", "reaction-roles", "reaction-role"),
    CommandModule("admin"),
    Description("Allows to review and change settings for Reaction Roles")]
    public class ReactionRoles : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        Dictionary<ulong, DiscordMessage> messageCache = new();

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            messageCache.Clear();

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        async Task CheckForInvalid(CommandContext ctx)
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                return;

            foreach (var b in _bot._guilds.List[ctx.Guild.Id].ReactionRoles.ToList())
            {
                if (!ctx.Guild.Channels.ContainsKey(b.Value.ChannelId))
                {
                    _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }

                if (!ctx.Guild.Roles.ContainsKey(b.Value.RoleId))
                {
                    _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }

                var channel = ctx.Guild.GetChannel(b.Value.ChannelId);

                if (!messageCache.ContainsKey(b.Key))
                {
                    try
                    {
                        var requested_msg = await channel.GetMessageAsync(b.Key);
                        messageCache.Add(b.Key, requested_msg);
                    }
                    catch (DisCatSharp.Exceptions.NotFoundException)
                    {
                        messageCache.Add(b.Key, null);

                        _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);
                        continue;
                    }
                    catch (DisCatSharp.Exceptions.UnauthorizedException)
                    {
                        messageCache.Add(b.Key, null);

                        _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);
                        continue;
                    }
                }

                if (messageCache[b.Key] == null)
                {
                    _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }

                var msg = messageCache[b.Key];

                if (!msg.Reactions.Any(x => x.Emoji.Id == b.Value.EmojiId && x.Emoji.GetUniqueDiscordName() == b.Value.EmojiName && x.IsMe))
                {
                    _ = msg.CreateReactionAsync(b.Value.GetEmoji(ctx.Client));
                    continue;
                }
            }
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "\n\n_To fulfill the `<MessageReply>` requirement, simply reply to a message you want to perform the action on._", "https://media.discordapp.net/attachments/906976602557145110/967751607418761257/unknown.png");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "\n\n_To fulfill the `<MessageReply>` requirement, simply reply to a message you want to perform the action on._", "https://media.discordapp.net/attachments/906976602557145110/967751607418761257/unknown.png");
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows a list of all currently defined Auto Crosspost Channels")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Loading Reaction Roles..`"
                });

                await CheckForInvalid(ctx);

                List<string> Desc = new();

                if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count == 0)
                {
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                        Color = EmbedColors.Info,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = "`No reaction roles are set up.`"
                    }.Build());
                    return;
                }

                foreach (var b in _bot._guilds.List[ctx.Guild.Id].ReactionRoles)
                {
                    var channel = ctx.Guild.GetChannel(b.Value.ChannelId);
                    var role = ctx.Guild.GetRole(b.Value.RoleId);
                    var message = messageCache[b.Key];

                    Desc.Add($"[`Message`]({message.JumpLink}) in {channel.Mention} `[#{channel.Name}]`\n" +
                            $"{b.Value.GetEmoji(ctx.Client)} - {role.Mention} `{role.Name}`");
                }

                List<string> Sections = new();
                string build = "";

                foreach (var b in Desc)
                {
                    string curstr = $"{b}\n\n";

                    if (build.Length + curstr.Length > 4096)
                    {
                        Sections.Add(build);
                        build = "";
                    }

                    build += curstr;
                }

                if (build.Length > 0)
                {
                    Sections.Add(build);
                    build = "";
                }

                List<DiscordEmbedBuilder> embeds = Sections.Select(x => new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = x
                }).ToList();

                foreach (var b in embeds)
                    _ = ctx.Channel.SendMessageAsync(b);

                _ = msg.DeleteAsync();
                return;
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to add, delete and modify reaction roles")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Loading Reaction Roles..`"
                });

                await CheckForInvalid(ctx);

                var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add a new reaction role", (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a reaction role", (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count} reaction roles are set up.`"
                };

                _ = msg.DeleteAsync();
                msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent> 
                { 
                    AddButton, RemoveButton 
                })
                .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == AddButton.CustomId)
                {
                    var action_embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                        Color = EmbedColors.AwaitingInput,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = "`Please copy and send the message link of the message you want the reaction role to be added to.`",
                        ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                    };

                    if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count > 100)
                    {
                        action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                        action_embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));

                    var link = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(2));

                    if (link.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    try
                    { _ = link.Result.DeleteAsync(); }
                    catch { }

                    if (!Regex.IsMatch(link.Result.Content, Resources.Regex.DiscordChannelUrl))
                    {
                        action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                                   $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                        action_embed.Color = EmbedColors.Error;
                        action_embed.ImageUrl = "";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (!link.Result.Content.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                    {
                        action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                                   $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                   $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                        action_embed.Color = EmbedColors.Error;
                        action_embed.ImageUrl = "";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (GuildId != ctx.Guild.Id)
                    {
                        action_embed.Description = $"`The link you provided leads to another server.`";
                        action_embed.Color = EmbedColors.Error;
                        action_embed.ImageUrl = "";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                    {
                        action_embed.Description = $"`The link you provided leads to a channel that doesn't exist.`";
                        action_embed.Color = EmbedColors.Error;
                        action_embed.ImageUrl = "";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    var channel = ctx.Guild.GetChannel(ChannelId);

                    if (!channel.TryGetMessage(MessageId, out DiscordMessage reactionMessage))
                    {
                        action_embed.Description = $"`The link you provided leads a message that doesn't exist or the bot has no access to.`";
                        action_embed.Color = EmbedColors.Error;
                        action_embed.ImageUrl = "";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    action_embed.Description = "`Please react with the emoji you want to use for the reaction role.`";
                    action_embed.ImageUrl = "";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));

                    var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == msg.Id, TimeSpan.FromMinutes(2));

                    if (emoji_wait.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    try
                    { _ = emoji_wait.Result.Message.DeleteAllReactionsAsync(); }
                    catch { }

                    var emoji = emoji_wait.Result.Emoji;

                    if (emoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji.Id))
                    {
                        action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                        action_embed.Color = EmbedColors.Error;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    action_embed.Description = "`Please select the role you want to use.`";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    msg = await ctx.Channel.GetMessageAsync(msg.Id);

                    try
                    {
                        var role = await GenericSelectors.PromptRoleSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count > 100)
                        {
                            action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                            action_embed.Color = EmbedColors.Error;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            _ = ctx.Command.ExecuteAsync(ctx);
                            return;
                        }


                        if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == MessageId && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                            action_embed.Color = EmbedColors.Error;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            _ = ctx.Command.ExecuteAsync(ctx);
                            return;
                        }

                        if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == role.Id))
                        {
                            action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                            action_embed.Color = EmbedColors.Error;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            _ = ctx.Command.ExecuteAsync(ctx);
                            return;
                        }

                        _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoles>(reactionMessage.Id, new Entities.ReactionRoles
                        {
                            ChannelId = ChannelId,
                            RoleId = role.Id,
                            EmojiId = emoji.Id,
                            EmojiName = emoji.GetUniqueDiscordName()
                        }));

                        await reactionMessage.CreateReactionAsync(emoji);

                        action_embed.Color = EmbedColors.Info;
                        action_embed.Description = $"`Added role` {role.Mention} `to message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {emoji} `.`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
                {
                    try
                    {
                        msg = await ctx.Channel.GetMessageAsync(msg.Id);
                        var roleuuid = await GenericSelectors.PromptCustomSelection(_bot, _bot._guilds.List[ctx.Guild.Id].ReactionRoles
                                                        .Select(x => new DiscordSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList(),
                                                        ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        var obj = _bot._guilds.List[ctx.Guild.Id].ReactionRoles.First(x => x.Value.UUID == roleuuid);

                        var role = ctx.Guild.GetRole(obj.Value.RoleId);
                        var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
                        var reactionMessage = await channel.GetMessageAsync(obj.Key);
                        _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                        _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(obj);

                        embed.Color = EmbedColors.Info;
                        embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        return;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }

            }).Add(_bot._watcher, ctx);
        }

        [Command("add"), Description("Allows adding a reaction role to a message directly, skipping the lengthy questioning. **This command requires replying to a message.**"), Priority(0)]
        public async Task Add(CommandContext ctx, DiscordEmoji emoji_parameter, DiscordRole role_parameter)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Message.ReferencedMessage is null)
                {
                    _ = ctx.SendSyntaxError(" <Message Reply>");
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.AwaitingInput,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Adding reaction role..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count > 100)
                {
                    action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
                {
                    action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
                {
                    action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
                {
                    action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                await ctx.Message.ReferencedMessage.CreateReactionAsync(emoji_parameter);

                _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoles>(ctx.Message.ReferencedMessage.Id, new Entities.ReactionRoles
                {
                    ChannelId = ctx.Message.ReferencedMessage.Channel.Id,
                    RoleId = role_parameter.Id,
                    EmojiId = emoji_parameter.Id,
                    EmojiName = emoji_parameter.GetUniqueDiscordName()
                }));

                action_embed.Color = EmbedColors.Info;
                action_embed.Description = $"`Added role` {role_parameter.Mention} `to message sent by` {ctx.Message.ReferencedMessage.Author.Mention} `in` {ctx.Message.ReferencedMessage.Channel.Mention} `with emoji` {emoji_parameter} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
            }).Add(_bot._watcher, ctx);
        }

        [Command("add"), Description("Allows adding a reaction role to a message directly, skipping the lengthy questioning. **This command requires replying to a message.**"), Priority(1)]
        public async Task Add2(CommandContext ctx, DiscordRole role_parameter, DiscordEmoji emoji_parameter) => await Add(ctx, emoji_parameter, role_parameter);

        [Command("remove"), Description("Allows removing a specific reaction role from a message directly, skipping the lengthy questioning. **This command requires replying to a message.**")]
        public async Task Remove(CommandContext ctx, DiscordEmoji emoji_parameter)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Message.ReferencedMessage is null)
                {
                    _ = ctx.SendSyntaxError(" <Message Reply>");
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.AwaitingInput,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Removing reaction role..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (!_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName()))
                {
                    action_embed.Description = $"`The specified message doesn't contain specified reaction.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                var obj = _bot._guilds.List[ctx.Guild.Id].ReactionRoles.First(x => x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName());

                var role = ctx.Guild.GetRole(obj.Value.RoleId);
                var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
                var reactionMessage = await channel.GetMessageAsync(obj.Key);
                _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(obj);

                action_embed.Color = EmbedColors.Info;
                action_embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
            }).Add(_bot._watcher, ctx);
        }

        [Command("removeall"), Description("Allows removing all reaction roles from a message directly, skipping the lengthy questioning. **This command requires replying to a message.**")]
        public async Task Remove(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Message.ReferencedMessage is null)
                {
                    _ = ctx.SendSyntaxError(" <Message Reply>");
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.AwaitingInput,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Removing all reaction roles..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (!_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == ctx.Message.ReferencedMessage.Id))
                {
                    action_embed.Description = $"`The specified message doesn't contain any reaction roles.`";
                    action_embed.Color = EmbedColors.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                foreach (var b in _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Where(x => x.Key == ctx.Message.ReferencedMessage.Id).ToList())
                    _bot._guilds.List[ctx.Guild.Id].ReactionRoles.Remove(b);

                _ = ctx.Message.ReferencedMessage.DeleteAllReactionsAsync();

                action_embed.Color = EmbedColors.Info;
                action_embed.Description = $"`Removed all reaction roles from message sent by` {ctx.Message.ReferencedMessage.Author.Mention} `in` {ctx.Message.ReferencedMessage.Channel.Mention} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
            }).Add(_bot._watcher, ctx);
        }
    }


    [Group("invoiceprivacy"), Aliases("in-voice-privacy", "vc-privacy", "vcprivacy"),
    CommandModule("admin"),
    Description("Allows to review and change In-Voice Text Channel Privacy Settings")]
    public class InVoiceTextPrivacy : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            return $"`Clear Messages on empty Voice Channel`: {_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled.BoolToEmote(ctx.Client)}\n" +
                   $"`Set Permissions on User Join         `: {_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled.BoolToEmote(ctx.Client)}";
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "In-Voice Text Channel Privacy");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "In-Voice Text Channel Privacy");
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows currently defined settings for In-Voice Text Channel Privacy")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"In-Voice Text Channel Privacy • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined In-Voice Text Channel Privacy Settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"In-Voice Text Channel Privacy • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var ToggleDeletion = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Message Deletion", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
                var TogglePermission = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Permission Protection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    ToggleDeletion,
                    TogglePermission
                })
                .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == ToggleDeletion.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled = !_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == TogglePermission.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled = !_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);

                    if (_bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled)
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.SendMessages , "Enabled In-Voice Privacy");
                        }
                    }
                    else
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.DeleteOverwriteAsync(ctx.Guild.EveryoneRole, "Disabled In-Voice Privacy");
                        }
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }

            }).Add(_bot._watcher, ctx);
        }
    }


    [Group("invitetracker"), Aliases("invite-tracker", "invitetracking", "invite-tracking"),
    CommandModule("admin"),
    Description("Allows to review and change Invite Tracker Settings")]
    public class InviteTracker : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            return $"`Invite Tracker Enabled`: {_bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled.BoolToEmote(ctx.Client)}";
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "Invite Tracker");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "Invite Tracker");
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows currently defined settings for Invite Tracking Settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Invite Tracker • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined Invite Tracking Settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Invite Tracker • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var Toggle = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Invite Tracking", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📲")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    Toggle
                })
                .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == Toggle.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled = !_bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled;

                    if (_bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled)
                        _ = InviteTrackerEvents.UpdateCachedInvites(_bot, ctx.Guild);

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }

            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("namenormalizer"), Aliases("name-normalizer"),
    CommandModule("admin"),
    Description("Allows to review and change Name Normalizer Settings")]
    public class NameNormalizer : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            return $"`Name Normalizer Enabled`: {_bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled.BoolToEmote(ctx.Client)}";
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "Name Normalizer");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "Name Normalizer");
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows currently defined settings for Name Normalizer")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Name Normalizer • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined Name Normalizer Settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Name Normalizer • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var Toggle = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Name Normalizer", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                var SearchAllNames = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Normalize Everyone's Names", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    Toggle,
                    SearchAllNames
                })
                .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == Toggle.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled = !_bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == SearchAllNames.CustomId)
                {
                    if (_bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning)
                    {
                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        embed.Color = EmbedColors.Error;
                        embed.Description = $"`A normalizer is already running.`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                        return;

                    _bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = true;

                    try
                    {
                        embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                        embed.Color = EmbedColors.Loading;
                        embed.Description = $"`Renaming all members. This might take a while..`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        var members = await ctx.Guild.GetAllMembersAsync();
                        int Renamed = 0;

                        for (int i = 0; i < members.Count; i++)
                        {
                            var b = members.ElementAt(i);

                            string PingableName = Regex.Replace(b.DisplayName.Normalize(NormalizationForm.FormKC), @"[^a-zA-Z0-9 _\-!.,:;#+*~´`?^°<>|""§$%&\/\\()={\[\]}²³€@_]", "");

                            if (PingableName.IsNullOrWhiteSpace())
                                PingableName = "Pingable Name";

                            if (PingableName != b.DisplayName)
                            {
                                _ = b.ModifyAsync(x => x.Nickname = PingableName);
                                Renamed++;
                                await Task.Delay(5000);
                            }
                        }

                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        embed.Color = EmbedColors.Info;
                        embed.Description = $"`Renamed {Renamed} members.`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        await Task.Delay(5000);
                        _bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = false;
                    }
                    catch (Exception)
                    {
                        _bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = false;
                        throw;
                    }

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }

            }).Add(_bot._watcher, ctx);
        }

        [Command("test"),
        Description(" ")]
        public async Task Test(CommandContext ctx, [RemainingText]string test)
        {
            Task.Run(async () =>
            {
                string PingableName = Regex.Replace(test.Normalize(NormalizationForm.FormKC), @"[^a-zA-Z0-9 _\-!.,:;#+*~´`?^°<>|""§$%&\/\\()={\[\]}²³€@_]", "");

                if (PingableName.IsNullOrWhiteSpace())
                    PingableName = "Pingable Name";

                await ctx.RespondAsync(PingableName);
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("autounarchive"), Aliases("auto-unarchive"),
    CommandModule("admin"),
    Description("Allows to review and change Auto Thread Unarchiver Settings")]
    public class AutoUnarchive : BaseCommandModule
    {
        string ReadableModuleName = "Auto Thread Unarchiver";

        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            foreach (var b in _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.ToList())
            {
                if (!ctx.Guild.Channels.ContainsKey(b))
                    _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Remove(b);
            }

            return $"{(_bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Any() ? string.Join("\n", _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Select(x => $"{ctx.Guild.GetChannel(x).Mention} [`#{ctx.Guild.GetChannel(x).Name}`] (`{x}`)")) : "`No channels defined.`")}";
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**", "", ReadableModuleName);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**", "", ReadableModuleName);
            }).Add(_bot._watcher, ctx);
        }

        [Command("review"), Aliases("list"),
        Description("Shows currently defined settings for Auto Thread Unarchiver")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"{ReadableModuleName} • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined Auto Thread Unarchiver Settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"{ReadableModuleName} • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"{GetCurrentConfiguration(ctx)}\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**"
                };

                var Add = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add new channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var Remove = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    Add,
                    Remove
                })
                .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == Add.CustomId)
                {
                    DiscordChannel channel;

                    try
                    {
                        channel = await GenericSelectors.PromptChannelSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    if (!_bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Contains(channel.Id))
                        _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Add(channel.Id);

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == Remove.CustomId)
                {
                    ulong ChannelToRemove;

                    try
                    {
                        var channel = await GenericSelectors.PromptCustomSelection(_bot, _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads
                            .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList(),
                            ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        ChannelToRemove = Convert.ToUInt64(channel);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    if (_bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Contains(ChannelToRemove))
                        _bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Remove(ChannelToRemove);

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }

            }).Add(_bot._watcher, ctx);
        }
    }
}
