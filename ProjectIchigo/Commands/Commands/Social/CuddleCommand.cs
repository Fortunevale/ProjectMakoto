namespace ProjectIchigo.Commands;

internal class CuddleCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif("cuddle");

            string[] phrases =
            {
                $"%1 cuddles with %2! {ctx.Bot._status.LoadedConfig.CuddleEmoji}",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
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