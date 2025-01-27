// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Events;
internal sealed class ReminderEvents(Bot bot) : RequiresTranslation(bot)
{
    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (!e.Channel?.IsPrivate ?? true)
            return;

        try
        {
            var v = JsonConvert.DeserializeObject<object[]>(e.Id)[0];
            var privateButtonType = (PrivateButtonType)v.ToInt32();
            if (privateButtonType != PrivateButtonType.ReminderSnooze)
                return;
        }
        catch (Exception)
        {
            return;
        }

        var reminder = JsonConvert.DeserializeObject<ReminderSnoozeButton>(e.Id);

        _ = new RemindersCommand().ExecuteCommand(e, sender, "reminders", this.Bot, new Dictionary<string, object>
        {
            { "description", reminder.Description },
        }).Add(this.Bot);
    }
}
