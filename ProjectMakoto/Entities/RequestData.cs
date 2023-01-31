namespace ProjectMakoto.Entities;

internal class RequestData
{
    public User User { get; set; }
    public Dictionary<ulong, Member> GuildData { get; set; } = new();
}
