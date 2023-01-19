namespace ProjectIchigo.Commands;

internal class PollCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if (ctx.Bot.guilds[ctx.Guild.Id].Polls.RunningPolls.Count >= 10)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`There's already 10 polls running on this guild.`").AsError(ctx));
                return;
            }

            DiscordRole SelectedRole = null;
            DiscordChannel SelectedChannel = ctx.Channel;

            DateTime? selectedDueDate = DateTime.UtcNow.AddMinutes(5);
            string SelectedPrompt = null;
            List<DiscordStringSelectComponentOption> SelectedOptions = new();

            int SelectedMin = 1;
            int SelectedMax = 1;

            while (true)
            {
                if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 1)))
                {
                    selectedDueDate = null;
                    await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You specified a date in the past or a date further away than 1 month.`").AsError(ctx));
                    await Task.Delay(5000);
                }

                if (SelectedMin < 1 || SelectedMax > 20 || SelectedMin > SelectedMax)
                {
                    SelectedMin = 1;
                    SelectedMax = 20;
                    await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You specified an invalid minimum or maximum.`").AsError(ctx));
                    await Task.Delay(5000);
                }

                var SelectRoleButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Select Role", false, DiscordEmoji.FromUnicode("👥").ToComponent());
                var SelectChannelButton = new DiscordButtonComponent((SelectedChannel is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Channel", false, DiscordEmoji.FromUnicode("📢").ToComponent());

                var SelectPromptButton = new DiscordButtonComponent((SelectedPrompt.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Poll Content", false, DiscordEmoji.FromUnicode("❓").ToComponent());
            
                var AddOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add new option", (SelectedOptions.Count >= 20), DiscordEmoji.FromUnicode("➕").ToComponent());
                var RemoveOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Remove option", (SelectedOptions.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());
                var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Due Date & Time", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                var SelectMultiSelectButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Set Multi Select Limits", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💠")));

                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit", (SelectedChannel is null || SelectedPrompt.IsNullOrWhiteSpace() || !SelectedOptions.IsNotNullAndNotEmpty() || SelectedOptions.Count < 2 || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
            
                var embed = new DiscordEmbedBuilder()
                    .WithDescription($"`Poll Content     `: {(SelectedPrompt.IsNullOrWhiteSpace() ? "`Not yet selected.`" : $"{SelectedPrompt.FullSanitize()}")}\n" +
                                     $"`Available Options`: {(!SelectedOptions.IsNotNullAndNotEmpty() ? "`No options set.`" : $"{string.Join(", ", SelectedOptions.Select(x => $"`{x.Label.TruncateWithIndication(10)}`"))}")}\n\n" +
                                     $"`Selected Channel `: {(SelectedChannel is null ? "`Not yet selected.`" : SelectedChannel.Mention)}\n" +
                                     $"`Due Time & Time  `: {(selectedDueDate is null ? "`Not yet selected.`" : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}\n" +
                                     $"`Role to mention  `: {(SelectedRole is null ? "`No Role selected`" : SelectedRole.Mention)}\n" +
                                     $"`Minimum Votes    `: `{SelectedMin}`\n" +
                                     $"`Maximum Votes    `: `{SelectedMax}`")
                    .AsAwaitingInput(ctx);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(SelectPromptButton, SelectDueDateButton, SelectMultiSelectButton)
                    .AddComponents(AddOptionButton, RemoveOptionButton)
                    .AddComponents(SelectChannelButton, SelectRoleButton, Finish)
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

                var Menu = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(10));

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectRoleButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var RoleResult = await PromptRoleSelection(new RolePromptConfiguration { DisableOption = "Do not ping anyone for this poll.", IncludeEveryone = true });

                    if (RoleResult.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }
                    else if (RoleResult.Cancelled)
                    {
                        continue;
                    }
                    else if (RoleResult.Failed)
                    {
                        if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any roles in your server.`"));
                            await Task.Delay(3000);
                            continue;
                        }

                        throw RoleResult.Exception;
                    }

                    SelectedRole = RoleResult.Result;
                    continue;
                }
                else if (Menu.GetCustomId() == SelectChannelButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var ChannelResult = await PromptChannelSelection(ChannelType.Text);

                    if (ChannelResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ChannelResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ChannelResult.Failed)
                    {
                        if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                            await Task.Delay(3000);
                            continue;
                        }

                        throw ChannelResult.Exception;
                    }

                    SelectedChannel = ChannelResult.Result;
                    continue;
                }
                else if (Menu.GetCustomId() == SelectPromptButton.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("New Poll", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "prompt", "Poll Content", "", 1, 256, true));

                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ModalResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        throw ModalResult.Exception;
                    }

                    SelectedPrompt = ModalResult.Result.Interaction.GetModalValueByCustomId("prompt").Truncate(256);
                    continue;
                }
                else if (Menu.GetCustomId() == SelectDueDateButton.CustomId)
                {
                    var ModalResult = await PromptModalForDateTime(Menu.Result.Interaction, false);

                    if (ModalResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ModalResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        if (ModalResult.Exception.GetType() == typeof(ArgumentException) || ModalResult.Exception.GetType() == typeof(ArgumentOutOfRangeException))
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You specified an invalid date time.`").AsError(ctx));
                            await Task.Delay(5000);
                            continue;
                        }

                        throw ModalResult.Exception;
                    }

                    selectedDueDate = ModalResult.Result;
                    continue;
                }
                else if (Menu.GetCustomId() == SelectMultiSelectButton.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("New Poll", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "min", "Minimum", null, 1, 2, true, "1"))
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "max", "Maximum", null, 1, 2, false, "1"));

                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ModalResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        throw ModalResult.Exception;
                    }

                    try
                    {
                        if (!ModalResult.Result.Interaction.GetModalValueByCustomId("min").Truncate(2).IsDigitsOnly() || !ModalResult.Result.Interaction.GetModalValueByCustomId("max").Truncate(2).IsDigitsOnly())
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Invalid input.`"));
                            await Task.Delay(3000);
                            continue;
                        }

                        SelectedMin = ModalResult.Result.Interaction.GetModalValueByCustomId("min").Truncate(2).ToInt32();
                        SelectedMax = ModalResult.Result.Interaction.GetModalValueByCustomId("max").Truncate(2).ToInt32();
                        continue;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else if (Menu.GetCustomId() == AddOptionButton.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("New Poll", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "", 1, 20, true))
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "description", "Description", "", null, 256, false));

                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ModalResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        throw ModalResult.Exception;
                    }

                    var title = ModalResult.Result.Interaction.GetModalValueByCustomId("title").Truncate(20);
                    var desc = ModalResult.Result.Interaction.GetModalValueByCustomId("description").Truncate(256);

                    var hash = (title + desc).GetSHA256();

                    if (SelectedOptions.Any(x => x.Value == hash || x.Label.ToLower() == title.ToLower()))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`This option has already been added.`"));
                        await Task.Delay(3000);
                        continue;
                    }

                    SelectedOptions.Add(new DiscordStringSelectComponentOption(title, hash, desc));
                    continue;
                }
                else if (Menu.GetCustomId() == RemoveOptionButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var SelectionResult = await PromptCustomSelection(SelectedOptions);

                    if (SelectionResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (SelectionResult.Cancelled)
                    {
                        continue;
                    }
                    else if (SelectionResult.Errored)
                    {
                        throw SelectionResult.Exception;
                    }

                    SelectedOptions = SelectedOptions.Where(x => x.Value != SelectionResult.Result).ToList();
                    continue;
                }
                else if (Menu.GetCustomId() == Finish.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    if (!ctx.Guild.Channels.ContainsKey(SelectedChannel.Id))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`The channel you selected no longer exists.`").AsError(ctx));
                        await Task.Delay(1000);
                        continue;
                    }

                    if (SelectedRole is not null && !ctx.Guild.Roles.ContainsKey(SelectedRole.Id))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`The role you selected no longer exists.`").AsError(ctx));
                        await Task.Delay(1000);
                        continue;
                    }

                    if (SelectedPrompt.IsNullOrWhiteSpace())
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`The poll content you set is empty.`").AsError(ctx));
                        await Task.Delay(1000);
                        continue;
                    }

                    if (SelectedOptions?.Count < 2)
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please specify at least 2 options.`").AsError(ctx));
                        await Task.Delay(1000);
                        continue;
                    }

                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 1)))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Invalid Date & Time.`").AsError(ctx));
                        await Task.Delay(1000);
                        continue;
                    }

                    if (ctx.Bot.guilds[ctx.Guild.Id].Polls.RunningPolls.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`There's already 10 polls running on this guild.`").AsError(ctx));
                        return;
                    }

                    var select = new DiscordStringSelectComponent("Vote on this poll..", SelectedOptions.Take(20), Guid.NewGuid().ToString(), (SelectedMin >= SelectedOptions.Take(20).Count() ? SelectedOptions.Take(20).Count() - 1 : SelectedMin), (SelectedMax > SelectedOptions.Take(20).Count() ? SelectedOptions.Take(20).Count() : SelectedMax));
                    var endearly = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "End this poll early", false, DiscordEmoji.FromUnicode("🗑").ToComponent());
                    var polltxt = $"{SelectedPrompt.FullSanitize()}";

                    var msg = await SelectedChannel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent(SelectedRole is null ? "" : SelectedRole.Mention)
                        .WithEmbed(new DiscordEmbedBuilder().AsAwaitingInput(ctx, "Poll")
                            .WithDescription($"> **{polltxt}**\n\n_This poll will end {selectedDueDate.Value.ToTimestamp()}._\n\n`0 Total Votes`"))
                        .AddComponents(select).AddComponents(endearly));

                    ctx.Bot.guilds[ctx.Guild.Id].Polls.RunningPolls.Add(new PollEntry
                    {
                        PollText = polltxt,
                        ChannelId = SelectedChannel.Id,
                        MessageId = msg.Id,
                        SelectUUID = select.CustomId,
                        EndEarlyUUID = endearly.CustomId,
                        Votes = new(),
                        DueTime = selectedDueDate.Value,
                        Options = SelectedOptions.ToDictionary(x => x.Value, y => y.Label)
                    });

                    DeleteOrInvalidate();
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                {
                    DeleteOrInvalidate();
                    return;
                }
            }
        });
    }
}