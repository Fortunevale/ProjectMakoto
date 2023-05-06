// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.VcCreator;

internal class NameCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            string newName = (string)arguments["newName"];
            DiscordChannel channel = ctx.Member.VoiceState?.Channel;

            newName = (newName.IsNullOrWhiteSpace() ? GetGuildString(t.Commands.Utility.VoiceChannelCreator.Events.DefaultChannelName, new TVar("User", ctx.Member.DisplayName)) : newName);

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

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename.GetTimespanSince() < TimeSpan.FromMinutes(5))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.Name.Cooldown, true, 
                    new TVar("Timestamp", ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename.AddMinutes(5).ToTimestamp()))).AsError(ctx));
                return;
            }

            foreach (var b in ctx.Bot.profanityList)
                newName = newName.Replace(b, new String('*', b.Length));

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename = DateTime.UtcNow;
            await channel.ModifyAsync(x => x.Name = newName.TruncateWithIndication(25));
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.Name.Success, true,
                new TVar("Name", newName, true))).AsSuccess(ctx));
        });
    }
}