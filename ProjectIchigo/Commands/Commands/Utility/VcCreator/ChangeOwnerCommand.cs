namespace ProjectIchigo.Commands.VcCreator;

internal class ChangeOwnerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            DiscordMember victim = (DiscordMember)arguments["victim"];
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

            if (victim.IsBot)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `is a bot.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention}  `is already the owner.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                if (ctx.Member.Permissions.HasPermission(Permissions.ManageChannels))
                {
                    ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId = victim.Id;
                    _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Forcefully assigned` {victim.Mention} `as the new owner of this channel.`").AsSuccess(ctx));
                    return;
                }

                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You don't own this channel.`").AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId = victim.Id;
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `now owns this channel.`").AsSuccess(ctx));
        });
    }
}