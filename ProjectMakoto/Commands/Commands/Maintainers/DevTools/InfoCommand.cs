// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class InfoCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Fetching system details..`").AsLoading(ctx));

            var history = ctx.Bot.MonitorClient.GetHistory();

            var ServerUptime = "";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
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

                var Output = b.StandardOutput.ReadToEnd();
                ServerUptime = Output.Remove(Output.IndexOf(','), Output.Length - Output.IndexOf(',')).TrimStart();
            }

            IEnumerable<string> bFile;

            try
            {
                bFile = File.ReadLines("LatestGitPush.cfg");
            }
            catch (Exception)
            {
                bFile = new List<string>
                {
                     "Developer Version",
                     "dev",
                    $"{DateTime.UtcNow:dd.MM.yy}",
                    $"{DateTime.UtcNow:HH:mm:ss},00"
                };
            }

            var Version = bFile.First().Trim();
            var Branch = bFile.Skip(1).First().Trim();
            var Date = bFile.Skip(2).First().Trim().Replace("/", ".");

            var Time = bFile.Skip(3).First().Trim();
            Time = Time[..Time.IndexOf(",")];

            var miscEmbed = new DiscordEmbedBuilder().WithTitle($"{ctx.CurrentUser.GetUsername()} Details")
                .AddField(new DiscordEmbedField("Currently running as", $"`{ctx.CurrentUser.GetUsernameWithIdentifier()}`"))
                .AddField(new DiscordEmbedField("Currently running software", $"`Project Makoto by {(await ctx.Client.GetUserAsync(411950662662881290)).GetUsernameWithIdentifier()} ({Version} ({Branch}) built on the {Date} at {Time})`"))
                .AddField(new DiscordEmbedField("Process PID", $"`{Environment.ProcessId}`"))
                .AddField(new DiscordEmbedField("Current bot library and version", $"[`{ctx.Client.BotLibrary} {ctx.Client.VersionString}`](https://github.com/Aiko-IT-Systems/DisCatSharp)"))
                .AddField(new DiscordEmbedField("Bot uptime", $"`{Math.Round((DateTime.UtcNow - ctx.Bot.status.startupTime).TotalHours, 2)} hours`"))
                .AddField(new DiscordEmbedField("Discord API Latency", $"`{ctx.Client.Ping}ms`"))
                .AddField(new DiscordEmbedField("Server uptime", $"`{(ServerUptime.IsNullOrWhiteSpace() ? "Currently unavailable" : ServerUptime)}`"))
                .AsInfo(ctx).WithFooter().WithTimestamp(null);

            var cpuEmbed1 = new DiscordEmbedBuilder().WithTitle("CPU").WithDescription($"`Load        `: `{history.MaxBy(x => x.Key).Value.Cpu.Load.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),3}%`\n" +
                                                                                       $"`  (15m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(15)).Select(x => x.Value.Cpu.Load).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),3}%`\n" +
                                                                                       $"`  (30m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(30)).Select(x => x.Value.Cpu.Load).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),3}%`\n" +
                                                                                       $"`  (60m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(60)).Select(x => x.Value.Cpu.Load).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),3}%`").AsLoading(ctx).WithFooter().WithTimestamp(null).WithAuthor();

            var cpuEmbed2 = new DiscordEmbedBuilder().WithDescription($"`Temperature `: `{history.MaxBy(x => x.Key).Value.Cpu.Temperature.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),2}°C`\n" +
                                                                      $"`  (15m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(15)).Select(x => x.Value.Cpu.Temperature).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),2}°C`\n" +
                                                                      $"`  (30m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(30)).Select(x => x.Value.Cpu.Temperature).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),2}°C`\n" +
                                                                      $"`  (60m avg.)`: `{history.Reverse().TakeWhile(x => x.Key.GetTimespanSince() < TimeSpan.FromMinutes(60)).Select(x => x.Value.Cpu.Temperature).Average().ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),2}°C`\n").AsInfo(ctx).WithFooter().WithTimestamp(null).WithAuthor();


            var memoryEmbed = new DiscordEmbedBuilder().WithTitle("Memory").WithDescription($"`Usage`: `{history.MaxBy(x => x.Key).Value.Memory.Used.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}/{history.MaxBy(x => x.Key).Value.Memory.Total.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} MB`").AsLoading(ctx);

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(new List<DiscordEmbed>() { miscEmbed, cpuEmbed1, memoryEmbed }));

            try
            {
                var prev = "";
                Chart qc = new()
                {
                    Width = 1000,
                    Height = 500,
                    Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    {string.Join(",", history.Select(x => { var value = x.Key.GetTimespanSince().TotalMinutes.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")); if (prev == value) return "' '"; prev = value; return $"'{value}m ago'"; }))}
                                ],
                                datasets: 
                                [
                                    {{
                                        label: 'Usage (%)',
                                        data: [{string.Join(",", history.Select(x => $"{x.Value.Cpu.Load.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}"))}],
                                        fill: false,
                                        borderColor: getGradientFillHelper('vertical', ['#ff0000', '#00ff00']),
                                        id: ""yaxis2""
                                    }}
                                ]

                            }},
                            options:
                            {{
                                legend:
                                {{
                                    display: true,
                                }},
                                elements:
                                {{
                                    point:
                                    {{
                                        radius: 0
                                    }}
                                }},
                                scales: {{
                                    yAxes: [{{
                                    ticks: {{
                                        max: 100,
                                        min: 0
                                        }}
                                    }}]
                                }}
                            }}
                        }}"
                };

                var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GraphAssets))
                    .SendMessageAsync(new DiscordMessageBuilder().WithFile($"{Guid.NewGuid()}.png", new MemoryStream(qc.ToByteArray())));
                cpuEmbed1.ImageUrl = asset.Attachments[0].Url;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate cpu usage graph", ex);
            }
            finally
            {
                _ = cpuEmbed1.AsInfo(ctx).WithFooter().WithTimestamp(null).WithAuthor();
            }

            if (history.MaxBy(x => x.Key).Value.Cpu.Temperature != 0)
                try
                {
                    var prev = "";
                    Chart qc = new()
                    {
                        Width = 1000,
                        Height = 500,
                        Config = $@"{{
                                type: 'line',
                                data: 
                                {{
                                    labels: 
                                    [
                                        {string.Join(",", history.Select(x => { var value = x.Key.GetTimespanSince().TotalMinutes.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")); if (prev == value) return "' '"; prev = value; return $"'{value}m ago'"; }))}
                                    ],
                                    datasets: 
                                    [
                                        {{
                                            label: 'Temperature (°C)',
                                            data: [{string.Join(",", history.Select(x => $"{x.Value.Cpu.Temperature.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}"))}],
                                            fill: false,
                                            borderColor: getGradientFillHelper('vertical', ['#ff0000', '#00ff00']),
                                            id: ""yaxis2""
                                        }}
                                    ]

                                }},
                                options:
                                {{
                                    legend:
                                    {{
                                        display: true,
                                    }},
                                    elements:
                                    {{
                                        point:
                                        {{
                                            radius: 0
                                        }}
                                    }},
                                    scales: {{
                                        yAxes: [{{
                                        ticks: {{
                                            max: 100,
                                            min: 0
                                            }}
                                        }}]
                                    }}
                                }}
                            }}"
                    };

                    var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GraphAssets))
                        .SendMessageAsync(new DiscordMessageBuilder().WithFile($"{Guid.NewGuid()}.png", new MemoryStream(qc.ToByteArray())));
                    cpuEmbed2.ImageUrl = asset.Attachments[0].Url;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to generate cpu temp graph", ex);
                }

            try
            {
                var prev = "";
                Chart qc = new()
                {
                    Width = 1000,
                    Height = 500,
                    Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    {string.Join(",", history.Select(x => { var value = x.Key.GetTimespanSince().TotalMinutes.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")); if (prev == value) return "' '"; prev = value; return $"'{value}m ago'"; }))}
                                ],
                                datasets: 
                                [
                                    {{
                                        label: 'Usage (MB)',
                                        data: [{string.Join(",", history.Select(x => $"{x.Value.Memory.Used.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")).Replace(",", "").Replace(".", "")}"))}],
                                        fill: false,
                                        borderColor: getGradientFillHelper('vertical', ['#ff0000', '#00ff00']),
                                        id: ""yaxis2""
                                    }}
                                ]

                            }},
                            options:
                            {{
                                legend:
                                {{
                                    display: true,
                                }},
                                elements:
                                {{
                                    point:
                                    {{
                                        radius: 0
                                    }}
                                }},
                                scales: {{
                                    yAxes: [{{
                                    ticks: {{
                                        max: {history.First().Value.Memory.Total.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")).Replace(",", "").Replace(".", "")},
                                        min: 0
                                        }}
                                    }}]
                                }}
                            }}
                        }}"
                };

                var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GraphAssets))
                    .SendMessageAsync(new DiscordMessageBuilder().WithFile($"{Guid.NewGuid()}.png", new MemoryStream(qc.ToByteArray())));
                memoryEmbed.ImageUrl = asset.Attachments[0].Url;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate memory graph", ex);
            }
            finally
            {
                _ = memoryEmbed.AsInfo(ctx).WithAuthor();
            }

            var list = new List<DiscordEmbed>();
            list.Add(miscEmbed);
            list.Add(cpuEmbed1);

            if (!cpuEmbed2.ImageUrl.IsNullOrWhiteSpace())
                list.Add(cpuEmbed2);

            list.Add(memoryEmbed);

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(list));
        });
    }
}
