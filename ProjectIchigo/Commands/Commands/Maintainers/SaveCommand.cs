namespace ProjectIchigo.Commands;

internal class SaveCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx)
    {
        if (!ctx.User.IsMaintenance(ctx.Bot._status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            await RespondOrEdit("Saving all data to the database..");
            await ctx.Bot._databaseClient.SyncDatabase(true);
            await RespondOrEdit("All data has been saved to the database.");
        });
    }
}
