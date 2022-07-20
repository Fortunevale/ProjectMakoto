namespace ProjectIchigo.Commands.Music;

internal class PlayCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string search = (string)arguments["search"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (search.IsNullOrWhiteSpace())
            {
                SendSyntaxError();
                return;
            }

            if (Regex.IsMatch(search, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                throw new Exception();

            var embed = new DiscordEmbedBuilder
            {
                Description = $":arrows_counterclockwise: `Preparing connection..`",
                Color = EmbedColors.Processing,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            await RespondOrEdit(embed);

            try
            {
                await new JoinCommand().ExecuteCommand(ctx, null);
            }
            catch (CancelCommandException)
            {
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();

            embed.Description = $":arrows_counterclockwise: `Looking for '{search}'..`";
            await RespondOrEdit(embed.Build());

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            LavalinkLoadResult loadResult;

            if (Regex.IsMatch(search, Resources.Regex.YouTubeUrl))
                loadResult = await node.Rest.GetTracksAsync(search, LavalinkSearchType.Plain);
            else
                loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                embed.Description = $"❌ `Failed to load '{search}'.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(embed.Build());
                return;
            }
            else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                embed.Description = $"❌ `No matches found for '{search}'.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(embed.Build());
                return;
            }
            else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
            {
                int added = 0;

                foreach (var b in loadResult.Tracks)
                {
                    added++;
                    ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(b.Title, b.Uri.ToString(), ctx.Guild, ctx.User));
                }

                embed.Description = $"✅ `Queued {added} songs from `[`{loadResult.PlaylistInfo.Name}`]({search})`.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue positions", $"{(ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count - added + 1)} - {ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));

                embed.Color = EmbedColors.Success;
                await RespondOrEdit(embed.Build());
            }
            else if (loadResult.LoadResultType == LavalinkLoadResultType.TrackLoaded)
            {
                LavalinkTrack track = loadResult.Tracks.First();

                ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(track.Title, track.Uri.ToString(), ctx.Guild, ctx.User));

                embed.Description = $"✅ `Queued `[`{track.Title}`]({track.Uri})`.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue position", $"{ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));
                embed.AddField(new DiscordEmbedField($"🔼 Uploaded by", $"{track.Author}", true));
                embed.AddField(new DiscordEmbedField($"🕒 Duration", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                embed.Color = EmbedColors.Success;
                await RespondOrEdit(embed.Build());
            }
            else if (loadResult.LoadResultType == LavalinkLoadResultType.SearchResult)
            {
                embed.Description = $"❓ `Found {loadResult.Tracks.Count()} search result(s). Please select the song you want to add below.`";
                await RespondOrEdit(embed.Build());

                string SelectedUri;

                try
                {
                    SelectedUri = await PromptCustomSelection(loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                LavalinkTrack track = loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri);

                ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(track.Title, track.Uri.ToString(), ctx.Guild, ctx.User));

                embed.Description = $"✅ `Queued `[`{track.Title}`]({track.Uri})`.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue position", $"{ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));
                embed.AddField(new DiscordEmbedField($"🔼 Uploaded by", $"{track.Author}", true));
                embed.AddField(new DiscordEmbedField($"🕒 Duration", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                embed.Color = EmbedColors.Success;
                await RespondOrEdit(embed.Build());
            }
            else
            {
                throw new Exception($"Unknown Load Result Type: {loadResult.LoadResultType}");
            }
        });
    }
}