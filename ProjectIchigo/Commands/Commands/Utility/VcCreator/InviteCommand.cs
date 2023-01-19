namespace ProjectIchigo.Commands.VcCreator;

internal class InviteCommand : BaseCommand
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

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You cannot invite yourself.`").AsError(ctx));
                return;
            }

            if (channel.Users.Any(x => x.Id == victim.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `is already in your Voice Channel.`").AsError(ctx));
                return;
            }

            await channel.AddOverwriteAsync(victim, Permissions.UseVoice);

            try
            {
                await victim.SendMessageAsync($"{ctx.User.Mention} has invited you to join {channel.Mention}.");
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `can now join this channel. However, i was unable to send a Direct Message to them.`").AsError(ctx));
                return;
            }

            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"{victim.Mention} `has been invited to this channel.`").AsSuccess(ctx));
        });
    }
}