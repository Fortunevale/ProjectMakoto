namespace ProjectIchigo.Commands.NameNormalizerCommand;

internal class NameNormalizerCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💬 `Name Normalizer Enabled`: {ctx.Bot.guilds[ctx.Guild.Id].NameNormalizerSettings.NameNormalizerEnabled.BoolToEmote(ctx.Client)}";
    }
}
