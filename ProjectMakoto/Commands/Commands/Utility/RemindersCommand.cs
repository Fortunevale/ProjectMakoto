// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class RemindersCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string? snoozeDescription = null;

            if (arguments?.Any() ?? false)
                snoozeDescription = arguments["description"]?.ToString();

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var rem = ctx.DbUser.Reminders;

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Commands.Utility.Reminders.NewReminder), (rem.ScheduledReminders.Count >= 10), DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Commands.Utility.Reminders.DeleteReminder), (rem.ScheduledReminders.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());
            string SelectedCustomId = (snoozeDescription is null ? "" : AddButton.CustomId);

            if (snoozeDescription is null)
            {
                await RespondOrEdit(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"{GetString(this.t.Commands.Utility.Reminders.Count, true, new TVar("Count", rem.ScheduledReminders.Count))}\n\n" +
                         $"{string.Join("\n\n", rem.ScheduledReminders.Select(x => $"> {x.Description.FullSanitize()}\n{GetString(this.t.Commands.Utility.Reminders.CreatedOn, new TVar("Guild", $"**{x.CreationPlace}**"))}\n{GetString(this.t.Commands.Utility.Reminders.DueTime, new TVar("Relative", x.DueTime.ToTimestamp()), new TVar("DateTime", x.DueTime.ToTimestamp(TimestampFormat.LongDateTime)))}").ToList())}\n\n" +
                         $"**⚠ {GetString(this.t.Commands.Utility.Reminders.Notice)}**")
                        .AsInfo(ctx, GetString(this.t.Commands.Utility.Reminders.Title)))
                    .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (Button.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                SelectedCustomId = Button.GetCustomId();
            }

            if (SelectedCustomId == AddButton.CustomId)
            {
                string selectedDescription = snoozeDescription.IsNullOrWhiteSpace() ? "" : snoozeDescription;
                DateTime? selectedDueDate = null;

                while (true)
                {
                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 6)))
                    {
                        selectedDueDate = null;
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx));
                        await Task.Delay(5000);
                    }

                    var SelectDescriptionButton = new DiscordButtonComponent((selectedDescription.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(this.t.Commands.Utility.Reminders.SetDescription), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✏")));
                    var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(this.t.Commands.Utility.Reminders.SetDateTime), (selectedDescription is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(this.t.Common.Submit), (selectedDescription.IsNullOrWhiteSpace() || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    int padding = TranslationUtil.CalculatePadding(ctx.DbUser, this.t.Commands.Utility.Reminders.Description, this.t.Commands.Utility.Reminders.DateTime);

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(this.t.Commands.Utility.Reminders.Description).PadRight(padding)}`: {(selectedDescription.IsNullOrWhiteSpace() ? $"`{GetString(this.t.Common.NotSelected)}`" : $"`{selectedDescription.FullSanitize()}`")}\n" +
                                      $"`{GetString(this.t.Commands.Utility.Reminders.DateTime).PadRight(padding)}`: {(selectedDueDate is null ? $"`{GetString(this.t.Common.NotSelected)}`" : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}"
                    }.AsAwaitingInput(ctx, GetString(this.t.Commands.Utility.Reminders.Title));

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectDescriptionButton, SelectDueDateButton, Finish })
                        .AddComponents(MessageComponents.GetBackButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectDescriptionButton.CustomId)
                    {
                        var maxLength = 100 - JsonConvert.SerializeObject(new ReminderSnoozeButton(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }).Length;

                        var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Utility.Reminders.NewReminder), Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "desc", GetString(this.t.Commands.Utility.Reminders.Description), GetString(this.t.Commands.Utility.Reminders.SetDescription), 1, maxLength, true));


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

                        selectedDescription = ModalResult.Result.Interaction.GetModalValueByCustomId("desc").TruncateWithIndication(maxLength);
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
                                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx));
                                await Task.Delay(5000);
                                continue;
                            }

                            throw ModalResult.Exception;
                        }

                        selectedDueDate = ModalResult.Result;
                    }
                    else if (Menu.GetCustomId() == Finish.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (selectedDueDate < DateTime.UtcNow)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx, GetString(this.t.Commands.Utility.Reminders.Title)));
                            await Task.Delay(2000);
                            continue;
                        }

                        rem.ScheduledReminders.Add(new ReminderItem
                        {
                            Description = selectedDescription,
                            DueTime = selectedDueDate.Value.ToUniversalTime(),
                            CreationPlace = ctx.Channel.IsPrivate ? $"[`@{ctx.CurrentUser.GetUsername()}`](https://discord.com/channels/@me/{ctx.Channel.Id})" : $"[`{ctx.Guild.Name}`](https://discord.com/channels/{ctx.Guild.Id}/{ctx.Channel.Id})"
                        });

                        await ExecuteCommand(ctx, null);
                        return;
                    }
                    else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await ExecuteCommand(ctx, null);
                        return;
                    }
                }
            }
            else if (SelectedCustomId == RemoveButton.CustomId)
            {
                if (rem.ScheduledReminders.Count == 0)
                {
                    await ExecuteCommand(ctx, null);
                    return;
                }

                var UuidResult = await PromptCustomSelection(rem.ScheduledReminders
                        .Select(x => new DiscordStringSelectComponentOption($"{x.Description}".TruncateWithIndication(100), x.UUID, $"in {x.DueTime.GetTotalSecondsUntil().GetHumanReadable()}")).ToList());

                if (UuidResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (UuidResult.Cancelled)
                {
                    await ExecuteCommand(ctx, null);
                    return;
                }
                else if (UuidResult.Errored)
                {
                    throw UuidResult.Exception;
                }

                rem.ScheduledReminders.Remove(rem.ScheduledReminders.First(x => x.UUID == UuidResult.Result));
                await ExecuteCommand(ctx, null);
                return;
            }
            else if (SelectedCustomId == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}