namespace ProjectIchigo.Commands;

internal class StopCommand : BaseCommand
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
            var msg = await RespondOrEdit(new DiscordMessageBuilder().WithContent("Confirm?").AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "Shutdown", "Confirm shutdown", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")))));

            var x = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(1));
            
            if (x.TimedOut)
            {
                await RespondOrEdit("_Interaction timed out._");
                return;
            }

            await RespondOrEdit("Shutting down!");

            File.WriteAllText("updated", "");
        });
    }
}
