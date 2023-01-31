namespace ProjectMakoto.Entities;

public class InviteTrackerCacheItem
{
    public ulong CreatorId { get; set; }
    public string Code { get; set; }

    public long Uses { get; set; }
}
