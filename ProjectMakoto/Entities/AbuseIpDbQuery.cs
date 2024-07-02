// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class AbuseIpDbQuery
{
    public Data data { get; set; }

    public sealed class Data
    {
        public string? ipAddress { get; set; }
        public bool? isPublic { get; set; }
        public int? ipVersion { get; set; }
        public bool? isWhitelisted { get; set; }
        public int? abuseConfidenceScore { get; set; }
        public string? countryCode { get; set; }
        public string? countryName { get; set; }
        public string? usageType { get; set; }
        public string? isp { get; set; }
        public string? domain { get; set; }
        public object[]? hostnames { get; set; }
        public int? totalReports { get; set; }
        public int? numDistinctUsers { get; set; }
        public DateTime? lastReportedAt { get; set; }
        public Report[]? reports { get; set; }
    }

    public sealed class Report
    {
        public DateTime? reportedAt { get; set; }
        public string? comment { get; set; }
        public int[]? categories { get; set; }
        public int? reporterId { get; set; }
        public string? reporterCountryCode { get; set; }
        public string? reporterCountryName { get; set; }
    }
}
