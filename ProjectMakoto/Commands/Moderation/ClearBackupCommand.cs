// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Commands;

internal sealed class ClearBackupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ManageRoles));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["victim"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var CommandKey = this.t.Commands.Moderation.ClearBackup;

            if ((await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.IsOnServer, true, new TVar("Victim", victim.Mention)))
                    .WithThumbnail(victim.AvatarUrl)
                    .AsError(ctx));

                return;
            }

            ctx.DbGuild.Members[victim.Id].MemberRoles = Array.Empty<MemberRole>();
            ctx.DbGuild.Members[victim.Id].SavedNickname = "";

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Deleted, true, new TVar("Victim", victim.Mention)))
                .WithThumbnail(victim.AvatarUrl)
                .AsSuccess(ctx));
        });
    }
}