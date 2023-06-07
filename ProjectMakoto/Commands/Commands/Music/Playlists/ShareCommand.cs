// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ShareCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
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

            string ShareCode = $"{Guid.NewGuid()}";

            if (!Directory.Exists("PlaylistShares"))
                Directory.CreateDirectory("PlaylistShares");

            if (!Directory.Exists($"PlaylistShares/{ctx.User.Id}"))
                Directory.CreateDirectory($"PlaylistShares/{ctx.User.Id}");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Description = GetString(this.t.Commands.Music.Playlists.Share.Shared, true,
                new TVar("Command", $"{ctx.Prefix}playlists load-share {ctx.User.Id} {ShareCode}")),
            }.AsInfo(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

            File.WriteAllText($"PlaylistShares/{ctx.User.Id}/{ShareCode}.json", JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented));
        });
    }
}