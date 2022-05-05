namespace Project_Ichigo.Secrets.Dummy;

// This is a dummy file. When actually deploying, please remove "Dummy." at the beginning of the file name and fill in information.

internal class Secrets
{
    internal static readonly protected string LavalinkUrl = "";
    internal static readonly protected int LavalinkPort = 0;
    internal static readonly protected string LavalinkPassword = "";


    internal static readonly protected string DatabaseUrl = "";
    internal static readonly protected int DatabasePort = 0;
#if DEBUG
    internal static readonly protected string MainDatabaseName = "";
#endif
#if !DEBUG
    internal static readonly protected string MainDatabaseName = "";
#endif
#if DEBUG
    internal static readonly protected string GuildDatabaseName = "";
#endif
#if !DEBUG
    internal static readonly protected string GuildDatabaseName = "";
#endif
    internal static readonly protected string DatabaseUserName = "";
    internal static readonly protected string DatabasePassword = "";

    internal static readonly protected string GithubToken = "";
    internal static readonly protected DateTime GithubTokenExperiation = new(2000, 01, 01, 01, 00, 00, DateTimeKind.Utc);
}
