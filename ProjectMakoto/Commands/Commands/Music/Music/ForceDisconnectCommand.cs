// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class ForceDisconnectCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            if (!ctx.Member.IsDJ(ctx.Bot.status))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.DjRole, true, new TVar("Role", "DJ")),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
            ctx.Bot.guilds[ctx.Guild.Id].MusicModule = new(ctx.Bot.guilds[ctx.Guild.Id]);

            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

            await RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Music.ForceDisconnect.Disconnected, true),
            }.AsSuccess(ctx));
        });
    }
}