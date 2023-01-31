namespace ProjectMakoto.Entities;

public class CrosspostRatelimit
{
    public DateTime FirstPost { get; set; } = DateTime.MinValue;

    public int PostsRemaining { get; set; } = 0;
}
