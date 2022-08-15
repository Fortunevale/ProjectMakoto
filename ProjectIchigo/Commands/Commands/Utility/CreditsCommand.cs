namespace ProjectIchigo.Commands;

internal class CreditsCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Fetching contributors..`"
            }.SetBotLoading(ctx));

            var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

            var tokenAuth = new Credentials(Secrets.Secrets.GithubToken);
            client.Credentials = tokenAuth;

            var contributors = await client.Repository.GetAllContributors(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository);
            var contributorsdcs = await client.Repository.GetAllContributors("Aiko-IT-Systems", "DisCatSharp");
            var contributorslava = await client.Repository.GetAllContributors("freyacodes", "Lavalink");

            List<DiscordUser> userlist = new();

            foreach (var b in ctx.Bot._status.TeamMembers.Reverse<ulong>())
                userlist.Add(await ctx.Client.GetUserAsync(b));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{ctx.CurrentUser.Username} is mainly being developed and maintained by <@411950662662881290> ([`TheXorog`](https://github.com/TheXorog)).\n\n" +
                              $"These people help me manage and/or test the bot: {string.Join(", ", userlist.Select(x => $"{x.Mention} [`{x.UsernameWithDiscriminator}`]({x.ProfileUrl})"))}\n\n" +
                              $"{ctx.CurrentUser.Username} had some help from the following people:\n\n{string.Join("\n", contributors.Where(x => x.Login is not "dependabot[bot]" and not "TheXorog").OrderByDescending(x => x.Contributions).Select(x => $"• [`{x.Login}`]({x.HtmlUrl})"))}\n\n" +
                              $"{ctx.CurrentUser.Username} wouldn't be possible without [`DisCatSharp`](https://github.com/Aiko-IT-Systems/DisCatSharp), which was made by these people:\n" +
                              $"{string.Join(", ", contributorsdcs.Take(10).Where(x => x.Login != "dependabot[bot]").OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})"))} and [{contributorsdcs.Count - 10} more](https://github.com/Aiko-IT-Systems/DisCatSharp/graphs/contributors).\n\n" +
                              $"{ctx.CurrentUser.Username}'s Music Module uses [`Lavalink`](https://github.com/freyacodes/Lavalink), which was made by these people:\n" +
                              $"{string.Join(", ", contributorslava.Take(10).Where(x => x.Login != "dependabot[bot]").OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})"))} and [{contributorslava.Count - 10} more](https://github.com/freyacodes/Lavalink/graphs/contributors).\n\n" +
                              $"Special thanks go to [`nikolaischunk`](https://github.com/nikolaischunk), [`DevSpen`](https://github.com/DevSpen), [`PoorPocketsMcNewHold`](https://github.com/PoorPocketsMcNewHold), [`sk-cat`](https://github.com/sk-cat) and [`Junortiz`](https://github.com/Junortiz) who publicly provide a list of phishing urls."
            }.SetBotInfo(ctx));
        });
    }
}