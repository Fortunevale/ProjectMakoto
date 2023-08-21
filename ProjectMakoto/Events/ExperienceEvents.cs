// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class ExperienceEvents : RequiresTranslation
{
    public ExperienceEvents(Bot bot) : base(bot)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Message.WebhookMessage || e.Guild is null)
            return;

        if (!this.Bot.Guilds[e.Guild.Id].Experience.UseExperience)
            return;

        if (this.Bot.Guilds[e.Guild.Id].Members[e.Author.Id].Experience.Last_Message.AddSeconds(20) < DateTime.UtcNow && !e.Message.Author.IsBot && !e.Channel.IsPrivate)
        {
            var exp = this.Bot.ExperienceHandler.CalculateMessageExperience(e.Message);

            if (this.Bot.Guilds[e.Guild.Id].Experience.BoostXpForBumpReminder)
            {
                exp = (int)Math.Round(((await e.Author.ConvertToMember(e.Guild)).Roles.Any(x => x.Id == this.Bot.Guilds[e.Guild.Id].BumpReminder.RoleId) ? exp * 1.5 : exp), 0);
            }

            if (exp > 0)
            {
                this.Bot.Guilds[e.Guild.Id].Members[e.Author.Id].Experience.Last_Message = DateTime.UtcNow;
                _ = this.Bot.ExperienceHandler.ModifyExperience(e.Author, e.Guild, e.Channel, exp);
            }
        }
    }
}
