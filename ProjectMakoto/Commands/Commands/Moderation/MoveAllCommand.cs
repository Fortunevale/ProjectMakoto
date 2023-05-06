// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class MoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.MoveMembers) && await CheckOwnPermissions(Permissions.MoveMembers) && await CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordChannel newChannel = (DiscordChannel)arguments["newChannel"];

            if (newChannel.Type != ChannelType.Voice)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is not a voice channel.`").AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moving {ctx.Member.VoiceState.Channel.Users.Count} users to` {newChannel.Mention}`..`").AsLoading(ctx));

            foreach (var b in ctx.Member.VoiceState.Channel.Users)
            {
                await b.ModifyAsync(x => x.VoiceChannel = newChannel);
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moved {ctx.Member.VoiceState.Channel.Users.Count} users to` {newChannel.Mention}`.`").AsSuccess(ctx));
        });
    }
}