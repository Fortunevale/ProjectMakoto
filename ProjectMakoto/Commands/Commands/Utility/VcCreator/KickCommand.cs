﻿namespace ProjectMakoto.Commands.VcCreator;

internal class KickCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            DiscordMember victim = (DiscordMember)arguments["victim"];
            DiscordChannel channel = ctx.Member.VoiceState?.Channel;

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(channel?.Id ?? 0))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannel)}`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannelOwner)}`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.VoiceChannelCreator.Kick.CannotKickSelf)}`").AsError(ctx));
                return;
            }

            if (!channel.Users.Any(x => x.Id == victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.Utility.VoiceChannelCreator.VictimNotPresent).Replace("{User}", $"{victim.Mention}`")}`").AsError(ctx));
                return;
            }

            await victim.DisconnectFromVoiceAsync();
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.Utility.VoiceChannelCreator.Kick.Success).Replace("{User}", $"{victim.Mention}`")}`").AsSuccess(ctx));
        });
    }
}