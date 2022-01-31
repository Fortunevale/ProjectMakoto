namespace Project_Ichigo.Objects;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.Now;
    internal bool DiscordInitialized { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;

    internal List<ulong> TeamMembers { get; set; } = new();
}
