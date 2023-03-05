using System.Reflection;

namespace ProjectMakoto.Util;

internal static class GenericExtensions
{
    internal static string GetSHA256(this string value)
    {
        StringBuilder Sb = new();
        Encoding enc = Encoding.UTF8;
        byte[] result = SHA256.HashData(enc.GetBytes(value));

        foreach (byte b in result)
            Sb.Append(b.ToString("x2"));

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

    internal static string FullSanitize(this string str)
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

    /// <summary>
    /// Builds a string array into a string, used for MultiTranslationKeys.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="Code">Whether to prefix and suffix ` on non-empty lines.</param>
    /// <returns></returns>
    internal static string Build(this string[] array, bool Code = false, bool UseBoldMarker = false)
        => string.Join("\n", array.Select(x => 
        {
            if (!Code)
                return x;

            if (x.IsNullOrWhiteSpace())
                return x;

            bool boldLine = false;
            var y = x;

            if (y.StartsWith("**"))
            {
                boldLine = true;
                y = y.Remove(0, 2);
            }

            return $"{(boldLine ? "**" : "")}`{y}`{(boldLine ? "**" : "")}";
        }));

    /// <summary>
    /// Runs Replace on every string in a string array and returns the new array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    /// <returns></returns>
    internal static string[] Replace(this string[] array, string old, object @new) 
        => array.Select(x => x.Replace(old, @new)).ToArray();

    /// <summary>
    /// Calculates maximum character count for given list of translation keys.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="pairs"></param>
    /// <returns></returns>
    internal static int CalculatePadding(User user, params SingleTranslationKey[] pairs)
    {
        int pad = 0;

        foreach (var b in pairs)
        {
            var length = b.Get(user).Length;

            if (length > pad)
                pad = length;
        }

        return pad;
    }
}
