namespace ProjectMakoto.Commands.VcCreator;

internal class ChangeOwnerCommand : BaseCommand
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
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannel, true)).AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannelOwner, true)).AsError(ctx));
                return;
            }

            if (victim.IsBot)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.VictimIsBot, true, new TVar("User", victim.Mention))).AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId == victim.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.ChangeOwner.AlreadyOwner, true, new TVar("User", victim.Mention))).AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                if (ctx.Member.Permissions.HasPermission(Permissions.ManageChannels))
                {
                    ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId = victim.Id;
                    _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.ChangeOwner.ForceAssign, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
                    return;
                }

                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.NotAVccChannelOwner, true)).AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId = victim.Id;
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(t.Commands.Utility.VoiceChannelCreator.ChangeOwner.Success, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
        });
    }
}