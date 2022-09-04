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

            if (!ctx.Bot.globalBans.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`'{victim.UsernameWithDiscriminator}' is not global banned.`"
                }.SetError(ctx, "Global Ban"));
                return;
            }

            ctx.Bot.globalBans.Remove(victim.Id);
            await ctx.Bot.databaseClient._helper.DeleteRow(ctx.Bot.databaseClient.mainDatabaseConnection, "globalbans", "id", $"{victim.Id}");

            int Success = 0;
            int Failed = 0;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Removing global ban for '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            }.SetLoading(ctx, "Global Ban"));

            if (UnbanFromGuilds)
                foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
                {
                    if (!ctx.Bot.guilds.ContainsKey(b.Key))
                        ctx.Bot.guilds.Add(b.Key, new Guild(b.Key));

                    if (ctx.Bot.guilds[b.Key].JoinSettings.AutoBanGlobalBans)
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
                            _logger.LogError($"Exception occurred while trying to unban user from {b.Key}", ex);
                            Failed++;
                        }
                    }
                }

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Removed '{victim.UsernameWithDiscriminator}' from global bans.`"
            }.SetSuccess(ctx, "Global Ban"));

            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.Username,
                    IconUrl = AuditLogIcons.UserBanRemoved
                },
                Description = $"{victim.Mention} `{victim.UsernameWithDiscriminator}` (`{victim.Id}`) was removed from the global ban list.\n\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Success,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
