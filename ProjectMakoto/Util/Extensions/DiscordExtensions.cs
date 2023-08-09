// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Web;
using Newtonsoft.Json.Serialization;

namespace ProjectMakoto.Util;

internal static class DiscordExtensions
{
    private static string? LoadedHtml = null;

    public static string GenerateHtmlFromMessages(this IEnumerable<DiscordMessage> messages, Bot bot)
    {
        var sanitizer = new Ganss.Xss.HtmlSanitizer(new()
        {
            AllowedSchemes = new SortedSet<string> { "http", "https" },
        });

        string Sanitize(string? str)
        {
            if (str is null)
                return null;

            return sanitizer.Sanitize(str.Replace("<", "&lt;").Replace(">", "&gt;"));
        }

        LoadedHtml ??= File.ReadAllText("Assets/DiscordMessages.html");

        var currentFieldIndex = 0;
        int GetFieldIndex(bool inline)
        {
            if (!inline)
                return 1;

            currentFieldIndex++;
            return currentFieldIndex;
        }

        var messageStrings = messages.OrderBy(x => x.Id.GetSnowflakeTime().Ticks).Select(msg =>
        {
            var messageBuilder = 
            $"<discord-message author=\"{Sanitize(msg.Author?.GetUsername() ?? "Unknown User")} ({msg.Author?.Id})\" " +
            $"avatar=\"{msg.Author?.AvatarUrl}\" timestamp=\"{msg.Id.GetSnowflakeTime().ToUnixTimeSeconds()}\" " +
            $"twenty-four=\"true\" " +
            $"bot={((msg.Author?.IsBot ?? false) && !msg.WebhookMessage).ToString().ToLower()} " +
            $"verified={((msg.Author?.IsVerifiedBot ?? false) && !msg.WebhookMessage).ToString().ToLower()} " +
            $"webhook={msg.WebhookMessage.ToString().ToLower()} " +
            $"edited={msg.IsEdited.ToString().ToLower()}>" +
            $"{(msg.ReferencedMessage is not null ? 
                $"<discord-reply slot=\"reply\"" +
                $" author=\"{Sanitize(msg.ReferencedMessage.Author?.GetUsername() ?? "Unknown User")} ({msg.Author?.Id})\"" +
                $" avatar=\"{msg.ReferencedMessage.Author?.AvatarUrl}\">{Sanitize(msg.ReferencedMessage.Content.TruncateWithIndication(100))
                    .ConvertMarkdownToHtml(bot)}" +
                $"</discord-reply>" : "")}" +
            $"{Sanitize(msg.Content).ConvertMarkdownToHtml(bot)}" +
            $"{string.Join("", msg.Embeds?.Select(embed =>
            {
                currentFieldIndex = 0;

                string? videoId = null;

                if (embed.Provider?.Name == "YouTube")
                {
                    try
                    {
                        videoId = RegexTemplates.YouTubeUrl.Match(msg.Content).Groups[5].Value;
                    }
                    catch {}
                }

                if (msg.Flags?.HasMessageFlag(MessageFlags.SuppressedEmbeds) ?? false)
                    return "";

                return $"<discord-embed " +
                $"slot=\"embeds\" " +
                $"provider=\"{Sanitize(embed.Provider?.Name)}\" " +
                $"provider-url=\"{Sanitize(embed.Provider?.Url?.ToString())}\" " +
                $"author-image=\"{Sanitize(embed.Author?.IconUrl?.ToString())}\" " +
                $"author-name=\"{Sanitize(embed.Author?.Name)}\" " +
                $"author-url=\"{Sanitize(embed.Author?.Url?.ToString())}\" " +
                $"embed-title=\"{Sanitize(embed.Title.ConvertMarkdownToHtml(bot))}\" " +
                $"url=\"{Sanitize(embed.Url?.ToString())}\" " +
                $"image=\"{Sanitize(embed.Image?.Url?.ToString())}\" " +
                $"thumbnail=\"{Sanitize(videoId is null ? embed.Thumbnail?.Url?.ToString() : "")}\" " +
                $"video=\"{Sanitize(videoId ?? embed.Video?.Url?.ToString())}\" " +
                $"color=\"{(embed.Color.HasValue ? embed.Color.Value.ToHex() : "")}\">" +
                $"{((embed.Description?.Length > 0 && videoId is null) ? $"<discord-embed-description slot=\"description\">{Sanitize(embed.Description).ConvertMarkdownToHtml(bot)}</discord-embed-description>" : "")}" +
                $"{(embed.Fields?.Count > 0 ? $"<discord-embed-fields slot=\"fields\">{string.Join("", embed.Fields.Select(field =>
                    {
                        if (!field.Inline)
                            currentFieldIndex = 0;

                        return $"<discord-embed-field " +
                        $"field-title=\"{Sanitize(field.Name)}\" " +
                        $"inline=\"{field.Inline.ToString().ToLower()}\" " +
                        $"inline-index=\"{GetFieldIndex(field.Inline)}\" " +
                        $">{Sanitize(field.Value).ConvertMarkdownToHtml(bot)}</discord-embed-field>";
                    }))}</discord-embed-fields>" : "")}" +
                $"{(embed.Footer is not null ? $"<discord-embed-footer " +
                    $"slot=\"footer\" " +
                    $"footer-image=\"{Sanitize(embed.Footer?.IconUrl?.ToString())}\" " +
                    $"timestamp=\"{embed.Timestamp?.ToUnixTimeSeconds()}\" " +
                    $"twenty-four=\"true\">" +
                    $"{Sanitize(embed.Footer?.Text).ConvertMarkdownToHtml(bot)}</discord-embed-footer>" : "")}" +
                $"</discord-embed>";
            }))}" +
            $"{string.Join("", msg.Attachments?.Select(x =>
            {
                var tempUrl = x.Url.TruncateAt(true, '?');
                var type = string.Empty;
                var alt = x.Description;

                if (x.Url.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".webp", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".gifv", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
                    type = "image";
                else if (x.Url.EndsWith(".webm", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                    type = "video";
                else if (x.Url.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase) ||
                    x.Url.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    type = "audio";
                else
                {
                    type = "file";
                    alt = tempUrl[(tempUrl.LastIndexOf('/') + 1)..];
                }

                return $"<discord-attachment slot=\"attachments\" " +
                $"type=\"{type}\" " +
                $"url=\"{Sanitize(x.Url)}\" " +
                $"alt=\"{Sanitize(alt)}\" " +
                $"size=\"{x.FileSize.Value.FileSizeToHumanReadable()}\" " +
                $"height=\"{x.Height}\" " +
                $"width=\"{x.Width}\"/>";
            }))}" +
            $"{string.Join("", msg.Stickers?.Select(x =>
            {
                var tempUrl = x.Url.TruncateAt(true, '?');
                var type = "image";
                var alt = x.Description;

                return $"<discord-attachment slot=\"attachments\" " +
                $"type=\"{type}\" " +
                $"url=\"{Sanitize(x.Url)}\" " +
                $"alt=\"{Sanitize(alt)}\" " +
                $"height=\"160\" " +
                $"width=\"160\"/>";
            }))}" +
            $"</discord-message>";

            return messageBuilder;
        }).ToArray();

        if (!messageStrings.Any())
            return string.Empty;

        return LoadedHtml
            .Replace("<--! RawMessages -->", Uri.EscapeDataString(JsonConvert.SerializeObject(messages.OrderBy(x => x.Id.GetSnowflakeTime().Ticks), new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (serializer, err) =>
                    {
                        _logger.LogError("Failed to serialize member '{member}' at '{path}'", err.ErrorContext.Error, err.ErrorContext.Member, err.ErrorContext.Path);
                        err.ErrorContext.Handled = true;
                    },
                })))
            .Replace("<--! RawMessageFileName -->", $"{Guid.NewGuid()}.txt")
            .Replace("<!-- Title -->", "Chat History")
            .Replace("<!-- MessageCount -->", messages.Count())
            .Replace("<!-- Channel -->", $"{messages.First().Channel.GetIcon()}{Sanitize(messages.First().Channel.Name)} (<i>{messages.First().Channel.Id}</i>)")
            .Replace("<!-- Guild -->", $"{Sanitize(messages.First().Channel.Guild.Name)} (<i>{messages.First().Channel.Guild.Id}</i>)")
            .Replace("<!-- GenerationTime -->", DateTime.UtcNow.ToString())
            .Replace("<!-- Bot -->", bot.DiscordClient.CurrentUser.GetUsernameWithIdentifier())
            .Replace("<!-- Messages -->", string.Join("\n", messageStrings));
    }

    internal static string ConvertMarkdownToHtml(this string? md, Bot bot)
    {
        if (md.IsNullOrWhiteSpace())
            return md;

        md = Regex.Replace(md, @"(?<!\\)\`([^\n`]+?)\`", (e) =>
        {
            return $"<discord-inline-code>{e.Groups[1].Value.Replace(" ", "&nbsp;")}</discord-inline-code>";
        }, RegexOptions.Compiled);
        
        md = Regex.Replace(md, @"(?<!\\)(?:\`\`\`)(?:(\w{2,15})\n)?((?:.|\n)+?)(?:\`\`\`)", (e) =>
        {
            var lang = "";

            if (e.Groups[1].Success)
                lang = e.Groups[1].Value;

            return $"<discord-code-block language=\"{lang}\"><pre>{e.Groups[2].Value.Replace(" ", "&nbsp;")}</pre></discord-code-block>";
        }, RegexOptions.Compiled | RegexOptions.Multiline);

        md = Regex.Replace(md, @"(?<!\\)\*\*([^\n*]+?)(?<!\\)\*\*", (e) =>
        {
            return $"<discord-bold>{e.Groups[1].Value}</discord-bold>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)\~\~([^\n~]+?)(?<!\\)\~\~", (e) =>
        {
            return $"<s>{e.Groups[1].Value}</s>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)__([^\n~]+?)(?<!\\)__", (e) =>
        {
            return $"<u>{e.Groups[1].Value}</u>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)\|\|([^\n|]+?)(?<!\\)\|\|", (e) =>
        {
            return $"<discord-spoiler>{e.Groups[1].Value}</discord-spoiler>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)\*([^\n*]+?)(?<!\\)\*", (e) =>
        {
            return $"<discord-italic>{e.Groups[1].Value}</discord-italic>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<![\\_])_([^\n_]+?)(?<!\\)_", (e) =>
        {
            return $"<discord-italic>{e.Groups[1].Value}</discord-italic>";
        }, RegexOptions.Compiled);
        
        md = Regex.Replace(md, @"^(?<!\\)&gt; ([^\n_]+?)", (e) =>
        {
            return $"<discord-quote>{e.Groups[1].Value}</discord-quote>";
        }, RegexOptions.Compiled | RegexOptions.Multiline);

        md = Regex.Replace(md, @"(?<!\\)&lt;t:(\d+?)(:(\w))?&gt;", (e) =>
        {
            return $"<discord-time format=\"{e.Groups[3].Value}\" timestamp=\"{e.Groups[1].Value}\"></discord-time>";
        }, RegexOptions.Compiled);
        
        md = Regex.Replace(md, @"(?<!\\)&lt;/([\w -]+?):(?:\d+?)&gt;", (e) =>
        {
            return $"<discord-mention type=\"slash\">{e.Groups[1].Value}</discord-mention>";
        }, RegexOptions.Compiled);
        
        md = Regex.Replace(md, @"(&lt;)?((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/⁄~\+#]*[\w\-\@?^=%&amp;\/⁄~\+#])?)", (e) =>
        {
            var url = e.Groups[2].Value;

            if ((e.Groups[1]?.Success ?? false) && url.Contains("&gt;"))
                url = url[..url.IndexOf("&gt;")];

            return $"<a target=\"_blank\" href=\"{url}\">{url}</a>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)&lt;@(?:!)?(\d+?)&gt;", (e) =>
        {
            try
            {
                return $"<discord-mention>{bot.DiscordClient!.GetUserAsync(e.Groups[1].Value.ToUInt64()).GetAwaiter().GetResult().GetUsername()}</discord-mention>";
            }
            catch (Exception)
            {
                return $"@{e.Groups[1].Value}";
            }
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)&lt;#(?:!)?(\d+?)&gt;", (e) =>
        {
            try
            {
                var channel = bot.DiscordClient!.GetChannelAsync(e.Groups[1].Value.ToUInt64()).GetAwaiter().GetResult();
                var type = channel.Type switch
                {
                    ChannelType.Voice => "voice",
                    ChannelType.Stage => "voice",
                    ChannelType.Forum => "forum",
                    ChannelType.GuildMedia => "forum",
                    ChannelType.PublicThread => "thread",
                    ChannelType.PrivateThread => "thread",
                    ChannelType.NewsThread => "thread",
                    _ => "channel"
                };


                return $"<discord-mention type=\"{type}\">{channel.Name}</discord-mention>";
            }
            catch (Exception)
            {
                return $"@{e.Groups[1].Value}";
            }
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)&lt;(a)?:(\w+?):(\d+?)&gt;", (e) =>
        {
            var url = $"https://cdn.discordapp.com/emojis/{e.Groups[3].Value}.{(e.Groups[1].Success ? "gif" : "png")}";

            return $"<discord-custom-emoji name=\"{e.Groups[2].Value}\" url=\"{url}\"></discord-custom-emoji>";
        }, RegexOptions.Compiled);

        md = Regex.Replace(md, @"(?<!\\)(?<!\&gt;)(?<!a):\w+?:", (e) =>
        {
            if (!DiscordEmoji.TryFromName(bot.DiscordClient, e.Value, false, out var emoji))
                return e.Value;
            else
                try
                {
                    return $"{emoji.UnicodeEmoji}";
                }
                catch (Exception)
                {
                    return e.Value;
                }
        }, RegexOptions.Compiled);

        md = md.Replace("\\*", "*");
        md = md.Replace("\\_", "_");
        md = md.Replace("\\&gt;", "&gt;");
        md = md.Replace("\\&lt;", "&lt;");
        md = md.Replace("\\~", "~");
        md = md.Replace("\\`", "`");
        md = md.Replace("\\|", "|");

        return md; // .ReplaceLineEndings("<br />")
    }

    internal static Permissions[] GetEnumeration(this Permissions perms)
        => Enum.GetValues(perms.GetType()).Cast<Enum>().Where(x => perms.HasFlag(x)).Select(x => (Permissions)x.ToInt64()).ToArray();

    internal static Guild GetDbEntry(this DiscordGuild guild, Bot bot)
        => bot.Guilds[guild.Id];

    internal static User GetDbEntry(this DiscordUser user, Bot bot)
        => bot.Users[user.Id];

    internal static bool HasAnyPermission(this Permissions permissions, params Permissions[] list)
        => list.Any(x => permissions.HasPermission(x));

    internal static string GetGuildPrefix(this DiscordGuild guild, Bot bot)
    {
        try
        {
            return bot?.Guilds[guild?.Id ?? 0]?.PrefixSettings?.Prefix?.IsNullOrWhiteSpace() ?? true ? ";;" : bot.Guilds[guild.Id].PrefixSettings.Prefix;
        }
        catch (Exception)
        {
            return ";;";
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    internal static string GetUsername(this DiscordUser user)
        => user.IsMigrated ? user.GlobalName : user.Username;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0102:[Discord] Deprecated", Justification = "<Pending>")]
    internal static string GetUsernameWithIdentifier(this DiscordUser user)
        => user.IsMigrated ? $"{user.GlobalName} ({user.Username})" : user.UsernameWithDiscriminator;

    internal static string ToTranslatedPermissionString(this Permissions perm, Guild guild, Bot _bot)
        => GetTranslationObject(perm, _bot) == _bot.LoadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(CustomLogLevel.Warn, "Missing Translation") : GetTranslationObject(perm, _bot).Get(guild);

    internal static string ToTranslatedPermissionString(this Permissions perm, DiscordGuild guild, Bot _bot)
        => GetTranslationObject(perm, _bot) == _bot.LoadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(CustomLogLevel.Warn, "Missing Translation") : GetTranslationObject(perm, _bot).Get(guild);

    internal static string ToTranslatedPermissionString(this Permissions perm, User user, Bot _bot)
        => GetTranslationObject(perm, _bot) == _bot.LoadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(CustomLogLevel.Warn, "Missing Translation") : GetTranslationObject(perm, _bot).Get(user);

    internal static string ToTranslatedPermissionString(this Permissions perm, DiscordUser user, Bot _bot)
        => GetTranslationObject(perm, _bot) == _bot.LoadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(CustomLogLevel.Warn, "Missing Translation") : GetTranslationObject(perm, _bot).Get(user);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0103:[Discord] Unreleased", Justification = "<Pending>")]
    private static SingleTranslationKey GetTranslationObject(Permissions perm, Bot _bot)
        => perm switch
        {
            Permissions.None => _bot.LoadedTranslations.Common.Permissions.None,
            Permissions.All => _bot.LoadedTranslations.Common.Permissions.All,
            Permissions.CreateInstantInvite => _bot.LoadedTranslations.Common.Permissions.CreateInstantInvite,
            Permissions.KickMembers => _bot.LoadedTranslations.Common.Permissions.KickMembers,
            Permissions.BanMembers => _bot.LoadedTranslations.Common.Permissions.BanMembers,
            Permissions.Administrator => _bot.LoadedTranslations.Common.Permissions.Administrator,
            Permissions.ManageChannels => _bot.LoadedTranslations.Common.Permissions.ManageChannels,
            Permissions.ManageGuild => _bot.LoadedTranslations.Common.Permissions.ManageGuild,
            Permissions.AddReactions => _bot.LoadedTranslations.Common.Permissions.AddReactions,
            Permissions.ViewAuditLog => _bot.LoadedTranslations.Common.Permissions.ViewAuditLog,
            Permissions.PrioritySpeaker => _bot.LoadedTranslations.Common.Permissions.PrioritySpeaker,
            Permissions.Stream => _bot.LoadedTranslations.Common.Permissions.Stream,
            Permissions.AccessChannels => _bot.LoadedTranslations.Common.Permissions.AccessChannels,
            Permissions.SendMessages => _bot.LoadedTranslations.Common.Permissions.SendMessages,
            Permissions.SendTtsMessages => _bot.LoadedTranslations.Common.Permissions.SendTtsMessages,
            Permissions.ManageMessages => _bot.LoadedTranslations.Common.Permissions.ManageMessages,
            Permissions.EmbedLinks => _bot.LoadedTranslations.Common.Permissions.EmbedLinks,
            Permissions.AttachFiles => _bot.LoadedTranslations.Common.Permissions.AttachFiles,
            Permissions.ReadMessageHistory => _bot.LoadedTranslations.Common.Permissions.ReadMessageHistory,
            Permissions.MentionEveryone => _bot.LoadedTranslations.Common.Permissions.MentionEveryone,
            Permissions.UseExternalEmojis => _bot.LoadedTranslations.Common.Permissions.UseExternalEmojis,
            Permissions.ViewGuildInsights => _bot.LoadedTranslations.Common.Permissions.ViewGuildInsights,
            Permissions.UseVoice => _bot.LoadedTranslations.Common.Permissions.UseVoice,
            Permissions.Speak => _bot.LoadedTranslations.Common.Permissions.Speak,
            Permissions.MuteMembers => _bot.LoadedTranslations.Common.Permissions.MuteMembers,
            Permissions.DeafenMembers => _bot.LoadedTranslations.Common.Permissions.DeafenMembers,
            Permissions.MoveMembers => _bot.LoadedTranslations.Common.Permissions.MoveMembers,
            Permissions.UseVoiceDetection => _bot.LoadedTranslations.Common.Permissions.UseVoiceDetection,
            Permissions.ChangeNickname => _bot.LoadedTranslations.Common.Permissions.ChangeNickname,
            Permissions.ManageNicknames => _bot.LoadedTranslations.Common.Permissions.ManageNicknames,
            Permissions.ManageRoles => _bot.LoadedTranslations.Common.Permissions.ManageRoles,
            Permissions.ManageWebhooks => _bot.LoadedTranslations.Common.Permissions.ManageWebhooks,
            Permissions.ManageGuildExpressions => _bot.LoadedTranslations.Common.Permissions.ManageGuildExpressions,
            Permissions.UseApplicationCommands => _bot.LoadedTranslations.Common.Permissions.UseApplicationCommands,
            Permissions.RequestToSpeak => _bot.LoadedTranslations.Common.Permissions.RequestToSpeak,
            Permissions.ManageEvents => _bot.LoadedTranslations.Common.Permissions.ManageEvents,
            Permissions.ManageThreads => _bot.LoadedTranslations.Common.Permissions.ManageThreads,
            Permissions.CreatePublicThreads => _bot.LoadedTranslations.Common.Permissions.CreatePublicThreads,
            Permissions.CreatePrivateThreads => _bot.LoadedTranslations.Common.Permissions.CreatePrivateThreads,
            Permissions.UseExternalStickers => _bot.LoadedTranslations.Common.Permissions.UseExternalStickers,
            Permissions.SendMessagesInThreads => _bot.LoadedTranslations.Common.Permissions.SendMessagesInThreads,
            Permissions.StartEmbeddedActivities => _bot.LoadedTranslations.Common.Permissions.StartEmbeddedActivities,
            Permissions.ModerateMembers => _bot.LoadedTranslations.Common.Permissions.ModerateMembers,
            Permissions.ViewCreatorMonetizationInsights => _bot.LoadedTranslations.Common.Permissions.ViewCreatorMonetizationInsights,
            Permissions.UseSoundboard => _bot.LoadedTranslations.Common.Permissions.UseSoundboard,
            Permissions.CreateGuildExpressions => _bot.LoadedTranslations.Common.Permissions.CreateGuildExpressions,
            Permissions.CreateEvents => _bot.LoadedTranslations.Common.Permissions.CreateEvents,
            Permissions.UseExternalSounds => _bot.LoadedTranslations.Common.Permissions.UseExternalSounds,
            Permissions.SendVoiceMessages => _bot.LoadedTranslations.Common.Permissions.SendVoiceMessages,
            _ => _bot.LoadedTranslations.Common.MissingTranslation,
        };

    internal static DiscordEmoji UnicodeToEmoji(this string str)
        => DiscordEmoji.FromUnicode(str);

    internal static string GetCustomId(this InteractivityResult<ComponentInteractionCreateEventArgs> e)
        => e.Result.GetCustomId();

    internal static string GetCustomId(this ComponentInteractionCreateEventArgs e)
        => e.Interaction.Data.CustomId;

    internal static DiscordComponentEmoji ToComponent(this DiscordEmoji emoji)
        => new(emoji);

    internal static Task<DiscordMessage> Refetch(this DiscordMessage msg)
        => msg.Channel.GetMessageAsync(msg.Id, true);

    internal static int GetRoleHighestPosition(this DiscordMember member)
        => member is null ? -1 : (member.IsOwner ? 9999 : (!member.Roles.Any() ? 0 : member.Roles.OrderByDescending(x => x.Position).First().Position));

    internal static string GetUniqueDiscordName(this DiscordEmoji emoji)
        => $"{emoji.GetDiscordName().Replace(":", "")}:{emoji.Id}";

    internal static DiscordEmoji ToEmote(this bool b, Bot client)
        => b ? DiscordEmoji.FromUnicode("✅") : EmojiTemplates.GetError(client);

    internal static DiscordEmoji ToPillEmote(this bool b, Bot client)
        => b ? EmojiTemplates.GetPillOn(client) : EmojiTemplates.GetPillOff(client);

    internal static string ToEmotes(this long i)
        => DigitsToEmotes(i.ToString());

    internal static string ToEmotes(this int i)
        => DigitsToEmotes(i.ToString());

    internal static string ToTimestamp(this DateTime dateTime, TimestampFormat format = TimestampFormat.RelativeTime)
        => Formatter.Timestamp(dateTime, format);

    internal static string ToTimestamp(this DateTimeOffset dateTime, TimestampFormat format = TimestampFormat.RelativeTime)
        => Formatter.Timestamp(dateTime, format);

    internal static string GetCommandMention(this DiscordClient client, Bot bot, string command)
        => (bot.status.LoadedConfig.IsDev ?
        client.GetApplicationCommands().GuildCommands.FirstOrDefault(x => x.Key == bot.status.LoadedConfig.Discord.AssetsGuild).Value.ToList() :
        client.GetApplicationCommands().GlobalCommands.ToList())
        .First(x => x.Name == command).Mention;

    internal static IReadOnlyList<DiscordApplicationCommand> GetCommandList(this DiscordClient client, Bot bot)
        => (bot.status.LoadedConfig.IsDev ?
        client.GetApplicationCommands().GuildCommands.FirstOrDefault(x => x.Key == bot.status.LoadedConfig.Discord.AssetsGuild).Value :
        client.GetApplicationCommands().GlobalCommands);

    internal static string GetIcon(this DiscordChannel discordChannel) => discordChannel.Type switch
    {
        ChannelType.Text => "#",
        ChannelType.Voice => "🔊",
        ChannelType.Group => "👥",
        ChannelType.Private => "👤",
        ChannelType.GuildDirectory or ChannelType.Category => "📁",
        ChannelType.News => "📣",
        ChannelType.Store => "🛒",
        ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread => "🗣",
        ChannelType.Stage => "🎤",
        ChannelType.Forum => "📄",
        _ => "❔",
    };

    internal static List<Tuple<ulong, string, bool>>? GetEmotes(this string content)
    {
        if (Regex.IsMatch(content, @"<(a?):([\w]*):(\d*)>", RegexOptions.ExplicitCapture))
        {
            var matchCollection = Regex.Matches(content, @"<(a?):([\w]*):(\d*)>");
            return matchCollection.Select<Match, Tuple<ulong, string, bool>>(x => new Tuple<ulong, string, bool>(Convert.ToUInt64(x.Groups[3].Value), x.Groups[2].Value, !x.Groups[1].Value.IsNullOrWhiteSpace())).GroupBy<Tuple<ulong, string, bool>, ulong>(x => x.Item1).Select<IGrouping<ulong, Tuple<ulong, string, bool>>, Tuple<ulong, string, bool>>(y => y.First<Tuple<ulong, string, bool>>()).ToList<Tuple<ulong, string, bool>>();
        }
        else
            return new List<Tuple<ulong, string, bool>>();
    }

    internal static List<string>? GetMentions(this string content)
    {
        return Regex.IsMatch(content, @"(<@\d*>)") ? Regex.Matches(content, @"(<@\d*>)").Select(x => x.Value).ToList() : (List<string>)null;
    }

    internal static List<KeyValuePair<string, string>> PrepareEmbedFields(this List<KeyValuePair<string, string>> list, string startingText = "", string endingText = "")
    {
        if (startingText.Length > 1024)
            throw new Exception("startingText cant be more than 1024 characters");

        if (endingText.Length > 1024)
            throw new Exception("endingText cant be more than 1024 characters");

        List<KeyValuePair<string, string>> fields = new();
        var currentBuild = startingText;
        var lastTitle = list.First().Key;

        foreach (var field in list)
        {
            if (currentBuild.Length + field.Value.Length >= 1024 || field.Key != lastTitle)
            {
                fields.Add(new KeyValuePair<string, string>(lastTitle, currentBuild));
                currentBuild = "";
            }

            lastTitle = field.Key;
            currentBuild += $"{field.Value}\n";
        }

        if (currentBuild.Length + endingText.Length >= 1024)
        {
            fields.Add(new KeyValuePair<string, string>(lastTitle, currentBuild));
            currentBuild = "";
        }

        currentBuild += endingText;

        if (currentBuild.Length >= 0)
        {
            fields.Add(new KeyValuePair<string, string>(lastTitle, currentBuild));
        }

        return fields;
    }

    internal static List<DiscordEmbedBuilder> PrepareEmbeds(this List<KeyValuePair<string, string>> embedFields, DiscordEmbedBuilder template = null, bool InvisibleOnDuplicateTitles = false)
    {
        template ??= new();

        List<DiscordEmbedBuilder> embeds = new();

        DiscordEmbedBuilder currentBuilder = new(template);

        int CalculateCharacterLimit()
        {
            var currentCount = (currentBuilder.Title?.Length ?? 0) +
                               (currentBuilder.Description?.Length ?? 0) +
                               (currentBuilder.Author?.Name.Length ?? 0) +
                               (currentBuilder.Footer?.Text.Length ?? 0);

            foreach (var field in currentBuilder.Fields)
                currentCount += field.Name.Length + field.Value.Length;

            return currentCount;
        }

        foreach (var field in embedFields)
        {
            if ((currentBuilder.Fields.Any()) && field.Key != (currentBuilder.Fields.LastOrDefault(x => x.Name != "‍", null)?.Name ?? ""))
            {
                embeds.Add(currentBuilder);
                currentBuilder = new(template);
            }

            if (CalculateCharacterLimit() + field.Key.Length + field.Value.Length > 6000)
            {
                embeds.Add(currentBuilder);
                currentBuilder = new(template);
            }

            if (InvisibleOnDuplicateTitles && currentBuilder.Fields.Any(x => x.Name == field.Key))
                _ = currentBuilder.AddField(new DiscordEmbedField("‍", field.Value));
            else
                _ = currentBuilder.AddField(new DiscordEmbedField(field.Key, field.Value));
        }

        embeds.Add(currentBuilder);
        return embeds;
    }

    internal static DiscordEmoji GetClosestColorEmoji(this DiscordColor discordColor, DiscordClient client)
    {
        Dictionary<Color, string> colorArray = new()
        {
            { Color.FromArgb(49, 55, 61)    , ":black_circle:" },
            { Color.FromArgb(85, 172, 238)  , ":blue_circle:" },
            { Color.FromArgb(192, 105, 79)  , ":brown_circle:" },
            { Color.FromArgb(120, 177, 89)  , ":green_circle:" },
            { Color.FromArgb(244, 144, 12)  , ":orange_circle:" },
            { Color.FromArgb(170, 142, 214) , ":purple_circle:" },
            { Color.FromArgb(221, 46, 68)   , ":red_circle:" },
            { Color.FromArgb(230, 231, 232) , ":white_circle:" },
            { Color.FromArgb(253, 203, 88)  , ":yellow_circle:" },
        };

        var color = ColorTools.GetClosestColor(colorArray.Select(x => x.Key).ToList(), Color.FromArgb(discordColor.R, discordColor.G, discordColor.B));

        return DiscordEmoji.FromName(client, colorArray[color]);
    }

    internal static bool TryGetMessage(this DiscordChannel channel, ulong id, out DiscordMessage discordMessage)
    {
        try
        {
            var msg = channel.GetMessageAsync(id).Result;
            discordMessage = msg;
            return true;
        }
        catch (DisCatSharp.Exceptions.NotFoundException)
        {
            discordMessage = null;
            return false;
        }
        catch (DisCatSharp.Exceptions.UnauthorizedException)
        {
            discordMessage = null;
            return false;
        }
        catch (Exception)
        {
            discordMessage = null;
            return false;
        }
    }

    internal static bool TryParseMessageLink(this string link, out ulong GuildId, out ulong ChannelId, out ulong MessageId)
    {
        try
        {
            if (!RegexTemplates.DiscordChannelUrl.IsMatch(link))
                throw new Exception("Not a discord channel url");

            var processed = link.Remove(0, link.IndexOf("channels/") + 9);

            GuildId = Convert.ToUInt64(processed.Remove(processed.IndexOf("/"), processed.Length - processed.IndexOf("/")));
            processed = processed.Remove(0, processed.IndexOf("/") + 1);

            ChannelId = Convert.ToUInt64(processed.Remove(processed.IndexOf("/"), processed.Length - processed.IndexOf("/")));
            processed = processed.Remove(0, processed.IndexOf("/") + 1);

            MessageId = Convert.ToUInt64(processed);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to process channel link", ex);

            GuildId = 0;
            ChannelId = 0;
            MessageId = 0;
            return false;
        }
    }



    private static string DigitsToEmotes(string str)
    {
        return str.Replace("0", "0️⃣")
                  .Replace("1", "1️⃣")
                  .Replace("2", "2️⃣")
                  .Replace("3", "3️⃣")
                  .Replace("4", "4️⃣")
                  .Replace("5", "5️⃣")
                  .Replace("6", "6️⃣")
                  .Replace("7", "7️⃣")
                  .Replace("8", "8️⃣")
                  .Replace("9", "9️⃣");
    }

    public static Task<DiscordUser> ParseStringAsUser(string str, DiscordClient client)
    {
        if (str.IsDigitsOnly())
            return client.GetUserAsync(UInt64.Parse(str));
        else
        {
            var reg = RegexTemplates.UserMention.Match(str);

            if (reg.Success)
                return client.GetUserAsync(UInt64.Parse(reg.Groups[3].Value));
        }

        throw new ArgumentException("");
    }
}
