namespace ProjectIchigo.Commands;

internal class RemindersCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            var rem = ctx.Bot.users[ctx.User.Id].Reminders;

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Commands.Reminders.NewReminder), (rem.ScheduledReminders.Count >= 10), DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Commands.Reminders.DeleteReminder), (rem.ScheduledReminders.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"`{GetString(t.Commands.Reminders.Count).Replace("{Count}", rem.ScheduledReminders.Count)}`\n\n" +
                     $"{string.Join("\n\n", rem.ScheduledReminders.Select(x => $"> {x.Description.FullSanitize()}\n{GetString(t.Commands.Reminders.Created).Replace("{Guild}", $"**{x.CreationPlace}**")}\n{GetString(t.Commands.Reminders.DueTime).Replace("{Relative}", x.DueTime.ToTimestamp()).Replace("{DateTime}", x.DueTime.ToTimestamp(TimestampFormat.LongDateTime))}").ToList())}\n\n" +
                     $"**⚠ {GetString(t.Commands.Reminders.Notice).Replace("{Bot}", ctx.CurrentUser.Username)}**")
                    .AsInfo(ctx, GetString(t.Commands.Reminders.Title)))
                .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.GetCustomId() == AddButton.CustomId)
            {
                string selectedDescription = "";
                DateTime? selectedDueDate = null;

                while (true)
                {
                    if (selectedDueDate.HasValue && (selectedDueDate.Value.Ticks < DateTime.UtcNow.Ticks || selectedDueDate.Value.GetTimespanUntil() > TimeSpan.FromDays(30 * 6)))
                    {
                        selectedDueDate = null;
                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Reminders.InvalidDateTime)}`").AsError(ctx));
                        await Task.Delay(5000);
                    }

                    var SelectDescriptionButton = new DiscordButtonComponent((selectedDescription.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(t.Commands.Reminders.SetDescription), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✏")));
                    var SelectDueDateButton = new DiscordButtonComponent((selectedDueDate is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(t.Commands.Reminders.SetDateTime), (selectedDescription is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Submit), (selectedDescription.IsNullOrWhiteSpace() || selectedDueDate is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    int padding = GenericExtensions.CalculatePadding(ctx.DbUser, t.Commands.Reminders.Description, t.Commands.Reminders.DateTime);

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Reminders.Description).PadRight(padding)}`: {(selectedDescription.IsNullOrWhiteSpace() ? $"`{GetString(t.Common.NotSelected)}`" : $"`{selectedDescription.FullSanitize()}`")}\n" +
                                      $"`{GetString(t.Commands.Reminders.DateTime).PadRight(padding)}`: {(selectedDueDate is null ? $"`{GetString(t.Common.NotSelected)}`" : $"{selectedDueDate.Value.ToTimestamp(TimestampFormat.LongDateTime)} ({selectedDueDate.Value.ToTimestamp()})")}"
                    }.AsAwaitingInput(ctx, GetString(t.Commands.Reminders.Title));

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectDescriptionButton, SelectDueDateButton, Finish })
                        .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectDescriptionButton.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder(GetString(t.Commands.Reminders.NewReminder), Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "desc", GetString(t.Commands.Reminders.Description), GetString(t.Commands.Reminders.SetDescription), 1, 512, true));


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
                                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Reminders.InvalidDateTime)}`").AsError(ctx));
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
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Reminders.InvalidDateTime)}`").AsError(ctx, GetString(t.Commands.Reminders.Title)));
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
                    else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                }
            }
            else if (Button.GetCustomId() == RemoveButton.CustomId)
            {
                var UuidResult = await PromptCustomSelection(rem.ScheduledReminders
                        .Select(x => new DiscordStringSelectComponentOption($"{x.Description}".TruncateWithIndication(100), x.UUID, $"in {x.DueTime.GetTotalSecondsUntil().GetHumanReadable()}")).ToList());

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
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}