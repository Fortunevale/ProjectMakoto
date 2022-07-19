namespace ProjectIchigo.Commands.AutoUnarchiveCommand;

internal class AutoUnarchiveCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        foreach (var b in ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.ToList())
        {
            if (!ctx.Guild.Channels.ContainsKey(b))
                ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Remove(b);
        }

        return $"{(ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Any() ? string.Join("\n", ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Select(x => $"{ctx.Guild.GetChannel(x).Mention} [`#{ctx.Guild.GetChannel(x).Name}`] (`{x}`)")) : "`No channels defined.`")}";
    }
}
