// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class UnbanUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (!ctx.Bot.bannedUsers.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsername()}' is not banned from using the bot.`").AsError(ctx));
                return;
            }

            ctx.Bot.bannedUsers.Remove(victim.Id);
            await ctx.Bot.databaseClient._helper.DeleteRow(ctx.Bot.databaseClient.mainDatabaseConnection, "banned_users", "id", $"{victim.Id}");
            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsername()}' was unbanned from using the bot.`").AsSuccess(ctx));
        });
    }
}