// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.PhishingCommand;
internal sealed class PhishingCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"💀 `Detect Phishing Links   ` : {ctx.DbGuild.PhishingDetection.DetectPhishing.ToEmote(ctx.Bot)}\n" +
               $"⚠ `Redirect Warning        ` : {ctx.DbGuild.PhishingDetection.WarnOnRedirect.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetAbuseIpDb(ctx.Bot)} `AbuseIPDB Reports       ` : {ctx.DbGuild.PhishingDetection.AbuseIpDbReports.ToEmote(ctx.Bot)}\n" +
               $"🔨 `Punishment Type         ` : `{ctx.DbGuild.PhishingDetection.PunishmentType.ToString().ToLower().FirstLetterToUpper()}`\n" +
               $"💬 `Custom Punishment Reason` : `{ctx.DbGuild.PhishingDetection.CustomPunishmentReason}`\n" +
               $"🕒 `Custom Timeout Length   ` : `{ctx.DbGuild.PhishingDetection.CustomPunishmentLength.GetHumanReadable()}`";
    }
}
