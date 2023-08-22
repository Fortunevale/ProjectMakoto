// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;
using ProjectMakoto.Entities.Database;
using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Database.ColumnTypes;

namespace ProjectMakoto.Database;

public sealed class DatabaseClient : RequiresBotReference
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
    internal DatabaseHelper _helper { get; private set; }

    private bool Disposed { get; set; } = false;

    internal static async Task<DatabaseClient> InitializeDatabase(Bot bot)
    {
        _logger.LogInfo("Connecting to database..");

        var databaseClient = new DatabaseClient(bot);
        databaseClient._helper = new(databaseClient);

        databaseClient.mainDatabaseConnection.Open();
        databaseClient.guildDatabaseConnection.Open();

        try
        {
            var remoteTables = await databaseClient._helper.ListTables(databaseClient.mainDatabaseConnection);

            foreach (var internalTable in TableDefinitions.TableList)
            {
                var tableName = internalTable.GetCustomAttribute<TableNameAttribute>().Name;
                var propertyList = new List<PropertyInfo>();

                void AddProperties(Type type)
                {
                    foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (property.GetCustomAttribute<ColumnNameAttribute>() is null &&
                            property.GetCustomAttribute<ColumnType>() is null &&
                            property.GetCustomAttribute<MaxValueAttribute>() is null &&
                            property.GetCustomAttribute<CollationAttribute>() is null &&
                            property.GetCustomAttribute<NullableAttribute>() is null &&
                            property.GetCustomAttribute<ContainsValuesAttribute>() is null)
                            continue;

                        propertyList.Add(property);

                        if (property.GetCustomAttribute<ContainsValuesAttribute>() is not null)
                        {
                            AddProperties(property.PropertyType);
                        }
                    }
                }

                AddProperties(internalTable);

                string Build(PropertyInfo info)
                {
                    var columnName = info.GetCustomAttribute<ColumnNameAttribute>();
                    var propertyType = info.GetCustomAttribute<ColumnType>();
                    var maxValue = info.GetCustomAttribute<MaxValueAttribute>();
                    var collation = info.GetCustomAttribute<CollationAttribute>();
                    var nullable = info.GetCustomAttribute<NullableAttribute>();

                    if (columnName is null || propertyType is null)
                        return string.Empty;

                    return $"`{columnName.Name}` {Enum.GetName(propertyType.Type).ToUpper()}" +
                           $"{(maxValue is not null ? $"({maxValue.MaxValue})" : "")}" +
                           $"{(collation is not null ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}" +
                           $"{(nullable is not null ? " NULL" : " NOT NULL")}";
                }

                if (!remoteTables.Contains(tableName))
                {
                    _logger.LogWarn("Missing table '{Name}'. Creating..", tableName);

                    var sql = $"CREATE TABLE `{bot.status.LoadedConfig.Secrets.Database.MainDatabaseName}`.`{tableName}` " +
                        $"( {string.Join(", ", propertyList.Select(x =>
                        {
                            if (x.GetCustomAttribute<ColumnNameAttribute>() is not null)
                                return Build(x);

                            return string.Empty;
                        }))} )" + 
                        $"{(propertyList.Any(x => x.GetCustomAttribute<PrimaryAttribute>() is not null) ?
                            $", PRIMARY KEY (`{propertyList.First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")}";

                    var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = databaseClient.mainDatabaseConnection;

                    await databaseClient.RunCommand(cmd);
                    _logger.LogInfo("Created table '{Name}'.", tableName);
                }

                var remoteColumns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, tableName);

                foreach (var internalColumn in propertyList)
                {
                    var columnName = internalColumn.GetCustomAttribute<ColumnNameAttribute>()?.Name;
                    var columnType = Enum.GetName(internalColumn.GetCustomAttribute<ColumnType>()?.Type ?? ColumnTypes.Unknown);

                    if (columnName is null || columnType == "Unknown")
                        continue;

                    if (!remoteColumns.ContainsKey(columnName.ToLower()))
                    {
                        _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", columnName, tableName);
                        var sql = $"ALTER TABLE `{tableName}` ADD {Build(internalColumn)}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient.RunCommand(cmd);

                        _logger.LogInfo("Created column '{Column}' in '{Table}'.", columnName, tableName);
                        remoteColumns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, tableName);
                    }

                    if (remoteColumns[columnName].ToLower() != columnType.ToLower() + (internalColumn.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
                    {
                        _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'", columnName, tableName);
                        var sql = $"ALTER TABLE `{tableName}` CHANGE `{columnName}` {Build(internalColumn)}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient.RunCommand(cmd);

                        _logger.LogInfo("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.", 
                            columnName,
                            tableName,
                            Enum.GetName(internalColumn.GetCustomAttribute<ColumnType>().Type).ToUpper());

                        remoteColumns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, tableName);
                    }
                }

                foreach (var remoteColumn in remoteColumns)
                {
                    if (!propertyList.Any(x => x.GetCustomAttribute<ColumnNameAttribute>()?.Name == remoteColumn.Key))
                    {
                        _logger.LogWarn("Invalid column '{Column}' in '{Table}'", remoteColumn.Key, tableName);

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{tableName}` DROP COLUMN `{remoteColumn.Key}`";
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient.RunCommand(cmd);
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        //await databaseClient.CheckGuildTables();

        bot.DatabaseClient = databaseClient;
        _logger.LogInfo("Connected to database.");
        return databaseClient;
    }

    //internal async Task CheckGuildTables()
    //{
    //    IEnumerable<string> GuildTables;

    //    var retries = 1;

    //    while (true)
    //    {
    //        try
    //        {
    //            GuildTables = await this._helper.ListTables(this.guildDatabaseConnection);
    //            break;
    //        }
    //        catch (Exception ex)
    //        {
    //            if (retries >= 3)
    //            {
    //                throw;
    //            }

    //            _logger.LogWarn("Failed to get a list of guild tables. Retrying in 1000ms.. ({current}/{max})", ex, retries, 3);
    //            retries++;
    //            await Task.Delay(1000);
    //        }
    //    }

    //    foreach (var b in this.Bot.Guilds)
    //    {
    //        if (!GuildTables.Contains($"{b.Key}"))
    //        {
    //            _logger.LogWarn("Missing table '{Guild}'. Creating..", b.Key);
    //            var sql = $"CREATE TABLE `{this.Bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", typeof(TableDefinitions.guild_users).GetProperties().Select(x => $"`{x.Name}` {x.PropertyType.Name.ToUpper()}{(x.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : "")}{(x.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(x.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out _) ? " NULL" : " NOT NULL")}"))}{(typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)) ? $", PRIMARY KEY (`{typeof(TableDefinitions.guild_users).GetProperties().First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")})";

    //            var cmd = this.guildDatabaseConnection.CreateCommand();
    //            cmd.CommandText = sql;
    //            cmd.Connection = this.guildDatabaseConnection;

    //            await this.RunCommand(cmd);
    //            _logger.LogInfo("Created table '{Guild}'.", b.Key);
    //        }
    //    }

    //    GuildTables = await this._helper.ListTables(this.guildDatabaseConnection);

    //    foreach (var b in GuildTables)
    //    {
    //        if (b != "writetester")
    //        {
    //            var Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);

    //            foreach (var col in typeof(TableDefinitions.guild_users).GetProperties())
    //            {
    //                if (!Columns.ContainsKey(col.Name))
    //                {
    //                    _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", col.Name, b);
    //                    var sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue1) ? $"({maxvalue1.MaxValue})" : "")}{col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

    //                    var cmd = this.guildDatabaseConnection.CreateCommand();
    //                    cmd.CommandText = sql;
    //                    cmd.Connection = this.guildDatabaseConnection;

    //                    await this.RunCommand(cmd);

    //                    _logger.LogInfo("Created column '{Column}' in '{Table}'.", col.Name, b);
    //                    Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);
    //                }

    //                if (Columns[col.Name].ToLower() != col.PropertyType.Name.ToLower() + (col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
    //                {
    //                    _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'", col.Name, b);
    //                    var sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.PropertyType.Name.ToUpper()}{(maxvalue is not null ? $"({maxvalue.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

    //                    var cmd = this.guildDatabaseConnection.CreateCommand();
    //                    cmd.CommandText = sql;
    //                    cmd.Connection = this.guildDatabaseConnection;

    //                    await this.RunCommand(cmd);

    //                    _logger.LogInfo("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.", col.Name, b, col.PropertyType.Name.ToUpper());
    //                    Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);
    //                }
    //            }

    //            foreach (var col in Columns)
    //            {
    //                if (!typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.Name == col.Key))
    //                {
    //                    _logger.LogWarn("Invalid column '{Column}' in '{Table}'", col.Key, b);

    //                    var cmd = this.guildDatabaseConnection.CreateCommand();
    //                    cmd.CommandText = $"ALTER TABLE `{b}` DROP COLUMN `{col.Key}`";
    //                    cmd.Connection = this.guildDatabaseConnection;

    //                    await this.RunCommand(cmd);
    //                }
    //            }
    //        }
    //    }
    //}

    public Task RunCommand(MySqlCommand cmd)
    {
        using (cmd.Connection)
        {
            cmd.Connection.Open();
            _ = cmd.ExecuteNonQuery();

            return Task.CompletedTask;
        }
    }

    public Task SetValue(string table, string columnKey, object columnValue, string columnToEdit, object newValue, MySqlConnection connection)
    {
        try
        {
            using (connection)
            {
                if (table != "users")
                    return Task.CompletedTask;

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

    public T GetValue<T>(string table, string columnKey, object columnValue, string columnToGet, MySqlConnection connection)
    {
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

                if (GetCache.TryGetValue($"{table}-{columnKey}-{columnValue}-{columnToGet}", out var cacheItem) && cacheItem.CacheTime.GetTimespanSince() < TimeSpan.FromSeconds(1))
                {
                    return BuildReturnItem(cacheItem.item);
                }

                connection.Open();

                var command = new MySqlCommand($"SELECT `{columnToGet}` FROM `{table}` WHERE `{columnKey}` = '{columnValue}'", connection);
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

                GetCache[$"{table}-{columnKey}-{columnValue}-{columnToGet}"] = new CacheItem(value, DateTime.UtcNow);

                return BuildReturnItem(value);
            }
        }
        catch (Exception ex)
        {
            var newEx = new AggregateException("Failed to get value from database", ex);
            throw newEx;
        }
    }

    public async Task Dispose()
    {
        this.Disposed = true;
        await this.mainDatabaseConnection.CloseAsync();
    }

    public bool IsDisposed()
    {
        return this.Disposed;
    }
}
