// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

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

            if (!_bot.guilds[e.Guild.Id].Experience.UseExperience)
                return;

            if (!_bot.guilds[e.Guild.Id].Members.ContainsKey(e.Author.Id))
                _bot.guilds[e.Guild.Id].Members.Add(e.Author.Id, new(_bot.guilds[e.Guild.Id], e.Author.Id));

            if (_bot.guilds[e.Guild.Id].Members[e.Author.Id].Experience.Last_Message.AddSeconds(20) < DateTime.UtcNow && !e.Message.Author.IsBot && !e.Channel.IsPrivate)
            {
                var exp = _bot.experienceHandler.CalculateMessageExperience(e.Message);

                if (_bot.guilds[e.Guild.Id].Experience.BoostXpForBumpReminder)
                {
                    exp = (int)Math.Round(((await e.Author.ConvertToMember(e.Guild)).Roles.Any(x => x.Id == _bot.guilds[e.Guild.Id].BumpReminder.RoleId) ? exp * 1.5 : exp), 0);
                }

                if (exp > 0)
                {
                    _bot.guilds[e.Guild.Id].Members[e.Author.Id].Experience.Last_Message = DateTime.UtcNow;
                    _bot.experienceHandler.ModifyExperience(e.Author, e.Guild, e.Channel, exp);
                }
            }

        }).Add(_bot.watcher);
    }
}
