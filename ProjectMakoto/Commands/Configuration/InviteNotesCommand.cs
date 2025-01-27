// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Configuration;

internal sealed class InviteNotesCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.InviteNotes;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                return ctx.DbGuild.InviteNotes.Notes.Length == 0
                    ? ctx.BaseCommand.GetString(this.t.Commands.Config.InviteNotes.NoNotesDefined, true)
                    : $"{string.Join('\n', ctx.DbGuild.InviteNotes.Notes.Select(x => $"> `{x.Invite}`\n{x.Note.FullSanitize()}"))}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.AddNoteButton), false, DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.RemoveNoteButton), false, DiscordEmoji.FromUnicode("➖").ToComponent());

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, this.GetString(CommandKey.Title));

            if (!(ctx.DbGuild.InviteNotes.Notes.Length > 19))
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                .AddComponents(new List<DiscordComponent>
                {
                    AddButton,
                    RemoveButton,
                }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));
            }
            else
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { RemoveButton }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));
            }

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == AddButton.CustomId)
            {
                string? SelectedText = null;
                DiscordInvite SelectedInvite = null;

                while (true)
                {
                    var SelectTextButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SetNoteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                    var SelectInviteButton = new DiscordButtonComponent((SelectedText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectInviteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(CommandKey.CreateButton), (SelectedText.IsNullOrWhiteSpace() || SelectedInvite is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Note, CommandKey.Invite);

                    embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{this.GetString(CommandKey.Note).PadRight(pad)}`: `{(SelectedText.IsNullOrWhiteSpace() ? this.GetString(this.t.Common.NotSelected) : SelectedText).SanitizeForCode()}`\n" +
                                      $"`{this.GetString(CommandKey.Invite).PadRight(pad)}`: `{(SelectedInvite is null ? this.GetString(this.t.Common.NotSelected) : $"{SelectedInvite.Code}")}`"
                    }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                        .AddComponents(new List<DiscordComponent> { SelectTextButton, SelectInviteButton, Finish })
                        .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        this.ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectTextButton.CustomId)
                    {
                        var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, new DiscordInteractionModalBuilder()
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", this.GetString(CommandKey.Note), "", 1, 128, true)), false);

                        if (ModalResult.TimedOut)
                        {
                            this.ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            await this.ExecuteCommand(ctx, arguments);
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

                        var SelectionResult = await this.PromptCustomSelection(invites.Where(x => !ctx.DbGuild.InviteNotes.Notes.Any(a => a.Invite == x.Code))
                            .Select(x => new DiscordStringSelectComponentOption(x.Code, x.Code, this.GetString(CommandKey.InviteDescription, new TVar("Uses", x.Uses), new TVar("Creator", x.Inviter.GetUsernameWithIdentifier())))).ToList());

                        if (SelectionResult.TimedOut)
                        {
                            this.ModifyToTimedOut(true);
                            return;
                        }
                        else if (SelectionResult.Cancelled)
                        {
                            await this.ExecuteCommand(ctx, arguments);
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

                        ctx.DbGuild.InviteNotes.Notes = ctx.DbGuild.InviteNotes.Notes.Add(new()
                        {
                            Invite = SelectedInvite.Code,
                            Moderator = ctx.User.Id,
                            Note = SelectedText
                        });

                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }
                }
            }
            else if (e.GetCustomId() == RemoveButton.CustomId)
            {
                var SelectionResult = await this.PromptCustomSelection(ctx.DbGuild.InviteNotes.Notes.Select(x => new DiscordStringSelectComponentOption(x.Invite, x.Invite, $"{x.Note.TruncateWithIndication(50)}")).ToList());

                if (SelectionResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (SelectionResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (SelectionResult.Errored)
                {
                    throw SelectionResult.Exception;
                }

                ctx.DbGuild.InviteNotes.Notes = ctx.DbGuild.InviteNotes.Notes
                    .Remove(x => x.Invite, ctx.DbGuild.InviteNotes.Notes.First(x => x.Invite == SelectionResult.Result));

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}
