namespace ProjectMakoto.Commands.GuildLanguage;
internal class GuildLanguageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"🗨 `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.GuildLanguage.Disclaimer)}`\n`{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.GuildLanguage.Response)}`: `{(ctx.Bot.guilds[ctx.Guild.Id].OverrideLocale.IsNullOrWhiteSpace() ? (ctx.Bot.guilds[ctx.Guild.Id].CurrentLocale.IsNullOrWhiteSpace() ? "en (Default)" : $"{ctx.Bot.guilds[ctx.Guild.Id].CurrentLocale} (Discord)") : $"{ctx.Bot.guilds[ctx.Guild.Id].OverrideLocale} (Override)")}`";
    }
}
