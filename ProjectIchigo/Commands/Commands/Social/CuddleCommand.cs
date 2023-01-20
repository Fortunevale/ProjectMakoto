namespace ProjectIchigo.Commands;

internal class CuddleCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string[] phrases =
            {
                $"%1 cuddles with %2! {ctx.Bot.status.LoadedConfig.Emojis.Cuddle}",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
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

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "cuddle");

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