using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ProjectMakoto.Entities;

public class MultiTranslationKey : IDictionary<string, string[]>
{
    public Dictionary<string, string[]> t { get; set; } = new();

    public string[] Get(User user)
    {
        string? Locale;

        if (!user.OverrideLocale.IsNullOrWhiteSpace())
            Locale = user.OverrideLocale;
        else
            Locale = user.CurrentLocale;

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return new string[] { "Missing Translation. Please report to the developer." };

        return t[Locale];
    }
    
    public string[] Get(Guild user)
    {
        string? Locale;

        if (!user.OverrideLocale.IsNullOrWhiteSpace())
            Locale = user.OverrideLocale;
        else
            Locale = user.CurrentLocale;

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return new string[] { "Missing Translation. Please report to the developer." };

        return t[Locale];
    }

    public string[] Get(DiscordGuild guild)
    {
        string? Locale = null;

        if (!guild.PreferredLocale.IsNullOrWhiteSpace())
            Locale = guild.PreferredLocale;

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return new string[] { "Missing Translation. Please report to the developer." };

        return t[Locale];
    }

    public string[] Get(DiscordUser user)
    {
        string? Locale = user.Locale;

        if (Locale is null && !t.ContainsKey("en"))
            return new string[] { "Missing Translation. Please report to the developer." };

        if (Locale is null || !t.ContainsKey(Locale))
            Locale = "en";

        if (!t.ContainsKey(Locale))
            return new string[] { "Missing Translation. Please report to the developer." };

        return t[Locale];
    }

    public ICollection<string> Keys => t.Keys;

    public ICollection<string[]> Values => t.Values;

    public int Count => t.Count;

    public bool IsReadOnly => false;

    public string[] this[string key] { get => t[key]; set => t[key] = value; }

    public void Add(string key, string[] value)
        => t.Add(key, value);

    public bool ContainsKey(string key)
        => t.ContainsKey(key);

    public bool Remove(string key)
        => t.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string[] value)
        => t.TryGetValue(key, out value);

    public void Add(KeyValuePair<string, string[]> item)
        => t.Add(item.Key, item.Value);

    public void Clear()
        => t.Clear();

    public bool Contains(KeyValuePair<string, string[]> item)
        => t.Contains(item);

    public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex) { }

    public bool Remove(KeyValuePair<string, string[]> item)
        => t.Remove(item.Key);

    public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        => t.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => t.GetEnumerator();
}