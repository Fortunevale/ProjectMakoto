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

public sealed class SelfFillingDictionary<T> : RequiresBotReference, IDictionary<ulong, T> where T : BaseSelfFillingListValue<T>
{
    public SelfFillingDictionary(Bot bot) : base(bot)
    {
    }

    private Dictionary<ulong, T> _items = new();

    public T this[ulong key]
    {
        get
        {
            if (!this._items.ContainsKey(key) && key != 0)
            {
                var t = new BaseSelfFillingListValue<T>(this.Bot, key);
                this._items.Add(key, t.Convert(t));
            }

            return this._items[key];
        }

        set
        {
            if (!this._items.ContainsKey(key) && key != 0)
            {
                var t = new BaseSelfFillingListValue<T>(this.Bot, key);
                this._items.Add(key, t.Convert(t));
            }

            this._items[key] = value;
        }
    }

    public ICollection<ulong> Keys
        => this._items.Keys;

    public ICollection<T> Values
        => this._items.Values;

    public int Count
        => this._items.Count;

    public bool IsReadOnly
        => false;

    public void Add(ulong key, T value)
        => this._items.Add(key, value);

    public void Add(KeyValuePair<ulong, T> item)
        => this._items.Add(item.Key, item.Value);

    public void Clear()
        => this._items.Clear();

    public bool Remove(ulong key)
        => this._items.Remove(key);

    public bool Remove(KeyValuePair<ulong, T> item)
        => this._items.Remove(item.Key);

    public void CopyTo(KeyValuePair<ulong, T>[] array, int arrayIndex)
    {
        return;
    }

    public bool Contains(KeyValuePair<ulong, T> item)
        => this._items.Contains(item);

    public bool ContainsKey(ulong key)
        => this._items.ContainsKey(key);

    public IEnumerator<KeyValuePair<ulong, T>> GetEnumerator()
        => this._items.GetEnumerator();

    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out T value)
        => this._items.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => this._items.GetEnumerator();
}
