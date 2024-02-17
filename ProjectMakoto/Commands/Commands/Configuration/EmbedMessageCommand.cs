// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class EmbedMessageCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.EmbedMessages;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.EmbedMessages;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.EmbedGithubCode, CommandKey.EmbedMessageLinks);

                return $"{"💬".UnicodeToEmoji()} `{CommandKey.EmbedMessageLinks.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.EmbedMessage.UseEmbedding.ToEmote(ctx.Bot)}\n" +
                       $"{"🤖".UnicodeToEmoji()} `{CommandKey.EmbedGithubCode.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.EmbedMessage.UseGithubEmbedding.ToEmote(ctx.Bot)}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var ToggleMsg = new DiscordButtonComponent((ctx.DbGuild.EmbedMessage.UseEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleMessageLinkButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ToggleGithub = new DiscordButtonComponent((ctx.DbGuild.EmbedMessage.UseGithubEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleGithubCodeButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleMsg,
                ToggleGithub
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleMsg.CustomId)
            {
                ctx.DbGuild.EmbedMessage.UseEmbedding = !ctx.DbGuild.EmbedMessage.UseEmbedding;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            if (e.GetCustomId() == ToggleGithub.CustomId)
            {
                ctx.DbGuild.EmbedMessage.UseGithubEmbedding = !ctx.DbGuild.EmbedMessage.UseGithubEmbedding;

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