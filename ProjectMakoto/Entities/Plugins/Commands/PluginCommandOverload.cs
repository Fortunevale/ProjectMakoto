// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;
/// <summary>
/// Creates a new required overload for a command.
/// </summary>
/// <param name="Type">The type to use for the overload.</param>
/// <param name="Name">The name for the overload to use.</param>
/// <param name="Description">The description of the overload to use.</param>
/// <param name="Required">If the overload should be required.</param>
/// <param name="UseRemainingString">If the remaining string of the triggering message should be used as the last argument.</param>
public sealed class PluginCommandOverload(Type Type, string Name, string Description, bool Required = true, bool UseRemainingString = false)
{

    /// <summary>
    /// The type of overload.
    /// </summary>
    public Type Type { get; set; } = Type;

    /// <summary>
    /// The name of the overload.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// The description of the overload.
    /// </summary>
    public string Description { get; set; } = Description;

    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool Required { get; set; } = Required;

    /// <summary>
    /// If the overload is required.
    /// </summary>
    public bool UseRemainingString { get; set; } = UseRemainingString;
}
