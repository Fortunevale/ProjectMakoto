namespace ProjectMakoto.Commands;

internal class MoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.MoveMembers) && await CheckOwnPermissions(Permissions.MoveMembers) && await CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordChannel newChannel = (DiscordChannel)arguments["newChannel"];

            if (newChannel.Type != ChannelType.Voice)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is not a voice channel.`").AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moving {ctx.Member.VoiceState.Channel.Users.Count} users to` {newChannel.Mention}`..`").AsLoading(ctx));

            foreach (var b in ctx.Member.VoiceState.Channel.Users)
            {
                await b.ModifyAsync(x => x.VoiceChannel = newChannel);
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moved {ctx.Member.VoiceState.Channel.Users.Count} users to` {newChannel.Mention}`.`").AsSuccess(ctx));
        });
    }
}