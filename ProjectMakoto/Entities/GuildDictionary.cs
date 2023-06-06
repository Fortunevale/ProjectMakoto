// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections;
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
            if (!_items.ContainsKey(key) && key != 0)
                _items.Add(key, new(key, _bot));

            return _items[key]; 
        }

        set
        {
            if (!_items.ContainsKey(key) && key != 0)
                _items.Add(key, new(key, _bot));

            _items[key] = value;
        }
    }

    public ICollection<ulong> Keys 
        => _items.Keys;

    public ICollection<Guild> Values 
        => _items.Values;

    public int Count 
        => _items.Count;

    public bool IsReadOnly 
        => false;

    public void Add(ulong key, Guild value) 
        => _items.Add(key, value);

    public void Add(KeyValuePair<ulong, Guild> item) 
        => _items.Add(item.Key, item.Value);

    public void Clear() 
        => _items.Clear();

    public bool Remove(ulong key) 
        => _items.Remove(key);

    public bool Remove(KeyValuePair<ulong, Guild> item) 
        => _items.Remove(item.Key);

    public void CopyTo(KeyValuePair<ulong, Guild>[] array, int arrayIndex)
    {
        return;
    }

    public bool Contains(KeyValuePair<ulong, Guild> item)
        => _items.Contains(item);

    public bool ContainsKey(ulong key)
        => _items.ContainsKey(key);

    public IEnumerator<KeyValuePair<ulong, Guild>> GetEnumerator()
        => _items.GetEnumerator();

    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out Guild value)
        => _items.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => _items.GetEnumerator();
}
