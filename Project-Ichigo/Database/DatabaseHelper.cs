using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Ichigo.Database;
internal class DatabaseHelper
{
    DatabaseClient _databaseClient { get; set; }

    internal DatabaseHelper(DatabaseClient client)
    {
        this._databaseClient = client;
    }

    public string GetLoadCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"SELECT {string.Join(", ", columns.Select(x => x.Name))} FROM `{table}`";
    }

    public string GetSaveCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"INSERT INTO `{table}` ( {string.Join(", ", columns.Select(x => x.Name))} ) VALUES ";
    }

    public string GetValueCommand(List<DatabaseColumnLists.Column> columns, int i)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $"( {string.Join(", ", columns.Select(x => $"@{x.Name}{i}"))} ), ";
    }

    public string GetOverwriteCommand(List<DatabaseColumnLists.Column> columns)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        return $" ON DUPLICATE KEY UPDATE {string.Join(", ", columns.Select(x => $"{x.Name}=values({x.Name})"))}";
    }

    public async Task<IEnumerable<string>> ListTables(MySqlConnection connection)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

        List<string> SavedTables = new();

        using (IDataReader reader = connection.ExecuteReader($"SHOW TABLES"))
        {
            while (reader.Read())
            {
                SavedTables.Add(reader.GetString(0));
            }
        }

        return SavedTables as IEnumerable<string>;
    }

    public async Task<Dictionary<string, string>> ListColumns(MySqlConnection connection, string table)
    {
        if (_databaseClient.IsDisposed())
            throw new Exception("DatabaseHelper is disposed");

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
