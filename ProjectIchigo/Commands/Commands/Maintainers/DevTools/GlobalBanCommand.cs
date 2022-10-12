namespace ProjectIchigo.Commands;
internal class GlobalBanCommand : BaseCommand
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
                _ = RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Please provide a reason for the global ban.`").SetError(ctx, "Global Ban"));
                return;
            }

            if (ctx.Bot.globalBans.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Updating Global Ban Entry for '{victim.UsernameWithDiscriminator}'..`").SetLoading(ctx, "Global Ban"));
                ctx.Bot.globalBans[victim.Id] = new() { Reason = reason, Moderator = ctx.User.Id };
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Global Ban Entry for '{victim.UsernameWithDiscriminator}' updated.`").SetSuccess(ctx, "Global Ban"));

                var announceChannel1 = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                await announceChannel1.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.CurrentUser.Username,
                        IconUrl = AuditLogIcons.UserUpdated
                    },
                    Description = $"The global ban entry of {victim.Mention} `{victim.UsernameWithDiscriminator}` (`{victim.Id}`) was updated.\n\n" +
                                  $"Reason: `{reason.SanitizeForCode()}`\n" +
                                  $"Moderator: {ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator}` (`{ctx.User.Id}`)",
                    Color = EmbedColors.Warning,
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`Global banning '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            }.SetLoading(ctx, "Global Ban");

            var msg = RespondOrEdit(embed);

            if (ctx.Bot.status.TeamMembers.Contains(victim.Id))
            {
                embed.Description = $"`'{victim.UsernameWithDiscriminator}' is registered in the staff team.`";
                msg = RespondOrEdit(embed.SetError(ctx, "Global Ban"));
                return;
            }

            ctx.Bot.globalBans.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
            {
                if (!ctx.Bot.guilds.ContainsKey(b.Key))
                    ctx.Bot.guilds.Add(b.Key, new Guild(b.Key, ctx.Bot));

                if (ctx.Bot.guilds[b.Key].JoinSettings.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occurred while trying to ban user from {b.Key}", ex);
                        Failed++;
                    }
                }
            }

            embed.Description = $"`Banned '{victim.UsernameWithDiscriminator}' ({victim.Id}) from {Success}/{Success + Failed} guilds.`";
            msg = RespondOrEdit(embed.SetSuccess(ctx, "Global Ban"));


            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.Username,
                    IconUrl = AuditLogIcons.UserBanned
                },
                Description = $"{victim.Mention} `{victim.UsernameWithDiscriminator}` (`{victim.Id}`) was added to the global ban list.\n\n" +
                              $"Reason: `{reason.SanitizeForCode()}`\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Error,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
