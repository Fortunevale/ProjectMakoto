// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class ForceSkipCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
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

            if (!ctx.Member.IsDJ(ctx.Bot.status))
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.DjRole, true, new TVar("Role", "DJ")),
                }.AsError(ctx));
                return;
            }

            _ = await conn.StopAsync();

            _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Music.ForceSkip.Skipped, true),
            }.AsSuccess(ctx));
        });
    }
}