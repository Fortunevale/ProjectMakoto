namespace ProjectIchigo.Commands.InviteNotesCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;


            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add Note", false, DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Remove Note", false, DiscordEmoji.FromUnicode("➖").ToComponent());

            var embed = new DiscordEmbedBuilder
            {
                Description = InviteNotesCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, "Invite Notes");

            if (!(ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Count > 19))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    AddButton,
                    RemoveButton,
                }).AddComponents(MessageComponents.CancelButton));
            }
            else
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { RemoveButton }).AddComponents(MessageComponents.CancelButton));
            }

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == AddButton.CustomId)
            {
                string? SelectedText = null;
                DiscordInvite SelectedInvite = null;

                while (true)
                {
                    var SelectTextButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Note", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                    var SelectInviteButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Invite", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Create Invite Note", (SelectedText.IsNullOrWhiteSpace() || SelectedInvite is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));


                    embed = new DiscordEmbedBuilder
                    {
                        Description = $"`Note  `: `{(SelectedText.IsNullOrWhiteSpace() ? "Not yet selected." : SelectedText).SanitizeForCode()}`\n" +
                                      $"`Invite`: `{(SelectedInvite is null ? $"Not yet selected." : $"{SelectedInvite.Code}")}`"
                    }.AsAwaitingInput(ctx, "Playlists");

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                        .AddComponents(new List<DiscordComponent> { SelectTextButton, SelectInviteButton, Finish })
                        .AddComponents(MessageComponents.CancelButton));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectTextButton.CustomId)
                    {
                        var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, new DiscordInteractionModalBuilder()
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", "New Note", "", 1, 128, true)), false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        SelectedText = ModalResult.Result.Interaction.GetModalValueByCustomId("Note").Truncate(128);
                    }
                    else if (Menu.GetCustomId() == SelectInviteButton.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        var invites = await ctx.Guild.GetInvitesAsync();

                        var SelectionResult = await PromptCustomSelection(invites.Where(x => !ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.ContainsKey(x.Code))
                            .Select(x => new DiscordSelectComponentOption(x.Code, x.Code, $"Uses: {x.Uses}; Creator: {x.Inviter.UsernameWithDiscriminator}")).ToList());

                        if (SelectionResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (SelectionResult.Cancelled)
                        {
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                        else if (SelectionResult.Errored)
                        {
                            throw SelectionResult.Exception;
                        }

                        SelectedInvite = invites.First(x => x.Code == SelectionResult.Result);
                    }
                    else if (Menu.GetCustomId() == Finish.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Add(SelectedInvite.Code, new InviteNotesDetails()
                        {
                            Invite = SelectedInvite.Code,
                            Moderator = ctx.User.Id,
                            Note = SelectedText
                        });

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                }
            }
            else if (e.GetCustomId() == RemoveButton.CustomId)
            {
                var SelectionResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Select(x => new DiscordSelectComponentOption(x.Key, x.Key, $"{x.Value.Note.TruncateWithIndication(50)}")).ToList());

                if (SelectionResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (SelectionResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (SelectionResult.Errored)
                {
                    throw SelectionResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Remove(SelectionResult.Result);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}
