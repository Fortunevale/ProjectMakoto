// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Diagnostics.CodeAnalysis;

namespace ProjectMakoto.Entities;

public sealed class SingleTranslationKey : IDictionary<string, string>
{
    public Dictionary<string, string> t { get; set; } = new();

    public string Get(User user)
    {
        var Locale = !user.OverrideLocale.IsNullOrWhiteSpace() ? user.OverrideLocale : user.CurrentLocale;

        if (Locale is null || !this.t.ContainsKey(Locale))
            Locale = "en";

        return !this.t.TryGetValue(Locale, out var value) ? "Missing Translation. Please report to the developer." : value;
    }

    public string Get(Guild user)
    {
        var Locale = !user.OverrideLocale.IsNullOrWhiteSpace() ? user.OverrideLocale : user.CurrentLocale;

        if (Locale is null || !this.t.ContainsKey(Locale))
            Locale = "en";

        return !this.t.TryGetValue(Locale, out var value) ? "Missing Translation. Please report to the developer." : value;
    }

    public string Get(DiscordGuild guild)
    {
        string? Locale = null;

        if (!guild.PreferredLocale.IsNullOrWhiteSpace())
            Locale = guild.PreferredLocale;

        if (Locale is null || !this.t.ContainsKey(Locale))
            Locale = "en";

        return !this.t.TryGetValue(Locale, out var value) ? "Missing Translation. Please report to the developer." : value;
    }

    public string Get(DiscordUser user)
    {
        var Locale = user.Locale;

        if (Locale is null && !this.t.ContainsKey("en"))
            return "Missing Translation. Please report to the developer.";

        if (Locale is null || !this.t.ContainsKey(Locale))
            Locale = "en";

        return !this.t.TryGetValue(Locale, out var value) ? "Missing Translation. Please report to the developer." : value;
    }

    public ICollection<string> Keys => this.t.Keys;

    public ICollection<string> Values => this.t.Values;

    public int Count => this.t.Count;

    public bool IsReadOnly => false;

    public string this[string key] { get => this.t[key]; set => this.t[key] = value; }

    public void Add(string key, string value)
        => this.t.Add(key, value);

    public bool ContainsKey(string key)
        => this.t.ContainsKey(key);

    public bool Remove(string key)
        => this.t.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => this.t.TryGetValue(key, out value);

    public void Add(KeyValuePair<string, string> item)
        => this.t.Add(item.Key, item.Value);

    public void Clear()
        => this.t.Clear();

    public bool Contains(KeyValuePair<string, string> item)
        => this.t.Contains(item);

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) { }

    public bool Remove(KeyValuePair<string, string> item)
        => this.t.Remove(item.Key);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => this.t.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.t.GetEnumerator();

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    [Obsolete("Do not call .ToString(). Use the .Get() Method instead.", true)]
    public override string ToString()
    {
        StackTrace stackTrace = new();
        var stackFrames = stackTrace.GetFrames();

        var callingFrame = stackFrames[1];
        var method = callingFrame.GetMethod();

        Log.Error(new InvalidCallException(stackTrace.ToString()).AddData("DeclaryingType", method.DeclaringType).AddData("Method", method),
            "Key with english text '{text}' was incorrectly accessed. Defaulting to english translation.", this.t["en"]);
        return this.t["en"];
    }
}