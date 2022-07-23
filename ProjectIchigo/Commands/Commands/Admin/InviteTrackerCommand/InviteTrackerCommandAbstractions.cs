namespace ProjectIchigo.Commands.InviteTrackerCommand;

internal class InviteTrackerCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"`Invite Tracker Enabled`: {ctx.Bot._guilds[ctx.Guild.Id].InviteTrackerSettings.Enabled.BoolToEmote(ctx.Client)}";
    }
}
