namespace ProjectIchigo.Entities;

public class AbuseIpDbQuery
{
    public Data data { get; set; }

    public class Data
    {
        public string ipAddress { get; set; }
        public bool isPublic { get; set; }
        public int ipVersion { get; set; }
        public bool isWhitelisted { get; set; }
        public int abuseConfidenceScore { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string usageType { get; set; }
        public string isp { get; set; }
        public string domain { get; set; }
        public object[] hostnames { get; set; }
        public int totalReports { get; set; }
        public int numDistinctUsers { get; set; }
        public DateTime lastReportedAt { get; set; }
        public Report[] reports { get; set; }
    }

    public class Report
    {
        public DateTime reportedAt { get; set; }
        public string comment { get; set; }
        public int[] categories { get; set; }
        public int reporterId { get; set; }
        public string reporterCountryCode { get; set; }
        public string reporterCountryName { get; set; }
    }

}
