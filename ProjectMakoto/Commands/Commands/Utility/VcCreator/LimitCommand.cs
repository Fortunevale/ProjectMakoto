namespace ProjectMakoto.Commands.VcCreator;

internal class LimitCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            uint newLimit = (uint)arguments["newLimit"];
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

            if (newLimit > 99)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.VoiceChannelCreator.Limit.OutsideRange)}`").AsError(ctx));
                return;
            }

            await channel.ModifyAsync(x => x.UserLimit = newLimit.ToInt32());
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.VoiceChannelCreator.Limit.Success).Replace("{Count}", newLimit)}`").AsSuccess(ctx));
        });
    }
}