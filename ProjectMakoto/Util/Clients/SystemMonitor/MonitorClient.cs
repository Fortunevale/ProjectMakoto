// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.ObjectModel;
using ProjectMakoto.Entities.SystemMonitor;

namespace ProjectMakoto.Util.SystemMonitor;

public sealed class MonitorClient : RequiresBotReference
{
    internal MonitorClient(Bot bot) : base(bot)
    {
        if (!bot.status.LoadedConfig.MonitorSystem.Enabled)
            return;

        if (File.Exists("cache/monitor.json"))
            try
            {
                this.History = JsonConvert.DeserializeObject<Dictionary<DateTime, SystemInfo>>(File.ReadAllText("cache/monitor.json"));

                if (this.History is null)
                    throw new Exception();
            }
            catch (Exception)
            {
                this.History = new();
            }

        this.InitializeMonitor();
    }

    ~MonitorClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    private Dictionary<DateTime, SystemInfo> placeholder = new();

    public IReadOnlyDictionary<DateTime, SystemInfo> GetHistory()
    {
        if (!this.History.IsNotNullAndNotEmpty())
        {
            if (this.placeholder.Count == 0)
                for (var i = 0; i < 43200; i++)
                {
                    this.placeholder.Add(DateTime.UtcNow.AddSeconds((i * 2) * -1),
                        new SystemInfo
                        {
                            Cpu = new()
                            {
                                Load = new Random().Next(0, 50),
                                Temperature = new Random().Next(30, 50),
                            },
                            Memory = new()
                            {
                                Used = new Random().Next(0, 12000),
                                Total = 24000,
                            }
                        });
                }

            return this.placeholder.OrderBy(x => x.Key.Ticks).ToDictionary(x => x.Key, x => x.Value);
        }

        return this.History.OrderBy(x => x.Key.Ticks).ToDictionary(x => x.Key, x => x.Value);
    }

    public Task<SystemInfo> GetCurrent()
    {
        return this.ReadSystemInfoAsync();
    }

    private Dictionary<DateTime, SystemInfo> History = new();
    private DateTime LastScanStart = DateTime.UtcNow;

    private void InitializeMonitor()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                Log.Warning("Running under windows, system monitor unavailable.");
                return;
            }
        }

        _ = Task.Run(async () =>
        {
            while (!this._disposed)
            {
                try
                {
                    this.LastScanStart = DateTime.UtcNow;
                    var sensors = await this.ReadSystemInfoAsync();

                    Log.Debug(JsonConvert.SerializeObject(sensors, Formatting.Indented));

                    while (this.History.Any(x => x.Key.GetTimespanSince() > TimeSpan.FromDays(1)))
                        _ = this.History.Remove(this.History.Min(x => x.Key));

                    this.History.Add(DateTime.UtcNow, sensors);

                    _ = Directory.CreateDirectory("cache");
                    File.WriteAllText("cache/monitor.json", JsonConvert.SerializeObject(this.History));
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to fetch system info");

                    this.LastScanStart = DateTime.UtcNow;
                }

                var waitTime = this.LastScanStart.AddSeconds(2).GetTimespanUntil();

                if (waitTime < TimeSpan.FromSeconds(1))
                    waitTime = TimeSpan.FromSeconds(1);

                await Task.Delay(waitTime);
            }
        });
    }

    private Task<SystemInfo> ReadSystemInfoAsync()
    {
        return Task.Run<SystemInfo>(() =>
        {
            SystemInfo systemInfo = new();

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

                    var process = Process.Start(info);

                    process.WaitForExit();

                    var output = process.StandardOutput.ReadToEnd().ReplaceLineEndings("\n");
                    
                    var parsedSensors = this.ParseSensors(output);

                    systemInfo.Cpu.Temperature = parsedSensors
                        .FirstOrDefault(x => x.Key == this.Bot.status.LoadedConfig.MonitorSystem.SensorName).Value
                        .First(x => (x.Type == TrackType.C && x.Key == this.Bot.status.LoadedConfig.MonitorSystem.SensorKey)).Value;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to execute/parse sensors");
                }

                try
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "bash",
                        Arguments = $"-c \"awk '{{u=$2+$4; t=$2+$4+$5; if (NR==1){{u1=u; t1=t;}} else print ($2+$4-u1) * 100 / (t-t1); }}' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)\"",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    var process = Process.Start(info);

                    process.WaitForExit();

                    var output = process.StandardOutput.ReadToEnd();
                    systemInfo.Cpu.Load = float.Parse(output);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to execute cpu usage");
                }

                try
                {
                    var metrics = MemoryMetricsClient.GetMetrics();
                    systemInfo.Memory.Used = (float)metrics.Used;
                    systemInfo.Memory.Total = (float)metrics.Total;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to execute memory usage");
                }

                return systemInfo;
            }
            else
            {
                Log.Warning("Running on unknown operating system, system monitor not supported.");
                return systemInfo;
            }
        });
    }

    private enum TrackType
    {
        mV,
        V,
        RPM,
        C,
        Unknown
    }

    private class TrackDetail
    {
        internal string Key;
        internal decimal Value;
        internal TrackType Type;

        public override string ToString()
        {
            return $"{this.Key}, {this.Value}, {this.Type}";
        }
    }

    private ReadOnlyDictionary<string, List<TrackDetail>> ParseSensors(string sensorOutput)
    {
        Dictionary<string, List<TrackDetail>> parsedTemperatures = new();
        Dictionary<string, Tuple<int, int>> adapterRanges = new();
        Dictionary<string, List<string>> adapterList = new();

        var splitLines = sensorOutput.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < splitLines.Length; i++)
        {
            if (splitLines[i].IsNullOrWhiteSpace() || splitLines[i].Contains(':') || splitLines[i].StartsWith(' '))
                continue;

            if (adapterRanges.Count != 0)
                adapterRanges[adapterRanges.Last().Key] = new Tuple<int, int>(adapterRanges.Last().Value.Item1, i - 1);

            adapterRanges.Add(splitLines[i].Trim(), new Tuple<int, int>(i, i));
        }

        if (adapterRanges.Count != 0)
            adapterRanges[adapterRanges.Last().Key] = new Tuple<int, int>(adapterRanges.Last().Value.Item1, splitLines.Length);

        foreach (var adapter in adapterRanges)
            adapterList.Add(adapter.Key, splitLines.Skip(adapter.Value.Item1).Take(adapter.Value.Item2 - adapter.Value.Item1).ToList());

        foreach (var adapter in adapterList)
        {
            parsedTemperatures.Add(adapter.Key, new());
            var detail = parsedTemperatures[adapter.Key];

            foreach (var line in adapter.Value)
            {
                var match = Regex.Match(line, @"^([^:\n]+?): +((?:\+|\-)?[\d.]+?)(?: |°)(RPM|C|V|mV)(?: +?\(|\n| )?");

                if (!match.Success)
                    continue;

                detail.Add(new TrackDetail
                {
                    Key = match.Groups[1].Value,
                    Value = decimal.Parse(match.Groups[2].Value, new CultureInfo("en-US")),
                    Type = match.Groups[3].Value switch
                    {
                        "mV" => TrackType.mV,
                        "V" => TrackType.V,
                        "RPM" => TrackType.RPM,
                        "C" => TrackType.C,
                        _ => TrackType.Unknown,
                    }
                });
            }
        }

        return parsedTemperatures.AsReadOnly();
    }
}

