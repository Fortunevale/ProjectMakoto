// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.VcCreator;

internal sealed class UnbanCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var victim = (DiscordMember)arguments["victim"];
            var channel = ctx.Member.VoiceState?.Channel;

            if (!ctx.DbGuild.VcCreator.CreatedChannels.Any(x => x.ChannelId == (channel?.Id ?? 0)))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.VoiceChannelCreator.NotAVccChannel, true)).AsError(ctx));
                return;
            }

            if (ctx.DbGuild.VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.VoiceChannelCreator.NotAVccChannelOwner, true)).AsError(ctx));
                return;
            }

            if (!ctx.DbGuild.VcCreator.CreatedChannels[channel.Id].BannedUsers.Contains(victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.VoiceChannelCreator.Unban.VictimNotBanned, true, new TVar("User", victim.Mention))).AsError(ctx));
                return;
            }

            ctx.DbGuild.VcCreator.CreatedChannels[channel.Id].BannedUsers = ctx.DbGuild.VcCreator.CreatedChannels[channel.Id].BannedUsers.Remove(x => x.ToString(), victim.Id);
            await channel.AddOverwriteAsync(victim, deny: Permissions.None);
            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.VoiceChannelCreator.Unban.VictimUnbanned, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
        });
    }
}