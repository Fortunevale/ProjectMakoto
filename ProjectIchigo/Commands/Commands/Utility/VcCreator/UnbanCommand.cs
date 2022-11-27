namespace ProjectIchigo.Commands.VcCreator;

internal class UnbanCommand : BaseCommand
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
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You're not in a channel created by the Voice Channel Creator.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You don't own this channel.`").AsError(ctx));
                return;
            }

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Contains(victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `is not banned from your Voice Channel.`").AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Remove(victim.Id);
            await channel.AddOverwriteAsync(victim, deny: Permissions.None);
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `has been unbanned from this channel.`").AsSuccess(ctx));
        });
    }
}