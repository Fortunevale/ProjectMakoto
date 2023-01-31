namespace ProjectMakoto.Entities;

public class ReactionRoleEntry
{
    public string UUID = Guid.NewGuid().ToString();
    public ulong EmojiId { get; set; }
    public string EmojiName { get; set; }

    public DiscordEmoji GetEmoji(DiscordClient client)
    {
        if (EmojiId == 0)
            return DiscordEmoji.FromName(client, $":{EmojiName.Remove(EmojiName.LastIndexOf(":"), EmojiName.Length - EmojiName.LastIndexOf(":"))}:");

        return DiscordEmoji.FromGuildEmote(client, EmojiId);
    }

    public ulong RoleId { get; set; }
    public ulong ChannelId { get; set; }
}
