namespace ProjectIchigo.Entities;

public class PlaylistEntry
{
    private string _Title { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Title { get => _Title; set => _Title = value.TruncateWithIndication(100); }
    
    private TimeSpan? _Length { get; set; }
    public TimeSpan? Length { get => _Length; set => _Length = value; }

    private string _Url { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Url { get => _Url; set => _Url = value.TruncateWithIndication(2048); }

    public DateTime AddedTime { get; set; } = DateTime.UtcNow;
}
