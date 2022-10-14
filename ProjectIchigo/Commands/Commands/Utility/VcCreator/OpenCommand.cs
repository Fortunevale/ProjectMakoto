namespace ProjectIchigo.Commands.VcCreator;

internal class OpenCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {

        });
    }
}