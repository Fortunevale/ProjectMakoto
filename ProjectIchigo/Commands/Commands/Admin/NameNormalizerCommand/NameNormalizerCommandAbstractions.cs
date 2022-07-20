namespace ProjectIchigo.Commands.NameNormalizerCommand;

internal class NameNormalizerCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"`Name Normalizer Enabled`: {ctx.Bot._guilds.List[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled.BoolToEmote(ctx.Client)}";
    }
}
