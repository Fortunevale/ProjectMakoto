namespace ProjectIchigo.Entities;

internal class EmojiTemplates
{
    public static DiscordEmoji GetCheckboxTickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxTickedRedId);
    public static DiscordEmoji GetCheckboxUntickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxUntickedRedId);
    public static DiscordEmoji GetCheckboxTickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxTickedBlueId);
    public static DiscordEmoji GetCheckboxUntickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxUntickedBlueId);
    public static DiscordEmoji GetCheckboxTickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxTickedGreenId);
    public static DiscordEmoji GetCheckboxUntickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.CheckboxUntickedGreenId);

    public static DiscordEmoji GetQuestionMark(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.QuestionMarkId);

    public static DiscordEmoji GetGuild(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.GuildId);
    public static DiscordEmoji GetChannel(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.ChannelId);
    public static DiscordEmoji GetUser(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.UserId);
    public static DiscordEmoji GetVoiceState(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.VoiceStateId);
    public static DiscordEmoji GetMessage(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.MessageId);
    public static DiscordEmoji GetInvite(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.InviteId);

    public static DiscordEmoji GetYouTube(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.YouTubeId);
    public static DiscordEmoji GetSoundcloud(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.SoundCloudId);
    public static DiscordEmoji GetAbuseIpDb(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.Emojis.AbuseIPDBId);
}
