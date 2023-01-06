namespace ProjectIchigo.Entities;

public class TranslationKey
{
    public Dictionary<string, string> t { get; set; }

    public string Get(User user)
    {
        string? Locale;

        if (!user.OverrideLocale.IsNullOrWhiteSpace())
            Locale = user.OverrideLocale;
        else
            Locale = user.CurrentLocale;

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return "Missing Translation. Please report to the developer.";

        return t[Locale];
    }

    public string Get(DiscordGuild guild)
    {
        string? Locale = null;

        if (!guild.PreferredLocale.IsNullOrWhiteSpace())
            Locale = guild.PreferredLocale;

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return "Missing Translation. Please report to the developer.";

        return t[Locale];
    }

    public string Get(DiscordUser user)
    {
        string? Locale = user.Locale;

        if (Locale is null && !t.ContainsKey("en"))
            return "Missing Translation. Please report to the developer.";

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return "Missing Translation. Please report to the developer.";

        return t[Locale];
    }
}