namespace ProjectMakoto.Entities;

internal class ChannelPromptConfiguration
{
    internal ChannelConfig CreateChannelOption { get; set; } = null;

    internal string? DisableOption { get; set; } = null;

    internal class ChannelConfig
    {
        internal string Name { get; set; }
        internal ChannelType ChannelType { get; set; }
    }
}
