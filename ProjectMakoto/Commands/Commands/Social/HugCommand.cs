// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class HugCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            if (ctx.DbUser.BlockedUsers.Contains(user.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Social.BlockedVictim, true, new TVar("User", user.Mention))).AsError(ctx));
                return;
            }

            if (ctx.Bot.Users[user.Id].BlockedUsers.Contains(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Social.BlockedByVictim, true, new TVar("User", user.Mention))).AsError(ctx));
                return;
            }

            string[] PositiveEmojis = { "♥", ctx.Bot.status.LoadedConfig.Emojis.Hug };
            string[] NegativeEmojis = { "😢", "😓" };

            string[] phrases = this.t.Commands.Social.Hug.Other.Get(ctx.DbGuild);
            string[] self_phrases = this.t.Commands.Social.Hug.Self.Get(ctx.DbGuild);

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Build(
                    new TVar("User1", ctx.Member.DisplayName),
                    new TVar("Emoji", NegativeEmojis.SelectRandom())),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "hug");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = phrases.SelectRandom().Build(
                    new TVar("User1", ctx.User.Mention, false),
                    new TVar("User2", user.Mention, false),
                    new TVar("Emoji", PositiveEmojis.SelectRandom(), false))
                    .Bold(),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}