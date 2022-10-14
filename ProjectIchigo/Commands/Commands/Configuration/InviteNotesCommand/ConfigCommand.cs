namespace ProjectIchigo.Commands.Commands.Configuration.InviteNotesCommand
{
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
                }.SetInfo(ctx, "Invite Notes");

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

                if (e.Result.Interaction.Data.CustomId == AddButton.CustomId)
                {
                    var ModalResult = await PromptModalWithRetry(e.Result.Interaction, new DiscordInteractionModalBuilder().
                        AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", "New Note", "", 1, 128, true)), false);

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

                    var note = ModalResult.Result.Interaction.GetModalValueByCustomId("Note");

                    var SelectionResult = await PromptCustomSelection(ctx.Guild.GetInvitesAsync().Result.Where(x => !ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.ContainsKey(x.Code))
                    .Select(x => new DiscordSelectComponentOption(x.Code, x.CreatedAt.Ticks.ToString(), $"Uses: {x.Uses}; Creator: {x.Inviter.UsernameWithDiscriminator}")).ToList());

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

                    if (!ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.TryGetValue(SelectionResult.Result, out var details))
                    {
                        ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Add(SelectionResult.Result, new InviteNotesDetails()
                        { Invite = SelectionResult.Result, Moderator = ctx.Guild.GetInvite(SelectionResult.Result).Inviter.Id, Note = note });
                    }

                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                else if (e.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
                {
                    var SelectionResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Select(x => new DiscordSelectComponentOption(x.Key, $"Note: {x.Value.Note}")).ToList());

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

                else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                {
                    DeleteOrInvalidate();
                    return;
                }
            });
        }
    }
}
