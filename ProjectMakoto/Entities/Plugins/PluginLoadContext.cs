// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Runtime.Loader;

namespace ProjectMakoto.Plugins;
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
    {
        this._resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = this._resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? this.LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = this._resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? this.LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
