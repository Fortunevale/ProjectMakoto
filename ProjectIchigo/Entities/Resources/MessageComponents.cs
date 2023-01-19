namespace ProjectIchigo.Entities;
internal class MessageComponents
{
    public static DiscordButtonComponent GetCancelButton(User user)
        => new(ButtonStyle.Secondary, "cancel", Bot.loadedTranslations.Common.Cancel.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
    
    public static DiscordButtonComponent GetBackButton(User user)
        => new(ButtonStyle.Secondary, "back", Bot.loadedTranslations.Common.Back.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
}
