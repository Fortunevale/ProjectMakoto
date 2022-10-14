namespace ProjectIchigo.Commands.Commands.Configuration.InviteNotesCommand
{
    internal class InviteNotesCommandAbstractions
    {
        internal static string GetCurrentConfiguration(SharedCommandContext ctx)
        {
            return $"`Invite Notes Enabled: {ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.InviteNotesEnabled.ToEmote(ctx.Client)}";
        }
    }
}
