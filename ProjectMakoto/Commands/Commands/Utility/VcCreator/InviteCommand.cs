namespace ProjectMakoto.Commands.VcCreator;

internal class InviteCommand : BaseCommand
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

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.VoiceChannelCreator.Invite.CannotInviteSelf)}`").AsError(ctx));
                return;
            }

            if (channel.Users.Any(x => x.Id == victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.VoiceChannelCreator.Invite.AlreadyPresent).Replace("{User}", $"{victim.Mention}`")}`").AsError(ctx));
                return;
            }

            await channel.AddOverwriteAsync(victim, Permissions.UseVoice);

            try
            {
                await victim.SendMessageAsync($"{t.Commands.VoiceChannelCreator.Invite.VictimMessage.Get(ctx.Bot.users[victim.Id]).Replace("{User}", $"{ctx.User.Mention}`").Replace("{Channel}", $"`{channel.Mention}`")}`");
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.VoiceChannelCreator.Invite.PartialSuccess).Replace("{User}", $"{victim.Mention}`")}`").AsError(ctx));
                return;
            }

            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{GetString(t.Commands.VoiceChannelCreator.Invite.Success).Replace("{User}", $"{victim.Mention}`")}`").AsSuccess(ctx));
        });
    }
}