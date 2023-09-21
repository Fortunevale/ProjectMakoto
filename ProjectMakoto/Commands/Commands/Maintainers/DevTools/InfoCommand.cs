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

            Dictionary<DateTime, Entities.SystemMonitor.SystemInfo> history = new();

            try
            {
                var rawHistory = ctx.Bot.MonitorClient.GetHistory().GroupBy(x => $"{x.Key.Hour}-{x.Key.Minute}");
                foreach (var entry in rawHistory)
                {
                    history.Add(entry.Last().Key, new()
                    {
                        Cpu = new()
                        {
                            Load = entry.Average(x => x.Value.Cpu.Load),
                            Temperature = entry.Average(x => x.Value.Cpu.Temperature)
                        },
                        Memory = new()
                        {
                            Available = entry.Average(x => x.Value.Memory.Available),
                            Used = entry.Average(x => x.Value.Memory.Used),
                        }
                    });
                }
            }
            catch {}

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
                .AddField(new DiscordEmbedField("Currently running as", $"`{ctx.CurrentUser.GetUsernameWithIdentifier()}`", true))
                .AddField(new DiscordEmbedField("Process PID", $"`{Environment.ProcessId}`", true))
                .AddField(new DiscordEmbedField("󠂪 󠂪", $"󠂪 󠂪", true))
                .AddField(new DiscordEmbedField("Bot uptime", $"`{Math.Round((DateTime.UtcNow - ctx.Bot.status.startupTime).TotalHours, 2)} hours`", true))
                .AddField(new DiscordEmbedField("Discord API Latency", $"`{ctx.Client.Ping}ms`", true))
                .AddField(new DiscordEmbedField("Server uptime", $"`{(ServerUptime.IsNullOrWhiteSpace() ? "Currently unavailable" : ServerUptime)}`", true))
                .AddField(new DiscordEmbedField("Currently running software", $"`Project Makoto by {(await ctx.Client.GetUserAsync(411950662662881290)).GetUsernameWithIdentifier()} ({Version} ({Branch}) built on the {Date} at {Time})`"))
                .AddField(new DiscordEmbedField("Current bot library and version", $"[`{ctx.Client.BotLibrary} {ctx.Client.VersionString}`](https://github.com/Aiko-IT-Systems/DisCatSharp)"))
                .AsInfo(ctx).WithFooter().WithTimestamp(null);

            var cpuEmbed1 = new DiscordEmbedBuilder()
                .WithTitle("CPU")
                .AddField(new DiscordEmbedField("Load", $"`{history.MaxBy(x => x.Key).Value.Cpu.Load.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),3}%`", true))
                .AddField(new DiscordEmbedField("Temperature", $"`{history.MaxBy(x => x.Key).Value.Cpu.Temperature.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")),2}°C`", true))
                .AsLoading(ctx).WithFooter().WithTimestamp(null).WithAuthor();

            var memoryEmbed = new DiscordEmbedBuilder()
                .WithTitle("Memory")
                .AddField(new DiscordEmbedField("Usage", $"`{history.MaxBy(x => x.Key).Value.Memory.Used.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}/{history.MaxBy(x => x.Key).Value.Memory.Total.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} MB`", true))
                .AsLoading(ctx);

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(new List<DiscordEmbed>() { miscEmbed, cpuEmbed1, memoryEmbed }));

            Dictionary<string, Chart> charts = new();

            try
            {
                var prev = "";
                Chart qc = new()
                {
                    Width = 800,
                    Height = 600,
                    Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    {string.Join(",", history.Select(x =>
                                    {
                                        var value = x.Key.ToString("HH:mm", CultureInfo.CreateSpecificCulture("en-US"));
                                        if (prev == value)
                                            return "' '";
                                        prev = value;
                                        return $"'{value}'";
                                    }))}
                                ],
                                datasets: 
                                [
                                    {{
                                        label: 'Usage (%)',
                                        data: [{string.Join(",", history.Select(x => $"{x.Value.Cpu.Load.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}"))}],
                                        fill: false,
                                        borderColor: getGradientFillHelper('vertical', ['#ff0000', '#00ff00']),
                                        id: ""yaxis2""
                                    }},
                                    {{
                                        label: 'Temperature (°C)',
                                        data: [{string.Join(",", history.Select(x => $"{x.Value.Cpu.Temperature.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}"))}],
                                        fill: false,
                                        borderColor: getGradientFillHelper('vertical', ['#4287f5', '#ff0000']),
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

                charts.Add("cpu.png", qc);
                cpuEmbed1.ImageUrl = "attachment://cpu.png";
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate cpu usage graph", ex);
            }
            finally
            {
                _ = cpuEmbed1.AsInfo(ctx).WithFooter().WithTimestamp(null).WithAuthor();
            }

            try
            {
                var prev = "";
                Chart qc = new()
                {
                    Width = 800,
                    Height = 600,
                    Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    {string.Join(",", history.Select(x =>
                                    {
                                        var value = x.Key.ToString("HH:mm", CultureInfo.CreateSpecificCulture("en-US"));
                                        if (prev == value)
                                            return "' '";
                                        prev = value;
                                        return $"'{value}'";
                                    }))}
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

                charts.Add("mem.png", qc);
                memoryEmbed.ImageUrl = "attachment://mem.png";
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
            list.Add(miscEmbed.WithImageUrl("attachment://1.png"));
            list.Add(cpuEmbed1);
            list.Add(memoryEmbed);

            var files = charts.ToDictionary(x => x.Key, y => (Stream)new MemoryStream(y.Value.ToByteArray()));
            try
            {
                files.Add("1.png", new FileStream("Assets/1.png", FileMode.Open));
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(list).WithFiles(files));
            }
            finally
            {
                foreach (var file in files)
                    file.Value.Dispose();
            }
        });
    }
}
