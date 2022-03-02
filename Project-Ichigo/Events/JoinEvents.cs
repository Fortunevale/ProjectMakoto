namespace Project_Ichigo.Events;

internal class JoinEvents
{
    internal JoinEvents(ServerInfo _guilds, GlobalBans _globalBans, TaskWatcher.TaskWatcher watcher)
    {
        this._guilds = _guilds;
        this._globalBans = _globalBans;
        this._watcher = watcher;
    }

    ServerInfo _guilds { get; set; }
    GlobalBans _globalBans { get; set; }
    TaskWatcher.TaskWatcher _watcher { get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_guilds.Servers.ContainsKey(e.Guild.Id))
                _guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (_guilds.Servers[e.Guild.Id].JoinSettings.AutoBanGlobalBans)
            {
                if (_globalBans.Users.ContainsKey(e.Member.Id))
                {
                    _ = e.Member.BanAsync(7, $"Globalban: {_globalBans.Users[e.Member.Id].Reason}");
                    return;
                }
            }

            if (_guilds.Servers[e.Guild.Id].JoinSettings.AutoAssignRoleId != 0)
            {
                if (e.Guild.Roles.ContainsKey(_guilds.Servers[e.Guild.Id].JoinSettings.AutoAssignRoleId))
                {
                    _ = e.Member.GrantRoleAsync(e.Guild.GetRole(_guilds.Servers[e.Guild.Id].JoinSettings.AutoAssignRoleId));
                }
            }

            if (_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = Resources.AuditLogIcons.UserAdded,
                            Name = $"{e.Member.Username}#{e.Member.Discriminator}"
                        },
                        Description = $"joined the server.",
                        Timestamp = DateTime.UtcNow,
                        Color = new DiscordColor("#00ff00"),
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = e.Member.AvatarUrl
                        }
                    });
                }
            }
        }).Add(_watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_guilds.Servers.ContainsKey(e.Guild.Id))
                _guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_guilds.Servers[e.Guild.Id].JoinSettings.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = Resources.AuditLogIcons.UserLeft,
                            Name = $"{e.Member.Username}#{e.Member.Discriminator}"
                        },
                        Description = $"left the server. Sad to see you go. {DiscordEmoji.FromName(sender, ":wave:")}\n\n" +
                                      $"**Time on the server**: {e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}",
                        Timestamp = DateTime.UtcNow,
                        Color = DiscordColor.Red,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = e.Member.AvatarUrl
                        }
                    });
                }
            }
        }).Add(_watcher);
    }
}
