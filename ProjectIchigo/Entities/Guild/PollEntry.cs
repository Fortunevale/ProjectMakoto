namespace ProjectIchigo.Entities;

public class PollEntry
{
    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public Dictionary<string, string> Options { get; set; }
}
