namespace ProjectMakoto.Commands.BumpReminderCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = BumpReminderCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Bump Reminder");

            var Setup = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Set up Bump Reminder", ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Disable Bump Reminder", !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Channel", !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeRole = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Role", !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

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
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Setup.CustomId)
            {
                if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == ctx.Bot.status.LoadedConfig.Accounts.Disboard))
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`The Disboard bot is not on this server. Please create a guild listing on Disboard and invite the their bot.`"
                    }.AsError(ctx, "Bump Reminder"));
                    return;
                }

                embed = new DiscordEmbedBuilder
                {
                    Description = $"`Setting up Bump Reminder..`"
                }.AsLoading(ctx, "Bump Reminder");
                await RespondOrEdit(embed);

                embed.Description = "`Please select a role to ping when the server can be bumped.`";
                embed = embed.AsAwaitingInput(ctx, "Bump Reminder");
                await RespondOrEdit(embed);


                var RoleResult = await PromptRoleSelection(new() { CreateRoleOption = "BumpReminder" });

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any roles in your server.`"));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                if (RoleResult.Result.Id == ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId || ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == RoleResult.Result.Id))
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`The role you selected is already being assigned on join or part of a level reward.`"));
                    await Task.Delay(3000);
                    return;
                }

                var bump_reaction_msg = await ctx.Channel.SendMessageAsync($"React to this message with ✅ to receive notifications as soon as the server can be bumped again.");
                _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
                _ = bump_reaction_msg.PinAsync();

                _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId = RoleResult.Result.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId = ctx.Channel.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.MessageId = bump_reaction_msg.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastBump = DateTime.UtcNow.AddHours(-2);
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastReminder = DateTime.UtcNow.AddHours(-2);
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastUserId = 0;

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled = true;

                embed.Description = "`The Bump Reminder has been set up.`";
                embed = embed.AsSuccess(ctx, "Bump Reminder");
                await RespondOrEdit(embed);

                await Task.Delay(5000);
                ctx.Bot.bumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == Disable.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder = new(ctx.Bot.guilds[ctx.Guild.Id]);

                if (GetScheduleTasks() != null)
                    if (GetScheduleTasks().Any(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}"))
                        DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ctx.Guild.Id}").Key);

                embed.Description = "`The Bump Reminder has been disabled.`";
                embed = embed.AsSuccess(ctx, "Bump Reminder");
                await RespondOrEdit(embed);

                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text);

                if (ChannelResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId = ChannelResult.Result.Id;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeRole.CustomId)
            {

                var RoleResult = await PromptRoleSelection();

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any roles in your server.`"));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId = RoleResult.Result.Id;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}