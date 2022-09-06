namespace ProjectIchigo.Entities;

public class CrosspostRatelimit
{
    public DateTime FirstPost { get; set; } = DateTime.MinValue;

    public int PostsRemaining { get; set; } = 0;

    [JsonIgnore]
    public bool Waiting { get; set; } = false;

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

        Stopwatch sw = new Stopwatch();
        sw.Start();

        while (Waiting && sw.ElapsedMilliseconds < TimeSpan.FromMinutes(60).TotalMilliseconds)
            await Task.Delay(5000);

        sw.Stop();

        if (sw.ElapsedMilliseconds >= TimeSpan.FromMinutes(60).TotalMilliseconds)
            Waiting = false;

        if (FirstPost.AddMinutes(70).GetTotalSecondsUntil() > 0)
        {
            _logger.LogDebug($"No crossposts available for '{channelid}', waiting until {FirstPost.AddHours(1)} ({FirstPost.AddHours(1).GetTotalSecondsUntil()} seconds)");

            Waiting = true;
            await Task.Delay(FirstPost.AddHours(1).GetTimespanUntil());
            Waiting = false;
        }

        PostsRemaining = 9;
        FirstPost = DateTime.UtcNow;

        _logger.LogDebug($"Crossposts for '{channelid}' available again, allowing request. {PostsRemaining} requests remaining, first post at {FirstPost}.");
        return;
    }
}
