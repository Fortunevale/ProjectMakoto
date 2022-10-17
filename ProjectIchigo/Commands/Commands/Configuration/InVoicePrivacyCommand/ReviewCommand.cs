namespace ProjectIchigo.Commands.InVoicePrivacyCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = InVoicePrivacyCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, "In-Voice Text Channel Privacy"));
        });
    }
}