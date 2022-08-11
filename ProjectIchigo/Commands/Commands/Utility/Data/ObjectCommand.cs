namespace ProjectIchigo.Commands.Data;

internal class ObjectCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Placeholder`"
            }.SetInfo(ctx));
        });
    }
}