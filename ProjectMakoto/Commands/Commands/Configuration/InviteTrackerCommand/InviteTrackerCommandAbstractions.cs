namespace ProjectMakoto.Commands.InviteTrackerCommand;

internal class InviteTrackerCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"📲 `Invite Tracker Enabled`: {ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled.ToEmote(ctx.Bot)}";
    }
}
