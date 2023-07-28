// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class EmojiTemplates
{
    public static DiscordEmoji GetCheckboxTickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.CheckboxTicked);
    public static DiscordEmoji GetCheckboxUntickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.CheckboxUnticked);

    public static DiscordEmoji GetPillOff(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.PillOff);
    public static DiscordEmoji GetPillOn(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.PillOn);

    public static DiscordEmoji GetError(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.Error);

    public static DiscordEmoji GetQuestionMark(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.QuestionMark);

    public static DiscordEmoji GetGuild(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.Guild);
    public static DiscordEmoji GetChannel(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.Channel);
    public static DiscordEmoji GetUser(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.User);
    public static DiscordEmoji GetVoiceState(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.VoiceState);
    public static DiscordEmoji GetMessage(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.Message);
    public static DiscordEmoji GetInvite(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.Invite);

    public static DiscordEmoji GetYouTube(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.YouTube);
    public static DiscordEmoji GetSoundcloud(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.SoundCloud);
    public static DiscordEmoji GetAbuseIpDb(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient, bot.status.LoadedConfig.Emojis.AbuseIPDB);
}
