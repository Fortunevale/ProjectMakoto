namespace Project_Ichigo;

public static class Extensions
{
    public static string DigitsToEmotes(this long i) =>
        DigitsToEmotes(i.ToString());

    public static string DigitsToEmotes(this int i) =>
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

    public static List<DiscordEmbedBuilder> PrepareEmbeds(this List<KeyValuePair<string, string>> list, string title, string description = "")
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

            currentEmbed.AddField(b.Key, b.Value);

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

    public static List<KeyValuePair<string, string>> PrepareEmbedFields(this List<KeyValuePair<string, string>> list, string startingText = "", string endingText = "")
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

    public static List<string>? GetEmotes(this string content)
    {
        if (Regex.IsMatch(content, @"(<:[^ ]*:\d*>)"))
            return Regex.Matches(content, @"(<:[^ ]*:\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    public static List<string>? GetAnimatedEmotes(this string content)
    {
        if (Regex.IsMatch(content, @"(<a:[^ ]*:\d*>)"))
            return Regex.Matches(content, @"(<a:[^ ]*:\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    public static List<string>? GetMentions(this string content)
    {
        if (Regex.IsMatch(content, @"(<@\d*>)"))
            return Regex.Matches(content, @"(<@\d*>)").Select(x => x.Value).ToList();
        else
            return null;
    }

    //public static bool IsProtected(this DiscordMember member)
    //{
    //    if (member.Roles.Any(x => x.Id == Objects.Settings.basicPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.advancedPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.antibotRoleId) || 
    //        (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        (Objects.ProtectedAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        member.IsOwner)
    //        return true;

    //    return false;
    //}

    //public static bool IsStaff(this DiscordMember member)
    //{
    //    if (member.Roles.Any(x => x.Id == Objects.Settings.basicPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.advancedPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId) ||
    //        (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        member.IsOwner)
    //        return true;

    //    return false;
    //}

    //public static bool IsMaintenance(this DiscordMember member)
    //{
    //    if (member.Id == 411950662662881290 || (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370))
    //        return true;

    //    return false;
    //}

    //public static bool IsAdmin(this DiscordMember member)
    //{
    //    if ((member.Roles.Any(x => x.Id == Objects.Settings.advancedPermissionsRoleId) && member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId)) ||
    //        (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        member.IsOwner)
    //        return true;

    //    return false;
    //}

    //public static bool IsMod(this DiscordMember member)
    //{
    //    if ((member.Roles.Any(x => x.Id == Objects.Settings.basicPermissionsRoleId) && member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId)) ||
    //        (member.Roles.Any(x => x.Id == Objects.Settings.advancedPermissionsRoleId) && member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId)) ||
    //        (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        member.IsOwner)
    //        return true;

    //    return false;
    //}

    //public static bool IsDj(this DiscordMember member)
    //{
    //    if (member.Roles.Any(x => x.Id == Objects.Settings.basicPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.advancedPermissionsRoleId) ||
    //        member.Roles.Any(x => x.Id == Objects.Settings.teamRoleId) ||
    //        member.Roles.Any(x => x.Name.ToLower() == "dj") ||
    //        (Objects.MaintenanceAccounts.Contains(member.Id) && Objects.Settings.serverId != 494984485201379370) ||
    //        member.IsOwner)
    //        return true;

    //    return false;
    //}
}

public class SqLiteBaseRepository
{
    public static SQLiteConnection SimpleDbConnection(string file)
    {
        return new SQLiteConnection("Data Source=" + file);
    }
}