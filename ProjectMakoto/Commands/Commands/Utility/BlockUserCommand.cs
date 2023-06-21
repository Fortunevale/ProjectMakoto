// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class BlockUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Utility.BlockUser;

            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (ctx.DbUser.BlockedUsers.Contains(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(CommandKey.AlreadyBlocked, true)).AsError(ctx));
                return;
            }

            if (victim.Id == ctx.Client.CurrentUser.Id)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(CommandKey.CannotBlock, true)).AsError(ctx));
                return;
            }

            ctx.DbUser.BlockedUsers.Add(victim.Id);

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(CommandKey.Blocked, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
        });
    }
}