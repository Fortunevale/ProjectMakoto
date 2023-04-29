namespace ProjectMakoto.Commands.VcCreator;

internal class UnbanCommand : BaseCommand
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
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.NotAVccChannel)}`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.NotAVccChannelOwner)}`").AsError(ctx));
                return;
            }

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Contains(victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.VoiceChannelCreator.Unban.VictimNotBanned).Replace("{User}", $"{victim.Mention}`")}`").AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].BannedUsers.Remove(victim.Id);
            await channel.AddOverwriteAsync(victim, deny: Permissions.None);
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.VoiceChannelCreator.Unban.VictimUnbanned).Replace("{User}", $"{victim.Mention}`")}`").AsSuccess(ctx));
        });
    }
}