namespace Project_Ichigo.Extensions;

internal static class Extensions
{
    internal static bool IsMaintenance(this DiscordMember member, Status _status) => (member as DiscordUser).IsMaintenance(_status);

    internal static bool IsMaintenance(this DiscordUser user, Status _status)
    {
        if (_status.TeamMembers.Contains(user.Id))
            return true;

        return false;
    }

    internal static bool IsAdmin(this DiscordMember member, Status _status)
    {
        if ((member.Roles.Any(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed || x.CheckPermission(Permissions.ManageGuild) == PermissionLevel.Allowed)) ||
            (member.IsMaintenance(_status)) ||
            member.IsOwner)
            return true;

        return false;
    }

    internal static bool IsProtected(this DiscordMember member, Status _status)
    {
        if (member.Permissions.HasPermission(Permissions.Administrator) || member.Permissions.HasPermission(Permissions.ModerateMembers) ||
            member.IsMaintenance(_status) ||
            member.IsOwner)
            return true;

        return false;
    }

    internal static string BoolToEmote(this bool b)
    {
        return b ? ":white_check_mark:" : "<:white_x:939750475354472478>";
    }

    internal static string DigitsToEmotes(this long i) =>
        DigitsToEmotes(i.ToString());

    internal static string DigitsToEmotes(this int i) =>
        DigitsToEmotes(i.ToString());

    private static string DigitsToEmotes(string str)
    {
        return str.Replace("0", ":zero:")
            .Replace("1", ":one:")
            .Replace("2", ":two:")
            .Replace("3", ":three:")
            .Replace("4", ":four:")
            .Replace("5", ":five:")
            .Replace("6", ":six:")
            .Replace("7", ":seven:")
            .Replace("8", ":eight:")
            .Replace("9", ":nine:");
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

    internal static string ToHex(this DiscordColor c) => UniversalExtensions.ToHex(c.R, c.G, c.B);

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

    internal static List<string>? GetEmotes(this string content)
    {
        if (Regex.IsMatch(content, @"(<:[^ ]*:\d*>)"))
            return Regex.Matches(content, @"(<:[^ ]*:\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    internal static List<string>? GetAnimatedEmotes(this string content)
    {
        if (Regex.IsMatch(content, @"(<a:[^ ]*:\d*>)"))
            return Regex.Matches(content, @"(<a:[^ ]*:\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    internal static List<string>? GetMentions(this string content)
    {
        if (Regex.IsMatch(content, @"(<@\d*>)"))
            return Regex.Matches(content, @"(<@\d*>)").Select(x => x.Value).ToList();
        else
            return null;
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

    internal static string Sanitize(this string str)
    {
        var proc = str;

        proc = proc.Replace("`", "´");

        try { proc = Regex.Replace(proc, Resources.Regex.UserMention, ""); } catch { }
        try { proc = Regex.Replace(proc, Resources.Regex.ChannelMention, ""); } catch { }

        return Formatter.Sanitize(proc);
    }
    
    internal static string SanitizeForCodeBlock(this string str)
    {
        return str.Replace("`", "´");
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
            throw;
        }
    }

    internal static bool TryParseMessageLink(this string link, out ulong GuildId, out ulong ChannelId, out ulong MessageId)
    {
        try
        {
            if (!Regex.IsMatch(link, Resources.Regex.DiscordChannelUrl))
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
            LogError("Failed to process channel link", ex);

            GuildId = 0;
            ChannelId = 0;
            MessageId = 0;
            return false;
        }
    }

    internal static bool TryParseRole(this string str, DiscordGuild guild, out DiscordRole Role)
    {
        try
        {
            if (!DisCatSharp.Common.RegularExpressions.DiscordRegEx.Role.IsMatch(str))
            {
                if (!str.IsDigitsOnly())
                    throw new Exception("Not a role id");

                ulong id = Convert.ToUInt64(str);

                if (!guild.Roles.ContainsKey(id))
                    throw new Exception("Guild doesn't have role");

                Role = guild.GetRole(id);
                return true;
            }

            var match_str = Regex.Match(str, @"\d*").Value;

            if (!match_str.IsDigitsOnly())
                throw new Exception("Not a role id");

            ulong id0 = Convert.ToUInt64(match_str);

            if (!guild.Roles.ContainsKey(id0))
                throw new Exception("Guild doesn't have role");

            Role = guild.GetRole(id0);
            return true;
        }
        catch (Exception ex)
        {
            LogError("Failed to process role", ex);

            Role = null;
            return false;
        }
    }

    internal static string GetUniqueDiscordName(this DiscordEmoji emoji)
    {
        return $"{emoji.GetDiscordName().Replace(":", "")}:{emoji.Id}";
    }
}

internal class SqLiteBaseRepository
{
    internal static SQLiteConnection SimpleDbConnection(string file)
    {
        return new SQLiteConnection("Data Source=" + file);
    }
}