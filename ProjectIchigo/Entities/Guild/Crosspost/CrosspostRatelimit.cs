namespace ProjectIchigo.Entities;

public class CrosspostRatelimit
{
    public DateTimeOffset FirstPost { get; set; } = DateTimeOffset.MinValue;

    public int PostsRemaining { get; set; } = 0;

    public async Task WaitForRatelimit()
    {
        if (PostsRemaining > 0)
        {
            _logger.LogDebug($"{PostsRemaining} crossposts available, allowing request");
            PostsRemaining--;
            return;
        }

        _logger.LogDebug($"No crossposts available, waiting until {FirstPost.AddHours(1)} ({FirstPost.AddHours(1).GetTotalSecondsUntil()})");
        if (FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
            await Task.Delay(FirstPost.AddHours(1).GetTimespanUntil());

        _logger.LogDebug($"Crossposts available again, allowing request");

        PostsRemaining = 9;
        FirstPost = DateTimeOffset.Now;
        return;
    }
}
