namespace ProjectIchigo.Commands.VcCreatorCommand;

internal class VcCreatorCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"{EmojiTemplates.GetChannel(ctx.Client, ctx.Bot)} `Voice Channel Creator`: {(ctx.Bot.guilds[ctx.Guild.Id].VcCreator.Channel == 0 ? false.ToEmote(ctx.Client) : $"<#{ctx.Bot.guilds[ctx.Guild.Id].VcCreator.Channel}>")}";
    }
}
