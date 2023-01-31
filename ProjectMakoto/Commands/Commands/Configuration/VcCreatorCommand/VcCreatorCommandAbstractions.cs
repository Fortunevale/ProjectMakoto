namespace ProjectMakoto.Commands.VcCreatorCommand;

internal class VcCreatorCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"{EmojiTemplates.GetChannel(ctx.Bot)} `Voice Channel Creator`: {(ctx.Bot.guilds[ctx.Guild.Id].VcCreator.Channel == 0 ? false.ToEmote(ctx.Bot) : $"<#{ctx.Bot.guilds[ctx.Guild.Id].VcCreator.Channel}>")}";
    }
}
