namespace ProjectMakoto.Commands.VcCreator;

internal class CloseCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            DiscordChannel channel = ctx.Member.VoiceState?.Channel;

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(channel?.Id ?? 0))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.NotAVccChannel)}`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.NotAVccChannelOwner)}`").AsError(ctx));
                return;
            }

            await channel.ModifyAsync(x => x.PermissionOverwrites = channel.PermissionOverwrites.ConvertToBuilderWithNewOverwrites(ctx.Guild.EveryoneRole, Permissions.None, Permissions.UseVoice));
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.Close.Success)}`").AsSuccess(ctx));
        });
    }
}