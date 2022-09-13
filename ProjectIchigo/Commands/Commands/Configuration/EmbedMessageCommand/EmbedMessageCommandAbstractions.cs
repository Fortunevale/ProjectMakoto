namespace ProjectIchigo.Commands.EmbedMessageCommand;

internal class EmbedMessageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💬 `Embed Message Links`: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessageSettings.UseEmbedding.ToEmote(ctx.Client)}\n" +
               $"🤖 `Embed Github Code  `: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessageSettings.UseGithubEmbedding.ToEmote(ctx.Client)}";
    }
}
