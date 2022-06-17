namespace ProjectIchigo.Entities;

internal class PlaylistItem
{
    private string _Title { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Title { get => _Title; set => _Title = value.TruncateWithIndication(100); }

    private string _Url { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Url { get => _Url; set => _Url = value.TruncateWithIndication(2048); }

    public DateTime AddedTime { get; set; } = DateTime.UtcNow;
}
