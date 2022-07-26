namespace ProjectIchigo.Commands;

internal class GlobalUnbanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            bool UnbanFromGuilds = (bool)arguments["UnbanFromGuilds"];

            if (!ctx.Bot._globalBans.List.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                    Color = EmbedColors.Error,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`'{victim.UsernameWithDiscriminator}' is not global banned.`"
                });
                return;
            }

            ctx.Bot._globalBans.List.Remove(victim.Id);
            await ctx.Bot._databaseClient._helper.DeleteRow(ctx.Bot._databaseClient.mainDatabaseConnection, "globalbans", "id", $"{victim.Id}");

            int Success = 0;
            int Failed = 0;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Removing global ban for '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            });

            if (UnbanFromGuilds)
                foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
                {
                    if (!ctx.Bot._guilds.ContainsKey(b.Key))
                        ctx.Bot._guilds.Add(b.Key, new Guild(b.Key));

                    if (ctx.Bot._guilds[b.Key].JoinSettings.AutoBanGlobalBans)
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
                            _logger.LogError($"Exception occured while trying to unban user from {b.Key}", ex);
                            Failed++;
                        }
                    }
                }

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Removed '{victim.UsernameWithDiscriminator}' from global bans.`"
            });

            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.GlobalBanAnnouncementsChannelId);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.Username,
                    IconUrl = Resources.AuditLogIcons.UserBanRemoved
                },
                Description = $"{victim.Mention} `{victim.UsernameWithDiscriminator}` (`{victim.Id}`) was removed from the global ban list.\n\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Success,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
