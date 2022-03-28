namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("join-settings"), Aliases("joinsettings"),
    CommandModule("admin"),
    Description("Allows to review and change settings in the event somebody joins")]
    public async Task JoinSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.IsAdmin(_bot._status))
            {
                _ = ctx.SendAdminError();
                return;
            }

            static async Task SendHelp(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
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
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Autoban Globally Banned Users` : {_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote()}\n" +
                                  $"`Joinlog Channel              ` : {(_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote())}\n" +
                                  $"`Role On Join                 ` : {(_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote())}"
                });
                return;
            }
            else if (action.ToLower() == "config")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Autoban Globally Banned Users` : {_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote()}\n" +
                                  $"`Joinlog Channel              ` : {(_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote())}\n" +
                                  $"`Role On Join                 ` : {(_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote())}"
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var msg = await ctx.Channel.SendMessageAsync(builder.AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), "1", "Toggle Global Bans") },
                    { new DiscordButtonComponent(ButtonStyle.Primary, "2", "Change Joinlog Channel") },
                    { new DiscordButtonComponent(ButtonStyle.Primary, "3", "Change Role assigned on join") },
                    { new DiscordButtonComponent(ButtonStyle.Secondary, "cancel", "Cancel") }
                } as IEnumerable<DiscordComponent>));

                CancellationTokenSource cancellationTokenSource = new();

                int current_page = 0;

                List<DiscordSelectComponentOption> channels = new();
                List<DiscordSelectComponentOption> roles = new();

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            async Task RefreshChannelList()
                            {
                                var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page_channel", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                                var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page_channel", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                                var dropdown = new DiscordSelectComponent("channel_selection", "Select a channel..", channels.Skip(current_page * 20).Take(20) as IEnumerable<DiscordSelectComponentOption>);
                                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                                if (channels.Skip(current_page * 20).Count() > 20)
                                    builder.AddComponents(next_page_button);

                                if (current_page != 0)
                                    builder.AddComponents(previous_page_button);

                                await msg.ModifyAsync(builder);
                            }

                            async Task RefreshRoleList()
                            {
                                var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page_role", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                                var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page_role", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                                var dropdown = new DiscordSelectComponent("role_selection", "Select a role..", roles.Skip(current_page * 20).Take(20) as IEnumerable<DiscordSelectComponentOption>);
                                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                                if (roles.Skip(current_page * 20).Count() > 20)
                                    builder.AddComponents(next_page_button);

                                if (current_page != 0)
                                    builder.AddComponents(previous_page_button);

                                await msg.ModifyAsync(builder);
                            }

                            if (e.Interaction.Data.CustomId == "1")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans = !_bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans;

                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                cancellationTokenSource.Cancel();
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "2")
                            {
                                channels.Add(new DiscordSelectComponentOption("Disable Joinlog", "disable_channel"));
                                channels.Add(new DiscordSelectComponentOption("Create one for me..", "create_channel"));

                                foreach (var category in await ctx.Guild.GetOrderedChannelsAsync())
                                {
                                    foreach (var channel in category.Value)
                                        channels.Add(new DiscordSelectComponentOption(
                                            $"#{channel.Name} ({channel.Id})",
                                            channel.Id.ToString(),
                                            $"{(category.Key != 0 ? $"{channel.Parent.Name} " : "")}"));
                                }

                                await RefreshChannelList();
                            }
                            else if (e.Interaction.Data.CustomId == "3")
                            {
                                roles.Add(new DiscordSelectComponentOption("Disable Role on join", "disable_role"));
                                roles.Add(new DiscordSelectComponentOption("Create one for me..", "create_role"));

                                var HighestRoleOnBot = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;
                                var HighestRoleOnUser = (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                                foreach (var role in (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).Roles.OrderByDescending(x => x.Value.Position))
                                {
                                    if (HighestRoleOnBot > role.Value.Position && HighestRoleOnUser > role.Value.Position && !role.Value.IsManaged && role.Value.Id != ctx.Guild.EveryoneRole.Id)
                                        roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(ctx.Client))));
                                }

                                await RefreshRoleList();
                            }
                            else if (e.Interaction.Data.CustomId == "channel_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                if (e.Values.First() == "disable_channel")
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId = 0;
                                else if (e.Values.First() == "create_channel")
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId = (await ctx.Guild.CreateChannelAsync("joinlog", ChannelType.Text, overwrites: new List<DiscordOverwriteBuilder>
                                    {
                                        new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole)
                                        {
                                            Allowed = Permissions.ReadMessageHistory | Permissions.AccessChannels, Denied = Permissions.SendMessages
                                        }
                                    } as IEnumerable<DiscordOverwriteBuilder>)).Id;
                                else
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.JoinlogChannelId = Convert.ToUInt64(e.Values.First());

                                cancellationTokenSource.Cancel();
                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "role_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                if (e.Values.First() == "disable_role")
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = 0;
                                else if (e.Values.First() == "create_role")
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = (await ctx.Guild.CreateRoleAsync("AutoAssignedRole")).Id;
                                else
                                    _bot._guilds.Servers[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = Convert.ToUInt64(e.Values.First());

                                cancellationTokenSource.Cancel();
                                _ = msg.DeleteAsync();
                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "prev_page_role")
                            {
                                current_page--;
                                await RefreshRoleList();
                            }
                            else if (e.Interaction.Data.CustomId == "next_page_role")
                            {
                                current_page++;
                                await RefreshRoleList();
                            }
                            else if (e.Interaction.Data.CustomId == "prev_page_channel")
                            {
                                current_page--;
                                await RefreshChannelList();
                            }
                            else if (e.Interaction.Data.CustomId == "next_page_channel")
                            {
                                current_page++;
                                await RefreshChannelList();
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
            }
            else
            {
                await SendHelp(ctx);
                return;
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("experience-settings"), Aliases("experiencesettings"),
    CommandModule("admin"),
    Description("Allows to review and change settings related to experience")]
    public async Task ExperienceSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
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
            else if (action.ToLower() == "review")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience Enabled          ` : {_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote()}\n" +
                                  $"`Experience Boost for Bumpers` : {_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote()}"
                });
                return;
            }
            else if (action.ToLower() == "config")
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience Enabled          ` : {_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote()}\n" +
                                  $"`Experience Boost for Bumpers` : {_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote()}"
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var msg = await ctx.Channel.SendMessageAsync(builder.AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), "1", "Toggle Experience System") },
                    { new DiscordButtonComponent((_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), "2", "Toggle Experience Boost for Bumpers") },
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
                                _bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience = !_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience;
                            }
                            else if (e.Interaction.Data.CustomId == "2")
                            {
                                _bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder = !_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder;
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



    [Command("levelrewards"), Aliases("level-rewards"),
    CommandModule("admin"),
    Description("Allows to review, add and remove levelreward roles")]
    public async Task LevelRewards(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
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
                                 $"`{ctx.Prefix}{ctx.Command.Name} list` - _Displays a list of all level rewards._\n" +
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
                int current_page = 0;
                List<DiscordSelectComponentOption> roles = new();

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Level Rewards • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Select a role to assign.`"
                };

                var msg = await ctx.Channel.SendMessageAsync(embed);

                async Task RefreshRoleList()
                {
                    var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page_role", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                    var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page_role", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                    var dropdown = new DiscordSelectComponent("role_selection", "Select a role..", roles.Skip(current_page * 20).Take(20) as IEnumerable<DiscordSelectComponentOption>);
                    var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                    if (roles.Skip(current_page * 20).Count() > 20)
                        builder.AddComponents(next_page_button);

                    if (current_page != 0)
                        builder.AddComponents(previous_page_button);

                    await msg.ModifyAsync(builder);
                }

                var HighestRoleOnBot = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;
                var HighestRoleOnUser = (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                foreach (var role in (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).Roles.OrderByDescending(x => x.Value.Position))
                {
                    if (HighestRoleOnBot > role.Value.Position && HighestRoleOnUser > role.Value.Position && !role.Value.IsManaged && role.Value.Id != ctx.Guild.EveryoneRole.Id)
                        roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(ctx.Client))));
                }

                CancellationTokenSource cancellationTokenSource = new();

                async Task SelectRoleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Interaction.Data.CustomId == "role_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= SelectRoleInteraction;

                                var role = Convert.ToUInt64(e.Values.First());

                                cancellationTokenSource.Cancel();
                                cancellationTokenSource = new();

                                if (_bot._guilds.Servers[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == role))
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

                                        _bot._guilds.Servers[ctx.Guild.Id].LevelRewards.Add(new Objects.LevelRewards
                                        {
                                            Level = level,
                                            RoleId = role,
                                            Message = (string.IsNullOrEmpty(Message) ? "You received ##Role##!" : Message)
                                        });

                                        embed.Description = $"`The role` <@&{role}> `({role}) will be assigned at Level {level}.`";
                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    }
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

                                return;
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += SelectRoleInteraction;
                await RefreshRoleList();

                try
                {
                    await Task.Delay(120000, cancellationTokenSource.Token);
                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    ctx.Client.ComponentInteractionCreated -= SelectRoleInteraction;
                }
                catch { }
            }
            else if (action.ToLower() == "list")
            {
                string Build = "";
                if (_bot._guilds.Servers[ctx.Guild.Id].LevelRewards.Count != 0)
                {
                    foreach (var b in _bot._guilds.Servers[ctx.Guild.Id].LevelRewards.OrderBy(x => x.Level))
                    {
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



    [Command("phishing-settings"), Aliases("phishingsettings", "phishing"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
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
            else if (action.ToLower() == "review")
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Detect Phishing Links   ` : {_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                    $"`Punishment Type         ` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                    $"`Custom Punishment Reason` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                    $"`Custom Timeout Length   ` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
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
                    Description = $"`Detect Phishing Links   ` : {_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote()}\n" +
                                    $"`Punishment Type         ` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                                    $"`Custom Punishment Reason` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                                    $"`Custom Timeout Length   ` : `{_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { new DiscordButtonComponent((_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), "1", "Toggle Detection") },
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
                        _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
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
                                        _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.BAN;
                                        break;
                                    case "Kick":
                                        _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.KICK;
                                        break;
                                    case "Timeout":
                                        _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.TIMEOUT;
                                        break;
                                    case "Delete":
                                        _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.DELETE;
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
                            _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = reason.Result.Content;

                        _ = msg3.DeleteAsync();
                        _ = msg.DeleteAsync();
                        _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                        break;
                    }
                    case "4":
                    {
                        if (_bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
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

                            _bot._guilds.Servers[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

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
        }).Add(_bot._watcher, ctx);
    }



    [Command("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("admin"),
    Description("Allows to review, set up and change settings for the Bump Reminder")]
    public async Task BumpReminder(CommandContext ctx, [Description("Action")] string action = "help")
    {
        Task.Run(async () =>
        {
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
            else if (action.ToLower() == "review")
            {
                if (!_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
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
                    Description = $"`Bump Reminder Enabled` : {_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                });
                return;
            }
            else if (action.ToLower() == "setup")
            {
                if (_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
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

                var HighestRoleOnBot = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;
                var HighestRoleOnUser = (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                foreach (var role in (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).Roles.OrderByDescending(x => x.Value.Position))
                {
                    if (HighestRoleOnBot > role.Value.Position && HighestRoleOnUser > role.Value.Position && !role.Value.IsManaged && role.Value.Id != ctx.Guild.EveryoneRole.Id)
                        roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(ctx.Client))));
                }

                int current_page = 0;

                async Task RefreshRoleList()
                {
                    var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                    var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                    var dropdown = new DiscordSelectComponent("selection", "Select a role..", roles.Skip(current_page * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
                    var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                    if (roles.Skip(current_page * 25).Count() > 25)
                        builder.AddComponents(next_page_button);

                    if (current_page != 0)
                        builder.AddComponents(previous_page_button);

                    await msg.ModifyAsync(builder);
                }

                async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id != msg.Id || e.User.Id != ctx.User.Id)
                            return;

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == "selection")
                        {
                            ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;


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

                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = id;
                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = ctx.Channel.Id;
                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.MessageId = bump_reaction_msg.Id;
                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddHours(-2);
                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddHours(-2);
                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                            _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled = true;

                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = "`The Bump Reminder has been set up.`";
                            embed.Color = ColorHelper.Success;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();

                            _bot._bumpReminder.SendPersistentMessage(ctx.Client, e.Channel, null);
                        }
                        else if (e.Interaction.Data.CustomId == "prev_page")
                        {
                            current_page--;
                            await RefreshRoleList();
                        }
                        else if (e.Interaction.Data.CustomId == "next_page")
                        {
                            current_page++;
                            await RefreshRoleList();
                        }
                    }).Add(_bot._watcher, ctx);
                }

                ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`Please select a role to ping when the server can be bumped.`";
                await RefreshRoleList();

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
                if (!_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled)
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
                    Description = $"`Bump Reminder Enabled` : {_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote()}\n" +
                                  $"`Bump Reminder Channel` : <#{_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                                  $"`Bump Reminder Role   ` : <@&{_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId})`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Danger, "1", "Disable Bump Reminder", !_bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled) },
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
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = 0;
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = 0;
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.MessageId = 0;
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.MinValue;
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.MinValue;
                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.Enabled = false;

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
                                    List<DiscordSelectComponentOption> channels = new();

                                    foreach (var category in await ctx.Guild.GetOrderedChannelsAsync())
                                    {
                                        foreach (var channel in category.Value)
                                            channels.Add(new DiscordSelectComponentOption(
                                                $"#{channel.Name} ({channel.Id})",
                                                channel.Id.ToString(),
                                                $"{(category.Key != 0 ? $"{channel.Parent.Name} " : "")}"));
                                    }

                                    int current_page = 0;

                                    async Task RefreshChannelList()
                                    {
                                        var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                                        var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                                        var dropdown = new DiscordSelectComponent("selection", "Select a channel..", channels.Skip(current_page * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
                                        var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                                        if (channels.Skip(current_page * 25).Count() > 25)
                                            builder.AddComponents(next_page_button);

                                        if (current_page != 0)
                                            builder.AddComponents(previous_page_button);

                                        await msg.ModifyAsync(builder);
                                    }

                                    async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                                    {
                                        Task.Run(async () =>
                                        {
                                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                                            {
                                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                                if (e.Interaction.Data.CustomId == "selection")
                                                {
                                                    ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.ChannelId = Convert.ToUInt64(e.Values.First());
                                                    _ = msg.DeleteAsync();
                                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                                }
                                                else if (e.Interaction.Data.CustomId == "prev_page")
                                                {
                                                    current_page--;
                                                    await RefreshChannelList();
                                                }
                                                else if (e.Interaction.Data.CustomId == "next_page")
                                                {
                                                    current_page++;
                                                    await RefreshChannelList();
                                                }
                                            }
                                        }).Add(_bot._watcher, ctx);
                                    }

                                    ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                    await RefreshChannelList();

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

                                    var HighestRoleOnBot = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;
                                    var HighestRoleOnUser = (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                                    foreach (var role in (await ctx.Client.GetGuildAsync(ctx.Guild.Id)).Roles.OrderByDescending(x => x.Value.Position))
                                    {
                                        if (HighestRoleOnBot > role.Value.Position && HighestRoleOnUser > role.Value.Position && !role.Value.IsManaged && role.Value.Id != ctx.Guild.EveryoneRole.Id)
                                            roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(ctx.Client))));
                                    }

                                    int current_page = 0;

                                    async Task RefreshRoleList()
                                    {
                                        var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
                                        var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                                        var dropdown = new DiscordSelectComponent("selection", "Select a role..", roles.Skip(current_page * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
                                        var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown);

                                        if (roles.Skip(current_page * 25).Count() > 25)
                                            builder.AddComponents(next_page_button);

                                        if (current_page != 0)
                                            builder.AddComponents(previous_page_button);

                                        await msg.ModifyAsync(builder);
                                    }

                                    async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                                    {
                                        Task.Run(async () =>
                                        {
                                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                                            {
                                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                                if (e.Interaction.Data.CustomId == "selection")
                                                {
                                                    ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                                                    _bot._guilds.Servers[ctx.Guild.Id].BumpReminderSettings.RoleId = Convert.ToUInt64(e.Values.First());
                                                    _ = msg.DeleteAsync();
                                                    _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);
                                                }
                                                else if (e.Interaction.Data.CustomId == "prev_page")
                                                {
                                                    current_page--;
                                                    await RefreshRoleList();
                                                }
                                                else if (e.Interaction.Data.CustomId == "next_page")
                                                {
                                                    current_page++;
                                                    await RefreshRoleList();
                                                }
                                            }
                                        }).Add(_bot._watcher, ctx);
                                    }

                                    ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                    await RefreshRoleList();

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
}
