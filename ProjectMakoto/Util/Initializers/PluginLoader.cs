// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Immutable;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ProjectMakoto.Util.Initializers;
internal static class PluginLoader
{
    internal static async Task LoadPlugins(Bot bot, bool InitializeLoadedPlugins = true, string PluginDirectory = "Plugins")
    {
        if (InitializeLoadedPlugins && !bot.status.LoadedConfig.EnablePlugins)
            return;

        await bot.OfficialPlugins.Pull();
        await Task.Delay(500);
        while (bot.OfficialPlugins.PullRunning)
            await Task.Delay(1000);

        Log.Debug("Loading Plugins from '{PluginDirectory}'..", PluginDirectory);

        if (!Directory.Exists(PluginDirectory))
            _ = Directory.CreateDirectory(PluginDirectory);

        foreach (var pluginFile in Directory.GetFiles(PluginDirectory).Where(x => x.EndsWith(".pmpl")))
        {
            if (new DirectoryInfo(pluginFile).Name.StartsWith('.'))
                continue;

            var pluginName = Path.GetFileName(pluginFile);

            Log.Debug("Loading Plugin '{Name}'..", pluginName);

            var pluginHash = HashingExtensions.ComputeSHA256Hash(new FileInfo(pluginFile));

            using var pluginFileStream = new FileStream(pluginFile, FileMode.Open, FileAccess.ReadWrite);
            using var zipArchive = new ZipArchive(pluginFileStream, ZipArchiveMode.Update);
            var isOfficial = false;

            if (InitializeLoadedPlugins)
            {
                var (found, remoteInfo) = bot.OfficialPlugins.FindHash(pluginHash);

                if (!found && bot.status.LoadedConfig.OnlyLoadOfficialPlugins)
                {
                    Log.Warning("Skipped loading of unofficial plugin '{Name}'.", pluginName);
                    continue;
                }

                if (found)
                {
                    Log.Information("'{Name}' is an official plugin: {Hash}", pluginName, pluginHash);

                    using var localManifestStream = zipArchive.GetEntry("manifest.json").Open();
                    using var localManifestStreamReader = new StreamReader(localManifestStream);
                    var localManifestText = localManifestStreamReader.ReadToEnd();

                    var localInfo = JsonConvert.DeserializeObject<PluginManifest>(localManifestText);

                    if (localInfo.Name != remoteInfo.Name ||
                        localInfo.Description != remoteInfo.Description ||
                        localInfo.Author != remoteInfo.Author ||
                        localInfo.AuthorId != remoteInfo.AuthorId ||
                        localInfo.Version != localInfo.Version)
                    {
                        Log.Warning("Skipped loading of official plugin '{Name}', manifest mismatches.", pluginName);
                        continue;
                    }

                    isOfficial = true;
                }
            }

            var referenceFiles = zipArchive.Entries;
            Assembly? resolveAssemblyEvent(object? obj, ResolveEventArgs arg)
            {
                var name = $"{new AssemblyName(arg.Name).Name}.dll";
                var assemblyFile = referenceFiles.Where(x => x.Name.EndsWith(name)).FirstOrDefault();
                if (assemblyFile != null)
                {
                    using var assemblyStream = assemblyFile.Open();
                    return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
                }

                throw new Exception($"Could not locate: '{name}' ({arg.RequestingAssembly?.FullName})");
            }

            AppDomain.CurrentDomain.AssemblyResolve += resolveAssemblyEvent;

            try
            {
                var count = 0;

                foreach (var assemblyEntry in referenceFiles.Where(x => x.Name.EndsWith(".dll")))
                {
                    AssemblyLoadContext pluginLoadContext = new(null);

                    var assemblyName = Path.GetFileNameWithoutExtension(assemblyEntry.Name);

                    if (AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).Any(x => x.Name == assemblyName))
                    {
                        Log.Verbose("{Assembly} already loaded, skipping", assemblyName);
                        continue;
                    }

                    using var assemblyStream = assemblyEntry.Open();
                    var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);

                    try
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (typeof(BasePlugin).IsAssignableFrom(type))
                            {
                                Log.Debug("Loading Plugin from '{0}'", assemblyEntry);

                                count++;
                                var result = Activator.CreateInstance(type) as BasePlugin;
                                result.LoadedFile = new FileInfo(pluginFile);
                                result.OfficialPlugin = isOfficial;

                                if (result.SupportedPluginApis == null || !result.SupportedPluginApis.Contains(BasePlugin.CurrentApiVersion))
                                    throw new IndexOutOfRangeException($"Plugin does not support Api Version {BasePlugin.CurrentApiVersion}");

                                bot._Plugins.Add(assemblyName, result);

                                UniversalExtensions.LoadAllReferencedAssemblies(assembly.GetReferencedAssemblies());

                                _ = assemblyStream.Seek(0, SeekOrigin.Begin);
                                CommandCompiler.AssemblyReferences.Add(MetadataReference.CreateFromStream(assemblyStream));
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = bot._Plugins.Remove(assemblyName);
                        Log.Error(ex, "Failed to load Plugin '{0}' from '{1}'", assemblyName, assemblyEntry);
                    }
                }

                if (count == 0)
                {
                    Log.Warning("Cannot load Plugin '{0}': Plugin Assembly does not contain type that inherits BasePlugin.", pluginName);
                    continue;
                }

                Log.Information("Loaded Plugin from '{0}'", pluginName);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveAssemblyEvent;
            }
        }

        Log.Information("Loaded {0} Plugins.", bot.Plugins.Count);

        foreach (var b in bot.Plugins)
        {
            if (b.Value.Name.IsNullOrWhiteSpace())
            {
                Log.Warning("Skipped loading Plugin '{0}': Missing Name.", b.Key);
                continue;
            }

            if (b.Value.Description.IsNullOrWhiteSpace())
            {
                Log.Warning("Skipped loading Plugin '{0}': Missing Description.", b.Key);
                continue;
            }

            if (b.Value.Author.IsNullOrWhiteSpace())
            {
                Log.Warning("Skipped loading Plugin '{0}': Missing Author.", b.Key);
                continue;
            }

            if (b.Value.Version is null)
            {
                Log.Warning("Skipped loading Plugin '{0}': Missing Version.", b.Key);
                continue;
            }

            Log.Debug("Initializing Plugin '{0}' ({1})..", b.Value.Name, b.Key);

            try
            {
                if (InitializeLoadedPlugins)
                    b.Value.Load(bot);
                Log.Information("Initialized Plugin from '{0}': '{1}' (v{2}).", b.Key, b.Value.Name, b.Value.Version.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize Plugin from '{0}': '{1}' (v{2}).", b.Key, b.Value.Name, b.Value.Version.ToString());
            }

            try
            {
                if (InitializeLoadedPlugins)
                    await b.Value.CheckForUpdates();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to check updates for '{PluginName}'", b.Value.Name);
            }

            try
            {
                if (InitializeLoadedPlugins)
                {
                    var (path, type) = b.Value.LoadTranslations();

                    if (path != null)
                    {
                        using var stream = b.Value.LoadedFile.Open(FileMode.Open);
                        using var zip = new ZipArchive(stream);
                        using var file = zip.GetEntry(path).Open();
                        using var reader = new StreamReader(file);

                        b.Value.UsesTranslations = true;
                        b.Value.Translations = (ITranslations)JsonConvert.DeserializeObject(reader.ReadToEnd(), type);

                        foreach (var item in b.Value.Translations.CommandList)
                            bot.LoadedTranslations.CommandList = bot.LoadedTranslations.CommandList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load translations for '{PluginName}'", b.Value.Name);
            }
        }
    }

    internal static async Task LoadPluginCommands(Bot bot, IReadOnlyDictionary<int, CommandsNextExtension> cNext, IReadOnlyDictionary<int, ApplicationCommandsExtension> appCommands)
    {
        foreach (var plugin in bot.Plugins)
        {
            var pluginModules = await plugin.Value.RegisterCommands();
            bot._PluginCommandModules.Add(plugin.Key, pluginModules.ToList());
            var assemblies = await CommandCompiler.BuildCommands(bot, bot.status.CurrentAppHash, pluginModules, plugin);
            CommandCompiler.RegisterAssemblies(bot, cNext, appCommands, plugin.Value.EnableCommandTranslations, assemblies);
        }

        bot.status.LoadedConfig.DontModify.LastKnownHash = bot.status.CurrentAppHash;
        bot.status.LoadedConfig.Save();
    }
}

internal record CompilationData(CompilationType type, string code, IEnumerable<MakotoModule> moduleList, string Identifier);

public enum CompilationType
{
    App,
    Prefix
}