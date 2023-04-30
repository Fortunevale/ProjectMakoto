namespace ProjectMakoto.Commands;

internal class HugCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            string[] PositiveEmojis = { "♥", ctx.Bot.status.LoadedConfig.Emojis.Hug };
            string[] NegativeEmojis = { "😢", "😓" };

            string[] phrases = GetGuildString(t.Commands.Social.Hug.Other);
            string[] self_phrases = GetGuildString(t.Commands.Social.Hug.Self);

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Replace("{User1}", ctx.User.Username).Replace("{Emoji}", NegativeEmojis.SelectRandom()),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "hug");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.SelectRandom().Replace("{User1}", ctx.User.Mention).Replace("{User2}", user.Mention).Replace("{Emoji}", PositiveEmojis.SelectRandom())),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}