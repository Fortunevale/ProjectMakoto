// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class BanUserCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["victim"];
            var reason = (string)arguments["reason"];

            if (reason.IsNullOrWhiteSpace())
                reason = "No reason provided.";

            if (ctx.Bot.status.TeamMembers.Contains(victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsernameWithIdentifier()}' is registered in the staff team.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.bannedUsers.ContainsKey(victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsernameWithIdentifier()}' is already banned from using the bot.`").AsError(ctx));
                return;
            }

            ctx.Bot.bannedUsers.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            foreach (var b in ctx.Client.Guilds.Where(x => x.Value.OwnerId == victim.Id))
            {
                _logger.LogInfo("Leaving guild '{guild}'..", b.Key);
                await b.Value.LeaveAsync();
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsernameWithIdentifier()}' was banned from using the bot.`").AsSuccess(ctx));
        });
    }
}