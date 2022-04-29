namespace Project_Ichigo.Commands.Maintainers;
internal class Maintainers : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("globalban"), Aliases("global-ban"),
    CommandModule("maintainence"),
    Description("Bans a user from all servers opted into globalbans")]
    public async Task Globalban(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")] string reason = "-")
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = ColorHelper.Processing,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Global banning '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            };

            var msg = await ctx.Channel.SendMessageAsync(embed);

            if (_bot._status.TeamMembers.Contains(victim.Id))
            {
                embed.Color = ColorHelper.Error;
                embed.Description = $"`'{victim.UsernameWithDiscriminator}' is registered in the staff team.`";
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            _bot._globalBans.List.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds)
            {
                if (!_bot._guilds.List.ContainsKey(b.Key))
                    _bot._guilds.List.Add(b.Key, new Guilds.ServerSettings());

                if (_bot._guilds.List[ b.Key ].JoinSettings.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Exception occured while trying to ban user from {b.Key}", ex);
                        Failed++;
                    }
                }
            }

            embed.Color = ColorHelper.Success;
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

            _bot._globalBans.List.Remove(victim.Id);
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

            if (ctx.Message.Attachments[ 0 ].FileName.ToLower() == "usercache.json")
            {
                string file_content = await new HttpClient().GetStringAsync(ctx.Message.Attachments[ 0 ].Url);

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

                                if (!_bot._guilds.List[ ctx.Guild.Id ].Members.ContainsKey(user.Key))
                                    _bot._guilds.List[ ctx.Guild.Id ].Members.Add(user.Key, new());

                                _bot._guilds.List[ ctx.Guild.Id ].Members[ user.Key ].Experience = (long)user.Value.Experience;
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


    
    [Command("log"), Aliases("loglevel","log-level"),
    CommandModule("maintainence"),
    Description("Change the bot's log level")]
    public async Task Log(CommandContext ctx, int Level)
    {
        Task.Run(async () =>
        {
            if (ctx.User.Id != 411950662662881290)
                return;

            if (Level > 7)
                throw new Exception("Invalid Log Level");

            ChangeLogLevel((LoggerObjects.LogLevel)Level);
            _ = ctx.RespondAsync($"`Changed LogLevel to '{(LoggerObjects.LogLevel)Level}'`");
        }).Add(_bot._watcher, ctx);
    }



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



    [Command("cooldowntest-light"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task CooldownTestLight(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            _ = ctx.RespondAsync("Cooldown finished.");
        }).Add(_bot._watcher, ctx);
    }



    [Command("cooldowntest-moderate"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task CooldownTestModerate(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

            _ = ctx.RespondAsync("Cooldown finished.");
        }).Add(_bot._watcher, ctx);
    }



    [Command("cooldowntest-heavy"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task CooldownTestHeavy(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

            _ = ctx.RespondAsync("Cooldown finished.");
        }).Add(_bot._watcher, ctx);
    }



    [Command("roleselectortest"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task RoleSelectorTest(CommandContext ctx, bool forMe)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Role selector test`"
            }));

            var role = await new InteractionSelectors.GenericSelectors(_bot).PromptRoleSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, forMe, "Testrole");

            await msg.ModifyAsync($"Selected {role.Mention}");
        }).Add(_bot._watcher, ctx);
    }



    [Command("channelselectortest"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task ChannelSelectorTest(CommandContext ctx, bool forMe)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Bump Reminder Settings • {ctx.Guild.Name}" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Channel selector test`"
            }));

            var channel = await new InteractionSelectors.GenericSelectors(_bot).PromptChannelSelection(ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg, forMe);

            await msg.ModifyAsync($"Selected {channel.Mention}");
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


    [Command("save"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task Save(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
                return;

            await _bot._databaseClient.SyncDatabase(true);
        }).Add(_bot._watcher, ctx);
    }
}
