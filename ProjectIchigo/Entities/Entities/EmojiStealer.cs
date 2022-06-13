namespace ProjectIchigo.Entities.Entities;
internal class EmojiStealer
{
    public string Name { get; set; }
    public EmojiType Type { get; set; }
    public string Path { get; set; }
    public bool Animated { get; set; }
}

internal enum EmojiType
{
    STICKER,
    EMOJI
}
