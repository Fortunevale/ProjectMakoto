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

            string[] phrases = t.Commands.Social.Hug.Other.Get(ctx.DbGuild);
            string[] self_phrases = t.Commands.Social.Hug.Self.Get(ctx.DbGuild);

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Build(
                    new TVar("User1", ctx.Member.DisplayName),
                    new TVar("Emoji", NegativeEmojis.SelectRandom())),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "hug");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = phrases.SelectRandom().Build(
                    new TVar("User1", ctx.User.Mention, false),
                    new TVar("User2", user.Mention, false),
                    new TVar("Emoji", PositiveEmojis.SelectRandom(), false))
                    .Bold(),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}