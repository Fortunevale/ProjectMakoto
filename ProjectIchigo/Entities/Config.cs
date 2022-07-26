namespace ProjectIchigo.Entities;

public class Config
{
    public bool IsDev = false;

    public ulong GlobalBanAnnouncementsChannelId = 0;

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
}
