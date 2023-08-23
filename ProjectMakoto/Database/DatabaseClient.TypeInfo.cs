﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Database;
internal partial class DatabaseClient
{
    /// <summary>
    /// Gets the name of this table.
    /// </summary>
    /// <param name="type">The type to get the table name for.</param>
    /// <returns>The name of the table.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string GetTableName(Type type) 
        => type.GetCustomAttribute<TableNameAttribute>()?.Name ?? throw new InvalidOperationException("Type is not a table").AddData("info", type);

    /// <summary>
    /// Gets all valid properties of this table, recursively.
    /// </summary>
    /// <param name="type">The type to get the valid properties from.</param>
    /// <returns>An array of valid <see cref="PropertyInfo"/>s.</returns>
    public PropertyInfo[] GetValidProperties(Type type)
    {
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

                if (propertyList.Contains(property))
                    continue;

                propertyList.Add(property);

                if (property.GetCustomAttribute<ContainsValuesAttribute>() is not null)
                {
                    AddProperties(property.PropertyType);
                }
            }
        }

        AddProperties(type);

        return propertyList.ToArray();
    }

    /// <summary>
    /// Get's all column information backed by the specified property.
    /// </summary>
    /// <param name="info">The property to get the information for.</param>
    /// <returns>All values that are backed by this property.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) GetPropertyInfo(PropertyInfo info) 
        => (info.GetCustomAttribute<ColumnNameAttribute>()?.Name ?? throw new InvalidOperationException("Not a valid column.").AddData("info", info),
            info.GetCustomAttribute<ColumnType>()?.Type ?? throw new InvalidOperationException("Not a valid column.").AddData("info", info),
            info.GetCustomAttribute<PrimaryAttribute>()?.Primary ?? false,
            info.GetCustomAttribute<MaxValueAttribute>()?.MaxValue,
            info.GetCustomAttribute<CollationAttribute>()?.Collation,
            info.GetCustomAttribute<NullableAttribute>()?.Nullable ?? false,
            info.GetCustomAttribute<DefaultAttribute>()?.Default);

    public (string ColumnName, ColumnTypes ColumnType, bool Primary, long? MaxValue, string? Collation, bool Nullable, string Default) GetPrimaryKey(Type type)
        => this.GetPropertyInfo(this.GetValidProperties(type).First(x => this.GetPropertyInfo(x).Primary));
}
