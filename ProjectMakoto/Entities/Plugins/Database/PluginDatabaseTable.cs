// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;

[System.Reflection.Obfuscation]
public class PluginDatabaseTable : RequiresBotReference
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Prevents creation without proper data")]
    private PluginDatabaseTable(BasePlugin plugin) : base(plugin.Bot) { }

    public PluginDatabaseTable(BasePlugin plugin, object identifierValue) : base(plugin.Bot)
    {
        this.Plugin = plugin;

        this.Validate();

        if (identifierValue.GetType() == typeof(ulong))
            if (this.Plugin.HasUserObjected((ulong)identifierValue))
                throw new InvalidOperationException($"User {identifierValue} has objected to having their data processed.");

        this.CreateRow(identifierValue).GetAwaiter().GetResult();
    }

    protected BasePlugin Plugin { get; private set; }

    /// <summary>
    /// Creates a new row in the type's table.
    /// </summary>
    /// <param name="plugin">Your plugin instance.</param>
    /// <param name="identifierValue">The value that identifies this key, typically a user, channel or guild id.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task CreateRow(object identifierValue)
    {
        var type = this.GetType();
        var tableName = (this.Bot.DatabaseClient.MakePluginTablePrefix(this.Plugin) + type.GetCustomAttribute<TableNameAttribute>().Name).ToLower();
        return this.Plugin.Bot.DatabaseClient.CreateRow(tableName, type, identifierValue, this.Plugin.Bot.DatabaseClient.pluginDatabaseConnection);
    }

    /// <summary>
    /// Gets a value from this tables columns.
    /// </summary>
    /// <typeparam name="T">What this value is supposed to be.</typeparam>
    /// <param name="plugin">Your plugin instance.</param>
    /// <param name="identifierValue">The value that identifies this key, typically a user, channel or guild id.</param>
    /// <param name="columnName">The name of the column to retrieve.</param>
    /// <returns></returns>
    public T GetValue<T>(object identifierValue, string columnName)
    {
        var type = this.GetType();
        var tableName = (this.Bot.DatabaseClient.MakePluginTablePrefix(this.Plugin) + type.GetCustomAttribute<TableNameAttribute>().Name).ToLower();
        var uniqueValueColumn = this.Plugin.Bot.DatabaseClient.GetPrimaryKey(type);

        return this.Plugin.Bot.DatabaseClient.GetValue<T>(tableName, uniqueValueColumn.ColumnName, identifierValue, columnName, this.Plugin.Bot.DatabaseClient.pluginDatabaseConnection);
    }

    /// <summary>
    /// Sets a value in this table's columns.
    /// </summary>
    /// <param name="plugin">Your plugin instance.</param>
    /// <param name="identifierValue">The value that identifies this key, typically a user, channel or guild id.</param>
    /// <param name="columnName">The name of the column to modify.</param>
    /// <param name="newColumnData">The new column data.</param>
    /// <returns></returns>
    public Task SetValue(object identifierValue, string columnName, object newColumnData)
    {
        var type = this.GetType();
        var tableName = (this.Bot.DatabaseClient.MakePluginTablePrefix(this.Plugin) + type.GetCustomAttribute<TableNameAttribute>().Name).ToLower();
        var uniqueValueColumn = this.Plugin.Bot.DatabaseClient.GetPrimaryKey(type);

        return this.Plugin.Bot.DatabaseClient.SetValue(tableName, uniqueValueColumn.ColumnName, identifierValue, columnName, newColumnData, this.Bot.DatabaseClient.pluginDatabaseConnection);
    }

    private void Validate()
    {
        var type = this.GetType();

        if (type.BaseType != typeof(PluginDatabaseTable))
            throw new ArgumentException("Specified type has to inherit PluginDatabaseTable");

        if (!this.TryGetCustomAttribute<TableNameAttribute>(type, out var nameAttr))
            throw new ArgumentException("Specified type has to have TableNameAttribute");

        var validProperties = this.Bot.DatabaseClient.GetValidProperties(type);

        if (!validProperties.IsNotNullAndNotEmpty())
            throw new ArgumentException("Specified type has to have properties with ColumnNameAttribute");

        if (!this.Plugin.AllowedTables.Contains(this.Bot.DatabaseClient.MakePluginTablePrefix(this.Plugin) + nameAttr.Name))
            throw new ArgumentException("Your plugin is not allowed to use this table name. This may be because the database name is already in use by Makoto or another plugin already registered this table.");
    }

    private bool TryGetCustomAttribute<T>(Type type, out T attribute) where T : Attribute
    {
        var attr = type.GetCustomAttribute<T>();

        if (attr != null)
        {
            attribute = attr;
            return true;
        }

        attribute = default;
        return false;
    }
}
