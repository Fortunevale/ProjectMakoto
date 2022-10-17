namespace ProjectIchigo.Commands.VcCreator;

internal class OpenCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            DiscordChannel channel = ctx.Member.VoiceState.Channel;

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(channel.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You're not in a channel created by the Voice Channel Creator.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You don't own this channel.`").AsError(ctx));
                return;
            }

            await channel.ModifyAsync(x => x.PermissionOverwrites = channel.PermissionOverwrites.ConvertToBuilderWithNewOverwrites(ctx.Guild.EveryoneRole, Permissions.UseVoice, Permissions.None));
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel has been opened.`").AsSuccess(ctx));
        });
    }
}