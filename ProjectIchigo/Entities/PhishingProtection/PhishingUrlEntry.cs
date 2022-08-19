namespace ProjectIchigo;

public class PhishingUrlEntry
{
    public string Url { get; set; } = "";
    public List<string> Origin { get; set; } = new();
    public ulong Submitter { get; set; } = 0;
}