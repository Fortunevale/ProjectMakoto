namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    public Status _status { private get; set; }
    public Users _users { private get; set; }
    public ServerInfo _guilds { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }



    [Command("phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            try
            {
                if (!ctx.Member.IsAdmin(_status))
                {
                    _ = ctx.SendAdminError();
                    return;
                }

                if (action.ToLower() == "help")
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.Now,
                        Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                      $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows the currently used settings._\n" +
                                      $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to change the currently used settings._"
                    });
                    return;
                }
                else if (action.ToLower() == "review")
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.Now,
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
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.Now,
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
            }
            catch (Exception ex)
            {
                LogError($"{ex}");
            }
        }).Add(_watcher, ctx);
    }
}
