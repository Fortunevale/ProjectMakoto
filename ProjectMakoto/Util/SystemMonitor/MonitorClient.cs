using LibreHardwareMonitor.Hardware;
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
        return new Dictionary<DateTime, SystemInfo>(History);
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
                _logger.LogWarn("Running under windows, system monitor partially unavailable unless running as administrator.");
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

                    if (this.History.Count > 60)
                        this.History.Remove(this.History.First<KeyValuePair<DateTime, SystemInfo>>().Key);

                    
                    this.History.Add(DateTime.UtcNow, sensors);
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to fetch system info", ex);

                    LastScanStart = DateTime.UtcNow;
                    this.History.Add(DateTime.UtcNow, null);
                }

                var waitTime = LastScanStart.AddSeconds(60).GetTimespanUntil();

                if (waitTime < TimeSpan.FromSeconds(1))
                    waitTime = TimeSpan.FromSeconds(1);

                await Task.Delay(waitTime);
            }
        });
    }

    private static async Task<SystemInfo> ReadSystemInfoAsync()
    {
        return await Task.Run(() =>
        {
            SystemInfo systemInfo = new();

            UpdateVisitor updateVisitor = new();
            Computer computer = new()
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = true,
            };

            try
            {
                computer.Open();

                computer.Accept(updateVisitor);

                _logger.LogTrace(JsonConvert.SerializeObject(computer.Hardware, Formatting.Indented));

                foreach (IHardware hw in computer.Hardware)
                {
                    foreach (ISensor sensor in hw.Sensors)
                    {
                        if (hw.HardwareType == HardwareType.Cpu)
                            if (sensor.Name is "CPU Total" or "Core (Tctl/Tdie)")
                                switch (sensor.SensorType)
                                {
                                    case SensorType.Load:
                                        systemInfo.Cpu.Load = sensor.Value.GetValueOrDefault(0);

                                        break;
                                    case SensorType.Temperature:
                                        systemInfo.Cpu.Temperature = sensor.Value.GetValueOrDefault(0);

                                        break;
                                }

                        if (hw.HardwareType == HardwareType.Memory)
                                switch (sensor.Name)
                                {
                                    case "Memory Available":
                                        systemInfo.Memory.Available = sensor.Value.GetValueOrDefault(0);
                                        break;
                                
                                    case "Memory Used":
                                        systemInfo.Memory.Used = sensor.Value.GetValueOrDefault(0);
                                        break;
                                }
                        
                        if (hw.HardwareType == HardwareType.Network && hw.Name == "Ethernet")
                                switch (sensor.Name)
                                {
                                    case "Data Uploaded":
                                        systemInfo.Network.TotalUploaded = sensor.Value.GetValueOrDefault(0);
                                        break;
                                
                                    case "Data Downloaded":
                                            systemInfo.Network.TotalDownloaded = sensor.Value.GetValueOrDefault(0);
                                            break;

                                    case "Upload Speed":
                                            systemInfo.Network.CurrentUploadSpeed = sensor.Value.GetValueOrDefault(0);
                                            break;

                                    case "Download Speed":
                                            systemInfo.Network.CurrentDownloadSpeed = sensor.Value.GetValueOrDefault(0);
                                            break;

                                    case "Network Utilization":
                                                systemInfo.Network.TotalUtilization = sensor.Value.GetValueOrDefault(0);
                                                break;
                                }
                    }
                }
            }
            finally
            {
                computer.Close();
            }

            return systemInfo;
        });
    }

    private class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}

