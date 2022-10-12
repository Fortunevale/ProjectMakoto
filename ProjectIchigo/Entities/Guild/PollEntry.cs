namespace ProjectIchigo.Entities;

public class PollEntry
{
    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public string ComponentUUID { get; set; }

    public DateTime DueTime { get; set; }

    public Dictionary<string, string> Options { get; set; }

    public Dictionary<ulong, List<string>> Votes { get; set; }
}
