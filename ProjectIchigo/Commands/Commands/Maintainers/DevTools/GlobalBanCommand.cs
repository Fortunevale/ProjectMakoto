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

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Global banning '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            };

            var msg = RespondOrEdit(embed);

            if (ctx.Bot._status.TeamMembers.Contains(victim.Id))
            {
                embed.Color = EmbedColors.Error;
                embed.Description = $"`'{victim.UsernameWithDiscriminator}' is registered in the staff team.`";
                msg = RespondOrEdit(embed);
                return;
            }

            ctx.Bot._globalBans.List.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
            {
                if (!ctx.Bot._guilds.ContainsKey(b.Key))
                    ctx.Bot._guilds.Add(b.Key, new Guild(b.Key));

                if (ctx.Bot._guilds[b.Key].JoinSettings.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occured while trying to ban user from {b.Key}", ex);
                        Failed++;
                    }
                }
            }

            embed.Color = EmbedColors.Success;
            embed.Description = $"`Banned '{victim.UsernameWithDiscriminator}' ({victim.Id}) from {Success}/{Success + Failed} guilds.`";
            msg = RespondOrEdit(embed);


            var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.GlobalBanAnnouncementsChannelId);
            await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.CurrentUser.Username,
                    IconUrl = Resources.AuditLogIcons.UserBanned
                },
                Description = $"{victim.Mention} `{victim.UsernameWithDiscriminator}` (`{victim.Id}`) was added to the global ban list.\n\n" +
                              $"Reason: {reason.Sanitize()}\n" +
                              $"Moderator: {ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator}` (`{ctx.User.Id}`)",
                Color = EmbedColors.Error,
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
