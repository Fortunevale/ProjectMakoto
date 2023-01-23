namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var ListEmbed = new DiscordEmbedBuilder
            {
                Description = BumpReminderCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, "Bump Reminder");
            await RespondOrEdit(ListEmbed);
        });
    }
}