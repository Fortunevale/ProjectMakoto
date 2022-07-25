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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
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
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = ctx.Guild.IconUrl,
                            Name = ctx.Guild.Name
                        },
                        Description = $"The Duration you specified is invalid.",
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Color = EmbedColors.Error
                    });
                    return;
                }
            }

            if (DateTime.UtcNow > until)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = ctx.Guild.IconUrl,
                        Name = ctx.Guild.Name
                    },
                    Description = $"The Duration you specified is invalid.",
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Color = EmbedColors.Error
                });
                return;
            }

            try
            {
                if (ctx.Member.GetHighestPosition() <= victim.GetHighestPosition())
                    throw new Exception();

                await victim.TimeoutAsync(until, $"{ctx.User.UsernameWithDiscriminator} timed user out: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");
                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"✅ `{victim.UsernameWithDiscriminator} ({victim.Id}) was timed out for {until.GetTotalSecondsUntil().GetHumanReadable(TimeFormat.HOURS)}.`";
            }
            catch (Exception)
            {
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"❌ `{victim.UsernameWithDiscriminator} ({victim.Id}) couldn't be timed out.`";
            }

            await RespondOrEdit(embed);
        });
    }
}