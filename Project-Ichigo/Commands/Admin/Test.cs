namespace Project_Ichigo.Commands.Admin;
internal class Test : BaseCommandModule
{
    [Command("test-placeholder"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task PhishingSettings(CommandContext ctx)
    {
        _ = ctx.SendSyntaxError();
    }
}
