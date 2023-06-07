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
    public static DiscordEmoji GetCheckboxTickedRed(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxTickedRedId);
    public static DiscordEmoji GetCheckboxUntickedRed(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxUntickedRedId);
    public static DiscordEmoji GetCheckboxTickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxTickedBlueId);
    public static DiscordEmoji GetCheckboxUntickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxUntickedBlueId);
    public static DiscordEmoji GetCheckboxTickedGreen(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxTickedGreenId);
    public static DiscordEmoji GetCheckboxUntickedGreen(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.CheckboxUntickedGreenId);

    public static DiscordEmoji GetPillOff(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.PillOffId);
    public static DiscordEmoji GetPillOn(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.PillOnId);

    public static DiscordEmoji GetWhiteXMark(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.WhiteXMark);

    public static DiscordEmoji GetQuestionMark(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.QuestionMarkId);

    public static DiscordEmoji GetGuild(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.GuildId);
    public static DiscordEmoji GetChannel(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.ChannelId);
    public static DiscordEmoji GetUser(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.UserId);
    public static DiscordEmoji GetVoiceState(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.VoiceStateId);
    public static DiscordEmoji GetMessage(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.MessageId);
    public static DiscordEmoji GetInvite(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.InviteId);

    public static DiscordEmoji GetYouTube(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.YouTubeId);
    public static DiscordEmoji GetSoundcloud(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.SoundCloudId);
    public static DiscordEmoji GetAbuseIpDb(Bot bot) => DiscordEmoji.FromGuildEmote(bot.discordClient, bot.status.LoadedConfig.Emojis.AbuseIPDBId);
}
