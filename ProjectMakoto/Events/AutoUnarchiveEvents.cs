namespace ProjectMakoto.Events;

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
            await Task.Delay(5000);
            if (_bot.guilds[e.Guild.Id].AutoUnarchiveThreads.Contains(e.ThreadAfter.Parent.Id))
            {
                if (e.ThreadAfter.ThreadMetadata.Archived && (!e.ThreadAfter.ThreadMetadata.Locked ?? false))
                    _ = e.ThreadAfter.UnarchiveAsync();
            }
        }).Add(_bot.watcher);
    }
}
