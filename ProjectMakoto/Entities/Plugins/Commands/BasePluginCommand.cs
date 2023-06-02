// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Plugins.Commands;

public class BasePluginCommand
{
    private BasePluginCommand() { }

    /// <summary>
    /// Create a new Plugin Command.
    /// </summary>
    /// <param name="Name">The name of the command to be registered.</param>
    /// <param name="Description">The description of the command to be registered.</param>
    /// <param name="Command">The command to be executed.</param>
    /// <param name="Module">The module of the command to be registered.</param>
    /// <param name="Overloads">The required overloads of the command to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is <see langword="null"/> or consists only of whitespaces.</exception>
    public BasePluginCommand(string Name, string Description, string Module, BaseCommand Command, params BaseOverload[] Overloads)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Description.IsNullOrWhiteSpace()) 
            throw new ArgumentNullException(nameof(Description));

        if (Module.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Module));

        if (Command is null)
            throw new ArgumentNullException(nameof(Command));

        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.Module = Module.Trim();
        this.Command = Command;
        this.Overloads = Overloads?.ToArray() ?? Array.Empty<BaseOverload>();
    }

    /// <summary>
    /// Creates a new Plugin Command Group.
    /// </summary>
    /// <param name="Name">The name of this plugin group.</param>
    /// <param name="Description">The description of this plugin group.</param>
    /// <param name="Module">The module of this plugin group.</param>
    /// <param name="Commands">The commands of this group.</param>
    public BasePluginCommand(string Name, string Description, string Module, params BasePluginCommand[] Commands)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Description.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Description));

        if (Module.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Module));

        if ((Commands?.Length ?? 0) == 0)
            throw new ArgumentNullException(nameof(Commands));

        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.Module = Module.Trim();
        this.SubCommands = Commands;
        this.Overloads = Overloads?.ToArray() ?? Array.Empty<BaseOverload>();
    }

    /// <summary>
    /// The command's name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The command's description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The command's module.
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// This command's parent, if group.
    /// </summary>
    public BasePluginCommand? Parent { get; set; }

    /// <summary>
    /// Whether this command is a group.
    /// </summary>
    public bool IsGroup
        => (Command is null && SubCommands is not null);

    /// <summary>
    /// The command.
    /// </summary>
    public BaseCommand? Command { get; set; }

    /// <summary>
    /// The command's sub commands, if group.
    /// </summary>
    public BasePluginCommand[]? SubCommands { get; set; }

    /// <summary>
    /// The required overloads.
    /// </summary>
    public BaseOverload[] Overloads { get; set; }
}
