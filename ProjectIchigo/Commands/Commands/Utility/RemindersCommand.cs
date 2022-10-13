namespace ProjectIchigo.Commands;

internal class RemindersCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            var rem = ctx.Bot.users[ctx.User.Id].Reminders;

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "New Reminder", (rem.ScheduledReminders.Count >= 10), DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Delete a Reminder", (rem.ScheduledReminders.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"`You have {rem.ScheduledReminders.Count} reminders.`\n\n" +
                     $"{string.Join("\n\n", rem.ScheduledReminders.Select(x => $"> {x.Description.Sanitize()}\nCreated on **{x.CreationPlace}**\nDue {x.DueTime.ToTimestamp()} ({x.DueTime.ToTimestamp(TimestampFormat.LongDateTime)})").ToList())}\n\n" +
                     $"**⚠ For reminders to work, you need to enable Direct Messages on at least one server you share with {ctx.CurrentUser.Username}.**")
                    .SetInfo(ctx, "Reminders"))
                .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                .AddComponents(MessageComponents.CancelButton));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.Result.Interaction.Data.CustomId == AddButton.CustomId)
            {
                string selectedDescription = "";
                DateTime? selectedDueDate = null;

                while (true)
                {
                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 6)))
                    {
                        selectedDueDate = null;
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You specified a date in the past or a date further away than 6 months.`").SetError(ctx));
                        await Task.Delay(5000);
                    }

                    var SelectDescriptionButton = new DiscordButtonComponent((selectedDescription.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Description", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✏")));
                    var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Due Date & Time", (selectedDescription is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit", (selectedDescription.IsNullOrWhiteSpace() || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`Description    `: {(selectedDescription.IsNullOrWhiteSpace() ? "`Not yet selected.`" : $"`{selectedDescription.Sanitize()}`")}\n" +
                                      $"`Due Date & Time`: {(selectedDueDate is null ? "`Not yet selected.`" : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}"
                    }.SetAwaitingInput(ctx, "Reminders");

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectDescriptionButton, SelectDueDateButton, Finish })
                        .AddComponents(MessageComponents.CancelButton));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.Result.Interaction.Data.CustomId == SelectDescriptionButton.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("New Reminder", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "desc", "Description", "Enter a reminder description..", 1, 512, true));


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

                        selectedDescription = ModalResult.Result.Interaction.GetModalValueByCustomId("desc");
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == SelectDueDateButton.CustomId)
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
                                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You specified an invalid date time.`").SetError(ctx));
                                await Task.Delay(5000);
                                continue;
                            }

                            throw ModalResult.Exception;
                        }

                        selectedDueDate = ModalResult.Result;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == Finish.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (selectedDueDate < DateTime.UtcNow)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The Due Time is in the past.`").SetError(ctx, "Reminders"));
                            await Task.Delay(2000);
                            continue;
                        }

                        rem.ScheduledReminders.Add(new ReminderItem
                        {
                            Description = selectedDescription,
                            DueTime = selectedDueDate.Value.ToUniversalTime(),
                            CreationPlace = $"[`{ctx.Guild.Name}`](https://discord.com/channels/{ctx.Guild.Id}/{ctx.Channel.Id})"
                        });

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                }
            }
            else if (Button.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
            {
                var UuidResult = await PromptCustomSelection(rem.ScheduledReminders
                        .Select(x => new DiscordSelectComponentOption($"{x.Description}".TruncateWithIndication(100), x.UUID, $"in {x.DueTime.GetTotalSecondsUntil().GetHumanReadable()}")).ToList());

                if (UuidResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (UuidResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (UuidResult.Errored)
                {
                    throw UuidResult.Exception;
                }

                rem.ScheduledReminders.Remove(rem.ScheduledReminders.First(x => x.UUID == UuidResult.Result));
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}