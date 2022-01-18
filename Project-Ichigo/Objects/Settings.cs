namespace Project_Ichigo.Objects;

internal class Settings
{
    internal static Dictionary<ulong, ServerSettings> Servers = new();

    internal class ServerSettings
    {
        public ulong MemberRoleId { get; set; } = 0;
    }
}
