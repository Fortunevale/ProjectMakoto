namespace Project_Ichigo.Commands.Admin;
internal class Maintainers : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Command("throw"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task Throw(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            throw new NotImplementedException();
        }).Add(_bot._watcher, ctx);
    }



    [Command("stop"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task Stop(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            File.WriteAllText("updated", "");
        }).Add(_bot._watcher, ctx);
    }



    [Command("globalban"), Aliases("global-ban"),
    CommandModule("maintainence"),
    Description("Bans a user from all servers opted into globalbans")]
    public async Task Globalban(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")]string reason = "-")
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
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

            _bot._globalBans.Users.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds)
            {
                if (!_bot._guilds.Servers.ContainsKey(b.Key))
                    _bot._guilds.Servers.Add(b.Key, new ServerInfo.ServerSettings());

                if (_bot._guilds.Servers[b.Key].JoinSettings.AutoBanGlobalBans)
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("globalunban"), Aliases("global-unban"),
    CommandModule("maintainence"),
    Description("Removes a user from global bans (doesn't unban user from all servers)")]
    public async Task Globalunban(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            _bot._globalBans.Users.Remove(victim.Id);
            await _bot._databaseClient._helper.DeleteRow(_bot._databaseClient.mainDatabaseConnection, "globalbans", "id", $"{victim.Id}");

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Removed '{victim.UsernameWithDiscriminator}' from global bans.`"
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("import"),
    CommandModule("maintainence"),
    Description("Allows import of Kaffeemaschine settings")]
    public async Task SettingsImport(CommandContext ctx, string load)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            if (ctx.Message.Attachments.Count == 0)
                throw new Exception($"File required");

            if (ctx.Message.Attachments[0].FileName.ToLower() == "usercache.json")
            {
                string file_content = await new HttpClient().GetStringAsync(ctx.Message.Attachments[0].Url);

                Dictionary<ulong, UserCache.UserCacheObjects> Users = JsonConvert.DeserializeObject<Dictionary<ulong, UserCache.UserCacheObjects>>(file_content);

                await _bot._databaseClient.SyncDatabase(true);

                switch (load.ToLower())
                {
                    case "xp":
                    case "exp":
                    {
                        try
                        {
                            foreach (var user in Users)
                            {
                                if ((long)user.Value.Experience <= 0)
                                    continue;

                                if (!_bot._guilds.Servers[ctx.Guild.Id].Members.ContainsKey(user.Key))
                                    _bot._guilds.Servers[ctx.Guild.Id].Members.Add(user.Key, new());

                                _bot._guilds.Servers[ctx.Guild.Id].Members[user.Key].Experience = (long)user.Value.Experience;
                                _bot._experienceHandler.CheckExperience(user.Key, ctx.Guild);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        _ = ctx.Channel.SendMessageAsync($"`Imported {Users.Count} users`");

                        break;
                    }
                    default:
                        throw new Exception("Unknown load type");
                }

                await _bot._databaseClient.SyncDatabase(true);
            }
            else
                throw new Exception($"Unhandled file");

        }).Add(_bot._watcher, ctx);
    }
}
