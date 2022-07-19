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
                await new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined Auto Crosspost Channels and settings related to it")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }


    [Group("reactionroles"), Aliases("reactionrole", "reaction-roles", "reaction-role"),
    CommandModule("admin"),
    Description("Allows to review and change settings for Reaction Roles")]
    public class ReactionRoles : BaseCommandModule
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
                await new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to add, delete and modify reaction roles")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("add"), Description("Allows adding a reaction role to a message directly, skipping the lengthy questioning. **This command requires replying to a message.**"), Priority(0)]
        public async Task Add(CommandContext ctx, DiscordEmoji emoji_parameter, DiscordRole role_parameter)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "emoji_parameter", emoji_parameter },
                    { "role_parameter", role_parameter },
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("add"), Description("Allows adding a reaction role to a message directly, skipping the lengthy questioning. **This command requires replying to a message.**"), Priority(1)]
        public async Task Add2(CommandContext ctx, DiscordRole role_parameter, DiscordEmoji emoji_parameter) => await Add(ctx, emoji_parameter, role_parameter);

        [Command("remove"), Description("Allows removing a specific reaction role from a message directly, skipping the lengthy questioning. **This command requires replying to a message.**")]
        public async Task Remove(CommandContext ctx, DiscordEmoji emoji_parameter)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "emoji_parameter", emoji_parameter },
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("removeall"), Description("Allows removing all reaction roles from a message directly, skipping the lengthy questioning. **This command requires replying to a message.**")]
        public async Task RemoveAll(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }


    [Group("invoiceprivacy"), Aliases("in-voice-privacy", "vc-privacy", "vcprivacy"),
    CommandModule("admin"),
    Description("Allows to review and change In-Voice Text Channel Privacy Settings")]
    public class InVoiceTextPrivacy : BaseCommandModule
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
                await new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows modifying currently defined In-Voice Text Channel Privacy Settings")]
        public async Task Config(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
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
