namespace Project_Ichigo.Commands.User;
internal class User : BaseCommandModule
{
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

                if (ctx.Member.IsAdmin())
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "admin")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Admin Commands", x)).ToList());

                if (ctx.Member.IsMaintenance())
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "maintainence")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Maintenance Commands", x)).ToList());

                if (ctx.Member.IsMaintenance())
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



    [Command("avatar"), Aliases("pfp"),
    CommandModule("user"),
    Description("Sends the avatar of the user as an image.")]
    public async Task AvatarCommand(CommandContext ctx, DiscordUser victim = null)
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
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Bot uptime**\n`Loading..`", $"**Bot uptime**\n`{Math.Round((DateTime.Now - Bot._status.startupTime).TotalHours, 2)} hours`");
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
}
