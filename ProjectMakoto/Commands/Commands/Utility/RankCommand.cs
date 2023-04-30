namespace ProjectMakoto.Commands;
internal class RankCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            if (!ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Utility.Leaderboard.Disabled).Replace("{Command}", $"{ctx.Prefix}experiencesettings config")}`"
                }.AsError(ctx, GetString(t.Commands.Utility.Rank.Title)));
                return;
            }

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            long current = (long)Math.Floor((decimal)(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Points - ctx.Bot.experienceHandler.CalculateLevelRequirement(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level - 1)));
            long max = (long)Math.Floor((decimal)(ctx.Bot.experienceHandler.CalculateLevelRequirement(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level) - ctx.Bot.experienceHandler.CalculateLevelRequirement(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level - 1)));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{(victim.Id == ctx.User.Id ? GetString(t.Commands.Utility.Rank.Self) : GetString(t.Commands.Utility.Rank.Other)).Replace("{User}", victim.Mention).Replace("{Level}", ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level.ToEmotes()).Replace("{Points}", ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Points.ToString("N0", CultureInfo.GetCultureInfo("en-US")))}\n\n" +
                              $"**{GetString(t.Commands.Utility.Rank.Progress).Replace("{Level}", (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].Experience.Level + 1).ToEmotes())}**\n" +
                              $"`{Math.Floor((decimal)((decimal)((decimal)current / (decimal)max) * 100)).ToString().Replace(",", ".")}%` " +
                              $"`{GenerateASCIIProgressbar(current, max, 44)}` " +
                              $"`{current}/{max} XP`",
            }.AsInfo(ctx, GetString(t.Commands.Utility.Rank.Title)));
        });
    }
}
