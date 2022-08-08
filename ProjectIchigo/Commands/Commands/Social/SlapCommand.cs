namespace ProjectIchigo.Commands;

internal class SlapCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif("slap");

            string[] phrases =
            {
                $"%1 slaps %2! That looks like it hurt.. {ctx.Bot._status.LoadedConfig.SlapEmoji}",
            };

            string[] self_phrases =
            {
                "Come on, %1. There's no need to be so hard on yourself!",
                "Bad %1! I don't know what you did but bad!"
            };

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Mention).Replace("%2", user.Mention)),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }).WithAllowedMention(UserMention.All));
        });
    }
}