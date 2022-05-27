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
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

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

            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

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
                LogError($"Failed to get cpu load", ex);
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current CPU load**\n`Loading..`", $"**Current CPU load**\n`Error`");
            }

            try
            {
                var metrics = MemoryMetricsClient.GetMetrics();

                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current RAM usage**\n`Loading..`", $"**Current RAM usage**\n`{Math.Round(metrics.Used, 2)}/{Math.Round(metrics.Total, 2)}MB`");
            }
            catch (Exception ex)
            {
                LogError($"Failed to get cpu load", ex);
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
                    LogError($"Failed to get temps", ex);
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
                    LogError($"Failed to get uptime", ex);
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
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            try
            {
                if (!_bot._guilds.List.ContainsKey(ctx.Guild.Id))
                    _bot._guilds.List.Add(ctx.Guild.Id, new Guilds.ServerSettings());

                if (!_bot._guilds.List[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                    _bot._guilds.List[ctx.Guild.Id].Members.Add(victim.Id, new());

                DiscordMember bMember = null;

                try
                {
                    bMember = await ctx.Guild.GetMemberAsync(victim.Id);
                }
                catch (Exception ex)
                {
                    LogDebug($"Failed to get user", ex);
                }

                DateTime CreationAge = new DateTime().AddSeconds((DateTime.UtcNow - victim.CreationTimestamp.ToUniversalTime()).TotalSeconds);

                DateTime JoinedAtAge = new();

                if (bMember is not null)
                    JoinedAtAge = new DateTime().AddSeconds((DateTime.UtcNow - bMember.JoinedAt.ToUniversalTime()).TotalSeconds);

                string GenerateRoles = "";

                if (bMember is not null)
                {
                    if (bMember.Roles.Any())
                        GenerateRoles = string.Join(", ", bMember.Roles.Select(x => x.Mention));
                    else
                        GenerateRoles = "`User doesn't have any roles.`";
                }
                else
                {
                    if (_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].MemberRoles.Count > 0)
                        GenerateRoles = string.Join(", ", _bot._guilds.List[ctx.Guild.Id].Members[victim.Id].MemberRoles.Where(x => ctx.Guild.Roles.ContainsKey(x.Id)).Select(x => $"{ctx.Guild.GetRole(x.Id).Mention}"));
                    else
                        GenerateRoles = "`User doesn't have any stored roles.`";
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = ColorHelper.Info,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = victim.AvatarUrl,
                        Name = $"{victim.Username}#{victim.Discriminator} ({victim.Id})"
                    },
                    Description = $"[Avatar Url]({victim.AvatarUrl})",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                        IconUrl = ctx.Member.AvatarUrl
                    },
                    Timestamp = DateTime.UtcNow
                };

                var banList = await ctx.Guild.GetBansAsync();
                bool isBanned = banList.Any(x => x.User.Id == victim.Id);
                DiscordBan banDetails = (isBanned ? banList.First(x => x.User.Id == victim.Id) : null);

                if (bMember is null)
                {
                    if (_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].FirstJoinDate == DateTime.UnixEpoch)
                    {
                        embed.Description += "\n\n`User never joined this server.`";
                    }
                    else
                    {
                        if (isBanned)
                            embed.Description += "\n\n`User is currently banned from this server.`";
                        else
                            embed.Description += "\n\n`User is currently not in this server.`";
                    }
                }

                if (isBanned)
                    if (!string.IsNullOrWhiteSpace(banDetails.Reason))
                        embed.AddField(new DiscordEmbedField("Ban Details", $"`{banDetails.Reason}`", false));
                    else
                        embed.AddField(new DiscordEmbedField("Ban Details", $"`No reason provided.`", false));

                if (bMember is not null)
                    embed.AddField(new DiscordEmbedField("Roles", GenerateRoles.Truncate(1024), true));
                else
                    embed.AddField(new DiscordEmbedField($"Roles (Backup)", GenerateRoles.Truncate(1024), true));


                embed.AddField(new DiscordEmbedField("Created at", $"{Formatter.Timestamp(victim.CreationTimestamp.ToUniversalTime(), TimestampFormat.LongDateTime)} ({Formatter.Timestamp(victim.CreationTimestamp.ToUniversalTime())})", true));


                if (bMember is not null)
                    embed.AddField(new DiscordEmbedField("Joined at", $"{Formatter.Timestamp(bMember.JoinedAt.ToUniversalTime(), TimestampFormat.LongDateTime)} ({Formatter.Timestamp(bMember.JoinedAt.ToUniversalTime())})", true));
                else
                    embed.AddField(new DiscordEmbedField("Left at", $"{Formatter.Timestamp(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].LastLeaveDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].LastLeaveDate)})", true));


                embed.AddField(new DiscordEmbedField("First joined at", $"{Formatter.Timestamp(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].FirstJoinDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].FirstJoinDate)})", true));

                embed.AddField(new DiscordEmbedField("Invited by", $"`Not yet implemented.`", true));

                embed.AddField(new DiscordEmbedField("Users invited", $"`Not yet implemented.`", true));

                if (bMember is not null)
                    if (bMember.CommunicationDisabledUntil.HasValue)
                        if (((DateTime)bMember.CommunicationDisabledUntil).GetTotalSecondsUntil() > 0)
                            embed.AddField(new DiscordEmbedField("Timed out until", $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value.ToUniversalTime(), TimestampFormat.LongDateTime)} ({Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value.ToUniversalTime())})", true));

                await ctx.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                LogError($"Error occured while trying to generate info about a user", ex);
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
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (victim is null)
            {
                victim = ctx.Member;
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{victim.Username}#{victim.Discriminator}'s Avatar",
                    Url = victim.AvatarUrl
                },
                ImageUrl = victim.AvatarUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = ColorHelper.Info
            };

            DiscordMember member = null;

            try { member = await victim.ConvertToMember(ctx.Guild); } catch { }

            var ServerProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Primary, "ShowServer", "Show Server Profile Picture", (string.IsNullOrWhiteSpace(member?.GuildAvatarHash)));
            var ProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Primary, "ShowProfile", "Show Profile Picture", false);

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton);

            var msg = await ctx.Channel.SendMessageAsync(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        if (e.Interaction.Data.CustomId == ServerProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.GuildAvatarUrl;
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ProfilePictureButton));
                        }
                        else if (e.Interaction.Data.CustomId == ProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.AvatarUrl;
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton));
                        }

                        try
                        {
                            await Task.Delay(60000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        }
                        catch { }
                    }
                }).Add(_bot._watcher, ctx);
            }

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }
            catch { }
        }).Add(_bot._watcher, ctx);
    }



    [Command("banner"),
    CommandModule("user"),
    Description("Sends the user's banner as an embedded image")]
    public async Task Banner(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (victim is null)
            {
                victim = ctx.Member;
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{victim.Username}#{victim.Discriminator}'s Banner",
                    Url = victim.AvatarUrl
                },
                ImageUrl = victim.BannerUrl,
                Description = (string.IsNullOrWhiteSpace(victim.BannerUrl) ? "`This user has no banner.`" : ""),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = ColorHelper.Info
            };

            DiscordMember member = null;

            try { member = await victim.ConvertToMember(ctx.Guild); } catch { }

            var ServerBannerButton = new DiscordButtonComponent(ButtonStyle.Primary, "ShowServer", "Show Server Banner", (string.IsNullOrWhiteSpace(member?.GuildBannerHash)));
            var ProfileBannerButton = new DiscordButtonComponent(ButtonStyle.Primary, "ShowProfile", "Show Profile Banner", false);

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerBannerButton);

            var msg = await ctx.Channel.SendMessageAsync(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        if (e.Interaction.Data.CustomId == ServerBannerButton.CustomId)
                        {
                            embed.ImageUrl = member.GuildBannerUrl;
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ProfileBannerButton));
                        }
                        else if (e.Interaction.Data.CustomId == ProfileBannerButton.CustomId)
                        {
                            embed.ImageUrl = member.BannerUrl;
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerBannerButton));
                        }

                        try
                        {
                            await Task.Delay(60000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                        }
                        catch { }
                    }
                }).Add(_bot._watcher, ctx);
            }

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }
            catch { }
        }).Add(_bot._watcher, ctx);
    }



    [Command("rank"), Aliases("level", "lvl"),
    CommandModule("user"),
    Description("Shows you your current level and progress")]
    public async Task RankCommand(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            if (!_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience)
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

            long current = (long)Math.Floor((decimal)(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Experience - _bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Level - 1)));
            long max = (long)Math.Floor((decimal)(_bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Level) - _bot._experienceHandler.CalculateLevelRequirement(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Level - 1)));

            _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"Experience • {ctx.Guild.Name}",
                    IconUrl = ctx.Guild.IconUrl
                },
                Description = $"{(victim.Id == ctx.User.Id ? "You're" : $"{victim.Mention} is")} currently **Level {_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Level.DigitsToEmotes()} with `{_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Experience.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}` XP**\n\n" +
                              $"**Level {(_bot._guilds.List[ctx.Guild.Id].Members[victim.Id].Level + 1).DigitsToEmotes()} Progress**\n" +
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
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

            if (!_bot._guilds.List[ctx.Guild.Id].ExperienceSettings.UseExperience)
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

            foreach (var b in _bot._guilds.List[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience))
            {
                currentuserplacement++;
                if (b.Key == ctx.User.Id)
                    break;
            }

            var members = await ctx.Guild.GetAllMembersAsync();

            List<KeyValuePair<string, string>> Board = new();

            foreach (var b in _bot._guilds.List[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience))
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
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

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
                    Task.Run(async () =>
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Message.Id == tos_accept.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            _bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS = true;

                            var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")));
                            await tos_accept.ModifyAsync(new DiscordMessageBuilder().WithEmbed(tos_embed.WithColor(ColorHelper.Success)).AddComponents(accepted_button));

                            _ = ctx.Command.ExecuteAsync(ctx);

                            _ = Task.Delay(10000).ContinueWith(x =>
                            {
                                _ = tos_accept.DeleteAsync();
                            });
                        }
                    }).Add(_bot._watcher, ctx);
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

            if (_bot._submittedUrls.List.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(_bot._status))
            {
                if (_bot._submittedUrls.List.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 10)
                {
                    embed.Description = $"`You have 10 open url submissions. Please wait before trying to submit another url.`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return; 
                }
            }

            if (_bot._submissionBans.Users.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`You are banned from submitting URLs.`\n" +
                                    $"`Reason: {_bot._submissionBans.Users[ctx.User.Id].Reason}`";
                embed.Color = ColorHelper.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            if (_bot._submissionBans.Guilds.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                    $"`Reason: {_bot._submissionBans.Guilds[ctx.Guild.Id].Reason}`";
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
                embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') you're trying to submit is invalid.`";
                embed.Color = ColorHelper.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            embed.Description = $"`You are about to submit the domain '{domain.SanitizeForCodeBlock()}'. When submitting, your public user account and guild will be tracked and visible to verifiers. Do you want to proceed?`";
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
                    Task.Run(async () =>
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
                                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') is already present in the database. Thanks for trying to contribute regardless.`";
                                        embed.Color = ColorHelper.Error;
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        _ = msg.ModifyAsync(embed.Build());
                                        return;
                                    }
                                }

                                embed.Description = $"`Checking if your domain has already been submitted before..`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                foreach (var b in _bot._submittedUrls.List)
                                {
                                    if (b.Value.Url == domain)
                                    {
                                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') has already been submitted. Thanks for trying to contribute regardless.`";
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
                                    Description = $"`Submitted Url`: `{domain.SanitizeForCodeBlock()}`\n" +
                                                    $"`Submission by`: `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n" +
                                                    $"`Submitted on `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                                }).AddComponents(new List<DiscordComponent>
                                    {
                                        { continue_button },
                                        { cancel_button },
                                        { ban_user_button },
                                        { ban_guild_button },
                                    }));

                                _bot._submittedUrls.List.Add(subbmited_msg.Id, new SubmittedUrls.UrlInfo
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
                    }).Add(_bot._watcher, ctx);
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
        }).Add(_bot._watcher, ctx);
    }



    [Command("emoji"), Aliases("emojis", "emote", "steal", "grab", "sticker", "stickers"),
    CommandModule("user"),
    Description("Steals emojis of the message that this command was replied to")]
    public async Task EmojiStealer(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

            ulong messageid;

            if (ctx.Message.ReferencedMessage is not null)
            {
                messageid = ctx.Message.ReferencedMessage.Id;
            }
            else
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Downloading emojis of this message..`",
                Color = ColorHelper.Processing,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg = await ctx.Message.ReferencedMessage.RespondAsync(embed: embed);

            HttpClient client = new();

            var bMessage = await ctx.Channel.GetMessageAsync(Convert.ToUInt64(messageid));

            List<string> EmoteList = new();
            Dictionary<ulong, EmojiStealer> SanitizedEmoteList = new();

            List<string> Emotes = bMessage.Content.GetEmotes();
            List<string> AnimatedEmotes = bMessage.Content.GetAnimatedEmotes();

            if (Emotes is null && AnimatedEmotes is null && (bMessage.Stickers is null || bMessage.Stickers.Count == 0))
            {
                embed.Description = $"`This message doesn't contain any emojis or stickers.`";
                embed.Color = ColorHelper.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await msg.ModifyAsync(embed: embed.Build());

                return;
            }

            if (Emotes is not null)
                foreach (var b in Emotes)
                    if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                        SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = false, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value, Type = EmojiType.EMOJI });

            if (AnimatedEmotes is not null)
                foreach (var b in AnimatedEmotes)
                    if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                        SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = true, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value, Type = EmojiType.EMOJI });

            bool ContainsStickers = bMessage.Stickers.Count > 0;

            string guid = Guid.NewGuid().ToString().MakeValidFileName();

            if (Directory.Exists($"emotes-{guid}"))
                Directory.Delete($"emotes-{guid}", true);

            Directory.CreateDirectory($"emotes-{guid}");

            if (SanitizedEmoteList.Count > 0)
            {
                embed.Description = $"`Downloading {SanitizedEmoteList.Count} emojis of this message..`";
                await msg.ModifyAsync(embed: embed.Build());

                foreach (var b in SanitizedEmoteList.ToList())
                {
                    try
                    {
                        var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.{(b.Value.Animated ? "gif" : "png")}");

                        string FileExists = "";
                        int FileExistsInt = 1;

                        string fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');

                        while (File.Exists($"emotes-{guid}/{fileName}"))
                        {
                            FileExistsInt++;
                            FileExists = $" ({FileExistsInt})";

                            fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');
                        }

                        using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                        {
                            EmoteStream.CopyTo(fileStream);
                            await fileStream.FlushAsync();
                        }

                        SanitizedEmoteList[b.Key].Path = $"emotes-{guid}/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to download an emote", ex);
                        SanitizedEmoteList.Remove(b.Key);
                    }
                } 
            }

            if (ContainsStickers)
            {
                embed.Description = $"`Downloading {bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()).Count()} stickers of this message..`";
                await msg.ModifyAsync(embed: embed.Build());

                foreach (var b in bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()))
                {
                    var StickerStream = await client.GetStreamAsync(b.Url);

                    string FileExists = "";
                    int FileExistsInt = 1;

                    string fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');

                    while (File.Exists($"emotes-{guid}/{fileName}"))
                    {
                        FileExistsInt++;
                        FileExists = $" ({FileExistsInt})";

                        fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');
                    }

                    using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                    {
                        StickerStream.CopyTo(fileStream);
                        await fileStream.FlushAsync();
                    }

                    SanitizedEmoteList.Add(b.Id, new EmojiStealer { Animated = false, Name = b.Name, Path = $"emotes-{guid}/{fileName}", Type = EmojiType.STICKER });
                }
            }

            if (SanitizedEmoteList.Count == 0)
            {
                embed.Description = $"`Couldn't download any emojis or stickers from this message.`";
                embed.Color = ColorHelper.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await msg.ModifyAsync(embed: embed.Build());

                return;
            }

            string emojiText = "";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                emojiText += "emojis";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                emojiText += $"{(emojiText.Length > 0 ? " and stickers" : "stickers")}";

            embed.Author.IconUrl = ctx.Guild.IconUrl;
            embed.Description = $"`Select how you want to receive the downloaded {emojiText}.`";

            bool IncludeStickers = false;

            if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                IncludeStickers = true;

            var IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", !SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
            
            var AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:")));
            var ZipPrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "ZipPrivateMessage", "Direct Message as Zip File", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":desktop:")));
            var SinglePrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "SinglePrivateMessage", "Direct Message as Single Files", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":mobile_phone:")));

            var SendHereButton = new DiscordButtonComponent(ButtonStyle.Secondary, "SendHere", "In this chat as Zip File", !(ctx.Member.Permissions.HasPermission(Permissions.AttachFiles)), new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":speech_balloon:")));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                builder.AddComponents(IncludeStickersButton);

            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

            await msg.ModifyAsync(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        cancellationTokenSource.Cancel();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == AddToServerButton.CustomId)
                        {
                            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
                            {
                                _ = ctx.SendPermissionError(Permissions.ManageEmojisAndStickers);
                                return;
                            }
                            
                            if (IncludeStickers)
                            {
                                embed.Description = $"`You cannot add any emoji(s) to the server while including stickers.`";
                                embed.Color = ColorHelper.Error;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                await msg.ModifyAsync(embed: embed.Build());

                                return;
                            }

                            bool Pinned = false;
                            bool DiscordWarning = false;

                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Added 0/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                try
                                {
                                    Task task;

                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        task = ctx.Guild.CreateEmojiAsync(SanitizedEmoteList.ElementAt(i).Value.Name, fileStream);
                                    }

                                    int WaitSeconds = 0;

                                    while (task.Status == TaskStatus.WaitingForActivation)
                                    {
                                        WaitSeconds++;

                                        if (WaitSeconds > 10 && !DiscordWarning)
                                        {
                                            embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`\n\n" +
                                                                                $"_The bot most likely hit a rate limit. This can take up to an hour to continue._\n" +
                                                                                $"_If you want this to change, go scream at Discord. There's nothing i can do._";
                                            await msg.ModifyAsync(embed: embed.Build());

                                            DiscordWarning = true;

                                            try
                                            {
                                                await msg.PinAsync();
                                                Pinned = true;
                                            }
                                            catch { }
                                        }
                                        await Task.Delay(1000);
                                    }


                                    if (Pinned)
                                        try
                                        {
                                            await msg.UnpinAsync();
                                            Pinned = false;
                                        }
                                        catch { }

                                    embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
                                    await msg.ModifyAsync(embed: embed.Build());
                                }
                                catch (Exception ex)
                                {
                                    LogError($"Failed to add an emote to guild", ex);
                                }
                            }

                            embed.Thumbnail = null;
                            embed.Color = ColorHelper.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $":white_check_mark: `Downloaded and added {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to the server.`";
                            await msg.ModifyAsync(embed: embed.Build());
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == SinglePrivateMessageButton.CustomId)
                        {
                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Sending the {emojiText} in your DMs..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            try
                            {
                                for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                {
                                    if (!IncludeStickers)
                                        if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                            continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"`{i + 1}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())}` `{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}`").WithFile($"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}", fileStream));
                                    }

                                    await Task.Delay(1000);
                                }

                                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"Heyho! Here's the {emojiText} you requested."));
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

                                await msg.ModifyAsync(embed: errorembed.Build());
                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }

                            embed.Thumbnail = null;
                            embed.Color = ColorHelper.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $":white_check_mark: `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                            await msg.ModifyAsync(embed: embed.Build());
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId || e.Interaction.Data.CustomId == SendHereButton.CustomId)
                        {
                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Preparing your Zip File..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            if (Directory.Exists($"zipfile-{guid}"))
                                Directory.Delete($"zipfile-{guid}", true);

                            Directory.CreateDirectory($"zipfile-{guid}");

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                if (!IncludeStickers)
                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                string NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";

                                int FileExistsInt = 1;

                                while (File.Exists($"zipfile-{guid}/{NewFilename}"))
                                {
                                    FileExistsInt++;
                                    NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}_{FileExistsInt}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";
                                }

                                File.Copy(SanitizedEmoteList.ElementAt(i).Value.Path, $"zipfile-{guid}/{NewFilename}");
                            }

                            ZipFile.CreateFromDirectory($"zipfile-{guid}", $"Emotes-{guid}.zip");

                            if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId)
                            {
                                embed.Description = $"`Sending your Zip File in DMs..`";
                                await msg.ModifyAsync(embed: embed.Build());

                                try
                                {
                                    using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emojis.zip", fileStream).WithContent("Heyho! Here's the emojis you requested."));
                                    }
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

                                    await msg.ModifyAsync(embed: errorembed.Build());
                                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                    return;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                                embed.Thumbnail = null;
                                embed.Color = ColorHelper.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $":white_check_mark: `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                                await msg.ModifyAsync(embed: embed.Build());
                            }
                            else if (e.Interaction.Data.CustomId == SendHereButton.CustomId)
                            {
                                if (!ctx.Member.Permissions.HasPermission(Permissions.AttachFiles))
                                {
                                    _ = ctx.SendPermissionError(Permissions.AttachFiles);
                                    return;
                                }

                                embed.Description = $"`Sending your Zip File..`";
                                await msg.ModifyAsync(embed: embed.Build());

                                embed.Thumbnail = null;
                                embed.Color = ColorHelper.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $":white_check_mark: `Downloaded {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText}. Attached is a Zip File containing them.`";

                                using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                {
                                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream).WithEmbed(embed.Build()));
                                }

                                await msg.DeleteAsync();
                            }
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == IncludeStickersButton.CustomId)
                        {
                            IncludeStickers = !IncludeStickers;

                            if (!IncludeStickers)
                            {
                                if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                                    IncludeStickers = true;
                            }

                            IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
                            AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_plus_sign:")));

                            var builder = new DiscordMessageBuilder().WithEmbed(embed);

                            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                                builder.AddComponents(IncludeStickersButton);

                            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                            await msg.ModifyAsync(builder);
                        }

                        cancellationTokenSource = new();
                        ctx.Client.ComponentInteractionCreated += RunInteraction;

                        try
                        {
                            await Task.Delay(60000, cancellationTokenSource.Token);
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                    }

                }).Add(_bot._watcher, ctx);
            }

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);
                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        }).Add(_bot._watcher, ctx);
    }
}
