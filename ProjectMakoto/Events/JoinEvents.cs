// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal class JoinEvents
{
    internal JoinEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (_bot.guilds[e.Guild.Id].Join.AutoBanGlobalBans)
            {
                if (_bot.globalBans.TryGetValue(e.Member.Id, out GlobalBanDetails globalBanDetails))
                {
                    _ = e.Member.BanAsync(7, $"Globalban: {globalBanDetails.Reason}");
                    return;
                }
            }

            if (_bot.guilds[e.Guild.Id].Join.AutoAssignRoleId != 0)
            {
                if (e.Guild.Roles.ContainsKey(_bot.guilds[e.Guild.Id].Join.AutoAssignRoleId))
                {
                    _ = e.Member.GrantRoleAsync(e.Guild.GetRole(_bot.guilds[e.Guild.Id].Join.AutoAssignRoleId));
                }
            }

            if (_bot.guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_bot.guilds[e.Guild.Id].Join.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_bot.guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            IconUrl = AuditLogIcons.UserAdded,
                            Name = e.Member.GetUsername()
                        },
                        Description = $"has joined **{e.Guild.Name}**. Welcome! {_bot.status.LoadedConfig.Emojis.JoinEvent.SelectRandom()}",
                        Color = EmbedColors.Success,
                        Thumbnail = new()
                        {
                            Url = (e.Member.AvatarUrl.IsNullOrWhiteSpace() ? AuditLogIcons.QuestionMark : e.Member.AvatarUrl)
                        }
                    });
                }
            }
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (_bot.guilds[e.Guild.Id].Join.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_bot.guilds[e.Guild.Id].Join.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_bot.guilds[e.Guild.Id].Join.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            IconUrl = AuditLogIcons.UserLeft,
                            Name = e.Member.GetUsername()
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
        }).Add(_bot.watcher);
    }
}
