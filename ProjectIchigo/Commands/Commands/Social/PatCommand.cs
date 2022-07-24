namespace ProjectIchigo.Commands;

internal class PatCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif("pat");

            string[] phrases =
            {
                    "%1 gives %2 headpats!",
                };

            string[] self_phrases =
            {
                    "There, there.. I'll give you some headpats, %1 😢",
                    "I'll give you some headpats, %1.. 😢",
                    "You look lonely there, %1..",
                };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }).WithContent(ctx.CommandType == Enums.CommandType.ApplicationCommand ? user.Mention : "").WithAllowedMention(UserMention.All));
        });
    }
}