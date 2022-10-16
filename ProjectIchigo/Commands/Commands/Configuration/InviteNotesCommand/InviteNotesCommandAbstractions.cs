namespace ProjectIchigo.Commands.InviteNotesCommand;

internal class InviteNotesCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Any())
            return "`No Invite Notes defined.`";

        return $"{string.Join('\n', ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Select(x => $"> `{x.Key}`\n{x.Value.Note.Sanitize()}"))}";
    }
}
