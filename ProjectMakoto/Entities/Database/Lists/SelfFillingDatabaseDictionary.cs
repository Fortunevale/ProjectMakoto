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

namespace ProjectMakoto.Entities;

/// <summary>
/// Creates a new self filling dictionary. 
/// </summary>
/// <param name="newValuePredicate">A predicate to create the intended value. If null, will default to the default value of the value.</param>
public sealed class SelfFillingDatabaseDictionary<T> : DatabaseDictionary<ulong, T>
{
    public SelfFillingDatabaseDictionary(DatabaseClient client, string tableName, string primaryKey, DatabaseClient.MySqlConnectionInformation connection, Func<ulong, T>? newValuePredicate = null) : 
        base(client, tableName, primaryKey, connection, newValuePredicate)
    {
    }

    /// <summary>
    /// Creates a new Dictionary with ulong as key. Constructor for plugins.
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="type"></param>
    /// <param name="newValuePredicate"></param>
    public SelfFillingDatabaseDictionary(BasePlugin plugin, Type type, Func<ulong, T>? newValuePredicate = null) :
        base(plugin.Bot.DatabaseClient, 
        (plugin.Bot.DatabaseClient.MakePluginTablePrefix(plugin) + plugin.Bot.DatabaseClient.GetTableName(type)).ToLower(), 
         plugin.Bot.DatabaseClient.GetPrimaryKey(type).ColumnName, 
         plugin.Bot.DatabaseClient.pluginDatabaseConnection, 
         newValuePredicate)
    {
    }

    /// <summary>
    /// Gets a value from this dictionary, filling in values if not already present.
    /// </summary>
    /// <param name="key">The key to get. Will not automatically fill if key is 0.</param>
    /// <returns></returns>
    public override T this[ulong key]
    {
        get
        {
            if (!this.ContainsKey(key) && key != 0)
            {
                if (_newValuePredicate is not null)
                {
                    _logger.LogTrace("Creating '{id}' of type '{type}'", key, typeof(T).Name);
                    this.Add(key, _newValuePredicate.Invoke(key));
                }
                else
                {
                    _logger.LogWarn("Creating '{id}' of type '{type}' with default value", key, typeof(T).Name);
                    this.Add(key, default);
                }
            }

            return base[key];
        }
    }
}
