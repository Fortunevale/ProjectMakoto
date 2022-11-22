namespace ProjectIchigo.Entities;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.UtcNow;

    internal bool DiscordInitialized { get; set; } = false;
    internal bool DiscordGuildDownloadCompleted { get; set; } = false;
    internal bool DiscordCommandsRegistered { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;
    internal bool DatabaseInitialLoadCompleted { get; set; } = false;

    internal ulong TeamOwner { get; set; } = new();
    internal List<ulong> TeamMembers { get; set; } = new();

    internal long DiscordDisconnections = 0;

    internal Config LoadedConfig { get; set; }

    #region Legacy

    internal string DevelopmentServerInvite
    {
        get
        {
            if (LoadedConfig.SupportServerInvite.IsNullOrWhiteSpace())
                return "Invite not set.";

            return LoadedConfig.SupportServerInvite;
        }
    }

    #endregion
}
