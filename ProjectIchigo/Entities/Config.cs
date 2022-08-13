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

    public ulong ChannelEmojiId = 1005612865975238706;
    public ulong UserEmojiId = 1005612863051800746;
    public ulong VoiceStateEmojiId = 1005612864469487638;
    public ulong MessageEmojiId = 1005612861676077166;
    public ulong GuildEmojiId = 1005612867577458729;
    public ulong InviteEmojiId = 1005612860333899859;

    public ulong DisboardAccountId = 302050872383242240;

    public bool UseLavalinkAutoUpdater = false;
    public bool LavalinkDownloadPreRelease = true;
    public string LavalinkJarFolderPath = "";
}
