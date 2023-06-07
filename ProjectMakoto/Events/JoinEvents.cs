// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class JoinEvents
{
    internal JoinEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (this._bot.guilds[e.Guild.Id].Join.AutoBanGlobalBans)
        {
            if (this._bot.globalBans.TryGetValue(e.Member.Id, out GlobalBanDetails globalBanDetails))
            {
                _ = e.Member.BanAsync(7, $"Globalban: {globalBanDetails.Reason}");
                return;
            }
        }

        if (this._bot.guilds[e.Guild.Id].Join.AutoAssignRoleId != 0)
        {
            if (e.Guild.Roles.ContainsKey(this._bot.guilds[e.Guild.Id].Join.AutoAssignRoleId))
            {
                _ = e.Member.GrantRoleAsync(e.Guild.GetRole(this._bot.guilds[e.Guild.Id].Join.AutoAssignRoleId));
            }
        }

        if (this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
        {
            if (e.Guild.Channels.ContainsKey(this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId))
            {
                _ = e.Guild.GetChannel(this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new()
                    {
                        IconUrl = AuditLogIcons.UserAdded,
                        Name = e.Member.GetUsernameWithIdentifier()
                    },
                    Description = $"has joined **{e.Guild.Name}**. Welcome! {this._bot.status.LoadedConfig.Emojis.JoinEvent.SelectRandom()}",
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
        if (this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
        {
            if (e.Guild.Channels.ContainsKey(this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId))
            {
                _ = e.Guild.GetChannel(this._bot.guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new()
                    {
                        IconUrl = AuditLogIcons.UserLeft,
                        Name = e.Member.GetUsernameWithIdentifier()
                    },
                    Description = $"has left **{e.Guild.Name}**.\n" +
                                    $"They've been on the server for _{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}_.",
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
