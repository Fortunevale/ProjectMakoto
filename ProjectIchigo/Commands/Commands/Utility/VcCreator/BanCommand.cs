namespace ProjectIchigo.Commands.VcCreator;

internal class BanCommand : BaseCommand
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

            if (!channel.Users.Any(x => x.Id == victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention}` is not in your Voice Channel.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You cannot ban yourself.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Contains(victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `has already been banned from your Voice Channel.`").AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Add(victim.Id);
            await channel.AddOverwriteAsync(victim, deny: Permissions.UseVoice);
            await victim.DisconnectFromVoiceAsync();
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `has been banned from this channel.`").AsSuccess(ctx));
        });
    }
}