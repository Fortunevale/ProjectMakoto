namespace ProjectIchigo.Commands;

internal class HugCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            string[] phrases =
            {
                "%1 hugs %2! How sweet! ♥",
                $"%1 gives %2 a big fat hug! {ctx.Bot.status.LoadedConfig.Emojis.Hug}",
                $"%2, watch out! %1 is coming to squeeze you tight! {ctx.Bot.status.LoadedConfig.Emojis.Hug}",
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
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "hug");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.SelectRandom().Replace("%1", ctx.User.Mention).Replace("%2", user.Mention)),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}