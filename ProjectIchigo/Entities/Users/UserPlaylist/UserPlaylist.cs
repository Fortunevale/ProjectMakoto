namespace ProjectIchigo.Entities;

public class UserPlaylist
{
    public string PlaylistId { get; set; } = Guid.NewGuid().ToString();

    private string _PlaylistName { get; set; } = "";

    [JsonProperty(Required = Required.Always)]
    public string PlaylistName { get => _PlaylistName; set { _PlaylistName = value.TruncateWithIndication(256); } }

    private string _PlaylistColor { get; set; } = "#FFFFFF";
    public string PlaylistColor { get => _PlaylistColor; set { _PlaylistColor = value.Truncate(7).IsValidHexColor(); } }

    private string _PlaylistThumbnail { get; set; } = "";
    public string PlaylistThumbnail { get => _PlaylistThumbnail; set { _PlaylistThumbnail = value.Truncate(2048); } }

    [JsonProperty(Required = Required.Always)]
    public List<PlaylistEntry> List { get; set; } = new();
}
