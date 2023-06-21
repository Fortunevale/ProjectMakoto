// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.PrefixCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = PrefixCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(this.t.Commands.Config.PrefixConfigCommand.Title));

            var TogglePrefixCommands = new DiscordButtonComponent((ctx.DbGuild.PrefixSettings.PrefixDisabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(this.t.Commands.Config.PrefixConfigCommand.TogglePrefixCommands), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⌨")));
            var ChangePrefix = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(this.t.Commands.Config.PrefixConfigCommand.ChangePrefix), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗝")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { TogglePrefixCommands },
                { ChangePrefix },
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == TogglePrefixCommands.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbGuild.PrefixSettings.PrefixDisabled = !ctx.DbGuild.PrefixSettings.PrefixDisabled;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangePrefix.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Config.PrefixConfigCommand.NewPrefixModalTitle), Guid.NewGuid().ToString());

                modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "newPrefix", GetString(this.t.Commands.Config.PrefixConfigCommand.NewPrefix), ctx.Bot.Prefix, 1, 4, true, ctx.DbGuild.PrefixSettings.Prefix));

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

                ctx.DbGuild.PrefixSettings.Prefix = newPrefix;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}