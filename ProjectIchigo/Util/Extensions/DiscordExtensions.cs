namespace ProjectIchigo.Util;

internal static class DiscordExtensions
{
    internal static List<DiscordOverwriteBuilder> ConvertToBuilderWithNewOverwrites(this IReadOnlyList<DiscordOverwrite> overwrites, DiscordMember member, Permissions allowed, Permissions denied)
        => overwrites.Where(x => x.Id != member.Id).Select(x => (x.Type == OverwriteType.Role ? new DiscordOverwriteBuilder(x.GetRoleAsync().Result) { Allowed = x.Allowed, Denied = x.Denied } : new DiscordOverwriteBuilder(x.GetMemberAsync().Result) { Allowed = x.Allowed, Denied = x.Denied })).Append(new DiscordOverwriteBuilder(member) { Allowed = (overwrites.FirstOrDefault(x => x.Id == member.Id, null)?.Allowed ?? Permissions.None) | allowed, Denied = (overwrites.FirstOrDefault(x => x.Id == member.Id, null)?.Denied ?? Permissions.None) | denied }).ToList();
    
    internal static List<DiscordOverwriteBuilder> ConvertToBuilderWithNewOverwrites(this IReadOnlyList<DiscordOverwrite> overwrites, DiscordRole role, Permissions allowed, Permissions denied)
        => overwrites.Where(x => x.Id != role.Id).Select(x => (x.Type == OverwriteType.Role ? new DiscordOverwriteBuilder(x.GetRoleAsync().Result) { Allowed = x.Allowed, Denied = x.Denied } : new DiscordOverwriteBuilder(x.GetMemberAsync().Result) { Allowed = x.Allowed, Denied = x.Denied })).Append(new DiscordOverwriteBuilder(role) { Allowed = (overwrites.FirstOrDefault(x => x.Id == role.Id, null)?.Allowed ?? Permissions.None) | allowed, Denied = (overwrites.FirstOrDefault(x => x.Id == role.Id, null)?.Denied ?? Permissions.None) | denied }).ToList();

    internal static List<DiscordOverwriteBuilder> ConvertToBuilder(this IReadOnlyList<DiscordOverwrite> overwrites)
        => overwrites.Select(x => (x.Type == OverwriteType.Role ? new DiscordOverwriteBuilder(x.GetRoleAsync().Result) { Allowed = x.Allowed, Denied = x.Denied } : new DiscordOverwriteBuilder(x.GetMemberAsync().Result) { Allowed = x.Allowed, Denied = x.Denied })).ToList();

    internal static string GetCustomId(this ComponentInteractionCreateEventArgs e)
        => e.Interaction.Data.CustomId;

    internal static DiscordComponentEmoji ToComponent(this DiscordEmoji emoji)
        => new(emoji);

    internal static async Task<DiscordMessage> Refetch(this DiscordMessage msg) 
        => await msg.Channel.GetMessageAsync(msg.Id, true);

    internal static int GetRoleHighestPosition(this DiscordMember member) 
        => (member.IsOwner ? 9999 : (!member.Roles.Any() ? 0 : member.Roles.OrderByDescending(x => x.Position).First().Position));

    internal static string GetUniqueDiscordName(this DiscordEmoji emoji)
        => $"{emoji.GetDiscordName().Replace(":", "")}:{emoji.Id}";

    internal static DiscordEmoji ToEmote(this bool b, DiscordClient client) 
        => b ? DiscordEmoji.FromUnicode("✅") : DiscordEmoji.FromGuildEmote(client, 1005430134070841395);
    
    internal static DiscordEmoji ToPillEmote(this bool b, Bot client) 
        => b ? EmojiTemplates.GetPillOn(client.discordClient, client) : EmojiTemplates.GetPillOff(client.discordClient, client);

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

    internal static DiscordEmbedField AddField(this DiscordEmbedBuilder builder, string name, string value, bool inline = false)
    {
        var field = new DiscordEmbedField(name, value, inline);
        builder.AddField(field);
        return field;
    }

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

    internal static List<DiscordEmbedBuilder> PrepareEmbeds(this List<KeyValuePair<string, string>> list, string title, string description = "")
    {
        List<DiscordEmbedBuilder> discordEmbeds = new();

        int currentAmount = 0;
        var currentEmbed = new DiscordEmbedBuilder() { Title = title };

        foreach (var b in list)
        {
            if (currentAmount + b.Value.Length + b.Key.Length > (6000 - title.Length) - description.Length)
            {
                discordEmbeds.Add(currentEmbed);
                currentEmbed = new DiscordEmbedBuilder() { Title = title };
                currentAmount = 0;
            }

            currentEmbed.AddField(new DiscordEmbedField(b.Key, b.Value));

            currentAmount += b.Key.Length;
            currentAmount += b.Value.Length;
        }

        if (currentAmount > 0)
        {
            discordEmbeds.Add(currentEmbed);
        }

        discordEmbeds.First().Description = description;

        return discordEmbeds;
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
            { Color.FromArgb(49, 55, 61) , ":black_circle:" },
            { Color.FromArgb(85, 172, 238) , ":blue_circle:" },
            { Color.FromArgb(192, 105, 79) , ":brown_circle:" },
            { Color.FromArgb(120, 177, 89) , ":green_circle:" },
            { Color.FromArgb(244, 144, 12) , ":orange_circle:" },
            { Color.FromArgb(170, 142, 214) , ":purple_circle:" },
            { Color.FromArgb(221, 46, 68) , ":red_circle:" },
            { Color.FromArgb(230, 231, 232) , ":white_circle:" },
            { Color.FromArgb(253, 203, 88) , ":yellow_circle:" },
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
}
