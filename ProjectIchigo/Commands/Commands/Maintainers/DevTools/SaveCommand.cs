namespace ProjectIchigo.Commands;

internal class SaveCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            await RespondOrEdit("Saving all data to the database..");
            await ctx.Bot.databaseClient.FullSyncDatabase(true);
            await RespondOrEdit("All data has been saved to the database.");
        });
    }
}
