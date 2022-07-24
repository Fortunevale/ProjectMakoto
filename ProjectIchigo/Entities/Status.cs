namespace ProjectIchigo.Entities;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.UtcNow;

    internal string DevelopmentServerInvite = "";

    internal bool DiscordInitialized { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;
    internal bool DatabaseInitialLoadCompleted { get; set; } = false;

    internal List<ulong> TeamMembers { get; set; } = new();

    internal long WarnRaised = 0;
    internal long ErrorRaised = 0;
    internal long FatalRaised = 0;

    internal long DataReaderExceptions = 0;

    internal Config LoadedConfig { get; set; }
}
