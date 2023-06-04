// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

internal static class DiscordExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    internal static string GetUsername(this DiscordUser user)
        => user.IsMigrated ? user.GlobalName : user.Username;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0102:[Discord] Deprecated", Justification = "<Pending>")]
    internal static string GetUsernameWithIdentifier(this DiscordUser user)
        => user.IsMigrated ? user.UsernameWithGlobalName : user.UsernameWithDiscriminator;

    internal static string ToTranslatedPermissionString(this Permissions perm, Guild guild)
        => GetTranslationObject(perm) == Bot.loadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(Xorog.Logger.Enums.LogLevel.WARN, "Missing Translation") : GetTranslationObject(perm).Get(guild);

    internal static string ToTranslatedPermissionString(this Permissions perm, DiscordGuild guild)
        => GetTranslationObject(perm) == Bot.loadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(Xorog.Logger.Enums.LogLevel.WARN, "Missing Translation") : GetTranslationObject(perm).Get(guild);

    internal static string ToTranslatedPermissionString(this Permissions perm, User user)
        => GetTranslationObject(perm) == Bot.loadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(Xorog.Logger.Enums.LogLevel.WARN, "Missing Translation") : GetTranslationObject(perm).Get(user);

    internal static string ToTranslatedPermissionString(this Permissions perm, DiscordUser user)
        => GetTranslationObject(perm) == Bot.loadedTranslations.Common.MissingTranslation ? perm.ToPermissionString().Log(Xorog.Logger.Enums.LogLevel.WARN, "Missing Translation") : GetTranslationObject(perm).Get(user);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0101:[Discord] InExperiment", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DCS0103:[Discord] Unreleased", Justification = "<Pending>")]
    private static SingleTranslationKey GetTranslationObject(Permissions perm) 
        => perm switch
        {
            Permissions.None => Bot.loadedTranslations.Common.Permissions.None,
            Permissions.All => Bot.loadedTranslations.Common.Permissions.All,
            Permissions.CreateInstantInvite => Bot.loadedTranslations.Common.Permissions.CreateInstantInvite,
            Permissions.KickMembers => Bot.loadedTranslations.Common.Permissions.KickMembers,
            Permissions.BanMembers => Bot.loadedTranslations.Common.Permissions.BanMembers,
            Permissions.Administrator => Bot.loadedTranslations.Common.Permissions.Administrator,
            Permissions.ManageChannels => Bot.loadedTranslations.Common.Permissions.ManageChannels,
            Permissions.ManageGuild => Bot.loadedTranslations.Common.Permissions.ManageGuild,
            Permissions.AddReactions => Bot.loadedTranslations.Common.Permissions.AddReactions,
            Permissions.ViewAuditLog => Bot.loadedTranslations.Common.Permissions.ViewAuditLog,
            Permissions.PrioritySpeaker => Bot.loadedTranslations.Common.Permissions.PrioritySpeaker,
            Permissions.Stream => Bot.loadedTranslations.Common.Permissions.Stream,
            Permissions.AccessChannels => Bot.loadedTranslations.Common.Permissions.AccessChannels,
            Permissions.SendMessages => Bot.loadedTranslations.Common.Permissions.SendMessages,
            Permissions.SendTtsMessages => Bot.loadedTranslations.Common.Permissions.SendTtsMessages,
            Permissions.ManageMessages => Bot.loadedTranslations.Common.Permissions.ManageMessages,
            Permissions.EmbedLinks => Bot.loadedTranslations.Common.Permissions.EmbedLinks,
            Permissions.AttachFiles => Bot.loadedTranslations.Common.Permissions.AttachFiles,
            Permissions.ReadMessageHistory => Bot.loadedTranslations.Common.Permissions.ReadMessageHistory,
            Permissions.MentionEveryone => Bot.loadedTranslations.Common.Permissions.MentionEveryone,
            Permissions.UseExternalEmojis => Bot.loadedTranslations.Common.Permissions.UseExternalEmojis,
            Permissions.ViewGuildInsights => Bot.loadedTranslations.Common.Permissions.ViewGuildInsights,
            Permissions.UseVoice => Bot.loadedTranslations.Common.Permissions.UseVoice,
            Permissions.Speak => Bot.loadedTranslations.Common.Permissions.Speak,
            Permissions.MuteMembers => Bot.loadedTranslations.Common.Permissions.MuteMembers,
            Permissions.DeafenMembers => Bot.loadedTranslations.Common.Permissions.DeafenMembers,
            Permissions.MoveMembers => Bot.loadedTranslations.Common.Permissions.MoveMembers,
            Permissions.UseVoiceDetection => Bot.loadedTranslations.Common.Permissions.UseVoiceDetection,
            Permissions.ChangeNickname => Bot.loadedTranslations.Common.Permissions.ChangeNickname,
            Permissions.ManageNicknames => Bot.loadedTranslations.Common.Permissions.ManageNicknames,
            Permissions.ManageRoles => Bot.loadedTranslations.Common.Permissions.ManageRoles,
            Permissions.ManageWebhooks => Bot.loadedTranslations.Common.Permissions.ManageWebhooks,
            Permissions.ManageGuildExpressions => Bot.loadedTranslations.Common.Permissions.ManageGuildExpressions,
            Permissions.UseApplicationCommands => Bot.loadedTranslations.Common.Permissions.UseApplicationCommands,
            Permissions.RequestToSpeak => Bot.loadedTranslations.Common.Permissions.RequestToSpeak,
            Permissions.ManageEvents => Bot.loadedTranslations.Common.Permissions.ManageEvents,
            Permissions.ManageThreads => Bot.loadedTranslations.Common.Permissions.ManageThreads,
            Permissions.CreatePublicThreads => Bot.loadedTranslations.Common.Permissions.CreatePublicThreads,
            Permissions.CreatePrivateThreads => Bot.loadedTranslations.Common.Permissions.CreatePrivateThreads,
            Permissions.UseExternalStickers => Bot.loadedTranslations.Common.Permissions.UseExternalStickers,
            Permissions.SendMessagesInThreads => Bot.loadedTranslations.Common.Permissions.SendMessagesInThreads,
            Permissions.StartEmbeddedActivities => Bot.loadedTranslations.Common.Permissions.StartEmbeddedActivities,
            Permissions.ModerateMembers => Bot.loadedTranslations.Common.Permissions.ModerateMembers,
            Permissions.ViewCreatorMonetizationInsights => Bot.loadedTranslations.Common.Permissions.ViewCreatorMonetizationInsights,
            Permissions.UseSoundboard => Bot.loadedTranslations.Common.Permissions.UseSoundboard,
            Permissions.CreateGuildExpressions => Bot.loadedTranslations.Common.Permissions.CreateGuildExpressions,
            Permissions.CreateEvents => Bot.loadedTranslations.Common.Permissions.CreateEvents,
            Permissions.UseExternalSounds => Bot.loadedTranslations.Common.Permissions.UseExternalSounds,
            Permissions.SendVoiceMessages => Bot.loadedTranslations.Common.Permissions.SendVoiceMessages,
            _ => Bot.loadedTranslations.Common.MissingTranslation,
        };

    internal static DiscordEmoji UnicodeToEmoji(this string str)
        => DiscordEmoji.FromUnicode(str);

    internal static string GetCustomId(this InteractivityResult<ComponentInteractionCreateEventArgs> e)
        => e.Result.GetCustomId();

    internal static string GetCustomId(this ComponentInteractionCreateEventArgs e)
        => e.Interaction.Data.CustomId;

    internal static DiscordComponentEmoji ToComponent(this DiscordEmoji emoji)
        => new(emoji);

    internal static async Task<DiscordMessage> Refetch(this DiscordMessage msg) 
        => await msg.Channel.GetMessageAsync(msg.Id, true);

    internal static int GetRoleHighestPosition(this DiscordMember member) 
        => member is null ? -1 : (member.IsOwner ? 9999 : (!member.Roles.Any() ? 0 : member.Roles.OrderByDescending(x => x.Position).First().Position));

    internal static string GetUniqueDiscordName(this DiscordEmoji emoji)
        => $"{emoji.GetDiscordName().Replace(":", "")}:{emoji.Id}";

    internal static DiscordEmoji ToEmote(this bool b, Bot client) 
        => b ? DiscordEmoji.FromUnicode("✅") : EmojiTemplates.GetWhiteXMark(client);
    
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
        client.GetApplicationCommands().GuildCommands.FirstOrDefault(x => x.Key == bot.status.LoadedConfig.Channels.Assets).Value.ToList() :
        client.GetApplicationCommands().GlobalCommands.ToList())
        .First(x => x.Name == command).Mention;

    internal static IReadOnlyList<DiscordApplicationCommand> GetCommandList(this DiscordClient client, Bot bot)
        => (bot.status.LoadedConfig.IsDev ?
        client.GetApplicationCommands().GuildCommands.FirstOrDefault(x => x.Key == bot.status.LoadedConfig.Channels.Assets).Value :
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
            MatchCollection matchCollection = Regex.Matches(content, @"<(a?):([\w]*):(\d*)>");
            return matchCollection.Select<Match, Tuple<ulong, string, bool>>(x => new Tuple<ulong, string, bool>(Convert.ToUInt64(x.Groups[3].Value), x.Groups[2].Value, !x.Groups[1].Value.IsNullOrWhiteSpace())).GroupBy<Tuple<ulong, string, bool>, ulong>(x => x.Item1).Select<IGrouping<ulong, Tuple<ulong, string, bool>>, Tuple<ulong, string, bool>>(y => y.First<Tuple<ulong, string, bool>>()).ToList<Tuple<ulong, string, bool>>();
        }
        else
            return new List<Tuple<ulong, string, bool>>();
    }

    internal static List<string>? GetMentions(this string content)
    {
        if (Regex.IsMatch(content, @"(<@\d*>)"))
            return Regex.Matches(content, @"(<@\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    internal static List<KeyValuePair<string, string>> PrepareEmbedFields(this List<KeyValuePair<string, string>> list, string startingText = "", string endingText = "")
    {
        if (startingText.Length > 1024)
            throw new Exception("startingText cant be more than 1024 characters");

        if (endingText.Length > 1024)
            throw new Exception("endingText cant be more than 1024 characters");

        List<KeyValuePair<string, string>> fields = new();
        string currentBuild = startingText;
        string lastTitle = list.First().Key;

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

        var color = GetClosestColor(colorArray.Select(x => x.Key).ToList(), Color.FromArgb(discordColor.R, discordColor.G, discordColor.B));

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

            string processed = link.Remove(0, link.IndexOf("channels/") + 9);

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

    public async static Task<DiscordUser> ParseStringAsUser(string str, DiscordClient client)
    {
        if (str.IsDigitsOnly())
            return await client.GetUserAsync(UInt64.Parse(str));
        else
        {
            var reg = RegexTemplates.UserMention.Match(str);

            if (reg.Success)
                return await client.GetUserAsync(UInt64.Parse(reg.Groups[3].Value));
        }

        throw new ArgumentException("");
    }
}
