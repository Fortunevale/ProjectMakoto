namespace ProjectIchigo.Entities;

public class MessageDetails
{
    private ulong _MessageId { get; set; } = 0;
    public ulong MessageId { get => _MessageId; set { _MessageId = value; } }



    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId { get => _ChannelId; set { _ChannelId = value; } }



    private ulong _GuildId { get; set; } = 0;
    public ulong GuildId { get => _GuildId; set { _GuildId = value; } }



    private ulong _AuthorId { get; set; } = 0;
    public ulong AuthorId { get => _AuthorId; set { _AuthorId = value; } }
}
