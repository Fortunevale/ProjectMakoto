// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Diagnostics.CodeAnalysis;

namespace ProjectMakoto.Entities;

public sealed class GuildDictionary : IDictionary<ulong, Guild>
{
    public GuildDictionary(Bot _bot)
    {
        this._bot = _bot;
    }

    private Dictionary<ulong, Guild> _items = new();

    private Bot _bot { get; set; }

    public Guild this[ulong key]
    {
        get
        {
            if (!this._items.ContainsKey(key) && key != 0)
                this._items.Add(key, new(key, this._bot));

            if (key == 0)
                return null;

            return this._items[key];
        }

        set
        {
            if (!this._items.ContainsKey(key) && key != 0)
                this._items.Add(key, new(key, this._bot));

            if (key == 0)
                return;

            this._items[key] = value;
        }
    }

    public ICollection<ulong> Keys
        => this._items.Keys;

    public ICollection<Guild> Values
        => this._items.Values;

    public int Count
        => this._items.Count;

    public bool IsReadOnly
        => false;

    public void Add(ulong key, Guild value)
        => this._items.Add(key, value);

    public void Add(KeyValuePair<ulong, Guild> item)
        => this._items.Add(item.Key, item.Value);

    public void Clear()
        => this._items.Clear();

    public bool Remove(ulong key)
        => this._items.Remove(key);

    public bool Remove(KeyValuePair<ulong, Guild> item)
        => this._items.Remove(item.Key);

    public void CopyTo(KeyValuePair<ulong, Guild>[] array, int arrayIndex)
    {
        return;
    }

    public bool Contains(KeyValuePair<ulong, Guild> item)
        => this._items.Contains(item);

    public bool ContainsKey(ulong key)
        => this._items.ContainsKey(key);

    public IEnumerator<KeyValuePair<ulong, Guild>> GetEnumerator()
        => this._items.GetEnumerator();

    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out Guild value)
        => this._items.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => this._items.GetEnumerator();
}
