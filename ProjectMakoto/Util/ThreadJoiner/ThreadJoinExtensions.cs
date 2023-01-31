namespace ProjectMakoto.Util;

internal static class ThreadJoinExtensions
{
    public static async void JoinWithQueue(this DiscordThreadChannel channel, ThreadJoinClient client)
        => _ = client.JoinThread(channel);
}
