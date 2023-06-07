// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class UnbanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            var CommandKey = this.t.Commands.Moderation.Unban;

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Removing, true, new TVar("Victim", victim.Mention)))
                .AsLoading(ctx));

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.Removed, true, new TVar("Victim", victim.Mention)))
                    .AsSuccess(ctx));
            }
            catch (Exception)
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.Failed, true, new TVar("Victim", victim.Mention)))
                    .AsError(ctx));
            }
        });
    }
}