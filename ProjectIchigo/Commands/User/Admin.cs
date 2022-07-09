using ProjectIchigo.Exceptions;

namespace ProjectIchigo.Commands.User;
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
                throw new CancelCommandException("User is missing apprioriate permissions", ctx);
            }
        }

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            return  $"`Autoban Globally Banned Users` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote(ctx.Client)}\n" +
                    $"`Joinlog Channel              ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote(ctx.Client))}\n" +
                    $"`Role On Join                 ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote(ctx.Client))}\n" +
                    $"`Re-Apply Roles on Rejoin     ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles.BoolToEmote(ctx.Client)}\n" +
                    $"`Re-Apply Nickname on Rejoin  ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname.BoolToEmote(ctx.Client)}\n\n" +
                    $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                    $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied.";
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Join Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                });
            }).Add(_bot._watcher, ctx);
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
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var ToggleGlobalban = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Global Bans", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🌐")));
                var ChangeJoinlogChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Joinlog Channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👋")));
                var ChangeRoleOnJoin = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Role assigned on join", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                var ToggleReApplyRoles = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Role Re-Apply", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
                var ToggleReApplyName = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Nickname Re-Apply", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

                var msg = await ctx.Channel.SendMessageAsync(builder
                    .AddComponents(new List<DiscordComponent>
                    {
                        ToggleGlobalban,
                        ToggleReApplyRoles,
                        ToggleReApplyName,
                    })
                    .AddComponents(new List<DiscordComponent>
                    {
                        ChangeJoinlogChannel,
                        ChangeRoleOnJoin,
                    })
                    .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == ToggleGlobalban.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans;

                    _ = ctx.Command.ExecuteAsync(ctx);
                    _ = msg.DeleteAsync();
                }
                else if (e.Result.Interaction.Data.CustomId == ToggleReApplyRoles.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles;

                    _ = ctx.Command.ExecuteAsync(ctx);
                    _ = msg.DeleteAsync();
                }
                else if (e.Result.Interaction.Data.CustomId == ToggleReApplyName.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname = !_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname;

                    _ = ctx.Command.ExecuteAsync(ctx);
                    _ = msg.DeleteAsync();
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeJoinlogChannel.CustomId)
                {
                    try
                    {
                        var channel = await GenericSelectors.PromptChannelSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "joinlog", ChannelType.Text, true, "Disable Joinlog");

                        if (channel is null)
                            _bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId = 0;
                        else
                            _bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId = channel.Id;

                        _ = ctx.Command.ExecuteAsync(ctx);
                        _ = msg.DeleteAsync();
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeRoleOnJoin.CustomId)
                {
                    try
                    {
                        var role = await GenericSelectors.PromptRoleSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "AutoAssignedRole", true, "Disable Role on join");

                        if (role is null)
                            _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = 0;
                        else
                            _bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = role.Id;

                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                }
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("experience"), Aliases("experiencesettings", "experience-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings related to experience")]
    public class ExperienceSettings : BaseCommandModule
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
            return  $"`Experience Enabled          ` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote(ctx.Client)}\n" +
                    $"`Experience Boost for Bumpers` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote(ctx.Client)}";
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                });
            }).Add(_bot._watcher, ctx);
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
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience Enabled          ` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote(ctx.Client)}\n" +
                  $"`Experience Boost for Bumpers` : {_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote(ctx.Client)}"
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var ToggleExperienceSystem = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience System", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
                var ToggleBumperBoost = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience Boost for Bumpers", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏫")));

                var msg = await ctx.Channel.SendMessageAsync(builder
                    .AddComponents(new List<DiscordComponent>
                    {
                        ToggleExperienceSystem,
                        ToggleBumperBoost,
                    })
                    .AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == ToggleExperienceSystem.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience = !_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == ToggleBumperBoost.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder = !_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                }
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("levelrewards"), Aliases("level-rewards", "rewards"),
    CommandModule("admin"),
    Description("Allows to review, add, remove and modify Level Rewards")]
    public class LevelRewards : BaseCommandModule
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
            string str = "";
            if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Count != 0)
            {
                foreach (var b in _bot._guilds.List[ctx.Guild.Id].LevelRewards.OrderBy(x => x.Level))
                {
                    if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
                    {
                        _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(b);
                        continue;
                    }

                    str += $"**Level**: `{b.Level}`\n" +
                            $"**Role**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                            $"**Message**: `{b.Message}`\n";

                    str += "\n\n";
                }
            }
            else
            {
                str = $"`No Level Rewards are set up. Run '{ctx.Prefix}{ctx.Command.Parent.Name} config' to add one.`";
            }

            return str;
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
        Description("Shows a list of all currently defined Level Rewards")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Level Rewards • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows adding, removing and modifying currently defined Level Rewards")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                int CurrentPage = 0;

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Level Rewards • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Loading Level Rewards..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(embed);
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Color = EmbedColors.AwaitingInput;

                string selected = "";

                async Task RefreshMessage()
                {
                    List<DiscordSelectComponentOption> DefinedRewards = new();

                    embed.Description = "";

                    foreach (var reward in _bot._guilds.List[ctx.Guild.Id].LevelRewards.ToList().OrderBy(x => x.Level))
                    {
                        if (!ctx.Guild.Roles.ContainsKey(reward.RoleId))
                        {
                            _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(reward);
                            continue;
                        }

                        var role = ctx.Guild.GetRole(reward.RoleId);

                        DefinedRewards.Add(new DiscordSelectComponentOption($"Level {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

                        if (selected == role.Id.ToString())
                        {
                            embed.Description = $"**Level**: `{reward.Level}`\n" +
                                                $"**Role**: <@&{reward.RoleId}> (`{reward.RoleId}`)\n" +
                                                $"**Message**: `{reward.Message}`\n";
                        }
                    }

                    if (DefinedRewards.Count > 0)
                    {
                        if (embed.Description == "")
                            embed.Description = "`Please select a Level Reward to modify or delete.`";
                    }
                    else
                    {
                        embed.Description = "`No Level Rewards are defined.`";
                    }

                    var PreviousPage = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                    var NextPage = new DiscordButtonComponent(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                    var Add = new DiscordButtonComponent(ButtonStyle.Success, "Add", "Add new Level Reward", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                    var Modify = new DiscordButtonComponent(ButtonStyle.Primary, "Modify", "Modify Message", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));
                    var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "Delete", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                    var Dropdown = new DiscordSelectComponent("Select a Level Reward..", DefinedRewards.Skip(CurrentPage * 20).Take(20).ToList(), "RewardSelection");
                    var builder = new DiscordMessageBuilder().WithEmbed(embed);

                    if (DefinedRewards.Count > 0)
                        builder.AddComponents(Dropdown);

                    List<DiscordComponent> Row1 = new();
                    List<DiscordComponent> Row2 = new();

                    if (DefinedRewards.Skip(CurrentPage * 20).Count() > 20)
                        Row1.Add(NextPage);

                    if (CurrentPage != 0)
                        Row1.Add(PreviousPage);

                    Row2.Add(Add);

                    if (selected != "")
                    {
                        Row2.Add(Modify);
                        Row2.Add(Delete);
                    }

                    if (Row1.Count > 0)
                        builder.AddComponents(Row1);

                    builder.AddComponents(Row2);

                    builder.AddComponents(Resources.CancelButton);

                    await msg.ModifyAsync(builder);
                }

                CancellationTokenSource cancellationTokenSource = new();

                async Task SelectInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new();

                            _ = Task.Delay(120000, cancellationTokenSource.Token).ContinueWith(x =>
                            {
                                if (x.IsCompletedSuccessfully)
                                {
                                    ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                                    msg.ModifyToTimedOut(true);
                                }
                            });

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Interaction.Data.CustomId == "RewardSelection")
                            {
                                selected = e.Values.First();
                                await RefreshMessage();
                            }
                            else if (e.Interaction.Data.CustomId == "Add")
                            {
                                ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                                embed.Description = $"`Select a role to assign.`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                DiscordRole role;

                                try
                                {
                                    role = await GenericSelectors.PromptRoleSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                                }
                                catch (ArgumentException)
                                {
                                    msg.ModifyToTimedOut(true);
                                    return;
                                }

                                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == role.Id))
                                {
                                    embed.Description = "`The role you're trying to add has already been assigned to a level.`";
                                    embed.Color = EmbedColors.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }

                                embed.Description = $"`Selected` <@&{role.Id}> `({role.Id}). At what Level should this role be assigned?`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var LevelResult = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                if (LevelResult.TimedOut)
                                {
                                    msg.ModifyToTimedOut(true);
                                    return;
                                }

                                int level;

                                try
                                {
                                    level = Convert.ToInt32(LevelResult.Result.Content);
                                }
                                catch (Exception)
                                {
                                    embed.Description = "`You must specify a valid level.`";
                                    embed.Color = EmbedColors.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }

                                _ = LevelResult.Result.DeleteAsync();

                                string Message = "";

                                embed.Description = $"`Selected` <@&{role.Id}> `({role.Id}). It will be assigned at Level {level}. Please type out a custom message or send 'cancel', 'continue' or '.' to use the default message. (<256 characters)`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var CustomMessageResult = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                                if (CustomMessageResult.TimedOut)
                                {
                                    msg.ModifyToTimedOut(true);
                                    return;
                                }

                                _ = CustomMessageResult.Result.DeleteAsync();

                                if (CustomMessageResult.Result.Content.Length > 256)
                                {
                                    embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                    embed.Color = EmbedColors.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }

                                if (CustomMessageResult.Result.Content is not "cancel" and not  "continue" and not ".")
                                    Message = CustomMessageResult.Result.Content;

                                _bot._guilds.List[ctx.Guild.Id].LevelRewards.Add(new Entities.LevelReward
                                {
                                    Level = level,
                                    RoleId = role.Id,
                                    Message = (string.IsNullOrEmpty(Message) ? "You received ##Role##!" : Message)
                                });

                                embed.Description = $"`The role` <@&{role.Id}> `({role.Id}) will be assigned at Level {level}.`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                await Task.Delay(5000);
                                await RefreshMessage();
                                ctx.Client.ComponentInteractionCreated += SelectInteraction;
                            }
                            else if (e.Interaction.Data.CustomId == "Modify")
                            {
                                embed.Description = $"{embed.Description}\n\n`Please type out your new custom message (<256 characters). Type 'cancel' to cancel.`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                                if (result.TimedOut)
                                {
                                    msg.ModifyToTimedOut(true);
                                    return;
                                }

                                _ = result.Result.DeleteAsync();

                                if (result.Result.Content.Length > 256)
                                {
                                    embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                    embed.Color = EmbedColors.Error;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                if (result.Result.Content.ToLower() != "cancel")
                                {
                                    _bot._guilds.List[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message = result.Result.Content;
                                }

                                await RefreshMessage();
                            }
                            else if (e.Interaction.Data.CustomId == "Delete")
                            {
                                _bot._guilds.List[ctx.Guild.Id].LevelRewards.Remove(_bot._guilds.List[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)));

                                if (_bot._guilds.List[ctx.Guild.Id].LevelRewards.Count == 0)
                                {
                                    embed.Description = $"`There are no more Level Rewards to display.`";
                                    embed.Color = EmbedColors.Success;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
                                    _ = msg.DeleteAsync();
                                    return;
                                }

                                embed.Description = $"`Select a Level Reward to modify.`";
                                selected = "";

                                await RefreshMessage();
                            }
                            else if (e.Interaction.Data.CustomId == "PreviousPage")
                            {
                                CurrentPage--;
                                await RefreshMessage();
                            }
                            else if (e.Interaction.Data.CustomId == "NextPage")
                            {
                                CurrentPage++;
                                await RefreshMessage();
                            }
                            else if (e.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                            {
                                _ = msg.DeleteAsync();
                                return;
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                }

                await RefreshMessage();

                _ = Task.Delay(120000, cancellationTokenSource.Token).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                        msg.ModifyToTimedOut(true);
                    }
                });

                ctx.Client.ComponentInteractionCreated += SelectInteraction;
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("phishing"), Aliases("phishingsettings", "phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for the phishing detection")]
    public class PhishingSettings : BaseCommandModule
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
            return $"`Detect Phishing Links   ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote(ctx.Client)}\n" +
                    $"`Redirect Warning        ` : {_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect.BoolToEmote(ctx.Client)}\n" +
                    $"`Punishment Type         ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
                    $"`Custom Punishment Reason` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
                    $"`Custom Timeout Length   ` : `{_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`";
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
        Description("Shows a list of all currently defined Phishing Protection settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Phishing Protection Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Phishing Protection settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var ToggleDetectionButton = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
                var ToggleWarningButton = new DiscordButtonComponent((_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
                var ChangePunishmentButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Punishment", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));
                var ChangeReasonButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Reason", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                var ChangeTimeoutLengthButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Timeout Length", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { ToggleDetectionButton },
                    { ToggleWarningButton },
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { ChangePunishmentButton },
                    { ChangeReasonButton },
                    { ChangeTimeoutLengthButton }
                }).AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == ToggleDetectionButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == ToggleWarningButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect = !_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect;
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == ChangePunishmentButton.CustomId)
                {
                    var dropdown = new DiscordSelectComponent("Select an action..", new List<DiscordSelectComponentOption>
                    {
                        { new DiscordSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                    }, "selection");

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                    async Task ChangePunishmentInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                    {
                        if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;

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
                            _ = ctx.Command.ExecuteAsync(ctx);
                        }
                    };

                    _ = Task.Delay(60000).ContinueWith(x =>
                    {
                        if (x.IsCompletedSuccessfully)
                        {
                            ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;
                            msg.ModifyToTimedOut(true);
                        }
                    });

                    ctx.Client.ComponentInteractionCreated += ChangePunishmentInteraction;
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeReasonButton.CustomId)
                {
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Please specify a new Ban/Kick Reason.\n" +
                                                                                                        "_Type `cancel` or `.` to cancel._\n\n" +
                                                                                                        "**Placeholders**\n" +
                                                                                                        "`%R` - _A placeholder for the reason_")));

                    var reason = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                    if (reason.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = reason.Result.DeleteAsync();

                    if (reason.Result.Content.ToLower() is not "cancel" and not ".")
                        _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = reason.Result.Content;

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeTimeoutLengthButton.CustomId)
                {
                    if (_bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`You aren't using 'Timeout' as your Punishment`")));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Please specify how long the timeout should last with one of the following suffixes:\n" +
                                                                                                        "`d` - _Days (default)_\n" +
                                                                                                        "`h` - _Hours_\n" +
                                                                                                        "`m` - _Minutes_\n" +
                                                                                                        "`s` - _Seconds_\n\n" +
                                                                                                        "e.g.: `31h = 31 Hours`")));

                    var reason = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                    if (reason.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = reason.Result.DeleteAsync();

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
                                    length = TimeSpan.FromDays(Convert.ToInt32(reason.Result.Content));
                                    return;
                            }
                        }

                        if (length > TimeSpan.FromDays(28) || length < TimeSpan.FromSeconds(1))
                        {
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("The duration has to be between 1 second and 28 days.")));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();
                            _ = ctx.Command.ExecuteAsync(ctx);
                            return;
                        }

                        _bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                    }
                    catch (Exception)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Invalid duration")));
                        await Task.Delay(5000);
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                }
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("admin"),
    Description("Allows to review, set up and change settings for the Bump Reminder")]
    public class BumpReminder : BaseCommandModule
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
            if (!_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled)
                return $"`Bump Reminder Enabled` : {_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}";

            return $"`Bump Reminder Enabled` : {_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}\n" +
                $"`Bump Reminder Channel` : <#{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
                $"`Bump Reminder Role   ` : <@&{_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId})`";
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
        Description("Shows a list of all currently defined Bump Reminder settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Bump Reminder Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Bump Reminder settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var Setup = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Set up Bump Reminder", _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Disable Bump Reminder", !_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
                var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Channel", !_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                var ChangeRole = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Role", !_bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { Setup },
                    { Disable }
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { ChangeChannel },
                    { ChangeRole }
                }).AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == Setup.CustomId)
                {
                    if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == Resources.AccountIds.Disboard))
                    {
                        await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                            Color = EmbedColors.Error,
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow,
                            Description = $"`The Disboard bot is not on this server. Please create a guild listing on Disboard and invite the their bot.`"
                        });
                        return;
                    }

                    embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                        Color = EmbedColors.Loading,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = $"`Setting up Bump Reminder..`"
                    };
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.Description = "`Please select a role to ping when the server can be bumped.`";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    DiscordRole role;

                    try
                    {
                        role = await GenericSelectors.PromptRoleSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "BumpReminder");
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    var bump_reaction_msg = await ctx.Channel.SendMessageAsync($"React to this message with ✅ to receive notifications as soon as the server can be bumped again.");
                    _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
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
                    embed.Color = EmbedColors.Success;
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();

                    _bot._bumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == Disable.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings = new();

                    if (GetScheduleTasks() != null)
                        if (GetScheduleTasks().Any(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}"))
                            DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}").Key);

                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.Description = "`The Bump Reminder has been disabled.`";
                    embed.Color = EmbedColors.Success;
                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeChannel.CustomId)
                {
                    try
                    {
                        var channel = await GenericSelectors.PromptChannelSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.ChannelId = channel.Id;
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
                else if (e.Result.Interaction.Data.CustomId == ChangeRole.CustomId)
                {
                    try
                    {
                        var role = await GenericSelectors.PromptRoleSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        _bot._guilds.List[ctx.Guild.Id].BumpReminderSettings.RoleId = role.Id;
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
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("actionlog"), Aliases("action-log"),
    CommandModule("admin"),
    Description("Allows to review, change settings for the actionlog")]
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

        private string GetCurrentConfiguration(CommandContext ctx)
        {
            if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0)
                return $"❌ `The actionlog is disabled.`";

            return $"`Actionlog Channel                 ` : <#{_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel}>\n" +
                    $"`Attempt gathering more details    ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails.BoolToEmote(ctx.Client)}\n" +
                    $"`Join, Leaves & Kicks              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MembersModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Nickname, Role, Membership Updates` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberModified.BoolToEmote(ctx.Client)}\n" +
                    $"`User Profile Updates              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MemberProfileModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Message Deletions                 ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageDeleted.BoolToEmote(ctx.Client)}\n" +
                    $"`Message Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.MessageModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Role Updates                      ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.RolesModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Bans & Unbans                     ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.BanlistModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Server Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.GuildModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Channel Modifications             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.ChannelsModified.BoolToEmote(ctx.Client)}\n" +
                    $"`Voice Channel Updates             ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated.BoolToEmote(ctx.Client)}\n" +
                    $"`Invite Modifications              ` : {_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.InvitesModified.BoolToEmote(ctx.Client)}";
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
        Description("Shows a list of all currently defined Actionlog Settings")]
        public async Task Review(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var ListEmbed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Actionlog Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };
                await ctx.Channel.SendMessageAsync(embed: ListEmbed);
            }).Add(_bot._watcher, ctx);
        }


        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently used Actionlog settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Actionlog Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = GetCurrentConfiguration(ctx)
                };

                var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), $"Disable Actionlog", (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
                var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"{(_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0 ? "Set Channel" : "Change Channel")}", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                var ChangeFilter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"Change Filter", (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📣")));

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    { Disable }
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { ChangeChannel },
                    { ChangeFilter }
                }).AddComponents(Resources.CancelButton));

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == Disable.CustomId)
                {
                    _bot._guilds.List[ctx.Guild.Id].ActionLogSettings = new();

                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == ChangeChannel.CustomId)
                {
                    try
                    {
                        var channel = await GenericSelectors.PromptChannelSelection(_bot, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, true, "actionlog", ChannelType.Text);

                        await channel.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                        {
                            new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                            new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.All },
                        });

                        _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.Channel = channel.Id;

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
                else if (e.Result.Interaction.Data.CustomId == ChangeFilter.CustomId)
                {
                    try
                    {
                        var FilterSelection = await GenericSelectors.PromptCustomSelection(_bot, new List<DiscordSelectComponentOption>
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
                        }, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);

                        switch (FilterSelection)
                        {
                            case "attempt_further_detail":
                                _bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails = !_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails;

                                if (_bot._guilds.List[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails)
                                {
                                    embed.Description = $"⚠ `This may result in inaccurate details being displayed. Please make sure to double check the audit log on serious concerns.`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                    await Task.Delay(5000);
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
                            default:
                                throw new Exception("Unknown option selected.");
                        }

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
                else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                {
                    _ = msg.DeleteAsync();
                    return;
                }
            }).Add(_bot._watcher, ctx);
        }
    }

    [Group("autocrosspost"), Aliases("auto-crosspost", "crosspost"),
    CommandModule("admin"),
    Description("Allows to review, change settings for the automatic crossposts")]
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
    Description("Allows to review, change settings for Reaction Roles")]
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
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
    Description("Allows to review, change In-Voice Text Channel Privacy Settings")]
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
        Description("Allows modifying currently defined Auto Crosspost Channels and settings related to it")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
    Description("Allows to review, change Invite Tracker Settings")]
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
        Description("Allows modifying currently defined Auto Crosspost Channels and settings related to it")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
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
}
