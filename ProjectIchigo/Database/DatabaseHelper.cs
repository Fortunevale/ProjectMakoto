using ProjectIchigo.Entities.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectIchigo.Database;
internal class DatabaseHelper
{
    DatabaseClient _databaseClient { get; set; }

    internal DatabaseHelper(DatabaseClient client)
    {
        this._databaseClient = client;
    }

    public string GetLoadCommand(string table, string? propertyname = null)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"SELECT {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == (propertyname ?? table)).GetProperties().Select(x => x.Name))} FROM `{table}`";
    }

    public string GetSaveCommand(string table, string? propertyname = null)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"INSERT INTO `{table}` ( {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == (propertyname ?? table)).GetProperties().Select(x => x.Name))} ) VALUES ";
    }

    public string GetValueCommand(string propertyname, int i)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"( {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == propertyname).GetProperties().Select(x => $"@{x.Name}{i}"))} ), ";
    }

    public string GetOverwriteCommand(string propertyname)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $" ON DUPLICATE KEY UPDATE {string.Join(", ", typeof(TableDefinitions).GetNestedTypes().First(x => x.Name == propertyname).GetProperties().Select(x => $"{x.Name}=values({x.Name})"))}";
    }

    public string GetUpdateValueCommand(string table, string columnKey, object rowKey, string columnToEdit, object newValue)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        if (newValue.GetType() == typeof(bool))
            newValue = ((bool)newValue ? "1" : "0");

        if (newValue.GetType() == typeof(DateTime))
            newValue = ((DateTime)newValue).ToUniversalTime().Ticks;

        var v = MySqlHelper.EscapeString(newValue.ToString());

        if (Regex.IsMatch(v, @"^(?=.*SELECT.*FROM)(?!.*(?:CREATE|DROP|UPDATE|INSERT|ALTER|DELETE|ATTACH|DETACH)).*$", RegexOptions.IgnoreCase))
            throw new InvalidOperationException("Sql detected.");

        return $"UPDATE `{table}` SET `{columnToEdit}`='{v}' WHERE `{columnKey}`='{rowKey}'";
    }

    public async Task<IEnumerable<string>> ListTables(MySqlConnection connection)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        try
        {
            List<string> SavedTables = new();

            using (IDataReader reader = connection.ExecuteReader($"SHOW TABLES"))
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
            return await ListTables(connection);
        }
    }

    public async Task<Dictionary<string, string>> ListColumns(MySqlConnection connection, string table)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        try
        {
            Dictionary<string, string> Columns = new();

            using (IDataReader reader = connection.ExecuteReader($"SHOW FIELDS FROM `{table}`"))
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
            return await ListColumns(connection, table);
        }
    }

    public async Task DeleteRow(MySqlConnection connection, string table, string row_match, string value)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM `{table}` WHERE {row_match}='{value}'";
        cmd.Connection = connection;
        await _databaseClient._queue.RunCommand(cmd);
    }

    public async Task DropTable(MySqlConnection connection, string table)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP TABLE IF EXISTS `{table}`";
        cmd.Connection = connection;
        await _databaseClient._queue.RunCommand(cmd);
    }

    public async Task SelectDatabase(MySqlConnection connection, string databaseName, bool CreateIfNotExist = false)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        if (CreateIfNotExist)
            await connection.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS {databaseName}");

        await connection.ChangeDatabaseAsync(databaseName);
    }
}
