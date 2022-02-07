namespace Project_Ichigo.Helpers;
internal class ColorHelper
{
    public static DiscordColor Error => DiscordColor.DarkRed;
    public static DiscordColor StrongPunishment => DiscordColor.DarkRed;
    public static DiscordColor LightPunishment => DiscordColor.Red;
    public static DiscordColor Loading => DiscordColor.Orange;
    public static DiscordColor Info => DiscordColor.Aquamarine;
    public static DiscordColor Warning => DiscordColor.Orange;
    public static DiscordColor Success => DiscordColor.Green;
    public static DiscordColor HiddenSidebar => new("2f3136");
}
