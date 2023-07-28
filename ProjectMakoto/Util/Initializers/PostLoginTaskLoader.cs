// Project Makoto
// Copyright (C) 2023  Fortunevale
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
        DiscordGuild guild = await bot.DiscordClient.GetGuildAsync(bot.status.LoadedConfig.Discord.AssetsGuild);
        var emojis = await guild.GetEmojisAsync();

        foreach (var field in typeof(Config.EmojiConfig).GetFields())
        {
            if (field.FieldType != typeof(ulong))
                continue;

            ulong v = (ulong)field.GetValue(bot.status.LoadedConfig.Emojis);
            if (v is not 0UL)
                continue;

            try
            {
                if (emojis.Any(x => x.Name == field.Name))
                {
                    _logger.LogInfo("Missing '{emojiName}' Emoji but Guild '{guild}' contains emoji with same name. Using that..", field.Name, guild.Name);

                    field.SetValue(bot.status.LoadedConfig.Emojis, emojis.First(x => x.Name == field.Name).Id);
                    bot.status.LoadedConfig.Save();
                    continue;
                }

                _logger.LogInfo("Uploading '{emojiName}' Emoji to '{guild}'..", field.Name, guild.Name);

                string fileName = $"Assets/Emojis/Upload/{field.Name}.png";

                if (!File.Exists(fileName))
                    fileName = $"Assets/Emojis/Upload/{field.Name}.gif";

                if (!File.Exists(fileName))
                    throw new FileNotFoundException($"The emoji file for '{field.Name}' could not be found.");

                using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var emoji = await guild.CreateEmojiAsync(field.Name, fileStream);

                field.SetValue(bot.status.LoadedConfig.Emojis, emoji.Id);

                bot.status.LoadedConfig.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not upload emoji", ex);
            }
        }
    }
}
