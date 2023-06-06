// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class NameNormalizerEvents
{
    internal NameNormalizerEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (!_bot.guilds[e.Guild.Id].NameNormalizer.NameNormalizerEnabled)
            return;

        string PingableName = RegexTemplates.AllowedNickname.Replace(e.Member.DisplayName.Normalize(NormalizationForm.FormKC), "");

        if (PingableName.IsNullOrWhiteSpace())
            PingableName = "Pingable Name";

        if (PingableName != e.Member.DisplayName)
            _ = e.Member.ModifyAsync(x => x.Nickname = PingableName);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        if (!_bot.guilds[e.Guild.Id].NameNormalizer.NameNormalizerEnabled)
            return;

        if (e.NicknameBefore != e.NicknameAfter)
        {
            string PingableName = RegexTemplates.AllowedNickname.Replace(e.Member.DisplayName.Normalize(NormalizationForm.FormKC), "");

            if (PingableName.IsNullOrWhiteSpace())
                PingableName = "Pingable Name";

            if (PingableName != e.Member.DisplayName)
                _ = e.Member.ModifyAsync(x => x.Nickname = PingableName);
        }
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        if (e.UserBefore.GetUsername() == e.UserAfter.GetUsername())
            return;

        foreach (var guild in sender.Guilds)
        {
            if (!_bot.guilds[guild.Key].NameNormalizer.NameNormalizerEnabled)
                return;

            var member = await e.UserAfter.ConvertToMember(guild.Value);

            string PingableName = RegexTemplates.AllowedNickname.Replace(member.DisplayName.Normalize(NormalizationForm.FormKC), "");

            if (PingableName.IsNullOrWhiteSpace())
                PingableName = "Pingable Name";

            if (PingableName != member.DisplayName)
                _ = member.ModifyAsync(x => x.Nickname = PingableName);
        }
    }
}
