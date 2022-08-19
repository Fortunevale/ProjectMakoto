namespace ProjectIchigo.Commands.PhishingCommand;
internal class PhishingCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💀 `Detect Phishing Links   ` : {ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing.BoolToEmote(ctx.Client)}\n" +
               $"⚠ `Redirect Warning        ` : {ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect.BoolToEmote(ctx.Client)}\n" +
               $"🔨 `Punishment Type         ` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
               $"💬 `Custom Punishment Reason` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason}`\n" +
               $"🕒 `Custom Timeout Length   ` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength.GetHumanReadable()}`";
    }
}
