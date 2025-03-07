// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public static class EmojiTemplates
{
    public static DiscordEmoji GetCheckboxTickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.CheckboxTicked);
    public static DiscordEmoji GetCheckboxUntickedBlue(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.CheckboxUnticked);

    public static DiscordEmoji GetPillOff(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.PillOff);
    public static DiscordEmoji GetPillOn(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.PillOn);

    public static DiscordEmoji GetError(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Error);

    public static DiscordEmoji GetSlashCommand(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.SlashCommand);
    public static DiscordEmoji GetMessageCommand(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.MessageCommand);
    public static DiscordEmoji GetUserCommand(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.UserCommand);

    public static DiscordEmoji GetPrefixCommandDisabled(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.PrefixCommandDisabled);
    public static DiscordEmoji GetPrefixCommandEnabled(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.PrefixCommandEnabled);

    public static DiscordEmoji GetQuestionMark(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.QuestionMark);

    public static DiscordEmoji GetGuild(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Guild);
    public static DiscordEmoji GetChannel(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Channel);
    public static DiscordEmoji GetUser(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.User);
    public static DiscordEmoji GetVoiceState(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.VoiceState);
    public static DiscordEmoji GetMessage(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Message);
    public static DiscordEmoji GetInvite(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Invite);
    public static DiscordEmoji GetInVisible(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.In);

    public static DiscordEmoji GetYouTube(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.YouTube);
    public static DiscordEmoji GetSoundcloud(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.SoundCloud);
    public static DiscordEmoji GetSpotify(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Spotify);
    public static DiscordEmoji GetAbuseIpDb(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.AbuseIPDB);
    public static DiscordEmoji GetLoading(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Loading);

    public static DiscordEmoji GetPaused(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.Paused);
    public static DiscordEmoji GetDisabledPlay(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.DisabledPlay);
    public static DiscordEmoji GetDisabledRepeat(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.DisabledRepeat);
    public static DiscordEmoji GetDisabledShuffle(Bot bot) => DiscordEmoji.FromGuildEmote(bot.DiscordClient.GetShard(bot.status.LoadedConfig.Discord.AssetsGuild), bot.status.LoadedConfig.Emojis.DisabledShuffle);
}
