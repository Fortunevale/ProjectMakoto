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

    Translations.events.actionlog tKey => this.t.Events.Actionlog;

    private Task<DiscordMessage> SendActionlog(DiscordGuild guild, DiscordMessageBuilder builder)
        => guild.GetChannel(this.Bot.Guilds[guild.Id].ActionLog.Channel).SendMessageAsync(builder);

    internal async Task UserJoined(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MembersModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(e.Member.MemberFlags.HasFlag(MemberFlags.DidRejoin) ? this.tKey.UserRejoined.Get(this.Bot.Guilds[e.Guild.Id]).Build() : this.tKey.UserJoined.Get(this.Bot.Guilds[e.Guild.Id]).Build(), 
                null, AuditLogIcons.UserAdded)
            .WithColor(EmbedColors.Success)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id]).Build()}: {e.Member.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                             $"**{this.tKey.AccountAge.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {e.Member.CreationTimestamp.ToTimestamp()} ({e.Member.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)})");

        if (this.Bot.globalNotes.TryGetValue(e.Member.Id, out var globalNote) && globalNote.Any())
        {
            _ = embed.AddField(new DiscordEmbedField(this.tKey.StaffNotes.Get(this.Bot.Guilds[e.Guild.Id]).Build(),
                $"{string.Join("\n\n", globalNote.Select(x => $"{x.Reason.FullSanitize()} - <@{x.Moderator}> {x.Timestamp.ToTimestamp()}"))}".TruncateWithIndication(512)));
        }

        var message = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        await Task.Delay(5000);

        var Wait = 0;

        if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));

        while (Wait < 10 && this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code == "")
        {
            Wait++;
            await Task.Delay(1000);
        }

        if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code == "")
            return;

        embed.Description += $"\n\n**{this.tKey.InvitedBy.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: <@{this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId}>\n";
        embed.Description += $"**{this.tKey.InviteCode.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: `{this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code}`";

        if (this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes.TryGetValue(this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code, out var inviteNote))
            embed.Description += $"**{this.tKey.InviteNote.Get(this.Bot.Guilds[e.Guild.Id])}**: `{inviteNote.Note.SanitizeForCode()}`";

        _ = message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
    }

    internal async Task UserLeft(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MembersModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.UserLeft.Get(this.Bot.Guilds[e.Guild.Id]).Build(), null, AuditLogIcons.UserLeft)
            .WithColor(EmbedColors.Error)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id]).Build()}: {e.Member.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                             $"**{this.tKey.JoinedAt.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {e.Member.JoinedAt.ToTimestamp()} ({e.Member.JoinedAt.ToTimestamp(TimestampFormat.LongDateTime)})");

        if (e.Member.Roles.Any())
            _ = embed.AddField(new DiscordEmbedField(this.tKey.Roles.Get(this.Bot.Guilds[e.Guild.Id]).Build(), $"{string.Join(", ", e.Member.Roles.Select(x => x.Mention))}".TruncateWithIndication(1000)));

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        for (var i = 0; i < 3; i++)
        {
            var AuditKickLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Kick);
            var AuditBanLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Ban);

            if (AuditKickLogEntries.Count > 0 && AuditKickLogEntries.Any(x => ((DiscordAuditLogKickEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogKickEntry)AuditKickLogEntries.First(x => ((DiscordAuditLogKickEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));

                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Author.Name = this.tKey.UserKicked.Get(this.Bot.Guilds[e.Guild.Id]).Build();
                embed.Author.IconUrl = AuditLogIcons.UserKicked;
                embed.Description += $"\n\n**{this.tKey.KickedBy.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                if (!string.IsNullOrWhiteSpace(Entry.Reason))
                    embed.Description += $"\n**{this.tKey.Reason.Get(this.Bot.Guilds[e.Guild.Id]).Build()}**: {Entry.Reason.SanitizeForCode()}";

                embed.Footer = new();
                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.KickedBy.Get(this.Bot.Guilds[e.Guild.Id])}' & '{this.tKey.Reason.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

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
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted || e.Message.WebhookMessage || e.Message is null || e.Message.Author is null || e.Message.Author.IsBot)
            return;

        var prefix = e.Guild.GetGuildPrefix(this.Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.MessageDeleted.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.MessageDeleted)
            .WithColor(EmbedColors.Error)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Message.Author.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Message.Author.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Message.Author.Mention} `{e.Message.Author.GetUsernameWithIdentifier()}`\n" +
                             $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`");

        if (!string.IsNullOrWhiteSpace(e.Message.Content))
            _ = embed.AddField(new DiscordEmbedField(this.tKey.Content.Get(this.Bot.Guilds[e.Guild.Id]), $"`{e.Message.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));

        if (e.Message.Attachments.Count != 0)
            _ = embed.AddField(new DiscordEmbedField(this.tKey.Attachments.Get(this.Bot.Guilds[e.Guild.Id]), $"{string.Join("\n", e.Message.Attachments.Select(x => $"`[{x.FileSize.Value.FileSizeToHumanReadable()}]` `{x.Url}`"))}"));

        if (e.Message.Stickers.Count != 0)
            _ = embed.AddField(new DiscordEmbedField(this.tKey.Stickers.Get(this.Bot.Guilds[e.Guild.Id]), $"{string.Join("\n", e.Message.Stickers.Select(x => $"`{x.Name}`"))}"));

        if (e.Message.ReferencedMessage is not null)
            _ = embed.AddField(new DiscordEmbedField(this.tKey.ReplyTo.Get(this.Bot.Guilds[e.Guild.Id]), $"{(e.Message.ReferencedMessage.Author is not null ? $"{e.Message.ReferencedMessage.Author.Mention}: " : "")}[`{this.t.Common.JumpToMessage.Get(this.Bot.Guilds[e.Guild.Id])}`]({e.Message.ReferencedMessage.JumpLink})"));

        if (embed.Fields.Count == 0)
            return;

        _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.VoiceStateUpdated)
            return;

        var PreviousChannel = e.Before?.Channel;
        var NewChannel = e.After?.Channel;

        if (PreviousChannel != NewChannel)
            if (PreviousChannel is null && NewChannel is not null)
            {
                _ = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithAuthor(this.tKey.UserJoinedVoiceChannel.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.VoiceStateUserJoined)
                    .WithThumbnail(e.User.AvatarUrl)
                    .WithColor(EmbedColors.Success)
                    .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                     $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {NewChannel.Mention} `[{NewChannel.GetIcon()}{NewChannel.Name}]`")));
            }
            else if (PreviousChannel is not null && NewChannel is null)
            {
                _ = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithAuthor(this.tKey.UserLeftVoiceChannel.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.VoiceStateUserLeft)
                    .WithThumbnail(e.User.AvatarUrl)
                    .WithColor(EmbedColors.Error)
                    .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                     $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {PreviousChannel.Mention} `[{PreviousChannel.GetIcon()}{PreviousChannel.Name}]`")));
            }
            else if (PreviousChannel is not null && NewChannel is not null)
            {
                _ = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    .WithAuthor(this.tKey.UserSwitchedVoiceChannel.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.VoiceStateUserUpdated)
                    .WithThumbnail(e.User.AvatarUrl)
                    .WithColor(EmbedColors.Warning)
                    .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.User.Id}")
                    .WithTimestamp(DateTime.UtcNow)
                    .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.User.Mention} `{e.User.GetUsernameWithIdentifier()}`\n" +
                                     $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {PreviousChannel.Mention} `[{PreviousChannel.GetIcon()}{PreviousChannel.Name}]` ➡ {NewChannel.Mention} `[{NewChannel.GetIcon()}{NewChannel.Name}]`")));
            }
    }

    internal async Task MessageBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.MultipleMessagesDeleted.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.MessageDeleted)
            .WithColor(EmbedColors.Error)
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`\n" +
                             $"{this.tKey.CheckAttachedFileForDeletedMessages.Get(this.Bot.Guilds[e.Guild.Id]).Build(true)}");

        var Messages = "";

        foreach (var b in e.Messages)
        {
            if (b is null || b.WebhookMessage || b.Author is null)
                continue;

            var CurrentMessage = "";

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

        var FileContent = $"All dates are saved in universal time (UTC+0).\n\n\n" +
                             $"{e.Messages.Count} messages deleted in {e.Channel.GetIcon()}{e.Channel.Name} ({e.Channel.Id}) on {e.Guild.Name} ({e.Guild.Id})\n\n\n" +
                             $"{Messages}";

        var FileName = $"{Guid.NewGuid()}.txt";
        await File.WriteAllTextAsync(FileName, FileContent);
        using (FileStream fileStream = new(FileName, FileMode.Open))
        {
            _ = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed).WithFile(FileName, fileStream));
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
        if (!await this.ValidateServer(e.Guild) ||
            !this.Bot.Guilds[e.Guild.Id].ActionLog.MessageDeleted ||
            e.Message is null ||
            e.MessageBefore is null ||
            e.Message.WebhookMessage ||
            e.Message.Author is null ||
            e.Message.Author.IsBot)
            return;

        var prefix = e.Guild.GetGuildPrefix(this.Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.MessageUpdated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.MessageEdited)
            .WithColor(EmbedColors.Warning)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Message.Author?.Id ?? 0}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Message.Author?.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Message.Author?.Mention ?? "/"} `{e.Message.Author?.GetUsernameWithIdentifier() ?? "/"}`\n" +
                             $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`\n" +
                             $"**{this.tKey.Message.Get(this.Bot.Guilds[e.Guild.Id])}**: [`{this.t.Common.JumpToMessage.Get(this.Bot.Guilds[e.Guild.Id])}`]({e.Message.JumpLink})");

        if (e.MessageBefore.Content != e.Message.Content)
        {
            if (!string.IsNullOrWhiteSpace(e.MessageBefore.Content))
                _ = embed.AddField(new DiscordEmbedField(this.tKey.PreviousContent.Get(this.Bot.Guilds[e.Guild.Id]), $"`{e.MessageBefore.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));

            if (!string.IsNullOrWhiteSpace(e.Message.Content))
                _ = embed.AddField(new DiscordEmbedField(this.tKey.NewContent.Get(this.Bot.Guilds[e.Guild.Id]), $"`{e.Message.Content.SanitizeForCode().TruncateWithIndication(1022)}`"));
        }
        else
        {
            return;
        }

        _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
    }

    internal async Task MemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.MemberModified)
            return;

        if (e.NicknameBefore != e.NicknameAfter)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(this.tKey.MessageUpdated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserUpdated)
                .WithColor(EmbedColors.Warning)
                .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithThumbnail(e.Member.AvatarUrl)
                .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`");

            if (string.IsNullOrWhiteSpace(e.NicknameBefore))
                embed.Author.Name = this.tKey.NicknameAdded.Get(this.Bot.Guilds[e.Guild.Id]);
            else
                _ = embed.AddField(new DiscordEmbedField(this.tKey.PreviousNickname.Get(this.Bot.Guilds[e.Guild.Id]), $"`{e.NicknameBefore}`"));

            if (string.IsNullOrWhiteSpace(e.NicknameAfter))
                embed.Author.Name = this.tKey.NicknameRemoved.Get(this.Bot.Guilds[e.Guild.Id]);
            else
                _ = embed.AddField(new DiscordEmbedField(this.tKey.NewNickname.Get(this.Bot.Guilds[e.Guild.Id]), $"`{e.NicknameAfter}`"));

            _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
        }

        var RolesUpdated = false;

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
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(this.tKey.RolesUpdated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserUpdated)
                .WithColor(EmbedColors.Warning)
                .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithThumbnail(e.Member.AvatarUrl)
                .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.Member.GetUsernameWithIdentifier()}`");

            var Roles = "";

            var RolesAdded = false;
            var RolesRemoved = false;

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
                embed.Author.Name = this.tKey.RolesAdded.Get(this.Bot.Guilds[e.Guild.Id]);
                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = AuditLogIcons.UserAdded;
            }
            else if (!RolesAdded && RolesRemoved)
            {
                embed.Author.Name = this.tKey.RolesRemoved.Get(this.Bot.Guilds[e.Guild.Id]);
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = AuditLogIcons.UserLeft;
            }

            embed.Description += $"\n\n{Roles}";

            _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));
        }

        //if (e.TimeoutBefore != e.TimeoutAfter)
        //{
        //    // Timeouts don't seem to fire the member updated event, will keep this code for potential future updates.

        //    if (e.TimeoutAfter?.ToUniversalTime() > e.TimeoutBefore?.ToUniversalTime())
        //        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
        //        {
        //            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanned, Name = $"User timed out" },
        //            Color = EmbedColors.Error,
        //            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
        //            Timestamp = DateTime.UtcNow,
        //            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
        //            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
        //                            $"**Timed out until**: {Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.LongDateTime)} ({Formatter.Timestamp((DateTime)(e.TimeoutAfter?.ToUniversalTime().DateTime), TimestampFormat.RelativeTime)})"
        //        }));

        //    if (e.TimeoutAfter?.ToUniversalTime() < e.TimeoutBefore?.ToUniversalTime())
        //        _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
        //        {
        //            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserBanRemoved, Name = $"User timeout removed" },
        //            Color = EmbedColors.Success,
        //            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
        //            Timestamp = DateTime.UtcNow,
        //            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
        //            Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`"
        //        }));
        //}

        if (e.PendingBefore != e.PendingAfter)
        {
            try
            {
                if ((e.PendingBefore is null && e.PendingAfter is true) || (e.PendingAfter is true && e.PendingBefore is false))
                    _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        .WithAuthor(this.tKey.MembershipApproved.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserAdded)
                        .WithColor(EmbedColors.Success)
                        .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
                        .WithTimestamp(DateTime.UtcNow)
                        .WithThumbnail(e.Member.AvatarUrl)
                        .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`")));
            }
            catch { }
        }

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.MemberProfileModified)
            return;

        //if (e.AvatarHashBefore != e.AvatarHashAfter)
        //{
        //    // Normal avatar updates don't seem to fire the member updated event, will keep this code for potential future updates.

        //    _ = SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
        //    {
        //        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = AuditLogIcons.UserUpdated, Name = $"Member Profile Picture updated" },
        //        Color = EmbedColors.Warning,
        //        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User-Id: {e.Member.Id}" },
        //        Timestamp = DateTime.UtcNow,
        //        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Member.AvatarUrl },
        //        Description = $"**User**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`",
        //        ImageUrl = e.Member.AvatarUrl
        //    }));
        //}

        if (e.GuildAvatarHashBefore != e.GuildAvatarHashAfter)
        {
            _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                .WithAuthor(this.tKey.GuildProfilePictureUpdated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserUpdated)
                .WithColor(EmbedColors.Warning)
                .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithThumbnail(e.Member.AvatarUrl)
                .WithImageUrl(e.Member.GuildAvatarUrl)
                .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`")));
        }
    }

    internal async Task RoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
            return;

        var GeneratePermissions = string.Join(", ", e.Role.Permissions.GetEnumeration().Select(x => $"`{x.ToTranslatedPermissionString(this.Bot.Guilds[e.Guild.Id], this.Bot)}`"));
        var Integration = "";

        if (e.Role.IsManaged)
        {
            if (e.Role.Tags.IsPremiumSubscriber)
                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: `{this.tKey.ServerBooster.Get(this.Bot.Guilds[e.Guild.Id])}`\n\n";

            if (e.Role.Tags.BotId is not null and not 0)
            {
                var bot = await sender.GetUserAsync((ulong)e.Role.Tags.BotId);

                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.RoleCreated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserAdded)
            .WithColor(EmbedColors.Success)
            .WithFooter($"{this.tKey.RoleId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Role.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Role.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Role.Mention} `{e.Role.Name}`\n" +
                             $"**{this.tKey.Color.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.Role.Color.ToHex()}`\n" +
                             $"**{this.tKey.RoleMentionable.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Role.IsMentionable.ToPillEmote(this.Bot)}\n" +
                             $"**{this.tKey.DisplayedRoleMembers.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Role.IsHoisted.ToPillEmote(this.Bot)}\n" +
                             $"{Integration}" +
                             $"\n**{this.tKey.Permissions.Get(this.Bot.Guilds[e.Guild.Id])}**: {GeneratePermissions}");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleCreate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task RoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
            return;

        var GeneratePermissions = string.Join(", ", e.Role.Permissions.GetEnumeration().Select(x => $"`{x.ToTranslatedPermissionString(this.Bot.Guilds[e.Guild.Id], this.Bot)}`"));
        var Integration = "";

        if (e.Role.IsManaged)
        {
            if (e.Role.Tags.IsPremiumSubscriber)
                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: `{this.tKey.ServerBooster.Get(this.Bot.Guilds[e.Guild.Id])}`\n\n";

            if (e.Role.Tags.BotId is not null and not 0)
            {
                var bot = await sender.GetUserAsync((ulong)e.Role.Tags.BotId);

                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.RoleDeleted.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserLeft)
            .WithColor(EmbedColors.Error)
            .WithFooter($"{this.tKey.RoleId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Role.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Role.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.Role.Name}`\n" +
                             $"**{this.tKey.Color.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.Role.Color.ToHex()}`\n" +
                             $"**{this.tKey.RoleMentionable.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Role.IsMentionable.ToPillEmote(this.Bot)}\n" +
                             $"**{this.tKey.DisplayedRoleMembers.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Role.IsHoisted.ToPillEmote(this.Bot)}\n" +
                             $"{(e.Role.IsManaged ? $"{this.tKey.RoleWasIntegration.Get(this.Bot.Guilds[e.Guild.Id]).Build(true)}\n" : "")}" +
                             $"{Integration}\n" +
                             $"\n**{this.tKey.Permissions.Get(this.Bot.Guilds[e.Guild.Id])}**: {GeneratePermissions}");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.Role.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task RoleModified(DiscordClient sender, GuildRoleUpdateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.RolesModified)
            return;

        var BeforePermissions = e.RoleBefore.Permissions.GetEnumeration();
        var AfterPermissions = e.RoleAfter.Permissions.GetEnumeration();

        var PermissionsAdded = false;
        var PermissionsRemoved = false;
        var PermissionDifference = "";

        foreach (var perm in AfterPermissions)
        {
            if (perm == Permissions.None)
                continue;

            if (!BeforePermissions.Contains(perm))
            {
                PermissionsAdded = true;
                PermissionDifference += $"`+` `{perm.ToTranslatedPermissionString(this.Bot.Guilds[e.Guild.Id], this.Bot)}`\n";
            }
        }

        foreach (var perm in BeforePermissions)
        {
            if (perm == Permissions.None)
                continue;

            if (!AfterPermissions.Contains(perm))
            {
                PermissionsRemoved = true;
                PermissionDifference += $"`-` `{perm.ToTranslatedPermissionString(this.Bot.Guilds[e.Guild.Id], this.Bot)}`\n";
            }
        }

        if (PermissionDifference.Length > 0)
            if (!PermissionsAdded && PermissionsRemoved)
                PermissionDifference = $"\n**{this.tKey.PermissionsRemoved.Get(this.Bot.Guilds[e.Guild.Id])}**:\n{PermissionDifference}";
            else PermissionDifference = PermissionsAdded && !PermissionsRemoved
                ? $"\n**{this.tKey.PermissionsAdded.Get(this.Bot.Guilds[e.Guild.Id])}**:\n{PermissionDifference}"
                : $"\n**{this.tKey.PermissionsUpdated.Get(this.Bot.Guilds[e.Guild.Id])}**:\n{PermissionDifference}";

        var Integration = "";

        if (e.RoleAfter.IsManaged)
        {
            if (e.RoleAfter.Tags.IsPremiumSubscriber)
                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: `{this.tKey.ServerBooster.Get(this.Bot.Guilds[e.Guild.Id])}`\n\n";

            if (e.RoleAfter.Tags.BotId is not null and not 0)
            {
                var bot = await sender.GetUserAsync((ulong)e.RoleAfter.Tags.BotId);

                Integration = $"**{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}**: {bot.Mention} `{bot.GetUsernameWithIdentifier()}`\n\n";
            }
        }

        if (e.RoleBefore.Name == e.RoleAfter.Name && 
            e.RoleBefore.Color.ToHex() == e.RoleAfter.Color.ToHex() &&
            e.RoleBefore.IsMentionable == e.RoleAfter.IsMentionable &&
            e.RoleBefore.IsHoisted == e.RoleAfter.IsHoisted &&
            PermissionDifference.IsNullOrWhiteSpace())
                return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.RoleUpdated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserUpdated)
            .WithColor(EmbedColors.Warning)
            .WithFooter($"{this.tKey.RoleId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.RoleAfter.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Role.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.RoleAfter.Mention} {(e.RoleBefore.Name != e.RoleAfter.Name ? $"`{e.RoleBefore.Name}` ➡ `{e.RoleAfter.Name}`" : $"`{e.RoleAfter.Name}`")}\n" +
                      $"{(e.RoleBefore.Color.ToHex() != e.RoleAfter.Color.ToHex() ? $"**{this.tKey.Color.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.RoleBefore.Color.ToHex()}` ➡ `{e.RoleAfter.Color.ToHex()}`\n" : "")}" +
                      $"{(e.RoleBefore.IsMentionable != e.RoleAfter.IsMentionable ? $"**{this.tKey.RoleMentionable.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.RoleBefore.IsMentionable.ToPillEmote(this.Bot)} ➡ {e.RoleAfter.IsMentionable.ToPillEmote(this.Bot)}\n" : "")}" +
                      $"{(e.RoleBefore.IsHoisted != e.RoleAfter.IsHoisted ? $"**{this.tKey.DisplayedRoleMembers.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.RoleBefore.IsHoisted.ToPillEmote(this.Bot)} ➡ {e.RoleAfter.IsHoisted.ToPillEmote(this.Bot)}\n" : "")}" +
                      $"{(e.RoleAfter.IsManaged ? $"\n`{this.tKey.Integration.Get(this.Bot.Guilds[e.Guild.Id])}`\n" : "")}" +
                      $"{Integration}" +
                      $"{PermissionDifference}");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.RoleUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.RoleAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogRoleUpdateEntry)AuditLogEntries.First(x => ((DiscordAuditLogRoleUpdateEntry)x).Target.Id == e.RoleAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task BanAdded(DiscordClient sender, GuildBanAddEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.BanlistModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.UserBanned.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserBanned)
            .WithColor(EmbedColors.Error)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`\n" +
                             $"**{this.tKey.JoinedAt.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.JoinedAt.ToTimestamp()} ({e.Member.JoinedAt.ToTimestamp(TimestampFormat.LongDateTime)})")
            .AddField(new DiscordEmbedField(this.tKey.Roles.Get(this.Bot.Guilds[e.Guild.Id]), $"{string.Join(", ", e.Member.Roles.Select(x => x.Mention))}".TruncateWithIndication(1000)));

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Ban);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogBanEntry)AuditLogEntries.First(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.BannedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                if (!string.IsNullOrWhiteSpace(Entry.Reason))
                    embed.Description += $"\n**{this.tKey.Reason.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.Reason.SanitizeForCode()}";

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.BannedBy.Get(this.Bot.Guilds[e.Guild.Id])}' & '{this.tKey.Reason.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task BanRemoved(DiscordClient sender, GuildBanRemoveEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.BanlistModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.UserUnbanned.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.UserBanRemoved)
            .WithColor(EmbedColors.Success)
            .WithFooter($"{this.tKey.UserId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Member.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.Member.AvatarUrl)
            .WithDescription($"**{this.tKey.User.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Member.Mention} `{e.Member.GetUsernameWithIdentifier()}`");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.Unban);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogBanEntry)AuditLogEntries.First(x => ((DiscordAuditLogBanEntry)x).Target.Id == e.Member.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.UnbannedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.UnbannedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task GuildUpdated(DiscordClient sender, GuildUpdateEventArgs e)
    {
        if (!await this.ValidateServer(e.GuildAfter) || !this.Bot.Guilds[e.GuildAfter.Id].ActionLog.GuildModified)
            return;

        var Description = "";

        try
        { Description += $"{(e.GuildBefore.Owner.Id != e.GuildAfter.Owner.Id ? $"**{this.tKey.Owner.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.Owner.Mention} `{e.GuildBefore.Owner.GetUsernameWithIdentifier()}` ➡ {e.GuildAfter.Owner.Mention} `{e.GuildAfter.Owner.GetUsernameWithIdentifier()}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.Name != e.GuildAfter.Name ? $"**{this.tKey.Name.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.Name}` ➡ `{e.GuildAfter.Name}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.Description != e.GuildAfter.Description ? $"**{this.tKey.Description.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.Description}` ➡ `{e.GuildAfter.Description}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.PreferredLocale != e.GuildAfter.PreferredLocale ? $"**{this.tKey.PreferredLocale.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.PreferredLocale}` ➡ `{e.GuildAfter.PreferredLocale}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.VanityUrlCode != e.GuildAfter.VanityUrlCode ? $"**{this.tKey.VanityUrl.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.VanityUrlCode}` ➡ `{e.GuildAfter.VanityUrlCode}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IconHash != e.GuildAfter.IconHash ? $"`{this.tKey.IconUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id])}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.DefaultMessageNotifications != e.GuildAfter.DefaultMessageNotifications ? $"**{this.tKey.DefaultNotificationSettings.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.DefaultMessageNotifications}` ➡ `{e.GuildAfter.DefaultMessageNotifications}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.VerificationLevel != e.GuildAfter.VerificationLevel ? $"**{this.tKey.VerificationLevel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.VerificationLevel}` ➡ `{e.GuildAfter.VerificationLevel}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.BannerHash != e.GuildAfter.BannerHash ? $"`{this.tKey.BannerUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id])}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.SplashHash != e.GuildAfter.SplashHash ? $"`{this.tKey.SplashUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id])}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.HomeHeaderHash != e.GuildAfter.HomeHeaderHash ? $"`{this.tKey.HomeHeaderUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id])}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.DiscoverySplashHash != e.GuildAfter.DiscoverySplashHash ? $"`{this.tKey.DiscoverySplashUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id])}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.MfaLevel != e.GuildAfter.MfaLevel ? $"**{this.tKey.RequiredMfaLevel}**: {(e.GuildBefore.MfaLevel == MfaLevel.Enabled).ToPillEmote(this.Bot)} ➡ {(e.GuildAfter.MfaLevel == MfaLevel.Enabled).ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.ExplicitContentFilter != e.GuildAfter.ExplicitContentFilter ? $"**{this.tKey.ExplicitContentFilter.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.ExplicitContentFilter}` ➡ `{e.GuildAfter.ExplicitContentFilter}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.WidgetEnabled != e.GuildAfter.WidgetEnabled ? $"**{this.tKey.GuildWidgetEnabled.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {(e.GuildBefore.WidgetEnabled ?? false).ToPillEmote(this.Bot)} ➡ {(e.GuildAfter.WidgetEnabled ?? false).ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.WidgetChannel?.Id != e.GuildAfter.WidgetChannel?.Id ? $"**{this.tKey.GuildWidgetChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.WidgetChannel.Mention} `[{e.GuildBefore.WidgetChannel.GetIcon()}{e.GuildBefore.WidgetChannel.Name}]` ➡ {e.GuildAfter.WidgetChannel.Mention} `[{e.GuildAfter.WidgetChannel.GetIcon()}{e.GuildAfter.WidgetChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsLarge != e.GuildAfter.IsLarge ? $"**{this.tKey.LargeGuild.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.IsLarge.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.IsLarge.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsNsfw != e.GuildAfter.IsNsfw ? $"**{this.tKey.NsfwGuild.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.IsNsfw.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.IsNsfw.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.IsCommunity != e.GuildAfter.IsCommunity ? $"**{this.tKey.CommunityGuild.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.IsCommunity.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.IsCommunity.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.HasMemberVerificationGate != e.GuildAfter.HasMemberVerificationGate ? $"**{this.tKey.MembershipScreening.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.HasMemberVerificationGate.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.HasMemberVerificationGate.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.HasWelcomeScreen != e.GuildAfter.HasWelcomeScreen ? $"**{this.tKey.WelcomeScreen.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.HasWelcomeScreen.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.HasWelcomeScreen.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.PremiumProgressBarEnabled != e.GuildAfter.PremiumProgressBarEnabled ? $"**{this.tKey.BoostProgressBar.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.PremiumProgressBarEnabled.ToPillEmote(this.Bot)} ➡ {e.GuildAfter.PremiumProgressBarEnabled.ToPillEmote(this.Bot)}\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.RulesChannel?.Id != e.GuildAfter.RulesChannel?.Id ? $"**{this.tKey.RuleChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.RulesChannel.Mention} `[{e.GuildBefore.RulesChannel.GetIcon()}{e.GuildBefore.RulesChannel.Name}]` ➡ {e.GuildAfter.RulesChannel.Mention} `[{e.GuildAfter.RulesChannel.GetIcon()}{e.GuildAfter.RulesChannel.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.AfkTimeout != e.GuildAfter.AfkTimeout ? $"**{this.tKey.AfkTimeout.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{TimeSpan.FromSeconds(e.GuildBefore.AfkTimeout).GetHumanReadable(config: TranslationUtil.GetTranslatedHumanReadableConfig(this.Bot.Guilds[e.GuildAfter.Id], this.Bot))}` ➡ `{TimeSpan.FromSeconds(e.GuildAfter.AfkTimeout).GetHumanReadable(config: TranslationUtil.GetTranslatedHumanReadableConfig(this.Bot.Guilds[e.GuildAfter.Id], this.Bot))}`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.AfkChannel?.Id != e.GuildAfter.AfkChannel?.Id ? $"**{this.tKey.AfkChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.AfkChannel?.Mention} `[{e.GuildBefore.AfkChannel?.GetIcon()}{e.GuildBefore.AfkChannel?.Name}]` ➡ {e.GuildAfter.AfkChannel?.Mention} `[{e.GuildAfter.AfkChannel?.GetIcon()}{e.GuildAfter.AfkChannel?.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.SystemChannel?.Id != e.GuildAfter.SystemChannel?.Id ? $"**{this.tKey.SystemChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.SystemChannel?.Mention} `[{e.GuildBefore.SystemChannel?.GetIcon()}{e.GuildBefore.SystemChannel?.Name}]` ➡ {e.GuildAfter.SystemChannel?.Mention} `[{e.GuildAfter.SystemChannel?.GetIcon()}{e.GuildAfter.SystemChannel?.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.PublicUpdatesChannel?.Id != e.GuildAfter.PublicUpdatesChannel?.Id ? $"**{this.tKey.DiscordUpdateChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.PublicUpdatesChannel?.Mention} `[{e.GuildBefore.PublicUpdatesChannel?.GetIcon()}{e.GuildBefore.PublicUpdatesChannel?.Name}]` ➡ {e.GuildAfter.PublicUpdatesChannel?.Mention} `[{e.GuildAfter.PublicUpdatesChannel?.GetIcon()}{e.GuildAfter.PublicUpdatesChannel?.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.SafetyAltersChannel?.Id != e.GuildAfter.SafetyAltersChannel?.Id ? $"**{this.tKey.SafetyAlertsChannel.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {e.GuildBefore.SafetyAltersChannel?.Mention} `[{e.GuildBefore.SafetyAltersChannel?.GetIcon()}{e.GuildBefore.SafetyAltersChannel?.Name}]` ➡ {e.GuildAfter.SafetyAltersChannel?.Mention} `[{e.GuildAfter.SafetyAltersChannel?.GetIcon()}{e.GuildAfter.SafetyAltersChannel?.Name}]`\n" : "")}"; }
        catch { }
        try
        { Description += $"{(e.GuildBefore.MaxMembers != e.GuildAfter.MaxMembers ? $"**{this.tKey.MaximumMembers.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: `{e.GuildBefore.MaxMembers}` ➡ `{e.GuildAfter.MaxMembers}`\n" : "")}"; }
        catch { }

        if (Description.Length == 0)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.GuildUpdated.Get(this.Bot.Guilds[e.GuildAfter.Id]), null, AuditLogIcons.GuildUpdated)
            .WithColor(EmbedColors.Warning)
            .WithTimestamp(DateTime.UtcNow)
            .WithThumbnail(e.GuildAfter.IconUrl)
            .WithDescription(Description);

        if (e.GuildBefore.IconHash != e.GuildAfter.IconHash)
            embed.ImageUrl = e.GuildAfter.IconUrl;

        var msg = await this.SendActionlog(e.GuildAfter, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.GuildAfter.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.GuildAfter.GetAuditLogsAsync(actionType: AuditLogActionType.GuildUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => (!this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id))))
            {
                var Entry = (DiscordAuditLogGuildEntry)AuditLogEntries.First(x => !this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.GuildAfter.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.GuildAfter.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";

                embed.Footer = new();
                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.GuildAfter.Id]).Build(new TVar("Fields", $"'{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.GuildAfter.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.ChannelCreated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.ChannelAdded)
            .WithColor(EmbedColors.Success)
            .WithFooter($"{this.tKey.ChannelId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Channel.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Name.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelCreate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.ChannelDeleted.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.ChannelRemoved)
            .WithColor(EmbedColors.Error)
            .WithFooter($"{this.tKey.ChannelId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.Channel.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Name.Get(this.Bot.Guilds[e.Guild.Id])}**: `[{e.Channel.GetIcon()}{e.Channel.Name}]`");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.Channel.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.ChannelsModified)
            return;

        if (e.ChannelBefore?.Name == e.ChannelAfter?.Name && e.ChannelBefore?.IsNsfw == e.ChannelAfter?.IsNsfw)
            return;

        var Description = $"{(e.ChannelBefore.Name != e.ChannelAfter.Name ? $"**{this.tKey.Name.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.ChannelBefore.Mention} `[{e.ChannelBefore.GetIcon()}{e.ChannelBefore.Name}]` ➡ `[{e.ChannelAfter.GetIcon()}{e.ChannelAfter.Name}]`\n" : $"{e.ChannelAfter.Mention} `[{e.ChannelAfter}{e.ChannelAfter.Name}]`\n")}" +
                             $"{(e.ChannelBefore.IsNsfw != e.ChannelAfter.IsNsfw ? $"**{this.tKey.NsfwChannel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.ChannelBefore.IsNsfw.ToPillEmote(this.Bot)} ➡ {e.ChannelAfter.IsNsfw.ToPillEmote(this.Bot)}\n" : "")}" +
                             $"{(e.ChannelBefore.DefaultAutoArchiveDuration != e.ChannelAfter.DefaultAutoArchiveDuration ? $"**{this.tKey.DefaultAutoArchiveDuration.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.ChannelBefore.DefaultAutoArchiveDuration}` ➡ `{e.ChannelAfter.DefaultAutoArchiveDuration}`\n" : "")}" +
                             $"{(e.ChannelBefore.Bitrate != e.ChannelAfter.Bitrate ? $"**{this.tKey.Bitrate.Get(this.Bot.Guilds[e.Guild.Id])}**: `{e.ChannelBefore.Bitrate?.FileSizeToHumanReadable()}` ➡ `{e.ChannelAfter.Bitrate?.FileSizeToHumanReadable()}`\n" : "")}";

        if (Description.Length == 0)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.ChannelModified.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.ChannelModified)
            .WithColor(EmbedColors.Warning)
            .WithFooter($"{this.tKey.ChannelId.Get(this.Bot.Guilds[e.Guild.Id])}: {e.ChannelAfter.Id}")
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription(Description);

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));

        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.ChannelUpdate);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.ChannelAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogChannelEntry)AuditLogEntries.First(x => ((DiscordAuditLogChannelEntry)x).Target.Id == e.ChannelAfter.Id && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.ModifiedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.InvitesModified)
            return;

        _ = this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.InviteCreated.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.InviteAdded)
            .WithColor(EmbedColors.Success)
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Invite.Get(this.Bot.Guilds[e.Guild.Id])}**: `https://discord.gg/{e.Invite.Code}`\n" +
                             $"**{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Invite.Inviter?.Mention ?? this.tKey.NoInviter.Get(this.Bot.Guilds[e.Guild.Id]).Build(true)} `{e.Invite.Inviter?.GetUsernameWithIdentifier() ?? "-"}`\n" +
                             $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`")));
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        if (!await this.ValidateServer(e.Guild) || !this.Bot.Guilds[e.Guild.Id].ActionLog.InvitesModified)
            return;

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(this.tKey.InviteDeleted.Get(this.Bot.Guilds[e.Guild.Id]), null, AuditLogIcons.InviteRemoved)
            .WithColor(EmbedColors.Error)
            .WithTimestamp(DateTime.UtcNow)
            .WithDescription($"**{this.tKey.Invite.Get(this.Bot.Guilds[e.Guild.Id])}**: `https://discord.gg/{e.Invite.Code}`\n" +
                             $"**{this.tKey.CreatedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Invite.Inviter?.Mention ?? this.tKey.NoInviter.Get(this.Bot.Guilds[e.Guild.Id]).Build(true)} `{e.Invite.Inviter?.GetUsernameWithIdentifier() ?? "-"}`\n" +
                             $"**{this.tKey.Channel.Get(this.Bot.Guilds[e.Guild.Id])}**: {e.Channel.Mention} `[{e.Channel.GetIcon()}{e.Channel.Name}]`");

        var msg = await this.SendActionlog(e.Guild, new DiscordMessageBuilder().WithEmbed(embed));


        if (!this.Bot.Guilds[e.Guild.Id].ActionLog.AttemptGettingMoreDetails)
            return;

        for (var i = 0; i < 3; i++)
        {
            var AuditLogEntries = await e.Guild.GetAuditLogsAsync(actionType: AuditLogActionType.InviteDelete);

            if (AuditLogEntries.Count > 0 && AuditLogEntries.Any(x => ((DiscordAuditLogInviteEntry)x).Target.Code == e.Invite.Code && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id)))
            {
                var Entry = (DiscordAuditLogInviteEntry)AuditLogEntries.First(x => ((DiscordAuditLogInviteEntry)x).Target.Code == e.Invite.Code && !this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Contains(x.Id));
                this.Bot.Guilds[e.Guild.Id].ActionLog.ProcessedAuditLogs.Add(Entry.Id);

                embed.Description += $"\n\n**{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}**: {Entry.UserResponsible.Mention} `{Entry.UserResponsible.GetUsernameWithIdentifier()}`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Entry.UserResponsible.AvatarUrl };

                embed.Footer = new();
                embed.Footer.Text += $"\n({this.tKey.FooterAuditLogDisclaimer.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Fields", $"'{this.tKey.DeletedBy.Get(this.Bot.Guilds[e.Guild.Id])}'"))})";

                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                break;
            }

            await Task.Delay(5000);
        }
    }
}
