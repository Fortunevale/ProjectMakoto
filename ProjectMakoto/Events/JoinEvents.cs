// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class JoinEvents : RequiresTranslation
{
    public JoinEvents(Bot bot) : base(bot)
    {
    }

    Translations.events.join tKey
        => this.t.Events.Join;

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (this.Bot.Guilds[e.Guild.Id].Join.AutoBanGlobalBans)
        {
            if (this.Bot.globalBans.TryGetValue(e.Member.Id, out BanDetails globalBanDetails))
            {
                _ = e.Member.BanAsync(7, $"{tKey.Globalban.Get(Bot.Guilds[e.Guild.Id])}: {globalBanDetails.Reason}");
                return;
            }
        }

        if (this.Bot.Guilds[e.Guild.Id].Join.AutoAssignRoleId != 0)
        {
            if (e.Guild.Roles.ContainsKey(this.Bot.Guilds[e.Guild.Id].Join.AutoAssignRoleId))
            {
                _ = e.Member.GrantRoleAsync(e.Guild.GetRole(this.Bot.Guilds[e.Guild.Id].Join.AutoAssignRoleId));
            }
        }

        if (this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
        {
            if (e.Guild.Channels.ContainsKey(this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId))
            {
                _ = e.Guild.GetChannel(this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new()
                    {
                        IconUrl = AuditLogIcons.UserAdded,
                        Name = e.Member.GetUsernameWithIdentifier()
                    },
                    Description = $"{tKey.UserJoined.Get(Bot.Guilds[e.Guild.Id]).Build(new TVar("Guild", $"**{e.Guild.Name}**"))} {this.Bot.status.LoadedConfig.Emojis.JoinEvent.SelectRandom()}",
                    Color = EmbedColors.Success,
                    Thumbnail = new()
                    {
                        Url = (e.Member.AvatarUrl.IsNullOrWhiteSpace() ? AuditLogIcons.QuestionMark : e.Member.AvatarUrl)
                    }
                });
            }
        }
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
        {
            if (e.Guild.Channels.ContainsKey(this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId))
            {
                _ = e.Guild.GetChannel(this.Bot.Guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new()
                    {
                        IconUrl = AuditLogIcons.UserLeft,
                        Name = e.Member.GetUsernameWithIdentifier()
                    },
                    Description = tKey.UserLeft.Get(Bot.Guilds[e.Guild.Id]).Build(
                        new TVar("Guild", $"**{e.Guild.Name}**"), 
                        new TVar("Timestamp", e.Member.JoinedAt.GetTimespanSince().GetHumanReadable(TimeFormat.DAYS, TranslationUtil.GetTranslatedHumanReadableConfig(Bot.Guilds[e.Guild.Id], Bot)))),
                    Color = EmbedColors.Error,
                    Thumbnail = new()
                    {
                        Url = (e.Member.AvatarUrl.IsNullOrWhiteSpace() ? AuditLogIcons.QuestionMark : e.Member.AvatarUrl)
                    }
                });
            }
        }
    }
}
