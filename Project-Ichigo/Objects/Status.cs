namespace Project_Ichigo.Objects;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.UtcNow;

    internal List<Task> runningTasks { get; set; } = new();

    internal bool DiscordInitialized { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;

    internal List<ulong> TeamMembers { get; set; } = new();

    internal long ExceptionsRaised = 0;
}
