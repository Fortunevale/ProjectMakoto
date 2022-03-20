namespace Project_Ichigo.Commands.Admin;
internal class Maintainers : BaseCommandModule
{
    public Status _status { private get; set; }
    public ServerInfo _guilds { private get; set; }
    public GlobalBans _globalBans { private get; set; }
    public DatabaseClient _databaseHelper { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }

    [Command("throw"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task Throw(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_status))
                return;

            throw new NotImplementedException();
        }).Add(_watcher, ctx);
    }

    [Command("globalban"), Aliases("global-ban"),
    CommandModule("maintainence"),
    Description("Bans a user from all servers opted into globalbans")]
    public async Task Globalban(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")]string reason = "-")
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_status))
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = ColorHelper.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Global banning '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            };

            var msg = await ctx.Channel.SendMessageAsync(embed);

            _globalBans.Users.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds)
            {
                if (!_guilds.Servers.ContainsKey(b.Key))
                    _guilds.Servers.Add(b.Key, new ServerInfo.ServerSettings());

                if (_guilds.Servers[b.Key].JoinSettings.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Exception occured while trying to ban user from {b.Key}: {ex}");
                        Failed++;
                    }
                }
            }

            embed.Color = ColorHelper.Info;
            embed.Description = $"`Banned '{victim.UsernameWithDiscriminator}' from {Success} guilds.`";
            _ = msg.ModifyAsync(embed.Build());
        }).Add(_watcher, ctx);
    }

    [Command("globalunban"), Aliases("global-unban"),
    CommandModule("maintainence"),
    Description("Removes a user from global bans (doesn't unban user from all servers)")]
    public async Task Globalunban(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_status))
                return;

            _globalBans.Users.Remove(victim.Id);
            await _databaseHelper._helper.DeleteRow(_databaseHelper.mainDatabaseConnection, "globalbans", "id", $"{victim.Id}");

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Removed '{victim.UsernameWithDiscriminator}' from global bans.`"
            });
        }).Add(_watcher, ctx);
    }
}
