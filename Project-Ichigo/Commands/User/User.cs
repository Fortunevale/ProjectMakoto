namespace Project_Ichigo.Commands.User;
internal class User : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("help"),
    CommandModule("user"),
    Description("Shows all available commands, their usage and their description")]
    public async Task Help(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            List<KeyValuePair<string, string>> Commands = new();


            foreach (var command in ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()))
            {
                if (command.Value.CustomAttributes.OfType<CommandModuleAttribute>() is null)
                    continue;

                string module = command.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString;

                switch (module)
                {
                    case "admin":
                        if (!ctx.Member.IsAdmin(_bot._status))
                            continue;
                        break;
                    case "maintainence":
                        if (!ctx.Member.IsMaintenance(_bot._status))
                            continue;
                        break;
                    case "hidden":
                        continue;
                    default:
                        break;
                }

                Commands.Add(new KeyValuePair<string, string>($"{module.FirstLetterToUpper()} Commands", $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_"));
            }

            var Fields = Commands.PrepareEmbedFields();
            var Embeds = Fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value
                .Replace("##Prefix##", ctx.Prefix)
                .Replace("##n##", "\n")))
                .ToList().PrepareEmbeds("", "All available commands will be listed below.\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                            "**Do not include the brackets when using commands, they're merely an indicator for requirement.**");

            try
            {
                foreach (var b in Embeds)
                    await ctx.Member.SendMessageAsync(embed: b.WithAuthor(ctx.Guild.Name, "", ctx.Guild.IconUrl).WithFooter($"Command used by {ctx.Member.UsernameWithDiscriminator}").WithTimestamp(DateTime.UtcNow).WithColor(ColorHelper.Info).Build());

                var successembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = ":mailbox_with_mail: `You got mail! Please check your dm's.`",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                        IconUrl = ctx.Member.AvatarUrl
                    },
                    Timestamp = DateTime.UtcNow,
                    Color = ColorHelper.Success
                };

                await ctx.Channel.SendMessageAsync(embed: successembed);
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = ":x: `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                        IconUrl = ctx.Member.AvatarUrl
                    },
                    Timestamp = DateTime.UtcNow,
                    Color = ColorHelper.Error,
                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                };

                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                await ctx.Channel.SendMessageAsync(embed: errorembed);
            }
            catch (Exception)
            {
                throw;
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("info"),
    CommandModule("user"),
    Description("Shows informations about the bot or the mentioned user")]
    public async Task Info(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            if (victim is not null)
            {
                _ = ctx.Client.GetCommandsNext().RegisteredCommands["user-info"].ExecuteAsync(ctx);
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = Resources.StatusIndicators.LoadingBlue,
                    Name = "Informations about this server and bot"
                },
                Color = ColorHelper.Info,
                Description = "",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };

            embed.AddField(new DiscordEmbedField("General info", "󠂪 󠂪", true));
            embed.Fields.First(x => x.Name == "General info").Value = "\n_All dates follow `DD.MM.YYYY` while the time zone is set to the `Coordinated Universal Time (UTC), +00:00`._\n";

            embed.AddField(new DiscordEmbedField("Guild", "󠂪 󠂪", true));
            embed.Fields.First(x => x.Name == "Guild").Value = "**Guild name**\n`Loading..`\n" +
                                                                "**Guild created at**\n`Loading..`\n" +
                                                                "**Owner of this guild**\n`Loading..`\n" +
                                                                "**Current member count**\n`Loading..`";

            embed.AddField(new DiscordEmbedField("Bot", "󠂪 󠂪", true));
            embed.Fields.First(x => x.Name == "Bot").Value = "**Currently running as**\n`Loading..`\n" +
                                                                "**Currently running software**\n`Loading..`\n" +
                                                                "**Currently running on**\n`Loading..`\n" +
                                                                "**Current bot lib and version**\n`Loading..`\n" +
                                                                "**Bot uptime**\n`Loading..`\n" +
                                                                "**Current API Latency**\n`Loading..`";

            embed.AddField(new DiscordEmbedField("Host", "󠂪 󠂪", true));
            embed.Fields.First(x => x.Name == "Host").Value = "**Current CPU load**\n`Loading..`\n" +
                                                                "**Current RAM usage**\n`Loading..`\n" +
                                                                "**Current temperature**\n`Loading..`\n" +
                                                                "**Server uptime**\n`Loading..`";

            var msg = await ctx.Channel.SendMessageAsync(embed: embed.Build());

            DateTime Age = new DateTime().AddSeconds((DateTime.UtcNow - ctx.Guild.CreationTimestamp).TotalSeconds);

            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Guild name**\n`Loading..`", $"**Guild name**\n`{ctx.Guild.Name}`");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Guild created at**\n`Loading..`", $"**Guild created at**\n`{ctx.Guild.CreationTimestamp.ToUniversalTime():dd.MM.yyyy HH:mm:ss}` ({Math.Round(TimeSpan.FromTicks(Age.Ticks).TotalDays, 0)} days ago)");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Owner of this guild**\n`Loading..`", $"**Owner of this guild**\n{ctx.Guild.Owner.Mention} `{ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}`");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Current member count**\n`Loading..`", $"**Current member count**\n`{ctx.Guild.MemberCount}/{ctx.Guild.MaxMembers}`");

            await msg.ModifyAsync(embed: embed.Build());

            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running as**\n`Loading..`", $"**Currently running as**\n`{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");

            if (File.Exists("LatestGitPush.cfg"))
            {
                var bFile = File.ReadLines("LatestGitPush.cfg");
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project-Ichigo by `Mira#2000` (GH-{bFile.First().Trim().Replace("/", ".")} ({bFile.Skip(1).First().Trim()}) built on the {bFile.Skip(2).First().Trim().Replace("/", ".")} at {bFile.Skip(3).First().Trim().Remove(bFile.Skip(3).First().Trim().IndexOf(","), bFile.Skip(3).First().Trim().Length - bFile.Skip(3).First().Trim().IndexOf(","))})`");
            }
            else
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project-Ichigo by `Mira#2000` (GH-UNIDENTIFIED)`");

            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running on**\n`Loading..`", $"**Currently running on**\n`{Environment.OSVersion.Platform} with DOTNET-{Environment.Version}`");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Current bot lib and version**\n`Loading..`", $"**Current bot lib and version**\n[`{ctx.Client.BotLibrary} {ctx.Client.VersionString}`](https://github.com/Aiko-IT-Systems/DisCatSharp)");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Bot uptime**\n`Loading..`", $"**Bot uptime**\n`{Math.Round((DateTime.UtcNow - _bot._status.startupTime).TotalHours, 2)} hours`");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Current API Latency**\n`Loading..`", $"**Current API Latency**\n`{ctx.Client.Ping}ms`");

            await msg.ModifyAsync(embed: embed.Build());

            await msg.ModifyAsync(embed: embed.Build());

            try
            {
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current CPU load**\n`Loading..`", $"**Current CPU load**\n`{Math.Round(await GetCpuUsageForProcess(), 2).ToString().Replace(",", ".")}%`");
            }
            catch (Exception ex)
            {
                LogError($"Failed to get cpu load: {ex}");
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current CPU load**\n`Loading..`", $"**Current CPU load**\n`Error`");
            }

            try
            {
                var metrics = MemoryMetricsClient.GetMetrics();

                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current RAM usage**\n`Loading..`", $"**Current RAM usage**\n`{Math.Round(metrics.Used, 2)}/{Math.Round(metrics.Total, 2)}MB`");
            }
            catch (Exception ex)
            {
                LogError($"Failed to get cpu load: {ex}");
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current RAM usage**\n`Loading..`", "**Current RAM usage**\n`Error`");
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                try
                {
                    ProcessStartInfo info = new();

                    info.FileName = "bash";
                    info.Arguments = $"-c sensors";
                    info.RedirectStandardError = true;
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;

                    var b = Process.Start(info);

                    b.WaitForExit();

                    var matches = Regex.Matches(b.StandardOutput.ReadToEnd(), "(\\+*[0-9]*.[0-9]*°C(?!,|\\)))");

                    int _temp = 0;
                    decimal[] temps = new decimal[ matches.Count ];

                    foreach (var c in matches)
                    {
                        temps[ _temp ] = Convert.ToDecimal(c.ToString().Replace("°C", "").Replace("+", ""));
                        _temp++;
                    }

                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Avg: {temps.Average()}°C Max: {temps.Max()}°C Min: {temps.Min()}°C`");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to get temps: {ex}");
                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Error`");
                }

                try
                {
                    ProcessStartInfo info = new();

                    info.FileName = "bash";
                    info.Arguments = $"-c uptime";
                    info.RedirectStandardError = true;
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;

                    var b = Process.Start(info);

                    b.WaitForExit();

                    string Output = b.StandardOutput.ReadToEnd();
                    Output = Output.Remove(Output.IndexOf(','), Output.Length - Output.IndexOf(',')).TrimStart();

                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`{Output}`");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to get uptime: {ex}");
                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`Error`");
                }
            }
            else
            {
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Currently unavailable`");
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`Currently unavailable`");
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;
            await msg.ModifyAsync(embed: embed.Build());
        }).Add(_bot._watcher, ctx);
    }



    [Command("user-info"), Aliases("userinfo"),
    CommandModule("user"),
    Description("Shows information about the mentioned user")]
    public async Task UserInfo(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            try
            {
                DiscordMember bMember = null;

                try
                {
                    bMember = await ctx.Guild.GetMemberAsync(victim.Id);
                }
                catch (Exception ex)
                {
                    LogDebug($"Failed to get user: {ex}");
                }

                DateTime CreationAge = new DateTime().AddSeconds((DateTime.UtcNow - victim.CreationTimestamp.ToUniversalTime()).TotalSeconds);

                DateTime JoinedAtAge = new();

                if (bMember is not null)
                    JoinedAtAge = new DateTime().AddSeconds((DateTime.UtcNow - bMember.JoinedAt.ToUniversalTime()).TotalSeconds);

                string GenerateRoles = "";

                if (bMember is not null)
                {
                    if (bMember.Roles.Any())
                    {
                        foreach (var b in bMember.Roles)
                        {
                            GenerateRoles += $"{b.Mention}, ";
                        }

                        GenerateRoles = GenerateRoles.Remove(GenerateRoles.Length - 2, 2);
                    }
                    else
                    {
                        GenerateRoles = "`User doesn't have any roles.`";
                    }
                }
                else
                {
                    GenerateRoles = "`Not yet implemented.`";
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = ColorHelper.Info,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = victim.AvatarUrl,
                        Name = $"{victim.Username}#{victim.Discriminator} ({victim.Id})"
                    },
                    Description = $"[Avatar]({victim.AvatarUrl})",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                        IconUrl = ctx.Member.AvatarUrl
                    },
                    Timestamp = DateTime.UtcNow
                };

                if (bMember is null)
                {
                    embed.Description += "\n\n`Not yet implemented.`";
                }

                if (bMember is not null)
                    embed.AddField(new DiscordEmbedField("Roles", GenerateRoles.Truncate(1024), true));
                else
                    embed.AddField(new DiscordEmbedField($"Roles (Backup)", GenerateRoles.Truncate(1024), true));

                embed.AddField(new DiscordEmbedField("Created at", $"`{victim.CreationTimestamp.ToUniversalTime():dd.MM.yyyy HH:mm:ss zzz}` ({Math.Round(TimeSpan.FromTicks(CreationAge.Ticks).TotalDays, 0)} days ago)", true));

                if (bMember is not null)
                {
                    embed.AddField(new DiscordEmbedField("Joined at", $"`{bMember.JoinedAt.ToUniversalTime():dd.MM.yyyy HH:mm:ss zzz}` ({Math.Round(TimeSpan.FromTicks(JoinedAtAge.Ticks).TotalDays, 0)} days ago)", true));
                }
                else
                {
                    embed.AddField(new DiscordEmbedField("󠂪 󠂪", $"󠂪 󠂪", true));
                }

                embed.AddField(new DiscordEmbedField("First joined at", $"`Not yet implemented.`", true));

                embed.AddField(new DiscordEmbedField("Invited by", $"`Not yet implemented.`", true));

                embed.AddField(new DiscordEmbedField("Users invited", $"`Not yet implemented.`", true));

                await ctx.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                LogError($"Error occured while trying to generate info about a user: {ex}");
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("avatar"), Aliases("pfp"),
    CommandModule("user"),
    Description("Sends the user's avatar as an embedded image")]
    public async Task Avatar(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            if (victim is null)
            {
                victim = ctx.Member;
            }

            var embed2 = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{victim.Username}#{victim.Discriminator}'s Avatar",
                    Url = victim.AvatarUrl
                },
                Title = "",
                ImageUrl = victim.AvatarUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = ColorHelper.Info
            };

            await ctx.Channel.SendMessageAsync(embed: embed2);
        }).Add(_bot._watcher, ctx);
    }



    [Command("rank"), Aliases("level", "lvl"),
    CommandModule("user"),
    Description("Shows you your current level and progress")]
    public async Task RankCommand(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Experience • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience is disabled on this server. Please run '{ctx.Prefix}experiencesettings config' to configure the experience system.`"
                });
                return;
            }

            if (victim is null)
            {
                victim = ctx.Member;
            }

            long current = (long)Math.Floor((decimal)(_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Experience - _bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Level - 1)));
            long max = (long)Math.Floor((decimal)(_bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Level) - _bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Level - 1)));

            _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"Experience • {ctx.Guild.Name}",
                    IconUrl = ctx.Guild.IconUrl
                },
                Description = $"{(victim.Id == ctx.User.Id ? "You're" : $"{victim.Mention} is")} currently **Level {_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Level.DigitsToEmotes()} with `{_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Experience.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}` XP**\n\n" +
                              $"**Level {(_bot._guilds.Servers[ctx.Guild.Id].Members[victim.Id].Level + 1).DigitsToEmotes()} Progress**\n" +
                              $"`{Math.Floor((decimal)((decimal)((decimal)current / (decimal)max) * 100)).ToString().Replace(",", ".")}%` " +
                              $"`{GenerateASCIIProgressbar(current, max, 44)}` " +
                              $"`{current}/{max} XP`",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = ColorHelper.HiddenSidebar
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("leaderboard"),
    CommandModule("user"),
    Description("Shows the current experience leaderboard")]
    public async Task LeaderboardCommand(CommandContext ctx, [Description("3-50")]int ShowAmount = 10)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.Servers[ctx.Guild.Id].ExperienceSettings.UseExperience)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Experience • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience is disabled on this server. Please run '{ctx.Prefix}experiencesettings config' to configure the experience system.`"
                });
                return;
            }

            if (ShowAmount is > 50 or < 3)
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Color = ColorHelper.HiddenSidebar,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading,
                    Name = $"Experience Leaderboard"
                },
                Description = $"`Loading Leaderboard, please wait..`",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };

            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            int count = 0;

            int currentuserplacement = 0;

            foreach (var b in _bot._guilds.Servers[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience))
            {
                currentuserplacement++;
                if (b.Key == ctx.User.Id)
                    break;
            }

            var members = await ctx.Guild.GetAllMembersAsync();

            List<KeyValuePair<string, string>> Board = new();

            foreach (var b in _bot._guilds.Servers[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience))
            {
                try
                {
                    if (!members.Any(x => x.Id == b.Key))
                        continue;

                    DiscordMember bMember = members.First(x => x.Id == b.Key);

                    if (bMember is null)
                        continue;

                    if (bMember.IsBot)
                        continue;

                    if (b.Value.Experience <= 1)
                        break;

                    count++;

                    Board.Add(new KeyValuePair<string, string>("󠂪 󠂪 ", $"**{count.DigitsToEmotes()}**. <@{b.Key}> `{bMember.Username}#{bMember.Discriminator}` (`Level {b.Value.Level} with {b.Value.Experience} XP`)"));

                    if (count >= ShowAmount)
                        break;
                }
                catch { }
            }

            var fields = Board.PrepareEmbedFields();

            foreach (var field in fields)
                PerformingActionEmbed.AddField(new DiscordEmbedField(field.Key, field.Value));

            if (count != 0)
            {
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"You're currently on the **{currentuserplacement}.** spot on the leaderboard.";
                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
            }
            else
            {
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $":no_entry_sign: `No one on this server has collected enough experience to show up on the leaderboard, get to typing!`";
                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("submit-url"),
    CommandModule("user"),
    Description("Allows submission of new malicous urls to our database")]
    public async Task UrlSubmit(CommandContext ctx, [Description("URL")]string url)
    {
        Task.Run(async () =>
        {
            try
            {
                if (!_bot._users.List.ContainsKey(ctx.User.Id))
                    _bot._users.List.Add(ctx.User.Id, new Users.Info());

                if (!_bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS)
                {
                    var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")));

                    var tos_embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                        Color = ColorHelper.Important,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"{1.DigitsToEmotes()}. You may not submit URLs that are non-malicous.\n" +
                                      $"{2.DigitsToEmotes()}. You may not spam submissions.\n" +
                                      $"{3.DigitsToEmotes()}. You may not submit unregistered domains.\n" +
                                      $"{4.DigitsToEmotes()}. You may not submit shortened URLs.\n\n" +
                                      $"We reserve the right to ban you for any reason that may not be listed.\n" +
                                      $"**Failing to follow these conditions may get you or your guild blacklisted from using this bot.**\n" +
                                      $"**This includes, but is not limited to, pre-existing guilds with your ownership and future guilds.**\n\n" +
                                      $"To accept these conditions, please click the button below. If you do not see a button to interact with, update your discord client."
                    };

                    var tos_accept = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                    async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                    {
                        try
                        {
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Message.Id == tos_accept.Id && e.User.Id == ctx.User.Id)
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                _bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS = true;

                                var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")));
                                await tos_accept.ModifyAsync(new DiscordMessageBuilder().WithEmbed(tos_embed.WithColor(ColorHelper.Success)).AddComponents(accepted_button));

                                _ = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].ExecuteAsync(ctx);

                                _ = Task.Delay(10000).ContinueWith(x =>
                                {
                                    _ = tos_accept.DeleteAsync();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"{ex}");
                        }
                    };

                    ctx.Client.ComponentInteractionCreated += RunInteraction;

                    try
                    {
                        await Task.Delay(60000);
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        tos_embed.Footer.Text += " • Interaction timed out";
                        await tos_accept.ModifyAsync(new DiscordMessageBuilder().WithEmbed(tos_embed));
                        await Task.Delay(5000);
                        _ = tos_accept.DeleteAsync();
                    }
                    catch { }
                    return;
                }

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                    Color = ColorHelper.Processing,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Processing your request..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

                if (_bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(_bot._status))
                {
                    embed.Description = $"`You cannot submit a domain for the next {_bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                if (_bot._submissionBans.BannedUsers.ContainsKey(ctx.User.Id))
                {
                    embed.Description = $"`You are banned from submitting URLs.`\n" +
                                        $"`Reason: {_bot._submissionBans.BannedUsers[ctx.User.Id].Reason}`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                if (_bot._submissionBans.BannedGuilds.ContainsKey(ctx.Guild.Id))
                {
                    embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                        $"`Reason: {_bot._submissionBans.BannedGuilds[ctx.Guild.Id].Reason}`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                string domain = url.ToLower();

                if (domain.StartsWith("https://") || domain.StartsWith("http://"))
                    domain = domain.Replace("https://", "").Replace("http://", "");

                if (domain.Contains('/'))
                    domain = domain.Remove(domain.IndexOf("/"), domain.Length - domain.IndexOf("/"));

                if (!domain.Contains('.') || domain.Contains(' '))
                {
                    embed.Description = $"`The domain ('{domain.Replace("`", "")}') you're trying to submit is invalid.`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                embed.Description = $"`You are about to submit the domain '{domain.Replace("`", "")}'. When submitting, your public user account and guild will be tracked and visible to verifiers. Do you want to proceed?`";
                embed.Color = ColorHelper.Success;
                embed.Author.IconUrl = Resources.LogIcons.Info;

                var continue_button = new DiscordButtonComponent(ButtonStyle.Success, "continue", "Submit domain", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")));
                var cancel_button = new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));


                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
                {
                    { continue_button },
                    { cancel_button }
                }));

                bool InteractionExecuted = false;

                async Task RunSubmissionInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                            {
                                InteractionExecuted = true;
                                ctx.Client.ComponentInteractionCreated -= RunSubmissionInteraction;

                                if (e.Interaction.Data.CustomId == "continue")
                                {
                                    embed.Description = $"`Submitting your domain..`";
                                    embed.Color = ColorHelper.Loading;
                                    embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    embed.Description = $"`Checking if your domain is already in the database..`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    foreach (var b in _bot._phishingUrls.List)
                                    {
                                        if (domain.Contains(b.Key))
                                        {
                                            embed.Description = $"`The domain ('{domain.Replace("`", "")}') is already present in the database. Thanks for trying to contribute regardless.`";
                                            embed.Color = ColorHelper.Error;
                                            embed.Author.IconUrl = Resources.LogIcons.Error;
                                            _ = msg.ModifyAsync(embed.Build());
                                            return;
                                        }
                                    }

                                    embed.Description = $"`Checking if your domain has already been submitted before..`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    foreach (var b in _bot._submittedUrls.Urls)
                                    {
                                        if (b.Value.Url == domain)
                                        {
                                            embed.Description = $"`The domain ('{domain.Replace("`", "")}') has already been submitted. Thanks for trying to contribute regardless.`";
                                            embed.Color = ColorHelper.Error;
                                            embed.Author.IconUrl = Resources.LogIcons.Error;
                                            _ = msg.ModifyAsync(embed.Build());
                                            return;
                                        }
                                    }

                                    embed.Description = $"`Creating submission..`";
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                    var channel = await ctx.Client.GetChannelAsync(940040486910066698);

                                    var continue_button = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")));
                                    var cancel_button = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                                    var ban_user_button = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                                    var ban_guild_button = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                                    var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                    {
                                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Info, Name = $"Phishing Link Submission" },
                                        Color = ColorHelper.Success,
                                        Timestamp = DateTime.UtcNow,
                                        Description = $"`Submitted Url`: `{domain.Replace("`", "")}`\n" +
                                                        $"`Submission by`: `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n" +
                                                        $"`Submitted on `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                                    }).AddComponents(new List<DiscordComponent>
                                    {
                                        { continue_button },
                                        { cancel_button },
                                        { ban_user_button },
                                        { ban_guild_button },
                                    }));

                                    _bot._submittedUrls.Urls.Add(subbmited_msg.Id, new SubmittedUrls.UrlInfo
                                    {
                                        Url = domain,
                                        Submitter = ctx.User.Id,
                                        GuildOrigin = ctx.Guild.Id
                                    });

                                    _bot._users.List[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                                    embed.Description = $"`Submission created. Thanks for your contribution.`";
                                    embed.Color = ColorHelper.Success;
                                    embed.Author.IconUrl = Resources.LogIcons.Info;
                                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                }
                                else if (e.Interaction.Data.CustomId == "cancel")
                                {
                                    _ = msg.DeleteAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"{ex}");
                        }
                    });
                };

                ctx.Client.ComponentInteractionCreated += RunSubmissionInteraction;

                try
                {
                    await Task.Delay(60000);

                    if (InteractionExecuted)
                        return;

                    ctx.Client.ComponentInteractionCreated -= RunSubmissionInteraction;

                    embed.Footer.Text += " • Interaction timed out";
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    _ = msg.DeleteAsync();
                }
                catch { }
            }
            catch (Exception ex)
            {
                LogError($"{ex}");
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("afk"),
    CommandModule("user"),
    Description("Set yourself afk: Notify users pinging you that you're currently not around")]
    public async Task Afk(CommandContext ctx, [RemainingText][Description("Text (<128 characters)")]string reason = "-")
    {
        Task.Run(async () =>
        {
            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info());

            if (reason.Length > 128)
            {
                await ctx.SendSyntaxError();
                return;
            }

            _bot._users.List[ctx.User.Id].AfkStatus.Reason = Formatter.Sanitize(reason).Replace("@", "").Replace("&", "").Replace("#", "").Replace("<", "").Replace(">", "");
            _bot._users.List[ctx.User.Id].AfkStatus.TimeStamp = DateTime.UtcNow;

            var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Afk Status • {ctx.Guild.Name}" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"{ctx.User.Mention} `You're now set to be afk. Next time you send a message, your afk status will be removed.`"
            });
            await Task.Delay(10000);
            _ = msg.DeleteAsync();
        }).Add(_bot._watcher, ctx);
    }



    [Command("scoresaber"), Aliases("ss"),
    CommandModule("user"),
    Description("Get show a users Score Saber profile by id")]
    public async Task ScoreSaber(CommandContext ctx, [Description("ID|@User")]string id = "")
    {
        Task.Run(async () =>
        {
            bool AddLinkButton = true;

            if ((string.IsNullOrWhiteSpace(id) || id.Contains('@')) && ctx.Message.MentionedUsers != null && ctx.Message.MentionedUsers.Count > 0)
            {
                if (id.Contains('@'))
                    if (!_bot._users.List.ContainsKey(ctx.Message.MentionedUsers[0].Id))
                        _bot._users.List.Add(ctx.Message.MentionedUsers[0].Id, new Users.Info());

                if (_bot._users.List[ctx.Message.MentionedUsers[0].Id].ScoreSaber.Id != 0)
                {
                    id = _bot._users.List[ctx.Message.MentionedUsers[0].Id].ScoreSaber.Id.ToString();
                    AddLinkButton = false;
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`This user has no Score Saber Profile linked to their Discord Account.`"
                    };

                    _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    return;
                }
            }

            await SendScoreSaberProfile(ctx, id, AddLinkButton);
        }).Add(_bot._watcher, ctx);
    }

    private async Task SendScoreSaberProfile(CommandContext ctx, string id = "", bool AddLinkButton = true)
    {
        if (!_bot._users.List.ContainsKey(ctx.User.Id))
            _bot._users.List.Add(ctx.User.Id, new Users.Info());

        if (string.IsNullOrWhiteSpace(id))
        {
            if (_bot._users.List[ctx.User.Id].ScoreSaber.Id != 0)
            {
                id = _bot._users.List[ctx.User.Id].ScoreSaber.Id.ToString();
            }
            else
            {
                _ = ctx.SendSyntaxError();
                return;
            }
        }

        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
            Color = ColorHelper.Processing,
            Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
            Timestamp = DateTime.UtcNow,
            Description = $"`Looking for player..`"
        };

        var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

        try
        {
            var player = await _bot._scoreSaberClient.GetPlayerById(id);

            CancellationTokenSource cancellationTokenSource = new();

            DiscordButtonComponent ShowProfileButton = new(ButtonStyle.Primary, "getmain", "Show Profile", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")));
            DiscordButtonComponent TopScoresButton = new(ButtonStyle.Primary, "gettopscores", "Show Top Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":sparkler:")));
            DiscordButtonComponent RecentScoresButton = new(ButtonStyle.Primary, "getrecentscores", "Show Recent Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":clock3:")));
            DiscordLinkButtonComponent OpenProfileInBrowser = new($"https://scoresaber.com/u/{id}", "Open in browser", false);

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == "thats_me")
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(TopScoresButton).AddComponents(RecentScoresButton));
                            _bot._users.List[ctx.User.Id].ScoreSaber.Id = Convert.ToUInt64(player.id);

                            var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                                Color = ColorHelper.Success,
                                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                                Timestamp = DateTime.UtcNow,
                                Description = $"{ctx.User.Mention} `Linked '{player.name}' ({player.id}) to your account. You can now run '{ctx.Prefix}scoresaber' without an argument to get your profile in an instant.`\n" +
                                              $"`To remove the link, run '{ctx.Prefix}scoresaber-unlink'.`"
                            }));

                            _ = Task.Delay(10000).ContinueWith(x =>
                            {
                                _ = new_msg.DeleteAsync();
                            });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == "gettopscores")
                        {
                            try
                            {
                                var scores = await _bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.TOP);
                                ShowScores(scores, RequestParameters.ScoreType.TOP).Add(_bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.Interaction.Data.CustomId == "getrecentscores")
                        {
                            try
                            {
                                var scores = await _bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.RECENT);
                                ShowScores(scores, RequestParameters.ScoreType.RECENT).Add(_bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.Interaction.Data.CustomId == "getmain")
                        {
                            ShowProfile().Add(_bot._watcher, ctx);
                        }

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                    }
                }).Add(_bot._watcher, ctx);
            }

            async Task ShowScores(PlayerScores scores, RequestParameters.ScoreType scoreType)
            {
                embed.ClearFields();
                embed.ImageUrl = "";
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n\n" +
                                    $"{(scoreType == RequestParameters.ScoreType.TOP ? "**Top Scores**" : "**Recent Scores**")}";

                foreach (var score in scores.playerScores.Take(5))
                {
                    embed.AddField(new DiscordEmbedField($"{score.leaderboard.songName}{(!string.IsNullOrWhiteSpace(score.leaderboard.songSubName) ? $" {score.leaderboard.songSubName}" : "")} - {score.leaderboard.songAuthorName} [{score.leaderboard.levelAuthorName}]".TruncateWithIndication(256),
                        $":globe_with_meridians: **#{score.score.rank}**  󠂪 󠂪| 󠂪 󠂪 {Formatter.Timestamp(score.score.timeSet, TimestampFormat.RelativeTime)}\n" +
                        $"{(score.leaderboard.ranked ? $"**`{((decimal)((decimal)score.score.modifiedScore / (decimal)score.leaderboard.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp [{(score.score.pp * score.score.weight).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp]`**\n" : "\n")}" +
                        $"`{score.score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}` 󠂪 󠂪| 󠂪 󠂪 **{(score.score.fullCombo ? ":white_check_mark: `FC`" : $"{false.BoolToEmote()} `{score.score.missedNotes + score.score.badCuts}`")}**"));
                }

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(OpenProfileInBrowser).AddComponents(ShowProfileButton).AddComponents((scoreType == RequestParameters.ScoreType.TOP ? RecentScoresButton : TopScoresButton)));
            }

            async Task ShowProfile()
            {
                embed.ClearFields();
                embed.Title = $"{player.name} 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪`{player.pp.ToString().Replace(",", ".")}pp`";
                embed.Color = ColorHelper.Info;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.profilePicture };
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n";
                embed.AddField(new DiscordEmbedField("Ranked Play Count", $"`{player.scoreStats.rankedPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Ranked Score", $"`{player.scoreStats.totalRankedScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Average Ranked Accuracy", $"`{Math.Round(player.scoreStats.averageRankedAccuracy, 2).ToString().Replace(",", ".")}%`", true));
                embed.AddField(new DiscordEmbedField("Total Play Count", $"`{player.scoreStats.totalPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Score", $"`{player.scoreStats.totalScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Replays Watched By Others", $"`{player.scoreStats.replaysWatched}`", true));

                DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent LinkButton = new(ButtonStyle.Primary, "thats_me", "Link Score Saber Profile to Discord Account", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_lower_right:")));

                builder.AddComponents(OpenProfileInBrowser);

                if (_bot._users.List[ctx.User.Id].ScoreSaber.Id == 0 && AddLinkButton)
                    builder.AddComponents(LinkButton);

                msg.ModifyAsync(builder).Add(_bot._watcher, ctx);

                var file = $"{Guid.NewGuid()}.png";

                try
                {
                    Chart qc = new();
                    qc.Width = 1000;
                    qc.Height = 500;
                    qc.Config = $@"{{
                        type: 'line',
	                    data: 
	                    {{
                            labels: 
		                    [
			                    '',
			                    '48 days ago','',
			                    '46 days ago','',
			                    '44 days ago','',
			                    '42 days ago','',
			                    '40 days ago','',
			                    '38 days ago','',
			                    '36 days ago','',
			                    '34 days ago','',
			                    '32 days ago','',
			                    '30 days ago','',
			                    '28 days ago','',
			                    '26 days ago','',
			                    '24 days ago','',
			                    '22 days ago','',
			                    '20 days ago','',
			                    '18 days ago','',
			                    '16 days ago','',
			                    '14 days ago','',
			                    '12 days ago','',
			                    '10 days ago','',
			                    '8 days ago','',
			                    '6 days ago','',
			                    '4 days ago','',
			                    '2 days ago','',
			                    'Today'
                            ],
		                    datasets: 
		                    [
			                    {{
				                    label: 'Placements',
				                    data: [{player.histories},{player.rank}],
				                    fill: false,
				                    borderColor: getGradientFillHelper('vertical', ['#6b76da', '#a336eb', '#FC0000']),
				                    reverse: true,
				                    id: ""yaxis2""

                                }}
		                    ]

                        }},
	                    options:
	                    {{
		                    legend:
		                    {{
			                    display: false,
		                    }},
                            elements:
                            {{
                                point:
                                {{
                                    radius: 0
                                }}
                            }},
		                    scales:
		                    {{
			                    yAxes:
			                    [
                                    {{
                                        reverse: true,
                                        ticks:
  			                            {{
                                            reverse: true
  			                            }}
			                        }}
                                ]
		                    }}
	                    }}
                    }}";

                    qc.ToFile(file);

                    using (FileStream stream = File.Open(file, FileMode.Open))
                    {
                        var asset = await (await ctx.Client.GetChannelAsync(945747744302174258)).SendMessageAsync(new DiscordMessageBuilder().WithFile(file, stream));

                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        embed.ImageUrl = asset.Attachments[0].Url;
                        builder = builder.WithEmbed(embed);
                        builder.AddComponents(TopScoresButton);
                        builder.AddComponents(RecentScoresButton);
                        _ = msg.ModifyAsync(builder);
                    }
                }
                catch (Exception ex)
                {
                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    builder.AddComponents(TopScoresButton);
                    builder.AddComponents(RecentScoresButton);
                    _ = msg.ModifyAsync(builder);
                    LogError(ex.ToString());
                }

                try
                {
                    await Task.Delay(1000);
                    File.Delete(file);
                }
                catch { }
            }

            ShowProfile().Add(_bot._watcher, ctx);

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            try
            {
                await Task.Delay(120000, cancellationTokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        }
        catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`An internal server exception occured. Please retry later.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`Couldn't find the specified player.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.UnprocessableEntity)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`Please provide an user id.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Exception)
        {
            _ = msg.DeleteAsync();
            throw;
        }
    }

    [Command("scoresaber-search"), Aliases("sss", "scoresabersearch"),
    CommandModule("user"),
    Description("Search a user on Score Saber by name")]
    public async Task ScoreSaberSearch(CommandContext ctx, [Description("Name")] [RemainingText]string name)
    {
        Task.Run(async () =>
        {
            DiscordSelectComponent GetContinents(string default_code)
            {
                List<DiscordSelectComponentOption> continents = new();
                continents.Add(new DiscordSelectComponentOption($"No country filter (may load much longer)", "no_country", "", (default_code == "no_country")));
                foreach (var b in _bot._countryCodes.List.GroupBy(x => x.Value.ContinentCode).Select(x => x.First()).Take(24))
                {
                    continents.Add(new DiscordSelectComponentOption($"{b.Value.ContinentCode}", b.Value.ContinentCode, "", (default_code == b.Value.ContinentCode)));
                }
                return new DiscordSelectComponent("continent_selection", "Select a country..", continents as IEnumerable<DiscordSelectComponentOption>);
            }

            DiscordSelectComponent GetCountries(string continent_code, string default_country, int page)
            {
                List<DiscordSelectComponentOption> countries = new();
                var currentCountryList = _bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == continent_code.ToLower()).Skip((page - 1) * 25).Take(25).ToList();

                foreach (var b in currentCountryList)
                {
                    DiscordEmoji flag_emote = null;
                    try { flag_emote = DiscordEmoji.FromName(ctx.Client, $":flag_{b.Key.ToLower()}:"); } catch (Exception) { flag_emote = DiscordEmoji.FromName(ctx.Client, $":white_large_square:"); }
                    countries.Add(new DiscordSelectComponentOption($"{b.Value.Name}", b.Key, "", (b.Key == default_country), new DiscordComponentEmoji(flag_emote)));
                }
                return new DiscordSelectComponent("country_selection", "Select a country..", countries as IEnumerable<DiscordSelectComponentOption>);
            }

            var start_search_button = new DiscordButtonComponent(ButtonStyle.Success, "start_search", "Start Search", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":mag:")));
            var next_step_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_step", "Next step", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

            var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
            var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Search • {ctx.Guild.Name}" },
                Color = ColorHelper.AwaitingInput,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Please select a continent filter below.`"
            };

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents("no_country")).AddComponents(start_search_button));
            CancellationTokenSource tokenSource = new();

            string selectedContinent = "no_country";
            string selectedCountry = "no_country";
            int lastFetchedPage = -1;
            int currentPage = 1;
            int currentFetchedPage = 1;
            bool playerSelection = false;
            PlayerSearch.SearchResult lastSearch = null;

            async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            async Task RefreshCountryList()
                            {
                                embed.Description = "`Please select a country filter below.`";

                                if (selectedCountry != "no_country")
                                {
                                    embed.Description += $"\n`Selected country: '{_bot._countryCodes.List[selectedCountry].Name}'`";
                                }

                                var page = GetCountries(selectedContinent, selectedCountry, currentPage);
                                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(page);

                                if (currentPage == 1 && _bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Count() > 25)
                                {
                                    builder.AddComponents(next_page_button);
                                }

                                if (currentPage != 1)
                                {
                                    if (_bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Skip((currentPage - 1) * 25).Count() > 25)
                                        builder.AddComponents(next_page_button);

                                    builder.AddComponents(previous_page_button);
                                }

                                if (selectedCountry != "no_country")
                                    builder.AddComponents(start_search_button);

                                msg.ModifyAsync(builder).Add(_bot._watcher, ctx);
                            }

                            async Task RefreshPlayerList()
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                embed.Description = "`Searching for players with specified criteria..`";
                                embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                if (currentFetchedPage != lastFetchedPage)
                                {
                                    try
                                    {
                                        lastSearch = await _bot._scoreSaberClient.SearchPlayer(name, currentFetchedPage, (selectedCountry != "no_country" ? selectedCountry : ""));
                                    }
                                    catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                                    {
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        embed.Color = ColorHelper.Error;
                                        embed.Description = $"`An internal server exception occured. Please retry later.`";
                                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                        return;
                                    }
                                    catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                                    {
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        embed.Color = ColorHelper.Error;
                                        embed.Description = $"`The access to the search api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    lastFetchedPage = currentFetchedPage;
                                }

                                List<DiscordSelectComponentOption> playerDropDownOptions = new();
                                var playerList = lastSearch.players.Skip((currentPage - 1) * 25).Take(25).ToList();
                                foreach (var b in playerList)
                                {
                                    playerDropDownOptions.Add(new DiscordSelectComponentOption($"{b.name} | {b.pp.ToString().Replace(",", ".")}pp", b.id, $"🌐 #{b.rank} | {b.country.IsoCountryCodeToFlagEmoji()} #{b.countryRank}"));
                                }
                                var player_dropdown = new DiscordSelectComponent("player_selection", "Select a player..", playerDropDownOptions as IEnumerable<DiscordSelectComponentOption>);

                                var builder = new DiscordMessageBuilder().AddComponents(player_dropdown);

                                bool added_next = false;

                                if (currentPage == 1 && lastSearch.players.Length > 25)
                                {
                                    builder.AddComponents(next_page_button);
                                    added_next = true;
                                }

                                if (currentPage != 1 || lastFetchedPage != 1)
                                {
                                    if ((lastSearch.players.Skip((currentPage - 1) * 25).Take(25).Count() > 25 || ((((lastSearch.metadata.total - (currentFetchedPage - 1)) * 50) > 0) && player_dropdown.Options.Count == 25)) && !added_next)
                                        builder.AddComponents(next_page_button);

                                    builder.AddComponents(previous_page_button);
                                }

                                ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                embed.Description = $"`Found {lastSearch.metadata.total} players. Fetched {lastSearch.players.Length} players. Showing {playerDropDownOptions.Count} players.`";
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                builder.WithEmbed(embed);
                                await msg.ModifyAsync(builder);
                            }

                            if (e.Interaction.Data.CustomId == "start_search")
                            {
                                tokenSource.Cancel();
                                tokenSource = null;

                                playerSelection = true;
                                currentPage = 1;
                                await RefreshPlayerList();

                                tokenSource = new();
                            }
                            else if (e.Interaction.Data.CustomId == "player_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                tokenSource.Cancel();
                                tokenSource = null;

                                embed.Description = "`Getting profile..`";
                                embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                await SendScoreSaberProfile(ctx, e.Values.First());
                                _ = msg.DeleteAsync();

                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "next_step")
                            {
                                _ = RefreshCountryList();
                            }
                            else if (e.Interaction.Data.CustomId == "country_selection")
                            {
                                selectedCountry = e.Values.First();

                                _ = RefreshCountryList();
                            }
                            else if (e.Interaction.Data.CustomId == "prev_page")
                            {
                                if (playerSelection)
                                {
                                    if (currentPage == 1)
                                    {
                                        currentPage = 2;
                                        currentFetchedPage -= 1;
                                    }
                                    else
                                        currentPage -= 1;

                                    tokenSource.Cancel();
                                    tokenSource = null;

                                    await RefreshPlayerList();

                                    tokenSource = new();
                                }
                                else
                                {
                                    currentPage -= 1;
                                    _ = RefreshCountryList();
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "next_page")
                            {
                                if (playerSelection)
                                {
                                    if (currentPage == 2)
                                    {
                                        currentPage = 1;
                                        currentFetchedPage += 1;
                                    }
                                    else
                                        currentPage += 1;

                                    tokenSource.Cancel();
                                    tokenSource = null;

                                    await RefreshPlayerList();

                                    tokenSource = new();
                                }
                                else
                                {
                                    currentPage += 1;
                                    _ = RefreshCountryList();
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "continent_selection")
                            {
                                selectedContinent = e.Values.First();

                                if (selectedContinent != "no_country")
                                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(next_step_button));
                                else
                                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(start_search_button));
                            }

                            try
                            {
                                tokenSource.Cancel();
                                tokenSource = new();
                                await Task.Delay(120000, tokenSource.Token);
                                embed.Footer.Text += " • Interaction timed out";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                _ = msg.DeleteAsync();

                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                            }
                            catch { }
                        }
                    }
                    catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
                    {
                        embed.Author.IconUrl = Resources.LogIcons.Error;
                        embed.Color = ColorHelper.Error;
                        embed.Description = $"`Couldn't find any player with the specified criteria.`";
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    }
                    catch (Exception)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                        throw;
                    }
                }).Add(_bot._watcher, ctx);
            }
            ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

            try
            {
                await Task.Delay(120000, tokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();

                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
            }
            catch { }

            return;
        }).Add(_bot._watcher, ctx);
    }



    [Command("scoresaber-unlink"), Aliases("ssu", "scoresaberunlink"),
    CommandModule("user"),
    Description("Unlink your Score Saber Profile from your Discord Account")]
    public async Task ScoreSaberUnlink(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info());

            if (_bot._users.List[ctx.User.Id].ScoreSaber.Id != 0)
            {
                _bot._users.List[ctx.User.Id].ScoreSaber.Id = 0;

                var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"{ctx.User.Mention} `Unlinked your Score Saber Profile from your Discord Account`"
                }));

                _ = Task.Delay(10000).ContinueWith(x =>
                {
                    _ = new_msg.DeleteAsync();
                });
            }
            else
            {
                var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"{ctx.User.Mention} `There is no Score Saber Profile linked to your Discord Account.`"
                }));

                _ = Task.Delay(10000).ContinueWith(x =>
                {
                    _ = new_msg.DeleteAsync();
                });
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("hug"),
    CommandModule("user"),
    Description("Hug another user!")]
    public async Task Hug(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            string[] urls = {
            "https://cdn.discordapp.com/attachments/906976602557145110/943950308369903636/1.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950308101472266/2.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950307820462100/3.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950307606536243/4.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950307296161803/5.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950306994176081/6.gif",
            "https://cdn.discordapp.com/attachments/906976602557145110/943950306587312138/7.gif",
            };

            string[] phrases =
            {
                "%1 hugs %2! How sweet! ♥",
                "%1 gives %2 a big fat hug! <:fv_woah:961993656129175592>",
                "%2, watch out! %1 is coming to squeeze you tight! <:fv_woah:961993656129175592>",
            };

            string[] self_phrases =
            {
                "There, there.. I'll hug you %1 😢",
                "Does no one else hug you, %1? There, there.. I'll hug you.. 😢",
                "There, there.. I'll hug you %1. 😢 Sorry if i'm a bit cold, i'm not human y'know.. 😓",
                "You look lonely there, %1..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = ColorHelper.HiddenSidebar,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = urls[new Random().Next(0, urls.Length)],
                Color = ColorHelper.HiddenSidebar,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("pat"), Aliases("pet", "headpat", "headpet"),
    CommandModule("user"),
    Description("Give someone some headpats!")]
    public async Task Pat(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            Task.Run(async () =>
            {
                string[] urls = {
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995160328228914/senpai-ga-uzai-kouhai-no-hanashi-futaba.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995160726700092/anime-head-pat-anime-head-rub.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995161150291968/neko-anime-girl.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995161720737822/anime-anime-headrub.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995162253422633/charlotte-pat.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995162815438908/anime-head-pat.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995163146780732/pat-pat-head.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995163352326224/fantasista-doll-anime.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995165801807912/headpats-anime.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995166514806904/mai-headpats.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995188463628318/behave-anime.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995188912390165/anime-head-pat_1.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995189482819594/kanna-kanna-kamui.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995190267162674/kanna-kamui-pat.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995191034736690/nagi-no-asukara-manaka-mukaido.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995191449964564/anime-headpats.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995191965876324/head-pat-anime.gif",
                    "https://cdn.discordapp.com/attachments/906976602557145110/961995192267862016/anime-anime-head-rub.gif",
                };

                string[] phrases =
                {
                    "%1 gives %2 headpats!",
                };

                string[] self_phrases =
                {
                    "There, there.. I'll give you some headpats, %1 😢",
                    "I'll give you some headpats, %1.. 😢",
                    "You look lonely there, %1..",
                };

                if (ctx.Member.Id == user.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                        Color = ColorHelper.HiddenSidebar,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    }));
                    return;
                }

                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                    ImageUrl = urls[new Random().Next(0, urls.Length)],
                    Color = ColorHelper.HiddenSidebar,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                }));
            }).Add(_bot._watcher, ctx);
        }).Add(_bot._watcher, ctx);
    }
}
