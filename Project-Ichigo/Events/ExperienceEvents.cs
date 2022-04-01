namespace Project_Ichigo.Events;

internal class ExperienceEvents
{
    internal ExperienceEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Message.WebhookMessage || e.Guild is null)
                return;

            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new());

            if (!_bot._guilds.Servers[e.Guild.Id].ExperienceSettings.UseExperience)
                return;

            if (!_bot._guilds.Servers[e.Guild.Id].Members.ContainsKey(e.Author.Id))
                _bot._guilds.Servers[e.Guild.Id].Members.Add(e.Author.Id, new());

            if (_bot._guilds.Servers[e.Guild.Id].Members[e.Author.Id].Last_Message.AddSeconds(20) < DateTime.UtcNow && !e.Message.Author.IsBot && !e.Channel.IsPrivate)
            {
                var exp = _bot._experienceHandler.CalculateMessageExperience(e.Message);

                if (_bot._guilds.Servers[e.Guild.Id].ExperienceSettings.BoostXpForBumpReminder)
                {
                    exp = (int)Math.Round(((await e.Author.ConvertToMember(e.Guild)).Roles.Any(x => x.Id == _bot._guilds.Servers[e.Guild.Id].BumpReminderSettings.RoleId) ? exp * 1.5 : exp), 0);
                }

                if (exp > 0)
                {
                    LogDebug(exp.ToString());
                    _bot._guilds.Servers[e.Guild.Id].Members[e.Author.Id].Last_Message = DateTime.UtcNow;
                    _bot._experienceHandler.ModifyExperience(e.Author, e.Guild, e.Channel, exp);
                }
            }

        }).Add(_bot._watcher);
    }
}
