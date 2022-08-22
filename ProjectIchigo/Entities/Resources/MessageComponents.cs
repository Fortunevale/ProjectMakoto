namespace ProjectIchigo.Entities;
internal class MessageComponents
{
    public static readonly DiscordButtonComponent CancelButton = new(ButtonStyle.Secondary, "cancel", "Cancel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
    public static readonly DiscordButtonComponent BackButton = new(ButtonStyle.Secondary, "back", "Back", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
}
