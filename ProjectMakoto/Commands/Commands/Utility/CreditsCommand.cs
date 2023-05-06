namespace ProjectMakoto.Commands;

internal class CreditsCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx, true))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.Credits.Fetching, true)
            }.AsBotLoading(ctx));

            var client = new GitHubClient(new ProductHeaderValue("Project-Makoto"));

            var tokenAuth = new Credentials(ctx.Bot.status.LoadedConfig.Secrets.Github.Token);
            client.Credentials = tokenAuth;

            var contributors = await client.Repository.GetAllContributors(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository);
            var contributorsdcs = await client.Repository.GetAllContributors("Aiko-IT-Systems", "DisCatSharp");
            var contributorslava = await client.Repository.GetAllContributors("freyacodes", "Lavalink");

            List<DiscordUser> userlist = new();

            foreach (var b in ctx.Bot.status.TeamMembers.Reverse<ulong>())
                userlist.Add(await ctx.Client.GetUserAsync(b));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.Credits.Credits, false, false,
                new TVar("BotName", ctx.CurrentUser.Username, false),
                new TVar("Developer", "<@411950662662881290> ([`TheXorog`](https://github.com/TheXorog))", false),
                new TVar("DiscordStaffList", string.Join(", ", userlist.Select(x => $"{x.Mention} [`{x.GetUsername()}`]({x.ProfileUrl})")), false),
                new TVar("GitHubContList", string.Join("\n", contributors.Where(x => !x.Login.Contains("[bot]") && x.Login != "TheXorog").OrderByDescending(x => x.Contributions).Select(x => $"• [`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("Library", "[`DisCatSharp`](https://github.com/Aiko-IT-Systems/DisCatSharp)", false),
                new TVar("LibraryContList", string.Join(", ", contributorsdcs.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("LibraryContCount", $"[{contributorsdcs.Count - 10}](https://github.com/Aiko-IT-Systems/DisCatSharp/graphs/contributors)", false),
                new TVar("MusicModule", $"[`Lavalink`](https://github.com/freyacodes/Lavalink)", false),
                new TVar("MusicContList", string.Join(", ", contributorslava.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("MusicContCount", $"[{contributorslava.Count - 10}](https://github.com/freyacodes/Lavalink/graphs/contributors)", false),
                new TVar("PhishingListRepos", $"[`nikolaischunk`](https://github.com/nikolaischunk), [`DevSpen`](https://github.com/DevSpen), [`PoorPocketsMcNewHold`](https://github.com/PoorPocketsMcNewHold), [`sk-cat`](https://github.com/sk-cat) & [`Junortiz`](https://github.com/Junortiz)", false))
            }.AsBotInfo(ctx));
        });
    }
}