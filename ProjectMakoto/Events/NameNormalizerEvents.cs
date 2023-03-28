﻿namespace ProjectMakoto.Events;

internal class NameNormalizerEvents
{
    internal NameNormalizerEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].NameNormalizer.NameNormalizerEnabled)
                return;

            string PingableName = RegexTemplates.AllowedNickname.Replace(e.Member.DisplayName.Normalize(NormalizationForm.FormKC), "");

            if (PingableName.IsNullOrWhiteSpace())
                PingableName = "Pingable Name";

            if (PingableName != e.Member.DisplayName)
                _ = e.Member.ModifyAsync(x => x.Nickname = PingableName);
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
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
        }).Add(_bot.watcher);
    }

    internal async Task UserUpdated(DiscordClient sender, UserUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.UserBefore.Username == e.UserAfter.Username)
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
        }).Add(_bot.watcher);
    }
}
