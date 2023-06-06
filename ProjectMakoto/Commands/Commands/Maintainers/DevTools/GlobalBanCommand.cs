// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal sealed class GlobalBanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            string reason = (string)arguments["reason"];

            if (reason.IsNullOrWhiteSpace())
            {
                _ = RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Please provide a reason for the global ban.`").AsError(ctx, "Global Ban"));
                return;
            }

            if (ctx.Bot.globalBans.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Updating Global Ban Entry for '{victim.GetUsernameWithIdentifier()}'..`").AsLoading(ctx, "Global Ban"));
                ctx.Bot.globalBans[victim.Id] = new() { Reason = reason, Moderator = ctx.User.Id };
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Global Ban Entry for '{victim.GetUsernameWithIdentifier()}' updated.`").AsSuccess(ctx, "Global Ban"));

                var announceChannel1 = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                await announceChannel1.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.CurrentUser.GetUsername(),
                        IconUrl = AuditLogIcons.UserUpdated
                    },
                    Description = $"The global ban entry of {victim.Mention} `{victim.GetUsernameWithIdentifier()}` (`{victim.Id}`) was updated.\n\n" +
                                  $"Reason: `{reason.SanitizeForCode()}`\n" +
                                  $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                    Color = EmbedColors.Warning,
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`Global banning '{victim.GetUsernameWithIdentifier()}' ({victim.Id})`.."
            }.AsLoading(ctx, "Global Ban");

            var msg = RespondOrEdit(embed);

            if (ctx.Bot.status.TeamMembers.Contains(victim.Id))
            {
                embed.Description = $"`'{victim.GetUsernameWithIdentifier()}' is registered in the staff team.`";
                msg = RespondOrEdit(embed.AsError(ctx, "Global Ban"));
                return;
            }

            ctx.Bot.globalBans.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
            {
                if (!ctx.Bot.guilds.ContainsKey(b.Key))
                    ctx.Bot.guilds.Add(b.Key, new Guild(b.Key, ctx.Bot));

                if (ctx.Bot.guilds[b.Key].Join.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occurred while trying to ban user from {guild}", ex, b.Key);
                        Failed++;
                    }
                }
            }

            embed.Description = $"`Banned '{victim.GetUsernameWithIdentifier()}' ({victim.Id}) from {Success}/{Success + Failed} guilds.`";
            msg = RespondOrEdit(embed.AsSuccess(ctx, "Global Ban"));


            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.GetUsername(),
                    IconUrl = AuditLogIcons.UserBanned
                },
                Description = $"{victim.Mention} `{victim.GetUsernameWithIdentifier()}` (`{victim.Id}`) was added to the global ban list.\n\n" +
                              $"Reason: `{reason.SanitizeForCode()}`\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Error,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
