namespace Project_Ichigo.Events;

internal class ExperienceEvents
{
    TaskWatcher.TaskWatcher _watcher { get; set; }
    ServerInfo _guilds { get; set; }

    internal ExperienceEvents(TaskWatcher.TaskWatcher watcher, ServerInfo _guilds)
    {
        _watcher = watcher;
        this._guilds = _guilds;
    }
}
