namespace Project_Ichigo.Events;

internal class ExperienceEvents
{
    TaskWatcher.TaskWatcher _watcher { get; set; }
    ServerInfo _guilds { get; set; }
    ExperienceHandler _experienceHandler { get; set; }

    internal ExperienceEvents(TaskWatcher.TaskWatcher _watcher, ServerInfo _guilds, ExperienceHandler _experienceHandler)
    {
        this._watcher = _watcher;
        this._guilds = _guilds;
        this._experienceHandler = _experienceHandler;
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Message.WebhookMessage || e.Guild is null)
                return;

            if (!_guilds.Servers.ContainsKey(e.Guild.Id))
                _guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (!_guilds.Servers[e.Guild.Id].ExperienceSettings.UseExperience)
                return;

            if (_guilds.Servers[e.Guild.Id].Members[e.Author.Id].Last_Message.AddSeconds(20) < DateTime.UtcNow && !e.Message.Author.IsBot && !e.Channel.IsPrivate)
            {
                var exp = _experienceHandler.CalculateMessageExperience(e.Message);

                if (exp > 0)
                {
                    _guilds.Servers[e.Guild.Id].Members[e.Author.Id].Last_Message = DateTime.UtcNow;
                    _experienceHandler.ModifyExperience(e.Author, e.Guild, e.Channel, exp);
                }
            }

        }).Add(_watcher);
    }
}
