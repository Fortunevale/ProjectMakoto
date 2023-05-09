// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class BanGuildCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong guild = (ulong)arguments["guild"];
            string reason = (string)arguments["reason"];

            if (reason.IsNullOrWhiteSpace())
                reason = "No reason provided.";

            if (ctx.Bot.bannedGuilds.ContainsKey(guild))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' is already banned from using the bot.`").AsError(ctx));
                return;
            }

            ctx.Bot.bannedGuilds.Add(guild, new() { Reason = reason, Moderator = ctx.User.Id });

            foreach (var b in ctx.Client.Guilds.Where(x => x.Key == guild))
            {
                _logger.LogInfo("Leaving guild '{guild}'..", b.Key);
                await b.Value.LeaveAsync();
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' was banned from using the bot.`").AsSuccess(ctx));
        });
    }
}