// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class BanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            int deleteMessageDays = (int)arguments["days"];
            string reason = (string)arguments["reason"];

            DiscordMember bMember = null;

            try
            {
                bMember = await victim.ConvertToMember(ctx.Guild);
            }
            catch { }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Banning {victim.GetUsername()} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                }
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= (bMember?.GetRoleHighestPosition() ?? -1))
                    throw new Exception();

                await ctx.Guild.BanMemberAsync(victim.Id, deleteMessageDays, $"{ctx.User.GetUsername()} banned user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");

                embed.Description = $"{victim.Mention} `was banned for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCode()}' by` {ctx.User.Mention}`.`";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"{victim.Mention} `could not be banned.`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}