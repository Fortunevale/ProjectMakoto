namespace ProjectIchigo.Commands.PhishingCommand;
internal class PhishingCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💀 `Detect Phishing Links   ` : {ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.DetectPhishing.ToEmote(ctx.Bot)}\n" +
               $"⚠ `Redirect Warning        ` : {ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.WarnOnRedirect.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetAbuseIpDb(ctx.Bot)} `AbuseIPDB Reports       ` : {ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.AbuseIpDbReports.ToEmote(ctx.Bot)}\n" +
               $"🔨 `Punishment Type         ` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
               $"💬 `Custom Punishment Reason` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentReason}`\n" +
               $"🕒 `Custom Timeout Length   ` : `{ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentLength.GetHumanReadable()}`";
    }
}
