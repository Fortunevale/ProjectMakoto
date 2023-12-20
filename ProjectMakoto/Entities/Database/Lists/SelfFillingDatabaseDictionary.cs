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
public sealed class SelfFillingDatabaseDictionary<T>(DatabaseClient client, string tableName, string primaryKey, bool useGuildConnection, Func<ulong, T>? newValuePredicate = null) : DatabaseDictionary<ulong, T>(client, tableName, primaryKey, useGuildConnection, newValuePredicate)
{

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
