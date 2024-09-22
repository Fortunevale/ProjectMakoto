// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public class OfficialPluginRepository : RequiresBotReference
{
    internal OfficialPluginRepository(Bot bot) : base(bot)
    {
        this.SyncRepository();
    }

    private void SyncRepository()
    {
        _ = new Func<Task>(() =>
        {
            this.SyncRepository();
            return Task.CompletedTask;
        }).CreateScheduledTask(DateTime.UtcNow.AddMinutes(30));

        _ = Task.Run(this.Pull);
    }

    internal (bool, PluginManifest?) FindHash(string hash)
    {
        (var found, var fileInfo) = this.FindFile(hash);

        if (found)
            return (true, JsonConvert.DeserializeObject<PluginManifest>(File.ReadAllText(fileInfo.FullName)));

        return (false, null);
    }

    private (bool, FileInfo?) FindFile(string searchQuery, string? startDirectory = null)
    {
        startDirectory ??= $"GitHub/ProjectMakoto.TrustedPlugins/";

        foreach (var directory in Directory.GetDirectories(startDirectory))
        {
            (var found, var fileInfo) = this.FindFile(searchQuery, directory);

            if (found)
                return (true, fileInfo);
        }

        foreach (var file in Directory.GetFiles(startDirectory).Where(x => x.EndsWith(".json")))
        {
            if (Path.GetFileNameWithoutExtension(file) == searchQuery)
                return (true, new FileInfo(file));
        }

        return (false, null);
    }

    internal bool PullRunning = false;

    public async Task Pull()
    {
        try
        {
            if (this.PullRunning) return;

            this.PullRunning = true;

            if (!this.ExistsOnPath("git"))
            {
                Log.Warning("Git was not found, cannot sync trusted plugins repository.");
                return;
            }

            if (!Directory.Exists("GitHub/ProjectMakoto.TrustedPlugins"))
            {
                _ = Directory.CreateDirectory("GitHub");

                _ = Process.Start(new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = $"clone https://github.com/Fortunevale/ProjectMakoto.TrustedPlugins.git",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "GitHub/"
                });
            }

            var fetch = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = $"fetch",
                WorkingDirectory = $"GitHub/ProjectMakoto.TrustedPlugins/"
            });
            await fetch.WaitForExitAsync();

            if (fetch.ExitCode != 0)
            {
                Log.Error("Git fetch exited with a non-zero exit code.");
                return;
            }

            var pull = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = $"pull",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = $"GitHub/ProjectMakoto.TrustedPlugins/"
            });
            pull.BeginOutputReadLine();
            pull.BeginErrorReadLine();

            var pullOutput = "";

            pull.OutputDataReceived += (e, s) =>
            {
                pullOutput += s.Data;
            };

            await pull.WaitForExitAsync();

            if (pull.ExitCode != 0)
            {
                Log.Error("Git pull exited with a non-zero exit code.");
                return;
            }

            if (!pullOutput.Contains("Already up to date.", StringComparison.InvariantCultureIgnoreCase))
                Log.Information("Updated {Repo} repository.", "ProjectMakoto.TrustedPlugins");
            else
                Log.Debug("{Repo} repository already up to date.", "ProjectMakoto.TrustedPlugins");
        }
        finally
        {
            this.PullRunning = false;
        }
    }

    private bool ExistsOnPath(string fileName) 
        => this.GetFullPath(fileName) != null;

    private string GetFullPath(string fileName)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && !fileName.EndsWith(".exe"))
            fileName += ".exe";

        if (File.Exists(fileName))
            return Path.GetFullPath(fileName);

        var values = Environment.GetEnvironmentVariable("PATH");
        foreach (var path in values.Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(path, fileName);
            if (File.Exists(fullPath))
                return fullPath;
        }
        return null;
    }
}
