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

public sealed class SelfFillingDictionary<T> : IDictionary<ulong, T>
{
    /// <summary>
    /// Creates a new self filling dictionary. 
    /// </summary>
    /// <param name="newValuePredicate">A predicate to create the intended value. If null, will default to the default value of the value.</param>
    public SelfFillingDictionary(Func<ulong, T>? newValuePredicate = null)
    {
        this._newValuePredicate = newValuePredicate;
    }

    private Func<ulong, T>? _newValuePredicate;
    private Dictionary<ulong, T> _items = new();

    /// <summary>
    /// Gets a value from this dictionary, filling in values if not already present.
    /// </summary>
    /// <param name="key">The key to get. Will not automatically fill if key is 0.</param>
    /// <returns></returns>
    public T this[ulong key]
    {
        get
        {
            if (!this._items.ContainsKey(key) && key != 0)
            {
                if (_newValuePredicate is not null)
                {
                    _logger.LogTrace("Creating '{id}' of type '{type}'", key, typeof(T).Name);
                    this._items.Add(key, _newValuePredicate.Invoke(key));
                }
                else
                {
                    _logger.LogWarn("Creating '{id}' of type '{type}' with default value", key, typeof(T).Name);
                    this._items.Add(key, default);
                }
            }

            return this._items[key];
        }

        set
        {
            if (!this._items.ContainsKey(key) && key != 0)
            {
                if (_newValuePredicate is not null)
                {
                    _logger.LogTrace("Creating '{id}' of type '{type}'", key, typeof(T).Name);
                    this._items.Add(key, _newValuePredicate.Invoke(key));
                }
                else
                {
                    _logger.LogWarn("Creating '{id}' of type '{type}' with default value", key, typeof(T).Name);
                    this._items.Add(key, default);
                }
            }

            this._items[key] = value;
        }
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Keys"/>
    public ICollection<ulong> Keys
        => this._items.Keys;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Values"/>
    public ICollection<T> Values
        => this._items.Values;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Count"/>
    public int Count
        => this._items.Count;

    /// <summary>
    /// Whether this collection is read-only. Will always be false.
    /// </summary>
    public bool IsReadOnly
        => false;

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
    public void Add(ulong key, T value)
        => this._items.Add(key, value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
    /// <param name="item">The item to add.</param>
    [Obsolete("Use .Add(TKey, TValue) instead.")]
    public void Add(KeyValuePair<ulong, T> item)
        => this._items.Add(item.Key, item.Value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Clear"/>
    public void Clear()
        => this._items.Clear();

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey)"/>
    public bool Remove(ulong key)
        => this._items.Remove(key);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey)"/>
    /// <param name="item">The item to remove.</param>
    [Obsolete("Use .Remove(ulong) instead.")]
    public bool Remove(KeyValuePair<ulong, T> item)
        => this._items.Remove(item.Key);

    /// <inheritdoc cref="Array.CopyTo"/>
    public void CopyTo(KeyValuePair<ulong, T>[] array, int arrayIndex)
        => this._items.ToArray().CopyTo(array, arrayIndex);

    /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
    public bool Contains(KeyValuePair<ulong, T> item)
        => this._items.Contains(item);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsKey(TKey)"/>
    public bool ContainsKey(ulong key)
        => this._items.ContainsKey(key);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.GetEnumerator"/>
    public IEnumerator<KeyValuePair<ulong, T>> GetEnumerator()
        => this._items.GetEnumerator();

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out T value)
        => this._items.TryGetValue(key, out value);

    /// <inheritdoc cref="Dictionary{TKey, TValue}.GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
        => this._items.GetEnumerator();
}
