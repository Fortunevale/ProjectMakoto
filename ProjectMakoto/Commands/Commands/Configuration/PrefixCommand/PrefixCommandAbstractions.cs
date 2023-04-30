namespace ProjectMakoto.Commands.PrefixCommand;
internal class PrefixCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var pad = GenericExtensions.CalculatePadding(ctx.DbUser, ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.CurrentPrefix, ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.PrefixDisabled);

        return $"⌨ `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.PrefixDisabled).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].PrefixSettings.PrefixDisabled.ToEmote(ctx.Bot)}\n" +
               $"🗝 `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.CurrentPrefix).PadRight(pad)}` : `{ctx.Bot.guilds[ctx.Guild.Id].PrefixSettings.Prefix}`";
    }
}
