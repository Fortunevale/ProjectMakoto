// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMakoto;
public class MakotoModule
{
    private MakotoModule() { }

    /// <summary>
    /// Creates a new Makoto Command Module.
    /// <para>To extent an existing module, add a module with the same name (case-insensitive).</para>
    /// </summary>
    /// <param name="ModuleName">The name of the module.</param>
    /// <param name="Commands">The commands contained within the module.</param>
    public MakotoModule(string ModuleName, IEnumerable<MakotoCommand> Commands)
    {
        this.Name = ModuleName;
        this.Commands = Commands;
    }

    /// <summary>
    /// The name of this module.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The priority in the help command.
    /// </summary>
    public int? Priority { get; internal set; } = null;

    /// <summary>
    /// The commands contained within this module.
    /// </summary>
    public IEnumerable<MakotoCommand> Commands { get; internal set; }

    /// <summary>
    /// <para>Whether the command has been registered.</para>
    /// <para>All modifications will fail if this values is true.</para>
    /// </summary>
    public bool Registered { get; internal set; } = false;

    /// <summary>
    /// Sets the priority for this module.
    /// <para>Internally used priorities are:</para>
    /// <para><b>999</b> - <b>Utility</b></para>
    /// <para><b>998</b> - <b>Social</b></para>
    /// <para><b>997</b> - <b>Music</b></para>
    /// <para><b>996</b> - <b>ScoreSaber</b></para>
    /// <para><b>995</b> - <b>Moderation</b></para>
    /// <para><b>994</b> - <b>Configuration</b></para>
    /// <para><b>-999</b> - <b>Maintainer</b></para>
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MakotoModule WithPriority(int priority)
    {
        if (this.Registered)
            throw new InvalidOperationException("The module is already registered. It can no longer be modified.");

        this.Priority = priority;
        return this;
    }
}
