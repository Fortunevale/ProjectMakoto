// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class GlobalNotesCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx, true))
                return;

            var victim = (DiscordUser)arguments["victim"];

            var ModeratorCache = new Dictionary<ulong, DiscordUser>();

            if (ctx.Bot.globalNotes.TryGetValue(victim.Id, out var globalNotes))
                foreach (var b in globalNotes.Notes)
                {
                    if (ModeratorCache.ContainsKey(b.Moderator))
                        continue;

                    try
                    {
                        ModeratorCache.Add(b.Moderator, await ctx.Client.GetUserAsync(b.Moderator));
                    }
                    catch (Exception)
                    {
                        ModeratorCache.Add(b.Moderator, null);
                    }
                }

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add Notes", false, DiscordEmoji.FromUnicode("➕").ToComponent());
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Remove Notes", (!ctx.Bot.globalNotes.ContainsKey(victim.Id)), DiscordEmoji.FromUnicode("➖").ToComponent());

            _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"{victim.Mention} `has {(ctx.Bot.globalNotes.TryGetValue(victim.Id, out var noteObj) ? noteObj.Notes.Length : 0)} global notes.`")
                    .AddFields((noteObj is not null ? noteObj.Notes.Take(20).Select(x => new DiscordEmbedField("󠂪 󠂪", $"{x.Reason.FullSanitize()} - `{(ModeratorCache[x.Moderator] is null ? "Unknown#0000" : ModeratorCache[x.Moderator].GetUsernameWithIdentifier())}` {x.Timestamp.ToTimestamp()}")) : new List<DiscordEmbedField>())))
                .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == AddButton.CustomId)
            {
                var ModalResult = await this.PromptModalWithRetry(Button.Result.Interaction,
                        new DiscordInteractionModalBuilder().AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", "New Note", "", 1, 256, true)), false);

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

                var note = ModalResult.Result.Interaction.GetModalValueByCustomId("Note");

                ctx.Bot.globalNotes[victim.Id].Notes = ctx.Bot.globalNotes[victim.Id].Notes.Add(new GlobalNote.Note() { Moderator = ctx.User.Id, Reason = note });

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var SelectionResult = await this.PromptCustomSelection(ctx.Bot.globalNotes[victim.Id].Notes
                    .Select(x => new DiscordStringSelectComponentOption(x.Reason.TruncateWithIndication(100), x.Timestamp.Ticks.ToString(), $"Added by {(ModeratorCache[x.Moderator] is null ? "Unknown#0000" : ModeratorCache[x.Moderator].GetUsernameWithIdentifier())} {x.Timestamp.GetTimespanSince().GetHumanReadable()} ago")).ToList());

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

                ctx.Bot.globalNotes[victim.Id].Notes = ctx.Bot.globalNotes[victim.Id].Notes.Remove(x => x.UUID, ctx.Bot.globalNotes[victim.Id].Notes.First(x => x.Timestamp.Ticks.ToString() == SelectionResult.Result));
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}