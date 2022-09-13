namespace ProjectIchigo.Commands.AutoCrosspostCommand;

internal class AutoCrosspostCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"🤖 `Exclude Bots             `: {ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.ExcludeBots.ToEmote(ctx.Client)}\n" +
               $"🕒 `Delay before crossposting`: `{TimeSpan.FromSeconds(ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting).GetHumanReadable()}`\n\n" +
               $"{(ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count != 0 ? string.Join("\n\n", ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : "`No Auto Crosspost Channels set up.`")}";
    }
}
