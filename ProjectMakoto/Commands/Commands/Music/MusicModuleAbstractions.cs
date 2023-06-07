// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class MusicModuleAbstractions
{
    public static async Task<(List<LavalinkTrack> Tracks, LavalinkLoadResult oriResult, bool Continue)> GetLoadResult(SharedCommandContext ctx, string load)
    {
        var t = ctx.BaseCommand.t;

        if (Regex.IsMatch(load, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+") || load.ToLower().Contains("localhost") || load.ToLower().Contains("127.0.0.1"))
            throw new Exception();

        List<LavalinkTrack> Tracks = new();

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);

        var embed = new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]);
        await ctx.BaseCommand.RespondOrEdit(embed.WithDescription(t.Commands.Music.Play.LookingFor.Get(ctx.DbUser).Build(true, new TVar("Search", load))).AsLoading(ctx));

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
            embed.Description = t.Commands.Music.Play.PlatformSelect.Get(ctx.DbUser).Build(true);
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

            await ctx.BaseCommand.RespondOrEdit(embed.WithDescription(t.Commands.Music.Play.LookingFor.Get(ctx.DbUser).Build(true,
                new TVar("Search", load),
                new TVar("Platform", (Menu1.GetCustomId() == YouTube.CustomId ? "YouTube" : "SoundCloud")))).AsLoading(ctx));

            loadResult = await node.Rest.GetTracksAsync(load, (Menu1.GetCustomId() == YouTube.CustomId ? LavalinkSearchType.Youtube : LavalinkSearchType.SoundCloud));
        }

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
        {
            _logger.LogError("An exception occurred while trying to load lavalink track: {Exception}", loadResult.Exception.Message);
            embed.Description = t.Commands.Music.Play.FailedToLoad.Get(ctx.DbUser).Build(true,
                new TVar("Search", load));
            embed.AsError(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());
            return (null, loadResult, false);
        }
        else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            embed.Description = t.Commands.Music.Play.NoMatches.Get(ctx.DbUser).Build(true,
                new TVar("Search", load));
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
            embed.Description = t.Commands.Music.Play.SearchSuccess.Get(ctx.DbUser).Build(true,
                new TVar("Count", loadResult.Tracks.Count));
            ;
            embed.AsAwaitingInput(ctx);
            await ctx.BaseCommand.RespondOrEdit(embed.Build());

            var UriResult = await ctx.BaseCommand.PromptCustomSelection(loadResult.Tracks
                .Select(x => new DiscordStringSelectComponentOption(x.Title.TruncateWithIndication(100), x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());

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
