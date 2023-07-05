// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class ActionlogEvents : RequiresTranslation
{
    public ActionlogEvents(Bot bot) : base(bot)
    {
    }

    internal async Task<bool> ValidateServer(DiscordGuild guild)
    {
        if (guild is null)
            return false;

        if (this.Bot.Guilds[guild.Id].ActionLog.Channel == 0)
            return false;

        if (!guild.Channels.ContainsKey(this.Bot.Guilds[guild.Id].ActionLog.Channel))
        {
            this.Bot.Guilds[guild.Id].ActionLog = new(this.Bot, this.Bot.Guilds[guild.Id]);
            return false;
        }

        return true;
    }

    private async Task<DiscordMessage> SendActionlog(DiscordGuild guild, DiscordMessageBuilder builder)
        => await guild.GetChannel(this.Bot.Guilds[guild.Id].ActionLog.Channel).SendMessageAsync(builder);

    internal async Task UserJoined(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MembersModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserAdded, Name = $"User {(e.Member.MemberFlags.HasFlag(MemberFlags.DidRejoin) ? "re" : "")}joined" },
            Color = EmbedColors.Success,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                            $"**Account Age**: `{e.Member.CreationTimestamp.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.CreationTimestamp, TimestampFormat.LongDateTime)}"
        };

        if (this.Bot.globalNotes.TryGetValue(e.Member.Id, out List<BanDetails> globalNote) && globalNote.Any())
        {
            embed.AddField(new DiscordEmbedField("Makoto Staff Notes", $"{string.Join("\n\n", globalNote.Select(x => $"{x.Reason.FullSanitize()} - <@{x.Moderator}> {x.Timestamp.ToTimestamp()}"))}".TruncateWithIndication(512)));
        }

        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed)).ContinueWith(async x =>
        {
            if (!x.IsCompletedSuccessfully || !this.Bot.Guilds[e.Guild.Id].InviteTracker.Enabled)
                return;

            await Task.Delay(5000);

            int Wait = 0;

            if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));

            while (Wait < 10 && this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code == "")
            {
                Wait++;
                await Task.Delay(1000);
            }

            if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code == "")
                return;

            if (this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes.TryGetValue(this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code, out var inviteNoteDetails))
            {
                embed.AddField(new DiscordEmbedField("Invite Notes", inviteNoteDetails.Note));
            }

            embed.Description += $"\n\n**Invited by**: <@{this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId}>\n";
            embed.Description += $"**Invite Code**: `{this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code}`";

            if (this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes.TryGetValue(this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code, out var inviteNote))
                embed.Description += $"**Invite Note**: `{inviteNote.Note.SanitizeForCode()}`";

            _ = x.Result.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }

    internal async Task UserLeft(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MembersModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserLeft, Name = $"User left" },
            Color = EmbedColors.Error,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                            $"**Joined at**: `{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.JoinedAt, TimestampFormat.LongDateTime)}"
        };

        if (e.Member.Roles.Any())
            embed.AddField(new DiscordEmbedField("Roles", $"{string.Join(", ", e.Member.Roles.Select(x => x.Mention))}".TruncateWithIndication(1000)));

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        for (int i = 0; i < 3; i++)
        {
            var AuditKickLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Kick);
            var AuditBanLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Ban);

            if (AuditKickLogEntries.Count > 0 && AuditKickLogEntries.Any(x => ((DiscordAuditLogKickEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogKickEntry)AuditKickLogEntries.First(x => ((DiscordAuditLogKickEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));

                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Author.Name = "User kicked";
                embed.Author.IconUrl = AuditLogIcons.UserKicked;
                embed.Description += $"\n\n**Kicked by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                if (!string.IsNullOrWhiteSpace(Entry.Reason))
                    embed.Description += $"\n**Reason**: {Entry.Reason.SanitizeForCode()}";

                embed.Footer = new();
                embed.Footer.Text += "\n(Please note that the 'Kicked by' and 'Reason' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            if (this.Bot.Guilds[e.Guild.Id].ActionLog.BanlistModified && AuditBanLogEntries.Count > 0 && AuditBanLogEntries.Any(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogBanEntry)AuditBanLogEntries.First(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));

                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                _ = msg.DeleteAsync();
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted || e.Message.WebhookMessage || e.Message is null || e.Message.Author is null || e.Message.Author.IsBot)
            return;

        string prefix = e.Guild.GetGuildPrefix(Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.MessageDeleted, Name = $"Message deleted" },
            Color = EmbedColors.Error,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Message.Author.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Message.Author.AvatarUrl },
            Description = $"**User**: {e.Message.Author.Mention} `{e.Message.Author.GetUsernameWithIdentifier()}`\n" +
                            $"**Channel**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`"
        };

        if (!string.IsNullOrWhiteSpace(e.Message.Content))
            embed.AddField(new DiscordEmbedField("Content", $"`{e.Message.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));

        if (e.Message.Attachments.Count != 0)
            embed.AddField(new DiscordEmbedField("Attachments", $"{string.Join("\n", e.Message.Attachments.Select(x => $"`[{Math.Round(Convert.ToDecimal(x.FileSize / 1024), 2)} KB]` `{x.Url}`"))}"));

        if (e.Message.Stickers.Count != 0)
            embed.AddField(new DiscordEmbedField("Stickers", $"{string.Join("\n", e.Message.Stickers.Select(x => $"`{x.Name}`"))}"));

        if (e.Message.ReferencedMessage is not null)
            embed.AddField(new DiscordEmbedField("Reply to", $"{(e.Message.ReferencedMessage.Author is not null ? $"{e.Message.ReferencedMessage.Author.Mention}: " : "")}[`Jump to message`]({e.Message.ReferencedMessage.JumpLink})"));

        if (embed.Fields.Count == 0)
            return;

        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.VoiceStateUpdated)
            return;

        DiscordChannel PreviousChannel = e.Before?.Channel;
        DiscordChannel NewChannel = e.After?.Channel;

        if (PreviousChannel != NewChannel)
            if (PreviousChannel is null && NewChannel is not null)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.VoiceStateUserJoined, Name = $"User joined Voice Channel" },
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.User.AvatarUrl },
                    Color = EmbedColors.Success,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.User.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"**User**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                    $"**Channel**: {NewChannel.Mention} `[🔊{NewChannel.Name}]`"
                };

                await e.Guild.GetChannel(this.Bot.Guilds[e.Guild.Id].ActionLog.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }
            else if (PreviousChannel is not null && NewChannel is null)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.VoiceStateUserLeft, Name = $"User left Voice Channel" },
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.User.AvatarUrl },
                    Color = EmbedColors.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.User.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"**User**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                    $"**Channel**: {PreviousChannel.Mention} `[🔊{PreviousChannel.Name}]`"
                };

                await e.Guild.GetChannel(this.Bot.Guilds[e.Guild.Id].ActionLog.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }
            else if (PreviousChannel is not null && NewChannel is not null)
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.VoiceStateUserUpdated, Name = $"User switched Voice Channel" },
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.User.AvatarUrl },
                    Color = EmbedColors.Warning,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.User.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"**User**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                    $"**Channel**: {PreviousChannel.Mention} `[🔊{PreviousChannel.Name}]` :arrow_right: {NewChannel.Mention} `[🔊{NewChannel.Name}]`"
                };

                await e.Guild.GetChannel(this.Bot.Guilds[e.Guild.Id].ActionLog.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
            }
    }

    internal async Task MessageBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.MessageDeleted, Name = $"Multiple Messages deleted" },
            Color = EmbedColors.Error,
            Timestamp = DateTime.UtcNow,
            Description = $"**Channel**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`\n" +
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
                CurrentMessage += $"[{b.Timestamp.ToUniversalTime():dd.MM.yyyy, HH:mm:ss zzz}] {b.Author.GetUsernameWithIdentifier()} (UserId: '{b.Author.Id}' | MessageId: {b.Id})\n";
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
                            $"{e.Messages.Count} messages deleted in {e.Channel.GetIcon()}{e.Channel.Name} ({e.Channel.Id}) on {e.Guild.Name} ({e.Guild.Id})\n\n\n" +
                            $"{Messages}";

        string FileName = $"{Guid.NewGuid()}.txt";
        File.WriteAllText(FileName, FileContent);
        using (FileStream fileStream = new(FileName, FileMode.Open))
        {
            await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed).WithFile(FileName, fileStream));
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
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) ||
            !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted ||
            e.Message is null ||
            e.MessageBefore is null ||
            e.Message.WebhookMessage ||
            e.Message.Author is null ||
            e.Message.Author.IsBot)
            return;

        string prefix = e.Guild.GetGuildPrefix(Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.MessageEdited, Name = $"Message updated" },
            Color = EmbedColors.Warning,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Message.Author.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Message.Author?.AvatarUrl },
            Description = $"**User**: {e.Message.Author?.Mention} `{e.Message.Author?.GetUsernameWithIdentifier()}`\n" +
                          $"**Channel**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`\n" +
                          $"**Message**: [`Jump to message`]({e.Message.JumpLink})"
        };

        if (e.MessageBefore.Content != e.Message.Content)
        {
            if (!string.IsNullOrWhiteSpace(e.MessageBefore.Content))
                embed.AddField(new DiscordEmbedField("Previous Content", $"`{e.MessageBefore.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));

            if (!string.IsNullOrWhiteSpace(e.Message.Content))
                embed.AddField(new DiscordEmbedField("New Content", $"`{e.Message.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));
        }
        else
        {
            return;
        }

        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
    }

    internal async Task MemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MemberModified)
            return;

        if (e.NicknameBefore != e.NicknameAfter)
        {
            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Nickname updated" },
                Color = EmbedColors.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
            };

            if (string.IsNullOrWhiteSpace(e.NicknameBefore))
                embed.Author.Name = "Nickname added";
            else
                embed.AddField(new DiscordEmbedField("Previous Nickname", $"`{e.NicknameBefore}`"));

            if (string.IsNullOrWhiteSpace(e.NicknameAfter))
                embed.Author.Name = "Nickname removed";
            else
                embed.AddField(new DiscordEmbedField("New Nickname", $"`{e.NicknameAfter}`"));

            _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
        }

        bool RolesUpdated = false;

        foreach (var role in e.RolesBefore)
        {
            if (!e.RolesAfter.Any(x => x.Id == role.Id))
            {
                await Task.Delay(3000);
                RolesUpdated = true;

                if (!e.Guild.Roles.ContainsKey(role.Id))
                {
                    RolesUpdated = false;
                    continue;
                }

                break;
            }
        }

        if (!RolesUpdated)
            foreach (var role in e.RolesAfter)
            {
                if (!e.RolesBefore.Any(x => x.Id == role.Id))
                {
                    await Task.Delay(3000);
                    RolesUpdated = true;

                    if (!e.Guild.Roles.ContainsKey(role.Id))
                    {
                        RolesUpdated = false;
                        continue;
                    }

                    break;
                }
            }

        if (RolesUpdated)
        {
            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Roles updated" },
                Color = EmbedColors.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
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
            {
                embed.Author.Name = "Roles added";
                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = AuditLogIcons.UserAdded;
            }
            else if (!RolesAdded && RolesRemoved)
            {
                embed.Author.Name = "Roles removed";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = AuditLogIcons.UserLeft;
            }
            else
            {
                embed.Author.Name = "Roles updated";
                embed.Color = EmbedColors.Warning;
                embed.Author.IconUrl = AuditLogIcons.UserUpdated;
            }

            embed.Description += $"\n\n{Roles}";

            _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
        }

        if (e.TimeoutBefore != e.TimeoutAfter)
        {
            // Timeouts don't seem to fire the member updated event, will keep this code for potential future updates.

            if (e.TimeoutAfter?.ToUniversalTime() > e.TimeoutBefore?.ToUniversalTime())
                _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanned, Name = $"User timed out" },
                    Color = EmbedColors.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                                    $"**Timed out until**: {Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.LongDateTime)} ({Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.RelativeTime)})"
                }));

            if (e.TimeoutAfter?.ToUniversalTime() < e.TimeoutBefore?.ToUniversalTime())
                _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanRemoved, Name = $"User timeout removed" },
                    Color = EmbedColors.Success,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                    Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
                }));
        }

        if (e.PendingBefore != e.PendingAfter)
        {
            try
            {
                if ((e.PendingBefore is null && e.PendingAfter is true) || (e.PendingAfter is true && e.PendingBefore is false))
                    _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserAdded, Name = $"Membership approved" },
                        Color = EmbedColors.Success,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                        Timestamp = DateTime.UtcNow,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                        Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
                    }));
            }
            catch { }
        }

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.MemberProfileModified)
            return;

        if (e.AvatarHashBefore != e.AvatarHashAfter)
        {
            // Normal avatar updates don't seem to fire the member updated event, will keep this code for potential future updates.

            _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Member Profile Picture updated" },
                Color = EmbedColors.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`",
                ImageUrl = e.Member.AvatarUrl
            }));
        }

        if (e.GuildAvatarHashBefore != e.GuildAvatarHashAfter)
        {
            _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Member Guild Profile Picture updated" },
                Color = EmbedColors.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
                Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`",
                ImageUrl = e.Member.GuildAvatarUrl
            }));
        }
    }

    internal async Task RoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
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

                Integration = $"**Integration**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserAdded, Name = $"Role created" },
            Color = EmbedColors.Success,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.Role.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = $"**Role**: {e.Role.Mention} `{e.Role.Name}`\n" +
                                        $"**Color**: `{ColorTools.ToHex(e.Role.Color.R, e.Role.Color.G, e.Role.Color.B)}`\n" +
                                        $"{(e.Role.IsManaged ? "\n`This role belongs to an integration and cannot be deleted.`\n" : "")}" +
                                        $"{Integration}" +
                                        $"{(e.Role.IsMentionable ? "`Everyone can mention this role.`\n" : "")}" +
                                        $"{(e.Role.IsHoisted ? "`Role members are displayed separately from others.`\n" : "")}" +
                                        $"\n**Permissions**: {GeneratePermissions}"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleCreate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Created by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Created by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task RoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
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

                Integration = $"**Integration**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserLeft, Name = $"Role deleted" },
            Color = EmbedColors.Error,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.Role.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = $"**Role**: `{e.Role.Name}`\n" +
                                        $"**Color**: `{e.Role.Color.ToHex()}`\n" +
                                        $"{(e.Role.IsManaged ? "\n`This role belonged to an integration and was therefor deleted automatically.`\n" : "")}" +
                                        $"{Integration}" +
                                        $"{(e.Role.IsMentionable ? "`Everyone could mention this role.`\n" : "")}" +
                                        $"{(e.Role.IsHoisted ? "`Role members were displayed separately from others.`\n" : "")}" +
                                        $"\n**Permissions**: {GeneratePermissions}"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Deleted by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Deleted by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task RoleModified(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
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

                Integration = $"**Integration**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        string Description = $"{(e.RoleBefore.Name != e.RoleAfter.Name ? "true" : "")}{(e.RoleBefore.Color.ToHex() != e.RoleAfter.Color.ToHex() ? $"**Color**: `{e.RoleBefore.Color.ToHex()}` :arrow_right: `{e.RoleAfter.Color.ToHex()}`\n" : "")}" +
                                $"{(e.RoleBefore.IsMentionable != e.RoleAfter.IsMentionable ? $"{(!e.RoleBefore.IsMentionable && e.RoleAfter.IsMentionable ? "`Everyone can mention this role now.`\n" : "`The role can no longer be mentioned by everyone.`\n")}" : "")}" +
                                $"{(e.RoleBefore.IsHoisted != e.RoleAfter.IsHoisted ? $"{(!e.RoleBefore.IsHoisted && e.RoleAfter.IsHoisted ? "`Role members now display separately from others.`\n" : "`Role members no longer display separately from others.`\n")}" : "")}" +
                                $"{PermissionDifference}";

        if (Description.Length == 0)
            return;

        Description = $"**Role**: {e.RoleAfter.Mention} {(e.RoleBefore.Name != e.RoleAfter.Name ? $"`{e.RoleBefore.Name}` :arrow_right: `{e.RoleAfter.Name}`" : $"`{e.RoleAfter.Name}`")}\n" +
                                        $"{(e.RoleBefore.Color.ToHex() != e.RoleAfter.Color.ToHex() ? $"**Color**: `{e.RoleBefore.Color.ToHex()}` :arrow_right: `{e.RoleAfter.Color.ToHex()}`\n" : "")}" +
                                        $"{(e.RoleAfter.IsManaged ? "\n`This role belongs to an integration and cannot be deleted.`\n" : "")}" +
                                        $"{Integration}" +
                                        $"{(e.RoleBefore.IsMentionable != e.RoleAfter.IsMentionable ? $"{(!e.RoleBefore.IsMentionable && e.RoleAfter.IsMentionable ? "`Everyone can mention this role now.`\n" : "`The role can no longer be mentioned by everyone.`\n")}" : "")}" +
                                        $"{(e.RoleBefore.IsHoisted != e.RoleAfter.IsHoisted ? $"{(!e.RoleBefore.IsHoisted && e.RoleAfter.IsHoisted ? "`Role members now display separately from others.`\n" : "`Role members no longer display separately from others.`\n")}" : "")}" +
                                        $"{PermissionDifference}";

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Role updated" },
            Color = EmbedColors.Warning,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Role-Id: {e.RoleAfter.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = Description
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.RoleAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.RoleAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Modified by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Modified by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task BanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.BanlistModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanned, Name = $"User banned" },
            Color = EmbedColors.Error,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`" +
                            $"{(e.Member.JoinedAt.Year > 2014 ? $"\n**Joined at**: `{e.Member.JoinedAt.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(e.Member.JoinedAt, TimestampFormat.LongDateTime)}" : (this.Bot.Guilds[e.Guild.Id].Members.TryGetValue(e.Member.Id, out var member) ? (member.FirstJoinDate != DateTime.UnixEpoch ? $"\n**First joined at**: `{member.FirstJoinDate.GetTotalSecondsSince().GetHumanReadable()}` {Formatter.Timestamp(member.FirstJoinDate, TimestampFormat.LongDateTime)}" : "") : ""))}"
        };
        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (e.Member.Roles.Any())
            embed.AddField(new DiscordEmbedField("Roles", $"{string.Join(", ", e.Member.Roles.Select(x => x.Mention))}".TruncateWithIndication(1000)));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Ban);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogBanEntry)AuditLogEntries.First(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Banned by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                if (!string.IsNullOrWhiteSpace(Entry.Reason))
                    embed.Description += $"\n**Reason**: {Entry.Reason.SanitizeForCode()}";

                embed.Footer.Text += "\n(Please note that the 'Banned by' and 'Reason' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task BanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.BanlistModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanRemoved, Name = $"User unbanned" },
            Color = EmbedColors.Success,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Unban);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogBanEntry)AuditLogEntries.First(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Unbanned by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                embed.Footer.Text += "\n(Please note that the 'Unbanned by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        if (!await ValidateServer(e.GuildAfter) || !this.Bot.Guilds[e.GuildAfter.Id].ActionLog.GuildModified)
            return;

        string Description = "";

        try
        { Description += $"{(e.GuildBefore.Owner.Id != e.GuildAfter.Owner.Id ? $"**Owner**: {e.GuildBefore.Owner.Mention} `{e.GuildBefore.Owner.GetUsernameWithIdentifier()}` :arrow_right: {e.GuildAfter.Owner.Mention} `{e.GuildAfter.Owner.GetUsernameWithIdentifier()}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.Name != e.GuildAfter.Name ? $"**Name**: `{e.GuildBefore.Name}` :arrow_right: `{e.GuildAfter.Name}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.Description != e.GuildAfter.Description ? $"**Description**: `{e.GuildBefore.Description}` :arrow_right: `{e.GuildAfter.Description}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IconHash != e.GuildAfter.IconHash ? $"`Icon updated`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.DefaultMessageNotifications != e.GuildAfter.DefaultMessageNotifications ? $"`Default Notification Settings: '{e.GuildAfter.DefaultMessageNotifications}'`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.VerificationLevel != e.GuildAfter.VerificationLevel ? $"`Verification Level changed: '{e.GuildAfter.VerificationLevel}'`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.BannerHash != e.GuildAfter.BannerHash ? $"`Banner updated`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.SplashHash != e.GuildAfter.SplashHash ? $"`Splash updated`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.DiscoverySplashHash != e.GuildAfter.DiscoverySplashHash ? $"`Discovery Splash updated`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.MfaLevel != e.GuildAfter.MfaLevel ? $"`Required Mfa Level changed: '{e.GuildAfter.MfaLevel}'`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.ExplicitContentFilter != e.GuildAfter.ExplicitContentFilter ? $"`Explicit Content Filter updated: '{e.GuildAfter.ExplicitContentFilter}'`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.WidgetEnabled != e.GuildAfter.WidgetEnabled ? $"{(!(bool)e.GuildBefore.WidgetEnabled && (bool)e.GuildAfter.WidgetEnabled ? "`Enabled Server Widget`" : "`Disabled Server Widget`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.WidgetChannel?.Id != e.GuildAfter.WidgetChannel?.Id ? $"**Widget Channel**: {e.GuildBefore.WidgetChannel.Mention} `[#{e.GuildBefore.WidgetChannel.Name}]` :arrow_right: {e.GuildAfter.WidgetChannel.Mention} `[#{e.GuildAfter.WidgetChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsLarge != e.GuildAfter.IsLarge ? $"{(!e.GuildBefore.IsLarge && e.GuildAfter.IsLarge ? "`The server is now considered 'Large'`" : "`The server is no longer considered 'Large'`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsNsfw != e.GuildAfter.IsNsfw ? $"{(!e.GuildBefore.IsNsfw && e.GuildAfter.IsNsfw ? "`The server is now considered as explicit`" : "`The server is no longer considered explicit`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsCommunity != e.GuildAfter.IsCommunity ? $"{(!e.GuildBefore.IsCommunity && e.GuildAfter.IsCommunity ? "`Enabled Community Features`" : "`Disabled Community Features`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.HasMemberVerificationGate != e.GuildAfter.HasMemberVerificationGate ? $"{(!e.GuildBefore.HasMemberVerificationGate && e.GuildAfter.HasMemberVerificationGate ? "`Enabled Membership Screening`" : "`Disabled Membership Screening`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.HasWelcomeScreen != e.GuildAfter.HasWelcomeScreen ? $"{(!e.GuildBefore.HasWelcomeScreen && e.GuildAfter.HasWelcomeScreen ? "`Enabled Welcome Screen`" : "`Disabled Welcome Screen`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.PremiumProgressBarEnabled != e.GuildAfter.PremiumProgressBarEnabled ? $"{(!e.GuildBefore.PremiumProgressBarEnabled && e.GuildAfter.PremiumProgressBarEnabled ? "`Enabled Boost Progress Bar`" : "`Disabled Boost Progress Bar`")}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.RulesChannel?.Id != e.GuildAfter.RulesChannel?.Id ? $"**Rules Channel**: {e.GuildBefore.RulesChannel.Mention} `[#{e.GuildBefore.RulesChannel.Name}]` :arrow_right: {e.GuildAfter.RulesChannel.Mention} `[#{e.GuildAfter.RulesChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.AfkChannel?.Id != e.GuildAfter.AfkChannel?.Id ? $"**Afk Channel**: {e.GuildBefore.AfkChannel.Mention} `[#{e.GuildBefore.AfkChannel.Name}]` :arrow_right: {e.GuildAfter.AfkChannel.Mention} `[#{e.GuildAfter.AfkChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.SystemChannel?.Id != e.GuildAfter.SystemChannel?.Id ? $"**System Channel**: {e.GuildBefore.SystemChannel.Mention} `[#{e.GuildBefore.SystemChannel.Name}]` :arrow_right: {e.GuildAfter.SystemChannel.Mention} `[#{e.GuildAfter.SystemChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.PublicUpdatesChannel?.Id != e.GuildAfter.PublicUpdatesChannel?.Id ? $"**Discord Update Channel**: {e.GuildBefore.PublicUpdatesChannel.Mention} `[#{e.GuildBefore.PublicUpdatesChannel.Name}]` :arrow_right: {e.GuildAfter.PublicUpdatesChannel.Mention} `[#{e.GuildAfter.PublicUpdatesChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.MaxMembers != e.GuildAfter.MaxMembers ? $"`Maximum members updated to {e.GuildAfter.MaxMembers}`\n" : "")}"; }
        catch { }

        if (Description.Length == 0)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.GuildUpdated, Name = $"Guild updated" },
            Color = EmbedColors.Warning,
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.GuildAfter.IconUrl },
            Description = Description

        };

        if (e.GuildBefore.IconHash != e.GuildAfter.IconHash)
            embed.ImageUrl = e.GuildAfter.IconUrl;

        var msg = await e.GuildAfter.GetChannel(this.Bot.Guilds[e.GuildAfter.Id].ActionLog.Channel).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.GuildAfter.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.GuildAfter.GetAuditLogsAsync(actionType: AuditLogActionType.GuildUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => (!this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id))))
            {
                var Entry = (DiscordAuditLogGuildEntry)AuditLogEntries.First(x => !this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Modified by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                embed.Footer = new();
                embed.Footer.Text += "\n(Please note that the 'Modified by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.ChannelAdded, Name = $"Channel created" },
            Color = EmbedColors.Success,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Channel-Id: {e.Channel.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = $"**Name**: {e.Channel.Mention} `[{(e.Channel.Type is ChannelType.Text or ChannelType.News or ChannelType.Store or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread ? "#" : $"{(e.Channel.Type is ChannelType.Voice or ChannelType.Stage ? "🔊" : "")}")}{e.Channel.Name}]`\n"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelCreate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Created by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Created by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.ChannelRemoved, Name = $"Channel deleted" },
            Color = EmbedColors.Error,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Channel-Id: {e.Channel.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = $"**Name**: `[{(e.Channel.Type is ChannelType.Text or ChannelType.News or ChannelType.Store or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread ? "#" : $"{(e.Channel.Type is ChannelType.Voice or ChannelType.Stage ? "🔊" : "")}")}{e.Channel.Name}]`\n"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Deleted by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Deleted by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        if (e.ChannelBefore?.Name == e.ChannelAfter?.Name && e.ChannelBefore?.IsNsfw == e.ChannelAfter?.IsNsfw)
            return;

        string Icon = $"{(e.ChannelAfter.Type is ChannelType.Text or ChannelType.News or ChannelType.Store or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread ? "#" : $"{(e.ChannelAfter.Type is ChannelType.Voice or ChannelType.Stage ? "🔊" : "")}")}";

        string Description = $"{(e.ChannelBefore.Name != e.ChannelAfter.Name ? $"**Name**: {e.ChannelBefore.Mention} `[{Icon}{e.ChannelBefore.Name}]` :arrow_right: `[{Icon}{e.ChannelAfter.Name}]`\n" : $"{e.ChannelAfter.Mention} `[{Icon}{e.ChannelAfter.Name}]`\n")}" +
                                $"{(e.ChannelBefore.IsNsfw != e.ChannelAfter.IsNsfw ? $"{(!e.ChannelBefore.IsNsfw && e.ChannelAfter.IsNsfw ? "`The channel is now marked as NSFW.`" : "`The channel is no longer marked as NSFW.`")}\n" : "")}";

        if (Description.Length == 0)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.ChannelModified, Name = $"Channel updated" },
            Color = EmbedColors.Warning,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Channel-Id: {e.ChannelAfter.Id}" },
            Timestamp = DateTime.UtcNow,
            Description = Description
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.ChannelAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.ChannelAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Modified by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += "\n(Please note that the 'Modified by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.InvitesModified)
            return;

        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.InviteAdded, Name = $"Invite created" },
            Color = EmbedColors.Success,
            Timestamp = DateTime.UtcNow,
            Description = $"**Invite**: `https://discord.gg/{e.Invite.Code}`\n" +
                            $"**Created by**: {e.Invite.Inviter?.Mention ?? "`No Inviter found.`"} `{e.Invite.Inviter?.GetUsernameWithIdentifier() ?? "-"}`\n" +
                            $"**Channel**: {e.Channel.Mention} `[{(e.Channel.Type is ChannelType.Text or ChannelType.News or ChannelType.Store or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread ? "#" : $"{(e.Channel.Type is ChannelType.Voice or ChannelType.Stage ? "🔊" : "")}")}{e.Channel.Name}]`"
        }));
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        if (!await ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.InvitesModified)
            return;

        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.InviteRemoved, Name = $"Invite deleted" },
            Color = EmbedColors.Error,
            Timestamp = DateTime.UtcNow,
            Description = $"**Invite**: `https://discord.gg/{e.Invite.Code}`\n" +
                            $"**Created by**: {e.Invite.Inviter?.Mention ?? "`No Inviter found.`"} `{e.Invite.Inviter?.GetUsernameWithIdentifier() ?? "-"}`\n" +
                            $"**Channel**: {e.Channel?.Mention} `[{(e.Channel?.Type is ChannelType.Text or ChannelType.News or ChannelType.Store or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread ? "#" : $"{(e.Channel.Type is ChannelType.Voice or ChannelType.Stage ? "🔊" : "")}")}{e.Channel?.Name}]`"
        };

        var msg = await SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));


        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (int i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.InviteDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogInviteEntry)x).Target.Code == e.Invite.Code && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogInviteEntry)AuditLogEntries.First(x => ((DiscordAuditLogInviteEntry)x).Target.Code == e.Invite.Code && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**Deleted by**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer = new();
                embed.Footer.Text += "\n(Please note that the 'Deleted by' may not be accurate as the bot can't differentiate between similar audit log entries that affect the same things.)";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }
}
