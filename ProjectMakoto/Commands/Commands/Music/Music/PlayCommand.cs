// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class PlayCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckVoiceState() && await this.CheckOwnPermissions(Permissions.UseVoice) && await this.CheckOwnPermissions(Permissions.UseVoiceDetection));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var search = (string)arguments["search"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (search.IsNullOrWhiteSpace())
            {
                this.SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.Play.Preparing, true),
            }.AsLoading(ctx);
            _ = await this.RespondOrEdit(embed);

            try
            {
                await new JoinCommand().TransferCommand(ctx, null);
            }
            catch (CancelException)
            {
                return;
            }

            var (Tracks, oriResult, Continue) = await MusicModuleAbstractions.GetLoadResult(ctx, search);


            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!Continue || !Tracks.IsNotNullAndNotEmpty())
            {
                return;
            }

            _ = await this.RespondOrEdit(embed);

            try
            {
                await new JoinCommand().TransferCommand(ctx, null);
            }
            catch (CancelException)
            {
                return;
            }

            if (Tracks.Count > 1)
            {
                var added = 0;

                foreach (var b in Tracks)
                {
                    added++;
                    ctx.DbGuild.MusicModule.SongQueue = ctx.DbGuild.MusicModule.SongQueue.Add(new(b.Info.Title, b.Info.Uri.ToString(), b.Info.Length, ctx.Guild, ctx.User));
                }

                embed.Description = this.GetString(this.t.Commands.Music.Play.QueuedMultiple, true,
                    new TVar("Count", added),
                    new TVar("Playlist", new EmbeddedLink(search, oriResult.GetResultAs<LavalinkPlaylist>().Info.Name)));

                _ = embed.AddField(new DiscordEmbedField($"📜 {this.GetString(this.t.Commands.Music.Play.QueuePositions)}", $"{(ctx.DbGuild.MusicModule.SongQueue.Length - added + 1)} - {ctx.DbGuild.MusicModule.SongQueue.Length}", true));

                _ = embed.AsSuccess(ctx);
                _ = await ctx.BaseCommand.RespondOrEdit(embed);
            }
            else if (Tracks.Count == 1)
            {
                var track = Tracks[0];

                ctx.DbGuild.MusicModule.SongQueue = ctx.DbGuild.MusicModule.SongQueue.Add(new(track.Info.Title, track.Info.Uri.ToString(), track.Info.Length, ctx.Guild, ctx.User));

                embed.Description = this.GetString(this.t.Commands.Music.Play.QueuedSingle, true,
                    new TVar("Track", new EmbeddedLink(track.Info.Uri.ToString(), track.Info.Title)));

                _ = embed.AddField(new DiscordEmbedField($"📜 {this.GetString(this.t.Commands.Music.Play.QueuePosition)}", $"{ctx.DbGuild.MusicModule.SongQueue.Length}", true));
                _ = embed.AddField(new DiscordEmbedField($"🔼 {this.GetString(this.t.Commands.Music.Play.Uploader)}", $"{track.Info.Author}", true));
                _ = embed.AddField(new DiscordEmbedField($"🕒 {this.GetString(this.t.Commands.Music.Play.Duration)}", $"{track.Info.Length.GetHumanReadable(TimeFormat.Minutes)}", true));

                _ = embed.AsSuccess(ctx);
                _ = await ctx.BaseCommand.RespondOrEdit(embed.Build());
            }
        });
    }
}