namespace ProjectIchigo.Events;

internal class AutoUnarchiveEvents
{
    internal AutoUnarchiveEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            _logger.LogDebug(e.ThreadAfter.Flags.ToString());
        }).Add(_bot._watcher);
    }
}
