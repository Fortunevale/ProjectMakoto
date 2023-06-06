// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ClearBackupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageRoles));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

        var CommandKey = t.Commands.Moderation.ClearBackup;

            if ((await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.IsOnServer, true, new TVar("Victim", victim.Mention)))
                    .WithThumbnail(victim.AvatarUrl)
                    .AsError(ctx));

                return;
            }

            if (!ctx.Bot.guilds[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                ctx.Bot.guilds[ctx.Guild.Id].Members.Add(victim.Id, new(ctx.Bot.guilds[ctx.Guild.Id], victim.Id));

            ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Clear();
            ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].SavedNickname = "";

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Deleted, true, new TVar("Victim", victim.Mention)))
                .WithThumbnail(victim.AvatarUrl)
                .AsSuccess(ctx));
        });
    }
}