// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class SoftBanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            int deleteMessageDays = (int)arguments["days"] > 7 ? 7 : ((int)arguments["days"] < 0 ? 0 : (int)arguments["days"]);
            string reason = (string)arguments["reason"];

            var CommandKey = t.Commands.Moderation.Softban;

            DiscordMember bMember = null;

            try
            {
                bMember = await victim.ConvertToMember(ctx.Guild);
            }
            catch { }

            var embed = new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Banning, true, new TVar("Victim", victim.Mention)))
                .WithThumbnail(victim.AvatarUrl)
                .AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= bMember.GetRoleHighestPosition())
                    throw new Exception();

                var newReason = (reason.IsNullOrWhiteSpace() ? GetGuildString(t.Commands.Moderation.NoReason) : reason);
                await ctx.Guild.BanMemberAsync(victim.Id, deleteMessageDays, GetGuildString(CommandKey.AuditLog, new TVar("Reason", newReason)));
                await ctx.Guild.UnbanMemberAsync(victim, GetGuildString(CommandKey.AuditLog, new TVar("Reason", newReason)));

                embed = embed.WithDescription(GetString(CommandKey.Banned, true, new TVar("Victim", victim.Mention), new TVar("Reason", newReason))).AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed = embed.WithDescription(GetString(CommandKey.Errored, true, new TVar("Victim", victim.Mention))).AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}
