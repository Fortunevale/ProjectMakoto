namespace ProjectIchigo.Entities;

public class Config
{
    public bool IsDev = false;

    public ulong GlobalBanAnnouncementsChannelId = 0;
    public ulong GithubLogChannelId = 0;
    public ulong NewsChannelId = 0;

    public ulong AssetsGuildId = 0;
    public ulong GraphAssetsChannelId = 0;
    public ulong PlaylistAssetsChannelId = 0;
    public ulong UrlSubmissionsChannelId = 0;

    public string DotEmoji = "🅿";

    public string DisabledRepeatEmoji = "🅿";
    public string DisabledShuffleEmoji = "🅿";
    public string PausedEmoji = "🅿";
    public string DisabledPlayEmoji = "🅿";

    public string[] JoinEventsEmojis = { "🙋‍", "🙋‍" };
    public string CuddleEmoji = "🅿";
    public string KissEmoji = "🅿";
    public string SlapEmoji = "🅿";
    public string ProudEmoji = "🅿";
    public string HugEmoji = "🅿";

    public ulong CheckboxTickedRedEmojiId = 970280327253725184;
    public ulong CheckboxUntickedRedEmojiId = 970280299466481745;

    public ulong CheckboxTickedBlueEmojiId = 970278964755038248;
    public ulong CheckboxUntickedBlueEmojiId = 970278964079767574;

    public ulong CheckboxTickedGreenEmojiId = 970280278138449920;
    public ulong CheckboxUntickedGreenEmojiId = 970280278025191454;

    public ulong QuestionMarkEmojiId = 1005464121472466984;

    public ulong ChannelEmojiId = 1005457978884882493;
    public ulong UserEmojiId = 1005457986564665374;
    public ulong VoiceStateEmojiId = 1005457988494037033;
    public ulong MessageEmojiId = 1005457984492679188;
    public ulong GuildEmojiId = 1005457980860403722;
    public ulong InviteEmojiId = 1005457982840123432;

    public ulong DisboardAccountId = 302050872383242240;
}
