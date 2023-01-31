namespace ProjectMakoto.Commands;

internal class DummyCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.CompletedTask;
    }
}
