// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class UnbanGuildCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var guild = (ulong)arguments["guild"];

            if (!ctx.Bot.bannedGuilds.ContainsKey(guild))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' is not banned from using the bot.`").AsError(ctx));
                return;
            }

            _ = ctx.Bot.bannedGuilds.Remove(guild);
            await ctx.Bot.DatabaseClient._helper.DeleteRow(ctx.Bot.DatabaseClient.mainDatabaseConnection, "banned_guilds", "id", $"{guild}");
            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' was unbanned from using the bot.`").AsSuccess(ctx));
        });
    }
}