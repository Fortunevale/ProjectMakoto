namespace ProjectIchigo.Entities;

internal class EmojiTemplates
{
    public static DiscordEmoji GetCheckboxTickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxTickedRedEmojiId);
    public static DiscordEmoji GetCheckboxUntickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxUntickedRedEmojiId);
    public static DiscordEmoji GetCheckboxTickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxTickedBlueEmojiId);
    public static DiscordEmoji GetCheckboxUntickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxUntickedBlueEmojiId);
    public static DiscordEmoji GetCheckboxTickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxTickedGreenEmojiId);
    public static DiscordEmoji GetCheckboxUntickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.CheckboxUntickedGreenEmojiId);

    public static DiscordEmoji GetQuestionMark(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.QuestionMarkEmojiId);

    public static DiscordEmoji GetGuild(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.GuildEmojiId);
    public static DiscordEmoji GetChannel(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.ChannelEmojiId);
    public static DiscordEmoji GetUser(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.UserEmojiId);
    public static DiscordEmoji GetVoiceState(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.VoiceStateEmojiId);
    public static DiscordEmoji GetMessage(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.MessageEmojiId);
    public static DiscordEmoji GetInvite(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot.status.LoadedConfig.InviteEmojiId);
}
