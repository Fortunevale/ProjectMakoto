namespace ProjectIchigo.Commands;

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
                Description = $"`{GetString(t.Commands.Credits.Fetching)}`"
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
                Description = GetString(t.Commands.Credits.Credits)
                .Replace("{BotName}", ctx.CurrentUser.Username)
                .Replace("{Developer}", "<@411950662662881290> ([`TheXorog`](https://github.com/TheXorog))")
                .Replace("{DiscordStaffList}", string.Join(", ", userlist.Select(x => $"{x.Mention} [`{x.UsernameWithDiscriminator}`]({x.ProfileUrl})")))
                .Replace("{GitHubContList}", string.Join("\n", contributors.Where(x => !x.Login.Contains("[bot]") && x.Login != "TheXorog").OrderByDescending(x => x.Contributions).Select(x => $"• [`{x.Login}`]({x.HtmlUrl})")))
                .Replace("{Library}", "[`DisCatSharp`](https://github.com/Aiko-IT-Systems/DisCatSharp)")
                .Replace("{LibraryContList}", string.Join(", ", contributorsdcs.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")))
                .Replace("{LibraryContCount}", $"[{contributorsdcs.Count - 10}](https://github.com/Aiko-IT-Systems/DisCatSharp/graphs/contributors)")
                .Replace("{MusicModule}", $"[`Lavalink`](https://github.com/freyacodes/Lavalink)")
                .Replace("{MusicContList}", string.Join(", ", contributorslava.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")))
                .Replace("{MusicContCount}", $"[{contributorslava.Count - 10}](https://github.com/freyacodes/Lavalink/graphs/contributors)")
                .Replace("{PhishingListRepos}", $"[`nikolaischunk`](https://github.com/nikolaischunk), [`DevSpen`](https://github.com/DevSpen), [`PoorPocketsMcNewHold`](https://github.com/PoorPocketsMcNewHold), [`sk-cat`](https://github.com/sk-cat) & [`Junortiz`](https://github.com/Junortiz)")
                .Build()
            }.AsBotInfo(ctx));
        });
    }
}