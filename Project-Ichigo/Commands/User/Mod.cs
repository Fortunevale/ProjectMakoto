namespace Project_Ichigo.Commands.User;
internal class Mod : BaseCommandModule
{
    [Command("mod-placeholder"),
    CommandModule("hidden"),
    Description(" ")]
    public async Task PhishingSettings(CommandContext ctx)
    {
        _ = ctx.SendSyntaxError();
    }
}
