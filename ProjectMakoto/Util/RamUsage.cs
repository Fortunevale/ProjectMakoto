namespace ProjectMakoto.Util;

public class MemoryMetrics
{
    public double Total;
    public double Used;
    public double Free;
}

public class MemoryMetricsClient
{
    public static MemoryMetrics GetMetrics() 
        => IsUnix() ? GetUnixMetrics() : GetWindowsMetrics();

    private static bool IsUnix() 
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static MemoryMetrics GetWindowsMetrics()
    {
        var output = "";

        var info = new ProcessStartInfo
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
        }

        var lines = output.Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

        var metrics = new MemoryMetrics
        {
            Total = Math.Round(double.Parse(totalMemoryParts[ 1 ]) / 1024, 0),
            Free = Math.Round(double.Parse(freeMemoryParts[ 1 ]) / 1024, 0)
        };
        metrics.Used = metrics.Total - metrics.Free;

        return metrics;
    }

    private static MemoryMetrics GetUnixMetrics()
    {
        var output = "";

        var info = new ProcessStartInfo("free -m")
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
        }

        var lines = output.Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

        var metrics = new MemoryMetrics
        {
            Total = double.Parse(memory[ 1 ]),
            Used = double.Parse(memory[ 2 ]),
            Free = double.Parse(memory[ 3 ])
        };

        return metrics;
    }
}
