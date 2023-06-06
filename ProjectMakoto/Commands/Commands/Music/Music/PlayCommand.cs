﻿// Project Makoto
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
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckVoiceState() && await CheckOwnPermissions(Permissions.UseVoice) && await CheckOwnPermissions(Permissions.UseVoiceDetection));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string search = (string)arguments["search"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (search.IsNullOrWhiteSpace())
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Music.Play.Preparing, true),
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                await new JoinCommand().TransferCommand(ctx, null);
            }
            catch (CancelException)
            {
                return;
            }

            var (Tracks, oriResult, Continue) = await MusicModuleAbstractions.GetLoadResult(ctx, search);

            await RespondOrEdit(embed);

            try
            {
                await new JoinCommand().TransferCommand(ctx, null);
            }
            catch (CancelException)
            {
                return;
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!Continue || !Tracks.IsNotNullAndNotEmpty())
            {
                DeleteOrInvalidate();
                return;
            }

            if (Tracks.Count > 1)
            {
                int added = 0;

                foreach (var b in Tracks)
                {
                    added++;
                    ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Add(new(b.Title, b.Uri.ToString(), b.Length, ctx.Guild, ctx.User));
                }

                embed.Description = GetString(this.t.Commands.Music.Play.QueuedMultiple, true,
                    new TVar("Count", added),
                    new TVar("Playlist", $"`[`{oriResult.PlaylistInfo.Name}`]({search})`"));

                embed.AddField(new DiscordEmbedField($"📜 {GetString(this.t.Commands.Music.Play.QueuePositions)}", $"{(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count - added + 1)} - {ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count}", true));

                embed.AsSuccess(ctx);
                await ctx.BaseCommand.RespondOrEdit(embed);
            }
            else if (Tracks.Count == 1)
            {
                var track = Tracks[0];

                ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Add(new(track.Title, track.Uri.ToString(), track.Length, ctx.Guild, ctx.User));

                embed.Description = GetString(this.t.Commands.Music.Play.QueuedSingle, true,
                    new TVar("Track", $"`[`{track.Title}`]({track.Uri})`"));

                embed.AddField(new DiscordEmbedField($"📜 {GetString(this.t.Commands.Music.Play.QueuePosition)}", $"{ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count}", true));
                embed.AddField(new DiscordEmbedField($"🔼 {GetString(this.t.Commands.Music.Play.Uploader)}", $"{track.Author}", true));
                embed.AddField(new DiscordEmbedField($"🕒 {GetString(this.t.Commands.Music.Play.Duration)}", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                embed.AsSuccess(ctx);
                await ctx.BaseCommand.RespondOrEdit(embed.Build());
            }
        });
    }
}