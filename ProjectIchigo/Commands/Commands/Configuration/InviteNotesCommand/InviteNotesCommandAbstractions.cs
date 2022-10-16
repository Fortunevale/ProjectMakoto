namespace ProjectIchigo.Commands.Commands.Configuration.InviteNotesCommand;

internal class InviteNotesCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"Invite Notes:\n{string.Join('\n', ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.Notes.Select(x => $"{x.Key}: {x.Value.Note.Sanitize()}"))}";
    }
}
