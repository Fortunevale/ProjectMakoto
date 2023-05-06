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

public class UserDictionary : IDictionary<ulong, User>
{
    public UserDictionary(Bot _bot)
    {
        this._bot = _bot;
    }

    private Dictionary<ulong, User> _items = new();

    private Bot _bot { get; set; }

    public User this[ulong key]
    {
        get
        {
            if (!_items.ContainsKey(key) && key != 0)
                _items.Add(key, new(_bot, key));

            return _items[key];
        }

        set
        {
            if (!_items.ContainsKey(key) && key != 0)
                _items.Add(key, new(_bot, key));

            _items[key] = value;
        }
    }

    public ICollection<ulong> Keys
        => _items.Keys;

    public ICollection<User> Values
        => _items.Values;

    public int Count
        => _items.Count;

    public bool IsReadOnly
        => false;

    public void Add(ulong key, User value)
        => _items.Add(key, value);

    public void Add(KeyValuePair<ulong, User> item)
        => _items.Add(item.Key, item.Value);

    public void Clear()
        => _items.Clear();

    public bool Remove(ulong key)
        => _items.Remove(key);

    public bool Remove(KeyValuePair<ulong, User> item)
        => _items.Remove(item.Key);

    public void CopyTo(KeyValuePair<ulong, User>[] array, int arrayIndex)
    {
        return;
    }

    public bool Contains(KeyValuePair<ulong, User> item)
        => _items.Contains(item);

    public bool ContainsKey(ulong key)
        => _items.ContainsKey(key);

    public IEnumerator<KeyValuePair<ulong, User>> GetEnumerator()
        => _items.GetEnumerator();

    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out User value)
        => _items.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => _items.GetEnumerator();
}
