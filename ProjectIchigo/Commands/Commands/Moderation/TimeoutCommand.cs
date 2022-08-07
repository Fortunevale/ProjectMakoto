namespace ProjectIchigo.Commands;

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
            catch (NotFoundException)
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
                Description = $"`Timing {victim.UsernameWithDiscriminator} ({victim.Id}) out..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.SetLoading(ctx);
            await RespondOrEdit(embed: embed);

            if (string.IsNullOrWhiteSpace(duration))
                duration = "30m";

            if (!DateTime.TryParse(duration, out DateTime until))
            {
                try
                {
                    switch (duration[^1..])
                    {
                        case "Y":
                            until = DateTime.UtcNow.AddYears(Convert.ToInt32(duration.Replace("Y", "")));
                            break;
                        case "M":
                            until = DateTime.UtcNow.AddMonths(Convert.ToInt32(duration.Replace("M", "")));
                            break;
                        case "d":
                            until = DateTime.UtcNow.AddDays(Convert.ToInt32(duration.Replace("d", "")));
                            break;
                        case "h":
                            until = DateTime.UtcNow.AddHours(Convert.ToInt32(duration.Replace("h", "")));
                            break;
                        case "m":
                            until = DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration.Replace("m", "")));
                            break;
                        case "s":
                            until = DateTime.UtcNow.AddSeconds(Convert.ToInt32(duration.Replace("s", "")));
                            break;
                        default:
                            until = DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration));
                            return;
                    }
                }
                catch (Exception)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"The Duration you specified is invalid.",
                    }.SetError(ctx));
                    return;
                }
            }

            if (DateTime.UtcNow > until)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"❌ `The duration you specified is invalid.`",
                }.SetError(ctx));
                return;
            }

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= victim.GetRoleHighestPosition())
                    throw new Exception();

                await victim.TimeoutAsync(until, $"{ctx.User.UsernameWithDiscriminator} timed user out: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");
                embed.Description = $"{victim.Mention} `was timed out for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCodeBlock()}' by` {ctx.User.Mention}`.`\n" +
                                    $"`The time out will last for {until.GetTimespanUntil().GetHumanReadable()}.`";
                embed = embed.SetSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"{victim.Mention} `could not be timed out.`";
                embed = embed.SetError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}