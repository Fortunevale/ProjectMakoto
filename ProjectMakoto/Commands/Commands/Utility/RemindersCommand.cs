// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Users;

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

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Utility.Reminders.NewReminder), (rem.ScheduledReminders.Length >= 10), DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Utility.Reminders.DeleteReminder), (rem.ScheduledReminders.Length <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());
            var SelectedCustomId = (snoozeDescription is null ? "" : AddButton.CustomId);

            if (snoozeDescription is null)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"{this.GetString(this.t.Commands.Utility.Reminders.Count, true, new TVar("Count", rem.ScheduledReminders.Length))}\n\n" +
                         $"{string.Join("\n\n", rem.ScheduledReminders.Select(x => $"> {x.Description.FullSanitize()}\n{this.GetString(this.t.Commands.Utility.Reminders.CreatedOn, new TVar("Guild", $"**{x.CreationPlace}**"))}\n{this.GetString(this.t.Commands.Utility.Reminders.DueTime, new TVar("Relative", x.DueTime.ToTimestamp()), new TVar("DateTime", x.DueTime.ToTimestamp(TimestampFormat.LongDateTime)))}").ToList())}\n\n" +
                         $"**⚠ {this.GetString(this.t.Commands.Utility.Reminders.Notice)}**")
                        .AsInfo(ctx, this.GetString(this.t.Commands.Utility.Reminders.Title)))
                    .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (Button.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }

                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                SelectedCustomId = Button.GetCustomId();
            }

            if (SelectedCustomId == AddButton.CustomId)
            {
                var selectedDescription = snoozeDescription.IsNullOrWhiteSpace() ? "" : snoozeDescription;
                DateTime? selectedDueDate = null;

                while (true)
                {
                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 6)))
                    {
                        selectedDueDate = null;
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx));
                        await Task.Delay(5000);
                    }

                    var SelectDescriptionButton = new DiscordButtonComponent((selectedDescription.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Utility.Reminders.SetDescription), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✏")));
                    var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Utility.Reminders.SetDateTime), (selectedDescription is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Submit), (selectedDescription.IsNullOrWhiteSpace() || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var padding = TranslationUtil.CalculatePadding(ctx.DbUser, this.t.Commands.Utility.Reminders.Description, this.t.Commands.Utility.Reminders.DateTime);

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{this.GetString(this.t.Commands.Utility.Reminders.Description).PadRight(padding)}`: {(selectedDescription.IsNullOrWhiteSpace() ? $"`{this.GetString(this.t.Common.NotSelected)}`" : $"`{selectedDescription.FullSanitize()}`")}\n" +
                                      $"`{this.GetString(this.t.Commands.Utility.Reminders.DateTime).PadRight(padding)}`: {(selectedDueDate is null ? $"`{this.GetString(this.t.Common.NotSelected)}`" : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}"
                    }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Utility.Reminders.Title));

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectDescriptionButton, SelectDueDateButton, Finish })
                        .AddComponents(MessageComponents.GetBackButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        this.ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectDescriptionButton.CustomId)
                    {
                        var maxLength = 100 - JsonConvert.SerializeObject(new ReminderSnoozeButton(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }).Length;

                        var modal = new DiscordInteractionModalBuilder(this.GetString(this.t.Commands.Utility.Reminders.NewReminder), Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "desc", this.GetString(this.t.Commands.Utility.Reminders.Description), this.GetString(this.t.Commands.Utility.Reminders.SetDescription), 1, maxLength, true));


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

                        selectedDescription = ModalResult.Result.Interaction.GetModalValueByCustomId("desc").TruncateWithIndication(maxLength);
                    }
                    else if (Menu.GetCustomId() == SelectDueDateButton.CustomId)
                    {

                        var ModalResult = await this.PromptModalForDateTime(Menu.Result.Interaction, selectedDueDate ?? DateTime.UtcNow.AddMinutes(5), false);

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
                                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx));
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
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.Reminders.InvalidDateTime, true)).AsError(ctx, this.GetString(this.t.Commands.Utility.Reminders.Title)));
                            await Task.Delay(2000);
                            continue;
                        }

                        rem.ScheduledReminders = rem.ScheduledReminders.Add(new()
                        {
                            Description = selectedDescription,
                            DueTime = selectedDueDate.Value.ToUniversalTime(),
                            CreationPlace = ctx.Channel.IsPrivate ? $"[`@{ctx.CurrentUser.GetUsername()}`](https://discord.com/channels/@me/{ctx.Channel.Id})" : $"[`{ctx.Guild.Name}`](https://discord.com/channels/{ctx.Guild.Id}/{ctx.Channel.Id})"
                        });

                        await this.ExecuteCommand(ctx, null);
                        return;
                    }
                    else if (Menu.GetCustomId() == MessageComponents.BackButtonId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await this.ExecuteCommand(ctx, null);
                        return;
                    }
                }
            }
            else if (SelectedCustomId == RemoveButton.CustomId)
            {
                if (rem.ScheduledReminders.Length == 0)
                {
                    await this.ExecuteCommand(ctx, null);
                    return;
                }

                var UuidResult = await this.PromptCustomSelection(rem.ScheduledReminders
                        .Select(x => new DiscordStringSelectComponentOption($"{x.Description}".TruncateWithIndication(100), x.UUID, $"in {x.DueTime.GetTotalSecondsUntil().GetHumanReadable()}")).ToList());

                if (UuidResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (UuidResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, null);
                    return;
                }
                else if (UuidResult.Errored)
                {
                    throw UuidResult.Exception;
                }

                rem.ScheduledReminders = rem.ScheduledReminders.Remove(x => x.UUID, rem.ScheduledReminders.First(x => x.UUID == UuidResult.Result));
                await this.ExecuteCommand(ctx, null);
                return;
            }
            else if (SelectedCustomId == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}