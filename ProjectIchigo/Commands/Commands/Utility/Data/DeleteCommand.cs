namespace ProjectIchigo.Commands.Data;

internal class DeleteCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Deleting your own data is not yet supported. Please join our Support Server to request data deletion:` {ctx.Bot._status.DevelopmentServerInvite}"
            }.SetInfo(ctx));
        });
    }
}