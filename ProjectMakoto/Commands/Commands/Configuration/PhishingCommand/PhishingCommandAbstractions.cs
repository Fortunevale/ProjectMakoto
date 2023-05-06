// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.PhishingCommand;
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
