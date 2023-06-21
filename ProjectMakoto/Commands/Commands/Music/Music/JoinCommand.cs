// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class JoinCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckVoiceState() && await CheckOwnPermissions(Permissions.UseVoice) && await CheckOwnPermissions(Permissions.UseVoiceDetection));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            bool Announce = arguments?.ContainsKey("announce") ?? false;

            if (Announce)
                if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                    return;

            var lava = ctx.Client.GetLavalink();

            while (!lava.ConnectedNodes.Values.Any(x => x.IsConnected))
                await Task.Delay(1000);

            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                if (!lava.ConnectedNodes.Any())
                {
                    throw new Exception("Lavalink connection isn't established.");
                }

                conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                ctx.DbGuild.MusicModule.QueueHandler(ctx.Bot, ctx.Client, node, conn);

                if (Announce)
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Join.Joined, true),
                    }.AsSuccess(ctx));
                return;
            }

            if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Join.AlreadyUsed, true),
                }.AsError(ctx));

                throw new CancelException();
            }

            if (ctx.Member.VoiceState.Channel.Id != conn.Channel.Id)
            {
                await conn.DisconnectAsync();
                conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
            }

            if (Announce)
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Join.Joined, true),
                }.AsSuccess(ctx));
        });
    }
}