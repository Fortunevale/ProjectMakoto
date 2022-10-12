using System.Reflection;

namespace ProjectIchigo.Util;

internal static class GenericExtensions
{
    internal static string GetSHA256(this string value)
    {
        StringBuilder Sb = new();

        using (var hash = SHA256.Create())
        {
            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(value));

            foreach (byte b in result)
                Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }

    internal static bool TryGetCustomAttribute<T>(this PropertyInfo collection, Type type, out T? attribute)
    {
        var objects = collection.GetCustomAttributes(false);
        if (objects.Any(x => x.GetType() == type))
        {
            attribute = (T)objects.First(x => x.GetType() == type);
            return true;
        }

        attribute = default;
        return false;
    }

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

    internal static Stream ToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
