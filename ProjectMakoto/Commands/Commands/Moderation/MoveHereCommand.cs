// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class MoveHereCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.MoveMembers) && await CheckOwnPermissions(Permissions.MoveMembers) && await CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordChannel oldChannel = (DiscordChannel)arguments["oldChannel"];

            if (oldChannel.Type != ChannelType.Voice)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is not a voice channel.`").AsError(ctx));
                return;
            }
            
            if (!oldChannel.Users.IsNotNullAndNotEmpty())
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is empty.`").AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moving {oldChannel.Users.Count} users to` {ctx.Member.VoiceState.Channel.Mention}`..`").AsLoading(ctx));

            foreach (var b in oldChannel.Users)
            {
                await b.ModifyAsync(x => x.VoiceChannel = ctx.Member.VoiceState.Channel);
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moved {oldChannel.Users.Count} users to` {ctx.Member.VoiceState.Channel.Mention}`.`").AsSuccess(ctx));
        });
    }
}