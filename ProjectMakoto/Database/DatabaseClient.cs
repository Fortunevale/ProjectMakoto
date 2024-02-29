// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Database;

public sealed partial class DatabaseClient : RequiresBotReference
{
    private DatabaseClient(Bot bot) : base(bot)
    {
    }

    public class MySqlConnectionInformation
    {
        public Func<MySqlConnection> CreateConnection { get; set; }
    }

    internal MySqlConnectionInformation mainDatabaseConnection;
    internal MySqlConnectionInformation guildDatabaseConnection;
    internal MySqlConnectionInformation pluginDatabaseConnection;

    public bool Disposed { get; private set; } = false;

    internal static async Task<DatabaseClient> InitializeDatabase(Bot bot)
    {
        Log.Information("Connecting to database..");

        var databaseClient = new DatabaseClient(bot)
        {
            mainDatabaseConnection = new()
            {
                CreateConnection = () =>
                {
                    var conn = new MySqlConnection($"Server={bot.status.LoadedConfig.Secrets.Database.Host};" +
                                                   $"Port={bot.status.LoadedConfig.Secrets.Database.Port};" +
                                                   $"User Id={bot.status.LoadedConfig.Secrets.Database.Username};" +
                                                   $"Password={bot.status.LoadedConfig.Secrets.Database.Password};" +
                                                   $"Connection Timeout=60;" +
                                                   $"Connection Lifetime=30;" +
                                                   $"Database={bot.status.LoadedConfig.Secrets.Database.MainDatabaseName};");
                    return conn;
                }
            },

            guildDatabaseConnection = new()
            {
                CreateConnection = () =>
                {
                    var conn = new MySqlConnection($"Server={bot.status.LoadedConfig.Secrets.Database.Host};" +
                                                   $"Port={bot.status.LoadedConfig.Secrets.Database.Port};" +
                                                   $"User Id={bot.status.LoadedConfig.Secrets.Database.Username};" +
                                                   $"Password={bot.status.LoadedConfig.Secrets.Database.Password};" +
                                                   $"Connection Timeout=60;" +
                                                   $"Connection Lifetime=30;" +
                                                   $"Database={bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName};");
                    return conn;
                }
            }
        };

        if (bot.status.LoadedConfig.EnablePlugins)
            databaseClient.pluginDatabaseConnection = new()
            {
                CreateConnection = () =>
                {
                    var conn = new MySqlConnection($"Server={bot.status.LoadedConfig.Secrets.Database.Host};" +
                                                   $"Port={bot.status.LoadedConfig.Secrets.Database.Port};" +
                                                   $"User Id={bot.status.LoadedConfig.Secrets.Database.Username};" +
                                                   $"Password={bot.status.LoadedConfig.Secrets.Database.Password};" +
                                                   $"Connection Timeout=60;" +
                                                   $"Connection Lifetime=30;" +
                                                   $"Database={bot.status.LoadedConfig.Secrets.Database.PluginDatabaseName};");
                    return conn;
                }
            };

        await databaseClient.SyncStandardTable(new KeyValuePair<Type, string>[]
        {
            new(typeof(User), ""),
            new(typeof(Guild), ""),
            new(typeof(PhishingUrlEntry), ""),
            new(typeof(SubmittedUrlEntry), ""),
            new(typeof(GlobalNote), ""),
            new(typeof(BanDetails), "banned_guilds"),
            new(typeof(BanDetails), "banned_users"),
            new(typeof(BanDetails), "globalbans"),
            new(typeof(DatabaseULongList), "objected_users")
        }, databaseClient.mainDatabaseConnection);

        var remoteTables = databaseClient.ListTables(databaseClient.guildDatabaseConnection);
        foreach (var tableName in databaseClient.ListTables(databaseClient.guildDatabaseConnection).Where(x => x.IsDigitsOnly()))
        {
            var internalTable = typeof(Member);
            var propertyList = databaseClient.GetValidProperties(internalTable);

            if (!remoteTables.Contains(tableName))
            {
                Log.Warning("Missing table '{Name}'. Creating..", tableName);

                _ = databaseClient.CreateTable(tableName, internalTable, databaseClient.guildDatabaseConnection);
            }

            var remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);

            foreach (var internalColumn in propertyList)
            {
                (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo;

                try
                {
                    columnInfo = databaseClient.GetPropertyInfo(internalColumn);
                }
                catch (Exception)
                {
                    continue;
                }

                if (columnInfo.ColumnName is null || columnInfo.ColumnType == ColumnTypes.Unknown)
                    continue;

                if (!remoteColumns.Any(x => x.Name.ToLower() == columnInfo.ColumnName.ToLower()))
                {
                    Log.Warning("Missing column '{Column}' in '{Table}'. Creating..", columnInfo.ColumnName, tableName);

                    databaseClient.AddColumn(databaseClient.guildDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }

                var remoteColumn = remoteColumns.First(x => x.Name == columnInfo.ColumnName);

                var typeMatches = remoteColumn.Type.ToLower() == columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : "");
                var nullabilityMatches = remoteColumn.Nullable == columnInfo.Nullable;
                var defaultMatches = remoteColumn.Default == columnInfo.Default;

                if (!typeMatches || !nullabilityMatches || !defaultMatches)
                {
                    Log.Warning("Wrong data type for column '{Column}' in '{Table}'\nType: {TypeMatch} ({Type1}:{Type2})\nNullable: {NullableMatch}\nDefault: {Default} ({Default1}:{Default2})",
                        columnInfo.ColumnName,
                        tableName,
                        typeMatches, remoteColumn.Type.ToLower(), columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : ""),
                        nullabilityMatches,
                        defaultMatches, remoteColumn.Default, columnInfo.Default);

                    databaseClient.ModifyColumn(databaseClient.guildDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }
            }

            foreach (var remoteColumn in remoteColumns)
            {
                if (!propertyList.Any(x => x.GetCustomAttribute<ColumnNameAttribute>()?.Name == remoteColumn.Name))
                {
                    Log.Warning("Invalid column '{Column}' in '{Table}'", remoteColumn.Name, tableName);

                    databaseClient.DropColumn(databaseClient.guildDatabaseConnection, tableName, remoteColumn.Name);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }
            }
        }

        var pluginTables = new List<KeyValuePair<Type, string>>();

        if (bot.status.LoadedConfig.EnablePlugins)
            foreach (var plugin in bot.Plugins)
            {
                var prefix = databaseClient.MakePluginTablePrefix(plugin.Value);
                var currentPluginTables = await plugin.Value.RegisterTables();

                if (currentPluginTables.GroupBy(x => x.FullName).Any(x => x.Count() >= 2))
                    throw new Exception("You cannot use the same type twice.");
            
                if (currentPluginTables.GroupBy(x => databaseClient.GetTableName(x)).Any(x => x.Count() >= 2))
                    throw new Exception("You cannot use the same tablename twice.");

                if (currentPluginTables.Any(x => x.BaseType != typeof(Plugins.PluginDatabaseTable)))
                    throw new Exception("One or more types do not inherit PluginDatabaseTable");

                if (currentPluginTables.Any(x => x.GetCustomAttribute<TableNameAttribute>() == null))
                    throw new ArgumentException("One or more types is missing the TableNameAttribute");

                if (currentPluginTables.Any(x => databaseClient.GetValidProperties(x).Length == 0))
                    throw new ArgumentException("One or more types is missing a property with ColumnNameAttribute");

                try
                {
                    var primaryKeys = currentPluginTables.Select(x => databaseClient.GetPrimaryKey(x)).ToList();
                }
                catch (Exception)
                {
                    throw new ArgumentException("One or more types is missing a property with PrimaryAttribute");
                }

                foreach (var table in currentPluginTables)
                {
                    var requestedTableName = databaseClient.GetTableName(table);
                    var newTableName = $"{prefix}{requestedTableName}".ToLower();

                    if (!Regex.IsMatch(newTableName, @"^[a-zA-Z_][a-zA-Z0-9_$]*$", RegexOptions.Compiled))
                        throw new ArgumentException($"Created invalid table name: {newTableName} ({requestedTableName}). Please make sure that the name starts with a letter or an underscore and only contains letters, underscores, digits and dollar signs.");

                    if (pluginTables.Any(x => x.Value == newTableName))
                        throw new Exception($"The specified plugin tablename '{newTableName}' ({requestedTableName}) already exists.");

                    pluginTables.Add(new KeyValuePair<Type, string>(table, newTableName));
                    plugin.Value.AllowedTables.Add(newTableName);
                }
            }

        if (bot.status.LoadedConfig.EnablePlugins)
            await databaseClient.SyncStandardTable(pluginTables, databaseClient.pluginDatabaseConnection);

        bot.DatabaseClient = databaseClient;
        Log.Information("Connected to database.");
        return databaseClient;
    }

    internal string MakePluginTablePrefix(BasePlugin plugin)
    {
        return $"{Regex.Replace(plugin.Name.ToLower().Replace(" ", ""), @"[^\w]", "-", RegexOptions.Singleline | RegexOptions.Compiled)}_";
    }

    private void AddColumn(MySqlConnectionInformation connectionInfo, string tableName, PropertyInfo internalColumn, (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo)
    {
        using (var connection = connectionInfo.CreateConnection())
        {
            var sql = $"ALTER TABLE `{tableName}` ADD {this.Build(internalColumn)}";

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            _ = this.RunCommand(cmd);

            Log.Information("Created column '{Column}' in '{Table}'.", columnInfo.ColumnName, tableName);
        }
    }

    private void ModifyColumn(MySqlConnectionInformation connectionInfo, string tableName, PropertyInfo internalColumn, (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo)
    {
        using (var connection = connectionInfo.CreateConnection())
        {
            var sql = $"ALTER TABLE `{tableName}` CHANGE `{columnInfo.ColumnName}` {this.Build(internalColumn)}";

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            _ = this.RunCommand(cmd);

            Log.Information("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.",
                columnInfo.ColumnName,
                tableName,
                Enum.GetName(internalColumn.GetCustomAttribute<ColumnTypeAttribute>().Type).ToUpper());
        }
    }

    private void DropColumn(MySqlConnectionInformation connectionInfo, string tableName, string remoteColumn)
    {
        using (var connection = connectionInfo.CreateConnection())
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"ALTER TABLE `{tableName}` DROP COLUMN `{remoteColumn}`";

            _ = this.RunCommand(cmd);
        }
    }

    internal async Task SyncStandardTable(IEnumerable<KeyValuePair<Type, string>> localTables, MySqlConnectionInformation connection)
    {
        var remoteTables = this.ListTables(connection);

        foreach (var keyValue in localTables)
        {
            var internalTable = keyValue.Key;
            var tableName = keyValue.Value.IsNullOrWhiteSpace() ? internalTable.GetCustomAttribute<TableNameAttribute>().Name : keyValue.Value;
            var propertyList = this.GetValidProperties(internalTable);

            if (!remoteTables.Contains(tableName))
            {
                Log.Warning("Missing table '{Name}'. Creating..", tableName);

                _ = this.CreateTable(tableName, internalTable, connection);
            }

            var remoteColumns = this.ListColumns(tableName, connection);

            foreach (var internalColumn in propertyList)
            {
                (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo;

                try
                {
                    columnInfo = this.GetPropertyInfo(internalColumn);
                }
                catch (Exception)
                {
                    continue;
                }

                if (columnInfo.ColumnName is null || columnInfo.ColumnType == ColumnTypes.Unknown)
                    continue;

                if (!remoteColumns.Any(x => x.Name.ToLower() == columnInfo.ColumnName.ToLower()))
                {
                    Log.Warning("Missing column '{Column}' in '{Table}'. Creating..", columnInfo.ColumnName, tableName);

                    this.AddColumn(connection, tableName, internalColumn, columnInfo);
                    remoteColumns = this.ListColumns(tableName, connection);
                }

                var remoteColumn = remoteColumns.First(x => x.Name == columnInfo.ColumnName);

                var typeMatches = remoteColumn.Type.ToLower() == columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : "");
                var nullabilityMatches = remoteColumn.Nullable == columnInfo.Nullable;
                var defaultMatches = remoteColumn.Default == columnInfo.Default;

                if (!typeMatches || !nullabilityMatches || !defaultMatches)
                {
                    Log.Warning("Wrong data type for column '{Column}' in '{Table}'\nType: {TypeMatch} ({Type1}:{Type2})\nNullable: {NullableMatch}\nDefault: {Default} ({Default1}:{Default2})",
                        columnInfo.ColumnName,
                        tableName,
                        typeMatches, remoteColumn.Type.ToLower(), columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : ""),
                        nullabilityMatches,
                        defaultMatches, remoteColumn.Default, columnInfo.Default);

                    this.ModifyColumn(connection, tableName, internalColumn, columnInfo);
                    remoteColumns = this.ListColumns(tableName, connection);
                }
            }

            foreach (var remoteColumn in remoteColumns)
            {
                if (!propertyList.Any(x => x.GetCustomAttribute<ColumnNameAttribute>()?.Name == remoteColumn.Name))
                {
                    Log.Warning("Invalid column '{Column}' in '{Table}'", remoteColumn.Name, tableName);

                    this.DropColumn(connection, tableName, remoteColumn.Name);
                    remoteColumns = this.ListColumns(tableName, connection);
                }
            }
        }
    }

    /// <summary>
    /// Runs a command.
    /// </summary>
    /// <param name="cmd">The command to run.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal Task RunCommand(MySqlCommand cmd)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (cmd.Connection)
        {
            cmd.Connection.Open();

            Log.Verbose("Executing command on database '{database}': {command}", cmd.Connection.Database, cmd.CommandText.Truncate(300));

            _ = cmd.ExecuteNonQuery();

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Sets a value in the database.
    /// </summary>
    /// <param name="table">The table to set the value in.</param>
    /// <param name="columnKey">The unique column to look for.</param>
    /// <param name="columnValue">The unique value to look for.</param>
    /// <param name="columnToEdit">The column to edit.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="connection">The connection to use.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal Task SetValue(string table, string columnKey, object columnValue, string columnToEdit, object newValue, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (var connection = connectionInfo.CreateConnection())
            {
                connection.Open();

                if (newValue?.GetType() == typeof(bool))
                    newValue = (bool)newValue ? "1" : "0";

                if (newValue?.GetType() == typeof(DateTime))
                    newValue = ((DateTime)newValue).ToUniversalTime().Ticks;
                
                if (newValue?.GetType() == typeof(TimeSpan))
                    newValue = ((TimeSpan)newValue).TotalSeconds;

                string? v;

                if (newValue is null)
                    v = null;
                else
                    v = MySqlHelper.EscapeString(newValue?.ToString());

                var v1 = new MySqlCommand(Regex.IsMatch(v ?? " ", @"^(?=.*SELECT.*FROM)(?!.*(?:CREATE|DROP|UPDATE|INSERT|ALTER|DELETE|ATTACH|DETACH)).*$", RegexOptions.IgnoreCase)
                                        ? throw new InvalidOperationException("Sql detected.")
                                        : $"UPDATE `{table}` SET `{columnToEdit}`={(v is not null ? $"'{v}'" : "NULL")} WHERE `{columnKey}`='{columnValue}'", connection).ExecuteNonQuery();

                if (v1 != 1)
                    throw new InvalidOperationException($"Affected more or less than 1 row: {v1} rows affected")
                        .AddData("table", table)
                        .AddData("columnKey", columnKey)
                        .AddData("columnValue", columnValue)
                        .AddData("columnToEdit", columnToEdit)
                        .AddData("newValue", newValue)
                        .AddData("newValueEscaped", v)
                        .AddData("connection", connection);

                this.GetCache[$"{table}-{columnKey}-{columnValue}-{columnToEdit}"] = new CacheItem(null, DateTime.MinValue);

                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            var newEx = new AggregateException("Failed to update value in database", ex);
            throw newEx;
        }
    }

    internal ConcurrentDictionary<string, CacheItem> GetCache = new();
    internal record CacheItem(object item, DateTime CacheTime);

    internal T GetValue<T>(string tableName, string columnKey, object columnValue, string columnToGet, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        T BuildReturnItem(object input)
        {
            if (typeof(T) == typeof(bool))
                return (Convert.ToInt16(input) == 1) is T t ? t : throw new Exception("Impossible Exception.");

            if (typeof(T) == typeof(DateTime))
                return new DateTime(Convert.ToInt64(input), DateTimeKind.Utc) is T t ? t : throw new Exception("Impossible Exception.");

            if (typeof(T) == typeof(TimeSpan))
                return TimeSpan.FromSeconds(Convert.ToInt64(input)) is T t ? t : throw new Exception("Impossible Exception.");

            return (T)Convert.ChangeType(input, typeof(T));
        }

        try
        {
            if (this.GetCache.TryGetValue($"{tableName}-{columnKey}-{columnValue}-{columnToGet}", out var cacheItem) && cacheItem.CacheTime.GetTimespanSince() < TimeSpan.FromMilliseconds(2000))
            {
                return BuildReturnItem(cacheItem.item);
            }

            using (var connection = connectionInfo.CreateConnection())
            {
                connection.Open();

                var command = new MySqlCommand($"SELECT `{columnToGet}` FROM `{tableName}` WHERE `{columnKey}` = '{columnValue}'", connection);

                var reader = command.ExecuteReader();

                string? value = null;
                if (!reader.HasRows)
                    return default;

                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        break;

                    value = reader.GetValue(0).ToString();
                    break;
                }

                reader.Close();

                this.GetCache[$"{tableName}-{columnKey}-keys"] = new CacheItem(null, DateTime.MinValue);
                this.GetCache[$"{tableName}-{columnKey}-{columnValue}-{columnToGet}"] = new CacheItem(value, DateTime.UtcNow);

                return BuildReturnItem(value);
            }
        }
        catch (Exception ex)
        {
            var newEx = new AggregateException("Failed to get value from database", ex);
            throw newEx;
        }
    }

    internal bool CreateTable(string tableName, Type internalTable, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        if (this.ListTables(connectionInfo).Contains(tableName))
            return false;

        using (var connection = connectionInfo.CreateConnection())
        {
            var propertyList = this.GetValidProperties(internalTable);

            var sql = $"CREATE TABLE `{connection.Database}`.`{tableName}` " +
            $"( {string.Join(", ", propertyList.Select(x =>
            {
                if (x.GetCustomAttribute<ColumnNameAttribute>() is not null)
                    return this.Build(x);

                return string.Empty;
            }).Where(x => !x.IsNullOrWhiteSpace()))}" +
            $"{(propertyList.Any(x => x.GetCustomAttribute<PrimaryAttribute>() is not null) ?
                $", PRIMARY KEY (`{this.GetPrimaryKey(internalTable).ColumnName}`)" : "")} )";

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            _ = this.RunCommand(cmd);
            Log.Information("Created table '{Name}'.", tableName); 
        }

        return true;
    }

    internal Task CreateRow(string tableName, Type type, object uniqueValue, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            var primaryColumn = this.GetPrimaryKey(type);

            if (this.RowExists(tableName, primaryColumn.ColumnName, uniqueValue, connectionInfo))
                return Task.CompletedTask;

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"INSERT INTO `{tableName}` ( {string.Join(", ", this.GetValidProperties(type)
                .Where(x =>
                {
                    try
                    {
                        var propInfo = this.GetPropertyInfo(x);
                        return (!propInfo.Nullable) || propInfo.Primary;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                })
                .Select(x =>
                {
                    return $"{this.GetPropertyInfo(x).ColumnName}";
                }))} ) VALUES ( {string.Join(", ", this.GetValidProperties(type)
                .Where(x =>
                {
                    try
                    {
                        var propInfo = this.GetPropertyInfo(x);

                        return (!propInfo.Nullable) || propInfo.Primary;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                })
                .Select(x =>
                {
                    return $"@{this.GetPropertyInfo(x).ColumnName}";
                }))} )";

            foreach (var property in this.GetValidProperties(type).Where(x =>
                {
                    try
                    {
                        var propInfo = this.GetPropertyInfo(x);

                        return (!propInfo.Nullable) || propInfo.Primary;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }))
            {
                var value = this.GetPropertyInfo(property);

                if (value.Primary)
                    _ = cmd.Parameters.AddWithValue($"@{value.ColumnName}", uniqueValue);
                else
                    _ = cmd.Parameters.AddWithValue($"@{value.ColumnName}", value.Default ?? (property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null));
            }

            _ = this.RunCommand(cmd);
            this.GetCache[$"{tableName}-{primaryColumn.ColumnName}-keys"] = new CacheItem(null, DateTime.MinValue);
            this.GetCache[$"{tableName}-{primaryColumn.ColumnName}-{uniqueValue}-exists"] = new CacheItem(null, DateTime.MinValue);
            return Task.CompletedTask; 
        }
    }

    internal bool CreateRow(string tableName, string key, object value, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            if (this.RowExists(tableName, key, value, connectionInfo))
                return false;

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"INSERT INTO `{tableName}` ( {key} ) VALUES ( @value )";
            _ = cmd.Parameters.AddWithValue($"@value", value);

            _ = this.RunCommand(cmd);

            this.GetCache[$"{tableName}-{key}-keys"] = new CacheItem(null, DateTime.MinValue);
            this.GetCache[$"{tableName}-{key}-{value}-exists"] = new CacheItem(null, DateTime.MinValue);

            return true; 
        }
    }

    internal long GetRowCount(string tableName, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            connection.Open();

            var Count = 0L;
            using (var reader = connection.ExecuteReader($"SELECT COUNT(*) FROM `{tableName}`"))
            {
                while (reader.Read())
                {
                    Count = reader.GetInt64(0);
                }
            }

            return Count;
        }
    }

    internal T[] GetRowKeys<T>(string tableName, string columnKey, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        if (this.GetCache.TryGetValue($"{tableName}-{columnKey}-keys", out var cacheItem) && cacheItem.CacheTime.GetTimespanSince() < TimeSpan.FromMilliseconds(60000))
        {
            return (T[])cacheItem.item;
        }

        using (var connection = connectionInfo.CreateConnection())
        {
            connection.Open();

            var rows = new List<T>();
            using (var reader = connection.ExecuteReader($"SELECT `{columnKey}` FROM `{tableName}`"))
            {
                while (reader.Read())
                {
                    rows.Add((T)Convert.ChangeType(reader.GetValue(0), typeof(T)));
                }
            }

            var ts = rows.ToArray();
            this.GetCache[$"{tableName}-{columnKey}-keys"] = new CacheItem(ts, DateTime.UtcNow);
            return ts;
        }
    }

    internal bool RowExists(string tableName, string columnKey, object columnValue, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        if (this.GetCache.TryGetValue($"{tableName}-{columnKey}-{columnValue}-exists", out var cacheItem) && cacheItem.CacheTime.GetTimespanSince() < TimeSpan.FromMilliseconds(1000) && (bool)cacheItem.item)
        {
            return true;
        }

        using (var connection = connectionInfo.CreateConnection())
        {
            connection.Open();

            var Exists = false;
            using (var reader = connection.ExecuteReader($"SELECT EXISTS(SELECT 1 FROM `{tableName}` WHERE `{columnKey}` = '{columnValue}')"))
            {
                while (reader.Read())
                {
                    Exists = reader.GetInt16(0) >= 1;
                }
            }

            //Log.Debug("Column exists: {tableName}:{columnKey} = {columnValue}:{Exists}", tableName, columnKey, columnValue, Exists);
            this.GetCache[$"{tableName}-{columnKey}-{columnValue}-exists"] = new CacheItem(Exists, DateTime.UtcNow);
            return Exists;
        }
    }

    internal IEnumerable<string> ListTables(MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (var connection = connectionInfo.CreateConnection())
            {
                connection.Open();

                List<string> SavedTables = new();

                using (var reader = connection.ExecuteReader($"SHOW TABLES"))
                {
                    while (reader.Read())
                    {
                        SavedTables.Add(reader.GetString(0));
                    }
                }

                return SavedTables;
            }
        }
        catch (Exception)
        {
            Thread.Sleep(1000);
            return this.ListTables(connectionInfo);
        }
    }

    internal (string Name, string Type, bool Nullable, string Key, string? Default, string Extra)[] ListColumns(string tableName, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (var connection = connectionInfo.CreateConnection())
            {
                connection.Open();

                List<(string Name, string Type, bool Nullable, string Key, string Default, string Extra)> Columns = new();

                using (var reader = connection.ExecuteReader($"SHOW FIELDS FROM `{tableName}`"))
                {
                    while (reader.Read())
                    {
                        Columns.Add((reader.GetString(0), 
                            reader.GetString(1),
                            (!reader.IsDBNull(2) ? reader.GetString(2) : "").Equals("yes", StringComparison.CurrentCultureIgnoreCase), 
                            (!reader.IsDBNull(3) ? reader.GetString(3) : ""), 
                            (!reader.IsDBNull(4) ? (reader.GetString(4).Contains('\'') ? Regex.Match(reader.GetString(4), @"\\?'(.*)\\?'").Groups[1].Value : reader.GetString(4)) : null), 
                            (!reader.IsDBNull(5) ? reader.GetString(5) : "")));
                    }
                }

                return Columns.ToArray();
            }
        }
        catch (Exception)
        {
            Thread.Sleep(1000);
            return this.ListColumns(tableName, connectionInfo);
        }
    }

    internal Task DeleteRow(string tableName, string columnKey, string columnValue, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM `{tableName}` WHERE {columnKey}='{columnValue}'";
            cmd.Connection = connection;

            this.GetCache[$"{tableName}-{columnKey}-keys"] = new CacheItem(null, DateTime.MinValue);
            return this.RunCommand(cmd); 
        }
    }
    
    internal Task ClearRows(string tableName, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"TRUNCATE `{tableName}`";
            cmd.Connection = connection;

            foreach (var b in this.GetCache.Where(x => x.Key.StartsWith(tableName)))
                this.GetCache[b.Key] = new CacheItem(null, DateTime.MinValue);

            return this.RunCommand(cmd); 
        }
    }

    internal Task DropTable(string tableName, MySqlConnectionInformation connectionInfo)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (var connection = connectionInfo.CreateConnection())
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS `{tableName}`";
            cmd.Connection = connection;
            return this.RunCommand(cmd); 
        }
    }

    internal Task Dispose()
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        this.Disposed = true;
        return Task.CompletedTask;
    }

    private string Build(PropertyInfo info)
    {
        var columnInfo = this.GetPropertyInfo(info);

        var defaultText = string.Empty;

        if (columnInfo.Default is not null)
            switch (columnInfo.ColumnType)
            {
                case ColumnTypes.BigInt:
                case ColumnTypes.Int:
                case ColumnTypes.TinyInt:
                    defaultText = $"{columnInfo.Default}";
                    break;
                case ColumnTypes.LongText:
                case ColumnTypes.Text:
                    defaultText = $"('{columnInfo.Default}')";
                    break;
                case ColumnTypes.VarChar:
                    defaultText = $"'{columnInfo.Default}'";
                    break;
                default:
                    break;
            }

        if (!columnInfo.Nullable && columnInfo.Default is null && !columnInfo.Primary)
            throw new InvalidOperationException("A column should be either nullable, have a default value or be the primary key.")
                .AddData("columnInfo", columnInfo);

        return $"`{columnInfo.ColumnName}` {Enum.GetName(columnInfo.ColumnType).ToUpper()}" +
               $"{(columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : "")}" +
               $"{(columnInfo.Collation is not null ? $" CHARACTER SET {columnInfo.Collation[..columnInfo.Collation.IndexOf('_')]} COLLATE {columnInfo.Collation}" : "")}" +
               $"{(columnInfo.Nullable ? " NULL" : " NOT NULL")}" +
               $"{(columnInfo.Default is not null ? $" DEFAULT {defaultText}" : "")}";
    }
}
