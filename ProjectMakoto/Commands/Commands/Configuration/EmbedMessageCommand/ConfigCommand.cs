// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.EmbedMessageCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.EmbedMessages;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = EmbedMessageCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var ToggleMsg = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleMessageLinkButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ToggleGithub = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleGithubCodeButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleMsg,
                ToggleGithub
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleMsg.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding = !ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            if (e.GetCustomId() == ToggleGithub.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding = !ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding;

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