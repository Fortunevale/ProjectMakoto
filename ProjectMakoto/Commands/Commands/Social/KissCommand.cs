namespace ProjectMakoto.Commands;

internal class KissCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            string[] phrases = GetGuildString(t.Commands.Social.Kiss.Other);
            string[] self_phrases = GetGuildString(t.Commands.Social.Kiss.Self);

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Replace("{User1}", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "kiss");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.SelectRandom().Replace("{User1}", ctx.User.Mention).Replace("{User2}", user.Mention).Replace("{Emoji}", ctx.Bot.status.LoadedConfig.Emojis.Cuddle)),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}