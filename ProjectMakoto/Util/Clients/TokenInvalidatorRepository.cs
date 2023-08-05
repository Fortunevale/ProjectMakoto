// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public class TokenInvalidatorRepository : RequiresBotReference
{
    internal TokenInvalidatorRepository(Bot bot) : base(bot)
    {
        this.SyncRepository().Add(bot);
    }

    private async Task SyncRepository()
    {
        Task task = new(() =>
        {
            _ = SyncRepository();
        });

        _ = task.CreateScheduledTask(DateTime.UtcNow.AddMinutes(30));
        task.Add(this.Bot);

        await this.Pull();
    }

    public (bool, FileInfo?) SearchForString(string searchQuery, string? startDirectory = null)
    {
        startDirectory ??= $"GitHub/{this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo}/";

        foreach (var directory in Directory.GetDirectories(startDirectory))
        {
            (bool found, FileInfo fileInfo) = SearchForString(searchQuery, directory);

            if (found)
                return (true, fileInfo);
        }

        foreach (var file in Directory.GetFiles(startDirectory))
        {
            var fileContent = File.ReadAllText(file);

            if (fileContent.Contains(searchQuery))
                return (true, new FileInfo(file));
        }

        return (false, null);
    }

    bool PullRunning = false;

    public async Task Pull()
    {
        try
        {
            if (this.PullRunning) return;

            this.PullRunning = true;

            if (!ExistsOnPath("git"))
            {
                _logger.LogWarn("Git was not found, cannot sync token invalidator repository.");
                return;
            }

            if (!Directory.Exists("GitHub"))
            {
                Directory.CreateDirectory("GitHub");

                Process.Start(new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = $"clone https://github.com/{this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepoOwner}/{this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo}.git",
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
                WorkingDirectory = $"GitHub/{this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo}/"
            });
            await fetch.WaitForExitAsync();

            if (fetch.ExitCode != 0)
            {
                _logger.LogError("Git fetch exited with a non-zero exit code.");
                return;
            }

            var pull = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = $"pull",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = $"GitHub/{this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo}/"
            });
            pull.BeginOutputReadLine();
            pull.BeginErrorReadLine();

            string pullOutput = "";

            pull.OutputDataReceived += (e, s) =>
            {
                pullOutput += s.Data;
            };

            await pull.WaitForExitAsync();

            if (pull.ExitCode != 0)
            {
                _logger.LogError("Git pull exited with a non-zero exit code.");
                return;
            }

            if (!pullOutput.Contains("Already up to date.", StringComparison.InvariantCultureIgnoreCase))
                _logger.LogInfo("Updated {TokenLeakRepo} repository.", this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo);
            else
                _logger.LogDebug("{TokenLeakRepo} repository already up to date.", this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo);
        }
        finally
        {
            this.PullRunning = false;
        }
    }

    private bool ExistsOnPath(string fileName) 
        => GetFullPath(fileName) != null;

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
