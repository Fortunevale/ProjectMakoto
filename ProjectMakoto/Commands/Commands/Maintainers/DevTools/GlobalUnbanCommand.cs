// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class GlobalUnbanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            bool UnbanFromGuilds = (bool)arguments["UnbanFromGuilds"];

            if (!ctx.Bot.globalBans.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`'{victim.GetUsernameWithIdentifier()}' is not global banned.`"
                }.AsError(ctx, "Global Ban"));
                return;
            }

            ctx.Bot.globalBans.Remove(victim.Id);
            await ctx.Bot.databaseClient._helper.DeleteRow(ctx.Bot.databaseClient.mainDatabaseConnection, "globalbans", "id", $"{victim.Id}");

            int Success = 0;
            int Failed = 0;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Removing global ban for '{victim.GetUsernameWithIdentifier()}' ({victim.Id})`.."
            }.AsLoading(ctx, "Global Ban"));

            if (UnbanFromGuilds)
                foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
                {
                    if (!ctx.Bot.guilds.ContainsKey(b.Key))
                        ctx.Bot.guilds.Add(b.Key, new Guild(b.Key, ctx.Bot));

                    if (ctx.Bot.guilds[b.Key].Join.AutoBanGlobalBans)
                    {
                        try
                        {
                            var Ban = await b.Value.GetBanAsync(victim);

                            if (Ban.Reason.StartsWith("Globalban: "))
                                await b.Value.UnbanMemberAsync(victim, $"Globalban removed.");

                            Success++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Exception occurred while trying to unban user from {guild}", ex, b.Key);
                            Failed++;
                        }
                    }
                }

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Removed '{victim.GetUsernameWithIdentifier()}' from global bans.`"
            }.AsSuccess(ctx, "Global Ban"));

            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.GetUsername(),
                    IconUrl = AuditLogIcons.UserBanRemoved
                },
                Description = $"{victim.Mention} `{victim.GetUsernameWithIdentifier()}` (`{victim.Id}`) was removed from the global ban list.\n\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Success,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
