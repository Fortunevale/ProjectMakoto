// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class CreditsCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx, true))
                return;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Utility.Credits.Fetching, true)
            }.AsLoading(ctx));

            var contributors = await ctx.Bot.GithubClient.Repository.GetAllContributors(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository);
            var contributorsdcs = await ctx.Bot.GithubClient.Repository.GetAllContributors("Aiko-IT-Systems", "DisCatSharp");
            var contributorslava = await ctx.Bot.GithubClient.Repository.GetAllContributors("freyacodes", "Lavalink");

            List<DiscordUser> userlist = new();

            foreach (var b in ctx.Bot.status.TeamMembers.Reverse<ulong>())
                userlist.Add(await ctx.Client.GetUserAsync(b));

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Utility.Credits.Credits, false, false,
                new TVar("BotName", ctx.CurrentUser.GetUsername(), false),
                new TVar("Developer", "<@411950662662881290> ([`TheXorog`](https://github.com/TheXorog))", false),
                new TVar("DiscordStaffList", string.Join(", ", userlist.Select(x => $"{x.Mention} [`{x.GetUsernameWithIdentifier()}`]({x.ProfileUrl})")), false),
                new TVar("GitHubContList", string.Join("\n", contributors.Where(x => !x.Login.Contains("[bot]") && x.Login != "TheXorog").OrderByDescending(x => x.Contributions).Select(x => $"• [`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("Library", "[`DisCatSharp`](https://github.com/Aiko-IT-Systems/DisCatSharp)", false),
                new TVar("LibraryContList", string.Join(", ", contributorsdcs.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("LibraryContCount", $"[{contributorsdcs.Count - 10}](https://github.com/Aiko-IT-Systems/DisCatSharp/graphs/contributors)", false),
                new TVar("MusicModule", $"[`Lavalink`](https://github.com/freyacodes/Lavalink)", false),
                new TVar("MusicContList", string.Join(", ", contributorslava.Take(10).Where(x => !x.Login.Contains("[bot]")).OrderByDescending(x => x.Contributions).Select(x => $"[`{x.Login}`]({x.HtmlUrl})")), false),
                new TVar("MusicContCount", $"[{contributorslava.Count - 10}](https://github.com/freyacodes/Lavalink/graphs/contributors)", false),
                new TVar("PhishingListRepos", $"[`nikolaischunk`](https://github.com/nikolaischunk), [`DevSpen`](https://github.com/DevSpen), [`PoorPocketsMcNewHold`](https://github.com/PoorPocketsMcNewHold), [`sk-cat`](https://github.com/sk-cat) & [`Junortiz`](https://github.com/Junortiz)", false))
            }.AsInfo(ctx));
        });
    }
}