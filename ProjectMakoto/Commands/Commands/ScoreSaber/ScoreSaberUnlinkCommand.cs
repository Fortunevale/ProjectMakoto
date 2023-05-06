// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class ScoreSaberUnlinkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            if (ctx.Bot.users[ctx.User.Id].ScoreSaber.Id != 0)
            {
                ctx.Bot.users[ctx.User.Id].ScoreSaber.Id = 0;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.ScoreSaber.Unlink.Unlinked, true)
                }.AsSuccess(ctx, "Score Saber")));
            }
            else
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.ScoreSaber.Unlink.NoLink, true)
                }.AsError(ctx, "Score Saber")));
            }
        });
    }
}