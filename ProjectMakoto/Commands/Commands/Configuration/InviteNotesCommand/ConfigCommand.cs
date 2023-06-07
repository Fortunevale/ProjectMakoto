// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.InviteNotesCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.InviteNotes;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.AddNoteButton), false, DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.RemoveNoteButton), false, DiscordEmoji.FromUnicode("➖").ToComponent());

            var embed = new DiscordEmbedBuilder
            {
                Description = InviteNotesCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, GetString(CommandKey.Title));

            if (!(ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Count > 19))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    AddButton,
                    RemoveButton,
                }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));
            }
            else
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { RemoveButton }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));
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
                    var SelectTextButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SetNoteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                    var SelectInviteButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectInviteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(CommandKey.CreateButton), (SelectedText.IsNullOrWhiteSpace() || SelectedInvite is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Note, CommandKey.Invite);

                    embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(CommandKey.Note).PadRight(pad)}`: `{(SelectedText.IsNullOrWhiteSpace() ? GetString(this.t.Common.NotSelected) : SelectedText).SanitizeForCode()}`\n" +
                                      $"`{GetString(CommandKey.Invite).PadRight(pad)}`: `{(SelectedInvite is null ? GetString(this.t.Common.NotSelected) : $"{SelectedInvite.Code}")}`"
                    }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                        .AddComponents(new List<DiscordComponent> { SelectTextButton, SelectInviteButton, Finish })
                        .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectTextButton.CustomId)
                    {
                        var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, new DiscordInteractionModalBuilder()
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", GetString(CommandKey.Note), "", 1, 128, true)), false);

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
                            .Select(x => new DiscordStringSelectComponentOption(x.Code, x.Code, GetString(CommandKey.InviteDescription, new TVar("Uses", x.Uses), new TVar("Creator", x.Inviter.GetUsernameWithIdentifier())))).ToList());

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
                var SelectionResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Select(x => new DiscordStringSelectComponentOption(x.Key, x.Key, $"{x.Value.Note.TruncateWithIndication(50)}")).ToList());

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
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}
