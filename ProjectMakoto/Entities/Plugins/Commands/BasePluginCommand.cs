// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;

public sealed class BasePluginCommand
{
    private BasePluginCommand() { }

    /// <summary>
    /// Creates a new Plugin Context Menu Command.
    /// </summary>
    /// <param name="Name">The name of the command to be registered.</param>
    /// <param name="Description">The description of the command to be registered.</param>
    /// <param name="Command">The command to be executed.</param>
    /// <param name="RegisterPrefixAlternative">Whether or not this command should have a equivalent prefix command.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is <see langword="null"/> or consists only of whitespaces.</exception>
    public BasePluginCommand(ApplicationCommandType type, string Name, string Description, BaseCommand Command, bool RegisterPrefixAlternative)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Command is null)
            throw new ArgumentNullException(nameof(Command));

        if (type is not ApplicationCommandType.Message and not ApplicationCommandType.User)
            throw new InvalidOperationException("The ApplicationCommandType has to be Message or User!");

        this.ContextMenuType = type;
        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.Command = Command;
        this.SupportedCommandTypes = RegisterPrefixAlternative ? new[] { PluginCommandType.ContextMenu, PluginCommandType.PrefixCommand } : new[] { PluginCommandType.ContextMenu };
    }

    /// <summary>
    /// Create a new Plugin Command.
    /// </summary>
    /// <param name="Name">The name of the command to be registered.</param>
    /// <param name="Description">The description of the command to be registered.</param>
    /// <param name="Command">The command to be executed.</param>
    /// <param name="Overloads">The required overloads of the command to be registered.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is <see langword="null"/> or consists only of whitespaces.</exception>
    public BasePluginCommand(string Name, string Description, BaseCommand Command, params BaseOverload[] Overloads)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Description.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Description));

        if (Command is null)
            throw new ArgumentNullException(nameof(Command));

        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.Command = Command;
        this.Overloads = Overloads?.ToArray() ?? Array.Empty<BaseOverload>();
    }

    /// <summary>
    /// Creates a new Plugin Command Group.
    /// </summary>
    /// <param name="Name">The name of this plugin group.</param>
    /// <param name="Description">The description of this plugin group.</param>
    /// <param name="Commands">The commands of this group.</param>
    public BasePluginCommand(string Name, string Description, params BasePluginCommand[] Commands)
    {
        if (Name.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Name));

        if (Description.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Description));

        if ((Commands?.Length ?? 0) == 0)
            throw new ArgumentNullException(nameof(Commands));

        if (this.UseDefaultHelp && Commands.Any(x => x.Name == "help"))
            throw new ArgumentException("You cannot provide a help command if the default help is enabled.");

        this.Name = Name.Trim();
        this.Description = Description.Trim();
        this.SubCommands = Commands;
        this.Overloads = this.Overloads?.ToArray() ?? Array.Empty<BaseOverload>();
        this.UseDefaultHelp = true;
    }

    /// <summary>
    /// <para>Whether the command has been registered.</para>
    /// <para>All modifications will fail if this values is true.</para>
    /// </summary>
    public bool Registered { get; internal set; } = false;

    /// <summary>
    /// The command's name.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The command's description.
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// This command's parent, if group.
    /// </summary>
    public BasePluginCommand? Parent { get; internal set; }

    /// <summary>
    /// Whether this command is a group.
    /// </summary>
    public bool IsGroup
        => (this.Command is null && this.SubCommands is not null);

    /// <summary>
    /// The command to execute.
    /// </summary>
    public BaseCommand? Command { get; internal set; }

    /// <summary>
    /// The command's sub commands, if group.
    /// </summary>
    public BasePluginCommand[]? SubCommands { get; internal set; }

    /// <summary>
    /// The required overloads.
    /// </summary>
    public BaseOverload[] Overloads { get; internal set; }

    /// <summary>
    /// <para>Whether to use the default help for command groups.</para>
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool UseDefaultHelp { get; internal set; } = true;

    /// <summary>
    /// The Context Menu Type, only usable if <see cref="SupportedCommandTypes"/> includes <see cref="PluginCommandType.ContextMenu"/>
    /// </summary>
    public ApplicationCommandType? ContextMenuType { get; internal set; } = null;

    /// <summary>
    /// Updates the <see cref="UseDefaultHelp"/> value.
    /// <inheritdoc cref="UseDefaultHelp"/>
    /// </summary>
    /// <param name="UseDefaultHelp">The new <see cref="UseDefaultHelp"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered.</exception>
    public BasePluginCommand WithUseDefaultHelp(bool UseDefaultHelp)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        if (!this.IsGroup)
            throw new InvalidOperationException("The command is not a group.");

        this.UseDefaultHelp = UseDefaultHelp;
        return this;
    }

    /// <summary>
    /// <para>The required permissions to <b>view</b> the command as application command.</para>
    /// <para><b>This does not protect the command from users without this permission. It only hides the command in the application command list when the user does not fulfill the requirement.</b></para>
    /// Defaults to <see cref="null"/>.
    /// </summary>
    public Permissions? RequiredPermissions { get; internal set; } = null;

    /// <summary>
    /// Updates the <see cref="RequiredPermissions"/> value.
    /// <inheritdoc cref="RequiredPermissions"/>
    /// </summary>
    /// <param name="RequiredPermissions">The new <see cref="RequiredPermissions"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered.</exception>
    public BasePluginCommand WithRequiredPermissions(Permissions RequiredPermissions)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        this.RequiredPermissions = RequiredPermissions;
        return this;
    }

    /// <summary>
    /// <para>Whether to allow running this command in Direct Messages.</para>
    /// <para>Make sure to adjust your command to accommodate for usage in direct messages. </para>
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowPrivateUsage { get; internal set; } = false;

    /// <summary>
    /// Updates the <see cref="AllowPrivateUsage"/> value.
    /// <inheritdoc cref="AllowPrivateUsage"/>
    /// </summary>
    /// <param name="AllowPrivateUsage">The new <see cref="AllowPrivateUsage"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered.</exception>
    public BasePluginCommand WithAllowPrivateUsage(bool AllowPrivateUsage)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        this.AllowPrivateUsage = AllowPrivateUsage;
        return this;
    }

    /// <summary>
    /// <para>Whether the command should be marked as NSFW.</para>
    /// <para><b>This does not ensure that the command is only run by adult users. It only hides this command in the application command list when the user does not fulfill the requirement.</b></para>
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool IsNsfw { get; internal set; } = false;

    /// <summary>
    /// Updates the <see cref="IsNsfw"/> value.
    /// <inheritdoc cref="IsNsfw"/>
    /// </summary>
    /// <param name="IsNsfw">The new <see cref="IsNsfw"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered.</exception>
    public BasePluginCommand WithIsNsfw(bool IsNsfw)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        this.IsNsfw = IsNsfw;
        return this;
    }

    /// <summary>
    /// <para>Which command types are supported.</para>
    /// Defaults to <see cref="PluginCommandType.PrefixCommand"/> and  <see cref="PluginCommandType.SlashCommand"/>.
    /// </summary>
    public IReadOnlyList<PluginCommandType> SupportedCommandTypes { get; internal set; } = new List<PluginCommandType>() { PluginCommandType.PrefixCommand, PluginCommandType.SlashCommand }.AsReadOnly();

    /// <summary>
    /// Updates the <see cref="SupportedCommandTypes"/> value.
    /// <inheritdoc cref="SupportedCommandTypes"/>
    /// </summary>
    /// <param name="SupportedCommands">The new <see cref="SupportedCommandTypes"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered or the type cannot be changed.</exception>
    public BasePluginCommand WithSupportedCommandTypes(params PluginCommandType[] SupportedCommands)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        if (SupportedCommands.Contains(PluginCommandType.ContextMenu))
            throw new InvalidOperationException("You cannot use ContextMenu or ContextMenuWithoutPrefix as supported CommandType, please use the constructor instead.");

        if (this.SupportedCommandTypes.Any(x => x == PluginCommandType.ContextMenu))
            throw new InvalidOperationException("You cannot modify the supported command types on context menus.");

        this.SupportedCommandTypes = SupportedCommands;
        return this;
    }

    /// <summary>
    /// <para>Whether to run this command with an ephemeral message when ran via slash command.</para>
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool IsEphemeral { get; internal set; } = true;

    /// <summary>
    /// Updates the <see cref="IsEphemeral"/> value.
    /// <inheritdoc cref="IsEphemeral"/>
    /// </summary>
    /// <param name="SupportedCommands">The new <see cref="SupportedCommandTypes"/> value.</param>
    /// <returns>This <see cref="BasePluginCommand"/> with the updated value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command is already registered.</exception>
    public BasePluginCommand WithIsEphemeral(bool useEphemeral)
    {
        if (this.Registered)
            throw new InvalidOperationException("The command is already registered. It can no longer be modified.");

        this.IsEphemeral = useEphemeral;
        return this;
    }
}
