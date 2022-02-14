namespace Project_Ichigo.Commands.Admin;
internal class Test : BaseCommandModule
{
    public Status _status { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }

    [Command("throw"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task PhishingSettings(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_status))
                return;

            throw new NotImplementedException();
        }).Add(_watcher, ctx);
    }
}
