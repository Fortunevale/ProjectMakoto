// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.SystemMonitor;

namespace ProjectMakoto.Util.SystemMonitor;

public sealed class MonitorClient : RequiresBotReference
{
    internal MonitorClient(Bot bot) : base(bot)
    {
        if (!bot.status.LoadedConfig.MonitorSystem.Enabled)
            return;

        this.InitializeMonitor();
    }

    ~MonitorClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    public IReadOnlyDictionary<DateTime, SystemInfo> GetHistory()
    {
        if (!this.History.IsNotNullAndNotEmpty())
            return new Dictionary<DateTime, SystemInfo>().AsReadOnly();

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
                _logger.LogWarn("Running under windows, system monitor unavailable.");
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

                    _logger.LogDebug(JsonConvert.SerializeObject(sensors, Formatting.Indented));

                    while (this.History.Any(x => x.Key.GetTimespanSince() > TimeSpan.FromDays(1)))
                        _ = this.History.Remove(this.History.Min(x => x.Key));

                    this.History.Add(DateTime.UtcNow, sensors);
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to fetch system info", ex);

                    this.LastScanStart = DateTime.UtcNow;
                    this.History.Add(DateTime.UtcNow, null);
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
                    _logger.LogWarn("Failed to execute/parse sensors", ex);
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
                    _logger.LogWarn("Failed to execute cpu usage", ex);
                }

                try
                {
                    var metrics = MemoryMetricsClient.GetMetrics();
                    systemInfo.Memory.Used = (float)metrics.Used;
                    systemInfo.Memory.Total = (float)metrics.Total;
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to execute memory usage", ex);
                }

                return systemInfo;
            }
            else
            {
                _logger.LogWarn("Running on unknown operating system, system monitor not supported.");
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

    private IReadOnlyDictionary<string, List<TrackDetail>> ParseSensors(string sensorOutput)
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

