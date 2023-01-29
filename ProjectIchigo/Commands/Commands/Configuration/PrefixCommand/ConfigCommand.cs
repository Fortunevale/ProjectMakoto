namespace ProjectIchigo.Commands.PrefixCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = PrefixCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(t.Commands.PrefixConfigCommand.Title));

            var TogglePrefixCommands = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PrefixDisabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(t.Commands.PrefixConfigCommand.TogglePrefixCommands), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⌨")));
            var ChangePrefix = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(t.Commands.PrefixConfigCommand.ChangePrefix), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗝")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { TogglePrefixCommands },
                { ChangePrefix },
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == TogglePrefixCommands.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PrefixDisabled = !ctx.Bot.guilds[ctx.Guild.Id].PrefixDisabled;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangePrefix.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder(GetString(t.Commands.PrefixConfigCommand.NewPrefixModalTitle), Guid.NewGuid().ToString());

                modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "newPrefix", GetString(t.Commands.PrefixConfigCommand.NewPrefix), ";;", 1, 4, true, ctx.DbGuild.Prefix));

                var ModalResult = await PromptModalWithRetry(Button.Result.Interaction, modal, false);

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

                var newPrefix = ModalResult.Result.Interaction.GetModalValueByCustomId("newPrefix");

                if (newPrefix.Length is > 4 or < 1)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                ctx.Bot.guilds[ctx.Guild.Id].Prefix = newPrefix;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}