// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class RemoveTimeoutCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ModerateMembers) && await CheckOwnPermissions(Permissions.ModerateMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMember victim;

            try
            {
                victim = await ((DiscordUser)arguments["victim"]).ConvertToMember(ctx.Guild);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                SendNoMemberError();
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            var CommandKey = this.t.Commands.Moderation.RemoveTimeout;

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Removing, true, new TVar("Victim", victim.Mention)))
                .AsLoading(ctx));

            try
            {
                await victim.RemoveTimeoutAsync();
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