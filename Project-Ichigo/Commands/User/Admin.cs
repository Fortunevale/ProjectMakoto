namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    public Status _status { private get; set; }
    public Users _users { private get; set; }
    public ServerInfo _guilds { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }
    public BumpReminder.BumpReminder _reminder { private get; set; }



    [Command("phishing-settings"), Aliases("phishingsettings", "phishing"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.IsAdmin(_status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows the currently used settings._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to change the currently used settings._"
                });
            }

            if (action.ToLower() == "help")
            {
                await SendHelp(ctx);
                return;
            }
            else if (action.ToLower() == "review")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Detect Phishing Links   ` : {_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                    $"`Punishment Type         ` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                    $"`Custom Punishment Reason` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                    $"`Custom Timeout Length   ` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
                });
                return;
            }
            else if (action.ToLower() == "config")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Detect Phishing Links   ` : {_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                    $"`Punishment Type         ` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                    $"`Custom Punishment Reason` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                    $"`Custom Timeout Length   ` : `{_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), "1", "Toggle Detection") },
                    { new DiscordButtonComponent(ButtonStyle.Primary, "2", "Change Punishment") },
                    { new DiscordButtonComponent(ButtonStyle.Secondary, "3", "Change Reason") },
                    { new DiscordButtonComponent(ButtonStyle.Secondary, "4", "Change Timeout Length") },
                    { new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel") }
                } as IEnumerable<DiscordComponent>));

                var interactivity = ctx.Client.GetInteractivity();
                var button = await interactivity.WaitForButtonAsync(msg, ctx.User, TimeSpan.FromSeconds(60));
                    
                if (button.TimedOut)
                {
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (button.Result.Id == "cancel")
                {
                    _ = msg.DeleteAsync();
                    return;
                }

                await button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                switch (button.Result.Id)
                {
                    case "1":
                    {
                        _ = msg.DeleteAsync();
                        _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        break;
                    }
                    case "2":
                    {
                        var dropdown = new DiscordSelectComponent("selection", "Select an action..", new List<DiscordSelectComponentOption>
                        {
                            { new DiscordSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                        } as IEnumerable<DiscordSelectComponentOption>);

                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                        {
                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                            {
                                switch (e.Values.First())
                                {
                                    case "Ban":
                                        _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = ServerInfo.PhishingPunishmentType.BAN;
                                        break;
                                    case "Kick":
                                        _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = ServerInfo.PhishingPunishmentType.KICK;
                                        break;
                                    case "Timeout":
                                        _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = ServerInfo.PhishingPunishmentType.TIMEOUT;
                                        break;
                                    case "Delete":
                                        _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = ServerInfo.PhishingPunishmentType.DELETE;
                                        break;
                                }

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            }
                        };

                        ctx.Client.ComponentInteractionCreated += RunInteraction;

                        try
                        {
                            await Task.Delay(60000);
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                        break;
                    }
                    case "3":
                    {
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        var msg3 = await ctx.Channel.SendMessageAsync("Please specify a new Ban Reason.\n" +
                                                                        "_Type `cancel` or `.` to cancel._\n\n" +
                                                                        "**Placeholders**\n" +
                                                                        "`%R` - _A placeholder for the reason_");

                        var reason = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                        _ = Task.Delay(2000).ContinueWith(x =>
                        {
                            _ = reason.Result.DeleteAsync();
                        });

                        if (reason.TimedOut)
                        {
                            _ = msg3.DeleteAsync();
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            return;
                        }

                        if (reason.Result.Content.ToLower() is not "cancel" or ".")
                            _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = reason.Result.Content;

                        _ = msg3.DeleteAsync();
                        _ = msg.DeleteAsync();
                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        break;
                    }
                    case "4":
                    {
                        if (_guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != ServerInfo.PhishingPunishmentType.TIMEOUT)
                        {
                            var msg4 = await ctx.Channel.SendMessageAsync("You aren't using `Timeout` as your Punishment");
                            await Task.Delay(5000);
                            _ = msg4.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            return;
                        }

                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        var msg3 = await ctx.Channel.SendMessageAsync("Please specify how long the timeout should last with one of the following suffixes:\n" +
                                                                        "`d` - _Days (default)_\n" +
                                                                        "`h` - _Hours_\n" +
                                                                        "`m` - _Minutes_\n" +
                                                                        "`s` - _Seconds_");

                        var reason = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                        if (reason.TimedOut)
                        {
                            _ = msg3.DeleteAsync();
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
                            _ = msg3.DeleteAsync();
                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
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
                                        length = TimeSpan.FromDays(Convert.ToInt32(reason.Result.Content));
                                        return;
                                }
                            }

                            if (length > TimeSpan.FromDays(28) || length < TimeSpan.FromSeconds(1))
                            {
                                _ = msg3.DeleteAsync();
                                _ = msg.DeleteAsync();
                                var msg4 = await ctx.Channel.SendMessageAsync("The duration has to be between 1 second and 28 days.");
                                await Task.Delay(5000);
                                _ = msg4.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }

                            _guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

                            _ = msg3.DeleteAsync();
                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        }
                        catch (Exception)
                        {
                            _ = msg3.DeleteAsync();
                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            return;
                        }
                        break;
                    }
                }

                return;
            }
            else
            {
                await SendHelp(ctx);
                return;
            }
        }).Add(_watcher, ctx);
    }

    [Command("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("admin"),
    Description("Allows to review, set up and change settings for the Bump Reminder")]
    public async Task BumpReminder(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.IsAdmin(_status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows the currently used settings._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} setup` - _Set up the current channel for bumping._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to change the currently used settings._"
                });
            }

            if (action.ToLower() == "help")
            {
                await SendHelp(ctx);
                return;
            }
            else if (action.ToLower() == "review")
            {
                if (!_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`The Bump Reminder is not set up on this server. Please run '{ctx.Prefix}{ctx.Command.Name} setup' in the channel used for bumping.`"
                    });
                    return;
                }

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Bump Reminder Enabled` : {_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                });
                return;
            }
            else if (action.ToLower() == "setup")
            {
                if (_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`The Bump Reminder is already set up on this server. Please run '{ctx.Prefix}{ctx.Command.Name} config' to change it's settings instead.`"
                    });
                    return;
                }

                if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == Resources.AccountIds.Disboard))
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`The Disboard bot is not on this server. Please create a guild listing on Disboard and invite the their bot.`"
                    });
                    return;
                }

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Setting up Bump Reminder..`"
                };
                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

                List<DiscordSelectComponentOption> roles = new();

                roles.Add(new DiscordSelectComponentOption("Create one for me..", "create"));

                foreach (var role in ctx.Guild.Roles)
                {
                    roles.Add(new DiscordSelectComponentOption(
                        $"@{role.Value.Name} ({role.Value.Id})",
                        role.Value.Id.ToString()));
                }

                async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id != msg.Id || e.User.Id != ctx.User.Id)
                            return;

                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                        embed.Description = "`Setting up Bump Reminder..`";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        ulong id;

                        if (e.Values.First() == "create")
                            id = (await ctx.Guild.CreateRoleAsync("BumpReminder")).Id;
                        else
                            id = Convert.ToUInt64(e.Values.First());

                        var bump_reaction_msg = await ctx.Channel.SendMessageAsync($"React to this message with :white_check_mark: to receive notifications as soon as the server can be bumped again.");
                        _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                        _ = bump_reaction_msg.PinAsync();

                        _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = id;
                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = ctx.Channel.Id;
                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.MessageId = bump_reaction_msg.Id;
                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddHours(-2);
                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddHours(-2);
                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                        _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled = true;

                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        embed.Description = "`The Bump Reminder has been set up.`";
                        embed.Color = ColorHelper.Success;
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();

                        _reminder.SendPersistentMessage(e.Channel, null);
                    }).Add(_watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                var dropdown = new DiscordSelectComponent("selection", "Select a role..", roles as IEnumerable<DiscordSelectComponentOption>);

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`Please select a role to ping when the server can be bumped.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                try
                {
                    await Task.Delay(60000);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                }
                catch { }

                return;
            }
            else if (action.ToLower() == "config")
            {
                if (!_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`The Bump Reminder is not set up on this server. Please run '{ctx.Prefix}{ctx.Command.Name} setup' in the channel used for bumping.`"
                    });
                    return;
                }

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Bump Reminder Enabled` : {_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Danger, "1", "Disable Bump Reminder", !_guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled) },
                        { new DiscordButtonComponent(ButtonStyle.Primary, "2", "Change Channel") },
                        { new DiscordButtonComponent(ButtonStyle.Primary, "3", "Change Role") },
                        { new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel") }
                    } as IEnumerable<DiscordComponent>));

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            switch (e.Interaction.Data.CustomId)
                            {
                                case "1":
                                {
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = 0;
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = 0;
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.MessageId = 0;
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.MinValue;
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.MinValue;
                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                                    _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled = false;
                                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                                    embed.Description = "`The Bump Reminder has been disabled.`";
                                    embed.Color = ColorHelper.Success;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }
                                case "2":
                                {
                                    List<DiscordSelectComponentOption> channels = new();

                                    foreach (var category in await ctx.Guild.GetOrderedChannelsAsync())
                                    {
                                        foreach (var channel in category.Value)
                                            channels.Add(new DiscordSelectComponentOption(
                                                $"#{channel.Name} ({channel.Id})", 
                                                channel.Id.ToString(),
                                                $"{(category.Key != 0 ? $"{channel.Parent.Name} " : "")}"));
                                    }

                                    async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                                    {
                                        Task.Run(async () =>
                                        {
                                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                                            {
                                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                                _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = Convert.ToUInt64(e.Values.First());
                                                _ = msg.DeleteAsync();
                                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                            }
                                        }).Add(_watcher, ctx);
                                    }

                                    ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                    var dropdown = new DiscordSelectComponent("selection", "Select a channel..", channels as IEnumerable<DiscordSelectComponentOption>);
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                                    try
                                    {
                                        await Task.Delay(60000);
                                        embed.Footer.Text += " • Interaction timed out";
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();

                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                    }
                                    catch { }

                                    return;
                                }
                                case "3":
                                {
                                    List<DiscordSelectComponentOption> roles = new();

                                    foreach (var role in ctx.Guild.Roles)
                                    {
                                        roles.Add(new DiscordSelectComponentOption(
                                            $"@{role.Value.Name} ({role.Value.Id})",
                                            role.Value.Id.ToString()));
                                    }

                                    async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                                    {
                                        Task.Run(async () =>
                                        {
                                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                                            {
                                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                                _guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = Convert.ToUInt64(e.Values.First());
                                                _ = msg.DeleteAsync();
                                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                            }
                                        }).Add(_watcher, ctx);
                                    }

                                    ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                    var dropdown = new DiscordSelectComponent("selection", "Select a role..", roles as IEnumerable<DiscordSelectComponentOption>);
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                                    try
                                    {
                                        await Task.Delay(60000);
                                        embed.Footer.Text += " • Interaction timed out";
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();

                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                    }
                                    catch { }

                                    return;
                                }
                                case "cancel":
                                    _ = msg.DeleteAsync();
                                    return;
                            }

                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        }
                    }).Add(_watcher, ctx);
                };

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                try
                {
                    await Task.Delay(60000);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
            }
            else
            {
                await SendHelp(ctx);
                return;
            }
        }).Add(_watcher, ctx);
    }
}
