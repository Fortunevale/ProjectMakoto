namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                Color = EmbedColors.Loading,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = BumpReminderCommandAbstractions.GetCurrentConfiguration(ctx)
            };

            var Setup = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Set up Bump Reminder", ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Disable Bump Reminder", !ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Channel", !ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeRole = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Role", !ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
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

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == Setup.CustomId)
            {
                if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == ctx.Bot._status.LoadedConfig.DisboardAccountId))
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
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
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.Loading, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                    Color = EmbedColors.Loading,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Setting up Bump Reminder..`"
                };
                await RespondOrEdit(embed);

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`Please select a role to ping when the server can be bumped.`";
                await RespondOrEdit(embed);

                DiscordRole role;

                try
                {
                    role = await PromptRoleSelection(true, "BumpReminder");
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                var bump_reaction_msg = await ctx.Channel.SendMessageAsync($"React to this message with ✅ to receive notifications as soon as the server can be bumped again.");
                _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
                _ = bump_reaction_msg.PinAsync();

                _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.RoleId = role.Id;
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId = ctx.Channel.Id;
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.MessageId = bump_reaction_msg.Id;
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.LastBump = DateTime.UtcNow.AddHours(-2);
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.LastReminder = DateTime.UtcNow.AddHours(-2);
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.LastUserId = 0;

                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled = true;

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`The Bump Reminder has been set up.`";
                embed.Color = EmbedColors.Success;
                await RespondOrEdit(embed);

                await Task.Delay(5000);
                ctx.Bot._bumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Disable.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings = new(ctx.Bot._guilds[ctx.Guild.Id]);

                if (GetScheduleTasks() != null)
                    if (GetScheduleTasks().Any(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}"))
                        DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}").Key);

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = "`The Bump Reminder has been disabled.`";
                embed.Color = EmbedColors.Success;
                await RespondOrEdit(embed);

                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeChannel.CustomId)
            {
                try
                {
                    var channel = await PromptChannelSelection();

                    ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId = channel.Id;
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeRole.CustomId)
            {
                try
                {
                    var role = await PromptRoleSelection();

                    ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.RoleId = role.Id;
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}