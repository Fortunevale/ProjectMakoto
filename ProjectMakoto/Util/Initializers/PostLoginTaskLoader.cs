// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util.Initializers;
internal class PostLoginTaskLoader
{
    public static async Task Load(Bot bot)
    {
        var guild = await bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild).GetGuildAsync(bot.status.LoadedConfig.Discord.AssetsGuild);
        var emojis = await guild.GetEmojisAsync();

        foreach (var field in typeof(Config.EmojiConfig).GetFields())
        {
            if (field.FieldType != typeof(ulong))
                continue;

            var v = (ulong)field.GetValue(bot.status.LoadedConfig.Emojis);
            if (v is not 0UL)
                if (emojis.Any(x => x.Id == v))
                    continue;

            try
            {
                if (emojis.Any(x => x.Name == field.Name))
                {
                    Log.Information("Missing '{emojiName}' Emoji but Guild '{guild}' contains emoji with same name. Using that..", field.Name, guild.Name);

                    field.SetValue(bot.status.LoadedConfig.Emojis, emojis.First(x => x.Name == field.Name).Id);
                    bot.status.LoadedConfig.Save();
                    continue;
                }

                Log.Information("Uploading '{emojiName}' Emoji to '{guild}'..", field.Name, guild.Name);

                var fileName = $"Assets/Emojis/Upload/{field.Name}.png";

                if (!Directory.GetFiles("Assets/Emojis/Upload/", "*", 
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
                        .Select(x => x.Replace("\\", "//"))
                        .Any(x => x.Contains(fileName)))
                    fileName = $"Assets/Emojis/Upload/{field.Name}.gif";

                if (!Directory.GetFiles("Assets/Emojis/Upload/", "*",
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
                        .Select(x => x.Replace("\\", "//"))
                        .Any(x => x.Contains(fileName)))
                    throw new FileNotFoundException($"The emoji file for '{field.Name}' could not be found.");

                using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var emoji = await guild.CreateEmojiAsync(field.Name, fileStream);

                field.SetValue(bot.status.LoadedConfig.Emojis, emoji.Id);

                bot.status.LoadedConfig.Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not upload emoji");
            }
        }
    }
}
