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

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Timing {victim.GetUsername()} ({victim.Id}) out..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.AsLoading(ctx);
            await RespondOrEdit(embed: embed);

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
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`The Duration you specified is invalid.`",
                    }.AsError(ctx));
                    return;
                }
            }

            if (DateTime.UtcNow > until || DateTime.UtcNow.AddDays(28) < until)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"``The duration you specified is invalid.``",
                }.AsError(ctx));
                return;
            }

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= victim.GetRoleHighestPosition())
                    throw new Exception();

                await victim.TimeoutAsync(until, $"{ctx.User.GetUsername()} timed user out: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");
                embed.Description = $"{victim.Mention} `was timed out for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCode()}' by` {ctx.User.Mention}`.`\n" +
                                    $"`The time out will end` {until.ToTimestamp()}`.`";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"{victim.Mention} `could not be timed out.`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}