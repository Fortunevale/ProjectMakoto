namespace Project_Ichigo.Entities.Database;

internal class DatabaseSubmittedUrls
{
    public ulong messageid { get; set; }
    public string url { get; set; }
    public ulong submitter { get; set; }
    public ulong guild { get; set; }
}
