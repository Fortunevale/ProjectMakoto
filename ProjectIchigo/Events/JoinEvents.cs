namespace ProjectIchigo.Events;

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

            string[] emojis = { "🙋‍", "🙋‍", "<a:3HC_wave:985374001062879292>", "<:wave:985373989927010315>", "<a:wave:985374007144632381>", "<a:waves:985373995996176384>", "<:WaveHello:985373984474419270>" };

            if (_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId != 0)
            {
                if (e.Guild.Channels.ContainsKey(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId))
                {
                    _ = e.Guild.GetChannel(_bot._guilds.List[e.Guild.Id].JoinSettings.JoinlogChannelId).SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            IconUrl = Resources.AuditLogIcons.UserAdded,
                            Name = e.Member.UsernameWithDiscriminator
                        },
                        Description = $"has joined **{e.Guild.Name}**. Welcome! {emojis.SelectRandom()}",
                        Color = ColorHelper.Success,
                        Thumbnail = new()
                        {
                            Url = (e.Member.AvatarUrl.IsNullOrWhiteSpace() ? Resources.AuditLogIcons.QuestionMark : e.Member.AvatarUrl)
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
                        Author = new()
                        {
                            IconUrl = Resources.AuditLogIcons.UserLeft,
                            Name = e.Member.UsernameWithDiscriminator
                        },
                        Description = $"has left **{e.Guild.Name}**.\n" +
                                      $"They've been on the server for _{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}_.",
                        Color = ColorHelper.Error,
                        Thumbnail = new()
                        {
                            Url = (e.Member.AvatarUrl.IsNullOrWhiteSpace() ? Resources.AuditLogIcons.QuestionMark : e.Member.AvatarUrl)
                        }
                    });
                }
            }
        }).Add(_bot._watcher);
    }
}
