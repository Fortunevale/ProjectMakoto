// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class RemoveTimeoutCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ModerateMembers) && await this.CheckOwnPermissions(Permissions.ModerateMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMember victim;

            try
            {
                victim = await ((DiscordUser)arguments["user"]).ConvertToMember(ctx.Guild);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                this.SendNoMemberError();
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            var CommandKey = this.t.Commands.Moderation.RemoveTimeout;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Removing, true, new TVar("Victim", victim.Mention)))
                .AsLoading(ctx));

            try
            {
                await victim.RemoveTimeoutAsync();
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.Removed, true, new TVar("Victim", victim.Mention)))
                    .AsSuccess(ctx));
            }
            catch (Exception)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.Failed, true, new TVar("Victim", victim.Mention)))
                    .AsError(ctx));
            }
        });
    }
}