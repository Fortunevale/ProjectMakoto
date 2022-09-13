namespace ProjectIchigo.Util;

internal static class GenericExtensions
{
    internal static string IsValidHexColor(this string str, string Default = "#FFFFFF") 
        => !str.IsNullOrWhiteSpace() && Regex.IsMatch(str, @"^(#([a-fA-f0-9]{6}))$") ? str : Default;

    internal static string ToHex(this DiscordColor c) 
        => UniversalExtensions.ToHex(c.R, c.G, c.B);

    internal static string SanitizeForCode(this string str) 
        => str.Replace("`", "´");

    internal static string Sanitize(this string str)
    {
        var proc = str;

        proc = proc.Replace("`", "´");

        try
        { proc = RegexTemplates.UserMention.Replace(proc, ""); }
        catch { }
        try
        { proc = RegexTemplates.ChannelMention.Replace(proc, ""); }
        catch { }

        proc = proc.Replace("@everyone", "");
        proc = proc.Replace("@here", "");

        return Formatter.Sanitize(proc);
    }


}
