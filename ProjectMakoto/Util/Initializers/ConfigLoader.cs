// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util.Initializers;
internal static class ConfigLoader
{
    internal static async Task Load(Bot bot)
    {
        if (!File.Exists("config.json"))
            new Config().Save();

        _ = Task.Run(async () =>
        {
            DateTime lastModify = new();

            bot.status.LoadedConfig = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("config.json"));
            await Task.Delay(500);
            bot.status.LoadedConfig.Save();

            while (true)
            {
                try
                {
                    FileInfo fileInfo = new("config.json");

                    if (lastModify != fileInfo.LastWriteTimeUtc || bot.status.LoadedConfig is null)
                    {
                        try
                        {
                            _logger.LogDebug("Reloading config..");
                            bot.status.LoadedConfig = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("config.json"));
                            _logger.LogInfo("Config reloaded.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to reload config", ex);
                        }
                    }

                    lastModify = fileInfo.LastWriteTimeUtc;

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred while trying to reload the config.json", ex);
                    await Task.Delay(10000);
                }
            }
        }).Add(bot);

        while (bot.status.LoadedConfig is null)
            await Task.Delay(100);

        foreach (var field in typeof(Config.DiscordConfig).GetFields())
        {
            if (field.FieldType != typeof(ulong))
                continue;

            var v = (ulong)field.GetValue(bot.status.LoadedConfig.Discord);
            if (v is not 0UL)
                continue;

            _logger.LogError("No {0} provided.", field.Name);
            await Task.Delay(1000);
            Console.Write("> ");
            field.SetValue(bot.status.LoadedConfig.Discord, Convert.ToUInt64(Console.ReadLine()));
        }

        foreach (var field in typeof(Config.ChannelsConfig).GetFields())
        {
            if (field.FieldType != typeof(ulong))
                continue;

            var v = (ulong)field.GetValue(bot.status.LoadedConfig.Channels);
            if (v is not 0UL)
                continue;

            _logger.LogError("No {0} provided.", field.Name);
            await Task.Delay(1000);
            Console.Write("> ");
            field.SetValue(bot.status.LoadedConfig.Channels, Convert.ToUInt64(Console.ReadLine()));

            bot.status.LoadedConfig.Save();
        }

        _logger.AddBlacklist(bot.status.LoadedConfig.Secrets.Database.Password,
            bot.status.LoadedConfig.Secrets.Discord.Token,
            bot.status.LoadedConfig.Secrets.Telegram.Token,
            bot.status.LoadedConfig.Secrets.Lavalink.Password,
            bot.status.LoadedConfig.Secrets.Github.Token,
            bot.status.LoadedConfig.Secrets.KawaiiRedToken);
    }
}
