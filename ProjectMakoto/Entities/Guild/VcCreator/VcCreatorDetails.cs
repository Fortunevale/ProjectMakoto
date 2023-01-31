namespace ProjectMakoto.Entities;

public class VcCreatorDetails
{
    public ulong OwnerId { get; set; }

    public List<ulong> BannedUsers { get; set; } = new();

    public DateTime LastRename { get; set; } = DateTime.MinValue;

    [JsonIgnore]
    public bool EventsRegistered { get; set; } = false;
}
