namespace ProjectIchigo.Commands.TokenDetectionCommand;

internal class TokenDetectionCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"⚠ `Detect Tokens`: {ctx.Bot.guilds[ctx.Guild.Id].TokenLeakDetectionSettings.DetectTokens.ToEmote(ctx.Client)}";
    }
}
