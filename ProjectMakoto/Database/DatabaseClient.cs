// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Database;

internal sealed partial class DatabaseClient : RequiresBotReference
{
    private DatabaseClient(Bot bot) : base(bot)
    {
    }

    internal MySqlConnection mainDatabaseConnection
    {
        get
        {
            var conn = new MySqlConnection($"Server={this.Bot.status.LoadedConfig.Secrets.Database.Host};" +
                                           $"Port={this.Bot.status.LoadedConfig.Secrets.Database.Port};" +
                                           $"User Id={this.Bot.status.LoadedConfig.Secrets.Database.Username};" +
                                           $"Password={this.Bot.status.LoadedConfig.Secrets.Database.Password};" +
                                           $"Connection Timeout=60;" +
                                           $"Connection Lifetime=30;" +
                                           $"Database={this.Bot.status.LoadedConfig.Secrets.Database.MainDatabaseName};");
            return conn;
        }
    }
    internal MySqlConnection guildDatabaseConnection
    {
        get
        {
            var conn = new MySqlConnection($"Server={this.Bot.status.LoadedConfig.Secrets.Database.Host};" +
                                           $"Port={this.Bot.status.LoadedConfig.Secrets.Database.Port};" +
                                           $"User Id={this.Bot.status.LoadedConfig.Secrets.Database.Username};" +
                                           $"Password={this.Bot.status.LoadedConfig.Secrets.Database.Password};" +
                                           $"Connection Timeout=60;" +
                                           $"Database={this.Bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName};");
            return conn;
        }
    }
    public bool Disposed { get; private set; } = false;

    internal static async Task<DatabaseClient> InitializeDatabase(Bot bot)
    {
        _logger.LogInfo("Connecting to database..");

        var databaseClient = new DatabaseClient(bot);

        void AddColumn(MySqlConnection connection, string tableName, PropertyInfo internalColumn, (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo)
        {
            var sql = $"ALTER TABLE `{tableName}` ADD {databaseClient.Build(internalColumn)}";

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            _ = databaseClient.RunCommand(cmd);

            _logger.LogInfo("Created column '{Column}' in '{Table}'.", columnInfo.ColumnName, tableName);
        }

        void ModifyColumn(MySqlConnection connection, string tableName, PropertyInfo internalColumn, (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) columnInfo)
        {
            var sql = $"ALTER TABLE `{tableName}` CHANGE `{columnInfo.ColumnName}` {databaseClient.Build(internalColumn)}";

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            _ = databaseClient.RunCommand(cmd);

            _logger.LogInfo("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.",
                columnInfo.ColumnName,
                tableName,
                Enum.GetName(internalColumn.GetCustomAttribute<ColumnType>().Type).ToUpper());
        }

        void DropColumn(MySqlConnection connection, string tableName, string remoteColumn)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"ALTER TABLE `{tableName}` DROP COLUMN `{remoteColumn}`";

            _ = databaseClient.RunCommand(cmd);
        }

        var remoteTables = databaseClient.ListTables(databaseClient.mainDatabaseConnection);

        foreach (var internalTable in new Type[]
        {
                typeof(User),
                typeof(Guild),
        })
        {
            var tableName = internalTable.GetCustomAttribute<TableNameAttribute>().Name;
            var propertyList = databaseClient.GetValidProperties(internalTable);

            if (!remoteTables.Contains(tableName))
            {
                _logger.LogWarn("Missing table '{Name}'. Creating..", tableName);

                _ = databaseClient.CreateTable(tableName, internalTable, databaseClient.mainDatabaseConnection);
            }

            var remoteColumns = databaseClient.ListColumns(tableName, databaseClient.mainDatabaseConnection);

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
                    _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", columnInfo.ColumnName, tableName);

                    AddColumn(databaseClient.mainDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.mainDatabaseConnection);
                }

                var remoteColumn = remoteColumns.First(x => x.Name == columnInfo.ColumnName);

                var typeMatches = remoteColumn.Type.ToLower() == columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : "");
                var nullabilityMatches = remoteColumn.Nullable == columnInfo.Nullable;
                var defaultMatches = remoteColumn.Default == columnInfo.Default;

                if (!typeMatches || !nullabilityMatches || !defaultMatches)
                {
                    _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'\nType: {TypeMatch}\nNullable: {NullableMatch}\nDefault: {Default}",
                        columnInfo.ColumnName, tableName, typeMatches, nullabilityMatches, defaultMatches);

                    ModifyColumn(databaseClient.mainDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.mainDatabaseConnection);
                }
            }

            foreach (var remoteColumn in remoteColumns)
            {
                if (!propertyList.Any(x => x.GetCustomAttribute<ColumnNameAttribute>()?.Name == remoteColumn.Name))
                {
                    _logger.LogWarn("Invalid column '{Column}' in '{Table}'", remoteColumn.Name, tableName);

                    DropColumn(databaseClient.mainDatabaseConnection, tableName, remoteColumn.Name);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.mainDatabaseConnection);
                }
            }
        }


        remoteTables = databaseClient.ListTables(databaseClient.guildDatabaseConnection);
        foreach (var tableName in databaseClient.ListTables(databaseClient.guildDatabaseConnection).Where(x => x.IsDigitsOnly()))
        {
            var internalTable = typeof(Member);
            var propertyList = databaseClient.GetValidProperties(internalTable);

            if (!remoteTables.Contains(tableName))
            {
                _logger.LogWarn("Missing table '{Name}'. Creating..", tableName);

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
                    _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", columnInfo.ColumnName, tableName);

                    AddColumn(databaseClient.guildDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }

                var remoteColumn = remoteColumns.First(x => x.Name == columnInfo.ColumnName);

                var typeMatches = remoteColumn.Type.ToLower() == columnInfo.ColumnType.GetName().ToLower() + (columnInfo.MaxValue is not null ? $"({columnInfo.MaxValue})" : "");
                var nullabilityMatches = remoteColumn.Nullable == columnInfo.Nullable;
                var defaultMatches = remoteColumn.Default == columnInfo.Default;

                if (!typeMatches || !nullabilityMatches || !defaultMatches)
                {
                    _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'\nType: {TypeMatch}\nNullable: {NullableMatch}\nDefault: {Default}",
                        columnInfo.ColumnName, tableName, typeMatches, nullabilityMatches, defaultMatches);

                    ModifyColumn(databaseClient.guildDatabaseConnection, tableName, internalColumn, columnInfo);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }
            }

            foreach (var remoteColumn in remoteColumns)
            {
                if (!propertyList.Any(x => x.GetCustomAttribute<ColumnNameAttribute>()?.Name == remoteColumn.Name))
                {
                    _logger.LogWarn("Invalid column '{Column}' in '{Table}'", remoteColumn.Name, tableName);

                    DropColumn(databaseClient.guildDatabaseConnection, tableName, remoteColumn.Name);
                    remoteColumns = databaseClient.ListColumns(tableName, databaseClient.guildDatabaseConnection);
                }
            }
        }

        //await databaseClient.CheckGuildTables();

        bot.DatabaseClient = databaseClient;
        _logger.LogInfo("Connected to database.");
        return databaseClient;
    }

    /// <summary>
    /// Runs a command.
    /// </summary>
    /// <param name="cmd">The command to run.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task RunCommand(MySqlCommand cmd)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (cmd.Connection)
        {
            cmd.Connection.Open();

            _logger.LogDebug("Executing command on database '{database}': {command}", cmd.Connection.Database, cmd.CommandText);

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
    public Task SetValue(string table, string columnKey, object columnValue, string columnToEdit, object newValue, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (connection)
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

                _logger.LogDebug("Setting value column '{ColumnToEdit}' of '{ColumnKey}:{ColumnValue}' to '{newValue}'", columnToEdit, columnKey, columnValue, v ?? "NULL");
                if (new MySqlCommand(Regex.IsMatch(v ?? " ", @"^(?=.*SELECT.*FROM)(?!.*(?:CREATE|DROP|UPDATE|INSERT|ALTER|DELETE|ATTACH|DETACH)).*$", RegexOptions.IgnoreCase)
                        ? throw new InvalidOperationException("Sql detected.")
                        : $"UPDATE `{table}` SET `{columnToEdit}`={(v is not null ? $"'{v}'" : "NULL")} WHERE `{columnKey}`='{columnValue}'", connection).ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Affected more or less than 1 row")
                        .AddData("table", table)
                        .AddData("columnKey", columnKey)
                        .AddData("columnValue", columnValue)
                        .AddData("columnToEdit", columnToEdit)
                        .AddData("newValue", newValue)
                        .AddData("connection", connection);

                GetCache[$"{table}-{columnKey}-{columnValue}-{columnToEdit}"] = new CacheItem(null, DateTime.MinValue);

                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            var newEx = new AggregateException("Failed to update value in database", ex);
            throw newEx;
        }
    }

    public ConcurrentDictionary<string, CacheItem> GetCache = new();
    public record CacheItem(string item, DateTime CacheTime);

    public T GetValue<T>(string tableName, string columnKey, object columnValue, string columnToGet, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (connection)
            {
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

                if (GetCache.TryGetValue($"{tableName}-{columnKey}-{columnValue}-{columnToGet}", out var cacheItem) && cacheItem.CacheTime.GetTimespanSince() < TimeSpan.FromMilliseconds(2000))
                {
                    _logger.LogDebug("Getting cached value column '{columnToGet}' of '{ColumnKey}:{ColumnValue}'", columnToGet, columnKey, columnValue);

                    return BuildReturnItem(cacheItem.item);
                }

                connection.Open();

                var command = new MySqlCommand($"SELECT `{columnToGet}` FROM `{tableName}` WHERE `{columnKey}` = '{columnValue}'", connection);
                _logger.LogDebug("Getting value column '{columnToGet}' of '{ColumnKey}:{ColumnValue}'", columnToGet, columnKey, columnValue);

                var reader = command.ExecuteReader();

                string? value = null;
                if (!reader.HasRows)
                    return default;

                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        break;

                    value = reader.GetString(0);
                    break;
                }

                reader.Close();

                GetCache[$"{tableName}-{columnKey}-{columnValue}-{columnToGet}"] = new CacheItem(value, DateTime.UtcNow);

                return BuildReturnItem(value);
            }
        }
        catch (Exception ex)
        {
            var newEx = new AggregateException("Failed to get value from database", ex);
            throw newEx;
        }
    }

    public bool CreateTable(string tableName, Type internalTable, MySqlConnection connection)
    {
        if (this.ListTables(connection).Contains(tableName))
            return false;

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
        _logger.LogInfo("Created table '{Name}'.", tableName);

        return true;
    }

    public bool CreateRow(string tableName, Type type, object uniqueValue, MySqlConnection connection)
    {
        if (this.RowExists(tableName, this.GetPrimaryKey(type).ColumnName, uniqueValue, connection))
            return false;

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

        cmd.Connection = connection;

        _ = this.RunCommand(cmd);
        return true;
    }

    public bool RowExists(string tableName, string columnKey, object columnValue, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        using (connection)
        {
            connection.Open();

            var Exists = false;
            using (var reader = connection.ExecuteReader($"SELECT EXISTS(SELECT * FROM `{tableName}` WHERE `{columnKey}` = '{columnValue}')"))
            {
                while (reader.Read())
                {
                    Exists = reader.GetInt16(0) == 1;
                }
            }

            _logger.LogDebug("Column exists: {tableName}:{columnKey} = {columnValue}:{Exists}", tableName, columnKey, columnValue, Exists);
            return Exists;
        }
    }

    public IEnumerable<string> ListTables(MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (connection)
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
            return this.ListTables(connection);
        }
    }

    public (string Name, string Type, bool Nullable, string Key, string? Default, string Extra)[] ListColumns(string tableName, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        try
        {
            using (connection)
            {
                connection.Open();

                List<(string Name, string Type, bool Nullable, string Key, string Default, string Extra)> Columns = new();

                using (var reader = connection.ExecuteReader($"SHOW FIELDS FROM `{tableName}`"))
                {
                    while (reader.Read())
                    {
                        Columns.Add((reader.GetString(0), 
                            reader.GetString(1), 
                            (!reader.IsDBNull(2) ? reader.GetString(2) : "").ToLower() == "yes", 
                            (!reader.IsDBNull(3) ? reader.GetString(3) : ""), 
                            (!reader.IsDBNull(4) ? (reader.GetString(4).Contains("\\'") ? Regex.Match(reader.GetString(4), @"\\'(.*)\\'").Groups[1].Value : reader.GetString(4)) : null), 
                            (!reader.IsDBNull(5) ? reader.GetString(5) : "")));
                    }
                }

                return Columns.ToArray();
            }
        }
        catch (Exception)
        {
            Thread.Sleep(1000);
            return this.ListColumns(tableName, connection);
        }
    }

    public Task DeleteRow(string tableName, string columnKey, string columnValue, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM `{tableName}` WHERE {columnKey}='{columnValue}'";
        cmd.Connection = connection;
        return this.RunCommand(cmd);
    }

    public Task DropTable(string tableName, MySqlConnection connection)
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP TABLE IF EXISTS `{tableName}`";
        cmd.Connection = connection;
        return this.RunCommand(cmd);
    }

    public Task Dispose()
    {
        if (this.Disposed)
            throw new Exception("DatabaseClient is disposed");

        this.Disposed = true;
        this.mainDatabaseConnection.Close();

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
               $"{(columnInfo.Collation is not null ? $" CHARACTER SET {columnInfo.Collation[..columnInfo.Collation.IndexOf("_")]} COLLATE {columnInfo.Collation}" : "")}" +
               $"{(columnInfo.Nullable ? " NULL" : " NOT NULL")}" +
               $"{(columnInfo.Default is not null ? $" DEFAULT {defaultText}" : "")}";
    }
}
