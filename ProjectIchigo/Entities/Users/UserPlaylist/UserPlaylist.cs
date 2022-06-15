namespace ProjectIchigo.Entities;

internal class UserPlaylist
{
    private string _PlaylistName { get; set; } = "";
    public string PlaylistName { get => _PlaylistName; set { _PlaylistName = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    public List<PlaylistItem> List { get; set; } = new();
}
