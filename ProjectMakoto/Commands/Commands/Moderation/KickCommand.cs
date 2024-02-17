// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class KickCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.KickMembers) && await this.CheckOwnPermissions(Permissions.KickMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["victim"];
            var reason = (string)arguments["reason"];

            var CommandKey = this.t.Commands.Moderation.Kick;

            DiscordMember bMember = null;

            try
            {
                bMember = await victim.ConvertToMember(ctx.Guild);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                this.SendNoMemberError();
                return;
            }
            catch (Exception)
            {
                throw;
            }

            var embed = new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Kicking, true, new TVar("Victim", victim.Mention)))
                .WithThumbnail(victim.AvatarUrl)
                .AsLoading(ctx);
            _ = await this.RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= bMember.GetRoleHighestPosition())
                    throw new Exception();

                var newReason = (reason.IsNullOrWhiteSpace() ? this.GetGuildString(this.t.Commands.Moderation.NoReason) : reason);
                await bMember.RemoveAsync(this.GetGuildString(CommandKey.AuditLog, new TVar("Reason", newReason)));

                embed = embed.WithDescription(this.GetString(CommandKey.Kicked, true, new TVar("Victim", victim.Mention), new TVar("Reason", newReason))).AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed = embed.WithDescription(this.GetString(CommandKey.Errored, true, new TVar("Victim", victim.Mention))).AsError(ctx);
            }

            _ = await this.RespondOrEdit(embed);
        });
    }
}