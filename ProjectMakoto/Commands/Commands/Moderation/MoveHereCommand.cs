// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class MoveHereCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.MoveMembers) && await this.CheckOwnPermissions(Permissions.MoveMembers) && await this.CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var oldChannel = (DiscordChannel)arguments["oldChannel"];

            var CommandKey = this.t.Commands.Moderation.Move;

            if (oldChannel.Type != ChannelType.Voice)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.NotAVc, true))
                    .AsError(ctx));
                return;
            }

            if (!oldChannel.Users.IsNotNullAndNotEmpty())
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.VcEmpty, true))
                    .AsError(ctx));
                return;
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Moving, true,
                    new TVar("Count", ctx.Member.VoiceState.Channel.Users.Count),
                    new TVar("Destination", ctx.Member.VoiceState.Channel.Mention),
                    new TVar("Origin", oldChannel.Mention)))
                .AsLoading(ctx));

            foreach (var b in oldChannel.Users)
            {
                _ = b.ModifyAsync(x => x.VoiceChannel = ctx.Member.VoiceState.Channel);
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Moved, true,
                    new TVar("Count", ctx.Member.VoiceState.Channel.Users.Count),
                    new TVar("Destination", ctx.Member.VoiceState.Channel.Mention),
                    new TVar("Origin", oldChannel.Mention)))
                .AsSuccess(ctx));
        });
    }
}