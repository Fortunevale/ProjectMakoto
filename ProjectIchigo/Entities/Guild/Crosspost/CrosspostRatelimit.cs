namespace ProjectIchigo.Entities;

public class CrosspostRatelimit
{
    public DateTime FirstPost { get; set; } = DateTime.MinValue;

    public int PostsRemaining { get; set; } = 0;

    public async Task WaitForRatelimit(ulong channelid = 0)
    {
        if (FirstPost.AddHours(1).GetTotalSecondsUntil() <= 0)
        {
            _logger.LogDebug($"First crosspost for '{channelid}' was at {FirstPost.AddHours(1)}, resetting crosspost availability");
            FirstPost = DateTime.UtcNow;
            PostsRemaining = 10;
        }

        if (PostsRemaining > 0)
        {
            _logger.LogDebug($"{PostsRemaining} crossposts available for '{channelid}', allowing request");
            PostsRemaining--;
            return;
        }

        if (FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
        {
            _logger.LogDebug($"No crossposts available for '{channelid}', waiting until {FirstPost.AddHours(1)} ({FirstPost.AddHours(1).GetTotalSecondsUntil()} seconds)");
            await Task.Delay(FirstPost.AddHours(1).GetTimespanUntil());
        }

        PostsRemaining = 9;
        FirstPost = DateTime.UtcNow;

        _logger.LogDebug($"Crossposts for '{channelid}' available again, allowing request. {PostsRemaining} requests remaining, first post at {FirstPost}.");
        return;
    }
}
