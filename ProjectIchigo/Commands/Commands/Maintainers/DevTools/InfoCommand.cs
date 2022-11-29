namespace ProjectIchigo.Commands;

internal class InfoCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = StatusIndicatorIcons.Loading,
                    Name = "Information about this server and bot"
                },
                Color = EmbedColors.Info,
                Description = "",
                Footer = ctx.GenerateUsedByFooter(),
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

            var msg = await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

            DateTime Age = new DateTime().AddSeconds((DateTime.UtcNow - ctx.Guild.CreationTimestamp).TotalSeconds);

            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Guild name**\n`Loading..`", $"**Guild name**\n`{ctx.Guild.Name}`");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Guild created at**\n`Loading..`", $"**Guild created at**\n`{ctx.Guild.CreationTimestamp.ToUniversalTime():dd.MM.yyyy HH:mm:ss}` ({Math.Round(TimeSpan.FromTicks(Age.Ticks).TotalDays, 0)} days ago)");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Owner of this guild**\n`Loading..`", $"**Owner of this guild**\n{ctx.Guild.Owner.Mention} `{ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}`");
            embed.Fields.First(x => x.Name == "Guild").Value = embed.Fields.First(x => x.Name == "Guild").Value.Replace("**Current member count**\n`Loading..`", $"**Current member count**\n`{ctx.Guild.MemberCount}/{ctx.Guild.MaxMembers}`");

            msg = await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running as**\n`Loading..`", $"**Currently running as**\n`{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");

            if (File.Exists("LatestGitPush.cfg"))
            {
                var bFile = File.ReadLines("LatestGitPush.cfg");
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project Ichigo by Mira#2000 (GH-{bFile.First().Trim().Replace("/", ".")} ({bFile.Skip(1).First().Trim()}) built on the {bFile.Skip(2).First().Trim().Replace("/", ".")} at {bFile.Skip(3).First().Trim().Remove(bFile.Skip(3).First().Trim().IndexOf(","), bFile.Skip(3).First().Trim().Length - bFile.Skip(3).First().Trim().IndexOf(","))})`");
            }
            else
                embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running software**\n`Loading..`", $"**Currently running software**\n`Project Ichigo by Mira#2000 (GH-UNIDENTIFIED)`");

            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Currently running on**\n`Loading..`", $"**Currently running on**\n`{Environment.OSVersion.Platform} with DOTNET-{Environment.Version}`");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Current bot lib and version**\n`Loading..`", $"**Current bot lib and version**\n[`{ctx.Client.BotLibrary} {ctx.Client.VersionString}`](https://github.com/Aiko-IT-Systems/DisCatSharp)");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Bot uptime**\n`Loading..`", $"**Bot uptime**\n`{Math.Round((DateTime.UtcNow - ctx.Bot.status.startupTime).TotalHours, 2)} hours`");
            embed.Fields.First(x => x.Name == "Bot").Value = embed.Fields.First(x => x.Name == "Bot").Value.Replace("**Current API Latency**\n`Loading..`", $"**Current API Latency**\n`{ctx.Client.Ping}ms`");

            msg = await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

            try
            {
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current CPU load**\n`Loading..`", $"**Current CPU load**\n`{Math.Round(await GetCpuUsageForProcess(), 2).ToString().Replace(",", ".")}%`");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get cpu load", ex);
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current CPU load**\n`Loading..`", $"**Current CPU load**\n`Error`");
            }

            try
            {
                var metrics = MemoryMetricsClient.GetMetrics();

                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current RAM usage**\n`Loading..`", $"**Current RAM usage**\n`{Math.Round(metrics.Used, 2)}/{Math.Round(metrics.Total, 2)}MB`");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get ram load", ex);
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current RAM usage**\n`Loading..`", "**Current RAM usage**\n`Error`");
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                try
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "bash",
                        Arguments = $"-c sensors",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    var b = Process.Start(info);

                    b.WaitForExit();

                    var matches = Regex.Matches(b.StandardOutput.ReadToEnd(), "(\\+*[0-9]*.[0-9]*°C(?!,|\\)))");

                    int _temp = 0;
                    decimal[] temps = new decimal[matches.Count];

                    foreach (var c in matches)
                    {
                        temps[_temp] = Convert.ToDecimal(c.ToString().Replace("°C", "").Replace("+", ""));
                        _temp++;
                    }

                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Avg: {temps.Average()}°C Max: {temps.Max()}°C Min: {temps.Min()}°C`");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get temps", ex);
                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Error`");
                }

                try
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "bash",
                        Arguments = $"-c uptime",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    var b = Process.Start(info);

                    b.WaitForExit();

                    string Output = b.StandardOutput.ReadToEnd();
                    Output = Output.Remove(Output.IndexOf(','), Output.Length - Output.IndexOf(',')).TrimStart();

                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`{Output}`");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get uptime", ex);
                    embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`Error`");
                }
            }
            else
            {
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Current temperature**\n`Loading..`", $"**Current temperature**\n`Currently unavailable`");
                embed.Fields.First(x => x.Name == "Host").Value = embed.Fields.First(x => x.Name == "Host").Value.Replace("**Server uptime**\n`Loading..`", $"**Server uptime**\n`Currently unavailable`");
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;
            msg = await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }
}
