// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class UnblockUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Utility.UnblockUser;

            var victim = (DiscordUser)arguments["victim"];

            if (!ctx.DbUser.BlockedUsers.Contains(victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.NotBlocked, true)).AsError(ctx));
                return;
            }

            ctx.DbUser.BlockedUsers = ctx.DbUser.BlockedUsers.Remove(x => x.ToString(), victim.Id);

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.Unblocked, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
        });
    }
}