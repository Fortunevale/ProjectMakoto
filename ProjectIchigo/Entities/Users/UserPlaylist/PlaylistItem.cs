namespace ProjectIchigo.Entities;

internal class PlaylistItem
{
    public string Title { get; set; }
    public string Url { get; set; }

    public DateTime AddedTime { get; set; } = DateTime.UtcNow;
}
