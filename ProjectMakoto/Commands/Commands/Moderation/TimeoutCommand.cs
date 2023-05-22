// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class TimeoutCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ModerateMembers) && await CheckOwnPermissions(Permissions.ModerateMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMember victim;
            string duration = (string)arguments["duration"];
            string reason = (string)arguments["reason"];

            try
            {
                victim = await ((DiscordUser)arguments["victim"]).ConvertToMember(ctx.Guild);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                SendNoMemberError();
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            var CommandKey = t.Commands.Moderation.Timeout;

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.TimingOut, true, new TVar("Victim", victim.Mention)))
                .AsLoading(ctx));

            if (string.IsNullOrWhiteSpace(duration))
                duration = "30m";

            if (!DateTime.TryParse(duration, out DateTime until))
            {
                try
                {
                    until = duration[^1..] switch
                    {
                        "Y" => DateTime.UtcNow.AddYears(Convert.ToInt32(duration.Replace("Y", ""))),
                        "M" => DateTime.UtcNow.AddMonths(Convert.ToInt32(duration.Replace("M", ""))),
                        "d" => DateTime.UtcNow.AddDays(Convert.ToInt32(duration.Replace("d", ""))),
                        "h" => DateTime.UtcNow.AddHours(Convert.ToInt32(duration.Replace("h", ""))),
                        "m" => DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration.Replace("m", ""))),
                        "s" => DateTime.UtcNow.AddSeconds(Convert.ToInt32(duration.Replace("s", ""))),
                        _ => DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration)),
                    };
                }
                catch (Exception)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(GetString(CommandKey.Invalid, true))
                        .AsError(ctx));
                    return;
                }
            }

            if (DateTime.UtcNow > until || DateTime.UtcNow.AddDays(28) < until)
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.Invalid, true))
                    .AsError(ctx));
                return;
            }

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= victim.GetRoleHighestPosition())
                    throw new Exception();

                await victim.TimeoutAsync(until, GetGuildString(CommandKey.AuditLog, new TVar("Reason", (reason.IsNullOrWhiteSpace() ? "No reason provided." : reason))));

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.TimedOut, true, new TVar("Victim", victim.Mention), new TVar("Reason", reason.IsNullOrWhiteSpace() ? "No reason provided" : reason)))
                    .AsSuccess(ctx));
            }
            catch (Exception)
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.Failed, true, new TVar("Victim", victim.Mention)))
                    .AsSuccess(ctx));
            }
        });
    }
}