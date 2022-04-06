namespace Project_Ichigo.Events;

internal class ActionlogEvents
{
    internal ActionlogEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task<bool> ValidateServer(DiscordGuild guild)
    {
        if (!_bot._guilds.Servers.ContainsKey(guild.Id))
            _bot._guilds.Servers.Add(guild.Id, new ServerInfo.ServerSettings());

        if (_bot._guilds.Servers[guild.Id].ActionLogSettings.Channel == 0 || !_bot._guilds.Servers[guild.Id].ActionLogSettings.MembersModified)
            return false;

        if (!guild.Channels.ContainsKey(_bot._guilds.Servers[guild.Id].ActionLogSettings.Channel))
        {
            _bot._guilds.Servers[guild.Id].ActionLogSettings.Channel = 0;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.AttemptGettingMoreDetails = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MembersModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MemberModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MessageDeleted = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MessageModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.RolesModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.BanlistModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.GuildModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.InvitesModified = false;
            return false;
        }

        return true;
    }

    internal async Task UserJoined(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MembersModified)
                return;

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserAdded, Name = $"User joined" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`\n" +
                              $"**Account Age**: `{e.Member.CreationTimestamp.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.CreationTimestamp, TimestampFormat.LongDateTime)}"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task UserLeft(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MembersModified)
                return;

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserLeft, Name = $"User left" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`\n" +
                              $"**Joined at**: `{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.JoinedAt, TimestampFormat.LongDateTime)}"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MessageDeleted || e.Message.WebhookMessage || e.Message is null)
                return;

            if (!string.IsNullOrEmpty(e.Message.Content))
                if (e.Message.Content.StartsWith($";;"))
                    foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                        if (e.Message.Content.StartsWith($";;{command.Key}") || e.Message.Content.StartsWith($">>{command.Key}"))
                            return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.MessageDeleted, Name = $"Message deleted" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Message.Author.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Message.Author.AvatarUrl },
                Description = $"**User**: {e.Message.Author.Mention} `{e.Message.Author.UsernameWithDiscriminator}`\n" +
                                $"**Channel**: {e.Channel.Mention} `[#{e.Channel.Name}]`"
            };

            if (!string.IsNullOrWhiteSpace(e.Message.Content))
                embed.AddField(new DiscordEmbedField("Content", $"`{e.Message.Content.Replace("`", "´").TruncateWithIndication(1022)}`"));

            if (e.Message.Attachments.Count != 0)
                embed.AddField(new DiscordEmbedField("Attachments", $"{string.Join("\n", e.Message.Attachments.Select(x => $"`[{Math.Round(Convert.ToDecimal(x.FileSize / 1024), 2)} KB]` `{x.Url}`"))}"));

            if (e.Message.Stickers.Count != 0)
                embed.AddField(new DiscordEmbedField("Stickers", $"{string.Join("\n", e.Message.Stickers.Select(x => $"`{x.Name}`"))}"));

            if (embed.Fields.Count == 0)
                return;

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }).Add(_bot._watcher);
    }

    internal async Task MessageBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MessageDeleted)
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.MessageDeleted, Name = $"Multiple Messages deleted" },
                Color = ColorHelper.Info,
                Timestamp = DateTime.UtcNow,
                Description = $"**Channel**: {e.Channel.Mention} `[#{e.Channel.Name}]`\n" +
                                $"`Check the attached file to view the deleted messages.`"
            };

            string Messages = "";

            foreach (var b in e.Messages)
            {
                if (b is null || b.WebhookMessage || b.Author is null)
                    continue;

                string CurrentMessage = "";

                try
                {
                    CurrentMessage += $"[{b.Timestamp.ToUniversalTime():dd.MM.yyyy, HH:mm:ss zzz}] {b.Author.UsernameWithDiscriminator} (UserId: '{b.Author.Id}' | MessageId: {b.Id})\n";
                }
                catch (Exception)
                {
                    CurrentMessage += $"[{b.Timestamp.ToUniversalTime():dd.MM.yyyy, HH:mm:ss zzz}] Unknown#0000 (UserId: '{b.Author.Id}' | MessageId: {b.Id})\n";
                }

                if (b.ReferencedMessage is not null)
                    CurrentMessage += $"[Reply to {b.ReferencedMessage.Id}]\n";

                if (!string.IsNullOrWhiteSpace(b.Content))
                    CurrentMessage += $"{b.Content}";

                if (b.Attachments.Count != 0)
                {
                    CurrentMessage += $"\n\n[Attachments]\n" +
                                        $"{string.Join("\n", b.Attachments.Select(x => $"`[{Math.Round(Convert.ToDecimal(x.FileSize / 1024), 2)} KB]` `{x.Url}`"))}";

                }

                if (b.Stickers.Count != 0)
                {
                    CurrentMessage += $"\n\n[Stickers]\n" +
                                        $"{string.Join("\n", b.Stickers.Select(x => $"`{x.Name}`"))}";

                }

                Messages += $"{CurrentMessage}\n\n";
            }

            if (Messages.Length == 0)
                return;

            string FileContent = $"All dates are saved in universal time (UTC+0).\n\n\n" +
                                $"{e.Messages.Count} messages deleted in #{e.Channel.Name} ({e.Channel.Id}) on {e.Guild.Name} ({e.Guild.Id})\n\n\n" +
                                $"{Messages}";

            string FileName = $"{Guid.NewGuid()}.txt";
            File.WriteAllText(FileName, FileContent);
            using (FileStream fileStream = new FileStream(FileName, FileMode.Open))
            {
                await e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).WithFile(FileName, fileStream));
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        File.Delete(FileName);
                        return;
                    }
                    catch { }
                    await Task.Delay(1000);
                }
            });

        }).Add(_bot._watcher);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task MemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task RoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task RoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task RoleModified(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task BanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task BanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }
}
