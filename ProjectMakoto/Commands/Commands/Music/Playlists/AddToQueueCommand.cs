namespace ProjectMakoto.Commands.Playlists;

internal class AddToQueueCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            string playlistId = (string)arguments["id"];

            if (!ctx.Bot.users[ctx.Member.Id].UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title)));
                return;
            }

            UserPlaylist SelectedPlaylist = ctx.Bot.users[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == playlistId);

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.Play.Preparing, true),
            }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title)));

            try
            {
                await new Music.JoinCommand().ExecuteCommand(ctx, null);
            }
            catch (CancelException)
            {
                DeleteOrInvalidate();
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.Playlists.AddToQueue.Adding, true, 
                new TVar("Name", SelectedPlaylist.PlaylistName),
                new TVar("", SelectedPlaylist.List.Count)),
            }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title)));

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.AddRange(SelectedPlaylist.List.Select(x => new Lavalink.QueueInfo(x.Title, x.Url, x.Length.Value, ctx.Guild, ctx.User)));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.Play.QueuedMultiple, true, 
                new TVar("Count", SelectedPlaylist.List.Count),
                new TVar("Playlist", SelectedPlaylist.PlaylistName))
            }
            .AddField(new DiscordEmbedField($"📜 {GetString(t.Commands.Music.Play.QueuePositions)}", $"{(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count - SelectedPlaylist.List.Count + 1)} - {ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count}"))
            .AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title)));
        });
    }
}