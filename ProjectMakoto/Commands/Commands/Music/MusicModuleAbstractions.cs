// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal static class MusicModuleAbstractions
{
    public static async Task<(List<LavalinkTrack> Tracks, LavalinkTrackLoadingResult oriResult, bool Continue)> GetLoadResult(SharedCommandContext ctx, string searchQuery)
    {
        var t = ctx.t;

        if (Regex.IsMatch(searchQuery, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+") || searchQuery.ToLower().Contains("localhost") || searchQuery.ToLower().Contains("127.0.0.1"))
            throw new Exception();

        List<LavalinkTrack> Tracks = new();

        var lava = ctx.Client.GetLavalink();
        var session = lava.ConnectedSessions.Values.First(x => x.IsConnected);

        var embed = new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]);
        _ = await ctx.BaseCommand.RespondOrEdit(embed.WithDescription(t.Commands.Music.Play.LookingFor.Get(ctx.DbUser).Build(true, new TVar("Search", searchQuery))).AsLoading(ctx));

        LavalinkTrackLoadingResult loadResult;

        if (RegexTemplates.YouTubeUrl.IsMatch(searchQuery))
        {
            if (Regex.IsMatch(searchQuery, @"((\?|&)list=RDMM\w+)(&*)"))
            {
                Group group = Regex.Match(searchQuery, @"((\?|&)list=RDMM\w+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                searchQuery = searchQuery.Replace(value, "");
            }

            if (Regex.IsMatch(searchQuery, @"((\?|&)start_radio=\d+)(&*)"))
            {
                Group group = Regex.Match(searchQuery, @"((\?|&)start_radio=\d+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                searchQuery = searchQuery.Replace(value, "");
            }

            var AndIndex = searchQuery.IndexOf("&");

            if (!searchQuery.Contains('?') && AndIndex != -1)
            {
                searchQuery = searchQuery.Remove(AndIndex, 1);
                searchQuery = searchQuery.Insert(AndIndex, "?");
            }

            loadResult = await session.LoadTracksAsync(LavalinkSearchType.Plain, searchQuery);
        }
        else if (RegexTemplates.SoundcloudUrl.IsMatch(searchQuery))
        {
            loadResult = await session.LoadTracksAsync(LavalinkSearchType.Plain, searchQuery);
        }
        else if (RegexTemplates.BandcampUrl.IsMatch(searchQuery))
        {
            loadResult = await session.LoadTracksAsync(LavalinkSearchType.Plain, searchQuery);
        }
        else
        {
            embed.Description = t.Commands.Music.Play.PlatformSelect.Get(ctx.DbUser).Build(true);
            _ = embed.AsAwaitingInput(ctx);

            var YouTube = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "YouTube", false, new DiscordComponentEmoji(EmojiTemplates.GetYouTube(ctx.Bot)));
            var SoundCloud = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Soundcloud", false, new DiscordComponentEmoji(EmojiTemplates.GetSoundcloud(ctx.Bot)));

            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { YouTube, SoundCloud }));

            var Menu1 = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Menu1.TimedOut)
            {
                ctx.BaseCommand.ModifyToTimedOut();
                return (null, null, false);
            }

            _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            _ = await ctx.BaseCommand.RespondOrEdit(embed.WithDescription(t.Commands.Music.Play.LookingFor.Get(ctx.DbUser).Build(true,
                new TVar("Search", searchQuery),
                new TVar("Platform", (Menu1.GetCustomId() == YouTube.CustomId ? "YouTube" : "SoundCloud")))).AsLoading(ctx));

            loadResult = await session.LoadTracksAsync((Menu1.GetCustomId() == YouTube.CustomId ? LavalinkSearchType.Youtube : LavalinkSearchType.SoundCloud), searchQuery);
        }

        if (loadResult.LoadType == LavalinkLoadResultType.Error)
        {
            _logger.LogError("An exception occurred while trying to load lavalink track.");
            embed.Description = t.Commands.Music.Play.FailedToLoad.Get(ctx.DbUser).Build(true,
                new TVar("Search", searchQuery));
            _ = embed.AsError(ctx);
            _ = await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadType == LavalinkLoadResultType.Empty)
        {
            embed.Description = t.Commands.Music.Play.NoMatches.Get(ctx.DbUser).Build(true,
                new TVar("Search", searchQuery));
            _ = embed.AsError(ctx);
            _ = await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadType == LavalinkLoadResultType.Playlist)
        {
            return (loadResult.GetResultAs<LavalinkPlaylist>().Tracks.ToList(), loadResult, true);
        }
        else if (loadResult.LoadType == LavalinkLoadResultType.Track)
        {
            Tracks.Add(loadResult.GetResultAs<LavalinkTrack>());
            return (Tracks, loadResult, true);
        }
        else if (loadResult.LoadType == LavalinkLoadResultType.Search)
        {
            var searchResults = loadResult.GetResultAs<List<LavalinkTrack>>();

            embed.Description = t.Commands.Music.Play.SearchSuccess.Get(ctx.DbUser).Build(true,
                new TVar("Count", searchResults.Count));
            _ = embed.AsAwaitingInput(ctx);
            _ = await ctx.BaseCommand.RespondOrEdit(embed.Build());

            var UriResult = await ctx.BaseCommand.PromptCustomSelection(searchResults
                .Select(x => new DiscordStringSelectComponentOption(x.Info.Title.TruncateWithIndication(100), x.Info.Uri.ToString(), $"🔼 {x.Info.Author} | 🕒 {x.Info.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());

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

            Tracks.Add(searchResults.First(x => x.Info.Uri.ToString() == UriResult.Result));

            return (Tracks, loadResult, true);
        }
        else
        {
            throw new Exception($"Unknown Load Result Type: {loadResult.LoadType}");
        }
    }
}
