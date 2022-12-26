namespace ProjectIchigo.Entities;

public class TranslationKey
{
    public Dictionary<string, string> t { get; set; }

    public string Get(User user)
    {
        if (!user.OverrideLocale.IsNullOrWhiteSpace())
            if (t.ContainsKey(user?.OverrideLocale ?? "en"))
                return t[user?.OverrideLocale ?? "en"];

        if (t.ContainsKey(user?.CurrentLocale ?? "en"))
            return t[user?.CurrentLocale ?? "en"];
        else
            return t["en"];
    }
    
    public string Get(DiscordUser user)
    {
        if (t.ContainsKey(user?.Locale ?? "en"))
            return t[user?.Locale ?? "en"];
        else
            return t["en"];
    }
}