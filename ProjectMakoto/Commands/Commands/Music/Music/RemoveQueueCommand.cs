// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Commands.Music;

internal sealed class RemoveQueueCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var selection = (string)arguments["selection"];

            if (string.IsNullOrWhiteSpace(selection))
            {
                this.SendSyntaxError();
                return;
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var session = lava.ConnectedSessions.Values.First(x => x.IsConnected);
            var conn = session.GetGuildPlayer(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            Lavalink.QueueInfo info = null;

            if (selection.IsDigitsOnly())
            {
                var Index = Convert.ToInt32(selection) - 1;

                if (Index < 0 || Index >= ctx.DbGuild.MusicModule.SongQueue.Count)
                {
                    _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Music.RemoveQueue.OutOfRange, true, new TVar("Min", 1), new TVar("Max", ctx.DbGuild.MusicModule.SongQueue.Count)),
                    }.AsError(ctx));
                    return;
                }

                info = ctx.DbGuild.MusicModule.SongQueue[Index];
            }
            else
            {
                if (!ctx.DbGuild.MusicModule.SongQueue.Any(x => x.VideoTitle.ToLower() == selection.ToLower()))
                {
                    _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Music.RemoveQueue.NoSong, true),
                    }.AsError(ctx));
                    return;
                }

                info = ctx.DbGuild.MusicModule.SongQueue.First(x => x.VideoTitle.ToLower() == selection.ToLower());
            }

            if (info is null)
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.RemoveQueue.NoSong, true),
                }.AsError(ctx));
                return;
            }

            _ = ctx.DbGuild.MusicModule.SongQueue.Remove(info);

            _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.RemoveQueue.Removed, true, new TVar("Track", $"`[`{info.VideoTitle}`]({info.Url})`")),
            }.AsSuccess(ctx));
        });
    }
}