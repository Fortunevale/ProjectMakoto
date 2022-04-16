namespace Project_Ichigo.Objects;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.UtcNow;

    internal List<Task> runningTasks { get; set; } = new();

    internal string DevelopmentServerInvite = "";

    internal bool DiscordInitialized { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;

    internal List<ulong> TeamMembers { get; set; } = new();

    internal long DebugRaised = 0;
    internal long InfoRaised = 0;
    internal long WarnRaised = 0;
    internal long ErrorRaised = 0;
    internal long FatalRaised = 0;

    internal long DataReaderExceptions = 0;
}
