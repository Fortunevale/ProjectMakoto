namespace ProjectIchigo.Entities;

public class CrosspostRatelimit
{
    public DateTimeOffset FirstPost { get; set; } = DateTimeOffset.MinValue;

    public int PostsRemaining { get; set; } = 0;

    public async Task WaitForRatelimit(ulong channelid = 0)
    {
        if (PostsRemaining > 0)
        {
            _logger.LogDebug($"{PostsRemaining} crossposts available for '{channelid}', allowing request");
            PostsRemaining--;
            return;
        }

        _logger.LogDebug($"No crossposts available for '{channelid}', waiting until {FirstPost.AddHours(1)} ({FirstPost.AddHours(1).GetTotalSecondsUntil()} seconds)");
        if (FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
            await Task.Delay(FirstPost.AddHours(1).GetTimespanUntil());

        _logger.LogDebug($"Crossposts for '{channelid}' available again, allowing request");

        PostsRemaining = 9;
        FirstPost = DateTimeOffset.Now;
        return;
    }
}
