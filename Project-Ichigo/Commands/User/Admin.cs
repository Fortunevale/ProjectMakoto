using Project_Ichigo.Exceptions;

namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("join"), Aliases("joinsettings", "join-settings"),
    CommandModule("admin"), 
    Description("Allows to review and change settings in the event somebody joins the server")]
    public class JoinSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                throw new CancelCommandException("User is missing apprioriate permissions");
            }
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Autoban Globally Banned Users` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote()}\n" +
                                $"`Joinlog Channel              ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote())}\n" +
                                $"`Role On Join                 ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote())}\n" +
                                $"`Re-Apply Roles on Rejoin     ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles.BoolToEmote()}\n" +
                                $"`Re-Apply Nickname on Rejoin  ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname.BoolToEmote()}"
                });
            }).Add(_bot._watcher);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modification of the currently used settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Autoban Globally Banned Users` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote()}\n" +
                                      $"`Joinlog Channel              ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote())}\n" +
                                      $"`Role On Join                 ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote())}\n" +
                                      $"`Re-Apply Roles on Rejoin     ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles.BoolToEmote()}\n" +
                                      $"`Re-Apply Nickname on Rejoin  ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname.BoolToEmote()}\n\n" +
                                      $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                                      $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied."
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var msg = await ctx.Channel.SendMessageAsync(builder.AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), "toggle_global_ban", "Toggle Global Bans") },
                    { new DiscordButtonComponent(ButtonStyle.Primary, "change_joinlog_channel", "Change Joinlog Channel") },
                    { new DiscordButtonComponent(ButtonStyle.Primary, "change_role_on_join", "Change Role assigned on join") },
                    { new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), "toggle_reapply_roles", "Toggle Role Re-Apply") },
                    { new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), "toggle_reapply_nickname", "Toggle Nickname Re-Apply") }
                } as IEnumerable<DiscordComponent>).AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel")));

                CancellationTokenSource cancellationTokenSource = new();

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Interaction.Data.CustomId == "toggle_global_ban")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans;

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                cancellationTokenSource.Cancel();
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "toggle_reapply_roles")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                _bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles;

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                cancellationTokenSource.Cancel();
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "toggle_reapply_nickname")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                _bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname;

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                cancellationTokenSource.Cancel();
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "change_joinlog_channel")
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                try
                                {
                                    var channel = await new GenericSelectors(_bot).PromptChannelSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "joinlog", ChannelType.Text, true, "Disable Joinlog");

                                    if (channel is null)
                                        _bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId = 0;
                                    else
                                        _bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId = channel.Id;

                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
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
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "change_role_on_join")
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                try
                                {
                                    var role = await new GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "AutoAssignedRole", true, "Disable Role on join");

                                    if (role is null)
                                        _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = 0;
                                    else
                                        _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = role.Id;

                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }
                                catch (ArgumentException)
                                {
                                    embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                _ = msg.DeleteAsync();
                                cancellationTokenSource.Cancel();
                                return;
                            }

                            try
                            {
                                cancellationTokenSource.Cancel();
                                cancellationTokenSource = new();
                                await Task.Delay(120000, cancellationTokenSource.Token);
                                embed.Footer.Text += " • Interaction timed out";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                _ = msg.DeleteAsync();

                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            }
                            catch { }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
            }).Add(_bot._watcher);
        }
    }


    [Command("experience"), Aliases("experiencesettings", "experience-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings related to experience")]
    public async Task ExperienceSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
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
            else if (action.ToLower() is "review" or "list")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience Enabled          ` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote()}\n" +
                                  $"`Experience Boost for Bumpers` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote()}"
                });
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience Enabled          ` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote()}\n" +
                                  $"`Experience Boost for Bumpers` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote()}"
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var msg = await ctx.Channel.SendMessageAsync(builder.AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), "1", "Toggle Experience System") },
                    { new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), "2", "Toggle Experience Boost for Bumpers") },
                    { new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel") }
                } as IEnumerable<DiscordComponent>));

                CancellationTokenSource cancellationTokenSource = new();

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            if (e.Interaction.Data.CustomId == "1")
                            {
                                _bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience = !_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience;
                            }
                            else if (e.Interaction.Data.CustomId == "2")
                            {
                                _bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder = !_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder;
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                cancellationTokenSource.Cancel();
                                _ = msg.DeleteAsync();
                                return;
                            }

                            cancellationTokenSource.Cancel();
                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            return;
                        }
                    }).Add(_bot._watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                try
                {
                    await Task.Delay(60000, cancellationTokenSource.Token);
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("levelrewards"), Aliases("level-rewards", "rewards"),
    CommandModule("admin"),
    Description("Allows to review, add and remove levelreward roles")]
    public async Task LevelRewards(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Level Rewards • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                    $"`{ctx.Prefix}{ctx.Command.Name} review` - _Displays a list of all level rewards._\n" +
                                    $"`{ctx.Prefix}{ctx.Command.Name} add` - _Adds new level rewards._\n" +
                                    $"`{ctx.Prefix}{ctx.Command.Name} modify` - _Allows deletion and modification of existing level rewards._\n"
                });
            }

            if (action.ToLower() == "help")
            {
                _ = SendHelp(ctx);
                return;
            }
            else if (action.ToLower() == "add")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Level Rewards • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Select a role to assign.`"
                };

                CancellationTokenSource cancellationTokenSource = new();
                var msg = await ctx.Channel.SendMessageAsync(embed);

                DiscordRole role;

                try
                {
                    role = await new GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                }
                catch (ArgumentException)
                {
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == role.Id))
                {
                    embed.Description = "`The role you're trying to add has already been assigned to a level.`";
                    embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                embed.Description = $"`Selected` <@&{role}> `({role}). At what Level should this role be assigned?`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                if (result.TimedOut)
                {
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                int level;

                try
                {
                    level = Convert.ToInt32(result.Result.Content);
                }
                catch (Exception)
                {
                    embed.Description = "`You must specify a valid level.`";
                    embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                embed.Description = $"`Selected` <@&{role}> `({role}). It will be assigned at Level {level}. Do you want to display a custom message when the role gets assigned?`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
                                {
                                    { new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes") },
                                    { new DiscordButtonComponent(ButtonStyle.Danger, "no", "No") }
                                }));

                _ = result.Result.DeleteAsync();

                async Task CustomMessageInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new();

                            string Message = "";

                            if (e.Interaction.Data.CustomId == "yes")
                            {
                                embed.Description = $"`Selected` <@&{role}> `({role}). It will be assigned at Level {level}. Please type out your custom message. (<256 characters)`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                                if (result.TimedOut)
                                {
                                    embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                _ = result.Result.DeleteAsync();

                                if (result.Result.Content.Length > 256)
                                {
                                    embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                Message = result.Result.Content;
                            }

                            _bot._guilds.List[ctx.Guild.Id].LevelRewards.Add(new Objects.LevelReward
                            {
                                Level = level,
                                RoleId = role.Id,
                                Message = (string.IsNullOrEmpty(Message) ? "You received ##Role##!" : Message)
                            });

                            embed.Description = $"`The role` <@&{role}> `({role}) will be assigned at Level {level}.`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        }
                    }).Add(_bot._watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += CustomMessageInteraction;

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= CustomMessageInteraction;
                }
                catch { }
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                int current_page = 0;

                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Count == 0)
                {
                    var ListEmbed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Level Rewards • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`No level rewards are set up. Run '{ctx.Prefix}{ctx.Command.Name} add' to add one.`"
                    };
                    await ctx.Channel.SendMessageAsync(embed: ListEmbed);
                    return;
                }

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Level Rewards • {ctx.Guild.Name}" },
                    Color = ColorHelper.AwaitingInput,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Select a level reward to modify.`"
                };

                var msg = await ctx.Channel.SendMessageAsync(embed);

                string selected = "";

                async Task RefreshRewardList()
                {
                    List<DiscordSelectComponentOption> roles = new();

                    foreach (var reward in _bot._guilds.List[ctx.Guild.Id].LevelRewards.ToList().OrderBy(x => x.Level))
                    {
                        if (!ctx.Guild.Roles.ContainsKey(reward.RoleId))
                        {
                            _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(reward);
                            continue;
                        }

                        var role = ctx.Guild.GetRole(reward.RoleId);

                        roles.Add(new DiscordSelectComponentOption($"Level {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

                        if (selected == role.Id.ToString())
                        {
                            embed.Description = $"**Level**: `{reward.Level}`\n" +
                                                $"**Role**: <@&{reward.RoleId}> (`{reward.RoleId}`)\n" +
                                                $"**Message**: `{reward.Message}`\n";
                        }
                    }

                    var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                    var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                    var modify_button = new DiscordButtonComponent(ButtonStyle.Primary, "modify", "Modify Message", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrows_counterclockwise:")));
                    var delete_button = new DiscordButtonComponent(ButtonStyle.Danger, "delete", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                    var cancel_button = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel");

                    var dropdown = new DiscordSelectComponent("reward_selection", "Select a level reward..", roles.Skip(current_page * 20).Take(20) as IEnumerable<DiscordSelectComponentOption>);
                    var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                    if (roles.Skip(current_page * 20).Count() > 20)
                        builder.AddComponents(next_page_button);

                    if (current_page != 0)
                        builder.AddComponents(previous_page_button);

                    if (selected != "")
                    {
                        builder.AddComponents(new List<DiscordComponent> { modify_button, delete_button});
                    }

                    builder.AddComponents(cancel_button);

                    await msg.ModifyAsync(builder);
                }

                CancellationTokenSource cancellationTokenSource = new();

                async Task SelectInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new();

                            if (e.Interaction.Data.CustomId == "reward_selection")
                            {
                                selected = e.Values.First();
                                await RefreshRewardList();
                            }
                            else if (e.Interaction.Data.CustomId == "delete")
                            {
                                _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(_bot._guilds.List[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)));

                                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Count == 0)
                                {
                                    embed.Description = $"`There are no more level reward to display.`";
                                    embed.Color = ColorHelper.Success;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                embed.Description = $"`Select a level reward to modify.`";
                                selected = "";

                                await RefreshRewardList();
                            }
                            else if (e.Interaction.Data.CustomId == "modify")
                            {
                                embed.Description = $"{embed.Description}\n\n`Please type out your new custom message (<256 characters). Type 'cancel' to cancel.`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                                if (result.TimedOut)
                                {
                                    embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                _ = result.Result.DeleteAsync();

                                if (result.Result.Content.Length > 256)
                                {
                                    embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                if (result.Result.Content.ToLower() != "cancel")
                                {
                                    _bot._guilds.List[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message = result.Result.Content;
                                }

                                await RefreshRewardList();
                            }
                            else if (e.Interaction.Data.CustomId == "prev_page")
                            {
                                current_page--;
                                await RefreshRewardList();
                            }
                            else if (e.Interaction.Data.CustomId == "next_page")
                            {
                                current_page++;
                                await RefreshRewardList();
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                _ = msg.DeleteAsync();
                                return;
                            }
                        }

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();

                            ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                        }
                        catch { }
                    }).Add(_bot._watcher, ctx);
                }

                await RefreshRewardList();

                ctx.Client.ComponentInteractionCreated += SelectInteraction;

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                }
                catch { }
            }
            else if (action.ToLower() is "review" or "list")
            {
                string Build = "";
                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Count != 0)
                {
                    foreach (var b in _bot._guilds.List[ctx.Guild.Id].LevelRewards.OrderBy(x => x.Level))
                    {
                        if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
                        {
                            _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(b);
                            continue;
                        }

                        Build += $"**Level**: `{b.Level}`\n" +
                                $"**Role**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                                $"**Message**: `{b.Message}`\n";

                        Build += "\n\n";
                    }
                }
                else
                {
                    Build = $"`No level rewards are set up. Run '{ctx.Prefix}{ctx.Command.Name} add' to add one.`";
                }

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Level Rewards • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"{Build}"
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
                return;
            }
            else
            {
                _ = SendHelp(ctx);
                return;
            }

        }).Add(_bot._watcher, ctx);
    }



    [Command("phishing"), Aliases("phishingsettings", "phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
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
            else if (action.ToLower() is "review" or "list")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Detect Phishing Links   ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                  $"`Redirect Warning        ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect.BoolToEmote()}\n" +
                                  $"`Punishment Type         ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                  $"`Custom Punishment Reason` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                  $"`Custom Timeout Length   ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
                });
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Detect Phishing Links   ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                  $"`Redirect Warning        ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect.BoolToEmote()}\n" +
                                  $"`Punishment Type         ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                  $"`Custom Punishment Reason` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                  $"`Custom Timeout Length   ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
                };

                var ToggleDetectionButton = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection");
                var ToggleWarningButton = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning");
                var ChangePunishmentButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Punishment");
                var ChangeReasonButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Reason");
                var ChangeTimeoutLengthButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Timeout Length");
                var CancelButton = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel");

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { ToggleDetectionButton },
                    { ToggleWarningButton },
                    { ChangePunishmentButton },
                    { ChangeReasonButton },
                    { ChangeTimeoutLengthButton }
                }).AddComponents(CancelButton));

                CancellationTokenSource cancellationTokenSource = new();

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            cancellationTokenSource.Cancel();
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            var interactivity = ctx.Client.GetInteractivity();

                            if (e.Interaction.Data.CustomId == ToggleDetectionButton.CustomId)
                            {
                                _ = msg.DeleteAsync();
                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            }
                            else if (e.Interaction.Data.CustomId == ToggleWarningButton.CustomId)
                            {
                                _ = msg.DeleteAsync();
                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect = !_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect;
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            }
                            else if (e.Interaction.Data.CustomId == ChangePunishmentButton.CustomId)
                            {
                                var dropdown = new DiscordSelectComponent("selection", "Select an action..", new List<DiscordSelectComponentOption>
                        {
                            { new DiscordSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                            { new DiscordSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                        } as IEnumerable<DiscordSelectComponentOption>);

                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                                async Task ChangePunishmentInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                                {
                                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                                    {
                                        switch (e.Values.First())
                                        {
                                            case "Ban":
                                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.BAN;
                                                break;
                                            case "Kick":
                                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.KICK;
                                                break;
                                            case "Timeout":
                                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.TIMEOUT;
                                                break;
                                            case "Delete":
                                                _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.DELETE;
                                                break;
                                        }

                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                        ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;
                                    }
                                };

                                ctx.Client.ComponentInteractionCreated += ChangePunishmentInteraction;

                                try
                                {
                                    await Task.Delay(60000);
                                    embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();

                                    ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;
                                }
                                catch { }
                            }
                            else if (e.Interaction.Data.CustomId == ChangeReasonButton.CustomId)
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
                                    _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = reason.Result.Content;

                                _ = msg3.DeleteAsync();
                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                            }
                            else if (e.Interaction.Data.CustomId == ChangeTimeoutLengthButton.CustomId)
                            {
                                if (_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
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

                                    _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

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
                            }
                            else if (e.Interaction.Data.CustomId == CancelButton.CustomId)
                            {
                                _ = msg.DeleteAsync();
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                try
                {
                    await Task.Delay(60000, cancellationTokenSource.Token);

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;

                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                }
                catch { }
            }
            else
            {
                await SendHelp(ctx);
                return;
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("admin"),
    Description("Allows to review, set up and change settings for the Bump Reminder")]
    public async Task BumpReminder(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
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
            else if (action.ToLower() is "review" or "list")
            {
                if (!_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"{false.BoolToEmote()} `The Bump Reminder is not set up on this server. Please run '{ctx.Prefix}{ctx.Command.Name} setup' in the channel used for bumping.`"
                    });
                    return;
                }

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Bump Reminder Enabled` : {_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                });
                return;
            }
            else if (action.ToLower() is "setup" or "set-up")
            {
                if (_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled)
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

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`Please select a role to ping when the server can be bumped.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                DiscordRole role;

                try
                {
                    role = await new GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "BumpReminder");
                }
                catch (ArgumentException)
                {
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                var bump_reaction_msg = await ctx.Channel.SendMessageAsync($"React to this message with :white_check_mark: to receive notifications as soon as the server can be bumped again.");
                _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                _ = bump_reaction_msg.PinAsync();

                _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId = role.Id;
                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId = ctx.Channel.Id;
                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.MessageId = bump_reaction_msg.Id;
                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddHours(-2);
                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddHours(-2);
                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled = true;

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`The Bump Reminder has been set up.`";
                embed.Color = ColorHelper.Success;
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                await Task.Delay(5000);
                _ = msg.DeleteAsync();

                _bot._bumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                if (!_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"{false.BoolToEmote()} `The Bump Reminder is not set up on this server. Please run '{ctx.Prefix}{ctx.Command.Name} setup' in the channel used for bumping.`"
                    });
                    return;
                }

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Bump Reminder Enabled` : {_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Danger, "1", "Disable Bump Reminder", !_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled) },
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
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId = 0;
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId = 0;
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.MessageId = 0;
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.MinValue;
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.MinValue;
                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled = false;

                                    if (GetScheduleTasks() != null)
                                        if (GetScheduleTasks().Any(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}"))
                                            DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}").Key);

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
                                    try
                                    {
                                        var channel = await new GenericSelectors(_bot).PromptChannelSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                                        _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId = channel.Id;
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
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
                                case "3":
                                {
                                    try
                                    {
                                        var role = await new GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                                        _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId = role.Id;
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
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
                                case "cancel":
                                    _ = msg.DeleteAsync();
                                    return;
                            }

                            _ = msg.DeleteAsync();
                            _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        }
                    }).Add(_bot._watcher, ctx);
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("actionlog"), Aliases("action-log"),
    CommandModule("admin"),
    Description("Allows to review, change settings for the actionlog")]
    public async Task ActionLog(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
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
            else if (action.ToLower() is "review" or "list")
            {
                if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"{false.BoolToEmote()} `The actionlog is disabled.`"
                    });
                    return;
                }

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Actionlog Channel                 ` : <#{_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel}>\n" +
                                  $"`Attempt gathering more details    ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails.BoolToEmote()}\n" +
                                  $"`Join, Leaves & Kicks              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified.BoolToEmote()}\n" +
                                  $"`Nickname, Role, Membership Updates` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified.BoolToEmote()}\n" +
                                  $"`User Profile Updates              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified.BoolToEmote()}\n" +
                                  $"`Message Deletions                 ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted.BoolToEmote()}\n" +
                                  $"`Message Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified.BoolToEmote()}\n" +
                                  $"`Role Updates                      ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified.BoolToEmote()}\n" +
                                  $"`Bans & Unbans                     ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified.BoolToEmote()}\n" +
                                  $"`Server Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified.BoolToEmote()}\n" +
                                  $"`Channel Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified.BoolToEmote()}\n" +
                                  $"`Voice Channel Updates             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated.BoolToEmote()}\n" +
                                  $"`Invite Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified.BoolToEmote()}"
                });
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Actionlog Channel                 ` : <#{_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel}>\n" +
                                  $"`Attempt gathering more details    ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails.BoolToEmote()}\n" +
                                  $"`Join, Leaves & Kicks              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified.BoolToEmote()}\n" +
                                  $"`Nickname, Role, Membership Updates` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified.BoolToEmote()}\n" +
                                  $"`User Profile Updates              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified.BoolToEmote()}\n" +
                                  $"`Message Deletions                 ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted.BoolToEmote()}\n" +
                                  $"`Message Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified.BoolToEmote()}\n" +
                                  $"`Role Updates                      ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified.BoolToEmote()}\n" +
                                  $"`Bans & Unbans                     ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified.BoolToEmote()}\n" +
                                  $"`Server Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified.BoolToEmote()}\n" +
                                  $"`Channel Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified.BoolToEmote()}\n" +
                                  $"`Voice Channel Updates             ` : {_bot._guilds.List[ ctx.Guild.Id ].ActionLogSettings.VoiceStateUpdated.BoolToEmote()}\n" +
                                  $"`Invite Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified.BoolToEmote()}"
                };

                if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0)
                    embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"{false.BoolToEmote()} `The actionlog is disabled.`"
                    };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                List<DiscordComponent> components = new();
                components.Add(new DiscordButtonComponent(ButtonStyle.Primary, "setchannel", $"{(_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0 ? "Set channel" : "Change channel")}"));

                if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel != 0)
                    components.Add(new DiscordButtonComponent(ButtonStyle.Danger, "disable", $"Disable Actionlog"));

                if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel != 0)
                    builder.AddComponents(new DiscordSelectComponent("togglelist", "Select an option to toggle", new List<DiscordSelectComponentOption>
                    {
                        new DiscordSelectComponentOption("Toggle 'Attempt gathering more details'", "attempt_further_detail"),
                        new DiscordSelectComponentOption("Toggle all options below", "toggle_all"),
                        new DiscordSelectComponentOption("Toggle 'Join, Leaves & Kicks'", "log_members_modified"),
                        new DiscordSelectComponentOption("Toggle 'Nickname, Role Updates'", "log_member_modified"),
                        new DiscordSelectComponentOption("Toggle 'User Profile Updates'", "log_memberprofile_modified"),
                        new DiscordSelectComponentOption("Toggle 'Message Deletions'", "log_message_deleted"),
                        new DiscordSelectComponentOption("Toggle 'Message Modifications'", "log_message_updated"),
                        new DiscordSelectComponentOption("Toggle 'Role Updates'", "log_roles_modified"),
                        new DiscordSelectComponentOption("Toggle 'Bans & Unbans'", "log_banlist_modified"),
                        new DiscordSelectComponentOption("Toggle 'Server Modifications'", "log_guild_modified"),
                        new DiscordSelectComponentOption("Toggle 'Channel Modifications'", "log_channels_modified"),
                        new DiscordSelectComponentOption("Toggle 'Voice Channel Updates'", "log_voice_state"),
                        new DiscordSelectComponentOption("Toggle 'Invite Modifications'", "log_invites_modified"),
                    }));

                components.Add(new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel"));

                builder.AddComponents(components);

                var msg = await ctx.Channel.SendMessageAsync(builder);

                CancellationTokenSource cancellationTokenSource = new();

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Interaction.Data.CustomId == "disable")
                            {
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel = 0;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated = false;
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified = false;

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "setchannel")
                            {
                                try
                                {
                                    var channel = await new GenericSelectors(_bot).PromptChannelSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "actionlog", ChannelType.Text);

                                    await channel.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                                {
                                    new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                                    new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.All },
                                });

                                    embed.Description = $"{channel.Mention} `has been created. For security, the everyone role has been denied all permissions. Please make sure to configure the permissions of this channel.`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel = channel.Id;

                                    await Task.Delay(6000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);

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
                            else if (e.Interaction.Data.CustomId == "togglelist")
                            {
                                switch (e.Values.First())
                                {
                                    case "attempt_further_detail":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails;

                                        if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails)
                                        {
                                            embed.Description = $":warning: `This may result in inaccurate details being displayed. Please make sure to double check the audit log on serious concerns.`";
                                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                            await Task.Delay(10000);
                                        }
                                        break;
                                    case "toggle_all":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated;
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified;
                                        break;
                                    case "log_members_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified;
                                        break;
                                    case "log_voice_state":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated;
                                        break;
                                    case "log_memberprofile_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified;
                                        break;
                                    case "log_member_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified;
                                        break;
                                    case "log_message_deleted":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted;
                                        break;
                                    case "log_message_updated":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified;
                                        break;
                                    case "log_roles_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified;
                                        break;
                                    case "log_banlist_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified;
                                        break;
                                    case "log_guild_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified;
                                        break;
                                    case "log_channels_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified;
                                        break;
                                    case "log_invites_modified":
                                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified;
                                        break;
                                }

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                _ = msg.DeleteAsync();
                                return;
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                };

                try
                {
                    await Task.Delay(60000, cancellationTokenSource.Token);
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("autocrosspost"), Aliases("auto-crosspost", "crosspost"),
    CommandModule("admin"),
    Description("Allows to review, change settings for the automatic crossposts")]
    public async Task AutoCrosspost(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows all active autocrosspost channels._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to change the currently used settings._"
                });
            }

            foreach (var b in _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                if (!ctx.Guild.Channels.ContainsKey(b))
                    _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

            if (action.ToLower() == "help")
            {
                await SendHelp(ctx);
                return;
            }
            else if (action.ToLower() is "review" or "list")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Delay before crossposting`: `{TimeSpan.FromSeconds(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting).GetHumanReadable()}`\n\n" +
                    $"{(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count != 0 ? string.Join("\n\n", _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : "`No Auto Crosspost Channels set up.`")}"
                });
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                CancellationTokenSource cancellationTokenSource = new();

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Delay before crossposting`: `{TimeSpan.FromSeconds(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting).GetHumanReadable()}`\n\n" + 
                    $"{(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count != 0 ? string.Join("\n\n", _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : "`No Auto Crosspost Channels set up.`")}"
                };

                var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, "SetDelay", "Set delay", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":clock3:")));
                var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, "AddChannel", "Add channel", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:")));
                var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, "RemoveChannel", "Remove channel", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:")));
                var CancelButton = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel");

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { SetDelayButton, AddButton, RemoveButton, CancelButton }));

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new();

                            if (e.Interaction.Data.CustomId == "AddChannel")
                            {
                                if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                                {
                                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {_bot._status.DevelopmentServerInvite}";
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }

                                DiscordChannel channel;

                                try
                                {
                                    channel = await new GenericSelectors(_bot).PromptChannelSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
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
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }

                                if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                                {
                                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {_bot._status.DevelopmentServerInvite}";
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }

                                _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Add(channel.Id);
                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "RemoveChannel")
                            {
                                if (_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count == 0)
                                {
                                    embed.Description = $"`No Crosspost Channels are set up.`";
                                    embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }

                                ulong ChannelToRemove;

                                try
                                {
                                    var channel = await new GenericSelectors(_bot).PromptCustomSelection(_bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels
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
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "SetDelay")
                            {
                                embed.Description = "Please specify how long to delay the crossposting:\n" +
                                                    "`m` - _Minutes_\n" +
                                                    "`s` - _Seconds_\n\n" +
                                                    "For example, `10s` would result to 10 seconds.";
                                embed.Color = ColorHelper.AwaitingInput;

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
                                                length = TimeSpan.FromSeconds(Convert.ToInt32(reason.Result.Content));
                                                return;
                                        }
                                    }

                                    if (length > TimeSpan.FromMinutes(5) || length < TimeSpan.FromSeconds(1))
                                    {
                                        embed.Description = "`The duration has to be between 1 second and 5 minutes.`";
                                        embed.Color = ColorHelper.Error;
                                        embed.Author.IconUrl = Resources.LogIcons.Error;

                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                        return;
                                    }

                                    _bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting = Convert.ToInt32(length.TotalSeconds);

                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                }
                                catch (Exception)
                                {
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                    return;
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                _ = msg.DeleteAsync();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }

                            try
                            {
                                await Task.Delay(120000, cancellationTokenSource.Token);
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed.Footer.Text += " • Interaction timed out";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                _ = msg.DeleteAsync();
                            }
                            catch { }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("reactionroles"), Aliases("reactionrole","reaction-roles", "reaction-role"),
    CommandModule("admin"),
    Description("Allows to review, change settings for reactionroles")]
    public async Task ReactionRoles(CommandContext ctx, [Description("Action")] string action = "help", DiscordEmoji emoji_parameter = null, DiscordRole role_parameter = null)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows all currently active reaction roles._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to add, delete and modify reaction roles._\n\n\n" +
                                                    $"**Harder, but faster commands**\n" +
                                                    $"_These commands are less user-friendly but if used correctly, can be faster than using `{ctx.Prefix}{ctx.Command.Name} config`._\n\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} add <MessageReply> <Emoji> <@Role>` - _Adds a new reaction role to the mentioned message._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} remove <MessageReply> <Emoji>` - _Removes a reaction role from the mentioned message._\n" +
                                                    $"`{ctx.Prefix}{ctx.Command.Name} removeall <MessageReply>` - _Removes all reaction roles from the mentioned message._\n\n" +
                                                    $"_To fulfill the `<MessageReply>` requirement, simply reply to a message you want to perform the action on._",
                    ImageUrl = "https://media.discordapp.net/attachments/906976602557145110/967751607418761257/unknown.png"
                };
                await ctx.Channel.SendMessageAsync(embed);
            }

            Dictionary<ulong, DiscordMessage> messageCache = new();

            async Task CheckForInvalid()
            {
                if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                    return;

                foreach (var b in _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.ToList())
                {
                    if (!ctx.Guild.Channels.ContainsKey(b.Value.ChannelId))
                    {
                        _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);
                        continue;
                    }

                    if (!ctx.Guild.Roles.ContainsKey(b.Value.RoleId))
                    {
                        _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);
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

                            _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);
                            continue;
                        }
                        catch (DisCatSharp.Exceptions.UnauthorizedException)
                        {
                            messageCache.Add(b.Key, null);

                            _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);
                            continue;
                        }
                    }

                    if (messageCache[ b.Key ] == null)
                    {
                        _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);
                        continue;
                    }

                    var msg = messageCache[ b.Key ];

                    if (!msg.Reactions.Any(x => x.Emoji.Id == b.Value.EmojiId && x.Emoji.GetUniqueDiscordName() == b.Value.EmojiName && x.IsMe))
                    {
                        _ = msg.CreateReactionAsync(b.Value.GetEmoji(ctx.Client));
                        continue;
                    }
                }
            }

            if (action.ToLower() == "help")
            {
                await SendHelp(ctx);
                return;
            }
            else if (action.ToLower() is "review" or "list")
            {
                var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = "`Loading Reaction Roles..`"
                });

                await CheckForInvalid();

                List<string> Desc = new();

                if (_bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count == 0)
                {
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = "`No reaction roles are set up.`"
                    }.Build());
                    return;
                }

                foreach (var b in _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles)
                {
                    var channel = ctx.Guild.GetChannel(b.Value.ChannelId);
                    var role = ctx.Guild.GetRole(b.Value.RoleId);
                    var message = messageCache[ b.Key ];

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
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = x
                }).ToList();

                foreach (var b in embeds)
                    _ = ctx.Channel.SendMessageAsync(b);

                _ = msg.DeleteAsync();
                return;
            }
            else if (action.ToLower() is "config" or "configure" or "settings" or "list" or "modify")
            {
                var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.Loading,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = "`Loading Reaction Roles..`"
                });

                await CheckForInvalid();

                var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, "Add", "Add a new reaction role", (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:")));
                var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, "Remove", "Remove a reaction role", (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:")));
                var CancelButton = new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel");

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`{_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count} reaction roles are set up.`"
                };

                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { AddButton, RemoveButton, CancelButton }));

                CancellationTokenSource cancellationTokenSource = new();
                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new();

                            if (e.Interaction.Data.CustomId == "Add")
                            {
                                var action_embed = new DiscordEmbedBuilder
                                {
                                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                                    Color = ColorHelper.AwaitingInput,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                                    Timestamp = DateTime.UtcNow,
                                    Description = "`Please copy and send the message link of the message you want the reaction role to be added to.`",
                                    ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                                };

                                if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count > 100)
                                {
                                    action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                                    action_embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));

                                var link = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(2));

                                if (link.TimedOut)
                                {
                                    action_embed.Footer.Text += " • Interaction timed out";
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                try { _ = link.Result.DeleteAsync(); } catch { }

                                if (!Regex.IsMatch(link.Result.Content, Resources.Regex.DiscordChannelUrl))
                                {
                                    action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                                               $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                                    action_embed.Color = ColorHelper.Error;
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                if (!link.Result.Content.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                                {
                                    action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                                               $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                               $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                                    action_embed.Color = ColorHelper.Error;
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                if (GuildId != ctx.Guild.Id)
                                {
                                    action_embed.Description = $"`The link you provided leads to another server.`";
                                    action_embed.Color = ColorHelper.Error;
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                                {
                                    action_embed.Description = $"`The link you provided leads to a channel that doesn't exist.`";
                                    action_embed.Color = ColorHelper.Error;
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                var channel = ctx.Guild.GetChannel(ChannelId);

                                if (!channel.TryGetMessage(MessageId, out DiscordMessage reactionMessage))
                                {
                                    action_embed.Description = $"`The link you provided leads a message that doesn't exist or the bot has no access to.`";
                                    action_embed.Color = ColorHelper.Error;
                                    action_embed.ImageUrl = "";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                action_embed.Description = "`Please react with the emoji you want to use for the reaction role.`";
                                action_embed.ImageUrl = "";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));

                                var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == msg.Id, TimeSpan.FromMinutes(2));

                                if (emoji_wait.TimedOut)
                                {
                                    action_embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                try { _ = emoji_wait.Result.Message.DeleteAllReactionsAsync(); } catch { }

                                var emoji = emoji_wait.Result.Emoji;

                                if (emoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji.Id))
                                {
                                    action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                                    action_embed.Color = ColorHelper.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }

                                action_embed.Description = "`Please select the role you want to use.`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                msg = await ctx.Channel.GetMessageAsync(msg.Id);

                                try
                                {
                                    var role = await new GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                                    if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count > 100)
                                    {
                                        action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                                        action_embed.Color = ColorHelper.Error;
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                        return;
                                    }


                                    if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => (x.Key == MessageId && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                                    {
                                        action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                                        action_embed.Color = ColorHelper.Error;
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                        return;
                                    }

                                    if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => x.Value.RoleId == role.Id))
                                    {
                                        action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                                        action_embed.Color = ColorHelper.Error;
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                        await Task.Delay(5000);
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                        return;
                                    }

                                    _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Add(new KeyValuePair<ulong, ReactionRoles>(reactionMessage.Id, new Objects.ReactionRoles
                                    {
                                        ChannelId = ChannelId,
                                        RoleId = role.Id,
                                        EmojiId = emoji.Id,
                                        EmojiName = emoji.GetUniqueDiscordName()
                                    }));

                                    await reactionMessage.CreateReactionAsync(emoji);

                                    action_embed.Color = ColorHelper.Info;
                                    action_embed.Description = $"`Added role` {role.Mention} `to message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {emoji} `.`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
                                    return;
                                }
                                catch (ArgumentException)
                                {
                                    action_embed.Footer.Text += " • Interaction timed out";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "Remove")
                            {
                                try
                                {
                                    msg = await ctx.Channel.GetMessageAsync(msg.Id);
                                    var roleuuid = await new GenericSelectors(_bot).PromptCustomSelection(_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles
                                                                    .Select(x => new DiscordSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList(),
                                                                    ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                                    var obj = _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.First(x => x.Value.UUID == roleuuid);

                                    var role = ctx.Guild.GetRole(obj.Value.RoleId);
                                    var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
                                    var reactionMessage = await channel.GetMessageAsync(obj.Key);
                                    _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                                    _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(obj);

                                    embed.Color = ColorHelper.Info;
                                    embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ ctx.Command.Name ].ExecuteAsync(ctx);
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
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                _ = msg.DeleteAsync();
                                return;
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
                return;
            }
            else if (action.ToLower() == "add")
            {
                if (ctx.Message.ReferencedMessage is null || role_parameter is null || emoji_parameter is null)
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.AwaitingInput,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = "`Adding reaction role..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Count > 100)
                {
                    action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
                {
                    action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => (x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
                {
                    action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                if (_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
                {
                    action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                await ctx.Message.ReferencedMessage.CreateReactionAsync(emoji_parameter);

                _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Add(new KeyValuePair<ulong, ReactionRoles>(ctx.Message.ReferencedMessage.Id, new Objects.ReactionRoles
                {
                    ChannelId = ctx.Message.ReferencedMessage.Channel.Id,
                    RoleId = role_parameter.Id,
                    EmojiId = emoji_parameter.Id,
                    EmojiName = emoji_parameter.GetUniqueDiscordName()
                }));

                action_embed.Color = ColorHelper.Info;
                action_embed.Description = $"`Added role` {role_parameter.Mention} `to message sent by` {ctx.Message.ReferencedMessage.Author.Mention} `in` {ctx.Message.ReferencedMessage.Channel.Mention} `with emoji` {emoji_parameter} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
                return;
            }
            else if (action.ToLower() == "remove")
            {
                if (ctx.Message.ReferencedMessage is null || emoji_parameter is null)
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.AwaitingInput,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = "`Removing reaction role..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (!_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName()))
                {
                    action_embed.Description = $"`The specified message doesn't contain specified reaction.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                var obj = _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.First(x => x.Key == ctx.Message.ReferencedMessage.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName());

                var role = ctx.Guild.GetRole(obj.Value.RoleId);
                var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
                var reactionMessage = await channel.GetMessageAsync(obj.Key);
                _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(obj);

                action_embed.Color = ColorHelper.Info;
                action_embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
                return;
            }
            else if (action.ToLower() == "removeall")
            {
                if (ctx.Message.ReferencedMessage is null)
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = ColorHelper.AwaitingInput,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = "`Removing all reaction roles..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(action_embed);
                action_embed.Author.IconUrl = ctx.Guild.IconUrl;

                if (!_bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Any(x => x.Key == ctx.Message.ReferencedMessage.Id))
                {
                    action_embed.Description = $"`The specified message doesn't contain any reaction roles.`";
                    action_embed.Color = ColorHelper.Error;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }

                foreach (var b in _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Where(x => x.Key == ctx.Message.ReferencedMessage.Id).ToList())
                    _bot._guilds.List[ ctx.Guild.Id ].ReactionRoles.Remove(b);

                _ = ctx.Message.ReferencedMessage.DeleteAllReactionsAsync();

                action_embed.Color = ColorHelper.Info;
                action_embed.Description = $"`Removed all reaction roles from message sent by` {ctx.Message.ReferencedMessage.Author.Mention} `in` {ctx.Message.ReferencedMessage.Channel.Mention} `.`";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(action_embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
                return;
            }
            else
            {
                await SendHelp(ctx);
                return;
            }
        }).Add(_bot._watcher, ctx);
    }
}
