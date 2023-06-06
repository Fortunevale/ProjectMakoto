// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ExportCommand : BaseCommand
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

            using (MemoryStream stream = new(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented))))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Description = GetString(this.t.Commands.Music.Playlists.Export.Exported, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
                }.AsInfo(ctx, GetString(this.t.Commands.Music.Playlists.Title))).WithFile($"{Guid.NewGuid().ToString().Replace("-", "").ToLower()}.json", stream));
            }
        });
    }
}