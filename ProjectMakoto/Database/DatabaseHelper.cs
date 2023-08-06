// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database;

namespace ProjectMakoto.Database;
internal sealed class DatabaseHelper
{
    DatabaseClient _databaseClient { get; set; }

    internal DatabaseHelper(DatabaseClient client)
    {
        this._databaseClient = client;
    }

    public string GetLoadCommand(string table, string? propertyname = null)
    {
        return this._databaseClient.IsDisposed()
            ? throw new Exception("DatabaseHelper is disposed")
            : $"SELECT {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == (propertyname ?? table)).GetProperties().Select(x => x.Name))} FROM `{table}`";
    }

    public string GetSaveCommand(string table, string? propertyname = null)
    {
        return this._databaseClient.IsDisposed()
            ? throw new Exception("DatabaseHelper is disposed")
            : $"INSERT INTO `{table}` ( {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == (propertyname ?? table)).GetProperties().Select(x => x.Name))} ) VALUES ";
    }

    public string GetValueCommand(string propertyname, int i)
    {
        return this._databaseClient.IsDisposed()
            ? throw new Exception("DatabaseHelper is disposed")
            : $"( {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == propertyname).GetProperties().Select(x => $"@{x.Name}{i}"))} ), ";
    }

    public string GetOverwriteCommand(string propertyname)
    {
        return this._databaseClient.IsDisposed()
            ? throw new Exception("DatabaseHelper is disposed")
            : $" ON DUPLICATE KEY UPDATE {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == propertyname).GetProperties().Select(x => $"{x.Name}=values({x.Name})"))}";
    }

    public string GetUpdateValueCommand(string table, string columnKey, object rowKey, string columnToEdit, object newValue)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        if (newValue.GetType() == typeof(bool))
            newValue = ((bool)newValue ? "1" : "0");

        if (newValue.GetType() == typeof(DateTime))
            newValue = ((DateTime)newValue).ToUniversalTime().Ticks;

        var v = MySqlHelper.EscapeString(newValue.ToString());

        return Regex.IsMatch(v, @"^(?=.*SELECT.*FROM)(?!.*(?:CREATE|DROP|UPDATE|INSERT|ALTER|DELETE|ATTACH|DETACH)).*$", RegexOptions.IgnoreCase)
            ? throw new InvalidOperationException("Sql detected.")
            : $"UPDATE `{table}` SET `{columnToEdit}`='{v}' WHERE `{columnKey}`='{rowKey}'";
    }

    public async Task<IEnumerable<string>> ListTables(MySqlConnection connection)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        try
        {
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
        catch (Exception)
        {
            await Task.Delay(1000);
            return await this.ListTables(connection);
        }
    }

    public async Task<Dictionary<string, string>> ListColumns(MySqlConnection connection, string table)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        try
        {
            Dictionary<string, string> Columns = new();

            using (var reader = connection.ExecuteReader($"SHOW FIELDS FROM `{table}`"))
            {
                while (reader.Read())
                {
                    Columns.Add(reader.GetString(0), reader.GetString(1));
                }
            }

            return Columns;
        }
        catch (Exception)
        {
            await Task.Delay(1000);
            return await this.ListColumns(connection, table);
        }
    }

    public async Task DeleteRow(MySqlConnection connection, string table, string row_match, string value)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM `{table}` WHERE {row_match}='{value}'";
        cmd.Connection = connection;
        await this._databaseClient._queue.RunCommand(cmd);
    }

    public async Task DropTable(MySqlConnection connection, string table)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP TABLE IF EXISTS `{table}`";
        cmd.Connection = connection;
        await this._databaseClient._queue.RunCommand(cmd);
    }

    public async Task SelectDatabase(MySqlConnection connection, string databaseName, bool CreateIfNotExist = false)
    {
        if (this._databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        if (CreateIfNotExist)
            _ = await connection.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS {databaseName}");

        await connection.ChangeDatabaseAsync(databaseName);
    }
}
