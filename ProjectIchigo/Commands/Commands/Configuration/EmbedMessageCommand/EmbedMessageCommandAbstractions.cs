namespace ProjectIchigo.Commands.EmbedMessageCommand;

internal class EmbedMessageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💬 `Embed Message Links`: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding.ToEmote(ctx.Bot)}\n" +
               $"🤖 `Embed Github Code  `: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding.ToEmote(ctx.Bot)}";
    }
}
