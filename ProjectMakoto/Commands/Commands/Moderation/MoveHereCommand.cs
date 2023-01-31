namespace ProjectMakoto.Commands;

internal class MoveHereCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.MoveMembers) && await CheckOwnPermissions(Permissions.MoveMembers) && await CheckVoiceState());

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordChannel oldChannel = (DiscordChannel)arguments["oldChannel"];

            if (oldChannel.Type != ChannelType.Voice)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is not a voice channel.`").AsError(ctx));
                return;
            }
            
            if (!oldChannel.Users.IsNotNullAndNotEmpty())
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The channel you selected is empty.`").AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moving {oldChannel.Users.Count} users to` {ctx.Member.VoiceState.Channel.Mention}`..`").AsLoading(ctx));

            foreach (var b in oldChannel.Users)
            {
                await b.ModifyAsync(x => x.VoiceChannel = ctx.Member.VoiceState.Channel);
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Moved {oldChannel.Users.Count} users to` {ctx.Member.VoiceState.Channel.Mention}`.`").AsSuccess(ctx));
        });
    }
}