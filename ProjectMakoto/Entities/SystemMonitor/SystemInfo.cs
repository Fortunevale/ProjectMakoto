namespace ProjectMakoto.Entities.SystemMonitor;

public class SystemInfo
{
    public CpuInfo Cpu { get; set; } = new();
    public MemoryInfo Memory { get; set; } = new();
    public NetworkInfo Network { get; set; } = new();

    public class CpuInfo
    {
        public float Load { get; set; } = 0;
        public float Temperature { get; set; } = 0;
    }

    public class MemoryInfo
    {
        public float Available { get; set; } = 0;
        public float Used { get; set; } = 0;

        public float Total
        {
            get { return Available + Used; }
            set { Available = value - Used; }
        }
    }

    public class NetworkInfo
    {
        public float TotalDownloaded { get; set; } = 0;
        public float TotalUploaded { get; set; } = 0;

        public float CurrentDownloadSpeed { get; set; } = 0;
        public float CurrentUploadSpeed { get; set; } = 0;

        public float TotalUtilization { get; set; } = 0;
    }
}
