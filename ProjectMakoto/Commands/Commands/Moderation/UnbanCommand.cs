// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class UnbanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Unbanning {victim.GetUsername()} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                embed.Description = $"<@{victim.Id}> `{victim.GetUsername()}` was unbanned.";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"`{victim.GetUsername()} ({victim.Id}) couldn't be unbanned.`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}