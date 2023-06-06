// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.SystemMonitor;

public sealed class SystemInfo
{
    public CpuInfo Cpu { get; set; } = new();
    public MemoryInfo Memory { get; set; } = new();
    public NetworkInfo Network { get; set; } = new();

    public sealed class CpuInfo
    {
        public float Load { get; set; } = 0;
        public float Temperature { get; set; } = 0;
    }

    public sealed class MemoryInfo
    {
        public float Available { get; set; } = 0;
        public float Used { get; set; } = 0;

        public float Total
        {
            get { return this.Available + this.Used; }
            set { this.Available = value - this.Used; }
        }
    }

    public sealed class NetworkInfo
    {
        public float TotalDownloaded { get; set; } = 0;
        public float TotalUploaded { get; set; } = 0;

        public float CurrentDownloadSpeed { get; set; } = 0;
        public float CurrentUploadSpeed { get; set; } = 0;

        public float TotalUtilization { get; set; } = 0;
    }
}
