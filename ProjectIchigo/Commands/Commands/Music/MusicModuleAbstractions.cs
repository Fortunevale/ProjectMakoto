namespace ProjectIchigo.Commands;

internal class MusicModuleAbstractions
{
    public static async Task<(List<LavalinkTrack> Tracks, LavalinkLoadResult oriResult, bool Continue)> GetLoadResult(SharedCommandContext ctx, string load)
    {
        if (Regex.IsMatch(load, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+") || load.ToLower().Contains("localhost") || load.ToLower().Contains("127.0.0.1"))
            throw new Exception();

        List<LavalinkTrack> Tracks = new();

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);

        var embed = new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]);
        embed.AsLoading(ctx);

        embed.Description = $"`Looking for '{load}'..`";
        await ctx.BaseCommand.RespondOrEdit(embed.Build());

        LavalinkLoadResult loadResult;

        if (RegexTemplates.YouTubeUrl.IsMatch(load))
        {
            if (Regex.IsMatch(load, @"((\?|&)list=RDMM\w+)(&*)"))
            {
                Group group = Regex.Match(load, @"((\?|&)list=RDMM\w+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                load = load.Replace(value, "");
            }
            
            if (Regex.IsMatch(load, @"((\?|&)start_radio=\d+)(&*)"))
            {
                Group group = Regex.Match(load, @"((\?|&)start_radio=\d+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                load = load.Replace(value, "");
            }
                
            var AndIndex = load.IndexOf("&");

            if (!load.Contains('?') && AndIndex != -1)
            {
                load = load.Remove(AndIndex, 1);
                load = load.Insert(AndIndex, "?");
            }

            loadResult = await node.Rest.GetTracksAsync(load, LavalinkSearchType.Plain);
        }
        else if (RegexTemplates.SoundcloudUrl.IsMatch(load))
        {
            loadResult = await node.Rest.GetTracksAsync(load, LavalinkSearchType.Plain);
        }
        else if (RegexTemplates.BandcampUrl.IsMatch(load))
        {
            loadResult = await node.Rest.GetTracksAsync(load, LavalinkSearchType.Plain);
        }
        else
        {
            embed.Description = $"`On what platform do you want to search?`";
            embed.AsAwaitingInput(ctx);
            
            var YouTube = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "YouTube", false, new DiscordComponentEmoji(EmojiTemplates.GetYouTube(ctx.Bot)));
            var SoundCloud = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Soundcloud", false, new DiscordComponentEmoji(EmojiTemplates.GetSoundcloud(ctx.Bot)));

            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { YouTube, SoundCloud }));

            var Menu1 = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Menu1.TimedOut)
            {
                ctx.BaseCommand.ModifyToTimedOut();
                return (null, null, false);
            }

            _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            embed.Description = $"`Looking for '{load}' on {(Menu1.GetCustomId() == YouTube.CustomId ? "YouTube" : "SoundCloud")}..`";
            await ctx.BaseCommand.RespondOrEdit(embed.Build());

            loadResult = await node.Rest.GetTracksAsync(load, (Menu1.GetCustomId() == YouTube.CustomId ? LavalinkSearchType.Youtube : LavalinkSearchType.SoundCloud));
        }

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
        {
            _logger.LogError($"An exception occurred while trying to load lavalink track: {loadResult.Exception.Message} {loadResult.Exception.Severity}");
            embed.Description = $"`Failed to load '{load}'.`";
            embed.AsError(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            embed.Description = $"`No matches found for '{load}'.`";
            embed.AsError(ctx);
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
            embed.AsAwaitingInput(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());

            var UriResult = await ctx.BaseCommand.PromptCustomSelection(loadResult.Tracks
                .Select(x => new DiscordSelectComponentOption(x.Title.TruncateWithIndication(100), x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());

            if (UriResult.TimedOut)
            {
                ctx.BaseCommand.ModifyToTimedOut();
                return (null, loadResult, false);
            }
            else if (UriResult.Cancelled)
            {
                return (null, loadResult, false);
            }
            else if (UriResult.Errored)
            {
                throw UriResult.Exception;
            }

            Tracks.Add(loadResult.Tracks.First(x => x.Uri.ToString() == UriResult.Result));

            return (Tracks, loadResult, true);
        }
        else
        {
            throw new Exception($"Unknown Load Result Type: {loadResult.LoadResultType}");
        }
    }
}
