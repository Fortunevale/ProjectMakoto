namespace ProjectIchigo.Commands;

internal class MusicModuleAbstractions
{
    public static async Task<(List<LavalinkTrack> Tracks, LavalinkLoadResult oriResult, bool Continue)> GetLoadResult(SharedCommandContext ctx, string load)
    {
        if (Regex.IsMatch(load, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
            throw new Exception();

        List<LavalinkTrack> Tracks = new();

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();

        var embed = new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]);
        embed.SetLoading(ctx);

        embed.Description = $"`Looking for '{load}'..`";
        await ctx.BaseCommand.RespondOrEdit(embed.Build());

        LavalinkLoadResult loadResult;

        if (Regex.IsMatch(load, Resources.Regex.YouTubeUrl))
            loadResult = await node.Rest.GetTracksAsync(load, LavalinkSearchType.Plain);
        else
            loadResult = await node.Rest.GetTracksAsync(load);

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
        {
            embed.Description = $"`Failed to load '{load}'.`";
            embed.SetError(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            embed.Description = $"`No matches found for '{load}'.`";
            embed.SetError(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
        {
            return (loadResult.Tracks.ToList(), loadResult, true);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.TrackLoaded)
        {
            Tracks.Add(loadResult.Tracks.First());

            return (Tracks, loadResult, true);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.SearchResult)
        {
            embed.Description = $"`Found {loadResult.Tracks.Count()} load result(s). Please select the song you want to add below.`";
            embed.SetAwaitingInput(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());

            string SelectedUri;

            try
            {
                SelectedUri = await ctx.BaseCommand.PromptCustomSelection(loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());
            }
            catch (ArgumentException)
            {
                ctx.BaseCommand.ModifyToTimedOut();
                return (null, loadResult, false);
            }

            Tracks.Add(loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri));

            return (Tracks, loadResult, true);
        }
        else
        {
            throw new Exception($"Unknown Load Result Type: {loadResult.LoadResultType}");
        }
    }
}
