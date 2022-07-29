namespace ProjectIchigo.Commands.EmbedMessageCommand;

internal class EmbedMessageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"`Embed Message Links`: {ctx.Bot._guilds[ctx.Guild.Id].EmbedMessageSettings.UseEmbedding.BoolToEmote(ctx.Client)}";
    }
}
