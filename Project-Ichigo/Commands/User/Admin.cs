namespace Project_Ichigo.Commands.User;
internal class Admin : BaseCommandModule
{
    [Command("phishing-settings"),
    CommandModule("admin"),
    Description("Allows to review and change settings for phishing detection")]
    public async Task PhishingSettings(CommandContext ctx, [Description("Action")] string action = "help")
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
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = ctx.Guild.Name },
                    Color = ColorHelper.Info,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.Now,
                    Description = $"`{ctx.Prefix}{ctx.Command.Name} help` - _Shows help on how to use this command._\n" +
                                  $"`{ctx.Prefix}{ctx.Command.Name} review` - _Shows the currently used settings._\n" +
                                  $"`{ctx.Prefix}{ctx.Command.Name} config` - _Allows you to change the currently used settings._"
                });
                return;
            }
        });
    }
}
