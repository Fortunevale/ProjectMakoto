// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;
internal static class ManifestBuilder
{
    public static async Task BuildPluginManifests(Bot bot, string[] args)
    {
        Log.Warning("Makoto has been started as a Manifest Builder.", args);

        var pluginDirectoryIndex = args.IndexOf("--build-manifests") + 1;
        if (pluginDirectoryIndex > args.Length)
        {
            Log.Fatal("No plugin directory provided.");
            bot.ExitApplication(true).Wait();
            return;
        }

        var pluginDirectoryPath = args[pluginDirectoryIndex];
        if (!Directory.Exists(pluginDirectoryPath))
        {
            Log.Fatal("Plugin directory was not found.");
            bot.ExitApplication(true).Wait();
            return;
        }

        string? manifestOutputDirectory = null;

        if (args.Contains("--output-manifests"))
        {
            var outputDirectoryIndex = args.IndexOf("--output-manifests") + 1;
            if (outputDirectoryIndex > args.Length)
            {
                Log.Fatal("No output directory provided.");
                bot.ExitApplication(true).Wait();
                return;
            }

            manifestOutputDirectory = args[outputDirectoryIndex];
        }

        Log.Information("Building Makoto Plugin manifests in '{Directory}'..", pluginDirectoryPath);

        UniversalExtensions.LoadAllReferencedAssemblies(AppDomain.CurrentDomain);
        await Util.Initializers.PluginLoader.LoadPlugins(bot, false, pluginDirectoryPath);

        foreach (var plugin in bot.Plugins)
        {
            Log.Information("Generating Plugin Manifest for '{assembly}'..", plugin.Key);

            var manifest = new PluginManifest()
            {
                Name = plugin.Value.Name,
                Description = plugin.Value.Description,
                Author = plugin.Value.Author,
                AuthorId = plugin.Value.AuthorId,
                Version = plugin.Value.Version,
            };

            if (manifestOutputDirectory is not null)
            {
                var pluginHash = HashingExtensions.ComputeSHA256Hash(plugin.Value.LoadedFile);

                File.WriteAllText($"{manifestOutputDirectory}/{pluginHash}.json", JsonConvert.SerializeObject(manifest, Formatting.Indented));
                continue;
            }

            using var zipStream = plugin.Value.LoadedFile.Open(FileMode.Open, FileAccess.ReadWrite);
            using var pluginZip = new ZipArchive(zipStream, ZipArchiveMode.Update, false);

            if (pluginZip.Entries.Any(x => x.Name == "manifest.json"))
            {
                Log.Warning("Plugin '{assembly}' already contains a manifest! Skipping..", plugin.Key);
                continue;
            }

            var manifestBytes = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(manifest, Formatting.Indented));

            var newEntry = pluginZip.CreateEntry("manifest.json");
            var newEntryStream = newEntry.Open();
            newEntryStream.Write(manifestBytes, 0, manifestBytes.Length);
            newEntryStream.Flush();
            newEntry.LastWriteTime = DateTime.Now;

            pluginZip.Dispose();
            zipStream.Close();
        }
    }
}
