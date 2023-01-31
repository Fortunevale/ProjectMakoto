namespace ProjectMakoto.Entities;

public class PollEntry
{
    public string PollText { get; set; }

    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public string EndEarlyUUID { get; set; }

    public string SelectUUID { get; set; }

    public DateTime DueTime { get; set; }

    public Dictionary<string, string> Options { get; set; }

    public Dictionary<ulong, List<string>> Votes { get; set; }
}
