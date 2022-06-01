﻿namespace Project_Ichigo.Events;

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
            if (!_bot._guilds.List.ContainsKey(e.Guild.Id))
                _bot._guilds.List.Add(e.Guild.Id, new Guilds.ServerSettings());

            if (_bot._guilds.List[e.Guild.Id].JoinSettings.AutoBanGlobalBans)
            {
                if (_bot._globalBans.List.ContainsKey(e.Member.Id))
                {
                    _ = e.Member.BanAsync(7, $"Globalban: {_bot._globalBans.List[e.Member.Id].Reason}");
                    return;
                }
            }

            if (_bot._guilds.List[e.Guild.Id].JoinSettings.AutoAssignRoleId != 0)
            {
                if (e.Guild.Roles.ContainsKey(_bot._guilds.List[e.Guild.Id].JoinSettings.AutoAssignRoleId))
                {
                    _ = e.Member.GrantRoleAsync(e.Guild.GetRole(_bot._guilds.List[e.Guild.Id].JoinSettings.AutoAssignRoleId));
                }
            }

            if (_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = Resources.AuditLogIcons.UserAdded,
                            Name = $"{e.Member.Username}#{e.Member.Discriminator}"
                        },
                        Description = $"joined the server.",
                        Timestamp = DateTime.UtcNow,
                        Color = ColorHelper.Success,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = e.Member.AvatarUrl
                        }
                    });
                }
            }
        }).Add(_bot._watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.List.ContainsKey(e.Guild.Id))
                _bot._guilds.List.Add(e.Guild.Id, new Guilds.ServerSettings());

            if (_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = Resources.AuditLogIcons.UserLeft,
                            Name = $"{e.Member.Username}#{e.Member.Discriminator}"
                        },
                        Description = $"left the server. Sad to see you go. 👋\n\n" +
                                      $"**Time on the server**: {e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}",
                        Timestamp = DateTime.UtcNow,
                        Color = ColorHelper.Error,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = e.Member.AvatarUrl
                        }
                    });
                }
            }
        }).Add(_bot._watcher);
    }
}
