namespace ProjectIchigo.Commands;
internal class RankCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            if (!ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.UseExperience)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Experience is disabled on this server. Please run '{ctx.Prefix}experiencesettings config' to configure the experience system.`"
                }.SetError(ctx, "Experience"));
                return;
            }

            if (victim is null)
                victim = ctx.User;

            victim = await victim.GetFromApiAsync();

            long current = (long)Math.Floor((decimal)(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Points - ctx.Bot._experienceHandler.CalculateLevelRequirement(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level - 1)));
            long max = (long)Math.Floor((decimal)(ctx.Bot._experienceHandler.CalculateLevelRequirement(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level) - ctx.Bot._experienceHandler.CalculateLevelRequirement(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level - 1)));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{(victim.Id == ctx.User.Id ? "You're" : $"{victim.Mention} is")} currently **Level {ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level.DigitsToEmotes()} with `{ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Points.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}` XP**\n\n" +
                              $"**Level {(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level + 1).DigitsToEmotes()} Progress**\n" +
                              $"`{Math.Floor((decimal)((decimal)((decimal)current / (decimal)max) * 100)).ToString().Replace(",", ".")}%` " +
                              $"`{GenerateASCIIProgressbar(current, max, 44)}` " +
                              $"`{current}/{max} XP`",
            }.SetInfo(ctx, "Experience"));
        });
    }
}
