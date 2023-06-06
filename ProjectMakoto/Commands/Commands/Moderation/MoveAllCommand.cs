// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class MoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.MoveMembers) && await CheckOwnPermissions(Permissions.MoveMembers) && await CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordChannel newChannel = (DiscordChannel)arguments["newChannel"];

            var CommandKey = t.Commands.Moderation.Move;

            if (newChannel.Type != ChannelType.Voice)
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.NotAVc, true))
                    .AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Moving, true, 
                    new TVar("Count", ctx.Member.VoiceState.Channel.Users.Count),
                    new TVar("Origin", ctx.Member.VoiceState.Channel.Mention),
                    new TVar("Destination", newChannel.Mention)))
                .AsLoading(ctx));

            foreach (var b in ctx.Member.VoiceState.Channel.Users)
            {
                _ = b.ModifyAsync(x => x.VoiceChannel = newChannel);
            }

            await RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(GetString(CommandKey.Moved, true,
                    new TVar("Count", ctx.Member.VoiceState.Channel.Users.Count),
                    new TVar("Origin", ctx.Member.VoiceState.Channel.Mention),
                    new TVar("Destination", newChannel.Mention)))
                .AsSuccess(ctx));
        });
    }
}