// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal class RepeatCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Repeat = !ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Repeat;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Repeat ? GetString(t.Commands.Music.Repeat.On, true) : GetString(t.Commands.Music.Repeat.Off, true)),
            }.AsSuccess(ctx));
        });
    }
}