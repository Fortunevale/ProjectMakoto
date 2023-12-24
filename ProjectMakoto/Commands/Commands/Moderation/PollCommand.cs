// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class PollCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var CommandKey = this.t.Commands.Moderation.Poll;

            if (ctx.DbGuild.Polls.RunningPolls.Length >= 10)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.PollLimitReached, true)).AsError(ctx));
                return;
            }

            DiscordRole SelectedRole = null;
            var SelectedChannel = ctx.Channel;

            DateTime? selectedDueDate = DateTime.UtcNow.AddMinutes(5);
            string SelectedPrompt = null;
            List<DiscordStringSelectComponentOption> SelectedOptions = new();

            var SelectedMin = 1;
            var SelectedMax = 1;

            while (true)
            {
                if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 1)))
                {
                    selectedDueDate = null;
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.InvalidTime, true)).AsError(ctx));
                    await Task.Delay(3000);
                }

                if (SelectedMin < 1 || SelectedMax > 20 || SelectedMin > SelectedMax)
                {
                    SelectedMin = 1;
                    SelectedMax = 20;
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.InvalidOptionLimit, true)).AsError(ctx));
                    await Task.Delay(3000);
                }

                var SelectRoleButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectRoleButton), false, DiscordEmoji.FromUnicode("👥").ToComponent());
                var SelectChannelButton = new DiscordButtonComponent((SelectedChannel is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectChannelButton), false, DiscordEmoji.FromUnicode("📢").ToComponent());

                var SelectPromptButton = new DiscordButtonComponent((SelectedPrompt.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectPollContentButton), false, DiscordEmoji.FromUnicode("❓").ToComponent());

                var AddOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.NewOptionButton), (SelectedOptions.Count >= 20), DiscordEmoji.FromUnicode("➕").ToComponent());
                var RemoveOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.RemoveOptionButton), (SelectedOptions.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());
                var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SetTimeButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                var SelectMultiSelectButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectMultiSelectButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💠")));

                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Submit), (SelectedChannel is null || SelectedPrompt.IsNullOrWhiteSpace() || !SelectedOptions.IsNotNullAndNotEmpty() || SelectedOptions.Count < 2 || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.PollContent, CommandKey.AvailableOptions, CommandKey.SelectedChannel, CommandKey.DueTime, CommandKey.Role, CommandKey.MinimumVotes, CommandKey.MaximumVotes);

                var embed = new DiscordEmbedBuilder()
                    .WithDescription($"`{this.GetString(CommandKey.PollContent).PadRight(pad)}`: {(SelectedPrompt.IsNullOrWhiteSpace() ? this.GetString(this.t.Common.NotSelected, true) : $"{SelectedPrompt.FullSanitize()}")}\n" +
                                     $"`{this.GetString(CommandKey.AvailableOptions).PadRight(pad)}`: {(!SelectedOptions.IsNotNullAndNotEmpty() ? this.GetString(CommandKey.NoOptions, true) : $"{string.Join(", ", SelectedOptions.Select(x => $"`{x.Label.TruncateWithIndication(10)}`"))}")}\n\n" +
                                     $"`{this.GetString(CommandKey.SelectedChannel).PadRight(pad)}`: {(SelectedChannel is null ? this.GetString(this.t.Common.NotSelected, true) : SelectedChannel.Mention)}\n" +
                                     $"`{this.GetString(CommandKey.DueTime).PadRight(pad)}`: {(selectedDueDate is null ? this.GetString(this.t.Common.NotSelected, true) : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}\n" +
                                     $"`{this.GetString(CommandKey.Role).PadRight(pad)}`: {(SelectedRole is null ? this.GetString(this.t.Common.NotSelected, true) : SelectedRole.Mention)}\n" +
                                     $"`{this.GetString(CommandKey.MinimumVotes).PadRight(pad)}`: `{SelectedMin}`\n" +
                                     $"`{this.GetString(CommandKey.MaximumVotes).PadRight(pad)}`: `{SelectedMax}`")
                    .AsAwaitingInput(ctx);

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(SelectPromptButton, SelectDueDateButton, SelectMultiSelectButton)
                    .AddComponents(AddOptionButton, RemoveOptionButton)
                    .AddComponents(SelectChannelButton, SelectRoleButton, Finish)
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Menu = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(10));

                if (Menu.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectRoleButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var RoleResult = await this.PromptRoleSelection(new RolePromptConfiguration { DisableOption = this.GetString(CommandKey.DontPing), IncludeEveryone = true });

                    if (RoleResult.TimedOut)
                    {
                        this.ModifyToTimedOut();
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
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoRoles)));
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

                    var ChannelResult = await this.PromptChannelSelection(ChannelType.Text);

                    if (ChannelResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoChannels)));
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
                    var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.ModalTitle), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "prompt", this.GetString(CommandKey.PollContent), "", 1, 256, true));

                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    var ModalResult = await this.PromptModalForDateTime(selectedDueDate ?? DateTime.UtcNow.AddMinutes(5), false);

                    if (ModalResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.InvalidTime, true)).AsError(ctx));
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
                    var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.ModalTitle), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "min", this.GetString(CommandKey.MinimumVotes), null, 1, 2, true, "1"))
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "max", this.GetString(CommandKey.MaximumVotes), null, 1, 2, false, "1"));

                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.InvalidOptionLimit, true)));
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
                    var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.ModalTitle), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", this.GetString(CommandKey.Title), "", 1, 20, true))
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "description", this.GetString(CommandKey.Description), "", null, 256, false));

                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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

                    if (SelectedOptions.Any(x => x.Value == hash || x.Label.Equals(title, StringComparison.OrdinalIgnoreCase)))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.OptionExists)));
                        await Task.Delay(3000);
                        continue;
                    }

                    SelectedOptions.Add(new DiscordStringSelectComponentOption(title, hash, desc));
                    continue;
                }
                else if (Menu.GetCustomId() == RemoveOptionButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var SelectionResult = await this.PromptCustomSelection(SelectedOptions);

                    if (SelectionResult.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
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
                        SelectedChannel = null;
                        continue;
                    }

                    if (SelectedRole is not null && !ctx.Guild.Roles.ContainsKey(SelectedRole.Id))
                    {
                        SelectedRole = null;
                        continue;
                    }

                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 1)))
                    {
                        selectedDueDate = null;
                        continue;
                    }

                    if (ctx.DbGuild.Polls.RunningPolls.Length >= 10)
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.PollLimitReached, true)).AsError(ctx));
                        return;
                    }

                    if (SelectedPrompt.IsNullOrWhiteSpace())
                        continue;

                    if (SelectedOptions?.Count < 2)
                        continue;

                    var select = new DiscordStringSelectComponent(this.GetGuildString(CommandKey.VoteOnThisPoll), SelectedOptions.Take(20), Guid.NewGuid().ToString(), (SelectedMin >= SelectedOptions.Take(20).Count() ? SelectedOptions.Take(20).Count() - 1 : SelectedMin), (SelectedMax > SelectedOptions.Take(20).Count() ? SelectedOptions.Take(20).Count() : SelectedMax));
                    var endearly = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetGuildString(CommandKey.EndPollEarly), false, DiscordEmoji.FromUnicode("🗑").ToComponent());
                    var polltxt = $"{SelectedPrompt.FullSanitize()}";

                    var msg = await SelectedChannel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent(SelectedRole is null ? "" : SelectedRole.Mention)
                        .WithEmbed(new DiscordEmbedBuilder().AsAwaitingInput(ctx, this.GetGuildString(CommandKey.Poll))
                            .WithDescription($"> **{polltxt}**\n\n_{this.GetGuildString(CommandKey.PollEnding, new TVar("Timestamp", selectedDueDate.Value.ToTimestamp()))}._\n\n`{this.GetGuildString(CommandKey.TotalVotes, new TVar("Count", 0))}`"))
                        .AddComponents(select).AddComponents(endearly));

                    ctx.DbGuild.Polls.RunningPolls = ctx.DbGuild.Polls.RunningPolls.Add(new()
                    {
                        PollText = polltxt,
                        ChannelId = SelectedChannel.Id,
                        MessageId = msg.Id,
                        SelectUUID = select.CustomId,
                        EndEarlyUUID = endearly.CustomId,
                        Votes = Array.Empty<Entities.Guilds.PollEntry.Vote>(),
                        DueTime = selectedDueDate.Value,
                        Options = SelectedOptions.ToDictionary(x => x.Value, y => y.Label)
                    });

                    this.DeleteOrInvalidate();
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.CancelButtonId)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
            }
        });
    }
}