namespace ProjectIchigo.Commands;
internal class LeaderboardCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int ShowAmount = (int)arguments["ShowAmount"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (!ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.UseExperience)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Experience • {ctx.Guild.Name}" },
                    Color = EmbedColors.Error,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Experience is disabled on this server. Please run '{ctx.Prefix}experiencesettings config' to configure the experience system.`"
                });
                return;
            }

            if (ShowAmount is > 50 or < 3)
            {
                SendSyntaxError();
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Color = EmbedColors.HiddenSidebar,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading,
                    Name = $"Experience Leaderboard"
                },
                Description = $"`Loading Leaderboard, please wait..`",
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };

            await RespondOrEdit(embed: PerformingActionEmbed);

            int count = 0;

            int currentuserplacement = 0;

            foreach (var b in ctx.Bot._guilds[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience.Points))
            {
                currentuserplacement++;
                if (b.Key == ctx.User.Id)
                    break;
            }

            var members = await ctx.Guild.GetAllMembersAsync();

            List<KeyValuePair<string, string>> Board = new();

            foreach (var b in ctx.Bot._guilds[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience.Points))
            {
                try
                {
                    if (!members.Any(x => x.Id == b.Key))
                        continue;

                    DiscordMember bMember = members.First(x => x.Id == b.Key);

                    if (bMember is null)
                        continue;

                    if (bMember.IsBot)
                        continue;

                    if (b.Value.Experience.Points <= 1)
                        break;

                    count++;

                    Board.Add(new KeyValuePair<string, string>("󠂪 󠂪 ", $"**{count.DigitsToEmotes()}**. <@{b.Key}> `{bMember.UsernameWithDiscriminator}` (`Level {b.Value.Experience.Level} with {b.Value.Experience.Points} XP`)"));

                    if (count >= ShowAmount)
                        break;
                }
                catch { }
            }

            var fields = Board.PrepareEmbedFields();

            foreach (var field in fields)
                PerformingActionEmbed.AddField(new DiscordEmbedField(field.Key, field.Value));

            if (count != 0)
            {
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"You're currently on the **{currentuserplacement}.** spot on the leaderboard.";
                await RespondOrEdit(PerformingActionEmbed);
            }
            else
            {
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $":no_entry_sign: `No one on this server has collected enough experience to show up on the leaderboard, get to typing!`";
                await RespondOrEdit(PerformingActionEmbed);
            }
        });
    }
}
