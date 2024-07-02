// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

/// <summary>
/// Create a new dictionary mapped to a database table. The key corresponds to the table's primary key.
/// </summary>
/// <param name="client">The database client to use.</param>
/// <param name="tableName">The table this dictionary is mapped to.</param>
/// <param name="primaryKey">The name of the table's primary key.</param>
/// <param name="useGuildConnection">Whether this table is in the guild database.</param>
public class DatabaseList<T1>(DatabaseClient client, string tableName, string primaryKey, bool useGuildConnection)
{
    private DatabaseClient _client = client;
    private string _tableName = tableName;
    private string _primaryKey = primaryKey;
    private bool _useGuildConnection = useGuildConnection;

    private DatabaseClient.MySqlConnectionInformation _connection
        => this._useGuildConnection ? this._client.guildDatabaseConnection : this._client.mainDatabaseConnection;

    private List<T1> _items = new();

    /// <summary>
    /// Gets a count of all rows.
    /// </summary>
    public int Count
        => (int)this._client.GetRowCount(this._tableName, this._connection);

    /// <summary>
    /// Adds a new item to the database.
    /// </summary>
    /// <param name="item">The new item to add.</param>
    /// <exception cref="ArgumentException">Thrown if the key failed to create or already exists.</exception>
    public void Add(T1 item)
    {
        if (!this._client.CreateRow(this._tableName, this._primaryKey, item, this._connection))
            throw new ArgumentException("Failed to create key or key already exists.");

        lock (this._items)
        {
            this._items.Add(item);
        }
    }

    /// <summary>
    /// Clears the table.
    /// </summary>
    public void Clear()
    {
        lock (this._items)
            this._items.Clear();

        _ = this._client.ClearRows(this._tableName, this._connection);
    }

    /// <summary>
    /// Removes a given key from the database, including it's value.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>Whether the key was removed.</returns>
    public bool Remove(T1 key)
    {
        lock (this._items)
            if (!this._items.Remove(key))
                return false;

        return this.Try(() => { _ = this._client.DeleteRow(this._tableName, this._primaryKey, key.ToString(), this._connection); });
    }

    /// <summary>
    /// Checks if the given key exists in the database.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether the key exists in the database.</returns>
    public bool Contains(T1 key)
        => this._client.RowExists(this._tableName, this._primaryKey, key, this._connection);

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T1> GetEnumerator()
        => this._client.GetRowKeys<T1>(this._tableName, this._primaryKey, this._connection).ToList().GetEnumerator();

    
    /// <summary>
    /// Retrieves a linq compatible read-only list.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<T1> Fetch()
        => this._client.GetRowKeys<T1>(this._tableName, this._primaryKey, this._connection);

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
