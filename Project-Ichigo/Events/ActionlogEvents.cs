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
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MemberProfileModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MessageDeleted = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.MessageModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.RolesModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.BanlistModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.GuildModified = false;
            _bot._guilds.Servers[guild.Id].ActionLogSettings.ChannelsModified = false;
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
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MessageDeleted || e.Message.WebhookMessage || e.Message is null || e.Message.Author is null)
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
            using (FileStream fileStream = new(FileName, FileMode.Open))
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
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MessageDeleted || e.Message.WebhookMessage || e.Author.IsBot || e.Message is null || e.Message.Author is null)
                return;

            if (!string.IsNullOrEmpty(e.Message.Content))
                if (e.Message.Content.StartsWith($";;"))
                    foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                        if (e.Message.Content.StartsWith($";;{command.Key}") || e.Message.Content.StartsWith($">>{command.Key}"))
                            return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.MessageEdited, Name = $"Message updated" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Message.Author.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Message.Author.AvatarUrl },
                Description = $"**User**: {e.Message.Author.Mention} `{e.Message.Author.UsernameWithDiscriminator}`\n" +
                                $"**Channel**: {e.Channel.Mention} `[#{e.Channel.Name}]`\n" +
                                $"**Message**: [`Jump to message`]({e.Message.JumpLink})"
            };

            if (e.MessageBefore.Content != e.Message.Content)
            {
                if (!string.IsNullOrWhiteSpace(e.MessageBefore.Content))
                    embed.AddField(new DiscordEmbedField("Previous Content", $"`{e.MessageBefore.Content.TruncateWithIndication(2022)}`"));

                if (!string.IsNullOrWhiteSpace(e.Message.Content))
                    embed.AddField(new DiscordEmbedField("New Content", $"`{e.Message.Content.TruncateWithIndication(2022)}`"));
            }
            else
            {
                return;
            }

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

        }).Add(_bot._watcher);
    }

    internal async Task MemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MemberModified)
                return;

            if (e.NicknameBefore != e.NicknameAfter)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserUpdated, Name = $"Nickname updated" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`"
                };

                if (string.IsNullOrWhiteSpace(e.NicknameBefore))
                    embed.Author.Name = "Nickname added";
                else
                    embed.AddField(new DiscordEmbedField("Previous Nickname", $"`{e.NicknameBefore}`"));

                if (string.IsNullOrWhiteSpace(e.NicknameAfter))
                    embed.Author.Name = "Nickname removed";
                else
                    embed.AddField(new DiscordEmbedField("New Nickname", $"`{e.NicknameAfter}`"));

                _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }

            bool RolesUpdated = false;

            foreach (var role in e.RolesBefore)
            {
                if (!e.RolesAfter.Any(x => x.Id == role.Id))
                {
                    RolesUpdated = true;
                    break;
                }
            }

            if (!RolesUpdated)
                foreach (var role in e.RolesAfter)
                {
                    if (!e.RolesBefore.Any(x => x.Id == role.Id))
                    {
                        RolesUpdated = true;
                        break;
                    }
                }

            if (RolesUpdated)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserUpdated, Name = $"Roles updated" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`"
                };

                string Roles = "";

                bool RolesAdded = false;
                bool RolesRemoved = false;

                foreach (var role in e.RolesAfter)
                {
                    if (!e.RolesBefore.Any(x => x.Id == role.Id))
                    {
                        Roles += $"`+` {role.Mention} `{role.Name}` `({role.Id})`\n";
                        RolesAdded = true;
                    }
                }

                foreach (var role in e.RolesBefore)
                {
                    if (!e.RolesAfter.Any(x => x.Id == role.Id))
                    {
                        Roles += $"`-` {role.Mention} `{role.Name}` `({role.Id})`\n";
                        RolesRemoved = true;
                    }
                }

                if (RolesAdded && !RolesRemoved)
                    embed.Author.Name = "Roles added";
                else if (!RolesAdded && RolesRemoved)
                    embed.Author.Name = "Roles removed";
                else
                    embed.Author.Name = "Roles updated";

                embed.Description += $"\n\n{Roles}";

                _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }

            if (e.TimeoutBefore != e.TimeoutAfter)
            {
                // Timeouts don't seem to fire the member updated event, will keep this code for potential future updates.

                if (e.TimeoutAfter?.ToUniversalTime() > e.TimeoutBefore?.ToUniversalTime())
                    _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserBanned, Name = $"User timed out" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                        Timestamp = DateTime.UtcNow,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                        Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`\n" +
                                        $"**Timed out until**: {Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.LongDateTime)} ({Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.RelativeTime)})"
                    }));

                if (e.TimeoutAfter?.ToUniversalTime() < e.TimeoutBefore?.ToUniversalTime())
                    _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserBanRemoved, Name = $"User timeout removed" },
                        Color = ColorHelper.Info,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                        Timestamp = DateTime.UtcNow,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                        Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`"
                    }));
            }

            if (e.PendingBefore != e.PendingAfter)
            {
                try
                {
                    if ((e.PendingBefore is null && e.PendingAfter is true) || (e.PendingAfter is true && e.PendingBefore is false))
                        _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserAdded, Name = $"Membership approved" },
                            Color = ColorHelper.Info,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                            Timestamp = DateTime.UtcNow,
                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                            Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`"
                        }));
                }
                catch { }
            }

            if (!_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.MemberProfileModified)
                return;

            if (e.AvatarHashBefore != e.AvatarHashAfter)
            {
                // Normal avatar updates don't seem to fire the member updated event, will keep this code for potential future updates.

                _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserUpdated, Name = $"Member Profile Picture updated" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`",
                    ImageUrl = e.Member.AvatarUrl
                }));
            }

            if (e.GuildAvatarHashBefore != e.GuildAvatarHashAfter)
            {
                _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserUpdated, Name = $"Member Guild Profile Picture updated" },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`",
                    ImageUrl = e.Member.GuildAvatarUrl
                }));
            }

        }).Add(_bot._watcher);
    }

    internal async Task RoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.RolesModified)
                return;

            string GeneratePermissions = string.Join(", ", e.Role.Permissions.ToString().Split(", ").Select(x => $"`{x}`"));
            string Integration = "";

            if (e.Role.IsManaged)
            {
                if (e.Role.Tags.IsPremiumSubscriber)
                    Integration = "**Integration**: `Server Booster`\n\n";

                if (e.Role.Tags.BotId is not null and not 0)
                {
                    var bot = await sender.GetUserAsync((ulong)e.Role.Tags.BotId);

                    Integration = $"**Integration**: {bot.Mention} `{bot.UsernameWithDiscriminator}`\n\n";
                }
            }

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserAdded, Name = $"Role created" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.Role.Id}" },
                Timestamp = DateTime.UtcNow,
                Description = $"**Role**: {e.Role.Mention} `{e.Role.Name}`\n" +
                                $"**Color**: `{ToHex(e.Role.Color.R, e.Role.Color.G, e.Role.Color.B)}`\n" +
                                $"{(e.Role.IsManaged ? "\n`This role belongs to an integration and cannot be deleted.`\n" : "")}" +
                                $"{Integration}" +
                                $"{(e.Role.IsMentionable ? "`Everyone can mention this role.`\n" : "")}" +
                                $"{(e.Role.IsHoisted ? "`Role members are displayed seperately from others.`\n" : "")}" +
                                $"\n**Permissions**: {GeneratePermissions}"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task RoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.RolesModified)
                return;

            string GeneratePermissions = string.Join(", ", e.Role.Permissions.ToString().Split(", ").Select(x => $"`{x}`"));
            string Integration = "";

            if (e.Role.IsManaged)
            {
                if (e.Role.Tags.IsPremiumSubscriber)
                    Integration = "**Integration**: `Server Booster`\n\n";

                if (e.Role.Tags.BotId is not null and not 0)
                {
                    var bot = await sender.GetUserAsync((ulong)e.Role.Tags.BotId);

                    Integration = $"**Integration**: {bot.Mention} `{bot.UsernameWithDiscriminator}`\n\n";
                }
            }

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserLeft, Name = $"Role deleted" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.Role.Id}" },
                Timestamp = DateTime.UtcNow,
                Description = $"**Role**: {e.Role.Mention} `{e.Role.Name}`\n" +
                                $"**Color**: `{e.Role.Color.ToHex()}`\n" +
                                $"{(e.Role.IsManaged ? "\n`This role belonged to an integration and was therefor deleted automatically.`\n" : "")}" +
                                $"{Integration}" +
                                $"{(e.Role.IsMentionable ? "`Everyone could mention this role.`\n" : "")}" +
                                $"{(e.Role.IsHoisted ? "`Role members were displayed seperately from others.`\n" : "")}" +
                                $"\n**Permissions**: {GeneratePermissions}"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task RoleModified(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.RolesModified)
                return;

            string[] BeforePermissions = e.RoleBefore.Permissions.ToString().Split(", ");
            string[] AfterPermissions = e.RoleAfter.Permissions.ToString().Split(", ");

            bool PermissionsAdded = false;
            bool PermissionsRemoved = false;
            string PermissionDifference = "";

            foreach (string perm in AfterPermissions)
            {
                if (perm == "None")
                    continue;

                if (!BeforePermissions.Contains(perm))
                {
                    PermissionsAdded = true;
                    PermissionDifference += $"`+` `{perm}`\n";
                }
            }

            foreach (string perm in BeforePermissions)
            {
                if (perm == "None")
                    continue;

                if (!AfterPermissions.Contains(perm))
                {
                    PermissionsRemoved = true;
                    PermissionDifference += $"`-` `{perm}`\n";
                }
            }

            if (PermissionDifference.Length > 0)
                if (!PermissionsAdded && PermissionsRemoved)
                    PermissionDifference = $"\n**Permissions removed**:\n{PermissionDifference}";
                else if (PermissionsAdded && !PermissionsRemoved)
                    PermissionDifference = $"\n**Permissions added**:\n{PermissionDifference}";
                else
                    PermissionDifference = $"\n**Permissions updated**:\n{PermissionDifference}";

            string Integration = "";

            if (e.RoleAfter.IsManaged)
            {
                if (e.RoleAfter.Tags.IsPremiumSubscriber)
                    Integration = "**Integration**: `Server Booster`\n\n";

                if (e.RoleAfter.Tags.BotId is not null and not 0)
                {
                    var bot = await sender.GetUserAsync((ulong)e.RoleAfter.Tags.BotId);

                    Integration = $"**Integration**: {bot.Mention} `{bot.UsernameWithDiscriminator}`\n\n";
                }
            }

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserUpdated, Name = $"Role updated" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.RoleAfter.Id}" },
                Timestamp = DateTime.UtcNow,
                Description = $"**Role**: {e.RoleAfter.Mention} {(e.RoleBefore.Name != e.RoleAfter.Name ? $"`{e.RoleBefore.Name}` :arrow_right: `{e.RoleAfter.Name}`" : $"`{e.RoleAfter.Name}`")}\n" +
                                $"{(e.RoleBefore.Color.ToHex() != e.RoleAfter.Color.ToHex() ? $"**Color**: `{e.RoleBefore.Color.ToHex()}` :arrow_right: `{e.RoleAfter.Color.ToHex()}`\n" : "")}" +
                                $"{(e.RoleAfter.IsManaged ? "\n`This role belongs to an integration and cannot be deleted.`\n" : "")}" +
                                $"{Integration}" +
                                $"{(e.RoleBefore.IsMentionable != e.RoleAfter.IsMentionable ? $"{(!e.RoleBefore.IsMentionable && e.RoleAfter.IsMentionable ? "`Everyone can mention this role now.`\n" : "`The role can no longer be mentioned by everyone.`\n")}" : "")}" +
                                $"{(e.RoleBefore.IsHoisted != e.RoleAfter.IsHoisted ? $"{(!e.RoleBefore.IsHoisted && e.RoleAfter.IsHoisted ? "`Role members now display seperately from others.`\n" : "`Role members no longer display seperately from others.`\n")}" : "")}" +
                                $"{PermissionDifference}"
            }));

        }).Add(_bot._watcher);
    }

    internal async Task BanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.BanlistModified)
                return;

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserBanned, Name = $"User banned" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`\n" +
                                $"**Joined at**: `{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.JoinedAt, TimestampFormat.LongDateTime)}"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task BanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.Guild) || !_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.BanlistModified)
                return;

            _ = e.Guild.GetChannel(_bot._guilds.Servers[e.Guild.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.UserBanRemoved, Name = $"User unbanned" },
                Color = ColorHelper.Info,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.UsernameWithDiscriminator}`"
            }));
        }).Add(_bot._watcher);
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await ValidateServer(e.GuildAfter) || !_bot._guilds.Servers[e.GuildAfter.Id].ActionLogSettings.GuildModified)
                return;

            string Description = "";

            try { Description += $"{(e.GuildBefore.Owner.Id != e.GuildAfter.Owner.Id ? $"**Owner**: {e.GuildBefore.Owner.Mention} `{e.GuildBefore.Owner.UsernameWithDiscriminator}` :arrow_right: {e.GuildAfter.Owner.Mention} `{e.GuildAfter.Owner.UsernameWithDiscriminator}`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.Name != e.GuildAfter.Name ? $"**Name**: `{e.GuildBefore.Name}` :arrow_right: `{e.GuildAfter.Name}`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.Description != e.GuildAfter.Description ? $"**Description**: `{e.GuildBefore.Description}` :arrow_right: `{e.GuildAfter.Description}`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.IconHash != e.GuildAfter.IconHash ? $"`Icon updated`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.DefaultMessageNotifications != e.GuildAfter.DefaultMessageNotifications ? $"`Default Notifaction Settings: '{e.GuildAfter.DefaultMessageNotifications}'`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.VerificationLevel != e.GuildAfter.VerificationLevel ? $"`Verification Level changed: '{e.GuildAfter.VerificationLevel}'`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.BannerHash != e.GuildAfter.BannerHash ? $"`Banner updated`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.SplashHash != e.GuildAfter.SplashHash ? $"`Splash updated`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.DiscoverySplashHash != e.GuildAfter.DiscoverySplashHash ? $"`Discovery Splash updated`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.MfaLevel != e.GuildAfter.MfaLevel ? $"`Required Mfa Level changed: '{e.GuildAfter.MfaLevel}'`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.ExplicitContentFilter != e.GuildAfter.ExplicitContentFilter ? $"`Explicit Content Filter updated: '{e.GuildAfter.ExplicitContentFilter}'`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.WidgetEnabled != e.GuildAfter.WidgetEnabled ? $"{(!(bool)e.GuildBefore.WidgetEnabled && (bool)e.GuildAfter.WidgetEnabled ? "`Enabled Server Widget`" : "`Disabled Server Widget`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.WidgetChannel?.Id != e.GuildAfter.WidgetChannel?.Id ? $"**Widget Channel**: {e.GuildBefore.WidgetChannel.Mention} `[#{e.GuildBefore.WidgetChannel.Name}]` :arrow_right: {e.GuildAfter.WidgetChannel.Mention} `[#{e.GuildAfter.WidgetChannel.Name}]`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.IsLarge != e.GuildAfter.IsLarge ? $"{(!e.GuildBefore.IsLarge && e.GuildAfter.IsLarge ? "`The server is now considered 'Large'`" : "`The server is no longer considered 'Large'`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.IsNsfw != e.GuildAfter.IsNsfw ? $"{(!e.GuildBefore.IsNsfw && e.GuildAfter.IsNsfw ? "`The server is now considered as explicit`" : "`The server is no longer considered explicit`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.IsCommunity != e.GuildAfter.IsCommunity ? $"{(!e.GuildBefore.IsCommunity && e.GuildAfter.IsCommunity ? "`Enabled Community Features`" : "`Disabled Community Features`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.HasMemberVerificationGate != e.GuildAfter.HasMemberVerificationGate ? $"{(!e.GuildBefore.HasMemberVerificationGate && e.GuildAfter.HasMemberVerificationGate ? "`Enabled Membership Screening`" : "`Disabled Membership Screening`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.HasWelcomeScreen != e.GuildAfter.HasWelcomeScreen ? $"{(!e.GuildBefore.HasWelcomeScreen && e.GuildAfter.HasWelcomeScreen ? "`Enabled Welcome Screen`" : "`Disabled Welcome Screen`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.PremiumProgressBarEnabled != e.GuildAfter.PremiumProgressBarEnabled ? $"{(!e.GuildBefore.PremiumProgressBarEnabled && e.GuildAfter.PremiumProgressBarEnabled ? "`Enabled Boost Progress Bar`" : "`Disabled Boost Progress Bar`")}\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.RulesChannel?.Id != e.GuildAfter.RulesChannel?.Id ? $"**Rules Channel**: {e.GuildBefore.RulesChannel.Mention} `[#{e.GuildBefore.RulesChannel.Name}]` :arrow_right: {e.GuildAfter.RulesChannel.Mention} `[#{e.GuildAfter.RulesChannel.Name}]`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.AfkChannel?.Id != e.GuildAfter.AfkChannel?.Id ? $"**Afk Channel**: {e.GuildBefore.AfkChannel.Mention} `[#{e.GuildBefore.AfkChannel.Name}]` :arrow_right: {e.GuildAfter.AfkChannel.Mention} `[#{e.GuildAfter.AfkChannel.Name}]`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.SystemChannel?.Id != e.GuildAfter.SystemChannel?.Id ? $"**System Channel**: {e.GuildBefore.SystemChannel.Mention} `[#{e.GuildBefore.SystemChannel.Name}]` :arrow_right: {e.GuildAfter.SystemChannel.Mention} `[#{e.GuildAfter.SystemChannel.Name}]`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.PublicUpdatesChannel?.Id != e.GuildAfter.PublicUpdatesChannel?.Id ? $"**Discord Update Channel**: {e.GuildBefore.PublicUpdatesChannel.Mention} `[#{e.GuildBefore.PublicUpdatesChannel.Name}]` :arrow_right: {e.GuildAfter.PublicUpdatesChannel.Mention} `[#{e.GuildAfter.PublicUpdatesChannel.Name}]`\n" : "")}"; } catch { }
            try { Description += $"{(e.GuildBefore.MaxMembers != e.GuildAfter.MaxMembers ? $"`Maximum members updated to {e.GuildAfter.MaxMembers}`\n" : "")}"; } catch { }

            if (Description.Length == 0)
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.AuditLogIcons.GuildUpdated, Name = $"Guild updated" },
                Color = ColorHelper.Info,
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.GuildAfter.IconUrl },
                Description = Description

            };

            if (e.GuildBefore.IconHash != e.GuildAfter.IconHash)
                embed.ImageUrl = e.GuildAfter.IconUrl;

            _ = e.GuildAfter.GetChannel(_bot._guilds.Servers[e.GuildAfter.Id].ActionLogSettings.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }).Add(_bot._watcher);
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        Task.Run(async () =>
        {

        }).Add(_bot._watcher);
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            throw new NotImplementedException();
        }).Add(_bot._watcher);
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
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
