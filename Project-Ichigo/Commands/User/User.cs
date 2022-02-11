namespace Project_Ichigo.Commands.User;
internal class User : BaseCommandModule
{
    public Status _status { private get; set; }
    public Users _users { private get; set; }
    public SubmissionBans _submissionBans { private get; set; }
    public PhishingUrls _phishingUrls { private get; set; }
    public SubmittedUrls _submittedUrls { private get; set; }



    [Command("help"),
    CommandModule("user"),
    Description("Shows all available commands, their usage and their description.")]
    public async Task Help(CommandContext ctx)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                List<KeyValuePair<string, string>> Commands = new();


                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "user")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("User Commands", x)).ToList());

                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "music")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Music Commands", x)).ToList());

                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "mod")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Mod Commands", x)).ToList());

                if (ctx.Member.IsAdmin(_status))
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "admin")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Admin Commands", x)).ToList());

                if (ctx.Member.IsMaintenance(_status))
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "maintainence")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Maintenance Commands", x)).ToList());

                if (ctx.Member.IsMaintenance(_status))
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "hidden")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Hidden Commands", x)).ToList());

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
                catch (Exception ex)
                {
                    LogError($"{ex}");

                    var errorembed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Description = ":x: `An unhandled exception occured while trying to send you a direct message. This error has been logged, please try again later.`",
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
            }
            catch (Exception ex)
            {
                LogError($"{ex}");
            }
        });
    }



    [Command("info"),
    CommandModule("user"),
    Description("Shows informations about the bot or the mentioned user.")]
    public async Task Info(CommandContext ctx, DiscordUser victim = null)
    {
        _ = Task.Run(async () =>
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

            embed.AddField("General info", "󠂪 󠂪", true);
            embed.Fields.First(x => x.Name == "General info").Value = "\n_All dates follow `DD.MM.YYYY` while the time zone is set to the `Coordinated Universal Time (UTC), +00:00`._\n";

            embed.AddField("Guild", "󠂪 󠂪", true);
            embed.Fields.First(x => x.Name == "Guild").Value = "**Guild name**\n`Loading..`\n" +
                                                                "**Guild created at**\n`Loading..`\n" +
                                                                "**Owner of this guild**\n`Loading..`\n" +
                                                                "**Current member count**\n`Loading..`";

            embed.AddField("Bot", "󠂪 󠂪", true);
            embed.Fields.First(x => x.Name == "Bot").Value = "**Currently running as**\n`Loading..`\n" +
                                                                "**Currently running software**\n`Loading..`\n" +
                                                                "**Currently running on**\n`Loading..`\n" +
                                                                "**Current bot lib and version**\n`Loading..`\n" +
                                                                "**Bot uptime**\n`Loading..`\n" +
                                                                "**Current API Latency**\n`Loading..`";

            embed.AddField("Host", "󠂪 󠂪", true);
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
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project-Ichigo by TheXorog (GH-{bFile.First().Trim().Replace("/", ".")} ({bFile.Skip(1).First().Trim()}) built on the {bFile.Skip(2).First().Trim().Replace("/", ".")} at {bFile.Skip(3).First().Trim().Remove(bFile.Skip(3).First().Trim().IndexOf(","), bFile.Skip(3).First().Trim().Length - bFile.Skip(3).First().Trim().IndexOf(","))})`");
            }
            else
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project-Ichigo by TheXorog (GH-UNIDENTIFIED)`");

            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running on**\n`Loading..`", $"**Currently running on**\n`{Environment.OSVersion.Platform} with DOTNET-{Environment.Version}`");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Current bot lib and version**\n`Loading..`", $"**Current bot lib and version**\n[`{ctx.Client.BotLibrary} {ctx.Client.VersionString}`](https://github.com/Aiko-IT-Systems/DisCatSharp)");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Bot uptime**\n`Loading..`", $"**Bot uptime**\n`{Math.Round((DateTime.Now - _status.startupTime).TotalHours, 2)} hours`");
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
        });
    }



    [Command("user-info"), Aliases("userinfo"),
    CommandModule("user"),
    Description("Shows information about the mentioned user.")]
    public async Task UserInfo(CommandContext ctx, DiscordUser victim)
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
                embed.AddField("Roles", GenerateRoles.Truncate(1024), true);
            else
                embed.AddField($"Roles (Backup)", GenerateRoles.Truncate(1024), true);

            embed.AddField("Created at", $"`{victim.CreationTimestamp.ToUniversalTime():dd.MM.yyyy HH:mm:ss zzz}` ({Math.Round(TimeSpan.FromTicks(CreationAge.Ticks).TotalDays, 0)} days ago)", true);

            if (bMember is not null)
            {
                embed.AddField("Joined at", $"`{bMember.JoinedAt.ToUniversalTime():dd.MM.yyyy HH:mm:ss zzz}` ({Math.Round(TimeSpan.FromTicks(JoinedAtAge.Ticks).TotalDays, 0)} days ago)", true);
            }
            else
            {
                embed.AddField("󠂪 󠂪", $"󠂪 󠂪", true);
            }

            embed.AddField("First joined at", $"`Not yet implemented.`", true);

            embed.AddField("Invited by", $"`Not yet implemented.`", true);

            embed.AddField("Users invited", $"`Not yet implemented.`", true);

            await ctx.Channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            LogError($"Error occured while trying to generate info about a user: {ex}");
        }
    }



    [Command("avatar"), Aliases("pfp"),
    CommandModule("user"),
    Description("Sends the user's avatar as an embedded image.")]
    public async Task Avatar(CommandContext ctx, DiscordUser victim = null)
    {
        _ = Task.Run(async () =>
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
        });
    }



    [Command("submit-url"),
    CommandModule("user"),
    Description("Allows submission of new malicous urls to our database.")]
    public async Task UrlSubmit(CommandContext ctx, string url)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                if (!_users.List.ContainsKey(ctx.User.Id))
                    _users.List.Add(ctx.User.Id, new Users.Info());

                if (!_users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS)
                {
                    var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")));

                    var tos_embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                        Color = ColorHelper.Warning,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.Now,
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
                                _users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS = true;

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
                    Color = ColorHelper.Warning,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.Now,
                    Description = $"`Processing your request..`"
                };

                var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

                if (_users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(_status))
                {
                    embed.Description = $"`You cannot submit a domain for the next {_users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                if (_submissionBans.BannedUsers.ContainsKey(ctx.User.Id))
                {
                    embed.Description = $"`You are banned from submitting URLs.`\n" +
                                        $"`Reason: {_submissionBans.BannedUsers[ctx.User.Id].Reason}`";
                    embed.Color = ColorHelper.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }

                if (_submissionBans.BannedGuilds.ContainsKey(ctx.Guild.Id))
                {
                    embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                        $"`Reason: {_submissionBans.BannedGuilds[ctx.Guild.Id].Reason}`";
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

                                    foreach (var b in _phishingUrls.List)
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

                                    foreach (var b in _submittedUrls.Urls)
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
                                        Timestamp = DateTime.Now,
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

                                    _submittedUrls.Urls.Add(subbmited_msg.Id, new SubmittedUrls.UrlInfo
                                    {
                                        Url = domain,
                                        Submitter = ctx.User.Id,
                                        GuildOrigin = ctx.Guild.Id
                                    });

                                    _users.List[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

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
        });
    }
}
