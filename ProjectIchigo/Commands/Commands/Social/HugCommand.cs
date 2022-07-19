namespace ProjectIchigo.Commands;

internal class HugCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif("hug");

            string[] phrases =
            {
                "%1 hugs %2! How sweet! ♥",
                $"%1 gives %2 a big fat hug! {ctx.Bot._status.LoadedConfig.HugEmoji}",
                $"%2, watch out! %1 is coming to squeeze you tight! {ctx.Bot._status.LoadedConfig.HugEmoji}",
            };

            string[] self_phrases =
            {
                "There, there.. I'll hug you %1 😢",
                "Does no one else hug you, %1? There, there.. I'll hug you.. 😢",
                "There, there.. I'll hug you %1. 😢 Sorry if i'm a bit cold, i'm not human y'know.. 😓",
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
            }).WithContent(ctx.CommandType == Enums.CommandType.ApplicationCommand ? user.Mention : ""));
        });
    }
}