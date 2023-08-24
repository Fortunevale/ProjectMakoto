// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities;

public class DatabaseDictionary<T1, T2>
{
    /// <summary>
    /// Create a new dictionary mapped to a database table. The key corresponds to the table's primary key.
    /// </summary>
    /// <param name="client">The database client to use.</param>
    /// <param name="tableName">The table this dictionary is mapped to.</param>
    /// <param name="primaryKey">The name of the table's primary key.</param>
    /// <param name="useGuildConnection">Whether this table is in the guild database.</param>
    /// <param name="newValuePredicate">A predicate returning the value object.</param>
    public DatabaseDictionary(DatabaseClient client, string tableName, string primaryKey, bool useGuildConnection, Func<T1, T2>? newValuePredicate)
    {
        if (typeof(T2).GetCustomAttribute<TableNameAttribute>() is null || !this.Try(() => { _ = client.GetPrimaryKey(typeof(T2)); }))
            throw new ArgumentException("The given type is not a valid database type. A valid database type needs to have the 'TableName' attribute.");

        this._client = client;
        this._tableName = tableName;
        this._primaryKey = primaryKey;
        this._useGuildConnection = useGuildConnection;
        this._newValuePredicate = newValuePredicate;
    }

    protected Func<T1, T2>? _newValuePredicate;

    private DatabaseClient _client;
    private string _tableName;
    private string _primaryKey;
    private bool _useGuildConnection;

    private MySqlConnection _connection
        => this._useGuildConnection ? this._client.guildDatabaseConnection : this._client.mainDatabaseConnection;

    private ConcurrentDictionary<T1, T2> _items = new();

    /// <summary>
    /// Gets a value from this dictionary, filling in values if not already present.
    /// </summary>
    /// <param name="key">The key to get. Will not automatically fill if key is 0.</param>
    /// <returns></returns>
    public virtual T2 this[T1 key]
    {
        get
        {
            if (!this._client.RowExists(this._tableName, this._primaryKey, key, this._connection))
                throw new NullReferenceException($"Could not find {this._primaryKey}:{key} in {this._tableName}");

            if (!this._items.ContainsKey(key))
            {
                if (_newValuePredicate is not null)
                    this._items[key] = _newValuePredicate.Invoke(key);
                else
                    this._items[key] = default;
            }

            return this._items[key];
        }
    }

    /// <summary>
    /// Gets a list of all keys.
    /// </summary>
    public ICollection<T1> Keys
        => this._client.GetRowKeys<T1>(this._tableName, this._primaryKey, this._connection);

    /// <summary>
    /// Gets a count of all rows.
    /// </summary>
    public int Count
        => (int)this._client.GetRowCount(this._tableName, this._connection);

    /// <summary>
    /// Adds a new key to the database.
    /// </summary>
    /// <param name="key">The new unique key to add.</param>
    /// <param name="value">The new value to add.</param>
    /// <exception cref="ArgumentException">Thrown if the key failed to create or already exists.</exception>
    public void Add(T1 key, T2 value)
    {
        if (!this._client.CreateRow(this._tableName, typeof(T2), key, this._connection))
            throw new ArgumentException("Failed to create key or key already exists.").AddData("key", key).AddData("value", value);

        this._items[key] = value;
    }

    /// <summary>
    /// Clears the table.
    /// </summary>
    public void Clear()
    {
        _ = this._client.ClearRows(this._tableName, this._connection);
        this._items.Clear();
    }

    /// <summary>
    /// Removes a given key from the database, including it's value.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>Whether the key was removed.</returns>
    public bool Remove(T1 key)
    {
        if (!this._items.Remove(key, out _))
            return false;

        return this.Try(() => { _ = this._client.DeleteRow(this._tableName, this._primaryKey, key.ToString(), this._connection); });
    }

    /// <summary>
    /// Checks if the given key exists in the database.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key exists in the database.</returns>
    public bool ContainsKey(T1 key)
        => this._client.RowExists(this._tableName, this._primaryKey, key.ToString(), this._connection);

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        => this.Fetch().GetEnumerator();

    /// <summary>
    /// Tries retrieving a value from the database.
    /// </summary>
    /// <param name="key">The key to retrieve.</param>
    /// <param name="value">The retrieved value. Null if false.</param>
    /// <returns>Whether the value was retrieved.</returns>
    public bool TryGetValue(T1 key, [MaybeNullWhen(false)] out T2 value)
    {
        try
        {
            value = this[key];
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Retreives a linq compatible read-only list.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<T1, T2> Fetch()
        => this.Keys.ToDictionary(x => x, y =>
        {
            if (!this._items.ContainsKey(y))
            {
                if (_newValuePredicate is not null)
                    this._items[y] = _newValuePredicate.Invoke(y);
                else
                    this._items[y] = default;
            }

            return this._items[y];
        });

    private bool Try(Action action)
    {
        try
        {
            action.Invoke();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
