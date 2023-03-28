namespace ProjectMakoto.Entities;

public class ChannelPromptConfiguration
{
    public ChannelConfig CreateChannelOption { get; set; } = null;

    public string? DisableOption { get; set; } = null;

    public class ChannelConfig
    {
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
    }
}
