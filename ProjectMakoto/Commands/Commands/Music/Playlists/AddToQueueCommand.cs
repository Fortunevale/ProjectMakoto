// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Commands.Playlists;

internal sealed class AddToQueueCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var playlistId = (string)arguments["id"];

            if (!ctx.DbUser.UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));
                return;
            }

            var SelectedPlaylist = ctx.DbUser.UserPlaylists.First(x => x.PlaylistId == playlistId);

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.Play.Preparing, true),
            }.AsLoading(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

            try
            {
                await new Music.JoinCommand().TransferCommand(ctx, null);
            }
            catch (CancelException)
            {
                this.DeleteOrInvalidate();
                return;
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.Playlists.AddToQueue.Adding, true,
                new TVar("Name", SelectedPlaylist.PlaylistName),
                new TVar("", SelectedPlaylist.List.Length)),
            }.AsLoading(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

            ctx.DbGuild.MusicModule.SongQueue = ctx.DbGuild.MusicModule.SongQueue.AddRange(SelectedPlaylist.List.Select(x => new Lavalink.QueueInfo(x.Title, x.Url, x.Length.Value, ctx.Guild, ctx.User)));

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.Play.QueuedMultiple, true,
                new TVar("Count", SelectedPlaylist.List.Length),
                new TVar("Playlist", SelectedPlaylist.PlaylistName))
            }
            .AddField(new DiscordEmbedField($"📜 {this.GetString(this.t.Commands.Music.Play.QueuePositions)}", $"{(ctx.DbGuild.MusicModule.SongQueue.Length - SelectedPlaylist.List.Length + 1)} - {ctx.DbGuild.MusicModule.SongQueue.Length}"))
            .AsSuccess(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));
        });
    }
}