namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    [Command("phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help", [Description("Value")] string value = "")
    {
        _ = Task.Run(async () =>
        {
            if (!ctx.Member.IsAdmin())
            {
                _ = ctx.SendAdminError();
                return;
            }

            if (action.ToLower() == "help")
            {

            }
        });
    }
}
