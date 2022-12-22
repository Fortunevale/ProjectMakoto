namespace ProjectIchigo.Commands;

internal class GlobalNotesCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx, true))
                return;

            DiscordUser victim = (DiscordUser)arguments["victim"];

            var ModeratorCache = new Dictionary<ulong, DiscordUser>();

            if (ctx.Bot.globalNotes.ContainsKey(victim.Id))
                foreach (var b in ctx.Bot.globalNotes[victim.Id])
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

            await RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"{victim.Mention} `has {(ctx.Bot.globalNotes.TryGetValue(victim.Id, out var list) ? list.Count : 0)} global notes.`")
                    .AddFields((list is not null ? list.Take(20).Select(x => new DiscordEmbedField("󠂪 󠂪", $"{x.Reason.FullSanitize()} - `{(ModeratorCache[x.Moderator] is null ? "Unknown#0000" : ModeratorCache[x.Moderator].UsernameWithDiscriminator)}` {x.Timestamp.ToTimestamp()}")) : new List<DiscordEmbedField>())))
                .AddComponents(new List<DiscordComponent> { AddButton, RemoveButton })
                .AddComponents(MessageComponents.CancelButton));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == AddButton.CustomId)
            {
                var ModalResult = await PromptModalWithRetry(Button.Result.Interaction,
                        new DiscordInteractionModalBuilder().AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "Note", "New Note", "", 1, 256, true)), false);

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

                if (!ctx.Bot.globalNotes.TryGetValue(victim.Id, out var user))
                {
                    ctx.Bot.globalNotes.Add(victim.Id, new());
                    user = ctx.Bot.globalNotes[victim.Id];
                }

                user.Add(new GlobalBanDetails { Moderator = ctx.User.Id, Reason = note });
                
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var SelectionResult = await PromptCustomSelection(ctx.Bot.globalNotes[victim.Id]
                    .Select(x => new DiscordStringSelectComponentOption(x.Reason.TruncateWithIndication(100), x.Timestamp.Ticks.ToString(), $"Added by {(ModeratorCache[x.Moderator] is null ? "Unknown#0000" : ModeratorCache[x.Moderator].UsernameWithDiscriminator)} {x.Timestamp.GetTimespanSince().GetHumanReadable()} ago")).ToList());

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

                ctx.Bot.globalNotes[victim.Id].Remove(ctx.Bot.globalNotes[victim.Id].First(x => x.Timestamp.Ticks.ToString() == SelectionResult.Result));
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}