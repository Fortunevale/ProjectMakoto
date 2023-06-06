// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.VcCreator;

internal sealed class LimitCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            uint newLimit = (uint)arguments["newLimit"];
            DiscordChannel channel = ctx.Member.VoiceState?.Channel;

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(channel?.Id ?? 0))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannel, true)).AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannelOwner, true)).AsError(ctx));
                return;
            }

            if (newLimit > 99)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.Limit.OutsideRange, true)).AsError(ctx));
                return;
            }

            await channel.ModifyAsync(x => x.UserLimit = newLimit.ToInt32());
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.Limit.Success, true, new TVar("Count", newLimit))).AsSuccess(ctx));
        });
    }
}