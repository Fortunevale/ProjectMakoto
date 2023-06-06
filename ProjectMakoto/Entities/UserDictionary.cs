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

public sealed class UserDictionary : IDictionary<ulong, User>
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
            if (!this._items.ContainsKey(key) && key != 0)
                this._items.Add(key, new(this._bot, key));

            return this._items[key];
        }

        set
        {
            if (!this._items.ContainsKey(key) && key != 0)
                this._items.Add(key, new(this._bot, key));

            this._items[key] = value;
        }
    }

    public ICollection<ulong> Keys
        => this._items.Keys;

    public ICollection<User> Values
        => this._items.Values;

    public int Count
        => this._items.Count;

    public bool IsReadOnly
        => false;

    public void Add(ulong key, User value)
        => this._items.Add(key, value);

    public void Add(KeyValuePair<ulong, User> item)
        => this._items.Add(item.Key, item.Value);

    public void Clear()
        => this._items.Clear();

    public bool Remove(ulong key)
        => this._items.Remove(key);

    public bool Remove(KeyValuePair<ulong, User> item)
        => this._items.Remove(item.Key);

    public void CopyTo(KeyValuePair<ulong, User>[] array, int arrayIndex)
    {
        return;
    }

    public bool Contains(KeyValuePair<ulong, User> item)
        => this._items.Contains(item);

    public bool ContainsKey(ulong key)
        => this._items.ContainsKey(key);

    public IEnumerator<KeyValuePair<ulong, User>> GetEnumerator()
        => this._items.GetEnumerator();

    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out User value)
        => this._items.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => this._items.GetEnumerator();
}
