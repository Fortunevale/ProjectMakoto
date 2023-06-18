// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class AddToQueueCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            string playlistId = (string)arguments["id"];

            if (!ctx.DbUser.UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title)));
                return;
            }

            UserPlaylist SelectedPlaylist = ctx.DbUser.UserPlaylists.First(x => x.PlaylistId == playlistId);

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Music.Play.Preparing, true),
            }.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title)));

            try
            {
                await new Music.JoinCommand().TransferCommand(ctx, null);
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
                Description = GetString(this.t.Commands.Music.Playlists.AddToQueue.Adding, true,
                new TVar("Name", SelectedPlaylist.PlaylistName),
                new TVar("", SelectedPlaylist.List.Count)),
            }.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title)));

            ctx.DbGuild.MusicModule.SongQueue.AddRange(SelectedPlaylist.List.Select(x => new Lavalink.QueueInfo(x.Title, x.Url, x.Length.Value, ctx.Guild, ctx.User)));

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Music.Play.QueuedMultiple, true,
                new TVar("Count", SelectedPlaylist.List.Count),
                new TVar("Playlist", SelectedPlaylist.PlaylistName))
            }
            .AddField(new DiscordEmbedField($"📜 {GetString(this.t.Commands.Music.Play.QueuePositions)}", $"{(ctx.DbGuild.MusicModule.SongQueue.Count - SelectedPlaylist.List.Count + 1)} - {ctx.DbGuild.MusicModule.SongQueue.Count}"))
            .AsSuccess(ctx, GetString(this.t.Commands.Music.Playlists.Title)));
        });
    }
}