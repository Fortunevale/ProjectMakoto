using ProjectMakoto.Entities.SystemMonitor;

namespace ProjectMakoto.Util.SystemMonitor;

internal class MonitorClient
{
    internal MonitorClient(Bot bot)
    {
        if (!bot.status.LoadedConfig.MonitorSystemStatus)
            return;

        this.InitializeMonitor();
    }

    internal IReadOnlyDictionary<DateTime, SystemInfo> GetHistory()
    {
        return History.OrderBy(x => x.Key.Ticks).ToDictionary(x => x.Key, x => x.Value);
    }
    
    internal async Task<SystemInfo> GetCurrent()
    {
        return await ReadSystemInfoAsync();
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
            while (true)
            {
                try
                {
                    LastScanStart = DateTime.UtcNow;
                    var sensors = await ReadSystemInfoAsync();

                    _logger.LogDebug(JsonConvert.SerializeObject(sensors, Formatting.Indented));

                    if (this.History.Count >= 180)
                        this.History.Remove(this.History.Min(x => x.Key));

                    
                    this.History.Add(DateTime.UtcNow, sensors);
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to fetch system info", ex);

                    LastScanStart = DateTime.UtcNow;
                    this.History.Add(DateTime.UtcNow, null);
                }

                var waitTime = LastScanStart.AddSeconds(20).GetTimespanUntil();

                if (waitTime < TimeSpan.FromSeconds(1))
                    waitTime = TimeSpan.FromSeconds(1);

                await Task.Delay(waitTime);
            }
        });
    }

    private static async Task<SystemInfo> ReadSystemInfoAsync()
    {
        return await Task.Run<SystemInfo>(() =>
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

                    var output = process.StandardOutput.ReadToEnd();
                    _logger.LogTrace("Executed sensors: {0}", output);

                    var matches = Regex.Matches(output, @"(((\w| )*): *(\+*[0-9]*.[0-9]*°C)(?!,|\)))");
                    Dictionary<string, float> tempsDict = new();

                    foreach (Match b in matches.Cast<Match>())
                    {
                        tempsDict.Add(b.Groups[2].Value, float.Parse(b.Groups[4].Value.Replace("+", "").Replace("°C", "")));
                    }

                    systemInfo.Cpu.Temperature = tempsDict.FirstOrDefault<KeyValuePair<string, float>>(x => x.Key.StartsWith("Package id"), new KeyValuePair<string, float>("", 0)).Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to execute sensors", ex);
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

                    _logger.LogTrace("Executed cpu usage: {0}", output);
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
}

